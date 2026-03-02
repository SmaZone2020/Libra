using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Libra.Virgo.Models.MessageType;

namespace Libra.Agent.Helper
{
    public static class MonitorHelper
    {
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

        public static byte[]? CaptureCameraFrame(
            int cameraIndex = 0,
            int width = 1280,
            int height = 720,
            long jpegQuality = 60)
        {
            return CameraHelper.CaptureFrame(cameraIndex, jpegQuality);
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

        /// <summary>
        /// 根据画质档位计算目标分辨率（保持宽高比，不超过实际屏幕分辨率）
        /// quality: native | 1080p | 720p | 540p | 370p
        /// </summary>
        private static (int Width, int Height) GetTargetDimensions(string quality)
        {
            int sw = GetSystemMetrics(SM_CXSCREEN);
            int sh = GetSystemMetrics(SM_CYSCREEN);

            if (quality == "native") return (sw, sh);

            int targetH = quality switch
            {
                "1080p" => 1080,
                "720p"  => 720,
                "540p"  => 540,
                "370p"  => 370,
                _       => 540     // 默认
            };

            // 不超过实际屏幕高度（防止放大）
            targetH = Math.Min(targetH, sh);
            int targetW = (int)((long)sw * targetH / sh);
            return (targetW, targetH);
        }

        /// <summary>
        /// 捕获屏幕并与上一帧比对，返回差异帧和当前原始像素（供下次比对）
        /// quality: native | 1080p | 720p | 540p | 370p
        /// blockSize: 差异比较的块大小（像素），越小越精细但开销越大
        /// </summary>
        public static (ScreenFrame Frame, byte[] RawPixels) CaptureWithDiff(
            byte[]? prevPixels, int prevWidth, int prevHeight,
            Guid streamId, string quality = "720p", int blockSize = 64, long jpegQuality = 30)
        {
            var (targetWidth, targetHeight) = GetTargetDimensions(quality);

            using Bitmap fullBitmap = CaptureFullScreen();
            using Bitmap resized = ResizeBitmap(fullBitmap, targetWidth, targetHeight);

            // 提取原始像素用于下一帧比对
            var bmpData = resized.LockBits(
                new Rectangle(0, 0, targetWidth, targetHeight),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            int stride = bmpData.Stride;
            byte[] currentPixels = new byte[stride * targetHeight];
            Marshal.Copy(bmpData.Scan0, currentPixels, 0, currentPixels.Length);
            resized.UnlockBits(bmpData);

            var jpegCodec = GetEncoder(ImageFormat.Jpeg);
            var ep = new EncoderParameters(1);
            ep.Param[0] = new EncoderParameter(Encoder.Quality, jpegQuality);

            // 首帧或分辨率变化时发送完整帧
            bool sendFull = prevPixels == null
                || prevWidth != targetWidth
                || prevHeight != targetHeight;

            if (sendFull)
            {
                using var ms = new MemoryStream();
                resized.Save(ms, jpegCodec, ep);
                return (new ScreenFrame
                {
                    StreamId = streamId,
                    IsFull = true,
                    ScreenWidth = targetWidth,
                    ScreenHeight = targetHeight,
                    Data = Convert.ToBase64String(ms.ToArray())
                }, currentPixels);
            }

            // 逐块比对，只传输变化的区块
            var blocks = new List<DiffBlock>();
            int cols = (targetWidth + blockSize - 1) / blockSize;
            int rows = (targetHeight + blockSize - 1) / blockSize;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int bx = col * blockSize;
                    int by = row * blockSize;
                    int bw = Math.Min(blockSize, targetWidth - bx);
                    int bh = Math.Min(blockSize, targetHeight - by);

                    if (!IsBlockChanged(currentPixels, prevPixels!, stride, bx, by, bw, bh))
                        continue;

                    using var blockBmp = resized.Clone(
                        new Rectangle(bx, by, bw, bh),
                        PixelFormat.Format32bppArgb);
                    using var ms = new MemoryStream();
                    blockBmp.Save(ms, jpegCodec, ep);

                    blocks.Add(new DiffBlock
                    {
                        X = bx, Y = by, W = bw, H = bh,
                        Data = Convert.ToBase64String(ms.ToArray())
                    });
                }
            }

            return (new ScreenFrame
            {
                StreamId = streamId,
                IsFull = false,
                ScreenWidth = targetWidth,
                ScreenHeight = targetHeight,
                Blocks = blocks.Count > 0 ? blocks : null
            }, currentPixels);
        }

        private static bool IsBlockChanged(
            byte[] current, byte[] prev, int stride,
            int x, int y, int w, int h)
        {
            for (int row = y; row < y + h; row++)
            {
                int offset = row * stride + x * 4;
                for (int px = 0; px < w * 4; px++)
                {
                    if (current[offset + px] != prev[offset + px])
                        return true;
                }
            }
            return false;
        }
    }
}