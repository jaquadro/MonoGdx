/**
 * Copyright 2011-2013 See AUTHORS file.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGdx.Scene2D.Utils
{
    public class SceneBatch
    {
        public SceneBatch (SpriteBatch spriteBatch)
        {
            SpriteBatch = spriteBatch;
            Transform = Matrix.Identity;
        }

        public SpriteBatch SpriteBatch { get; private set; }

        public Matrix Transform { get; private set; }

        public void Begin ()
        {
            Transform = Matrix.Identity;
            SpriteBatch.Begin();
        }

        public void Begin (SpriteSortMode sortMode, BlendState blendState)
        {
            Transform = Matrix.Identity;
            SpriteBatch.Begin(sortMode, blendState);
        }

        public void Begin (SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
        {
            Transform = Matrix.Identity;
            SpriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState);
        }
        
        public void Begin (SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect)
        {
            Transform = Matrix.Identity;
            SpriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect);
        }

        public void Begin (SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transform)
        {
            Transform = transform;
            SpriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transform);
        }

        public void End ()
        {
            SpriteBatch.End();
        }
    }
}
