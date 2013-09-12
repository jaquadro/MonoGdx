using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D;
using MonoGdx.Scene2D.UI;
using MonoGdx.Scene2D.Utils;
using MonoGdx.TableLayout;
using NUnit.Framework;

namespace MonoGdxTests.Tests
{
    [TestFixture]
    public class UITest : GdxTest
    {
        [Test]
        public void Run ()
        {
            using (GdxTestContext context = new GdxTestContext(this)) {
                context.Run();
            }
        }

        string[] listEntries = {"This is a list entry", "And another one", "The meaning of life", "Is hard to come by",
		    "This is a list entry", "And another one", "The meaning of life", "Is hard to come by", "This is a list entry",
		    "And another one", "The meaning of life", "Is hard to come by", "This is a list entry", "And another one",
		    "The meaning of life", "Is hard to come by", "This is a list entry", "And another one", "The meaning of life",
		    "Is hard to come by"};

        string[] selectEntries = { "Android", "Windows", "Linux", "OSX","Android", "Windows", "Linux", "OSX", 
            "Android", "Windows", "Linux", "OSX","Android", "Windows", "Linux", "OSX","Android", "Windows", 
            "Linux", "OSX","Android", "Windows", "Linux", "OSX","Android", "Windows", "Linux", "OSX"};


        private Stage _stage;
        private Skin _skin;
        private GdxSpriteBatch _spriteBatch;
        private TextureContext _texture1;
        private TextureContext _texture2;
        private Label _fpsLabel;

        protected override void InitializeCore ()
        {
            ShowDebug = true;

            Debugger.Launch();

            _spriteBatch = new GdxSpriteBatch(Context.GraphicsDevice);
            _skin = new Skin(Context.GraphicsDevice, "Data/uiskin.json");
            _texture1 = new TextureContext(Context.GraphicsDevice, "Data/badlogicsmall.jpg", true);
            _texture2 = new TextureContext(Context.GraphicsDevice, "Data/badlogic.jpg", true);

            TextureRegion image = new TextureRegion(_texture1);
            TextureRegion imageFlipped = new TextureRegion(image);
            imageFlipped.Flip(true, true);
            TextureRegion image2 = new TextureRegion(_texture2);
            _stage = new Stage(Context.GraphicsDevice.Viewport.Width, Context.GraphicsDevice.Viewport.Height, true, Context.GraphicsDevice);

            Context.Input.Processor = _stage;

            ImageButtonStyle style = new ImageButtonStyle(_skin.Get<ButtonStyle>()) {
                ImageUp = new TextureRegionDrawable(image),
                ImageDown = new TextureRegionDrawable(imageFlipped),
            };
            ImageButton iconButton = new ImageButton(style);

            Button buttonMulti = new TextButton("Multi\nLine\nToggle", _skin, "toggle");
            Button imgButton = new Button(new Image(image), _skin);
            Button imgToggleButton = new Button(new Image(image), _skin, "toggle");

            Label myLabel = new Label("This is some text.", _skin);
            myLabel.TextWrapping = true;

            Table t = new Table();
            t.Row();
            t.Add(myLabel);

            t.Layout();

            CheckBox checkbox = new CheckBox("Check me", _skin);
            Slider slider = new Slider(0, 10, 1, false, _skin);
            // TextField
            SelectBox dropdown = new SelectBox(selectEntries, _skin);
            Image imageActor = new Image(image2);
            ScrollPane scrollPane = new ScrollPane(imageActor);
            MonoGdx.Scene2D.UI.List list = new MonoGdx.Scene2D.UI.List(listEntries, _skin);
            ScrollPane scrollPane2 = new ScrollPane(list, _skin);
            //scrollPane2.FlickScroll = false;
            SplitPane splitPane = new SplitPane(scrollPane, scrollPane2, false, _skin, "default-horizontal");
            _fpsLabel = new Label("fps:", _skin);

            Label passwordLabel = new Label("Textfield in password mode: ", _skin);
            // TextField

            Window window = new Window("Dialog", _skin);
            window.SetPosition(0, 0);
            window.Defaults().SpaceBottom = 10;
            window.Row().Fill().ExpandX = 1;
            window.Add(iconButton);
            window.Add(buttonMulti);
            window.Add(imgButton);
            window.Add(imgToggleButton);
            window.Row();
            window.Add(checkbox);
            Cell cell = window.Add(slider);
            cell.MinWidth = 100;
            cell.FillX = 1;
            cell.Colspan = 3;
            window.Row();
            cell = window.Add(dropdown);
            cell.MinWidth = 100;
            cell.FillX = 1;
            cell.Colspan = 4;
            window.Row();
            cell = window.Add(splitPane);
            cell.Fill();
            cell.Expand();
            cell.Colspan = 4;
            cell.MaxHeight = 200;

            window.Pack();


            _stage.AddActor(window);


        }

        protected override void UpdateCore (Microsoft.Xna.Framework.GameTime gameTime)
        {
            _stage.Act((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        protected override void DrawCore (Microsoft.Xna.Framework.GameTime gameTime)
        {
            Context.GraphicsDevice.Clear(Color.Black);
            _stage.Draw();
        }
    }
}
