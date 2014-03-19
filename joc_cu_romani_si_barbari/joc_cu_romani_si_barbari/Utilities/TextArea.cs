using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace joc_cu_romani_si_barbari.Utilities
{
    class TextArea
    {
        private SpriteFont font;
        private Vector2[] start;//the starting point of each column
        private SpriteBatch spriteBatch;
        private Texture2D handleBar, upArrow, downArrow;
        private Rectangle handleBarRect, upArrowRect, downArrowRect;
        private GraphicsDevice gdi;
        public Rectangle textAreaRect;
        public RenderTarget2D textArea;

        //one column basic constructor
        public TextArea(GraphicsDevice gdi, SpriteBatch sb, SpriteFont font, int startX, int startY, int width, int height,
            Texture2D handleBarTex, Texture2D upArrowTex, Texture2D downArrowTex)
        {
            this.gdi = gdi;
            this.font = font;
            this.start = new Vector2[1];
            this.start[0] = Vector2.Zero;
            spriteBatch = sb;
            handleBar = handleBarTex;
            upArrow = upArrowTex;
            downArrow = downArrowTex;
            upArrowRect = new Rectangle(width - upArrow.Bounds.Width, 0, upArrow.Bounds.Width, upArrow.Bounds.Height);
            downArrowRect = new Rectangle(width - downArrow.Bounds.Width, height - downArrow.Bounds.Height, downArrow.Bounds.Width, downArrow.Bounds.Height);
            //the handlebar initially occupies the entire height of the textarea, minus the two arrows, of course
            handleBarRect = new Rectangle(width - upArrow.Bounds.Width, upArrow.Bounds.Width, upArrow.Bounds.Width, height - 2 * upArrow.Bounds.Height);
            textArea = new RenderTarget2D(gdi, width, height, false, gdi.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            textAreaRect = new Rectangle(startX, startY, width, height);
        }
        //n columns, size unspecified
        public TextArea(GraphicsDevice gdi, SpriteBatch sb, SpriteFont font, int startX, int startY, int n, int width, int height,
            Texture2D handleBarTex, Texture2D upArrowTex, Texture2D downArrowTex)
        {
            this.gdi = gdi;
            this.font = font;
            this.start = new Vector2[n];
            float colWidth = width;
            colWidth /= n;
            for (int i = 0; i < n; i++)
            {
                this.start[i] = new Vector2(colWidth*i, 0);
            }
            spriteBatch = sb;
            handleBar = handleBarTex;
            upArrow = upArrowTex;
            downArrow = downArrowTex;
            upArrowRect = new Rectangle(width - upArrow.Bounds.Width, 0, upArrow.Bounds.Width, upArrow.Bounds.Height);
            downArrowRect = new Rectangle(width - downArrow.Bounds.Width, height - downArrow.Bounds.Height, downArrow.Bounds.Width, downArrow.Bounds.Height);
            handleBarRect = new Rectangle(width - upArrow.Bounds.Width, upArrow.Bounds.Width, upArrow.Bounds.Width, height - 2 * upArrow.Bounds.Height);
            textArea = new RenderTarget2D(gdi, width, height, false, gdi.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            textAreaRect = new Rectangle(startX, startY, width, height);
        }
        //n columns with size specified
        public TextArea(GraphicsDevice gdi, SpriteBatch sb, SpriteFont font, int startX, int startY, Vector2[] columnStart, int width, int height,
            Texture2D handleBarTex, Texture2D upArrowTex, Texture2D downArrowTex)
        {
            this.gdi = gdi;
            this.font = font;
            this.start = new Vector2[columnStart.Length];
            columnStart.CopyTo(this.start, 0);
            spriteBatch = sb;
            handleBar = handleBarTex;
            upArrow = upArrowTex;
            downArrow = downArrowTex;
            upArrowRect = new Rectangle(width - upArrow.Bounds.Width, 0, upArrow.Bounds.Width, upArrow.Bounds.Height);
            downArrowRect = new Rectangle(width - downArrow.Bounds.Width, height - downArrow.Bounds.Height, downArrow.Bounds.Width, downArrow.Bounds.Height);
            handleBarRect = new Rectangle(width - upArrow.Bounds.Width, upArrow.Bounds.Width, upArrow.Bounds.Width, height - 2 * upArrow.Bounds.Height);
            textArea = new RenderTarget2D(gdi, width, height, false, gdi.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            textAreaRect = new Rectangle(startX, startY, width, height);
        }

        /// <summary>
        /// Updates the textArea texture in order to be drawn to screen. Simpler method for the one-column text areas.
        /// </summary>
        public void draw(String text)
        {
            String[] lines = text.Split('\n');
            Vector2 deplasament = Vector2.Zero;
            // why draw to a texture instead of directly to the screen? simple; there will often be more text that can fit into
            // this here TextArea => we need to keep text from overflowing outside the area's bounds
            // the easiest way to do this is render things to a texture; whatever's left outside won't be drawn - it's that simple!
            gdi.SetRenderTarget(textArea);
            // we set the background to transparent because the TextArea will be painted on top of other neat stuff
            gdi.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, null, null, null);
            foreach (String s in lines)
            {
                spriteBatch.DrawString(font, s, start[0] + deplasament, Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                deplasament.Y += font.LineSpacing;
                if (deplasament.Y > textArea.Height) //don't draw text that in no way shows up on screen
                    break;
            }
            spriteBatch.Draw(upArrow, upArrowRect, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(handleBar, handleBarRect, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(downArrow, downArrowRect, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.End();
            gdi.SetRenderTarget(null);
        }
        /// <summary>
        /// Updates the textArea texture in order to be drawn to screen.
        /// </summary>
        /// <param name="text">A string for each column</param>
        public void draw(String[] text)
        {
            gdi.SetRenderTarget(textArea);
            gdi.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, null, null, null);
            for (int i = 0; i < text.Length; i++)
            {
                String[] lines = text[i].Split('\n');
                Vector2 deplasament = Vector2.Zero;
                foreach (String s in lines)
                {
                    spriteBatch.DrawString(font, s, start[i] + deplasament, Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                    deplasament.Y += font.LineSpacing;
                }
            }
            spriteBatch.Draw(upArrow, upArrowRect, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(handleBar, handleBarRect, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(downArrow, downArrowRect, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.End();
            gdi.SetRenderTarget(null);
        }
    }
}
