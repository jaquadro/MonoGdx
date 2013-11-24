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

using MonoGdx.Geometry;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.Actions
{
    /// <summary>
    /// Base class for actions that transition over time using the percent complete.
    /// </summary>
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

        public override void Restart ()
        {
            Time = 0;
            _complete = false;
        }

        public override void Reset ()
        {
            base.Reset();
            IsReverse = false;
            Interpolation = null;
        }

        public float Time { get; set; }
        public float Duration { get; set; }
        public Interpolation Interpolation { get; set; }
        public bool IsReverse { get; set; }
    }
}
