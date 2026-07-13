/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Text;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Framework.Parser;
using FSO.Client.UI.Model;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Rendering.Framework.IO;
using FSO.Client.Utils;
using FSO.SimAntics;
using FSO.Common.Utils;
using FSO.Client.UI.Panels;
using UnityEngine;

namespace FSO.Client.UI.Controls
{
    /// <summary>
    /// Used to display an interaction. Will eventually have all features like the timer, big huge red x support for cancel etc.
    /// </summary>
    public class UIInteraction : UIElement
    {
        public Texture2D Icon;
        private UITooltipHandler m_TooltipHandler;
        private Texture2D Background;
        private Texture2D Overlay;
        private bool Active;
        private UIMouseEventRef ClickHandler;
        public event ButtonClickDelegate OnMouseEvent;
        public UIIQTrackEntry ParentEntry;

        public void SetCancelled()
        {
            Overlay = GetTexture((ulong)0x000003A100000001);
        }

        public void SetActive(bool active)
        {
            this.Active = active;
            if (active) Background = TextureGenerator.GetInteractionActive(GameFacade.GraphicsDevice);
            else Background = TextureGenerator.GetInteractionInactive(GameFacade.GraphicsDevice);
        }

        public UIInteraction(bool Active)
        {
            SetActive(Active);
            m_TooltipHandler = UIUtils.GiveTooltip(this);
            ClickHandler = ListenForMouse(new Rect(0, 0, 45, 45), new UIMouseEvent(MouseEvt));
        }

        private void MouseEvt(UIMouseEventType type, UpdateState state)
        {
            if (type == UIMouseEventType.MouseDown) OnMouseEvent(this); //pass to parents to handle
        }

        public override void Draw(UISpriteBatch batch)
        {
            DrawLocalTexture(batch, Background, new Vector2(0, 0));
            if (Icon != null)
            {
                if (Icon.width <= 45) {
                    DrawLocalTexture(batch, Icon, new Rect(0, 0, Icon.width, Icon.height), new Vector2(4, 4), new Vector2(37f/Icon.width, 37f/Icon.height));
                }
                else DrawLocalTexture(batch, Icon, new Rect(0, 0, Icon.width / 2, Icon.height), new Vector2(4, 4));
            }
            if (Overlay != null) DrawLocalTexture(batch, Overlay, new Vector2(0, 0));
        }

        public override Rect GetBounds()
        {
            return new Rect(0, 0, ClickHandler.Region.width, ClickHandler.Region.height); 
        }
    }
}
