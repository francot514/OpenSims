using FSO.Common.Utils;
using FSO.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FSO.Common;
using UnityEngine;

namespace FSO.Content.Model
{
    public interface ITextureRef
    {
        Texture2D Get(Graphics device);
    }

    public class FileTextureRef : AbstractTextureRef
    {
        private string _FilePath;

        public FileTextureRef(string filepath)
        {
            _FilePath = filepath;
        }

        protected override Stream GetStream()
        {
            return new FileStream(_FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }

    public class InMemoryTextureRef : AbstractTextureRef
    {
        private byte[] _Data;

        public InMemoryTextureRef(byte[] data)
        {
            _Data = data;
        }

        protected override Stream GetStream()
        {
            return new MemoryStream(_Data, false);
        }
    }

    public class InMemoryTextureRefWithMask : InMemoryTextureRef
    {
        private byte[] _Data;
        private uint[] _MaskColors;

        public InMemoryTextureRefWithMask(byte[] data, uint[] maskColors) : base(data)
        {
            _Data = data;
            _MaskColors = maskColors;
        }

        protected override Texture2D Process(Graphics device, Stream stream)
        {
            var texture = base.Process(device, stream);
            //TextureUtils.ManualTextureMaskSingleThreaded(ref texture, _MaskColors);
            return texture;
        }
    }

    public abstract class AbstractTextureRef : ITextureRef
    {
        private Texture2D _Instance;

        public AbstractTextureRef()
        {
        }

        protected abstract Stream GetStream();


        public Texture2D Get(Graphics device)
        {
            lock (this)
            {
                if (_Instance != null)
                {
                    return _Instance;
                }

                using (var stream = GetStream())
                {
                    _Instance = Process(device, stream);
                    return _Instance;
                }
            }
        }

        protected virtual Texture2D Process(Graphics device, Stream stream)
        {
            ImageLoader loader = new ImageLoader();
            return loader.LoadImageFromStream(stream);
        }
    }
}