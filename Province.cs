using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace joc_cu_romani_si_barbari
{
    class Province
    {
        internal Nation owner;
        internal int baseIncome;
        internal float prosperity;
        internal String name;
        internal int armyX, armyY;//the coords for the rectangle wherein we place the armies in the province
        internal int startX, startY, endX, endY;
        internal List<Neighbor> neighbors = new List<Neighbor>();
        internal List<Army> armies = new List<Army>();
        internal bool isSelected;

        public Province(int baseIncome, String name, int _startX, int _startY, int _endX, int _endY, int _armyX, int _armyY)
        {
            this.baseIncome = baseIncome;
            this.name = name;
            isSelected = false;
            armyX = _armyX;
            armyY = _armyY;
            startX = _startX;
            startY = _startY;
            endX = _endX;
            endY = _endY;
        }

        public int getIncome(){
            return (int)(baseIncome * prosperity);
        }

        // I do suggest, however, that equality be tested through position in the static Provinces array
        public bool equals(Province p){
            return name.CompareTo(p.name) == 0;
        }
    }
}
