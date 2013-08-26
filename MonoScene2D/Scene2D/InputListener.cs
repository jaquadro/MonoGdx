using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoGdx.Scene2D
{
    public class InputListener : EventListener<InputEvent>
    {
        public bool Handle (InputEvent e)
        {
            switch (e.Type) {
                case InputType.KeyDown:
                    return KeyDown(e, e.KeyCode);
                case InputType.KeyUp:
                    return KeyUp(e, e.KeyCode);
                case InputType.KeyTyped:
                    return KeyTyped(e, e.Character);
            }

            Vector2 tmpCoords = e.ToCoordinates(e.ListenerActor);

            switch (e.Type) {
                case InputType.TouchDown:
                    return TouchDown(e, tmpCoords.X, tmpCoords.Y, e.Pointer, e.Button);
                case InputType.TouchUp:
                    TouchUp(e, tmpCoords.X, tmpCoords.Y, e.Pointer, e.Button);
                    return true;
                case InputType.TouchDragged:
                    TouchDragged(e, tmpCoords.X, tmpCoords.Y, e.Pointer);
                    return true;
                case InputType.MouseMoved:
                    return MouseMoved(e, tmpCoords.X, tmpCoords.Y);
                case InputType.Scrolled:
                    return Scrolled(e, tmpCoords.X, tmpCoords.Y, e.ScrollAmount);
                case InputType.Enter:
                    Enter(e, tmpCoords.X, tmpCoords.Y, e.Pointer, e.RelatedActor);
                    return false;
                case InputType.Exit:
                    Exit(e, tmpCoords.X, tmpCoords.Y, e.Pointer, e.RelatedActor);
                    return false;
            }

            return false;
        }

        public virtual bool TouchDown (InputEvent e, float x, float y, int pointer, int button)
        {
            return false;
        }

        public virtual void TouchUp (InputEvent e, float x, float y, int pointer, int button)
        { }

        public virtual void TouchDragged (InputEvent e, float x, float y, int pointer)
        { }

        public virtual bool MouseMoved (InputEvent e, float x, float y)
        {
            return false;
        }

        public virtual void Enter (InputEvent e, float x, float y, int pointer, Actor fromActor)
        { }

        public virtual void Exit (InputEvent e, float x, float y, int pointer, Actor toActor)
        { }

        public virtual bool Scrolled (InputEvent e, float x, float y, int amount)
        {
            return false;
        }

        public virtual bool KeyDown (InputEvent e, int keycode)
        {
            return false;
        }

        public virtual bool KeyUp (InputEvent e, int keycode)
        {
            return false;
        }

        public virtual bool KeyTyped (InputEvent e, char character)
        {
            return false;
        }
    }
}
