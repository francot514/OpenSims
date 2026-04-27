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

namespace FSO.Common.Utils
{
    public class TextureGenerator //a fun class for cpu generating textures
    {
        private static Texture2D PxWhite;
        private static Texture2D PieButtonImg;

        private static Texture2D InteractionInactive;
        private static Texture2D InteractionActive;
        private static Texture2D CatalogInactive;
        private static Texture2D CatalogActive;
        private static Texture2D CatalogDisabled;
        private static Texture2D PieBG;
        private static Texture2D[] WallZBuffer;
        private static Texture2D[] AirTiles;
        private static Texture2D MotiveArrow; //actually a diamond, clip to get required direction
        private static Texture2D TerrainNoise;

        public static Texture2D GetPxWhite(Graphics gd)
        {
            if (PxWhite == null) PxWhite = TextureUtils.TextureFromColor(gd, Color.white);
            return PxWhite;
        }

        public static Texture2D GetTerrainNoise(Graphics gd)
        {
            if (TerrainNoise == null)
            {
                TerrainNoise = new Texture2D( 512, 512);
                Color[] data = new Color[512 * 512];

                var rd = new System.Random();
                for (int i = 0; i < data.Length; i++)
                {
                    //distribution is an average of two noise functions.
                    data[i].r = (byte)((rd.Next(255) + rd.Next(255)) / 2);
                    data[i].g = (byte)((rd.Next(255) + rd.Next(255)) / 2);
                    data[i].b = (byte)((rd.Next(255) + rd.Next(255)) / 2);
                    data[i].a = (byte)((rd.Next(255) + rd.Next(255)) / 2);
                }
                TextureUtils.UploadWithMips(TerrainNoise, gd, data);
            }
            return TerrainNoise;
        }

        public static Texture2D GetPieButtonImg(Graphics gd)
        {
            if (PieButtonImg == null) PieButtonImg = GenerateRoundedRectangle(gd, new Color(0, 40, 140), 27, 27, 6);
            return PieButtonImg;
        }

        public static Texture2D GetInteractionActive(Graphics gd)
        {
            if (InteractionActive == null) InteractionActive = GenerateObjectIconBorder(gd, new Color(255, 255, 0), new Color(56, 88, 120));
            return InteractionActive;
        }

        public static Texture2D GetInteractionInactive(Graphics gd)
        {
            if (InteractionInactive == null) InteractionInactive = GenerateObjectIconBorder(gd, new Color(128, 128, 128), new Color(56, 88, 120));
            return InteractionInactive;
        }

        public static Texture2D GetCatalogInactive(Graphics gd)
        {
            if (CatalogInactive == null) CatalogInactive = GenerateCatalogIconBorder(gd, new Color(140, 170, 206), new Color(56, 88, 120));
            return CatalogInactive;
        }

        public static Texture2D GetCatalogDisabled(Graphics gd)
        {
            if (CatalogDisabled == null) CatalogDisabled = GenerateCatalogIconBorder(gd, new Color(255, 0, 0), new Color(56, 88, 120));
            return CatalogDisabled;
        }

        public static Texture2D GetCatalogActive(Graphics gd)
        {
            if (CatalogActive == null) CatalogActive = GenerateCatalogIconBorder(gd, new Color(140, 170, 206), new Color(189, 215, 247));
            return CatalogActive;
        }

        public static Texture2D GetPieBG(Graphics gd)
        {
            if (PieBG == null)
            {
                PieBG = new Texture2D( 200, 200);
                Color[] data = new Color[200 * 200];
                int offset = 0;
                for (int y = 0; y < 200; y++)
                {
                    for (int x = 0; x < 200; x++)
                    {
                        data[offset++] = new Color(0, 0, 0, (float)Math.Min(1, 2 - Math.Sqrt(Math.Pow(y - 100, 2) + Math.Pow(x - 100, 2)) / 50) * 0.5f);
                    }
                }
                PieBG.SetPixels(data);
            }

            return PieBG;
        }

        public static Texture2D GetMotiveArrow(Graphics gd, Color highlight, Color bg)
        {
            if (MotiveArrow == null)
            {
                MotiveArrow = new Texture2D(5, 5);
                Color[] data = new Color[5 * 5];
                var size = new Vector2(5, 5);

                FillRect(data, size, new Rect(2, 0, 1, 1), Color.white);
                FillRect(data, size, new Rect(1, 1, 3, 1), Color.white);
                FillRect(data, size, new Rect(0, 2, 5, 1), Color.white);
                FillRect(data, size, new Rect(1, 3, 3, 1), Color.white);
                FillRect(data, size, new Rect(2, 4, 1, 1), Color.white);

                MotiveArrow.SetPixels(data);
            }
            return MotiveArrow;
        }

