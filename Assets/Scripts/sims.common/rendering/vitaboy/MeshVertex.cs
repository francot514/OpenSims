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
using UnityEngine;


namespace FSO.Vitaboy
{
    /// <summary>
    /// Vertex that makes up a mesh.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MeshVertex
    {
        public Vector3 Position;
        /** UV Mapping **/
        public Vector2 UV;
        public Vector3 Normal;

        public static int SizeInBytes = sizeof(float) * 8;

       
    }
}
