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
