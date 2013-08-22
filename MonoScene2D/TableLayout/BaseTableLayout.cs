using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoScene2D.TableLayout
{
    public abstract class BaseTableLayout
    {
        public object Table
        {
            get { return TableCore; }
        }

        public Toolkit Toolkit
        {
            get { return ToolkitCore; }
        }

        protected abstract object TableCore { get; }

        protected abstract Toolkit ToolkitCore { get; }

        public Cell Row ()
        {
            return RowCore();
        }

        protected abstract Cell RowCore ();
    }

    public enum Debug
    {
        None,
        All,
        Table,
        Cell,
        Widget,
    }

    [Flags]
    public enum Alignment
    {
        None = 0,
        Center = 1,
        Top = 2,
        Bottom = 4,
        Left = 8,
        Right = 16,
    }

    public abstract class BaseTableLayout<T, TTable, TLayout, TToolkit> : BaseTableLayout
        where T : class
        where TTable : T
        where TLayout : BaseTableLayout<T, TTable, TLayout, TToolkit>
        where TToolkit : Toolkit<T, TTable, BaseTableLayout<T, TTable, TLayout, TToolkit>>
    {
        private TToolkit _toolkit;
        private TTable _table;

        private int _columns;
        private int _rows;

        private readonly List<Cell> _cells = new List<Cell>(4);
        private readonly Cell _cellDefaults;
        private readonly List<Cell> _columnDefaults = new List<Cell>(2);
        private Cell _rowDefaults;

        private bool _sizeInvalid = true;
        private float[] _colMinWidth;
        private float[] _rowMinHeight;
        private float[] _colPrefWidth;
        private float[] _rowPrefHeight;
        private float _tableMinWidth;
        private float _tableMinHeight;
        private float _tablePrefWidth;
        private float _tablePrefHeight;
        private float[] _colWidth;
        private float[] _rowHeight;
        private float[] _expandWidth;
        private float[] _expandHeight;
        private float[] _colWeightedWidth;
        private float[] _rowWeightedHeight;

        internal Value _padTop;
        internal Value _padLeft;
        internal Value _padBottom;
        internal Value _padRight;
        internal Alignment _align = Alignment.Center;
        internal Debug _debug = TableLayout.Debug.None;

        public BaseTableLayout (TToolkit toolkit)
        {
            _toolkit = toolkit;
            _cellDefaults = toolkit.ObtainCell(this);
            _cellDefaults.Defaults();
        }

        public void Invalidate ()
        {
            _sizeInvalid = true;
        }

        public abstract void InvalidateHierarchy ();

        public Cell<T> Add (T widget)
        {
            Cell<T> cell = _toolkit.ObtainCell(this);
            cell.Widget = widget;

            if (_cells.Count > 0) {
                Cell lastCell = _cells[_cells.Count - 1];
                if (!lastCell.IsEndRow) {
                    cell.Column = lastCell.Column + (lastCell.Colspan ?? 0);
                    cell.Row = lastCell.Row;
                }
                else {
                    cell.Column = 0;
                    cell.Row = lastCell.Row + 1;
                }

                if (cell.Row > 0) {
                    for (int i = _cells.Count - 1; i >= 0; i--) {
                        Cell other = _cells[i];
                        for (int column = other.Column, nn = column + (other.Colspan ?? 0); column < nn; column++) {
                            if (column == cell.Column) {
                                cell.CellAboveIndex = i;
                                goto outer;
                            }
                        }
                    }
                    outer: ;
                }
            }
            else {
                cell.Column = 0;
                cell.Row = 0;
            }

            _cells.Add(cell);

            cell.Set(_cellDefaults);
            if (cell.Column < _columnDefaults.Count) {
                Cell columnCell = _columnDefaults[cell.Column];
                if (columnCell != null)
                    cell.Merge(columnCell);
            }

            cell.Merge(_rowDefaults);

            if (widget != null)
                _toolkit.AddChild(_table, widget);

            return cell;
        }

        public new Cell Row ()
        {
            if (_cells.Count > 0) {
                EndRow();
                Invalidate();
            }

            if (_rowDefaults != null)
                _toolkit.FreeCell(_rowDefaults);

            _rowDefaults = _toolkit.ObtainCell(this);
            _rowDefaults.Clear();

            return _rowDefaults;
        }

        protected override Cell RowCore ()
        {
            return Row();
        }

        private void EndRow ()
        {
            int rowColumns = 0;
            for (int i = _cells.Count - 1; i >= 0; i--) {
                Cell cell = _cells[i];
                if (cell.IsEndRow)
                    break;

                rowColumns += cell.Colspan ?? 0;
            }

            _columns = Math.Max(_columns, rowColumns);
            _rows++;

            _cells[_cells.Count - 1].IsEndRow = true;
        }

        public Cell ColumnDefaults (int column)
        {
            Cell cell = _columnDefaults.Count > column ? _columnDefaults[column] : null;
            if (cell == null) {
                cell = _toolkit.ObtainCell(this);
                cell.Clear();

                if (column >= _columnDefaults.Count) {
                    for (int i = _columnDefaults.Count; i < column; i++)
                        _columnDefaults.Add(null);
                    _columnDefaults.Add(cell);
                }
                else
                    _columnDefaults[column] = cell;
            }

            return cell;
        }

        public void Reset ()
        {
            Clear();

            _padTop = null;
            _padLeft = null;
            _padBottom = null;
            _padRight = null;
            _align = Alignment.Center;

            if (_debug != TableLayout.Debug.None)
                _toolkit.ClearDebugRectangles(this);
            _debug = TableLayout.Debug.None;

            _cellDefaults.Defaults();

            for (int i = 0, n = _columnDefaults.Count; i < n; i++) {
                Cell columnCell = _columnDefaults[i];
                if (columnCell != null)
                    _toolkit.FreeCell(columnCell);
            }

            _columnDefaults.Clear();
        }

        public void Clear ()
        {
            for (int i = _cells.Count - 1; i >= 0; i--) {
                Cell cell = _cells[i];
                object widget = cell.Widget;

                if (widget != null)
                    _toolkit.RemoveChild(_table, (T)widget);
                _toolkit.FreeCell(cell);
            }

            _cells.Clear();
            _rows = 0;
            _columns = 0;

            if (_rowDefaults != null)
                _toolkit.FreeCell(_rowDefaults);
            _rowDefaults = null;

            Invalidate();
        }

        public Cell GetCell (T widget)
        {
            for (int i = 0, n = _cells.Count; i < n; i++) {
                Cell c = _cells[i];
                if (c.Widget == widget)
                    return c;
            }

            return null;
        }

        public List<Cell> Cells
        {
            get { return _cells; }
        }

        public void SetToolkit (TToolkit toolkit)
        {
            _toolkit = toolkit;
        }

        public new TTable Table
        {
            get { return _table; }
            set { _table = value; }
        }

        protected override object TableCore
        {
            get { return _table; }
            //set { _table = value; }
        }

        public new TToolkit Toolkit
        {
            get { return _toolkit; }
        }

        protected override Toolkit ToolkitCore
        {
            get { return _toolkit; }
        }

        public float MinWidth
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _tableMinWidth;
            }
        }

        public float MinHeight
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _tableMinHeight;
            }
        }

        public float PrefWidth
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _tablePrefWidth;
            }
        }

        public float PrefHeight
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _tablePrefHeight;
            }
        }

        public Cell Defaults
        {
            get { return _cellDefaults; }
        }

        public TLayout Pad (Value pad)
        {
            _padTop = pad;
            _padLeft = pad;
            _padBottom = pad;
            _padRight = pad;
            _sizeInvalid = true;

            return this as TLayout;
        }

        public TLayout Pad (Value top, Value left, Value bottom, Value right)
        {
            _padTop = top;
            _padLeft = left;
            _padBottom = bottom;
            _padRight = right;
            _sizeInvalid = true;

            return this as TLayout;
        }

        public TLayout Pad (float pad)
        {
            _padTop = new FixedValue(pad);
            _padLeft = new FixedValue(pad);
            _padBottom = new FixedValue(pad);
            _padRight = new FixedValue(pad);
            _sizeInvalid = true;

            return this as TLayout;
        }

        public TLayout Pad (float top, float left, float bottom, float right)
        {
            _padTop = new FixedValue(top);
            _padLeft = new FixedValue(left);
            _padBottom = new FixedValue(bottom);
            _padRight = new FixedValue(right);
            _sizeInvalid = true;

            return this as TLayout;
        }

        public Alignment Align
        {
            get { return _align; }
            set { _align = value; }
        }

        public TLayout Center ()
        {
            _align |= Alignment.Top;
            _align &= ~Alignment.Bottom;

            return this as TLayout;
        }

        public TLayout Left ()
        {
            _align |= Alignment.Left;
            _align &= ~Alignment.Right;

            return this as TLayout;
        }

        public TLayout Bottom ()
        {
            _align |= Alignment.Bottom;
            _align &= ~Alignment.Top;

            return this as TLayout;
        }

        public TLayout Right ()
        {
            _align |= Alignment.Right;
            _align &= ~Alignment.Left;

            return this as TLayout;
        }

        public Debug Debug
        {
            get { return _debug; }
            set
            {
                _debug = value;
                if (_debug == TableLayout.Debug.None)
                    _toolkit.ClearDebugRectangles(this);
                else
                    Invalidate();
            }
        }

        public Value PadTopValue
        {
            get { return _padTop; }
            set
            {
                _padTop = value;
                _sizeInvalid = true;
            }
        }

        public float PadTop
        {
            get { return _padTop == null ? 0 : _padTop.Height(_table); }
            set
            {
                _padTop = new FixedValue(value);
                _sizeInvalid = true;
            }
        }

        public Value PadLeftValue
        {
            get { return _padLeft; }
            set
            {
                _padLeft = value;
                _sizeInvalid = true;
            }
        }

        public float PadLeft
        {
            get { return _padLeft == null ? 0 : _padLeft.Width(_table); }
            set
            {
                _padLeft = new FixedValue(value);
                _sizeInvalid = true;
            }
        }

        public Value PadBottomValue
        {
            get { return _padBottom; }
            set
            {
                _padBottom = value;
                _sizeInvalid = true;
            }
        }

        public float PadBottom
        {
            get { return _padBottom == null ? 0 : _padBottom.Height(_table); }
            set
            {
                _padBottom = new FixedValue(value);
                _sizeInvalid = true;
            }
        }

        public Value PadRightValue
        {
            get { return _padRight; }
            set
            {
                _padRight = value;
                _sizeInvalid = true;
            }
        }

        public float PadRight
        {
            get { return _padRight == null ? 0 : _padRight.Width(_table); }
            set
            {
                _padRight = new FixedValue(value);
                _sizeInvalid = true;
            }
        }

        public int GetRow (float y)
        {
            int row = 0;
            int i = 0;
            int n = _cells.Count;

            if (n == 0)
                return -1;
            if (n == 1)
                return 0;

            y += H(_padTop);

            if (_cells[0].WidgetY < _cells[1].WidgetY) {
                while (i < n) {
                    Cell c = _cells[i++];
                    if (c.Ignore == true)
                        continue;
                    if (c.WidgetY + c.ComputedPadTop > y)
                        break;
                    if (c.IsEndRow)
                        row++;
                }

                return row - 1;
            }

            while (i < n) {
                Cell c = _cells[i++];
                if (c.Ignore == true)
                    continue;
                if (c.WidgetY + c.ComputedPadTop < y)
                    break;
                if (c.IsEndRow)
                    row++;
            }

            return row;
        }

        private float[] EnsureSize (float[] array, int size)
        {
            if (array == null || array.Length < size)
                return new float[size];

            for (int i = 0, n = array.Length; i < n; i++)
                array[i] = 0;

            return array;
        }

        private float W (Value value)
        {
            return value == null ? 0 : value.Width(_table);
        }

        private float H (Value value)
        {
            return value == null ? 0 : value.Height(_table);
        }

        private float W (Value value, Cell cell)
        {
            return value == null ? 0 : value.Width(cell);
        }

        private float H (Value value, Cell cell)
        {
            return value == null ? 0 : value.Height(cell);
        }

        private void ComputeSize ()
        {
            _sizeInvalid = false;

            if (_cells.Count > 0 && !_cells[_cells.Count - 1].IsEndRow)
                EndRow();

            _colMinWidth = EnsureSize(_colMinWidth, _columns);
            _rowMinHeight = EnsureSize(_rowMinHeight, _rows);
            _colPrefWidth = EnsureSize(_colPrefWidth, _columns);
            _rowPrefHeight = EnsureSize(_rowPrefHeight, _rows);
            _colWidth = EnsureSize(_colWidth, _columns);
            _rowHeight = EnsureSize(_rowHeight, _rows);
            _expandWidth = EnsureSize(_expandWidth, _columns);
            _expandHeight = EnsureSize(_expandHeight, _rows);

            float spaceRightLast = 0;
            for (int i = 0, n = _cells.Count; i < n; i++) {
                Cell c = _cells[i];
                if (c.Ignore == true)
                    continue;

                // Collect columns/rows that expand
                if (c.ExpandY != 0 && _expandHeight[c.Row] == 0)
                    _expandHeight[c.Row] = c.ExpandY ?? 0;
                if (c.Colspan == 1 && c.ExpandX != 0 && _expandWidth[c.Column] == 0)
                    _expandWidth[c.Column] = c.ExpandX ?? 0;

                // Compute combined padding/spacing for cells.
                // Spacing between widgets isn't additive, the larger is used.  Also, no spacing around edges.
                c.ComputedPadLeft = W(c.PadLeftValue, c) + (c.Column == 0 ? 0 : Math.Max(0, W(c.SpaceLeftValue, c) - spaceRightLast));
                c.ComputedPadTop = H(c.PadTopValue, c);

                if (c.CellAboveIndex != -1) {
                    Cell above = _cells[c.CellAboveIndex];
                    c.ComputedPadTop += Math.Max(0, H(c.SpaceTopValue, c) - H(above.SpaceBottomValue, c));
                }

                float spaceRight = W(c.SpaceRightValue, c);
                c.ComputedPadRight = W(c.PadRightValue, c) + ((c.Column + c.Colspan) == _columns ? 0 : spaceRight);
                c.ComputedPadBottom = H(c.PadBottomValue, c) + (c.Row == _rows - 1 ? 0 : H(c.SpaceBottomValue, c));
                spaceRightLast = spaceRight;

                // Determine minimum and preferred cell sizes.
                float prefWidth = c.PrefWidthValue.Get(c);
                float prefHeight = c.PrefHeightValue.Get(c);
                float minWidth = c.MinWidthValue.Get(c);
                float minHeight = c.MinHeightValue.Get(c);
                float maxWidth = c.MaxWidthValue.Get(c);
                float maxHeight = c.MaxHeightValue.Get(c);

                if (prefWidth < minWidth)
                    prefWidth = minWidth;
                if (prefHeight < minHeight)
                    prefHeight = minHeight;

                if (maxWidth > 0 && prefWidth > maxWidth)
                    prefWidth = maxWidth;
                if (maxHeight > 0 && prefHeight > maxHeight)
                    prefHeight = maxHeight;

                if (c.Colspan == 1) {
                    float hpadding = c.ComputedPadLeft + c.ComputedPadRight;
                    _colPrefWidth[c.Column] = Math.Max(_colPrefWidth[c.Column], prefWidth + hpadding);
                    _colMinWidth[c.Column] = Math.Max(_colMinWidth[c.Column], minWidth + hpadding);
                }

                float vpadding = c.ComputedPadTop + c.ComputedPadBottom;
                _rowPrefHeight[c.Row] = Math.Max(_rowPrefHeight[c.Row], prefHeight + vpadding);
                _rowMinHeight[c.Row] = Math.Max(_rowMinHeight[c.Row], minHeight + vpadding);
            }

            // Colspan with expand will expand all spanned columns if none of the spanned columns have expand.
            for (int i = 0, n = _cells.Count; i < n; i++) {
                CONT_OUTER:

                Cell c = _cells[i];
                if (c.Ignore == true || c.ExpandX == 0)
                    continue;

                for (int column = c.Column, nn = column + (c.Colspan ?? 0); column < nn; column++) {
                    if (_expandWidth[column] != 0)
                        goto CONT_OUTER;
                }

                for (int column = c.Column, nn = column + (c.Colspan ?? 0); column < nn; column++)
                    _expandWidth[column] = c.ExpandX ?? 0;
            }

            // Distribute any additional min and pref width added by colspanned cells to the columns spanned.
            for (int i = 0, n = _cells.Count; i < n; i++) {
                Cell c = _cells[i];
                if (c.Ignore == true || c.Colspan == 1)
                    continue;

                float minWidth = c.MinWidthValue.Get(c);
                float prefWidth = c.PrefWidthValue.Get(c);
                float maxWidth = c.MaxWidthValue.Get(c);

                if (prefWidth < minWidth)
                    prefWidth = minWidth;
                if (maxWidth > 0 && prefWidth > maxWidth)
                    prefWidth = maxWidth;

                float spannedMinWidth = -(c.ComputedPadLeft + c.ComputedPadRight);
                float spannedPRefWidth = spannedMinWidth;

                for (int column = c.Column, nn = column + (c.Colspan ?? 0); column < nn; column++) {
                    spannedMinWidth += _colMinWidth[column];
                    spannedPRefWidth += _colPrefWidth[column];
                }

                // Distribute extra space using expand, if any columns have expand.
                float totalExpandWidth = 0;
                for (int column = c.Column, nn = column + (c.Colspan ?? 0); column < nn; column++)
                    totalExpandWidth += _expandWidth[column];

                float extraMinWidth = Math.Max(0, minWidth - spannedMinWidth);
                float extraPrefWidth = Math.Max(0, prefWidth - spannedPRefWidth);
                for (int column = c.Column, nn = column + (c.Colspan ?? 0); column < nn; column++) {
                    float ratio = totalExpandWidth == 0 ? (1f / (c.Colspan ?? 0)) : (_expandWidth[column] / totalExpandWidth);
                    _colMinWidth[column] += extraMinWidth * ratio;
                    _colPrefWidth[column] += extraPrefWidth * ratio;
                }
            }

            // Collect uniform size.
            float uniformMinWidth = 0;
            float uniformMinHeight = 0;
            float uniformPrefWidth = 0;
            float uniformPrefHeight = 0;

            for (int i = 0, n = _cells.Count; i < n; i++) {
                Cell c = _cells[i];
                if (c.Ignore == true)
                    continue;

                // Collect uniform sizes.
                if ((c.UniformX ?? false) && (c.Colspan ?? 0) == 1) {
                    float hpadding = c.ComputedPadLeft + c.ComputedPadRight;
                    uniformMinWidth = Math.Max(uniformMinWidth, _colMinWidth[c.Column] - hpadding);
                    uniformPrefWidth = Math.Max(uniformPrefWidth, _colPrefWidth[c.Column] - hpadding);
                }

                if (c.UniformY ?? false) {
                    float vpadding = c.ComputedPadTop + c.ComputedPadBottom;
                    uniformMinHeight = Math.Max(uniformMinHeight, _rowMinHeight[c.Row] - vpadding);
                    uniformPrefHeight = Math.Max(uniformPrefHeight, _rowPrefHeight[c.Row] - vpadding);
                }
            }

            // Size unform cells to the same width/height.
            if (uniformPrefWidth > 0 || uniformPrefHeight > 0) {
                for (int i = 0, n = _cells.Count; i < n; i++) {
                    Cell c = _cells[i];
                    if (c.Ignore == true)
                        continue;

                    if (uniformPrefWidth > 0 && (c.UniformX ?? false) && (c.Colspan ?? 0) == 1) {
                        float hpadding = c.ComputedPadLeft + c.ComputedPadRight;
                        _colMinWidth[c.Column] = uniformMinWidth + hpadding;
                        _colPrefWidth[c.Column] = uniformPrefWidth + hpadding;
                    }

                    if (uniformPrefHeight > 0 && (c.UniformY ?? false)) {
                        float vpadding = c.ComputedPadTop + c.ComputedPadBottom;
                        _rowMinHeight[c.Row] = uniformMinHeight + vpadding;
                        _rowPrefHeight[c.Row] = uniformPrefHeight + vpadding;
                    }
                }
            }

            // Determine table min and pref size.
            _tableMinWidth = 0;
            _tableMinHeight = 0;
            _tablePrefWidth = 0;
            _tablePrefHeight = 0;

            for (int i = 0; i < _columns; i++) {
                _tableMinWidth += _colMinWidth[i];
                _tablePrefWidth += _colPrefWidth[i];
            }

            for (int i = 0; i < _rows; i++) {
                _tableMinHeight += _rowMinHeight[i];
                _tablePrefHeight += Math.Max(_rowMinHeight[i], _rowPrefHeight[i]);
            }

            float thpadding = W(PadLeftValue) + W(PadRightValue);
            float tvpadding = H(PadTopValue) + H(PadBottomValue);

            _tableMinWidth = _tableMinWidth + thpadding;
            _tableMinHeight = _tableMinHeight + tvpadding;
            _tablePrefWidth = Math.Max(_tablePrefWidth + thpadding, _tableMinWidth);
            _tablePrefHeight = Math.Max(_tablePrefHeight + tvpadding, _tableMinHeight);
        }

        public void Layout (float layoutX, float layoutY, float layoutWidth, float layoutHeight)
        {
            if (_sizeInvalid)
                ComputeSize();

            float padLeft = W(PadLeftValue);
            float hpadding = padLeft + W(PadRightValue);
            float padTop = H(PadTopValue);
            float vpadding = padTop + H(PadBottomValue);

            float totalExpandWidth = 0;
            float totalExpandHeight = 0;

            for (int i = 0; i < _columns; i++)
                totalExpandWidth += _expandWidth[i];
            for (int i = 0; i < _rows; i++)
                totalExpandHeight += _expandHeight[i];

            // Size columns and rows between min and pref size using (preferred - min) size to weight distribution of extra space
            float[] columnWeightedWidth;
            float totalGrowWidth = _tablePrefWidth - _tableMinWidth;

            if (totalGrowWidth == 0)
                columnWeightedWidth = _colMinWidth;
            else {
                float extraWidth = Math.Min(totalGrowWidth, Math.Max(0, layoutWidth - _tableMinWidth));
                _colWeightedWidth = EnsureSize(_colWeightedWidth, _columns);
                columnWeightedWidth = _colWeightedWidth;

                for (int i = 0; i < _columns; i++) {
                    float growWidth = _colPrefWidth[i] - _colMinWidth[i];
                    float growRatio = growWidth / totalGrowWidth;
                    columnWeightedWidth[i] = _colMinWidth[i] + extraWidth * growRatio;
                }
            }

            float[] rowWeightedHeight;
            float totalGrowHeight = _tablePrefHeight - _tableMinHeight;

            if (totalGrowHeight == 0)
                rowWeightedHeight = _rowMinHeight;
            else {
                _rowWeightedHeight = EnsureSize(_rowWeightedHeight, _rows);
                rowWeightedHeight = _rowWeightedHeight;
                float extraHeight = Math.Min(totalGrowHeight, Math.Max(0, layoutHeight - _tableMinHeight));

                for (int i = 0; i < _rows; i++) {
                    float growHeight = _rowPrefHeight[i] - _rowMinHeight[i];
                    float growRatio = growHeight / totalGrowHeight;
                    rowWeightedHeight[i] = _rowMinHeight[i] + extraHeight * growRatio;
                }
            }

            // Determine widget and cell sizes (before expand or fill)
            for (int i = 0, n = _cells.Count; i < n; i++) {
                Cell c = _cells[i];
                if (c.Ignore == true)
                    continue;

                float spannedWeightedWidth = 0;
                for (int column = c.Column, nn = column + (c.Colspan ?? 0); column < nn; column++)
                    spannedWeightedWidth += _colWeightedWidth[column];

                float weightedHeight = rowWeightedHeight[c.Row];

                float prefWidth = c.PrefWidthValue.Get(c);
                float prefHeight = c.PrefHeightValue.Get(c);
                float minWidth = c.MinWidthValue.Get(c);
                float minHeight = c.MinHeightValue.Get(c);
                float maxWidth = c.MaxWidthValue.Get(c);
                float maxHeight = c.MaxHeightValue.Get(c);

                if (prefWidth < minWidth)
                    prefWidth = minWidth;
                if (prefHeight < minHeight)
                    prefHeight = minHeight;

                if (maxWidth > 0 && prefWidth > maxWidth)
                    prefWidth = maxWidth;
                if (maxHeight > 0 && prefHeight > maxHeight)
                    prefHeight = maxHeight;

                c.WidgetWidth = Math.Min(spannedWeightedWidth - c.ComputedPadLeft - c.ComputedPadRight, prefWidth);
                c.WidgetHeight = Math.Min(weightedHeight - c.ComputedPadTop - c.ComputedPadBottom, prefHeight);

                if ((c.Colspan ?? 0) == 1)
                    _colWidth[c.Column] = Math.Max(_colWidth[c.Column], spannedWeightedWidth);
                _rowHeight[c.Row] = Math.Max(_rowHeight[c.Row], weightedHeight);
            }

            // Distribute remaining space to any expanding columns / rows.
            if (totalExpandWidth > 0) {
                float extra = layoutWidth - hpadding;
                for (int i = 0; i < _columns; i++)
                    extra -= _colWidth[i];

                float used = 0;
                int lastIndex = 0;
                for (int i = 0; i < _columns; i++) {
                    if (_expandWidth[i] == 0)
                        continue;

                    float amount = extra * _expandWidth[i] / totalExpandWidth;
                    _colWidth[i] += amount;
                    used += amount;
                    lastIndex = i;
                }

                _colWidth[lastIndex] += extra - used;
            }

            if (totalExpandHeight > 0) {
                float extra = layoutHeight - vpadding;
                for (int i = 0; i < _rows; i++)
                    extra -= _rowHeight[i];

                float used = 0;
                int lastIndex = 0;
                for (int i = 0; i < _rows; i++) {
                    if (_expandHeight[i] == 0)
                        continue;

                    float amount = extra * _expandHeight[i] / totalExpandHeight;
                    _rowHeight[i] += amount;
                    used += amount;
                    lastIndex = i;
                }

                _rowHeight[lastIndex] += extra - used;
            }

            // Determine table size.
            float tableWidth = hpadding;
            float tableHeight = vpadding;

            for (int i = 0; i < _columns; i++)
                tableWidth += _colWidth[i];
            for (int i = 0; i < _rows; i++)
                tableHeight += _rowHeight[i];

            // Position table within the container.
            float x = layoutX + padLeft;
            if ((Align & Alignment.Right) != Alignment.None)
                x += layoutWidth - tableWidth;
            else if ((Align & Alignment.Left) == Alignment.None)
                x += (layoutWidth - tableWidth) / 2;

            float y = layoutY + padTop;
            if ((Align & Alignment.Bottom) != Alignment.None)
                y += layoutHeight - tableHeight;
            else if ((Align & Alignment.Top) == Alignment.None)
                y += (layoutHeight - tableHeight) / 2;

            // Position widgets within cells.
            float currentX = x;
            float currentY = y;

            for (int i = 0, n = _cells.Count; i < n; i++) {
                Cell c = _cells[i];
                if (c.Ignore == true)
                    continue;

                float spannedCellWidth = 0;
                for (int column = c.Column, nn = column + (c.Colspan ?? 0); column < nn; column++)
                    spannedCellWidth += _colWidth[column];

                spannedCellWidth -= c.ComputedPadLeft + c.ComputedPadRight;

                currentX += c.ComputedPadLeft;

                if (c.FillX > 0) {
                    c.WidgetWidth = spannedCellWidth * (c.FillX ?? 0);
                    float maxWidth = c.MaxWidthValue.Get(c);

                    if (maxWidth > 0)
                        c.WidgetWidth = Math.Min(c.WidgetWidth, maxWidth);
                }
                if (c.FillY > 0) {
                    c.WidgetHeight = _rowHeight[c.Row] * (c.FillY ?? 0) - c.ComputedPadTop - c.ComputedPadBottom;
                    float maxHeight = c.MaxHeightValue.Get(c);

                    if (maxHeight > 0)
                        c.WidgetHeight = Math.Min(c.WidgetHeight, maxHeight);
                }

                if ((c.Align & Alignment.Left) != Alignment.None)
                    c.WidgetX = currentX;
                else if ((c.Align & Alignment.Right) != Alignment.None)
                    c.WidgetX = currentX + spannedCellWidth - c.WidgetWidth;
                else
                    c.WidgetX = currentX + (spannedCellWidth - c.WidgetWidth) / 2;

                if ((c.Align & Alignment.Top) != Alignment.None)
                    c.WidgetY = currentY + c.ComputedPadTop;
                else if ((c.Align & Alignment.Bottom) != Alignment.None)
                    c.WidgetY = currentY + _rowHeight[c.Row] - c.WidgetHeight - c.ComputedPadBottom;
                else
                    c.WidgetY = currentY + (_rowHeight[c.Row] - c.WidgetHeight + c.ComputedPadTop - c.ComputedPadBottom) / 2;

                if (c.IsEndRow) {
                    currentX = x;
                    currentY += _rowHeight[c.Row];
                }
                else
                    currentX += spannedCellWidth + c.ComputedPadRight;
            }

            // Draw debug widgets and bounds
            if (_debug == TableLayout.Debug.None)
                return;

            _toolkit.ClearDebugRectangles(this);

            currentX = x;
            currentY = y;

            if (_debug == TableLayout.Debug.Table || _debug == TableLayout.Debug.All) {
                _toolkit.AddDebugRectangle(this, TableLayout.Debug.Table, layoutX, layoutY, layoutWidth, layoutHeight);
                _toolkit.AddDebugRectangle(this, TableLayout.Debug.Table, x, y, tableWidth - hpadding, tableHeight - vpadding);
            }

            for (int i = 0, n = _cells.Count; i < n; i++) {
                Cell c = _cells[i];
                if (c.Ignore == true)
                    continue;

                // Widget bounds
                if (_debug == TableLayout.Debug.Widget || _debug == TableLayout.Debug.All)
                    _toolkit.AddDebugRectangle(this, TableLayout.Debug.Widget, c.WidgetX, c.WidgetY, c.WidgetWidth, c.WidgetHeight);

                // Cell bounds
                float spannedCellWidth = 0;
                for (int column = c.Column, nn = column + (c.Colspan ?? 0); column < nn; column++)
                    spannedCellWidth += _colWidth[column];

                spannedCellWidth -= c.ComputedPadLeft + c.ComputedPadRight;
                currentX += c.ComputedPadLeft;

                if (_debug == TableLayout.Debug.Cell || _debug == TableLayout.Debug.All)
                    _toolkit.AddDebugRectangle(this, TableLayout.Debug.Cell, currentX, currentY + c.ComputedPadTop, 
                        spannedCellWidth, _rowHeight[c.Row] - c.ComputedPadTop - c.ComputedPadBottom);

                if (c.IsEndRow) {
                    currentX = x;
                    currentY += _rowHeight[c.Row];
                }
                else
                    currentX += spannedCellWidth + c.ComputedPadRight;
            }
        }
    }
}
