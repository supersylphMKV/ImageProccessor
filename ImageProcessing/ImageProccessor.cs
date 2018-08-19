using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing
{
    class ImageProccessor
    {

        public Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        public Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
               });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        public Bitmap ContrastStretch(Bitmap srcImage, double blackPointPercent = 0.02, double whitePointPercent = 0.01)
        {
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, srcImage.Width, srcImage.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            Bitmap destImage = new Bitmap(srcImage.Width, srcImage.Height);
            BitmapData destData = destImage.LockBits(new Rectangle(0, 0, destImage.Width, destImage.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            int stride = srcData.Stride;
            IntPtr srcScan0 = srcData.Scan0;
            IntPtr destScan0 = destData.Scan0;
            var freq = new int[256];

            unsafe
            {
                byte* src = (byte*)srcScan0;
                for (int y = 0; y < srcImage.Height; ++y)
                {
                    for (int x = 0; x < srcImage.Width; ++x)
                    {
                        ++freq[src[y * stride + x * 4]];
                    }
                }

                int numPixels = srcImage.Width * srcImage.Height;
                int minI = 0;
                var blackPixels = numPixels * blackPointPercent;
                int accum = 0;

                while (minI < 255)
                {
                    accum += freq[minI];
                    if (accum > blackPixels) break;
                    ++minI;
                }

                int maxI = 255;
                var whitePixels = numPixels * whitePointPercent;
                accum = 0;

                while (maxI > 0)
                {
                    accum += freq[maxI];
                    if (accum > whitePixels) break;
                    --maxI;
                }
                double spread = 255d / (maxI - minI);
                byte* dst = (byte*)destScan0;
                for (int y = 0; y < srcImage.Height; ++y)
                {
                    for (int x = 0; x < srcImage.Width; ++x)
                    {
                        int i = y * stride + x * 4;

                        byte val = (byte)Clamp(Math.Round((src[i] - minI) * spread), 0, 255);
                        dst[i] = val;
                        dst[i + 1] = val;
                        dst[i + 2] = val;
                        dst[i + 3] = 255;
                    }
                }
            }

            srcImage.UnlockBits(srcData);
            destImage.UnlockBits(destData);

            return destImage;
        }



        static double Clamp(double val, double min, double max)
        {
            return Math.Min(Math.Max(val, min), max);
        }

        public int[,] sudut0(int[,] matrik, Image gmbrAasli)
        {
            int jarak = 0, nilai = 0;
            int[,] runLength = new int[16, Math.Min(gmbrAasli.Height, gmbrAasli.Width)];

            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < Math.Min(gmbrAasli.Height, gmbrAasli.Width); j++)
                {
                    runLength[i, j] = 0;
                }
            }
            for (int x = 0; x < gmbrAasli.Width; x++)
            {
                for (int y = 0; y < gmbrAasli.Height; y++)
                {
                    if (nilai == matrik[x, y])
                    {
                        jarak++; ;
                        if (y == gmbrAasli.Height - 1)
                        {
                            runLength[nilai, jarak - 1]++;
                            jarak = 0;
                        }
                    }
                    else
                    {
                        if (y == 0)
                        {
                            nilai = matrik[x, y];
                            jarak++;
                        }
                        else
                        {
                            runLength[nilai, jarak - 1]++;
                            nilai = matrik[x, y];
                            jarak = 1;
                            if (y == gmbrAasli.Height - 1)
                            {
                                runLength[nilai, jarak - 1]++;
                                jarak = 0;
                            }
                        }
                    }
                }
            }
            return runLength;
        }

        public int[,] sudut90(int[,] matrik, Image gmbrAasli)

        {
            int jarak = 0, nilai = 0;
            int[,] runLength = new int[16, Math.Min(gmbrAasli.Height, gmbrAasli.Width)];

            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < Math.Min(gmbrAasli.Height, gmbrAasli.Width); j++)
                {
                    runLength[i, j] = 0;
                }
            }
            for (int y = 0; y < gmbrAasli.Height; y++)
            {
                for (int x = 0; x < gmbrAasli.Width; x++)
                {
                    if (nilai == matrik[x, y])
                    {
                        jarak++; ;
                        if (x == gmbrAasli.Width - 1)
                        {
                            runLength[nilai, jarak - 1]++;
                            jarak = 0;
                        }
                    }
                    else
                    {
                        if (x == 0)
                        {
                            nilai = matrik[x, y];
                            jarak++;
                        }
                        else
                        {
                            runLength[nilai, jarak - 1]++;
                            nilai = matrik[x, y];
                            jarak = 1;
                            if (x == gmbrAasli.Width - 1)
                            {
                                runLength[nilai, jarak - 1]++;
                                jarak = 0;
                            }
                        }
                    }
                }
            }
            return runLength;
        }

        public int[,] sudut135(int[,] matrik, Image gmbrAasli)
        {
            //Bitmap u = new Bitmap(gambar);
            int jarak = 0, nilai = 0;
            int[,] runLength = new int[16, Math.Min(gmbrAasli.Height, gmbrAasli.Width)];

            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < Math.Min(gmbrAasli.Height, gmbrAasli.Width); j++)
                {
                    runLength[i, j] = 0;
                }
            }
            for (int m = gmbrAasli.Height - 1; m >= 0; m--)
            {
                if (m == 0)
                {
                    int tanda = 0;
                    for (int n = gmbrAasli.Width; n > 0; n--)
                    {
                        int x = tanda;
                        for (int y = 0; y < n; y++)
                        {
                            if (y == gmbrAasli.Height)
                            {
                                break;
                            }
                            //*****************************
                            //System.out.print("| "+x+", "+y+" |");
                            if (x == gmbrAasli.Width - 1 && y == 0)
                            {
                                runLength[matrik[x, y], 0]++;
                            }
                            else
                            {
                                if (nilai == matrik[x, y])
                                {
                                    jarak++; ;
                                    if (x == gmbrAasli.Width - 1 || y == gmbrAasli.Height - 1)
                                    {
                                        runLength[nilai, jarak - 1]++;
                                        jarak = 0;
                                    }
                                }
                                else
                                {
                                    if (y == 0 || x == 0)
                                    {
                                        nilai = matrik[x, y];
                                        jarak++;
                                    }
                                    else
                                    {
                                        runLength[nilai, jarak - 1]++;
                                        nilai = matrik[x, y];
                                        jarak = 1;
                                        if (x == gmbrAasli.Width - 1 || y == gmbrAasli.Height - 1)
                                        {
                                            runLength[nilai, jarak - 1]++;
                                            jarak = 0;
                                        }
                                    }
                                }
                            }
                            //********************************
                            x++;
                        }
                        //System.out.println("");
                        tanda++;
                    }
                }
                else
                {
                    int x = 0;
                    for (int y = m; y < gmbrAasli.Height; y++)
                    {
                        if (x == gmbrAasli.Width)
                        {
                            break;
                        }
                        //******************************
                        //System.out.print("| "+x+", "+y+" |");
                        if (x == gmbrAasli.Width - 1 && y == gmbrAasli.Height - 1)
                        {
                            runLength[matrik[x, y], 0]++;
                        }
                        else
                        {
                            if (nilai == matrik[x, y])
                            {
                                jarak++; ;
                                if (x == gmbrAasli.Width - 1 || y == gmbrAasli.Height - 1)
                                {
                                    runLength[nilai, jarak - 1]++;
                                    jarak = 0;
                                }
                            }
                            else
                            {
                                if (y == 0 || x == 0)
                                {
                                    nilai = matrik[x, y];
                                    jarak++;
                                }
                                else
                                {
                                    runLength[nilai, jarak - 1]++;
                                    nilai = matrik[x, y];
                                    jarak = 1;
                                    if (x == gmbrAasli.Width - 1 || y == gmbrAasli.Height - 1)
                                    {
                                        runLength[nilai, jarak - 1]++;
                                        jarak = 0;
                                    }
                                }
                            }
                        }
                        //*******************************
                        x++;
                    }
                    //System.out.println("");
                }
            }
            return runLength;
        }

        public int[,] sudut45(int[,] matrik, Image gmbrAasli)
        {
            //Bitmap u = new Bitmap(gambar);
            int jarak = 0, nilai = 0;
            int[,] runLength = new int[16, Math.Min(gmbrAasli.Height, gmbrAasli.Width)];

            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < Math.Min(gmbrAasli.Height, gmbrAasli.Width); j++)
                {
                    runLength[i, j] = 0;
                }
            }
            for (int i = 0; i < gmbrAasli.Width; i++)
            {
                int y = 0;
                for (int x = i; x >= 0; x--)
                {

                    //*********************
                    //Color c1 = u.GetPixel(x, y);
                    if (x == 0 && y == 0)
                    {
                        runLength[matrik[x, y], 0]++;
                    }
                    else
                    {
                        if (nilai == matrik[x, y])
                        {
                            jarak++; ;
                            if (x == 0 || y == gmbrAasli.Height - 1)
                            {
                                runLength[nilai, jarak - 1]++;
                                jarak = 0;
                            }
                        }
                        else
                        {
                            if (y == 0 || x == gmbrAasli.Width - 1)
                            {
                                nilai = matrik[x, y];
                                jarak++;
                            }
                            else
                            {
                                runLength[nilai, jarak - 1]++;
                                nilai = matrik[x, y];
                                jarak = 1;
                                if (x == 0 || y == gmbrAasli.Height - 1)
                                {
                                    runLength[nilai, jarak - 1]++;
                                    jarak = 0;
                                }
                            }
                        }
                    }
                    //************************
                    //System.out.print("|"+x+","+y+"|");
                    if (y == gmbrAasli.Height - 1)
                    {
                        break;
                    }
                    y++;
                }
                //System.out.println();
                if (i == gmbrAasli.Width - 1)
                {
                    for (int j = 1; j < gmbrAasli.Height; j++)
                    {
                        int x = gmbrAasli.Width - 1;
                        for (y = j; y < gmbrAasli.Height; y++)
                        {
                            //Color c1 = u.GetPixel(x, y);
                            //System.out.print("|"+x+","+y+"|");
                            //*********************
                            if (x == gmbrAasli.Width - 1 && y == gmbrAasli.Height - 1)
                            {
                                runLength[matrik[x, y], 0]++;
                            }
                            else
                            {
                                if (nilai == matrik[x, y])
                                {
                                    jarak++; ;
                                    if (x == 0 || y == gmbrAasli.Height - 1)
                                    {
                                        runLength[nilai, jarak - 1]++;
                                        jarak = 0;
                                    }
                                }
                                else
                                {
                                    if (y == 0 || x == gmbrAasli.Width - 1)
                                    {
                                        nilai = matrik[x, y];
                                        jarak++;
                                    }
                                    else
                                    {
                                        runLength[nilai, jarak - 1]++;
                                        nilai = matrik[x, y];
                                        jarak = 1;
                                        if (x == 0 || y == gmbrAasli.Height - 1)
                                        {
                                            runLength[nilai, jarak - 1]++;
                                            jarak = 0;
                                        }
                                    }
                                }
                            }
                            //************************
                            if (x == 0)
                            {
                                break;
                            }
                            x--;
                        }
                        //System.out.println();
                    }
                }
            }
            return runLength;
        }

        public int[,] grayLevel(Image gambar)
        {
            Bitmap b = new Bitmap(gambar);
            int[,] penyimpan = new int[b.Width, b.Height];
            //penyimpan[1, 2] = 1;

            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    Color c2 = b.GetPixel(i, j);

                    if (c2.R >= 0 && c2.R <= 15)
                    {
                        penyimpan[i, j] = 0;
                    }
                    else if (c2.R >= 16 && c2.R <= 31)
                    {
                        penyimpan[i, j] = 1;
                    }
                    else if (c2.R >= 32 && c2.R <= 47)
                    {
                        penyimpan[i, j] = 2;
                    }
                    else if (c2.R >= 48 && c2.R <= 63)
                    {
                        penyimpan[i, j] = 3;
                    }
                    else if (c2.R >= 64 && c2.R <= 79)
                    {
                        penyimpan[i, j] = 4;
                    }
                    else if (c2.R >= 80 && c2.R <= 95)
                    {
                        penyimpan[i, j] = 5;
                    }

                    else if (c2.R >= 96 && c2.R <= 111)
                    {
                        penyimpan[i, j] = 6;
                    }
                    else if (c2.R >= 112 && c2.R <= 127)
                    {
                        penyimpan[i, j] = 7;
                    }

                    else if (c2.R >= 128 && c2.R <= 143)
                    {
                        penyimpan[i, j] = 8;
                    }
                    else if (c2.R >= 144 && c2.R <= 159)
                    {
                        penyimpan[i, j] = 9;
                    }
                    else if (c2.R >= 160 && c2.R <= 175)
                    {
                        penyimpan[i, j] = 10;
                    }
                    else if (c2.R >= 176 && c2.R <= 191)
                    {
                        penyimpan[i, j] = 11;
                    }

                    else if (c2.R >= 192 && c2.R <= 207)
                    {
                        penyimpan[i, j] = 12;
                    }
                    else if (c2.R >= 208 && c2.R <= 223)
                    {
                        penyimpan[i, j] = 13;
                    }
                    else if (c2.R >= 224 && c2.R <= 239)
                    {
                        penyimpan[i, j] = 14;
                    }
                    else if (c2.R >= 240 && c2.R <= 255)
                    {
                        penyimpan[i, j] = 15;
                    }
                    else
                    {
                        Console.WriteLine("Rusak" + penyimpan[i, j]);
                    }
                    //richTextBox2.Text = richTextBox2.Text + "[" + i + "][" + j + "]:" + penyimpan[i, j] + "/r/n";
                    //Console.WriteLine("R[" + i + "][" + j + "]:" + penyimpan[i, j]);
                }
                //richTextBox2.Text = richTextBox2.Text + "/r/n";
            }
            return penyimpan;
        }
    }
}
