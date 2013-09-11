using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace MonoGdx.Scene2D.Utils
{
    public class ClickListener : InputListener
    {
        private long _tapCountInterval = (long)(0.4 * 1000000000L);
        private long _lastTapTime;
        private bool _pressed;
        private bool _over;
        private bool _cancelled;

        public ClickListener ()
        {
            TapSquareSize = 14;
            TouchDownX = -1;
            TouchDownY = -1;
            PressedPointer = -1;
            PressedButton = -1;
        }

        public ClickListener (int button)
            : this()
        {
            Button = button;
        }

        public override bool TouchDown (InputEvent e, float x, float y, int pointer, int button)
        {
            if (IsPressed)
                return false;
            if (pointer == 0 && Button != -1 && Button != button)
                return false;

            _pressed = true;

            PressedPointer = pointer;
            PressedButton = button;
            TouchDownX = x;
            TouchDownY = y;
            return true;
        }

        public override void TouchDragged (InputEvent e, float x, float y, int pointer)
        {
            if (pointer != PressedPointer || _cancelled)
                return;

            _pressed = IsOverActor(e.ListenerActor, x, y);

            if (_pressed && pointer == 0 && Button != -1 && !Mouse.GetState().IsButtonPressed(Button))
                _pressed = false;

            if (!_pressed)
                InvalidateTapSquare();
        }

        public override void TouchUp (InputEvent e, float x, float y, int pointer, int button)
        {
            if (pointer == PressedPointer) {
                if (!_cancelled) {
                    bool touchUpOver = IsOverActor(e.ListenerActor, x, y);

                    if (touchUpOver && pointer == 0 && Button != -1 && button != Button)
                        touchUpOver = false;
                    if (touchUpOver) {
                        long time = DateTime.Now.Ticks * 100;
                        if (time - _lastTapTime > _tapCountInterval)
                            TapCount = TapCount + 1;
                        _lastTapTime = time;

                        Clicked(e, x, y);
                    }
                }

                _pressed = false;
                PressedPointer = -1;
                PressedButton = -1;
                _cancelled = false;
            }
        }

        public override void Enter (InputEvent e, float x, float y, int pointer, Actor fromActor)
        {
            if (pointer == -1 && !_cancelled)
                _over = true;
        }

        public override void Exit (InputEvent e, float x, float y, int pointer, Actor toActor)
        {
            if (pointer == -1 && !_cancelled)
                _over = false;
        }

        public void Cancel ()
        {
            if (PressedPointer == -1)
                return;

            _cancelled = true;
            _over = false;
            _pressed = false;
        }

        public virtual void Clicked (InputEvent ev, float x, float y)
        { }

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

    public class DispatchClickListener : ClickListener
    {
        public Action<InputEvent, float, float> OnClicked { get; set; }
        public Func<InputEvent, float, float, int, int, bool> OnTouchDown { get; set; }

        public override void Clicked (InputEvent ev, float x, float y)
        {
            if (OnClicked != null)
                OnClicked(ev, x, y);
        }

        public override bool TouchDown (InputEvent e, float x, float y, int pointer, int button)
        {
            return OnClicked != null ? OnTouchDown(e, x, y, pointer, button) : base.TouchDown(e, x, y, pointer, button);
        }
    }
}
