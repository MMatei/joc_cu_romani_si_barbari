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
    // This is where the update function resides, along with all update-related helper functions
    public partial class Game
    {
        /// <summary>
        /// Allows the game to run logic such as updating the world, checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!isActive) return;//the game doesn't respond to input
            KeyboardState key = Keyboard.GetState();
            MouseState mouseStateCurrent = Mouse.GetState();

            switch (gameState)
            {

                case IN_GAME:
                    {
                        #region Responding to input
                        Vector2 movement = Vector2.Zero;

                        if (mouseStateCurrent.LeftButton == ButtonState.Pressed && mouseStatePrevious.LeftButton == ButtonState.Released)
                        {//we've clicked on something
                            if (prevSelectedProv != null)//first we clear old selections
                                prevSelectedProv.setDeselected();
                            selectedArmies.Clear();
                            //then we compute our coordinates factoring in zoom level
                            Vector2 q1 = new Vector2();
                            Vector2 q2 = new Vector2();
                            q1.X = mouseStateCurrent.X;
                            q1.Y = mouseStateCurrent.Y;
                            q2 = camera.ScreenToWorld(q1);
                            Province p = provinces[mapMatrix[(int)q2.Y, (int)q2.X]];//then use the mapMatrix to deduce on which province we clicked
                            //check if we've clicked on an army
                            bool armySelected = false;
                            foreach (Army army in p.armies)
                            {
                                if (army.iconLocation.Contains((int)q2.X, (int)q2.Y))
                                {
                                    selectedArmies.Add(army);
                                    armySelected = true;
                                }
                            }
                            if (armySelected)
                                clickSFX.Play();
                            else
                            {
                                p.setSelected();
                                prevSelectedProv = p;
                            }
                        }
                        if (mouseStateCurrent.RightButton == ButtonState.Pressed && mouseStatePrevious.RightButton == ButtonState.Released)
                        {//order all selected armies to the indicated province
                            Vector2 q1 = new Vector2();
                            Vector2 q2 = new Vector2();
                            q1.X = mouseStateCurrent.X;
                            q1.Y = mouseStateCurrent.Y;
                            q2 = camera.ScreenToWorld(q1);
                            Province p = provinces[mapMatrix[(int)q2.Y, (int)q2.X]];
                            foreach (Army army in selectedArmies)
                            {
                                army.goTo(p);
                            }
                        }

                        // Adjust zoom if the mouse wheel has moved
                        if (mouseStateCurrent.ScrollWheelValue > previousScroll)
                        {
                            camera.Zoom += 0.1f;
                        }
                        else if (mouseStateCurrent.ScrollWheelValue < previousScroll)
                        {
                            camera.Zoom -= 0.1f;
                        }
                        previousScroll = mouseStateCurrent.ScrollWheelValue;
                        // Move the camera when the pointer is near the edges
                        if (mouseStateCurrent.X < 40)
                            movement.X--;
                        if (mouseStateCurrent.X > scrollMarginRight)
                            movement.X++;
                        if (mouseStateCurrent.Y < 40)
                            movement.Y--;
                        if (mouseStateCurrent.Y > scrollMarginDown)
                            movement.Y++;

                        // Move the camera when the arrow keys are pressed
                        if (key.IsKeyDown(Keys.Left))
                            movement.X--;
                        if (key.IsKeyDown(Keys.Right))
                            movement.X++;
                        if (key.IsKeyDown(Keys.Up))
                            movement.Y--;
                        if (key.IsKeyDown(Keys.Down))
                            movement.Y++;
                        if (key.IsKeyDown(Keys.Space))
                        {
                            if (spacebarNotPressed)
                            {
                                spacebarNotPressed = false;
                                Console.WriteLine("bazoongas!");
                                /*provinces[40].owner = nations[1];
                                startX = provinces[40].startX;
                                startY = provinces[40].startY;
                                endX = provinces[40].endX;
                                endY = provinces[40].endY;
                                Thread t = new Thread(new ThreadStart(run));
                                t.Start();*/
                                //nations[3].armies[0].borderStance = Army.ANNIHILATE;
                                //nations[3].armies[0].goTo(provinces[11]);
                                //nations[1].armies[0].goTo(provinces[6]);
                            }
                        }
                        else spacebarNotPressed = true;
                        #region PrintScreen
                        if (key.IsKeyDown(Keys.PrintScreen))
                        {
                            if (prtscNotPressed)
                            {
                                prtscNotPressed = false;
                                GraphicsDevice.Clear(Color.CornflowerBlue);
                                makeMinimap();
                                RenderTarget2D screenshot = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

                                GraphicsDevice.SetRenderTarget(screenshot);
                                _draw();
                                GraphicsDevice.SetRenderTarget(null);
                                screenshot.SaveAsPng(new FileStream("screenie.png", FileMode.Create), screenW, screenH);
                            }
                        }
                        else prtscNotPressed = true;
                        #endregion
                        // Allows the game to exit
                        if (key.IsKeyDown(Keys.Escape))
                            this.Exit();

                        camera.Pos += movement * 20;
                        #endregion

                        #region Game Update
                        timeBetweenDays = timeBetweenDays.Add(gameTime.ElapsedGameTime);
                        if (timeBetweenDays.Seconds >= 5)//do the stuff necessary to pass to the next day
                        {
                            timeBetweenDays = new TimeSpan(0);
                            //cycle through each army
                            /*foreach (Nation nation in nations)
                            {
                                foreach (Army army in nation.armies)
                                {
                                    if (army.isOnBorder)
                                    {
                                        if (army.nextProvIsFriendly() || !army.targetBorder.hasDefenders())
                                        {//the border is crossed efortlessly
                                            army.isOnBorder = false;
                                            army.crrtProv = army.nextProv;
                                            army.iconLocation.X = army.crrtProv.armyX;
                                            army.iconLocation.Y = army.crrtProv.armyY;
                                        }
                                        else
                                        {//we must breach the border
                                            Neighbor border = army.targetBorder;
                                            double dmg1 = border.getDamage();
                                            double dmg2 = army.getAssaultDamage(border.hasDefenses());
                                            border.eatDamage(dmg1);
                                            army.eatAssaultDamage(dmg2, border.hasDefenses());
                                            double ch = Math.Min(95, Math.Min(5, ((1-army.borderStance)*20 - border.getDefenseRating() * border.coverage()) * 5));
                                            Console.WriteLine(dmg1+" "+dmg2+" "+ch);
                                            if (rand.Next(100) < ch)//the border is breached
                                            {
                                                army.isOnBorder = false;
                                                army.crrtProv = army.nextProv;
                                                army.iconLocation.X = army.crrtProv.armyX;
                                                army.iconLocation.Y = army.crrtProv.armyY;
                                            }
                                        }
                                    }
                                    if (army.nextProv != army.crrtProv)//inseamna ca ma duc undeva
                                    {
                                        if (army.distToNext == 0)//we have arrived at the border with the next province
                                        {//effects to be refined
                                            //army.crrtProv = army.nextProv;
                                            army.iconLocation.X = army.targetBorder.armyX;
                                            army.iconLocation.Y = army.targetBorder.armyY;
                                            army.isOnBorder = true;
                                        }
                                        army.march();
                                    }
                                }
                            }*/
                            date.next();
                            //Console.WriteLine("A day has passed!");
                        }
                        #endregion
                    }
                    break;

                case MAIN_MENU:
                    {
                        if (mouseStateCurrent.LeftButton == ButtonState.Pressed && mouseStatePrevious.LeftButton == ButtonState.Released)
                        {//we've clicked on something
                            int x = mouseStateCurrent.X, y = mouseStateCurrent.Y;
                            if (newGameRect.Contains(x, y))
                            {
                                clickSFX.Play();
                                gameState = IN_GAME;
                                List<Song> music = new List<Song>();
                                music.Add(Content.Load<Song>("Caesar 3 Soundtrack - Rome 1"));
                                music.Add(Content.Load<Song>("Caesar 3 Soundtrack - Rome 2"));
                                music.Add(Content.Load<Song>("Caesar 3 Soundtrack - Rome 3"));
                                music.Add(Content.Load<Song>("Caesar 3 Soundtrack - Rome 4"));
                                music.Add(Content.Load<Song>("Caesar 3 Soundtrack - Rome 5"));
                                musicPlayer.newPlaylist(music);
                            }
                            if (optionsRect.Contains(x, y))
                            {
                                clickSFX.Play();
                                gameState = OPTIONS_MENU;
                                optionsMenu = new OptionsMenu(screenW, screenH, GraphicsDevice, spriteBatch, this, musicPlayer.getVolume());
                            }
                            if (quitRect.Contains(x, y))
                            {
                                clickSFX.Play();
                                this.Exit();
                            }
                        }
                    }
                    break;

                case OPTIONS_MENU:
                    {
                        optionsMenu.update(key, mouseStateCurrent);
                    }
                    break;
            }
            mouseStatePrevious = mouseStateCurrent;

            musicPlayer.wazzap();//poke, poke (check that the music is still playing)
            base.Update(gameTime);
        }

        // called by OptionsMenu to change resolution 'n shit
        internal void applySettings()
        {
            if (optionsMenu.resolutionChanged)
            {
                screenW = graphics.PreferredBackBufferWidth = optionsMenu.supportedResolutions[optionsMenu.currentResolution].Width;
                screenH = graphics.PreferredBackBufferHeight = optionsMenu.supportedResolutions[optionsMenu.currentResolution].Height;
                graphics.ApplyChanges();
                scrollMarginRight = screenW - 40;
                scrollMarginDown = screenH - 40;
                camera.setViewport(screenW, screenH);
                newGameRect = new Rectangle((int)(screenW * 0.45), (int)(screenH * 0.4), (int)(screenW * 0.1), (int)(screenH * 0.05));
                optionsRect = new Rectangle((int)(screenW * 0.45), (int)(screenH * 0.5), (int)(screenW * 0.1), (int)(screenH * 0.05));
                quitRect = new Rectangle((int)(screenW * 0.45), (int)(screenH * 0.6), (int)(screenW * 0.1), (int)(screenH * 0.05));
                optionsMenu.reset(screenW, screenH);
                minimapTexture = new RenderTarget2D(GraphicsDevice, screenW, screenH, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            }
            //modifica audio
            musicPlayer.setVolume(optionsMenu.musicVolume.Value / 100);
            soundVolume = optionsMenu.soundVolume.Value / 100;
            clickSFX.Volume = soundVolume;
            //modifica setarile default din config
            StreamWriter config = new StreamWriter("config.ini");
            config.WriteLine("Width=" + screenW);
            config.WriteLine("Height=" + screenH);
            config.WriteLine("MusicVolume=" + musicPlayer.getVolume());
            config.WriteLine("SoundVolume=" + soundVolume);
            config.Close();
        }
    }
}
