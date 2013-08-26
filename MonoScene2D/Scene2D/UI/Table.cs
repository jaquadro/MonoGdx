using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        /*public Table ()
            : this(null)
        { }

        public Table (Skin skin)
        {
            throw new NotImplementedException();
        }*/

        public override void Draw (SpriteBatch spriteBatch, float parentAlpha)
        {
            throw new NotImplementedException();
        }

        protected void DrawBackground (SpriteBatch spriteBatch, float parentAlpha)
        {
            throw new NotImplementedException();
        }

        public override void Invalidate ()
        {
            throw new NotImplementedException();
            base.Invalidate();
        }

        public override float PrefWidth
        {
            get { throw new NotImplementedException(); }
        }

        public override float PrefHeight
        {
            get { throw new NotImplementedException(); }
        }

        public override float MinWidth
        {
            get { throw new NotImplementedException(); }
        }

        public override float MinHeight
        {
            get { throw new NotImplementedException(); }
        }

        public void SetBackground (string drawableName)
        {
            throw new NotImplementedException();
        }

        public void SetBackground (IDrawable background)
        {
            throw new NotImplementedException();
        }

        public IDrawable Background { get; private set; }

        public override Actor Hit (float x, float y, bool touchable)
        {
            throw new NotImplementedException();
        }

        public void SetClip (bool enabled)
        {
            throw new NotImplementedException();
        }

        public int GetRow (float y)
        {
            throw new NotImplementedException();
        }

        public override void ClearChildren ()
        {
            base.ClearChildren();
            throw new NotImplementedException();
        }

        public Cell Add (string text)
        {
            throw new NotImplementedException();
        }

        public Cell Add (string text, string labelStyleName)
        {
            throw new NotImplementedException();
        }

        public Cell Add (string text, string frontName, Color color)
        {
            throw new NotImplementedException();
        }

        public Cell Add (string text, string frontName, string colorName)
        {
            throw new NotImplementedException();
        }

        public Cell Add ()
        {
            throw new NotImplementedException();
        }

        public Cell Add (Actor actor)
        {
            throw new NotImplementedException();
        }

        public override bool RemoveActor (Actor actor)
        {
            throw new NotImplementedException();
        }

        public Cell Stack (params Actor actors)
        {
            throw new NotImplementedException();
        }

        public Cell Row ()
        {
            throw new NotImplementedException();
        }

        public Cell ColumnDefaults (int column)
        {
            throw new NotImplementedException();
        }

        public Cell Defaults ()
        {
            throw new NotImplementedException();
        }

        public override void Layout ()
        {
            throw new NotImplementedException();
        }

        public void Reset ()
        {
            throw new NotImplementedException();
        }

        public Cell GetCell (Actor actor)
        {
            throw new NotImplementedException();
        }

        public List<Cell> GetCells ()
        {
            throw new NotImplementedException();
        }

        public Table Pad (Value pad)
        {
            throw new NotImplementedException();
        }

        public Table Pad (Value top, Value left, Value bottom, Value right)
        {
            throw new NotImplementedException();
        }

        public Value PadTopValue
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Value PadLeftValue
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Value PadBottomValue
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Value PadRightValue
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Table Pad (float pad)
        {
            throw new NotImplementedException();
        }

        public Table Pad (float top, float left, float bottom, float right)
        {
            throw new NotImplementedException();
        }

        public float PadTop
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public float PadLeft
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public float PadBottom
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public float PadRight
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Alignment Align
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Table Center ()
        {
            throw new NotImplementedException();
        }

        public Table Top ()
        {
            throw new NotImplementedException();
        }

        public Table Left ()
        {
            throw new NotImplementedException();
        }

        public Table Bottom ()
        {
            throw new NotImplementedException();
        }

        public Table Right ()
        {
            throw new NotImplementedException();
        }

        public Debug Debug
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public float PadX
        {
            get { throw new NotImplementedException(); }
        }

        public float PadY
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsRound
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        static public void DrawDebug (Stage stage)
        {
            throw new NotImplementedException();
        }

        static private void DrawDebug (List<Actor> actors, SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }
    }
}
