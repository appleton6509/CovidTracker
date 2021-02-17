﻿using CovidDataExtractor.DTO;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using Tesseract;

namespace CovidDataExtractor.Services
{
    public enum CropLocation
    {
        Chilliwack
    }
    public class OcrService : IOcrService
    {
        private static int counter = 0;
        private readonly TesseractEngine engine;
        private IImagePreprocessor processor;
        private static readonly Dictionary<CropLocation, Rectangle> map = new Dictionary<CropLocation, Rectangle>()
        {
              {CropLocation.Chilliwack, new Rectangle(2290, 2098, 70,35) }
        };

        public OcrService(IImagePreprocessor processor)
        {
            this.engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            this.processor = processor;
            DefaultSettings();
        }

        private void DefaultSettings()
        {
            engine.SetVariable("tessedit_char_whitelist", "0123456789"); // show only digits
            engine.SetVariable("load_system_dawg", false); // disable dictionary values
            engine.SetVariable("load_freq_dawg", false); // disable dictionary values
            engine.DefaultPageSegMode = PageSegMode.SingleWord;
        }
        public ProcessedBitmap Extract(ParsedBitmap image, CropLocation location)
        {
            ProcessedBitmap processedBitmap = new ProcessedBitmap(image)
            {
                Location = location
            };
            Bitmap clone = new Bitmap(image.Image);

            Bitmap processed = processor
                .SetImage(clone)
                .Crop(map[location])
                .FloodFill()
                .Resize()
                .AddBorder(Color.White)
                .Binarization()
                .Image;


            processedBitmap.Image = processed;
            processedBitmap.Count = ReadBitmap(processed);
            if (processedBitmap.Count == -1)
            {
                clone = new Bitmap(image.Image);
                processed = processor
                    .SetImage(clone)
                    .Crop(map[location])
                    .FloodFill()
                    .Resize()
                    .AddBorder(Color.White)
                    .Binarization(BinarizationThreshold.Heavy)
                    .Image;
                processedBitmap.Image = processed;
                processedBitmap.Count = ReadBitmap(processed);
            }
            return processedBitmap;
        }
        private int ReadBitmap(Bitmap image)
        {

            ImageConverter converter = new ImageConverter();
            byte[] newimage = (byte[])converter.ConvertTo(image, typeof(byte[]));
            using var page = engine.Process(Pix.LoadFromMemory(newimage));
            string convert = page.GetText();

            if (String.IsNullOrEmpty(convert) || String.IsNullOrWhiteSpace(convert))
                return -1;
            return Convert.ToInt32(convert);
        }

        private static void SaveToFile(Bitmap image)
        {
            image.Save(@"C:\data\" + counter + ".jpg");
            counter++;
        }

    }
}
