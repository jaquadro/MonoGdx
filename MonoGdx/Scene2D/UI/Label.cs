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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using MonoGdx.TableLayout;

namespace MonoGdx.Scene2D.UI
{
    public class Label : Widget
    {
        private LabelStyle _style;
        private BitmapFontCache _cache;
        private string _text = "";
        private bool _sizeInvalid = true;
        private Alignment _labelAlign = Alignment.Left;
        private HAlignment _lineAlign = HAlignment.Left;
        private bool _wrap;
        private float _lastPrefHeight;
        private float _fontScaleX = 1;
        private float _fontScaleY = 1;
        private TextBounds _bounds;

        public Label (string text, Skin skin)
            : this(text, skin.Get<LabelStyle>())
        { }

        public Label (string text, Skin skin, string styleName)
            : this(text, skin.Get<LabelStyle>(styleName))
        { }

        public Label (string text, Skin skin, string fontName, Color color)
            : this(text, new LabelStyle(skin.GetFont(fontName), color))
        { }

        public Label (string text, Skin skin, string fontName, string colorName)
            : this(text, new LabelStyle(skin.GetFont(fontName), skin.GetColor(colorName)))
        { }

        public Label (string text, LabelStyle style)
        {
            if (text != null)
                Text = Text + text;

            Style = style;
            Width = PrefWidth;
            Height = PrefHeight;
        }

        public LabelStyle Style
        {
            get { return _style; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Font == null)
                    throw new ArgumentException("Missing LabelStyle font");

                _style = value;
                _cache = new BitmapFontCache(_style.Font, _style.Font.UsesIntegerPostions);
                InvalidateHierarchy();
            }
        }

        public bool TextEquals (string other)
        {
            return _text == other;
            /*int length = _text.Length;
            if (length != other.Length)
                return false;
            for (int i = 0; i < length; i++) {
                if (_text[i] != other[i])
                    return false;
            }

            return true;*/
        }

        public string Text
        {
            get { return _text.ToString(); }
            set
            {
                if (value == null)
                    value = "";
                if (TextEquals(value))
                    return;
                _text = value;
            }
        }

        public override void Invalidate ()
        {
            base.Invalidate();
            _sizeInvalid = true;
        }

        private void ComputeSize ()
        {
            _sizeInvalid = false;
            if (_wrap) {
                float width = Width;
                if (_style.Background != null)
                    width -= _style.Background.LeftWidth + _style.Background.RightWidth;
                _bounds = _cache.Font.GetWrappedBounds(_text, width);
            }
            else
                _bounds = _cache.Font.GetMultiLineBounds(_text);

            _bounds.Width *= _fontScaleX;
            _bounds.Height *= _fontScaleY;
        }

        public override void Layout ()
        {
            if (_sizeInvalid)
                ComputeSize();

            if (_wrap) {
                float prefHeight = PrefHeight;
                if (prefHeight != _lastPrefHeight) {
                    _lastPrefHeight = prefHeight;
                    InvalidateHierarchy();
                }
            }

            BitmapFont font = _cache.Font;
            float oldScaleX = font.ScaleX;
            float oldScaleY = font.ScaleY;
            if (_fontScaleX != 1 || _fontScaleY != 1)
                font.SetScale(_fontScaleX, _fontScaleY);

            ISceneDrawable background = _style.Background;
            float width = Width;
            float height = Height;
            float x = 0;
            float y = 0;
            if (background != null) {
                x = background.LeftWidth;
                y = background.BottomHeight;
                width -= background.LeftWidth + background.RightWidth;
                height -= background.BottomHeight + background.TopHeight;
            }

            if ((_labelAlign & Alignment.Top) != 0) {
                y += _cache.Font.IsFlipped ? 0 : height - _bounds.Height;
                y += _style.Font.Descent;
            }
            else if ((_labelAlign & Alignment.Bottom) != 0) {
                y += _cache.Font.IsFlipped ? height - _bounds.Height : 0;
                y -= _style.Font.Descent;
            }
            else
                y += (int)((height - _bounds.Height) / 2);

            if (!_cache.Font.IsFlipped)
                y += _bounds.Height;

            if ((_labelAlign & Alignment.Left) == 0) {
                if ((_labelAlign & Alignment.Right) != 0)
                    x += width - _bounds.Width;
                else
                    x += (int)((width - _bounds.Width) / 2);
            }

            if (_wrap)
                _cache.SetWrappedText(_text, x, y, _bounds.Width, _lineAlign);
            else
                _cache.SetMultiLineText(_text, x, y, _bounds.Width, _lineAlign);

            if (_fontScaleX != 1 || _fontScaleY != 1)
                font.SetScale(oldScaleX, oldScaleY);
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            Validate();

            if (_style.Background != null) {
                spriteBatch.Color = Color.MultiplyAlpha(parentAlpha);
                _style.Background.Draw(spriteBatch, X, Y, Width, Height);
            }

            _cache.Color = (_style.FontColor == null) ? Color : Color.Multiply(_style.FontColor.Value);
            _cache.SetPosition(X, Y);
            _cache.Draw(spriteBatch, (Color.A / 255f) * parentAlpha);
        }

