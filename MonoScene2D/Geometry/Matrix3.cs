using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGdx.Geometry
{
    [DataContract]
    public struct Matrix3 : IEquatable<Matrix3>
    {
        public Matrix3 (float m11, float m12, float m13, float m21, float m22, float m23, float m31, float m32, float m33)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M31 = m31;
            M32 = m32;
            M33 = m33;
        }

        [DataMember]
        public float M11;

        [DataMember]
        public float M12;

        [DataMember]
        public float M13;

        [DataMember]
        public float M21;

        [DataMember]
        public float M22;

        [DataMember]
        public float M23;

        [DataMember]
        public float M31;

        [DataMember]
        public float M32;

        [DataMember]
        public float M33;

        public float this[int index]
        {
            get
            {
                switch (index) 
                {
                    case 0: return M11;
                    case 1: return M12;
                    case 2: return M13;
                    case 3: return M21;
                    case 4: return M22;
                    case 5: return M23;
                    case 6: return M31;
                    case 7: return M32;
                    case 8: return M33;
                }
                throw new ArgumentOutOfRangeException();
            }

            set
            {
                switch (index) {
                    case 0: M11 = value; break;
                    case 1: M12 = value; break;
                    case 2: M13 = value; break;
                    case 3: M21 = value; break;
                    case 4: M22 = value; break;
                    case 5: M23 = value; break;
                    case 6: M31 = value; break;
                    case 7: M32 = value; break;
                    case 8: M33 = value; break;
                }
                throw new ArgumentOutOfRangeException();
            }
        }

        public float this[int row, int column]
        {
            get { return this[(row * 3) + column]; }
            set { this[(row * 3) + column] = value; }
        }

        private static Matrix3 identity = new Matrix3(1, 0, 0, 0, 1, 0, 0, 0, 1);

        public Vector2 Down
        {
            get { return new Vector2(-M21, -M22); }
            set
            {
                M21 = -value.X;
                M22 = -value.Y;
            }
        }

        public static Matrix3 Identity
        {
            get { return identity; }
        }

        public static float[] ToFloatArray (Matrix3 mat)
        {
            return new float[] { mat.M11, mat.M12, mat.M13, mat.M21, mat.M22, mat.M23, mat.M31, mat.M32, mat.M33 };
        }

        public Vector2 Left
        {
            get { return new Vector2(-M11, -M12); }
            set
            {
                M11 = -value.X;
                M12 = -value.Y;
            }
        }

        public Vector2 Right
        {
            get { return new Vector2(M11, M12); }
            set
            {
                M11 = value.X;
                M12 = value.Y;
            }
        }

        public Vector2 Translation
        {
            get { return new Vector2(M31, M32); }
            set
            {
                M31 = value.X;
                M32 = value.Y;
            }
        }

        public Vector2 Up
        {
            get { return new Vector2(M21, M22); }
            set
            {
                M21 = value.X;
                M22 = value.Y;
            }
        }

        public static Matrix3 Add (Matrix3 matrix1, Matrix3 matrix2)
        {
            matrix1.M11 += matrix2.M11;
            matrix1.M12 += matrix2.M12;
            matrix1.M13 += matrix2.M13;
            matrix1.M21 += matrix2.M21;
            matrix1.M22 += matrix2.M22;
            matrix1.M23 += matrix2.M23;
            matrix1.M31 += matrix2.M31;
            matrix1.M32 += matrix2.M32;
            matrix1.M33 += matrix2.M33;
            return matrix1;
        }

        public static void Add (ref Matrix3 matrix1, ref Matrix3 matrix2, out Matrix3 result)
        {
            result.M11 = matrix1.M11 + matrix2.M11;
            result.M12 = matrix1.M12 + matrix2.M12;
            result.M13 = matrix1.M13 + matrix2.M13;
            result.M21 = matrix1.M21 + matrix2.M21;
            result.M22 = matrix1.M22 + matrix2.M22;
            result.M23 = matrix1.M23 + matrix2.M23;
            result.M31 = matrix1.M31 + matrix2.M31;
            result.M32 = matrix1.M32 + matrix2.M32;
            result.M33 = matrix1.M33 + matrix2.M33;
        }

        public static Matrix3 CreateRotation (float radians)
        {
            var val1 = (float)Math.Cos(radians);
            var val2 = (float)Math.Sin(radians);

            return new Matrix3(val1, val2, 0, -val2, val1, 0, 0, 0, 1);
        }

        public static void CreateRotation (float radians, out Matrix3 result)
        {
            var val1 = (float)Math.Cos(radians);
            var val2 = (float)Math.Sin(radians);

            result.M11 = val1;
            result.M12 = val2;
            result.M13 = 0;
            result.M21 = -val2;
            result.M22 = val1;
            result.M23 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
        }

        public static Matrix3 CreateScale (float scale)
        {
            return new Matrix3(scale, 0, 0, 0, scale, 0, 0, 0, 1);
        }

        public static void CreateScale (float scale, out Matrix3 result)
        {
            result.M11 = scale;
            result.M12 = 0;
            result.M13 = 0;
            result.M21 = 0;
            result.M22 = scale;
            result.M23 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
        }

        public static Matrix3 CreateScale (float xScale, float yScale)
        {
            return new Matrix3(xScale, 0, 0, 0, yScale, 0, 0, 0, 1);
        }

        public static void CreateScale (float xScale, float yScale, out Matrix3 result)
        {
            result.M11 = xScale;
            result.M12 = 0;
            result.M13 = 0;
            result.M21 = 0;
            result.M22 = yScale;
            result.M23 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
        }

        public static Matrix3 CreateScale (Vector2 scales)
        {
            return new Matrix3(scales.X, 0, 0, 0, scales.Y, 0, 0, 0, 1);
        }

        public static void CreateScale (Vector2 scales, out Matrix3 result)
        {
            result.M11 = scales.X;
            result.M12 = 0;
            result.M13 = 0;
            result.M21 = 0;
            result.M22 = scales.Y;
            result.M23 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
        }

        public static Matrix3 CreateTranslation (float xPosition, float yPosition)
        {
            return new Matrix3(1, 0, 0, 0, 1, 0, xPosition, yPosition, 1);
        }

        public static void CreateTranslation (float xPosition, float yPosition, out Matrix3 result)
        {
            result.M11 = 1;
            result.M12 = 0;
            result.M13 = 0;
            result.M21 = 0;
            result.M22 = 1;
            result.M23 = 0;
            result.M31 = xPosition;
            result.M32 = yPosition;
            result.M33 = 1;
        }

        public static Matrix3 CreateTranslation (Vector2 position)
        {
            return new Matrix3(1, 0, 0, 0, 1, 0, position.X, position.Y, 1);
        }

        public static void CreateTranslation (Vector2 position, out Matrix3 result)
        {
            result.M11 = 1;
            result.M12 = 0;
            result.M13 = 0;
            result.M21 = 0;
            result.M22 = 1;
            result.M23 = 0;
            result.M31 = position.X;
            result.M32 = position.Y;
            result.M33 = 1;
        }

        // CreateReflection (make Line type to mirror Plane)

        public float Determinant ()
        {
            return M11 * M22 * M33 - M11 * M23 * M32 - M12 * M21 * M33 + M12 * M23 * M31 + M13 * M21 * M32 - M13 * M22 * M31;
        }

        public static Matrix3 Divide (Matrix3 matrix1, Matrix3 matrix2)
        {
            matrix1.M11 = matrix1.M11 / matrix2.M11;
            matrix1.M12 = matrix1.M12 / matrix2.M12;
            matrix1.M13 = matrix1.M13 / matrix2.M13;
            matrix1.M21 = matrix1.M21 / matrix2.M21;
            matrix1.M22 = matrix1.M22 / matrix2.M22;
            matrix1.M23 = matrix1.M23 / matrix2.M23;
            matrix1.M31 = matrix1.M31 / matrix2.M31;
            matrix1.M32 = matrix1.M32 / matrix2.M32;
            matrix1.M33 = matrix1.M33 / matrix2.M33;
            return matrix1;
        }

        public static void Divide (ref Matrix3 matrix1, ref Matrix3 matrix2, out Matrix3 result)
        {
            result.M11 = matrix1.M11 / matrix2.M11;
            result.M12 = matrix1.M12 / matrix2.M12;
            result.M13 = matrix1.M13 / matrix2.M13;
            result.M21 = matrix1.M21 / matrix2.M21;
            result.M22 = matrix1.M22 / matrix2.M22;
            result.M23 = matrix1.M23 / matrix2.M23;
            result.M31 = matrix1.M31 / matrix2.M31;
            result.M32 = matrix1.M32 / matrix2.M32;
            result.M33 = matrix1.M33 / matrix2.M33;
        }

        public static Matrix3 Divide (Matrix3 matrix, float divider)
        {
            float num = 1f / divider;
            matrix.M11 = matrix.M11 * num;
            matrix.M12 = matrix.M12 * num;
            matrix.M13 = matrix.M13 * num;
            matrix.M21 = matrix.M21 * num;
            matrix.M22 = matrix.M22 * num;
            matrix.M23 = matrix.M23 * num;
            matrix.M31 = matrix.M31 * num;
            matrix.M32 = matrix.M32 * num;
            matrix.M33 = matrix.M33 * num;
            return matrix;
        }

        public static void Divide (ref Matrix3 matrix, float divider, out Matrix3 result)
        {
            float num = 1f / divider;
            result.M11 = matrix.M11 * num;
            result.M12 = matrix.M12 * num;
            result.M13 = matrix.M13 * num;
            result.M21 = matrix.M21 * num;
            result.M22 = matrix.M22 * num;
            result.M23 = matrix.M23 * num;
            result.M31 = matrix.M31 * num;
            result.M32 = matrix.M32 * num;
            result.M33 = matrix.M33 * num;
        }

        public bool Equals (Matrix3 other)
        {
            return M11 == other.M11
                && M12 == other.M12
                && M13 == other.M13
                && M21 == other.M21
                && M22 == other.M22
                && M23 == other.M32
                && M31 == other.M31
                && M32 == other.M32
                && M33 == other.M33;
        }

        public static bool Equals (ref Matrix3 matrix1, ref Matrix3 matrix2)
        {
            return matrix1.M11 == matrix2.M11
                && matrix1.M12 == matrix2.M12
                && matrix1.M13 == matrix2.M13
                && matrix1.M21 == matrix2.M21
                && matrix1.M22 == matrix2.M22
                && matrix1.M23 == matrix2.M32
                && matrix1.M31 == matrix2.M31
                && matrix1.M32 == matrix2.M32
                && matrix1.M33 == matrix2.M33;
        }

        public override bool Equals (object obj)
        {
            if (obj is Matrix3)
                return Equals((Matrix3)obj);
            return false;
        }

        public override int GetHashCode ()
        {
            return M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode()
                + M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode()
                + M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode();
        }

        public static Matrix3 Invert (Matrix3 matrix)
        {
            Invert(ref matrix, out matrix);
            return matrix;
        }

        public static void Invert (ref Matrix3 matrix, out Matrix3 result)
        {
            float det = matrix.Determinant();
            if (det == 0)
                throw new InvalidOperationException("Can't invert a singular matrix");

            float invDet = 1f / det;

            float m11 = matrix.M22 * matrix.M33 - matrix.M32 * matrix.M23;
            float m12 = matrix.M21 * matrix.M33 - matrix.M23 * matrix.M31;
            float m13 = matrix.M21 * matrix.M32 - matrix.M31 * matrix.M22;
            float m21 = matrix.M12 * matrix.M33 - matrix.M13 * matrix.M32;
            float m22 = matrix.M11 * matrix.M33 - matrix.M13 * matrix.M31;
            float m23 = matrix.M11 * matrix.M32 - matrix.M31 * matrix.M12;
            float m31 = matrix.M12 * matrix.M23 - matrix.M13 * matrix.M22;
            float m32 = matrix.M11 * matrix.M23 - matrix.M21 * matrix.M13;
            float m33 = matrix.M11 * matrix.M22 - matrix.M21 * matrix.M12;

            result.M11 = m11 * invDet;
            result.M12 = m12 * invDet;
            result.M13 = m13 * invDet;
            result.M21 = m21 * invDet;
            result.M22 = m22 * invDet;
            result.M23 = m23 * invDet;
            result.M31 = m31 * invDet;
            result.M32 = m32 * invDet;
            result.M33 = m33 * invDet;
        }

        public static Matrix3 Lerp (Matrix3 matrix1, Matrix3 matrix2, float amount)
        {
            matrix1.M11 = matrix1.M11 + ((matrix2.M11 - matrix1.M11) * amount);
            matrix1.M12 = matrix1.M12 + ((matrix2.M12 - matrix1.M12) * amount);
            matrix1.M13 = matrix1.M13 + ((matrix2.M13 - matrix1.M13) * amount);
            matrix1.M21 = matrix1.M21 + ((matrix2.M21 - matrix1.M21) * amount);
            matrix1.M22 = matrix1.M22 + ((matrix2.M22 - matrix1.M22) * amount);
            matrix1.M23 = matrix1.M23 + ((matrix2.M23 - matrix1.M23) * amount);
            matrix1.M31 = matrix1.M31 + ((matrix2.M31 - matrix1.M31) * amount);
            matrix1.M32 = matrix1.M32 + ((matrix2.M32 - matrix1.M32) * amount);
            matrix1.M33 = matrix1.M33 + ((matrix2.M33 - matrix1.M33) * amount);
            return matrix1;
        }

        public static void Lerp (ref Matrix3 matrix1, ref Matrix3 matrix2, float amount, out Matrix3 result)
        {
            result.M11 = matrix1.M11 + ((matrix2.M11 - matrix1.M11) * amount);
            result.M12 = matrix1.M12 + ((matrix2.M12 - matrix1.M12) * amount);
            result.M13 = matrix1.M13 + ((matrix2.M13 - matrix1.M13) * amount);
            result.M21 = matrix1.M21 + ((matrix2.M21 - matrix1.M21) * amount);
            result.M22 = matrix1.M22 + ((matrix2.M22 - matrix1.M22) * amount);
            result.M23 = matrix1.M23 + ((matrix2.M23 - matrix1.M23) * amount);
            result.M31 = matrix1.M31 + ((matrix2.M31 - matrix1.M31) * amount);
            result.M32 = matrix1.M32 + ((matrix2.M32 - matrix1.M32) * amount);
            result.M33 = matrix1.M33 + ((matrix2.M33 - matrix1.M33) * amount);
        }

        public static Matrix3 Multiply (Matrix3 matrix1, Matrix3 matrix2)
        {
            float m11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31;
            float m12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32;
            float m13 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33;
            float m21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31;
            float m22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32;
            float m23 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33;
            float m31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31;
            float m32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32;
            float m33 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33;

            return new Matrix3(m11, m12, m13, m21, m22, m23, m31, m32, m33);
        }

        [TODO("can't write directly")]
        public static void Multiply (ref Matrix3 matrix1, ref Matrix3 matrix2, out Matrix3 result)
        {
            float m11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31;
            float m12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32;
            float m13 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33;
            float m21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31;
            float m22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32;
            float m23 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33;
            float m31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31;
            float m32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32;
            float m33 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33;

            result.M11 = m11;
            result.M12 = m12;
            result.M13 = m13;
            result.M21 = m21;
            result.M22 = m22;
            result.M23 = m23;
            result.M31 = m31;
            result.M32 = m32;
            result.M33 = m33;
        }

        public static Matrix3 Multiply (Matrix3 matrix, float factor)
        {
            matrix.M11 *= factor;
            matrix.M12 *= factor;
            matrix.M13 *= factor;
            matrix.M21 *= factor;
            matrix.M22 *= factor;
            matrix.M23 *= factor;
            matrix.M31 *= factor;
            matrix.M32 *= factor;
            matrix.M33 *= factor;
            return matrix;
        }

        public static void Multiply (ref Matrix3 matrix, float factor, out Matrix3 result)
        {
            result.M11 = matrix.M11 * factor;
            result.M12 = matrix.M12 * factor;
            result.M13 = matrix.M13 * factor;
            result.M21 = matrix.M21 * factor;
            result.M22 = matrix.M22 * factor;
            result.M23 = matrix.M23 * factor;
            result.M31 = matrix.M31 * factor;
            result.M32 = matrix.M32 * factor;
            result.M33 = matrix.M33 * factor;
        }

        public static Matrix3 Negate (Matrix3 matrix)
        {
            matrix.M11 = -matrix.M11;
            matrix.M12 = -matrix.M12;
            matrix.M13 = -matrix.M13;
            matrix.M21 = -matrix.M21;
            matrix.M22 = -matrix.M22;
            matrix.M23 = -matrix.M23;
            matrix.M31 = -matrix.M31;
            matrix.M32 = -matrix.M32;
            matrix.M33 = -matrix.M33;
            return matrix;
        }

        public static void Negate (ref Matrix3 matrix, out Matrix3 result)
        {
            result.M11 = -matrix.M11;
            result.M12 = -matrix.M12;
            result.M13 = -matrix.M13;
            result.M21 = -matrix.M21;
            result.M22 = -matrix.M22;
            result.M23 = -matrix.M23;
            result.M31 = -matrix.M31;
            result.M32 = -matrix.M32;
            result.M33 = -matrix.M33;
        }

        public static Matrix3 operator + (Matrix3 matrix1, Matrix3 matrix2)
        {
            Matrix3.Add(ref matrix1, ref matrix2, out matrix1);
            return matrix1;
        }

        public static Matrix3 operator / (Matrix3 matrix1, Matrix3 matrix2)
        {
            Matrix3.Divide(ref matrix1, ref matrix2, out matrix1);
            return matrix1;
        }

        public static Matrix3 operator / (Matrix3 matrix, float divider)
        {
            Matrix3.Divide(ref matrix, divider, out matrix);
            return matrix;
        }

        public static bool operator == (Matrix3 matrix1, Matrix3 matrix2)
        {
            return matrix1.M11 == matrix2.M11
                && matrix1.M12 == matrix2.M12
                && matrix1.M13 == matrix2.M13
                && matrix1.M21 == matrix2.M21
                && matrix1.M22 == matrix2.M22
                && matrix1.M23 == matrix2.M32
                && matrix1.M31 == matrix2.M31
                && matrix1.M32 == matrix2.M32
                && matrix1.M33 == matrix2.M33;
        }

        public static bool operator != (Matrix3 matrix1, Matrix3 matrix2)
        {
            return matrix1.M11 != matrix2.M11
                || matrix1.M12 != matrix2.M12
                || matrix1.M13 != matrix2.M13
                || matrix1.M21 != matrix2.M21
                || matrix1.M22 != matrix2.M22
                || matrix1.M23 != matrix2.M32
                || matrix1.M31 != matrix2.M31
                || matrix1.M32 != matrix2.M32
                || matrix1.M33 != matrix2.M33;
        }

        public static Matrix3 operator * (Matrix3 matrix1, Matrix3 matrix2)
        {
            Matrix3.Multiply(ref matrix1, ref matrix2, out matrix1);
            return matrix1;
        }

        public static Matrix3 operator * (Matrix3 matrix, float scaleFactor)
        {
            Matrix3.Multiply(ref matrix, scaleFactor, out matrix);
            return matrix;
        }

        public static Matrix3 operator - (Matrix3 matrix1, Matrix3 matrix2)
        {
            Matrix3.Subtract(ref matrix1, ref matrix2, out matrix1);
            return matrix1;
        }

        public static Matrix3 operator - (Matrix3 matrix)
        {
            Matrix3.Negate(ref matrix, out matrix);
            return matrix;
        }

        public static Matrix3 Subtract (Matrix3 matrix1, Matrix3 matrix2)
        {
            matrix1.M11 = matrix1.M11 - matrix2.M11;
            matrix1.M12 = matrix1.M12 - matrix2.M12;
            matrix1.M13 = matrix1.M13 - matrix2.M13;
            matrix1.M21 = matrix1.M21 - matrix2.M21;
            matrix1.M22 = matrix1.M22 - matrix2.M22;
            matrix1.M23 = matrix1.M23 - matrix2.M23;
            matrix1.M31 = matrix1.M31 - matrix2.M31;
            matrix1.M32 = matrix1.M32 - matrix2.M32;
            matrix1.M33 = matrix1.M33 - matrix2.M33;
            return matrix1;
        }

        public static void Subtract (ref Matrix3 matrix1, ref Matrix3 matrix2, out Matrix3 result)
        {
            result.M11 = matrix1.M11 - matrix2.M11;
            result.M12 = matrix1.M12 - matrix2.M12;
            result.M13 = matrix1.M13 - matrix2.M13;
            result.M21 = matrix1.M21 - matrix2.M21;
            result.M22 = matrix1.M22 - matrix2.M22;
            result.M23 = matrix1.M23 - matrix2.M23;
            result.M31 = matrix1.M31 - matrix2.M31;
            result.M32 = matrix1.M32 - matrix2.M32;
            result.M33 = matrix1.M33 - matrix2.M33;
        }

        public Matrix ToMatrix ()
        {
            return new Matrix(M11, M12, M13, 0, M21, M22, M23, 0, 0, 0, 1, 0, M31, M32, 0, M33);
        }

        public void ToMatrix (out Matrix result)
        {
            result.M11 = M11;
            result.M12 = M12;
            result.M13 = M13;
            result.M14 = 0;
            result.M21 = M21;
            result.M22 = M22;
            result.M23 = M23;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
            result.M34 = 0;
            result.M41 = M31;
            result.M42 = M32;
            result.M43 = 0;
            result.M44 = M33;
        }

        public override string ToString ()
        {
            return "{" + String.Format("M11:{0} M12:{1} M13:{2}", M11, M12, M13) + "}"
                + " {" + String.Format("M21:{0} M22:{1} M23:{2}", M21, M22, M23) + "}"
                + " {" + String.Format("M31:{0} M32:{1} M33:{2}", M31, M32, M33) + "}";
        }

        public void Translate (float x, float y)
        {
            M31 += x;
            M32 += y;
        }

        public void Translate (Vector2 amount)
        {
            M31 += amount.X;
            M32 += amount.Y;
        }

        public static Matrix3 Transpose (Matrix3 matrix)
        {
            return new Matrix3(matrix.M11, matrix.M21, matrix.M31, matrix.M12, matrix.M22, matrix.M32, matrix.M13, matrix.M23, matrix.M33);
        }

        public static void Transpose (ref Matrix3 matrix, out Matrix3 result)
        {
            result = Transpose(matrix);
        }
    }
}
