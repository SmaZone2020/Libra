using FlashCap;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Libra.Agent.Helper
{
    internal static class CameraHelper
    {
        private static readonly CaptureDevices _devices = new();

        private static readonly ImageCodecInfo _jpegCodec =
            Array.Find(ImageCodecInfo.GetImageDecoders(), c => c.FormatID == ImageFormat.Jpeg.Guid)!;

        // ── Public API ───────────────────────────────────────────────────────────

        public static string[] GetCameraNames()
        {
            try
            {
                return _devices.EnumerateDescriptors()
                    .Select(d => d.Name)
                    .ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CameraHelper.GetCameraNames error: {ex.Message}");
                return [];
            }
        }

        public static CaptureDeviceDescriptor? GetDescriptor(int cameraIndex)
        {
            try
            {
                var descriptors = _devices.EnumerateDescriptors().ToArray();
                if (cameraIndex < 0 || cameraIndex >= descriptors.Length) return null;
                return descriptors[cameraIndex];
            }
            catch
            {
                return null;
            }
        }

        public static VideoCharacteristics? ChooseCharacteristics(
            CaptureDeviceDescriptor descriptor, int preferredFps = 10)
        {
            var chars = descriptor.Characteristics
                .Where(c => c.PixelFormat != PixelFormats.Unknown)
                .ToArray();

            if (chars.Length == 0) return null;

            // Prefer JPEG/MJPEG to avoid transcoding, then pick closest FPS
            var jpeg = chars
                .Where(c => c.PixelFormat == PixelFormats.JPEG)
                .OrderBy(c => Math.Abs(c.FramesPerSecond - preferredFps))
                .FirstOrDefault();
            if (jpeg != null) return jpeg;

            // Fallback: any format, prefer reasonable resolution, closest FPS
            return chars
                .OrderBy(c => Math.Abs(c.FramesPerSecond - preferredFps))
                .ThenByDescending(c => c.Width * c.Height)
                .First();
        }

        public static byte[]? CaptureFrame(int cameraIndex, long jpegQuality = 60)
        {
            try
            {
                var descriptor = GetDescriptor(cameraIndex);
                if (descriptor == null) return null;

                var chars = ChooseCharacteristics(descriptor, 10);
                if (chars == null) return null;

                var imageData = descriptor.TakeOneShotAsync(chars).GetAwaiter().GetResult();
                if (imageData == null || imageData.Length == 0) return null;

                return ToJpeg(imageData, jpegQuality);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CameraHelper.CaptureFrame error: {ex.Message}");
                return null;
            }
        }

        // ── JPEG encoding ────────────────────────────────────────────────────────

        public static byte[] ToJpeg(byte[] imageData, long quality)
        {
            // FlashCap returns DIB (BMP) data by default after transcoding
            // Try to load as image and re-encode as JPEG
            using var ms = new MemoryStream(imageData);
            using var bmp = new Bitmap(ms);
            using var outMs = new MemoryStream();
            using var ep = new EncoderParameters(1);
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            bmp.Save(outMs, _jpegCodec, ep);
            return outMs.ToArray();
        }

        // No CloseCamera/CloseAll needed — FlashCap manages device lifetime internally
        public static void CloseCamera(int cameraIndex) { }
        public static void CloseAll() { }
    }
}
