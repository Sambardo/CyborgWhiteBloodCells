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
    }

    //Enum used for determining type
    //Enemies will be able to:
    //Pace, Jump, stand, Float, Fly
    enum Behaviour
    {
        Stand, //enemy stands and shoots at the player
        Pace, //enemy paces on the platform and shoots at the player
        Jump, //enemy runs across the screen *FAST* jumping. 
        JumpShooter, //enemy runs across the screen *FAST?* jumping & shooting. 
        Floater, //floats in the air, rotating and shooting the player
        Fly, //fly's forward, shoots at the player, turns around if it hits the begining or end of level
    }
   

    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    class Enemy
    {

        public bool IsAlive
        {
            get { return isAlive; }
        }
        bool isAlive;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Position in world space of the bottom center of this enemy.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        protected Vector2 position;

        private float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;

        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this enemy in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        /// <summary>
        /// An array for the bullets of this enemy.  WHOA!
        /// </summary>
        protected Bullet[] bullets;
        protected float timeSinceFiring;
        protected float MaxNonFiringTime = 1.0f;

        //way the enemy should act
        Behaviour behave;

        public int Health
        {
            get { return health; }
            set
            {
                health = value;
                if (health <= 0)
                {
                    this.isAlive = false;
                    this.OnKilled();
                }
            }
        }
        protected int health;

        // Animations
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation idleAnimation;
        private Animation dieAnimation;
        private AnimationPlayer sprite;

        // Sounds
        private SoundEffect killedSound;

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        protected FaceDirection direction = FaceDirection.Left;

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        private float waitTime;

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        private const float MaxWaitTime = 0.15f;

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
#if ZUNE
        private const float MoveSpeed = 64.0f;
        private const float JumpSpeed = 96.0f;
        
        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.45f;
        private const float JumpLaunchVelocity = -2000.0f;
        private const float GravityAcceleration = 1700.0f;
        private const float MaxFallSpeed = 450.0f;
        private const float JumpControlPower = 0.13f;
#else
        private const float MoveSpeed = 256.0f;
        private const float JumpSpeed = 384.0f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.45f;
        private const float JumpLaunchVelocity = -4000.0f;
        private const float GravityAcceleration = 3500.0f;
        private const float MaxFallSpeed = 600.0f;
        private const float JumpControlPower = 0.14f;

        // Constants for controlling bullet movement
        const int BulletDamage = 5; // base bullet damage
        const float BulletSpeed = 8; // base bullet speed
        const int MaxBullets = 30;
#endif

        #region Enemy Construction

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public Enemy(Level level, Vector2 position, string spriteSet, int type)
        {
            this.level = level;
            this.position = position;
            this.isAlive = true;
            behave = (Behaviour)type;
            switch (behave)
            {
                case Behaviour.Stand:
                    health = 1;
                    break;
                case Behaviour.Pace:
                    health = 1;
                    break;
                case Behaviour.Jump:
                    health = 1;
                    break;
                case Behaviour.JumpShooter:
                    health = 1;
                    break;
                case Behaviour.Floater:
                    health = 1;
                    break;
                case Behaviour.Fly:
                    health = 1;
                    break;
            }

            LoadContent(spriteSet);

        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public void LoadContent(string spriteSet)
        {
            // Load animations.
            spriteSet = "Sprites/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.1f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Die"), 0.07f, false);
            sprite.PlayAnimation(idleAnimation);

            // Load sounds.
            killedSound = Level.Content.Load<SoundEffect>("Sounds/MonsterKilled");

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * .85);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Load the bullets!
            bullets = new Bullet[MaxBullets];
            for (int i = 0; i < MaxBullets; i++)
            {
                bullets[i] = new Bullet(Level.Content.Load<Texture2D>("Sprites\\bullet"));
            }
        }
        #endregion

        #region Enemy Update

        /// <summary>
        /// Performs actions based on behavior
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (!isAlive) return;

            switch (behave)
            {
                case Behaviour.Stand:
                    ApplyPhysicsTurn(gameTime);
                    fireIfPossible(gameTime);
                    UpdateBullets();
                    break;
                case Behaviour.Pace:
                    ApplyPhysics(gameTime);
                    fireIfPossible(gameTime);
                    UpdateBullets();
                    break;
                case Behaviour.Jump:
                    ApplyPhysics(gameTime);
                    break;
                case Behaviour.JumpShooter:
                    ApplyPhysics(gameTime);
                    fireIfPossible(gameTime);
                    UpdateBullets();
                    break;
                case Behaviour.Floater:
                    ApplyPhysicsRotate(gameTime);
                    fireIfPossible(gameTime);
                    UpdateBullets();
                    break;
                case Behaviour.Fly:
                    ApplyPhysicsFly(gameTime);
                    fireIfPossible(gameTime);
                    UpdateBullets();
                    break;
            }
        }



        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsAlive)
            {
                //if they already died make them disapear
                if (sprite.FrameIndex == (dieAnimation.FrameCount - 1)) return;
                
                sprite.PlayAnimation(dieAnimation);
            }
            else if (!Level.Player.IsAlive ||
                      Level.ReachedExit ||
                      behave == Behaviour.Stand ||
                      waitTime > 0)
            {
                sprite.PlayAnimation(idleAnimation);
              
                // Draw the enemy's bullets
                foreach (Bullet bullet in bullets)
                {
                    if (bullet.alive)
                    {
                        spriteBatch.Draw(bullet.sprite,
                        bullet.position, Color.Lime);
                    }
                }
            }
            else
            {
              sprite.PlayAnimation(runAnimation);
              
                
              // Draw the enemy's bullets
              foreach (Bullet bullet in bullets)
              {
                  if (bullet.alive)
                  {
                      spriteBatch.Draw(bullet.sprite,
                          bullet.position, Color.Lime);
                  }
              }
            }

                // Draw facing the way the enemy is moving.
                SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                sprite.Draw(gameTime, spriteBatch, Position, flip);
        }

        public void OnKilled()
        {
            isAlive = false;
            killedSound.Play();
        }

        public void UpdateBullets()
        {
            foreach (Bullet bullet in bullets)
            {
                if (bullet.alive)
                {
                    bullet.position += bullet.velocity;

                    Rectangle bulletRect = new Rectangle(
                        (int)bullet.position.X,
                        (int)bullet.position.Y,
                        bullet.sprite.Width,
                        bullet.sprite.Height);

                    if (!Level.isOnScreen(bullet.position))
                    {
                        bullet.alive = false;
                    }
                    else
                    {
                        if (bulletRect.Intersects(Level.Player.BoundingRectangle) &&
                            Level.Player.IsAlive && !Level.Player.Invulnerable)
                        {
                            bullet.alive = false;
                            Level.Player.Health -= BulletDamage;
                        }
                    }
                }
            }
        }
        #endregion

        #region Enemy Jump Physics / Stuff

        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            //only have pacing, jumping and jumping-shooting enemies move on the platforms
            if (behave == Behaviour.Jump || behave == Behaviour.Pace || behave == Behaviour.JumpShooter)
            {

                // Calculate tile position based on the side we are walking towards.
                float posX = Position.X + localBounds.Width / 2 * (int)direction;
                int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
                if ((int)direction == -1) tileX--;
                int tileY = (int)Math.Floor(Position.Y / Tile.Height);

                if (waitTime > 0)
                {
                    // Wait for some amount of time.
                    waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                    if (waitTime <= 0.0f)
                    {
                        // Then turn around.
                        direction = (FaceDirection)(-(int)direction);
                    }
                }
                else
                {
                    // If we are about to run off a cliff, start waiting.
                    if (isOnGround && Level.EnemyGetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                    {
                        Random rand_gen = new Random();
                        int rand = rand_gen.Next(2);
                        if (rand == 0 || behave == Behaviour.Pace)//pacing enemies should never jump
                        {
                            waitTime = MaxWaitTime;
                            velocity.X = 0;
                        }
                        else
                        {
                            isJumping = true;
                            velocity.X = (int)direction * JumpSpeed;
                            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
                            velocity.Y = DoJump(velocity.Y, gameTime);
                        }
                    }
                    else if (isOnGround &&
                        ((Level.EnemyGetCollision(tileX + (int)direction, tileY - 2) != TileCollision.Passable &&
                        Level.EnemyGetCollision(tileX + (int)direction, tileY - 2) != TileCollision.Offscreen) ||
                        (Level.EnemyGetCollision(tileX + (int)direction, tileY - 1) != TileCollision.Passable &&
                        Level.EnemyGetCollision(tileX + (int)direction, tileY - 1) != TileCollision.Offscreen)))
                    {
                        isJumping = true;
                        velocity.X = (int)direction * JumpSpeed;
                        velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
                        velocity.Y = DoJump(velocity.Y, gameTime);
                    }
                    else if (Level.EnemyGetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Offscreen)
                    {
                        waitTime = MaxWaitTime;
                        velocity.X = 0;
                    }
                    else
                    {
                        if (!isOnGround) velocity.X = (int)direction * JumpSpeed;
                        else velocity.X = (int)direction * MoveSpeed;
                        velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
                        velocity.Y = DoJump(velocity.Y, gameTime);
                    }

                }

                // Apply velocity.
                Position += velocity * elapsed;
                Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

                // If the player is now colliding with the level, separate them.
                HandleCollisions();

                // If the collision stopped us from moving, reset the velocity to zero.
                if (Position.X == previousPosition.X)
                    velocity.X = 0;

                if (Position.Y == previousPosition.Y)
                    velocity.Y = 0;
            }
        }

        public void ApplyPhysicsFly(GameTime gameTime)
        {
        }

        public void ApplyPhysicsRotate(GameTime gameTime)
        {
        }

        public void ApplyPhysicsTurn(GameTime gameTime)
        {
            if (level.Player.Position.X < Position.X)
            {
                direction = FaceDirection.Left;
            }
            else
            {
                direction = FaceDirection.Right;
            }
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sprite.PlayAnimation(idleAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                    isJumping = false;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        private void HandleCollisions()
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

        #endregion

        #region Enemy Weaponry

        /// <summary>
        /// Fire a bullet.  
        /// </summary>
        public void fireBullet()
        {
            // enemyShootSound.Play(); // No enemy fire sound exists now.  Should there be one?  TBD.

            foreach (Bullet bullet in bullets)
            {
                if (!bullet.alive)
                {
                    bullet.alive = true;
                    bullet.position = position - bullet.center;
                    bullet.position.Y -= (sprite.Animation.FrameHeight / 2);
                    float dX = Level.Player.Position.X - this.Position.X;
                    float dY = Level.Player.Position.Y - this.Position.Y;
                    float dTotal = (float)Math.Sqrt(dX*dX + dY*dY);
                    bullet.velocity = new Vector2(BulletSpeed * dX/dTotal  , BulletSpeed * dY/dTotal);
                    return;
                }
            }
        }

        public void fireIfPossible(GameTime gameTime)
        {
            // If we've fired recently...
            if (timeSinceFiring <= MaxNonFiringTime)
            {
                // Just increment the timer.
                timeSinceFiring += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                // Otherwise, fire!
                float dX = Level.Player.Position.X - this.Position.X;

                if (Level.isOnScreen(this.Position) && (dX / Math.Abs(dX)) == (int)direction )
                {                    
                    fireBullet();
                    timeSinceFiring = 0.0f;
                }
                // sprite.PlayAnimation(firingAnimation); // Firing animation not implemented yet.
            }
        }

        #endregion
    }
}
