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
        private readonly DelayedRemovalList<EventListener> _listeners = new DelayedRemovalList<EventListener>(0);
        private readonly DelayedRemovalList<EventListener> _captureListeners = new DelayedRemovalList<EventListener>(0);
        private readonly List<SceneAction> _actions = new List<SceneAction>(0);

        private Dictionary<int, DelayedRemovalList<RoutedEventHandlerInfo>> _handlers = new Dictionary<int, DelayedRemovalList<RoutedEventHandlerInfo>>(0);

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

        public bool Fire (Event ev)
        {
            if (ev.Stage == null)
                ev.Stage = Stage;
            ev.TargetActor = this;

            // Collect ancestors so event propagation is unaffected by hierarchy changes.
            List<Group> ancestors = Pools<List<Group>>.Obtain();
            Group parent = Parent;
            while (parent != null) {
                ancestors.Add(parent);
                parent = parent.Parent;
            }

            try {
                // Notify all parent capture listeners, starting at the root. Ancestors may stop an event before children receive it.
                for (int i = ancestors.Count - 1; i >= 0; i--) {
                    Group currentTarget = ancestors[i];
                    currentTarget.Notify(ev, true);
                    if (ev.IsStopped)
                        return ev.IsCancelled;
                }

                // Notify the target capture listeners.
                Notify(ev, true);
                if (ev.IsStopped)
                    return ev.IsCancelled;

                // Notify the target listeners.
                Notify(ev, false);
                if (!ev.Bubbles)
                    return ev.IsCancelled;
                if (ev.IsStopped)
                    return ev.IsCancelled;

                // Notify all parent listeners, starting at the target. Children may stop an event before ancestors receive it.
                foreach (Group ancestor in ancestors) {
                    ancestor.Notify(ev, false);
                    if (ev.IsStopped)
                        return ev.IsCancelled;
                }

                return ev.IsCancelled;
            }
            finally {
                ancestors.Clear();
                Pools<List<Group>>.Release(ancestors);
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
                        currentTarget.InvokeHandler(e);
                        if (e.Stopped)
                            return e.Cancelled;
                    }

                    InvokeHandler(e);
                    if (e.Stopped)
                        return e.Cancelled;
                }
                else if (e.RoutedEvent.RoutingStrategy == RoutingStrategy.Bubble) {
                    InvokeHandler(e);
                    if (e.Stopped)
                        return e.Cancelled;

                    // Notify all parent listeners, starting at the target. Children may stop an event before ancestors receive it.
                    foreach (Group ancestor in ancestors) {
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

        public bool Notify (Event ev, bool capture)
        {
            if (ev.TargetActor == null)
                throw new ArgumentException("The event target cannot be null.");

            DelayedRemovalList<EventListener> listeners = capture ? _captureListeners : _listeners;
            if (listeners.Count == 0)
                return ev.IsCancelled;

            ev.ListenerActor = this;
            ev.IsCapture = capture;
            if (ev.Stage == null)
                ev.Stage = Stage;

            listeners.Begin();
            foreach (EventListener listener in listeners) {
                if (listener.Handle(ev)) {
                    ev.Handle();
                    if (ev is InputEvent) {
                        InputEvent inputEvent = ev as InputEvent;
                        if (inputEvent.Type == InputType.TouchDown)
                            ev.Stage.AddTouchFocus(listener, this, inputEvent.TargetActor, inputEvent.Pointer, inputEvent.Button);
                    }
                }
            }
            listeners.End();

            return ev.IsCancelled;
        }

        internal bool InvokeHandler (RoutedEventArgs args)
        {
            DelayedRemovalList<RoutedEventHandlerInfo> handlerList;
            if (!_handlers.TryGetValue(args.RoutedEvent.Index, out handlerList) || handlerList.Count == 0)
                return args.Cancelled;

            if (args.Stage == null)
                args.Stage = Stage;

            handlerList.Begin();
            foreach (var handlerInfo in handlerList) {
                handlerInfo.InvokeHandler(this, args);
                // TouchDownEvent specialization
            }
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

        public bool AddListener (EventListener listener)
        {
            if (!_listeners.Contains(listener)) {
                _listeners.Add(listener);
                return true;
            }

            return false;
        }

        public bool RemoveListener (EventListener listener)
        {
            return _listeners.Remove(listener);
        }

        public IList<EventListener> Listeners
        {
            get { return _listeners; }
        }

        public bool AddCaptureListener (EventListener listener)
        {
            if (!_captureListeners.Contains(listener)) {
                _captureListeners.Add(listener);
                return true;
            }

            return false;
        }

        public bool RemoveCaptureListener (EventListener listener)
        {
            return _captureListeners.Remove(listener);
        }

        public IList<EventListener> CaptureListeners
        {
            get { return _captureListeners; }
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

        public void ClearListeners ()
        {
            _listeners.Clear();
            _captureListeners.Clear();
        }

        public virtual void Clear ()
        {
            ClearActions();
            ClearListeners();
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
