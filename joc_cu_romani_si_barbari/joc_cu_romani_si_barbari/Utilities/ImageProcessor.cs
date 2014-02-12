using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace joc_cu_romani_si_barbari.Utilities
{
    class ImageProcessor
    {
        /// <summary>
        /// Reads mapMatrix from map.bin
        /// </summary>
        /// <param name="dimensions">a hack to permit us to send back height and width</param>
        /// <returns></returns>
        public static byte[,] readMapMatrix(int[] dimensions)
        {
            BinaryReader file = new BinaryReader(new FileStream("map.bin", FileMode.Open));
            int w = file.ReadInt32();
            int h = file.ReadInt32();
            dimensions[0] = w;
            dimensions[1] = h;
            int p = 0;
            byte[,] mapMatrix = new byte[h, w];
            byte[] data = file.ReadBytes(h * w);
            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                    mapMatrix[i, j] = data[p++];
            return mapMatrix;
        }

        // The following 3 functions are used only when the map files are changed
        /// <summary>
        /// functia creeaza mapMatrix care contine informatii despre carei provincii ii apartine pixelul i, j
        /// si apoi scrie matricea in fisierul map.bin
        /// pt a determina acest lucru, parcurgem bmp-ul pixel cu pixel
        /// fiecarei culori diferite intalnite ii asignam un nr diferit si fiecarui pixel ii asignam nr corespunzator
        /// astfel, dintr-o matrice de culori => o matrice de id-uri de provincii
        /// (PS: id-ul descoperit aici va trebui sa fie asignat provinciei in provinces.txt)
        /// </summary>
        public unsafe static void writeMapMatrixToFile()
        {
            Bitmap bmp = new Bitmap("provinces.bmp");
            byte[,] mapMatrix = new byte[bmp.Height, bmp.Width];
            List<Color> colors = new List<Color>();
            //accesam info care ne intereseaza din bmp (adica pixelii)
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte* b = (byte*)data.Scan0;//folosim un pointer ca sa parcurgem pas cu pas acest vector de date
            int p = 0, h = bmp.Height, w = bmp.Width;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    byte blue = b[p]; //B
                    byte green = b[p + 1]; //G
                    byte red = b[p + 2]; //R
                    Color c = Color.FromArgb(red, green, blue);
                    int k = colors.IndexOf(c);
                    if (k == -1)
                    {//am descoperit culoare noua => o adaugam la lista (cu un index/id mai mare, evident)
                        //Console.WriteLine(c);
                        k = colors.Count;
                        colors.Add(c);
                    }
                    mapMatrix[y, x] = (byte)k;
                    //daca tot suntem aici, hai sa si desenam harta
                    //tot ce trebuie sa fac este sa vad cine detine provincia => culoarea pe care trb sa o aiba pixelii provinciei
                    b[p++] = Game.provinces[k].owner.color.B;// et voila
                    b[p++] = Game.provinces[k].owner.color.G;
                    b[p++] = Game.provinces[k].owner.color.R;
                }
            }
            bmp.UnlockBits(data);//frumos e sa si eliberam resursele dupa ce nu mai avem nevoie de ele

            BinaryWriter file = new BinaryWriter(new FileStream("map.bin", FileMode.OpenOrCreate));
            file.Write(w);
            file.Write(h);
            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                    file.Write(mapMatrix[i, j]);
        }

        /// <summary>
        /// functia imparte map.png in bucati de maxim 2048 x 2048; rezultatul e depozitat in graphics\\map\\
        /// </summary>
        public static void splitMapPng()
        {
            Bitmap bmp = new Bitmap("map.png");
            int w = bmp.Width, h = bmp.Height, crrtX = 0, crrtY = 0, textureNr = 0;
            while (crrtY != h)
            {
                int width = crrtX + 2048 > w ? w - crrtX : 2048;
                int height = crrtY + 2048 > h ? h - crrtY : 2048;
                Bitmap bmp1 = bmp.Clone(new Rectangle(crrtX, crrtY, width, height), System.Drawing.Imaging.PixelFormat.DontCare);
                bmp1.Save("graphics\\map\\" + textureNr + ".png", System.Drawing.Imaging.ImageFormat.Png);
                textureNr++;
                crrtX += width;
                if (crrtX == w)
                {
                    crrtX = 0;
                    crrtY += height;
                }
            }
        }

        /// <summary>
        /// Creates the white background for province provID and saves it in provName.png
        /// </summary>
        public static unsafe void createProvWhitespace(byte[,] mapMatrix, int startX, int startY, int endX, int endY, byte provID)
        {
            Bitmap bmp = new Bitmap(endX - startX, endY - startY, PixelFormat.Format32bppArgb);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            byte* b = (byte*)data.Scan0;//folosim un pointer ca sa parcurgem pas cu pas acest vector de date
            int p = 0, h = bmp.Height, w = bmp.Width;
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    if (mapMatrix[y, x] == provID)
                    {
                        b[p++] = 255;
                        b[p++] = 255;
                        b[p++] = 255;
                        b[p++] = 255;//alpha
                    }
                    else
                    {
                        b[p++] = 0;
                        b[p++] = 0;
                        b[p++] = 0;
                        b[p++] = 0;
                    }
                }
            }
            bmp.UnlockBits(data);
            bmp.Save("graphics\\map\\"+Game.provinces[provID].name+".png", System.Drawing.Imaging.ImageFormat.Png);
        }

        public static unsafe void makeCircle(int r, int ctrX, int ctrY)
        {
            Bitmap bmp = new Bitmap(64, 64, PixelFormat.Format32bppArgb);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            byte* b = (byte*)data.Scan0;//folosim un pointer ca sa parcurgem pas cu pas acest vector de date
            int p = 0, h = bmp.Height, w = bmp.Width;
            int rSquared = r * r, rMin1Squared = (r - 1) * (r), rPls1Squared = (r + 1) * (r), rPls2Squared = (r + 1) * (r+1);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int radius = (x-ctrX)*(x-ctrX) + (y-ctrY)*(y-ctrY);
                    if (rMin1Squared < radius)// && radius < rPls1Squared
                    {
                        b[p++] = 120;
                        b[p++] = 225;
                        b[p++] = 225;
                        double d = 255 * (2.0 - (double)radius / (double)rSquared);
                        if (d > 255) d = 255;
                        if (d < 100) d = 0;
                        b[p++] = (byte)d;//alpha
                        Console.WriteLine(d);
                    }
                    /*else if (rPls1Squared <= radius && radius < rPls2Squared)
                    {
                        b[p++] = 0;
                        b[p++] = 0;
                        b[p++] = 0;
                        b[p++] = 200;//alpha
                    }*/
                    else
                    {
                        b[p++] = 0;
                        b[p++] = 0;
                        b[p++] = 0;
                        b[p++] = 0;
                    }
                }
            }
            bmp.UnlockBits(data);
            bmp.Save("graphics/army icons/circle.png", System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
