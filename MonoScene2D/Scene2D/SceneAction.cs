using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D
{
    public class SceneAction : IPoolable
    {
        private Actor _actor;

        public Actor Actor
        {
            get { return _actor; }
            set {
                _actor = Actor;
                if (_actor == null && Pool != null) {
                    Pool.Release(this);
                    Pool = null;
                }
            }
        }

        public Pool<SceneAction> Pool { get; set; }

        public abstract bool Act (float delta);

        public virtual void Restart ()
        { }

        public virtual void Reset ()
        {
            Restart();
        }

        public override string ToString ()
        {
            string name = GetType().Name;
            int index = name.LastIndexOf('.');
            if (index != -1)
                name = name.Substring(index + 1);
            if (name.EndsWith("Action"))
                name = name.Substring(0, name.Length - 6);
            return name;
        }
    }
}
