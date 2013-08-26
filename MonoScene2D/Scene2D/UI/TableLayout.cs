using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoScene2D.Scene2D.Utils;
using MonoScene2D.TableLayout;
using TLToolkit = MonoScene2D.TableLayout.Toolkit;

namespace MonoScene2D.Scene2D.UI
{
    internal class TableLayout : BaseTableLayout<Actor, Table, TableLayout, TableToolkit>
    {
        bool _round;

        [TODO]
        public TableLayout ()
            : base(TLToolkit.Instance as TableToolkit)
        {
            DebugRects = new List<TableToolkit.DebugRect>();
        }

        internal List<TableToolkit.DebugRect> DebugRects { get; private set; }

        public void Layout ()
        {
            Table table = Table;
            float width = table.Width;
            float height = table.Height;

            List<Cell> cells = Cells;
            if (_round) {
                foreach (Cell c in cells) {
                    if (c.Ignore == true)
                        continue;

                    float widgetWidth = (float)Math.Round(c.WidgetWidth);
                    float widgetHeight = (float)Math.Round(c.WidgetHeight);
                    float widgetX = (float)Math.Round(c.WidgetX);
                    float widgetY = height - (float)Math.Round(c.WidgetY) - widgetHeight;

                    c.WidgetX = widgetX;
                    c.WidgetY = widgetY;
                    c.WidgetWidth = widgetWidth;
                    c.WidgetHeight = widgetHeight;

                    Actor actor = c.Widget as Actor;
                    if (actor != null) {
                        actor.X = widgetX;
                        actor.Y = widgetY;

                        if (actor.Width != widgetWidth || actor.Height != widgetHeight) {
                            actor.Width = widgetWidth;
                            actor.Height = widgetHeight;
                            if (actor is ILayout)
                                (actor as ILayout).Invalidate();
                        }
                    }
                }
            }
            else {
                foreach (Cell c in cells) {
                    if (c.Ignore == true)
                        continue;

                    float widgetWidth = c.WidgetWidth;
                    float widgetHeight = c.WidgetHeight;
                    float widgetX = c.WidgetX;
                    float widgetY = height - c.WidgetY - widgetHeight;

                    c.WidgetX = widgetX;
                    c.WidgetY = widgetY;
                    c.WidgetWidth = widgetWidth;
                    c.WidgetHeight = widgetHeight;

                    Actor actor = c.Widget as Actor;
                    if (actor != null) {
                        actor.X = widgetX;
                        actor.Y = widgetY;

                        if (actor.Width != widgetWidth || actor.Height != widgetHeight) {
                            actor.Width = widgetWidth;
                            actor.Height = widgetHeight;
                            if (actor is ILayout)
                                (actor as ILayout).Invalidate();
                        }
                    }
                }
            }

            // Validate children separately from sizing actors to ensure actors without a cell are validated.
            foreach (Actor child in table.Children) {
                if (child is ILayout)
                    (child as ILayout).Validate();
            }
        }

        public override void InvalidateHierarchy ()
        {
            base.Invalidate();
            Table.InvalidateHierarchy();
        }

        private Vector2 ToStageCoordinates (Actor actor, Vector2 point)
        {
            if (actor == null)
                return point;

            point.X += actor.X;
            point.Y += actor.Y;
            return ToStageCoordinates(actor.Parent, point);
        }

        [TODO]
        public void DrawDebug (SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }
    }
}
