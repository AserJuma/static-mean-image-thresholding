using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace study1
{
    public static class BitmapOperations
    {
        public static bool Convert2GrayScaleFast(Bitmap bmp)
        {
            BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0.ToPointer();
                int stopAddress = (int)p + bmData.Stride * bmData.Height;
                while ((int)p != stopAddress)
                {
                    p[0] = (byte)(.299 * p[2] + .587 * p[1] + .114 * p[0]);
                    p[1] = p[0];
                    p[2] = p[0];
                    p += 3;
                }
            }
            bmp.UnlockBits(bmData);
            return true;
        }
        //Compute q values
        private static float Px(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;
            for (i = init; i <= end; i++)
                sum += hist[i];

            return (float)sum;
        }

        // Compute the mean values in the equation
        private static float Mx(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;
            for (i = init; i <= end; i++)
                sum += i * hist[i];

            return (float)sum;
        }

        // Maximum element in a vector
        private static int FindMax(float[] vec, int n)
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

        // Computes the image histogram
        private static unsafe void GetHistogram(byte* p, int w, int h, int ws, int[] hist)
        {
            hist.Initialize();
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w*3; j+=3)
                {
                    int index=i*ws+j;
                    hist[p[index]]++;
                }
            }
        }

        // Find otsu threshold
        private static int GetOtsuThreshold(Bitmap bmp)
        {
            byte threshold = 0;
	        float[] vet=new float[256];
            int[] hist=new int[256];
            vet.Initialize();

	        float p1,p2,p12;
	        int k;

            BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0.ToPointer();

                GetHistogram(p,bmp.Width,bmp.Height,bmData.Stride, hist);

                // loop through all possible threshold values and maximize between class variance
                for (k = 1; k != 255; k++)
                {
                    p1 = Px(0, k, hist);
                    p2 = Px(k + 1, 255, hist);
                    p12 = p1 * p2;
                    if (p12 == 0) 
                        p12 = 1;
                    float diff=(Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1);
                    vet[k] = (float)diff * diff / p12;
                    vet[k] = (float)Math.Pow((Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1), 2) / p12;
                }
            }
            bmp.UnlockBits(bmData);

            threshold = (byte)FindMax(vet, 256);

            return threshold;
        }
        
        public static void ApplyThreshold(Bitmap bmp, int thresholdValue)
        {
            var maxVal = 256;
            if (thresholdValue < 0) return;
            else if (thresholdValue >= maxVal) return;
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                var ptr = (byte*)bmpData.Scan0.ToPointer();
                var stopAddress = (int)ptr + bmpData.Stride * bmpData.Height;

                while ((int)ptr != stopAddress)
                {
                    var totalRgb = (ptr[0] + ptr[1] + ptr[2]) / 3;
                    if (totalRgb <= thresholdValue) 
                    { 
                        ptr[0] = 0; 
                        ptr[1] = 0; 
                        ptr[2] = 0; 
                    }
                    else
                    {
                        ptr[0] = 255;
                        ptr[1] = 255;
                        ptr[2] = 255;
                    }
                    ptr += 3;
                }
            }
            bmp.UnlockBits(bmpData);
        }
        
        public static int GetOptimalThreshold(Bitmap bmp)
        {
            var thresh = GetOtsuThreshold(bmp);
            Console.WriteLine("Optimal Threshold: " + thresh);
            return (thresh);
        }
    }
}
