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
            foreach (Army army in selectedArmies)//we draw an appropriate selection halo for each selected army
                spriteBatch.Draw(selectHalo, army.iconLocation, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.8f);
            spriteBatch.End();

            //desenam elementele statice in raport cu camera (adica nu sunt afectate de ViewMatrix-ul camerei)
            spriteBatch.Begin();
            spriteBatch.Draw(uiBackground, new Rectangle(screenW - 300, 0, 300, 50), uiStatusBarRect, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.95f);
            spriteBatch.Draw(coin, new Rectangle(screenW - 290, 10, 30, 30), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(font, nations[player].money + "", new Vector2(screenW - 250, 10), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(font, date.ToString(), new Vector2(screenW - 100, 10), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(minimapTexture, new Rectangle(screenW - 300, 50, 300, 150), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(uiProvinceDetailTexture, uiProvinceDetailRect, null, Color.White, 0.0f,Vector2.Zero, SpriteEffects.None, 1.0f);
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
                        makeMinimap();
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
                        spriteBatch.DrawString(font, "v 0.02b", new Vector2((int)(screenW * 0.9), (int)(screenH * 0.9)), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
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
