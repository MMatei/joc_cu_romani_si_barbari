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

        internal static void readNations()
        {
            int n;
            System.IO.StreamReader file = new System.IO.StreamReader("nations.txt");
            String s = file.ReadLine();
            while (s.StartsWith("#"))//while commentary, skip over it
                s = file.ReadLine();
            n = Convert.ToInt32(s);//get nr of nations
            Game.nations = new Nation[n];
            for (int i = 0; i < n; i++)
            {
                s = file.ReadLine();
                while (s.StartsWith("#"))
                    s = file.ReadLine();
                String[] word = s.Split(';');
                Game.nations[i] = new Nation(
                        word[0],//name
                        System.Drawing.Color.FromArgb(Convert.ToInt32(word[1]), Convert.ToInt32(word[2]), Convert.ToInt32(word[3])),
                        Convert.ToInt32(word[4])//money
                        );
            }
            file.Close();
        }

        // I do suggest, however, that equality be tested through position in the static Nations array
        public bool equals(Nation n){
            return name.CompareTo(n.name) == 0;
        }
    }
}
