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
        public override bool Handle (InputEvent e)
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

    /*public delegate bool TouchDownHandler (InputEvent e, float x, float y, int pointer, int button);
    public delegate void TouchUpHandler (InputEvent e, float x, float y, int pointer, int button);
    public delegate void TouchDraggedHandler (InputEvent e, float x, float y, int pointer);
    public delegate bool MouseMovedHandler (InputEvent e, float x, float y);
    public delegate void BoundaryHandler (InputEvent e, float x, float y, int pointer, Actor actor);
    public delegate bool ScrollHandler (InputEvent e, float x, float y, int amount);
    public delegate bool KeyHandler (InputEvent e, int keycode);
    public delegate bool KeyTypedHandler (InputEvent e, char character);*/

    public delegate bool TouchDownHandler (InputEvent e, float x, float y, int pointer, int button);
    public delegate void TouchUpHandler (InputEvent e, float x, float y, int pointer, int button);
    public delegate void TouchDraggedHandler (InputEvent e, float x, float y, int pointer);
    public delegate bool MouseMovedHandler (InputEvent e, float x, float y);
    public delegate void BoundaryHandler (InputEvent e, float x, float y, int pointer, Actor actor);
    public delegate bool ScrollHandler (InputEvent e, float x, float y, int amount);
    public delegate bool KeyHandler (InputEvent e, int keycode);
    public delegate bool KeyTypedHandler (InputEvent e, char character);

    public class TouchListener : InputListener
    {
        public TouchDownHandler Down { get; set; }
        public TouchUpHandler Up { get; set; }
        public TouchDraggedHandler Dragged { get; set; }

        public override bool TouchDown (InputEvent e, float x, float y, int pointer, int button)
        {
            return (Down != null) ? Down(e, x, y, pointer, button) : base.TouchDown(e, x, y, pointer, button);
        }

        public override void TouchUp (InputEvent e, float x, float y, int pointer, int button)
        {
            if (Up != null)
                Up(e, x, y, pointer, button);
            else
                base.TouchUp(e, x, y, pointer, button);
        }

        public override void TouchDragged (InputEvent e, float x, float y, int pointer)
        {
            if (Dragged != null)
                Dragged(e, x, y, pointer);
            else
                base.TouchDragged(e, x, y, pointer);
        }
    }

    public class DispatchInputListener : InputListener
    {
        public TouchDownHandler OnTouchDown { get; set; }
        public TouchUpHandler OnTouchUp { get; set; }
        public TouchDraggedHandler OnTouchDragged { get; set; }
        public MouseMovedHandler OnMouseMoved { get; set; }
        public BoundaryHandler OnEnter { get; set; }
        public BoundaryHandler OnExit { get; set; }
        public ScrollHandler OnScrolled { get; set; }
        public KeyHandler OnKeyDown { get; set; }
        public KeyHandler OnKeyUp { get; set; }
        public KeyTypedHandler OnKeyTyped { get; set; }

        public override bool TouchDown (InputEvent e, float x, float y, int pointer, int button)
        {
            return (OnTouchDown != null) ? OnTouchDown(e, x, y, pointer, button) : base.TouchDown(e, x, y, pointer, button);
        }

        public override void TouchUp (InputEvent e, float x, float y, int pointer, int button)
        {
            if (OnTouchUp != null)
                OnTouchUp(e, x, y, pointer, button);
            else
                base.TouchUp(e, x, y, pointer, button);
        }

        public override void TouchDragged (InputEvent e, float x, float y, int pointer)
        {
            if (OnTouchDragged != null)
                OnTouchDragged(e, x, y, pointer);
            else
                base.TouchDragged(e, x, y, pointer);
        }

        public override bool MouseMoved (InputEvent e, float x, float y)
        {
            return (OnMouseMoved != null) ? OnMouseMoved(e, x, y) : base.MouseMoved(e, x, y);
        }

        public override void Enter (InputEvent e, float x, float y, int pointer, Actor fromActor)
        {
            if (OnEnter != null)
                OnEnter(e, x, y, pointer, fromActor);
            else
                base.Enter(e, x, y, pointer, fromActor);
        }

        public override void Exit (InputEvent e, float x, float y, int pointer, Actor toActor)
        {
            if (OnExit != null)
                OnExit(e, x, y, pointer, toActor);
            else
                base.Exit(e, x, y, pointer, toActor);
        }

        public override bool Scrolled (InputEvent e, float x, float y, int amount)
        {
            return (OnScrolled != null) ? OnScrolled(e, x, y, amount) : base.Scrolled(e, x, y, amount);
        }

        public override bool KeyDown (InputEvent e, int keycode)
        {
            return (OnKeyDown != null) ? OnKeyDown(e, keycode) : base.KeyDown(e, keycode);
        }

        public override bool KeyUp (InputEvent e, int keycode)
        {
            return (OnKeyUp != null) ? OnKeyUp(e, keycode) : base.KeyUp(e, keycode);
        }

        public override bool KeyTyped (InputEvent e, char character)
        {
            return (OnKeyTyped != null) ? OnKeyTyped(e, character) : base.KeyTyped(e, character);
        }
    }
}
