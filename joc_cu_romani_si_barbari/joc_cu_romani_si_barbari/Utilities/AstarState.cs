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
                //the distance between provinces A and B is the distance from A to its border with B
                // + the distance from B to its border with A
                l.Add(new AstarState(n.otherProv, this, distance + n.distance + n.otherSide.distance));
            }
            return l;
        }

        /// <summary>
        /// Functia calculeaza euristica h(n) care aproximeaza (optimist!) distanta pana la final
        /// </summary>
        public int approximateDistance(Province dest)
        {
            // aproximarea foloseste coordonatele in pixeli la care plasam armatele pt a aproxima distanta intre 2 provincii
            // aceasta euristica tine (si este optimista) cat timp un pixel reprezinta > 1km
            return (int)Math.Sqrt((prov.armyX - dest.armyX) * (prov.armyX - dest.armyX) + (prov.armyY - dest.armyY) * (prov.armyY - dest.armyY));
        }

        public static List<Province> solve(Province start, Province end)
        {
            SortedSet<AstarState> open = new SortedSet<AstarState>(new AstarComparator(end));
            AstarState initialState = new AstarState(start);
            open.Add(initialState);
            List<AstarState> closed = new List<AstarState>();
            while (open.Count > 0)
            {
                AstarState state = open.Min;
                open.Remove(open.Min);
                if (state.prov.equals(end))
                {
                    //printReversePath(state);
                    List<Province> path = new List<Province>();
                    while (state.parent != null)
                    {
                        path.Add(state.prov);
                        Console.WriteLine(state.prov);
                        state = state.parent;
                    }
                    path.Reverse();
                    return path;
                }
                List<AstarState> neighbors = state.expand();
                closed.Add(state);
                foreach (AstarState s in neighbors)
                {
                    if (!closed.Contains(s))
                        open.Add(s);
                }
            }
            return null;
        }

        private static void printReversePath(AstarState current)
        {
		    if(current == null) return;
		    printReversePath(current.parent);
		    Console.WriteLine(current.prov);
	    }

        public override bool Equals(object obj)
        {
            return this.prov.equals(((AstarState)obj).prov);
        }
    }

    class AstarComparator : IComparer<AstarState>
    {
        private Province destination;
        public AstarComparator(Province dest)
        {
            destination = dest;
        }
        public int Compare(AstarState a, AstarState b)
        {
            //Possible optimization: pre-compute approximateDistance
            return (a.distance + a.approximateDistance(destination)) - (b.distance + b.approximateDistance(destination));
        }
    }
}
