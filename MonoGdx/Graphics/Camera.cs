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
    public abstract class Camera
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public Vector3 Up { get; set; }

        public Matrix Projection { get; set; }
        public Matrix View { get; set; }
        public Matrix Combined { get; set; }
        public Matrix InverseProjectionView { get; set; }

        public float Near { get; set; }
        public float Far { get; set; }

        public float ViewportWidth { get; set; }
        public float ViewportHeight { get; set; }

        public BoundingFrustum Frustum { get; set; }

        public Camera (GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;

            Position = Vector3.Zero;
            Direction = Vector3.Forward;
            Up = Vector3.Up;

            Projection = Matrix.Identity;
            View = Matrix.Identity;
            Combined = Matrix.Identity;
            InverseProjectionView = Matrix.Identity;

            Near = 1;
            Far = 100;

            Frustum = new BoundingFrustum(Matrix.Identity);
        }

        public abstract void Update ();

        public abstract void Update (bool updateFrustum);

        [TODO]
        public void Apply ()
        {
            throw new NotImplementedException();
        }

        public void LookAt (float x, float y, float z)
        {
            Direction = Vector3.Normalize(new Vector3(x, y, z) - Position);
            NormalizeUp();
        }

        public void LookAt (Vector3 target)
        {
            Direction = Vector3.Normalize(target - Position);
            NormalizeUp();
        }

        public void NormalizeUp ()
        {
            Vector3 vec = Vector3.Cross(Direction, Up);
            vec.Normalize();

            Up = Vector3.Cross(vec, Direction);
            Up.Normalize();
        }

        public void Rotate (float angle, float axisX, float axisY, float axisZ)
        {
            Rotate(new Vector3(axisX, axisY, axisZ), angle);
        }

        public void Rotate (Vector3 axis, float angle)
        {
            Direction = Direction.Rotate(axis, angle);
            Up = Up.Rotate(axis, angle);
        }

        public void Rotate (Matrix transform)
        {
            Direction = Vector3.Transform(Direction, transform);
            Up = Vector3.Transform(Up, transform);
        }

        public void Rotate (Quaternion quat)
        {
            Direction = Vector3.Transform(Direction, quat);
            Up = Vector3.Transform(Up, quat);
        }

        public void RotateAround (Vector3 point, Vector3 axis, float angle)
        {
            Vector3 tmp = point - Position;
            Translate(tmp);
            Rotate(axis, angle);

            tmp.Rotate(axis, angle);
            Translate(-tmp.X, -tmp.Y, -tmp.Z);
        }

        public void Transform (Matrix transform)
        {
            Position = Vector3.Transform(Position, transform);
            Rotate(transform);
        }

        public void Translate (float x, float y, float z)
        {
            Position += new Vector3(x, y, z);
        }

        public void Translate (Vector3 vec)
        {
            Position += vec;
        }

        public Vector3 Unproject (Vector3 vec, float viewportX, float viewportY, float viewportWidth, float viewportHeight)
        {
            float x = vec.X - viewportX;
            float y = GraphicsDevice.Viewport.Height - vec.Y - 1 - viewportY;

            vec = new Vector3((2 * x) / viewportWidth - 1, (2 * y) / viewportHeight - 1, 2 * vec.Z - 1);
            return vec.Project(InverseProjectionView);
        }

        public Vector3 Unproject (Vector3 vec)
        {
            return Unproject(vec, 0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        public Vector3 Project (Vector3 vec, float viewportX, float viewportY, float viewportWidth, float viewportHeight)
        {
            vec = vec.Project(Combined);

            float x = viewportWidth * (vec.X + 1) / 2 + viewportX;
            float y = viewportHeight * (vec.Y + 1) / 2 + viewportY;
            float z = (vec.Z + 1) / 2;
            return new Vector3(x, y, z);
        }

        public Vector3 Project (Vector3 vec)
        {
            return Project(vec, 0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        public Ray PickRay (float x, float y, float viewportX, float viewportY, float viewportWidth, float viewportHeight)
        {
            return PickRay(new Vector2(x, y), viewportX, viewportY, viewportWidth, viewportHeight);
        }

        public Ray PickRay (Vector2 vec, float viewportX, float viewportY, float viewportWidth, float viewportHeight)
        {
            Vector3 origin = Unproject(new Vector3(vec, 0), viewportX, viewportY, viewportWidth, viewportHeight);
            Vector3 direction = Unproject(new Vector3(vec, 1), viewportX, viewportY, viewportWidth, viewportHeight);

            return new Ray(origin, Vector3.Normalize(direction - origin));
        }

        public Ray PickRay (float x, float y)
        {
            return PickRay(new Vector2(x, y), 0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        public Ray PickRay (Vector2 vec)
        {
            return PickRay(vec, 0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }
    }
}
