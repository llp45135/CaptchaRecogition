using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CaptchaRecogition
{
    /// <summary>
    /// 原始图像转换成格式为Bgr565或者Bgr555的16位图像
    /// </summary>
    public static partial class ImageUtils
    {
        /// <summary>
        /// 将原始图像转换成格式为Bgr565的16位图像
        /// </summary>
        /// <param name="bmp">用于转换的原始图像</param>
        /// <returns>转换后格式为Bgr565的16位图像</returns>
        public static Bitmap ToBgr565(this Bitmap bmp)
        {
            Int32 PixelHeight = bmp.Height; // 图像高度
            Int32 PixelWidth = bmp.Width;   // 图像宽度
            Int32 Stride = ((PixelWidth * 3 + 3) >> 2) << 2;    // 跨距宽度
            Byte[] Pixels = new Byte[PixelHeight * Stride];

            // 锁定位图到系统内存
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(bmpData.Scan0, Pixels, 0, Pixels.Length);  // 从非托管内存拷贝数据到托管内存
            bmp.UnlockBits(bmpData);    // 从系统内存解锁位图

            // Bgr565格式为 RRRRR GGGGGG BBBBB
            Int32 TargetStride = ((PixelWidth + 1) >> 1) << 2;  // 每个像素占2字节，且跨距要求4字节对齐
            Byte[] TargetPixels = new Byte[PixelHeight * TargetStride];
            for (Int32 i = 0; i < PixelHeight; i++)
            {
                Int32 Index = i * Stride;
                Int32 Loc = i * TargetStride;
                for (Int32 j = 0; j < PixelWidth; j++)
                {
                    Byte B = Pixels[Index++];
                    Byte G = Pixels[Index++];
                    Byte R = Pixels[Index++];

                    TargetPixels[Loc++] = (Byte)(((G << 3) & 0xe0) | ((B >> 3) & 0x1f));
                    TargetPixels[Loc++] = (Byte)((R & 0xf8) | ((G >> 5) & 7));
                }
            }

            // 创建Bgr565图像
            Bitmap TargetBmp = new Bitmap(PixelWidth, PixelHeight, PixelFormat.Format16bppRgb565);

            // 设置位图图像特性
            BitmapData TargetBmpData = TargetBmp.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.WriteOnly, PixelFormat.Format16bppRgb565);
            Marshal.Copy(TargetPixels, 0, TargetBmpData.Scan0, TargetPixels.Length);
            TargetBmp.UnlockBits(TargetBmpData);

            return TargetBmp;
        }

        /// <summary>
        /// 将原始图像转换成格式为Bgr555的16位图像
        /// </summary>
        /// <param name="bmp">用于转换的原始图像</param>
        /// <returns>转换后格式为Bgr555的16位图像</returns>
        public static Bitmap ToBgr555(this Bitmap bmp)
        {
            Int32 PixelHeight = bmp.Height; // 图像高度
            Int32 PixelWidth = bmp.Width;   // 图像宽度
            Int32 Stride = ((PixelWidth * 3 + 3) >> 2) << 2;    // 跨距宽度
            Byte[] Pixels = new Byte[PixelHeight * Stride];

            // 锁定位图到系统内存
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(bmpData.Scan0, Pixels, 0, Pixels.Length);  // 从非托管内存拷贝数据到托管内存
            bmp.UnlockBits(bmpData);    // 从系统内存解锁位图

            // Bgr555格式为 X RRRRR GGGGG BBBBB
            Int32 TargetStride = ((PixelWidth + 1) >> 1) << 2;  // 每个像素占2字节，且跨距要求4字节对齐
            Byte[] TargetPixels = new Byte[PixelHeight * TargetStride];
            for (Int32 i = 0; i < PixelHeight; i++)
            {
                Int32 Index = i * Stride;
                Int32 Loc = i * TargetStride;
                for (Int32 j = 0; j < PixelWidth; j++)
                {
                    Byte B = Pixels[Index++];
                    Byte G = Pixels[Index++];
                    Byte R = Pixels[Index++];

                    TargetPixels[Loc++] = (Byte)(((G << 2) & 0xe0) | ((B >> 3) & 0x1f));
                    TargetPixels[Loc++] = (Byte)(((R >> 1) & 0x7c) | ((G >> 6) & 3));
                }
            }

            // 创建Bgr555图像
            Bitmap TargetBmp = new Bitmap(PixelWidth, PixelHeight, PixelFormat.Format16bppRgb555);

            // 设置位图图像特性
            BitmapData TargetBmpData = TargetBmp.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.WriteOnly, PixelFormat.Format16bppRgb555);
            Marshal.Copy(TargetPixels, 0, TargetBmpData.Scan0, TargetPixels.Length);
            TargetBmp.UnlockBits(TargetBmpData);

            return TargetBmp;
        }
    }
}
