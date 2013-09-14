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

namespace MonoGdx.Scene2D.Utils
{
    public interface ILayout
    {
        void Layout ();
        void Invalidate ();
        void InvalidateHierarchy ();
        void Validate ();
        void Pack ();

        void SetFillParent (bool fillParent);
        void SetLayoutEnabled (bool enabled);

        float MinWidth { get; }
        float MinHeight { get; }
        float PrefWidth { get; }
        float PrefHeight { get; }
        float MaxWidth { get; }
        float MaxHeight { get; }
    }
}
