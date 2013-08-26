using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGdx.Scene2D.Utils
{
    public abstract class FocusListener : EventListener<FocusEvent>
    {
        public override bool Handle (FocusEvent e)
        {
            switch (e.Type) {
                case FocusType.Keyboard:
                    KeyboardFocusChanged(e, e.TargetActor, e.IsFocused);
                    break;

                case FocusType.Scroll:
                    ScrollFocusChanged(e, e.TargetActor, e.IsFocused);
                    break;
            }

            return false;
        }

        public virtual void KeyboardFocusChanged (FocusEvent ev, Actor actor, bool focused)
        { }

        public virtual void ScrollFocusChanged (FocusEvent ev, Actor actor, bool focused)
        { }
    }
}
