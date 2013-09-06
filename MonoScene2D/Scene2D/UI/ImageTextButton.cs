using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using MonoGdx.TableLayout;
using MonoGdx.Utils;
using Microsoft.Xna.Framework;

namespace MonoGdx.Scene2D.UI
{
    public class ImageTextButton : Button
    {
        private readonly Image _image;
        private readonly Label _label;
        private ImageTextButtonStyle _style;

        public ImageTextButton (string text, Skin skin)
            : this(text, skin.Get<ImageTextButtonStyle>())
        {
            Skin = skin;
        }

        public ImageTextButton (string text, Skin skin, string styleName)
            : this(text, skin.Get<ImageTextButtonStyle>(styleName))
        {
            Skin = skin;
        }

        public ImageTextButton (string text, ImageTextButtonStyle style)
            : base(style)
        {
            _style = style;

            Defaults().Space(3);

            _image = new Image();
            _image.Scaling = Scaling.Fit;
            Add(_image);

            _label = new Label(text, new LabelStyle(style.Font, style.FontColor));
            _label.SetAlignment(Alignment.Center);
            Add(_label);

            Width = PrefWidth;
            Height = PrefHeight;
        }

        public new ImageTextButtonStyle Style
        {
            get { return _style; }
            set
            {
                base.StyleCore = value;
                _style = value;

                if (_image != null)
                    UpdateImage();
                if (_label != null) {
                    LabelStyle labelStyle = _label.Style;
                    labelStyle.Font = _style.Font;
                    labelStyle.FontColor = _style.FontColor;
                    _label.Style = labelStyle;
                }
            }
        }

        protected override ButtonStyle StyleCore
        {
            get { return _style; }
            set
            {
                if (!(value is ImageTextButtonStyle))
                    throw new ArgumentException("Style must be an ImageTextButtonStyle");
                Style = value as ImageTextButtonStyle;
            }
        }

        private void UpdateImage ()
        {
            if (IsDisabled && _style.ImageDisabled != null)
                _image.Drawable = _style.ImageDisabled;
            else if (IsPressed && _style.ImageDown != null)
                _image.Drawable = _style.ImageDown;
            else if (IsChecked && _style.ImageChecked != null)
                _image.Drawable = IsOver ? _style.ImageCheckedOver ?? _style.ImageChecked : _style.ImageChecked;
            else if (IsOver && _style.ImageOver != null)
                _image.Drawable = _style.ImageOver;
            else if (_style.ImageUp != null)
                _image.Drawable = _style.ImageUp;
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            UpdateImage();

            Color? fontColor;
            if (IsDisabled && _style.DisabledFontColor != null)
                fontColor = _style.DisabledFontColor;
            else if (IsPressed && _style.DownFontColor != null)
                fontColor = _style.DownFontColor;
            else if (IsChecked && _style.CheckedFontColor != null)
                fontColor = IsOver ? (_style.CheckedOverFontColor ?? _style.CheckedFontColor) : _style.CheckedFontColor;
            else if (IsOver && _style.OverFontColor != null)
                fontColor = _style.OverFontColor;
            else
                fontColor = _style.FontColor;

            if (fontColor != null)
                _label.Style.FontColor = fontColor;

            base.Draw(spriteBatch, parentAlpha);
        }

        public Image Image
        {
            get { return _image; }
        }

        public Cell ImageCell
        {
            get { return GetCell(_image); }
        }

        public Label Label
        {
            get { return _label; }
        }

        public Cell LabelCell
        {
            get { return GetCell(_label); }
        }

        public string Text
        {
            get { return _label.Text; }
            set { _label.Text = value; }
        }
    }

    public class ImageTextButtonStyle : TextButtonStyle
    {
        public ImageTextButtonStyle ()
        { }

        public ImageTextButtonStyle (ISceneDrawable up, ISceneDrawable down, ISceneDrawable chkd,
            BitmapFont font)
            : base(up, down, chkd, font)
        { }

        public ImageTextButtonStyle (ImageTextButtonStyle style)
            : base(style)
        {
            ImageUp = style.ImageUp;
            ImageDown = style.ImageDown;
            ImageOver = style.ImageOver;
            ImageChecked = style.ImageChecked;
            ImageCheckedOver = style.ImageCheckedOver;
            ImageDisabled = style.ImageDisabled;
        }

        public ImageTextButtonStyle (TextButtonStyle style)
            : base(style)
        { }

        public ISceneDrawable ImageUp { get; set; }
        public ISceneDrawable ImageDown { get; set; }
        public ISceneDrawable ImageOver { get; set; }
        public ISceneDrawable ImageChecked { get; set; }
        public ISceneDrawable ImageCheckedOver { get; set; }
        public ISceneDrawable ImageDisabled { get; set; }
    }
}
