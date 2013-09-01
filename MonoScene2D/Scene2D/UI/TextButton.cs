using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using MonoGdx.TableLayout;

namespace MonoGdx.Scene2D.UI
{
    public class TextButton : Button
    {
        private readonly Label _label;
        private TextButtonStyle _style;

        public TextButton (string text, Skin skin)
            : this(text, skin.Get<TextButtonStyle>())
        {
            Skin = skin;
        }

        public TextButton (string text, Skin skin, string styleName)
            : this(text, skin.Get<TextButtonStyle>(styleName))
        {
            Skin = skin;
        }

        public TextButton (string text, TextButtonStyle style)
        {
            Style = style;

            _label = new Label(text, new LabelStyle(style.Font, style.FontColor));
            _label.SetAlignment(Alignment.Center);

            Add(_label).Expand().Fill();
            Width = PrefWidth;
            Height = PrefHeight;
        }

        public new TextButtonStyle Style
        {
            get { return StyleCore as TextButtonStyle; }
            set { StyleCore = value; }
        }

        protected override ButtonStyle StyleCore
        {
            get { return _style; }
            set
            {
                if (!(value is TextButtonStyle))
                    throw new ArgumentException("Style must be a TextButtonStyle");

                base.StyleCore = value;
                _style = value as TextButtonStyle;

                if (_label != null) {
                    LabelStyle labelStyle = _label.Style;
                    labelStyle.Font = _style.Font;
                    labelStyle.FontColor = _style.FontColor;
                    _label.Style = labelStyle;
                }
            }
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
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

    public class TextButtonStyle : ButtonStyle
    {
        public TextButtonStyle ()
        { }

        public TextButtonStyle (ISceneDrawable up, ISceneDrawable down, ISceneDrawable chkd, BitmapFont font)
            : base(up, down, chkd)
        {
            Font = font;
        }

        public TextButtonStyle (TextButtonStyle style)
            : base(style)
        {
            Font = style.Font;
            FontColor = style.FontColor;
            DownFontColor = style.DownFontColor;
            OverFontColor = style.OverFontColor;
            CheckedFontColor = style.CheckedFontColor;
            CheckedOverFontColor = style.CheckedOverFontColor;
            DisabledFontColor = style.DisabledFontColor;
        }

        public BitmapFont Font { get; set; }
        public Color? FontColor { get; set; }
        public Color? DownFontColor { get; set; }
        public Color? OverFontColor { get; set; }
        public Color? CheckedFontColor { get; set; }
        public Color? CheckedOverFontColor { get; set; }
        public Color? DisabledFontColor { get; set; }
    }
}
