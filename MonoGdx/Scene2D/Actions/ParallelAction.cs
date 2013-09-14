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
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.Actions
{
    public class ParallelAction : SceneAction
    {
        private List<SceneAction> _actions = new List<SceneAction>(4);
        private bool _complete;

        public ParallelAction ()
        { }

        public ParallelAction (SceneAction action1)
        {
            AddAction(action1);
        }

        public ParallelAction (SceneAction action1, SceneAction action2)
        {
            AddAction(action1);
            AddAction(action2);
        }

        public ParallelAction (SceneAction action1, SceneAction action2, SceneAction action3)
        {
            AddAction(action1);
            AddAction(action2);
            AddAction(action3);
        }

        public ParallelAction (SceneAction action1, SceneAction action2, SceneAction action3, SceneAction action4)
        {
            AddAction(action1);
            AddAction(action2);
            AddAction(action3);
            AddAction(action4);
        }

        public ParallelAction (SceneAction action1, SceneAction action2, SceneAction action3, SceneAction action4, SceneAction action5)
        {
            AddAction(action1);
            AddAction(action2);
            AddAction(action3);
            AddAction(action4);
            AddAction(action5);
        }

        public override bool Act (float delta)
        {
            if (_complete)
                return true;

            _complete = true;
            Pool pool = Pool;
            Pool = null;

            try {
                if (Actor == null)
                    return true;

                foreach (SceneAction action in _actions) {
                    if (!action.Act(delta))
                        _complete = false;
                    if (Actor == null)
                        return true;
                }

                return _complete;
            }
            finally {
                Pool = pool;
            }
        }

        public override void Restart ()
        {
            _complete = false;
            foreach (SceneAction action in _actions)
                action.Restart();
        }

        public override void Reset ()
        {
            base.Reset();
            _actions.Clear();
        }
        
        public void AddAction (SceneAction action)
        {
            _actions.Add(action);
            if (Actor != null)
                action.Actor = Actor;
        }

        public override Actor Actor
        {
            get { return base.Actor; }
            set
            {
                foreach (SceneAction action in _actions)
                    action.Actor = value;
                base.Actor = value;
            }
        }

        public List<SceneAction> Actions
        {
            get { return _actions; }
        }

        public override string ToString ()
        {
            StringBuilder buffer = new StringBuilder(64);
            buffer.Append(base.ToString());
            buffer.Append('(');

            for (int i = 0; i < _actions.Count; i++) {
                if (i > 0)
                    buffer.Append(", ");
                buffer.Append(_actions[i]);
            }

            buffer.Append(')');
            return buffer.ToString();
        }
    }
}
