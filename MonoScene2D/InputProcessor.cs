using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoScene2D
{
    public interface InputProcessor
    {
        bool KeyDown (int keycode);
        bool KeyUp (int keycode);
        bool KeyTyped (char character);

        bool TouchDown (int screenX, int screenY, int pointer, int button);
        bool TouchUp (int screenX, int screenY, int pointer, int button);
        bool TouchDragged (int screenX, int screenY, int pointer);

        bool MouseMoved (int screenX, int screenY);

        bool Scrolled (int amount);
    }
}
