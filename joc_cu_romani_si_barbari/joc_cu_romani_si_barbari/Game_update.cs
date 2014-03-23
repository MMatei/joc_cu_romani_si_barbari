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
            KeyboardState keyCurrent = Keyboard.GetState();
            MouseState mouseStateCurrent = Mouse.GetState();

            switch (gameState)
            {
                case IN_GAME:
                    {
                        #region Responding to input
                        Vector2 movement = Vector2.Zero;

                        if (!uiProvinceDetailRect.Contains(mouseStateCurrent.X, mouseStateCurrent.Y))
                        {//only register province selection if the button press happens outside the details scroll
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
                                {
                                    prevSelectedProv = null;
                                    clickSFX.Play();
                                    //am selectat armate => afisare corespunzatoare pe pergament => trebuie setat textul corespunzator
                                    String text = "Selected armies\n";
                                    foreach (Army army in selectedArmies)
                                    {
                                        text += army.name + "\n";
                                    }
                                    armiesSelectedTextArea.setText(text);
                                }
                                else
                                {
                                    p.setSelected();
                                    prevSelectedProv = p;
                                    //am selectat prov => trebuie afisate zone de text pe pergament => trebuie setat textul pt ele
                                    String text = "Stationed armies\n";
                                    prevSelectedProv.armies.ForEach(delegate(Army army)
                                    {
                                        text += army.name + "\n";
                                    });
                                    armiesInProvTextArea.setText(text);
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
                                    neighborsTextArea.setText(txt);
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
                        if (keyCurrent.IsKeyDown(Keys.Left))
                            movement.X--;
                        if (keyCurrent.IsKeyDown(Keys.Right))
                            movement.X++;
                        if (keyCurrent.IsKeyDown(Keys.Up))
                            movement.Y--;
                        if (keyCurrent.IsKeyDown(Keys.Down))
                            movement.Y++;
                        #region PrintScreen
                        if (keyCurrent.IsKeyDown(Keys.PrintScreen) && keyPrevious.IsKeyUp(Keys.PrintScreen))
                        {
                            RenderTarget2D screenshot = new RenderTarget2D(GraphicsDevice, screenW, screenH, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
                            GraphicsDevice.SetRenderTarget(screenshot);
                            _draw();
                            GraphicsDevice.SetRenderTarget(null);
                            screenshot.SaveAsPng(new FileStream("screenie.png", FileMode.Create), screenW, screenH);
                        }
                        #endregion
                        // Allows the game to exit
                        if (keyCurrent.IsKeyDown(Keys.Escape))
                            this.Exit();

                        camera.Pos += movement * 20;
                        #endregion

                        armiesInProvTextArea.update(mouseStateCurrent);
                        neighborsTextArea.update(mouseStateCurrent);
                        armiesSelectedTextArea.update(mouseStateCurrent);

                        #region Game Update
                        timeBetweenDays = timeBetweenDays.Add(gameTime.ElapsedGameTime);
                        if (timeBetweenDays.Seconds >= 5)//do the stuff necessary to pass to the next day
                        {
                            timeBetweenDays = new TimeSpan(0);
                            //cycle through each army
                            foreach (Nation nation in nations)
                            {
                                foreach (Army army in nation.armies)
                                {
                                    if (army.state == ArmyState.ON_BORDER)
                                    {
                                        if (army.nextProvIsFriendly())
                                        {//the border is crossed efortlessly
                                            army.state = ArmyState.IN_FRIENDLY_PROVINCE;
                                            army.provinceEntered();
                                        }
                                        else if (!army.targetBorder.otherSide.hasDefenders())
                                        {//the border is crossed efortlessly
                                            army.state = ArmyState.IN_ENEMY_PROVINCE;
                                            army.provinceEntered();
                                        }
                                        else
                                        {//we must breach the border
                                            Neighbor border = army.targetBorder.otherSide;
                                            double dmg1 = border.getDamage();
                                            double dmg2 = army.getAssaultDamage(border.hasDefenses());
                                            border.eatDamage(dmg1);
                                            army.eatAssaultDamage(dmg2, border.hasDefenses());
                                            //the chance to breach the border depends on the army's stance, the saturation of the border with defenders
                                            //and the defensive strength of the border
                                            double ch = (1 - army.borderStance) * 100 - border.getDefenseRating() * border.coverage();
                                            //Console.WriteLine(1 - army.borderStance + " " + border.coverage() + " " + border.getDefenseRating());
                                            Console.WriteLine(dmg1 + " " + dmg2 + " " + ch);
                                            if (rand.Next(100) < ch)//the border is breached
                                            {
                                                army.state = ArmyState.IN_ENEMY_PROVINCE;
                                                army.provinceEntered();
                                            }
                                        }
                                    }
                                    else //I am in a province
                                    if (army.distToMarch != 0)//inseamna ca ma duc undeva
                                    {
                                        army.distToMarch -= 100;//TODO : vary the mvmnt speed
                                        if (army.distToMarch <= 0)//we have arrived at the border with the next province
                                        {
                                            if (army.targetBorder == null) //the army has reached the ctr of the prov
                                            {//if path continues, then select next targetBorder
                                                if (army.path != null)
                                                    army.goToNextProvInPath();
                                                //regardless, change position to be center of prov
                                                army.iconLocation.X = army.crrtProv.armyX;
                                                army.iconLocation.Y = army.crrtProv.armyY;
                                            }
                                            else //the army has reached the target border
                                            {
                                                army.iconLocation.X = army.targetBorder.armyX;
                                                army.iconLocation.Y = army.targetBorder.armyY;
                                                army.state = ArmyState.ON_BORDER;
                                            }
                                        }
                                    }
                                }
                            }
                            date.next();
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
                        optionsMenu.update(keyCurrent, mouseStateCurrent);
                    }
                    break;
            }
            mouseStatePrevious = mouseStateCurrent;
            keyPrevious = keyCurrent;

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
