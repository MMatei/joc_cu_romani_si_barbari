using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace joc_cu_romani_si_barbari.Utilities
{
    class TextArea
    {
        String[] lines;//what to write in the TextArea, line by line
        private SpriteFont font;
        private Vector2 start;
        private SpriteBatch spriteBatch;
        public TextArea(SpriteFont font, Vector2 start, SpriteBatch sb)
        {
            this.font = font;
            this.start = start;
            spriteBatch = sb;
        }
        public void draw(String text)
        {
            lines = text.Split('\n');
            Vector2 deplasament = Vector2.Zero;
            foreach (String s in lines)
            {
                spriteBatch.DrawString(font, s, start+deplasament, Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                deplasament.Y += font.LineSpacing;
            }
        }
    }
}
