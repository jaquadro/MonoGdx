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

namespace MonoGdx.Scene2D.Actions
{
    /// <summary>
    /// Sets the actor's scale from its current value to a specific value.
    /// </summary>
    public class ScaleToAction : TemporalAction
    {
        private float _startX;
        private float _startY;

        public float X { get; set; }
        public float Y { get; set; }

        public void SetScale (float scale)
        {
            X = scale;
            Y = scale;
        }

        public void SetScale (float x, float y)
        {
            X = x;
            Y = y;
        }

        protected override void Begin ()
        {
            _startX = Actor.ScaleX;
            _startY = Actor.ScaleY;
        }

        protected override void Update (float percent)
        {
            Actor.SetScale(_startX + (X - _startX) * percent, _startY + (Y - _startY) * percent);
        }
    }
}
