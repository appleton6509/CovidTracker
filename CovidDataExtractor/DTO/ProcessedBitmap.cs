using CovidDataExtractor.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CovidDataExtractor.DTO
{
    public class ProcessedBitmap : ParsedBitmap
    {
        private IImagePreprocessor imagePreprocessor;

        public Location Location { get; set; }
        public string ProcessedHtml { get; set; }
        public int NumberReadFromImage { get; set; }

        public ProcessedBitmap( IImagePreprocessor imageProcessor)
        {
            this.imagePreprocessor = imageProcessor;
        }
        public ProcessedBitmap Process(ParsedBitmap parsed, Location location, Rectangle cropLocation, BinarizationThreshold binarization = BinarizationThreshold.Light)
        {
            this.Url = parsed.Url;
            this.Location = location;
            Bitmap clone = new Bitmap(parsed.Image);
            this.Image = imagePreprocessor
                .SetImage(clone)
                .Crop(cropLocation)
                .FloodFill()
                .Resize()
                .AddBorder(Color.White)
                .Binarization(binarization)
                .Image;
            return this;
        }
    }
}
