using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoScene2D.Geometry
{
    public static class Vector2Ext
    {
        public static float Angle (this Vector2 vec)
        {
            float angle = (float)Math.Atan2(vec.Y, vec.X);
            if (angle < 0)
                angle += (float)(Math.PI * 2);

            return angle;
        }
    }
}
