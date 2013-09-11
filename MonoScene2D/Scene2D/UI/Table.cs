using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using MonoGdx.TableLayout;

namespace MonoGdx.Scene2D.UI
{
    public class Table : WidgetGroup
    {
        static Table ()
        {
            if (Toolkit.Instance == null)
                Toolkit.Instance = new TableToolkit();
        }

        
        private readonly TableLayout _layout;
        private bool _clip;

        public Table ()
            : this(null)
        { }

        public Table (Skin skin)
        {
            Skin = skin;
            _layout = new TableLayout();
            _layout.Table = this;

            IsTransform = false;
            Touchable = Touchable.ChildrenOnly;
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            Validate();
            DrawBackground(spriteBatch, parentAlpha);

            if (IsTransform) {
                ApplyTransform(spriteBatch, ComputeTransform());
                if (_clip) {
                    bool draw = Background == null
                        ? ClipBegin(0, 0, Width, Height)
                        : ClipBegin(_layout.PadLeft, _layout.PadBottom, Width - _layout.PadLeft - _layout.PadRight, Height - _layout.PadBottom - _layout.PadTop);

                    if (draw) {
                        DrawChildren(spriteBatch, parentAlpha);
                        ClipEnd();
                    }
                }
                else
                    DrawChildren(spriteBatch, parentAlpha);
            }
            else
                base.Draw(spriteBatch, parentAlpha);
        }

        protected virtual void DrawBackground (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            if (Background != null) {
                Color c = Color;
                spriteBatch.Color = new Color(c.R, c.G, c.B, c.A * parentAlpha);
                Background.Draw(spriteBatch, X, Y, Width, Height);
            }
        }

        public override void Invalidate ()
        {
            _layout.Invalidate();
            base.Invalidate();
        }

        public override float PrefWidth
        {
            get { return (Background != null) ? Math.Max(_layout.PrefWidth, Background.MinWidth) : _layout.PrefWidth; }
        }

        public override float PrefHeight
        {
            get { return (Background != null) ? Math.Max(_layout.PrefHeight, Background.MinHeight) : _layout.PrefHeight; }
        }

        public override float MinWidth
        {
            get { return _layout.MinWidth; }
        }

        public override float MinHeight
        {
            get { return _layout.MinHeight; }
        }

        public void SetBackground (string drawableName)
        {
            SetBackground(Skin.GetDrawable(drawableName));
        }

        public void SetBackground (ISceneDrawable background)
        {
            if (Background == background)
                return;

            Background = background;
            if (background == null)
                Pad(null);
            else {
                PadBottom = background.BottomHeight;
                PadTop = background.TopHeight;
                PadLeft = background.LeftWidth;
                PadRight = background.RightWidth;
                Invalidate();
            }
        }

        public ISceneDrawable Background { get; private set; }

        public override Actor Hit (float x, float y, bool touchable)
        {
            if (_clip) {
                if (Touchable == Touchable.Disabled)
                    return null;
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    return null;
            }

            return base.Hit(x, y, touchable);
        }

        public void SetClip (bool enabled)
        {
            _clip = enabled;
            IsTransform = enabled;
            Invalidate();
        }

        public int GetRow (float y)
        {
            return _layout.GetRow(y);
        }

        public override void ClearChildren ()
        {
            base.ClearChildren();
            _layout.Clear();
            Invalidate();
        }

        public Cell Add (string text)
        {
            if (Skin == null)
                throw new InvalidOperationException("Table must have a skin to use this method.");
            return Add(new Label(text, Skin));
        }

        public Cell Add (string text, string labelStyleName)
        {
            if (Skin == null)
                throw new InvalidOperationException("Table must have a skin to use this method.");
            return Add(new Label(text, Skin.Get<LabelStyle>(labelStyleName)));
        }

        public Cell Add (string text, string fontName, Color color)
        {
            if (Skin == null)
                throw new InvalidOperationException("Table must have a skin to use this method.");
            return Add(new Label(text, new LabelStyle(Skin.GetFont(fontName), color)));
        }

