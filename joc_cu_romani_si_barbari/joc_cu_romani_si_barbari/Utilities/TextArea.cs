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
        private String[][] lines;
        private SpriteBatch spriteBatch;
        private Texture2D handleBar, upArrow, downArrow;
        private Rectangle handleBarRect, upArrowRect, downArrowRect;
        private int handleBarMaxHeight, arrowsHeight;
        private GraphicsDevice gdi;
        private MouseState mouseStatePrev;
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
            this.lines = new String[1][];
            spriteBatch = sb;
            handleBar = handleBarTex;
            upArrow = upArrowTex;
            downArrow = downArrowTex;
            arrowsHeight = upArrow.Bounds.Height;
            upArrowRect = new Rectangle(width - upArrow.Bounds.Width, 0, upArrow.Bounds.Width, arrowsHeight);
            downArrowRect = new Rectangle(width - downArrow.Bounds.Width, height - arrowsHeight, downArrow.Bounds.Width, arrowsHeight);
            //the handlebar initially occupies the entire height of the textarea, minus the two arrows, of course
            handleBarMaxHeight = height - 2 * arrowsHeight;
            handleBarRect = new Rectangle(width - upArrow.Bounds.Width, upArrow.Bounds.Width, upArrow.Bounds.Width, handleBarMaxHeight);
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
            this.lines = new String[n][];
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
            arrowsHeight = upArrow.Bounds.Height;
            upArrowRect = new Rectangle(width - upArrow.Bounds.Width, 0, upArrow.Bounds.Width, arrowsHeight);
            downArrowRect = new Rectangle(width - downArrow.Bounds.Width, height - arrowsHeight, downArrow.Bounds.Width, arrowsHeight);
            handleBarMaxHeight = height - 2 * arrowsHeight;
            handleBarRect = new Rectangle(width - upArrow.Bounds.Width, upArrow.Bounds.Width, upArrow.Bounds.Width, handleBarMaxHeight);
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
            this.lines = new String[columnStart.Length][];
            columnStart.CopyTo(this.start, 0);
            spriteBatch = sb;
            handleBar = handleBarTex;
            upArrow = upArrowTex;
            downArrow = downArrowTex;
            arrowsHeight = upArrow.Bounds.Height;
            upArrowRect = new Rectangle(width - upArrow.Bounds.Width, 0, upArrow.Bounds.Width, arrowsHeight);
            downArrowRect = new Rectangle(width - downArrow.Bounds.Width, height - arrowsHeight, downArrow.Bounds.Width, arrowsHeight);
            handleBarMaxHeight = height - 2 * arrowsHeight;
            handleBarRect = new Rectangle(width - upArrow.Bounds.Width, upArrow.Bounds.Width, upArrow.Bounds.Width, handleBarMaxHeight);
            textArea = new RenderTarget2D(gdi, width, height, false, gdi.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            textAreaRect = new Rectangle(startX, startY, width, height);
        }

        /// <summary>
        /// Sets the text to be displayed in this textArea; single column variant.
        /// </summary>
        public void setText(String text)
        {
            lines[0] = text.Split('\n');
            handleBarRect.Height = Math.Min(handleBarMaxHeight, (int)(handleBarMaxHeight * ((float)(handleBarMaxHeight + 2*arrowsHeight) / (float)((lines[0].Length - 1) * font.LineSpacing))));
            //reset bar to top of textarea
            handleBarRect.Y = arrowsHeight;
            this.start[0].Y = 0;
        }
        /// <summary>
        /// Sets the text to be displayed in this textArea; multiple columns variant.
        /// </summary>
        public void setText(String[] text)
        {
            int mostColumns = 0;
            for (int i = 0; i < text.Length; i++)
            {
                lines[i] = text[i].Split('\n');
                if (lines[i].Length > mostColumns)
                    mostColumns = lines[i].Length;
            }
            handleBarRect.Height = Math.Min(handleBarMaxHeight, (int)(handleBarMaxHeight * ((float)(handleBarMaxHeight + 2*arrowsHeight) / (float)((mostColumns - 1) * font.LineSpacing))));
            handleBarRect.Y = arrowsHeight;
            for (int i = 0; i < start.Length; i++)
                start[i].Y = 0;
        }

        /// <summary>
        /// Updates the textArea texture in order to be drawn to screen. ALWAYS call this before drawing stuff to the _screen_
        /// </summary>
        public void draw()
        {
            gdi.SetRenderTarget(textArea);
            gdi.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, null, null, null);
            for (int i = 0; i < lines.Length; i++)
            {
                Vector2 deplasament = Vector2.Zero;
                foreach (String s in lines[i])
                {
                    spriteBatch.DrawString(font, s, start[i] + deplasament, Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                    deplasament.Y += font.LineSpacing;
                    if (deplasament.Y > textArea.Height)
                        break;
                }
            }
            spriteBatch.Draw(upArrow, upArrowRect, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(handleBar, handleBarRect, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(downArrow, downArrowRect, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.End();
            gdi.SetRenderTarget(null);
        }

        public void update(MouseState mouseStateCrrt)
        {
            if (mouseStateCrrt.LeftButton == ButtonState.Pressed)
            {
                if (handleBarRect.Contains(mouseStateCrrt.X - textAreaRect.X, mouseStateCrrt.Y-textAreaRect.Y))
                {//the handleBar is selected; we'll drag it up or down
                    //ATENTIE la acesta diferenta! vreau ca atat textul cat si bara sa se mute cu acelasi nr de pixeli
                    //in acelasi timp, vreau ca bara sa ramana in [arrowsHeight, arrowsHeight+handleBarMaxHeight]
                    //de aceea calculez diff ca EXACT diferenta intre pct curent si limita maxima
                    int diff = mouseStateCrrt.Y - mouseStatePrev.Y;
                    if (handleBarRect.Y + diff < arrowsHeight)
                        diff = 32 - handleBarRect.Y;
                    if (handleBarRect.Bottom + diff > arrowsHeight + handleBarMaxHeight)
                        diff = 32 + handleBarMaxHeight - handleBarRect.Bottom;
                    //handleBar si textul merg in directii opuse => semn opus la operatii
                    handleBarRect.Y += diff;
                    for (int i = 0; i < start.Length; i++)
                        start[i].Y -= diff;
                }
                else if (upArrowRect.Contains(mouseStateCrrt.X - textAreaRect.X, mouseStateCrrt.Y - textAreaRect.Y))
                {//we go up by fixed amount
                    int diff = -2;
                    if (handleBarRect.Y + diff < arrowsHeight)
                        diff = 32 - handleBarRect.Y;
                    if (handleBarRect.Bottom + diff > arrowsHeight + handleBarMaxHeight)
                        diff = 32 + handleBarMaxHeight - handleBarRect.Bottom;
                    handleBarRect.Y += diff;
                    for (int i = 0; i < start.Length; i++)
                        start[i].Y -= diff;
                }
                else if (downArrowRect.Contains(mouseStateCrrt.X - textAreaRect.X, mouseStateCrrt.Y - textAreaRect.Y))
                {//we go down by fixed amount
                    int diff = 2;
                    if (handleBarRect.Y + diff < arrowsHeight)
                        diff = 32 - handleBarRect.Y;
                    if (handleBarRect.Bottom + diff > arrowsHeight + handleBarMaxHeight)
                        diff = 32 + handleBarMaxHeight - handleBarRect.Bottom;
                    handleBarRect.Y += diff;
                    for (int i = 0; i < start.Length; i++)
                        start[i].Y -= diff;
                }
            }
            mouseStatePrev = mouseStateCrrt;
        }
    }
}
