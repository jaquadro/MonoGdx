﻿/**
 * Copyright 2013 See AUTHORS file.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGdx.Graphics.G2D
{
    public class TextureContext : IDisposable
    {
        private static Dictionary<int, SamplerState> _samplerCache = new Dictionary<int, SamplerState>();

        private Texture2D _texture;
        private TextureFilter _filter = TextureFilter.Point;
        private TextureAddressMode _wrapU = TextureAddressMode.Clamp;
        private TextureAddressMode _wrapV = TextureAddressMode.Clamp;
        private SamplerState _samplerState;

        public TextureContext (Texture2D texture)
        {
            _texture = texture;
        }

        public TextureContext (GraphicsDevice graphicsDevice, Stream stream, bool premultiplyAlpha)
        {
            _texture = Texture2D.FromStream(graphicsDevice, stream);

            if (premultiplyAlpha)
                PremultiplyTexture(_texture);
        }

        public TextureContext (GraphicsDevice graphicsDevice, string file, bool premultiplyAlpha)
        {
            using (FileStream fs = File.OpenRead(file)) {
                _texture = Texture2D.FromStream(graphicsDevice, fs);
            }

            if (premultiplyAlpha)
                PremultiplyTexture(_texture);
        }

        public TextureContext (ContentManager contentManager, string assetName, bool premultiplyAlpha)
        {
            _texture = contentManager.Load<Texture2D>(assetName);

            if (premultiplyAlpha)
                PremultiplyTexture(_texture);
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (disposing) {
                if (_texture != null)
                    _texture.Dispose();
            }
        }

        private static void PremultiplyTexture (Texture2D tex)
        {
            byte[] data = new byte[tex.Width * tex.Height * 4];
            tex.GetData(data);

            for (int i = 0; i < data.Length; i += 4) {
                float a = data[i + 3] / 255f;
                data[i + 0] = (byte)(data[i + 0] * a);
                data[i + 1] = (byte)(data[i + 1] * a);
                data[i + 2] = (byte)(data[i + 2] * a);
                //data[i + 3] = (byte)(data[i + 3] * a);
            }

            tex.SetData(data);
        }

        public Texture2D Texture 
        { 
            get { return _texture; }
            set { _texture = value; }
        }

        public TextureFilter Filter
        {
            get { return _filter; }
            set
            {
                if (_filter != value) {
                    _filter = value;
                    _samplerState = null;
                }
            }
        }

        public TextureAddressMode WrapU
        {
            get { return _wrapU; }
            set
            {
                if (_wrapU != value) {
                    _wrapU = value;
                    _samplerState = null;
                }
            }
        }

        public TextureAddressMode WrapV
        {
            get { return _wrapV; }
            set
            {
                if (_wrapV != value) {
                    _wrapV = value;
                    _samplerState = null;
                }
            }
        }

        public SamplerState SamplerState
        {
            get
            {
                if (_samplerState == null) {
                    if (!_samplerCache.TryGetValue(Key, out _samplerState)) {
                        _samplerState = new SamplerState() {
                            Filter = Filter,
                            AddressU = WrapU,
                            AddressV = WrapV,
                        };
                        _samplerCache[Key] = _samplerState;
                    }
                }

                return _samplerState;
            }
        }

        public int Width
        {
            get { return _texture.Width; }
        }

        public int Height
        {
            get { return _texture.Height; }
        }

        /*public void ApplyPreferredFormat (SurfaceFormat format)
        {
            if (_texture == null || _texture.Format == format)
                return;

            byte[] buffer = new byte[_texture.Width * _texture.Height * SurfaceFormatSize(_texture.Format) / 8];
            _texture.GetData<byte>(buffer);

            texture = new Texture2D(graphicsDevice, texture.Width, texture.Height, page.UseMipMaps, page.Format);
            texture.SetData<byte>(buffer);
        }*/

        private static int SurfaceFormatSize (SurfaceFormat format)
        {
            switch (format) {
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1a:
                    return 4;
                case SurfaceFormat.Alpha8:
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt5:
                    return 8 * 1;
                case SurfaceFormat.Bgr565:
                case SurfaceFormat.Bgra4444:
                case SurfaceFormat.Bgra5551:
                case SurfaceFormat.HalfSingle:
                case SurfaceFormat.NormalizedByte2:
                    return 8 * 2;
                case SurfaceFormat.Bgr32:
                case SurfaceFormat.Bgra32:
                case SurfaceFormat.Color:
                case SurfaceFormat.HalfVector2:
                case SurfaceFormat.NormalizedByte4:
                case SurfaceFormat.Rg32:
                case SurfaceFormat.Rgba1010102:
                case SurfaceFormat.Single:
                    return 8 * 4;
                case SurfaceFormat.HalfVector4:
                case SurfaceFormat.Rgba64:
                case SurfaceFormat.Vector2:
                case SurfaceFormat.HdrBlendable:
                    return 8 * 8;
                case SurfaceFormat.Vector4:
                    return 8 * 16;
                default:
                    return 1;
            }
        }

        private int Key
        {
            get { return (byte)Filter << 16 | (byte)WrapU << 8 | (byte)WrapV; }
        }
    }
}
