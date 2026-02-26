using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Libra.Agent.Helper
{
    public static class MonitorHelper
    {
        #region Win32

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(
            IntPtr hdcDest,
            int xDest,
            int yDest,
            int width,
            int height,
            IntPtr hdcSrc,
            int xSrc,
            int ySrc,
            CopyPixelOperation rop);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;

        #endregion

        public static byte[]? CaptureCameraFrame(
            int cameraIndex = 0,
            int width = 1280,
            int height = 720,
            long jpegQuality = 60)
        {

            return [];
        }

        /// <summary>
        /// 捕获完整桌面截图
        /// </summary>
        public static Bitmap CaptureFullScreen()
        {
            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);

            Bitmap bitmap = new Bitmap(screenWidth, screenHeight);

            IntPtr desktopWindow = GetDesktopWindow();
            IntPtr desktopDC = GetWindowDC(desktopWindow);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                IntPtr hdcDest = graphics.GetHdc();

                BitBlt(
                    hdcDest,
                    0,
                    0,
                    screenWidth,
                    screenHeight,
                    desktopDC,
                    0,
                    0,
                    CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);

                graphics.ReleaseHdc(hdcDest);
            }

            ReleaseDC(desktopWindow, desktopDC);

            return bitmap;
        }

        /// <summary>
        /// 捕获屏幕并压缩为 JPEG
        /// </summary>
        public static byte[] CaptureScreenCompressed(int targetWidth, int targetHeight, long jpegQuality = 50L)
        {
            using (Bitmap fullBitmap = CaptureFullScreen())
            using (Bitmap resizedBitmap = ResizeBitmap(fullBitmap, targetWidth, targetHeight))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, jpegQuality);

                ImageCodecInfo jpegCodec = GetEncoder(ImageFormat.Jpeg);
                resizedBitmap.Save(memoryStream, jpegCodec, encoderParams);

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// 获取指定格式的编码器
        /// </summary>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return ImageCodecInfo
                .GetImageDecoders()
                .FirstOrDefault(c => c.FormatID == format.Guid);
        }

        /// <summary>
        /// 高质量缩放 Bitmap
        /// </summary>
        public static Bitmap ResizeBitmap(Bitmap source, int width, int height)
        {
            Bitmap destination = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(destination))
            {
                graphics.InterpolationMode =
                    System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                graphics.DrawImage(source, 0, 0, width, height);
            }

            return destination;
        }
    }
}