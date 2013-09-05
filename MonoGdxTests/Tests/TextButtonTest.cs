using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D;
using MonoGdx.Scene2D.UI;
using NUnit.Framework;

namespace MonoGdxTests.Tests
{
    [TestFixture]
    public class TextButtonTest : GdxTest
    {
        [Test]
        public void Run ()
        {
            using (GdxTestContext context = new GdxTestContext(this)) {
                context.Run();
            }
        }

        private Stage _stage;
        private Random _rand = new Random();

        protected override void InitializeCore ()
        {
            ShowDebug = true;

            Skin skin = new Skin(new TextureAtlas(Context.GraphicsDevice, "Data/uiskin.atlas"));
            skin.Add("white", Color.White);
            skin.Add("red", Color.Red);
            skin.Add("default-font", new BitmapFont(Context.GraphicsDevice, "Data/default.fnt", false));
            skin.Add("default", new TextButtonStyle() {
                Down = skin.GetDrawable("default-round-down"),
                Up = skin.GetDrawable("default-round"),
                Font = skin.GetFont("default-font"),
                FontColor = skin.GetColor("white"),
            });
            _stage = new Stage(Context.Window.ClientBounds.Width, Context.Window.ClientBounds.Height, false, Context.GraphicsDevice);

            Context.Input.Processor = _stage;

            TextButton button = new TextButton("Button " + 0, skin) {
                X = 200, Y = 200, Width = 150, Height = 100,
                /*X = _rand.Next(0, Context.GraphicsDevice.Viewport.Width - 200),
                Y = _rand.Next(0, Context.GraphicsDevice.Viewport.Height - 100),
                Width = _rand.Next(50, 200),
                Height = _rand.Next(0, 100),*/
            };
            _stage.AddActor(button);

            Context.Window.ClientSizeChanged += (s, e) => {
                _stage.SetViewport(Context.Window.ClientBounds.Width, Context.Window.ClientBounds.Height, false);
            };
        }

        protected override void DrawCore (GameTime gameTime)
        {
            Context.GraphicsDevice.Clear(Color.Black);
            _stage.Draw();
        }
    }
}
