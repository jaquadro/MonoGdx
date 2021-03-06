﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGdx.Geometry;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.UI
{
    [TODO]
    public class TextField : Widget
    {
        public static RoutedEvent SelectionChangedEvent = 
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextField));
        public static RoutedEvent TextChangedEvent =
            EventManager.RegisterRoutedEvent(RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextField));

        private const char CharBackspace = (char)8;
        private const char CharEnterDesktop = '\r';
        private const char CharEnterAndroid = '\n';
        private const char CharTab = '\t';
        private const char CharDelete = (char)127;
        private const char CharBullet = (char)149;

        private TextFieldStyle _style;
        private string _text;
        private string _displayText;
        private int _cursor;

        private bool _passwordMode;
        private StringBuilder _passwordBuffer;

        private TextBounds _textBounds;
        private float _renderOffset;
        private float _textOffset;
        private int _visibleTextStart;
        private int _visibleTextEnd;
        private List<float> _glyphAdvances = new List<float>();
        private List<float> _glyphPositions = new List<float>();

        private bool _cursorOn = true;
        private long _lastBlink;

        private bool _hasSelection;
        private int _selectionStart;
        private float _selectionX;
        private float _selectionWidth;

        private char _passwordCharacter = CharBullet;

        private KeyRepeatTask _keyRepeatTask = new KeyRepeatTask();
        private float _keyRepeatInitialTime = .4f;
        private float _keyRepeatTime = .1f;

        private ClickEventManager _clickManager;

        static TextField ()
        {
            EventManager.RegisterClassHandler(typeof(TextField), Stage.PreviewKeyTypedEvent, new KeyCharEventHandler(PreviewKeyTypedClass));
            EventManager.RegisterClassHandler(typeof(TextField), Stage.KeyTypedEvent, new KeyCharEventHandler(KeyTypedClass));
        }

        public TextField (string text, Skin skin)
            : this(text, skin.Get<TextFieldStyle>())
        { }

        public TextField (string text, Skin skin, string styleName)
            : this(text, skin.Get<TextFieldStyle>(styleName))
        { }

        [TODO("Clipboard")]
        public TextField (string text, TextFieldStyle style)
        {
            FocusTraversal = true;
            OnlyFontChars = true;
            OnscreenKeyboard = new DefaultOnscreenKeyboard();
            BlinkTime = .32f;

            Style = style;
            //Clipboard = App.Clipboard;
            Text = text;
            Width = PrefWidth;
            Height = PrefHeight;
            Initialize();
        }

        private void Initialize ()
        {
            _clickManager = new ClickEventManager(this);
            _clickManager.ClickHandler += e => {
                if (_clickManager.TapCount > 1)
                    SetSelection(0, Text.Length);
            };
        }

        protected override void OnTouchDown (TouchEventArgs e)
        {
            do {
                if (e.Pointer == 0 && e.Button != 0)
                    break;
                if (IsDisabled) {
                    e.Handled = true;
                    break;
                }

                Vector2 position = e.GetPosition(this);

                ClearSelection();
                SetCursorPosition(position.X);
                _selectionStart = _cursor;

                Stage stage = Stage;
                if (stage != null)
                    stage.SetKeyboardFocus(this);

                OnscreenKeyboard.Show(true);
                //e.Handled = true;
            } while (false);
                
            base.OnTouchDown(e);
        }

        protected override void OnTouchDrag (TouchEventArgs e)
        {
            Vector2 position = e.GetPosition(this);

            _lastBlink = 0;
            _cursorOn = false;
            SetCursorPosition(position.X);
            _hasSelection = true;

            base.OnTouchDrag(e);
        }

        private void SetCursorPosition (float x)
        {
            _lastBlink = 0;
            _cursorOn = false;

            x -= _renderOffset + _textOffset;
            for (int i = 0; i < _glyphPositions.Count; i++) {
                if (_glyphPositions[i] > x) {
                    _cursor = Math.Max(0, i - 1);
                    return;
                }
            }

            _cursor = Math.Max(0, _glyphPositions.Count - 1);
        }

        [TODO("KeyRepeatTask")]
        protected override void OnKeyDown (KeyEventArgs e)
        {
            if (IsDisabled) {
                base.OnKeyDown(e);
                return;
            }

            BitmapFont font = _style.Font;

            _lastBlink = 0;
            _cursorOn = false;

            Stage stage = Stage;
            if (stage != null && stage.GetKeyboardFocus() == this) {
                KeyboardState keyboard = Keyboard.GetState();
                Keys key = (Keys)e.KeyCode;

                bool repeat = false;
                bool ctrl = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
                bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

                if (ctrl) {
                    e.Handled = true;

                    switch (key) {
                        case Keys.V:
                            Paste();
                            break;
                        case Keys.C:
                        case Keys.Insert:
                            Copy();
                            break;
                        case Keys.X:
                        case Keys.Back:
                            Cut();
                            break;
                        case Keys.A:
                            SelectAll();
                            break;
                        default:
                            e.Handled = false;
                            break;
                    }

                    if (e.Handled) {
                        base.OnKeyDown(e);
                        return;
                    }
                }

                if (shift) {
                    bool hasSelection = _hasSelection;
                    int cursor = _cursor;

                    switch (key) {
                        case Keys.Insert:
                            Paste();
                            break;
                        case Keys.Delete:
                            if (_hasSelection) {
                                Copy();
                                Delete();
                            }
                            break;
                        case Keys.Left:
                            if (!_hasSelection) {
                                _selectionStart = _cursor;
                                _hasSelection = true;
                            }
                            while (--_cursor > 0 && ctrl) {
                                char c = _text[_cursor];
                                if (c >= 'A' && c <= 'Z')
                                    continue;
                                if (c >= 'a' && c <= 'z')
                                    continue;
                                if (c >= '0' && c <= '9')
                                    continue;
                                break;
                            }
                            repeat = true;
                            break;
                        case Keys.Right:
                            if (!_hasSelection) {
                                _selectionStart = _cursor;
                                _hasSelection = true;
                            }
                            while (++_cursor < _text.Length && ctrl) {
                                char c = _text[_cursor - 1];
                                if (c >= 'A' && c <= 'Z')
                                    continue;
                                if (c >= 'a' && c <= 'z')
                                    continue;
                                if (c >= '0' && c <= '9')
                                    continue;
                                break;
                            }
                            repeat = true;
                            break;
                        case Keys.Home:
                            if (!_hasSelection) {
                                _selectionStart = _cursor;
                                _hasSelection = true;
                            }
                            _cursor = 0;
                            break;
                        case Keys.End:
                            if (!_hasSelection) {
                                _selectionStart = _cursor;
                                _hasSelection = true;
                            }
                            _cursor = _text.Length;
                            break;
                    }

                    _cursor = Math.Max(0, _cursor);
                    _cursor = Math.Min(_text.Length, _cursor);

                    if (hasSelection != _hasSelection || cursor != _cursor)
                        OnSelectionChanged();
                }
                else {
                    switch (key) {
                        case Keys.Left:
                            while (_cursor-- > 1 && ctrl) {
                                char c = _text[_cursor - 1];
                                if (c >= 'A' && c <= 'Z')
                                    continue;
                                if (c >= 'a' && c <= 'z')
                                    continue;
                                if (c >= '0' && c <= '9')
                                    continue;
                                break;
                            }
                            ClearSelection();
                            repeat = true;
                            break;
                        case Keys.Right:
                            while (++_cursor < _text.Length && ctrl) {
                                char c = _text[_cursor - 1];
                                if (c >= 'A' && c <= 'Z')
                                    continue;
                                if (c >= 'a' && c <= 'z')
                                    continue;
                                if (c >= '0' && c <= '9')
                                    continue;
                                break;
                            }
                            ClearSelection();
                            repeat = true;
                            break;
                        case Keys.Home:
                            _cursor = 0;
                            ClearSelection();
                            break;
                        case Keys.End:
                            _cursor = _text.Length;
                            ClearSelection();
                            break;
                    }

                    _cursor = Math.Max(0, _cursor);
                    _cursor = Math.Min(_text.Length, _cursor);
                }

                //if (repeat && (!_keyRepeatTask.IsScheduled() || _keyRepeatTask.Keycode != keycode)) {
                //    _keyRepeatTask.Keycode = keycode;
                //    _keyRepeatTask.Cancel();
                //    Timer.Schedule(_keyRepeatTask, _keyRepeatInitialTime, _keyRepeatTime);
                //}

                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        [TODO]
        protected override void OnKeyUp (KeyEventArgs e)
        {
            if (IsDisabled) {
                base.OnKeyUp(e);
                return;
            }

            //_self._keyRepeatTask.Cancel();
            e.Handled = true;

            base.OnKeyUp(e);
        }

        private static void PreviewKeyTypedClass (Actor sender, KeyCharEventArgs e)
        {
            TextField field = sender as TextField;
            if (field != null)
                field.OnPreviewKeyTyped(e);
        }

        private static void KeyTypedClass (Actor sender, KeyCharEventArgs e)
        {
            TextField field = sender as TextField;
            if (field != null)
                field.OnKeyTyped(e);
        }

        protected virtual void OnPreviewKeyTyped (KeyCharEventArgs e)
        { }

        protected virtual void OnKeyTyped (KeyCharEventArgs e)
        {
            if (IsDisabled)
                return;

            BitmapFont font = _style.Font;

            Stage stage = Stage;
            if (stage != null && stage.GetKeyboardFocus() == this) {
                if (e.Character == CharBackspace) {
                    if (_cursor > 0 || _hasSelection) {
                        if (!_hasSelection) {
                            _text = _text.Substring(0, _cursor - 1) + _text.Substring(_cursor);
                            UpdateDisplayText();
                            _cursor--;
                            _renderOffset = 0;
                            OnTextChanged();
                        }
                        else
                            Delete();
                    }
                }
                else if (e.Character == CharDelete) {
                    if (_cursor < _text.Length || _hasSelection) {
                        if (!_hasSelection) {
                            _text = _text.Substring(0, _cursor) + _text.Substring(_cursor + 1);
                            UpdateDisplayText();
                            OnTextChanged();
                        }
                        else
                            Delete();
                    }
                }
                else if ((e.Character == CharTab || e.Character == CharEnterAndroid) && FocusTraversal) {
                    KeyboardState keyboard = Keyboard.GetState();
                    Next(keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift));
                }
                else if (font.ContainsCharacter(e.Character)) {
                    if (e.Character != CharEnterDesktop && e.Character != CharEnterAndroid) {
                        if (TextFieldFilter != null && !TextFieldFilter.AcceptChar(this, e.Character)) {
                            e.Handled = true;
                            return;
                        }
                    }

                    if (MaxLength > 0 && _text.Length + 1 > MaxLength) {
                        e.Handled = true;
                        return;
                    }

                    if (!_hasSelection) {
                        _text = _text.Substring(0, _cursor) + e.Character
                            + _text.Substring(_cursor, _text.Length - _cursor);
                        UpdateDisplayText();
                        _cursor++;
                        OnTextChanged();
                    }
                    else {
                        int minIndex = Math.Min(_cursor, _selectionStart);
                        int maxIndex = Math.Max(_cursor, _selectionStart);

                        _text = (minIndex > 0 ? _text.Substring(0, minIndex) : "")
                            + (maxIndex < _text.Length ? _text.Substring(maxIndex, _text.Length - maxIndex) : "");
                        _cursor = minIndex;
                        _text = _text.Substring(0, _cursor) + e.Character
                            + _text.Substring(_cursor, _text.Length - _cursor);
                        OnTextChanged();

                        UpdateDisplayText();
                        _cursor++;
                        ClearSelection();
                    }
                }

                e.Handled = true;
            }
        }
        public int MaxLength { get; set; }

        public bool OnlyFontChars { get; set; }

        public TextFieldStyle Style
        {
            get { return _style; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("style");
                _style = value;
                InvalidateHierarchy();
            }
        }

        public char PasswordCharacter
        {
            get { return _passwordCharacter; }
            set
            {
                _passwordCharacter = value;
                if (_passwordMode)
                    UpdateDisplayText();
            }
        }

        private void CalculateOffsets ()
        {
            float visibleWidth = Width;
            if (_style.Background != null)
                visibleWidth -= _style.Background.LeftWidth + _style.Background.RightWidth;

            // Check if the cursor has gone out the left or right side of the visible area and adjust renderoffset.
            float position = _glyphPositions[_cursor];
            float distance = position - Math.Abs(_renderOffset);
            if (distance <= 0) {
                if (_cursor > 0)
                    _renderOffset = -_glyphPositions[_cursor - 1];
                else
                    _renderOffset = 0;
            }
            else if (distance > visibleWidth)
                _renderOffset -= distance - visibleWidth;

            // calculate first visible char based on render offset
            _visibleTextStart = 0;
            _textOffset = 0;

            float start = Math.Abs(_renderOffset);
            int len = _glyphPositions.Count;
            float startPos = 0;

            for (int i = 0; i < len; i++) {
                if (_glyphPositions[i] >= start) {
                    _visibleTextStart = i;
                    startPos = _glyphPositions[i];
                    _textOffset = startPos - start;
                    break;
                }
            }

            // calculate last visible char based on visible width and render offset
            _visibleTextEnd = Math.Min(_displayText.Length, _cursor + 1);
            for (; _visibleTextEnd <= _displayText.Length; _visibleTextEnd++) {
                if (_glyphPositions[_visibleTextEnd] - startPos > visibleWidth)
                    break;
            }
            _visibleTextEnd = Math.Max(0, _visibleTextEnd - 1);

            // calculate selection x position and width
            if (_hasSelection) {
                int minIndex = Math.Min(_cursor, _selectionStart);
                int maxIndex = Math.Max(_cursor, _selectionStart);
                float minX = Math.Max(_glyphPositions[minIndex], startPos);
                float maxX = Math.Min(_glyphPositions[maxIndex], _glyphPositions[_visibleTextEnd]);

                _selectionX = minX;
                _selectionWidth = maxX - minX;
            }

            if (IsRightAligned) {
                _textOffset = visibleWidth - (_glyphPositions[_visibleTextEnd] - startPos);
                if (_hasSelection)
                    _selectionX += _textOffset;
            }
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            bool focused = IsKeyboardFocused;

            BitmapFont font = _style.Font;
            Color? fontColor = (IsDisabled) 
                ? _style.DisabledFontColor ?? _style.FontColor
                : (focused) ? _style.FocusedFontColor ?? _style.FontColor : _style.FontColor;
            ISceneDrawable selection = _style.Selection;
            ISceneDrawable cursorPatch = _style.Cursor;
            ISceneDrawable background = (IsDisabled)
                ? _style.DisabledBackground ?? _style.Background
                : (focused) ? _style.FocusedBackground ?? _style.Background : _style.Background;

            float x = (int)X;
            float y = (int)Y;
            float width = Width;
            float height = Height;
            float textY = _textBounds.Height / 2 + font.Descent;

            spriteBatch.Color = Color.MultiplyAlpha(parentAlpha);
            float bgLeftWidth = 0;

            if (background != null) {
                background.Draw(spriteBatch, x, y, width, height);
                bgLeftWidth = background.LeftWidth;
                float bottom = background.BottomHeight;
                textY = (int)(textY + (height - background.TopHeight - bottom) / 2 + bottom);
            }
            else
                textY = (int)(textY + height / 2);

            CalculateOffsets();

            if (focused && _hasSelection && selection != null)
                selection.Draw(spriteBatch, x + _selectionX + bgLeftWidth + _renderOffset, y + textY - _textBounds.Height - font.Descent,
                    _selectionWidth, _textBounds.Height + font.Descent / 2);

            float yOffset = font.IsFlipped ? -_textBounds.Height : 0;
            if (_displayText.Length == 0) {
                if (!focused && MessageText != null) {
                    if (_style.MessageFontColor != null)
                        font.Color = _style.MessageFontColor.Value.MultiplyAlpha(parentAlpha);
                    else
                        font.Color = new Color(.7f, .7f, .7f, parentAlpha);

                    BitmapFont messageFont = _style.MessageFont ?? font;
                    messageFont.Draw(spriteBatch, MessageText, x + bgLeftWidth, y + textY + yOffset);
                }
            }
            else {
                font.Color = fontColor.Value.MultiplyAlpha(parentAlpha);
                font.Draw(spriteBatch, _displayText, x + bgLeftWidth + _textOffset, y + textY + yOffset, _visibleTextStart, _visibleTextEnd);
            }

            if (focused && !IsDisabled) {
                Blink();
                if (_cursorOn && cursorPatch != null)
                    cursorPatch.Draw(spriteBatch, x + bgLeftWidth + _textOffset + _glyphPositions[_cursor] - _glyphPositions[_visibleTextStart] - 1,
                        y + textY - _textBounds.Height - font.Descent,
                        cursorPatch.MinWidth, _textBounds.Height + font.Descent / 2);
            }
        }
        

        private void UpdateDisplayText ()
        {
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < _text.Length; i++) {
                char c = _text[i];
                buffer.Append(_style.Font.ContainsCharacter(c) ? c : ' ');
            }
            String text = buffer.ToString();

            if (_passwordMode && _style.Font.ContainsCharacter(_passwordCharacter)) {
                if (_passwordBuffer == null)
                    _passwordBuffer = new StringBuilder(text.Length);
                if (_passwordBuffer.Length > text.Length)
                    _passwordBuffer.Length = text.Length;
                else {
                    for (int i = _passwordBuffer.Length, n = text.Length; i < n; i++)
                        _passwordBuffer.Append(_passwordCharacter);
                }
                _displayText = _passwordBuffer.ToString();
            }
            else
                _displayText = text;

            _style.Font.ComputeGlyphAdvancesAndPositions(_displayText, _glyphAdvances, _glyphPositions);
            if (_selectionStart > _text.Length)
                _selectionStart = _text.Length;
        }

        private void Blink ()
        {
            long time = DateTime.Now.Ticks * 100;
            if ((time - _lastBlink) / 1000000000f > BlinkTime) {
                _cursorOn = !_cursorOn;
                _lastBlink = time;
            }
        }

        public void Copy ()
        {
            if (_hasSelection && !_passwordMode && Clipboard != null) {
                int minIndex = Math.Min(_cursor, _selectionStart);
                int maxIndex = Math.Max(_cursor, _selectionStart);
                Clipboard.Contents = _text.Substring(minIndex, maxIndex - minIndex);
            }
        }

        public void Cut ()
        {
            if (_hasSelection && !_passwordMode) {
                Copy();
                Delete();
            }
        }

        public void Paste ()
        {
            if (Clipboard == null)
                return;

            string content = Clipboard.Contents;
            if (content != null) {
                StringBuilder buffer = new StringBuilder();
                for (int i = 0; i < content.Length; i++) {
                    if (MaxLength > 0 && _text.Length + buffer.Length + 1 > MaxLength)
                        break;

                    char c = content[i];
                    if (!_style.Font.ContainsCharacter(c))
                        continue;
                    if (TextFieldFilter != null && !TextFieldFilter.AcceptChar(this, c))
                        continue;
                    buffer.Append(c);
                }
                content = buffer.ToString();

                if (!_hasSelection) {
                    _text = _text.Substring(0, _cursor) + content + _text.Substring(_cursor, _text.Length - _cursor);
                    UpdateDisplayText();
                    _cursor += content.Length;
                    OnTextChanged();
                }
                else {
                    int minIndex = Math.Min(_cursor, _selectionStart);
                    int maxIndex = Math.Max(_cursor, _selectionStart);

                    _text = (minIndex > 0 ? _text.Substring(0, minIndex) : "")
                        + (maxIndex < _text.Length ? _text.Substring(maxIndex, _text.Length - maxIndex) : "");
                    _cursor = minIndex;
                    _text = _text.Substring(0, _cursor) + content + _text.Substring(_cursor, _text.Length - _cursor);
                    OnTextChanged();

                    UpdateDisplayText();
                    _cursor = minIndex + content.Length;
                    ClearSelection();
                }
            }
        }

        public void Delete ()
        {
            int minIndex = Math.Min(_cursor, _selectionStart);
            int maxIndex = Math.Max(_cursor, _selectionStart);
            if (maxIndex - minIndex == 0)
                return;

            _text = (minIndex > 0 ? _text.Substring(0, minIndex) : "")
                + (maxIndex < _text.Length ? _text.Substring(maxIndex, _text.Length - maxIndex) : "");

            UpdateDisplayText();
            _cursor = minIndex;
            ClearSelection();

            OnTextChanged();
        }

        [TODO("OnScreenKeyboard")]
        public void Next (bool up)
        {
            Stage stage = Stage;
            if (stage == null)
                return;

            Vector2 stageCoords = Parent.LocalToStageCoordinates(new Vector2(X, Y));
            TextField textField = FindNextTextField(stage.Actors, null, Vector2.Zero, stageCoords, up);

            if (textField == null) {
                if (up)
                    stageCoords = new Vector2(float.MinValue, float.MinValue);
                else
                    stageCoords = new Vector2(float.MaxValue, float.MaxValue);
                textField = FindNextTextField(Stage.Actors, null, Vector2.Zero, stageCoords, up);
            }

            if (textField != null)
                stage.SetKeyboardFocus(textField);
            else
                OnscreenKeyboard.Show(false);
        }

        private TextField FindNextTextField (IList<Actor> actors, TextField best, Vector2 bestCoords, Vector2 currentCoords, bool up)
        {
            foreach (Actor actor in actors) {
                if (actor == this)
                    continue;
                if (actors is TextField) {
                    TextField textField = actor as TextField;
                    if (textField.IsDisabled || !textField.FocusTraversal)
                        continue;

                    Vector2 actorCoords = actor.Parent.LocalToStageCoordinates(new Vector2(actor.X, actor.Y));
                    if ((actorCoords.Y < currentCoords.Y || (actorCoords.Y == currentCoords.Y && actorCoords.X > currentCoords.X)) ^ up) {
                        if (best == null || (actorCoords.Y > bestCoords.Y || (actorCoords.Y == bestCoords.Y && actorCoords.X < bestCoords.X)) ^ up) {
                            best = textField;
                            bestCoords = actorCoords;
                        }
                    }
                }
                else if (actor is Group)
                    best = FindNextTextField((actor as Group).Children, best, bestCoords, currentCoords, up);
            }

            return best;
        }

        //public TextFieldListener TextFieldListener { get; set; }
        public TextFieldFilter TextFieldFilter { get; set; }

        public bool FocusTraversal { get; set; }

        public string MessageText { get; set; }

        public string Text
        {
            get { return _text; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Text");

                BitmapFont font = _style.Font;

                StringBuilder buffer = new StringBuilder();
                for (int i = 0; i < value.Length; i++) {
                    if (MaxLength > 0 && buffer.Length + 1 > MaxLength)
                        break;

                    char c = value[i];
                    if (OnlyFontChars && !font.ContainsCharacter(c))
                        continue;
                    if (TextFieldFilter != null && !TextFieldFilter.AcceptChar(this, c))
                        continue;
                    buffer.Append(c);
                }

                string bufferText = buffer.ToString();
                if (Text == bufferText)
                    return;

                _text = bufferText;
                UpdateDisplayText();
                _cursor = 0;
                ClearSelection();

                _textBounds = font.GetBounds(_displayText);
                _textBounds.Height -= font.Descent * 2;
                font.ComputeGlyphAdvancesAndPositions(_displayText, _glyphAdvances, _glyphPositions);

                OnTextChanged();
            }
        }

        public void SetSelection (int selectionStart, int selectionEnd)
        {
            if (selectionStart < 0)
                throw new ArgumentOutOfRangeException("selectionStart", "selectionStart must be >= 0");
            if (selectionEnd < 0)
                throw new ArgumentOutOfRangeException("selectionEnd", "selectionEnd must be >= 0");

            selectionStart = Math.Min(_text.Length, selectionStart);
            selectionEnd = Math.Min(_text.Length, selectionEnd);

            if (selectionStart == selectionEnd) {
                ClearSelection();
                return;
            }

            if (selectionEnd < selectionStart) {
                int temp = selectionEnd;
                selectionEnd = selectionStart;
                selectionStart = temp;
            }

            _hasSelection = true;
            _selectionStart = selectionStart;
            _cursor = selectionEnd;

            OnSelectionChanged();
        }

        public void SelectAll ()
        {
            SetSelection(0, _text.Length);
        }

        public void ClearSelection ()
        {
            if (_hasSelection) {
                _hasSelection = false;
                OnSelectionChanged();
            }
        }

        public int CursorPosition
        {
            get { return _cursor; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("CursorPosition", "CursorPosition must be >= 0");

                ClearSelection();
                _cursor = Math.Min(value, _text.Length);
            }
        }

        public OnscreenKeyboard OnscreenKeyboard { get; set; }

        public IClipboard Clipboard { get; set; }

        public override float PrefWidth
        {
            get { return 150; }
        }

        public override float PrefHeight
        {
            get
            {
                float prefHeight = _textBounds.Height;
                if (_style.Background != null)
                    prefHeight = Math.Max(prefHeight + _style.Background.BottomHeight + _style.Background.TopHeight, _style.Background.MinHeight);
                return prefHeight;
            }
        }

        public bool IsRightAligned { get; set; }

        public bool IsPasswordMode
        {
            get { return _passwordMode; }
            set
            {
                _passwordMode = value;
                UpdateDisplayText();
            }
        }

        public float BlinkTime { get; set; }

        public bool IsDisabled { get; set; }

        public event RoutedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        public event RoutedEventHandler TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }

        protected virtual bool OnSelectionChanged ()
        {
            RoutedEventArgs args = InitializeEventArgs(Pools<RoutedEventArgs>.Obtain(), SelectionChangedEvent);
            bool cancel = RaiseEvent(args);
            Pools<RoutedEventArgs>.Release(args);
            return cancel;
        }

        protected virtual bool OnTextChanged ()
        {
            RoutedEventArgs args = InitializeEventArgs(Pools<RoutedEventArgs>.Obtain(), TextChangedEvent);
            bool cancel = RaiseEvent(args);
            Pools<RoutedEventArgs>.Release(args);
            return cancel;
        }
    }

    [TODO("Task")]
    public class KeyRepeatTask
    {

    }

    /*public interface TextFieldListener
    {
        void KeyTyped (TextField textField, char key);
    }*/

    public interface TextFieldFilter
    {
        bool AcceptChar (TextField textField, char key);
    }

    public class DigitsOnlyFilter : TextFieldFilter
    {
        public bool AcceptChar (TextField textField, char key)
        {
            return char.IsDigit(key);
        }
    }

    public interface OnscreenKeyboard
    {
        void Show (bool visible);
    }

    [TODO("Delegate to system back-end")]
    public class DefaultOnscreenKeyboard : OnscreenKeyboard
    {
        public void Show (bool visible)
        { }
    }

    public class TextFieldStyle
    {
        public TextFieldStyle ()
        { }

        public TextFieldStyle (BitmapFont font, Color? fontColor, ISceneDrawable cursor, ISceneDrawable selection, ISceneDrawable background)
        {
            Background = background;
            Cursor = cursor;
            Font = font;
            FontColor = fontColor;
            Selection = selection;
        }

        public TextFieldStyle (TextFieldStyle style)
        {
            MessageFont = style.MessageFont;
            MessageFontColor = style.MessageFontColor;
            Background = style.Background;
            FocusedBackground = style.FocusedBackground;
            DisabledBackground = style.DisabledBackground;
            Cursor = style.Cursor;
            Font = style.Font;
            FontColor = style.FontColor;
            FocusedFontColor = style.FocusedFontColor;
            DisabledFontColor = style.DisabledFontColor;
            Selection = style.Selection;
        }

        public BitmapFont Font { get; set; }
        public Color? FontColor { get; set; }
        public Color? FocusedFontColor { get; set; }
        public Color? DisabledFontColor { get; set; }

        public ISceneDrawable Background { get; set; }
        public ISceneDrawable FocusedBackground { get; set; }
        public ISceneDrawable DisabledBackground { get; set; }
        public ISceneDrawable Cursor { get; set; }
        public ISceneDrawable Selection { get; set; }

        public BitmapFont MessageFont { get; set; }
        public Color? MessageFontColor { get; set; }
    }
}
