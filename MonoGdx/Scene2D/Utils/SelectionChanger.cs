using System;
using System.Collections.Generic;

namespace MonoGdx.Scene2D.Utils
{
    internal class SelectionChanger<T>
    {
        private bool _active;
        private bool _canSelectMultiple;

        private List<T> _internalSelected = new List<T>();
        private List<T> _selected;
        private List<T> _toSelect = new List<T>();
        private List<T> _toUnselect = new List<T>();

        public Action<List<T>, List<T>> SelectionChangeHandler;

        public bool CanSelectMultiple
        {
            get { return _canSelectMultiple; }
            set
            {
                if (_canSelectMultiple == value)
                    return;

                _canSelectMultiple = value;
                ApplySelectMultiple();
            }
        }

        public void Begin ()
        {
            _internalSelected.Clear();
            Begin(_internalSelected);
        }

        public void Begin (T item)
        {
            _internalSelected.Clear();
            _internalSelected.Add(item);
            Begin(_internalSelected);
        }

        public void Begin (List<T> selection)
        {
            _active = true;
            _toSelect.Clear();
            _toUnselect.Clear();

            _selected = selection;
        }

        public List<T> End ()
        {
            ApplySelectMultiple();

            foreach (var item in _toUnselect)
                _selected.Remove(item);
            foreach (var item in _toSelect)
                _selected.Add(item);

            if (_toSelect.Count > 0 || _toUnselect.Count > 0) {
                if (SelectionChangeHandler != null)
                    SelectionChangeHandler(_toSelect, _toUnselect);
            }

            _active = false;
            _toSelect.Clear();
            _toUnselect.Clear();

            return _selected;
        }

        public bool Select (T item)
        {
            if (_toUnselect.Remove(item))
                return true;
            if (_toSelect.Contains(item) || _selected.Contains(item))
                return false;

            if (!_canSelectMultiple && _toSelect.Count > 0)
                _toSelect.Clear();

            _toSelect.Add(item);

            return true;
        }

        public void SelectAll (IEnumerable<T> items)
        {
            _toUnselect.Clear();

            foreach (var item in items) {
                if (!_selected.Contains(item))
                    _toSelect.Add(item);
            }
        }

        public void SelectOnly (T item)
        {
            UnselectAll();

            if (_toUnselect.Remove(item))
                return;

            _toSelect.Add(item);
        }

        public bool Unselect (T item)
        {
            if (_toSelect.Remove(item))
                return true;
            if (_toUnselect.Contains(item) || !_selected.Contains(item))
                return false;

            _toUnselect.Add(item);

            return true;
        }

        public void UnselectAll ()
        {
            _toSelect.Clear();

            _toUnselect.Clear();
            _toUnselect.AddRange(_selected);
        }

        private void ApplySelectMultiple ()
        {
            if (_canSelectMultiple)
                return;

            if (_toSelect.Count > 1)
                _toSelect.RemoveRange(0, _toSelect.Count - 1);

            if (_toSelect.Count == 1) {
                _toUnselect.Clear();
                _toUnselect.AddRange(_selected);
            }
            else if (_toUnselect.Count + 1 < _selected.Count) {
                _toUnselect.Clear();
                for (int i = 1; i < _selected.Count; i++)
                    _toUnselect.Add(_selected[i]);
            }
        }
    }
}
