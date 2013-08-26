using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoScene2D.Graphics.G2D;

namespace MonoScene2D.Scene2D.Utils
{
    public class TiledDrawable : TextureRegionDrawable
    {
        public TiledDrawable ()
        { }

        public TiledDrawable (TextureRegion region)
            : base(region)
        { }

        public TiledDrawable (TextureRegionDrawable drawable)
            : base(drawable)
        { }

        public override void Draw (GdxSpriteBatch spriteBatch, float x, float y, float width, float height)
        {
            TextureRegion region = Region;

            float regionWidth = region.RegionWidth;
            float regionHeight = region.RegionHeight;
            float remainingX = width % regionWidth;
            float remainingY = height % regionHeight;

            float startX = x;
            float startY = y;
            float endX = x + width - remainingX;
            float endY = y + height - remainingY;

            while (x < endX) {
                y = startY;
                while (y < endY) {
                    spriteBatch.Draw(region, x, y, regionWidth, regionHeight);
                    y += regionHeight;
                }
                x += regionWidth;
            }

            Texture2D texture = region.Texture;
            float u = region.U;
            float v2 = region.V2;

            if (remainingX > 0) {
                float u2 = u + remainingX / texture.Width;
                float v = region.V;

                y = startY;
                while (y < endY) {
                    spriteBatch.Draw(texture, x, y, remainingX, regionHeight, u, v2, u2, v);
                    y += regionHeight;
                }

                if (remainingY > 0) {
                    v = v2 - remainingY / texture.Height;
                    spriteBatch.Draw(texture, x, y, remainingX, remainingY, u, v2, u2, v);
                }
            }

            if (remainingY > 0) {
                float u2 = region.U2;
                float v = v2 - remainingY / texture.Height;

                x = startX;
                while (x < endX) {
                    spriteBatch.Draw(texture, x, y, regionWidth, remainingY, u, v2, u2, v);
                    x += regionWidth;
                }
            }
        }
    }
}
