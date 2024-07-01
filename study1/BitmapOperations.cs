using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace study1
{
    public static class BitmapOperations
    {
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

            return sum;
        }
        
        // Otsu helper function, Get the mean values in the equation
        // Gets weighted sum of histogram values in an intensity range
        private static float Mx(int init, int end, int[] hist) 
        {
            int sum = 0;
            int i;
            for (i = init; i <= end; i++)
                sum += i * hist[i];

            return sum;
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
            float[] vet = new float[256];
            int[] hist = new int[256];
            vet.Initialize();

            BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadOnly, bmp.PixelFormat);
            unsafe
            {
                byte* p = (byte*)bmData.Scan0.ToPointer();
                GetHistogram(p,bmp.Width,bmp.Height,bmData.Stride, hist); // Fills up an array with pixel intensity values
                // loop through all possible threshold values and maximize between-class variance
                for (int k = 1; k != 255; k++)
                {
                    var p1 = Px(0, k, hist);
                    var p2 = Px(k + 1, 255, hist);
                    // Continually sums up histogram values in different ranges, covering the span of the image data, in two float values p1, p2
                    var p12 = p1 * p2;
                    if (p12 == 0) 
                        p12 = 1;
                    float diff = (Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1);
                    vet[k] = diff * diff / p12; // Computes and stores variance values for each threshold value using simple variance formula from statistics.
                    //vet[k] = (float)Math.Pow((Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1), 2) / p12; // Another way to compute variance (more overhead/overly complex)
                }
            }
            bmp.UnlockBits(bmData);

            return (byte)FindMax(vet, 256); // Finds maximum variance value
        }
        
        public static Bitmap ApplyThreshold(Bitmap bmp, int thresholdValue)
        {
            const int maxVal = 256;
            if (thresholdValue >= maxVal) return null;
            
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            int size = (bmpData.Stride * bmp.Height);
            var counter = 0;
            unsafe
            {
                var pt = (byte*)bmpData.Scan0.ToPointer();
                while (counter < size)
                {
                    *pt = (byte)(*pt <= thresholdValue ? 0 : 255);
                    pt += 1;
                    counter++;
                }
            }
            bmp.UnlockBits(bmpData);
            return bmp;
        }
        public static int GetOptimalThreshold(Bitmap bmp)
        {
            if (bmp.Height == 0 || bmp.Width == 0) return (-1);
            return GetOtsuThreshold(bmp);
        }
    }
}


// TODO : Check Bitmap data before processing. OK
// TODO : Return a new bitmap after processing. OK