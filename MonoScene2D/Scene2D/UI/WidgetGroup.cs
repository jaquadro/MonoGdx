using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoGdx.Scene2D.Utils;

namespace MonoGdx.Scene2D.UI
{
    public class WidgetGroup : Group, ILayout
    {
        private bool _layoutEnabled = true;
        private bool _fillParent;

        public WidgetGroup ()
        {
            NeedsLayout = true;
        }

        public virtual float MinWidth
        {
            get { return PrefWidth;  }
        }

        public virtual float MinHeight
        {
            get { return PrefHeight; }
        }

        public virtual float PrefWidth
        {
            get { return 0; }
        }

        public virtual float PrefHeight
        {
            get { return 0; }
        }

        public virtual float MaxWidth
        {
            get { return 0; }
        }

        public virtual float MaxHeight
        {
            get { return 0; }
        }

        public void SetLayoutEnabled (bool enabled)
        {
            if (_layoutEnabled == enabled)
                return;

            _layoutEnabled = enabled;
            SetLayoutEnabled(this, enabled);
        }

        private void SetLayoutEnabled (Group parent, bool enabled)
        {
            foreach (Actor actor in Children) {
                if (actor is ILayout)
                    (actor as ILayout).SetLayoutEnabled(enabled);
                else if (actor is Group)
                    SetLayoutEnabled(actor as Group, enabled);
            }
        }

        public void Validate ()
        {
            if (!_layoutEnabled)
                return;

            if (_fillParent && Parent != null) {
                float parentWidth, parentHeight;
                if (Stage != null && Parent == Stage.Root) {
                    parentWidth = Stage.Width;
                    parentHeight = Stage.Height;
                }
                else {
                    parentWidth = Parent.Width;
                    parentHeight = Parent.Height;
                }

                if (Width != parentWidth || Height != parentHeight) {
                    Width = parentWidth;
                    Height = parentHeight;
                    Invalidate();
                }
            }

            if (!NeedsLayout)
                return;

            NeedsLayout = false;
            Layout();
        }

        public bool NeedsLayout { get; private set; }

        public virtual void Invalidate ()
        {
            NeedsLayout = true;
        }

        public void InvalidateHierarchy ()
        {
            Invalidate();

            ILayout parent = Parent as ILayout;
            if (parent != null)
                parent.InvalidateHierarchy();
        }

        protected override void ChildrenChanged ()
        {
            InvalidateHierarchy();
        }

        public void Pack ()
        {
            float newWidth = PrefWidth;
            float newHeight = PrefHeight;

            if (newWidth != Width || newHeight != Height) {
                Width = newWidth;
                Height = newHeight;
                Invalidate();
            }

            Validate();
        }

        public void SetFillParent (bool fillParent)
        {
            _fillParent = fillParent;
        }

        public virtual void Layout ()
        { }

        public override void Draw (SpriteBatch spriteBatch, float parentAlpha)
        {
            Validate();
            base.Draw(spriteBatch, parentAlpha);
        }
    }
}
