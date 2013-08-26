using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace MonoScene2D
{
    public static class XnaExt
    {
        public static bool IsMapMap (this TextureFilter filter)
        {
            switch (filter) {
                case TextureFilter.LinearMipPoint:
                case TextureFilter.MinLinearMagPointMipLinear:
                case TextureFilter.MinLinearMagPointMipPoint:
                case TextureFilter.MinPointMagLinearMipLinear:
                case TextureFilter.MinPointMagLinearMipPoint:
                case TextureFilter.PointMipLinear:
                    return true;
                default:
                    return false;
            }
        }
    }
}
