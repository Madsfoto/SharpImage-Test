using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;

namespace SharpImageTest
{
    // Credit for the idea: https://www.andreweckel.com/LeastAverageImage/

    class Program
    {
        static int ImageWidth = 640; // TODO: Make the size fit the first image the program encounters
        static int ImageHeight = 480;
        static int RGB = 3;

        static int[,,] arr3d = new int[ImageWidth, ImageHeight, RGB]; // pixel X,pixel Y,(R,G,B)

        static int[,,] arr3dDiff = new int[ImageWidth, ImageHeight, RGB]; // pixel X,pixel Y,(R,G,B). Values are the current furthest difference from average


        static int imgNr = 0;

        static void Setarr3dDiff(int x, int y, Rgba32 rgba)
        {
            // read from arr3d[]
            // write new data to arr3dDiff[]
            // Test if current distance to average is larger than previous distance, if so: Set new values

            int redArr = arr3d[x, y, 0];
            int greenArr = arr3d[x, y, 1];
            int blueArr = arr3d[x, y, 2];

            int redDiff = arr3dDiff[x, y, 0];
            int greenDiff = arr3dDiff[x, y, 1];
            int blueDiff = arr3dDiff[x, y, 2];

            int redPixel = rgba.R;
            int greenPixel = rgba.G;
            int bluePixel = rgba.B;



            double euclideanDistanceArr_Pixel = Math.Sqrt(Math.Pow((redArr - redPixel), 2) + Math.Pow((greenArr - greenPixel), 2) + Math.Pow((blueArr - bluePixel), 2));
            double euclideanDistanceDiffArr = Math.Sqrt(Math.Pow((redDiff - redPixel), 2) + Math.Pow((greenDiff - greenPixel), 2) + Math.Pow((blueDiff - bluePixel), 2));




            if (euclideanDistanceArr_Pixel > euclideanDistanceDiffArr)
            {

                arr3dDiff[x, y, 0] = rgba.R;
                arr3dDiff[x, y, 1] = rgba.G;
                arr3dDiff[x, y, 2] = rgba.B;
            }

        }

        static void Setarr3d(int x, int y, Rgba32 rgba)
        {
            arr3d[x, y, 0] = arr3d[x, y, 0] + rgba.R;
            arr3d[x, y, 1] = arr3d[x, y, 1] + rgba.G;
            arr3d[x, y, 2] = arr3d[x, y, 2] + rgba.B;

        }


        static void IncrNrImg()
        {
            imgNr++;
        }

        static void SetPerPixelRGBValues(string image)
        {

            using (Image<Rgba32> sourcePngImage = Image.Load<Rgba32>(image))
            {
                for (int y = 0; y < sourcePngImage.Height; y++)
                {
                    for (int x = 0; x < sourcePngImage.Width; x++)
                    {
                        Rgba32 sourceColor = sourcePngImage[x, y];

                        Setarr3d(x, y, sourceColor); // 1 == Set values in array

                    }
                }

            }


        }

        static void CalculateAverages()
        {
            for (int x = 0; x < ImageWidth; x++)
            {
                for (int y = 0; y < ImageHeight; y++)
                {
                    arr3d[x, y, 0] = (int)Math.Round((double)arr3d[x, y, 0] / imgNr);
                    arr3d[x, y, 1] = (int)Math.Round((double)arr3d[x, y, 1] / imgNr);
                    arr3d[x, y, 2] = (int)Math.Round((double)arr3d[x, y, 2] / imgNr);
                }

            }

        }


        static void SetPixelDistance(string image)
        {
            // From average pixel values in arr3d[x,y,(RGB)]
            // compare each pixel in image with the corresponding pixel at arr3d[],
            // set the furthest pixel values into arr3dDiff[x,y,RGB] (as defined as sqrt((r-r)^2+(g-g)^2+(b-b)^2)
            using (Image<Rgba32> sourcePngImage = Image.Load<Rgba32>(image))
            {
                for (int y = 0; y < sourcePngImage.Height; y++)
                {
                    for (int x = 0; x < sourcePngImage.Width; x++)
                    {
                        Rgba32 sourceColor = sourcePngImage[x, y];

                        Setarr3dDiff(x, y, sourceColor);

                    }
                }

            }


        }



        static Image ArrayToImage(int[,,] arr)
        {
            Image<Rgba32> newImg = new Image<Rgba32>(ImageWidth, ImageHeight);

            for (int x = 0; x < ImageWidth; x++)
            {
                for (int y = 0; y < ImageHeight; y++)
                {
                    newImg[x, y] = new Rgba32(
                    (byte)arr[x, y, 0],
                    (byte)arr[x, y, 1],
                    (byte)arr[x, y, 2]
                );


                }
            }

            return newImg;

        }

        static void WriteArray(int[,,] arr, string filename)
        {
            using (var sw = new StreamWriter(filename + ".txt"))
            {
                for (int i = 0; i < ImageWidth; i++)
                {
                    sw.Write(arr[i, 300, 0] + " ");
                    sw.Write(arr[i, 300, 1] + " ");
                    sw.Write(arr[i, 300, 2] + " ");



                    sw.Write("\n");
                }

                sw.Flush();
                sw.Close();
            }

        }


        static void WritePNG(Image img, string number)
        {
            img.SaveAsPng(number + ".png");
        }

        static void Main(string[] args)
        {
            // 1: List all images in current folder
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<string> fileList = new List<string>();
            foreach (var pngFile in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.png"))
            {
                fileList.Add(pngFile);

            }

            foreach (var pngFile in fileList)
            {
                SetPerPixelRGBValues(pngFile);  // Works 8 mar 2022
                IncrNrImg();
            }

            CalculateAverages();

            Image averageImg = ArrayToImage(arr3d);

            WritePNG(averageImg, "average");


            // Currently have the average pixel value for each pixel in arr3d (of all the images in the current folder)

            long imgNrLong = 0;

            foreach (var pngFile in fileList)
            {
                SetPixelDistance(pngFile);

                Image img = ArrayToImage(arr3dDiff);

                WritePNG(img, "a" + imgNrLong.ToString());
                imgNrLong++;

            }

            sw.Stop();
            Console.WriteLine("elapsed ms " + sw.ElapsedMilliseconds);
        }
    }
}




