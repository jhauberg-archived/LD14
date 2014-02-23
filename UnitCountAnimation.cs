using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LD14
{
    public sealed class UnitCountAnimation : IUpdateable, IDrawable
    {
        public SpriteBatch Sprite { get; set; }
        public SpriteFont Font { get; set; }

        int units;

        Color color;
        Vector2 screenSpaceCoordinate;

        float speed = 17.5f;
        float fadeRate = 0.35f;

        float alpha = 1;

        public bool Ended { get; set; }

        public UnitCountAnimation(int units, Vector2 screenSpaceCoordinate)
        {
            this.units = units;
            this.screenSpaceCoordinate = screenSpaceCoordinate;

            color = units > 0 ? Color.LightGreen : Color.Crimson;

            Debug.WriteLine(String.Format("{0} begun", this));
        }

        #region IDrawable Members

        public void Draw(GameTime gameTime)
        {
            Sprite.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

            string text = String.Format("{0} {1}", units > 0 ? "+" : "-", Math.Abs(units));

            Sprite.DrawString(Font, text, screenSpaceCoordinate, color);

            Sprite.End();
        }

        public int DrawOrder
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler DrawOrderChanged;

        public bool Visible
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler VisibleChanged;

        #endregion

        #region IUpdateable Members

        public bool Enabled
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler EnabledChanged;

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float movement = speed * elapsed;

            screenSpaceCoordinate.Y += units > 0 ? -movement : movement;

            if (alpha > 0) {
                alpha -= fadeRate * elapsed;
            } else {
                Ended = true;

                Debug.WriteLine(String.Format("{0} ended", this));
            }

            color.A = (byte)(255.0f * alpha);
        }

        public int UpdateOrder
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler UpdateOrderChanged;

        #endregion
    }
}
