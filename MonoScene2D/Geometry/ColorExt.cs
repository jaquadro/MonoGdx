using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoGdx.Geometry
{
    public static class ColorExt
    {
        public static Color Multiply (Color color1, Color color2)
        {
            Vector4 v1 = color1.ToVector4();
            Vector4 v2 = color2.ToVector4();

            return new Color(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z, v1.W * v2.W);
        }
    }
}
