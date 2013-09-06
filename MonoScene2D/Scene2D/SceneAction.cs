using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D
{
    public abstract class SceneAction : IPoolable
    {
        private Actor _actor;
        private Pool _pool;

        public abstract bool Act (float delta);

        public virtual Actor Actor
        {
            get { return _actor; }
            set
            {
                _actor = value;
                if (_actor == null && _pool != null) {
                    _pool.Release(this);
                    _pool = null;
                }
            }
        }

        public Pool Pool
        {
            get { return _pool; }
            set { _pool = value; }
        }

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
