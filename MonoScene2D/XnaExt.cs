using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XFramework = Microsoft.Xna.Framework;

namespace MonoGdx
{
    public static class XnaExt
    {
        public static bool IsMapMap (this TextureFilter filter)
        {
            switch (filter) {
                case TextureFilter.LinearMipPoint:
                case TextureFilter.MinLinearMagPointMipLinear:
                case TextureFilter.MinLinearMagPointMipPoint:
                case TextureFilter.MinPointMagLinearMipLinear:
                case TextureFilter.MinPointMagLinearMipPoint:
                case TextureFilter.PointMipLinear:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsButtonPressed (this MouseState state, int button)
        {
            switch (button) {
                case 1: return state.LeftButton == ButtonState.Pressed;
                case 2: return state.MiddleButton == ButtonState.Pressed;
                case 3: return state.RightButton == ButtonState.Pressed;
                case 4: return state.XButton1 == ButtonState.Pressed;
                case 5: return state.XButton2 == ButtonState.Pressed;
                default: return false;
            }
        }

        public static Color MultiplyAlpha (this Color color, float alphaFactor)
        {
            Vector4 v = color.ToVector4();
            return new Color(v.X, v.Y, v.Z, v.W * alphaFactor);
        }

        public static Color Multiply (this Color color1, Color color2)
        {
            Vector4 v1 = color1.ToVector4();
            Vector4 v2 = color2.ToVector4();

            return new Color(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z, v1.W * v2.W);
        }

        public static class Matrix
        {
            public static XFramework.Matrix CreateOrthographic2D (float x, float y, float width, float height)
            {
                return CreateOrthographic(x, x + width, y, y + height, 0, 1);
            }

            public static XFramework.Matrix CreateOrthographic2D (float x, float y, float width, float height, float near, float far)
            {
                return CreateOrthographic(x, x + width, y, y + height, near, far);
            }

            public static XFramework.Matrix CreateOrthographic (float left, float right, float bottom, float top, float near, float far)
            {
                float xOrth = 2 / (right - left);
                float yOrth = 2 / (top - bottom);
                float zOrth = -2 / (far - near);

                float tx = -(right + left) / (right - left);
                float ty = -(top + bottom) / (top - bottom);
                float tz = -(far + near) / (far - near);

                return new XFramework.Matrix(
                    xOrth, 0, 0, 0,
                    0, yOrth, 0, 0,
                    0, 0, zOrth, 0,
                    tx, ty, tz, 1
                );
            }
        }
    }
}
