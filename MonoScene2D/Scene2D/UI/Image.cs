using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using MonoGdx.TableLayout;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.UI
{
    public class Image : Widget
    {
        private ISceneDrawable _drawable;

        public Image ()
            : this((ISceneDrawable)null)
        { }

        public Image (NinePatch patch)
            : this(new NinePatchDrawable(patch), Scaling.Stretch, Alignment.Center)
        { }

        public Image (TextureRegion region)
            : this(new TextureRegionDrawable(region), Scaling.Stretch, Alignment.Center)
        { }

        public Image (Texture2D texture)
            : this(new TextureRegionDrawable(new TextureRegion(texture)))
        { }

        public Image (Skin skin, string drawableName)
            : this(skin.GetDrawable(drawableName), Scaling.Stretch, Alignment.Center)
        { }

        public Image (ISceneDrawable drawable)
            : this(drawable, Scaling.Stretch, Alignment.Center)
        { }

        public Image (ISceneDrawable drawable, Scaling scaling)
            : this(drawable, scaling, Alignment.Center)
        { }

        public Image (ISceneDrawable drawable, Scaling scaling, Alignment align)
        {
            Drawable = drawable;
            Scaling = scaling;
            Align = align;

            Width = PrefWidth;
            Height = PrefHeight;
        }

        public override void Layout ()
        {
            if (_drawable == null)
                return;

            float regionWidth = _drawable.MinWidth;
            float regionHeight = _drawable.MinHeight;
            float width = Width;
            float height = Height;

            Vector2 size = Scaling.Apply(regionWidth, regionHeight, width, height);
            ImageWidth = size.X;
            ImageHeight = size.Y;

            if ((Align & Alignment.Left) != 0)
                ImageX = 0;
            else if ((Align & Alignment.Right) != 0)
                ImageX = (int)(width - ImageWidth);
            else
                ImageX = (int)(width / 2 - ImageWidth / 2);

            if ((Align & Alignment.Top) != 0)
                ImageY = (int)(height - ImageHeight);
            else if ((Align & Alignment.Bottom) != 0)
                ImageY = 0;
            else
                ImageY = (int)(height / 2 - ImageHeight / 2);
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            Validate();

            spriteBatch.Color = Color.MultiplyAlpha(parentAlpha);

            float x = X;
            float y = Y;
            float scaleX = ScaleX;
            float scaleY = ScaleY;

            if (_drawable != null) {
                if (_drawable is TextureRegionDrawable) {
                    TextureRegion region = (Drawable as TextureRegionDrawable).Region;
                    float rotation = Rotation;
                    if (scaleX == 1 && scaleY == 1 && rotation == 0)
                        spriteBatch.Draw(region, x + ImageX, y + ImageY, ImageWidth, ImageHeight);
                    else
                        spriteBatch.Draw(region, x + ImageX, y + ImageY, OriginX - ImageX, OriginY - ImageY, ImageWidth, ImageHeight, scaleX, scaleY, rotation);
                }
                else
                    _drawable.Draw(spriteBatch, x + ImageX, y + ImageY, ImageWidth * scaleX, ImageHeight * scaleY);
            }
        }

        public ISceneDrawable Drawable
        {
            get { return _drawable; }
            set
            {
                if (value != null) {
                    if (_drawable == value)
                        return;
                    if (PrefWidth != value.MinWidth || PrefHeight != value.MinHeight)
                        InvalidateHierarchy();
                }
                else
                    if (PrefWidth != 0 || PrefHeight != 0)
                        InvalidateHierarchy();

                _drawable = value;
            }
        }

        public Scaling Scaling { get; set; }

        public Alignment Align { get; set; }

        public override float MinWidth
        {
            get { return 0; }
        }

        public override float MinHeight
        {
            get { return 0; }
        }

        public override float PrefWidth
        {
            get
            {
                if (_drawable != null)
                    return _drawable.MinWidth;
                return 0;
            }
        }

        public override float PrefHeight
        {
            get
            {
                if (_drawable != null)
                    return _drawable.MinHeight;
                return 0;
            }
        }

        public float ImageX { get; private set; }
        public float ImageY { get; private set; }
        public float ImageWidth { get; private set; }
        public float ImageHeight { get; private set; }
    }
}
