/**
 * Copyright 2011-2013 See AUTHORS file.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGdx.Geometry;
using MonoGdx.Scene2D.Actions;
using MonoGdx.Scene2D.Utils;

namespace MonoGdx.Scene2D.UI
{
    public class Dialog : Window
    {
        public static float FadeDuration = .4f;

        private Table _contentTable;
        private Table _buttonTable;
        private Skin _skin;
        private Dictionary<Actor, object> _values = new Dictionary<Actor, object>();
        private bool _cancelHide;
        private Actor _prevKeyboardFocus;
        private Actor _prevScrollFocus;

        private InputListener _ignoreTouchDown = new TouchListener() {
            Down = (ev, x, y, pointer, button) => {
                ev.Cancel();
                return false;
            },
        };

        public Dialog (string title, Skin skin)
            : base(title, skin.Get<WindowStyle>())
        {
            Skin = skin;
            Initialize();
        }

        public Dialog (string title, Skin skin, string windowStyleName)
            : base(title, skin.Get<WindowStyle>(windowStyleName))
        {
            Skin = skin;
            Initialize();
        }

        public Dialog (string title, WindowStyle windowStyle)
            : base(title, windowStyle)
        {
            Initialize();
        }

        private void Initialize ()
        {
            IsModal = true;

            Defaults().Configure.Space(6);
            Add(_contentTable = new Table(_skin)).Configure.Expand().Fill();
            Row();
            Add(_buttonTable = new Table(_skin));

            _contentTable.Defaults().Configure.Space(6);
            _buttonTable.Defaults().Configure.Space(6);

            _buttonTable.AddHandler(Button.ClickEvent, new RoutedEventHandler((actor, e) => {
                if (!_values.ContainsKey(actor))
                    return;
                while (actor.Parent != _buttonTable)
                    actor = actor.Parent;
                Result(_values[actor]);
                if (!_cancelHide)
                    Hide();
                _cancelHide = false;
            }));

            AddListener(new DispatchFocusListener() {
                OnKeyboardFocusChanged = (ev, actor, focused) => {
                    if (!focused)
                        FocusChanged(ev);
                },
                OnScrollFocusChanged = (ev, actor, focused) => {
                    if (!focused)
                        FocusChanged(ev);
                },
            });
        }

        private void FocusChanged (FocusEvent ev)
        {
            Stage stage = Stage;
            if (IsModal && stage != null && stage.Root.Children.Count > 0 && stage.Root.Children[stage.Root.Children.Count - 1] == this) {
                Actor newFocusedActor = ev.RelatedActor;
                if (newFocusedActor != null && !newFocusedActor.IsDescendentOf(this))
                    ev.Cancel();
            }
        }

        public Table ContentTable
        {
            get { return _contentTable; }
        }

        public Table ButtonTable
        {
            get { return _buttonTable; }
        }

        public void AddText (string text)
        {
            if (_skin == null)
                throw new InvalidOperationException("This method may only be used if the dialog was constructed with a Skin");
            AddText(text, Skin.Get<LabelStyle>());
        }

        public void AddText (string text, LabelStyle labelStyle)
        {
            AddText(new Label(text, labelStyle));
        }

        public void AddText (Label label)
        {
            _contentTable.Add(label);
        }

        public void AddButton (string text)
        {
            AddButton(text, null);
        }

        public void AddButton (string text, object obj)
        {
            if (_skin == null)
                throw new InvalidOperationException("This method may only be used if the dialog was constructed with a Skin");
            AddButton(text, obj, _skin.Get<TextButtonStyle>());
        }

        public void AddButton (string text, object obj, TextButtonStyle buttonStyle)
        {
            AddButton(new TextButton(text, buttonStyle), obj);
        }

        public void AddButton (Button button)
        {
            AddButton(button, null);
        }

        public void AddButton (Button button, object obj)
        {
            _buttonTable.Add(button);
            SetObject(button, obj);
        }

        public void Show (Stage stage)
        {
            ClearActions();
            RemoveCaptureListener(_ignoreTouchDown);

            _prevKeyboardFocus = null;
            Actor actor = stage.GetKeyboardFocus();
            if (actor != null && !actor.IsDescendentOf(this))
                _prevKeyboardFocus = actor;

            _prevScrollFocus = null;
            actor = stage.GetScrollFocus();
            if (actor != null && !actor.IsDescendentOf(this))
                _prevScrollFocus = actor;

            Pack();
            SetPosition((float)Math.Round((stage.Width - Width) / 2), (float)Math.Round((stage.Height - Height) / 2));

            stage.AddActor(this);
            stage.SetKeyboardFocus(this);
            stage.SetScrollFocus(this);

            if (FadeDuration > 0) {
                Color = Color.MultiplyAlpha(0);
                AddAction(ActionRepo.FadeIn(FadeDuration, Interpolation.Fade));
            }
        }

        public void Hide ()
        {
            if (FadeDuration > 0) {
                AddCaptureListener(_ignoreTouchDown);
                AddAction(ActionRepo.Sequence(
                    ActionRepo.FadeOut(FadeDuration, Interpolation.Fade),
                    ActionRepo.RemoveListener(_ignoreTouchDown, true),
                    ActionRepo.RemoveActor()
                    ));
            }
            else
                Remove();
        }

        public override Group Parent
        {
            get { return base.Parent; }
            protected internal set
            {
                base.Parent = value;
                if (value != null) {
                    Stage stage = Stage;
                    if (stage != null) {
                        if (_prevKeyboardFocus != null && _prevKeyboardFocus.Stage == null)
                            _prevKeyboardFocus = null;
                        Actor actor = stage.GetKeyboardFocus();
                        if (actor == null || actor.IsDescendentOf(this))
                            stage.SetKeyboardFocus(_prevKeyboardFocus);

                        if (_prevScrollFocus != null && _prevScrollFocus.Stage == null)
                            _prevScrollFocus = null;
                        actor = stage.GetScrollFocus();
                        if (actor == null || actor.IsDescendentOf(this))
                            stage.SetScrollFocus(_prevScrollFocus);
                    }
                }
            }
        }

        public void SetObject (Actor actor, object obj)
        {
            _values[actor] = obj;
        }

        public void SetKey (int keycode, object obj)
        {
            AddListener(new DispatchInputListener() {
                OnKeyDown = (ev, keycode2) => {
                    if (keycode == keycode2) {
                        Result(obj);
                        if (!_cancelHide)
                            Hide();
                        _cancelHide = false;
                    }
                    return false;
                },
            });
        }

        protected virtual void Result (object obj)
        { }

        public void Cancel ()
        {
            _cancelHide = true;
        }
    }
}
