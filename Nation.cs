using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace joc_cu_romani_si_barbari
{
    class Nation
    {
        public String name;
        internal Texture2D armyIcon;
        internal Color color;
        internal int money;
        internal List<Army> armies = new List<Army>();
    
        public Nation(String name, Color color, int moneyInit){
            this.name = name;
            this.color = color;
            money = moneyInit;
        }

        // I do suggest, however, that equality be tested through position in the static Nations array
        public bool equals(Nation n){
            return name.CompareTo(n.name) == 0;
        }
    }
}
