using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeRulerLibrary;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Amphibian.Debug
{
    public class MemoryTracker : DrawableGameComponent
    {
        // Reference for debug manager.
        private DebugManager debugManager;

        // stringBuilder for tracker draw.
        private StringBuilder stringBuilder = new StringBuilder(32);

        // Stopwatch for sample measuring.
        private Stopwatch stopwatch;

        private WeakReference garbageTracker;

        public MemoryTracker (Game game)
            : base(game)
        {
            SampleSpan = TimeSpan.FromSeconds(1);
            garbageTracker = new WeakReference(new object());
        }

        public int Collections { get; private set; }

        public long ManagedHeapSize { get; private set; }

        public long ManagedHeapDelta { get; private set; }

        /// <summary>
        /// Gets/Sets memory sample duration.
        /// </summary>
        public TimeSpan SampleSpan { get; set; }

        public override void Initialize ()
        {
            // Get debug manager from game service.
            debugManager = Game.Services.GetService(typeof(DebugManager)) as DebugManager;

            if (debugManager == null)
                throw new InvalidOperationException("DebugManaer is not registered.");

            // Register 'fps' command if debug command is registered as a service.
            IDebugCommandHost host = Game.Services.GetService(typeof(IDebugCommandHost)) as IDebugCommandHost;

            if (host != null) {
                host.RegisterCommand("memory", "Memory Tracker", this.CommandExecute);
                Visible = false;
            }

            // Initialize parameters.
            Collections = 0;
            ManagedHeapSize = GC.GetTotalMemory(false);
            ManagedHeapDelta = 0;
            stopwatch = Stopwatch.StartNew();

            stringBuilder.Length = 0;

            base.Initialize();
        }

        /// <summary>
        /// memory command implementation.
        /// </summary>
        private void CommandExecute (IDebugCommandHost host, string command, IList<string> arguments)
        {
            if (arguments.Count == 0)
                Visible = !Visible;

            foreach (string arg in arguments) {
                switch (arg.ToLower()) {
                    case "on":
                        Visible = true;
                        break;
                    case "off":
                        Visible = false;
                        break;
                }
            }
        }

        #region Update and Draw

        public override void Update (GameTime gameTime)
        {
            if (stopwatch.Elapsed > SampleSpan) {
                stopwatch.Reset();
                stopwatch.Start();

                long heapSize = GC.GetTotalMemory(false);
                ManagedHeapDelta = heapSize - ManagedHeapSize;
                ManagedHeapSize = heapSize;

                if (garbageTracker.Target == null) {
                    garbageTracker.Target = new object();
                    Collections++;
                }

                // Update draw string.
                stringBuilder.Length = 0;
                stringBuilder.Append("Heap: ");
                stringBuilder.AppendNumber((int)(ManagedHeapSize / 1024));
                stringBuilder.Append("K");
                stringBuilder.AppendLine();
                stringBuilder.Append("Delta: ");
                stringBuilder.AppendNumber((float)(ManagedHeapDelta / 1024f), 1, AppendNumberOptions.None);
                stringBuilder.Append("K");
                stringBuilder.AppendLine();
                stringBuilder.Append("Collections: ");
                stringBuilder.AppendNumber(Collections);
            }
        }

        public override void Draw (GameTime gameTime)
        {
            SpriteBatch spriteBatch = debugManager.SpriteBatch;
            SpriteFont font = debugManager.DebugFont;

            // Compute size of borader area.
            Vector2 size = font.MeasureString("X");
            Rectangle rc =
                new Rectangle(0, 0, (int)(size.X * 18f), (int)(size.Y * 3.2f));

            Layout layout = new Layout(spriteBatch.GraphicsDevice.Viewport);
            rc = layout.Place(rc, 0.01f, 0.01f, Alignment.TopRight);

            // Place FPS string in borader area.
            size = font.MeasureString(stringBuilder);
            layout.ClientArea = rc;
            Vector2 pos = layout.Place(size, 0.09f, 0.1f, Alignment.TopLeft);

            // Draw
            spriteBatch.Begin();
            spriteBatch.Draw(debugManager.WhiteTexture, rc, new Color(0, 0, 0, 128));
            spriteBatch.DrawString(font, stringBuilder, pos, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion
    }
}
