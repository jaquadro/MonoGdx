using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGdx.Scene2D
{
    public interface EventListener
    {
        bool Handle (Event e);
    }

    public abstract class EventListener<T> : EventListener
        where T : Event
    {
        public bool Handle (Event e)
        {
            return (e is T) ? Handle(e as T) : false;

        }

        public abstract bool Handle (T e);
    }
}
