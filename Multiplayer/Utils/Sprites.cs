using System;
using UnityEngine;


namespace Multiplayer.Utils
{
    public static class Sprites
    {
        private static UnityEngine.Sprite _padlock = null;

        private const  int textureWidth = 50;
        private const int textureHeight = 50;

        public static UnityEngine.Sprite Padlock
        {
            get
            {
                if (_padlock == null)
                {
                    _padlock = DrawPadlock();
                }
                return _padlock;
            }
        }

        private static UnityEngine.Sprite DrawPadlock()
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.DXT5, false); ;//, TextureFormat.BGRA32, false);// (textureWidth, textureHeight);

            Debug.Log($"loading from {System.IO.Path.Combine(Multiplayer.ModEntry.Path, "lock.png")}");
            // Load the PNG file from the specified file path
            byte[] fileData = System.IO.File.ReadAllBytes(System.IO.Path.Combine(Multiplayer.ModEntry.Path, "lock.png"));

            ImageConversion.LoadImage(texture, fileData);
            //texture.LoadRawTextureData(pngBytes); // Load the PNG data into the texture

            //int border = 5;


            //Color padlockColor = Color.white;
            //Color transparentColor = new Color(0, 0, 0, 0); // Fully transparent

            //// Clear the texture with the transparent color
            //for (int y = 0; y < textureHeight; y++)
            //{
            //    for (int x = 0; x < textureWidth; x++)
            //    {
            //        texture.SetPixel(x, y, transparentColor);
            //    }
            //}

            //// Draw the padlock body (rectangle)
            //int bodyWidth = (textureWidth - 2 * border)/2;
            //int bodyHeight = (textureHeight - 2 * border) / 3; // Adjusting body height
            //int bodyX = border;
            //int bodyY = border;

            //for (int y = bodyY; y < bodyY + bodyHeight; y++)
            //{
            //    for (int x = bodyX; x < bodyX + bodyWidth; x++)
            //    {
            //        texture.SetPixel(x, y, padlockColor);
            //    }
            //}

            ////Draw shanks
            //int shankThickness = 6;
            //int shankOffset = 2;
            //int shankHeight = bodyHeight * 2/3;

            //for (int y = bodyHeight+border; y < bodyHeight+border+shankHeight; y++)
            //{
            //    for (int x = 0; x < shankThickness; x++)
            //    {
            //        texture.SetPixel(border + shankOffset + x, y, padlockColor);
            //        texture.SetPixel(textureWidth-( bodyWidth + border + shankOffset + x) , y, padlockColor);
            //    }
            //}

            //// Draw the padlock shackle (semi-circle)
            //int shackleRadius = (bodyWidth - 2* shankOffset)/ 2;
            //int shackleCenterX = textureWidth / 2;
            //int shackleCenterY = bodyHeight + border + shankHeight; //bodyY + bodyHeight;

            //// Adjust the length of the straight part of the shackle
            //int shackleStraightLength = bodyHeight / 2;

            //// Adjust the thickness of the shackle
            //int shackleThickness = 1; // Set the shackle thickness to 1 pixel

            //for (int y = shackleCenterY - shackleRadius; y <= shackleCenterY; y++)
            //{
            //    for (int x = shackleCenterX - shackleRadius; x <= shackleCenterX + shackleRadius; x++)
            //    {
            //        float distanceToCenter = Mathf.Sqrt((x - shackleCenterX) * (x - shackleCenterX) + (y - shackleCenterY) * (y - shackleCenterY));

            //        // Check if the current pixel is within the semicircle and thickness
            //        if (distanceToCenter <= shackleRadius && distanceToCenter >= shackleRadius - shankThickness && y >= shackleCenterY)
            //        {
            //            texture.SetPixel(x, y, padlockColor);
            //        }
            //    }
            //}

            //texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

    }
}
