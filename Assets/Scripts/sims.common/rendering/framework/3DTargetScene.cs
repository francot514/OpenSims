using FSO.Common.Rendering.Framework.Camera;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace FSO.Common.Rendering.Framework
{
    public class _3DTargetScene : _3DScene
    {
        public RenderTexture Target;
        private Graphics Device;
        private int Multisample = 0;
        public _3DTargetScene(Graphics device, ICamera camera, Point size, int multisample) : this(device, size, multisample) { Camera = camera; }
        public _3DTargetScene(Graphics device, Point size, int multisample) : base(device)
        {
            Device = device;
            Multisample = multisample;
            SetSize(size);
        }

        public void SetSize(Point size)
        {
            if (Target != null) Target.Release();
            Target = new RenderTexture( size.X, size.Y, (int)UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_DXT1_SRGB, UnityEngine.Experimental.Rendering.DefaultFormat.DepthStencil);
        }

        public override void Draw(Graphics device, RenderTexture text)
        {
            var oldTargets = device;
            
            Camera.ProjectionDirty();
            base.Draw(device, text);
            
        }
    }
}
