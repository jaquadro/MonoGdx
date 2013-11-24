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
    /// Moves an actor from its current size to a specific size.
    /// </summary>
    public class SizeToAction : TemporalAction
    {
        private float _startWidth;
        private float _startHeight;

        public float Width { get; set; }
        public float Height { get; set; }

        public void SetSize (float width, float height)
        {
            Width = width;
            Height = height;
        }

        protected override void Begin ()
        {
            _startWidth = Actor.Width;
            _startHeight = Actor.Height;
        }

        protected override void Update (float percent)
        {
            Actor.SetSize(_startWidth + (Width - _startWidth) * percent, _startHeight + (Height - _startHeight) * percent);
        }
    }
}
