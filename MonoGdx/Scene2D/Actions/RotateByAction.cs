﻿/**
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

namespace MonoGdx.Scene2D.Actions
{
    /// <summary>
    /// Sets the actor's rotation from its current value to a relative value.
    /// </summary>
    public class RotateByAction : RelativeTemporalAction
    {
        public float Amount { get; set; }

        protected override void UpdateRelative (float percentDelta)
        {
            Actor.Rotate(Amount * percentDelta);
        }
    }
}
