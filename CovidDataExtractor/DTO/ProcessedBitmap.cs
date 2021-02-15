using CovidDataExtractor.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace CovidDataExtractor.DTO
{
    public class ProcessedBitmap : ParsedBitmap
    {
        public CropLocation Location { get; set; }
        public string ProcessedHtml { get; set; }
        public int Count { get; set; }
        public ProcessedBitmap(ParsedBitmap parsed)
        {
            this.Url = parsed.Url;
        }
    }
}
