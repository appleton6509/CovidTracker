using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CovidDataExtractor.Services
{
    public enum BinarizationThreshold
    {
        Heavy = 25,
        Medium =55,
        Light = 85
    }
    public class ImagePreprocessor : IImagePreprocessor
    {
        private ILogger<ImagePreprocessor> log;
        public Bitmap Image { get; set; }

        public ImagePreprocessor(ILogger<ImagePreprocessor> log) {
            this.log = log;
        }

        public ImagePreprocessor SetImage(Bitmap image)
        {
            this.Image = image;
            return this;
        }
        public ImagePreprocessor AddBorder(Color color)
        {
            try
            {
                const int borderSize = 20;
                int newWidth = Image.Width + (borderSize * 2);
                int newHeight = Image.Height + (borderSize * 2);
                Image newImage = new Bitmap(newWidth, newHeight);
                using (Brush border = new SolidBrush(color))
                {
                    Graphics g = Graphics.FromImage(newImage);
                    g.FillRectangle(border, new Rectangle(0, 0, newWidth, newHeight));
                    g.DrawImage(Image, new Rectangle(borderSize, borderSize, Image.Width, Image.Height));
                }
                Image = (Bitmap)newImage;
            } catch (Exception e)
            {
                log.LogWarning("AddBorder failed: " + e.Message);
            }

            return this;
        }
        /// <summary>
        /// Fills all occurances of a specified color to black
        /// </summary>
        /// <param name="bmp">bitmap to fill</param>
        /// <param name="targetColor">target color to fill</param>
        public ImagePreprocessor FloodFill()
        {
            try
            {
                Color targetColor = Color.FromArgb(255, 255, 255);
                Point a = new Point(0, 0);
                while (a.X < Image.Width)
                {
                    if (a.Y == Image.Height)
                        a.Y = 0;
                    if (Image.GetPixel(a.X, a.Y) == targetColor)
                        Image.SetPixel(a.X, a.Y, Color.Black);

                    while (a.Y < Image.Height)
                    {
                        if (Image.GetPixel(a.X, a.Y) == targetColor)
                            Image.SetPixel(a.X, a.Y, Color.Black);
                        a.Y += 1;
                    }
                    a.X += 1;
                }
            } catch (Exception e)
            {
                log.LogWarning("FloodFill failed: " + e.Message);
            }
 
            return this;
        }
        public ImagePreprocessor Resize()
        {
            Image = new Bitmap(Image, new Size(350, 280));
            return this;
        }
        /// <summary>
        /// Turns a bitmap black and white
        /// </summary>
        /// <param name="thresh"></param>
        /// <returns></returns>
        public ImagePreprocessor Binarization(BinarizationThreshold thresh = BinarizationThreshold.Light)
        {
            try
            {
                // Establish a color object.
                Color curColor;
                int ret;
                // The width of the image.
                for (int iX = 0; iX < Image.Width; iX++)
                {
                    // The height of the image.
                    for (int iY = 0; iY < Image.Height; iY++)
                    {
                        // Get the pixel from bitmap object.
                        curColor = Image.GetPixel(iX, iY);
                        // Transform RGB to Y (gray scale)
                        ret = (int)(curColor.R * 0.299 + curColor.G * 0.578 + curColor.B * 0.114);
                        // This is our threshold, you can change it and to try what are different.
                        if (ret > (int)thresh)
                        {
                            ret = 255;
                        }
                        else
                        {
                            ret = 0;
                        }
                        // Set the pixel into the bitmap object.
                        Image.SetPixel(iX, iY, Color.FromArgb(ret, ret, ret));
                    } // The closing 'The height of the image'.
                } // The closing 'The width of the image'.
            } catch (Exception e)
            {
                log.LogWarning("Binarization Failed: " + e.Message);
            }

            return this;
        }
        public ImagePreprocessor Crop(Rectangle cropArea)
        {
            Image = Image.Clone(cropArea, Image.PixelFormat);
            return this;
        }
    }
}
