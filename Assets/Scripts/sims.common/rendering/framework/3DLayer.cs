/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using FSO.Common.Rendering.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FSO.Common.Rendering.Framework
{
    public class _3DLayer : IGraphicsLayer
    {
        public Graphics Device;
        public List<_3DAbstract> Scenes = new List<_3DAbstract>();
        public List<_3DAbstract> External = new List<_3DAbstract>();

        #region IGraphicsLayer Members

        public void Update(UpdateState state)
        {
            foreach (var scene in Scenes)
            {
                scene.Update(state);
            }
            foreach (var scene in External)
            {
                scene.Update(state);
            }

            
        }

        public void PreDraw(Graphics device)
        {
            foreach (var scene in Scenes)
            {
                if (scene.Visible) scene.PreDraw(device);
            }
        }

        public void Draw(Graphics device, RenderTexture text)
        {
            foreach (var scene in Scenes)
            {
                if (scene.Visible) scene.Draw(device, text);
            }
        }

        public void Initialize(Graphics device)
        {
            this.Device = device;
            foreach (var scene in Scenes)
            {
                scene.Initialize(this);
            }
            foreach (var scene in External)
            {
                scene.Initialize(this);
            }
        }

        public void Add(_3DAbstract scene)
        {
            Scenes.Add(scene);
            if (this.Device != null)
            {
                scene.Initialize(this);
            }
        }

        public void Remove(_3DAbstract scene)
        {
            Scenes.Remove(scene);
        }

        /// <summary>
        /// Adds a scene to the draw stack. The system will not call
        /// Draw on the scene but it will be initialized and given updates
        /// </summary>
        /// <param name="scene"></param>
        public void AddExternal(_3DAbstract scene){
            External.Add(scene);
            if (this.Device != null)
            {
                scene.Initialize(this);
            }
        }

        public void RemoveExternal(_3DAbstract scene)
        {
            External.Remove(scene);
        }

        #endregion
    }
}
