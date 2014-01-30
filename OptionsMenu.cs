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
    class OptionsMenu
    {
        // Options Menu variables
        internal Rectangle resolutionRect, applyRect, backRect, leftArrowRect, rightArrowRect;
        private Texture2D leftArrow, rightArrow;
        internal List<DisplayMode> supportedResolutions;
        internal int currentResolution;

        public OptionsMenu(int screenW, int screenH, GraphicsDevice gdi)
        {
            resolutionRect = new Rectangle((int)(screenW * 0.35), (int)(screenH * 0.4), (int)(screenW * 0.3), (int)(screenH * 0.05));
            applyRect = new Rectangle((int)(screenW * 0.35), (int)(screenH * 0.6), (int)(screenW * 0.1), (int)(screenH * 0.05));
            backRect = new Rectangle((int)(screenW * 0.55), (int)(screenH * 0.6), (int)(screenW * 0.1), (int)(screenH * 0.05));
            //so, in order to make it look nice at all resolutions, we fix the distances between the word 'Resolution' and the two arrows
            leftArrowRect = new Rectangle((int)(screenW * 0.35 + 100), (int)(screenH * 0.395), 32, 32);
            rightArrowRect = new Rectangle((int)(screenW * 0.35 + 250), (int)(screenH * 0.395), 32, 32);
            leftArrow = Texture2D.FromStream(gdi, new FileStream("graphics/left_arrow.png", FileMode.Open));
            rightArrow = Texture2D.FromStream(gdi, new FileStream("graphics/right_arrow.png", FileMode.Open));

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
        }

        public void reset(int screenW, int screenH)
        {
            resolutionRect = new Rectangle((int)(screenW * 0.35), (int)(screenH * 0.4), (int)(screenW * 0.3), (int)(screenH * 0.05));
            applyRect = new Rectangle((int)(screenW * 0.35), (int)(screenH * 0.6), (int)(screenW * 0.1), (int)(screenH * 0.05));
            backRect = new Rectangle((int)(screenW * 0.55), (int)(screenH * 0.6), (int)(screenW * 0.1), (int)(screenH * 0.05));
            leftArrowRect = new Rectangle((int)(screenW * 0.35 + 100), (int)(screenH * 0.395), 32, 32);
            rightArrowRect = new Rectangle((int)(screenW * 0.35 + 250), (int)(screenH * 0.395), 32, 32);
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

        public void draw(SpriteBatch spriteBatch, Game game, int screenW, int screenH)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(game.mainMenuTexture, new Rectangle(0, 0, screenW, screenH), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(leftArrow, leftArrowRect, null, Color.Yellow, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            spriteBatch.Draw(rightArrow, rightArrowRect, null, Color.Yellow, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            spriteBatch.DrawString(game.font, "Resolution:", new Vector2(resolutionRect.X, resolutionRect.Y), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(game.font, supportedResolutions[currentResolution].Width + " x " + supportedResolutions[currentResolution].Height, new Vector2(resolutionRect.X + 150, resolutionRect.Y), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(game.font, "Apply", new Vector2(applyRect.X, applyRect.Y), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(game.font, "Back", new Vector2(backRect.X, backRect.Y), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(game.font, "v 0.01b", new Vector2((int)(screenW * 0.9), (int)(screenH * 0.9)), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.End();
        }
    }
}
