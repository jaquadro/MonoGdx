using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGdx.Scene2D.Actions
{
    public class RemoveListenerAction : SceneAction
    {
        public Actor TargetActor { get; set; }
        public EventListener Listener { get; set; }
        public bool Capture { get; set; }

        public override bool Act (float delta)
        {
            Actor actor = (TargetActor != null) ? TargetActor : Actor;
            if (Capture)
                actor.RemoveCaptureListener(Listener);
            else
                actor.RemoveListener(Listener);
            return true;
        }

        public override void Reset ()
        {
            base.Reset();
            TargetActor = null;
            Listener = null;
        }
    }
}
