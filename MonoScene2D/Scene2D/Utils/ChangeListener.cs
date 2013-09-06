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
