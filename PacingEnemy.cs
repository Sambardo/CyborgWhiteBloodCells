using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace ImagineCup
{



    class PacingEnemy : Enemy
    {
        private FaceDirection direction = FaceDirection.Left;
        protected Animation runAnimation;
        public float WaitTime
        {
        get {return waitTime;}
        set {waitTime = value;}
        }float waitTime;

        public PacingEnemy(Level level, Vector2 position, string spriteSet)
            : base(level, position, spriteSet)
        { }


        /// <summary>
        /// overides the loadcontent because we need to load one more spriteset than in the base class; runAnimation
        /// </summary>
        /// <param name="spriteSet"></param>
        public override void LoadContent(string spriteSet)
        {
            base.LoadContent(spriteSet);
            spriteSet = "Sprites/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.15f, true);
        }

        public override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate tile base.Position based on the side we are walking towards.
            float posX = base.Position.X + base.LocalBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(base.Position.Y / Tile.Height);

            if (waitTime > 0)
            {
                // Wait for some amount of time.
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (waitTime <= 0.0f)
                {
                    // Then turn around.
                    direction = (FaceDirection)(-(int)direction);
                    //WAIT, they actually fucking DID THAT? ARE YOU KIDDING?
                }
            }
            else
            {
                // If we are about to run into a wall or off a cliff, start waiting.
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                    Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                {
                    waitTime = MaxWaitTime;
                }
                else
                {
                    // Move in the current direction.
                    Vector2 velocity = new Vector2((int)direction * /*MoveSpeed was here instead of 1*/ 100 * elapsed, 0.0f);
                    if (IsAlive)base.Position = base.Position + velocity;
                }
            }
        }

        public override void ApplyPhysics(GameTime gameTime)
        {
            /*//double time = ((double)gameTime.ElapsedGameTime.Milliseconds / 1000.0);
            double time = .1;
            Vy += g * time;
            double y = Vy * time;//what y have we fallen
            //y = .5;
            base.Position = new Vector2((float)base.Position.X, (float)(base.Position.Y + y));//new base.Position
            base.HandleCollisions();*/
        }


        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!this.IsAlive)
            {
                //if they already died make them disapear
                if (base.sprite.FrameIndex == (dieAnimation.FrameCount - 1))
                {

                    base.Level.QueueEnemyChange(this);
                    return;//TODO: add in blinky
                }
                base.sprite.PlayAnimation(dieAnimation);
            }
            else
            {
                
                if (!Level.Player.IsAlive ||
                Level.ReachedExit ||
                    //Level.TimeRemaining == TimeSpan.Zero ||
               waitTime > 0)
                {
                    base.sprite.PlayAnimation(base.IdleAnimation);
                }
                else
                {
                    base.sprite.PlayAnimation(runAnimation);
                }
            }

            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            base.sprite.Draw(gameTime, spriteBatch, Position, flip);
        }


        public override void SetBounds()
        {



            int width = (int)(base.IdleAnimation.FrameWidth * .90);//TODO: why idleAnimation? we might be loading a different animation
            int left = base.IdleAnimation.FrameWidth - width - 5;//minus 5 percent, 2.5 on each side
            int height = (int)(base.IdleAnimation.FrameWidth * .60);//half the height
            int top = (int)(base.IdleAnimation.FrameWidth * .40);//moved down half a height

            LocalBounds = new Rectangle(left, top, width, height);
        }

    }
}
