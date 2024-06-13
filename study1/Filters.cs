using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace study1
{
    public static class BitmapOperations
    {
        /*public static bool GreyScale(Bitmap b)
        {
            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - b.Width * 3;

                byte red, green, blue;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];

                        //string pixel_bgr_values = ("B:" + blue + ", G:" + green + ", R:" + red);
                        //Console.WriteLine(pixel_bgr_values);

                        p[0] = p[1] = p[2] = (byte)(.299 * red + .587 * green + .114 * blue);

                        p += 3;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);

            return true;
        }*/
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

        private static float Px(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;
            for (i = init; i <= end; i++)
                sum += hist[i];

            return (float)sum;
        }

        // function is used to compute the mean values in the equation (mu)
        private static float Mx(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;
            for (i = init; i <= end; i++)
                sum += i * hist[i];

            return (float)sum;
        }

        // finds the maximum element in a vector
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

        // simply computes the image histogram
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

        // find otsu threshold
        public static int GetOtsuThreshold(Bitmap bmp)
        {
            byte t=0;
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

                // loop through all possible t values and maximize between class variance
                for (k = 1; k != 255; k++)
                {
                    p1 = Px(0, k, hist);
                    p2 = Px(k + 1, 255, hist);
                    p12 = p1 * p2;
                    if (p12 == 0) 
                        p12 = 1;
                    float diff=(Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1);
                    vet[k] = (float)diff * diff / p12;
                    //vet[k] = (float)Math.Pow((Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1), 2) / p12;
                }
            }
            bmp.UnlockBits(bmData);

            t = (byte)FindMax(vet, 256);

            return t;
        }
        
        public static int GetOptimalThreshold(Bitmap bmp)
        {
            var thresh = GetOtsuThreshold(bmp);
            Console.WriteLine("Optimal Threshold: " + thresh);
            return (thresh);
        }
    }
}
