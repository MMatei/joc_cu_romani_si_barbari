using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace joc_cu_romani_si_barbari.Utilities
{
    interface HelperComponent
    {
        void update(KeyboardState key, MouseState mouse);
        void draw();
    }
}
