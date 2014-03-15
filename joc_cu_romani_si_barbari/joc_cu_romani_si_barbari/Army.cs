using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace joc_cu_romani_si_barbari
{
    class Army
    {
        private Nation owner;
        private List<Unit> units = new List<Unit>();
        internal Neighbor targetBorder;//this is the border of the prov we're marching towards
        internal Province crrtProv;
        internal List<Province> path = new List<Province>();
        internal String name;
        internal int distToMarch;
        internal Rectangle iconLocation;//the rectangle defining the coordinates where the army's icon will be drawn

        internal byte state;//check ArmyState

        internal float borderStance;//the agressiveness of the army's assault on the border
        public const float BYPASS = (float) 0.1;
        public const float NORMAL = (float) 0.5;
        public const float ANNIHILATE = (float) 1.0;

        internal byte enemyProvStance;
        public const byte RAIDING = 0;
        public const byte CONQUER = 1;

        internal byte friendlyProvStance;
        public const byte DEFEND = 2;//try to engage hostile armies
        public const byte AVOID = 3;//avoid hostile armies
    
        public Army(String name, Nation owner, Province crrt){
            this.name = name;
            this.owner = owner;
            crrtProv = crrt;
            iconLocation = new Rectangle(crrt.armyX, crrt.armyY, 64, 64);
            state = ArmyState.IN_FRIENDLY_PROVINCE;
            borderStance = NORMAL;
        }
    
        public void addUnit(Unit u){
            units.Add(u);
        }
    
        public bool nextProvIsFriendly(){
            return owner.equals(path[0].owner);
        }

        /// <summary>
        /// this function sets the neccessary paramaters for the army to march to Province prov
        /// </summary>
        public void goTo(Province destination)
        {
            path = Utilities.AstarState.solve(crrtProv, destination);
            if (path == null) return;//se poate sa dau o destinatie invalida, sau destination == crrtProv
            //gasesc granita catre care merg - am nevoie de ea pt distanta, coord de afisare armata, si sa
            //verific apararile nextProv
            goToNextProvInPath();
        }

        public void goToNextProvInPath()
        {
            Province nextProv = path[0];
            foreach (Neighbor neigh in crrtProv.neighbors)
            {
                if (neigh.otherProv == nextProv)//this is the border we're looking for
                {
                    distToMarch = neigh.distance;
                    targetBorder = neigh;
                    return;
                }
            }
        }

        /// <summary>
        /// On entering a province, ther army's location is updated to the other side of the border
        /// in addition, we must update distToNextProv to be the distance to the center of the province
        /// and we must eliminate this province from the path
        /// </summary>
        public void provinceEntered()
        {
            Neighbor aux = targetBorder.otherSide;//otherSide is where we're at
            crrtProv.armies.Remove(this);//leave the army list of the previous provicne
            crrtProv = targetBorder.otherProv;
            crrtProv.armies.Add(this);//and enter the army list of the new province
            iconLocation.X = aux.armyX;
            iconLocation.Y = aux.armyY;
            distToMarch = aux.distance;
            targetBorder = null;//we're marching towards the center of a province, not towards a border
            path.RemoveAt(0);
            if (path.Count == 0)
                path = null;
        }
    
        /// <summary>
        /// the function returns the damage the army does on assaults
        /// </summary>
        /// <param name="defensesPresent">if there are no defenses present, the cavalry will participate in the attack</param>
        public double getAssaultDamage(bool defensesPresent) {
            if (units.Count == 0) return 0;
            double dmg = 0;
            Random r = new Random();
            int n = units.Count;
            for (int i = 0; i < n; i++)
            {
                Unit u = units[i];
                if(u.stats.type == UnitStats.LIGHT_CAVALRY || u.stats.type == UnitStats.HEAVY_CAVALRY){
                    if(!defensesPresent){//if there are no fortifications, then cavalry will participate in the attack
                        dmg += u.stats.attack * u.efficiency * u.size;
                    }
                }
                else {
                    dmg += u.stats.attack * u.efficiency * u.size;
                }
            }
            //we apply a modifier of 0.8 -> 1.2 on the damage to bring some randomness to the game
            Console.WriteLine("raw army dmg is " + dmg);
            return dmg * ((float)r.Next(401)/1000.0f + 0.8f);
        }
    
        /// <summary>
        /// the function deals damage to the army during an assault phase
        /// </summary>
        /// <param name="dmg">the damage to absorb</param>
        /// <param name="defensesPresent">if there are no defenses, cavalry also participated in the assault (so it receives damage)</param>
        public void eatAssaultDamage(double dmg, bool defensesPresent) {
            if (units.Count == 0) return;
            int size = 0;
            int n = units.Count;
            for (int i = 0; i < n; i++)
            {
                Unit u = units[i];
                if (u.stats.type == UnitStats.LIGHT_CAVALRY || u.stats.type == UnitStats.HEAVY_CAVALRY) {
                    if (!defensesPresent)//cavalry gets damaged only if it participated in the assault
                        size += u.size;
                }
                else
                    size += u.size;
            }
            for (int i = n-1; i >= 0; i--)
            {
                Unit u = units[i];
                double temp_dmg = dmg * ((double)u.size/(double)size);//if a unit makes up x% of the force, it eats x% of the damage
                if (temp_dmg == 0)//no reason to... continue =))
                    continue;
                u.size -= (int)(temp_dmg / (u.stats.defense * u.efficiency + 1));
                Console.WriteLine("Army unit " + u.stats.name + " suffers " + (int)(temp_dmg / (u.stats.defense * u.efficiency + 1)) + " damage.");
                if (u.size <= 0)
                    units.RemoveAt(i);
            }
        }
    }

    class ArmyState
    {
        public const byte IN_FRIENDLY_PROVINCE = 0;
        public const byte ON_BORDER = 1;
        public const byte IN_ENEMY_PROVINCE = 2;
    }
}