        public static float FLAT_Z_INC = 1.525f;
        public static float[][] WallZBufferConfig = new float[][] {
            // format: width, height, startIntensity, Xdiff, Ydiff

            new float[] {64, 271, 74, 1, 0.5f}, //near top left
            new float[] {64, 271, 135, -1, 0.5f}, //near top right
            new float[] {128, 240, 89.5f, 0, 0.5f}, //near horiz diag
            new float[] {16, 232, 45, 0, 0.5f}, //near vert diag

            new float[] {32, 135, 74, 2, 1f}, //med top left
            new float[] {32, 135, 135, -2, 1f}, //med top right
            new float[] {64, 120, 89.5f, 0, 1f}, //med horiz diag
            new float[] {8, 116, 45, 0, 1f}, //med vert diag

            new float[] {16, 67, 74, 4, 2f}, //far top left
            new float[] {16, 67, 135, -4, 2f}, //far top right
            new float[] {32, 60, 89.5f, 0, 2f}, //far horiz diag
            new float[] {4, 58, 45, 0, 2f}, //far vert diag

            //12
            new float[] {128, 64, 255, 0, -FLAT_Z_INC}, //near floor
            new float[] {64, 32, 255, 0, -FLAT_Z_INC*2}, //med floor
            new float[] {32, 16, 255, 0, -FLAT_Z_INC*4}, //far floor

            //vert flips of the above
            //15
            new float[] {128, 64, 153, 0, FLAT_Z_INC},
            new float[] {64, 32, 153, 0, FLAT_Z_INC*2},
            new float[] {32, 16, 153, 0, FLAT_Z_INC*4},

            //18
            new float[] {128, 64, 257, 0, -FLAT_Z_INC}, //near junction walls up
            new float[] {64, 32, 257, 0, -FLAT_Z_INC*2}, //med junction walls up
            new float[] {32, 16, 257, 0, -FLAT_Z_INC*4}, //far junction walls up

            
            //versions for corners (man this is getting complicated)
            //21
            //top corner
            new float[] {43, 22, 254, 0, -FLAT_Z_INC}, //near
            new float[] {21, 12, 254, 0, -FLAT_Z_INC*2}, //med 
            new float[] {13, 7, 254, 0, -FLAT_Z_INC*4}, //far

            //24
            //side corner
            new float[] {35, 21, 254 - (FLAT_Z_INC* 22), 0, -FLAT_Z_INC}, //near
            new float[] {16, 13, 254 - (FLAT_Z_INC * 22), 0, -FLAT_Z_INC*2}, //med 
            new float[] {11, 8, 254 - (FLAT_Z_INC * 22), 0, -FLAT_Z_INC*4}, //far

            //27
            new float[] {41, 23, 254 - (FLAT_Z_INC * (64 - 23)), 0, -FLAT_Z_INC}, //near
            new float[] {18, 13, 254 - (FLAT_Z_INC * (64 - 23)), 0, -FLAT_Z_INC*2}, //med 
            new float[] {9, 8, 254 - (FLAT_Z_INC * (64 - 23)), 0, -FLAT_Z_INC*4}, //far

            //30
            new float[] {1, 1, 49, 0, 0} //balloon
        };

        public static Texture2D[] GetWallZBuffer(Graphics gd)
        {
            if (WallZBuffer == null)
            {
                var count = WallZBufferConfig.Length;
                WallZBuffer = new Texture2D[count];
                for (int i = 0; i < count; i++)
                {
                    var config = WallZBufferConfig[i];
                    int width = (int)config[0];
                    int height = (int)config[1];

                    WallZBuffer[i] = new Texture2D( width, height);
                    Color[] data = new Color[width * height];
                    int offset = 0;

                    float yInt = config[2];
                    for (int y = 0; y < height; y++)
                    {
                        float xInt = yInt;
                        for (int x = 0; x < width; x++)
                        {
                            byte zCol = (byte)Math.Round(Math.Min(255, xInt));
                            data[offset++] = new Color(zCol, zCol, zCol, zCol);
                            xInt += config[3];
                        }
                        yInt += config[4];
                    }
                    WallZBuffer[i].SetPixels(data);
                }
            }

            return WallZBuffer;
        }

        public static Texture2D[] GetAirTiles(Graphics gd)
        {
            if (AirTiles == null)
            {
                AirTiles = new Texture2D[3];
                AirTiles[0] = GenerateAirTile(gd, 127, 64);
                AirTiles[1] = GenerateAirTile(gd, 63, 32);
                AirTiles[2] = GenerateAirTile(gd, 31, 16);
                
            }
            return AirTiles;
        }

