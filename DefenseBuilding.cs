using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace joc_cu_romani_si_barbari
{
    class DefenseBuilding
    {
        internal int maxHP, crtHP, def;
        internal String name;
        public DefenseBuilding(String s, int hp, int def)
        {
            name = s;
            maxHP = crtHP = hp;
            this.def = def;
        }
    }
}
