using FSO.Client.UI.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Common.Rendering.Framework.Model;
using FSO.Client.UI.Controls;
using UnityEngine;

namespace FSO.Client.UI.Panels
{
    public class UIChatBalloon : UIContainer
    {
        private Texture2D BPointerBottom;
        private Texture2D BPointerSide;
        private Texture2D BTiles;

        private static bool ProcessedBGFX = false;

        private TextRendererResult BodyTextLabels;
        private List<Vector2> BTOffsets;
        private TextStyle BodyTextStyle;
        private TextStyle ShadowStyle;
        private string BodyText;
        public int FadeTime;

        private UIChatPanel Owner;

        public Color Color;

        public string Name;
        public string Message;
        public float Alpha;

        public Vector2 TargetLocation;

        public Rect DisplayRect;
        public Vector2 TargetPt;

        public Vector2 DesiredRectPos;
        private bool Offscreen;
        public int ClosestDir = 3; //left, up, right, down, N/A

        public UIChatBalloon(UIChatPanel owner)
        {
            Owner = owner;
            var gfx = Content.Content.Get().UIGraphics;
            //TODO: switch entire ui onto real content system

            BPointerBottom = GetTexture(0x1AF0856DDBAC);
            BPointerSide = GetTexture(0x1B00856DDBAC);
            BTiles = GetTexture(0x1B10856DDBAC);

            if (!ProcessedBGFX)
            {
                ProcessedBGFX = true;
                AlphaCopy(BPointerBottom);
                AlphaCopy(BPointerSide);
                AlphaCopy(BTiles);
            }

            BodyTextStyle = TextStyle.DefaultLabel.Clone();
            BodyTextStyle.Size = 10;
            BodyTextStyle.Color = new Color(240, 240, 48);

            ShadowStyle = BodyTextStyle.Clone();
            ShadowStyle.Color = Color.black;
        }

        public void SetNameMessage(string name, string message)
        {
            Name = name;
            Message = message;
            Offscreen = false;
            if (message == "") name = "";
            TextChanged();
        }

        private void TextChanged()
        {
            BodyText = ((Offscreen && Message != "") ? "[" + Name + "] " : "") + Message;

            var textW = Math.Max(130, Message.Length/2);
            BodyTextLabels = TextRenderer.ComputeText(BodyText, new TextRendererOptions
            {
                Alignment = Framework.TextAlignment.Center,
                MaxWidth = textW,
                Position = new Vector2(18, 18),
                Scale = _Scale,
                TextStyle = BodyTextStyle,
                WordWrap = true,
            }, this);

            BTOffsets = new List<Vector2>();

            foreach (var cmd in BodyTextLabels.DrawingCommands)
            {
                if (cmd is TextDrawCmd_Text) BTOffsets.Add(((TextDrawCmd_Text)cmd).Position);
            }

            DisplayRect.width = textW + 18 * 2;
            DisplayRect.height = BodyTextLabels.BoundingBox.Height + 18 * 2;
        }

        private void AlphaCopy(Texture2D tex)
        {
            var data = new Color[tex.width * tex.height];
            tex.GetPixelData<Color>(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                data[i].a = data[i].r;
            }
            tex.SetPixelData<Color>(data, 1,0);
            
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);

            UpdateDesiredPosition();

            DisplayRect.position = DesiredRectPos;

            DeterminePointSide();
        }

        public void UpdateDesiredPosition()
        {
            DesiredRectPos = new Vector2((int)(TargetPt.x - DisplayRect.width / 2), (int)(TargetPt.y - (DisplayRect.height + 20)));
            var dr = new Rect(DesiredRectPos.x, DesiredRectPos.y, DisplayRect.width, DisplayRect.height); 

            bool changed = false;
            foreach (var area in Owner.InvalidAreas)
            {
                if (dr.Overlaps(area))
                {
                    //move desired rectangle out of area
                    //first determine problem direction
                    var xDist = (area.X + area.Width / 2) - (dr.x + dr.width / 2);
                    var yDist = (area.Y + area.Height / 2) - (dr.y + dr.height / 2);

                    if (Math.Abs(xDist) > Math.Abs(yDist))
                    {
                        if (xDist < 0) dr.x = area.Right;
                        else dr.x = area.Left-dr.width;
                    } else
                    {
                        if (yDist < 0) dr.y = area.Bottom;
                        else dr.y = area.Top-dr.height;
                    }
                    changed = true;
                }
            }

            if (changed)
            {
                if (!Offscreen)
                {
                    Offscreen = true;
                    TextChanged();
                }
            }
            else
            {
                if (Offscreen)
                {
                    Offscreen = false;
                    TextChanged();
                }
            }

            DesiredRectPos = dr.position;
        }

