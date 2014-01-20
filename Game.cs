using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace joc_cu_romani_si_barbari
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        #region Variable declarations
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // sound stuff
        private Utilities.MusicPlayer musicPlayer;

        // textures
        private Texture2D[] mapTexture;//the 2048x2048 (or less) rectangles forming our map
        private Rectangle[] mapPosition;
        private int mapTextureLength;
        private Texture2D uiBackground;//the texture which will serve as background for all UI elements
        private Rectangle uiStatusBarRect;//the portion of the texture used for the status bar
        //private Texture2D uiProvinceBarBackground;
        //private Rectangle uiProvinceBarRect;
        private Texture2D coin;//money gfx
        private SpriteFont font;
        private RenderTarget2D minimapTexture;//http://rbwhitaker.wikidot.com/render-to-texture

        //input control
        private bool spacebarNotPressed = true, prtscNotPressed = true;
        private MouseState mouseStateCurrent, mouseStatePrevious;

        // other stuff
        private bool isActive;//if I alt-Tab, then the game deactivates and no longer responds to input
        private int screenH, screenW, scrollMarginRight, scrollMarginDown;//vezi functia de scroll
        private Vector2 spritePosition = Vector2.Zero;
        private Vector2 spriteSpeed = new Vector2(50.0f, 50.0f);
        private _2DCamera camera;
        private float previousScroll = 0f;
        private TimeSpan timeBetweenDays = new TimeSpan(0);//the timespan that must elapse before we switch over to the next day
        private static Utilities.Date date;//aici tinem minte data curenta
        private Random rand = new Random();
        private Province prevSelectedProv = null;

        // gameplay variables - aici tinem vectori cu datele care trebuie tinute minte doar o data, intr-un singur loc
        internal static Province[] provinces;
        internal static Nation[] nations;
        internal static DefenseBuilding[] defBuildings;
        internal static UnitStats[] unitStats;
        internal static Tactic[] meleeTactics, skirmishTactics;
        internal static byte[,] mapMatrix;// aici stocam info despre carei provincii ii apartine pixelul i, j
        //ne intereseaza atat la desenarea provinciilor, cat si pentru a determina pe ce provincie am dat click
        internal static byte player = 1;//index in vectorul de natiuni (momentan doar WRE e jucabil)
        #endregion

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            screenW = this.graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            screenH = this.graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            this.graphics.IsFullScreen = true;
            scrollMarginRight = screenW - 40;
            scrollMarginDown = screenH - 40;
            mouseStatePrevious = Mouse.GetState();
        }

        #region Alt-Tab management
        protected override void OnActivated(object sender, EventArgs args)
        {
            base.OnActivated(sender, args);
            isActive = true;
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            base.OnDeactivated(sender, args);
            isActive = false;
        }
        #endregion

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            minimapTexture = new RenderTarget2D(GraphicsDevice,
                            GraphicsDevice.PresentationParameters.BackBufferWidth,
                            GraphicsDevice.PresentationParameters.BackBufferHeight,
                            false,
                            GraphicsDevice.PresentationParameters.BackBufferFormat,
                            DepthFormat.Depth24);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Read text files to form data cache
            Nation.readNations(GraphicsDevice);
            Province.readProvinces(GraphicsDevice);
            DefenseBuilding.readDefBuildings();
            UnitStats.readUnitStats();
            Tactic.readMeleeTactics();
            Tactic.readSkirmishTactics();
            readScenario();
            // Load other textures
            uiBackground = Texture2D.FromStream(GraphicsDevice, new FileStream("graphics/fish_mosaic.jpg", FileMode.Open));
            uiStatusBarRect = new Rectangle(100, 100, 300, 50);
            coin = Texture2D.FromStream(GraphicsDevice, new FileStream("graphics/coin.png", FileMode.Open));
            font = Content.Load<SpriteFont>("SpriteFont1");
            // Load map matrix from map.bin
            int[] dim = new int[2];
            mapMatrix = Utilities.ImageProcessor.readMapMatrix(dim);
            // Load map textures
            int w = dim[0], h = dim[1], crrtX = 0, crrtY = 0;
            mapTextureLength = 0;
            mapTexture = new Texture2D[6];
            mapPosition = new Rectangle[6];
            int maxLength = 6;
            /* The basic ideea is this: starting with (0,0) we load consecutive rectangles on the same row
             *  when the row is exhausted, we move on to the next row and so on until all rows are exhausted
             */
            while (crrtY != h)
            {
                if (mapTextureLength == maxLength)//we need to allocate further memory
                {
                    maxLength *= 2;
                    Texture2D[] newMapTexture = new Texture2D[maxLength];
                    mapTexture.CopyTo(newMapTexture, 0);
                    mapTexture = newMapTexture;
                    Rectangle[] newMapPosition = new Rectangle[maxLength];
                    mapPosition.CopyTo(newMapPosition, 0);
                    mapPosition = newMapPosition;
                }
                int width = crrtX + 2048 > w ? w - crrtX : 2048;//we try to load a 2048 x 2048 rectangle; if we're nearing an edge
                int height = crrtY + 2048 > h ? h - crrtY : 2048;//we settle for less
                mapTexture[mapTextureLength] = Texture2D.FromStream(GraphicsDevice, new FileStream("graphics\\map\\" + mapTextureLength + ".png", FileMode.Open));
                mapPosition[mapTextureLength] = new Rectangle(crrtX, crrtY, width, height);
                mapTextureLength++;
                crrtX += width;
                if (crrtX == w)
                {
                    crrtX = 0;
                    crrtY += height;
                }
            }
            // Initialize the camera with the map's dimensions
            camera = new _2DCamera(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, dim[0], dim[1], 1f, 0.5f, 1f);

            // Initialize playlist
            List<Song> music = new List<Song>();
            music.Add(Content.Load<Song>("Caesar 3 Soundtrack - Rome 1"));
            music.Add(Content.Load<Song>("Caesar 3 Soundtrack - Rome 2"));
            music.Add(Content.Load<Song>("Caesar 3 Soundtrack - Rome 3"));
            music.Add(Content.Load<Song>("Caesar 3 Soundtrack - Rome 4"));
            music.Add(Content.Load<Song>("Caesar 3 Soundtrack - Rome 5"));
            musicPlayer = new Utilities.MusicPlayer(music);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!isActive) return;//the game doesn't respond to input
            KeyboardState key = Keyboard.GetState();
            mouseStateCurrent = Mouse.GetState();
            Vector2 movement = Vector2.Zero;

            #region Responding to input
            if (mouseStateCurrent.LeftButton == ButtonState.Pressed && mouseStatePrevious.LeftButton == ButtonState.Released)
            {//we've clicked on something
                if (prevSelectedProv != null)
                    prevSelectedProv.setDeselected();
                Vector2 q1 = new Vector2();
                Vector2 q2 = new Vector2();
                q1.X = mouseStateCurrent.X;
                q1.Y = mouseStateCurrent.Y;
                q2 = camera.ScreenToWorld(q1);
                Province p = provinces[mapMatrix[(int)q2.Y, (int)q2.X]];
                p.setSelected();
                prevSelectedProv = p;
            }
            mouseStatePrevious = mouseStateCurrent;
            
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
            musicPlayer.wazzap();//poke, poke (check that the music is still playing)
            base.Update(gameTime);

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
            for(int i=1;i<provinces.Length;i++)
                spriteBatch.Draw(provinces[i].background, provinces[i].position, null, provinces[i].color, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            for(int i=0;i<mapTextureLength;i++)
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
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null, camera.GetTransformation());
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
            spriteBatch.End();

            //desenam elementele statice in raport cu camera (adica nu sunt afectate de ViewMatrix-ul camerei)
            spriteBatch.Begin();
            spriteBatch.Draw(uiBackground, new Rectangle(screenW - 300, 0, 300, 50), uiStatusBarRect, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.95f);
            spriteBatch.Draw(coin, new Rectangle(screenW - 290, 10, 30, 30), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(font, nations[player].money + "", new Vector2(screenW - 250, 10), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(font, date.ToString(), new Vector2(screenW - 100, 10), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(minimapTexture, new Rectangle(screenW - 300, 50, 300, 150), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.End();
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!isActive) return;//no need to draw when the game is not in focus
            GraphicsDevice.Clear(Color.Black);
            makeMinimap();
            _draw();
        }

        // from here - content loading functions called just once in the loadContent phase
        private static void readScenario(){
            char[] separator = {' ', ';'};
            StreamReader file = new System.IO.StreamReader("scenario.txt");
            //citesc data de inceput a jocului
            String s = file.ReadLine();
            while (s.StartsWith("#"))
                s = file.ReadLine();
            String[] word = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
            date = new Utilities.Date(Convert.ToInt32(word[0]), Convert.ToInt32(word[1]), Convert.ToInt32(word[2]));
            //citesc informatiile despre fiecare provincie in parte
            for(int i=0;i<provinces.Length;i++){
                s = file.ReadLine();
                while (s.StartsWith("#"))
                    s = file.ReadLine();
                if(s.EndsWith("{")){//incepe bloc descriere provincie urmatoare
                    s = file.ReadLine();
                    while(s.StartsWith("#"))
                        s = file.ReadLine();
                    word = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                    provinces[i].prosperity = Convert.ToSingle(word[0]);
                    provinces[i].setOwner(nations[Convert.ToInt32(word[1])]);
                    //poate urma descrierea unei armate
                    s = file.ReadLine();
                    while(s.StartsWith("#"))
                        s = file.ReadLine();
                    if(s.EndsWith("{")){//is army
                        s = file.ReadLine();
                        while(s.StartsWith("#"))
                            s = file.ReadLine();
                        word = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                        String armyName = word[0].Replace('_', ' ');
                        Nation nat = nations[Convert.ToInt32(word[1])];
                        Army a = new Army(armyName, nat, provinces[i]);
                        nat.armies.Add(a);//the army must be added to both the nation and the province
                        provinces[i].armies.Add(a);
                        while(!s.StartsWith("}")){//cetim unitati pana la finish
                            s = file.ReadLine();
                            while(s.StartsWith("#"))
                                s = file.ReadLine();
                            if(s.EndsWith("}")){
                                s = file.ReadLine();//urmeaza vecinii, si trebuie sa furnizam o linie valida
                                while(s.StartsWith("#"))
                                    s = file.ReadLine();
                                break;
                            }
                            word = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                            a.addUnit(new Unit(unitStats[Convert.ToInt32(word[0])], Convert.ToInt32(word[1])));
                        }
                    }//fie armata, fie nu, urmeaza vecinii
                    foreach (Neighbor n in provinces[i].neighbors) {
                        word = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                        int lvl = Convert.ToInt32(word[0]);
                        if (lvl != 0)
                            n.addDefBuilding(defBuildings[lvl-1]);
                        s = file.ReadLine();
                        while (s.StartsWith("#"))
                            s = file.ReadLine();
                        if (s.EndsWith("{")) {//defensive army
                            while (!s.StartsWith("}")) {//cetim unitati pana la finish
                                s = file.ReadLine();
                                while (s.StartsWith("#"))
                                    s = file.ReadLine();
                                if (s.EndsWith("}"))
                                {
                                    s = file.ReadLine();
                                    while (s.StartsWith("#"))
                                        s = file.ReadLine();
                                    break;
                                }
                                word = s.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                                n.addDefUnit(new Unit(unitStats[Convert.ToInt32(word[0])], Convert.ToInt32(word[1])));
                            }
                        }
                    }
                }
            }
            file.Close();
        }
    }
}
