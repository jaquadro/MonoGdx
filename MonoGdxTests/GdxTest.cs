using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amphibian.Debug;
using Microsoft.Xna.Framework;
using MonoGdx;

namespace MonoGdxTests
{
    public abstract class GdxTest : InputAdapter
    {
        protected GdxTestContext Context { get; private set; }
        protected bool ShowDebug { get; set; }

        public virtual void Create (GdxTestContext context) 
        {
            Context = context;
        }

        public void Initialize ()
        {
            InitializeCore();

            if (ShowDebug) {
                Performance.Initialize(Context);
                ADebug.Initialize(Context);
            }
        }

        protected virtual void InitializeCore ()
        { }

        public void Update (GameTime gameTime)
        {
            if (!ShowDebug) {
                UpdateCore(gameTime);
                return;
            }

            Performance.StartFrame();

            using (new PerformanceRuler("Update", Color.Yellow)) {
                UpdateCore(gameTime);
            }
        }

        protected virtual void UpdateCore (GameTime gameTime)
        { }

        public void Draw (GameTime gameTime)
        {
            if (!ShowDebug) {
                DrawCore(gameTime);
                return;
            }

            using (new PerformanceRuler("Draw", Color.Purple)) {
                DrawCore(gameTime);
            }
        }

        protected virtual void DrawCore (GameTime gameTime)
        { }
    }

    public class GdxTestContext : Game
    {
        GraphicsDeviceManager _graphics;
        GdxTest _test;
        XnaInput _input;

        public GdxTestContext (GdxTest test)
        {
            _test = test;

            _graphics = new GraphicsDeviceManager(this);
            _graphics.SynchronizeWithVerticalRetrace = true;
            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            this.IsFixedTimeStep = false;
            this.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 1000 / 60);

            _test.Create(this);
        }

        public GraphicsDeviceManager Graphics
        {
            get { return _graphics; }
        }

        protected override void Initialize ()
        {
            base.Initialize();

            _input = new XnaInput();
            _input.Processor = _test;

            _test.Initialize();
        }

        protected override void Update (GameTime gameTime)
        {
            _input.Update();
            _input.ProcessEvents();

            _test.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw (GameTime gameTime)
        {
            _test.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
