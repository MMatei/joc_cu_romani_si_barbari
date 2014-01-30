using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace joc_cu_romani_si_barbari
{
    class Tactic
    {
        String name;
        internal float progress;//to next phase
        internal float[] attackModif = new float[7], defenseModif= new float[7];//each unit type has an attack and defense modifier thanks to this tactic
        internal int[] commitWeight= new int[7];//determines how much of the frontline will be occupied by this unit
        internal float[] reqPercentage = new float[7];//the troop percentage required to select this tactic

        public Tactic(String _name)
        {
            name = _name;
        }

        internal static void readMeleeTactics(){
            char[] separator = { ' ', ';' };
            System.IO.StreamReader file = new System.IO.StreamReader("melee_tactics.txt");
            //citesc nr de tactici
            String s = file.ReadLine();
            while (s.StartsWith("#"))
                s = file.ReadLine();
            int n = Convert.ToInt32(s);
            Tactic[] meleeTactics = Game.meleeTactics = new Tactic[n];
            for (int i = 0; i < n; i++)
            {
                s = file.ReadLine();
                while (s.StartsWith("#"))
                    s = file.ReadLine();
                if (s.EndsWith("{"))
                {//incepe bloc descriere tactica urmatoare
                    s = file.ReadLine();
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    s = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries)[0];//removes any whitespace
                    Tactic t = new Tactic(s.Replace('_', ' '));
                    s = file.ReadLine();
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    s = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries)[0];
                    t.progress = Convert.ToSingle(s, Game.cultureInfo);
                    s = file.ReadLine();
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    String[] word = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                    for(int j = 0; j < 7; j++)
                        t.attackModif[j] = Convert.ToSingle(word[j], Game.cultureInfo);
                    s = file.ReadLine();
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    word = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < 7; j++)
                        t.defenseModif[j] = Convert.ToSingle(word[j], Game.cultureInfo);
                    s = file.ReadLine();
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    word = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < 7; j++)
                        t.commitWeight[j] = Convert.ToInt32(word[j], Game.cultureInfo);
                    s = file.ReadLine();
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    word = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < 7; j++)
                        t.reqPercentage[j] = Convert.ToSingle(word[j], Game.cultureInfo);
                    meleeTactics[i] = t;
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    s = file.ReadLine();//read the closing '}'
                }
                else
                {
                    Console.WriteLine("ERROR: expected tactic #" + i + ", but instead found " + s);
                }
            }
        }

        internal static void readSkirmishTactics()
        {
            char[] separator = { ' ', ';' };
            System.IO.StreamReader file = new System.IO.StreamReader("skirmish_tactics.txt");
            //citesc nr de tactici
            String s = file.ReadLine();
            while (s.StartsWith("#"))
                s = file.ReadLine();
            int n = Convert.ToInt32(s);
            Tactic[] skirmishTactics = Game.skirmishTactics;
            skirmishTactics = new Tactic[n];
            for (int i = 0; i < n; i++)
            {
                s = file.ReadLine();
                while (s.StartsWith("#"))
                    s = file.ReadLine();
                if (s.EndsWith("{"))
                {//incepe bloc descriere tactica urmatoare
                    s = file.ReadLine();
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    s = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries)[0];//removes any whitespace
                    Tactic t = new Tactic(s.Replace('_', ' '));
                    s = file.ReadLine();
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    s = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries)[0];
                    t.progress = Convert.ToSingle(s, Game.cultureInfo);
                    s = file.ReadLine();
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    String[] word = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < 7; j++)
                        t.attackModif[j] = Convert.ToSingle(word[j], Game.cultureInfo);
                    s = file.ReadLine();
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    word = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < 7; j++)
                        t.defenseModif[j] = Convert.ToSingle(word[j], Game.cultureInfo);
                    s = file.ReadLine();
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    word = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < 7; j++)
                        t.commitWeight[j] = Convert.ToInt32(word[j], Game.cultureInfo);
                    s = file.ReadLine();
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    word = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < 7; j++)
                        t.reqPercentage[j] = Convert.ToSingle(word[j], Game.cultureInfo);
                    skirmishTactics[i] = t;
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    s = file.ReadLine();//read the closing '}'
                }
                else
                {
                    Console.WriteLine("ERROR: expected tactic #" + i + ", but instead found " + s);
                }
            }
        }
    }
}
