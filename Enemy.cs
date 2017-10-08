using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace ImagineCup
{

    /// <summary>
    /// Facing direction along the X axis.
    /// </summary>
    enum FaceDirection
    {
        Left = -1,
        Right = 1,
    };

    

    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    class Enemy : VisibleEvent
    {
        #region members
        private bool blink = false;
       private int blinktime = 0;
        public int Score
        {
            get { return score; }
            set { score = value; }
        }int score;

        protected bool IsOnGround
        {

            get { return isOnGround; }
            set { isOnGround = value; }

        }bool isOnGround;


        private int PreviousBottom
        {
            get { return previousBottom; }
            set { previousBottom = value; }
        }int previousBottom;




        /// <summary>
        /// Position in world space of the bottom center of this enemy.
        /// </summary>

        //private float previousBottom; TODO, use with handle collision?


        protected Rectangle LocalBounds
        {
            get { return localBounds; }
            set { localBounds = value; }
        }
        Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this enemy in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - base.Sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - base.Sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        public int CollisionDamage
        {
            get { return collisionDamage; }
            set { collisionDamage = value; }
        }
        private int collisionDamage;

        public int Health
        {
            get { return health; }
            set
            {
                health = value;
                if (health <= 0)
                {
                    base.IsAlive = false;
                    this.OnKilled();
                }
            }
        }
        int health;

        // Animations
        protected Animation dieAnimation;


        // Sounds
        protected SoundEffect killedSound;
        protected SoundEffect hurtSound;

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        public FaceDirection Direction
        {
            get { return direction; }
        }
        private FaceDirection direction = FaceDirection.Left;

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        // protected float waitTime; TODO: use with pacing enemy.

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        protected const float MaxWaitTime = 0.5f;

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        /// 







        //currently not used but alright to keep for now
#if ZUNE
        private const float MoveSpeed = 64.0f;
        
        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -2000.0f;
        private const float GravityAcceleration = 1700.0f;
        private const float MaxFallSpeed = 450.0f;
        private const float JumpControlPower = 0.13f;
#else
        private const float MoveSpeed = 128.0f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -4000.0f;
        private const float GravityAcceleration = 3500.0f;
        private const float MaxFallSpeed = 600.0f;
        private const float JumpControlPower = 0.14f;
#endif

        #endregion
        #region Enemy Construction

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public Enemy(Level level, Vector2 position, string spriteSet)
            : base(level, position, spriteSet)
        {
            collisionDamage = 20; //base collision damage from enemies;

        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public override void LoadContent(string spriteSet)
        {

            base.LoadContent(spriteSet);

            spriteSet = "Sprites/" + spriteSet + "/";


            base.IdleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true); // TODO remove

            dieAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Die"), 0.09f, false);
            Sprite.PlayAnimation(base.IdleAnimation);           //TODO remove
            // Load sounds.
            killedSound = Level.Content.Load<SoundEffect>("Sounds/MonsterKilled");
            hurtSound = Level.Content.Load<SoundEffect>("Sounds/MonsterHit");

            // Calculate bounds within texture size.
            /*int width = (int)(base.IdleAnimation.FrameWidth * 0.35);//TODO: why idleAnimation? we might be loading a different animation
            int left = (base.IdleAnimation.FrameWidth - width) / 2;
            int height = (int)(base.IdleAnimation.FrameWidth * 0.7);
            int top = base.IdleAnimation.FrameHeight - height;*/

            SetBounds();
            // Load the bullets!
        }
        #endregion

        #region Enemy Update

        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        virtual public void Update(GameTime gameTime)
        {
            if (blinktime > 0)
            {
                blinktime--;
            }
            else if (blink == true)
            {
                blink = false;
            }

            if (!base.IsAlive) return;

            //TODO: ALL ENEMIES MUST RE-WRITE APPLYPHYSICS 
            ApplyPhysics(gameTime);

        }



        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsAlive)
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
                //maybe make a DWID function here so things dont have to override draw and redo the code every time
                base.sprite.PlayAnimation(base.IdleAnimation);
            }

            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color color = Color.White;
            if (blink)
            {
                if (blinktime % 10 > 0)
                    color = Color.DarkGray;
                else
                    color = Color.LightGray;
            }
            base.sprite.Draw(gameTime, spriteBatch, Position, flip,color);
        }

        public void OnKilled()//should be in base class
        {
            base.IsAlive = false;
            killedSound.Play();
        }


        #endregion

        #region Enemy Interactions / Physics

        virtual public void ApplyPhysics(GameTime gameTime)
        {
            //HandleCollisions();
        }

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        protected virtual void HandleCollisions()//TODO: change name; only tile collisions are handled here
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        public virtual void SetBounds()
        {



            int width = (int)(base.IdleAnimation.FrameWidth);//TODO: why idleAnimation? we might be loading a different animation
            int left = 0;
            int height = (int)(base.IdleAnimation.FrameWidth);
            int top = 0;

            localBounds = new Rectangle(left, top, width, height);
        }
        public void Hit(int damage)
        {
            blinktime = 1000;
            blink = true;
            Health -= damage;
            hurtSound.Play();
         

        }
        #endregion


    }


}
