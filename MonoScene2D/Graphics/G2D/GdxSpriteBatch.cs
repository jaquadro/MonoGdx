using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace MonoGdx.Graphics.G2D
{
    public class GdxSpriteBatch : IDisposable
    {
        private class LocalSpriteEffect : SpriteEffect
        {
            public LocalSpriteEffect (GraphicsDevice device)
                : base(device)
            { }

            public LocalSpriteEffect (LocalSpriteEffect cloneSource)
                : base(cloneSource)
            { }

            protected override bool OnApply ()
            {
                return false;
            }
        }

        private const int DefaultBatchSize = 256;

        private VertexPositionColorTexture[] _vertices = new VertexPositionColorTexture[DefaultBatchSize * 4];
        private short[] _indexes = new short[DefaultBatchSize * 6];

        private int _vBufferIndex = 0;

        private Matrix _projectionMatrix;
        private Matrix _transformMatrix;
        private Matrix _combinedMatrix;

        private Effect _spriteEffect;
        private readonly EffectParameter _matrixTransform;
        private readonly EffectPass _spritePass;

        private GraphicsDevice _device;
        private Texture2D _lastTexture;

        private bool _inBegin = false;
        private bool _disposed = false;

        public GdxSpriteBatch (GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");

            _device = graphicsDevice;

            _spriteEffect = new LocalSpriteEffect(graphicsDevice);
            _matrixTransform = _spriteEffect.Parameters["MatrixTransform"];
            _spritePass = _spriteEffect.CurrentTechnique.Passes[0];

            _transformMatrix = Matrix.Identity;
            //_projectionMatrix = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0, 0, 1);
            _projectionMatrix = XnaExt.Matrix.CreateOrthographic2D(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

            Color = Color.White;

            CalculateIndexBuffer();

            // projection uses CreateOrthographicOffCenter to create 2d projection
            // matrix with 0,0 in the upper left.
            /*_basicEffect.Projection = Matrix.CreateOrthographicOffCenter
                (0, graphicsDevice.Viewport.Width,
                graphicsDevice.Viewport.Height, 0,
                0, 1);
            this._basicEffect.World = Matrix.Identity;
            this._basicEffect.View = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward,
                Vector3.Up);*/
        }

        public void Dispose ()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (disposing && !_disposed) {
                if (_spriteEffect != null)
                    _spriteEffect.Dispose();

                _disposed = true;
            }
        }

        public GraphicsDevice GraphicsDevice
        {
            get { return _device; }
        }

        private Matrix DefaultProjectionMatrix
        {
            //get { return Matrix.CreateOrthographicOffCenter(0, _device.Viewport.Width, _device.Viewport.Height, 0, 0, 1); }
            get { return XnaExt.Matrix.CreateOrthographic2D(0, 0, _device.Viewport.Width, _device.Viewport.Height); }
        }

        public void Begin ()
        {
            Begin(DefaultProjectionMatrix, Matrix.Identity);
        }

        public void Begin (Matrix projectionMatrix, Matrix transformMatrix)
        {
            if (_inBegin)
                throw new InvalidOperationException("End must be called before Begin can be called again.");

            _projectionMatrix = projectionMatrix;
            _transformMatrix = transformMatrix;

            SetupMatrix();

            _inBegin = true;
        }

        public void End ()
        {
            if (!_inBegin) {
                throw new InvalidOperationException("Begin must be called before End can be called.");
            }

            Flush();

            _inBegin = false;
        }

        private void Setup ()
        {
            _device.SamplerStates[0] = SamplerState.PointClamp;
            //_device.BlendState = BlendState.NonPremultiplied;
            //_device.RasterizerState = new RasterizerState() { CullMode = Microsoft.Xna.Framework.Graphics.CullMode.None };
            _matrixTransform.SetValue(_combinedMatrix);
            _spritePass.Apply();
        }

        private void SetupMatrix ()
        {
            //Matrix.Multiply(ref _projectionMatrix, ref _transformMatrix, out _combinedMatrix);
            Matrix.Multiply(ref _transformMatrix, ref _projectionMatrix, out _combinedMatrix);
        }

        public Matrix ProjectionMatrix
        {
            get { return _projectionMatrix; }
            set
            {
                if (_inBegin)
                    Flush();
                _projectionMatrix = value;
                SetupMatrix();
            }
        }

        public Matrix TransformMatrix
        {
            get { return _transformMatrix; }
            set
            {
                if (_inBegin)
                    Flush();
                _transformMatrix = value;
                SetupMatrix();
            }
        }

        public Color Color { get; set; }

        private void CheckValid (Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            if (!_inBegin)
                throw new InvalidOperationException("Draw was called, but Begin has not yet been called.");
        }

        public void Draw (Texture2D texture, VertexPositionColorTexture[] vertices, int offset, int count)
        {
            CheckValid(texture);

            if (count % 4 != 0)
                throw new ArgumentException("Vertices must be provided in multiples of 4");

            if (texture != _lastTexture)
                SwitchTexture(texture);

            while (count > 0) {
                if (_vertices.Length - _vBufferIndex < count)
                    Flush();

                int copyCount = Math.Min(_vertices.Length, count);
                Array.Copy(vertices, offset, _vertices, _vBufferIndex, copyCount);

                _vBufferIndex += copyCount;
                count -= copyCount;

                if (count == 0)
                    break;
            }
        }

        public void Draw (Texture2D texture, float x, float y, float width, float height, float u, float v, float u2, float v2)
        {
            CheckValid(texture);

            if (texture != _lastTexture)
                SwitchTexture(texture);

            if (_vertices.Length - _vBufferIndex < 4)
                Flush();

            float fx2 = x + width;
            float fy2 = y + height;

            _vertices[_vBufferIndex + 0] = new VertexPositionColorTexture(new Vector3(x, y, 0), Color, new Vector2(u, v));
            _vertices[_vBufferIndex + 1] = new VertexPositionColorTexture(new Vector3(x, fy2, 0), Color, new Vector2(u, v2));
            _vertices[_vBufferIndex + 2] = new VertexPositionColorTexture(new Vector3(fx2, fy2, 0), Color, new Vector2(u2, v2));
            _vertices[_vBufferIndex + 3] = new VertexPositionColorTexture(new Vector3(fx2, y, 0), Color, new Vector2(u2, v));

            _vBufferIndex += 4;
        }

        public void Draw (TextureRegion region, float x, float y)
        {
            Draw(region, x, y, region.RegionWidth, region.RegionHeight);
        }

        public void Draw (TextureRegion region, float x, float y, float width, float height)
        {
            CheckValid(region.Texture);

            if (region.Texture != _lastTexture)
                SwitchTexture(region.Texture);

            if (_vertices.Length - _vBufferIndex < 4)
                Flush();

            float fx2 = x + width;
            float fy2 = y + height;
            float u = region.U;
            float v = region.V;
            float u2 = region.U2;
            float v2 = region.V2;

            _vertices[_vBufferIndex + 0] = new VertexPositionColorTexture(new Vector3(x, y, 0), Color, new Vector2(u, v));
            _vertices[_vBufferIndex + 1] = new VertexPositionColorTexture(new Vector3(x, fy2, 0), Color, new Vector2(u, v2));
            _vertices[_vBufferIndex + 2] = new VertexPositionColorTexture(new Vector3(fx2, fy2, 0), Color, new Vector2(u2, v2));
            _vertices[_vBufferIndex + 3] = new VertexPositionColorTexture(new Vector3(fx2, y, 0), Color, new Vector2(u2, v));

            _vBufferIndex += 4;
        }

        public void Draw (TextureRegion region, float x, float y, float originX, float originY, float width, float height, float scaleX, float scaleY, float rotation)
        {
            CheckValid(region.Texture);

            if (region.Texture != _lastTexture)
                SwitchTexture(region.Texture);

            if (_vertices.Length - _vBufferIndex < 4)
                Flush();

            float worldOriginX = x + originX;
            float worldOriginY = y + originY;
            float fx = -originX;
            float fy = -originY;
            float fx2 = width - originX;
            float fy2 = height - originY;

            if (scaleX != 1 || scaleY != 1) {
                fx *= scaleX;
                fy *= scaleY;
                fx2 *= scaleX;
                fy2 *= scaleY;
            }

            float x1, y1;
            float x2, y2;
            float x3, y3;
            float x4, y4;

            if (rotation != 0) {
                float cos = (float)Math.Cos(rotation);
                float sin = (float)Math.Sin(rotation);

                x1 = cos * fx - sin * fy;
                y1 = sin * fx + cos * fy;

                x2 = cos * fx - sin * fy2;
                y2 = sin * fx + cos * fy2;

                x3 = cos * fx2 - sin * fy2;
                y3 = sin * fx2 + cos * fy2;

                x4 = cos * fx2 - sin * fy;
                y4 = sin * fx2 + cos * fy;
            }
            else {
                x1 = fx;
                y1 = fy;

                x2 = fx;
                y2 = fy2;

                x3 = fx2;
                y3 = fy2;

                x4 = fx2;
                y4 = fy;
            }

            x1 += worldOriginX;
            y1 += worldOriginY;
            x2 += worldOriginX;
            y2 += worldOriginY;
            x3 += worldOriginX;
            y3 += worldOriginY;
            x4 += worldOriginX;
            y4 += worldOriginY;

            float u = region.U;
            float v = region.V2;
            float u2 = region.U2;
            float v2 = region.V;

            _vertices[_vBufferIndex + 0] = new VertexPositionColorTexture(new Vector3(x1, y1, 0), Color, new Vector2(u, v));
            _vertices[_vBufferIndex + 1] = new VertexPositionColorTexture(new Vector3(x2, y2, 0), Color, new Vector2(u, v2));
            _vertices[_vBufferIndex + 2] = new VertexPositionColorTexture(new Vector3(x3, y3, 0), Color, new Vector2(u2, v2));
            _vertices[_vBufferIndex + 3] = new VertexPositionColorTexture(new Vector3(x4, y4, 0), Color, new Vector2(u2, v));

            _vBufferIndex += 4;
        }

        public void Flush ()
        {
            if (!_inBegin)
                throw new InvalidOperationException("Begin must be called before Flush can be called.");

            if (_vBufferIndex == 0)
                return;

            Setup();

            _device.Textures[0] = _lastTexture;

            int primitiveCount = _vBufferIndex / 2;

            //_device.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, _vertices, 0, primitiveCount);
            _device.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, _vertices, 0, _vBufferIndex, _indexes, 0, primitiveCount);

            _vBufferIndex = 0;
        }

        private void CalculateIndexBuffer ()
        {
            for (short i = 0; i < DefaultBatchSize; i++) {
                short vIndex = (short)(i * 4);
                short iIndex = (short)(i * 6);

                _indexes[iIndex + 0] = (short)(vIndex + 0);
                _indexes[iIndex + 1] = (short)(vIndex + 1);
                _indexes[iIndex + 2] = (short)(vIndex + 2);
                _indexes[iIndex + 3] = (short)(vIndex + 2);
                _indexes[iIndex + 4] = (short)(vIndex + 3);
                _indexes[iIndex + 5] = (short)(vIndex + 0);
            }
        }

        private void SwitchTexture (Texture2D texture)
        {
            Flush();

            _lastTexture = texture;
        }
    }
}