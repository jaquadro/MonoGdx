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
    public class BaseDrawable : ISceneDrawable
    {
        public BaseDrawable ()
        { }

        public BaseDrawable (ISceneDrawable drawable)
        {
            LeftWidth = drawable.LeftWidth;
            RightWidth = drawable.RightWidth;
            TopHeight = drawable.TopHeight;
            BottomHeight = drawable.BottomHeight;
            MinWidth = drawable.MinWidth;
            MinHeight = drawable.MinHeight;
        }

        public virtual void Draw (GdxSpriteBatch spriteBatch, float x, float y, float width, float height)
        { }

        public float LeftWidth { get; set; }
        public float RightWidth { get; set; }
        public float TopHeight { get; set; }
        public float BottomHeight { get; set; }
        public float MinWidth { get; set; }
        public float MinHeight { get; set; }
    }
}
