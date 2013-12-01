using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace joc_cu_romani_si_barbari
{
    class Neighbor
    {
        internal Province p;//this class belongs to Province p2 and p is adjacent to p2
        internal int borderLength, naturalDef, distance, armyX, armyY;
        private DefenseBuilding defBuilding;
        private List<Unit> defUnits = new List<Unit>();
    
        public Neighbor(Province p, int length, int def, int dist, int x, int y){
            this.p = p;
            borderLength = length;
            naturalDef = def;
            distance = dist;
            armyX = x;
            armyY = y;
        }
    
        public void addDefBuilding(DefenseBuilding b){
            defBuilding = b;
        }
    
        public int getDefenseRating(){
            if(defBuilding == null) return 0;
            return defBuilding.def;
        }

        public bool hasDefenses()
        {
            return defBuilding == null;
        }

        public bool hasDefenders()
        {
            return defUnits.Count > 0;
        }
    
        public void damageDefenses(int damage){
            if(defBuilding == null) return;
            defBuilding.crtHP -= damage;
            if(defBuilding.crtHP < 0)
                defBuilding = null;
        }
    
        public void addDefUnit(Unit u){
            defUnits.Add(u);
        }
    
        public double getDamage(){
            double dmg = 0;
            Random r = new Random();
            double defRating = getDefenseRating() + naturalDef;//each point of defense rating increases unit attack by 10%
            int size = defUnits.Count;
            for(int i=0; i<size; i++)
            {
                Unit u = defUnits[i];
                dmg += u.stats.attack * ((10 + defRating) * 0.1) * u.efficiency * u.size;
                Console.WriteLine("raw border dmg is " + dmg);
            }
            //we apply a modifier of 0.8 -> 1.2 on the damage to bring some randomness to the game
            return dmg * ((float)r.Next(401) / 1000.0f + 0.8f);
        }
    
        public void eatDamage(double dmg){//you know you're in trouble when you get DOUBLE damage =))
            int size = 0;
            double defRating = getDefenseRating() + naturalDef;
            int n = defUnits.Count;
            for (int i = 0; i < n; i++)
                size += defUnits[i].size;
            for (int i = n-1; i >= 0; i--)//parcurgem invers deoarece se poate sa scoatem elemente din lista
            {//think about it: daca merg crescator si sterg un element, urmatorul element e tot la i; pe cand asa nu mai fac decrementari suplimentare
                Unit u = defUnits[i];
                double temp_dmg = dmg * ((double)u.size / (double)size);//if a unit makes up x% of the border force, it eats x% of the damage
                //each point of defense rating increases unit defense by 10%
                if (temp_dmg == 0)//no reason to... continue =))
                    continue;
                int temp = (int)(temp_dmg / ((u.stats.defense * u.efficiency * (10 + defRating) * 0.1) + 1));
                Console.WriteLine("Border unit " + u.stats.name + " suffers " + temp + " damage.");
                u.size -= (int)(temp_dmg / ((u.stats.defense * u.efficiency * (10 + defRating) * 0.1) + 1));
                if (u.size <= 0)
                    defUnits.RemoveAt(i);
            }
        }

        /// <summary>
        /// The function returns a factor representing the ratio of troops to border length
        /// </summary>
        public double coverage()
        {
            double size = 0;
            int n = defUnits.Count;
            for (int i = 0; i < n; i++)
                size += defUnits[i].size;
            return size / (double)(borderLength * 100);
        }
    }
}
