using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using MonoGdx.TableLayout;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.UI
{
    public class ImageButton : Button
    {
        private readonly Image _image;
        private ImageButtonStyle _style;

        public ImageButton (Skin skin)
            : this(skin.Get<ImageButtonStyle>())
        { }

        public ImageButton (Skin skin, string styleName)
            : this(skin.Get<ImageButtonStyle>(styleName))
        { }

        public ImageButton (ImageButtonStyle style)
            : base(style)
        {
            _image = new Image();
            _image.Scaling = Scaling.Fit;

            Add(_image);
            Style = style;
            Width = PrefWidth;
            Height = PrefHeight;
        }

        public ImageButton (ISceneDrawable imageUp)
            : this(new ImageButtonStyle(null, null, null, imageUp, null, null))
        { }

        public ImageButton (ISceneDrawable imageUp, ISceneDrawable imageDown)
            : this(new ImageButtonStyle(null, null, null, imageUp, imageDown, null))
        { }

        public ImageButton (ISceneDrawable imageUp, ISceneDrawable imageDown, ISceneDrawable imageChecked)
            : this(new ImageButtonStyle(null, null, null, imageUp, imageDown, imageChecked))
        { }

        public new ImageButtonStyle Style
        {
            get { return _style; }
            set 
            {
                base.StyleCore = value;
                _style = value;

                if (_image != null)
                    UpdateImage();
            }
        }

        protected override ButtonStyle StyleCore
        {
            get { return _style; }
            set
            {
                if (!(value is ImageButtonStyle))
                    throw new ArgumentException("Style must be an ImageButtonStyle");
                Style = value as ImageButtonStyle;
            }
        }

        private void UpdateImage ()
        {
            if (IsDisabled && _style.ImageDisabled != null)
                _image.Drawable = _style.ImageDisabled;
            else if (IsPressed && _style.ImageDown != null)
                _image.Drawable = _style.ImageDown;
            else if (IsChecked && _style.ImageChecked != null)
                _image.Drawable = IsOver ? _style.ImageCheckedOver ?? _style.ImageChecked : _style.ImageChecked;
            else if (IsOver && _style.ImageOver != null)
                _image.Drawable = _style.ImageOver;
            else if (_style.ImageUp != null)
                _image.Drawable = _style.ImageUp;
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            UpdateImage();
            base.Draw(spriteBatch, parentAlpha);
        }

        public Image Image
        {
            get { return _image; }
        }

        public Cell ImageCell
        {
            get { return GetCell(_image); }
        }
    }

    public class ImageButtonStyle : ButtonStyle
    {
        public ImageButtonStyle ()
        { }

        public ImageButtonStyle (ISceneDrawable up, ISceneDrawable down, ISceneDrawable chkd, 
            ISceneDrawable imageUp, ISceneDrawable imageDown, ISceneDrawable imageChkd)
            : base(up, down, chkd)
        {
            ImageUp = imageUp;
            ImageDown = imageDown;
            ImageChecked = imageChkd;
        }

        public ImageButtonStyle (ImageButtonStyle style)
            : base(style)
        {
            ImageUp = style.ImageUp;
            ImageDown = style.ImageDown;
            ImageOver = style.ImageOver;
            ImageChecked = style.ImageChecked;
            ImageCheckedOver = style.ImageCheckedOver;
            ImageDisabled = style.ImageDisabled;
        }

        public ImageButtonStyle (ButtonStyle style)
            : base(style)
        { }

        public ISceneDrawable ImageUp { get; set; }
        public ISceneDrawable ImageDown { get; set; }
        public ISceneDrawable ImageOver { get; set; }
        public ISceneDrawable ImageChecked { get; set; }
        public ISceneDrawable ImageCheckedOver { get; set; }
        public ISceneDrawable ImageDisabled { get; set; }
    }
}
