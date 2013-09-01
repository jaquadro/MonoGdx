﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGdx.Graphics.G2D
{
    public class TextureAtlas : IDisposable
    {
        public class TextureAtlasData
        {
            public class Page
            {
                public string TextureFile { get; private set; }
                public Texture2D Texture { get; set; }
                public bool UseMipMaps { get; private set; }
                public SurfaceFormat Format { get; private set; }
                public TextureFilter MinFilter { get; private set; }
                public TextureFilter MagFilter { get; private set; }
                public TextureAddressMode UWrap { get; private set; }
                public TextureAddressMode VWrap { get; private set; }

                public Page (string file, bool useMipMaps, SurfaceFormat format, TextureFilter minFilter, TextureFilter magFilter,
                    TextureAddressMode uWrap, TextureAddressMode vWrap)
                {
                    TextureFile = file;
                    UseMipMaps = useMipMaps;
                    Format = format;
                    MinFilter = minFilter;
                    MagFilter = magFilter;
                    UWrap = uWrap;
                    VWrap = vWrap;
                }
            }

            public class Region
            {
                public Page Page { get; set; }
                public int Index { get; set; }
                public String Name { get; set; }
                public float OffsetX { get; set; }
                public float OffsetY { get; set; }
                public int OriginalWidth { get; set; }
                public int OriginalHeight { get; set; }
                public bool Rotate { get; set; }
                public int Left { get; set; }
                public int Top { get; set; }
                public int Width { get; set; }
                public int Height { get; set; }
                public bool Flip { get; set; }
                public int[] Splits { get; set; }
                public int[] Pads { get; set; }
            }

            public List<Page> Pages { get; private set; }
            public List<Region> Regions { get; private set; }

            public TextureAtlasData (string packFile, string imagesDir, bool flip)
            {
                using (StreamReader reader = new StreamReader(packFile)) {
                    List<string> tupleData = new List<string>();
                    Page pageImage = null;

                    while (true) {
                        string line = reader.ReadLine();
                        if (line == null)
                            break;

                        if (line.Trim().Length == 0)
                            pageImage = null;
                        else if (pageImage == null) {
                            string file = Path.Combine(imagesDir, line);
                            

                            SurfaceFormat format = (SurfaceFormat)Enum.Parse(typeof(SurfaceFormat), ReadValue(reader), true);

                            ReadTuple(reader, tupleData);
                            TextureFilter min = (TextureFilter)Enum.Parse(typeof(TextureFilter), tupleData[0], true);
                            TextureFilter max = (TextureFilter)Enum.Parse(typeof(TextureFilter), tupleData[1], true);

                            string direction = ReadValue(reader);
                            TextureAddressMode repeatX = TextureAddressMode.Clamp;
                            TextureAddressMode repeatY = TextureAddressMode.Clamp;
                            switch (direction) {
                                case "x":
                                    repeatX = TextureAddressMode.Wrap;
                                    break;
                                case "y":
                                    repeatY = TextureAddressMode.Wrap;
                                    break;
                                case "xy":
                                    repeatX = TextureAddressMode.Wrap;
                                    repeatY = TextureAddressMode.Wrap;
                                    break;
                            }

                            pageImage = new Page(file, min.IsMapMap(), format, min, max, repeatX, repeatY);
                            Pages.Add(pageImage);
                        }
                        else {
                            bool rotate = bool.Parse(ReadValue(reader));

                            ReadTuple(reader, tupleData);
                            int left = int.Parse(tupleData[0]);
                            int top = int.Parse(tupleData[1]);

                            ReadTuple(reader, tupleData);
                            int width = int.Parse(tupleData[0]);
                            int height = int.Parse(tupleData[1]);

                            Region region = new Region() {
                                Page = pageImage,
                                Left = left,
                                Top = top,
                                Width = width,
                                Height = height,
                                Name = line,
                                Rotate = rotate,
                            };

                            if (ReadTuple(reader, tupleData) == 4) {
                                region.Splits = new int[] {
                                    int.Parse(tupleData[0]), int.Parse(tupleData[1]),
                                    int.Parse(tupleData[2]), int.Parse(tupleData[3]),
                                };

                                if (ReadTuple(reader, tupleData) == 4) {
                                    region.Pads = new int[] {
                                        int.Parse(tupleData[0]), int.Parse(tupleData[1]),
                                        int.Parse(tupleData[2]), int.Parse(tupleData[3]),
                                    };

                                    ReadTuple(reader, tupleData);
                                }
                            }

                            region.OriginalWidth = int.Parse(tupleData[0]);
                            region.OriginalHeight = int.Parse(tupleData[1]);

                            ReadTuple(reader, tupleData);
                            region.OffsetX = int.Parse(tupleData[0]);
                            region.OffsetY = int.Parse(tupleData[1]);

                            region.Index = int.Parse(ReadValue(reader));

                            if (flip)
                                region.Flip = true;

                            Regions.Add(region);
                        }
                    }
                }

                Regions.Sort(IndexComparator);
            }

            /*private static SurfaceFormat TranslateSurfaceFormat (string format)
            {
                switch (format) {
                    case "Alpha": return SurfaceFormat.Alpha8;
                    case "Intensity": return SurfaceFormat.Alpha8;
                    case "LuminanceAlpha": return SurfaceFormat.NormalizedByte2;
                    case "RGB565": return SurfaceFormat.Bgr565;
                    case "RGBA4444": return SurfaceFormat.Bgra4444;
                    case "RGB888": return SurfaceFormat.Bgr32;
                    case "RGBA8888": return SurfaceFormat.Bgra32;
                    default:
                        throw new Exception("Unknown surface format");
                }
            }*/
        }

        public TextureAtlas (GraphicsDevice graphicsDevice)
        { }

        public TextureAtlas (GraphicsDevice graphicsDevice, string packFile)
            : this(graphicsDevice, packFile, Path.GetDirectoryName(packFile))
        { }

        public TextureAtlas (GraphicsDevice graphicsDevice, string packFile, bool flip)
            : this(graphicsDevice, packFile, Path.GetDirectoryName(packFile), flip)
        { }

        public TextureAtlas (GraphicsDevice graphicsDevice, string packFile, string imagesDir)
            : this(graphicsDevice, packFile, imagesDir, false)
        { }

        public TextureAtlas (GraphicsDevice graphicsDevice, string packFile, string imagesDir, bool flip)
            : this(graphicsDevice, new TextureAtlasData(packFile, imagesDir, flip))
        { }

        public TextureAtlas (GraphicsDevice graphicsDevice, TextureAtlasData data)
        {
            if (data != null)
                Load(graphicsDevice, data);
        }

        [TODO]
        private void Load (GraphicsDevice graphicsDevice, TextureAtlasData data)
        {
            Dictionary<TextureAtlasData.Page, Texture2D> pageToTexture = new Dictionary<TextureAtlasData.Page, Texture2D>();

            foreach (var page in data.Pages) {
                Texture2D texture = null;
                if (page.Texture == null) {
                    using (FileStream fs = File.Open(page.TextureFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        texture = Texture2D.FromStream(graphicsDevice, fs);
                        if (texture.Format != page.Format) {
                            byte[] buffer = new byte[texture.Width * texture.Height * 4];
                            texture.GetData<byte>(buffer);

                            texture = new Texture2D(graphicsDevice, texture.Width, texture.Height, page.UseMipMaps, page.Format);
                            texture.SetData<byte>(buffer);
                        }
                    }
                }
                else {
                    texture = page.Texture;
                }

                //texture.SetFilter(page.MinFilter, page.MagFilter);
                //texture.SetWrap(page.UWrap, page.VWrap);

                Textures.Add(texture);
                pageToTexture[page] = texture;
            }

            foreach (var region in data.Regions) {
                int width = region.Width;
                int height = region.Height;

                AtlasRegion atlasRegion = new AtlasRegion(pageToTexture[region.Page], region.Left, region.Top,
                    region.Rotate ? height : width, region.Rotate ? width : height) {
                    Index = region.Index,
                    Name = region.Name,
                    OffsetX = region.OffsetX,
                    OffsetY = region.OffsetY,
                    OriginalWidth = region.OriginalWidth,
                    OriginalHeight = region.OriginalHeight,
                    Rotate = region.Rotate,
                    Splits = region.Splits,
                    Pads = region.Pads,
                };

                if (region.Flip)
                    atlasRegion.Flip(false, true);

                Regions.Add(atlasRegion);
            }
        }

        public AtlasRegion AddRegion (string name, Texture2D texture, int x, int y, int width, int height)
        {
            Textures.Add(texture);

            AtlasRegion region = new AtlasRegion(texture, x, y, width, height) {
                Name = name,
                OriginalWidth = width,
                OriginalHeight = height,
                Index = -1,
            };

            Regions.Add(region);
            return region;
        }

        public AtlasRegion AddRegion (string name, TextureRegion textureRegion)
        {
            return AddRegion(name, textureRegion.Texture, textureRegion.RegionX, textureRegion.RegionY, 
                textureRegion.RegionWidth, textureRegion.RegionHeight);
        }

        public List<AtlasRegion> Regions { get; private set; }

        public AtlasRegion FindRegion (string name)
        {
            foreach (AtlasRegion region in Regions) {
                if (region.Name == name)
                    return region;
            }

            return null;
        }

        public AtlasRegion FindRegion (string name, int index)
        {
            foreach (AtlasRegion region in Regions) {
                if (region.Name != name)
                    continue;
                if (region.Index != index)
                    continue;
                return region;
            }

            return null;
        }

        public List<AtlasRegion> FindRegions (string name)
        {
            List<AtlasRegion> matched = new List<AtlasRegion>();
            foreach (AtlasRegion region in Regions) {
                if (region.Name == name)
                    matched.Add(region);
            }

            return matched;
        }

        public List<Sprite> CreateSprites ()
        {
            List<Sprite> sprites = new List<Sprite>(Regions.Count);
            foreach (AtlasRegion region in Regions)
                sprites.Add(NewSprite(region));

            return sprites;
        }

        public Sprite CreateSprite (string name)
        {
            foreach (AtlasRegion region in Regions) {
                if (region.Name == name)
                    return NewSprite(region);
            }

            return null;
        }

        public Sprite CreateSprite (string name, int index)
        {
            foreach (AtlasRegion region in Regions) {
                if (region.Name != name)
                    continue;
                if (region.Index != index)
                    continue;
                return NewSprite(region);
            }

            return null;
        }

        public List<Sprite> CreateSprites (string name)
        {
            List<Sprite> matched = new List<Sprite>();
            foreach (AtlasRegion region in Regions) {
                if (region.Name == name)
                    matched.Add(NewSprite(region));
            }

            return matched;
        }

        private Sprite NewSprite (AtlasRegion region)
        {
            if (region.PackedWidth == region.OriginalWidth && region.PackedHeight == region.OriginalHeight) {
                if (region.Rotate) {
                    Sprite sprite = new Sprite(region);
                    sprite.SetBounds(0, 0, region.RegionHeight, region.RegionWidth);
                    sprite.Rotate90(true);
                    return sprite;
                }
                return new Sprite(region);
            }
            return new AtlasSprite(region);
        }

        public NinePatch CreatePatch (string name)
        {
            foreach (AtlasRegion region in Regions) {
                if (region.Name == name) {
                    int[] splits = region.Splits;
                    if (splits == null)
                        throw new ArgumentException("Region does not have ninepatch splits: " + name);

                    NinePatch patch = new NinePatch(region, splits[0], splits[1], splits[2], splits[3]);
                    if (region.Pads != null)
                        patch.SetPadding(region.Pads[0], region.Pads[1], region.Pads[2], region.Pads[3]);

                    return patch;
                }
            }

            return null;
        }

        public HashSet<Texture2D> Textures { get; private set; }

        public void Dispose ()
        {
            foreach (Texture2D texture in Textures)
                texture.Dispose();

            Textures.Clear();
        }

        static int IndexComparator (TextureAtlasData.Region region1, TextureAtlasData.Region region2)
        {
            int i1 = region1.Index;
            if (i1 == -1)
                i1 = int.MaxValue;

            int i2 = region2.Index;
            if (i2 == -1)
                i2 = int.MaxValue;

            return i1 - i2;
        }

        static string ReadValue (TextReader reader)
        {
            string line = reader.ReadLine();
            int colon = line.IndexOf(':');
            if (colon == -1)
                throw new Exception("Invalid line: " + line);

            return line.Substring(colon + 1).Trim();
        }

        static int ReadTuple (TextReader reader, List<string> resultsList)
        {
            resultsList.Clear();

            string line = reader.ReadLine();
            int colon = line.IndexOf(':');
            if (colon == -1)
                throw new Exception("Invalid line: " + line);

            int lastMatch = colon + 1;

            for (int i = 0; i < 3; i++) {
                int comma = line.IndexOf(',', lastMatch);
                if (comma == -1) {
                    if (i == 0)
                        throw new Exception("Invalid line: " + line);
                    break;
                }

                resultsList.Add(line.Substring(lastMatch, comma).Trim());
                lastMatch = comma + 1;
            }

            resultsList.Add(line.Substring(lastMatch).Trim());
            return resultsList.Count;
        }

        public class AtlasRegion : TextureRegion
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public float OffsetX { get; set; }
            public float OffsetY { get; set; }
            public int PackedWidth { get; set; }
            public int PackedHeight { get; set; }
            public int OriginalWidth { get; set; }
            public int OriginalHeight { get; set; }
            public bool Rotate { get; set; }
            public int[] Splits { get; set; }
            public int[] Pads { get; set; }

            public AtlasRegion (Texture2D texture, int x, int y, int width, int height)
                : base(texture, x, y, width, height)
            {
                OriginalWidth = width;
                OriginalHeight = height;
                PackedWidth = width;
                PackedHeight = height;
            }

            public AtlasRegion (AtlasRegion region)
            {
                SetRegion(region);

                Index = region.Index;
                Name = region.Name;
                OffsetX = region.OffsetX;
                OffsetY = region.OffsetY;
                PackedWidth = region.PackedWidth;
                PackedHeight = region.PackedHeight;
                OriginalWidth = region.OriginalWidth;
                OriginalHeight = region.OriginalHeight;
                Rotate = region.Rotate;
                Splits = region.Splits;
            }

            public override void Flip (bool x, bool y)
            {
                base.Flip(x, y);

                if (x)
                    OffsetX = OriginalWidth - OffsetX - RotatedPackedWidth;
                if (y)
                    OffsetY = OriginalHeight - OffsetY - RotatedPackedHeight;
            }

            public float RotatedPackedWidth
            {
                get { return Rotate ? PackedHeight : PackedWidth; }
            }

            public float RotatedPackedHeight
            {
                get { return Rotate ? PackedWidth : PackedHeight; }
            }
        }

        public class AtlasSprite : Sprite
        {
            private float _originalOffsetX;
            private float _originalOffsetY;

            public AtlasRegion Region { get; private set; }

            public AtlasSprite (AtlasRegion region)
            {
                Region = new AtlasRegion(region);
                _originalOffsetX = region.OffsetX;
                _originalOffsetY = region.OffsetY;

                SetRegion(region);
                SetOrigin(region.OriginalWidth / 2f, region.OriginalHeight / 2f);

                int width = region.RegionWidth;
                int height = region.RegionHeight;

                if (region.Rotate) {
                    base.Rotate90(true);
                    base.SetBounds(region.OffsetX, region.OffsetY, height, width);
                }
                else
                    base.SetBounds(region.OffsetX, region.OffsetY, width, height);

                Color = new Color(1f, 1f, 1f, 1f);
            }

            public AtlasSprite (AtlasSprite sprite)
            {
                Region = sprite.Region;

                _originalOffsetX = sprite._originalOffsetX;
                _originalOffsetY = sprite._originalOffsetY;

                Set(sprite);
            }

            public override void SetPosition (float x, float y)
            {
                base.SetPosition(x + Region.OffsetX, y + Region.OffsetY);
            }

            public override void SetBounds (float x, float y, float width, float height)
            {
                float widthRatio = width / Region.OriginalWidth;
                float heightRatio = height / Region.OriginalHeight;

                Region.OffsetX = _originalOffsetX * widthRatio;
                Region.OffsetY = _originalOffsetY * heightRatio;

                int packedWidth = Region.Rotate ? Region.PackedHeight : Region.PackedWidth;
                int packedHeight = Region.Rotate ? Region.PackedWidth : Region.PackedHeight;

                base.SetBounds(x + Region.OffsetX, y + Region.OffsetY, packedWidth * widthRatio, packedHeight * heightRatio);
            }

            public override void SetSize (float width, float height)
            {
                SetBounds(X, Y, width, height);
            }

            public override void SetOrigin (float originX, float originY)
            {
                base.SetOrigin(originX - Region.OffsetX, originY - Region.OffsetY);
            }

            public override void Flip (bool x, bool y)
            {
                base.Flip(x, y);

                float oldOriginX = OriginX;
                float oldOriginY = OriginY;
                float oldOffsetX = Region.OffsetX;
                float oldOffsetY = Region.OffsetY;

                float widthRatio = WidthRatio;
                float heightRatio = HeightRatio;

                Region.OffsetX = _originalOffsetX;
                Region.OffsetY = _originalOffsetY;
                Region.Flip(x, y);

                _originalOffsetX = Region.OffsetX;
                _originalOffsetY = Region.OffsetY;

                Region.OffsetX *= widthRatio;
                Region.OffsetY *= HeightRatio;

                Translate(Region.OffsetX - oldOffsetX, Region.OffsetY - oldOffsetY);
                SetOrigin(oldOriginX, oldOriginY);
            }

            public override void Rotate90 (bool clockwise)
            {
                base.Rotate90(clockwise);

                float oldOriginX = OriginX;
                float oldOriginY = OriginY;
                float oldOffsetX = Region.OffsetX;
                float oldOffsetY = Region.OffsetY;

                float widthRatio = WidthRatio;
                float heightRatio = HeightRatio;

                if (clockwise) {
                    Region.OffsetX = oldOffsetY;
                    Region.OffsetY = Region.OriginalHeight * heightRatio - oldOffsetX - Region.PackedWidth * widthRatio;
                }
                else {
                    Region.OffsetX = Region.OriginalWidth * widthRatio - oldOffsetY - Region.PackedHeight * heightRatio;
                    Region.OffsetY = oldOffsetX;
                }

                Translate(Region.OffsetX - oldOffsetX, Region.OffsetY - oldOffsetY);
                SetOrigin(oldOriginX, oldOriginY);
            }

            public override float X
            {
                get { return base.X - Region.OffsetX; }
            }

            public override float Y
            {
                get { return base.Y - Region.OffsetY; }
            }

            public override float OriginX
            {
                get { return base.OriginX + Region.OffsetX; }
            }

            public override float OriginY
            {
                get { return base.OriginY + Region.OffsetY; }
            }

            public override float Width
            {
                get { return base.Width / Region.RotatedPackedWidth * Region.OriginalWidth; }
            }

            public override float Height
            {
                get { return base.Height / Region.RotatedPackedHeight * Region.OriginalHeight; }
            }

            public float WidthRatio
            {
                get { return base.Width / Region.RotatedPackedWidth; }
            }

            public float HeightRatio
            {
                get { return base.Height / Region.RotatedPackedHeight; }
            }
        }
    }
}
