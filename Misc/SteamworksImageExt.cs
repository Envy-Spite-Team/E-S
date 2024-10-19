using UnityEngine;
using Steamworks.Data;  // Assuming this is where Steamworks' Image class is from

namespace EnvyLevelLoader
{
    public static class SteamworksImageExtensions
    {
        // Extension method to convert Steamworks Image to Unity Texture2D
        public static Texture2D ConvertToTexture2D(this Image image)
        {
            // Create a new Texture2D with the same width and height as the Steamworks image
            var avatar = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.ARGB32, false);

            // Set filter mode to Trilinear for better scaling
            avatar.filterMode = FilterMode.Trilinear;

            // Iterate through each pixel of the image and flip it vertically
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    // Get the pixel color from the Steamworks image
                    var p = image.GetPixel(x, y);

                    // Set the pixel in the Unity texture, converting color values from 0-255 range to 0-1 range
                    avatar.SetPixel(x, (int)image.Height - y - 1, new UnityEngine.Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
                }
            }

            // Apply the changes to the texture to update it
            avatar.Apply();

            // Return the created Texture2D
            return avatar;
        }
        
        public static Texture2D ConvertToTexture2D(this Image? image)
        {
            if (image == null)
                return null;
            return ConvertToTexture2D(image.Value);
        }
    }

}