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

namespace MonoGdx.Scene2D.UI
{
    public class SplitPane : WidgetGroup
    {
        private SplitPaneStyle _style;
        private Actor _firstWidget;
        private Actor _secondWidget;
        private float _splitAmount = .5f;
        private float _minAmount = 0;
        private float _maxAmount = 1;
        private int _draggingPointer = -1;

        private RectangleF _firstWidgetBounds;
        private RectangleF _secondWidgetBounds;
        private RectangleF _handleBounds;
        private Rectangle _firstScissors;
        private Rectangle _secondScissors;

        private Vector2 _lastPoint;
        private Vector2 _handlePosition;

        public SplitPane (Actor firstWidget, Actor secondWidget, bool vertical, Skin skin)
            : this(firstWidget, secondWidget, vertical, skin.Get<SplitPaneStyle>())
        { }

        public SplitPane (Actor firstWidget, Actor secondWidget, bool vertical, Skin skin, string styleName)
            : this(firstWidget, secondWidget, vertical, skin.Get<SplitPaneStyle>(styleName))
        { }

        public SplitPane (Actor firstWidget, Actor secondWidget, bool vertical, SplitPaneStyle style)
        {
            _firstWidget = firstWidget;
            _secondWidget = secondWidget;
            IsVertical = vertical;

            Style = style;
            FirstWidget = firstWidget;
            SecondWidget = secondWidget;
            Width = PrefWidth;
            Height = PrefHeight;
        }

        protected override void OnTouchDown (TouchEventArgs e)
        {
            try {
                if (_draggingPointer != -1)
                    return;
                if (e.Pointer == 0 && e.Button != 0)
                    return;

                Vector2 position = e.GetPosition(this);

                if (_handleBounds.Contains(position.X, position.Y)) {
                    _draggingPointer = e.Pointer;
                    _lastPoint = position;
                    _handlePosition = new Vector2(_handleBounds.X, _handleBounds.Y);

                    if (Stage != null)
                        CaptureTouch(e.Pointer);

                    e.Handled = true;
                }
            }
            finally {
                base.OnTouchDown(e);
            }
        }

        protected override void OnTouchUp (TouchEventArgs e)
        {
            base.OnTouchUp(e);

            if (Stage != null)
                ReleaseTouchCapture(e.Pointer);

            if (e.Pointer == _draggingPointer)
                _draggingPointer = -1;
        }

        protected override void OnTouchDrag (TouchEventArgs e)
        {
            base.OnTouchDrag(e);

            if (e.Pointer != _draggingPointer)
                return;

            Vector2 position = e.GetPosition(this);

            ISceneDrawable handle = _style.Handle;
            if (!IsVertical) {
                float delta = position.X - _lastPoint.X;
                float availWidth = Width - handle.MinWidth;
                float dragX = _handlePosition.X + delta;
                _handlePosition.X = dragX;

                dragX = MathHelper.Clamp(dragX, 0, availWidth);
                _splitAmount = dragX / availWidth;
                _splitAmount = MathHelper.Clamp(_splitAmount, _minAmount, _maxAmount);
                _lastPoint = position;
            }
            else {
                float delta = position.Y - _lastPoint.Y;
                float availHeight = Height - handle.MinHeight;
                float dragY = _handlePosition.Y + delta;
                _handlePosition.Y = dragY;

                dragY = MathHelper.Clamp(dragY, 0, availHeight);
                _splitAmount = 1 - (dragY / availHeight);
                _splitAmount = MathHelper.Clamp(_splitAmount, _minAmount, _maxAmount);
                _lastPoint = position;
            }

            Invalidate();
        }

        public SplitPaneStyle Style
        {
            get { return _style; }
            set
            {
                _style = value;
                InvalidateHierarchy();
            }
        }

        public override void Layout ()
        {
            if (!IsVertical)
                CalculateHorizBoundsAndPositions();
            else
                CalculateVertBoundsAndPositions();

            Actor firstWidget = _firstWidget;
            RectangleF firstWidgetBounds = _firstWidgetBounds;

            if (firstWidget != null) {
                firstWidget.X = firstWidgetBounds.X;
                firstWidget.Y = firstWidgetBounds.Y;

                if (firstWidget.Width != firstWidgetBounds.Width || firstWidget.Height != firstWidgetBounds.Height) {
                    firstWidget.Width = firstWidgetBounds.Width;
                    firstWidget.Height = firstWidgetBounds.Height;

                    if (firstWidget is ILayout) {
                        ILayout layout = firstWidget as ILayout;
                        layout.Invalidate();
                        layout.Validate();
                    }
                }
                else {
                    if (firstWidget is ILayout)
                        (firstWidget as ILayout).Validate();
                }
            }

            Actor secondWidget = _secondWidget;
            RectangleF secondWidgetBounds = _secondWidgetBounds;

            if (secondWidget != null) {
                secondWidget.X = secondWidgetBounds.X;
                secondWidget.Y = secondWidgetBounds.Y;

                if (secondWidget.Width != secondWidgetBounds.Width || secondWidget.Height != secondWidgetBounds.Height) {
                    secondWidget.Width = secondWidgetBounds.Width;
                    secondWidget.Height = secondWidgetBounds.Height;

                    if (secondWidget is ILayout) {
                        ILayout layout = secondWidget as ILayout;
                        layout.Invalidate();
                        layout.Validate();
                    }
                }
                else {
                    if (secondWidget is ILayout)
                        (secondWidget as ILayout).Validate();
                }
            }
        }

        public override float PrefWidth
        {
            get
            {
                float width = (_firstWidget is ILayout) ? (_firstWidget as ILayout).PrefWidth : _firstWidget.Width;
                width += (_secondWidget is ILayout) ? (_secondWidget as ILayout).PrefWidth : _secondWidget.Width;
                if (!IsVertical)
                    width += _style.Handle.MinWidth;
                return width;
            }
        }

