using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGdx.Utils
{
    public static class Pools<T>
        where T : new()
    {
        private static readonly Pool<T> _pool = new Pool<T>();

        public static Pool<T> Pool
        {
            get { return _pool; }
        }

        public static T Obtain ()
        {
            return _pool.Obtain();
        }

        public static void Release (T obj)
        {
            _pool.Release(obj);
        }

        public static void Release (IList<T> objects)
        {
            _pool.Release(objects);
        }
    }
}
