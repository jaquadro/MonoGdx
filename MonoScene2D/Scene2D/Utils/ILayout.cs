using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGdx.Scene2D.Utils
{
    public interface ILayout
    {
        void Layout ();
        void Invalidate ();
        void InvalidateHierarchy ();
        void Validate ();
        void Pack ();

        void SetFillParent (bool fillParent);
        void SetLayoutEnabled (bool enabled);

        float MinWidth { get; }
        float MinHeight { get; }
        float PrefWidth { get; }
        float PrefHeight { get; }
        float MaxWidth { get; }
        float MaxHeight { get; }
    }
}
