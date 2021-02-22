using CovidDataExtractor.DTO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CovidDataExtractor.Entity
{
   public class Data
    {
        public Guid Id { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int Count { get; set; }
        public byte[] Image { get; set; }

        public static Data Convert(ProcessedBitmap bitmap, DateRange dates)
        {
            ImageConverter converter = new ImageConverter();
            byte[] bytes = (byte[])converter.ConvertTo(bitmap.Image, typeof(byte[]));
            return new Data()
            {
                Image = bytes,
                 FromDate = dates.FromDate,
                 ToDate = dates.ToDate,
                 Count = bitmap.NumberReadFromImage
            };
        }
    }
}
