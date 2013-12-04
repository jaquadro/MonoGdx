using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGdx.Scene2D.Utils
{
    internal class ClickEventManager
    {
        private Actor _actor;
        private long _tapCountInterval = (long)(0.4 * 1000000000L);
        private long _lastTapTime;
        private bool _pressed;
        private bool _over;
        private bool _cancelled;

        public ClickEventManager (Actor actor)
        {
            _actor = actor;
            _actor.TouchDown += TouchDownHandler;
            _actor.TouchDrag += TouchDraggedHandler;
            _actor.TouchUp += TouchUpHandler;
            _actor.TouchEnter += TouchEnterHandler;
            _actor.TouchLeave += TouchLeaveHandler;

            TapSquareSize = 14;
            TouchDownX = -1;
            TouchDownY = -1;
            PressedPointer = -1;
            PressedButton = -1;
        }

        public ClickEventManager (Actor actor, int button)
            : this(actor)
        {
            Button = button;
        }

        public Action<TouchEventArgs> ClickHandler { get; set; }

        private void TouchDownHandler (Actor sender, TouchEventArgs e)
        {
            if (IsPressed)
                return;
            if (e.Pointer == 0 && Button != -1 && Button != e.Button)
                return;

            _pressed = true;
            if (_actor.Stage != null)
                _actor.CaptureTouch(e.Pointer);

            PressedPointer = e.Pointer;
            PressedButton = e.Button;
            TouchDownX = e.StagePosition.X;
            TouchDownY = e.StagePosition.Y;

            e.Handled = true;
        }

        private void TouchDraggedHandler (Actor sender, TouchEventArgs e)
        {
            if (e.Pointer != PressedPointer || _cancelled)
                return;

            Vector2 position = e.GetPosition(sender);

            _pressed = IsOverActor(sender, position.X, position.Y);

            if (_pressed && e.Pointer == 0 && Button != -1 && !Mouse.GetState().IsButtonPressed(Button))
                _pressed = false;

            if (!_pressed)
                InvalidateTapSquare();
        }

        private void TouchUpHandler (Actor sender, TouchEventArgs e)
        {
            if (e.Pointer == PressedPointer) {
                if (!_cancelled) {
                    Vector2 position = e.GetPosition(sender);
                    bool touchUpOver = IsOverActor(sender, position.X, position.Y);

                    if (touchUpOver && e.Pointer == 0 && Button != -1 && e.Button != Button)
                        touchUpOver = false;
                    if (touchUpOver) {
                        long time = DateTime.Now.Ticks * 100;
                        if (time - _lastTapTime > _tapCountInterval)
                            TapCount = 0;
                        TapCount = TapCount + 1;
                        _lastTapTime = time;

                        if (ClickHandler != null)
                            ClickHandler(e);
                    }

                    if (_actor.Stage != null)
                        _actor.ReleaseTouchCapture(e.Pointer);
                    else if (e.Stage != null)
                        e.Stage.ReleaseTouchCapture(e.Pointer);
                }

                _pressed = false;
                PressedPointer = -1;
                PressedButton = -1;
                _cancelled = false;
            }
        }

        private void TouchEnterHandler (Actor sender, TouchEventArgs e)
        {
            if (e.Pointer == -1 && !_cancelled)
                _over = true;
        }

        private void TouchLeaveHandler (Actor sender, TouchEventArgs e)
        {
            if (e.Pointer == -1 && !_cancelled)
                _over = false;
        }

        public void Cancel ()
        {
            if (PressedPointer == -1)
                return;

            _actor.ReleaseTouchCapture(PressedPointer);

            _cancelled = true;
            _over = false;
            _pressed = false;
        }

        public bool IsOverActor (Actor actor, float x, float y)
        {
            Actor hit = actor.Hit(x, y, true);
            if (hit == null || !hit.IsDescendentOf(actor))
                return InTapSquare(x, y);
            return true;
        }

        public bool InTapSquare (float x, float y)
        {
            if (TouchDownX == -1 && TouchDownY == -1)
                return false;
            return Math.Abs(x - TouchDownX) < TapSquareSize && Math.Abs(y - TouchDownY) < TapSquareSize;
        }

        public void InvalidateTapSquare ()
        {
            TouchDownX = -1;
            TouchDownY = -1;
        }

        public bool IsPressed
        {
            get { return _pressed; }
        }

        public bool IsOver
        {
            get { return _over || _pressed; }
        }

        public float TapSquareSize { get; set; }

        public void SetTapCountInterval (float tapCountInterval)
        {
            _tapCountInterval = (long)(tapCountInterval * 1000000000L);
        }

        public int TapCount { get; private set; }

        public float TouchDownX { get; private set; }

        public float TouchDownY { get; private set; }

        public int PressedButton { get; private set; }

        public int PressedPointer { get; private set; }

        public int Button { get; set; }
    }
}
