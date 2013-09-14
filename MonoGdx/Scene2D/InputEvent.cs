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
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoGdx.Scene2D
{
    public enum InputType
    {
        TouchDown,
        TouchUp,
        TouchDragged,
        MouseMoved,
        Enter,
        Exit,
        Scrolled,
        KeyDown,
        KeyUp,
        KeyTyped,
    }

    public class InputEvent : Event
    {
        public InputType Type { get; set; }
        public float StageX { get; set; }
        public float StageY { get; set; }
        public int Pointer { get; set; }

        public int Button { get; set; }
        public int KeyCode { get; set; }
        public char Character { get; set; }
        public int ScrollAmount { get; set; }

        public Actor RelatedActor { get; set; }

        public override void Reset ()
        {
            base.Reset();
            RelatedActor = null;
            Button = -1;
        }

        public Vector2 ToCoordinates (Actor actor)
        {
            return actor.StageToLocalCoordinates(new Vector2(StageX, StageY));
        }

        public override string ToString ()
        {
            return Type.ToString();
        }
    }
}
