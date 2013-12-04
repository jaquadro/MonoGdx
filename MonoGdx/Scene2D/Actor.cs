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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D
{
    public enum Touchable
    {
        Enabled,
        Disabled,
        ChildrenOnly,
    }

    public class Actor
    {
        //public static readonly RoutedEvent GotKeyboardFocusEvent = Stage.GotKeyboardFocusEvent;
        //public static readonly RoutedEvent LostKeyboardFocusEvent = Stage.LostKeyboardFocusEvent;

        private readonly List<SceneAction> _actions = new List<SceneAction>(0);

        private Dictionary<int, DelayedRemovalList<RoutedEventHandlerInfo>> _handlers = new Dictionary<int, DelayedRemovalList<RoutedEventHandlerInfo>>(0);

        static Actor ()
        {
            EventManager.RegisterClassHandler(typeof(Actor), Stage.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(GotKeyboardFocusClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(LostKeyboardFocusClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.GotScrollFocusEvent, new ScrollFocusChangedEventHandler(GotScrollFocusClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.LostScrollFocusEvent, new ScrollFocusChangedEventHandler(LostScrollFocusClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.GotTouchCaptureEvent, new TouchEventHandler(GotTouchCaptureClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.LostTouchCaptureEvent, new TouchEventHandler(LostTouchCaptureClass));

            EventManager.RegisterClassHandler(typeof(Actor), Stage.PreviewKeyDownEvent, new KeyEventHandler(PreviewKeyDownClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.KeyDownEvent, new KeyEventHandler(KeyDownClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.PreviewKeyUpEvent, new KeyEventHandler(PreviewKeyUpClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.KeyUpEvent, new KeyEventHandler(KeyUpClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.PreviewMouseMoveEvent, new MouseEventHandler(PreviewMouseMoveClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.MouseMoveEvent, new MouseEventHandler(MouseMoveClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.PreviewTouchDownEvent, new TouchEventHandler(PreviewTouchDownClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.TouchDownEvent, new TouchEventHandler(TouchDownClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.PreviewTouchDragEvent, new TouchEventHandler(PreviewTouchDragClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.TouchDragEvent, new TouchEventHandler(TouchDragClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.PreviewTouchUpEvent, new TouchEventHandler(PreviewTouchUpClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.TouchUpEvent, new TouchEventHandler(TouchUpClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.TouchEnterEvent, new TouchEventHandler(TouchEnterClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.TouchLeaveEvent, new TouchEventHandler(TouchLeaveClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.PreviewScrollEvent, new ScrollEventHandler(PreviewScrollClass));
            EventManager.RegisterClassHandler(typeof(Actor), Stage.ScrollEvent, new ScrollEventHandler(ScrollClass));
        }

        public Actor ()
        {
            Touchable = Touchable.Enabled;
            ScaleX = 1;
            ScaleY = 1;
            IsVisible = true;
            Color = Color.White;
        }

        public virtual void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        { }

        public virtual void Act (float delta)
        {
            for (int i = 0; i < _actions.Count; i++) {
                SceneAction action = _actions[i];
                if (action.Act(delta)) {
                    _actions.RemoveAt(i);
                    action.Actor = null;
                    i--;
                }
            }
        }

        public bool RaiseEvent (RoutedEventArgs e)
        {
            if (e.Stage == null)
                e.Stage = Stage;

            if (e.OriginalSource == null) {
                e.OriginalSource = this;
                e.Source = this;
            }

            if (e.RoutedEvent.RoutingStrategy == RoutingStrategy.Direct) {
                EventManager.InvokeClassHandlers(this, e);
                if (!e.Stopped)
                    InvokeHandler(e);
                return e.Cancelled;
            }

            // Collect ancestors so event propagation is unaffected by hierarchy changes.
            List<Group> ancestors = Pools<List<Group>>.Obtain();
            Group parent = Parent;
            while (parent != null) {
                ancestors.Add(parent);
                parent = parent.Parent;
            }

            try {
                if (e.RoutedEvent.RoutingStrategy == RoutingStrategy.Tunnel) {
                    // Notify all parent capture listeners, starting at the root. Ancestors may stop an event before children receive it.
                    for (int i = ancestors.Count - 1; i >= 0; i--) {
                        Group currentTarget = ancestors[i];
                        EventManager.InvokeClassHandlers(currentTarget, e);
                        if (!e.Stopped)
                            currentTarget.InvokeHandler(e);
                        if (e.Stopped)
                            return e.Cancelled;
                    }

                    EventManager.InvokeClassHandlers(this, e);
                    if (!e.Stopped)
                        InvokeHandler(e);
                    if (e.Stopped)
                        return e.Cancelled;
                }
                else if (e.RoutedEvent.RoutingStrategy == RoutingStrategy.Bubble) {
                    EventManager.InvokeClassHandlers(this, e);
                    if (!e.Stopped)
                        InvokeHandler(e);
                    if (e.Stopped)
                        return e.Cancelled;

                    // Notify all parent listeners, starting at the target. Children may stop an event before ancestors receive it.
                    foreach (Group ancestor in ancestors) {
                        EventManager.InvokeClassHandlers(ancestor, e);
                        if (!e.Stopped)
                            ancestor.InvokeHandler(e);
                        if (e.Stopped)
                            return e.Cancelled;
                    }
                }

                return e.Cancelled;
            }
            finally {
                ancestors.Clear();
                Pools<List<Group>>.Release(ancestors);
            }
        }

        protected virtual RoutedEventArgs InitializeEventArgs (RoutedEventArgs e, RoutedEvent routedEvent)
        {
            e.RoutedEvent = routedEvent;
            e.OriginalSource = this;
            e.Source = this;

            return e;
        }

        internal bool InvokeHandler (RoutedEventArgs args)
        {
            DelayedRemovalList<RoutedEventHandlerInfo> handlerList;
            if (!_handlers.TryGetValue(args.RoutedEvent.Index, out handlerList) || handlerList.Count == 0)
                return args.Cancelled;

            if (args.Stage == null)
                args.Stage = Stage;

            handlerList.Begin();
            foreach (var handlerInfo in handlerList)
                handlerInfo.InvokeHandler(this, args);
            handlerList.End();

            return args.Cancelled;
        }

        public virtual Actor Hit (float x, float y, bool touchable)
        {
            if (touchable && Touchable != Scene2D.Touchable.Enabled)
                return null;

            return (x >= 0 && x < Width && y >= 0 && y < Height) ? this : null;
        }

        public bool Remove ()
        {
            if (Parent != null)
                return Parent.RemoveActor(this);
            return false;
        }

        public bool AddHandler (RoutedEvent routedEvent, Delegate handler)
        {
            return AddHandler(routedEvent, handler, false);
        }

        public bool AddHandler (RoutedEvent routedEvent, Delegate handler, bool capturing)
        {
            DelayedRemovalList<RoutedEventHandlerInfo> handlerList;
            if (!_handlers.TryGetValue(routedEvent.Index, out handlerList))
                _handlers.Add(routedEvent.Index, handlerList = new DelayedRemovalList<RoutedEventHandlerInfo>(1));

            foreach (var handlerInfo in handlerList) {
                if (handlerInfo.Handler == handler)
                    return false;
            }

            handlerList.Add(new RoutedEventHandlerInfo(handler, capturing));
            return true;
        }

        public bool RemoveHandler (RoutedEvent routedEvent, Delegate handler)
        {
            DelayedRemovalList<RoutedEventHandlerInfo> handlerList;
            if (!_handlers.TryGetValue(routedEvent.Index, out handlerList))
                return false;

            for (int i = 0; i < handlerList.Count; i++) {
                if (handlerList[i].Handler == handler) {
                    handlerList.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void AddAction (SceneAction action)
        {
            action.Actor = this;
            _actions.Add(action);
        }

        public void RemoveAction (SceneAction action)
        {
            if (_actions.Remove(action))
                action.Actor = null;
        }

        public IList<SceneAction> SceneActions
        {
            get { return _actions; }
        }

        public void ClearActions ()
        {
            foreach (SceneAction action in _actions)
                action.Actor = null;
            _actions.Clear();
        }

        public void ClearHandlers ()
        {
            _handlers.Clear();
        }

        public virtual void Clear ()
        {
            ClearActions();
            ClearHandlers();
        }

        public virtual Stage Stage { get; protected internal set; }

        public bool IsDescendentOf (Actor actor)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            Actor parent = this;
            while (true) {
                if (parent == null)
                    return false;
                if (parent == actor)
                    return true;
                parent = parent.Parent;
            }
        }

        public bool IsAscendentOf (Actor actor)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            while (true) {
                if (actor == null)
                    return false;
                if (actor == this)
                    return true;
                actor = actor.Parent;
            }
        }

        public event KeyEventHandler PreviewKeyDown
        {
            add { AddHandler(Stage.PreviewKeyDownEvent, value); }
            remove { AddHandler(Stage.PreviewKeyDownEvent, value); }
        }

        public event KeyEventHandler KeyDown
        {
            add { AddHandler(Stage.KeyDownEvent, value); }
            remove { AddHandler(Stage.KeyDownEvent, value); }
        }

        public event KeyEventHandler PreviewKeyUp
        {
            add { AddHandler(Stage.PreviewKeyUpEvent, value); }
            remove { AddHandler(Stage.PreviewKeyUpEvent, value); }
        }

        public event KeyEventHandler KeyUp
        {
            add { AddHandler(Stage.KeyUpEvent, value); }
            remove { AddHandler(Stage.KeyUpEvent, value); }
        }

        public event MouseEventHandler PreviewMouseMove
        {
            add { AddHandler(Stage.PreviewMouseMoveEvent, value); }
            remove { AddHandler(Stage.PreviewMouseMoveEvent, value); }
        }

        public event MouseEventHandler MouseMove
        {
            add { AddHandler(Stage.MouseMoveEvent, value); }
            remove { AddHandler(Stage.MouseMoveEvent, value); }
        }

        public event TouchEventHandler PreviewTouchDown
        {
            add { AddHandler(Stage.PreviewTouchDownEvent, value); }
            remove { AddHandler(Stage.PreviewTouchDownEvent, value); }
        }

        public event TouchEventHandler TouchDown
        {
            add { AddHandler(Stage.TouchDownEvent, value); }
            remove { AddHandler(Stage.TouchDownEvent, value); }
        }

        public event TouchEventHandler PreviewTouchDrag
        {
            add { AddHandler(Stage.PreviewTouchDragEvent, value); }
            remove { AddHandler(Stage.PreviewTouchDragEvent, value); }
        }

        public event TouchEventHandler TouchDrag
        {
            add { AddHandler(Stage.TouchDragEvent, value); }
            remove { AddHandler(Stage.TouchDragEvent, value); }
        }

        public event TouchEventHandler PreviewTouchUp
        {
            add { AddHandler(Stage.PreviewTouchUpEvent, value); }
            remove { AddHandler(Stage.PreviewTouchUpEvent, value); }
        }

        public event TouchEventHandler TouchUp
        {
            add { AddHandler(Stage.TouchUpEvent, value); }
            remove { AddHandler(Stage.TouchUpEvent, value); }
        }

        public event TouchEventHandler TouchEnter
        {
            add { AddHandler(Stage.TouchEnterEvent, value); }
            remove { AddHandler(Stage.TouchEnterEvent, value); }
        }

        public event TouchEventHandler TouchLeave
        {
            add { AddHandler(Stage.TouchLeaveEvent, value); }
            remove { AddHandler(Stage.TouchLeaveEvent, value); }
        }

        public event ScrollEventHandler PreviewScroll
        {
            add { AddHandler(Stage.PreviewScrollEvent, value); }
            remove { AddHandler(Stage.PreviewScrollEvent, value); }
        }

        public event ScrollEventHandler Scroll
        {
            add { AddHandler(Stage.ScrollEvent, value); }
            remove { AddHandler(Stage.ScrollEvent, value); }
        }

        private static void PreviewKeyDownClass (Actor sender, KeyEventArgs e)
        {
            if (sender != null)
                sender.OnPreviewKeyDown(e);
        }

        private static void KeyDownClass (Actor sender, KeyEventArgs e)
        {
            if (sender != null)
                sender.OnKeyDown(e);
        }

        private static void PreviewKeyUpClass (Actor sender, KeyEventArgs e)
        {
            if (sender != null)
                sender.OnPreviewKeyUp(e);
        }

        private static void KeyUpClass (Actor sender, KeyEventArgs e)
        {
            if (sender != null)
                sender.OnKeyUp(e);
        }

        private static void PreviewMouseMoveClass (Actor sender, MouseEventArgs e)
        {
            if (sender != null)
                sender.OnPreviewMouseMove(e);
        }

        private static void MouseMoveClass (Actor sender, MouseEventArgs e)
        {
            if (sender != null)
                sender.OnMouseMove(e);
        }

        private static void PreviewTouchDownClass (Actor sender, TouchEventArgs e)
        {
            if (sender != null)
                sender.OnPreviewTouchDown(e);
        }

        private static void TouchDownClass (Actor sender, TouchEventArgs e)
        {
            if (sender != null)
                sender.OnTouchDown(e);
        }

        private static void PreviewTouchDragClass (Actor sender, TouchEventArgs e)
        {
            if (sender != null)
                sender.OnPreviewTouchDrag(e);
        }

        private static void TouchDragClass (Actor sender, TouchEventArgs e)
        {
            if (sender != null)
                sender.OnTouchDrag(e);
        }

        private static void PreviewTouchUpClass (Actor sender, TouchEventArgs e)
        {
            if (sender != null)
                sender.OnPreviewTouchUp(e);
        }

        private static void TouchUpClass (Actor sender, TouchEventArgs e)
        {
            if (sender != null)
                sender.OnTouchUp(e);
        }

        private static void TouchEnterClass (Actor sender, TouchEventArgs e)
        {
            if (sender != null)
                sender.OnTouchEnter(e);
        }

        private static void TouchLeaveClass (Actor sender, TouchEventArgs e)
        {
            if (sender != null)
                sender.OnTouchLeave(e);
        }

        private static void PreviewScrollClass (Actor sender, ScrollEventArgs e)
        {
            if (sender != null)
                sender.OnPreviewScroll(e);
        }

        private static void ScrollClass (Actor sender, ScrollEventArgs e)
        {
            if (sender != null)
                sender.OnScroll(e);
        }

        protected virtual void OnPreviewKeyDown (KeyEventArgs e)
        { }

        protected virtual void OnKeyDown (KeyEventArgs e)
        { }

        protected virtual void OnPreviewKeyUp (KeyEventArgs e)
        { }

        protected virtual void OnKeyUp (KeyEventArgs e)
        { }

        protected virtual void OnPreviewMouseMove (MouseEventArgs e)
        { }

        protected virtual void OnMouseMove (MouseEventArgs e)
        { }

        protected virtual void OnPreviewTouchDown (TouchEventArgs e)
        { }

        protected virtual void OnTouchDown (TouchEventArgs e)
        { }

        protected virtual void OnPreviewTouchDrag (TouchEventArgs e)
        { }

        protected virtual void OnTouchDrag (TouchEventArgs e)
        { }

        protected virtual void OnPreviewTouchUp (TouchEventArgs e)
        { }

        protected virtual void OnTouchUp (TouchEventArgs e)
        { }

        protected virtual void OnTouchEnter (TouchEventArgs e)
        { }

        protected virtual void OnTouchLeave (TouchEventArgs e)
        { }

        protected virtual void OnPreviewScroll (ScrollEventArgs e)
        { }

        protected virtual void OnScroll (ScrollEventArgs e)
        { }

        public bool IsKeyboardFocused
        {
            get { return Stage != null && Stage.GetKeyboardFocus() == this; }
        }

        public bool IsScrollFocused
        {
            get { return Stage != null && Stage.GetScrollFocus() == this; }
        }

        public void CaptureTouch (int pointer)
        {
            if (Stage == null)
                throw new InvalidOperationException("Only an actor attached to a Stage can capture devices.");

            Stage.SetTouchCapture(this, pointer);
        }

        public void ReleaseTouchCapture (int pointer)
        {
            if (Stage == null)
                throw new InvalidOperationException("Only an actor attached to a Stage can capture devices.");

            if (Stage.GetTouchCapture(pointer) != this)
                return;

            Stage.SetTouchCapture(null, pointer);
        }

        public event KeyboardFocusChangedEventHandler GotKeyboardFocus
        {
            add { AddHandler(Stage.GotKeyboardFocusEvent, value); }
            remove { RemoveHandler(Stage.GotKeyboardFocusEvent, value); }
        }

        public event KeyboardFocusChangedEventHandler LostKeyboardFocus
        {
            add { AddHandler(Stage.LostKeyboardFocusEvent, value); }
            remove { RemoveHandler(Stage.LostKeyboardFocusEvent, value); }
        }

        public event ScrollFocusChangedEventHandler GotScrollFocus
        {
            add { AddHandler(Stage.GotScrollFocusEvent, value); }
            remove { RemoveHandler(Stage.GotScrollFocusEvent, value); }
        }

        public event ScrollFocusChangedEventHandler LostScrollFocus
        {
            add { AddHandler(Stage.LostScrollFocusEvent, value); }
            remove { RemoveHandler(Stage.LostScrollFocusEvent, value); }
        }

        public event TouchEventHandler GotTouchCapture
        {
            add { AddHandler(Stage.GotTouchCaptureEvent, value); }
            remove { RemoveHandler(Stage.GotTouchCaptureEvent, value); }
        }

        public event TouchEventHandler LostTouchCapture
        {
            add { AddHandler(Stage.LostTouchCaptureEvent, value); }
            remove { RemoveHandler(Stage.LostTouchCaptureEvent, value); }
        }

        private static void GotKeyboardFocusClass (Actor sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender != null)
                sender.OnGotKeyboardFocus(e);
        }

        private static void LostKeyboardFocusClass (Actor sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender != null)
                sender.OnLostKeyboardFocus(e);
        }

        private static void GotScrollFocusClass (Actor sender, ScrollFocusChangedEventArgs e)
        {
            if (sender != null)
                sender.OnGotScrollFocus(e);
        }

        private static void LostScrollFocusClass (Actor sender, ScrollFocusChangedEventArgs e)
        {
            if (sender != null)
                sender.OnLostScrollFocus(e);
        }

        private static void GotTouchCaptureClass (Actor sender, TouchEventArgs e)
        {
            if (sender != null)
                sender.OnGotTouchCapture(e);
        }

        private static void LostTouchCaptureClass (Actor sender, TouchEventArgs e)
        {
            if (sender != null)
                sender.OnLostTouchCapture(e);
        }

        protected virtual void OnGotKeyboardFocus (KeyboardFocusChangedEventArgs e)
        { }

        protected virtual void OnLostKeyboardFocus (KeyboardFocusChangedEventArgs e)
        { }

        protected virtual void OnGotScrollFocus (ScrollFocusChangedEventArgs e)
        { }

        protected virtual void OnLostScrollFocus (ScrollFocusChangedEventArgs e)
        { }

        protected virtual void OnGotTouchCapture (TouchEventArgs e)
        { }

        protected virtual void OnLostTouchCapture (TouchEventArgs e)
        { }

        public bool HasParent
        {
            get { return Parent != null; }
        }

        public virtual Group Parent { get; protected internal set; }

        public Touchable Touchable { get; set; }

        public bool IsVisible { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public void SetPosition (float x, float y)
        {
            X = x;
            Y = y;
        }

        public void Translate (float x, float y)
        {
            X += x;
            Y += y;
        }

        public float Width { get; set; }

        public float Height { get; set; }

        public float Top
        {
            get { return Y + Height; }
        }

        public float Right
        {
            get { return X + Width; }
        }

        public void SetSize (float width, float height)
        {
            Width = width;
            Height = height;
        }

        public void Size (float size)
        {
            Width += size;
            Height += size;
        }

        public void Size (float width, float height)
        {
            Width += width;
            Height += height;
        }

        public void SetBounds (float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float OriginX { get; set; }

        public float OriginY { get; set; }

        public void SetOrigin (float originX, float originY)
        {
            OriginX = originX;
            OriginY = originY;
        }

        public float ScaleX { get; set; }

        public float ScaleY { get; set; }

        public void SetScale (float scale)
        {
            ScaleX = scale;
            ScaleY = scale;
        }

        public void SetScale (float scaleX, float scaleY)
        {
            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        public void Scale (float scale)
        {
            ScaleX += scale;
            ScaleY += scale;
        }

        public void Scale (float scaleX, float ScaleY)
        {
            ScaleX += scaleX;
            ScaleY += ScaleY;
        }

        public float Rotation { get; set; }

        public void Rotate (float amount)
        {
            Rotation += amount;
        }

        public Color Color { get; set; }

        public string Name { get; set; }

        public void ToFront ()
        {
            ZIndex = int.MaxValue;
        }

        public void ToBack ()
        {
            ZIndex = 0;
        }

        public int ZIndex
        {
            get
            {
                if (Parent == null)
                    return -1;
                return Parent.Children.IndexOf(this);
            }

            set
            {
                if (value < 0)
                    throw new ArgumentException("ZIndex cannot be < 0");

                if (Parent == null)
                    return;

                IList<Actor> children = Parent.Children;
                if (children.Count == 1)
                    return;
                if (!children.Remove(this))
                    return;

                if (value >= children.Count)
                    children.Add(this);
                else
                    children.Insert(value, this);
            }
        }

        public bool ClipBegin ()
        {
            return ClipBegin(X, Y, Width, Height);
        }

        public bool ClipBegin (float x, float y, float width, float height)
        {
            Rectangle tableBounds = new Rectangle((int)x, (int)y, (int)width, (int)height);
            Rectangle scissorBounds = ScissorStack.CalculateScissors(Stage.Camera, Stage.SpriteBatch.TransformMatrix, tableBounds);

            return Stage.ScissorStack.PushScissors(scissorBounds);
        }

        public void ClipEnd ()
        {
            Stage.ScissorStack.PopScissors();
        }

        public Vector2 ScreenToLocalCoordinates (Vector2 screenCoords)
        {
            if (Stage == null)
                return screenCoords;

            return StageToLocalCoordinates(Stage.ScreenToStageCoordinates(screenCoords));
        }

        public Vector2 StageToLocalCoordinates (Vector2 stageCoords)
        {
            if (Parent == null)
                return stageCoords;

            stageCoords = Parent.StageToLocalCoordinates(stageCoords);
            return ParentToLocalCoordinates(stageCoords);
        }

        public Vector2 LocalToStageCoordinates (Vector2 localCoords)
        {
            return LocalToAscendantCoordinates(null, localCoords);
        }

        public Vector2 LocalToParentCoordinates (Vector2 localCoords)
        {
            float rotation = -Rotation;

            if (Rotation == 0) {
                if (ScaleX == 1 && ScaleY == 1) {
                    localCoords.X += X;
                    localCoords.Y += Y;
                }
                else {
                    localCoords.X = (localCoords.X - OriginX) * ScaleX + OriginX + X;
                    localCoords.Y = (localCoords.Y - OriginY) * ScaleY + OriginY + Y;
                }
            }
            else {
                float cos = (float)Math.Cos(rotation);
                float sin = (float)Math.Sin(rotation);
                float tox = localCoords.X - OriginX;
                float toy = localCoords.Y - OriginY;

                localCoords.X = (tox * cos + toy * sin) * ScaleX + OriginX + X;
                localCoords.Y = (tox * -sin + toy * cos) * ScaleY + OriginY + Y;
            }

            return localCoords;
        }

        public Vector2 LocalToAscendantCoordinates (Actor ascendant, Vector2 localCoords)
        {
            Actor actor = this;
            while (actor.Parent != null) {
                localCoords = actor.LocalToParentCoordinates(localCoords);
                actor = actor.Parent;

                if (actor == ascendant)
                    break;
            }

            return localCoords;
        }

        public Vector2 ParentToLocalCoordinates (Vector2 parentCoords)
        {
            float rotation = Rotation;

            if (rotation == 0) {
                if (ScaleX == 1 && ScaleY == 1) {
                    parentCoords.X -= X;
                    parentCoords.Y -= Y;
                }
                else {
                    parentCoords.X = (parentCoords.X - X - OriginX) / ScaleX + OriginX;
                    parentCoords.Y = (parentCoords.Y - Y - OriginY) / ScaleY + OriginY;
                }
            }
            else {
                float cos = (float)Math.Cos(rotation);
                float sin = (float)Math.Sin(rotation);
                float tox = parentCoords.X - X - OriginX;
                float toy = parentCoords.Y - Y - OriginY;

                parentCoords.X = (tox * cos + toy * sin) / ScaleX + OriginX;
                parentCoords.Y = (tox * -sin + toy * cos) / ScaleY + OriginY; 
            }

            return parentCoords;
        }

        public override string ToString ()
        {
            string name = GetType().Name;
            int index = name.LastIndexOf('.');
            if (index != -1)
                name = name.Substring(index + 1);
            return name;
        }
    }
}
