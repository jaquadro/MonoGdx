using Microsoft.Xna.Framework;
using MonoGdx.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGdx.Scene2D
{
    public delegate void RoutedEventHandler (Actor sender, RoutedEventArgs e);
    public delegate void TouchEventHandler (Actor sender, TouchEventArgs e);
    public delegate void SelectionChangedEventHandler (Actor sender, SelectionChangedEventArgs e);
    public delegate void RoutedPropertyChangedEventHandler<T> (Actor sender, RoutedPropertyChangedEventArgs<T> e);
    public delegate void KeyboardFocusChangedEventHandler (Actor sender, KeyboardFocusChangedEventArgs e);

    public enum RoutingStrategy
    {
        Bubble,
        Direct,
        Tunnel,
    }

    public sealed class RoutedEvent
    {
        internal RoutedEvent(RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
        {
            HandlerType = handlerType;
            OwnerType = ownerType;
            RoutingStrategy = routingStrategy;
            Index = EventManager.TakeNextIndex();
        }

        internal int Index { get; private set; }
        public Type HandlerType { get; private set; }
        public Type OwnerType { get; private set; }
        public RoutingStrategy RoutingStrategy { get; private set; }
    }

    public struct RoutedEventHandlerInfo
    {
        private readonly Delegate _handler;
        private readonly bool _capturingHandler;

        internal RoutedEventHandlerInfo (Delegate handler, bool capturingHandler)
        {
            _handler = handler;
            _capturingHandler = capturingHandler;
        }

        public Delegate Handler
        {
            get { return _handler; }
        }

        public bool IsCapturingHandler
        {
            get { return _capturingHandler; }
        }

        internal void InvokeHandler (Actor target, RoutedEventArgs e)
        {
            if (e.Handled && !IsCapturingHandler)
                return;

            if (Handler is RoutedEventHandler)
                ((RoutedEventHandler)Handler)(target, e);
            else
                e.InvokeHandler(Handler, target);
        }
    }

    public class RoutedEventArgs : EventArgs, IPoolable
    {
        private Actor _source;

        public bool Handled { get; set; }
        public bool Cancelled { get; set; }
        public bool Stopped { get; set; }
        public RoutedEvent RoutedEvent { get; set; }

        public Stage Stage { get; set; }
        public Actor OriginalSource { get; internal set; }
        public Actor Source
        {
            get { return _source; }
            set
            {
                _source = value;
                if (OriginalSource == null)
                    OriginalSource = value;
            }
        }

        public virtual void Reset ()
        {
            RoutedEvent = null;
            Stage = null;
            OriginalSource = null;
            Source = null;

            Handled = false;
            Cancelled = false;
            Stopped = false;
        }

        protected virtual void InvokeEventHandler (Delegate handler, Actor target)
        {
            if (handler is RoutedEventHandler)
                ((RoutedEventHandler)handler)(target, this);
            else
                handler.DynamicInvoke(target, this);
        }

        internal void InvokeHandler (Delegate handler, Actor target)
        {
            InvokeEventHandler(handler, target);
        }
    }

    public class RoutedPropertyChangedEventArgs<T> : RoutedEventArgs
    {
        public T NewValue { get; set; }
        public T OldValue { get; set; }

        public RoutedPropertyChangedEventArgs (T oldValue, T newValue)
        {
            NewValue = newValue;
            OldValue = oldValue;
        }

        public override void Reset ()
        {
            base.Reset();

            NewValue = default(T);
            OldValue = default(T);
        }

        protected override void InvokeEventHandler (Delegate handler, Actor target)
        {
            ((RoutedPropertyChangedEventHandler<T>)handler)(target, this);
        }
    }

    public class KeyboardFocusChangedEventArgs : RoutedEventArgs
    {
        public Actor OldFocus { get; internal set; }
        public Actor NewFocus { get; internal set; }

        public override void Reset ()
        {
            base.Reset();

            OldFocus = null;
            NewFocus = null;
        }

        protected override void InvokeEventHandler (Delegate handler, Actor target)
        {
            ((KeyboardFocusChangedEventHandler)handler)(target, this);
        }
    }

    public class TouchEventArgs : RoutedEventArgs
    {
        public int Pointer { get; internal set; }
        public int Button { get; internal set; }
        public Vector2 StagePosition { get; internal set; }

        public Vector2 GetPosition (Actor actor)
        {
            return actor.StageToLocalCoordinates(StagePosition);
        }

        protected override void InvokeEventHandler (Delegate handler, Actor target)
        {
            ((TouchEventHandler)handler)(target, this);
        }
    }

    public class SelectionChangedEventArgs : RoutedEventArgs
    {
        private static readonly IList _empty = new List<object>(0);

        private IList _added;
        private IList _removed;

        public SelectionChangedEventArgs (IList addedItems, IList removedItems)
        {
            _added = addedItems;
            _removed = removedItems;
        }

        public SelectionChangedEventArgs (object addedItem, object removedItem)
        {
            _added = (addedItem != null) ? new List<object>(1) { addedItem } : _empty;
            _removed = (removedItem != null) ? new List<object>(1) { removedItem } : _empty;
        }

        public IList AddedItems
        {
            get { return _added; }
        }

        public IList RemovedItems
        {
            get { return _removed; }
        }

        protected override void InvokeEventHandler (Delegate handler, Actor target)
        {
            ((SelectionChangedEventHandler)handler)(target, this);
        }
    }

    public static class EventManager
    {
        private static int _nextIndex = 0;
        private static HashSet<RoutedEvent> _registry = new HashSet<RoutedEvent>();

        private static Dictionary<RoutedEvent, Dictionary<Type, RoutedEventHandlerInfo>> _classHandlers = 
            new Dictionary<RoutedEvent, Dictionary<Type, RoutedEventHandlerInfo>>();

        public static RoutedEvent RegisterRoutedEvent(RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
        {
            //if (!typeof(Actor).IsAssignableFrom(ownerType))
            //    throw new ArgumentException("Owner Type must be derived from Actor");

            RoutedEvent rev = new RoutedEvent(routingStrategy, handlerType, ownerType);
            _registry.Add(rev);

            return rev;
        }

        public static void RegisterClassHandler (Type classType, RoutedEvent routedEvent, Delegate handler)
        {
            RegisterClassHandler(classType, routedEvent, handler, false);
        }

        public static void RegisterClassHandler (Type classType, RoutedEvent routedEvent, Delegate handler, bool capturingHandler)
        {
            Dictionary<Type, RoutedEventHandlerInfo> eventHandlers;
            if (!_classHandlers.TryGetValue(routedEvent, out eventHandlers))
                _classHandlers.Add(routedEvent, eventHandlers = new Dictionary<Type, RoutedEventHandlerInfo>());

            RoutedEventHandlerInfo handlerInfo = new RoutedEventHandlerInfo(handler, capturingHandler);
            eventHandlers[classType] = handlerInfo;
        }

        internal static void InvokeClassHandlers (Actor target, RoutedEventArgs e)
        {
            if (e.RoutedEvent == null)
                return;

            Dictionary<Type, RoutedEventHandlerInfo> eventHandlers;
            if (!_classHandlers.TryGetValue(e.RoutedEvent, out eventHandlers))
                return;

            foreach (var kvp in eventHandlers)
                kvp.Value.InvokeHandler(target, e);
        }

        internal static int TakeNextIndex ()
        {
            return _nextIndex++;
        }
    }


}
