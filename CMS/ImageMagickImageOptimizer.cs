using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CMS.DataEngine;
using CMS.DocumentEngine;
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

        public void Optimize(MediaFileInfo mediaFile)
        {
            if (!Enabled || !EnabledMediaFiles ||
                !FileExtensions.Contains(mediaFile.FileExtension, StringComparer.OrdinalIgnoreCase)) return;
            var eventArgs = new ImageMagickImageOptimizerEventArgs();

            try
            {
                eventArgs.MediaFile = mediaFile;
                Events.Before?.Invoke(null, eventArgs);

                // If cancelled in Before event
                if (eventArgs.CancelImageOptimization) return;

                var fileBinary = GetMediaFileBinary(mediaFile);

                var ms = OptimizeFileBinary(fileBinary);
                if (ms.Length > 0)
                {
                    mediaFile.FileSize = ms.Length;
                    mediaFile.FileBinary = ms.ToArray();

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

        public void Optimize(AttachmentHistoryInfo pageAttachment)
        {
            if (!Enabled || !EnabledPageAttachments ||
                !FileExtensions.Contains(pageAttachment.AttachmentExtension, StringComparer.OrdinalIgnoreCase)) return;
            var eventArgs = new ImageMagickImageOptimizerEventArgs();

            try
            {
                eventArgs.PageAttachmentVersion = pageAttachment;
                Events.Before?.Invoke(null, eventArgs);

                // If cancelled in Before event
                if (eventArgs.CancelImageOptimization) return;

                var fileBinary = pageAttachment.AttachmentBinary;

                var ms = OptimizeFileBinary(fileBinary);
                if (ms.Length > 0)
                {
                    pageAttachment.AttachmentSize = Convert.ToInt32(ms.Length);
                    pageAttachment.AttachmentBinary = ms.ToArray();

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

        public void Optimize(AttachmentInfo pageAttachment)
        {
            if (!Enabled || !EnabledPageAttachments ||
                !FileExtensions.Contains(pageAttachment.AttachmentExtension, StringComparer.OrdinalIgnoreCase)) return;
            var eventArgs = new ImageMagickImageOptimizerEventArgs();

            try
            {
                eventArgs.PageAttachment = pageAttachment;
                Events.Before?.Invoke(null, eventArgs);

                // If cancelled in Before event
                if (eventArgs.CancelImageOptimization) return;

                var fileBinary = pageAttachment.AttachmentBinary;

                var ms = OptimizeFileBinary(fileBinary);
                if (ms.Length > 0)
                {
                    pageAttachment.AttachmentSize = Convert.ToInt32(ms.Length);
                    pageAttachment.AttachmentBinary = ms.ToArray();

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

        private byte[] GetMediaFileBinary(MediaFileInfo mediaFile)
        {
            // For files with uploaded binary (new file or update)
            if (mediaFile.FileBinary != null) return mediaFile.FileBinary;

            // For existing files
            var mediaLibrary = MediaLibraryInfoProvider.GetMediaLibraryInfo(mediaFile.FileLibraryID);
            return MediaFileInfoProvider.GetFile(mediaFile, mediaLibrary.LibraryFolder, SiteContext.CurrentSiteName);
        }
    }
}