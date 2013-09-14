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
using MonoGdx.Graphics;

namespace MonoGdx.Scene2D.Utils
{
    public class ScissorStack
    {
        private GraphicsDevice _device;
        private Stack<Rectangle> _scissors = new Stack<Rectangle>();

        public ScissorStack (GraphicsDevice graphicsDevice)
        {
            _device = graphicsDevice;
        }

        public bool PushScissors (Rectangle scissor)
        {
            scissor = Fix(scissor);

            if (_scissors.Count == 0) {
                if (scissor.Width < 1 || scissor.Height < 1)
                    return false;
            }
            else {
                Rectangle parent = _scissors.Peek();
                int minX = Math.Max(parent.Left, scissor.Left);
                int maxX = Math.Min(parent.Right, scissor.Right);
                if (maxX - minX < 1)
                    return false;

                int minY = Math.Max(parent.Top, scissor.Top);
                int maxY = Math.Min(parent.Bottom, scissor.Bottom);
                if (maxY - minY < 1)
                    return false;

                scissor = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            }

            _scissors.Push(scissor);
            _device.ScissorRectangle = new Rectangle(scissor.X, _device.Viewport.Height - scissor.Height - scissor.Y, scissor.Width, scissor.Height);

            return true;
        }

        public Rectangle PopScissors ()
        {
            Rectangle old = _scissors.Pop();
            if (_scissors.Count == 0)
                _device.ScissorRectangle = Rectangle.Empty;
            else {
                Rectangle scissor = _scissors.Peek();
                _device.ScissorRectangle = new Rectangle(scissor.X, _device.Viewport.Height - scissor.Height - scissor.Y, scissor.Width, scissor.Height);
            }

            return old;
        }

        public static Rectangle Fix (Rectangle rect)
        {
            if (rect.Width < 0) {
                rect.Width = -rect.Width;
                rect.X -= rect.Width;
            }
            if (rect.Height < 0) {
                rect.Height = -rect.Height;
                rect.Y -= rect.Height;
            }

            return rect;
        }

        public static Rectangle CalculateScissors (Camera camera, Matrix batchTransform, Rectangle area)
        {
            Vector3 pos = Vector3.Transform(new Vector3(area.X, area.Y, 0), batchTransform);
            pos = camera.Project(pos);

            Vector3 size = Vector3.Transform(new Vector3(area.X + area.Width, area.Y + area.Height, 0), batchTransform);
            size = camera.Project(size);

            return new Rectangle((int)pos.X, (int)pos.Y, (int)size.X - (int)pos.X, (int)size.Y - (int)pos.Y);
        }

        public Rectangle Viewport
        {
            get
            {
                if (_scissors.Count == 0)
                    return new Rectangle(0, 0, _device.Viewport.Width, _device.Viewport.Height);
                else
                    return _scissors.Peek();
            }
        }

        public static Vector2 ToWindowCoordinates (Camera camera, Matrix transform, Vector2 point)
        {
            Vector3 vec = Vector3.Transform(new Vector3(point, 0), transform);
            vec = camera.Project(vec);
            vec.Y = camera.GraphicsDevice.Viewport.Height - vec.Y;

            return new Vector2(vec.X, vec.Y);
        }
    }
}
