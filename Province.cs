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

        public Province()
        { }

        public void setProvince(int baseIncome, String name, int _startX, int _startY, int _endX, int _endY, int _armyX, int _armyY)
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

        internal static void readProvinces()
        {
            int n;
            System.IO.StreamReader file = new System.IO.StreamReader("provinces.txt");
            String s = file.ReadLine();
            while (s.StartsWith("#"))
                s = file.ReadLine();
            n = Convert.ToInt32(s);//get nr of provinces
            Province[] provinces = Game.provinces = new Province[n];
            //in descrierea unei provincii pot aparea referinte catre provincii nedefinite inca
            //astfel, creeam de la inceput toate provinciile (albeit empty) ca sa putem pune referinte valide
            for (int i = 0; i < n; i++)
                provinces[i] = new Province();
            for (int i = 0; i < n; i++)
            {
                s = file.ReadLine();
                while (s.StartsWith("#"))
                    s = file.ReadLine();
                if (s.EndsWith("{"))
                {//incepe bloc descriere provincie urmatoare
                    s = file.ReadLine();
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    String[] word = s.Split(';');
                    provinces[i].setProvince(Convert.ToInt32(word[0]), word[1], Convert.ToInt32(word[2]), Convert.ToInt32(word[3]), Convert.ToInt32(word[4]), Convert.ToInt32(word[5]), Convert.ToInt32(word[6]), Convert.ToInt32(word[7]));
                    s = file.ReadLine();
                    while (!s.StartsWith("}"))
                    {//pana se termina descrierea, citim despre vecini
                        while (s.StartsWith("#"))
                            s = file.ReadLine();
                        if (s.StartsWith("}")) break;
                        word = s.Split(';');
                        provinces[i].neighbors.Add(new Neighbor(provinces[Convert.ToInt32(word[0])], Convert.ToInt32(word[1]), Convert.ToInt32(word[2]), Convert.ToInt32(word[3]), Convert.ToInt32(word[4]), Convert.ToInt32(word[5])));
                        s = file.ReadLine();
                    }
                }
            }
            file.Close();
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
