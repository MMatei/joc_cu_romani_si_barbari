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
    // This is the main type for your game
    // This file contains variable declarations, initialization and content loading
    public partial class Game : Microsoft.Xna.Framework.Game
    {
        #region Variable declarations
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // sound stuff
        private Utilities.MusicPlayer musicPlayer;
        internal float soundVolume;
        internal SoundEffectInstance clickSFX;

        // textures
        private Texture2D[] mapTexture;//the 2048x2048 (or less) rectangles forming our map
        private Rectangle[] mapPosition;
        private int mapTextureLength;
        private Texture2D uiBackground;//the texture which will serve as background for all UI elements
        private Rectangle uiStatusBarRect;//the portion of the texture used for the status bar
        private Texture2D selectHalo;
        //private Texture2D uiProvinceBarBackground;
        //private Rectangle uiProvinceBarRect;
        private Texture2D coin;//money gfx
        internal SpriteFont font;
        private RenderTarget2D minimapTexture;//http://rbwhitaker.wikidot.com/render-to-texture

        //input control
        private bool spacebarNotPressed = true, prtscNotPressed = true;
        internal MouseState mouseStatePrevious;

        // Game State - we need to keep track of where we are to know what to draw and what input to receive
        public byte gameState;
        public const byte MAIN_MENU = 1;
        public const byte OPTIONS_MENU = 2;
        public const byte IN_GAME = 3;

        // Main Menu variables
        private Rectangle newGameRect, optionsRect, quitRect;
        internal Texture2D mainMenuTexture;
        private OptionsMenu optionsMenu;

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
        /// <summary>
        /// now here we have a bit of cultural shock ;))
        /// you see, C# Convert functions take into consideration cultural aspects when converting
        /// thus we must ensure a standard; this is important for floats, where there are varying notations, such as 1,0 and 1.0
        /// IMPORTANT: we don't use cultureInfo for config.ini, because some pinhead at M$ decided it'd be a good ideea
        /// if WriteLine took into consideration culture by default, while at the same time not providing a way to change this moronic behaviour
        /// as a bottom line, use culturalInfo wherever we read files shipped with the game - 'cause they're written using a global standard
        /// don't use it when reading/writing local files - the local standard will suffice
        /// If you have further questions on the subject, I suggest burning down your nearest M$ office.
        /// </summary>
        public static CultureInfo cultureInfo = new CultureInfo("en-US");

        // gameplay variables - aici tinem vectori cu datele care trebuie tinute minte doar o data, intr-un singur loc
        internal static Province[] provinces;
        internal static Nation[] nations;
        internal static DefenseBuilding[] defBuildings;
        internal static UnitStats[] unitStats;
        internal static Tactic[] meleeTactics, skirmishTactics;
        internal static List<Army> selectedArmies = new List<Army>();//far better than searching through ALL the armies for the selected ones
        internal static byte[,] mapMatrix;// aici stocam info despre carei provincii ii apartine pixelul i, j
        //ne intereseaza atat la desenarea provinciilor, cat si pentru a determina pe ce provincie am dat click
        internal static byte player = 1;//index in vectorul de natiuni (momentan doar WRE e jucabil)
        #endregion
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        public Game()
        {// we need to initialize the graphics device here, so that we may use it in Initialize()
            //XNA does stuff inbetween these two methods
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //Read config file for saved options
            if(!readConfigIni())//if file doesn't exist/ read failed
            {// we use default resolution and music/sound volumes
                screenW = this.graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                screenH = this.graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                musicPlayer = new Utilities.MusicPlayer(1.0f);
                soundVolume = 1.0f;
                //then we write a valid config file
                StreamWriter config = new StreamWriter("config.ini");
                config.WriteLine("Width=" + screenW);
                config.WriteLine("Height=" + screenH);
                config.WriteLine("MusicVolume=1.0");
                config.WriteLine("SoundVolume=1.0");
                config.Close();
            }
            this.graphics.IsFullScreen = true;
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
        /// This is where it can query for any required services and load any non-graphic related content.
        /// Calling base.Initialize will enumerate through any components and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            minimapTexture = new RenderTarget2D(GraphicsDevice, screenW, screenH, false,
                            GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            newGameRect = new Rectangle((int)(screenW * 0.45), (int)(screenH * 0.4), (int)(screenW * 0.1), (int)(screenH * 0.05));
            optionsRect = new Rectangle((int)(screenW * 0.45), (int)(screenH * 0.5), (int)(screenW * 0.1), (int)(screenH * 0.05));
            quitRect = new Rectangle((int)(screenW * 0.45), (int)(screenH * 0.6), (int)(screenW * 0.1), (int)(screenH * 0.05));
            scrollMarginRight = screenW - 40;
            scrollMarginDown = screenH - 40;
            mouseStatePrevious = Mouse.GetState();
            gameState = MAIN_MENU;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //Utilities.ImageProcessor.makeCircle(24, 32, 32);
            
            // Read text files to form data cache
            Nation.readNations(GraphicsDevice);
            Province.readProvinces(GraphicsDevice);
            DefenseBuilding.readDefBuildings();
            UnitStats.readUnitStats();
            Tactic.readMeleeTactics();
            Tactic.readSkirmishTactics();
            readScenario();
            // Load other textures
            mainMenuTexture = Texture2D.FromStream(GraphicsDevice, new FileStream("graphics/main menu.png", FileMode.Open));
            uiBackground = Texture2D.FromStream(GraphicsDevice, new FileStream("graphics/fish_mosaic.jpg", FileMode.Open));
            uiStatusBarRect = new Rectangle(100, 100, 300, 50);
            coin = Texture2D.FromStream(GraphicsDevice, new FileStream("graphics/coin.png", FileMode.Open));
            selectHalo = Texture2D.FromStream(GraphicsDevice, new FileStream("graphics/army icons/circle.png", FileMode.Open));
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
            camera = new _2DCamera(screenW, screenH, dim[0], dim[1], 1f, 0.5f, 1f);

            // Load sound effects
            SoundEffect clickEffect = Content.Load<SoundEffect>("click");
            clickSFX = clickEffect.CreateInstance();//only an instance allows us to stop a sound effect mid-play
            clickSFX.Volume = soundVolume;

            // Initialize playlist
            List<Song> music = new List<Song>();
            music.Add(Content.Load<Song>("Caesar 3 Soundtrack - Victory"));
            musicPlayer.newPlaylist(music);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        // from here - content loading functions called just once in the loadContent phase
        private void readScenario(){
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
                    provinces[i].prosperity = Convert.ToSingle(word[0], cultureInfo);
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

            //now we need to set the counterpart for each neighbor (we will need this info a lot)
            for (int i = 0; i < provinces.Length; i++)
                foreach (Neighbor n in provinces[i].neighbors)
                    n.findOtherSide(provinces[i]);
        }

        /// <summary>
        /// Reads config.ini for information regarding screen resolution and music and sound volume.
        /// It recognizes only a rigid structure and if any information is not there/ is corrupted the method will return false.
        /// </summary>
        /// <returns>If the read succeeded or not</returns>
        private bool readConfigIni()
        {
            if (!File.Exists("config.ini"))
            {
                return false;//fail
            }
            StreamReader config = new StreamReader("config.ini");
            String s = config.ReadLine();
            if (s == null)//linia nu exista => fail
            {
                config.Close();
                return false;
            }
            String[] word = s.Split('=');
            if (word[0].CompareTo("Width") == 0)
                screenW = this.graphics.PreferredBackBufferWidth = Convert.ToInt32(word[1]);
            else
            {
                config.Close();
                return false;
            }
            s = config.ReadLine();
            if (s == null)
            {
                config.Close();
                return false;
            }
            word = s.Split('=');
            if (word[0].CompareTo("Height") == 0)
                screenH = this.graphics.PreferredBackBufferHeight = Convert.ToInt32(word[1]);
            else
            {
                config.Close();
                return false;
            }
            s = config.ReadLine();
            if (s == null)
            {
                config.Close();
                return false;
            }
            word = s.Split('=');
            if (word[0].CompareTo("MusicVolume") == 0)
                musicPlayer = new Utilities.MusicPlayer(Convert.ToSingle(word[1]));
            else
            {
                config.Close();
                return false;
            }
            s = config.ReadLine();
            if (s == null)
            {
                config.Close();
                return false;
            }
            word = s.Split('=');
            if (word[0].CompareTo("SoundVolume") == 0)
                soundVolume = Convert.ToSingle(word[1]);
            else
            {
                config.Close();
                return false;
            }
            config.Close();
            // verificam ca latimea si inaltimea citite sa fie suportate
            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (mode.Width == screenW && mode.Height == screenH)
                    return true;
            }
            return false;
        }
    }
}
