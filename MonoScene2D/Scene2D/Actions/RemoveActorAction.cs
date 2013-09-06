using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGdx.Scene2D.Actions
{
    public class RemoveActorAction : SceneAction
    {
        private bool _removed;

        public Actor RemoveActor { get; set; }

        public override bool Act (float delta)
        {
            if (!_removed) {
                _removed = true;
                (RemoveActor ?? Actor).Remove();
            }
            return true;
        }

        public override void Restart ()
        {
            _removed = false;
        }

        public override void Reset ()
        {
            base.Reset();
            RemoveActor = null;
        }
    }
}
