using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoScene2D.Scene2D.Utils
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
