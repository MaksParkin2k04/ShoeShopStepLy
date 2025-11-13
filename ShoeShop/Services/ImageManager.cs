using ShoeShop.Infrastructure;
using SkiaSharp;

namespace ShoeShop.Services
{
    public class ImageManager : IImageManager {

       public ImageManager(IWebHostEnvironment environment) {
            this.environment = environment;
        }

        private readonly IWebHostEnvironment environment;

        public void Delete(string filePath) {
            File.Delete(filePath);
        }

        public void Create(Stream readStream, string writeFileePath, int width, int height) {

            using (FileStream writeStream = File.OpenWrite(writeFileePath)) {
                writeStream.SetLength(0);

                SKBitmap bitmap = SKBitmap.Decode(readStream);
                SKBitmap resizeBitmap = bitmap.Resize(new SKSizeI(width, height), SKFilterQuality.High);
                resizeBitmap.Encode(writeStream, SKEncodedImageFormat.Jpeg, 60);
            }
        }
    }
}
