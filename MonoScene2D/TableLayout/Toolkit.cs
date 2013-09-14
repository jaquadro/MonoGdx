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
    public abstract class Toolkit
    {
        public static Toolkit Instance;

        public virtual float Width (float value)
        {
            return value;
        }

        public virtual float Height (float value)
        {
            return value;
        }

        public abstract void FreeCell (Cell cell);

        public abstract float MinWidth (object widget);

        public abstract float MinHeight (object widget);

        public abstract float PrefWidth (object widget);

        public abstract float PrefHeight (object widget);

        public abstract float MaxWidth (object widget);

        public abstract float MaxHeight (object widget);

        public abstract float Width (object widget);

        public abstract float Height (object widget);

        public void SetWidget (object layout, Cell cell, object widget)
        {
            SetWidgetCore(layout, cell, widget);
        }

        protected abstract void SetWidgetCore (object layout, Cell cell, object widget);
    }

    public abstract class Toolkit<T, TTable, TLayout> : Toolkit
        where T : class
        where TTable : T
        where TLayout : BaseTableLayout 
    {
        public abstract Cell<T> ObtainCell (TLayout layout);

        public abstract void FreeCell (Cell<T> cell);

        public override void FreeCell (Cell cell)
        {
            FreeCell((Cell<T>)cell);
        }

        public abstract void AddChild (T parent, T child);

        public abstract void RemoveChild (T parent, T child);

        public abstract float MinWidth (T widget);

        public abstract float MinHeight (T widget);

        public abstract float PrefWidth (T widget);

        public abstract float PrefHeight (T widget);

        public abstract float MaxWidth (T widget);

        public abstract float MaxHeight (T widget);

        public abstract float Width (T widget);

        public abstract float Height (T widget);

        public override float MinWidth (object widget)
        {
            return MinWidth((T)widget);
        }

        public override float MinHeight (object widget)
        {
            return MinHeight((T)widget);
        }

        public override float PrefWidth (object widget)
        {
            return PrefWidth((T)widget);
        }

        public override float PrefHeight (object widget)
        {
            return PrefHeight((T)widget);
        }

        public override float MaxWidth (object widget)
        {
            return MaxWidth((T)widget);
        }

        public override float MaxHeight (object widget)
        {
            return MaxHeight((T)widget);
        }

        public override float Width (object widget)
        {
            return Width((T)widget);
        }

        public override float Height (object widget)
        {
            return Height((T)widget);
        }

        public abstract void ClearDebugRectangles (TLayout layout);

        public abstract void AddDebugRectangle (TLayout layout, Debug type, float x, float y, float w, float h);

        public void SetWidget (TLayout layout, Cell<T> cell, T widget)
        {
            if (cell.Widget == widget)
                return;

            RemoveChild((TTable)layout.Table, cell.Widget);
            cell.Widget = widget;

            if (widget != null)
                AddChild((TTable)layout.Table, widget);
        }

        protected override void SetWidgetCore (object layout, Cell cell, object widget)
        {
            SetWidget((TLayout)layout, (Cell<T>)cell, (T)widget);
        }
    }
}
