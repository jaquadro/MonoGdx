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

    public class DispatchFocusListener : FocusListener
    {
        public Action<FocusEvent, Actor, bool> OnKeyboardFocusChanged { get; set; }
        public Action<FocusEvent, Actor, bool> OnScrollFocusChanged { get; set; }

        public override void KeyboardFocusChanged (FocusEvent ev, Actor actor, bool focused)
        {
            if (OnKeyboardFocusChanged != null)
                OnKeyboardFocusChanged(ev, actor, focused);
            else
                base.KeyboardFocusChanged(ev, actor, focused);
        }

        public override void ScrollFocusChanged (FocusEvent ev, Actor actor, bool focused)
        {
            if (OnScrollFocusChanged != null)
                OnScrollFocusChanged(ev, actor, focused);
            else
                base.ScrollFocusChanged(ev, actor, focused);
        }
    }
}
