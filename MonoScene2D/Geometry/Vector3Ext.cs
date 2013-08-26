using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoScene2D.Geometry
{
    public static class Vector3Ext
    {
        public static Vector3 Rotate (this Vector3 vec, Vector3 axis, float angle)
        {
            return Vector3.Transform(vec, Quaternion.CreateFromAxisAngle(axis, angle));
        }

        public static Vector3 Project (this Vector3 vec, Matrix matrix)
        {
            float w = 1 / (vec.X * matrix.M41 + vec.Y * matrix.M42 + vec.Z * matrix.M43 + matrix.M44);
            float x = w * (vec.X * matrix.M11 + vec.Y * matrix.M12 + vec.Z * matrix.M13 + matrix.M14);
            float y = w * (vec.X * matrix.M21 + vec.Y * matrix.M22 + vec.Z * matrix.M23 + matrix.M24);
            float z = w * (vec.X * matrix.M31 + vec.Y * matrix.M32 + vec.Z * matrix.M33 + matrix.M34);

            return new Vector3(x, y, z);
        }
    }
}
