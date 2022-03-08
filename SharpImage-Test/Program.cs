using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;

// for all images in current folder
// { pick 30 at random and put in new list
// for each image in list
//  { Find the color triplet values for each pixel and put it in a double list OR one long list, write the list with the image filename }
// Get the average image color of the 30 images
// 

// Average for each pixel:
// 3d array: [x,y,z]
// 

namespace SharpImageTest
{
    class Program
    {

        static int ImageWidth = 640;
        static int ImageHeight = 480;
        static int RGB = 3;
        static int[,,] arr3d = new int[ImageWidth, ImageHeight, RGB]; // pixel X,pixel Y,(R,G,B)
        // Idea is to take the RGB values of each pixel, add them to the 3D array, average all the 3rd dimention values (eg: all red / number of images)
        static int[,,] arr3dDiff = new int[ImageWidth, ImageHeight, RGB];

        
        static int imgNr = 0;
        
        static void SetRGBArray(int x, int y, Rgba32 rgba, int function)
        { 
            // Function list:
            // 1: Set values in the array
            // 2: Test if current distance to average is larger than previous distance, if so: Set new values

            if(function==1)
            {
                arr3d[x, y, 0] = arr3d[x, y, 0] + rgba.R;
                arr3d[x, y, 1] = arr3d[x, y, 1] + rgba.G;
                arr3d[x, y, 2] = arr3d[x, y, 2] + rgba.B;
                
            }
            else if (function==2)
            {
                // read from arr3d[]
                // write new data to arr3dDiff[]
                int redArr = arr3d[x, y, 0]; // has values
                int greenArr = arr3d[x, y, 1]; // has values
                int blueArr = arr3d[x, y, 2];// has values

                int redDiff = arr3dDiff[x, y, 0]; // has values
                int greenDiff = arr3dDiff[x, y, 1]; // has values
                int blueDiff = arr3dDiff[x, y, 2];// has values

                int redPixel = rgba.R; // has values
                int greenPixel = rgba.G;// has values
                int bluePixel = rgba.B;// has values

                

                double euclideanDistanceArr_Pixel = Math.Sqrt(Math.Pow((redArr - redPixel),2) + Math.Pow((greenArr - greenPixel),2) + Math.Pow((blueArr - bluePixel),2));
                double euclideanDistanceDiffArr = Math.Sqrt(Math.Pow((redDiff - redPixel),2) + Math.Pow((greenDiff - greenPixel),2) + Math.Pow((blueDiff - bluePixel),2));
                
            


                if (euclideanDistanceArr_Pixel > euclideanDistanceDiffArr)
                {
                    
                    arr3dDiff[x, y, 0] = rgba.R;
                    arr3dDiff[x, y, 1] = rgba.G;
                    arr3dDiff[x, y, 2] = rgba.B;
                }

            }



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

                        SetRGBArray(x, y, sourceColor, 1); // 1 == Set values in array

                    }
                }

            }
            IncrNrImg();

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

                        SetRGBArray(x, y, sourceColor, 2); // 2 == set the furthest distance from average

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
            using (var sw = new StreamWriter(filename+".txt"))
            {
                for (int i = 0; i< ImageWidth; i++)
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
            img.SaveAsPng(number+".png");
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
            }
            WriteArray(arr3d, "total_Values");


            CalculateAverages(); // Works 8 mar 2022
            WriteArray(arr3d, "average_Values");

            Image averageImg = ArrayToImage(arr3d);

            WritePNG(averageImg, "average");


            // Currently have the average pixel value for each pixel in arr3d (of all the images in the current folder)

            long imgNrLong = 0;

            foreach (var pngFile in fileList)
            {
                SetPixelDistance(pngFile);

                Image img = ArrayToImage(arr3dDiff);

                WritePNG(img, "a"+imgNrLong.ToString());
                imgNrLong++;

                //Console.WriteLine(arr3dDiff[300, 300+imgNrLong, 0]);
            }




            WriteArray(arr3dDiff, "Diff_Values");



            sw.Stop();
            Console.WriteLine("elapsed ms " + sw.ElapsedMilliseconds);
        }
    }
}




