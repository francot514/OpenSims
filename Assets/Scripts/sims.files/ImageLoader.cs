/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FSO.Files
{
    public class ImageLoader
    {
        public static bool UseSoftLoad = true;
        public static int PremultiplyPNG = 0;
        public Color[] ColorData;
        public readonly byte[] ByteData;

        public static HashSet<Color> MASK_COLORS = new HashSet<Color>{
            new Color(0xFF, 0x00, 0xFF, 0xFF),
            new Color(0xFE, 0x02, 0xFE, 0xFF),
            new Color(0xFF, 0x01, 0xFF, 0xFF)
        };


        public Texture2D GetTexture(Graphics gd, int width, int height)
        {
            if (ColorData == null && ByteData == null)
            {
                return null;
            }

            var tex = new Texture2D(width, height);
            if (ColorData != null)
            {
                tex.SetPixelData(ColorData, 1);
            }
            else
            {
                tex.SetPixelData(ByteData, 1);
            }

            return tex;
        }
       

        public Texture2D LoadImageFromStream(Stream stream)
        {
            // 1. Create a buffer and read the stream
            byte[] imageData = new byte[stream.Length];
            stream.Read(imageData, 0, (int)stream.Length);

            // 2. Create an empty Texture (it will be resized by LoadImage)
            Texture2D tex = new Texture2D(2, 2);

            // 3. Load byte data into the texture
            if (tex.LoadImage(imageData))
            {
                return tex;
            }
            return null;
        }

        public static void ManualTextureMaskSingleThreaded(ref Texture2D Texture, Color[] ColorsFrom)
		{
			var ColorTo = Color.black;

			var size = Texture.width * Texture.height;
			Color[] buffer = new Color[size];

			//Texture.GetPixelData(1);

			var didChange = false;

			for (int i = 0; i < size; i++)
			{

				if (ColorsFrom.Contains(buffer[i]))
				{
					didChange = true;
					buffer[i] = ColorTo;
				}
			}

			if (didChange)
			{
				Texture.SetPixelData(buffer, 0, size);
			}
			else return;
		}

	}
}
