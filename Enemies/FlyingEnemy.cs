using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace ImagineCup
{
    class FlyingEnemy : Enemy
    {

        private double degrees = 90;
        private bool droppedrock = false;

        //used to fall TODO?
        private const double g = 9.8;
        private double Vy = 3;

        public FlyingEnemy(Level level, Vector2 position)
            : base(level, position, "FlyingEnemy")
        {
            Health = 5;
            Score = 50;
            base.CollisionDamage = 10;
        }


        override public void Update(GameTime gameTime)
        {
            if (IsAlive)
            {
                degrees += .05;
                degrees %= 360;
                //moves left for a while
                base.Position = new Vector2((float)(base.Position.X - 2), (float)base.Position.Y);//TODO: gameTime.ElapsedGameTime.Milliseconds/1000.0?
                //we now oscillate our y based on the total time passed in game
                Oscillate(((double)gameTime.TotalGameTime.Milliseconds / 10.0));
                AbovePlayer();
            }
            else
            {
                ApplyPhysics(gameTime);
            }
        }

        private void Oscillate(double radians)
        {
            base.Position = new Vector2(base.Position.X, (float)(base.Position.Y + Math.Sin(degrees) * 3));
            //well fuck that was easy
        }

        private void AbovePlayer()
        {
            if (this.Position.X >= (Level.Player.Position.X - 5) && this.Position.X <= (Level.Player.Position.X + 5) && this.Position.Y < Level.Player.Position.Y && droppedrock == false && IsAlive == true)
            {
                //TODO: MAKE THIS SHIT WORK
                Level.QueueEnemyChange(new SpikeEnemy(Level, this.Position));
                droppedrock = true;
            }
        }

        public override void ApplyPhysics(GameTime gameTime) //TODO, move to enemy as a falling function Obey Gravisty
        {
            base.HandleCollisions();
            //double time = ((double)gameTime.ElapsedGameTime.Milliseconds / 1000.0);
            double time = .1;

            if (!base.IsOnGround)
            {
                Vy += g * time;

                double y = Vy * time;//what y have we fallen
                //y = .5;
                base.Position = new Vector2((float)base.Position.X, (float)(base.Position.Y + y));//new position
            }
            else Vy = 0;


        }

        ///<summary>
        ///Draw the flying enemy. When they die, make them fall to the ground before they disappear.
        ///</summary>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsAlive)
            {
                //if they hit the ground and died already make them dissappear
                if ((base.sprite.FrameIndex == (dieAnimation.FrameCount - 1) ) && (IsOnGround || !base.Level.isOnScreen(base.Position)) )
                {

                    base.Level.QueueEnemyChange(this);
                    return;//TODO: add in blinky
                }
                base.sprite.PlayAnimation(dieAnimation);
            }
            else
            {
                //maybe make a DWID function here so things dont have to override draw and redo the code every time
                base.sprite.PlayAnimation(base.IdleAnimation);
            }

            // Draw facing the way the enemy is moving.
            SpriteEffects flip = Direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            base.sprite.Draw(gameTime, spriteBatch, Position, flip);
        }
        public override void SetBounds()
        {



            int width = (int)(base.IdleAnimation.FrameWidth);//TODO: why idleAnimation? we might be loading a different animation
            int left = 0;
            int height = (int)(base.IdleAnimation.FrameWidth* .6);
            int top = (int)(base.IdleAnimation.FrameWidth * .4);

            LocalBounds = new Rectangle(left, top, width, height);
        }
    }
}
