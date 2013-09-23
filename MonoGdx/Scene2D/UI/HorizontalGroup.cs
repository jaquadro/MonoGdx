/**
 * Copyright 2013 See AUTHORS file.
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
using MonoGdx.Scene2D.Utils;
using MonoGdx.TableLayout;
using MonoGdx.Graphics.G2D;

namespace MonoGdx.Scene2D.UI
{
    public class HorizontalGroup : WidgetGroup
    {
        private HorizontalGroupStyle _style;
        private float _prefWidth;
        private float _prefHeight;
        private bool _sizeInvalid = true;

        public HorizontalGroup ()
        {
            Touchable = Scene2D.Touchable.ChildrenOnly;
        }

        public HorizontalGroup (Skin skin)
            : this(skin.Get<HorizontalGroupStyle>())
        { }

        public HorizontalGroup (Skin skin, string styleName)
            : this(skin.Get<HorizontalGroupStyle>(styleName))
        { }

        public HorizontalGroup (HorizontalGroupStyle style)
            : this()
        {
            Style = style;
        }

        public HorizontalGroupStyle Style
        {
            get { return _style; }
            set { _style = value; }
        }

        public Alignment Alignment { get; set; }

        public bool IsReversed { get; set; }

        public override void Invalidate ()
        {
            base.Invalidate();
            _sizeInvalid = true;
        }

        private void ComputeSize ()
        {
            _sizeInvalid = false;
            _prefWidth = 0;
            _prefHeight = 0;

            foreach (var child in Children) {
                if (child is ILayout) {
                    ILayout layout = child as ILayout;
                    _prefWidth += layout.PrefWidth;
                    _prefHeight = Math.Max(_prefHeight, layout.PrefHeight);
                    
                }
                else {
                    _prefWidth += child.Width;
                    _prefHeight = Math.Max(_prefHeight, child.Height);
                }
            }
        }

        public override void Layout ()
        {
            float groupHeight = Height;
            float x = IsReversed ? 0 : Width;
            float dir = IsReversed ? 1 : -1;

            foreach (var child in Children) {
                float width;
                float height;

                if (child is ILayout) {
                    ILayout layout = child as ILayout;
                    width = layout.PrefWidth;
                    height = layout.PrefHeight;
                }
                else {
                    width = child.Width;
                    height = child.Height;
                }

                float y;
                if ((Alignment & Alignment.Bottom) != 0)
                    y = 0;
                else if ((Alignment & Alignment.Top) != 0)
                    y = groupHeight - height;
                else
                    y = (groupHeight - height) / 2;

                if (!IsReversed)
                    x += width * dir;
                child.SetBounds(x, y, width, height);
                if (IsReversed)
                    x += width * dir;
            }
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            Validate();

            if (_style != null && _style.Background != null) {
                spriteBatch.Color = Color.MultiplyAlpha(parentAlpha);
                _style.Background.Draw(spriteBatch, X, Y, Width, Height);
            }

            base.Draw(spriteBatch, parentAlpha);
        }

        public override float PrefWidth
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _prefWidth;
            }
        }

        public override float PrefHeight
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _prefHeight;
            }
        }
    }

    public class HorizontalGroupStyle
    {
        public HorizontalGroupStyle ()
        { }

        public HorizontalGroupStyle (ISceneDrawable background)
        {
            Background = background;
        }

        public HorizontalGroupStyle (HorizontalGroupStyle style)
        {
            Background = style.Background;
        }

        public ISceneDrawable Background { get; set; }
    }
}
