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
    public delegate void ScrollFocusChangedEventHandler (Actor sender, ScrollFocusChangedEventArgs e);

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

    public class ScrollFocusChangedEventArgs : RoutedEventArgs
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
            ((ScrollFocusChangedEventHandler)handler)(target, this);
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
        private class ClassHandlerNode
        {
            public Type Class;
            public RoutedEventHandlerInfo Handler;
            public List<ClassHandlerNode> SubClassHandlers = new List<ClassHandlerNode>();
        }

        private static int _nextIndex = 0;
        private static HashSet<RoutedEvent> _registry = new HashSet<RoutedEvent>();

        private static Dictionary<RoutedEvent, List<ClassHandlerNode>> _classHandlers = 
            new Dictionary<RoutedEvent, List<ClassHandlerNode>>();

        public static RoutedEvent RegisterRoutedEvent(RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
        {
            RoutedEvent rev = new RoutedEvent(routingStrategy, handlerType, ownerType);
            _registry.Add(rev);

            return rev;
        }

        public static void RegisterClassHandler (Type classType, RoutedEvent routedEvent, Delegate handler)
        {
            RegisterClassHandler(classType, routedEvent, handler, false);
        }

        public static void RegisterClassHandler (Type classType, RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
        {
            List<ClassHandlerNode> rootNodes;
            if (!_classHandlers.TryGetValue(routedEvent, out rootNodes))
                _classHandlers.Add(routedEvent, rootNodes = new List<ClassHandlerNode>());

            RoutedEventHandlerInfo handlerInfo = new RoutedEventHandlerInfo(handler, handledEventsToo);

            foreach (ClassHandlerNode node in rootNodes) {
                ClassHandlerNode resultNode = PlaceInNode(node, classType, handlerInfo);
                if (resultNode == null)
                    continue;

                if (resultNode != node) {
                    rootNodes.Remove(node);
                    rootNodes.Add(resultNode);
                }

                return;
            }

            ClassHandlerNode newNode = new ClassHandlerNode() {
                Class = classType,
                Handler = handlerInfo,
            };

            rootNodes.Add(newNode);
        }

        private static ClassHandlerNode PlaceInNode (ClassHandlerNode node, Type classType, RoutedEventHandlerInfo handler)
        {
            // If classes are the same, replace existing handler
            Type nodeType = node.Class.GetType();
            if (nodeType == classType) {
                node.Handler = handler;
                return node;
            }

            // If new class is a subclass of the node, insert it
            if (classType.IsSubclassOf(nodeType)) {
                foreach (ClassHandlerNode subNode in node.SubClassHandlers) {
                    ClassHandlerNode resultNode = PlaceInNode(subNode, classType, handler);
                    if (resultNode == null)
                        continue;

                    if (resultNode != subNode) {
                        node.SubClassHandlers.Remove(subNode);
                        node.SubClassHandlers.Add(resultNode);
                    }

                    return node;
                }

                // No interior place found to insert node
                ClassHandlerNode newNode = new ClassHandlerNode() {
                    Class = classType,
                    Handler = handler,
                };

                node.SubClassHandlers.Add(newNode);
                return node;
            }

            // If the new node is a super of the node, rearrange
            if (nodeType.IsSubclassOf(classType)) {
                ClassHandlerNode newNode = new ClassHandlerNode() {
                    Class = classType,
                    Handler = handler,
                };

                newNode.SubClassHandlers.Add(node);
                return newNode;
            }

            return null;
        }

        internal static void InvokeClassHandlers (Actor target, RoutedEventArgs e)
        {
            if (e.RoutedEvent == null)
                return;

            List<ClassHandlerNode> rootNodes;
            if (!_classHandlers.TryGetValue(e.RoutedEvent, out rootNodes))
                return;

            foreach (var node in rootNodes)
                InvokeClassHandlers(node, target, e);
        }

        private static void InvokeClassHandlers (ClassHandlerNode node, Actor target, RoutedEventArgs e)
        {
            foreach (var subNode in node.SubClassHandlers)
                InvokeClassHandlers(subNode, target, e);

            node.Handler.InvokeHandler(target, e);
        }

        internal static int TakeNextIndex ()
        {
            return _nextIndex++;
        }
    }


}
