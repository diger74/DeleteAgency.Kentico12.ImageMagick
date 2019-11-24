using System;
using CMS.DocumentEngine;
using CMS.MediaLibrary;

namespace DeleteAgency.Kentico12.ImageMagick.Events
{
    public class ImageMagickImageOptimizerEventArgs : EventArgs
    {
        public MediaFileInfo MediaFile { get; set; }

        public AttachmentHistoryInfo PageAttachmentVersion { get; set; }

        public AttachmentInfo PageAttachment { get; set; }

        public bool CancelImageOptimization { get; set; }

        public bool ImageOptimizationSuccessful { get; set; }

        public Exception Error { get; set; }

        public ImageMagickImageOptimizerEventArgs()
        {
            ImageOptimizationSuccessful = false;
            CancelImageOptimization = false;
        }
    }
}