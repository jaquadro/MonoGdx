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
using MonoGdx.Utils;

namespace MonoGdx.Scene2D
{
    [TODO]
    public class Event : IPoolable
    {
        public Actor TargetActor { get; set; }
        public Actor ListenerActor { get; set; }
        public Stage Stage { get; set; }
        
        public bool Bubbles { get; set; }
        public bool IsHandled { get; private set; }
        public bool IsCancelled { get; private set; }
        public bool IsStopped { get; private set; }
        public bool IsCapture { get; set; }

        public Event ()
        {
            Bubbles = true;
        }

        public void Handle ()
        {
            IsHandled = true;
        }

        public void Cancel ()
        {
            IsCancelled = true;
            IsStopped = true;
            IsHandled = true;
        }

        public void Stop ()
        {
            IsStopped = true;
        }

        public virtual void Reset ()
        {
            Stage = null;
            TargetActor = null;
            ListenerActor = null;
            IsCapture = false;
            Bubbles = true;
            IsHandled = false;
            IsStopped = false;
            IsCancelled = false;
        }
    }
}
