using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoGdxTests
{
    public abstract class GdxTest
    {
        protected GdxTestContext Context { get; private set; }

        public virtual void Create (GdxTestContext context) 
        {
            Context = context;
        }

        public virtual void Initialize ()
        { }

        public virtual void Update (GameTime gameTime)
        { }

        public virtual void Draw (GameTime gameTime)
        { }
    }

    public class GdxTestContext : Game
    {
        GraphicsDeviceManager _graphics;
        GdxTest _test;

        public GdxTestContext (GdxTest test)
        {
            _test = test;

            _graphics = new GraphicsDeviceManager(this);
            _graphics.SynchronizeWithVerticalRetrace = true;
            Content.RootDirectory = "Content";

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
            _test.Initialize();
        }

        protected override void Update (GameTime gameTime)
        {
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
