using CovidDataExtractor.DTO;
using System.Drawing;

namespace CovidDataExtractor.Services
{
    public interface IOcrService
    {
        ProcessedBitmap ExtractText(ParsedBitmap image, CropLocation location);
    }
}