        public override float PrefHeight
        {
            get
            {
                float height = (_firstWidget is ILayout) ? (_firstWidget as ILayout).PrefHeight : _firstWidget.Height;
                height += (_secondWidget is ILayout) ? (_secondWidget as ILayout).PrefHeight : _secondWidget.Height;
                if (IsVertical)
                    height += _style.Handle.MinHeight;
                return height;
            }
        }

        public override float MinWidth
        {
            get { return 0; }
        }

        public override float MinHeight
        {
            get { return 0; }
        }

        public bool IsVertical { get; set; }

        private void CalculateHorizBoundsAndPositions ()
        {
            ISceneDrawable handle = _style.Handle;
            float height = Height;

            float availWidth = Width - handle.MinWidth;
            float leftAreaWidth = (int)(availWidth * _splitAmount);
            float rightAreaWidth = availWidth - leftAreaWidth;
            float handleWidth = handle.MinWidth;

            _firstWidgetBounds = new RectangleF(0, 0, leftAreaWidth, height);
            _secondWidgetBounds = new RectangleF(leftAreaWidth + handleWidth, 0, rightAreaWidth, height);
            _handleBounds = new RectangleF(leftAreaWidth, 0, handleWidth, height);
        }

        private void CalculateVertBoundsAndPositions ()
        {
            ISceneDrawable handle = _style.Handle;
            float width = Width;
            float height = Height;

            float availHeight = height - handle.MinHeight;
            float topAreaHeight = (int)(availHeight * _splitAmount);
            float bottomAreaHeight = availHeight - topAreaHeight;
            float handleHeight = handle.MinHeight;

            _firstWidgetBounds = new RectangleF(0, height - topAreaHeight, width, topAreaHeight);
            _secondWidgetBounds = new RectangleF(0, 0, width, bottomAreaHeight);
            _handleBounds = new RectangleF(0, bottomAreaHeight, width, handleHeight);
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            Stage stage = Stage;

            Validate();

            ISceneDrawable handle = _style.Handle;
            ApplyTransform(spriteBatch, ComputeTransform());
            Matrix transform = spriteBatch.TransformMatrix;

            if (_firstWidget != null) {
                _firstScissors = ScissorStack.CalculateScissors(stage.Camera, spriteBatch.TransformMatrix, (Rectangle)_firstWidgetBounds);
                if (stage.ScissorStack.PushScissors(_firstScissors)) {
                    if (_firstWidget.IsVisible)
                        _firstWidget.Draw(spriteBatch, parentAlpha * Color.A / 255f);
                    spriteBatch.Flush();
                    stage.ScissorStack.PopScissors();
                }
            }

            if (_secondWidget != null) {
                _secondScissors = ScissorStack.CalculateScissors(stage.Camera, spriteBatch.TransformMatrix, (Rectangle)_secondWidgetBounds);
                if (stage.ScissorStack.PushScissors(_secondScissors)) {
                    if (_secondWidget.IsVisible)
                        _secondWidget.Draw(spriteBatch, parentAlpha * Color.A / 255f);
                    spriteBatch.Flush();
                    stage.ScissorStack.PopScissors();
                }
            }

            spriteBatch.Color = Color;
            handle.Draw(spriteBatch, _handleBounds.X, _handleBounds.Y, _handleBounds.Width, _handleBounds.Height);
            ResetTransform(spriteBatch);
        }

        public float Split
        {
            get { return _splitAmount; }
            set
            {
                _splitAmount = MathHelper.Clamp(value, _minAmount, _maxAmount);
                Invalidate();
            }
        }

        public float MinSplit
        {
            get { return _minAmount; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("MinSplit must be >= 0");
                if (value >= _maxAmount)
                    throw new ArgumentOutOfRangeException("MinSplit must be < MaxSplit");
                _minAmount = value;
            }
        }

        public float MaxSplit
        {
            get { return _maxAmount; }
            set
            {
                if (value > 1)
                    throw new ArgumentOutOfRangeException("MaxSplit must be <= 1");
                if (value <= _minAmount)
                    throw new ArgumentOutOfRangeException("MaxSplit must be >= MinSplit");
                _maxAmount = value;
            }
        }

        public Actor FirstWidget
        {
            get { return _firstWidget; }
            set
            {
                if (_firstWidget != null)
                    base.RemoveActor(_firstWidget);
                _firstWidget = value;
                if (value != null)
                    base.AddActor(value);
                Invalidate();
            }
        }

        public Actor SecondWidget
        {
            get { return _secondWidget; }
            set
            {
                if (_secondWidget != null)
                    base.RemoveActor(_secondWidget);
                _secondWidget = value;
                if (value != null)
                    base.AddActor(value);
                Invalidate();
            }
        }

        public override void AddActor (Actor actor)
        {
            throw new NotSupportedException("Use ScrollPane.Widget");
        }

        public override void AddActorAt (int index, Actor actor)
        {
            throw new NotSupportedException("Use ScrollPane.Widget");
        }

        public override void AddActorBefore (Actor actorBefore, Actor actor)
        {
            throw new NotSupportedException("Use ScrollPane.Widget");
        }

        public override void AddActorAfter (Actor actorAfter, Actor actor)
        {
            throw new NotSupportedException("Use ScrollPane.Widget");
        }
    }

    public class SplitPaneStyle
    {
        public SplitPaneStyle ()
        { }

        public SplitPaneStyle (ISceneDrawable handle)
        {
            Handle = handle;
        }

        public SplitPaneStyle (SplitPaneStyle style)
        {
            Handle = style.Handle;
        }

        public ISceneDrawable Handle { get; set; }
    }
}
