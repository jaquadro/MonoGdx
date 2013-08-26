using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoScene2D.Graphics;

namespace MonoScene2D.Scene2D.Utils
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
                _device.RasterizerState.ScissorTestEnable = true;
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
            _device.ScissorRectangle = scissor;

            return true;
        }

        public Rectangle PopScissors ()
        {
            Rectangle old = _scissors.Pop();
            if (_scissors.Count == 0)
                _device.RasterizerState.ScissorTestEnable = false;
            else
                _device.ScissorRectangle = _scissors.Peek();

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
