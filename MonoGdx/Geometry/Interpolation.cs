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

        public static readonly Interpolation Exp10 = new ExpInterpolation(2, 10);
        public static readonly Interpolation Exp10In = new ExpInInterpolation(2, 10);
        public static readonly Interpolation Exp10Out = new ExpOutInterpolation(2, 10);

        public static readonly Interpolation Exp5 = new ExpInterpolation(2, 5);
        public static readonly Interpolation Exp5In = new ExpInInterpolation(2, 5);
        public static readonly Interpolation Exp5Out = new ExpOutInterpolation(2, 5);

        public static readonly Interpolation Circle = new DelegateInterpolation(a => {
            if (a <= .5f)
                return (1 - (float)Math.Sqrt(1 - a * a * 4)) / 2;
            a = (a - 1) * 2;
            return ((float)Math.Sqrt(1 - a * a) + 1) / 2;
        });

        public static readonly Interpolation CircleIn = new DelegateInterpolation(a =>
            1 - (float)Math.Sqrt(1 - a * a));

        public static readonly Interpolation CircleOut = new DelegateInterpolation(a =>
            (float)Math.Sqrt(1 - (a - 1) * (a - 1)));

        public static readonly Interpolation Elastic = new DelegateInterpolation(a => _elastic(2, 10, a));
        public static readonly Interpolation ElasticIn = new DelegateInterpolation(a => _elasticIn(2, 10, a));
        public static readonly Interpolation ElasticOut = new DelegateInterpolation(a => _elasticOut(2, 10, a));

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

        private static Func<float, float, float, float> _elastic = (value, power, a) => {
            if (a <= .5f) {
                a *= 2;
                return (float)Math.Pow(value, power * (a - 1)) * (float)Math.Sin(a * 20) * 1.0955f / 2;
            }
            a = 1 - a;
            a *= 2;
            return 1 - (float)Math.Pow(value, power * (a - 1)) * (float)Math.Sin(a * 20) * 1.0955f / 2;
        };

        private static Func<float, float, float, float> _elasticIn = (value, power, a) => {
            return (float)Math.Pow(value, power * (a - 1)) * (float)Math.Sin(a * 20) * 1.0955f;
        };

        private static Func<float, float, float, float> _elasticOut = (value, power, a) => {
            a = 1 - a;
            return (1 - (float)Math.Pow(value, power * (a - 1)) * (float)Math.Sin(a * 20) * 1.0955f);
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

        private class ExpInterpolation : Interpolation
        {
            protected float _value;
            protected float _power;
            protected float _min;
            protected float _scale;

            public ExpInterpolation (float value, float power)
            {
                _value = value;
                _power = power;
                _min = (float)Math.Pow(value, -power);
                _scale = 1 / (1 - _min);
            }

            public override float Apply (float a)
            {
                if (a <= .5f)
                    return ((float)Math.Pow(_value, _power * (a * 2 - 1)) - _min) * _scale / 2;
                return (2 - ((float)Math.Pow(_value, -_power * (a * 2 - 1)) - _min) * _scale) / 2;
            }
        }

        private class ExpInInterpolation : ExpInterpolation
        {
            public ExpInInterpolation (float value, float power)
                : base(value, power)
            { }

            public override float Apply (float a)
            {
                return ((float)Math.Pow(_value, _power * (a - 1)) - _min) * _scale;
            }
        }

        private class ExpOutInterpolation : ExpInterpolation
        {
            public ExpOutInterpolation (float value, float power)
                : base(value, power)
            { }

            public override float Apply (float a)
            {
                return 1 - ((float)Math.Pow(_value, -_power * a) - _min) * _scale;
            }
        }
    }
}
