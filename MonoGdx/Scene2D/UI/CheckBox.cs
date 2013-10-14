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
                _image.Drawable = (IsOver) ? _style.CheckboxOnOver ?? _style.CheckboxOn : _style.CheckboxOn;
            else
                _image.Drawable = (IsOver) ? _style.CheckboxOver ?? _style.CheckboxOff : _style.CheckboxOff;

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
            CheckboxOver = style.CheckboxOver;
            CheckboxOnOver = style.CheckboxOnOver;
            Font = style.Font;
            FontColor = style.FontColor;
        }

        public ISceneDrawable CheckboxOn { get; set; }
        public ISceneDrawable CheckboxOff { get; set; }
        public ISceneDrawable CheckboxOver { get; set; }
        public ISceneDrawable CheckboxOnOver { get; set; }
    }
}
