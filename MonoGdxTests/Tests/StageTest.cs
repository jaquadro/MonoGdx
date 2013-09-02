using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGdx;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D;
using MonoGdx.Scene2D.UI;
using MonoGdx.TableLayout;
using MonoGdx.Utils;
using NUnit.Framework;

namespace MonoGdxTests.Tests
{
    [TestFixture]
    public class StageTest : GdxTest
    {
        [Test]
        public void Run ()
        {
            using (GdxTestContext context = new GdxTestContext(this)) {
                context.Run();
            }
        }

        private const int NumGroups = 5;
        private static int NumSprites = (int)Math.Sqrt(400 / NumGroups);
        private const float Spacing = 5;

        Stage _stage;
        Stage _ui;
        Texture2D _texture;
        Texture2D _uiTexture;
        BitmapFont _font;

        bool _rotateSprites = false;
        bool _scaleSprites = false;
        float _angle;
        List<Image> _images = new List<Image>();
        float _scale = 1;
        float _vScale = 1;
        Label _fps;
        private Random _rand = new Random();

        public override void Initialize ()
        {
            _texture = XnaExt.Texture2D.FromFile(Context.GraphicsDevice, "Data/badlogicsmall.jpg");
            _font = new BitmapFont(Context.GraphicsDevice, "Data/arial-15.fnt", "data/arial-15_00.png", false);

            _stage = new Stage(480, 320, true, Context.GraphicsDevice);

            float loc = (NumSprites * (32 + Spacing) - Spacing) / 2;
            for (int i = 0; i < NumGroups; i++) {
                Group group = new Group() {
                    X = (float)(_rand.NextDouble() * (_stage.Width - NumSprites * (32 + Spacing))),
                    Y = (float)(_rand.NextDouble() * (_stage.Height - NumSprites * (32 + Spacing))),
                    OriginX = loc,
                    OriginY = loc,
                };

                FillGroup(group, _texture);
                _stage.AddActor(group);
            }
        }

        private void FillGroup (Group group, Texture2D texture)
        {
            float advance = 32 + Spacing;
            for (int y = 0; y < NumSprites * advance; y += (int)advance) {
                for (int x = 0; x < NumSprites * advance; x += (int)advance) {
                    Image img = new Image(new TextureRegion(texture)) {
                        Align = Alignment.Center,
                        Scaling = Scaling.None,
                    };
                    img.SetBounds(x, y, 32, 32);
                    img.SetOrigin(16, 16);

                    group.AddActor(img);
                    _images.Add(img);
                }
            }
        }

        public override void Draw (GameTime gameTime)
        {
            Context.GraphicsDevice.Clear(Color.Black);

            if (_rotateSprites) {
                foreach (Actor actor in _stage.Actors)
                    actor.Rotate((float)gameTime.ElapsedGameTime.TotalSeconds * 10);
            }

            _scale += _vScale * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_scale > 1) {
                _scale = 1;
                _vScale = -_vScale;
            }
            if (_scale < .5f) {
                _scale = .5f;
                _vScale = -_vScale;
            }

            foreach (Image img in _images) {
                if (_rotateSprites)
                    img.Rotate(-40 * (float)gameTime.ElapsedGameTime.TotalSeconds);
                else
                    img.Rotation = 0;

                if (_scaleSprites)
                    img.SetScale(_scale);
                else
                    img.SetScale(1);

                img.Invalidate();
            }

            _stage.Draw();
        }
    }
}
