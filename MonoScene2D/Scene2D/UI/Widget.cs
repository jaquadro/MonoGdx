using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;

namespace MonoGdx.Scene2D.UI
{
    public class Widget : Actor, ILayout
    {
        private bool _fillParent;
        private bool _layoutEnabled = true;

        public Widget ()
        {
            NeedsLayout = true;
        }

        public virtual float MinWidth
        {
            get { return PrefWidth; }
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
            _layoutEnabled = enabled;
            if (enabled)
                InvalidateHierarchy();
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
            if (!_layoutEnabled)
                return;

            Invalidate();

            ILayout parent = Parent as ILayout;
            if (parent != null)
                parent.InvalidateHierarchy();
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

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            Validate();
        }

        public virtual void Layout ()
        { }
    }
}
