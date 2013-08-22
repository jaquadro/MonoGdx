using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoScene2D.TableLayout
{
    public abstract class Cell
    {
        public Value MinWidthValue { get; set; }
        public Value MinHeightValue { get; set; }

        public float MinWidth
        {
            get { return MinWidthValue == null ? 0 : MinWidthValue.Width(this); }
            set { MinWidthValue = new FixedValue(value); }
        }

        public float MinHeight
        {
            get { return MinHeightValue == null ? 0 : MinHeightValue.Height(this); }
            set { MinHeightValue = new FixedValue(value); }
        }

        public Value PrefWidthValue { get; set; }
        public Value PrefHeightValue { get; set; }

        public float PrefWidth
        {
            get { return PrefWidthValue == null ? 0 : PrefWidthValue.Width(this); }
            set { PrefWidthValue = new FixedValue(value); }
        }

        public float PrefHeight
        {
            get { return PrefHeightValue == null ? 0 : PrefHeightValue.Height(this); }
            set { PrefHeightValue = new FixedValue(value); }
        }

        public Value MaxWidthValue { get; set; }
        public Value MaxHeightValue { get; set; }

        public float MaxWidth
        {
            get { return MaxWidthValue == null ? 0 : MaxWidthValue.Width(this); }
            set { MaxWidthValue = new FixedValue(value); }
        }

        public float MaxHeight
        {
            get { return MaxHeightValue == null ? 0 : MaxHeightValue.Height(this); }
            set { MaxHeightValue = new FixedValue(value); }
        }

        public float? FillX { get; set; }
        public float? FillY { get; set; }
        public int? ExpandX { get; internal set; }
        public int? ExpandY { get; internal set; }
        internal bool? Ignore { get; set; }
        public int? Colspan { get; internal set; }
        public bool? UniformX { get; internal set; }
        public bool? UniformY { get; internal set; }

        public float WidgetX { get; set; }
        public float WidgetY { get; set; }
        public float WidgetWidth { get; set; }
        public float WidgetHeight { get; set; }

        public bool IsEndRow { get; internal set; }
        public int Column { get; internal set; }
        public int Row { get; internal set; }
        internal int CellAboveIndex { get; set; }

        public float ComputedPadTop { get; internal set; }
        public float ComputedPadLeft { get; internal set; }
        public float ComputedPadBottom { get; internal set; }
        public float ComputedPadRight { get; internal set; }

        public Value SpaceTopValue { get; set; }
        public Value SpaceLeftValue { get; set; }
        public Value SpaceBottomValue { get; set; }
        public Value SpaceRightValue { get; set; }

        public float SpaceTop
        {
            get { return SpaceTopValue == null ? 0 : SpaceTopValue.Height(this); }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value cannot be < 0.");
                SpaceTopValue = new FixedValue(value);
            }
        }

        public float SpaceLeft
        {
            get { return SpaceLeftValue == null ? 0 : SpaceLeftValue.Width(this); }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value cannot be < 0.");
                SpaceLeftValue = new FixedValue(value);
            }
        }

        public float SpaceBottom
        {
            get { return SpaceBottomValue == null ? 0 : SpaceBottomValue.Height(this); }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value cannot be < 0.");
                SpaceBottomValue = new FixedValue(value);
            }
        }

        public float SpaceRight
        {
            get { return SpaceRightValue == null ? 0 : SpaceRightValue.Width(this); }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value cannot be < 0.");
                SpaceRightValue = new FixedValue(value);
            }
        }

        public Value PadTopValue { get; set; }
        public Value PadLeftValue { get; set; }
        public Value PadBottomValue { get; set; }
        public Value PadRightValue { get; set; }

        public float PadTop
        {
            get { return PadTopValue == null ? 0 : PadTopValue.Height(this); }
            set { PadTopValue = new FixedValue(value); }
        }

        public float PadLeft
        {
            get { return PadLeftValue == null ? 0 : PadLeftValue.Width(this); }
            set { PadLeftValue = new FixedValue(value); }
        }

        public float PadBottom
        {
            get { return PadBottomValue == null ? 0 : PadBottomValue.Height(this); }
            set { PadBottomValue = new FixedValue(value); }
        }

        public float PadRight
        {
            get { return PadRightValue == null ? 0 : PadRightValue.Width(this); }
            set { PadRightValue = new FixedValue(value); }
        }

        public Alignment? Align { get; set; }

        public object Widget
        {
            get { return WidgetCore; }
        }

        protected virtual object WidgetCore { get; }
    }

    public class Cell<T> : Cell
        where T : class
    {
        internal T _widget;
        
        private BaseTableLayout _layout;

        public BaseTableLayout Layout
        {
            get { return _layout; }
        }

        //public Cell<T> LayoutRow
        //{
        //    get { return _layout.Row; }
        //}

        public void Cell ()
        {
            CellAboveIndex = -1;
        }

        public void SetLayout (BaseTableLayout layout)
        {
            _layout = layout;
        }

        internal void Set (Cell defaults)
        {
            MinWidthValue = defaults.MinWidthValue;
            MinHeightValue = defaults.MinHeightValue;
            PrefWidthValue = defaults.PrefWidthValue;
            PrefHeightValue = defaults.PrefHeightValue;
            MaxWidthValue = defaults.MaxWidthValue;
            MaxHeightValue = defaults.MaxHeightValue;
            SpaceTopValue = defaults.SpaceTopValue;
            SpaceLeftValue = defaults.SpaceLeftValue;
            SpaceBottomValue = defaults.SpaceBottomValue;
            SpaceRightValue = defaults.SpaceRightValue;
            PadTopValue = defaults.PadTopValue;
            PadLeftValue = defaults.PadLeftValue;
            PadBottomValue = defaults.PadBottomValue;
            PadRightValue = defaults.PadRightValue;
            FillX = defaults.FillX;
            FillY = defaults.FillY;
            Align = defaults.Align;
            ExpandX = defaults.ExpandX;
            ExpandY = defaults.ExpandY;
            Ignore = defaults.Ignore;
            Colspan = defaults.Colspan;
            UniformX = defaults.UniformX;
            UniformY = defaults.UniformY;
        }

        internal void Merge (Cell cell)
        {
            if (cell == null)
                return;

            MinWidthValue = (cell.MinWidthValue != null) ? cell.MinWidthValue : MinWidthValue;
            MinHeightValue = (cell.MinHeightValue != null) ? cell.MinHeightValue : MinHeightValue;
            PrefWidthValue = (cell.PrefWidthValue != null) ? cell.PrefWidthValue : PrefWidthValue;
            PrefHeightValue = (cell.PrefHeightValue != null) ? cell.PrefHeightValue : PrefHeightValue;
            MaxWidthValue = (cell.MaxWidthValue != null) ? cell.MaxWidthValue : MaxWidthValue;
            MaxHeightValue = (cell.MaxHeightValue != null) ? cell.MaxHeightValue : MaxHeightValue;
            SpaceTopValue = (cell.SpaceTopValue != null) ? cell.SpaceTopValue : SpaceTopValue;
            SpaceLeftValue = (cell.SpaceLeftValue != null) ? cell.SpaceLeftValue : SpaceLeftValue;
            SpaceBottomValue = (cell.SpaceBottomValue != null) ? cell.SpaceBottomValue : SpaceBottomValue;
            SpaceRightValue = (cell.SpaceRightValue != null) ? cell.SpaceRightValue : SpaceRightValue;
            PadTopValue = (cell.PadTopValue != null) ? cell.PadTopValue : PadTopValue;
            PadLeftValue = (cell.PadLeftValue != null) ? cell.PadLeftValue : PadLeftValue;
            PadBottomValue = (cell.PadBottomValue != null) ? cell.PadBottomValue : PadBottomValue;
            PadRightValue = (cell.PadRightValue != null) ? cell.PadRightValue : PadRightValue;
            FillX = (cell.FillX != null) ? cell.FillX : FillX;
            FillY = (cell.FillY != null) ? cell.FillY : FillY;
            Align = (cell.Align != null) ? cell.Align : Align;
            ExpandX = (cell.ExpandX != null) ? cell.ExpandX : ExpandX;
            ExpandY = (cell.ExpandY != null) ? cell.ExpandY : ExpandY;
            Ignore = (cell.Ignore != null) ? cell.Ignore : Ignore;
            Colspan = (cell.Colspan != null) ? cell.Colspan : Colspan;
            UniformX = (cell.UniformX != null) ? cell.UniformX : UniformX;
            UniformY = (cell.UniformY != null) ? cell.UniformY : UniformY;
        }

        public new T Widget
        {
            get { return _widget; }
            set { }
        }

        protected override object WidgetCore
        {
            get { return _widget; }
        }

        public bool HasWidget
        {
            get { return _widget != null; }
        }
        
        public Cell<T> Size (Value size)
        {
            MinWidthValue = size;
            MinHeightValue = size;
            PrefWidthValue = size;
            PrefHeightValue = size;
            MaxWidthValue = size;
            MaxHeightValue = size;

            return this;
        }

        public Cell<T> Size (Value width, Value height)
        {
            MinWidthValue = width;
            MinHeightValue = height;
            PrefWidthValue = width;
            PrefHeightValue = height;
            MaxWidthValue = width;
            MaxHeightValue = height;

            return this;
        }

        public Cell<T> Size (float size)
        {
            Size(new FixedValue(size));

            return this;
        }

        public Cell<T> Width (Value width)
        {
            MinWidthValue = width;
            PrefWidthValue = width;
            MaxWidthValue = width;

            return this;
        }

        public Cell<T> Width (float width)
        {
            Width(new FixedValue(width));

            return this;
        }

        public Cell<T> Height (Value height)
        {
            MinHeightValue = height;
            PrefHeightValue = height;
            MaxHeightValue = height;

            return this;
        }

        public Cell<T> Height (float height)
        {
            Height(new FixedValue(height));

            return this;
        }

        public Cell<T> MinSize (Value size)
        {
            MinWidthValue = size;
            MinHeightValue = size;

            return this;
        }

        public Cell<T> MinSize (Value width, Value height)
        {
            MinWidthValue = width;
            MinHeightValue = height;

            return this;
        }

        public Cell<T> MinSize (float size)
        {
            MinWidthValue = new FixedValue(size);
            MinHeightValue = new FixedValue(size);

            return this;
        }

        public Cell<T> MinSize (float width, float height)
        {
            MinWidthValue = new FixedValue(width);
            MinHeightValue = new FixedValue(height);

            return this;
        }

        public Cell<T> PrefSize (Value size)
        {
            PrefWidthValue = size;
            PrefHeightValue = size;

            return this;
        }

        public Cell<T> PrefSize (Value width, Value height)
        {
            PrefWidthValue = width;
            PrefHeightValue = height;

            return this;
        }

        public Cell<T> PrefSize (float size)
        {
            PrefWidthValue = new FixedValue(size);
            PrefHeightValue = new FixedValue(size);

            return this;
        }

        public Cell<T> PrefSize (float width, float height)
        {
            PrefWidthValue = new FixedValue(width);
            PrefHeightValue = new FixedValue(height);

            return this;
        }

        public Cell<T> MaxSize (Value size)
        {
            MaxWidthValue = size;
            MaxHeightValue = size;

            return this;
        }

        public Cell<T> MaxSize (Value width, Value height)
        {
            MaxWidthValue = width;
            MaxHeightValue = height;

            return this;
        }

        public Cell<T> MaxSize (float size)
        {
            MaxWidthValue = new FixedValue(size);
            MaxHeightValue = new FixedValue(size);

            return this;
        }

        public Cell<T> MaxSize (float width, float height)
        {
            MaxWidthValue = new FixedValue(width);
            MaxHeightValue = new FixedValue(height);

            return this;
        }

        public Cell<T> Space (Value space)
        {
            SpaceTopValue = space;
            SpaceLeftValue = space;
            SpaceBottomValue = space;
            SpaceRightValue = space;

            return this;
        }

        public Cell<T> Space (Value top, Value left, Value bottom, Value right)
        {
            SpaceTopValue = top;
            SpaceLeftValue = left;
            SpaceBottomValue = bottom;
            SpaceRightValue = right;

            return this;
        }

        public Cell<T> Space (float space)
        {
            if (space < 0)
                throw new ArgumentException("space cannot be < 0.");

            SpaceTop = space;
            SpaceLeft = space;
            SpaceBottom = space;
            SpaceRight = space;

            return this;
        }

        public Cell<T> Space (float top, float left, float bottom, float right)
        {
            if (top < 0)
                throw new ArgumentException("top cannot be < 0.");
            if (left < 0)
                throw new ArgumentException("left cannot be < 0.");
            if (bottom < 0)
                throw new ArgumentException("bottom cannot be < 0.");
            if (right < 0)
                throw new ArgumentException("right cannot be < 0.");

            SpaceTop = top;
            SpaceLeft = left;
            SpaceBottom = bottom;
            SpaceRight = right;

            return this;
        }

        public Cell<T> Pad (Value pad)
        {
            PadTopValue = pad;
            PadLeftValue = pad;
            PadBottomValue = pad;
            PadRightValue = pad;

            return this;
        }

        public Cell<T> Pad (Value top, Value left, Value bottom, Value right)
        {
            PadTopValue = top;
            PadLeftValue = left;
            PadBottomValue = bottom;
            PadRightValue = right;

            return this;
        }

        public Cell<T> Pad (float pad)
        {
            PadTop = pad;
            PadLeft = pad;
            PadBottom = pad;
            PadRight = pad;

            return this;
        }

        public Cell<T> Pad (float top, float left, float bottom, float right)
        {
            PadTop = top;
            PadLeft = left;
            PadBottom = bottom;
            PadRight = right;

            return this;
        }

        public Cell<T> Fill ()
        {
            FillX = 1;
            FillY = 1;

            return this;
        }

        public Cell<T> Fill (float x, float y)
        {
            FillX = x;
            FillY = y;

            return this;
        }

        public Cell<T> Fill (bool fill)
        {
            FillX = fill ? 1 : 0;
            FillY = fill ? 1 : 0;

            return this;
        }

        public Cell<T> Fill (bool x, bool y)
        {
            FillX = x ? 1 : 0;
            FillY = y ? 1 : 0;

            return this;
        }

        public Cell<T> Center ()
        {
            Align = Alignment.Center;

            return this;
        }

        public Cell<T> Top ()
        {
            if (Align == null)
                Align = Alignment.Top;
            else {
                Align |= Alignment.Top;
                Align &= ~Alignment.Bottom;
            }

            return this;
        }

        public Cell<T> Left ()
        {
            if (Align == null)
                Align = Alignment.Left;
            else {
                Align |= Alignment.Left;
                Align &= ~Alignment.Right;
            }

            return this;
        }

        public Cell<T> Bottom ()
        {
            if (Align == null)
                Align = Alignment.Bottom;
            else {
                Align |= Alignment.Bottom;
                Align &= ~Alignment.Top;
            }

            return this;
        }

        public Cell<T> Right ()
        {
            if (Align == null)
                Align = Alignment.Right;
            else {
                Align |= Alignment.Right;
                Align &= ~Alignment.Left;
            }

            return this;
        }

        public Cell<T> Expand ()
        {
            ExpandX = 1;
            ExpandY = 1;

            return this;
        }

        public Cell<T> Expand (int? x, int? y)
        {
            ExpandX = x;
            ExpandY = y;

            return this;
        }

        public Cell<T> Expand (bool x, bool y)
        {
            ExpandX = x ? 1 : 0;
            ExpandY = y ? 1 : 0;

            return this;
        }

        public Cell<T> Uniform ()
        {
            UniformX = true;
            UniformY = true;

            return this;
        }

        public Cell<T> Uniform (bool x, bool y)
        {
            UniformX = x;
            UniformY = y;

            return this;
        }

        public void Clear ()
        {
            MinWidthValue = null;
            MinHeightValue = null;
            PrefWidthValue = null;
            PrefHeightValue = null;
            MaxWidthValue = null;
            MaxHeightValue = null;
            SpaceTopValue = null;
            SpaceLeftValue = null;
            SpaceBottomValue = null;
            SpaceRightValue = null;
            PadTopValue = null;
            PadLeftValue = null;
            PadBottomValue = null;
            PadRightValue = null;
            FillX = null;
            FillY = null;
            Align = null;
            ExpandX = null;
            ExpandY = null;
            Ignore = null;
            Colspan = null;
            UniformX = null;
            UniformY = null;
        }

        public void Free ()
        {
            Widget = null;
            _layout = null;
            IsEndRow = false;
            CellAboveIndex = -1;
        }

        internal void Defaults ()
        {
            MinWidthValue = Value.MinWidth;
            MinHeightValue = Value.MinHeight;
            PrefWidthValue = Value.PrefWidth;
            PrefHeightValue = Value.PrefHeight;
            MaxWidthValue = Value.MaxWidth;
            MaxHeightValue = Value.MaxHeight;
            SpaceTopValue = Value.Zero;
            SpaceLeftValue = Value.Zero;
            SpaceBottomValue = Value.Zero;
            SpaceRightValue = Value.Zero;
            PadTopValue = Value.Zero;
            PadLeftValue = Value.Zero;
            PadBottomValue = Value.Zero;
            PadRightValue = Value.Zero;
            FillX = 0;
            FillY = 0;
            Align = Alignment.Center;
            ExpandX = 0;
            ExpandY = 0;
            Ignore = false;
            Colspan = 1;
            UniformX = null;
            UniformY = null;
        }
    }
}
