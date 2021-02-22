using CovidDataExtractor.DTO;
using System.Drawing;

namespace CovidDataExtractor.Services
{
    public interface IOcrService
    {
        ProcessedBitmap GetNumberFromProcessedImage(ParsedBitmap image, Location location, Rectangle cropLocation);
    }
}