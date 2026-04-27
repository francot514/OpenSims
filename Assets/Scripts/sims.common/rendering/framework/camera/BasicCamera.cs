/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Numerics;

namespace FSO.Common.Rendering.Framework.Camera
{
    /// <summary>
    /// A basic camera for the game.
    /// </summary>
    [DisplayName("BasicCamera")]
    public class BasicCamera : ICamera
    {
        public float NearPlane { get; set; }
        public float FarPlane { get; set; }

        public float AspectRatioMultiplier { get; set; }

        protected Vector3 m_Position;
        protected Vector3 m_Target;
        protected Vector3 m_Up;
        protected UnityEngine.Canvas m_Device;

        protected UnityEngine.Camera camera;
        
        /// <summary>
        /// Creates a new BasicCamera instance. Assumes projection is full screen!
        /// </summary>
        /// <param name="device">A GraphicsDevice instance used for rendering.</param>
        /// <param name="Position">Camera's initial position.</param>
        /// <param name="Target">Camera's initial target.</param>
        /// <param name="Up">Camera's initial up vector.</param>
        public BasicCamera(UnityEngine.Canvas device, Vector3 Position, Vector3 Target, Vector3 Up)
        {
            m_Device = device;
            AspectRatioMultiplier = 1.0f;
            NearPlane = 1.0f;
            FarPlane = 800.0f;

            m_Position = Position;
            m_Target = Target;
            m_Up = Up;

            m_ViewDirty = true;

            /**
             * Assume the projection is full screen, center origin
             */

            
        }

        protected Vector2 m_ProjectionOrigin = Vector2.Zero;

        /// <summary>
        /// Gets or sets this BasicCamera's projection origin.
        /// </summary>
        public Vector2 ProjectionOrigin
        {
            get
            {
                return m_ProjectionOrigin;
            }
            set
            {
                m_ProjectionOrigin = value;
                m_ProjectionDirty = true;
            }
        }

        protected Matrix4x4 m_Projection;
        protected bool m_ProjectionDirty;

        public void ProjectionDirty()
        {
            m_ProjectionDirty = true;
        }

        /// <summary>
        /// Gets this camera's projection.
        /// </summary>
        [Browsable(false)]
        public Matrix4x4 Projection
        {
            get
            {
                if (m_ProjectionDirty)
                {
                    CalculateProjection();
                    m_ProjectionDirty = false;
                }
                return m_Projection;
            }
        }

        protected virtual void CalculateProjection()
        {
            var device = m_Device;
           camera.ResetAspect();

            var ratioX = m_ProjectionOrigin.X / camera.rect.width;
            var ratioY = m_ProjectionOrigin.Y / camera.rect.height;

            var projectionX = 0.0f - (1.0f * ratioX);
            var projectionY = (1.0f * ratioY);

            m_Projection = Matrix4x4.CreatePerspectiveOffCenter(
                projectionX, projectionX + 1.0f,
                ((projectionY-1.0f) / camera.aspect), (projectionY) / camera.aspect,
                NearPlane, FarPlane
            );

            m_Projection = Matrix4x4.CreateScale(Zoom, Zoom, 1.0f) * m_Projection;
        }

        protected virtual void CalculateView()
        {
            var translate = System.Numerics.Matrix4x4.CreateTranslation(m_Translation);
            var position = Vector3.Transform(m_Position, translate);
            var target = Vector3.Transform(m_Target, translate);
            

            m_View = System.Numerics.Matrix4x4.CreateLookAt(position, target,m_Up);
        }

        protected bool m_ViewDirty = false;
        protected System.Numerics.Matrix4x4 m_View = System.Numerics.Matrix4x4.Identity;
        [Browsable(false)]
        public System.Numerics.Matrix4x4 View
        {
            get
            {
                if (m_ViewDirty)
                {
                    m_ViewDirty = false;
                    CalculateView();
                }
                return m_View;
            }
        }

        
        protected float m_Zoom = 1.0f;

        /// <summary>
        /// Gets or sets this BasicCamera's zoom level.
        /// </summary>
        public float Zoom
        {
            get { return m_Zoom; }
            set
            {
                m_Zoom = value;
                m_ViewDirty = true;
                m_ProjectionDirty = true;
            }
        }

        protected Vector3 m_Translation;

        /// <summary>
        /// Gets or sets this BasicCamera's translation.
        /// </summary>
        public Vector3 Translation
        {
            get
            {
                return m_Translation;
            }
            set
            {
                m_Translation = value;
                m_ViewDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets this BasicCamera's position.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return m_Position;
            }
            set
            {
                m_Position = value;
                m_ViewDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets this BasicCamera's target.
        /// </summary>
        public Vector3 Target
        {
            get
            {
                return m_Target;
            }
            set
            {
                m_Target = value;
                m_ViewDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets this BasicCamera's up vector.
        /// </summary>
        public Vector3 Up
        {
            get
            {
                return m_Up;
            }
            set
            {
                m_Up = value;
                m_ViewDirty = true;
            }
        }

        UnityEngine.Matrix4x4 ICamera.View { get; }

        UnityEngine.Matrix4x4 ICamera.Projection { get; }

        public bool DrawCamera = false;

        public void Draw(UnityEngine.SpriteRenderer device)
        {
            /*
            device.RasterizerState.PointSize = 30.0f;
            device.VertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);

            var effect = new BasicEffect(device);

            effect.World = Matrix.Identity;
            effect.View = View;
            effect.Projection = Projection;
            effect.VertexColorEnabled = true;

            foreach (var pass in effect.Techniques[0].Passes)
            {
                pass.Apply();

                var vertex = new VertexPositionColor(Position, Color.Green);
                var vertexList = new VertexPositionColor[1] { vertex };
                device.DrawUserPrimitives(PrimitiveType.PointList, vertexList, 0, 1);

                vertex.Color = Color.Red;
                vertex.Position = Target;
                device.DrawUserPrimitives(PrimitiveType.PointList, vertexList, 0, 1);

            }
             * XNA4 no longer has support for point primitives.
             */
        }

    }
}
