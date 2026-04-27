/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Numerics;

namespace FSO.Vitaboy.Model
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VitaboyVertex
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector3 BvPosition; //blend vert
        public Vector3 Parameters;
        public Vector3 Normal;

        public VitaboyVertex(Vector3 position, Vector2 textureCoords, Vector3 bvPosition, Vector3 parameters, Vector3 normal)
        {
            this.Position = position;
            this.TextureCoordinate = textureCoords;
            this.BvPosition = bvPosition;
            this.Parameters = parameters;
            this.Normal = normal;
        }

        public static int SizeInBytes = sizeof(float) * 14;

        
    }
}
