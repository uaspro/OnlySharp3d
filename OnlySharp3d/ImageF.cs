using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OnlySharp3d
{
    public class ImageF
    {
        public Vector3[][] Buffer { get; }

        public int Width { get; }

        public int Height { get; }

        internal ImageF(int width, int height)
        {
            Width = width;
            Height = height;

            Buffer = new Vector3[Width][];
            for (var i = 0; i < Width; i++)
            {
                Buffer[i] = new Vector3[Height];
            }
        }

        internal ImageF(string path)
        {
            var envMap = new Bitmap(path);
            
            Width = envMap.Width;
            Height = envMap.Height;

            Buffer = new Vector3[Width][];
            for (var i = 0; i < Width; i++)
            {
                Buffer[i] = new Vector3[Height];
            }

            var bitmapData = envMap.LockBits(
                new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, envMap.PixelFormat);
            var bytesPerPixel = Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8;
            var byteCount = bitmapData.Stride * Height;
            var pixels = new byte[byteCount];
            var ptrFirstPixel = bitmapData.Scan0;

            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);

            var heightInPixels = Height;
            var widthInBytes = Width * bytesPerPixel;

            Parallel.For(
                0, heightInPixels, j =>
                {
                    var currentLine = j * bitmapData.Stride;
                    for (var i = 0; i < widthInBytes; i += bytesPerPixel)
                    {
                        int red = pixels[currentLine + i + 2];
                        int green = pixels[currentLine + i + 1];
                        int blue = pixels[currentLine + i];
                        Buffer[i / bytesPerPixel][j] = new Vector3(red / 255f, green / 255f, blue / 255f);
                    }
                });

            envMap.UnlockBits(bitmapData);
            envMap.Dispose();
        }

        public void SaveToFile(string path)
        {
            var imageBitmap = new Bitmap(Width, Height);
            var bitmapData = imageBitmap.LockBits(
                new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, imageBitmap.PixelFormat);

            var bytesPerPixel = Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8;
            var byteCount = bitmapData.Stride * Height;
            var pixels = new byte[byteCount];
            var ptrFirstPixel = bitmapData.Scan0;

            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);

            var heightInPixels = Height;
            var widthInBytes = Width * bytesPerPixel;

            Parallel.For(
                0, heightInPixels, j =>
                {
                    var currentLine = j * bitmapData.Stride;
                    for (var i = 0; i < widthInBytes; i += bytesPerPixel)
                    {
                        var colorValue = Buffer[i / bytesPerPixel][j];
                        var max = MathF.Max(colorValue.X, MathF.Max(colorValue.Y, colorValue.Z));
                        if (max > 1)
                        {
                            colorValue *= 1f / max;
                        }

                        pixels[currentLine + i + 2] = (byte)(255 * MathF.Max(0f, MathF.Min(1f, colorValue.X)));
                        pixels[currentLine + i + 1] = (byte)(255 * MathF.Max(0f, MathF.Min(1f, colorValue.Y)));
                        pixels[currentLine + i] = (byte)(255 * MathF.Max(0f, MathF.Min(1f, colorValue.Z)));
                    }
                });

            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            imageBitmap.UnlockBits(bitmapData);

            using (var output = File.Open(string.Format(path), FileMode.OpenOrCreate))
            {
                imageBitmap.Save(output, ImageFormat.Bmp);
            }

            imageBitmap.Dispose();
        }
    }
}
