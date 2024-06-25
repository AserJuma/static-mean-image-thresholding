using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace study1
{
    public static class BitmapOperations
    {
        public static bool Convert2GrayScaleFast(Bitmap bmp) // Not a true conversion, treats it as a 24bit rgb image with all equal values.
        {
            BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0.ToPointer();
                int stopAddress = (int)p + bmData.Stride * bmData.Height;
                
                while ((int)p != stopAddress)
                {
                    p[0] = (byte)(p[0] * .114 + p[1] * .587 + p[2] * .299);
                    p[1] = p[0];
                    p[2] = p[0];
                    p += 3;
                }
            }
            bmp.UnlockBits(bmData);
            return true;
        }
        
        
        // Otsu helper function, Computes image histogram of pixel intensities
        // Initializes an array, iterates through and fills up histogram count values
        private static unsafe void GetHistogram(byte* pt, int width, int height, int stride, int[] histArr)
        {
            histArr.Initialize();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width * 3; j += 3)
                {
                    int index = i * stride + j;
                    histArr[pt[index]]++;
                }
            }
        }
        
        // Otsu helper function, Compute q values
        // Gets the sum of some histogram values within an intensity range
        private static float Px(int init, int end, int[] hist) 
        {
            int sum = 0;
            int i;
            for (i = init; i <= end; i++)
                sum += hist[i];

            return (float)sum;
        }
        
        // Otsu helper function, Get the mean values in the equation
        // Gets weighted sum of histogram values in an intensity range
        private static float Mx(int init, int end, int[] hist) 
        {
            int sum = 0;
            int i;
            for (i = init; i <= end; i++)
                sum += i * hist[i];

            return (float)sum;
        }

        // Otsu helper function, Maximum element
        private static int FindMax(float[] vec, int n) // Returns index of maximum float value in array
        {
            float maxVec = 0;
            int idx=0;
            int i;
            for (i = 1; i < n - 1; i++)
            {
                if (vec[i] > maxVec)
                {
                    maxVec = vec[i];
                    idx = i;
                }
            }
            return idx;
        }

        // Otsu's threshold
        private static int GetOtsuThreshold(Bitmap bmp)
        {
            byte threshold = 0;
	        float[] vet = new float[256];
            int[] hist = new int[256];
            vet.Initialize();

	        float p1, p2, p12;
	        int k;

            BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0.ToPointer();

                GetHistogram(p,bmp.Width,bmp.Height,bmData.Stride, hist); // Fills up an array with pixel intensity values

                // loop through all possible threshold values and maximize between class variance
                for (k = 1; k != 255; k++)
                {
                    p1 = Px(0, k, hist); // foreground
                    p2 = Px(k + 1, 255, hist); // background
                    // Continually sums up histogram values in different ranges, covering the span of the image data, in two float values p1, p2
                    p12 = p1 * p2; // product of probabilities p1, p2
                    if (p12 == 0) 
                        p12 = 1;
                    float diff = (Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1);
                    
                    vet[k] = (float)diff * diff / p12; // Computes and stores variance values for each threshold value using simple variance formula from statistics.
                    //vet[k] = (float)Math.Pow((Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1), 2) / p12; // Another way to compute variance (more overhead/overly complex)
                }
            }
            bmp.UnlockBits(bmData);

            threshold = (byte)FindMax(vet, 256); // Finds maximum variance value

            return threshold;
        }
        
        public static void ApplyThreshold(Bitmap bmp, int thresholdValue)
        {
            var maxVal = 256;
            if (thresholdValue < 0) return;
            else if (thresholdValue >= maxVal) return;
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb); // 8bit grayscale: Format8bppIndexed
            unsafe
            {
                var ptr = (byte*)bmpData.Scan0.ToPointer();
                var stopAddress = (int)ptr + bmpData.Stride * bmpData.Height;

                while ((int)ptr != stopAddress)
                {
                    //var totalRgb = (ptr[0] + ptr[1] + ptr[2]) / 3;
                    if (ptr[0] <= thresholdValue) // Since they are all the same values in a 24bit format, it's enough to check the first value (ptr[0])
                    { 
                        ptr[0] = 0; ptr[1] = 0; ptr[2] = 0; 
                    }
                    else
                    {
                        ptr[0] = 255; ptr[1] = 255; ptr[2] = 255;
                    }
                    ptr += 3;
                }
            }
            bmp.UnlockBits(bmpData);
        }
        // Runs Otsu's method group
        public static int GetOptimalThreshold(Bitmap bmp)
        {
            var threshold = GetOtsuThreshold(bmp);
            // Console.WriteLine("Optimal Threshold: " + thresh);
            return (threshold);
        }
    }
}
