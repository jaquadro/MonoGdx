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

            Actor.Color = _color.Value;
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
