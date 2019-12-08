using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MediaLibrary;
using CMS.SiteProvider;
using DeleteAgency.Kentico12.ImageMagick.Events;
using DeleteAgency.Kentico12.ImageMagick.Optimizations;
using DeleteAgency.Kentico12.ImageMagick.Optimizations.Interfaces;
using ImageMagick;

namespace DeleteAgency.Kentico12.ImageMagick
{
    public class ImageMagickImageOptimizer
    {
        protected static IOptimization Optimization;

        public static ImageMagickImageOptimizerEvents Events = new ImageMagickImageOptimizerEvents();
        
        protected bool Enabled => SettingsKeyInfoProvider.GetBoolValue("ImageMagickOptimizationEnabled", _siteName);
        protected bool EnabledMediaFiles => SettingsKeyInfoProvider.GetBoolValue("ImageMagickOptimizeMediaFiles", _siteName);
        protected bool EnabledPageAttachments => SettingsKeyInfoProvider.GetBoolValue("ImageMagickOptimizePageAttachments", _siteName);
        protected bool EnabledMetaFiles => SettingsKeyInfoProvider.GetBoolValue("ImageMagickOptimizeMetaFiles", _siteName);
        protected IEnumerable<string> FileExtensions => SettingsKeyInfoProvider.GetValue("ImageMagickFileExtensions", _siteName)
            .Split(new[] {",", ";", "|", " "}, StringSplitOptions.RemoveEmptyEntries);
        
        private readonly string _siteName;

        public ImageMagickImageOptimizer(string siteName)
        {
            _siteName = siteName;
            if (Optimization == null)
            {
                SetOptimization(new DefaultOptimization(siteName));
            }
        }

        public static void SetOptimization(IOptimization optimization)
        {
            Optimization = optimization;
        }

        public void Optimize(BaseInfo image)
        {
            if (!Enabled) return;

            string fileExtension;
            switch (image)
            {
                case MediaFileInfo mediaFile:
                    if (!EnabledMediaFiles) return;
                    fileExtension = mediaFile.FileExtension;
                    break;

                case MetaFileInfo metaFile:
                    if (!EnabledMetaFiles) return;
                    fileExtension = metaFile.MetaFileExtension;
                    break;

                case AttachmentInfo attachment:
                    if (!EnabledPageAttachments) return;
                    fileExtension = attachment.AttachmentExtension;
                    break;

                case AttachmentHistoryInfo attachmentHistory:
                    if (!EnabledPageAttachments) return;
                    fileExtension = attachmentHistory.AttachmentExtension;
                    break;

                default:
                    return;
            }
            if (!FileExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase)) return;

            var eventArgs = new ImageMagickImageOptimizerEventArgs();

            try
            {
                eventArgs.Image = image;
                Events.Before?.Invoke(null, eventArgs);

                // If cancelled in Before event
                if (eventArgs.CancelImageOptimization) return;

                var fileBinary = GetFileBinary(image);

                var ms = OptimizeFileBinary(fileBinary);
                if (ms.Length > 0)
                {
                    SaveOptimized(image, ms);

                    eventArgs.ImageOptimizationSuccessful = true;
                    Events.After?.Invoke(null, eventArgs);
                }
                else
                {
                    throw new Exception("ImageMagick download response is empty!");
                }
            }
            catch (Exception exception)
            {
                eventArgs.Error = exception;
                Events.Error?.Invoke(null, eventArgs);
            }
        }

        private void SaveOptimized(BaseInfo image, MemoryStream ms)
        {
            switch (image)
            {
                case MediaFileInfo mediaFile:
                    mediaFile.FileSize = ms.Length;
                    mediaFile.FileBinary = ms.ToArray();
                    break;

                case MetaFileInfo metaFile:
                    metaFile.MetaFileSize = ValidationHelper.GetInteger(ms.Length, 0);
                    metaFile.MetaFileBinary = ms.ToArray();
                    break;

                case AttachmentInfo attachment:
                    attachment.AttachmentSize = Convert.ToInt32(ms.Length);
                    attachment.AttachmentBinary = ms.ToArray();
                    break;

                case AttachmentHistoryInfo attachmentHistory:
                    attachmentHistory.AttachmentSize = Convert.ToInt32(ms.Length);
                    attachmentHistory.AttachmentBinary = ms.ToArray();
                    break;
            }
        }

        private byte[] GetFileBinary(BaseInfo image)
        {
            switch (image)
            {
                case MediaFileInfo mediaFile:
                    // For files with uploaded binary (new file or update)
                    if (mediaFile.FileBinary != null) return mediaFile.FileBinary;
                    // For existing files
                    var mediaLibrary = MediaLibraryInfoProvider.GetMediaLibraryInfo(mediaFile.FileLibraryID);
                    return MediaFileInfoProvider.GetFile(mediaFile, mediaLibrary.LibraryFolder, SiteContext.CurrentSiteName);

                case MetaFileInfo metaFile:
                    // For files with uploaded binary (new file or update)
                    if (metaFile.MetaFileBinary != null) return metaFile.MetaFileBinary;
                    // For existing files
                    return MetaFileInfoProvider.GetFile(metaFile, SiteContext.CurrentSiteName);

                case AttachmentInfo attachment:
                    return attachment.AttachmentBinary;

                case AttachmentHistoryInfo attachmentHistory:
                    return attachmentHistory.AttachmentBinary;

                default:
                    return null;
            }
        }

        private MemoryStream OptimizeFileBinary(byte[] fileBinary)
        {
            var ms = new MemoryStream();

            using (var image = new MagickImage(fileBinary))
            {
                Optimization?.Apply(image);

                image.Write(ms);
            }

            return ms;
        }
    }
}