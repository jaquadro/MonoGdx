using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGdx.Graphics.G2D;
using NUnit.Framework;

namespace MonoGdxTests.Tests
{
    [TestFixture]
    public class BitmapFontTest : GdxTest
    {
        [Test]
        public void Run ()
        {
            using (GdxTestContext context = new GdxTestContext(this)) {
                context.Run();
            }
        }

        private GdxSpriteBatch _batch;
        private BitmapFont _font;

        public override void Initialize ()
        {
            string fontFile = "verdana39.fnt";
            string imageFile = "verdana39.png";

            _batch = new GdxSpriteBatch(Context.GraphicsDevice);
            _font = new BitmapFont(Context.GraphicsDevice, fontFile, imageFile, false);
        }

        public override void Draw (GameTime gameTime)
        {
            Context.GraphicsDevice.Clear(Color.Black);

            _batch.Begin();

            string text = "Sphinx of black quartz, judge my vow.";
            float x = 100;
            float y = 20;
            float alignmentWidth = 280;

            _font.DrawWrapped(_batch, text, x, Context.GraphicsDevice.Viewport.Height - y, alignmentWidth, HAlignment.Right);

            _batch.End();
        }
    }
}
