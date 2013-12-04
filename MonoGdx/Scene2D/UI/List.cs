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
using Microsoft.Xna.Framework;
using MonoGdx.Geometry;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.UI
{
    public class List : Widget, ICullable
    {
        public static readonly RoutedEvent SelectionChangedEvent = 
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(List));

        private ListStyle _style;
        private object[] _items;
        private string[] _itemsText;
        private int _selectedIndex;
        private RectangleF _cullingArea;
        private float _prefWidth;
        private float _prefHeight;
        private float _itemHeight;
        private float _textOffsetX;
        private float _textOffsetY;

        private List ()
        {
            IsSelectable = true;
        }

        public List (object[] items, Skin skin)
            : this(items, skin.Get<ListStyle>())
        { }

        public List (object[] items, Skin skin, string styleName)
            : this(items, skin.Get<ListStyle>(styleName))
        { }

        public List (object[] items, ListStyle style)
            : this()
        {
            Style = style;
            SetItems(items);
            Width = PrefWidth;
            Height = PrefHeight;
        }

        protected override void OnTouchDown (TouchEventArgs e)
        {
            try {
                Vector2 position = e.GetPosition(this);

                if (e.Pointer == 0 && e.Button != 0)
                    return;
                if (!IsSelectable)
                    return;

                DoTouchDown(position.Y);

                e.Handled = true;
            }
            finally {
                base.OnTouchDown(e);
            }
        }

        public bool IsSelectable { get; set; }

        void DoTouchDown (float y)
        {
            int oldIndex = _selectedIndex;
            _selectedIndex = (int)((Height - y) / _itemHeight);
            _selectedIndex = Math.Max(0, _selectedIndex);
            _selectedIndex = Math.Min(_items.Length - 1, _selectedIndex);

            if (oldIndex != _selectedIndex) {
                object oldSelection = (oldIndex == -1) ? null : _items[oldIndex];
                object newSelection = (_selectedIndex == -1) ? null : _items[_selectedIndex];
                OnSelectionChanged(oldSelection, newSelection);
            }
        }

        public ListStyle Style
        {
            get { return _style; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Style");
                _style = value;

                if (_items != null)
                    SetItems(_items);
                else
                    InvalidateHierarchy();
            }
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            BitmapFont font = _style.Font;
            ISceneDrawable selectedDrawable = _style.Selection;
            Color fontColorSelected = _style.FontColorSelected;
            Color fontColorUnselected = _style.FontColorUnselected;

            spriteBatch.Color = Color.MultiplyAlpha(parentAlpha);

            float x = X;
            float y = Y;

            font.Color = fontColorUnselected.MultiplyAlpha(parentAlpha);
            float itemY = Height;

            for (int i = 0; i < _items.Length; i++) {
                if (_cullingArea.IsEmpty || (itemY - _itemHeight <= _cullingArea.Y + _cullingArea.Height && itemY >= _cullingArea.Y)) {
                    if (_selectedIndex == i) {
                        selectedDrawable.Draw(spriteBatch, x, y + itemY - _itemHeight, Width, ItemHeight);
                        font.Color = fontColorSelected.MultiplyAlpha(parentAlpha);
                    }
                    font.Draw(spriteBatch, _itemsText[i], x + _textOffsetX, y + itemY - _textOffsetY);

                    if (_selectedIndex == i)
                        font.Color = fontColorUnselected.MultiplyAlpha(parentAlpha);
                }
                else if (itemY < _cullingArea.Y)
                    break;

                itemY -= ItemHeight;
            }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (value == _selectedIndex)
                    return;
                if (value < -1 || value >= _items.Length)
                    throw new ArgumentOutOfRangeException("SelectedIndex");

                object oldSelection = (_selectedIndex == -1) ? null : _items[_selectedIndex];

                _selectedIndex = value;

                object newSelection = (_selectedIndex == -1) ? null : _items[_selectedIndex];
                OnSelectionChanged(oldSelection, newSelection);
            }
        }

        public object Selection
        {
            get
            {
                if (_items.Length == 0 || _selectedIndex == -1)
                    return null;
                return _items[_selectedIndex];
            }

            set { SetSelection(value); }
        }

        public int SetSelection (object item)
        {
            int oldIndex = _selectedIndex;

            _selectedIndex = -1;
            for (int i = 0, n = _items.Length; i < n; i++) {
                if (_items[i] == item) {
                    _selectedIndex = i;
                    break;
                }
            }

            if (oldIndex == _selectedIndex)
                return _selectedIndex;

            object oldSelection = (oldIndex == -1) ? null : _items[oldIndex];
            object newSelection = (_selectedIndex == -1) ? null : _items[_selectedIndex];
            OnSelectionChanged(oldSelection, newSelection);

            return _selectedIndex;
        }

        public void SetItems (object[] objects)
        {
            if (objects == null)
                throw new ArgumentNullException("objects");

            _items = objects;
            if (!(objects is string[])) {
                string[] strings = new string[objects.Length];
                for (int i = 0, n = objects.Length; i < n; i++)
                    strings[i] = objects[i].ToString();
                _itemsText = strings;
            }
            else
                _itemsText = objects as string[];

            _selectedIndex = 0;

            BitmapFont font = _style.Font;
            ISceneDrawable selectedDrawable = _style.Selection;

            _itemHeight = font.CapHeight - font.Descent * 2;
            _itemHeight += selectedDrawable.TopHeight + selectedDrawable.BottomHeight;
            _textOffsetX = selectedDrawable.LeftWidth;
            _textOffsetY = selectedDrawable.TopHeight - font.Descent;

            _prefWidth = 0;
            for (int i = 0; i < _items.Length; i++) {
                TextBounds bounds = font.GetBounds(_itemsText[i]);
                _prefWidth = Math.Max(bounds.Width, _prefWidth);
            }
            _prefWidth += selectedDrawable.LeftWidth + selectedDrawable.RightWidth;
            _prefHeight = _items.Length * _itemHeight;

            InvalidateHierarchy();
        }

        public object[] Items
        {
            get { return _items; }
        }

        public float ItemHeight
        {
            get { return _itemHeight; }
        }

        public override float PrefWidth
        {
            get { return _prefWidth; }
        }

        public override float PrefHeight
        {
            get { return _prefHeight; }
        }

        public RectangleF CullingArea
        {
            get { return _cullingArea; }
            set { _cullingArea = value; }
        }

        public void SetCullingArea (RectangleF cullingArea)
        {
            _cullingArea = cullingArea;
        }

        public event SelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        protected virtual void OnSelectionChanged (object oldSelection, object newSelection)
        {
            RaiseEvent(new SelectionChangedEventArgs(newSelection, oldSelection) {
                RoutedEvent = SelectionChangedEvent,
                OriginalSource = this,
                Source = this,
            });
        }
    }

    public class ListStyle
    {
        public ListStyle ()
        {
            FontColorSelected = Color.White;
            FontColorUnselected = Color.White;
        }

        public ListStyle (BitmapFont font, Color fontColorSelected, Color fontColorUnselected, ISceneDrawable selection)
        {
            Font = font;
            FontColorSelected = fontColorSelected;
            FontColorUnselected = fontColorUnselected;
            Selection = selection;
        }

        public ListStyle (ListStyle style)
        {
            Font = style.Font;
            FontColorSelected = style.FontColorSelected;
            FontColorUnselected = style.FontColorUnselected;
            Selection = style.Selection;
        }

        public BitmapFont Font { get; set; }
        public Color FontColorSelected { get; set; }
        public Color FontColorUnselected { get; set; }
        public ISceneDrawable Selection { get; set; }
    }
}
