using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace joc_cu_romani_si_barbari
{
    /// <summary>
    /// We keep all the variables pertaining to the options menu here, both to improve the readability of Game.cs
    /// and to be able to delete some useless variables
    /// </summary>
    class OptionsMenu : Utilities.HelperComponent
    {
        // Options Menu variables
        internal Rectangle resolutionRect, applyRect, backRect, leftArrowRect, rightArrowRect;
        private Texture2D leftArrow, rightArrow;
        internal List<DisplayMode> supportedResolutions;
        internal int currentResolution;
        internal bool resolutionChanged = false;//if we didn't change the resolution settings, we should avoid resetting the graphics
        private SpriteBatch spriteBatch;
        private int screenW, screenH;
        private Game game;
        internal Utilities.SliderComponent musicVolume, soundVolume;

        public OptionsMenu(int screenW, int screenH, GraphicsDevice gdi, SpriteBatch sb, Game _game, float mVol)
        {
            spriteBatch = sb;
            this.screenW = screenW;
            this.screenH = screenH;
            game = _game;
            resolutionRect = new Rectangle((int)(screenW * 0.35), (int)(screenH * 0.35), (int)(screenW * 0.3), (int)(screenH * 0.05));
            applyRect = new Rectangle((int)(screenW * 0.35), (int)(screenH * 0.7), (int)(screenW * 0.1), (int)(screenH * 0.05));
            backRect = new Rectangle((int)(screenW * 0.55), (int)(screenH * 0.7), (int)(screenW * 0.1), (int)(screenH * 0.05));
            //so, in order to make it look nice at all resolutions, we fix the distances between the word 'Resolution' and the two arrows
            leftArrowRect = new Rectangle((int)(screenW * 0.35 + 100), (int)(screenH * 0.345), 32, 32);
            rightArrowRect = new Rectangle((int)(screenW * 0.35 + 250), (int)(screenH * 0.345), 32, 32);
            FileStream fs = new FileStream("graphics/left_arrow.png", FileMode.Open);
            leftArrow = Texture2D.FromStream(gdi, fs);
            fs.Close();//we do this little trick because the FromStream method is dumb and doesn't close the file stream it uses
            //since OptionsMenu can be recreated at a later date, moment at which the GC may not have cleaned up fs
            //it falls upon us to prevent a crash of type "resource already in use"
            fs = new FileStream("graphics/right_arrow.png", FileMode.Open);
            rightArrow = Texture2D.FromStream(gdi, fs);
            fs.Close();

            supportedResolutions = new List<DisplayMode>();
            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (mode.Format.CompareTo(SurfaceFormat.Color) == 0)
                {
                    supportedResolutions.Add(mode);
                    if (mode.Width == screenW && mode.Height == screenH)
                        currentResolution = supportedResolutions.Count - 1;
                }
            }
            musicVolume = new Utilities.SliderComponent(gdi, spriteBatch, game.font);
            musicVolume.Position = new Vector2((int)(screenW * 0.4), (int)(screenH * 0.45));
            musicVolume.Minimum = 0.0f;
            musicVolume.Maximum = 100.0f;
            musicVolume.Step = 1.0f;
            musicVolume.Value = mVol*100;
            musicVolume.Text = "Music volume:";
            soundVolume = new Utilities.SliderComponent(gdi, spriteBatch, game.font);
            soundVolume.Position = new Vector2((int)(screenW * 0.4), (int)(screenH * 0.55));
            soundVolume.Minimum = 0.0f;
            soundVolume.Maximum = 100.0f;
            soundVolume.Step = 1.0f;
            soundVolume.Value = game.soundVolume*100;
            soundVolume.Text = "Sound volume:";
        }

        public void reset(int screenW, int screenH)
        {
            resolutionRect = new Rectangle((int)(screenW * 0.35), (int)(screenH * 0.35), (int)(screenW * 0.3), (int)(screenH * 0.05));
            applyRect = new Rectangle((int)(screenW * 0.35), (int)(screenH * 0.7), (int)(screenW * 0.1), (int)(screenH * 0.05));
            backRect = new Rectangle((int)(screenW * 0.55), (int)(screenH * 0.7), (int)(screenW * 0.1), (int)(screenH * 0.05));
            leftArrowRect = new Rectangle((int)(screenW * 0.35 + 100), (int)(screenH * 0.345), 32, 32);
            rightArrowRect = new Rectangle((int)(screenW * 0.35 + 250), (int)(screenH * 0.345), 32, 32);
            musicVolume.Position = new Vector2((int)(screenW * 0.4), (int)(screenH * 0.45));
            soundVolume.Position = new Vector2((int)(screenW * 0.4), (int)(screenH * 0.55));
        }

        public void changeResolution(bool left)
        {
            if (left)
            {
                currentResolution--;
                if (currentResolution < 0) currentResolution = supportedResolutions.Count - 1;
            }
            else
            {
                currentResolution++;
                if (currentResolution == supportedResolutions.Count) currentResolution = 0;
            }
        }

        public void update(KeyboardState key, MouseState mouseStateCurrent)
        {
            musicVolume.update(key, mouseStateCurrent);
            soundVolume.update(key, mouseStateCurrent);

            if (mouseStateCurrent.LeftButton == ButtonState.Pressed && game.mouseStatePrevious.LeftButton == ButtonState.Released)
            {//we've clicked on something
                int x = mouseStateCurrent.X, y = mouseStateCurrent.Y;
                if (leftArrowRect.Contains(x, y))
                {
                    game.clickSFX.Play();
                    changeResolution(true);
                }
                if (rightArrowRect.Contains(x, y))
                {
                    game.clickSFX.Play();
                    changeResolution(false);
                }
                if (applyRect.Contains(x, y))
                {
                    game.clickSFX.Play();
                    game.applySettings();
                }
                if (backRect.Contains(x, y))
                {
                    game.clickSFX.Play();
                    game.gameState = Game.MAIN_MENU;
                }
            }
        }

        public void draw()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(game.mainMenuTexture, new Rectangle(0, 0, screenW, screenH), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(leftArrow, leftArrowRect, null, Color.Yellow, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            spriteBatch.Draw(rightArrow, rightArrowRect, null, Color.Yellow, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            spriteBatch.DrawString(game.font, "Resolution:", new Vector2(resolutionRect.X, resolutionRect.Y), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(game.font, supportedResolutions[currentResolution].Width + " x " + supportedResolutions[currentResolution].Height, new Vector2(resolutionRect.X + 150, resolutionRect.Y), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(game.font, "Apply", new Vector2(applyRect.X, applyRect.Y), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(game.font, "Back", new Vector2(backRect.X, backRect.Y), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(game.font, "v 0.02", new Vector2((int)(screenW * 0.9), (int)(screenH * 0.9)), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            musicVolume.draw();
            soundVolume.draw();
            spriteBatch.End();
        }

        public void dispose()
        {
            leftArrow.Dispose();
            rightArrow.Dispose();
        }
    }
}
