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
