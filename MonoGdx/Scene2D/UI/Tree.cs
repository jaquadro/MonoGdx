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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.UI
{
    public class Tree : WidgetGroup
    {
        public static readonly RoutedEvent SelectionChangedEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(Tree));

        private TreeStyle _style;
        private readonly SelectionChanger<TreeNode> _selectionChanger;
        private readonly List<TreeNode> _rootNodes = new List<TreeNode>();
        private readonly List<TreeNode> _selectedNodes = new List<TreeNode>();
        private float _indentSpacing;
        private float _leftColumnWidth;
        private float _prefWidth;
        private float _prefHeight;
        private bool _sizeInvalid = true;
        private TreeNode _foundNode;
        private TreeNode _overNode;
        private ClickListener _clickListener;

        private readonly List<TreeNode> _selectEventAdd = new List<TreeNode>();
        private readonly List<TreeNode> _selectEventRemove = new List<TreeNode>();

        public Tree (Skin skin)
            : this(skin.Get<TreeStyle>())
        { }

        public Tree (Skin skin, string styleName)
            : this(skin.Get<TreeStyle>(styleName))
        { }

        public Tree (TreeStyle style)
        {
            YSpacing = 4;
            IconSpacing = 2;
            MultiSelect = true;
            ToggleSelect = true;
            Style = style;

            _selectionChanger = new SelectionChanger<TreeNode>() {
                CanSelectMultiple = true,
                SelectionChangeHandler = SelectionChangeHandler,
            };

            Initialize();
        }

        private void Initialize ()
        {
            AddListener(_clickListener = new LocalClickListener(this));
        }

        private class LocalClickListener : ClickListener
        {
            private Tree _tree;
            public LocalClickListener (Tree tree)
            {
                _tree = tree;
            }

            public override void Clicked (InputEvent ev, float x, float y)
            {
                TreeNode node = _tree.NodeAt(y);
                if (node == null)
                    return;
                if (node != _tree.NodeAt(TouchDownY))
                    return;

                KeyboardState keystate = Keyboard.GetState();
                bool shiftPressed = keystate.IsKeyDown(Keys.LeftShift) || keystate.IsKeyDown(Keys.RightShift);
                bool ctrlPressed = keystate.IsKeyDown(Keys.LeftControl) || keystate.IsKeyDown(Keys.RightControl);

                if (_tree.MultiSelect && _tree._selectedNodes.Count > 0 && shiftPressed) {
                    float low = _tree._selectedNodes.First().Actor.Y;
                    float high = node.Actor.Y;

                    _tree._selectionChanger.Begin(_tree._selectedNodes);

                    if (!ctrlPressed)
                        _tree._selectionChanger.UnselectAll();

                    if (low > high)
                        _tree.SelectNodes(_tree._rootNodes, high, low);
                    else
                        _tree.SelectNodes(_tree._rootNodes, low, high);

                    _tree._selectionChanger.End();

                    _tree.FireChangeEvent();
                    return;
                }

                if (!_tree.MultiSelect || !ctrlPressed) {
                    if (_tree.Children.Count > 0) {
                        float rowX = node.Actor.X;
                        if (node.Icon != null)
                            rowX -= _tree.IconSpacing + node.Icon.MinWidth;

                        if (x < rowX) {
                            node.IsExpanded = !node.IsExpanded;
                            return;
                        }
                    }

                    if (!node.IsSelectable)
                        return;

                    bool unselect = _tree.ToggleSelect && _tree._selectedNodes.Count == 1 && _tree._selectedNodes.Contains(node);

                    _tree._selectionChanger.Begin(_tree._selectedNodes);
                    _tree._selectionChanger.UnselectAll();
                    if (!unselect)
                        _tree._selectionChanger.Select(node);
                    _tree._selectionChanger.End();

                    if (unselect) {
                        _tree.FireChangeEvent();
                    }

                    return;
                }
                else if (!node.IsSelectable)
                    return;

                _tree._selectionChanger.Begin(_tree._selectedNodes);
                _tree._selectionChanger.Select(node);
                _tree._selectionChanger.End();

                _tree.FireChangeEvent();
            }

            public override bool MouseMoved (InputEvent e, float x, float y)
            {
                _tree.OverNode = _tree.NodeAt(y);
                return false;
            }

            public override void Exit (InputEvent e, float x, float y, int pointer, Actor toActor)
            {
                base.Exit(e, x, y, pointer, toActor);
                if (toActor == null || !toActor.IsDescendentOf(_tree))
                    _tree.OverNode = null;
            }
        }

        private void SelectionChangeHandler (List<TreeNode> addedItems, List<TreeNode> removedItems) 
        {
            OnSelectionChanged(removedItems, addedItems);
        }

        public TreeStyle Style
        {
            get { return _style; }
            set
            {
                _style = value;
                _indentSpacing = Math.Max(_style.Plus.MinWidth, Style.Minus.MinWidth) + IconSpacing;
            }
        }

        public void Add (TreeNode node)
        {
            Insert(_rootNodes.Count, node);
        }

        public void Insert (int index, TreeNode node)
        {
            Remove(node);
            node.Parent = null;
            _rootNodes.Insert(index, node);
            node.AddToTree(this);
            InvalidateHierarchy();
        }

        public void Remove (TreeNode node)
        {
            if (node.Parent != null) {
                node.Parent.Remove(node);
                return;
            }

            _rootNodes.Remove(node);
            node.RemoveFromTree(this);
            InvalidateHierarchy();
        }

        public override void ClearChildren ()
        {
            base.ClearChildren();
            _rootNodes.Clear();
            _selectedNodes.Clear();
            OverNode = null;
            FireChangeEvent();
        }

        internal void FireChangeEvent ()
        {
            ChangeEvent ev = Pools<ChangeEvent>.Obtain();
            Fire(ev);
            Pools<ChangeEvent>.Release(ev);
        }

        public List<TreeNode> Nodes
        {
            get { return _rootNodes; }
        }

        public override void Invalidate ()
        {
            base.Invalidate();
            _sizeInvalid = true;
        }

        private void ComputeSize ()
        {
            _sizeInvalid = false;
            _prefWidth = Math.Max(_style.Plus.MinWidth, _style.Minus.MinWidth);
            _prefHeight = Height;
            _leftColumnWidth = 0;

            ComputeSize(_rootNodes, _indentSpacing);

            _leftColumnWidth += IconSpacing + Padding;
            _prefWidth += _leftColumnWidth + Padding;
            _prefHeight = Height - _prefHeight;
        }

        private void ComputeSize (List<TreeNode> nodes, float indent)
        {
            float ySpacing = YSpacing;
            foreach (TreeNode node in nodes) {
                float rowWidth = indent + IconSpacing;
                Actor actor = node.Actor;

                if (actor is ILayout) {
                    ILayout layout = actor as ILayout;
                    rowWidth += layout.PrefWidth;
                    node.Height = layout.PrefHeight;
                    layout.Pack();
                }
                else {
                    rowWidth += actor.Width;
                    node.Height = actor.Height;
                }

                if (node.Icon != null) {
                    rowWidth += IconSpacing * 2 + node.Icon.MinWidth;
                    node.Height = Math.Max(node.Height, node.Icon.MinHeight);
                }

                _prefWidth = Math.Max(_prefWidth, rowWidth);
                _prefHeight -= node.Height + YSpacing;

                if (node.IsExpanded)
                    ComputeSize(node.Children, indent + _indentSpacing);
            }
        }

        public override void Layout ()
        {
            if (_sizeInvalid)
                ComputeSize();
            Layout(_rootNodes, _leftColumnWidth + _indentSpacing + IconSpacing, Height - YSpacing / 2);
        }

        private float Layout (List<TreeNode> nodes, float indent, float y)
        {
            float ySpacing = YSpacing;
            float indentSpacing = _indentSpacing;
            ISceneDrawable plus = _style.Plus;
            ISceneDrawable minus = _style.Minus;

            foreach (TreeNode node in nodes) {
                Actor actor = node.Actor;
                float x = indent;
                if (node.Icon != null)
                    x += node.Icon.MinWidth;

                y -= node.Height;
                node.Actor.SetPosition(x, y);
                y -= ySpacing;

                if (node.IsExpanded)
                    y = Layout(node.Children, indent + indentSpacing, y);
            }

            return y;
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            if (_style.Background != null) {
                spriteBatch.Color = Color.MultiplyAlpha(parentAlpha);
                _style.Background.Draw(spriteBatch, X, Y, Width, Height);
                spriteBatch.Color = Microsoft.Xna.Framework.Color.White;
            }

            Draw(spriteBatch, _rootNodes, _leftColumnWidth);
            base.Draw(spriteBatch, parentAlpha);
        }

        private void Draw (GdxSpriteBatch spriteBatch, List<TreeNode> nodes, float indent)
        {
            ISceneDrawable plus = _style.Plus;
            ISceneDrawable minus = _style.Minus;
            float x = X;
            float y = Y;

            foreach (TreeNode node in nodes) {
                Actor actor = node.Actor;
                float iconY = 0;

                bool selected = _selectedNodes.Contains(node);
                if (selected && _style.Selection != null)
                    _style.Selection.Draw(spriteBatch, x, y + actor.Y - YSpacing / 2, Width, node.Height + YSpacing);
                else if (node == _overNode && _style.Over != null)
                    _style.Over.Draw(spriteBatch, x, y + actor.Y - YSpacing / 2, Width, node.Height + YSpacing);

                if (node.Icon != null) {
                    ISceneDrawable icon = node.Icon;
                    iconY = actor.Y + (float)Math.Round((node.Height - icon.MinHeight) / 2);
                    spriteBatch.Color = actor.Color;
                    icon.Draw(spriteBatch, x + node.Actor.X - IconSpacing - icon.MinWidth, y + iconY, icon.MinWidth, icon.MinHeight);
                    spriteBatch.Color = Microsoft.Xna.Framework.Color.White;
                }

                if (node.Children.Count == 0)
                    continue;

                ISceneDrawable expandIcon = node.IsExpanded ? minus : plus;
                if (selected)
                    expandIcon = node.IsExpanded ? _style.MinusSelection ?? minus : _style.PlusSelection ?? plus;

                iconY = actor.Y + (float)Math.Round((node.Height - expandIcon.MinHeight) / 2);
                expandIcon.Draw(spriteBatch, x + indent - IconSpacing, y + iconY, expandIcon.MinWidth, expandIcon.MinHeight);

                if (node.IsExpanded)
                    Draw(spriteBatch, node.Children, indent + _indentSpacing);
            }
        }

        public TreeNode NodeAt (float y)
        {
            _foundNode = null;
            NodeAt(_rootNodes, y, Height);
            return _foundNode;
        }

        private float NodeAt (List<TreeNode> nodes, float y, float rowY)
        {
            foreach (TreeNode node in nodes) {
                if (y >= rowY - node.Height - YSpacing && y < rowY) {
                    _foundNode = node;
                    return -1;
                }

                rowY -= node.Height + YSpacing;
                if (node.IsExpanded) {
                    rowY = NodeAt(node.Children, y, rowY);
                    if (rowY == -1)
                        return -1;
                }
            }

            return rowY;
        }

        internal void SelectNodes (List<TreeNode> nodes, float low, float high)
        {
            float ySpacing = YSpacing;
            foreach (TreeNode node in nodes) {
                if (node.Actor.Y < low)
                    break;
                if (!node.IsSelectable)
                    continue;
                if (node.Actor.Y <= high)
                    _selectionChanger.Select(node);
                    //_selectedNodes.Add(node);
                if (node.IsExpanded)
                    SelectNodes(node.Children, low, high);
            }
        }

        public List<TreeNode> Selection
        {
            get { return _selectedNodes; }
            set
            {
                _selectionChanger.Begin(_selectedNodes);
                _selectionChanger.UnselectAll();
                _selectionChanger.SelectAll(value);
                _selectionChanger.End();

                FireChangeEvent();
            }
        }

        public void SetSelection (TreeNode node)
        {
            _selectionChanger.Begin(_selectedNodes);
            if (node != null)
                _selectionChanger.SelectOnly(node);
            else
                _selectionChanger.UnselectAll();
            _selectionChanger.End();

            FireChangeEvent();
        }

        public void AddSelection (TreeNode node)
        {
            if (node == null)
                return;

            _selectionChanger.Begin(_selectedNodes);
            _selectionChanger.Select(node);
            _selectionChanger.End();

            FireChangeEvent();
        }

        public void ClearSelection ()
        {
            _selectionChanger.Begin(_selectedNodes);
            _selectionChanger.UnselectAll();
            _selectionChanger.End();

            FireChangeEvent();
        }

        public TreeNode OverNode { get; set; }

        public float Padding { get; set; }

        public float YSpacing { get; set; }

        public float IconSpacing { get; set; }

        public override float PrefWidth
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _prefWidth;
            }
        }

        public override float PrefHeight
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _prefHeight;
            }
        }

        public void FindExpandedObjects (IList objects)
        {
            FindExpandedObjects(_rootNodes, objects);
        }

        public void RestoreExpandedObjects (IList objects)
        {
            foreach (object obj in objects) {
                TreeNode node = FindNode(obj);
                if (node != null) {
                    node.IsExpanded = true;
                    node.ExpandTo();
                }
            }
        }

        internal static bool FindExpandedObjects (List<TreeNode> nodes, IList objects)
        {
            bool expanded = false;
            foreach (TreeNode node in nodes) {
                if (node.IsExpanded && !FindExpandedObjects(node.Children, objects))
                    objects.Add(node.Object);
            }
            return expanded; // Always returns false?
        }

        public TreeNode FindNode (object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("Object cannot be null.");
            return FindNode(_rootNodes, obj);
        }

        internal static TreeNode FindNode (List<TreeNode> nodes, object obj)
        {
            foreach (TreeNode node in nodes) {
                if (obj.Equals(node.Object))
                    return node;
            }

            foreach (TreeNode node in nodes) {
                TreeNode found = FindNode(node.Children, obj);
                if (found != null)
                    return found;
            }

            return null;
        }

        public void CollapseAll ()
        {
            CollapseAll(_rootNodes);
        }

        internal static void CollapseAll (List<TreeNode> nodes)
        {
            foreach (TreeNode node in nodes) {
                node.IsExpanded = false;
                CollapseAll(node.Children);
            }
        }

        public void ExpandAll ()
        {
            ExpandAll(_rootNodes);
        }

        internal static void ExpandAll (List<TreeNode> nodes)
        {
            foreach (TreeNode node in nodes)
                node.ExpandAll();
        }

        public ClickListener ClickListener
        {
            get { return _clickListener; }
        }

        public bool MultiSelect { get; set; }
        public bool ToggleSelect { get; set; }

        public event SelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        protected virtual void OnSelectionChanged (IList<TreeNode> oldSelection, IList<TreeNode> newSelection)
        {
            RaiseEvent(new SelectionChangedEventArgs(newSelection as IList, oldSelection as IList) {
                RoutedEvent = SelectionChangedEvent,
                OriginalSource = this,
                Source = this,
            });
        }
    }

    public class TreeNode
    {
        private Actor _actor;
        private TreeNode _parent;
        private readonly List<TreeNode> _children = new List<TreeNode>(0);
        private bool _expanded;
        private ISceneDrawable _icon;
        
        public TreeNode (Actor actor)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            IsSelectable = true;
            _actor = actor;
        }

        internal float Height { get; set; }

        public bool IsExpanded
        {
            get { return _expanded; }
            set
            {
                if (_expanded == value)
                    return;

                _expanded = value;
                if (_children.Count == 0)
                    return;

                Tree tree = Tree;
                if (tree == null)
                    return;

                if (_expanded) {
                    foreach (TreeNode node in _children)
                        node.AddToTree(tree);
                }
                else {
                    foreach (TreeNode node in _children)
                        node.RemoveFromTree(tree);
                }

                tree.InvalidateHierarchy();
            }
        }

        internal void AddToTree (Tree tree)
        {
            tree.AddActor(_actor);
            if (!_expanded)
                return;

            foreach (TreeNode node in _children)
                node.AddToTree(tree);
        }

        internal void RemoveFromTree (Tree tree)
        {
            tree.RemoveActor(_actor);
            if (!_expanded)
                return;

            foreach (TreeNode node in _children)
                node.RemoveFromTree(tree);
        }

        public void Add (TreeNode node)
        {
            Insert(_children.Count, node);
        }

        public void AddAll (IEnumerable<TreeNode> nodes)
        {
            foreach (TreeNode node in nodes)
                Insert(_children.Count, node);
        }

        public void Insert (int index, TreeNode node)
        {
            node._parent = this;
            _children.Insert(index, node);
            UpdateChildren();
        }

        public void Remove ()
        {
            Tree tree = Tree;
            if (tree != null)
                tree.Remove(this);
            else if (_parent != null)
                _parent.Remove(this);
        }

        public void Remove (TreeNode node)
        {
            _children.Remove(node);
            if (!_expanded)
                return;

            Tree tree = Tree;
            if (tree == null)
                return;

            node.RemoveFromTree(tree);
            if (_children.Count == 0)
                _expanded = false;
        }

        public void RemoveAll ()
        {
            Tree tree = Tree;
            if (tree != null) {
                foreach (TreeNode node in _children)
                    node.RemoveFromTree(tree);
            }

            _children.Clear();
        }

        public Tree Tree
        {
            get
            {
                Group parent = _actor.Parent;
                if (!(parent is Tree))
                    return null;
                return parent as Tree;
            }
        }

        public Actor Actor
        {
            get { return _actor; }
        }

        public List<TreeNode> Children
        {
            get { return _children; }
        }

        public void UpdateChildren ()
        {
            if (!_expanded)
                return;

            Tree tree = Tree;
            if (tree == null)
                return;

            foreach (TreeNode node in _children)
                node.AddToTree(tree);
        }

        public TreeNode Parent
        {
            get { return _parent; }
            internal set { _parent = value; }
        }

        public object Object { get; set; }

        public ISceneDrawable Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }

        public TreeNode FindNode (object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (obj.Equals(Object))
                return this;

            return Tree.FindNode(_children, obj);
        }

        public void CollapseAll ()
        {
            IsExpanded = false;
            Tree.CollapseAll(_children);
        }

        public void ExpandAll ()
        {
            IsExpanded = true;
            if (_children.Count > 0)
                Tree.ExpandAll(_children);
        }

        public void ExpandTo ()
        {
            TreeNode node = _parent;
            while (node != null) {
                node.IsExpanded = true;
                node = node._parent;
            }
        }

        public bool IsSelectable { get; set; }

        public void FindExpandedObjects (IList objects)
        {
            if (_expanded && !Tree.FindExpandedObjects(_children, objects))
                objects.Add(Object);
        }

        public void RestoreExpandedObjects (IList objects)
        {
            foreach (object obj in objects) {
                TreeNode node = FindNode(obj);
                if (node != null) {
                    node.IsExpanded = true;
                    node.ExpandTo();
                }
            }
        }
    }

    public class TreeStyle
    {
        public TreeStyle ()
        { }

        public TreeStyle (ISceneDrawable plus, ISceneDrawable minus, ISceneDrawable selection)
        {
            Plus = plus;
            Minus = minus;
            Selection = selection;
        }

        public TreeStyle (TreeStyle style)
        {
            Plus = style.Plus;
            PlusSelection = style.PlusSelection;
            Minus = style.Minus;
            MinusSelection = style.MinusSelection;
            Selection = style.Selection;
        }

        public ISceneDrawable Plus { get; set; }
        public ISceneDrawable PlusSelection { get; set; }
        public ISceneDrawable Minus { get; set; }
        public ISceneDrawable MinusSelection { get; set; }
        public ISceneDrawable Over { get; set; }
        public ISceneDrawable Selection { get; set; }
        public ISceneDrawable Background { get; set; }
    }
}
