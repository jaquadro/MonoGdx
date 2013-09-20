/**
 * Copyright (c) 2011, Nathan Sweet <nathan.sweet@gmail.com>
 * All rights reserved.
 * 
 * Copyright (c) 2013, Justin Aquadro <jaquadro@gmail.com>
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the <organization> nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGdx.TableLayout
{
    public abstract class Cell
    {
        protected Cell ()
        {
            CellAboveIndex = -1;
            Configure = new Configurer(this);
        }

        public Configurer Configure { get; protected set; }

        public BaseTableLayout Layout { get; set; }

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
        public int? ExpandX { get; set; }
        public int? ExpandY { get; set; }
        internal bool? Ignore { get; set; }
        public int? Colspan { get; set; }
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

        public Cell LayoutRow
        {
            get { return Layout.Row(); }
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

        public object Widget
        {
            get { return WidgetCore; }
        }

        protected abstract object WidgetCore { get; }

        public abstract void ClearWidget ();

        public class Configurer
        {
            private Cell _cell;

            public Configurer (Cell cell)
            {
                _cell = cell;
            }

            public Configurer Bottom ()
            {
                if (_cell.Align == null)
                    _cell.Align = Alignment.Bottom;
                else {
                    _cell.Align |= Alignment.Bottom;
                    _cell.Align &= ~Alignment.Top;
                }
                return this;
            }

            public Configurer Center ()
            {
                _cell.Align = Alignment.Center;
                return this;
            }

            public Configurer Colspan (int? colspan)
            {
                _cell.Colspan = colspan;
                return this;
            }

            public Configurer Expand ()
            {
                _cell.ExpandX = 1;
                _cell.ExpandY = 1;
                return this;
            }

            public Configurer Expand (int? x, int? y)
            {
                _cell.ExpandX = x;
                _cell.ExpandY = y;
                return this;
            }

            public Configurer Expand (bool x, bool y)
            {
                _cell.ExpandX = x ? 1 : 0;
                _cell.ExpandY = y ? 1 : 0;
                return this;
            }

            public Configurer ExpandX ()
            {
                _cell.ExpandX = 1;
                return this;
            }

            public Configurer ExpandY ()
            {
                _cell.ExpandY = 1;
                return this;
            }

            public Configurer Fill ()
            {
                _cell.FillX = 1;
                _cell.FillY = 1;
                return this;
            }

            public Configurer Fill (float x, float y)
            {
                _cell.FillX = x;
                _cell.FillY = y;
                return this;
            }

            public Configurer Fill (bool fill)
            {
                _cell.FillX = fill ? 1 : 0;
                _cell.FillY = fill ? 1 : 0;
                return this;
            }

            public Configurer Fill (bool x, bool y)
            {
                _cell.FillX = x ? 1 : 0;
                _cell.FillY = y ? 1 : 0;
                return this;
            }

            public Configurer FillX ()
            {
                _cell.FillX = 1;
                return this;
            }

            public Configurer FillY ()
            {
                _cell.FillY = 1;
                return this;
            }

            public Configurer Height (Value height)
            {
                _cell.MinHeightValue = height;
                _cell.PrefHeightValue = height;
                _cell.MaxHeightValue = height;
                return this;
            }

            public Configurer Height (float height)
            {
                Height(new FixedValue(height));
                return this;
            }

            public Configurer Ignore ()
            {
                _cell.Ignore = true;
                return this;
            }

            public Configurer Ignore (bool ignore)
            {
                _cell.Ignore = ignore;
                return this;
            }

            public Configurer Left ()
            {
                if (_cell.Align == null)
                    _cell.Align = Alignment.Left;
                else {
                    _cell.Align |= Alignment.Left;
                    _cell.Align &= ~Alignment.Right;
                }
                return this;
            }

            public Configurer MaxHeight (Value height)
            {
                _cell.MaxHeightValue = height;
                return this;
            }

            public Configurer MaxHeight (float height)
            {
                _cell.MaxHeight = height;
                return this;
            }

            public Configurer MaxSize (Value size)
            {
                _cell.MaxWidthValue = size;
                _cell.MaxHeightValue = size;
                return this;
            }

            public Configurer MaxSize (Value width, Value height)
            {
                _cell.MaxWidthValue = width;
                _cell.MaxHeightValue = height;
                return this;
            }

            public Configurer MaxSize (float size)
            {
                _cell.MaxWidthValue = new FixedValue(size);
                _cell.MaxHeightValue = new FixedValue(size);
                return this;
            }

            public Configurer MaxSize (float width, float height)
            {
                _cell.MaxWidthValue = new FixedValue(width);
                _cell.MaxHeightValue = new FixedValue(height);
                return this;
            }

            public Configurer MaxWidth (Value width)
            {
                _cell.MaxWidthValue = width;
                return this;
            }

            public Configurer MaxWidth (float width)
            {
                _cell.MaxWidth = width;
                return this;
            }

            public Configurer MinHeight (Value height)
            {
                _cell.MinHeightValue = height;
                return this;
            }

            public Configurer MinHeight (float height)
            {
                _cell.MinHeight = height;
                return this;
            }

            public Configurer MinSize (Value size)
            {
                _cell.MinWidthValue = size;
                _cell.MinHeightValue = size;
                return this;
            }

            public Configurer MinSize (Value width, Value height)
            {
                _cell.MinWidthValue = width;
                _cell.MinHeightValue = height;
                return this;
            }

            public Configurer MinSize (float size)
            {
                _cell.MinWidthValue = new FixedValue(size);
                _cell.MinHeightValue = new FixedValue(size);
                return this;
            }

            public Configurer MinSize (float width, float height)
            {
                _cell.MinWidthValue = new FixedValue(width);
                _cell.MinHeightValue = new FixedValue(height);
                return this;
            }

            public Configurer MinWidth (Value width)
            {
                _cell.MinWidthValue = width;
                return this;
            }

            public Configurer MinWidth (float width)
            {
                _cell.MinWidth = width;
                return this;
            }

            public Configurer Pad (Value pad)
            {
                _cell.PadTopValue = pad;
                _cell.PadLeftValue = pad;
                _cell.PadBottomValue = pad;
                _cell.PadRightValue = pad;
                return this;
            }

            public Configurer Pad (Value top, Value left, Value bottom, Value right)
            {
                _cell.PadTopValue = top;
                _cell.PadLeftValue = left;
                _cell.PadBottomValue = bottom;
                _cell.PadRightValue = right;
                return this;
            }

            public Configurer Pad (float pad)
            {
                _cell.PadTop = pad;
                _cell.PadLeft = pad;
                _cell.PadBottom = pad;
                _cell.PadRight = pad;
                return this;
            }

            public Configurer Pad (float top, float left, float bottom, float right)
            {
                _cell.PadTop = top;
                _cell.PadLeft = left;
                _cell.PadBottom = bottom;
                _cell.PadRight = right;
                return this;
            }

            public Configurer PadBottom (Value bottom)
            {
                _cell.PadBottomValue = bottom;
                return this;
            }

            public Configurer PadBottom (float bottom)
            {
                _cell.PadBottom = bottom;
                return this;
            }

            public Configurer PadLeft (Value left)
            {
                _cell.PadLeftValue = left;
                return this;
            }

            public Configurer PadLeft (float left)
            {
                _cell.PadLeft = left;
                return this;
            }

            public Configurer PadRight (Value right)
            {
                _cell.PadRightValue = right;
                return this;
            }

            public Configurer PadRight (float right)
            {
                _cell.PadRight = right;
                return this;
            }

            public Configurer PadTop (Value top)
            {
                _cell.PadTopValue = top;
                return this;
            }

            public Configurer PadTop (float top)
            {
                _cell.PadTop = top;
                return this;
            }

            public Configurer PrefHeight (Value height)
            {
                _cell.PrefHeightValue = height;
                return this;
            }

            public Configurer PrefHeight (float height)
            {
                _cell.PrefHeight = height;
                return this;
            }

            public Configurer PrefSize (Value size)
            {
                _cell.PrefWidthValue = size;
                _cell.PrefHeightValue = size;
                return this;
            }

            public Configurer PrefSize (Value width, Value height)
            {
                _cell.PrefWidthValue = width;
                _cell.PrefHeightValue = height;
                return this;
            }

            public Configurer PrefSize (float size)
            {
                _cell.PrefWidthValue = new FixedValue(size);
                _cell.PrefHeightValue = new FixedValue(size);
                return this;
            }

            public Configurer PrefSize (float width, float height)
            {
                _cell.PrefWidthValue = new FixedValue(width);
                _cell.PrefHeightValue = new FixedValue(height);
                return this;
            }

            public Configurer PrefWidth (Value width)
            {
                _cell.PrefWidthValue = width;
                return this;
            }

            public Configurer PrefWidth (float width)
            {
                _cell.PrefWidth = width;
                return this;
            }

            public Configurer Right ()
            {
                if (_cell.Align == null)
                    _cell.Align = Alignment.Right;
                else {
                    _cell.Align |= Alignment.Right;
                    _cell.Align &= ~Alignment.Left;
                }
                return this;
            }

            public Configurer Size (Value size)
            {
                _cell.MinWidthValue = size;
                _cell.MinHeightValue = size;
                _cell.PrefWidthValue = size;
                _cell.PrefHeightValue = size;
                _cell.MaxWidthValue = size;
                _cell.MaxHeightValue = size;
                return this;
            }

            public Configurer Size (Value width, Value height)
            {
                _cell.MinWidthValue = width;
                _cell.MinHeightValue = height;
                _cell.PrefWidthValue = width;
                _cell.PrefHeightValue = height;
                _cell.MaxWidthValue = width;
                _cell.MaxHeightValue = height;
                return this;
            }

            public Configurer Size (float size)
            {
                Size(new FixedValue(size));
                return this;
            }

            public Configurer Space (Value space)
            {
                _cell.SpaceTopValue = space;
                _cell.SpaceLeftValue = space;
                _cell.SpaceBottomValue = space;
                _cell.SpaceRightValue = space;
                return this;
            }

            public Configurer Space (Value top, Value left, Value bottom, Value right)
            {
                _cell.SpaceTopValue = top;
                _cell.SpaceLeftValue = left;
                _cell.SpaceBottomValue = bottom;
                _cell.SpaceRightValue = right;
                return this;
            }

            public Configurer Space (float space)
            {
                if (space < 0)
                    throw new ArgumentException("space cannot be < 0.");

                _cell.SpaceTop = space;
                _cell.SpaceLeft = space;
                _cell.SpaceBottom = space;
                _cell.SpaceRight = space;
                return this;
            }

            public Configurer Space (float top, float left, float bottom, float right)
            {
                if (top < 0)
                    throw new ArgumentException("top cannot be < 0.");
                if (left < 0)
                    throw new ArgumentException("left cannot be < 0.");
                if (bottom < 0)
                    throw new ArgumentException("bottom cannot be < 0.");
                if (right < 0)
                    throw new ArgumentException("right cannot be < 0.");

                _cell.SpaceTop = top;
                _cell.SpaceLeft = left;
                _cell.SpaceBottom = bottom;
                _cell.SpaceRight = right;
                return this;
            }

            public Configurer SpaceBottom (Value bottom)
            {
                _cell.SpaceBottomValue = bottom;
                return this;
            }

            public Configurer SpaceBottom (float bottom)
            {
                if (bottom < 0)
                    throw new ArgumentException("bottom cannot be < 0.");

                _cell.SpaceBottom = bottom;
                return this;
            }

            public Configurer SpaceLeft (Value left)
            {
                _cell.SpaceLeftValue = left;
                return this;
            }

            public Configurer SpaceLeft (float left)
            {
                if (left < 0)
                    throw new ArgumentException("left cannot be < 0.");

                _cell.SpaceLeft = left;
                return this;
            }

            public Configurer SpaceRight (Value right)
            {
                _cell.SpaceRightValue = right;
                return this;
            }

            public Configurer SpaceRight (float right)
            {
                if (right < 0)
                    throw new ArgumentException("right cannot be < 0.");

                _cell.SpaceRight = right;
                return this;
            }

            public Configurer SpaceTop (Value top)
            {
                _cell.SpaceTopValue = top;
                return this;
            }

            public Configurer SpaceTop (float top)
            {
                if (top < 0)
                    throw new ArgumentException("top cannot be < 0.");

                _cell.SpaceTop = top;
                return this;
            }

            public Configurer Top ()
            {
                if (_cell.Align == null)
                    _cell.Align = Alignment.Top;
                else {
                    _cell.Align |= Alignment.Top;
                    _cell.Align &= ~Alignment.Bottom;
                }
                return this;
            }

            public Configurer Uniform ()
            {
                _cell.UniformX = true;
                _cell.UniformY = true;
                return this;
            }

            public Configurer Uniform (bool x, bool y)
            {
                _cell.UniformX = x;
                _cell.UniformY = y;
                return this;
            }

            public Configurer UniformX ()
            {
                _cell.UniformX = true;
                return this;
            }

            public Configurer UniformY ()
            {
                _cell.UniformY = true;
                return this;
            }

            public Configurer Width (Value width)
            {
                _cell.MinWidthValue = width;
                _cell.PrefWidthValue = width;
                _cell.MaxWidthValue = width;
                return this;
            }

            public Configurer Width (float width)
            {
                Width(new FixedValue(width));
                return this;
            }
        }
    }

    public class Cell<T> : Cell
        where T : class
    {
        public new T Widget { get; internal set; }

        public void SetWidget (T widget)
        {
            Layout.Toolkit.SetWidget(Layout, this, Widget);
        }

        protected override object WidgetCore
        {
            get { return Widget; }
        }

        public override void ClearWidget ()
        {
            Widget = null;
        }

        public bool HasWidget
        {
            get { return Widget != null; }
        }

        public void Free ()
        {
            Widget = null;
            Layout = null;
            IsEndRow = false;
            CellAboveIndex = -1;
        }
    }
}
