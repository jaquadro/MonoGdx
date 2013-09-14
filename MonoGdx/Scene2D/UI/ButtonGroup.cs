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

namespace MonoGdx.Scene2D.UI
{
    public class ButtonGroup
    {
        private readonly List<Button> _buttons = new List<Button>();
        private List<Button> _checkedButtons = new List<Button>();
        private Button _lastChecked;

        public ButtonGroup ()
        {
            MinCheckCount = 1;
            MaxCheckCount = 1;
        }

        public ButtonGroup (params Button[] buttons)
            : this()
        {
            MinCheckCount = 0;
            Add(buttons);
            MinCheckCount = 1;
        }

        public int MinCheckCount { get; set; }
        public int MaxCheckCount { get; set; }
        public bool UncheckLast { get; set; }

        public void Add (Button button)
        {
            if (button == null)
                throw new ArgumentNullException("button");

            button.ButtonGroup = null;
            bool shouldCheck = button.IsChecked || Buttons.Count < MinCheckCount;
            button.IsChecked = false;
            button.ButtonGroup = this;
            Buttons.Add(button);

            if (shouldCheck)
                button.IsChecked = true;
        }

        public void Add (params Button[] buttons)
        {
            if (buttons == null)
                throw new ArgumentNullException("buttons");

            foreach (var button in buttons)
                Add(button);
        }

        public void Remove (Button button)
        {
            if (button == null)
                throw new ArgumentNullException("button");

            button.ButtonGroup = null;
            Buttons.Remove(button);
        }

        public void Remove (params Button[] buttons)
        {
            if (buttons == null)
                throw new ArgumentNullException("buttons");

            foreach (var button in buttons)
                Remove(button);
        }

        public void SetChecked (string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            foreach (var button in Buttons) {
                if (button is TextButton && text == (button as TextButton).Text) {
                    button.IsChecked = true;
                    return;
                }
            }
        }

        internal bool CanCheck (Button button, bool newState)
        {
            if (button.IsChecked == newState)
                return false;

            if (!newState) {
                if (_checkedButtons.Count <= MinCheckCount)
                    return false;
                _checkedButtons.Remove(button);
            }
            else {
                if (MaxCheckCount != -1 && _checkedButtons.Count >= MaxCheckCount) {
                    if (UncheckLast) {
                        int old = MinCheckCount;
                        MinCheckCount = 0;
                        _lastChecked.IsChecked = false;
                        MinCheckCount = old;
                    }
                    else
                        return false;
                }

                _checkedButtons.Add(button);
                _lastChecked = button;
            }

            return true;
        }

        public void UncheckAll ()
        {
            int old = MinCheckCount;
            MinCheckCount = 0;

            foreach (var button in Buttons)
                button.IsChecked = false;

            MinCheckCount = old;
        }

        public Button GetChecked ()
        {
            if (_checkedButtons.Count > 0)
                return _checkedButtons[0];

            return null;
        }

        public List<Button> AllChecked
        {
            get { return _checkedButtons; }
        }

        public List<Button> Buttons
        {
            get { return _buttons; }
        }
    }
}
