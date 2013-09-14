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
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.UI
{
    public class Stack : WidgetGroup
    {
        private float _prefWidth;
        private float _prefHeight;
        private float _minWidth;
        private float _minheight;
        private float _maxWidth;
        private float _maxHeight;
        private bool _sizeInvalid = true;

        public Stack ()
        {
            IsTransform = false;
            Width = 150;
            Height = 150;
            Touchable = Touchable.ChildrenOnly;
        }

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
            _minWidth = 0;
            _minheight = 0;
            _maxWidth = 0;
            _maxHeight = 0;

            foreach (var child in Children) {
                float childMaxWidth;
                float childMaxHeight;

                if (child is ILayout) {
                    ILayout layout = child as ILayout;
                    _prefWidth = Math.Max(_prefWidth, layout.PrefWidth);
                    _prefHeight = Math.Max(_prefHeight, layout.PrefHeight);
                    _minWidth = Math.Max(_minWidth, layout.MinWidth);
                    _minheight = Math.Max(_minheight, layout.MinHeight);
                    childMaxWidth = layout.MaxWidth;
                    childMaxHeight = layout.MaxHeight;
                }
                else {
                    _prefWidth = Math.Max(_prefWidth, child.Width);
                    _prefHeight = Math.Max(_prefHeight, child.Height);
                    _minWidth = Math.Max(_minWidth, child.Width);
                    _minheight = Math.Max(_minheight, child.Height);
                    childMaxWidth = 0;
                    childMaxHeight = 0;
                }

                if (childMaxWidth > 0)
                    _maxWidth = (_maxWidth == 0) ? childMaxWidth : Math.Min(_maxWidth, childMaxWidth);
                if (childMaxHeight > 0)
                    _maxHeight = (_maxHeight == 0) ? childMaxHeight : Math.Min(_maxHeight, childMaxHeight);
            }
        }

        public void Add (Actor actor)
        {
            AddActor(actor);
        }

        public override void Layout ()
        {
            if (_sizeInvalid)
                ComputeSize();

            float width = Width;
            float height = Height;

            foreach (var child in Children) {
                child.X = 0;
                child.Y = 0;
                if (child.Width != width || child.Height != height) {
                    child.Width = width;
                    child.Height = height;

                    if (child is ILayout) {
                        ILayout layout = child as ILayout;
                        layout.Invalidate();
                        layout.Validate();
                    }
                }
                else {
                    if (child is ILayout)
                        (child as ILayout).Invalidate();
                }
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

        public override float MinWidth
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _minWidth;
            }
        }

        public override float MinHeight
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _minheight;
            }
        }

        public override float MaxWidth
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _maxWidth;
            }
        }

        public override float MaxHeight
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _maxHeight;
            }
        }
    }
}
