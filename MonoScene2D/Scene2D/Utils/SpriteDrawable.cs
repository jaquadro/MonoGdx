using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoScene2D.Geometry;
using MonoScene2D.Graphics.G2D;

namespace MonoScene2D.Scene2D.Utils
{
    public class SpriteDrawable : BaseDrawable
    {
        private Sprite _sprite;

        public SpriteDrawable ()
        { }

        public SpriteDrawable (Sprite sprite)
        {
            Sprite = sprite;
        }

        public SpriteDrawable (SpriteDrawable drawable)
            : base(drawable)
        {
            Sprite = drawable.Sprite;
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float x, float y, float width, float height)
        {
            Sprite.SetBounds(x, y, width, height);

            Color color = Sprite.Color;
            Sprite.Color = ColorExt.Multiply(color, spriteBatch.Color);

            Sprite.Draw(spriteBatch);
            Sprite.Color = color;
        }

        public Sprite Sprite
        {
            get { return _sprite; }
            set
            {
                _sprite = value;

                MinWidth = _sprite.Width;
                MinHeight = _sprite.Height;
            }
        }
    }
}
