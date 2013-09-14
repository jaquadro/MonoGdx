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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGdx.Geometry;
using MonoGdx.Scene2D.Utils;
using MonoGdx.TableLayout;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.UI
{
    public class TableToolkit : Toolkit<Actor, Table, TableLayout>
    {
        internal bool DrawDebug { get; set; }

        public override Cell<Actor> ObtainCell (TableLayout layout)
        {
            Cell<Actor> cell = Pools<Cell<Actor>>.Obtain();
            cell.Layout = layout;
            return cell;
        }

        public override void FreeCell (Cell<Actor> cell)
        {
            cell.Free();
            Pools<Cell<Actor>>.Release(cell);
        }

        public override void AddChild (Actor parent, Actor child)
        {
            child.Remove();
            (parent as Group).AddActor(child);
        }

        public override void RemoveChild (Actor parent, Actor child)
        {
            (parent as Group).RemoveActor(child);
        }

        public override float MinWidth (Actor widget)
        {
            if (widget is ILayout)
                return (widget as ILayout).MinWidth;
            return widget.Width;
        }

        public override float MinHeight (Actor widget)
        {
            if (widget is ILayout)
                return (widget as ILayout).MinHeight;
            return widget.Height;
        }

        public override float PrefWidth (Actor widget)
        {
            if (widget is ILayout)
                return (widget as ILayout).PrefWidth;
            return widget.Width;
        }

        public override float PrefHeight (Actor widget)
        {
            if (widget is ILayout)
                return (widget as ILayout).PrefHeight;
            return widget.Height;
        }

        public override float MaxWidth (Actor widget)
        {
            if (widget is ILayout)
                return (widget as ILayout).MaxWidth;
            return widget.Width;
        }

        public override float MaxHeight (Actor widget)
        {
            if (widget is ILayout)
                return (widget as ILayout).MaxHeight;
            return widget.Height;
        }

        public override float Width (Actor widget)
        {
            return widget.Width;
        }

        public override float Height (Actor widget)
        {
            return widget.Height;
        }

        public override void ClearDebugRectangles (TableLayout layout)
        {
            if (layout.DebugRects != null)
                layout.DebugRects.Clear();
        }

        public override void AddDebugRectangle (TableLayout layout, Debug type, float x, float y, float w, float h)
        {
            DrawDebug = true;
            layout.DebugRects.Add(new DebugRect(type, x, layout.Table.Height - y, w, h));
        }

        internal class DebugRect
        {
            public Debug Type { get; set; }
            public RectangleF Rect { get; set; }

            public DebugRect (Debug debug, RectangleF rect)
            {
                Type = debug;
                Rect = rect;
            }

            public DebugRect (Debug debug, float x, float y, float width, float height)
            {
                Type = debug;
                Rect = new RectangleF(x, y, width, height);
            }
        }
    }
}
