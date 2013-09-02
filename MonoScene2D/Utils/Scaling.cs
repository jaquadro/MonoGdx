using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGdx.Utils
{
    public enum Scaling
    {
        None,
        Fit,
        Fill,
        FillX,
        FillY,
        Stretch,
        StretchX,
        StretchY,
    }

    public static class ScalingExt
    {
        public static Vector2 Apply (this Scaling scaling, float sourceWidth, float sourceHeight, float targetWidth, float targetHeight)
        {
            float targetRatio, sourceRatio, scale;

            switch (scaling) {
                case Scaling.Fit:
                    targetRatio = targetHeight / targetWidth;
                    sourceRatio = sourceHeight / sourceWidth;
                    scale = (targetRatio > sourceRatio) ? (targetWidth / sourceWidth) : (targetHeight / sourceHeight);
                    return new Vector2(sourceWidth * scale, sourceHeight * scale);

                case Scaling.Fill:
                    targetRatio = targetHeight / targetWidth;
                    sourceRatio = sourceHeight / sourceWidth;
                    scale = (targetRatio < sourceRatio) ? (targetWidth / sourceWidth) : (targetHeight / sourceHeight);
                    return new Vector2(sourceWidth * scale, sourceHeight * scale);

                case Scaling.FillX:
                    targetRatio = targetHeight / targetWidth;
                    sourceRatio = sourceHeight / sourceWidth;
                    scale = targetWidth / sourceWidth;
                    return new Vector2(sourceWidth * scale, sourceHeight * scale);

                case Scaling.FillY:
                    targetRatio = targetHeight / targetWidth;
                    sourceRatio = sourceHeight / sourceWidth;
                    scale = targetHeight / sourceHeight;
                    return new Vector2(sourceWidth * scale, sourceHeight * scale);

                case Scaling.Stretch:
                    return new Vector2(targetWidth, targetHeight);

                case Scaling.StretchX:
                    return new Vector2(targetWidth, sourceHeight);

                case Scaling.StretchY:
                    return new Vector2(sourceWidth, targetHeight);

                case Scaling.None:
                default:
                    return new Vector2(sourceWidth, sourceHeight);
            }
        }
    }
}