        public override float PrefWidth
        {
            get
            {
                if (_wrap)
                    return 0;
                if (_sizeInvalid)
                    ComputeSize();

                float width = _bounds.Width;
                ISceneDrawable background = _style.Background;
                if (background != null)
                    width += background.LeftWidth + background.RightWidth;
                return width;
            }
        }

        public override float PrefHeight
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();

                float height = _bounds.Height - _style.Font.Descent * 2;
                ISceneDrawable background = _style.Background;
                if (background != null)
                    height += background.TopHeight + background.BottomHeight;
                return height;
            }
        }

        public TextBounds TextBounds
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _bounds;
            }
        }

        public bool TextWrapping
        {
            get { return _wrap; }
            set
            {
                _wrap = value;
                InvalidateHierarchy();
            }
        }

        public void SetAlignment (Alignment wrapAlign)
        {
            SetAlignment(wrapAlign, wrapAlign);
        }

        public void SetAlignment (Alignment labelAlign, HAlignment lineAlign)
        {
            _labelAlign = labelAlign;
            _lineAlign = lineAlign;
            Invalidate();
        }

        public void SetAlignment (Alignment labelAlign, Alignment lineAlign)
        {
            _labelAlign = labelAlign;

            if ((lineAlign & Alignment.Left) != 0)
                _lineAlign = HAlignment.Left;
            else if ((lineAlign & Alignment.Right) != 0)
                _lineAlign = HAlignment.Right;
            else
                _lineAlign = HAlignment.Center;

            Invalidate();
        }

        public void SetFontScale (float fontScale)
        {
            _fontScaleX = fontScale;
            _fontScaleY = fontScale;
            InvalidateHierarchy();
        }

        public void SetFontScale (float fontScaleX, float fontScaleY)
        {
            _fontScaleX = fontScaleX;
            _fontScaleY = fontScaleY;
            InvalidateHierarchy();
        }

        public float FontScaleX
        {
            get { return _fontScaleX; }
            set
            {
                _fontScaleX = value;
                InvalidateHierarchy();
            }
        }

        public float FontScaleY
        {
            get { return _fontScaleY; }
            set
            {
                _fontScaleY = value;
                InvalidateHierarchy();
            }
        }
    }

    public class LabelStyle
    {
        public LabelStyle ()
        { }

        public LabelStyle (BitmapFont font, Color? fontColor)
        {
            Font = font;
            FontColor = fontColor;
        }

        public LabelStyle (LabelStyle style)
        {
            Font = style.Font;
            FontColor = style.FontColor;
            Background = style.Background;
        }

        public BitmapFont Font { get; set; }
        public Color? FontColor { get; set; }
        public ISceneDrawable Background { get; set; }
    }
}
