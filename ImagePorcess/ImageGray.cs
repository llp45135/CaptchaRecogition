using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CaptchaRecogition
{
    /// <summary>
    /// WinForm：图像的灰度化
    /// </summary>
    public static partial class ImageUtils
    {
        /// <summary>
        /// 将位图转换为彩色数组
        /// </summary>
        /// <param name="bmp">原始位图</param>
        /// <returns>彩色数组</returns>
        public static Color[,] ToColorArray(this Bitmap bmp)
        {
            Int32 PixelHeight = bmp.Height; // 图像高度
            Int32 PixelWidth = bmp.Width;   // 图像宽度
            Int32[] Pixels = new Int32[PixelHeight * PixelWidth];

            // 锁定位图到系统内存
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bmpData.Scan0, Pixels, 0, Pixels.Length);  // 从非托管内存拷贝数据到托管内存
            bmp.UnlockBits(bmpData);    // 从系统内存解锁位图

            // 将像素数据转换为彩色数组
            Color[,] ColorArray = new Color[PixelHeight, PixelWidth];
            for (Int32 i = 0; i < PixelHeight; i++)
            {
                for (Int32 j = 0; j < PixelWidth; j++)
                {
                    ColorArray[i, j] = Color.FromArgb(Pixels[i * PixelWidth + j]);
                }
            }

            return ColorArray;
        }

        /// <summary>
        /// 将位图转换为灰度数组（256级灰度）
        /// </summary>
        /// <param name="bmp">原始位图</param>
        /// <returns>灰度数组</returns>
        public static Byte[,] ToGrayArray(this Bitmap bmp)
        {
            Int32 PixelHeight = bmp.Height; // 图像高度
            Int32 PixelWidth = bmp.Width;   // 图像宽度
            Int32 Stride = ((PixelWidth * 3 + 3) >> 2) << 2;    // 跨距宽度
            Byte[] Pixels = new Byte[PixelHeight * Stride];

            // 锁定位图到系统内存
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(bmpData.Scan0, Pixels, 0, Pixels.Length);  // 从非托管内存拷贝数据到托管内存
            bmp.UnlockBits(bmpData);    // 从系统内存解锁位图

            // 将像素数据转换为灰度数组
            Byte[,] GrayArray = new Byte[PixelHeight, PixelWidth];
            for (Int32 i = 0; i < PixelHeight; i++)
            {
                Int32 Index = i * Stride;
                for (Int32 j = 0; j < PixelWidth; j++)
                {
                    GrayArray[i, j] = Convert.ToByte((Pixels[Index + 2] * 19595 + Pixels[Index + 1] * 38469 + Pixels[Index] * 7471 + 32768) >> 16);
                    Index += 3;
                }
            }

            return GrayArray;
        }

        /// <summary>
        /// 位图灰度化
        /// </summary>
        /// <param name="bmp">原始位图</param>
        /// <returns>灰度位图</returns>
        public static Bitmap ToGrayBitmap(this Bitmap bmp)
        {
            Int32 PixelHeight = bmp.Height; // 图像高度
            Int32 PixelWidth = bmp.Width;   // 图像宽度
            Int32 Stride = ((PixelWidth * 3 + 3) >> 2) << 2;    // 跨距宽度
            Byte[] Pixels = new Byte[PixelHeight * Stride];

            // 锁定位图到系统内存
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(bmpData.Scan0, Pixels, 0, Pixels.Length);  // 从非托管内存拷贝数据到托管内存
            bmp.UnlockBits(bmpData);    // 从系统内存解锁位图

            // 将像素数据转换为灰度数据
            Int32 GrayStride = ((PixelWidth + 3) >> 2) << 2;
            Byte[] GrayPixels = new Byte[PixelHeight * GrayStride];
            for (Int32 i = 0; i < PixelHeight; i++)
            {
                Int32 Index = i * Stride;
                Int32 GrayIndex = i * GrayStride;
                for (Int32 j = 0; j < PixelWidth; j++)
                {
                    GrayPixels[GrayIndex++] = Convert.ToByte((Pixels[Index + 2] * 19595 + Pixels[Index + 1] * 38469 + Pixels[Index] * 7471 + 32768) >> 16);
                    Index += 3;
                }
            }

            // 创建灰度图像
            Bitmap GrayBmp = new Bitmap(PixelWidth, PixelHeight, PixelFormat.Format8bppIndexed);

            // 设置调色表
            ColorPalette cp = GrayBmp.Palette;
            for (int i = 0; i < 256; i++) cp.Entries[i] = Color.FromArgb(i, i, i);
            GrayBmp.Palette = cp;

            // 设置位图图像特性
            BitmapData GrayBmpData = GrayBmp.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            Marshal.Copy(GrayPixels, 0, GrayBmpData.Scan0, GrayPixels.Length);
            GrayBmp.UnlockBits(GrayBmpData);

            return GrayBmp;
        }

        /// <summary>
        /// 将灰度数组转换为灰度图像（256级灰度）
        /// </summary>
        /// <param name="grayArray">灰度数组</param>
        /// <returns>灰度图像</returns>
        public static Bitmap GrayArrayToGrayBitmap(Byte[,] grayArray)
        {   // 将灰度数组转换为灰度数据
            Int32 PixelHeight = grayArray.GetLength(0);     // 图像高度
            Int32 PixelWidth = grayArray.GetLength(1);      // 图像宽度
            Int32 Stride = ((PixelWidth + 3) >> 2) << 2;    // 跨距宽度
            Byte[] Pixels = new Byte[PixelHeight * Stride];
            for (Int32 i = 0; i < PixelHeight; i++)
            {
                Int32 Index = i * Stride;
                for (Int32 j = 0; j < PixelWidth; j++)
                {
                    Pixels[Index++] = grayArray[i, j];
                }
            }

            // 创建灰度图像
            Bitmap GrayBmp = new Bitmap(PixelWidth, PixelHeight, PixelFormat.Format8bppIndexed);

            // 设置调色表
            ColorPalette cp = GrayBmp.Palette;
            for (int i = 0; i < 256; i++) cp.Entries[i] = Color.FromArgb(i, i, i);
            GrayBmp.Palette = cp;

            // 设置位图图像特性
            BitmapData GrayBmpData = GrayBmp.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            Marshal.Copy(Pixels, 0, GrayBmpData.Scan0, Pixels.Length);
            GrayBmp.UnlockBits(GrayBmpData);

            return GrayBmp;
        }

        /// <summary>
        /// 将二值化数组转换为二值化图像
        /// </summary>
        /// <param name="binaryArray">二值化数组</param>
        /// <returns>二值化图像</returns>
        public static Bitmap BinaryArrayToBinaryBitmap(Byte[,] binaryArray)
        {   // 将二值化数组转换为二值化数据
            Int32 PixelHeight = binaryArray.GetLength(0);
            Int32 PixelWidth = binaryArray.GetLength(1);
            Int32 Stride = ((PixelWidth + 31) >> 5) << 2;
            Byte[] Pixels = new Byte[PixelHeight * Stride];
            for (Int32 i = 0; i < PixelHeight; i++)
            {
                Int32 Base = i * Stride;
                for (Int32 j = 0; j < PixelWidth; j++)
                {
                    if (binaryArray[i, j] != 0)
                    {
                        Pixels[Base + (j >> 3)] |= Convert.ToByte(0x80 >> (j & 0x7));
                    }
                }
            }

            // 创建黑白图像
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
    }
}
