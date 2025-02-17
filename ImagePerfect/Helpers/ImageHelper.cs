using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Helpers
{
    public static class ImageHelper
    {
        public static Bitmap LoadFromResource(Uri resourceUri)
        {
            return new Bitmap(AssetLoader.Open(resourceUri));
        }

        public static Bitmap LoadFromFileSystem(string resourcePath)
        {
            return new Bitmap(resourcePath);
        }

        public static async Task<Bitmap?> LoadFromWeb(Uri url)
        {
            using var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsByteArrayAsync();
                return new Bitmap(new MemoryStream(data));
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"An error occurred while downloading image '{url}' : {ex.Message}");
                return null;
            }
        }

        //need to rotate portrait images and resize for screen
        public static async Task<Bitmap> FormatImage(string path)
        {
            if (File.Exists(path))
            {
                using (MemoryStream ms = new MemoryStream())
                using (var image = await Image.LoadAsync(path))
                {
                    image.Mutate(x => {
                        x.AutoOrient();
                        x.Resize(600, 0);
                    });
                    await image.SaveAsBmpAsync(ms);
                    //set stream to begining after writing
                    ms.Seek(0, SeekOrigin.Begin);
                    Bitmap img = new Bitmap(ms);
                    return img;
                }
            }
            else
            {
                return LoadFromResource(new Uri("avares://ImagePerfect/Assets/missing_image.png"));
            }
            
        }
    }
}
