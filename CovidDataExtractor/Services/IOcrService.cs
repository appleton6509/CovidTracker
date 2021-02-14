using CovidDataExtractor.DTO;
using System.Drawing;

namespace CovidDataExtractor.Services
{
    public interface IOcrService
    {
        ProcessedBitmap Extract(ParsedBitmap image, CropLocation location);
    }
}