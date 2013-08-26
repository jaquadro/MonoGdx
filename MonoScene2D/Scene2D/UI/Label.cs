using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoScene2D.TableLayout;

namespace MonoScene2D.Scene2D.UI
{
    public class Label : Widget
    {
        public Label (CharSequence text, Skin skin)
        {
            throw new NotImplementedException();
        }

        public Label (CharSequence text, Skin skin, string styleName)
        {
            throw new NotImplementedException();
        }

        public Label (CharSequence text, Skin skin, string fontName, Color color)
        {
            throw new NotImplementedException();
        }

        public Label (CharSequence text, Skin skin, string fontName, string colorName)
        {
            throw new NotImplementedException();
        }

        public Label (CharSequence text, LabelStyle style)
        {
            throw new NotImplementedException();
        }

        public LabelStyle Style
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool TextEquals (CharSequence other)
        {
            throw new NotImplementedException();
        }

        public CharSequence Text
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override void Invalidate ()
        {
            base.Invalidate();
            throw new NotImplementedException();
        }

        private void ComputeSize ()
        {
            throw new NotImplementedException();
        }

        public override void Layout ()
        {
            throw new NotImplementedException();
        }

        public override void Draw (SpriteBatch spriteBatch, float parentAlpha)
        {
            throw new NotImplementedException();
        }

        public override float PrefWidth
        {
            get { throw new NotImplementedException(); }
        }

        public override float PrefHeight
        {
            get { throw new NotImplementedException(); }
        }

        public TextBounds TextBounds
        {
            get { throw new NotImplementedException(); }
            private set { throw new NotImplementedException(); }
        }

        public bool TextWrapping
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Alignment TextAlignment
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void SetFontScale (float fontScale)
        {
            throw new NotImplementedException();
        }

        public void SetFontScale (float fontScaleX, float fontScaleY)
        {
            throw new NotImplementedException();
        }

        public float FontScaleX
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public float FontScaleY
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public static class LabelStyle
        {
            public LabelStyle ()
            {
                throw new NotImplementedException();
            }

            public LabelStyle (BitmapFont font, Color fontColor)
            {
                throw new NotImplementedException();
            }

            public LabelStyle (LabelStyle style)
            {
                throw new NotImplementedException();
            }
        }
    }
}
