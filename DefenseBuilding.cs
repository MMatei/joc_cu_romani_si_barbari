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

        internal static void readDefBuildings()
        {
            System.IO.StreamReader file = new System.IO.StreamReader("defenseBuildings.txt");
            String s = file.ReadLine();
            while (s.StartsWith("#"))
                s = file.ReadLine();
            int n = Convert.ToInt32(s);
            Game.defBuildings = new DefenseBuilding[n];
            for (int i = 0; i < n; i++)
            {
                s = file.ReadLine();
                while (s.StartsWith("#"))
                    s = file.ReadLine();
                String[] word = s.Split(';');
                Game.defBuildings[i] = new DefenseBuilding(word[0], Convert.ToInt32(word[1]), Convert.ToInt32(word[2]));
            }
            file.Close();
        }
    }
}
