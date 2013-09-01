using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGdx.Graphics.G2D
{
    public class BitmapFont : IDisposable
    {
        internal const int Log2PageSize = 9;
        internal const int PageSize = 1 << Log2PageSize;
        internal const int Pages = 0x10000 / PageSize;

        public static readonly char[] XChars = { 'x', 'e', 'a', 'o', 'n', 's', 'r', 'c', 'u', 'm', 'v', 'w', 'z' };
        public static readonly char[] CapChars = {'M', 'N', 'B', 'D', 'C', 'E', 'F', 'K', 'A', 'G', 'H', 'I', 'J', 
            'L', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};

        private readonly BitmapFontData _data;
        internal TextureRegion _region;

        private readonly BitmapFontCache _cache;
        private bool _flipped;
        private bool _integer;

        [TODO("Embedded resources")]
        public BitmapFont (GraphicsDevice device)
            : this(device, "Resources/arial-15.fnt", "Resources/arial-15.png", false, true)
        {
            throw new NotImplementedException();
        }

        [TODO("Embedded resources")]
        public BitmapFont (GraphicsDevice device, bool flip)
            : this(device, "Resources/arial-15.fnt", "Resources/arial-15.png", flip, true)
        {
            throw new NotImplementedException();
        }

        public BitmapFont (GraphicsDevice device, string fontFile, TextureRegion region, bool flip)
            : this(device, new BitmapFontData(fontFile, flip), region, true)
        { }

        public BitmapFont (GraphicsDevice device, string fontFile, bool flip)
            : this(device, new BitmapFontData(fontFile, flip), null, true)
        { }

        public BitmapFont (GraphicsDevice device, string fontFile, string imageFile, bool flip)
            : this(device, fontFile, imageFile, flip, true)
        { }

        public BitmapFont (GraphicsDevice device, string fontFile, string imageFile, bool flip, bool integer)
            : this(device, new BitmapFontData(fontFile, flip), new TextureRegion(CreateTexture(device, imageFile)), integer)
        {
            OwnsTexture = true;
        }

        private static Texture2D CreateTexture (GraphicsDevice device, string file)
        {
            using (FileStream fs = File.OpenRead(file)) {
                return Texture2D.FromStream(device, fs);
            }
        }

        [TODO("FileHandle")]
        public BitmapFont (GraphicsDevice device, BitmapFontData data, TextureRegion region, bool integer)
        {
            if (region != null)
                _region = region;
            else {
                if (data.FontFile == null) {
                    _region = new TextureRegion(CreateTexture(device, data.ImagePath));
                    // this.region = new TextureRegion(new Texture(Gdx.files.internal(data.imagePath), false));
                    throw new NotImplementedException();
                }
                else {
                    _region = new TextureRegion(CreateTexture(device, data.ImagePath));
                    // this.region = new TextureRegion(new Texture(Gdx.files.getFileHandle(data.imagePath, data.fontFile.type()), false));
                    throw new NotImplementedException();
                }
            }

            _flipped = data.IsFlipped;
            _data = data;
            UsesIntegerPostions = integer;

            _cache = new BitmapFontCache(this) {
                UsesIntegerPositions = integer,
            };

            Load(_data);
            //OwnsTexture = (_region == null);
        }

        private void Load (BitmapFontData data)
        {
            float invTexWidth = 1f / _region.Texture.Width;
            float invTexHeight = 1f / _region.Texture.Height;
            float u = _region.U;
            float v = _region.V;

            float offsetX = 0;
            float offsetY = 0;
            float regionWidth = _region.RegionWidth;
            float regionHeight = _region.RegionHeight;

            if (_region is TextureAtlas.AtlasRegion) {
                // Compensate for whitespace stripped from left and top edges.
                TextureAtlas.AtlasRegion atlasRegion = _region as TextureAtlas.AtlasRegion;
                offsetX = atlasRegion.OffsetX;
                offsetY = atlasRegion.OriginalHeight - atlasRegion.PackedHeight - atlasRegion.OffsetY;
            }

            foreach (Glyph[] page in data.Glyphs) {
                if (page == null)
                    continue;

                foreach (Glyph glyph in page) {
                    if (glyph == null)
                        continue;

                    float x = glyph.SrcX;
                    float x2 = glyph.SrcX + glyph.Width;
                    float y = glyph.SrcY;
                    float y2 = glyph.SrcY + glyph.Height;

                    // Shift glyph for left and top edge stripped whitespace. 
                    // Clip glyph for right and bottom edge stripped whitespace.
                    if (offsetX > 0) {
                        x -= offsetX;
                        if (x < 0) {
                            glyph.Width += (int)x;
                            glyph.XOffset -= (int)x;
                            x = 0;
                        }

                        x2 -= offsetX;
                        if (x2 > regionWidth) {
                            glyph.Width -= (int)(x2 - regionWidth);
                            x2 = regionWidth;
                        }
                    }

                    if (offsetY > 0) {
                        y -= offsetY;
                        if (y < 0) {
                            glyph.Height += (int)y;
                            y = 0;
                        }

                        y2 -= offsetY;
                        if (y2 > regionHeight) {
                            float amount = y2 - regionWidth;
                            glyph.Height -= (int)amount;
                            glyph.YOffset += (int)amount;
                            y2 = regionHeight;
                        }
                    }

                    glyph.U = u + x * invTexWidth;
                    glyph.U2 = u + x2 * invTexWidth;

                    if (data.IsFlipped) {
                        glyph.V = v + y * invTexHeight;
                        glyph.V2 = v + y2 * invTexHeight;
                    }
                    else {
                        glyph.V2 = v + y * invTexHeight;
                        glyph.V = v + y2 * invTexHeight;
                    }
                }
            }
        }

        public TextBounds Draw (GdxSpriteBatch spriteBatch, string str, float x, float y)
        {
            _cache.Clear();
            TextBounds bounds = _cache.AddText(str, x, y, 0, str.Length);
            _cache.Draw(spriteBatch);
            return bounds;
        }

        public TextBounds Draw (GdxSpriteBatch spriteBatch, string str, float x, float y, int start, int end)
        {
            _cache.Clear();
            TextBounds bounds = _cache.AddText(str, x, y, start, end);
            _cache.Draw(spriteBatch);
            return bounds;
        }

        public TextBounds DrawMultiLine (GdxSpriteBatch spriteBatch, string str, float x, float y)
        {
            _cache.Clear();
            TextBounds bounds = _cache.AddMultiLineText(str, x, y, 0, HAlignment.Left);
            _cache.Draw(spriteBatch);
            return bounds;
        }

        public TextBounds DrawMultiLine (GdxSpriteBatch spriteBatch, string str, float x, float y, float alignmentWidth, HAlignment alignment)
        {
            _cache.Clear();
            TextBounds bounds = _cache.AddMultiLineText(str, x, y, alignmentWidth, alignment);
            _cache.Draw(spriteBatch);
            return bounds;
        }

        public TextBounds DrawWrapped (GdxSpriteBatch spriteBatch, string str, float x, float y, float wrapWidth)
        {
            _cache.Clear();
            TextBounds bounds = _cache.AddWrappedText(str, x, y, wrapWidth, HAlignment.Left);
            _cache.Draw(spriteBatch);
            return bounds;
        }

        public TextBounds DrawWrapped (GdxSpriteBatch spriteBatch, string str, float x, float y, float wrapWidth, HAlignment alignment)
        {
            _cache.Clear();
            TextBounds bounds = _cache.AddWrappedText(str, x, y, wrapWidth, alignment);
            _cache.Draw(spriteBatch);
            return bounds;
        }

        public TextBounds GetBounds (string str)
        {
            return GetBounds(str, 0, str.Length);
        }

        public TextBounds GetBounds (string str, int start, int end)
        {
            BitmapFontData data = Data;
            int width = 0;
            Glyph lastGlyph = null;

            while (start < end) {
                lastGlyph = data[str[start++]];
                if (lastGlyph != null) {
                    width = lastGlyph.XAdvance;
                    break;
                }
            }

            while (start < end) {
                char ch = str[start++];
                Glyph g = data[ch];
                if (g != null) {
                    width += lastGlyph.GetKerning(ch);
                    lastGlyph = g;
                    width += g.XAdvance;
                }
            }

            return new TextBounds() {
                Width = width * data.ScaleX,
                Height = data.CapHeight,
            };
        }

        public TextBounds GetMultiLineBounds (string str)
        {
            int start = 0;
            float maxWidth = 0;
            int numLines = 0;
            int length = str.Length;

            while (start < length) {
                int lineEnd = str.IndexOf('\n', start);
                float lineWidth = GetBounds(str, start, lineEnd).Width;
                maxWidth = Math.Max(maxWidth, lineWidth);
                start = lineEnd + 1;
                numLines++;
            }

            return new TextBounds() {
                Width = maxWidth,
                Height = Data.CapHeight + (numLines - 1) * Data.LineHeight,
            };
        }

        public TextBounds GetWrappedBounds (string str, float wrapWidth)
        {
            if (wrapWidth <= 0)
                wrapWidth = int.MaxValue;

            float down = Data.Down;
            int start = 0;
            int numLines = 0;
            int length = str.Length;
            float maxWidth =0;

            while (start < length) {
                int newLine = str.IndexOf('\n', start);
                while (start < newLine) {
                    if (!IsWhitespace(str[start]))
                        break;
                    start++;
                }

                int lineEnd = start + ComputeVisibleGlyphs(str, start, newLine, wrapWidth);
                int nextStart = lineEnd + 1;

                if (lineEnd < newLine) {
                    while (lineEnd > start) {
                        if (IsWhitespace(str[lineEnd]))
                            break;
                        lineEnd--;
                    }

                    if (lineEnd == start) {
                        if (nextStart > start + 1)
                            nextStart--;
                        lineEnd = nextStart;
                    }
                    else {
                        nextStart = lineEnd;
                        while (lineEnd > start) {
                            if (IsWhitespace(str[lineEnd - 1]))
                                break;
                            lineEnd--;
                        }
                    }
                }

                if (lineEnd > start) {
                    float lineWidth = GetBounds(str, start, lineEnd).Width;
                    maxWidth = Math.Max(maxWidth, lineWidth);
                }

                start = nextStart;
                numLines++;
            }

            return new TextBounds() {
                Width = maxWidth,
                Height = Data.CapHeight + (numLines - 1) * Data.LineHeight,
            };
        }

        public void ComputeGlyphAdvancesAndPositions (string str, IList<float> glyphAdvances, IList<float> glyphPositions)
        {
            glyphAdvances.Clear();
            glyphPositions.Clear();

            int index = 0;
            int end = str.Length;
            float width = 0;
            Glyph lastGlyph = null;
            BitmapFontData data = Data;

            if (data.ScaleX == 1) {
                for (; index < end; index++) {
                    char ch = str[index];
                    Glyph g = data[ch];

                    if (g != null) {
                        if (lastGlyph != null)
                            width += lastGlyph.GetKerning(ch);

                        lastGlyph = g;
                        glyphAdvances.Add(g.XAdvance);
                        glyphPositions.Add(width);
                        width += g.XAdvance;
                    }
                }

                glyphAdvances.Add(0);
                glyphPositions.Add(width);
            }
            else {
                float scaleX = data.ScaleX;
                for (; index < end; index++) {
                    char ch = str[index];
                    Glyph g = data[ch];

                    if (g != null) {
                        if (lastGlyph != null)
                            width += lastGlyph.GetKerning(ch) * scaleX;

                        lastGlyph = g;
                        float xAdvance = g.XAdvance * scaleX;
                        glyphAdvances.Add(xAdvance);
                        glyphPositions.Add(width);
                        width += xAdvance;
                    }
                }

                glyphAdvances.Add(0);
                glyphPositions.Add(width);
            }
        }

        public int ComputeVisibleGlyphs (string str, int start, int end, float availableWidth)
        {
            BitmapFontData data = Data;
            int index = start;
            float width = 0;
            Glyph lastGlyph = null;
            availableWidth /= data.ScaleX;

            for (; index < end; index++) {
                char ch = str[index];
                Glyph g = data[ch];

                if (g != null) {
                    if (lastGlyph != null)
                        width += lastGlyph.GetKerning(ch);
                    if ((width + g.XAdvance) - availableWidth > .001f)
                        break;

                    width += g.XAdvance;
                    lastGlyph = g;
                }
            }

            return index = start;
        }

        public Color Color
        {
            get { return _cache.Color; }
            set { _cache.Color = value; }
        }

        public void SetScale (float scaleX, float scaleY)
        {
            BitmapFontData data = Data;
            float x = scaleX / data.ScaleX;
            float y = scaleY / data.ScaleY;

            data.LineHeight = data.LineHeight * y;
            data.SpaceWidth = data.SpaceWidth * x;
            data.XHeight = data.XHeight * y;
            data.CapHeight = data.CapHeight * y;
            data.Ascent = data.Ascent * y;
            data.Descent = data.Descent * y;
            data.Down = data.Down * y;
            data.ScaleX = scaleX;
            data.ScaleY = scaleY;
        }

        public void SetScale (float scaleXY)
        {
            SetScale(scaleXY, scaleXY);
        }

        public void Scale (float amount)
        {
            SetScale(Data.ScaleX + amount, Data.ScaleY + amount);
        }

        public float ScaleX
        {
            get { return Data.ScaleX; }
        }

        public float ScaleY
        {
            get { return Data.ScaleY; }
        }

        public TextureRegion Region
        {
            get { return _region; }
        }

        public float LineHeight
        {
            get { return Data.LineHeight; }
        }

        public float SpaceWidth
        {
            get { return Data.SpaceWidth; }
        }

        public float XHeight
        {
            get { return Data.XHeight; }
        }

        public float CapHeight
        {
            get { return Data.CapHeight; }
        }

        public float Ascent
        {
            get { return Data.Ascent; }
        }

        public float Descent
        {
            get { return Data.Descent; }
        }

        public bool IsFlipped
        {
            get { return _flipped; }
        }

        public void Dispose ()
        {
            if (OwnsTexture)
                _region.Texture.Dispose();
        }

        public void SetFixedWidthGlyphs (string glyphs)
        {
            BitmapFontData data = Data;
            int maxAdvance = 0;

            for (int index = 0, end = glyphs.Length; index < end; index++) {
                Glyph g = data[glyphs[index]];
                if (g != null && g.XAdvance > maxAdvance)
                    maxAdvance = g.XAdvance;
            }

            for (int index = 0, end = glyphs.Length; index < end; index++) {
                Glyph g = data[glyphs[index]];
                if (g == null)
                    continue;

                g.XOffset += (maxAdvance - g.XAdvance) / 2;
                g.XAdvance = maxAdvance;
                g.Kerning = null;
            }
        }

        public bool ContainsCharacter (char character)
        {
            return Data[character] != null;
        }

        public bool UsesIntegerPostions
        {
            get { return _integer; }

            set
            {
                _integer = value;
                _cache.UsesIntegerPositions = value;
            }
        }

        public BitmapFontData Data
        {
            get { return _data; }
        }

        public bool OwnsTexture { get; set; }

        internal static bool IsWhitespace (char ch)
        {
            switch (ch) {
                case '\n':
                case '\r':
                case '\t':
                case ' ':
                    return true;
                default:
                    return false;
            }
        }
    }

    public class Glyph
    {
        public int SrcX;
        public int SrcY;
        public int Width;
        public int Height;
        public float U;
        public float V;
        public float U2;
        public float V2;
        public int XOffset;
        public int YOffset;
        public int XAdvance;
        public byte[][] Kerning;

        public int GetKerning (char ch)
        {
            if (Kerning != null) {
                byte[] page = Kerning[ch >> BitmapFont.Log2PageSize];
                if (page != null)
                    return page[ch & BitmapFont.PageSize - 1];
            }
            return 0;
        }

        public void SetKerning (char ch, int value)
        {
            if (Kerning == null)
                Kerning = new byte[BitmapFont.Pages][];
            byte[] page = Kerning[ch >> BitmapFont.Log2PageSize];
            if (page == null)
                Kerning[ch >> BitmapFont.Log2PageSize] = page = new byte[BitmapFont.PageSize];
            page[ch & BitmapFont.Log2PageSize - 1] = (byte)value;
        }
    }

    public struct TextBounds
    {
        public float Width;
        public float Height;
    }

    public enum HAlignment
    {
        Left,
        Center,
        Right,
    }

    public class BitmapFontData
    {
        private static readonly char[] SpaceToken = { ' ' };
        private static readonly char[] SpaceEqToken = { ' ', '=' };

        public BitmapFontData ()
        {
            CapHeight = 1;
            ScaleX = 1;
            ScaleY = 1;
            XHeight = 1;
            Glyphs = new Glyph[BitmapFont.Pages][];
        }

        public BitmapFontData (string fontFile, bool flip)
            : this()
        {
            FontFile = fontFile;
            IsFlipped = flip;

            using (TextReader reader = new StreamReader(FontFile)) {
                reader.ReadLine(); // Info

                string line = reader.ReadLine();
                if (line == null)
                    throw new IOException("Invalid font file: " + fontFile);

                string[] common = line.Split(SpaceToken, 4);
                if (common.Length < 4)
                    throw new IOException("Invalid font file: " + fontFile);

                if (!common[1].StartsWith("lineHeight="))
                    throw new IOException("Invalid font file: " + fontFile);
                LineHeight = int.Parse(common[1].Substring(11));

                if (!common[2].StartsWith("base="))
                    throw new IOException("Invalid font file: " + fontFile);
                int baseLine = int.Parse(common[2].Substring(5));

                line = reader.ReadLine();
                if (line == null)
                    throw new IOException("Invalid font file: " + fontFile);

                string[] pageLine = line.Split(SpaceToken, 4);
                if (pageLine.Length < 4)
                    throw new IOException("Invalid font file: " + fontFile);

                if (!pageLine[2].StartsWith("file="))
                    throw new IOException("Invalid font file: " + fontFile);
                string imageFilename = null;
                if (pageLine[2].EndsWith("\""))
                    imageFilename = pageLine[2].Substring(6, pageLine[2].Length - 1);
                else
                    imageFilename = pageLine[2].Substring(5, pageLine[2].Length);

                ImagePath = Path.Combine(Path.GetDirectoryName(fontFile), imageFilename);
                Descent = 0;

                while (true) {
                    line = reader.ReadLine();
                    if (line == null)
                        break;
                    if (line.StartsWith("kernings "))
                        break;
                    if (!line.StartsWith("char "))
                        continue;

                    Glyph glyph = new Glyph();

                    string[] tokens = line.Split(SpaceEqToken, StringSplitOptions.RemoveEmptyEntries);
                    char ch = char.Parse(tokens[2]);
                    this[ch] = glyph;

                    glyph.SrcX = int.Parse(tokens[4]);
                    glyph.SrcY = int.Parse(tokens[6]);
                    glyph.Width = int.Parse(tokens[8]);
                    glyph.Height = int.Parse(tokens[10]);
                    glyph.XOffset = int.Parse(tokens[12]);
                    glyph.YOffset = flip ? int.Parse(tokens[14]) : -(glyph.Height + int.Parse(tokens[14]));
                    glyph.XAdvance = int.Parse(tokens[16]);

                    if (glyph.Width > 0 && glyph.Height > 0)
                        Descent = Math.Min(baseLine + glyph.YOffset, Descent);
                }

                while (true) {
                    line = reader.ReadLine();
                    if (line == null)
                        break;
                    if (!line.StartsWith("kerning "))
                        break;

                    string[] tokens = line.Split(SpaceEqToken, StringSplitOptions.RemoveEmptyEntries);

                    int first = int.Parse(tokens[2]);
                    int second = int.Parse(tokens[4]);

                    if (first < 0 || first > char.MaxValue || second < 0 || second > char.MaxValue)
                        continue;
                    Glyph g = this[(char)first];

                    int amount = int.Parse(tokens[6]);
                    g.SetKerning((char)second, amount);
                }

                Glyph spaceGlyph = this[' '];
                if (spaceGlyph == null) {
                    Glyph xAdvanceGlyph = this['l'];
                    if (xAdvanceGlyph == null)
                        xAdvanceGlyph = FirstGlyph;

                    this[' '] = spaceGlyph = new Glyph() {
                        XAdvance = xAdvanceGlyph.XAdvance,
                    };
                }
                SpaceWidth = (spaceGlyph != null) ? spaceGlyph.XAdvance + spaceGlyph.Width : 1;

                Glyph xGlyph = null;
                for (int i = 0; i < BitmapFont.XChars.Length; i++) {
                    xGlyph = this[BitmapFont.XChars[i]];
                    if (xGlyph != null)
                        break;
                }
                if (xGlyph == null)
                    xGlyph = FirstGlyph;
                XHeight = xGlyph.Height;

                Glyph capGlyph = null;
                for (int i = 0; i < BitmapFont.CapChars.Length; i++) {
                    capGlyph = this[BitmapFont.CapChars[i]];
                    if (capGlyph != null)
                        break;
                }

                if (capGlyph == null) {
                    foreach (Glyph[] page in Glyphs) {
                        if (page == null)
                            continue;

                        foreach (Glyph glyph in page) {
                            if (glyph == null || glyph.Height == 0 || glyph.Width == 0)
                                continue;
                            CapHeight = Math.Min(CapHeight, glyph.Height);
                        }
                    }
                }
                else
                    CapHeight = capGlyph.Height;

                Ascent = baseLine - CapHeight;
                Down = -LineHeight;

                if (flip) {
                    Ascent = -Ascent;
                    Down = -Down;
                }
            }
        }

        public Glyph this[char ch]
        {
            get
            {
                Glyph[] page = Glyphs[ch / BitmapFont.PageSize];
                if (page == null)
                    return page[ch & BitmapFont.PageSize - 1];
                return null;
            }

            set
            {
                Glyph[] page = Glyphs[ch / BitmapFont.PageSize];
                if (page == null)
                    Glyphs[ch / BitmapFont.PageSize] = page = new Glyph[BitmapFont.PageSize];
                page[ch & BitmapFont.PageSize - 1] = value;
            }
        }

        public Glyph FirstGlyph
        {
            get
            {
                foreach (Glyph[] page in Glyphs) {
                    if (page == null)
                        continue;

                    foreach (Glyph glyph in page) {
                        if (glyph == null || glyph.Height == 0 || glyph.Width == 0)
                            continue;
                        return glyph;
                    }
                }

                throw new Exception("No glyphs found!");
            }
        }

        public string ImagePath { get; set; }
        public string FontFile { get; set; }
        public bool IsFlipped { get; set; }
        public float LineHeight { get; set; }
        public float CapHeight { get; set; }
        public float Ascent { get; set; }
        public float Descent { get; set; }
        public float Down { get; set; }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }

        public Glyph[][] Glyphs { get; private set; }
        public float SpaceWidth { get; set; }
        public float XHeight { get; set; }
    }
}
