using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace ImagineCup
{
  
    /// <summary>
    /// A valuable item the player can collect.
    /// </summary>
    class GunPowerup
    {
        private Texture2D texture;
        private SoundEffect collectedSound;
        private Vector2 origin;

        private Gun power;//keep this private for now

        // declares the types of power ups as enums.
        //Easy to add new oens to this and switch statements

        public readonly int PointValue = 30;
        public Color Color;

        // The gem is animated from a base position along the Y axis.
        private Vector2 basePosition;
        private float bounce;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Gets the current position of this gem in world space.
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return basePosition + new Vector2(0.0f, bounce);
            }
        }

        /// <summary>
        /// Gets a circle which bounds this gem in world space.
        /// </summary>
        public Circle BoundingCircle
        {
            get
            {
                return new Circle(Position, Tile.Width / 3.0f);
            }
        }

        /// <summary>
        /// Constructs a new gem.
        /// </summary>
        public GunPowerup(Level level, Vector2 position, Gun power)
        {
            
            this.level = level;
            this.basePosition = position;
            this.power = power;
            
            LoadContent();
            
        }

        /// <summary>
        /// Loads the gem texture and collected sound.
        /// </summary>
        

        /// <summary>
        /// Bounces up and down in the air to entice players to collect them.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Bounce control constants
            const float BounceHeight = 0.18f;
            const float BounceRate = 3.0f;
            const float BounceSync = -0.75f;

            // Bounce along a sine curve over time.
            // Include the X coordinate so that neighboring gems bounce in a nice wave pattern.            
            double t = gameTime.TotalGameTime.TotalSeconds * BounceRate + Position.X * BounceSync;
            bounce = (float)Math.Sin(t) * BounceHeight * texture.Height;
        }


        /// <summary>
        /// Draws a gem in the appropriate color.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }

        /// <summary>
        /// Called when this gem has been collected by a player and removed from the level.
        /// </summary>
        /// <param name="collectedBy">
        /// The player who collected this gem. Although currently not used, this parameter would be
        /// useful for creating special powerup gems. For example, a gem could make the player invincible.
        /// </param>
        public void OnCollected(Player collectedBy)
        {
            collectedSound.Play();
            collectedBy.AddGun(power);//passing the entire gun then checking on the players side if they have it isnt perfectly efficient, but it works for now
            //TODO: FREE
        }

        public void LoadContent()
        {
                    collectedSound = Level.Content.Load<SoundEffect>(power.PowerupSound);
                    texture = Level.Content.Load<Texture2D>(power.PowerupTexture);
                    origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
        }

        
    }
}
