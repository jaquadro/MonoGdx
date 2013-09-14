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

namespace MonoGdx.Scene2D.Utils
{
    public abstract class ChangeListener : EventListener<ChangeEvent>
    {
        public override bool Handle (ChangeEvent e)
        {
            Changed(e, e.TargetActor);
            return false;
        }

        public abstract void Changed (ChangeEvent e, Actor actor);
    }

    public class DispatchChangeListener : ChangeListener
    {
        public Action<ChangeEvent, Actor> OnChanged { get; set; }

        public override void Changed (ChangeEvent e, Actor actor)
        {
            if (OnChanged != null)
                OnChanged(e, actor);
        }
    }

    public class ChangeEvent : Event
    { }
}
