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

using Microsoft.Xna.Framework;

namespace MonoGdx.Scene2D.Actions
{
    /// <summary>
    /// Sets the actor's color (or a specified color), from the current to the new color.
    /// </summary>
    public class ColorAction : TemporalAction
    {
        private Color _startColor;

        /// <summary>
        /// Sets the color to modify.  If null (default), the <see ref="Actor">actor's</see> color will be used.
        /// </summary>
        public Color? Color { get; set; }

        /// <summary>
        /// Sets the color to transition to.
        /// </summary>
        public Color EndColor { get; set; }

        protected override void Begin ()
        {
            if (Color == null)
                Color = Actor.Color;
            _startColor = Color.Value;
        }

        protected override void Update (float percent)
        {
            float r = _startColor.R + (EndColor.R - _startColor.R) * percent;
            float g = _startColor.G + (EndColor.G - _startColor.G) * percent;
            float b = _startColor.B + (EndColor.B - _startColor.B) * percent;
            float a = _startColor.A + (EndColor.A - _startColor.A) * percent;

            Color = new Color((byte)r, (byte)g, (byte)b, (byte)a);
        }

        public override void Reset ()
        {
            base.Reset();
            Color = null;
        }
    }
}
