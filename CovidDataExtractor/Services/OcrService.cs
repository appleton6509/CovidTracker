using CovidDataExtractor.DTO;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using Tesseract;

namespace CovidDataExtractor.Services
{
    public enum Location
    {
        Chilliwack
    }
    public class OcrService : IOcrService
    {
        private static int counter = 0;
        private readonly TesseractEngine engine;
        private IImagePreprocessor imageProcessor;


        public OcrService(IImagePreprocessor processor)
        {
            this.engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            this.imageProcessor = processor;
            DefaultSettings();
        }

        private void DefaultSettings()
        {
            engine.SetVariable("tessedit_char_whitelist", "0123456789"); // show only digits
            engine.SetVariable("load_system_dawg", false); // disable dictionary values
            engine.SetVariable("load_freq_dawg", false); // disable dictionary values
            engine.DefaultPageSegMode = PageSegMode.SingleWord;
        }
        public ProcessedBitmap GetNumberFromProcessedImage(ParsedBitmap image, Location location, Rectangle cropLocation)
        {
            ProcessedBitmap processed = new ProcessedBitmap(imageProcessor);
            processed = processed.Process(image, location, cropLocation);
            try
            {
                processed.NumberReadFromImage = ReadImageText(processed.Image);
            }
            catch (NotSupportedException e) {
                processed = processed.Process(image, location, cropLocation,BinarizationThreshold.Heavy);
                processed.NumberReadFromImage = ReadImageText(processed.Image);
            }
            return processed; ;
        }

        /// <summary>
        /// read a bitmap image for a number
        /// </summary>
        /// <param name="image">image to read</param>
        /// <returns>number read from the bitmap</returns>
        /// <exception cref="NotSupportedException">No number could be read from image</exception>
        private int ReadImageText(Bitmap image)
        {
            ImageConverter converter = new ImageConverter();
            byte[] newimage = (byte[])converter.ConvertTo(image, typeof(byte[]));
            using var page = engine.Process(Pix.LoadFromMemory(newimage));
            string convert = page.GetText();

            if (String.IsNullOrEmpty(convert) || String.IsNullOrWhiteSpace(convert))
                throw new NotSupportedException();
            return Convert.ToInt32(convert);
        }

        private static void SaveToFile(Bitmap image)
        {
            image.Save(@"C:\data\" + counter + ".jpg");
            counter++;
        }

    }
}