        public Cell Add (string text, string fontName, string colorName)
        {
            if (Skin == null)
                throw new InvalidOperationException("Table must have a skin to use this method.");
            return Add(new Label(text, new LabelStyle(Skin.GetFont(fontName), Skin.GetColor(colorName))));

        }

        public Cell Add ()
        {
            return _layout.Add(null);
        }

        public Cell Add (Actor actor)
        {
            return _layout.Add(actor);
        }

        public override bool RemoveActor (Actor actor)
        {
            if (!base.RemoveActor(actor))
                return false;

            Cell cell = GetCell(actor);
            if (cell != null)
                cell.ClearWidget();

            return true;
        }

        public Cell Stack (params Actor[] actors)
        {
            Stack stack = new Stack();
            if (actors != null) {
                foreach (var actor in actors)
                    stack.AddActor(actor);
            }

            return Add(stack);
        }

        public Cell Row ()
        {
            return _layout.Row();
        }

        public Cell ColumnDefaults (int column)
        {
            return _layout.ColumnDefaults(column);
        }

        public Cell Defaults ()
        {
            return _layout.Defaults;
        }

        public override void Layout ()
        {
            _layout.Layout();
        }

        public void Reset ()
        {
            _layout.Reset();
        }

        public Cell GetCell (Actor actor)
        {
            return _layout.GetCell(actor);
        }

        public List<Cell> GetCells ()
        {
            return _layout.Cells;
        }

        public Table Pad (Value pad)
        {
            _layout.Pad(pad);
            return this;
        }

        public Table Pad (Value top, Value left, Value bottom, Value right)
        {
            _layout.Pad(top, left, bottom, right);
            return this;
        }

        public Value PadTopValue
        {
            get { return _layout.PadTopValue; }
            set { _layout.PadTopValue = value; }
        }

        public Value PadLeftValue
        {
            get { return _layout.PadLeftValue; }
            set { _layout.PadLeftValue = value; }
        }

        public Value PadBottomValue
        {
            get { return _layout.PadBottomValue; }
            set { _layout.PadBottomValue = value; }
        }

        public Value PadRightValue
        {
            get { return _layout.PadRightValue; }
            set { _layout.PadRightValue = value; }
        }

        public Table Pad (float pad)
        {
            _layout.Pad(pad);
            return this;
        }

        public Table Pad (float top, float left, float bottom, float right)
        {
            _layout.Pad(top, left, bottom, right);
            return this;
        }

        public float PadTop
        {
            get { return _layout.PadTop; }
            set { _layout.PadTop = value; }
        }

        public float PadLeft
        {
            get { return _layout.PadLeft; }
            set { _layout.PadLeft = value; }
        }

        public float PadBottom
        {
            get { return _layout.PadBottom; }
            set { _layout.PadBottom = value; }
        }

        public float PadRight
        {
            get { return _layout.PadRight; }
            set { _layout.PadRight = value; }
        }

        public Alignment Align
        {
            get { return _layout.Align; }
            set { _layout.Align = value; }
        }

        /*public Table Center ()
        {
            _layout.Center();
            return this;
        }

        public Table Top ()
        {
            _layout.Top();
            return this;
        }

        public Table Left ()
        {
            _layout.Left();
            return this;
        }

        public Table Bottom ()
        {
            _layout.Bottom();
            return this;
        }

        public Table Right ()
        {
            _layout.Right();
            return this;
        }*/

        public Debug Debug
        {
            get { return _layout.Debug; }
            set { _layout.Debug = value; }
        }

        public float PadX
        {
            get { return _layout.PadLeft + _layout.PadRight; }
        }

        public float PadY
        {
            get { return _layout.PadTop + _layout.PadBottom; }
        }

        public bool IsRound
        {
            get { return _layout.IsRound; }
            set { _layout.IsRound = value; }
        }

        public Skin Skin { get; set; }

        [TODO]
        static public void DrawDebug (Stage stage)
        {
            throw new NotImplementedException();
        }

        [TODO]
        static private void DrawDebug (List<Actor> actors, SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }
    }
}
