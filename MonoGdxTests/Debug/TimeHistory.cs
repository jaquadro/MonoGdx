using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TimeRulerLibrary;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
//using LilyPath;

namespace Amphibian.Debug
{
    /// <summary>
    /// Component for FPS measure and draw.
    /// </summary>
    public class TimeHistory : DrawableGameComponent
    {
        #region Fields

        // Reference for debug manager.
        private DebugManager debugManager;

        // stringBuilder for FPS counter draw.
        private StringBuilder stringBuilder = new StringBuilder(16);

        private TimeRuler _ruler;

        private const int _historyCount = 640;

        private int _graphIndex;

        private VertexPositionColor[][][] _history;
        private Vector2[][][] _history2;

        private VertexPositionColor[] _total;
        private Vector2[] _total2;

        //private DrawBatch _drawBatch;

        #endregion

        #region Initialize

        public TimeHistory (Game game, TimeRuler ruler)
            : base(game)
        {
            _ruler = ruler;
        }

        public override void Initialize ()
        {
            // Get debug manager from game service.
            debugManager =
                Game.Services.GetService(typeof(DebugManager)) as DebugManager;

            if (debugManager == null)
                throw new InvalidOperationException("DebugManaer is not registered.");

            // Register 'fps' command if debug command is registered as a service.
            IDebugCommandHost host =
                                Game.Services.GetService(typeof(IDebugCommandHost))
                                                                as IDebugCommandHost;

            if (host != null) {
                host.RegisterCommand("th", "Time History", this.CommandExecute);
                Visible = false;
            }

            // Initialize parameters.
            stringBuilder.Length = 0;

            InitializeHistory();

            base.Initialize();
        }

        private void InitializeHistory ()
        {
            if (_ruler.Frame == null)
                return;

            int bars = _ruler.Frame.Bars.Length;
            _history = new VertexPositionColor[bars][][];
            _history2 = new Vector2[bars][][];

            for (int i = 0; i < bars; i++) {
                int samples = _ruler.Frame.Bars[i].Markers.Length;
                _history[i] = new VertexPositionColor[samples][];
                _history2[i] = new Vector2[samples][];
            }
        }

        #endregion

        /// <summary>
        /// fps command implementation.
        /// </summary>
        private void CommandExecute (IDebugCommandHost host,
                                    string command, IList<string> arguments)
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

        private VertexPositionColor[] InitializeHistory (Vector2 origin, float xStride, Color c)
        {
            VertexPositionColor[] vec = new VertexPositionColor[_historyCount];
            for (int i = 0; i < _historyCount; i++) {
                vec[i] = new VertexPositionColor(new Vector3(origin.X + i * xStride, origin.Y, 0), c);
            }

            return vec;
        }

        private Vector2[] InitializeHistory2 (Vector2 origin, float xStride)
        {
            Vector2[] vec = new Vector2[_historyCount];
            for (int i = 0; i < _historyCount; i++) {
                vec[i] = new Vector2(origin.X + i * xStride, origin.Y);
            }

            return vec;
        }

        #region Update and Draw

        public override void Update (GameTime gameTime)
        {
            // Update draw string.
            stringBuilder.Length = 0;

            if (_history == null) {
                InitializeHistory();
                return;
            }

            SpriteFont font = debugManager.DebugFont;

            // Compute command window size and draw.
            float w = GraphicsDevice.Viewport.Width;
            float h = GraphicsDevice.Viewport.Height;
            float topMargin = h * 0.1f;
            float leftMargin = w * 0.1f;

            Rectangle rect = new Rectangle();
            rect.X = (int)leftMargin;
            rect.Y = (int)topMargin;
            rect.Width = (int)(w * 0.8f);
            rect.Height = (int)(20 * font.LineSpacing);

            float sampleStride = rect.Width / (float)_historyCount;

            const float frameSpan = 1.0f / 60.0f * 1000f;
            float scaleY = rect.Height * 1f / frameSpan;

            if (_total == null)
                _total = InitializeHistory(new Vector2(rect.Left, rect.Bottom), sampleStride, Color.White * 0.33f);
            if (_total2 == null)
                _total2 = InitializeHistory2(new Vector2(rect.Left, rect.Bottom), sampleStride);

            int bars = _ruler.Frame.Bars.Length;

            float accum = 0;
            for (int i = 0; i < bars; i++) {
                for (int j = 0; j < _ruler.Frame.Bars[i].MarkCount; j++) {
                    float time = _ruler.Frame.Bars[i].Markers[j].EndTime - _ruler.Frame.Bars[i].Markers[j].BeginTime;
                    accum += time;

                    Color c = _ruler.Frame.Bars[i].Markers[j].Color;

                    if (_history[i][j] == null)
                        _history[i][j] = InitializeHistory(new Vector2(rect.Left, rect.Bottom), sampleStride, c);
                    if (_history2[i][j] == null)
                        _history2[i][j] = InitializeHistory2(new Vector2(rect.Left, rect.Bottom), sampleStride);
                    _history[i][j][_graphIndex] = new VertexPositionColor(new Vector3(rect.X + _graphIndex * sampleStride, rect.Bottom - time * scaleY, 0), c);
                    _history2[i][j][_graphIndex] = new Vector2(rect.X + _graphIndex * sampleStride, rect.Bottom - time * scaleY);
                }
            }

            _total[_graphIndex] = new VertexPositionColor(new Vector3(rect.X + _graphIndex * sampleStride, rect.Bottom - accum * scaleY, 0), Color.White * .33f);
            _total2[_graphIndex] = new Vector2(rect.X + _graphIndex * sampleStride, rect.Bottom - accum * scaleY);

            _graphIndex++;
            if (_graphIndex >= _historyCount)
                _graphIndex = 0;

            base.Update(gameTime);
        }

