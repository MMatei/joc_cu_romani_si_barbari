using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace joc_cu_romani_si_barbari
{
    class Unit
    {
        internal UnitStats stats;
        internal float efficiency;
        internal int size;

        public Unit(UnitStats stats, int size)
        {
            this.stats = stats;
            efficiency = 1;
            this.size = size;
        }
    }
}
