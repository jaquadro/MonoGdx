using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGdx.Graphics.G2D;

namespace MonoGdx.Scene2D.Utils
{
    public class NinePatchDrawable : BaseDrawable
    {
        private NinePatch _patch;

        public NinePatchDrawable ()
        { }

        public NinePatchDrawable (NinePatch patch)
        {
            Patch = patch;
        }

        public NinePatchDrawable (NinePatchDrawable drawable)
            : base(drawable)
        {
            Patch = drawable.Patch;
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float x, float y, float width, float height)
        {
            Patch.Draw(spriteBatch, x, y, width, height);
        }

        public NinePatch Patch
        {
            get { return _patch; }
            set
            {
                _patch = value;

                MinWidth = _patch.TotalWidth;
                MinHeight = _patch.TotalHeight;
                TopHeight = _patch.PadTop;
                RightWidth = _patch.PadRight;
                BottomHeight = _patch.PadBottom;
                LeftWidth = _patch.PadLeft;
            }
        }
    }
}
