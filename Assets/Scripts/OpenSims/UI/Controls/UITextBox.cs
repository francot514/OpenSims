/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Model;
using FSO.Client.Utils;
using FSO.Common.Rendering.Framework.IO;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Utils;
using FSO.Client.GameContent;
using FSO.Common;
using System.Threading;
using UnityEngine;

namespace FSO.Client.UI.Controls
{
    public class UITextBox : UIContainer, IFocusableUI, ITextControl
    {

        public static ITextureRef StandardBackground;

        static UITextBox()
        {
            StandardBackground = new SlicedTextureRef(
                UIElement.GetTexture((ulong)FileIDs.UIFileIDs.dialog_textboxbackground),
                new Rect(13, 13, 13, 13)
            );
        }

        /**
         * Background texture & resize info
         */
        private Texture2D m_BackgroundTex;
        private NineSliceMargins NineSliceMargins;
        private float m_Width;
        private float m_Height;

        /**
         * Text box vars
         */
        private StringBuilder m_SBuilder = new StringBuilder();


        /**
         * Interaction
         */
        private UIMouseEventRef m_MouseEvent;

        public event KeyPressDelegate OnEnterPress;
        public event KeyPressDelegate OnTabPress;

        private int SelectionStart = -1;
        private int SelectionEnd = -1;


        public UITextBox()
        {
            this.SetBackgroundTexture(
                GetTexture((ulong)FileIDs.UIFileIDs.dialog_textboxbackground),
                13, 13, 13, 13);

            TextMargin = new Rect(8, 3, 8, 5);

            m_MouseEvent = ListenForMouse(new Rect(0, 0, 10, 10), new UIMouseEvent(OnMouseEvent));
        }

        /**
         * Functionality
         */

        /// <summary>
        /// Returns the current text (input) in this textbox.
        /// </summary>
        public string CurrentText
        {
            get { return m_SBuilder.ToString(); }
            set
            {
                m_SBuilder = new StringBuilder(value);
                SelectionStart = Math.Max(0, Math.Min(SelectionStart, value.Length - 1));
                SelectionEnd = -1; //todo: move along maybe?
                m_DrawDirty = true;
            }
        }

        public void Clear()
        {
            SelectionEnd = -1;
            SelectionStart = -1;
            m_SBuilder.Clear();
        }

        public TextStyle TextStyle = TextStyle.DefaultLabel;
        public Rect TextMargin = Rect.zero;

        /**
         * Properties
         */
        
        /// <summary>
        /// Background texture
        /// </summary>
        public Texture2D BackgroundTexture
        {
            get { return m_BackgroundTex; }
        }


        public void SetBackgroundTexture(Texture2D texture, int marginLeft, int marginRight, int marginTop, int marginBottom)
        {
            m_BackgroundTex = texture;
            if (texture != null)
            {
                NineSliceMargins = new NineSliceMargins
                {
                    Left = marginLeft,
                    Right = marginRight,
                    Top = marginTop,
                    Bottom = marginBottom
                };
                NineSliceMargins.CalculateOrigins(texture);
            }
            else
            {
                NineSliceMargins = null;
            }
        }

        /// <summary>
        /// Component width
        /// </summary>
        public float Width
        {
            get { return m_Height; }
        }

        /// <summary>
        /// Component height
        /// </summary>
        public float Height
        {
            get { return m_Height; }
        }


        public void SetSize(float width, float height)
        {
            m_Width = width;
            m_Height = height;
            
            NineSliceMargins.CalculateScales(m_Width, m_Height);
            m_Bounds = new Rect(0, 0, (int)m_Width, (int)m_Height);
            
            if (m_MouseEvent != null)
            {
                m_MouseEvent.Region = new Rect(0, 0, (int)m_Width, (int)m_Height);
            }
        }

        private Rect m_Bounds;
        public override Rect GetBounds()
        {
            return m_Bounds;
        }


        /**
         * Interaction Functionality
         */
        public void OnMouseEvent(UIMouseEventType evt, UpdateState state)
        {
            switch (evt)
            {
                case UIMouseEventType.MouseUp:
                    state.isMouse = true;
                    break;

                case UIMouseEventType.MouseOver:
                    break;

                case UIMouseEventType.MouseOut:
                    break;
            }
        }



        #region IFocusableUI Members

        private bool IsFocused;
        private string QueuedChange;
        public void OnFocusChanged(FocusEvent newFocus)
        {
            IsFocused = newFocus == FocusEvent.FocusIn;
            if (IsFocused)
            {
                m_cursorBlink = true;
                m_cursorBlinkLastTime = (long)Time.time;
                if (FSOEnvironment.SoftwareKeyboard)
                {
                   
                }
            }
            else
            {
                m_cursorBlink = false;
            }
        }

        #endregion

        private bool m_cursorBlink = false;
        private long m_cursorBlinkLastTime;

