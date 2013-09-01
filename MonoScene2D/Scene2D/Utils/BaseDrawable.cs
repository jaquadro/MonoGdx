using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGdx.Graphics.G2D;

namespace MonoGdx.Scene2D.Utils
{
    public class BaseDrawable : ISceneDrawable
    {
        public BaseDrawable ()
        { }

        public BaseDrawable (ISceneDrawable drawable)
        {
            LeftWidth = drawable.LeftWidth;
            RightWidth = drawable.RightWidth;
            TopHeight = drawable.TopHeight;
            BottomHeight = drawable.BottomHeight;
            MinWidth = drawable.MinWidth;
            MinHeight = drawable.MinHeight;
        }

        public virtual void Draw (GdxSpriteBatch spriteBatch, float x, float y, float width, float height)
        { }

        public float LeftWidth { get; set; }
        public float RightWidth { get; set; }
        public float TopHeight { get; set; }
        public float BottomHeight { get; set; }
        public float MinWidth { get; set; }
        public float MinHeight { get; set; }
    }
}
