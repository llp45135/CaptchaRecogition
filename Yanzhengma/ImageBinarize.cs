using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CaptchaRecogition
{
    /// <summary>
    /// 图像二值化方法：大津法和迭代法
    /// </summary>
    public enum BinarizationMethods
    {
        Otsu,       // 大津法
        Iterative   // 迭代法
    }

    /// <summary>
    /// WinForm：图像的二值化
    /// </summary>
    public static partial class ImageUtils
    {
        /// <summary>
        /// 全局阈值图像二值化
        /// </summary>
        /// <param name="bmp">原始图像</param>
        /// <param name="method">二值化方法</param>
        /// <param name="threshold">输出：全局阈值</param>
        /// <returns>二值化后的图像数组</returns>        
        public static Byte[,] ToBinaryArray(this Bitmap bmp, BinarizationMethods method, out Int32 threshold)
        {   // 位图转换为灰度数组
            Byte[,] GrayArray = bmp.ToGrayArray();

            // 计算全局阈值
            if (method == BinarizationMethods.Otsu)
                threshold = OtsuThreshold(GrayArray);
            else
                threshold = IterativeThreshold(GrayArray);

            // 根据阈值进行二值化
            Int32 PixelHeight = bmp.Height;
            Int32 PixelWidth = bmp.Width;
            Byte[,] BinaryArray = new Byte[PixelHeight, PixelWidth];
            for (Int32 i = 0; i < PixelHeight; i++)
            {
                for (Int32 j = 0; j < PixelWidth; j++)
                {
                    BinaryArray[i, j] = Convert.ToByte((GrayArray[i, j] > threshold) ? 255 : 0);
                }
            }

            return BinaryArray;
        }

        /// <summary>
        /// 全局阈值图像二值化
        /// </summary>
        /// <param name="bmp">原始图像</param>
        /// <param name="method">二值化方法</param>
        /// <param name="threshold">输出：全局阈值</param>
        /// <returns>二值化图像</returns>
        public static Bitmap ToBinaryBitmap(this Bitmap bmp, BinarizationMethods method, out Int32 threshold)
        {   // 位图转换为灰度数组
            Byte[,] GrayArray = bmp.ToGrayArray();

            // 计算全局阈值
            if (method == BinarizationMethods.Otsu)
                threshold = OtsuThreshold(GrayArray);
            else
                threshold = IterativeThreshold(GrayArray);

            // 将灰度数组转换为二值数据
            Int32 PixelHeight = bmp.Height;
            Int32 PixelWidth = bmp.Width;
            Int32 Stride = ((PixelWidth + 31) >> 5) << 2;
            Byte[] Pixels = new Byte[PixelHeight * Stride];
            for (Int32 i = 0; i < PixelHeight; i++)
            {
                Int32 Base = i * Stride;
                for (Int32 j = 0; j < PixelWidth; j++)
                {
                    if (GrayArray[i, j] > threshold)
                    {
                        Pixels[Base + (j >> 3)] |= Convert.ToByte(0x80 >> (j & 0x7));
                    }
                }
            }

            // 从二值数据中创建黑白图像
            Bitmap BinaryBmp = new Bitmap(PixelWidth, PixelHeight, PixelFormat.Format1bppIndexed);

            // 设置调色表
            ColorPalette cp = BinaryBmp.Palette;
            cp.Entries[0] = Color.Black;    // 黑色
            cp.Entries[1] = Color.White;    // 白色
            BinaryBmp.Palette = cp;

            // 设置位图图像特性
            BitmapData BinaryBmpData = BinaryBmp.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
            Marshal.Copy(Pixels, 0, BinaryBmpData.Scan0, Pixels.Length);
            BinaryBmp.UnlockBits(BinaryBmpData);

            return BinaryBmp;
        }

        /// <summary>
        /// 大津法计算阈值
        /// </summary>
        /// <param name="grayArray">灰度数组</param>
        /// <returns>二值化阈值</returns> 
        public static Int32 OtsuThreshold(Byte[,] grayArray)
        {   
            // 建立统计直方图
            Int32[] Histogram = new Int32[256];
            Array.Clear(Histogram, 0, 256);     // 初始化
            foreach (Byte b in grayArray)
            {
                Histogram[b]++;                 // 统计直方图
            }

            // 总的质量矩和图像点数
            Int32 SumC = grayArray.Length;    // 总的图像点数
            Double SumU = 0;                  // 双精度避免方差运算中数据溢出
            for (Int32 i = 1; i < 256; i++)
            {
                SumU += i * Histogram[i];     // 总的质量矩                
            }

            // 灰度区间
            Int32 MinGrayLevel = Array.FindIndex(Histogram, NonZero);       // 最小灰度值
            Int32 MaxGrayLevel = Array.FindLastIndex(Histogram, NonZero);   // 最大灰度值

            // 计算最大类间方差
            Int32 Threshold = MinGrayLevel;
            Double MaxVariance = 0.0;       // 初始最大方差
            Double U0 = 0;                  // 初始目标质量矩
            Int32 C0 = 0;                   // 初始目标点数
            for (Int32 i = MinGrayLevel; i < MaxGrayLevel; i++)
            {
                if (Histogram[i] == 0) continue;

                // 目标的质量矩和点数                
                U0 += i * Histogram[i];
                C0 += Histogram[i];

                // 计算目标和背景的类间方差
                Double Diference = U0 * SumC - SumU * C0;
                Double Variance = Diference * Diference / C0 / (SumC - C0); // 方差
                if (Variance > MaxVariance)
                {
                    MaxVariance = Variance;
                    Threshold = i;
                }
            }

            // 返回类间方差最大阈值
            return Threshold;
        }

        /// <summary>
        /// 检测非零值
        /// </summary>
        /// <param name="value">要检测的数值</param>
        /// <returns>
        ///     true：非零
        ///     false：零
        /// </returns>
        private static Boolean NonZero(Int32 value)
        {
            return (value != 0) ? true : false;
        }

        /// <summary>
        /// 迭代法计算阈值
        /// </summary>
        /// <param name="grayArray">灰度数组</param>
        /// <returns>二值化阈值</returns> 
        public static Int32 IterativeThreshold(Byte[,] grayArray)
        {   // 建立统计直方图
            Int32[] Histogram = new Int32[256];
            Array.Clear(Histogram, 0, 256);     // 初始化
            foreach (Byte b in grayArray)
            {
                Histogram[b]++;                 // 统计直方图
            }

            // 总的质量矩和图像点数
            Int32 SumC = grayArray.Length;    // 总的图像点数
            Int32 SumU = 0;
            for (Int32 i = 1; i < 256; i++)
            {
                SumU += i * Histogram[i];     // 总的质量矩                
            }

            // 确定初始阈值
            Int32 MinGrayLevel = Array.FindIndex(Histogram, NonZero);       // 最小灰度值
            Int32 MaxGrayLevel = Array.FindLastIndex(Histogram, NonZero);   // 最大灰度值
            Int32 T0 = (MinGrayLevel + MaxGrayLevel) >> 1;
            if (MinGrayLevel != MaxGrayLevel)
            {
                for (Int32 Iteration = 0; Iteration < 100; Iteration++)
                {   // 计算目标的质量矩和点数
                    Int32 U0 = 0;
                    Int32 C0 = 0;
                    for (Int32 i = MinGrayLevel; i <= T0; i++)
                    {   // 目标的质量矩和点数                
                        U0 += i * Histogram[i];
                        C0 += Histogram[i];
                    }

                    // 目标的平均灰度值和背景的平均灰度值的中心值
                    Int32 T1 = (U0 / C0 + (SumU - U0) / (SumC - C0)) >> 1;
                    if (T0 == T1) break; else T0 = T1;
                }
            }

            // 返回最佳阈值
            return T0;
        }
    }
}
