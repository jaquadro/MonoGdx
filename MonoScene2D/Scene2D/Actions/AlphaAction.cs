using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGdx.Scene2D.Actions
{
    public class AlphaAction : TemporalAction
    {
        private float _start;
        private float _end;
        private Color? _color;

        protected override void Begin ()
        {
            if (_color == null)
                _color = Actor.Color;
            _start = _color.Value.A / 255f;
        }

        protected override void Update (float percent)
        {
            byte a = (byte)((_start + (_end - _start) * percent) * 255);

            Color c = _color.Value;
            _color = new Color(c.R, c.G, c.B, a);
        }

        public override void Reset ()
        {
            base.Reset();
            _color = null;
        }

        public Color? Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public float Alpha
        {
            get { return _end; }
            set { _end = value; }
        }
    }
}
