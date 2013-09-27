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
using MonoGdx.Scene2D.Actions;
using MonoGdx.Scene2D.Utils;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.UI
{
    public class SelectBox : Widget
    {
        SelectBoxStyle _style;
        string[] _items;
        TextBounds _bounds;
        SelectList _list;
        private float _prefWidth;
        private float _prefHeight;
        private ClickListener _clickListener;

        public SelectBox (object[] items, Skin skin)
            : this (items, skin.Get<SelectBoxStyle>())
        { }

        public SelectBox (object[] items, Skin skin, string styleName)
            : this (items, skin.Get<SelectBoxStyle>(styleName))
        { }

        public SelectBox (object[] items, SelectBoxStyle style)
        {
            Style = style;
            SetItems(items);
            Width = PrefWidth;
            Height = PrefHeight;

            AddListener(_clickListener = new DispatchClickListener() {
                OnTouchDown = (ev, x, y, pointer, button) => {
                    if (IsDisabled)
                        return false;
                    if (pointer == 0 && button != 0)
                        return false;
                    if (_list == null)
                        _list = new SelectList(this);
                    _list.Show(Stage);
                    return true;
                },
            });
        }

        public int MaxListCount { get; set; }

        public SelectBoxStyle Style
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

        public void SetItems (object[] objects)
        {
            if (objects == null)
                throw new ArgumentNullException("objects");

            if (!(objects is string[])) {
                string[] strings = new string[objects.Length];
                for (int i = 0, n = objects.Length; i < n; i++)
                    strings[i] = objects[i].ToString();
                objects = strings;
            }

            _items = objects as string[];
            SelectionIndex = 0;

            ISceneDrawable bg = _style.Background;
            BitmapFont font = _style.Font;

            _prefHeight = Math.Max(bg.TopHeight + bg.BottomHeight + font.CapHeight - font.Descent * 2, bg.MinHeight);

            float maxItemWIdth = 0;
            for (int i = 0; i < _items.Length; i++)
                maxItemWIdth = Math.Max(font.GetBounds(_items[i]).Width, maxItemWIdth);

            _prefWidth = bg.LeftWidth + bg.RightWidth + maxItemWIdth;

            ListStyle listStyle = _style.ListStyle;
            ScrollPaneStyle scrollStyle = _style.ScrollStyle;

            _prefWidth = Math.Max(_prefWidth, maxItemWIdth + scrollStyle.Background.LeftWidth + scrollStyle.Background.RightWidth
                + listStyle.Selection.LeftWidth + listStyle.Selection.RightWidth
                + Math.Max(_style.ScrollStyle.VScroll != null ? _style.ScrollStyle.VScroll.MinWidth : 0,
                    _style.ScrollStyle.VScrollKnob != null ? _style.ScrollStyle.VScrollKnob.MinWidth : 0));

            if (_items.Length > 0) {
                ChangeEvent changeEvent = Pools<ChangeEvent>.Obtain();
                Fire(changeEvent);
                Pools<ChangeEvent>.Release(changeEvent);
            }

            InvalidateHierarchy();
        }

        public string[] Items
        {
            get { return _items; }
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            ISceneDrawable background;
            if (_list != null && _list.Parent != null && _style.BackgroundOpen != null)
                background = _style.BackgroundOpen;
            else if (IsDisabled && _style.BackgroundDisabled != null)
                background = _style.BackgroundDisabled;
            else if (_clickListener.IsOver && _style.BackgroundOver != null)
                background = _style.BackgroundOver;
            else
                background = _style.Background;

            BitmapFont font = _style.Font;
            Color fontColor = _style.FontColor;

            if (IsDisabled && _style.FontColorDisabled != null)
                fontColor = _style.FontColorDisabled.Value;

            float x = (int)X;
            float y = (int)Y;
            float width = Width;
            float height = Height;

            spriteBatch.Color = Color.MultiplyAlpha(parentAlpha);
            background.Draw(spriteBatch, x, y, width, height);

            if (_items.Length > 0) {
                float availableWidth = width - background.LeftWidth - background.RightWidth;
                int numGlyphs = font.ComputeVisibleGlyphs(_items[SelectionIndex], 0, _items[SelectionIndex].Length, availableWidth);
                _bounds = font.GetBounds(_items[SelectionIndex]);

                height -= background.BottomHeight + background.TopHeight;
                float textY = (int)(height / 2 + background.BottomHeight + _bounds.Height / 2);

                font.Color = fontColor.MultiplyAlpha(parentAlpha);
                font.Draw(spriteBatch, _items[SelectionIndex], x + background.LeftWidth, y + textY, 0, numGlyphs);
            }
        }

        public int SelectionIndex { get; set; }

        public string Selection
        {
            get { return _items[SelectionIndex]; }
            set
            {
                for (int i = 0; i < _items.Length; i++) {
                    if (_items[i] == value)
                        SelectionIndex = i;
                }
            }
        }

        public override float PrefWidth
        {
            get { return _prefWidth; }
        }

        public override float PrefHeight
        {
            get { return _prefHeight; }
        }

        public bool IsDisabled { get; set; }

        public void HideList ()
        {
            if (_list == null || _list.Parent == null)
                return;

            _list.AddAction(ActionRepo.Sequence(ActionRepo.FadeOut(.15f, Interpolation.Fade), ActionRepo.RemoveActor()));
        }

        private class SelectList : ScrollPane
        {
            private SelectBox _selectBox;
            private List _list;
            private Vector2 _screenCoords;

            public SelectList (SelectBox selectBox)
                : base(null, selectBox.Style.ScrollStyle)
            {
                _selectBox = selectBox;

                OverscrollX = false;
                OverscrollY = false;
                FadeScrollBars = false;

                _list = new List(new object[0], _selectBox.Style.ListStyle);
                Widget = _list;

                _list.AddListener(new DispatchInputListener() {
                    OnMouseMoved = (ev, x, y) => {
                        _list.SelectedIndex = Math.Min(_selectBox.Items.Length - 1, (int)((_list.Height - y) / _list.ItemHeight));
                        return true;
                    },
                });

                AddListener(new TouchListener() {
                    Down = (ev, x, y, pointer, button) => {
                        if (ev.TargetActor == _list && !_selectBox.IsDisabled)
                            return true;
                        _selectBox.HideList();
                        return false;
                    },

                    Up = (ev, x, y, pointer, button) => {
                        if (Hit(x, y, true) == _list) {
                            _selectBox.SelectionIndex = _list.SelectedIndex;

                            ChangeEvent changeEvent = Pools<ChangeEvent>.Obtain();
                            _selectBox.Fire(changeEvent);

                            Pools<ChangeEvent>.Release(changeEvent);
                            _selectBox.HideList();
                        }
                    },
                });
            }

            public void Show (Stage stage)
            {
                stage.AddActor(this);

                Vector2 stageCoords = _selectBox.LocalToStageCoordinates(Vector2.Zero);
                _screenCoords = stageCoords;

                _list.SetItems(_selectBox.Items);
                _list.SelectedIndex = _selectBox.SelectionIndex;

                // Show the list above or below the select box, limited to a number of items and the available height in the stage.
                float itemHeight = _list.ItemHeight;
                float height = itemHeight * (_selectBox.MaxListCount <= 0 ? _selectBox.Items.Length 
                    : Math.Min(_selectBox.MaxListCount, _selectBox.Items.Length));
                ISceneDrawable background = Style.Background;
                if (background != null)
                    height += background.TopHeight + background.BottomHeight;

                float heightBelow = stageCoords.Y;
                float heightAbove = stage.Camera.ViewportHeight - stageCoords.Y - _selectBox.Height;

                bool below = true;
                if (height > heightBelow) {
                    if (heightAbove > heightBelow) {
                        below = false;
                        height = Math.Min(height, heightAbove);
                    }
                    else
                        height = heightBelow;
                }

                if (below)
                    Y = stageCoords.Y - height;
                else
                    Y = stageCoords.Y + _selectBox.Height;

                X = stageCoords.X + _selectBox.Style.ListLeftOffset;
                Width = _selectBox.Width + _selectBox.Style.ListLeftOffset + _selectBox.Style.ListRightOffset;
                Height = height;

                ScrollToCenter(0, _list.Height - _selectBox.SelectionIndex * itemHeight - itemHeight / 2, 0, 0);
                UpdateVisualScroll();

                ClearActions();
                Color = new Color(Color.R, Color.G, Color.B, 0);
                AddAction(ActionRepo.FadeIn(.3f, Interpolation.Fade));

                stage.SetScrollFocus(this);
            }

            public override Actor Hit (float x, float y, bool touchable)
            {
                Actor actor = base.Hit(x, y, touchable);
                return (actor != null) ? actor : this;
            }

            public override void Act (float delta)
            {
                base.Act(delta);

                Vector2 stageCoords = _selectBox.LocalToStageCoordinates(Vector2.Zero);
                if (stageCoords.X != _screenCoords.X || stageCoords.Y != _screenCoords.Y)
                    _selectBox.HideList();
            }
        }
    }

    public class SelectBoxStyle
    {
        public SelectBoxStyle ()
        {
            FontColor = Color.White;
        }

        public SelectBoxStyle (BitmapFont font, Color fontColor, ISceneDrawable background, ScrollPaneStyle scrollStyle, ListStyle listStyle)
        {
            Font = font;
            FontColor = fontColor;
            Background = background;
            ScrollStyle = scrollStyle;
            ListStyle = listStyle;
        }

        public SelectBoxStyle (SelectBoxStyle style)
        {
            Font = style.Font;
            FontColor = style.FontColor;
            FontColorDisabled = style.FontColorDisabled;
            Background = style.Background;
            ScrollStyle = style.ScrollStyle;
            ListStyle = style.ListStyle;
            BackgroundOver = style.BackgroundOver;
            BackgroundOpen = style.BackgroundOpen;
            BackgroundDisabled = style.BackgroundDisabled;
            ListLeftOffset = style.ListLeftOffset;
            ListRightOffset = style.ListRightOffset;
        }

        public BitmapFont Font { get; set; }
        public Color FontColor { get; set; }
        public Color? FontColorDisabled { get; set; }
        public ISceneDrawable Background { get; set; }
        public ScrollPaneStyle ScrollStyle { get; set; }
        public ListStyle ListStyle { get; set; }
        public ISceneDrawable BackgroundOver { get; set; }
        public ISceneDrawable BackgroundOpen { get; set; }
        public ISceneDrawable BackgroundDisabled { get; set; }
        public float ListLeftOffset { get; set; }
        public float ListRightOffset { get; set; }
    }
}
