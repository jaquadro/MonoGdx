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
using MonoGdx.Scene2D.Utils;
using MonoGdx.TableLayout;

namespace MonoGdx.Scene2D.UI
{
    public class VerticalGroup : WidgetGroup
    {
        private float _prefWidth;
        private float _prefHeight;
        private bool _sizeInvalid = true;

        public VerticalGroup ()
        {
            Touchable = Scene2D.Touchable.ChildrenOnly;
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
                    _prefWidth = Math.Max(_prefWidth, layout.PrefWidth);
                    _prefHeight += layout.PrefHeight;
                }
                else {
                    _prefWidth = Math.Max(_prefWidth, child.Width);
                    _prefHeight += child.Height;
                }
            }
        }

        public override void Layout ()
        {
            float groupWidth = Width;
            float y = IsReversed ? 0 : Height;
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

                float x;
                if ((Alignment & Alignment.Left) != 0)
                    x = 0;
                else if ((Alignment & Alignment.Right) != 0)
                    x = groupWidth - width;
                else
                    x = (groupWidth - width) / 2;

                if (!IsReversed)
                    y += height * dir;
                child.SetBounds(x, y, width, height);
                if (IsReversed)
                    y += height * dir;
            }
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
}