        public void DeterminePointSide()
        {
            float xDist = TargetPt.x - (DisplayRect.x + DisplayRect.width / 2);
            float ax = (Math.Abs(xDist) - DisplayRect.width / 2);

            float yDist = TargetPt.y - (DisplayRect.y + DisplayRect.height / 2);
            float ay = (Math.Abs(yDist) - DisplayRect.height / 2);

            if (ax < 30 && ay < 30)
            {
                ClosestDir = 4;
            }

            if (ax > ay)
            {
                //x pointer
                if (DisplayRect.height < 80) ClosestDir = 4; //cannot fit horizontal arrow
                else if (xDist < 0) ClosestDir = 0; //left
                else ClosestDir = 2; //right
            }
            else
            {
                //y pointer
                if (yDist < 0) ClosestDir = 1; //up
                else ClosestDir = 3; //down
            }
        }

        public override void Draw(UISpriteBatch batch)
        {
            if (Alpha == 0) return;
            base.Draw(batch);
            Color bgCol = new Color(8,8,128) * Alpha;
            
            //draw corners
            DrawLocalTexture(batch, BTiles, new Rect(0, 0, 40, 40), new Vector2(DisplayRect.left-20, DisplayRect.top-20), Vector2.one, bgCol);
            DrawLocalTexture(batch, BTiles, new Rect(40, 0, 40, 40), new Vector2(DisplayRect.right + 20, DisplayRect.top - 20), new Vector2(-1,1), bgCol);
            DrawLocalTexture(batch, BTiles, new Rect(80, 0, 40, 40), new Vector2(DisplayRect.right + 20, DisplayRect.bottom + 20), new Vector2(-1, -1), bgCol);
            DrawLocalTexture(batch, BTiles, new Rect(120, 0, 40, 40), new Vector2(DisplayRect.left - 20, DisplayRect.bottom + 20), new Vector2(1, -1), bgCol);

            //draw edges
            //if the pointer is on this edge, it needs to be split into 3... Before point, point and after point. 

            var vertH = DisplayRect.height - 40;
            var vertPt = Math.Max(DisplayRect.top + 20, Math.Min(DisplayRect.bottom - 60, TargetPt.y-20)) - (DisplayRect.top + 20);

            var horizW = DisplayRect.width - 40;
            var horizPt = Math.Max(DisplayRect.left + 20, Math.Min(DisplayRect.right - 60, TargetPt.x -20)) - (DisplayRect.left + 20);

            int ptSel = 0;

            //left
            if (ClosestDir == 0)
            {
                ptSel = Math.Max(0, Math.Min(3, (int)Math.Floor((DisplayRect.left - TargetPt.x) / 40f)));
                DrawLocalTexture(batch, BTiles, new Rect(0, 40, 40, 40), new Vector2(DisplayRect.left - 20, DisplayRect.top + 20), new Vector2(1, vertPt / 40f), bgCol);
                DrawLocalTexture(batch, BPointerSide, new Rect(0, ptSel * 40, 200, 40), new Vector2(DisplayRect.left - 180, DisplayRect.top + 20 + vertPt), Vector2.one, bgCol);
                DrawLocalTexture(batch, BTiles, new Rect(0, 40, 40, 40), new Vector2(DisplayRect.left - 20, DisplayRect.top + 60 + vertPt), new Vector2(1, (vertH-(vertPt+40)) / 40f), bgCol);
            }
            else DrawLocalTexture(batch, BTiles, new Rect(0, 40, 40, 40), new Vector2(DisplayRect.left - 20, DisplayRect.top + 20), new Vector2(1, (DisplayRect.height-40)/40f), bgCol);

            //top
            if (ClosestDir == 1)
            {
                ptSel = Math.Max(0, Math.Min(3, (int)Math.Floor((DisplayRect.top - TargetPt.y) / 40f)));
                DrawLocalTexture(batch, BTiles, new Rect(0, 80, 40, 40), new Vector2(DisplayRect.left + 20, DisplayRect.top - 20), new Vector2(horizPt / 40f, 1), bgCol);
                DrawLocalTexture(batch, BPointerBottom, new Rect(ptSel * 40, 0, 40, 200), new Vector2(DisplayRect.left + 20 + horizPt, DisplayRect.top + 20), new Vector2(1, -1), bgCol);
                DrawLocalTexture(batch, BTiles, new Rect(0, 80, 40, 40), new Vector2(DisplayRect.left + 60 + horizPt, DisplayRect.top - 20), new Vector2(((horizW - (horizPt + 40)) / 40f), 1), bgCol);
            }
            else DrawLocalTexture(batch, BTiles, new Rect(0, 80, 40, 40), new Vector2(DisplayRect.left + 20, DisplayRect.top - 20), new Vector2((DisplayRect.width - 40) / 40f, 1), bgCol);

            //right
            if (ClosestDir == 2)
            {
                ptSel = Math.Max(0, Math.Min(3, (int)Math.Floor((TargetPt.x - DisplayRect.right) / 40f)));
                DrawLocalTexture(batch, BTiles, new Rect(0, 40, 40, 40), new Vector2(DisplayRect.right + 20, DisplayRect.top + 20), new Vector2(-1, vertPt / 40f), bgCol);
                DrawLocalTexture(batch, BPointerSide, new Rect(0, ptSel * 40, 200, 40), new Vector2(DisplayRect.right + 180, DisplayRect.top + 20 + vertPt), new Vector2(-1, 1), bgCol);
                DrawLocalTexture(batch, BTiles, new Rect(0, 40, 40, 40), new Vector2(DisplayRect.right + 20, DisplayRect.top + 60 + vertPt), new Vector2(-1, (vertH - (vertPt + 40)) / 40f), bgCol);
            }
            else DrawLocalTexture(batch, BTiles, new Rect(0, 40, 40, 40), new Vector2(DisplayRect.right + 20, DisplayRect.top + 20), new Vector2(-1, (DisplayRect.height - 40) / 40f), bgCol);

            //bottom
            if (ClosestDir == 3)
            {
                ptSel = Math.Max(0, Math.Min(3, (int)Math.Floor((TargetPt.y - DisplayRect.bottom) / 40f)));
                DrawLocalTexture(batch, BTiles, new Rect(0, 120, 40, 40), new Vector2(DisplayRect.left + 20, DisplayRect.bottom - 20), new Vector2(horizPt / 40f, 1), bgCol);
                DrawLocalTexture(batch, BPointerBottom, new Rect(ptSel * 40, 0, 40, 200), new Vector2(DisplayRect.left + 20 + horizPt, DisplayRect.bottom - 20), Vector2.one, bgCol);
                DrawLocalTexture(batch, BTiles, new Rect(0, 120, 40, 40), new Vector2(DisplayRect.left + 60 + horizPt, DisplayRect.bottom - 20), new Vector2(((horizW - (horizPt + 40)) / 40f), 1), bgCol);
            }
            else DrawLocalTexture(batch, BTiles, new Rect(0, 120, 40, 40), new Vector2(DisplayRect.left + 20, DisplayRect.bottom - 20), new Vector2((DisplayRect.width - 40) / 40f, 1), bgCol);

            //draw middle
            DrawLocalTexture(batch, BTiles, new Rect(40, 120, 1, 1), new Vector2(DisplayRect.left + 20, DisplayRect.top + 20), new Vector2(DisplayRect.width-40, DisplayRect.height-40), bgCol);


            Vector2 offpos = new Vector2(DisplayRect.x + 1, DisplayRect.y + 1);
            int posi = 0;
            foreach (var cmd in BodyTextLabels.DrawingCommands)
            {
                if (cmd is TextDrawCmd_Text)
                {
                    ((TextDrawCmd_Text)cmd).Style = ShadowStyle;
                    ((TextDrawCmd_Text)cmd).Position = BTOffsets[posi++] + offpos;
                }
            }

            ShadowStyle.Color = Color.black * Alpha;
            TextRenderer.DrawText(BodyTextLabels.DrawingCommands, this, batch);

            posi = 0;
            offpos = new Vector2(DisplayRect.x, DisplayRect.y);
            foreach (var cmd in BodyTextLabels.DrawingCommands)
            {
                if (cmd is TextDrawCmd_Text) {
                    ((TextDrawCmd_Text)cmd).Style = BodyTextStyle;
                    ((TextDrawCmd_Text)cmd).Position = BTOffsets[posi++] + offpos;
                }
            }
            BodyTextStyle.Color = this.Color * Alpha;
            TextRenderer.DrawText(BodyTextLabels.DrawingCommands, this, batch);

            this.Position = new Vector2();
        }
    }
}
