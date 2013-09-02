using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using MonoGdx.Utils;

namespace MonoGdx
{
    public class XnaInput
    {
        private List<TouchEvent> _touchEvents = new List<TouchEvent>(10);
        private List<KeyEvent> _keyEvents = new List<KeyEvent>(10);
        private MouseState _oldMouseState;

        private Pool<TouchEvent> _usedTouchEvents = new Pool<TouchEvent>(16, 512);
        private Pool<KeyEvent> _usedKeyEvents = new Pool<KeyEvent>(16, 512);

        public InputProcessor Processor { get; set; }

        public void ProcessEvents ()
        {
            if (Processor != null) {
                var processor = Processor;
                foreach (KeyEvent ev in _keyEvents) {
                    switch (ev.Type) {
                        case KeyEventType.Down:
                            processor.KeyDown(ev.KeyCode);
                            break;
                        case KeyEventType.Up:
                            processor.KeyUp(ev.KeyCode);
                            break;
                        case KeyEventType.Typed:
                            processor.KeyTyped(ev.KeyChar);
                            break;
                    }
                    _usedKeyEvents.Release(ev);
                }

                foreach (TouchEvent ev in _touchEvents) {
                    switch (ev.Type) {
                        case TouchEventType.Down:
                            processor.TouchDown(ev.X, ev.Y, ev.Pointer, ev.Button);
                            break;
                        case TouchEventType.Up:
                            processor.TouchUp(ev.X, ev.Y, ev.Pointer, ev.Button);
                            break;
                        case TouchEventType.Dragged:
                            processor.TouchDragged(ev.X, ev.Y, ev.Pointer);
                            break;
                        case TouchEventType.Moved:
                            processor.MouseMoved(ev.X, ev.Y);
                            break;
                        case TouchEventType.Scrolled:
                            processor.Scrolled(ev.ScrollAmount);
                            break;
                    }
                    _usedTouchEvents.Release(ev);
                }
            }
            else {
                foreach (KeyEvent ev in _keyEvents)
                    _usedKeyEvents.Release(ev);
                foreach (TouchEvent ev in _touchEvents)
                    _usedTouchEvents.Release(ev);
            }

            _keyEvents.Clear();
            _touchEvents.Clear();
        }

        public void Update ()
        {
            UpdateMouse();
        }

        private void UpdateMouse ()
        {
            MouseState state = Mouse.GetState();

            if (state.LeftButton != _oldMouseState.LeftButton)
                PushTouchEvent(ref state, 0, state.LeftButton);
            if (state.RightButton != _oldMouseState.RightButton)
                PushTouchEvent(ref state, 1, state.RightButton);
            if (state.MiddleButton != _oldMouseState.MiddleButton)
                PushTouchEvent(ref state, 2, state.MiddleButton);

            if (state.Position != _oldMouseState.Position) {
                TouchEvent ev = ObtainTouchEvent(ref state);
                if (state.LeftButton == ButtonState.Pressed
                    || state.MiddleButton == ButtonState.Pressed
                    || state.RightButton == ButtonState.Pressed)
                    ev.Type = TouchEventType.Dragged;
                else
                    ev.Type = TouchEventType.Moved;
                _touchEvents.Add(ev);
            }

            if (state.ScrollWheelValue != _oldMouseState.ScrollWheelValue) {
                TouchEvent ev = ObtainTouchEvent(ref state);
                ev.Type = TouchEventType.Scrolled;
                ev.ScrollAmount = state.ScrollWheelValue - _oldMouseState.ScrollWheelValue;
                _touchEvents.Add(ev);
            }

            _oldMouseState = state;
        }

        private TouchEvent ObtainTouchEvent (ref MouseState state)
        {
            TouchEvent ev = _usedTouchEvents.Obtain();
            ev.X = state.X;
            ev.Y = state.Y;
            ev.Pointer = 0;
            ev.Timestamp = DateTime.Now.Ticks * 100;

            return ev;
        }

        private void PushTouchEvent (ref MouseState state, int button, ButtonState buttonState)
        {
            TouchEvent ev = ObtainTouchEvent(ref state);
            ev.Button = button;

            switch (buttonState) {
                case ButtonState.Pressed:
                    ev.Type = TouchEventType.Down;
                    break;
                case ButtonState.Released:
                    ev.Type = TouchEventType.Up;
                    break;
            }

            _touchEvents.Add(ev);
        }

        private enum KeyEventType
        {
            Down,
            Up,
            Typed,
        }

        private class KeyEvent
        {
            public KeyEventType Type;
            public long Timestamp;
            public int KeyCode;
            public char KeyChar;
        }

        private enum TouchEventType
        {
            Down,
            Up,
            Dragged,
            Scrolled,
            Moved,
        }

        private class TouchEvent 
        {
            public TouchEventType Type;
            public long Timestamp;
            public int X;
            public int Y;
            public int ScrollAmount;
            public int Button;
            public int Pointer;
        }
    }
}
