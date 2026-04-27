/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Common.Rendering.Framework.Model;
using UnityEngine;



namespace FSO.Common.Rendering.Framework
{
    /// <summary>
    /// A screen used for drawing.
    /// </summary>
    public class GameScreen
    {
        public List<IGraphicsLayer> Layers = new List<IGraphicsLayer>();
        public Graphics Device;
        public UpdateState State;

        
        private int touchedFrames;
		private int lastTouchCount;
		private Input lastMouseState;
        private Vector2? prevTouchAvg;
        private const int TOUCH_ACCEPT_TIME = 5;

        public GameScreen(Graphics device)
        {
            this.Device = device;

            State = new UpdateState();
        }

        private static List<char> TextCharacters = new List<char>();
       

        /// <summary>
        /// Adds a graphical element to this scene.
        /// </summary>
        /// <param name="layer">Element inheriting from IGraphicsLayer.</param>
        public void Add(IGraphicsLayer layer)
        {
            layer.Initialize(Device);
            Layers.Add(layer);
        }

        public void Update(Time time, bool hasFocus)
        {
            State.Time = time;
           
            State.FrameTextInput = TextCharacters;

            if (hasFocus)
            {
                State.MouseState = Input.GetMouseButton(0);
              
            }
            else
            {
                State.MouseState = Input.GetMouseButton(1);
                
            }

            State.SharedData.Clear();
            State.Update();

            foreach (var layer in Layers){
                layer.Update(State);
            }

            TextCharacters.Clear();
        }

       

        public void Draw(Time time)
        {
            lock (Device)
            {
                foreach (var layer in Layers)
                {
                    layer.PreDraw(Device);
                }
            }

           
            //Device.RasterizerState.AlphaBlendEnable = true;
            //Device.DepthStencilState.DepthBufferEnable = true;

            lock (Device)
            {
                foreach (var layer in Layers)
                {
                    layer.Draw(Device, null);
                }
            }
        }
    }
}