        public override void Draw (GameTime gameTime)
        {
            if (_history == null) {
                InitializeHistory();
                return;
            }

            SpriteFont font = debugManager.DebugFont;
            SpriteBatch spriteBatch = debugManager.SpriteBatch;
            Texture2D whiteTexture = debugManager.WhiteTexture;
            //Effect solidEffect = debugManager.SolidColorEffect;

            // Compute command window size and draw.
            float w = GraphicsDevice.Viewport.Width;
            float h = GraphicsDevice.Viewport.Height;
            float topMargin = h * 0.1f;
            float leftMargin = w * 0.1f;

            Rectangle rect = new Rectangle();
            rect.X = (int)leftMargin;
            rect.Y = (int)topMargin;
            rect.Width = (int)(w * 0.8f);
            rect.Height = (int)(20 * font.LineSpacing);

            float sampleStride = rect.Width / (float)_historyCount;

            Matrix mtx = Matrix.CreateTranslation(
                        new Vector3(0, -rect.Height * (1.0f - 1.0f), 0));

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, mtx);

            spriteBatch.Draw(whiteTexture, rect, new Color(0, 0, 0, 200));

            spriteBatch.Draw(whiteTexture, new Rectangle(rect.X + (int)(_graphIndex * sampleStride), rect.Top, 1, rect.Height), Color.White);

            spriteBatch.End();

            Viewport viewport = spriteBatch.GraphicsDevice.Viewport;
            float nw = (viewport.Width > 0)
                ? 1f / viewport.Width : 0;
            float nh = (viewport.Height > 0)
                ? -1f / viewport.Height : 0;
            Matrix mat = new Matrix
            {
                M11 = nw * 2f,
                M22 = nh * 2f,
                M33 = 1f,
                M44 = 1f,
                M41 = -1f - nh,
                M42 = 1f - nw,
            };

            //solidEffect.Parameters["MatrixTransform"].SetValue(mtx * mat);

            //debugManager.BasicEffect.TextureEnabled = false;
            debugManager.BasicEffect.VertexColorEnabled = true;
            debugManager.BasicEffect.World = mtx * mat;

            const float frameSpan = 1.0f / 60.0f * 1000f;
            float scaleY = rect.Height * 1f / frameSpan;

            int bars = _ruler.Frame.Bars.Length;

            for (int i = 0; i < bars; i++) {
                for (int j = 0; j < _ruler.Frame.Bars[i].MarkCount; j++) {
                    float time = _ruler.Frame.Bars[i].Markers[j].EndTime - _ruler.Frame.Bars[i].Markers[j].BeginTime;

                    Color c = _ruler.Frame.Bars[i].Markers[j].Color;

                    if (_history[i][j] == null)
                        _history[i][j] = InitializeHistory(new Vector2(rect.Left, rect.Bottom), sampleStride, c);
                    _history[i][j][_graphIndex] = new VertexPositionColor(new Vector3(rect.X + _graphIndex * sampleStride, rect.Bottom - time * scaleY, 0), c);

                    //solidEffect.Parameters["solidColor"].SetValue(new Vector4(c.R, c.G, c.B, 1));
                    //solidEffect.CurrentTechnique.Passes[0].Apply();
                    debugManager.BasicEffect.CurrentTechnique.Passes[0].Apply();

                    spriteBatch.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, _history[i][j], 0, _historyCount - 1);

                    //_drawBatch.DrawPrimitivePath(_history2[i][j], Pens.White);

                    /*for (int seg = 1; seg < _historyCount; seg++) {
                        Vector3 pos1 = _history[i][j][seg - 1].Position;
                        Vector3 pos2 = _history[i][j][seg].Position;

                        _drawBatch.DrawPrimitiveLine(new Point((int)pos1.X, (int)pos1.Y), new Point((int)pos2.X, (int)pos2.Y), Pens.White);
                    }*/
                }
            }

            //solidEffect.Parameters["solidColor"].SetValue(new Vector4(1, 1, 1, .33f));
            //solidEffect.CurrentTechnique.Passes[0].Apply();

            spriteBatch.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, _total, 0, _historyCount - 1);
        }

        #endregion

    }

    public struct VertexPosition : IVertexType
    {
        public Vector3 Position;

        public VertexPosition (Vector3 position)
        {
            Position = position;
        }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
            );

        #region IVertexType Members

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        #endregion
    }
}
