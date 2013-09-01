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
        private class LocalClickListener : ClickListener
        {
            private Button _button;

            public LocalClickListener (Button button)
            {
                _button = button;
            }

            public override void Clicked (InputEvent ev, float x, float y)
            {
                if (_button.IsDisabled)
                    return;
                _button.IsChecked = !_button.IsChecked;
            }
        }

        private ButtonStyle _style;
        private bool _isChecked;

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
            Touchable = Touchable.Enabled;
            ClickListener = new LocalClickListener(this);
            AddListener(ClickListener);
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

        internal ButtonGroup ButtonGroup { get; set; }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked == value)
                    return;
                if (ButtonGroup != null && !ButtonGroup.CanCheck(this, _isChecked))
                    return;

                _isChecked = value;
                if (!IsDisabled) {
                    ChangeEvent changeEvent = Pools<ChangeEvent>.Obtain();
                    if (Fire(changeEvent))
                        _isChecked = !_isChecked;
                    Pools<ChangeEvent>.Release(changeEvent);
                }
            }
        }

        public void Toggle ()
        {
            IsChecked = !IsChecked;
        }

        public bool IsPressed
        {
            get { return ClickListener.IsPressed; }
        }

        public bool IsOver
        {
            get { return ClickListener.IsOver; }
        }

        public ClickListener ClickListener { get; private set; }

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
                if (_style == null)
                    throw new ArgumentNullException("value");
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
