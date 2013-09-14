/**
 * Copyright 2013 See AUTHORS file.
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

namespace MonoGdx.Geometry
{
    public static class Vector3Ext
    {
        public static Vector3 Rotate (this Vector3 vec, Vector3 axis, float angle)
        {
            return Vector3.Transform(vec, Quaternion.CreateFromAxisAngle(axis, angle));
        }

        public static Vector3 Project (this Vector3 vec, Matrix matrix)
        {
            float w = 1 / (vec.X * matrix.M14 + vec.Y * matrix.M24 + vec.Z * matrix.M34 + matrix.M44);
            float x = w * (vec.X * matrix.M11 + vec.Y * matrix.M21 + vec.Z * matrix.M31 + matrix.M41);
            float y = w * (vec.X * matrix.M12 + vec.Y * matrix.M22 + vec.Z * matrix.M32 + matrix.M42);
            float z = w * (vec.X * matrix.M13 + vec.Y * matrix.M23 + vec.Z * matrix.M33 + matrix.M43);

            return new Vector3(x, y, z);
        }
    }
}
