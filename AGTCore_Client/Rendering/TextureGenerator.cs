using SkiaSharp;
using System.IO;
using System.Collections.Generic;

namespace AGTCore
{
    public static class TextureGenerator
    {
        public static int blockCount { get; private set; }
        public static float blockScale { get; private set; }
        // using the images textures in the blocks folder, a combined "textureAtlas" image is created
        // this makes adding blocks to the game trivial for developers
        public static void StitchTextures(string[] blockNames)
        {
            // sets the block count using the number of present images
            blockCount = blockNames.Length;
            // the SKImage library is used for image processing 
            var info = new SKImageInfo(blockNames.Length * 16, 48, SKColorType.Rgba8888);
            // "using" disposes the SKSurface object when we're done with it
            using (var surface = SKSurface.Create(info))
            {
                SKCanvas canvas = surface.Canvas;
                for (int x = 0; x < blockNames.Length; x++)
                {
                    string path = "textures/" + blockNames[x] + ".png";
                    var codec = SKCodec.Create(path);
                    var imageInfo = new SKImageInfo(16, 48, SKColorType.Rgba8888);
                    SKBitmap blockTexture = SKBitmap.Decode(codec, imageInfo);
                    if (codec != null)
                    {
                        canvas.DrawBitmap(blockTexture, x * 16, 0);
                    }
                }

                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 80))
                // the stream represents the textureAtlas.png file as a writable byte array
                using (var stream = File.OpenWrite("textureAtlas.png"))
                {
                    // save the data to the stream
                    data.SaveTo(stream);
                }
            }
        }
    }
}