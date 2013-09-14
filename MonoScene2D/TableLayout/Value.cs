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
    abstract public class Value
    {
        public abstract float Get (object table);

        public abstract float Get (Cell cell);

        public float Width (object table)
        {
            return Toolkit.Instance.Width(Get(table));
        }

        public float Width (Cell cell)
        {
            return Toolkit.Instance.Width(Get(cell));
        }

        public float Height (object table)
        {
            return Toolkit.Instance.Height(Get(table));
        }

        public float Height (Cell cell)
        {
            return Toolkit.Instance.Height(Get(cell));
        }

        public static readonly Value Zero = new ZeroValue();

        public static Value MinWidth = new MinWidthValue();
        public static Value MinHeight = new MinHeightValue();
        public static Value PrefWidth = new PrefWidthValue();
        public static Value PrefHeight = new PrefHeightValue();
        public static Value MaxWidth = new MaxWidthValue();
        public static Value MaxHeight = new MaxHeightValue();

        public static Value PercentWidth (float percent)
        {
            return new PercentWidthValue(percent);
        }

        public static Value PercentHeight (float percent)
        {
            return new PercentHeightValue(percent);
        }

        public static Value PercentWidth (float percent, object widget)
        {
            return new PercentWidthWidgetValue(percent, widget);
        }

        public static Value PercentHeight (float percent, object widget)
        {
            return new PercentHeightWidgetValue(percent, widget);
        }

        private sealed class ZeroValue : CellValue
        {
            public override float Get (Cell cell)
            {
                return 0;
            }

            public override float Get (object table)
            {
                return 0;
            }
        }

        private sealed class MinWidthValue : CellValue
        {
            public override float Get (Cell cell)
            {
                if (cell == null)
                    throw new ArgumentNullException("Cell property not set.");

                object widget = cell.Widget;
                if (widget == null)
                    return 0;

                return Toolkit.Instance.MinWidth(widget);
            }
        }

        private sealed class MinHeightValue : CellValue
        {
            public override float Get (Cell cell)
            {
                if (cell == null)
                    throw new ArgumentNullException("Cell property not set.");

                object widget = cell.Widget;
                if (widget == null)
                    return 0;

                return Toolkit.Instance.MinHeight(widget);
            }
        }

        private sealed class PrefWidthValue : CellValue
        {
            public override float Get (Cell cell)
            {
                if (cell == null)
                    throw new ArgumentNullException("Cell property not set.");

                object widget = cell.Widget;
                if (widget == null)
                    return 0;

                return Toolkit.Instance.PrefWidth(widget);
            }
        }

        private sealed class PrefHeightValue : CellValue
        {
            public override float Get (Cell cell)
            {
                if (cell == null)
                    throw new ArgumentNullException("Cell property not set.");

                object widget = cell.Widget;
                if (widget == null)
                    return 0;

                return Toolkit.Instance.PrefHeight(widget);
            }
        }

        private sealed class MaxWidthValue : CellValue
        {
            public override float Get (Cell cell)
            {
                if (cell == null)
                    throw new ArgumentNullException("Cell property not set.");

                object widget = cell.Widget;
                if (widget == null)
                    return 0;

                return Toolkit.Instance.MaxWidth(widget);
            }
        }

        private sealed class MaxHeightValue : CellValue
        {
            public override float Get (Cell cell)
            {
                if (cell == null)
                    throw new ArgumentNullException("Cell property not set.");

                object widget = cell.Widget;
                if (widget == null)
                    return 0;

                return Toolkit.Instance.MaxHeight(widget);
            }
        }

        private sealed class PercentWidthValue : TableValue
        {
            private float _percent;

            public PercentWidthValue (float percent)
            {
                _percent = percent;
            }

            public override float Get(object table)
            {
 	            return Toolkit.Instance.Width(table) * _percent;
            }
        }

        private sealed class PercentHeightValue : TableValue
        {
            private float _percent;

            public PercentHeightValue (float percent)
            {
                _percent = percent;
            }

            public override float Get(object table)
            {
 	            return Toolkit.Instance.Height(table) * _percent;
            }
        }

        private sealed class PercentWidthWidgetValue : Value
        {
            private float _percent;
            private object _widget;

            public PercentWidthWidgetValue (float percent, object widget)
            {
                _percent = percent;
                _widget = widget;
            }

            public override float Get (object table)
            {
                return Toolkit.Instance.Width(_widget) * _percent;
            }

            public override float Get (Cell cell)
            {
                return Toolkit.Instance.Width(_widget) * _percent;
            }
        }

        private sealed class PercentHeightWidgetValue : Value
        {
            private float _percent;
            private object _widget;

            public PercentHeightWidgetValue (float percent, object widget)
            {
                _percent = percent;
                _widget = widget;
            }

            public override float Get (object table)
            {
                return Toolkit.Instance.Height(_widget) * _percent;
            }

            public override float Get (Cell cell)
            {
                return Toolkit.Instance.Height(_widget) * _percent;
            }
        }
    }

    public abstract class CellValue : Value
    {
        public override float Get (object table)
        {
            throw new InvalidOperationException("This value can only be used for a cell property.");
        }
    }

    public abstract class TableValue : Value
    {
        public override float Get (Cell cell)
        {
            return Get(cell.Layout.Table);
        }
    }

    public class FixedValue : Value
    {
        private float _value;

        public FixedValue (float value)
        {
            _value = value;
        }

        public void Set (float value)
        {
            _value = value;
        }

        public override float Get (object table)
        {
            return _value;
        }

        public override float Get (Cell cell)
        {
            return _value;
        }
    }
}
