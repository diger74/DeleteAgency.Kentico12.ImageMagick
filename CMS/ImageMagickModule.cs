using System;
using System.Linq;
using CMS;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.IO;
using CMS.MediaLibrary;
using CMS.SiteProvider;
using DeleteAgency.Kentico12.ImageMagick;
using ImageMagick;

[assembly: RegisterModule(typeof(ImageMagickModule))]
namespace DeleteAgency.Kentico12.ImageMagick
{
    public class ImageMagickModule : Module
    {
        protected string CacheDirectory => SettingsKeyInfoProvider
            .GetValue("ImageMagickCacheDirectory", SiteContext.CurrentSiteName).Trim('/', '\\');

        public ImageMagickModule() : base("DeleteAgency.Kentico12.ImageMagick", true)
        {
        }

        protected override void OnInit()
        {
            base.OnInit();

            try
            {
                var fullCachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CacheDirectory);
                if (!Directory.Exists(fullCachePath))
                {
                    Directory.CreateDirectory(fullCachePath);
                }

                MagickAnyCPU.CacheDirectory = fullCachePath;
            }
            catch (Exception exception)
            {
                EventLogProvider.LogException("ImageMagick", "MODULESTART", exception);
            }
            
            MediaFileInfo.TYPEINFO.Events.Insert.Before += MediaFileOnBeforeSave;
            MediaFileInfo.TYPEINFO.Events.Update.Before += MediaFileOnBeforeSave;

            AttachmentInfo.TYPEINFO.Events.Insert.Before += AttachmentOnBeforeSave;
            AttachmentInfo.TYPEINFO.Events.Update.Before += AttachmentOnBeforeSave;

            MetaFileInfo.TYPEINFO.Events.Insert.Before += MetaFileOnBeforeSave;
            MetaFileInfo.TYPEINFO.Events.Update.Before += MetaFileOnBeforeSave;

            AttachmentHistoryInfo.TYPEINFO.Events.Insert.Before += AttachmentOnBeforeSave;

            EventLogProvider.LogInformation("ImageMagick", "MODULESTART");
        }

        private void AttachmentOnBeforeSave(object sender, ObjectEventArgs e)
        {
            if (e.Object == null) return;

            // If workflow enabled
            if (e.Object is AttachmentHistoryInfo attachmentVersion)
            {
                var latestAttachmentVersion = AttachmentHistoryInfoProvider.GetAttachmentHistories()
                    .WhereEquals("AttachmentGUID", attachmentVersion.AttachmentGUID)
                    .OrderByDescending("AttachmentLastModified")
                    .TopN(1)
                    .FirstOrDefault();

                if (latestAttachmentVersion == null ||
                    latestAttachmentVersion.AttachmentSize != attachmentVersion.AttachmentSize)
                {
                    var optimizer = new ImageMagickImageOptimizer(SiteContext.CurrentSiteName);
                    optimizer.Optimize(attachmentVersion);
                }
            }

            // If workflow disabled
            if (e.Object is AttachmentInfo attachment)
            {
                var document = DocumentHelper.GetDocument(attachment.AttachmentDocumentID, new TreeProvider());
                
                if (document.WorkflowStep == null)
                {
                    var currentAttachment = AttachmentInfoProvider.GetAttachmentInfo(attachment.AttachmentID, true);

                    if (currentAttachment == null || currentAttachment.AttachmentSize != attachment.AttachmentSize)
                    {
                        var optimizer = new ImageMagickImageOptimizer(SiteContext.CurrentSiteName);
                        optimizer.Optimize(attachment);
                    }
                }
            }
        }

        private void MediaFileOnBeforeSave(object sender, ObjectEventArgs e)
        {
            if (e.Object == null) return;

            if (e.Object is MediaFileInfo image)
            {
                var optimizer = new ImageMagickImageOptimizer(SiteContext.CurrentSiteName);
                optimizer.Optimize(image);
            }
        }

        private void MetaFileOnBeforeSave(object sender, ObjectEventArgs e)
        {
            if (e.Object == null) return;

            if (e.Object is MetaFileInfo metaFile)
            {
                var optimizer = new ImageMagickImageOptimizer(SiteContext.CurrentSiteName);
                optimizer.Optimize(metaFile);
            }
        }
    }
}