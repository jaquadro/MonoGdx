using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoScene2D.Graphics.G2D;

namespace MonoScene2D.Scene2D.Utils
{
    public class TextureRegionDrawable : BaseDrawable
    {
        private TextureRegion _region;

        public TextureRegionDrawable ()
        { }

        public TextureRegionDrawable (TextureRegion region)
        {
            Region = region;
        }

        public TextureRegionDrawable (TextureRegionDrawable drawable)
            : base(drawable)
        {
            Region = drawable.Region;
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float x, float y, float width, float height)
        {
            spriteBatch.Draw(Region, x, y, width, height);
        }

        public TextureRegion Region
        {
            get { return _region; }
            set
            {
                _region = value;

                MinWidth = _region.RegionWidth;
                MinHeight = _region.RegionHeight;
            }
        }
    }
}
