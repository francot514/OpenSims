/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FSO.Common.Rendering.Framework
{
    public abstract class Game
    {
        protected Graphics Graphics;
        protected GameScreen Screen;
        protected bool IsActive = false;

		public Game()
        {
            Graphics = new Graphics();
        }

        protected void Initialize(){
            

            Screen = new GameScreen(Graphics);
        }

        protected  void Update(Time gameTime){
            Screen.Update(gameTime, IsActive);
        }

        protected  void Draw(Time gameTime){
            
            Screen.Draw(gameTime);
        }
    }
}
