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
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGdx.Geometry;
using MonoGdx.Graphics.G2D;

namespace MonoGdx.Scene2D.Utils
{
    public class SpriteDrawable : BaseDrawable
    {
        private Sprite _sprite;

        public SpriteDrawable ()
        { }

        public SpriteDrawable (Sprite sprite)
        {
            Sprite = sprite;
        }

        public SpriteDrawable (SpriteDrawable drawable)
            : base(drawable)
        {
            Sprite = drawable.Sprite;
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float x, float y, float width, float height)
        {
            Sprite.SetBounds(x, y, width, height);

            Color color = Sprite.Color;
            Sprite.Color = ColorExt.Multiply(color, spriteBatch.Color);

            Sprite.Draw(spriteBatch);
            Sprite.Color = color;
        }

        public Sprite Sprite
        {
            get { return _sprite; }
            set
            {
                _sprite = value;

                MinWidth = _sprite.Width;
                MinHeight = _sprite.Height;
            }
        }
    }
}
