using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGdx.Geometry;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.Actions
{
    public abstract class TemporalAction : SceneAction
    {
        private bool _complete;

        protected TemporalAction ()
        { }

        protected TemporalAction (float duration)
        {
            Duration = duration;
        }

        protected TemporalAction (float duration, Interpolation interpolation)
        {
            Duration = duration;
            Interpolation = interpolation;
        }
        
        public override bool Act (float delta)
        {
            if (_complete)
                return true;

            Pool pool = Pool;
            Pool = null;

            try {
                if (Time == 0)
                    Begin();
                Time += delta;

                _complete = Time >= Duration;
                float percent = 1;

                if (!_complete) {
                    percent = Time / Duration;
                    if (Interpolation != null)
                        percent = Interpolation.Apply(percent);
                }

                Update(IsReverse ? 1 - percent : percent);
                if (_complete)
                    End();

                return _complete;
            }
            finally {
                Pool = pool;
            }
        }

        protected virtual void Begin ()
        { }

        protected virtual void End ()
        { }

        protected abstract void Update (float percent);

        public void Finish ()
        {
            Time = Duration;
        }

        public float Time { get; set; }
        public float Duration { get; set; }
        public Interpolation Interpolation { get; set; }
        public bool IsReverse { get; set; }
    }
}
