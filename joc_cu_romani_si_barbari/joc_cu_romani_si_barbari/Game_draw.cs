using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace joc_cu_romani_si_barbari
{
    // This is where ALL draw-related functions are located
    public partial class Game
    {
        //placed all drawing in a separate functions so we can reuse that code when taking screenshots
        //Render world map to minimapTexture
        private void makeMinimap()
        {
            // Set the render target
            GraphicsDevice.SetRenderTarget(minimapTexture);

            // Draw the scene to texture
            float initZoom = camera.Zoom;
            Vector2 dummy = new Vector2(0.0f, 0.0f), initPos;
            camera.Zoom = 0.5f;
            initPos = camera.Pos;
            camera.Pos += dummy;
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null, camera.GetTransformation());

            // Draw our images
            // spriteBatch.Draw( texture to draw, rectangle in which to draw, what part of the image to draw (null means all of it),
            //      Color with which to tint the texture (White means no modification), rotation, sprite origin, sprite effects,
            //      layer depth (in the FrontToBack order, 0.0 lies at the back and 1.0 at the front) )
            spriteBatch.Draw(provinces[0].background, provinces[0].position, null, provinces[0].color, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            for (int i = 1; i < provinces.Length; i++)
                spriteBatch.Draw(provinces[i].background, provinces[i].position, null, provinces[i].color, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            for (int i = 0; i < mapTextureLength; i++)
                spriteBatch.Draw(mapTexture[i], mapPosition[i], null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            spriteBatch.End();

            // Drop the render target
            GraphicsDevice.SetRenderTarget(null);
            camera.Zoom = initZoom;
            camera.Pos = initPos;
        }
        //Draw the world map to screen, along with the UI
        private void _draw()
        {
            // In order to properly render TextArea 's, I had to resort to a little trick which involves rendering to textures
            // Due to the way XNA works, ALL rendering to texture must be done BEFORE rendering to the screen
            // Hence, we render the TextArea 's here; the field textArea will contain the texture we need to draw
            if (prevSelectedProv != null)
            {
                String text = "Stationed armies\n";
                prevSelectedProv.armies.ForEach(delegate(Army army)
                {
                    text += army.name + "\n";
                });
                armiesInProvTextArea.draw(text);
                String[] txt = new String[3];
                txt[0] = "Adjacent provinces\n";
                txt[1] = "Distance\n";
                txt[2] = "Border Length\n";
                foreach (Neighbor neigh in prevSelectedProv.neighbors)
                {
                    txt[0] += neigh.otherProv.name + "\n";
                    txt[1] += neigh.distance + "km\n";
                    txt[2] += neigh.borderLength + "km\n";
                }
                neighborsTextArea.draw(txt);
            }
            if (selectedArmies.Count > 0)
            {
                String text = "Selected armies\n";
                foreach (Army army in selectedArmies)
                {
                    text += army.name + "\n";
                }
                armiesSelectedTextArea.draw(text);
            }

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, null, null, null, null, camera.GetTransformation());
            spriteBatch.Draw(provinces[0].background, provinces[0].position, null, provinces[0].color, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            for (int i = 1; i < provinces.Length; i++)
                spriteBatch.Draw(provinces[i].background, provinces[i].position, null, provinces[i].color, 0.0f, Vector2.Zero, SpriteEffects.None, 0.05f);
            for (int i = 0; i < mapTextureLength; i++)
                spriteBatch.Draw(mapTexture[i], mapPosition[i], null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            //draw the armies of the world
            foreach (Nation nation in nations)
            {
                foreach (Army army in nation.armies)
                {
                    spriteBatch.Draw(nation.armyIcon, army.iconLocation, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                }
            }
            Vector2 rotPoint = new Vector2(0, 15);//punctul in dreptunghiul barei de mers in jurul caruia se face rotirea
            foreach (Army army in selectedArmies)
            {
                //we draw an appropriate selection halo for each selected army
                spriteBatch.Draw(selectHalo, army.iconLocation, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.8f);
                //now we represent the path of an army via yellow lines
                if (army.path == null)
                    continue;
                Province crrtProv = army.crrtProv;
                foreach (Province nextProv in army.path)
                {
                    float width = (float)Math.Sqrt((crrtProv.armyX - nextProv.armyX) * (crrtProv.armyX - nextProv.armyX) + (crrtProv.armyY - nextProv.armyY) * (crrtProv.armyY - nextProv.armyY));
                    float angle;//unghiul fata de orizontala la care se afla dreapta dintre crrtProv si nextProv
                    //cum trigonometria mea e pe butuci, habar-n-am de ce functioneaza secventa urmatoare
                    //daca gasesti ceva mai elegant, be my guest
                    if (nextProv.armyX < crrtProv.armyX)
                    {
                        if(nextProv.armyY < crrtProv.armyY)
                            angle = (float)Math.Asin((float)(nextProv.armyX - crrtProv.armyX) / width) - (float)(Math.PI / 2);
                        else
                            angle = (float)Math.Acos((float)(nextProv.armyX - crrtProv.armyX) / width);
                    }
                    else
                        angle = (float)Math.Asin((float)(nextProv.armyY - crrtProv.armyY) / width);
                    spriteBatch.Draw(pathBar, new Rectangle(crrtProv.armyX+32, crrtProv.armyY+32, (int)width, 31), null, Color.White, angle, new Vector2(0, 15), SpriteEffects.None, 0.8f);
                    //punem la capat si un punctulet care sa acopere imperfectiunile liniilor
                    spriteBatch.Draw(dot, new Rectangle(nextProv.armyX+24, nextProv.armyY+24, 16, 16), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.81f);
                    crrtProv = nextProv;
                }
            }
            spriteBatch.End();

            //desenam elementele statice in raport cu camera (adica nu sunt afectate de ViewMatrix-ul camerei)
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, null, null, null);
            spriteBatch.Draw(uiProvinceDetailTexture, uiProvinceDetailRect, null, Color.White, 0.0f,Vector2.Zero, SpriteEffects.None, 0.95f);
            spriteBatch.Draw(coin, new Rectangle((int)(screenW * 0.75), (int)(screenH * 0.7), 30, 30), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(font, "Imperial Ledger", new Vector2((int)(screenW * 0.48), (int)(screenH * 0.7)), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(font, nations[player].money + "", new Vector2((int)(screenW * 0.775), (int)(screenH * 0.7)), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(font, date.ToString(), new Vector2((int)(screenW * 0.9), (int)(screenH * 0.7)), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            if (prevSelectedProv != null)
            {
                spriteBatch.DrawString(font, prevSelectedProv.name + "", new Vector2((int)(screenW * 0.1), (int)(screenH * 0.75)), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                spriteBatch.DrawString(font, "Prosperity: " + prevSelectedProv.prosperity + "", new Vector2((int)(screenW * 0.1), (int)(screenH * 0.8)), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                spriteBatch.Draw(armiesInProvTextArea.textArea, armiesInProvTextArea.textAreaRect, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
                spriteBatch.Draw(neighborsTextArea.textArea, neighborsTextArea.textAreaRect, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            }
            if (selectedArmies.Count > 0)
            {
                spriteBatch.Draw(armiesSelectedTextArea.textArea, armiesSelectedTextArea.textAreaRect, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            }
            spriteBatch.End();
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!isActive) return;//no need to draw when the game is not in focus
            //watch.Restart();
            GraphicsDevice.Clear(Color.Black);
            switch (gameState)
            {
                case IN_GAME:
                    {
                        _draw();
                    }
                    break;
                case MAIN_MENU:
                    {
                        spriteBatch.Begin();
                        spriteBatch.Draw(mainMenuTexture, new Rectangle(0, 0, screenW, screenH), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
                        spriteBatch.DrawString(font, "New Game", new Vector2(newGameRect.X, newGameRect.Y), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                        spriteBatch.DrawString(font, "Options", new Vector2(optionsRect.X, optionsRect.Y), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                        spriteBatch.DrawString(font, "Quit", new Vector2(quitRect.X, quitRect.Y), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                        spriteBatch.DrawString(font, Game.version, new Vector2((int)(screenW * 0.9), (int)(screenH * 0.9)), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                        spriteBatch.End();
                    }
                    break;
                case OPTIONS_MENU:
                    {
                        optionsMenu.draw();
                    }
                    break;
            }
            //watch.Stop();
            //Console.WriteLine(watch.ElapsedMilliseconds);
        }
    }
}
