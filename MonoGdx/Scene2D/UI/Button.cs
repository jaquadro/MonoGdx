﻿/**
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
using MonoGdx.Scene2D.Utils;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.UI
{
    public class Button : Table
    {
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Button));
        public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Button));
        public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Button));
        public static readonly RoutedEvent ToggleEvent = EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Button));

        private ButtonStyle _style;
        private bool _isToggle;
        private bool _isChecked;
        private ClickEventManager _clickManager;

        public Button (Skin skin)
            : base(skin)
        {
            Initialize();
            Style = skin.Get<ButtonStyle>();
            Width = PrefWidth;
            Height = PrefHeight;
        }

        public Button (Skin skin, string styleName)
            : base(skin)
        {
            Initialize();
            Style = skin.Get<ButtonStyle>(styleName);
            Width = PrefWidth;
            Height = PrefHeight;
        }

        public Button (Actor child, Skin skin, string styleName)
            : this(child, skin.Get<ButtonStyle>(styleName))
        { }

        public Button (Actor child, ButtonStyle style)
        {
            Initialize();
            Add(child);
            Style = style;
            Width = PrefWidth;
            Height = PrefHeight;
        }

        public Button (ButtonStyle style)
        {
            Initialize();
            Style = style;
            Width = PrefWidth;
            Height = PrefHeight;
        }

        public Button ()
        {
            Initialize();
        }

        [TODO]
        private void Initialize ()
        {
            _clickManager = new ClickEventManager(this);
            _clickManager.ClickHandler = e => {
                if (IsDisabled)
                    return;

                if (OnClicked())
                    return;

                if (IsToggle)
                    IsChecked = !IsChecked;
            };

            Touchable = Touchable.Enabled;
        }

        public Button (ISceneDrawable up)
            : this(new ButtonStyle(up, null, null))
        { }

        public Button (ISceneDrawable up, ISceneDrawable down)
            : this(new ButtonStyle(up, down, null))
        { }

        public Button (ISceneDrawable up, ISceneDrawable down, ISceneDrawable chkd)
            : this(new ButtonStyle(up, down, chkd))
        { }

        public Button (Actor child, Skin skin)
            : this(child, skin.Get<ButtonStyle>())
        { }

        internal ButtonGroup ButtonGroup { get; set; }

        public bool IsToggle
        {
            get { return _isToggle; }
            set
            {
                _isToggle = value;
                if (!_isToggle)
                    _isChecked = false;
            }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (!IsToggle)
                    return;
                if (_isChecked == value)
                    return;
                if (ButtonGroup != null && !ButtonGroup.CanCheck(this, _isChecked))
                    return;

                _isChecked = value;
                if (!IsDisabled) {
                    bool cancel = OnToggled();

                    if (cancel) {
                        if (_isChecked)
                            cancel = OnChecked();
                        else
                            cancel = OnUnchecked();
                    }

                    if (cancel)
                        _isChecked = !_isChecked;
                }
            }
        }

        public void Toggle ()
        {
            if (!IsToggle)
                return;
            IsChecked = !IsChecked;
        }

        public bool IsPressed
        {
            get { return _clickManager.IsPressed; }
        }

        public bool IsOver
        {
            get { return _clickManager.IsOver; }
        }

        public bool IsDisabled { get; set; }

        public ButtonStyle Style
        {
            get { return StyleCore; }
            set { StyleCore = value; }
        }

        protected virtual ButtonStyle StyleCore
        {
            get { return _style; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("StyleCore");
                _style = value;

                ISceneDrawable background = _style.Up ?? _style.Down ?? _style.Checked;
                if (background != null) {
                    PadBottom = background.BottomHeight;
                    PadTop = background.TopHeight;
                    PadLeft = background.LeftWidth;
                    PadRight = background.RightWidth;
                }
                InvalidateHierarchy();
            }
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            Validate();

            ISceneDrawable background = null;
            float offsetX = 0;
            float offsetY = 0;

            if (IsPressed && !IsDisabled) {
                background = _style.Down ?? _style.Up;
                offsetX = _style.PressedOffsetX;
                offsetY = _style.PressedOffsetY;
            }
            else {
                if (IsDisabled && _style.Disabled != null)
                    background = _style.Disabled;
                else if (IsChecked && _style.Checked != null)
                    background = IsOver ? (_style.CheckedOver ?? _style.Checked) : _style.Checked;
                else if (IsOver && _style.Over != null)
                    background = _style.Over;
                else
                    background = _style.Up;

                offsetX = _style.UnpressedOffsetX;
                offsetY = _style.UnpressedOffsetY;
            }

            if (background != null) {
                spriteBatch.Color = Color.MultiplyAlpha(parentAlpha);
                background.Draw(spriteBatch, X, Y, Width, Height);
            }

            foreach (var child in Children)
                child.Translate(offsetX, offsetY);

            base.Draw(spriteBatch, parentAlpha);

            foreach (var child in Children)
                child.Translate(-offsetX, -offsetY);
        }

        protected override void DrawBackground (GdxSpriteBatch spriteBatch, float parentAlpha)
        { }

        public override float PrefWidth
        {
            get
            {
                float width = base.PrefWidth;
                if (_style.Up != null)
                    width = Math.Max(width, _style.Up.MinWidth);
                if (_style.Down != null)
                    width = Math.Max(width, _style.Down.MinWidth);
                if (_style.Checked != null)
                    width = Math.Max(width, _style.Checked.MinWidth);
                return width;
            }
        }

        public override float PrefHeight
        {
            get
            {
                float height = base.PrefHeight;
                if (_style.Up != null)
                    height = Math.Max(height, _style.Up.MinHeight);
                if (_style.Down != null)
                    height = Math.Max(height, _style.Down.MinHeight);
                if (_style.Checked != null)
                    height = Math.Max(height, _style.Checked.MinHeight);
                return height;
            }
        }

        public override float MinWidth
        {
            get { return PrefWidth; }
        }

        public override float MinHeight
        {
            get { return PrefHeight; }
        }

        public event RoutedEventHandler Clicked
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        public event RoutedEventHandler Checked
        {
            add { AddHandler(CheckedEvent, value); }
            remove { RemoveHandler(CheckedEvent, value); }
        }

        public event RoutedEventHandler Unchecked
        {
            add { AddHandler(UncheckedEvent, value); }
            remove { RemoveHandler(UncheckedEvent, value); }
        }

        public event RoutedEventHandler Toggled
        {
            add { AddHandler(ToggleEvent, value); }
            remove { RemoveHandler(ToggleEvent, value); }
        }

        protected virtual bool OnClicked ()
        {
            RoutedEventArgs args = InitializeEventArgs(Pools<RoutedEventArgs>.Obtain(), ClickEvent);
            bool cancel = RaiseEvent(args);
            Pools<RoutedEventArgs>.Release(args);
            return cancel;
        }

        protected virtual bool OnChecked ()
        {
            RoutedEventArgs args = InitializeEventArgs(Pools<RoutedEventArgs>.Obtain(), CheckedEvent);
            bool cancel = RaiseEvent(args);
            Pools<RoutedEventArgs>.Release(args);
            return cancel;
        }

        protected virtual bool OnUnchecked ()
        {
            RoutedEventArgs args = InitializeEventArgs(Pools<RoutedEventArgs>.Obtain(), UncheckedEvent);
            bool cancel = RaiseEvent(args);
            Pools<RoutedEventArgs>.Release(args);
            return cancel;
        }

        protected virtual bool OnToggled ()
        {
            RoutedEventArgs args = InitializeEventArgs(Pools<RoutedEventArgs>.Obtain(), ToggleEvent);
            bool cancel = RaiseEvent(args);
            Pools<RoutedEventArgs>.Release(args);
            return cancel;
        }
    }

    public class ButtonStyle
    {
        public ButtonStyle ()
        { }

        public ButtonStyle (ISceneDrawable up, ISceneDrawable down, ISceneDrawable chkd)
        {
            Up = up;
            Down = down;
            Checked = chkd;
        }

        public ButtonStyle (ButtonStyle style)
        {
            Up = style.Up;
            Down = style.Down;
            Over = style.Over;
            Checked = style.Checked;
            CheckedOver = style.CheckedOver;
            Disabled = style.Disabled;
            PressedOffsetX = style.PressedOffsetX;
            PressedOffsetY = style.PressedOffsetY;
            UnpressedOffsetX = style.UnpressedOffsetX;
            UnpressedOffsetY = style.UnpressedOffsetY;
        }

        public ISceneDrawable Up { get; set; }
        public ISceneDrawable Down { get; set; }
        public ISceneDrawable Over { get; set; }
        public ISceneDrawable Checked { get; set; }
        public ISceneDrawable CheckedOver { get; set; }
        public ISceneDrawable Disabled { get; set; }

        public float PressedOffsetX { get; set; }
        public float PressedOffsetY { get; set; }

        public float UnpressedOffsetX { get; set; }
        public float UnpressedOffsetY { get; set; }
    }
}
