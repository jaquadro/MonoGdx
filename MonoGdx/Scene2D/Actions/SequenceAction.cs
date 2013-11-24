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

using MonoGdx.Utils;

namespace MonoGdx.Scene2D.Actions
{
    /// <summary>
    /// Executes a number of actions one at a time.
    /// </summary>
    public class SequenceAction : ParallelAction
    {
        private int _index;

        public SequenceAction ()
        { }

        public SequenceAction (SceneAction action1)
        {
            AddAction(action1);
        }

        public SequenceAction (SceneAction action1, SceneAction action2)
        {
            AddAction(action1);
            AddAction(action2);
        }

        public SequenceAction (SceneAction action1, SceneAction action2, SceneAction action3)
        {
            AddAction(action1);
            AddAction(action2);
            AddAction(action3);
        }

        public SequenceAction (SceneAction action1, SceneAction action2, SceneAction action3, SceneAction action4)
        {
            AddAction(action1);
            AddAction(action2);
            AddAction(action3);
            AddAction(action4);
        }

        public SequenceAction (SceneAction action1, SceneAction action2, SceneAction action3, SceneAction action4, SceneAction action5)
        {
            AddAction(action1);
            AddAction(action2);
            AddAction(action3);
            AddAction(action4);
            AddAction(action5);
        }

        public override bool Act (float delta)
        {
            if (_index >= Actions.Count)
                return true;

            Pool pool = Pool;
            Pool = null;

            try {
                if (Actions[_index].Act(delta)) {
                    if (Actor == null)
                        return true;
                    _index++;
                    if (_index >= Actions.Count)
                        return true;
                }
                return false;
            }
            finally {
                Pool = pool;
            }
        }

        public override void Restart ()
        {
            base.Restart();
            _index = 0;
        }
    }
}
