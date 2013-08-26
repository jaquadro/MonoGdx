using System;
using System.Collections.Generic;
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

        private readonly SnapshotList<TouchFocus> _touchFocuses = new SnapshotList<TouchFocus>(4);

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
                        InputEvent ev = Pools<InputEvent>.Obtain();
                        ev.Type = InputType.Exit;
                        ev.Stage = this;
                        ev.StageX = stageCoords.X;
                        ev.StageY = stageCoords.Y;
                        ev.RelatedActor = overLast;
                        ev.Pointer = pointer;

                        overLast.Fire(ev);
                        Pools<InputEvent>.Release(ev);
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

            InputEvent ev = Pools<InputEvent>.Obtain();
            ev.Stage = this;
            ev.StageX = stageCoords.X;
            ev.StageY = stageCoords.Y;
            ev.Pointer = pointer;

            // Exit overLast
            if (overLast != null) {
                ev.Type = InputType.Exit;
                ev.RelatedActor = over;
                overLast.Fire(ev);
            }

            // Enter over
            if (over != null) {
                ev.Type = InputType.Enter;
                ev.RelatedActor = overLast;
                over.Fire(ev);
            }

            Pools<InputEvent>.Release(ev);
            return over;
        }

        public override bool TouchDown (int screenX, int screenY, int pointer, int button)
        {
            _pointerTouched[pointer] = true;
            _pointerScreen[pointer] = new Point(screenX, screenY);

            Vector2 stageCoords = ScreenToStageCoordinates(new Vector2(screenX, screenY));

            InputEvent ev = Pools<InputEvent>.Obtain();
            ev.Type = InputType.TouchDown;
            ev.Stage = this;
            ev.StageX = stageCoords.X;
            ev.StageY = stageCoords.Y;
            ev.Pointer = pointer;
            ev.Button = button;

            Actor target = Hit(stageCoords.X, stageCoords.Y, true);
            if (target == null)
                target = Root;

            target.Fire(ev);
            bool handled = ev.IsHandled;

            Pools<InputEvent>.Release(ev);
            return handled;
        }

        public override bool TouchDragged (int screenX, int screenY, int pointer)
        {
            _pointerScreen[pointer] = new Point(screenX, screenY);

            if (_touchFocuses.Count == 0)
                return false;

            Vector2 stageCoords = ScreenToStageCoordinates(new Vector2(screenX, screenY));

            InputEvent ev = Pools<InputEvent>.Obtain();
            ev.Type = InputType.TouchDragged;
            ev.Stage = this;
            ev.StageX = stageCoords.X;
            ev.StageY = stageCoords.Y;
            ev.Pointer = pointer;

            IList<TouchFocus> focuses = _touchFocuses.Begin();
            for (int i = 0, n = _touchFocuses.Count; i < n; i++) {
                TouchFocus focus = focuses[i];
                if (focus.Pointer != pointer)
                    continue;

                ev.TargetActor = focus.TargetActor;
                ev.ListenerActor = focus.ListenerActor;
                if (focus.Listener.Handle(ev))
                    ev.Handle();
            }
            _touchFocuses.End();

            bool handled = ev.IsHandled;

            Pools<InputEvent>.Release(ev);
            return handled;
        }

        public override bool TouchUp (int screenX, int screenY, int pointer, int button)
        {
            _pointerTouched[pointer] = false;
            _pointerScreen[pointer] = new Point(screenX, screenY);

            if (_touchFocuses.Count == 0)
                return false;

            Vector2 stageCoords = ScreenToStageCoordinates(new Vector2(screenX, screenY));

            InputEvent ev = Pools<InputEvent>.Obtain();
            ev.Type = InputType.TouchUp;
            ev.Stage = this;
            ev.StageX = stageCoords.X;
            ev.StageY = stageCoords.Y;
            ev.Pointer = pointer;
            ev.Button = button;

            IList<TouchFocus> focuses = _touchFocuses.Begin();
            for (int i = 0, n = _touchFocuses.Count; i < n; i++) {
                TouchFocus focus = focuses[i];
                if (focus.Pointer != pointer || focus.Button != button)
                    continue;
                if (!_touchFocuses.Remove(focus))
                    continue;

                ev.TargetActor = focus.TargetActor;
                ev.ListenerActor = focus.ListenerActor;
                if (focus.Listener.Handle(ev))
                    ev.Handle();

                Pools<TouchFocus>.Release(focus);
            }
            _touchFocuses.End();

            bool handled = ev.IsHandled;

            Pools<InputEvent>.Release(ev);
            return handled;
        }

        public override bool MouseMoved (int screenX, int screenY)
        {
            _mouseScreenX = screenX;
            _mouseScreenY = screenY;

            Vector2 stageCoords = ScreenToStageCoordinates(new Vector2(screenX, screenY));

            InputEvent ev = Pools<InputEvent>.Obtain();
            ev.Stage = this;
            ev.Type = InputType.MouseMoved;
            ev.StageX = stageCoords.X;
            ev.StageY = stageCoords.Y;

            Actor target = Hit(stageCoords.X, stageCoords.Y, true);
            if (target == null)
                target = Root;

            target.Fire(ev);
            bool handled = ev.IsHandled;

            Pools<InputEvent>.Release(ev);
            return handled;
        }

        public override bool Scrolled (int amount)
        {
            Actor target = (_scrollFocus == null) ? Root : _scrollFocus;

            Vector2 stageCoords = ScreenToStageCoordinates(new Vector2(_mouseScreenX, _mouseScreenY));

            InputEvent ev = Pools<InputEvent>.Obtain();
            ev.Stage = this;
            ev.Type = InputType.Scrolled;
            ev.ScrollAmount = amount;
            ev.StageX = stageCoords.X;
            ev.StageY = stageCoords.Y;

            target.Fire(ev);
            bool handled = ev.IsHandled;

            Pools<InputEvent>.Release(ev);
            return handled;
        }

        public override bool KeyDown (int keycode)
        {
            Actor target = (_keyboardFocus == null) ? Root : _keyboardFocus;

            InputEvent ev = Pools<InputEvent>.Obtain();
            ev.Stage = this;
            ev.Type = InputType.KeyDown;
            ev.KeyCode = keycode;

            target.Fire(ev);
            bool handled = ev.IsHandled;

            Pools<InputEvent>.Release(ev);
            return handled;
        }

        public override bool KeyUp (int keycode)
        {
            Actor target = (_keyboardFocus == null) ? Root : _keyboardFocus;

            InputEvent ev = Pools<InputEvent>.Obtain();
            ev.Stage = this;
            ev.Type = InputType.KeyUp;
            ev.KeyCode = keycode;

            target.Fire(ev);
            bool handled = ev.IsHandled;

            Pools<InputEvent>.Release(ev);
            return handled;
        }

        public override bool KeyTyped (char character)
        {
            Actor target = (_keyboardFocus == null) ? Root : _keyboardFocus;

            InputEvent ev = Pools<InputEvent>.Obtain();
            ev.Stage = this;
            ev.Type = InputType.KeyTyped;
            ev.Character = character;

            target.Fire(ev);
            bool handled = ev.IsHandled;

            Pools<InputEvent>.Release(ev);
            return handled;
        }

        public void AddTouchFocus (EventListener listener, Actor listenerActor, Actor target, int pointer, int button)
        {
            TouchFocus focus = Pools<TouchFocus>.Obtain();
            focus.ListenerActor = listenerActor;
            focus.TargetActor = target;
            focus.Listener = listener;
            focus.Pointer = pointer;
            focus.Button = button;

            _touchFocuses.Add(focus);
        }

        public void RemoveTouchFocus (EventListener listener, Actor listenerActor, Actor target, int pointer, int button)
        {
            for (int i = _touchFocuses.Count - 1; i >= 0; i--) {
                TouchFocus focus = _touchFocuses[i];
                if (focus.Listener == listener 
                    && focus.ListenerActor == listenerActor 
                    && focus.TargetActor == target 
                    && focus.Pointer == pointer 
                    && focus.Button == button) {
                    _touchFocuses.RemoveAt(i);
                    Pools<TouchFocus>.Release(focus);
                }
            }
        }

        public void CancelTouchFocus ()
        {
            CancelTouchFocus(null, null);
        }

        public void CancelTouchFocus (EventListener listener, Actor actor)
        {
            InputEvent ev = Pools<InputEvent>.Obtain();
            ev.Stage = this;
            ev.Type = InputType.TouchUp;
            ev.StageX = float.MinValue;
            ev.StageY = float.MinValue;

            // Cancel all current touch focuses except for the specified listener, allowing for concurrent modification, and never
            // cancel the same focus twice.
            IList<TouchFocus> items = _touchFocuses.Begin();
            for (int i = 0, n = _touchFocuses.Count; i < n; i++) {
                TouchFocus focus = items[i];
                if (focus.Listener == listener && focus.ListenerActor == actor)
                    continue;
                if (!_touchFocuses.Remove(focus))
                    continue;

                ev.TargetActor = focus.TargetActor;
                ev.ListenerActor = focus.ListenerActor;
                ev.Pointer = focus.Pointer;
                ev.Button = focus.Button;
                focus.Listener.Handle(ev);
            }
            _touchFocuses.End();

            Pools<InputEvent>.Release(ev);
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

        public bool AddListener (EventListener listener)
        {
            return Root.AddListener(listener);
        }

        public bool RemoveListener (EventListener listener)
        {
            return Root.RemoveListener(listener);
        }

        public bool AddCaptureListener (EventListener listener)
        {
            return Root.AddCaptureListener(listener);
        }

        public bool RemoveCaptureListener (EventListener listener)
        {
            return Root.RemoveCaptureListener(listener);
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
            CancelTouchFocus();
        }

        public void Unfocus (Actor actor)
        {
            if (_scrollFocus != null && _scrollFocus.IsDescendentOf(actor))
                _scrollFocus = null;
            if (_keyboardFocus != null && _keyboardFocus.IsDescendentOf(actor))
                _keyboardFocus = null;
        }

        public void SetKeyboardFocus (Actor actor)
        {
            if (_keyboardFocus == actor)
                return;

            FocusEvent ev = Pools<FocusEvent>.Obtain();
            ev.Stage = this;
            ev.Type = FocusType.Keyboard;

            Actor oldKeyboardFocus = _keyboardFocus;
            if (oldKeyboardFocus != null) {
                ev.IsFocused = false;
                ev.RelatedActor = actor;
                oldKeyboardFocus.Fire(ev);
            }

            if (!ev.IsCancelled) {
                _keyboardFocus = actor;
                if (actor != null) {
                    ev.IsFocused = true;
                    ev.RelatedActor = oldKeyboardFocus;
                    actor.Fire(ev);
                    if (ev.IsCancelled)
                        SetKeyboardFocus(oldKeyboardFocus);
                }
            }

            Pools<FocusEvent>.Release(ev);
        }

        public Actor GetKeyboardFocus ()
        {
            return _keyboardFocus;
        }

        public void SetScrollFocus (Actor actor)
        {
            if (_scrollFocus == actor)
                return;

            FocusEvent ev = Pools<FocusEvent>.Obtain();
            ev.Stage = this;
            ev.Type = FocusType.Scroll;

            Actor oldScrollFocus = _scrollFocus;
            if (oldScrollFocus != null) {
                ev.IsFocused = false;
                ev.RelatedActor = actor;
                oldScrollFocus.Fire(ev);
            }

            if (!ev.IsCancelled) {
                _scrollFocus = actor;
                if (actor != null) {
                    ev.IsFocused = true;
                    ev.RelatedActor = oldScrollFocus;
                    actor.Fire(ev);
                    if (ev.IsCancelled)
                        SetScrollFocus(oldScrollFocus);
                }
            }

            Pools<FocusEvent>.Release(ev);
        }

        public Actor GetScrollFocus ()
        {
            return _scrollFocus;
        }

        public float Width { get; private set; }

        public float Height { get; private set; }

        public float GutterWidth { get; private set; }

        public float GutterHeight { get; private set; }

        public GdxSpriteBatch SpriteBatch
        {
            get { return SpriteBatch; }
        }

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

        internal class TouchFocus : IPoolable
        {
            public EventListener Listener { get; set; }

            public Actor ListenerActor { get; set; }
            public Actor TargetActor { get; set; }

            public int Pointer { get; set; }
            public int Button { get; set; }

            public void Reset ()
            {
                ListenerActor = null;
                TargetActor = null;
            }
        }
    }
}
