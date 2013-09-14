/**
 * Copyright 2013 See AUTHORS file.
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

    public abstract class Pool
    {
        public abstract int MaxReserve { get; set; }
        public abstract int Peak { get; set; }
        public abstract int Count { get; }

        public void Release (object obj)
        {
            ReleaseCore(obj);
        }

        protected abstract void ReleaseCore (object obj);

        public abstract void Clear ();
    }

    public class Pool<T> : Pool
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

        public override int MaxReserve { get; set; }

        public override int Peak { get; set; }

        public override int Count
        {
            get { return _free.Count; }
        }

        public T Obtain ()
        {
            if (_free.Count == 0)
                return new T();

            return _free.Pop();
        }

        protected override void ReleaseCore (object obj)
        {
            if (obj is T)
                Release((T)obj);
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

        public override void Clear ()
        {
            _free.Clear();
            Peak = 0;
        }
    }
}
