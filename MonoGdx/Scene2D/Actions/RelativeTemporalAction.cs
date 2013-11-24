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

namespace MonoGdx.Scene2D.Actions
{
    /// <summary>
    /// Base class for actions that transition over time using the percent complete since the last frame.
    /// </summary>
    abstract public class RelativeTemporalAction : TemporalAction
    {
        private float _lastPercent;

        protected override void Begin ()
        {
            _lastPercent = 0;
        }

        protected override void Update (float percent)
        {
            UpdateRelative(percent - _lastPercent);
            _lastPercent = percent;
        }

        protected abstract void UpdateRelative (float percentDelta);
    }
}
