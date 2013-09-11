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
    [TODO("Flick Listener")]
    public class ScrollPane : WidgetGroup
    {
        private ScrollPaneStyle _style;
        private Actor _widget;

        RectangleF _hScrollBounds;
        RectangleF _vScrollBounds;
        RectangleF _hKnobBounds;
        RectangleF _vKnobBounds;
        private RectangleF _widgetAreaBounds;
        private RectangleF _widgetCullingArea;
        private Rectangle _scissorBounds;
        //private ActorGestureListener _flickScrollListener;

        float _visualAmountX;
        float _visualAmountY;
        bool _touchScrollH;
        bool _touchScrollV;
        Vector2 _lastPoint;
        float _areaWidth;
        float _areaHeight;
        bool _fadeScrollBars = true;
        float _fadeAlpha;
        float _fadeDelay;

        bool _flickScroll = true;
        float _velocityX;
        float _velocityY;
        float _flingTimer;
        private bool _scollbarsOnTop;
        int _draggingPointer = -1;

        private float _handlePosition;

        public ScrollPane ()
        {
            FadeAlphaSeconds = 1;
            FadeDelaySeconds = 1;
            CancelTouchFocus = true;
            OverscrollX = true;
            OverscrollY = true;
            FlingTime = 1;
            OverscrollDistance = 50;
            OverscrollSpeedMin = 30;
            OverscrollSpeedMax = 200;
            IsClamped = true;

            SmoothScrolling = false;
            ScrollBarsOnTop = false;
            FadeScrollBars = false;
        }

        public ScrollPane (Actor widget)
            : this(widget, new ScrollPaneStyle())
        { }

        public ScrollPane (Actor widget, Skin skin)
            : this(widget, skin.Get<ScrollPaneStyle>())
        { }

        public ScrollPane (Actor widget, Skin skin, string styleName)
            : this(widget, skin.Get<ScrollPaneStyle>(styleName))
        { }

        [TODO]
        public ScrollPane (Actor widget, ScrollPaneStyle style)
            : this()
        {
            if (style == null)
                throw new ArgumentNullException("style");
            _style = style;

            Widget = widget;
            Width = 150;
            Height = 150;

            AddCaptureListener(new DispatchInputListener() {
                OnTouchDown = (ev, x, y, pointer, button) => {
                    if (_draggingPointer != -1)
                        return false;
                    if (pointer == 0 && button != 0)
                        return false;
                    Stage.SetScrollFocus(this);

                    if (!_flickScroll)
                        ResetFade();

                    if (_fadeAlpha == 0)
                        return false;

                    if (IsScrollX && _hScrollBounds.Contains(x, y)) {
                        ev.Stop();
                        ResetFade();
                        if (_hKnobBounds.Contains(x, y)) {
                            _lastPoint = new Vector2(x, y);
                            _handlePosition = _hKnobBounds.X;
                            _touchScrollH = true;
                            _draggingPointer = pointer;
                            return true;
                        }

                        SetScrollX(ScrollX + Math.Max(_areaWidth * .9f, MaxX * .1f) * (x < _hKnobBounds.X ? -1 : 1));
                        return true;
                    }

                    if (IsScrollY && _vScrollBounds.Contains(x, y)) {
                        ev.Stop();
                        ResetFade();
                        if (_vKnobBounds.Contains(x, y)) {
                            _lastPoint = new Vector2(x, y);
                            _handlePosition = _vKnobBounds.Y;
                            _touchScrollV = true;
                            _draggingPointer = pointer;
                            return true;
                        }

                        SetScrollY(ScrollY + Math.Max(_areaHeight * .9f, MaxY * .1f) * (y < _vKnobBounds.Y ? 1 : -1));
                        return true;
                    }

                    return false;
                },

                OnTouchUp = (ev, x, y, pointer, button) => {
                    if (pointer != _draggingPointer)
                        return;

                    _draggingPointer = -1;
                    _touchScrollH = false;
                    _touchScrollV = false;
                },

                OnTouchDragged = (ev, x, y, pointer) => {
                    if (pointer != _draggingPointer)
                        return;

                    if (_touchScrollH) {
                        float delta = x - _lastPoint.X;
                        float scrollH = _handlePosition + delta;
                        _handlePosition = scrollH;

                        scrollH = Math.Max(_hScrollBounds.X, scrollH);
                        scrollH = Math.Min(_hScrollBounds.X + _hScrollBounds.Width - _hKnobBounds.Width, scrollH);

                        float total = _hScrollBounds.Width - _hKnobBounds.Width;
                        if (total != 0)
                            ScrollPercentX = (scrollH - _hScrollBounds.X) / total;

                        _lastPoint = new Vector2(x, y);
                    }
                    else if (_touchScrollV) {
                        float delta = y - _lastPoint.Y;
                        float scrollV = _handlePosition + delta;
                        _handlePosition = scrollV;

                        scrollV = Math.Max(_vScrollBounds.Y, scrollV);
                        scrollV = Math.Min(_vScrollBounds.Y + _vScrollBounds.Height - _vKnobBounds.Height, scrollV);

                        float total = _vScrollBounds.Height - _vKnobBounds.Height;
                        if (total != 0)
                            ScrollPercentY = 1 - (scrollV - _vScrollBounds.Y) / total;

                        _lastPoint = new Vector2(x, y);
                    }
                },

                OnMouseMoved = (ev, x, y) => {
                    if (!_flickScroll)
                        ResetFade();
                    return false;
                },
            });

            // Flick Scroll Listener

            AddListener(new DispatchInputListener() {
                OnScrolled = (ev, x, y, amount) => {
                    ResetFade();
                    if (IsScrollY)
                        SetScrollY(ScrollY + Math.Max(_areaHeight * .9f, MaxY * .1f) / 4 * amount);
                    if (IsScrollX)
                        SetScrollX(ScrollX + Math.Max(_areaWidth * .9f, MaxX * .1f) / 4 * amount);
                    return true;
                },
            });
        }

        void ResetFade ()
        {
            _fadeAlpha = FadeAlphaSeconds;
            _fadeDelay = FadeDelaySeconds;
        }

        [TODO]
        void CancelTouchFocusedChild (InputEvent ev)
        {
            if (!CancelTouchFocus)
                return;

            //Stage stage = Stage;
            //if (stage != null)
            //    stage.CancelTouchFocus(_flickScrollListener, this);
        }

        void Clamp ()
        {
            if (!IsClamped)
                return;

            ScrollX = OverscrollX ? MathHelper.Clamp(ScrollX, -OverscrollDistance, MaxX + OverscrollDistance) 
                : MathHelper.Clamp(ScrollX, 0, MaxX);
            ScrollY = OverscrollY ? MathHelper.Clamp(ScrollY, -OverscrollDistance, MaxY + OverscrollDistance)
                : MathHelper.Clamp(ScrollY, 0, MaxY);
        }

        public ScrollPaneStyle Style
        {
            get { return _style; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("ScrollPaneStyle");
                _style = Style;
                InvalidateHierarchy();
            }
        }

        public override void Act (float delta)
        {
            base.Act(delta);

            bool panning = false; // TODO: flickScrollListener

            if (_fadeAlpha > 0 && _fadeScrollBars && !panning && !_touchScrollH && !_touchScrollV) {
                _fadeDelay -= delta;
                if (_fadeDelay <= 0)
                    _fadeAlpha = Math.Max(0, _fadeAlpha - delta);
            }

            if (_flingTimer > 0) {
                ResetFade();

                float alpha = _flingTimer / FlingTime;
                ScrollX -= _velocityX * alpha * delta;
                ScrollY -= _velocityY * alpha * delta;
                Clamp();

                // Stop fling if hit overscroll distance.
                if (ScrollX == -OverscrollDistance)
                    _velocityX = 0;
                if (ScrollX >= MaxX + OverscrollDistance)
                    _velocityX = 0;
                if (ScrollY == -OverscrollDistance)
                    _velocityY = 0;
                if (ScrollY >= MaxY + OverscrollDistance)
                    _velocityY = 0;

                _flingTimer -= delta;
                if (_flingTimer <= 0) {
                    _velocityX = 0;
                    _velocityY = 0;
                }
            }

            if (SmoothScrolling && _flingTimer <= 0 && !_touchScrollH && !_touchScrollV && !panning) {
                if (_visualAmountX != ScrollX) {
                    if (_visualAmountX < ScrollX)
                        VisualScrollX = Math.Min(ScrollX, _visualAmountX + Math.Max(150 * delta, (ScrollX - _visualAmountX) * 5 * delta));
                    else
                        VisualScrollX = Math.Max(ScrollX, _visualAmountX - Math.Max(150 * delta, (_visualAmountX - ScrollX) * 5 * delta));
                }
                if (_visualAmountY != ScrollY) {
                    if (_visualAmountY < ScrollY)
                        VisualScrollY = Math.Min(ScrollY, _visualAmountY + Math.Max(150 * delta, (ScrollY - _visualAmountY) * 5 * delta));
                    else
                        VisualScrollY = Math.Max(ScrollY, _visualAmountY - Math.Max(150 * delta, (_visualAmountY - ScrollY) * 5 * delta));
                }
            }
            else {
                if (_visualAmountX != ScrollX)
                    VisualScrollX = ScrollX;
                if (_visualAmountY != ScrollY)
                    VisualScrollY = ScrollY;
            }

            if (!panning) {
                if (OverscrollX && IsScrollX) {
                    if (ScrollX < 0) {
                        ResetFade();
                        ScrollX += (OverscrollSpeedMin + (OverscrollSpeedMax - OverscrollSpeedMin) * -ScrollX / OverscrollDistance) * delta;
                        if (ScrollX > 0)
                            ScrollX = 0;
                    }
                    else if (ScrollX > MaxX) {
                        ResetFade();
                        ScrollX -= (OverscrollSpeedMin + (OverscrollSpeedMax - OverscrollSpeedMin) * (-MaxX - ScrollX) / OverscrollDistance) * delta;
                        if (ScrollX < MaxX)
                            ScrollX = MaxX;
                    }
                }

                if (OverscrollY && IsScrollY) {
                    if (ScrollY < 0) {
                        ResetFade();
                        ScrollY += (OverscrollSpeedMin + (OverscrollSpeedMax - OverscrollSpeedMin) * -ScrollY / OverscrollDistance) * delta;
                        if (ScrollY > 0)
                            ScrollY = 0;
                    }
                    else if (ScrollY > MaxY) {
                        ResetFade();
                        ScrollY -= (OverscrollSpeedMin + (OverscrollSpeedMax - OverscrollSpeedMin) * (-MaxY - ScrollY) / OverscrollDistance) * delta;
                        if (ScrollY < MaxY)
                            ScrollY = MaxY;
                    }
                }
            }
        }

        public override void Layout ()
        {
            ISceneDrawable bg = _style.Background;
            ISceneDrawable hScrollKnob = _style.HScrollKnob;
            ISceneDrawable vScrollKnob = _style.VScrollKnob;

            float bgLeftWidth = 0;
            float bgRightWidth = 0;
            float bgTopHeight = 0;
            float bgBottomHeight = 0;

            if (bg != null) {
                bgLeftWidth = bg.LeftWidth;
                bgRightWidth = bg.RightWidth;
                bgTopHeight = bg.TopHeight;
                bgBottomHeight = bg.BottomHeight;
            }

            float width = Width;
            float height = Height;

            float scrollbarHeight = 0;
            if (hScrollKnob != null)
                scrollbarHeight = hScrollKnob.MinHeight;
            if (_style.HScroll != null)
                scrollbarHeight = Math.Max(scrollbarHeight, _style.HScroll.MinHeight);

            float scrollbarWidth = 0;
            if (vScrollKnob != null)
                scrollbarWidth = vScrollKnob.MinWidth;
            if (_style.VScroll != null)
                scrollbarWidth = Math.Max(scrollbarWidth, _style.VScroll.MinWidth);

            // Get available space size by subtracting background's padded area.
            _areaWidth = width - bgLeftWidth - bgRightWidth;
            _areaHeight = height - bgTopHeight - bgBottomHeight;

            if (_widget == null)
                return;

            // Get widget's desired width.
            float widgetWidth = 0;
            float widgetHeight = 0;

            if (_widget is ILayout) {
                ILayout layout = _widget as ILayout;
                widgetWidth = layout.PrefWidth;
                widgetHeight = layout.PrefHeight;
            }
            else {
                widgetWidth = _widget.Width;
                widgetHeight = _widget.Height;
            }

            // Determine if horizontal/vertical scrollbars are needed.
            IsScrollX = ForceScrollX || (widgetWidth > _areaWidth && !IsDisabledX);
            IsScrollY = ForceScrollY || (widgetHeight > _areaHeight && !IsDisabledY);

            bool fade = _fadeScrollBars;
            if (!fade) {
                // Check again, now taking into account the area that's taken up by any enabled scrollbars.
                if (IsScrollY) {
                    _areaWidth -= scrollbarWidth;
                    if (!IsScrollX && widgetWidth > _areaWidth && !IsDisabledX)
                        IsScrollX = true;
                }

                if (IsScrollX) {
                    _areaHeight -= scrollbarHeight;
                    if (!IsScrollY && widgetHeight > _areaHeight && !IsDisabledY) {
                        IsScrollY = true;
                        _areaWidth -= scrollbarWidth;
                    }
                }
            }

            // Set the widget area bounds
            _widgetAreaBounds = new RectangleF(bgLeftWidth, bgBottomHeight, _areaWidth, _areaHeight);

            if (fade) {
                // Make sure widget is drawn under fading scrollbars.
                if (IsScrollX)
                    _areaHeight -= scrollbarHeight;
                if (IsScrollY)
                    _areaWidth -= scrollbarWidth;
            }
            else {
                if (ScrollBarsOnTop) {
                    // Make sure widget is drawn under non-fading scrollbars.
                    if (IsScrollX)
                        _widgetAreaBounds.Height += scrollbarHeight;
                    if (IsScrollY)
                        _widgetAreaBounds.Width += scrollbarWidth;
                }
                else {
                    // Offset widget area y for horizontal scrollbar.
                    if (IsScrollX)
                        _widgetAreaBounds.Y += scrollbarHeight;
                }
            }

            // If the widget is smaller than the available space, make it take up the available space.
            widgetWidth = IsDisabledX ? width : Math.Max(_areaWidth, widgetWidth);
            widgetHeight = IsDisabledY ? height : Math.Max(_areaHeight, widgetHeight);

            MaxX = widgetWidth - _areaWidth;
            MaxY = widgetHeight - _areaHeight;

            if (fade) {
                // Make sure widget is drawn under fading scrollbars.
                if (IsScrollX)
                    MaxY -= scrollbarHeight;
                if (IsScrollY)
                    MaxX -= scrollbarWidth;
            }

            ScrollX = MathHelper.Clamp(ScrollX, 0, MaxX);
            ScrollY = MathHelper.Clamp(ScrollY, 0, MaxY);

            // Set the bounds and scroll knob sizes if scrollbars are needed.
            if (IsScrollX) {
                if (hScrollKnob != null) {
                    float hScrollHeight = _style.HScroll != null ? _style.HScroll.MinHeight : hScrollKnob.MinHeight;
                    _hScrollBounds = new RectangleF(bgLeftWidth, bgBottomHeight, _areaWidth, hScrollHeight);
                    _hKnobBounds.Width = Math.Max(hScrollKnob.MinWidth, (int)(_hScrollBounds.Width * _areaWidth / widgetWidth));
                    _hKnobBounds.Height = hScrollKnob.MinHeight;
                    _hKnobBounds.X = _hScrollBounds.X + (int)((_hScrollBounds.Width - _hKnobBounds.Width) * ScrollPercentX);
                    _hKnobBounds.Y = _hScrollBounds.Y;
                }
                else {
                    _hScrollBounds = RectangleF.Empty;
                    _hKnobBounds = RectangleF.Empty;
                }
            }

            if (IsScrollY) {
                if (vScrollKnob != null) {
                    float vScrollWidth = _style.VScroll != null ? _style.VScroll.MinWidth : vScrollKnob.MinWidth;
                    _vScrollBounds = new RectangleF(width - bgRightWidth - vScrollWidth, height - bgTopHeight - _areaHeight, vScrollWidth, _areaHeight);
                    _vKnobBounds.Width = vScrollKnob.MinWidth;
                    _vKnobBounds.Height = Math.Max(vScrollKnob.MinHeight, (int)(_vScrollBounds.Height * _areaHeight / widgetHeight));
                    _vKnobBounds.X = width - bgRightWidth - vScrollKnob.MinWidth;
                    _vKnobBounds.Y = _vScrollBounds.Y + (int)((_vScrollBounds.Height - _vKnobBounds.Height) * (1 - ScrollPercentY));
                }
                else {
                    _vScrollBounds = RectangleF.Empty;
                    _vKnobBounds = RectangleF.Empty;
                }
            }

            if (_widget.Width != widgetWidth || _widget.Height != widgetHeight) {
                _widget.Width = widgetWidth;
                _widget.Height = widgetHeight;

                if (_widget is ILayout) {
                    ILayout layout = _widget as ILayout;
                    layout.Invalidate();
                    layout.Validate();
                }
            }
            else {
                if (_widget is ILayout)
                    (_widget as ILayout).Validate();
            }
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            if (_widget == null)
                return;

            Validate();

            // Setup transform for this group
            ApplyTransform(spriteBatch, ComputeTransform());

            if (IsScrollX)
                _hKnobBounds.X = _hScrollBounds.X + (int)((_hScrollBounds.Width - _hKnobBounds.Width) * ScrollPercentX);
            if (IsScrollY)
                _vKnobBounds.Y = _vScrollBounds.Y + (int)((_vScrollBounds.Height - _vKnobBounds.Height) * (1 - ScrollPercentY));

            // Calculate the widget's position depending on the scroll state and available widget area.
            float y = _widgetAreaBounds.Y;
            if (!IsScrollY)
                y -= (int)MaxY;
            else
                y -= (int)(MaxY - _visualAmountY);

            if (!_fadeScrollBars && ScrollBarsOnTop && IsScrollX) {
                float scollbarHeight = 0;
                if (_style.HScrollKnob != null)
                    scollbarHeight = _style.HScrollKnob.MinHeight;
                if (_style.HScroll != null)
                    scollbarHeight = Math.Max(scollbarHeight, _style.HScroll.MinHeight);
                y += scollbarHeight;
            }

            float x = _widgetAreaBounds.X;
            if (IsScrollX)
                x -= (int)_visualAmountX;

            _widget.SetPosition(x, y);

            if (_widget is ICullable) {
                _widgetCullingArea.X = -_widget.X + _widgetAreaBounds.X;
                _widgetCullingArea.Y = -_widget.Y + _widgetAreaBounds.Y;
                _widgetCullingArea.Width = _widgetAreaBounds.Width;
                _widgetCullingArea.Height = _widgetAreaBounds.Height;

                (_widget as ICullable).SetCullingArea(_widgetCullingArea);
            }

            // Caculate the scissor bounds based on the batch transform, the available widget area and the camera transform. We need to
            // project those to screen coordinates for OpenGL ES to consume.
            _scissorBounds = ScissorStack.CalculateScissors(Stage.Camera, spriteBatch.TransformMatrix, (Rectangle)_widgetAreaBounds);

            // Draw the background ninepatch
            spriteBatch.Color = Color.MultiplyAlpha(parentAlpha);
            if (_style.Background != null)
                _style.Background.Draw(spriteBatch, 0, 0, Width, Height);
            spriteBatch.Flush();

            // Enable scissors for widget area and draw the widget.
            if (Stage.ScissorStack.PushScissors(_scissorBounds)) {
                DrawChildren(spriteBatch, parentAlpha);
                Stage.ScissorStack.PopScissors();
            }

            // Render scrollbars and knobs on top.
            spriteBatch.Color = Color.MultiplyAlpha(parentAlpha * Interpolation.Fade.Apply(_fadeAlpha / FadeAlphaSeconds));
            if (IsScrollX && IsScrollY) {
                if (_style.Corner != null)
                    _style.Corner.Draw(spriteBatch, _hScrollBounds.X + _hScrollBounds.Width, _hScrollBounds.Y, _vScrollBounds.Width, _vScrollBounds.Height);
            }

            if (IsScrollX) {
                if (_style.HScroll != null)
                    _style.HScroll.Draw(spriteBatch, _hScrollBounds.X, _hScrollBounds.Y, _hScrollBounds.Width, _hScrollBounds.Height);
                if (_style.HScrollKnob != null)
                    _style.HScrollKnob.Draw(spriteBatch, _hKnobBounds.X, _hKnobBounds.Y, _hKnobBounds.Width, _hKnobBounds.Height);
            }

            if (IsScrollY) {
                if (_style.VScroll != null)
                    _style.VScroll.Draw(spriteBatch, _vScrollBounds.X, _vScrollBounds.Y, _vScrollBounds.Width, _vScrollBounds.Height);
                if (_style.VScrollKnob != null)
                    _style.VScrollKnob.Draw(spriteBatch, _vKnobBounds.X, _vKnobBounds.Y, _vKnobBounds.Width, _vKnobBounds.Height);
            }

            ResetTransform(spriteBatch);
        }

        public override float PrefWidth
        {
            get
            {
                if (_widget is ILayout) {
                    float width = (_widget as ILayout).PrefWidth;
                    if (_style.Background != null)
                        width += _style.Background.LeftWidth + _style.Background.RightWidth;
                    return width;
                }
                return 150;
            }
        }

        public override float PrefHeight
        {
            get
            {
                if (_widget is ILayout) {
                    float height = (_widget as ILayout).PrefHeight;
                    if (_style.Background != null)
                        height += _style.Background.TopHeight + _style.Background.BottomHeight;
                    return height;
                }
                return 150;
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

        public Actor Widget
        {
            get { return _widget; }
            set
            {
                if (value == this)
                    throw new ArgumentException("Widget cannot be same object");
                if (_widget != null)
                    base.RemoveActor(_widget);

                _widget = value;
                if (_widget != null)
                    base.AddActor(_widget);
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

        public override bool RemoveActor (Actor actor)
        {
            if (actor != _widget)
                return false;

            Widget = null;
            return true;
        }

        public override Actor Hit (float x, float y, bool touchable)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;
            if (IsScrollX && _hScrollBounds.Contains(x, y))
                return this;
            if (IsScrollY && _vScrollBounds.Contains(x, y))
                return this;

            return base.Hit(x, y, touchable);
        }

        public float ScrollX { get; protected set; }
        public float ScrollY { get; protected set; }

        public float VisualScrollX
        {
            get { return IsScrollX ? _visualAmountX : 0; }
            set { _visualAmountX = value; }
        }

        public float VisualScrollY
        {
            get { return IsScrollY ? _visualAmountY : 0; }
            set { _visualAmountY = value; }
        }

        public void SetScrollX (float pixels)
        {
            ScrollX = MathHelper.Clamp(pixels, 0, MaxX);
        }

        public void SetScrollY (float pixels)
        {
            ScrollY = MathHelper.Clamp(pixels, 0, MaxY);
        }

        public void UpdateVisualScroll ()
        {
            _visualAmountX = ScrollX;
            _visualAmountY = ScrollY;
        }

        public float ScrollPercentX
        {
            get { return MathHelper.Clamp(ScrollX / MaxX, 0, 1); }
            set { ScrollX = MaxX * MathHelper.Clamp(value, 0, 1); }
        }

        public float ScrollPercentY
        {
            get { return MathHelper.Clamp(ScrollY / MaxY, 0, 1); }
            set { ScrollY = MaxY * MathHelper.Clamp(value, 0, 1); }
        }

        [TODO]
        public bool FlickScroll
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void ScrollTo (float x, float y, float width, float height)
        {
            float amountX = ScrollX;
            if (x + width > amountX + _areaWidth)
                amountX = x + width - _areaWidth;
            if (x < amountX)
                amountX = x;
            ScrollX = MathHelper.Clamp(amountX, 0, MaxX);

            float amountY = ScrollY;
            if (amountY > MaxY - y - height + _areaHeight)
                amountY = MaxY - y - height + _areaHeight;
            if (amountY < MaxY - y)
                amountY = MaxY - y;
            ScrollY = MathHelper.Clamp(amountY, 0, MaxY);
        }

        public void ScrollToCenter (float x, float y, float width, float height)
        {
            float amountX = ScrollX;
            if (x + width > amountX + _areaWidth)
                amountX = x + width - _areaWidth;
            if (x < amountX)
                amountX = x;
            ScrollX = MathHelper.Clamp(amountX, 0, MaxX);

            float amountY = ScrollY;
            float centerY = MaxY - y + _areaHeight / 2 - height / 2;
            if (amountY < centerY - _areaHeight / 4 || amountY > centerY + _areaHeight / 4)
                amountY = centerY;
            ScrollY = MathHelper.Clamp(amountY, 0, MaxY);
        }

        public float MaxX { get; private set; }
        public float MaxY { get; private set; }

        public float ScrollBarHeight
        {
            get { return _style.HScrollKnob == null || !IsScrollX ? 0 : _style.HScrollKnob.MinHeight; }
        }

        public float ScrollBarWidth
        {
            get { return _style.VScrollKnob == null || !IsScrollY ? 0 : _style.VScrollKnob.MinWidth; }
        }

        public bool IsScrollX { get; private set; }
        public bool IsScrollY { get; private set; }

        public bool IsDisabledX { get; set; }
        public bool IsDisabledY { get; set; }

        public bool IsDragging
        {
            get { return _draggingPointer != -1; }
        }

        [TODO]
        public bool IsPanning
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsFlinging
        {
            get { return _flingTimer > 0; }
        }

        public float VelocityX
        {
            get
            {
                if (_flingTimer <= 0)
                    return 0;
                float alpha = _flingTimer / FlingTime;
                alpha = alpha * alpha * alpha;
                return _velocityX * alpha * alpha * alpha;
            }
            set { _velocityX = value; }
        }

        public float VelocityY
        {
            get { return _velocityY; }
            set { _velocityY = value; }
        }

        public bool OverscrollX { get; set; }
        public bool OverscrollY { get; set; }

        public float OverscrollDistance { get; set; }
        public float OverscrollSpeedMin { get; set; }
        public float OverscrollSpeedMax { get; set; }

        public bool ForceScrollX { get; set; }
        public bool ForceScrollY { get; set; }

        public float FlingTime { get; set; }

        public bool IsClamped { get; set; }

        public bool FadeScrollBars
        {
            get { return _fadeScrollBars; }
            set
            {
                if (_fadeScrollBars == value)
                    return;
                _fadeScrollBars = value;
                if (!_fadeScrollBars)
                    _fadeAlpha = FadeAlphaSeconds;
                Invalidate();
            }
        }

        public float FadeAlphaSeconds { get; set; }
        public float FadeDelaySeconds { get; set; }

        public bool SmoothScrolling { get; set; }

        public bool ScrollBarsOnTop
        {
            get { return _scollbarsOnTop; }
            set
            {
                _scollbarsOnTop = value;
                Invalidate();
            }
        }

        public bool CancelTouchFocus { get; set; }
    }

    public class ScrollPaneStyle
    {
        public ScrollPaneStyle ()
        { }

        public ScrollPaneStyle (ISceneDrawable background, ISceneDrawable hScroll, ISceneDrawable hScrollKnob, 
            ISceneDrawable vScoll, ISceneDrawable vScrollKnob)
        {
            Background = background;
            HScroll = hScroll;
            HScrollKnob = hScrollKnob;
            VScroll = vScoll;
            VScrollKnob = vScrollKnob;
        }

        public ScrollPaneStyle (ScrollPaneStyle style)
        {
            Background = style.Background;
            HScroll = style.HScroll;
            HScrollKnob = style.HScrollKnob;
            VScroll = style.VScroll;
            VScrollKnob = style.VScrollKnob;
        }

        public ISceneDrawable Background { get; set; }
        public ISceneDrawable Corner { get; set; }
        public ISceneDrawable HScroll { get; set; }
        public ISceneDrawable HScrollKnob { get; set; }
        public ISceneDrawable VScroll { get; set; }
        public ISceneDrawable VScrollKnob { get; set; }
    }
}
