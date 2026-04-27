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
using System.Drawing;
using FSO.Files.Utils;
using UnityEngine;
using UnityEngine.UI;



namespace FSO.Files.Formats.IFF.Chunks
{
    /// <summary>
    /// This chunk type holds an image in BMP format.
    /// </summary>
    public class BMP : IffChunk
    {
        private byte[] data;

        /// <summary>
        /// Reads a BMP chunk from a stream.
        /// </summary>
        /// <param name="iff">An Iff instance.</param>
        /// <param name="stream">A Stream object holding a BMP chunk.</param>
        public override void Read(IffFile iff, Stream stream)
        {
            data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);

            
        }

        public Sprite GetBitmap(Texture2D text)
        {
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Sprite spr = Sprite.Create(text, new Rect(0,0,text.width, text.height),pivot);

            return spr;
        }



        public Texture2D GetTexture()
        {
            Texture2D text = new Texture2D(1, 1);
            text.LoadRawTextureData(data);
            text.Apply();
            return text;
        }

    }


}
