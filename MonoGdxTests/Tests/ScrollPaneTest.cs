using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGdx.Scene2D;
using MonoGdx.Scene2D.UI;
using MonoGdx.Scene2D.Utils;
using MonoGdx.TableLayout;
using NUnit.Framework;

namespace MonoGdxTests.Tests
{
    [TestFixture]
    public class ScrollPaneTest : GdxTest
    {
        [Test]
        public void Run ()
        {
            using (GdxTestContext context = new GdxTestContext(this)) {
                context.Run();
            }
        }

        private Stage _stage;
        private Table _container;

        protected override void InitializeCore ()
        {
            //Debugger.Launch();
            _stage = new Stage(Context.Window.ClientBounds.Width, Context.Window.ClientBounds.Height, true, Context.GraphicsDevice);
            Skin skin = new Skin(Context.GraphicsDevice, "Data/uiskin.json");
            Context.Input.Processor = _stage;

            _container = new Table();
            _stage.AddActor(_container);
            _container.SetFillParent(true);

            Table table = new Table();

            ScrollPane scroll = new ScrollPane(table, skin);

            InputListener stopTouchDown = new DispatchInputListener() {
                OnTouchDown = (ev, x, y, pointer, button) => {
                    ev.Stop();
                    return false;
                },
            };

            table.Pad(10);
            table.Defaults().Configure.ExpandX().Space(4);

            for (int i = 0; i < 100; i++) {
                table.Row();
                Cell rowCell = table.Add(new Label(i + "uno", skin));
                rowCell.ExpandX = 1;
                rowCell.FillX = 1;

                TextButton button = new TextButton(i + "dos", skin);
                table.Add(button);
                button.AddListener(new DispatchClickListener() {
                    OnClicked = (ev, x, y) => {
                        Console.WriteLine("click " + x + ", " + y);
                    },
                });

                Slider slider = new Slider(0, 100, 1, false, skin);
                slider.AddListener(stopTouchDown);
                table.Add(slider);

                table.Add(new Label(i + "tres long0 long1 long2 long3 long4 long5 long6 long7 long8 long9 long10 long11 long12", skin));
            }

            TextButton flickButton = new TextButton("Flick Scroll", skin.Get<TextButtonStyle>("toggle")) {
                IsToggle = true,
                IsChecked = false,
            };
            flickButton.Checked += (sender, e) => { scroll.FlickScroll = true; };
            flickButton.Unchecked += (sender, e) => { scroll.FlickScroll = false; };

            TextButton fadeButton = new TextButton("Fade Scrollbars", skin.Get<TextButtonStyle>("toggle")) {
                IsToggle = true,
                IsChecked = false,
            };
            fadeButton.Checked += (sender, e) => { scroll.FadeScrollBars = true; };
            fadeButton.Unchecked += (sender, e) => { scroll.FadeScrollBars = false; };

            TextButton smoothButton = new TextButton("Smooth Scrolling", skin.Get<TextButtonStyle>("toggle")) {
                IsToggle = true,
                IsChecked = false,
            };
            smoothButton.Checked += (sender, e) => { scroll.SmoothScrolling = true; };
            smoothButton.Unchecked += (sender, e) => { scroll.SmoothScrolling = false; };

            TextButton onTopButton = new TextButton("Scrollbars On Top", skin.Get<TextButtonStyle>("toggle")) {
                IsToggle = true,
                IsChecked = false,
            };
            onTopButton.Checked += (sender, e) => { scroll.ScrollBarsOnTop = true; };
            onTopButton.Unchecked += (sender, e) => { scroll.ScrollBarsOnTop = false; };

            _container.Add(scroll).Configure.Expand().Fill().Colspan(4);
            _container.Row().Configure.Space(10).PadBottom(10);
            _container.Add(flickButton).Configure.Right().ExpandX();
            _container.Add(onTopButton);
            _container.Add(smoothButton);
            _container.Add(fadeButton).Configure.Left().ExpandX();
        }

        protected override void UpdateCore (GameTime gameTime)
        {
            _stage.Act((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        protected override void DrawCore (GameTime gameTime)
        {
            Context.GraphicsDevice.Clear(Color.Black);
            _stage.Draw();
        }
    }
}
