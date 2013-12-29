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
using Microsoft.Xna.Framework.Graphics;
using MonoGdx.Utils;

namespace MonoGdx.Graphics.G2D
{
    public class BitmapFontCache
    {
        private static readonly StringSequence _workingSequence = new StringSequence("");

        private Color _color = Color.White;
        private int _vertexCount;

        public BitmapFontCache (BitmapFont font)
            : this(font, font.UsesIntegerPostions)
        { }

        public BitmapFontCache (BitmapFont font, bool integer)
        {
            Vertices = new VertexPositionColorTexture[0];
            Font = font;
            UsesIntegerPositions = integer;
        }

        public void SetPosition (float x, float y)
        {
            Translate(x - X, y - Y);
        }

        public void Translate (float xAmount, float yAmount)
        {
            if (xAmount == 0 && yAmount == 0)
                return;

            if (UsesIntegerPositions) {
                xAmount = (int)Math.Round(xAmount);
                yAmount = (int)Math.Round(yAmount);
            }

            X = X + xAmount;
            Y = Y + yAmount;

            VertexPositionColorTexture[] vertices = Vertices;
            for (int i = 0, n = _vertexCount; i < n; i++) {
                vertices[i].Position.X += xAmount;
                vertices[i].Position.Y += yAmount;
            }
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color == value)
                    return;
                _color = value;

                Color pColor = Color.FromNonPremultiplied(value.R, value.G, value.B, value.A);

