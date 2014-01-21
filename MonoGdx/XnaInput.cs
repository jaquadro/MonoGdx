/**
 * Copyright 2011-2013 See AUTHORS file.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGdx.Utils;

namespace MonoGdx
{
    public class XnaInput
    {
        public const float KeyRepeatInitialTime = .4f;
        public const float KeyRepeatTime = .1f;

        private List<TouchEvent> _touchEvents = new List<TouchEvent>(10);
        private List<KeyEvent> _keyEvents = new List<KeyEvent>(10);
        private MouseState _oldMouseState;
        private KeyboardState _oldKeyboardState;
        private TouchLocation _oldTouchLocation;

        private Pool<TouchEvent> _usedTouchEvents = new Pool<TouchEvent>(16, 512);
        private Pool<KeyEvent> _usedKeyEvents = new Pool<KeyEvent>(16, 512);

        private char _lastKeyCharPressed;
        private float _keyRepeatTimer;
        private long _currentEventTimeStamp;

        private Keys[] _oldKeysPressed = new Keys[0];
        private bool[] _keysHeld = new bool[256];

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

        public void Update (GameTime gameTime)
        {
            Update(gameTime.ElapsedGameTime);
        }

        public void Update (TimeSpan elapsedTime)
        {
            UpdateMouse();
            UpdateKeyboard((float)elapsedTime.TotalSeconds);
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
                ev.Type = TouchEventType.Moved;
                _touchEvents.Add(ev);

                if (state.LeftButton == ButtonState.Pressed
                    || state.MiddleButton == ButtonState.Pressed
                    || state.RightButton == ButtonState.Pressed) {
                    ev = ObtainTouchEvent(ref state);
                    ev.Type = TouchEventType.Dragged;
                    _touchEvents.Add(ev);
                }
            }

            if (state.ScrollWheelValue != _oldMouseState.ScrollWheelValue) {
                TouchEvent ev = ObtainTouchEvent(ref state);
                ev.Type = TouchEventType.Scrolled;
                ev.ScrollAmount = state.ScrollWheelValue - _oldMouseState.ScrollWheelValue;
                _touchEvents.Add(ev);
            }

            _oldMouseState = state;

            TouchCollection touchState = TouchPanel.GetState();
            TouchLocation touchLocation = (touchState.Count > 0) ? touchState[0] : new TouchLocation(0, TouchLocationState.Invalid, Vector2.Zero);

            if ((touchLocation.State & (TouchLocationState.Pressed | TouchLocationState.Moved)) != 0 && 
                (_oldTouchLocation.State & (TouchLocationState.Pressed | TouchLocationState.Moved)) == 0)
                PushTouchEvent(ref touchLocation, 0, ButtonState.Pressed);
            if ((touchLocation.State & (TouchLocationState.Pressed | TouchLocationState.Moved)) == 0 &&
                (_oldTouchLocation.State & (TouchLocationState.Pressed | TouchLocationState.Moved)) != 0) {
                if (touchLocation.State != TouchLocationState.Invalid)
                    PushTouchEvent(ref touchLocation, 0, ButtonState.Released);
                else
                    PushTouchEvent(ref _oldTouchLocation, 0, ButtonState.Released);
            }

            if (touchLocation.State == TouchLocationState.Moved) {
                TouchEvent ev = ObtainTouchEvent(ref touchLocation);
                ev.Type = TouchEventType.Moved;
                _touchEvents.Add(ev);

                ev = ObtainTouchEvent(ref touchLocation);
                ev.Type = TouchEventType.Dragged;
                _touchEvents.Add(ev);
            }

            _oldTouchLocation = touchLocation;
        }

        private void UpdateKeyboard (float elapsedTime)
        {
            if (_lastKeyCharPressed != 0) {
                _keyRepeatTimer -= elapsedTime;
                if (_keyRepeatTimer < 0) {
                    _keyRepeatTimer = KeyRepeatTime;

                    KeyEvent ev = _usedKeyEvents.Obtain();
                    ev.KeyCode = 0;
                    ev.KeyChar = _lastKeyCharPressed;
                    ev.Type = KeyEventType.Typed;
                    ev.Timestamp = DateTime.Now.Ticks * 100;
                    _keyEvents.Add(ev);
                    // Request Rendering? (Dirty flag)
                }
            }

            KeyboardState keyboard = Keyboard.GetState();
            Keys[] keysPressed = keyboard.GetPressedKeys();

            foreach (Keys key in keysPressed) {
                if (!_keysHeld[(int)key]) {
                    char keyChar = (char)0;
                    if (!KeyboardUtils.KeyToChar(key, keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift), out keyChar))
                        keyChar = (char)0;

                    switch (key) {
                        case Keys.Back:
                            keyChar = (char)8;
                            break;
                        case Keys.Delete:
                            keyChar = (char)127;
                            break;
                    }

                    long timestamp = DateTime.Now.Ticks * 100;

                    KeyEvent ev = _usedKeyEvents.Obtain();
                    ev.KeyCode = (int)key;
                    ev.KeyChar = (char)0;
                    ev.Type = KeyEventType.Down;
                    ev.Timestamp = timestamp;
                    _keyEvents.Add(ev);

                    if (keyChar != (char)0) {
                        ev = _usedKeyEvents.Obtain();
                        ev.KeyCode = 0;
                        ev.KeyChar = keyChar;
                        ev.Type = KeyEventType.Typed;
                        ev.Timestamp = timestamp;
                        _keyEvents.Add(ev);
                    }

                    _lastKeyCharPressed = keyChar;
                    _keyRepeatTimer = KeyRepeatInitialTime;

                    _keysHeld[(int)key] = true;
                }
            }

            foreach (Keys key in _oldKeysPressed) {
                if (!keysPressed.Contains(key)) {
                    KeyEvent ev = _usedKeyEvents.Obtain();
                    ev.KeyCode = (int)key;
                    ev.KeyChar = (char)0;
                    ev.Type = KeyEventType.Up;
                    ev.Timestamp = DateTime.Now.Ticks * 100;
                    _keyEvents.Add(ev);

                    _lastKeyCharPressed = (char)0;
                    _keysHeld[(int)key] = false;
                }
            }

            _oldKeysPressed = keysPressed;

            // Request rendering
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

        private TouchEvent ObtainTouchEvent (ref TouchLocation state)
        {
            TouchEvent ev = _usedTouchEvents.Obtain();
            ev.X = (int)state.Position.X;
            ev.Y = (int)state.Position.Y;
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

        private void PushTouchEvent (ref TouchLocation state, int button, ButtonState buttonState)
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

    static class KeyboardUtils
    {
        class CharPair
        {
            public CharPair (char normalChar, char? shiftChar)
            {
                this.NormalChar = normalChar;
                this.ShiftChar = shiftChar;
            }

            public char NormalChar;
            public char? ShiftChar;
        }

        static private Dictionary<Keys, CharPair> keyMap = new Dictionary<Keys, CharPair>();

        public static bool KeyToChar (Keys key, bool shitKeyPressed, out char character)
        {
            bool result = false;
            character = ' ';
            CharPair charPair;

            if ((Keys.A <= key && key <= Keys.Z) || key == Keys.Space) {
                character = (shitKeyPressed) ? (char)key : Char.ToLower((char)key);
                result = true;
            }
            else if (keyMap.TryGetValue(key, out charPair)) {
                if (!shitKeyPressed) {
                    character = charPair.NormalChar;
                    result = true;
                }
                else if (charPair.ShiftChar.HasValue) {
                    character = charPair.ShiftChar.Value;
                    result = true;
                }
            }

            return result;
        }

        static KeyboardUtils ()
        {
            InitializeKeyMap();
        }

        static void InitializeKeyMap ()
        {
            // First row of US keyboard.
            AddKeyMap(Keys.OemTilde, "`~");
            AddKeyMap(Keys.D1, "1!");
            AddKeyMap(Keys.D2, "2@");
            AddKeyMap(Keys.D3, "3#");
            AddKeyMap(Keys.D4, "4$");
            AddKeyMap(Keys.D5, "5%");
            AddKeyMap(Keys.D6, "6^");
            AddKeyMap(Keys.D7, "7&");
            AddKeyMap(Keys.D8, "8*");
            AddKeyMap(Keys.D9, "9(");
            AddKeyMap(Keys.D0, "0)");
            AddKeyMap(Keys.OemMinus, "-_");
            AddKeyMap(Keys.OemPlus, "=+");

            // Second row of US keyboard.
            AddKeyMap(Keys.OemOpenBrackets, "[{");
            AddKeyMap(Keys.OemCloseBrackets, "]}");
            AddKeyMap(Keys.OemPipe, "\\|");

            // Third row of US keyboard.
            AddKeyMap(Keys.OemSemicolon, ";:");
            AddKeyMap(Keys.OemQuotes, "'\"");
            AddKeyMap(Keys.OemComma, ",<");
            AddKeyMap(Keys.OemPeriod, ".>");
            AddKeyMap(Keys.OemQuestion, "/?");

            // Keypad keys of US keyboard.
            AddKeyMap(Keys.NumPad1, "1");
            AddKeyMap(Keys.NumPad2, "2");
            AddKeyMap(Keys.NumPad3, "3");
            AddKeyMap(Keys.NumPad4, "4");
            AddKeyMap(Keys.NumPad5, "5");
            AddKeyMap(Keys.NumPad6, "6");
            AddKeyMap(Keys.NumPad7, "7");
            AddKeyMap(Keys.NumPad8, "8");
            AddKeyMap(Keys.NumPad9, "9");
            AddKeyMap(Keys.NumPad0, "0");
            AddKeyMap(Keys.Add, "+");
            AddKeyMap(Keys.Divide, "/");
            AddKeyMap(Keys.Multiply, "*");
            AddKeyMap(Keys.Subtract, "-");
            AddKeyMap(Keys.Decimal, ".");
        }

        static void AddKeyMap (Keys key, string charPair)
        {
            char char1 = charPair[0];
            Nullable<char> char2 = null;
            if (charPair.Length > 1)
                char2 = charPair[1];

            keyMap.Add(key, new CharPair(char1, char2));
        }
    }
}
