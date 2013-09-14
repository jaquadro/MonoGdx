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
using MonoGdx.Graphics.G2D;

namespace MonoGdx.Scene2D.Utils
{
    public class TextureRegionDrawable : BaseDrawable
    {
        private TextureRegion _region;

        public TextureRegionDrawable ()
        { }

        public TextureRegionDrawable (TextureRegion region)
        {
            Region = region;
        }

        public TextureRegionDrawable (TextureRegionDrawable drawable)
            : base(drawable)
        {
            Region = drawable.Region;
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float x, float y, float width, float height)
        {
            spriteBatch.Draw(Region, x, y, width, height);
        }

        public TextureRegion Region
        {
            get { return _region; }
            set
            {
                _region = value;

                MinWidth = _region.RegionWidth;
                MinHeight = _region.RegionHeight;
            }
        }
    }
}
