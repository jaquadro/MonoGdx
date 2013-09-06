using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.Actions
{
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
