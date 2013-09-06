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
    public class CheckBox : TextButton
    {
        private Image _image;
        private CheckBoxStyle _style;

        public CheckBox (string text, Skin skin)
            : this(text, skin.Get<CheckBoxStyle>())
        { }

        public CheckBox (string text, Skin skin, string styleName)
            : this(text, skin.Get<CheckBoxStyle>(styleName))
        { }

        public CheckBox (string text, CheckBoxStyle style)
            : base(text, style)
        {
            ClearChildren();
            Add(_image = new Image(style.CheckboxOff));
            Add(Label);

            Label.SetAlignment(Alignment.Left);
            Width = PrefWidth;
            Height = PrefHeight;
        }

        public new CheckBoxStyle Style
        {
            get { return _style; }
            set
            {
                base.StyleCore = value;
                _style = value;
            }
        }

        protected override ButtonStyle StyleCore
        {
            get { return _style;  }
            set
            {
                if (!(value is CheckBoxStyle))
                    throw new ArgumentException("Style must be a CheckBoxStyle");
                Style = value as CheckBoxStyle;
            }
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            if (IsChecked && _style.CheckboxOn != null)
                _image.Drawable = _style.CheckboxOn;
            else if (IsOver && _style.CheckboxOver != null)
                _image.Drawable = _style.CheckboxOver;
            else
                _image.Drawable = Style.CheckboxOff;

            base.Draw(spriteBatch, parentAlpha);
        }
    }

    public class CheckBoxStyle : TextButtonStyle
    {
        public CheckBoxStyle ()
        { }

        public CheckBoxStyle (ISceneDrawable checkboxOff, ISceneDrawable checkboxOn, BitmapFont font, Color fontColor)
        {
            CheckboxOff = checkboxOff;
            CheckboxOn = checkboxOn;
            Font = font;
            FontColor = fontColor;
        }

        public CheckBoxStyle (CheckBoxStyle style)
        {
            CheckboxOff = style.CheckboxOff;
            CheckboxOn = style.CheckboxOn;
            Font = style.Font;
            FontColor = style.FontColor;
        }

        public ISceneDrawable CheckboxOn { get; set; }
        public ISceneDrawable CheckboxOff { get; set; }
        public ISceneDrawable CheckboxOver { get; set; }
    }
}
