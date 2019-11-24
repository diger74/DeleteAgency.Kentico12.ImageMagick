using ImageMagick;

namespace DeleteAgency.Kentico12.ImageMagick.Optimizations.Interfaces
{
    public interface IOptimization
    {
        void Apply(MagickImage image);
    }
}