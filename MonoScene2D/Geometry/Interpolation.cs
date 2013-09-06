using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGdx.Geometry
{
    public abstract class Interpolation
    {
        public abstract float Apply (float a);

        public float Apply (float start, float end, float a)
        {
            return start + (end - start) * Apply(a);
        }

        public static readonly Interpolation Linear = new DelegateInterpolation(a => a);

        public static readonly Interpolation Fade = new DelegateInterpolation(a =>
            MathHelper.Clamp(a * a * a * (a * (a * 6 - 15) + 10), 0, 1));

        public static readonly Interpolation Pow2 = new DelegateInterpolation(a => _pow(2, a));
        public static readonly Interpolation Pow2In = new DelegateInterpolation(a => _powIn(2, a));
        public static readonly Interpolation Pow2Out = new DelegateInterpolation(a => _powOut(2, a));

        public static readonly Interpolation Pow3 = new DelegateInterpolation(a => _pow(3, a));
        public static readonly Interpolation Pow3In = new DelegateInterpolation(a => _powIn(3, a));
        public static readonly Interpolation Pow3Out = new DelegateInterpolation(a => _powOut(3, a));

        public static readonly Interpolation Pow4 = new DelegateInterpolation(a => _pow(4, a));
        public static readonly Interpolation Pow4In = new DelegateInterpolation(a => _powIn(4, a));
        public static readonly Interpolation Pow4Out = new DelegateInterpolation(a => _powOut(4, a));

        public static readonly Interpolation Pow5 = new DelegateInterpolation(a => _pow(5, a));
        public static readonly Interpolation Pow5In = new DelegateInterpolation(a => _powIn(5, a));
        public static readonly Interpolation Pow5Out = new DelegateInterpolation(a => _powOut(5, a));

        public static readonly Interpolation Sine = new DelegateInterpolation(a =>
            (float)(1 - Math.Cos(a * Math.PI)) / 2);

        public static readonly Interpolation SineIn = new DelegateInterpolation(a =>
            (float)(1 - Math.Cos(a * Math.PI / 2)));

        public static readonly Interpolation SineOut = new DelegateInterpolation(a =>
            (float)(Math.Sin(a * Math.PI / 2)));

        private static Func<int, float, float> _pow = (power, a) => {
            if (a <= .5f)
                return (float)Math.Pow(a * 2, power) / 2;
            return (float)Math.Pow((a - 1) * 2, power) / (power % 2 == 0 ? -2 : 2) + 1;
        };

        private static Func<int, float, float> _powIn = (power, a) => {
            return (float)Math.Pow(a, power);
        };

        private static Func<int, float, float> _powOut = (power, a) => {
            return (float)Math.Pow(a - 1, power) * (power % 2 == 0 ? -1 : 1) + 1;
        };

        private class DelegateInterpolation : Interpolation
        {
            private Func<float, float> _apply;
            public DelegateInterpolation (Func<float, float> apply)
            {
                _apply = apply;
            }

            public override float Apply (float a)
            {
                return _apply(a);
            }
        }
    }
}
