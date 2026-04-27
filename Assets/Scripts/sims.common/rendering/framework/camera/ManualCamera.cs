/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace FSO.Common.Rendering.Framework.Camera
{
    public class ManualCamera : ICamera
    {
        #region ICamera Members

        public UnityEngine.Matrix4x4 View { get; set; }

        public UnityEngine.Matrix4x4 Projection { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 Target { get; set; }

        public Vector3 Up { get; set; }

        public Vector3 Translation { get; set; }

        public Vector2 ProjectionOrigin { get; set; }

        public float NearPlane { get; set; }

        public float FarPlane { get; set; }

        public float Zoom { get; set; }

        public float AspectRatioMultiplier { get; set; }

        public void ProjectionDirty()
        {
        }

        #endregion
    }
}
