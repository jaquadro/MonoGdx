using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGdx.Scene2D;
using MonoGdx.Scene2D.UI;
using NUnit.Framework;

namespace MonoGdxTests.Tests
{
    [TestFixture]
    class TreeTest : GdxTest
    {
        [Test]
        public void Run ()
        {
            using (GdxTestContext context = new GdxTestContext(this)) {
                context.Run();
            }
        }

        private Stage _stage;

        protected override void InitializeCore ()
        {
            _stage = new Stage(Context.GraphicsDevice);
            Context.Input.Processor = _stage;

            Skin skin = new Skin(Context.GraphicsDevice, "Data/uiskin.json");
        }
    }
}
