/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FSO.Common.Utils
{
    public class TextureUtils
    {
        public static Color[] Decimate(Color[] old, int w, int h)
        {
            var nw = w / 2;
            var nh = h / 2;
            bool linex = false, liney = false;
            if (nw == 0 && nh == 0) return null;
            if (nw == 0) { nw = 1; liney = true; }
            if (nh == 0) { nh = 1; linex = true; }
            var size = nw * nh;
            Color[] buffer = new Color[size];

            int tind = 0;
            int fyind = 0;
            for (int y = 0; y < nh; y++)
            {
                var yb = y * 2 == h || linex;
                int find = fyind;
                for (int x = 0; x < nw; x++)
                {
                    var xb = x * 2 == h || liney;
                    var c1 = old[find];
                    var c2 = (xb) ? Color.white : old[find + 1];
                    var c3 = (yb) ? Color.white : old[find + w];
                    var c4 = (xb || yb) ? Color.white : old[find + 1 + w];

                    int r = 0, g = 0, b = 0, t = 0;
                    if (c1.a > 0)
                    {
                        r += (int)c1.r; g += (int)c1.g; b += (int)c1.b; t++;
                    }
                    if (c2.a > 0)
                    {
                        r += (int)c2.r; g += (int)c2.g; b += (int)c2.b; t++;
                    }
                    if (c3.a > 0)
                    {
                        r += (int)c3.r; g += (int)c3.g; b += (int)c3.b; t++;
                    }
                    if (c4.a > 0)
                    {
                        r += (int)c4.r; g += (int)c4.g; b += (int)c4.b; t++;
                    }
                    if (t == 0) t = 1;

                    buffer[tind++] = new Color(
                        (byte)(r / t),
                        (byte)(g / t),
                        (byte)(b / t),
                        Math.Max(Math.Max(Math.Max(c1.a, c2.a), c3.a), c4.a)
                        );
                    find += 2;
                }
                fyind += w * 2;
            }
            return buffer;
        }

        public static void UploadWithMips(Texture2D Texture, Graphics gd, Color[] data)
        {
            int level = 0;
            int w = Texture.width;
            int h = Texture.height;
            while (data != null)
            {

                Texture.SetPixels(data);
                data = Decimate(data, w, h);
                w /= 2;
                h /= 2;
            }
        }

        public static Color[] AvgDecimate(Color[] old, int w, int h)
        {
            var nw = w / 2;
            var nh = h / 2;
            bool linex = false, liney = false;
            if (nw == 0 && nh == 0) return null;
            if (nw == 0) { nw = 1; liney = true; }
            if (nh == 0) { nh = 1; linex = true; }
            var size = nw * nh;
            Color[] buffer = new Color[size];

            int tind = 0;
            int fyind = 0;
            for (int y = 0; y < nh; y++)
            {
                var yb = y * 2 == h || linex;
                int find = fyind;
                for (int x = 0; x < nw; x++)
                {
                    var xb = x * 2 == w || liney;
                    var c1 = old[find];
                    var c2 = (xb) ? Color.white : old[find + 1];
                    var c3 = (yb) ? Color.white : old[find + w];
                    var c4 = (xb || yb) ? Color.white : old[find + 1 + w];

                    int r = 0, g = 0, b = 0, a = 0, t = 0;
                    if (c1.a > 0)
                    {
                        r += (int)c1.r; g += (int)c1.g; b += (int)c1.b; a += (int)c1.a; t++;
                    }
                    if (c2.a > 0)
                    {
                        r += (int)c2.r; g += (int)c2.g; b += (int)c2.b; a += (int)c2.a; t++;
                    }
                    if (c3.a > 0)
                    {
                        r += (int)c3.r; g += (int)c3.g; b += (int)c3.b; a += (int)c3.a; t++;
                    }
                    if (c4.a > 0)
                    {
                        r += (int)c4.r; g += (int)c4.g; b += (int)c4.b; a += (int)c4.a; t++;
                    }
                    if (t == 0) t = 1;

                    buffer[tind++] = new Color(
                        (byte)(r / t),
                        (byte)(g / t),
                        (byte)(b / t),
                        (byte)(a / 4)
                        );
                    find += 2;
                }
                fyind += w * 2;
            }
            return buffer;
        }
        public static void UploadWithAvgMips(Texture2D Texture, Graphics gd, Color[] data)
        {
            int level = 0;
            int w = Texture.width;
            int h = Texture.height;
            while (data != null)
            {
                Texture.SetPixels(data);
                data = AvgDecimate(data, w, h);
                w /= 2;
                h /= 2;
            }
        }
        public static Texture2D TextureFromFile(Graphics gd, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Texture2D text = new Texture2D(1, 1);
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length); 
                text.LoadRawTextureData(buffer);
                return text;
            }
        }
        public static Texture2D MipTextureFromFile(Graphics gd, string filePath)
        {
            var tex = TextureFromFile(gd, filePath);
            var data = new Color[tex.width * tex.height];
            data = tex.GetPixels(tex.width, tex.height, tex.height, tex.height);
            var newTex = new Texture2D(tex.width, tex.height);
            UploadWithAvgMips(newTex, gd, data);
            
            return newTex;
        }
        public static Texture2D TextureFromColor(Graphics gd, Color color)
        {
            var tex = new Texture2D( 1, 1);
            tex.SetPixels(new[] { color });
            return tex;
        }

        public static Texture2D TextureFromColor(Graphics gd, Color color, int width, int height)
        {
            var tex = new Texture2D( width, height);
            var data = new Color[width * height];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = color;
            }
            tex.SetPixels(data);
            return tex;
        }

        /**
         * Because the buffers can be fairly big, its much quicker to just keep some
         * in memory and reuse them for resampling textures
         * 
         * rhy: yeah, maybe, if the code actually did that. i'm also not sure about keeping ~32MB 
         * of texture buffers in memory at all times when the game is largely single threaded.
         */
        private static List<uint[]> ResampleBuffers = new List<uint[]>();
        private static ulong MaxResampleBufferSize = 1024 * 768;

        static TextureUtils()
        {
            /*for (var i = 0; i < 10; i++)
            {
                ResampleBuffers.Add(new uint[MaxResampleBufferSize]);
            }*/
        }

        private static Color[] GetBuffer(int size) //todo: maybe implement something like described, old implementation was broken
        {
            var newBuffer = new Color[size];
            return newBuffer;
        }

        private static void FreeBuffer(uint[] buffer)
        {
        }

        public static void MaskFromTexture(ref Texture2D Texture, Texture2D Mask, Color[] ColorsFrom)
        {
            if (Texture.width != Mask.width || Texture.height != Mask.height)
            {
                return;
            }

            var ColorTo = Color.white;

            var size = Texture.width * Texture.height;
            Color[] buffer = GetBuffer(size);
            Texture.GetPixels(Texture.width, Texture.height, Texture.width, Texture.height);

            var sizeMask = Mask.width * Mask.height;
            var bufferMask = GetBuffer(sizeMask);
            Mask.GetPixels(Texture.width, Texture.height, Texture.width, Texture.height);

            var didChange = false;
            for (int i = 0; i < size; i++)
            {
                if (ColorsFrom.Contains(bufferMask[i]))
                {
                    didChange = true;
                    buffer[i] = ColorTo;
                }
            }

            if (didChange)
            {
                Texture.SetPixels(buffer);
            }
        }

        public static Texture2D Clip(Graphics gd, Texture2D texture, Rect source)
        {
            var newTexture = new Texture2D( (int)source.width, (int)source.height);
            var size = source.width * source.height;
            Color[] buffer = GetBuffer((int)size);
            if (FSOEnvironment.SoftwareDepth)
            {
                //opengl es does not like this
                var texBuf = GetBuffer(texture.width * texture.height);
                texture.GetPixels();
                var destOff = 0;
                for (int y=(int)source.y; y<source.bottom; y++)
                {
                    int offset = y * texture.width + (int)source.x;
                    for (int x = 0; x < source.width; x++)
                    {
                        buffer[destOff++] = texBuf[offset++];
                    }
                }
            }
            else
            {
                texture.GetRawTextureData();
            }

            newTexture.SetPixels(buffer);
            return newTexture;
        }

        public static Texture2D Copy(Graphics gd, Texture2D texture)
        {
            var newTexture = new Texture2D( texture.width, texture.height);

            var size = texture.width * texture.height;
            Color[] buffer = GetBuffer(size);
            texture.GetPixels(buffer.Length);

            newTexture.SetPixels(buffer);
            return newTexture;
        }

        public static void CopyAlpha(ref Texture2D TextureTo, Texture2D TextureFrom)
        {
            if (TextureTo.width != TextureFrom.width || TextureTo.height != TextureFrom.height)
            {
                return;
            }


            var size = TextureTo.width * TextureTo.height;
            Color[] buffer = GetBuffer(size);
            TextureTo.GetPixels();

            var sizeFrom = TextureFrom.width * TextureFrom.height;
            var bufferFrom = GetBuffer(sizeFrom);
            TextureFrom.GetPixels();


            TextureTo.SetPixels(buffer);
        }

        /// <summary>
        /// Manually replaces a specified color in a texture with transparent black,
        /// thereby masking it.
        /// </summary>
        /// <param name="Texture">The texture on which to apply the mask.</param>
        /// <param name="ColorFrom">The color to  mask away.</param>
        public static void ManualTextureMask(ref Texture2D Texture, Color[] ColorsFrom)
        {
            var ColorTo = Color.white;

            //lock (TEXTURE_MASK_BUFFER)
            //{
                
                var size = Texture.width * Texture.height;
                Color[] buffer = GetBuffer(size);
                //uint[] buffer = new uint[size];

                //var buffer = TEXTURE_MASK_BUFFER;
                Texture.GetPixels();

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
                    Texture.SetPixels(buffer);
                }
        }

        private static uint[] SINGLE_THREADED_TEXTURE_BUFFER = new uint[MaxResampleBufferSize];
        public static void ManualTextureMaskSingleThreaded(ref Texture2D Texture, Color[] ColorsFrom)
        {
            var ColorTo = Color.white;
            
            var size = Texture.width * Texture.height;
            Color[] buffer = new Color[size];

            Texture.GetPixelData<Color>(1);

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
                Texture.SetPixels(buffer);
            }
            else return;
        }

        /// <summary>
        /// Combines multiple textures into a single texture
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="textures"></param>
        /// <returns></returns>
        public static Texture2D MergeHorizontal(Graphics gd, params Texture2D[] textures)
        {
            return MergeHorizontal(gd, 0,textures);
        }

        /// <summary>
        /// Combines multiple textures into a single texture
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="textures"></param>
        /// <returns></returns>
        public static Texture2D MergeHorizontal(Graphics gd, int tailPx, params Texture2D[] textures)
        {
            var width = 0;
            var maxHeight = 0;
            var maxWidth = 0;
            
            foreach (var texture in textures)
            {
                width += texture.width;
                maxHeight = Math.Max(maxHeight, texture.height);
                maxWidth = Math.Max(maxWidth, texture.width);
            }

            width += tailPx;

            Texture2D newTexture = new Texture2D( width, maxHeight);
            Color[] newTextureData = new Color[width * maxHeight];
            Color[] tempTexData = new Color[maxWidth * maxHeight];

            var xo = 0;
            for (var i = 0; i < textures.Length; i++)
            {
                var tx = textures[i];
                tx.GetPixelData<Color>(1);
                for (var y = 0; y < tx.height; y++)
                {
                    var yOffset = y * width;

                    for (var x = 0; x < tx.width; x++)
                    {
                        newTextureData[yOffset + xo + x] = tempTexData[tx.width * y + x];
                    }
                }
                xo += tx.width;
            }

            newTexture.SetPixels(newTextureData);
            tempTexData = null;

            return newTexture;
        }

        public static Texture2D Decimate(Texture2D Texture, Graphics gd, int factor, bool disposeOld)
        {
            if (Texture.width < factor || Texture.height < factor) return Texture;
            var size = Texture.width * Texture.height * 4;
            int[] buffer = new int[size];

            Texture.GetPixels();

            var newWidth = Texture.width / factor;
            var newHeight = Texture.height / factor;
            var target = new byte[newWidth * newHeight * 4];

            for (int y = 0; y < Texture.height; y += factor)
            {
                for (int x = 0; x < Texture.width; x += factor)
                {
                    for (int c = 0; c < 4; c++)
                    {
                        var targy = (y / factor);
                        var targx = (x / factor);
                        if (targy >= newHeight || targx >= newWidth) continue;
                        int avg = 0;
                        int total = 0;
                        for (int yo = y; yo < y + factor && yo < Texture.height; yo++)
                        {
                            for (int xo = x; xo < x + factor && xo < Texture.width; xo++)
                            {
                                avg += (int)buffer[(yo * Texture.width + xo) * 4 + c];
                                total++;
                            }
                        }

                        avg /= total;
                        target[(targy * newWidth + targx) * 4] = (byte)avg;
                    }
                }
            }

            var colors = new Color[target.Length];
            var outTex = new Texture2D( newWidth, newHeight);
            outTex.SetPixels(colors);
            return outTex;
        }

        public static Texture2D Resize(Graphics gd, Texture2D texture, int newWidth, int newHeight)
        {
            return texture; //todo: why is this broken (framebuffer incomplete when we try to bind it)

            /*RenderTarget2D renderTarget = new RenderTarget2D(
                gd,
                newWidth, newHeight, false,
                SurfaceFormat.Color, DepthFormat.None);
           
            Rectangle destinationRectangle = new Rectangle(0, 0, newWidth, newHeight);
            lock (gd)
            {
                gd.SetRenderTarget(renderTarget);
                SpriteBatch batch = new SpriteBatch(gd);
                batch.Begin();
                batch.Draw(texture, destinationRectangle, Color.White);
                batch.End();
                gd.SetRenderTarget(null);
            }
            var newTexture = renderTarget;
            return newTexture; */
        }

        public static Texture2D Scale(Graphics gd, Texture2D texture, float scaleX, float scaleY)
        {
            var newWidth = (int)(texture.width * scaleX);
            var newHeight = (int)(texture.height * scaleY);

            RenderTexture renderTarget = new RenderTexture(new RenderTextureDescriptor());

            Graphics.SetRenderTarget(renderTarget);


            Rect destinationRectangle = new Rect(0, 0, newWidth, newHeight);

            

            Graphics.SetRenderTarget(null);

            var newTexture = RenderTexture.active;

            
            return ToTexture2D(renderTarget);
        }

        public static Texture2D ToTexture2D(RenderTexture rTex)
        {
            // 1. Create a new Texture2D with matching dimensions
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);

            // 2. Cache the current active RenderTexture to restore it later
            RenderTexture oldRT = RenderTexture.active;

            // 3. Set the source RenderTexture as the active one for the GPU
            RenderTexture.active = rTex;

            // 4. Copy pixels from the active RenderTexture into the Texture2D
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();

            // 5. Restore the previous active RenderTexture
            RenderTexture.active = oldRT;

            return tex;
        }
    }
}
