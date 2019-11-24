using CMS.DataEngine;
using DeleteAgency.Kentico12.ImageMagick.Optimizations.Interfaces;
using ImageMagick;

namespace DeleteAgency.Kentico12.ImageMagick.Optimizations
{
    public class DefaultOptimization : IOptimization
    {
        protected int ImageQuality => SettingsKeyInfoProvider.GetIntValue("ImageMagickImageQuality", _siteName);

        private readonly string _siteName;

        public DefaultOptimization(string siteName)
        {
            _siteName = siteName;
        }

        public void Apply(MagickImage image)
        {
            switch (image.Format)
            {
                case MagickFormat.Jpe:
                case MagickFormat.Jpeg:
                case MagickFormat.Jpg:
                    image.Format = MagickFormat.Pjpeg;
                    break;

                case MagickFormat.Png:
                case MagickFormat.Png8:
                case MagickFormat.Png00:
                case MagickFormat.Png24:
                case MagickFormat.Png32:
                case MagickFormat.Png48:
                case MagickFormat.Png64:
                    image.Format = MagickFormat.Png8;
                    break;
            }

            image.Strip();
            image.Quality = ImageQuality;
        }
    }
}