using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace joc_cu_romani_si_barbari.Utilities
{
    /// <summary>
    /// A state in the A* algorithm represents a graph node, coupled with the distance to the source of the search and the parent node
    /// (for path reconstitution)
    /// </summary>
    class AstarState
    {
        internal Province prov;// a pointer to the graph node it is representing
        internal AstarState parent;
        internal int distance;

        public AstarState(Province _prov)
        {
            prov = _prov;
            parent = null;
            distance = 0;
        }

        public AstarState(Province _prov, AstarState _parent, int _distance)
        {
            prov = _prov;
            parent = _parent;
            distance = _distance;
        }

        /// <summary>
        /// Returns a list of all possible next states reachable from this state
        /// More to the point: creates states for each neighbor of this node (calculating distance)
        /// </summary>
        public List<AstarState> expand()
        {
            List<AstarState> l = new List<AstarState>();
            foreach (Neighbor n in prov.neighbors)
            {
                l.Add(new AstarState(n.p, this, distance + n.distance));
            }
            return l;
        }

        /// <summary>
        /// Functia calculeaza euristica h(n) care aproximeaza (optimist!) distanta pana la final
        /// </summary>
        public int approximateDistance(AstarState other)
        {
            // aproximarea foloseste coordonatele in pixeli la care plasam armatele pt a aproxima distanta intre 2 provincii
            // aceasta euristica tine (si este optimista)
            return (int) Math.Sqrt((prov.armyX - other.prov.armyX) * (prov.armyX - other.prov.armyX) + (prov.armyY - other.prov.armyY) * (prov.armyY - other.prov.armyY));
        }
    }
}
