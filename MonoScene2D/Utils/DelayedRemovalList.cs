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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGdx.Utils
{
    public class DelayedRemovalList<T> : Collection<T>
    {
        private bool _iterating;
        private List<int> _remove = new List<int>(0);

        public DelayedRemovalList ()
        { }

        public DelayedRemovalList (IList<T> list)
            : base(list)
        { }

        public DelayedRemovalList (int initialCapacity)
            : base(new List<T>(initialCapacity))
        { }

        public void Begin ()
        {
            _iterating = true;
        }

        public void End ()
        {
            _iterating = false;
            for (int i = 0, n = _remove.Count; i < n; i++)
                RemoveAt(_remove[i]);
            _remove.Clear();
        }

        private void Remove (int index)
        {
            for (int i = 0, n = _remove.Count; i < n; i++) {
                int removeIndex = _remove[i];
                if (index == removeIndex)
                    return;
                if (index < removeIndex) {
                    _remove.Insert(i, index);
                    return;
                }
            }
            _remove.Add(index);
        }

        protected override void RemoveItem (int index)
        {
            if (_iterating)
                Remove(index);
            else
                base.RemoveItem(index);
        }

        protected override void ClearItems ()
        {
            if (_iterating)
                throw new InvalidOperationException("Invalid between begin/end");
            base.ClearItems();
        }

        protected override void InsertItem (int index, T item)
        {
            if (_iterating)
                throw new InvalidOperationException("Invalid between begin/end");
            base.InsertItem(index, item);
        }

        protected override void SetItem (int index, T item)
        {
            if (_iterating)
                throw new InvalidOperationException("Invalid between begin/end");
            base.SetItem(index, item);
        }
    }
}
