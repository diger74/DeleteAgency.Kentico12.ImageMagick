using System;
using CMS.DataEngine;

namespace DeleteAgency.Kentico12.ImageMagick.Events
{
    public class ImageMagickImageOptimizerEventArgs : EventArgs
    {
        public BaseInfo Image { get; set; }

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