        public override void Update(UpdateState state)
        {
            base.Update(state);
            lock (this) {
                if (QueuedChange != null) {
                    CurrentText = QueuedChange;
                    QueuedChange = null;
                }
            }
            if (FSOEnvironment.SoftwareKeyboard)
            if (IsFocused)
            {
                /**
                 * TODO: Selection management
                 */
                var now = Time.fixedTime;
                if (now - m_cursorBlinkLastTime > 5000000)
                {
                    m_cursorBlinkLastTime = (long)now;
                    m_cursorBlink = !m_cursorBlink;
                }

                var inputResult = state.InputManager.ApplyKeyboardInput(m_SBuilder, state, SelectionStart, SelectionEnd, true);
                if (inputResult != null)
                {
                    if (inputResult.ContentChanged)
                    {
                        m_cursorBlink = true;
                        m_cursorBlinkLastTime = (long)Time.time;

                        /** We need to recompute the drawing commands **/
                        m_DrawDirty = true;

                        if (SelectionStart != -1)
                        {
                            SelectionStart += inputResult.NumInsertions;
                            SelectionStart -= inputResult.NumDeletes;
                        }
                    }

                    /**
                     * Selection?
                     */
                    foreach (var key in inputResult.UnhandledKeys)
                    {
                        if (key == KeyCode.LeftArrow)
                                if (inputResult.ShiftDown)                                {
                                    /** SelectionEnd**/
                                    if (SelectionEnd == -1)
                                    {
                                        if (SelectionStart != -1)
                                        {
                                            SelectionEnd = SelectionStart;
                                        }
                                        else
                                        {
                                            SelectionEnd = m_SBuilder.Length;
                                        }
                                    }
                                    SelectionEnd--;
                                    SelectionEnd = Math.Max(SelectionEnd, 0);
                                    if (SelectionEnd > m_SBuilder.Length) { SelectionEnd = m_SBuilder.Length; }

                                    /** Selection size = 0, act as if there is no selection **/
                                    if (SelectionEnd == SelectionStart)
                                    {
                                        SelectionEnd = -1;
                                    }
                                }
                                else
                                {
                                    if (SelectionEnd != -1)
                                    {
                                        SelectionStart = SelectionEnd;
                                        SelectionEnd = -1;
                                    }
                                    if (SelectionStart == -1)
                                    {
                                        SelectionStart = m_SBuilder.Length - 1;
                                    }
                                    else
                                    {
                                        SelectionStart--;
                                    }
                                    if (SelectionStart < 0) { SelectionStart = 0; }
                                }
                                
                               

                           else if (key == KeyCode.RightArrow)
                                if (inputResult.ShiftDown)
                                {
                                    /** SelectionEnd**/
                                    if (SelectionEnd == -1)
                                    {
                                        if (SelectionStart != -1)
                                        {
                                            SelectionEnd = SelectionStart;
                                        }
                                        else
                                        {
                                            SelectionEnd = m_SBuilder.Length;
                                        }
                                    }
                                    SelectionEnd++;
                                    if (SelectionEnd > m_SBuilder.Length) { SelectionEnd = m_SBuilder.Length; }

                                    /** Selection size = 0, act as if there is no selection **/
                                    if (SelectionEnd == SelectionStart)
                                    {
                                        SelectionEnd = -1;
                                    }
                                }
                                else
                                {
                                    if (SelectionEnd != -1)
                                    {
                                        SelectionStart = SelectionEnd;
                                        SelectionEnd = -1;
                                    }

                                    if (SelectionStart != -1)
                                    {
                                        SelectionStart++;
                                        if (SelectionStart >= m_SBuilder.Length)
                                        {
                                            SelectionStart = -1;
                                        }
                                    }
                                }
                                m_DrawDirty = true;
                               
                        }
                    }
                    if (inputResult.EnterPressed && OnEnterPress != null) OnEnterPress(this);
                    if (inputResult.TabPressed && OnTabPress != null) OnTabPress(this);
            }
        }
        

        private bool m_DrawDirty = false;
        private List<ITextDrawCmd> m_DrawCmds = new List<ITextDrawCmd>();
        private Vector2 m_CursorPosition = Vector2.zero;

