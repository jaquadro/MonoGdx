using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGdx.Utils
{
    public interface IPoolable
    {
        void Reset ();
    }

    public class Pool<T>
        where T : new()
    {
        private Stack<T> _free;

        public Pool ()
            : this(16, int.MaxValue)
        { }

        public Pool (int initialCapacity)
            : this(initialCapacity, int.MaxValue)
        { }

        public Pool (int initialCapacity, int max)
        {
            MaxReserve = max;
            _free = new Stack<T>(initialCapacity);
        }

        public int MaxReserve { get; set; }

        public int Peak { get; set; }

        public int Count
        {
            get { return _free.Count; }
        }

        public T Obtain ()
        {
            if (_free.Count == 0)
                return new T();

            return _free.Pop();
        }

        public void Release (T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (_free.Count < MaxReserve) {
                _free.Push(obj);
                Peak = Math.Max(Peak, _free.Count);
            }

            if (obj is IPoolable)
                (obj as IPoolable).Reset();
        }

        public void Release (IList<T> objects)
        {
            if (objects == null)
                throw new ArgumentNullException("objects");

            foreach (T obj in objects) {
                if (obj == null)
                    continue;

                if (_free.Count < MaxReserve)
                    _free.Push(obj);

                if (obj is IPoolable)
                    (obj as IPoolable).Reset();
            }

            Peak = Math.Max(Peak, _free.Count);
        }

        public void Clear ()
        {
            _free.Clear();
            Peak = 0;
        }
    }
}
