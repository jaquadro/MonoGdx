using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoScene2D.Utils;

namespace MonoScene2D.Scene2D
{
    [TODO]
    public class Event : IPoolable
    {
        public Actor TargetActor { get; set; }
        public Actor ListenerActor { get; set; }
        public Stage Stage { get; set; }
        
        public bool Bubbles { get; set; }
        public bool IsHandled { get; private set; }
        public bool IsCancelled { get; private set; }
        public bool IsStopped { get; private set; }
        public bool IsCapture { get; set; }

        public void Handle ()
        {
            IsHandled = true;
        }

        public void Cancel ()
        {
            IsCancelled = true;
            IsStopped = true;
            IsHandled = true;
        }

        public void Stop ()
        {
            IsStopped = true;
        }

        public virtual void Reset ()
        {
            Stage = null;
            TargetActor = null;
            ListenerActor = null;
            IsCapture = false;
            Bubbles = false;
            IsHandled = false;
            IsStopped = false;
            IsCancelled = false;
        }
    }
}
