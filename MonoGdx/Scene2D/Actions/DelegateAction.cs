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
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.Actions
{
    /// <summary>
    /// Base class for an action that wraps another action.
    /// </summary>
    public abstract class DelegateAction : SceneAction
    {
        public SceneAction Action { get; set; }

        protected abstract bool Delegate (float delta);

        public override bool Act (float delta)
        {
            Pool pool = Pool;
            Pool = null;

            try {
                return Delegate(delta);
            }
            finally {
                Pool = pool;
            }
        }

        public override void Restart ()
        {
            if (Action != null)
                Action.Restart();
        }

        public override void Reset ()
        {
            base.Reset();
            Action = null;
        }

        public override Actor Actor
        {
            get { return base.Actor; }
            set
            {
                if (Action != null)
                    Action.Actor = value;
                base.Actor = value;
            }
        }

        public override string ToString ()
        {
            return base.ToString() + (Action == null ? "" : "(" + Action + ")");
        }
    }
}
