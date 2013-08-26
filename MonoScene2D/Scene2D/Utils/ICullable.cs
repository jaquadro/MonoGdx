using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGdx.Geometry;

namespace MonoGdx.Scene2D.Utils
{
    public interface ICullable
    {
        void SetCullingArea (RectangleF cullingArea);
    }
}
