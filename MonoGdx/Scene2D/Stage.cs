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
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGdx.Graphics;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D
{
    public class Stage : InputAdapter, IDisposable
    {
        public static readonly RoutedEvent GotKeyboardFocusEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(KeyboardFocusChangedEventHandler), typeof(Stage));
        public static readonly RoutedEvent LostKeyboardFocusEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(KeyboardFocusChangedEventHandler), typeof(Stage));
        public static readonly RoutedEvent GotScrollFocusEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(ScrollFocusChangedEventHandler), typeof(Stage));
        public static readonly RoutedEvent LostScrollFocusEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(ScrollFocusChangedEventHandler), typeof(Stage));
        public static readonly RoutedEvent GotTouchCaptureEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(TouchEventHandler), typeof(Stage));
        public static readonly RoutedEvent LostTouchCaptureEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(TouchEventHandler), typeof(Stage));

        public static readonly RoutedEvent PreviewKeyDownEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Tunnel, typeof(KeyEventHandler), typeof(Stage));
        public static readonly RoutedEvent KeyDownEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(KeyEventHandler), typeof(Stage));
        public static readonly RoutedEvent PreviewKeyUpEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Tunnel, typeof(KeyEventHandler), typeof(Stage));
        public static readonly RoutedEvent KeyUpEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(KeyEventHandler), typeof(Stage));
        public static readonly RoutedEvent PreviewKeyTypedEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Tunnel, typeof(KeyCharEventHandler), typeof(Stage));
        public static readonly RoutedEvent KeyTypedEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(KeyCharEventHandler), typeof(Stage));
        public static readonly RoutedEvent PreviewMouseMoveEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Tunnel, typeof(MouseEventHandler), typeof(Stage));
        public static readonly RoutedEvent MouseMoveEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(Stage));
        public static readonly RoutedEvent PreviewTouchDownEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Tunnel, typeof(TouchEventHandler), typeof(Stage));
        public static readonly RoutedEvent TouchDownEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(TouchEventHandler), typeof(Stage));
        public static readonly RoutedEvent PreviewTouchDragEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Tunnel, typeof(TouchEventHandler), typeof(Stage));
        public static readonly RoutedEvent TouchDragEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(TouchEventHandler), typeof(Stage));
        public static readonly RoutedEvent PreviewTouchUpEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Tunnel, typeof(TouchEventHandler), typeof(Stage));
        public static readonly RoutedEvent TouchUpEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(TouchEventHandler), typeof(Stage));
        public static readonly RoutedEvent TouchEnterEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(TouchEventHandler), typeof(Stage));
        public static readonly RoutedEvent TouchLeaveEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(TouchEventHandler), typeof(Stage));
        public static readonly RoutedEvent PreviewScrollEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Tunnel, typeof(ScrollEventHandler), typeof(Stage));
        public static readonly RoutedEvent ScrollEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(ScrollEventHandler), typeof(Stage));

        private bool _ownsSpriteBatch;
        private float _viewportX;
        private float _viewportY;
        private float _viewportWidth;
        private float _viewportHeight;
        private float _centerX;
        private float _centerY;

        private readonly Actor[] _pointerOverActors = new Actor[20];
        private readonly bool[] _pointerTouched = new bool[20];
        private readonly Point[] _pointerScreen = new Point[20];

        private int _mouseScreenX;
        private int _mouseScreenY;
        private Actor _mouseOverActor;
        private Actor _keyboardFocus;
        private Actor _scrollFocus;
        private readonly Actor[] _touchCapture = new Actor[20];

        public Stage (GraphicsDevice graphicsDevice)
            : this(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, false, graphicsDevice)
        { }

        public Stage (float width, float height, bool keepAspectRatio, GraphicsDevice graphicsDevice)
            : this(width, height, keepAspectRatio, new GdxSpriteBatch(graphicsDevice))
        {
            _ownsSpriteBatch = true;
        }

        public Stage (float width, float height, bool keepAspectRatio, GdxSpriteBatch spriteBatch)
        {
            Width = width;
            Height = height;
            SpriteBatch = spriteBatch;

            Root = new Group() {
                Stage = this,
            };

            Camera = new OrthographicCamera(spriteBatch.GraphicsDevice);
            SetViewport(width, height, keepAspectRatio);

            ScissorStack = new ScissorStack(spriteBatch.GraphicsDevice);
        }

        public void SetViewport (float width, float height, bool keepAspectRatio)
        {
            SetViewport(width, height, keepAspectRatio, 0, 0, SpriteBatch.GraphicsDevice.Viewport.Width, SpriteBatch.GraphicsDevice.Viewport.Height);
        }

        public void SetViewport (float stageWidth, float stageHeight, bool keepAspectRatio, float viewportX, float viewportY, float viewportWidth, float viewportHeight)
        {
            _viewportX = viewportX;
            _viewportY = viewportY;
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;

            if (keepAspectRatio) {
                if (viewportHeight / viewportWidth < stageHeight / stageWidth) {
                    float toViewportSpace = viewportHeight / stageHeight;
                    float toStageSpace = stageHeight / viewportHeight;
                    float deviceWidth = stageWidth * toViewportSpace;
                    float lengthen = (viewportWidth - deviceWidth) * toStageSpace;

                    Width = stageWidth + lengthen;
                    Height = stageHeight;
                    GutterWidth = lengthen / 2;
                    GutterHeight = 0;
                }
                else {
                    float toViewportSpace = viewportWidth / stageWidth;
                    float toStageSpace = stageWidth / viewportWidth;
                    float deviceHeight = stageHeight * toViewportSpace;
                    float lengthen = (viewportHeight - deviceHeight) * toStageSpace;

                    Width = stageWidth;
                    Height = stageHeight + lengthen;
                    GutterWidth = 0;
                    GutterHeight = lengthen / 2;
                }
            }
            else {
                Width = stageWidth;
                Height = stageHeight;
                GutterWidth = 0;
                GutterHeight = 0;
            }

            _centerX = Width / 2;
            _centerY = Height / 2;

            Camera.Position = new Vector3(_centerX, _centerY, 0);
            Camera.ViewportWidth = Width;
            Camera.ViewportHeight = Height;
        }

        public void Draw ()
        {
            Camera.Update();
            if (!Root.IsVisible)
                return;

            SpriteBatch.Begin(Camera.Combined, Matrix.Identity);
            Root.Draw(SpriteBatch, 1);
            SpriteBatch.End();
        }

        [TODO]
        public void Act ()
        {
            throw new NotImplementedException();
        }

        [TODO]
        public void Act (float delta)
        {
            // Update over actors. Done in act() because actors may change position, which can fire enter/exit without an input event.
            for (int pointer = 0, n = _pointerOverActors.Length; pointer < n; pointer++) {
                Actor overLast = _pointerOverActors[pointer];

                // Check if pointer is gone.
                if (!_pointerTouched[pointer]) {
                    if (overLast != null) {
                        _pointerOverActors[pointer] = null;
                        Vector2 stageCoords = ScreenToStageCoordinates(new Vector2(_pointerScreen[pointer].X, _pointerScreen[pointer].Y));

                        // Exit over last
                        TouchEventArgs ev = Pools<TouchEventArgs>.Obtain();
                        ev.RoutedEvent = TouchLeaveEvent;
                        ev.Stage = this;
                        ev.StagePosition = stageCoords;
                        ev.RelatedActor = overLast;
                        ev.Pointer = pointer;

                        overLast.RaiseEvent(ev);
                        Pools<TouchEventArgs>.Release(ev);
                    }
                    continue;
                }

                // Update over actor for the pointer
                _pointerOverActors[pointer] = FireEnterAndExit(overLast, _pointerScreen[pointer].X, _pointerScreen[pointer].Y, pointer);
            }

            // Update over actor the mouse on the desktop
            // ApplicationType type = Gdx.app.getType();
            // if (type == ApplicationType.Desktop || type == ApplicationType.Applet || type == ApplicationType.WebGL)
            _mouseOverActor = FireEnterAndExit(_mouseOverActor, _mouseScreenX, _mouseScreenY, -1);

            Root.Act(delta);
        }

        private Actor FireEnterAndExit (Actor overLast, int screenX, int screenY, int pointer)
        {
            Vector2 stageCoords = ScreenToStageCoordinates(new Vector2(screenX, screenY));
            Actor over = Hit(stageCoords.X, stageCoords.Y, true);
            if (over == overLast)
                return overLast;

            TouchEventArgs ev = Pools<TouchEventArgs>.Obtain();
            ev.Stage = this;
            ev.StagePosition = stageCoords;
            ev.Pointer = pointer;

            if (overLast != null) {
                ev.RoutedEvent = TouchLeaveEvent;
                ev.RelatedActor = over;
                overLast.RaiseEvent(ev);
            }

            if (over != null) {
                ev.RoutedEvent = TouchEnterEvent;
                ev.RelatedActor = overLast;
                over.RaiseEvent(ev);
            }

            Pools<TouchEventArgs>.Release(ev);

            return over;
        }

        public override bool TouchDown (int screenX, int screenY, int pointer, int button)
        {
            _pointerTouched[pointer] = true;
            _pointerScreen[pointer] = new Point(screenX, screenY);

            Vector2 stageCoords = ScreenToStageCoordinates(new Vector2(screenX, screenY));

            TouchEventArgs ev = Pools<TouchEventArgs>.Obtain();
            ev.RoutedEvent = PreviewTouchDownEvent;
            ev.Stage = this;
            ev.StagePosition = stageCoords;
            ev.Pointer = pointer;
            ev.Button = button;

            Actor target = _touchCapture[pointer] ?? Hit(stageCoords.X, stageCoords.Y, true);
            if (target == null)
                target = Root;

            if (!target.RaiseEvent(ev)) {
                ev.RoutedEvent = TouchDownEvent;
                target.RaiseEvent(ev);
            }

            bool handled = ev.Handled;
            Pools<TouchEventArgs>.Release(ev);

            return handled;
        }

        public override bool TouchDragged (int screenX, int screenY, int pointer)
        {
            _pointerScreen[pointer] = new Point(screenX, screenY);

            Vector2 stageCoords = ScreenToStageCoordinates(new Vector2(screenX, screenY));

            TouchEventArgs ev = Pools<TouchEventArgs>.Obtain();
            ev.RoutedEvent = PreviewTouchDragEvent;
            ev.Stage = this;
            ev.StagePosition = stageCoords;
            ev.Pointer = pointer;

            Actor target = _touchCapture[pointer] ?? Hit(stageCoords.X, stageCoords.Y, true);
            if (target == null)
                target = Root;

            if (!target.RaiseEvent(ev)) {
                ev.RoutedEvent = TouchDragEvent;
                target.RaiseEvent(ev);
            }

            bool handled = ev.Handled;
            Pools<TouchEventArgs>.Release(ev);

            return handled;
        }

        public override bool TouchUp (int screenX, int screenY, int pointer, int button)
        {
            _pointerTouched[pointer] = false;
            _pointerScreen[pointer] = new Point(screenX, screenY);

            Vector2 stageCoords = ScreenToStageCoordinates(new Vector2(screenX, screenY));

            TouchEventArgs ev = Pools<TouchEventArgs>.Obtain();
            ev.RoutedEvent = PreviewTouchUpEvent;
            ev.Stage = this;
            ev.StagePosition = stageCoords;
            ev.Pointer = pointer;
            ev.Button = button;

            Actor target = _touchCapture[pointer] ?? Hit(stageCoords.X, stageCoords.Y, true);
            if (target == null)
                target = Root;

            if (!target.RaiseEvent(ev)) {
                ev.RoutedEvent = TouchUpEvent;
                target.RaiseEvent(ev);
            }

            bool handled = ev.Handled;
            Pools<TouchEventArgs>.Release(ev);

            return handled;
        }

        public override bool MouseMoved (int screenX, int screenY)
        {
            _mouseScreenX = screenX;
            _mouseScreenY = screenY;

            Vector2 stageCoords = ScreenToStageCoordinates(new Vector2(screenX, screenY));

            MouseEventArgs ev = Pools<MouseEventArgs>.Obtain();
            ev.RoutedEvent = PreviewMouseMoveEvent;
            ev.Stage = this;
            ev.StagePosition = stageCoords;

            Actor target = Hit(stageCoords.X, stageCoords.Y, true);
            if (target == null)
                target = Root;

            if (!target.RaiseEvent(ev)) {
                ev.RoutedEvent = MouseMoveEvent;
                target.RaiseEvent(ev);
            }

            bool handled = ev.Handled;
            Pools<MouseEventArgs>.Release(ev);

            return handled;
        }

        public override bool Scrolled (int amount)
        {
            Actor target = (_scrollFocus == null) ? Root : _scrollFocus;

            Vector2 stageCoords = ScreenToStageCoordinates(new Vector2(_mouseScreenX, _mouseScreenY));

            ScrollEventArgs ev = Pools<ScrollEventArgs>.Obtain();
            ev.RoutedEvent = PreviewScrollEvent;
            ev.Stage = this;
            ev.ScrollAmountV = amount;
            ev.ScrollAmountH = 0;

            if (!target.RaiseEvent(ev)) {
                ev.RoutedEvent = ScrollEvent;
                target.RaiseEvent(ev);
            }

            bool handled = ev.Handled;
            Pools<ScrollEventArgs>.Release(ev);

            return handled;
        }

        public override bool KeyDown (int keycode)
        {
            Actor target = (_keyboardFocus == null) ? Root : _keyboardFocus;

            KeyEventArgs ev = Pools<KeyEventArgs>.Obtain();
            ev.RoutedEvent = PreviewKeyDownEvent;
            ev.Stage = this;
            ev.KeyCode = keycode;
            ev.IsDown = true;

            if (!target.RaiseEvent(ev)) {
                ev.RoutedEvent = KeyDownEvent;
                target.RaiseEvent(ev);
            }

            bool handled = ev.Handled;
            Pools<KeyEventArgs>.Release(ev);

            return handled;
        }

        public override bool KeyUp (int keycode)
        {
            Actor target = (_keyboardFocus == null) ? Root : _keyboardFocus;

            KeyEventArgs ev = Pools<KeyEventArgs>.Obtain();
            ev.RoutedEvent = PreviewKeyUpEvent;
            ev.Stage = this;
            ev.KeyCode = keycode;
            ev.IsUp = true;

            if (!target.RaiseEvent(ev)) {
                ev.RoutedEvent = KeyUpEvent;
                target.RaiseEvent(ev);
            }

            bool handled = ev.Handled;
            Pools<KeyEventArgs>.Release(ev);

            return handled;
        }

        public override bool KeyTyped (char character)
        {
            Actor target = (_keyboardFocus == null) ? Root : _keyboardFocus;

            KeyCharEventArgs ev = Pools<KeyCharEventArgs>.Obtain();
            ev.RoutedEvent = PreviewKeyTypedEvent;
            ev.Stage = this;
            ev.Character = character;

            if (!target.RaiseEvent(ev)) {
                ev.RoutedEvent = KeyTypedEvent;
                target.RaiseEvent(ev);
            }

            bool handled = ev.Handled;
            Pools<KeyCharEventArgs>.Release(ev);

            return handled;
        }

        public void AddActor (Actor actor)
        {
            Root.AddActor(actor);
        }

        public void AddAction (SceneAction action)
        {
            Root.AddAction(action);
        }

        public IList<Actor> Actors
        {
            get { return Root.Children; }
        }

        public void Clear ()
        {
            UnfocusAll();
            Root.Clear();
        }

        public void UnfocusAll ()
        {
            _scrollFocus = null;
            _keyboardFocus = null;
            ReleaseTouchCapture();
        }

        public void Unfocus (Actor actor)
        {
            if (_scrollFocus != null && _scrollFocus.IsDescendentOf(actor))
                _scrollFocus = null;
            if (_keyboardFocus != null && _keyboardFocus.IsDescendentOf(actor))
                _keyboardFocus = null;
        }

        public Actor GetTouchCapture (int pointer)
        {
            if (pointer < 0 || pointer >= _touchCapture.Length)
                throw new ArgumentOutOfRangeException("pointer");

            return _touchCapture[pointer];
        }

        public void SetTouchCapture (Actor actor, int pointer)
        {
            if (pointer < 0 || pointer >= _touchCapture.Length)
                throw new ArgumentOutOfRangeException("pointer");

            Actor oldTouchCapture = _touchCapture[pointer];
            _touchCapture[pointer] = actor;

            if (oldTouchCapture != null) {
                TouchEventArgs ev = Pools<TouchEventArgs>.Obtain();
                ev.RoutedEvent = LostTouchCaptureEvent;
                ev.Pointer = pointer;

                bool cancel = oldTouchCapture.RaiseEvent(ev);
                Pools<TouchEventArgs>.Release(ev);

                if (cancel)
                    return;
            }

            if (actor != null) {
                TouchEventArgs ev = Pools<TouchEventArgs>.Obtain();
                ev.RoutedEvent = GotTouchCaptureEvent;
                ev.Pointer = pointer;

                if (actor.RaiseEvent(ev))
                    SetTouchCapture(oldTouchCapture, pointer);

                Pools<TouchEventArgs>.Release(ev);
            }
        }

        public void ReleaseTouchCapture (int pointer)
        {
            SetTouchCapture(null, pointer);
        }

        public void ReleaseTouchCapture ()
        {
            for (int i = 0; i < _touchCapture.Length; i++)
                ReleaseTouchCapture(i);
        }

        public void SetKeyboardFocus (Actor actor)
        {
            if (_keyboardFocus == actor)
                return;

            Actor oldKeyboardFocus = _keyboardFocus;
            _keyboardFocus = actor;

            if (oldKeyboardFocus != null) {
                KeyboardFocusChangedEventArgs eva = Pools<KeyboardFocusChangedEventArgs>.Obtain();
                eva.RoutedEvent = LostKeyboardFocusEvent;
                eva.NewFocus = actor;
                eva.OldFocus = oldKeyboardFocus;

                bool cancel = oldKeyboardFocus.RaiseEvent(eva);
                Pools<KeyboardFocusChangedEventArgs>.Release(eva);

                if (cancel)
                    return;
            }

            if (actor != null) {
                KeyboardFocusChangedEventArgs eva = Pools<KeyboardFocusChangedEventArgs>.Obtain();
                eva.RoutedEvent = GotKeyboardFocusEvent;
                eva.NewFocus = actor;
                eva.OldFocus = oldKeyboardFocus;

                if (actor.RaiseEvent(eva))
                    SetKeyboardFocus(oldKeyboardFocus);

                Pools<KeyboardFocusChangedEventArgs>.Release(eva);
            }
        }

        public Actor GetKeyboardFocus ()
        {
            return _keyboardFocus;
        }

        public void SetScrollFocus (Actor actor)
        {
            if (_scrollFocus == actor)
                return;

            Actor oldScrollFocus = _scrollFocus;
            _scrollFocus = actor;

            if (oldScrollFocus != null) {
                ScrollFocusChangedEventArgs eva = Pools<ScrollFocusChangedEventArgs>.Obtain();
                eva.RoutedEvent = LostScrollFocusEvent;
                eva.NewFocus = actor;
                eva.OldFocus = oldScrollFocus;

                bool cancel = oldScrollFocus.RaiseEvent(eva);
                Pools<ScrollFocusChangedEventArgs>.Release(eva);

                if (cancel)
                    return;
            }

            if (actor != null) {
                ScrollFocusChangedEventArgs eva = Pools<ScrollFocusChangedEventArgs>.Obtain();
                eva.RoutedEvent = GotScrollFocusEvent;
                eva.NewFocus = actor;
                eva.OldFocus = oldScrollFocus;

                if (actor.RaiseEvent(eva))
                    SetScrollFocus(oldScrollFocus);

                Pools<ScrollFocusChangedEventArgs>.Release(eva);
            }
        }

        public Actor GetScrollFocus ()
        {
            return _scrollFocus;
        }

        public float Width { get; private set; }

        public float Height { get; private set; }

        public float GutterWidth { get; private set; }

        public float GutterHeight { get; private set; }

        public GdxSpriteBatch SpriteBatch { get; private set; }

        public Camera Camera { get; set; }

        public Group Root { get; private set; }

        public ScissorStack ScissorStack { get; private set; }

        public Actor Hit (float stageX, float stageY, bool touchable)
        {
            Vector2 actorCoords = Root.ParentToLocalCoordinates(new Vector2(stageX, stageY));
            return Root.Hit(actorCoords.X, actorCoords.Y, touchable);
        }

        public Vector2 ScreenToStageCoordinates (Vector2 screenCoords)
        {
            Vector3 stageCoords = Camera.Unproject(new Vector3(screenCoords, 0), _viewportX, _viewportY, _viewportWidth, _viewportHeight);
            return new Vector2(stageCoords.X, stageCoords.Y);
        }

        public Vector2 StageToScreenCoordinates (Vector2 stageCoords)
        {
            Vector3 screenCoords = Camera.Project(new Vector3(stageCoords, 0), _viewportX, _viewportY, _viewportWidth, _viewportHeight);
            return new Vector2(screenCoords.X, _viewportHeight - screenCoords.Y);
        }

        public Vector2 ToScreenCoordinates (Vector2 coords, Matrix transformMatrix)
        {
            return ScissorStack.ToWindowCoordinates(Camera, transformMatrix, coords);
        }

        public void Dispose ()
        {
            Clear();
            if (_ownsSpriteBatch)
                SpriteBatch.Dispose();
        }
    }
}
