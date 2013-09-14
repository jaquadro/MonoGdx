/**
 * Copyright 2011-2013 See AUTHORS file.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGdx.Geometry;

namespace MonoGdx.Graphics
{
    public class OrthographicCamera : Camera
    {
        public float Zoom { get; set; }

        public OrthographicCamera (GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            Zoom = 1;
            Near = 0;
        }

        public OrthographicCamera (GraphicsDevice graphicsDevice, float viewportWidth, float viewportHeight)
            : this(graphicsDevice)
        {
            ViewportWidth = viewportWidth;
            ViewportHeight = viewportHeight;
            Update();
        }

        public OrthographicCamera (GraphicsDevice graphicsDevice, float viewportWidth, float viewportHeight, float diamondAngle)
            : this(graphicsDevice)
        {
            ViewportWidth = viewportWidth;
            ViewportHeight = viewportHeight;
            FindDirectionForIsoView(diamondAngle, .00000001f, 20);
            Update();
        }

        public void FindDirectionForIsoView (float targetAngle, float epsilon, int maxIterations)
        {
            float start = targetAngle - 5;
            float end = targetAngle + 5;
            float mid = targetAngle;

            int iterations = 0;
            float aMid = 0;

            while (Math.Abs(targetAngle - aMid) > epsilon && iterations++ < maxIterations) {
                aMid = CalculateAngle(mid);

                if (targetAngle < aMid)
                    end = mid;
                else
                    start = mid;

                mid = start + (end - start) / 2;
            }

            Position = CalculateDirection(mid);
            Position = new Vector3(Position.X, -Position.Y, Position.Z);

            LookAt(0, 0, 0);
            NormalizeUp();
        }

        private float CalculateAngle (float a)
        {
            Vector3 camPos = CalculateDirection(a);
            Position = Position * 30;
            LookAt(0, 0, 0);
            NormalizeUp();
            Update();

            Vector3 orig = Project(Vector3.Zero);
            Vector3 vec = Project(Vector3.UnitX);

            return new Vector2(vec.X - orig.X, -(vec.Y - orig.Y)).Angle();
        }

        private Vector3 CalculateDirection (float angle)
        {
            Matrix transform = Matrix.CreateFromAxisAngle(Vector3.Normalize(new Vector3(1, 0, 1)), angle);
            Vector3 dir = Vector3.Normalize(new Vector3(-1, 0, 1));
            return Vector3.Normalize(Vector3.Transform(dir, transform));
        }

        public override void Update ()
        {
            Update(true);
        }

        public override void Update (bool updateFrustum)
        {
            //Projection = Matrix.CreateOrthographic(Zoom * ViewportWidth, Zoom * ViewportHeight, Math.Abs(Near), Math.Abs(Far));
            Projection = XnaExt.Matrix.CreateOrthographic(Zoom * -ViewportWidth / 2, Zoom * ViewportWidth / 2, 
                Zoom * -ViewportHeight / 2, Zoom * ViewportHeight / 2, Math.Abs(Near), Math.Abs(Far));
            View = Matrix.CreateLookAt(Position, Position + Direction, Up);
            Combined = View * Projection;

            if (updateFrustum) {
                InverseProjectionView = Matrix.Invert(Combined);
                Frustum.Matrix = InverseProjectionView;
            }
        }

        public void ToOrtho (bool yDown)
        {
            ToOrtho(yDown, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        public void ToOrtho (bool yDown, float viewportWidth, float viewportHeight)
        {
            if (yDown) {
                Up = Vector3.Down;
                Direction = Vector3.UnitZ;
            }

            Position = new Vector3(Zoom * viewportWidth / 2, Zoom * viewportHeight / 2, 0);
            ViewportWidth = viewportWidth;
            ViewportHeight = viewportHeight;

            Update();
        }

        public void Rotate (float angle)
        {
            Rotate(Direction, angle);
        }

        public void Translate (float x, float y)
        {
            Translate(x, y, 0);
        }

        public void Translate (Vector2 vec)
        {
            Translate(vec.X, vec.Y, 0);
        }
    }
}
