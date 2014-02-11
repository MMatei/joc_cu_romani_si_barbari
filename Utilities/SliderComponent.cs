using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace joc_cu_romani_si_barbari.Utilities
{
    class SliderComponent : HelperComponent
    {
        private static Texture2D sliderRail, sliderCart;
        static EventArgs defaultArgs = new EventArgs();
        static SliderComponent SliderCapturingMouse;

        public event EventHandler ValueChanged;
        private SpriteBatch spriteBatch;
        private SpriteFont font;

        public Vector2 Position = new Vector2(0, 0);
        public readonly Vector2 Size = new Vector2(158, 36);
        public string Name = string.Empty;
        public string Text = string.Empty;
        public float Minimum = 0;
        public float Maximum = 10;
        public float Value = 5;
        public float Step = 1;
        public bool AppendValue = true;

        private MouseState lastMs = new MouseState();


        public SliderComponent(GraphicsDevice gdi, SpriteBatch spriteBatch, SpriteFont font)
        {
            this.spriteBatch = spriteBatch;
            this.font = font;
            if (sliderRail == null)
            {
                sliderRail = Texture2D.FromStream(gdi, new FileStream("graphics/sliderRail.png", FileMode.Open));
                sliderCart = Texture2D.FromStream(gdi, new FileStream("graphics/sliderCart.png", FileMode.Open));
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void update(KeyboardState key, MouseState ms)
        {
            if (ms.LeftButton == ButtonState.Pressed)
            {
                float cX = ms.X - Position.X;
                float cY = ms.Y - Position.Y;

                if (cX >= 0 && cX <= Size.X && cY > 0 && cY <= Size.Y)
                {
                    if (lastMs.LeftButton == ButtonState.Released)
                    {
                        SliderCapturingMouse = this;
                    }

                    if (SliderCapturingMouse == this)
                    {
                        float oldValue = Value;
                        float drawValue = MathHelper.Clamp((cX - 16) / 138f, 0, 1);
                        Value = drawValue * (Maximum - Minimum) + Minimum;
                        Value = Step * (float)Math.Round(Value / Step);

                        if (Value != oldValue)
                        {
                            if (ValueChanged != null)
                            {
                                ValueChanged(this, defaultArgs);
                            }
                        }
                    }
                }
            }
            else
            {
                if (SliderCapturingMouse == this)
                {
                    SliderCapturingMouse = null;
                }
            }

            lastMs = ms;
        }

        public void draw()
        {
            string toDraw = AppendValue ? string.Format("{0} {1}", Text, Value) : Text;
            spriteBatch.DrawString(font, toDraw, Position + new Vector2(10, 0), Color.Black);
            spriteBatch.Draw(sliderRail, Position + new Vector2(0, 10), Color.White);

            float drawValue = MathHelper.Clamp((Value - Minimum) / (Maximum - Minimum), 0, 1);
            drawValue *= 138f;
            spriteBatch.Draw(sliderCart, Position + new Vector2(9 + drawValue - 7, 23), Color.White);
        }
    }
}
