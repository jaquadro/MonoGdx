using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoGdx.Graphics.G2D;

namespace MonoGdx.Scene2D.Utils
{
    public interface ISceneDrawable
    {
        void Draw (GdxSpriteBatch spriteBatch, float x, float y, float width, float height);

        float LeftWidth { get; set; }
        float RightWidth { get; set; }
        float TopHeight { get; set; }
        float BottomHeight { get; set; }
        float MinWidth { get; set; }
        float MinHeight { get; set; }
    }
}
