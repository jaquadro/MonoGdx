using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoGdx.Scene2D
{
    public enum InputType
    {
        TouchDown,
        TouchUp,
        TouchDragged,
        MouseMoved,
        Enter,
        Exit,
        Scrolled,
        KeyDown,
        KeyUp,
        KeyTyped,
    }

    public class InputEvent : Event
    {
        public InputType Type { get; set; }
        public float StageX { get; set; }
        public float StageY { get; set; }
        public int Pointer { get; set; }

        public int Button { get; set; }
        public int KeyCode { get; set; }
        public char Character { get; set; }
        public int ScrollAmount { get; set; }

        public Actor RelatedActor { get; set; }

        public override void Reset ()
        {
            base.Reset();
            RelatedActor = null;
            Button = -1;
        }

        public Vector2 ToCoordinates (Actor actor)
        {
            return actor.StageToLocalCoordinates(new Vector2(StageX, StageY));
        }

        public override string ToString ()
        {
            return Type.ToString();
        }
    }
}
