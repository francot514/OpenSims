/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Common.Rendering.Framework.Camera;
using FSO.Common.Rendering.Framework.Model;
using System.Numerics;
using UnityEngine;

namespace FSO.Common.Rendering.Framework
{
    /// <summary>
    /// Base class for scenes with 3D elements.
    /// </summary>
    public abstract class _3DAbstract
    {
        public ICamera Camera;
        public string ID;
        public bool Visible = true;
        public abstract List<_3DComponent> GetElements();
        public abstract void Add(_3DComponent item);
        public abstract void Update(UpdateState state);
        public abstract void Draw(Graphics device, RenderTexture text);
        public object Controller { get; internal set; }

        public virtual void PreDraw(Graphics device)
        {
        }

        public virtual void Initialize(_3DLayer layer)
        {
        }

        /// <summary>
        /// Creates a new _3DAbstract instance.
        /// </summary>
        /// <param name="Device">A GraphicsDevice instance.</param>
        public _3DAbstract(Graphics Device)
        {
            m_Device = Device;
           
        }

        /// <summary>
        /// Called when m_Device is reset.
        /// </summary>
        private void m_Device_DeviceReset(object sender, EventArgs e)
        {
            DeviceReset(m_Device);
        }

        public void SetController(object controller)
        {
            this.Controller = controller;
        }

        public T FindController<T>()
        {
            if (Controller is T)
            {
                return (T)Controller;
            }
            return default(T);
        }


        public abstract void Dispose();

        protected Graphics m_Device; 

        public abstract void DeviceReset(Graphics Device);
        public static bool IsInvalidated;
    }
}