        /// <summary>
        /// When the text / scroll / highlight changes we need to
        /// re-compute how we are going to draw this text field
        /// </summary>
        private void ComputeDrawingCommands()
        {
            m_DrawCmds.Clear();
            m_DrawDirty = false;

            var topLeft = new Vector2(TextMargin.left, TextMargin.top);
            var cursorPosition = topLeft;

            var txt = m_SBuilder.ToString();
            var txtScale = TextStyle.Scale * _Scale;


            if (SelectionEnd != -1)
            {
                var start = SelectionStart == -1 ? m_SBuilder.Length : SelectionStart;
                var end = SelectionEnd;
                if (end < start) {
                    var temp = start;
                    start = end;
                    end = temp;
                }

                var prefixSize = Vector2.zero;
                if (start > 0)
                {
                    /** Prefix **/
                    var prefix = txt.Substring(0, start);
                    prefixSize = new Vector2(TextStyle.SpriteFont.fontSize, TextStyle.Scale);

                    m_DrawCmds.Add(new TextDrawCmd_Text
                    {
                        Text = prefix,
                        Style = TextStyle,
                        Position = LocalPoint(topLeft),
                        Scale = txtScale
                    });
                }


                /** Selection text **/
                var selectionTxt = txt.Substring(start, end - start);
                var selectionPosition = LocalPoint(new Vector2(prefixSize.x + topLeft.y, topLeft.y));
                var selectionTxtSize = new Vector2(TextStyle.SpriteFont.fontSize, TextStyle.Scale);

                /** Selection box **/
                m_DrawCmds.Add(new TextDrawCmd_SelectionBox {
                    BlendColor = TextStyle.SelectionBoxColor,
                    Texture = TextureGenerator.GetPxWhite(GameFacade.GraphicsDevice),
                    Position = selectionPosition,
                    Scale = new Vector2(selectionTxtSize.x, selectionTxtSize.y) * _Scale
                });
                
                m_DrawCmds.Add(new TextDrawCmd_Text
                {
                    Selected = true,
                    Text = selectionTxt,
                    Style = TextStyle,
                    Position = selectionPosition,
                    Scale = txtScale
                });


                if (end < txt.Length)
                {
                    /** Suffix **/
                    m_DrawCmds.Add(new TextDrawCmd_Text
                    {
                        Text = txt.Substring(end),
                        Style = TextStyle,
                        Position = LocalPoint(new Vector2(prefixSize.x + selectionTxtSize.x + topLeft.x, topLeft.y)),
                        Scale = txtScale
                    });
                }
            }
            else
            {
                m_DrawCmds.Add(new TextDrawCmd_Text
                {
                    Text = txt,
                    Style = TextStyle,
                    Position = LocalPoint(topLeft),
                    Scale = txtScale
                });

                var cursorPrefix = txt;
                if (SelectionStart != -1)
                {
                    cursorPrefix = txt.Substring(0, SelectionStart);
                }

                var stringSize = new Vector2(TextStyle.SpriteFont.fontSize, TextStyle.Scale);
                cursorPosition = LocalPoint(new Vector2(stringSize.x + topLeft.x, topLeft.y));
            }



            m_DrawCmds.Add(new TextDrawCmd_Cursor
            {
                Scale = new Vector2(_Scale.x, (m_Height-(TextMargin.top + TextMargin.height)) * _Scale.y),
                Position = cursorPosition,
                Texture = TextureGenerator.GetPxWhite(GameFacade.GraphicsDevice),
                Color = TextStyle.Color
            });




            //var str = m_SBuilder.ToString();
            //if (IsFocused)
            //{
                //if (SelectionStart != -1)
                //{
                    /** We need to draw selection! **/
                //}

                //if (m_cursorBlink)
                //{
                //    str += "|";
                //}
            //}
        }



        /// <summary>
        /// Render
        /// </summary>
        /// <param name="batch"></param>
        public override void Draw(UISpriteBatch batch)
        {
            if (!Visible) return;
            if (m_DrawDirty)
            {
                ComputeDrawingCommands();
            }

            /** Can have a text box without a background **/
            if (m_BackgroundTex != null && NineSliceMargins != null)
            {
                NineSliceMargins.DrawOnto(batch, this, m_BackgroundTex, m_Width, m_Height);
            }
            
            /**
             * Draw text
             */
            foreach (var cmd in m_DrawCmds)
            {
                cmd.Draw(this, batch);
            }
        }



        protected override void CalculateMatrix()
        {
            base.CalculateMatrix();
            m_DrawDirty = true;
        }

        #region ITextControl Members

        bool ITextControl.DrawCursor
        {
            get
            {
                return IsFocused && m_cursorBlink;
            }
        }

        #endregion
    }


    public interface ITextDrawCmd
    {
        void Draw(UIElement ui, UISpriteBatch batch);
        void Init();
    }

    public class TextDrawCmd_Text : ITextDrawCmd
    {
        public bool Selected;
        public Vector2 Position;
        public string Text;
        public TextStyle Style;
        public Vector2 Scale;


        public void Init()
        {
            //Position.Y += Style.BaselineOffset;
        }

        #region ITextDrawCmd Members
        public virtual void Draw(UIElement ui, UISpriteBatch batch)
        {
            
        }
        #endregion
    }

    public interface ITextControl
    {
        bool DrawCursor { get; }
    }

    public class TextDrawCmd_Cursor : ITextDrawCmd
    {
        public Vector2 Position;
        public Texture2D Texture;
        public Color Color;
        public Vector2 Scale;

        public void Init()
        {
        }

        public void Draw(UIElement ui, UISpriteBatch batch)
        {
            if (((ITextControl)ui).DrawCursor)
            {
               
            }
        }
    }


    public class TextDrawCmd_SelectionBox : ITextDrawCmd
    {
        public Texture2D Texture;
        public Vector2 Position;
        public Vector2 Scale;
        public Color BlendColor;

        public void Init()
        {
        }

        public void Draw(UIElement ui, UISpriteBatch batch)
        {
            //batch.Draw(Texture, Position, null, BlendColor, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }
    }

}
