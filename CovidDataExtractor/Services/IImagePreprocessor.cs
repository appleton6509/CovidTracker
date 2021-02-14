using System.Drawing;

namespace CovidDataExtractor.Services
{
    public interface IImagePreprocessor
    {
        Bitmap Image { get; set; }

        ImagePreprocessor AddBorder(Color color);
        ImagePreprocessor Crop(Rectangle cropArea);
        ImagePreprocessor FloodFill();
        ImagePreprocessor Resize();
        ImagePreprocessor SetImage(Bitmap image);
        ImagePreprocessor Binarization(BinarizationThreshold thresh);
    }
}