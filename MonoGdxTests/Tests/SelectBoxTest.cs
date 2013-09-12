using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGdx.Scene2D;
using MonoGdx.Scene2D.UI;
using NUnit.Framework;

namespace MonoGdxTests.Tests
{
    [TestFixture]
    public class SelectBoxTest : GdxTest
    {
        [Test]
        public void Run ()
        {
            using (GdxTestContext context = new GdxTestContext(this)) {
                context.Run();
            }
        }

        private string[] selectItems = new string[] { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten" };

        private Stage stage;

        protected override void InitializeCore ()
        {
            ShowDebug = true;

            //Debugger.Launch();

            stage = new Stage(Context.GraphicsDevice.Viewport.Width, Context.GraphicsDevice.Viewport.Height, true, Context.GraphicsDevice);

            Skin skin = new Skin(Context.GraphicsDevice, "Data/uiskin.json");
            Context.Input.Processor = stage;

            SelectBox box = new SelectBox(selectItems, skin);
            box.SetPosition(200, 300);

            stage.AddActor(box);
        }

        protected override void UpdateCore (Microsoft.Xna.Framework.GameTime gameTime)
        {
            stage.Act((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        protected override void DrawCore (Microsoft.Xna.Framework.GameTime gameTime)
        {
            Context.GraphicsDevice.Clear(Color.DarkGray);

            stage.Draw();
        }
    }
}
