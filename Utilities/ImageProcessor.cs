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
        /// functia creeaza mapMatrix care contine informatii despre carei provincii ii apartine pixelul i, j
        /// pt a determina acest lucru, parcurgem bmp-ul pixel cu pixel
        /// fiecarei culori diferite intalnite ii asignam un nr diferit si fiecarui pixel ii asignam nr corespunzator
        /// astfel, dintr-o matrice de culori => o matrice de id-uri de provincii
        /// (PS: id-ul descoperit aici coincide cu cel asignat provinciei in provinces.txt)
        /// mai mult, tot aici creeam texturile care vor depicta culorile provinciilor
        /// </summary>
        /// <param name="provTextureStream">array de 6 MemoryStream-uri in care depozitam temporar datele despre textura
        /// (XNA nu are metode mai directe pt incarcat texturi)</param>
        public unsafe static byte[,] createMapMatrix(MemoryStream[] provTextureStream)
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
                    byte green = b[p+1]; //G
                    byte red = b[p+2]; //R
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
                    c = Game.provinces[k].owner.color;
                    b[p++] = c.B;// et voila
                    b[p++] = c.G;
                    b[p++] = c.R;
                }
            }
            bmp.UnlockBits(data);//frumos e sa si eliberam resursele dupa ce nu mai avem nevoie de ele
            //provinciile colorate vor forma o textura; dar cum o textura are o dimensiune maxima
            Bitmap bmp1 = bmp.Clone(new Rectangle(0, 0, 1466, 1363), PixelFormat.Format24bppRgb);//trb impartita pe bucati
            provTextureStream[0] = new MemoryStream();//aceste MemoryStream-uri vor fi incarcate apoi in textura
            bmp1.Save(provTextureStream[0], ImageFormat.Png);
            bmp1 = bmp.Clone(new Rectangle(1465, 0, 1466, 1363), PixelFormat.Format24bppRgb);
            provTextureStream[1] = new MemoryStream();
            bmp1.Save(provTextureStream[1], ImageFormat.Png);
            bmp1 = bmp.Clone(new Rectangle(2932, 0, 1468, 1363), PixelFormat.Format24bppRgb);
            provTextureStream[2] = new MemoryStream();
            bmp1.Save(provTextureStream[2], ImageFormat.Png);
            bmp1 = bmp.Clone(new Rectangle(0, 1363, 1466, 1364), PixelFormat.Format24bppRgb);
            provTextureStream[3] = new MemoryStream();
            bmp1.Save(provTextureStream[3], ImageFormat.Png);
            bmp1 = bmp.Clone(new Rectangle(1466, 1363, 1466, 1364), PixelFormat.Format24bppRgb);
            provTextureStream[4] = new MemoryStream();
            bmp1.Save(provTextureStream[4], ImageFormat.Png);
            bmp1 = bmp.Clone(new Rectangle(2932, 1363, 1468, 1364), PixelFormat.Format24bppRgb);
            provTextureStream[5] = new MemoryStream();
            bmp1.Save(provTextureStream[5], ImageFormat.Png);
            return mapMatrix;
        }

        /// <summary>
        /// functia asigura recolorarea provinciilor; pt a parcurge numarul minim de pixeli, furnizam functiei un dreptunghi pe care sa-l
        /// recoloreze; de asemenea, furnizam referinte catre texturi, pt ca functia sa le poata accesa datele direct
        /// (fara alte mijlociri care consuma timp pretios pe procesor; fct trb sa fie cat mai rapida)
        /// </summary>
        public unsafe static void updateMap(byte[,] mapMatrix, int startX, int startY, int endX, int endY,
            Texture2D prov00, Texture2D prov01, Texture2D prov02, Texture2D prov10, Texture2D prov11, Texture2D prov12)
        {
            //conditiile se asigura ca intram sa modificam doar texturile strict necesare
            if (startX < 1466 && startY < 1363)
            {
                //Console.WriteLine("rect0");
                int p = 0;
                byte[] b = new byte[1466 * 1363 * 4];
                prov00.GetData(b);
                int endH = endY < 1363 ? endY : 1363;
                int endW = endX < 1466 ? endX : 1466;
                for (int y = startY; y < endH; y++)
                {
                    p = y * 1466 * 4 + startX * 4;
                    for (int x = startX; x < endW; x++)
                    {
                        Color c = Game.provinces[mapMatrix[y,x]].owner.color;
                        b[p++] = c.R;
                        b[p++] = c.G;
                        b[p++] = c.B;
                        p++;//Alpha remains unchanged
                    }
                }
                prov00.SetData(b);
            }
            if (startX < 2931 && startY < 1363 && endX > 1465)
            {
                //Console.WriteLine("rect1");
                int p = 0;
                byte[] b = new byte[1466 * 1363 * 4];
                prov01.GetData(b);
                int startW = startX > 1466 ? startX : 1466;
                int endH = endY < 1363 ? endY : 1363;
                int endW = endX < 2932 ? endX : 2932;
                for (int y = startY; y < endH; y++)
                {
                    p = y * 1466 * 4 + (startW - 1466) * 4;
                    for (int x = startW; x < endW; x++)
                    {
                        Color c = Game.provinces[mapMatrix[y, x]].owner.color;
                        b[p++] = c.R;
                        b[p++] = c.G;
                        b[p++] = c.B;
                        p++;//Alpha remains unchanged
                    }
                }
                prov01.SetData(b);
            }
            if (startY < 1363 && endX > 2931)
            {
                //Console.WriteLine("rect2");
                int p = 0;
                byte[] b = new byte[1468 * 1363 * 4];
                prov02.GetData(b);
                int startW = startX > 2932 ? startX : 2932;
                int endH = endY < 1363 ? endY : 1363;
                for (int y = 0; y < endH; y++)
                {
                    p = y * 1468 * 4 + (startW - 2932) * 4;
                    for (int x = startW; x < endX; x++)
                    {
                        Color c = Game.provinces[mapMatrix[y, x]].owner.color;
                        b[p++] = c.R;
                        b[p++] = c.G;
                        b[p++] = c.B;
                        p++;//Alpha remains unchanged
                    }
                }
                prov02.SetData(b);
            }
            if (startX < 1363 && endY > 1362)
            {
                //Console.WriteLine("rect3");
                int p = 0;
                byte[] b = new byte[1466 * 1364 * 4];
                prov10.GetData(b);
                int startH = startY > 1363 ? startY : 1363;
                int endW = endX < 1466 ? endX : 1466;
                for (int y = startH; y < endY; y++)
                {
                    p = (y - 1363) * 1466 * 4 + startX * 4;
                    for (int x = startX; x < endW; x++)
                    {
                        Color c = Game.provinces[mapMatrix[y, x]].owner.color;
                        b[p++] = c.R;
                        b[p++] = c.G;
                        b[p++] = c.B;
                        p++;//Alpha remains unchanged
                    }
                }
                prov10.SetData(b);
            }
            if (startX < 2931 && endX > 1465 && endY > 1362)
            {
                //Console.WriteLine("rect4");
                int p = 0;
                byte[] b = new byte[1466 * 1364 * 4];
                prov11.GetData(b);
                int startH = startY > 1363 ? startY : 1363;
                int startW = startX > 1466 ? startX : 1466;
                int endW = endX < 2932 ? endX : 2932;
                for (int y = startH; y < endY; y++)
                {
                    p = (y - 1363) * 1466 * 4 + (startW - 1466) * 4;
                    for (int x = startW; x < endW; x++)
                    {
                        Color c = Game.provinces[mapMatrix[y, x]].owner.color;
                        b[p++] = c.R;
                        b[p++] = c.G;
                        b[p++] = c.B;
                        p++;//Alpha remains unchanged
                    }
                }
                prov11.SetData(b);
            }
            if (endX > 2932 && endY > 1362)
            {
                //Console.WriteLine("rect5");
                int p = 0;
                byte[] b = new byte[1468 * 1364 * 4];
                prov12.GetData(b);
                int startH = startY > 1363 ? startY : 1363;
                int startW = startX > 2932 ? startX : 2932;
                for (int y = startH; y < endY; y++)
                {
                    p = (y - 1363) * 1468 * 4 + (startW - 2932) * 4;
                    for (int x = startW; x < endX; x++)
                    {
                        Color c = Game.provinces[mapMatrix[y, x]].owner.color;
                        b[p++] = c.R;
                        b[p++] = c.G;
                        b[p++] = c.B;
                        p++;//Alpha remains unchanged
                    }
                }
                prov12.SetData(b);
            }
        }

        /// <summary>
        /// functia imparte map.png in sase bucati (frumoase)
        /// cat timp nu schimbi harta, n-ai ce cauta aici
        /// </summary>
        public static void splitMapPng()
        {
            Bitmap bmp = new Bitmap("map.png");
            Bitmap bmp1 = bmp.Clone(new Rectangle(0, 0, 1466, 1363), System.Drawing.Imaging.PixelFormat.DontCare);
            bmp1.Save("00.png", System.Drawing.Imaging.ImageFormat.Png);
            bmp1 = bmp.Clone(new Rectangle(1465, 0, 1466, 1363), System.Drawing.Imaging.PixelFormat.DontCare);
            bmp1.Save("01.png", System.Drawing.Imaging.ImageFormat.Png);
            bmp1 = bmp.Clone(new Rectangle(2932, 0, 1468, 1363), System.Drawing.Imaging.PixelFormat.DontCare);
            bmp1.Save("02.png", System.Drawing.Imaging.ImageFormat.Png);
            bmp1 = bmp.Clone(new Rectangle(0, 1363, 1466, 1364), System.Drawing.Imaging.PixelFormat.DontCare);
            bmp1.Save("10.png", System.Drawing.Imaging.ImageFormat.Png);
            bmp1 = bmp.Clone(new Rectangle(1466, 1363, 1466, 1364), System.Drawing.Imaging.PixelFormat.DontCare);
            bmp1.Save("11.png", System.Drawing.Imaging.ImageFormat.Png);
            bmp1 = bmp.Clone(new Rectangle(2932, 1363, 1468, 1364), System.Drawing.Imaging.PixelFormat.DontCare);
            bmp1.Save("12.png", System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
