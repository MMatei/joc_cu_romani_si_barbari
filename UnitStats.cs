using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace joc_cu_romani_si_barbari
{
    class UnitStats
    {
        internal byte attrition, attack, defense, morale, siege, type;
        internal int recruitCost, recruitTime;
        internal float upkeep;//for 1 man
        internal String name;
        public const byte LIGHT_INFANTRY = 1;
        public const byte HEAVY_INFANTRY = 2;
        public const byte LIGHT_CAVALRY = 3;
        public const byte HEAVY_CAVALRY = 4;
        public const byte ARCHER = 5;
        public const byte HORSE_ARCHER = 6;
        public const byte SIEGE = 7;

        public UnitStats(String name, byte attrition, byte att, byte def, byte morale, byte siege, byte type, int recruitCost, int recruitTime, float upkeep){
            this.name = name;
            this.attrition = attrition;
            attack = att;
            defense = def;
            this.morale = morale;
            this.siege = siege;
            this.type = type;
            this.recruitCost = recruitCost;
            this.recruitTime = recruitTime;
            this.upkeep = upkeep;
        }

        internal static void readUnitStats()
        {
            BinaryReader file = new BinaryReader(new FileStream("units.bin", FileMode.Open));
            //mai intai citim nr de unitati
            int n = file.ReadInt32();
            Game.unitStats = new UnitStats[n];
            for (int i = 0; i < n; i++)
            {
                String s = "";
                char c = (char)file.ReadByte();
                while (c != 0)//numele se termina cu un octet 0
                {//nu avem de ales decat sa citim octet cu octet
                    s += c;
                    c = (char)file.ReadByte();
                }
                Game.unitStats[i] = new UnitStats(s, file.ReadByte(), file.ReadByte(), file.ReadByte(), file.ReadByte(), file.ReadByte(), file.ReadByte(), file.ReadInt32(), file.ReadInt32(), file.ReadSingle());
            }
            file.Close();
        }
    }
}
