using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGdx.Input
{
    [TODO]
    public class GestureDetector : InputAdapter
    {
        private readonly GestureListener _listener;
        private long _tapCounterInterval;

        private bool _inTapSquare;
        private int _tapCount;
        private long _lastTapTime;
        private float _lastTapX;
        private float _lastTapY;
        private int _lastTapButton;
        private int _lastTapPointer;
        private bool _longPressFired;
        private bool _pinching;
        private bool _panning;

        private readonly VelocityTracker _tracker = new VelocityTracker();
        private float _tapSquareCenterX;
        private float _tapSquareCenterY;
        private long _gestureStartTime;
        private Vector2 _pointer1;
        private Vector2 _pointer2;
        private Vector2 _initialPointer1;
        private Vector2 _initialPointer2;

        // LongPressTask
        
        public GestureDetector (GestureListener listener)
            : this(20, .4f, 1.1f, .15f, listener)
        { }

        public GestureDetector (float halfTapSquareSize, float tapCountInterval, float longPressDuration, float maxFlingDelay, GestureListener listener)
        {
            TapSquareSize = halfTapSquareSize;
            _tapCounterInterval = (long)(tapCountInterval * 1000000000L);
            LongPressSeconds = longPressDuration;
            MaxFlingDelay = (long)(maxFlingDelay * 1000000000L);
            _listener = listener;
        }

        public override bool TouchDown (int screenX, int screenY, int pointer, int button)
        {
            return TouchDown((float)screenX, (float)screenY, pointer, button);
        }

        public bool TouchDown (float x, float y, int pointer, int button)
        {
            if (pointer > 1)
                return false;

            if (pointer == 0) {
                _pointer1 = new Vector2(x, y);
                _gestureStartTime = DateTime.Now.Ticks * 100;
                //_tracker.Start(x, y, _gestureStartTime);
            }

            return _listener.TouchDown(x, y, pointer, button);
        }

        public override bool TouchDragged (int screenX, int screenY, int pointer)
        {
            throw new NotImplementedException();
        }

        public bool TouchDragged (float x, float y, int pointer)
        {
            throw new NotImplementedException();
        }

        public override bool TouchUp (int screenX, int screenY, int pointer, int button)
        {
            throw new NotImplementedException();
        }

        public bool TouchUp (float x, float y, int pointer, int button)
        {
            throw new NotImplementedException();
        }

        public bool IsLongPressed
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsPressedFor (float duration)
        {
            throw new NotImplementedException();
        }

        public bool IsPanning
        {
            get { throw new NotImplementedException(); }
        }

        public void Reset ()
        {
            throw new NotImplementedException();
        }

        private bool IsWithinTapSquare (float x, float y, float centerX, float centerY)
        {
            throw new NotImplementedException();
        }

        public void InvalidateTapSquare ()
        {
            throw new NotImplementedException();
        }

        public float TapSquareSize { get; set; }

        public float TapCountInterval
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public float LongPressSeconds { get; set; }

        public long MaxFlingDelay { get; set; }
    }

    public interface GestureListener
    {
        bool TouchDown (float x, float y, int pointer, int button);
        bool Tap (float x, float y, int count, int button);
        bool LongPress (float x, float y);

        bool Fling (float velocityX, float velocityY, int button);
        bool Pan (float x, float y, float deltaX, float deltaY);
        bool Zoom (float initialDistance, float distance);
        bool Pinch (Vector2 initialPointer1, Vector2 initialPointer2, Vector2 pointer1, Vector2 pointer2);
    }

    public class GestureAdapter : GestureListener
    {
        public bool TouchDown(float x, float y, int pointer, int button)
        {
 	        return false;
        }

        public bool Tap(float x, float y, int count, int button)
        {
 	        return false;
        }

        public bool LongPress(float x, float y)
        {
 	        return false;
        }

        public bool Fling(float velocityX, float velocityY, int button)
        {
 	        return false;
        }

        public bool Pan(float x, float y, float deltaX, float deltaY)
        {
 	        return false;
        }

        public bool Zoom(float initialDistance, float distance)
        {
 	        return false;
        }

        public bool Pinch(Vector2 initialPointer1, Vector2 initialPointer2, Vector2 pointer1, Vector2 pointer2)
        {
 	        return false;
        }
    }

    internal class VelocityTracker
    {
    }

    /*public abstract class GdxTask
    {
        long _executeTimeMillis;
        long _intervalMillis;
    }*/
}