        private static Texture2D GenerateAirTile(Graphics gd, int width, int height)
        {
            var tex = new Texture2D(width, height);
            Color[] data = new Color[width * height];

            int center = width/2;
            int middleOff = 0;
            for (int i=0; i<height; i++)
            {
                int index = i * width + (center - middleOff);
                for (int j=0; j<((middleOff==0)?1:2); j++)
                    data[index++] = (i+j > height / 2)?Color.black:Color.black;
                for (int j = 0; j < (middleOff * 2) - 3; j++)
                    if (i % 2 == 0 && (i + (center - middleOff)+j) % 4 == 0) data[index++] = Color.black;
                    else index++;
                if (middleOff != 0)
                {
                    for (int j = 0; j < 2; j++)
                        data[index++] = (i + (1-j) > height / 2) ? Color.black : Color.white;
                }

                middleOff += (i == height/2-1)?1:((i<height/2)?2:-2);
            }
            tex.SetPixels(data);
            return tex;
        }

        public static Texture2D GenerateObjectIconBorder(Graphics gd, Color highlight, Color bg)
        {
            var tex = new Texture2D( 45, 45);
            Color[] data = new Color[45*45];
            var size = new Vector2(45, 45);

            //border
            FillRect(data, size, new Rect(3, 0, 39, 2), highlight);
            FillRect(data, size, new Rect(0, 3, 2, 39), highlight);
            FillRect(data, size, new Rect(3, 43, 39, 2), highlight);
            FillRect(data, size, new Rect(43, 3, 2, 39), highlight);
            //end border

            //bg
            FillRect(data, size, new Rect(2, 2, 41, 41), bg);
            //end bg

            //top left rounded
            FillRect(data, size, new Rect(2, 1, 2, 2), highlight);
            FillRect(data, size, new Rect(1, 2, 2, 2), highlight);

            //top right rounded
            FillRect(data, size, new Rect(41, 1, 2, 2), highlight);
            FillRect(data, size, new Rect(42, 2, 2, 2), highlight);

            //btm left rounded
            FillRect(data, size, new Rect(1, 41, 2, 2), highlight);
            FillRect(data, size, new Rect(2, 42, 2, 2), highlight);

            //btm right rounded
            FillRect(data, size, new Rect(41, 42, 2, 2), highlight);
            FillRect(data, size, new Rect(42, 41, 2, 2), highlight);

            tex.SetPixels(data);
            return tex;
        }

        public static Texture2D GenerateCatalogIconBorder(Graphics gd, Color highlight, Color bg)
        {
            var tex = new Texture2D( 41, 41);
            Color[] data = new Color[41 * 41];
            var size = new Vector2(41, 41);

            //border
            FillRect(data, size, new Rect(2, 0, 37, 1), highlight);
            FillRect(data, size, new Rect(0, 2, 1, 37), highlight);
            FillRect(data, size, new Rect(2, 40, 37, 1), highlight);
            FillRect(data, size, new Rect(40, 2, 1, 37), highlight);
            //end border

            //bg
            FillRect(data, size, new Rect(1, 1, 39, 39), bg);
            //end bg

            //top left rounded
            FillRect(data, size, new Rect(1, 1, 1, 1), highlight);

            //top right rounded
            FillRect(data, size, new Rect(39, 1, 1, 1), highlight);

            //btm left rounded
            FillRect(data, size, new Rect(1, 39, 1, 1), highlight);

            //btm right rounded
            FillRect(data, size, new Rect(39, 39, 1, 1), highlight);

            tex.SetPixels(data);
            return tex;
        }

        public static Texture2D GenerateRoundedRectangle(Graphics gd, Color color, int width, int height, int radius)
        {
            var tex = new Texture2D(width, height);
            Color[] data = new Color[width * height];
            var size = new Vector2(width, height);

            //rect fills
            FillRect(data, size, new Rect(radius, radius, width - radius * 2, height - radius * 2), color);
            FillRect(data, size, new Rect(radius, 0, width - radius * 2, radius), color);
            FillRect(data, size, new Rect(radius, height-radius, width - radius * 2, radius), color);
            FillRect(data, size, new Rect(0, radius, radius, height-radius*2), color);
            FillRect(data, size, new Rect(width - radius, radius, radius, height - radius * 2), color);

            //corners now
            for (int i = 0; i < radius; i++)
            {
                int seg = (int)Math.Round(Math.Sin(Math.Acos((radius-(i+0.5))/radius))*radius);
                FillRect(data, size, new Rect(radius-seg, i, seg, 1), color);
                FillRect(data, size, new Rect(width-radius, i, seg, 1), color);
                FillRect(data, size, new Rect(radius - seg, height - i - 1, seg, 1), color);
                FillRect(data, size, new Rect(width-radius, height - i - 1, seg, 1), color);
            }

            tex.SetPixels(data);
            return tex;
        }

        private static void FillRect(Color[] data, Vector2 texSize, Rect dest, Color fillColor)
        {
            int x;
            int y=(int)dest.y;
            for (int i = 0; i < dest.height; i++)
            {
                x = (int)dest.x;
                for (int j = 0; j < dest.width; j++)
                {
                    data[y * (int)texSize.x + x] = fillColor;
                    x++;
                }
                y++;
            }
        }
    }
}
