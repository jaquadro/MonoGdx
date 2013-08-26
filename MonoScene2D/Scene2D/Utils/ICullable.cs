using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoScene2D.Geometry;

namespace MonoScene2D.Scene2D.Utils
{
    public interface ICullable
    {
        void SetCullingArea (RectangleF cullingArea);
    }
}
