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

    public class ChangeEvent : Event
    { }
}