                VertexPositionColorTexture[] vertices = Vertices;
                for (int i = 0, n = _vertexCount; i < n; i++)
                    vertices[i].Color = pColor;
            }
        }

        public void SetColor (Color tint, int start, int end)
        {
            Color pColor = Color.FromNonPremultiplied(tint.R, tint.G, tint.B, tint.A);

            VertexPositionColorTexture[] vertices = Vertices;
            for (int i = start * 4, n = end * 4; i < n; i++)
                vertices[i].Color = pColor;
        }

        public void Draw (GdxSpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Font.Region.Texture, Vertices, 0, _vertexCount);
        }

        public void Draw (GdxSpriteBatch spriteBatch, int start, int end)
        {
            spriteBatch.Draw(Font.Region.Texture, Vertices, start * 4, end * 4);
        }

        public void Draw (GdxSpriteBatch spriteBatch, float alphaModulation)
        {
            if (alphaModulation == 1) {
                Draw(spriteBatch);
                return;
            }

            Color color = _color;
            Color = _color.MultiplyAlpha(alphaModulation);

            Draw(spriteBatch);
            Color = color;
        }

        public void Clear ()
        {
            X = 0;
            Y = 0;
            _vertexCount = 0;
        }

        private void Require (int glyphCount)
        {
            int vertexCount = _vertexCount + glyphCount * 4;
            if (Vertices == null || Vertices.Length < vertexCount) {
                VertexPositionColorTexture[] newVertices = new VertexPositionColorTexture[vertexCount];
                Array.Copy(Vertices, newVertices, _vertexCount);
                Vertices = newVertices;
            }
        }

        private float AddToCache (CharSequence str, float x, float y, int start, int end)
        {
            float startX = x;
            BitmapFont font = Font;
            Glyph lastGlyph = null;
            BitmapFontData data = font.Data;

            if (data.ScaleX == 1 && data.ScaleY == 1) {
                while (start < end) {
                    lastGlyph = data[str[start++]];
                    if (lastGlyph != null) {
                        AddGlyph(lastGlyph, x + lastGlyph.XOffset, y + lastGlyph.YOffset, lastGlyph.Width, lastGlyph.Height);
                        x += lastGlyph.XAdvance;
                        break;
                    }
                }

                while (start < end) {
                    char ch = str[start++];
                    Glyph g = data[ch];
                    if (g != null) {
                        x += lastGlyph.GetKerning(ch);
                        lastGlyph = g;
                        AddGlyph(lastGlyph, x + g.XOffset, y + g.YOffset, g.Width, g.Height);
                        x += g.XAdvance;
                    }
                }
            }
            else {
                float scaleX = data.ScaleX;
                float scaleY = data.ScaleY;

                while (start < end) {
                    lastGlyph = data[str[start++]];
                    if (lastGlyph != null) {
                        AddGlyph(lastGlyph, x + lastGlyph.XOffset * scaleX, y + lastGlyph.YOffset * scaleY, 
                            lastGlyph.Width * scaleX, lastGlyph.Height * scaleY);
                        x += lastGlyph.XAdvance * scaleX;
                    }
                }

                while (start < end) {
                    char ch = str[start++];
                    Glyph g = data[ch];
                    if (g != null) {
                        x += lastGlyph.GetKerning(ch) * scaleX;
                        lastGlyph = g;
                        AddGlyph(lastGlyph, x + g.XOffset * scaleX, y + g.YOffset * scaleY,
                            lastGlyph.Width * scaleX, lastGlyph.Height * scaleY);
                        x += g.XAdvance * scaleX;
                    }
                }
            }

            return x - startX;
        }

        private void AddGlyph (Glyph glyph, float x, float y, float width, float height)
        {
            float x2 = x + width;
            float y2 = y + height;
            float u = glyph.U;
            float u2 = glyph.U2;
            float v = glyph.V;
            float v2 = glyph.V2;

            VertexPositionColorTexture[] vertices = Vertices;

            if (UsesIntegerPositions) {
                x = (int)Math.Round(x);
                y = (int)Math.Round(y);
                x2 = (int)Math.Round(x2);
                y2 = (int)Math.Round(y2);
            }

            int idx = _vertexCount;
            _vertexCount += 4;

            Color pColor = Color.FromNonPremultiplied(Color.R, Color.G, Color.B, Color.A);

            vertices[idx + 0].Position = new Vector3(x, y, 0);
            vertices[idx + 0].Color = pColor;
            vertices[idx + 0].TextureCoordinate = new Vector2(u, v);

            vertices[idx + 1].Position = new Vector3(x, y2, 0);
            vertices[idx + 1].Color = pColor;
            vertices[idx + 1].TextureCoordinate = new Vector2(u, v2);

            vertices[idx + 2].Position = new Vector3(x2, y2, 0);
            vertices[idx + 2].Color = pColor;
            vertices[idx + 2].TextureCoordinate = new Vector2(u2, v2);

            vertices[idx + 3].Position = new Vector3(x2, y, 0);
            vertices[idx + 3].Color = pColor;
            vertices[idx + 3].TextureCoordinate = new Vector2(u2, v);
        }

        public TextBounds SetText (string str, float x, float y)
        {
            Clear();
            return AddText(str, x, y, 0, str.Length);
        }

        public TextBounds SetText (string str, float x, float y, int start, int end)
        {
            Clear();
            return AddText(str, x, y, start, end);
        }

        public TextBounds AddText (string str, float x, float y)
        {
            return AddText(str, x, y, 0, str.Length);
        }

        public TextBounds AddText (string str, float x, float y, int start, int end)
        {
            _workingSequence.Value = str;
            return AddText(_workingSequence, x, y, start, end);
        }

        public TextBounds SetText (CharSequence str, float x, float y)
        {
            Clear();
            return AddText(str, x, y, 0, str.Length);
        }

        public TextBounds SetText (CharSequence str, float x, float y, int start, int end)
        {
            Clear();
            return AddText(str, x, y, start, end);
        }

        public TextBounds AddText (CharSequence str, float x, float y)
        {
            return AddText(str, x, y, 0, str.Length);
        }

        public TextBounds AddText (CharSequence str, float x, float y, int start, int end)
        {
            Require(end - start);
            y += Font.Data.Ascent;

            return Bounds = new TextBounds() {
                Width = AddToCache(str, x, y, start, end),
                Height = Font.Data.CapHeight,
            };
        }

        public TextBounds SetMultiLineText (string str, float x, float y)
        {
            Clear();
            return AddMultiLineText(str, x, y, 0, HAlignment.Left);
        }

        public TextBounds SetMultiLineText (string str, float x, float y, float alignmentWidth, HAlignment alignment)
        {
            Clear();
            return AddMultiLineText(str, x, y, alignmentWidth, alignment);
        }

        public TextBounds AddMultiLineText (string str, float x, float y)
        {
            return AddMultiLineText(str, x, y, 0, HAlignment.Left);
        }

        public TextBounds AddMultiLineText (string str, float x, float y, float alignmentWidth, HAlignment alignment)
        {
            _workingSequence.Value = str;
            return AddMultiLineText(_workingSequence, x, y, alignmentWidth, alignment);
        }

        public TextBounds SetMultiLineText (CharSequence str, float x, float y)
        {
            Clear();
            return AddMultiLineText(str, x, y, 0, HAlignment.Left);
        }

        public TextBounds SetMultiLineText (CharSequence str, float x, float y, float alignmentWidth, HAlignment alignment)
        {
            Clear();
            return AddMultiLineText(str, x, y, alignmentWidth, alignment);
        }

        public TextBounds AddMultiLineText (CharSequence str, float x, float y)
        {
            return AddMultiLineText(str, x, y, 0, HAlignment.Left);
        }

        public TextBounds AddMultiLineText (CharSequence str, float x, float y, float alignmentWidth, HAlignment alignment)
        {
            BitmapFont font = Font;

            int length = str.Length;
            Require(length);

            y += Font.Data.Ascent;
            float down = font.Data.Down;

            float maxWidth = 0;
            float startY = y;
            int start = 0;
            int numLines = 0;

            while (start < length) {
                int lineEnd = BitmapFont.IndexOf(str, '\n', start);
                float xOffset = 0;
                float lineWidth = 0;

                if (alignment != HAlignment.Left) {
                    lineWidth = font.GetBounds(str, start, lineEnd).Width;
                    xOffset = alignmentWidth - lineWidth;

                    if (alignment == HAlignment.Center)
                        xOffset /= 2;
                }

                lineWidth = AddToCache(str, x + xOffset, y, start, lineEnd);
                maxWidth = Math.Max(maxWidth, lineWidth);
                start = lineEnd + 1;
                y += down;
                numLines++;
            }

            return Bounds = new TextBounds() {
                Width = maxWidth,
                Height = font.Data.CapHeight + (numLines - 1) * font.Data.LineHeight,
            };
        }

        public TextBounds SetWrappedText (string str, float x, float y, float wrapWidth)
        {
            Clear();
            return AddWrappedText(str, x, y, wrapWidth, HAlignment.Left);
        }

        public TextBounds SetWrappedText (string str, float x, float y, float wrapWidth, HAlignment alignment)
        {
            Clear();
            return AddWrappedText(str, x, y, wrapWidth, alignment);
        }

        public TextBounds AddWrappedText (string str, float x, float y, float wrapWidth)
        {
            return AddWrappedText(str, x, y, wrapWidth, HAlignment.Left);
        }

        public TextBounds AddWrappedText (string str, float x, float y, float wrapWidth, HAlignment alignment)
        {
            _workingSequence.Value = str;
            return AddWrappedText(_workingSequence, x, y, wrapWidth, alignment);
        }

        public TextBounds SetWrappedText (CharSequence str, float x, float y, float wrapWidth)
        {
            Clear();
            return AddWrappedText(str, x, y, wrapWidth, HAlignment.Left);
        }

        public TextBounds SetWrappedText (CharSequence str, float x, float y, float wrapWidth, HAlignment alignment)
        {
            Clear();
            return AddWrappedText(str, x, y, wrapWidth, alignment);
        }

        public TextBounds AddWrappedText (CharSequence str, float x, float y, float wrapWidth)
        {
            return AddWrappedText(str, x, y, wrapWidth, HAlignment.Left);
        }

        public TextBounds AddWrappedText (CharSequence str, float x, float y, float wrapWidth, HAlignment alignment)
        {
            BitmapFont font = Font;

            int length = str.Length;
            Require(length);

            y += font.Data.Ascent;
            float down = font.Data.Down;

            if (wrapWidth <= 0)
                wrapWidth = int.MaxValue;
            float maxWidth = 0;
            int start = 0;
            int numLines = 0;

            while (start < length) {
                int newLine = BitmapFont.IndexOf(str, '\n', start);
                while (start < newLine) {
                    if (!BitmapFont.IsWhitespace(str[start]))
                        break;
                    start++;
                }

                int lineEnd = start + font.ComputeVisibleGlyphs(str, start, newLine, wrapWidth);
                int nextStart = lineEnd + 1;

                if (lineEnd < newLine) {
                    while (lineEnd > start) {
                        if (BitmapFont.IsWhitespace(str[lineEnd]))
                            break;
                        lineEnd--;
                    }

                    if (lineEnd == start) {
                        if (nextStart > start + 1)
                            nextStart--;
                        lineEnd = nextStart;
                    }
                    else {
                        nextStart = lineEnd;
                        while (lineEnd > start) {
                            if (!BitmapFont.IsWhitespace(str[lineEnd - 1]))
                                break;
                            lineEnd--;
                        }
                    }
                }

                if (lineEnd > start) {
                    float xOffset = 0;
                    float lineWidth = 0;

                    if (alignment != HAlignment.Left) {
                        lineWidth = font.GetBounds(str, start, lineEnd).Width;
                        xOffset = wrapWidth - lineWidth;
                        if (alignment == HAlignment.Center)
                            xOffset /= 2;
                    }

                    lineWidth = AddToCache(str, x + xOffset, y, start, lineEnd);
                    maxWidth = Math.Max(maxWidth, lineWidth);
                }

                start = nextStart;
                y += down;
                numLines++;
            }

            return Bounds = new TextBounds() {
                Width = maxWidth,
                Height = font.Data.CapHeight + (numLines - 1) * font.Data.LineHeight,
            };
        }

        public TextBounds Bounds { get; private set; }

        public float X { get; private set; }

        public float Y { get; private set; }

        public BitmapFont Font { get; private set; }

        public bool UsesIntegerPositions { get; set; }

        public VertexPositionColorTexture[] Vertices { get; private set; }
    }
}
