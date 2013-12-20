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
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Viewport defaultViewport = new Viewport();

        // sound stuff
        private Utilities.MusicPlayer musicPlayer;

        // textures
        private Texture2D map00, map01, map02, map10, map11, map12;
        private Texture2D prov00, prov01, prov02, prov10, prov11, prov12;
        private Rectangle rect00, rect01, rect02, rect10, rect11, rect12, ui1;
        private Texture2D uiBackground;//the texture which will serve as background for all UI elements
        private Rectangle uiStatusBarRect;//the portion of the texture used for the status bar
        //private Texture2D uiProvinceBarBackground;
        //private Rectangle uiProvinceBarRect;
        private Texture2D coin;//money gfx
        private SpriteFont font;

        // other stuff
        private bool isActive;//if I alt-Tab, then the game deactivates and no longer responds to input
        private int screenH, screenW, scrollMarginRight, scrollMarginDown;//vezi functia de scroll
        private Vector2 spritePosition = Vector2.Zero;
        private Vector2 spriteSpeed = new Vector2(50.0f, 50.0f);
        private _2DCamera Camera = new _2DCamera(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, 4400, 2727, 1f);
        private float previousScroll = 0f;
        private int startX, startY, endX, endY;//needed when updating a province's color
        public bool REMOVE = true;// TO BE REMOVED
        public bool smth = true;// TO BE REMOVED
        private TimeSpan timeBetweenDays = new TimeSpan(0);//the timespan that must elapse before we switch over to the next day
        private static Utilities.Date date;//aici tinem minte data curenta
        // game variables - aici tinem vectori cu datele care trebuie tinute minte doar o data, intr-un singur loc
        internal static Province[] provinces;
        internal static Nation[] nations;
        internal static DefenseBuilding[] defBuildings;
        internal static UnitStats[] unitStats;
        internal static Tactic[] meleeTactics, skirmishTactics;
        internal static byte[,] mapMatrix;// aici stocam info despre carei provincii ii apartine pixelul i, j
        //ne intereseaza atat la desenarea provinciilor, cat si pentru a determina pe ce provincie am dat click
        internal static byte player = 1;//index in vectorul de natiuni (momentan doar WRE e jucabil)
        private Random rand = new Random();
        public MouseState mouseStateCurrent, mouseStatePrevious;

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
            //lastSelectedProvince = null;
        }

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

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;
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
            defaultViewport = GraphicsDevice.Viewport;

            //put up a loading screen
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //spriteBatch.Begin();
            //spriteBatch.Draw(Texture2D.FromStream(GraphicsDevice, new FileStream("graphics/loading screen.png", FileMode.Open)), new Rectangle(0, 0, screenW, screenH), Color.White);
            //spriteBatch.End();
            
            Nation.readNations();
            for(int i=1;i<nations.Length;i++)// load army icons as textures (we start from 1 to skip Sea)
                nations[i].armyIcon = Texture2D.FromStream(GraphicsDevice, new FileStream("graphics/army icons/" + nations[i].name + " icon.png", FileMode.Open));
            Province.readProvinces();
            DefenseBuilding.readDefBuildings();
            UnitStats.readUnitStats();
            Tactic.readMeleeTactics();
            Tactic.readSkirmishTactics();
            readScenario();
            GraphicsDevice.Clear(Color.CornflowerBlue);
            map00 = Content.Load<Texture2D>("00");
            map01 = Content.Load<Texture2D>("01");
            map02 = Content.Load<Texture2D>("02");
            map10 = Content.Load<Texture2D>("10");
            map11 = Content.Load<Texture2D>("11");
            map12 = Content.Load<Texture2D>("12");
            //uiProvinceBarBackground = Content.Load<Texture2D>("scroll.png");
            rect00 = new Rectangle(0, 0, map00.Width, map00.Height);
            rect01 = new Rectangle(map00.Width, 0, map01.Width, map01.Height);
            rect02 = new Rectangle(map00.Width + map01.Width, 0, map02.Width, map02.Height);
            rect10 = new Rectangle(0, map00.Height, map10.Width, map10.Height);
            rect11 = new Rectangle(map10.Width, map01.Height, map11.Width, map11.Height);
            rect12 = new Rectangle(map10.Width + map11.Width, map02.Height, map12.Width, map12.Height);
            uiBackground = Texture2D.FromStream(GraphicsDevice, new FileStream("graphics/fish_mosaic.jpg", FileMode.Open));
            uiStatusBarRect = new Rectangle(0, 0, 300, 50);
            //uiProvinceBarRect = new Rectangle(0, 0, screenW, (int)screenH/5);
            coin = Texture2D.FromStream(GraphicsDevice, new FileStream("graphics/coin.png", FileMode.Open));
            font = Content.Load<SpriteFont>("SpriteFont1");

            MemoryStream[] provTextureStream = new MemoryStream[6];
            mapMatrix = Utilities.ImageProcessor.createMapMatrix(provTextureStream);
            prov00 = Texture2D.FromStream(GraphicsDevice, provTextureStream[0]);
            prov01 = Texture2D.FromStream(GraphicsDevice, provTextureStream[1]);
            prov02 = Texture2D.FromStream(GraphicsDevice, provTextureStream[2]);
            prov10 = Texture2D.FromStream(GraphicsDevice, provTextureStream[3]);
            prov11 = Texture2D.FromStream(GraphicsDevice, provTextureStream[4]);
            prov12 = Texture2D.FromStream(GraphicsDevice, provTextureStream[5]);
            for (int i = 0; i < 6; i++)
            {
                provTextureStream[i].Close();
                provTextureStream[i] = null;
            }

            //initialize playlist
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
            
            Vector2 movement = Vector2.Zero;
            Viewport vp = GraphicsDevice.Viewport;

            mouseStateCurrent = Mouse.GetState();
            if (mouseStateCurrent.LeftButton == ButtonState.Pressed && mouseStatePrevious.LeftButton == ButtonState.Released)
            {


                Utilities.ImageProcessor.updateMap(mapMatrix, startX, startY, endX, endY, prov00, prov01, prov02, prov10, prov11, prov12);
                Vector2 q1 = new Vector2();
                Vector2 q2 = new Vector2();
                q1.X = mouseStateCurrent.X;
                q1.Y = mouseStateCurrent.Y;
                q2 = Camera.ScreenToWorld(q1);
                Province p = provinces[mapMatrix[(int)q2.Y, (int)q2.X]];
                p.isSelected = true;
                Utilities.ImageProcessor.updateMap(mapMatrix, p.startX, p.startY, p.endX, p.endY, prov00, prov01, prov02, prov10, prov11, prov12);
                p.isSelected = false;
                startX = p.startX;
                startY = p.startY; 
                endX = p.endX; 
                endY = p.endY;
               
            }
            mouseStatePrevious = mouseStateCurrent;

            
            // Adjust zoom if the mouse wheel has moved
            if (mouseStateCurrent.ScrollWheelValue > previousScroll)
            {
                Camera.Zoom += 0.1f;
            }
            else if (mouseStateCurrent.ScrollWheelValue < previousScroll)
                 { 
                    Camera.Zoom -= 0.1f; 
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
            if (REMOVE && key.IsKeyDown(Keys.Space))
            {
                REMOVE = false;
                provinces[40].owner = nations[1];
                startX = provinces[40].startX;
                startY = provinces[40].startY;
                endX = provinces[40].endX;
                endY = provinces[40].endY;
                Thread t = new Thread(new ThreadStart(run));
<<<<<<< HEAD
                t.Start();
                //nations[3].armies[0].borderStance = Army.ANNIHILATE;
                //nations[3].armies[0].goTo(provinces[11]);
=======
                t.Start();*/
                nations[1].armies[0].goTo(provinces[6]);
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
>>>>>>> 514ff1315fcb8801acc2e5e97947ee7bab39b657
=======
>>>>>>> 514ff1315fcb8801acc2e5e97947ee7bab39b657
=======
>>>>>>> 514ff1315fcb8801acc2e5e97947ee7bab39b657
=======
>>>>>>> 514ff1315fcb8801acc2e5e97947ee7bab39b657
            }
            // Allows the game to exit
            if (key.IsKeyDown(Keys.Escape))
                this.Exit();

            Camera.Pos += movement * 20;
            musicPlayer.wazzap();//poke, poke
            base.Update(gameTime);
            //Console.WriteLine(gameTime.ElapsedGameTime);
            timeBetweenDays = timeBetweenDays.Add(gameTime.ElapsedGameTime);
            //Console.WriteLine(timeBetweenDays);
            if (timeBetweenDays.Seconds >= 5)//do the stuff necessary to pass to the next day
            {
                timeBetweenDays = new TimeSpan(0);
                //cycle through each army
                foreach (Nation nation in nations)
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
                }
                date.next();
                //Console.WriteLine("A day has passed!");
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!isActive) return;//no need to draw when the game is not in focus
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null, Camera.GetTransformation());

            // Draw our images
            // spriteBatch.Draw( texture to draw, rectangle in which to draw, what part of the image to draw (null means all of it),
            //      Color with which to tint the texture (White means no modification), rotation, sprite origin, sprite effects,
            //      layer depth (in the FrontToBack order, 0.0 lies at the back and 1.0 at the front) )
            spriteBatch.Draw(prov00, rect00, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(prov01, rect01, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(prov02, rect02, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(prov10, rect10, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(prov11, rect11, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(prov12, rect12, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(map00, rect00, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            spriteBatch.Draw(map01, rect01, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            spriteBatch.Draw(map02, rect02, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            spriteBatch.Draw(map10, rect10, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            spriteBatch.Draw(map11, rect11, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            spriteBatch.Draw(map12, rect12, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            //draw the armies of the world
            foreach (Nation nation in nations)
            {
                foreach (Army army in nation.armies)
                {
                    spriteBatch.Draw(nation.armyIcon, army.iconLocation, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                }
            }
            //draw the status bar

            spriteBatch.Draw(uiBackground, new Rectangle((int)(Camera.Pos.X + (screenW / 2  - 300)/Camera.Zoom), (int)(Camera.Pos.Y - (screenH / 2)/Camera.Zoom), (int)(300 / Camera.Zoom), (int)(50 / Camera.Zoom)), uiStatusBarRect, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.95f);
            //spriteBatch.Draw(uiProvinceBarBackground, new Rectangle((int)(Camera.Pos.X - screenW/ (2 * Camera.Zoom)), (int)(Camera.Pos.Y + (screenH * 0.2) * Camera.Zoom), (int)(screenW / Camera.Zoom), (int)(screenW / (5 * Camera.Zoom))), uiProvinceBarRect, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.95f);
            spriteBatch.Draw(coin, new Rectangle((int)(Camera.Pos.X + (screenW / 2 - 290)/Camera.Zoom), (int)(Camera.Pos.Y - (screenH / 2 - 10)/Camera.Zoom ), (int)(30 /Camera.Zoom ), (int)(30/Camera.Zoom)), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(font, nations[player].money + "", new Vector2(Camera.Pos.X + (screenW / 2 - 250) / Camera.Zoom, Camera.Pos.Y - (screenH / 2 - 10) / Camera.Zoom), Color.White, 0.0f, Vector2.Zero, 1.0f / Camera.Zoom, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(font, date.ToString(), new Vector2(Camera.Pos.X + (screenW / 2 - 100) / Camera.Zoom, Camera.Pos.Y - (screenH / 2 - 10) / Camera.Zoom), Color.White, 0.0f, Vector2.Zero, 1.0f / Camera.Zoom, SpriteEffects.None, 1.0f);
            
            spriteBatch.End();
        }

        // from here - content loading functions called just once in the loadContent phase
<<<<<<< HEAD
=======
        private static void readNations(){
            int n;
            StreamReader file = new System.IO.StreamReader("nations.txt");
            String s = file.ReadLine();
            while (s.StartsWith("#"))//while commentary, skip over it
                s = file.ReadLine();
            n = Convert.ToInt32(s);//get nr of nations
            nations = new Nation[n];
            for(int i=0;i<n;i++){
                s = file.ReadLine();
                while (s.StartsWith("#"))
                    s = file.ReadLine();
                String[] word = s.Split(';');
                nations[i] = new Nation(
                        word[0],//name
                        System.Drawing.Color.FromArgb(Convert.ToInt32(word[1]), Convert.ToInt32(word[2]), Convert.ToInt32(word[3])),
                        Convert.ToInt32(word[4])//money
                        );
            }
            file.Close();
        }

        private static void readProvinces(){
            int n;
            StreamReader file = new System.IO.StreamReader("provinces.txt");
            String s = file.ReadLine();
            while (s.StartsWith("#"))
                s = file.ReadLine();
            n = Convert.ToInt32(s);//get nr of provinces
            provinces = new Province[n];
            for(int i=0;i<n;i++){
                s = file.ReadLine();
                while (s.StartsWith("#"))
                    s = file.ReadLine();
                if(s.EndsWith("{")){//incepe bloc descriere provincie urmatoare
                    s = file.ReadLine();
                    while (s.StartsWith("#"))
                        s = file.ReadLine();
                    String[] word = s.Split(';');
                    provinces[i] = new Province(Convert.ToInt32(word[0]), word[1], Convert.ToInt32(word[2]), Convert.ToInt32(word[3]), Convert.ToInt32(word[4]), Convert.ToInt32(word[5]), Convert.ToInt32(word[6]), Convert.ToInt32(word[7]));
                    s = file.ReadLine();
                    while(!s.StartsWith("}")){//pana se termina descrierea, citim despre vecini
                        while (s.StartsWith("#"))
                            s = file.ReadLine();
                        if (s.StartsWith("}")) break;
                        word = s.Split(';');
                        provinces[i].neighbors.Add(new Neighbor(provinces[Convert.ToInt32(word[0])], Convert.ToInt32(word[1]), Convert.ToInt32(word[2]), Convert.ToInt32(word[3]), Convert.ToInt32(word[4]), Convert.ToInt32(word[5])));
                        s = file.ReadLine();
                    }
                }
            }
            file.Close();
        }

        private static void readDefBuildings(){
            StreamReader file = new System.IO.StreamReader("defenseBuildings.txt");
            String s = file.ReadLine();
            while (s.StartsWith("#"))
                s = file.ReadLine();
            int n = Convert.ToInt32(s);
            defBuildings = new DefenseBuilding[n];
            for(int i=0;i<n;i++){
                s = file.ReadLine();
                while (s.StartsWith("#"))
                    s = file.ReadLine();
                String[] word = s.Split(';');
                defBuildings[i] = new DefenseBuilding(word[0], Convert.ToInt32(word[1]), Convert.ToInt32(word[2]));
            }
            file.Close();
        }

        private static void readUnitStats(){
            StreamReader file = new System.IO.StreamReader("units.txt");
            String s = file.ReadLine();
            while (s.StartsWith("#"))
                s = file.ReadLine();
            int n = Convert.ToInt32(s);
            unitStats = new UnitStats[n];
            for(int i=0;i<n;i++){
                s = file.ReadLine();
                while (s.StartsWith("#"))
                    s = file.ReadLine();
                String[] word = s.Split(';');
                unitStats[i] = new UnitStats(word[0].Replace('_',' '), Convert.ToByte(word[1]), Convert.ToByte(word[2]), Convert.ToByte(word[3]), Convert.ToByte(word[4]), Convert.ToByte(word[5]), Convert.ToByte(word[6]), Convert.ToInt32(word[7]), Convert.ToInt32(word[8]), (float)Convert.ToDouble(word[9]));
            }
            file.Close();
        }

>>>>>>> 514ff1315fcb8801acc2e5e97947ee7bab39b657
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
                    provinces[i].owner = nations[Convert.ToInt32(word[1])];
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

        private void run()
        {
            Utilities.ImageProcessor.updateMap(mapMatrix, 0, 0, 4400, 2727, prov00, prov01, prov02, prov10, prov11, prov12);
        }
    }
}
