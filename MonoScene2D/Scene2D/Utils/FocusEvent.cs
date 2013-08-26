using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoScene2D.Scene2D.Utils
{
    public enum FocusType
    {
        Keyboard,
        Scroll,
    }

    public class FocusEvent : Event
    {
        public bool IsFocused { get; set; }
        public FocusType Type { get; set; }
        public Actor RelatedActor { get; set; }

        public override void Reset ()
        {
            base.Reset();
            RelatedActor = null;
        }
    }
}
