using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ImagineCup
{
    /// <summary>
    /// Our fearless adventurer!
    /// </summary>
    class Player
    {
        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation aimUp;
        private Animation aimDiagUp;
        private Animation aimDown;
        private Animation aimDiagDown;
        private Animation jumpAimUp;
        private Animation jumpAimDiagUp;
        private Animation jumpAimDown;
        private Animation jumpAimDiagDown;

        private Animation celebrateAnimation;
        private Animation dieAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;

        private Color color;

        // Sounds
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;
        private SoundEffect hurtSound; 

        public Level Level
        {
            get { return level; }
            set { level = value; }
        }
        Level level;

        public int Score
        {
            get { return score; }
            set { score = value; }
        }
        int score;

        public int Lives
        {
            get { return lives; }
            set { lives = value; }
        }
        int lives;


        public Vector2 Center
        {
            get { return new Vector2(position.X,position.Y - localBounds.Height/2); }
        }

        

        public bool IsAlive
        {
            get { return isAlive; }
        }
        bool isAlive;

        public int Health
        {
            get { return health; }

            set
            {
                health = value;
                if (health <= 0) this.isAlive = false;
            }
        }
        int health;

        public bool Invulnerable
        {
            get { return invulnerable; }
        }
        private bool invulnerable = false;
        private int invulnerableTime = 0;

        // Battle
        private List<Bullet> bullets; //bullets in inventory
        private List<Gun> guns; //guns in inventory


        private int equippedGun; //currently equipped gun (index for guns)
        private int equippedBullet; //currently equipped bullet (index for bullets)

        //allow level's HUD to see what gun you have equipped
        public Gun EquippedGun
        {
            get { return guns[equippedGun]; }
        }

        public Bullet EquippedBullet
        {
            get { return bullets[equippedBullet]; }
        }
       
        //set by the power up gems
        // Physics state
        int direction = 1;
        int aimY = 0;
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        private float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        private Vector2 velocity;

        //input states
        GamePadState previousGamePadState = GamePad.GetState(PlayerIndex.One);
        KeyboardState previousKeyboardState = Keyboard.GetState();

#if ZUNE
        // Constants for controling horizontal movement
        private const float MoveAcceleration = 7000.0f;
        private const float MaxMoveSpeed = 1000.0f;
        private const float GroundDragFactor = 0.38f;
        private const float AirDragFactor = 0.48f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -2000.0f;
        private const float GravityAcceleration = 1700.0f;
        private const float MaxFallSpeed = 450.0f;
        private const float JumpControlPower = 0.13f;

        // Input configuration
        private const float MoveStickScale = 0.0f;
        private const Buttons JumpButton = Buttons.B;        
#else
        // Constants for controling horizontal movement
        private const float MoveAcceleration = 14000.0f;
        private const float MaxMoveSpeed = 2000.0f;
        private const float GroundDragFactor = 0.58f;
        private const float AirDragFactor = 0.65f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -4000.0f;
        private const float GravityAcceleration = 3500.0f;
        private const float MaxFallSpeed = 600.0f;
        private const float JumpControlPower = 0.14f;

        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const Buttons JumpButton = Buttons.A;
#endif

        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        /// <summary>
        /// Current user movement input.
        /// </summary>
        private float movement;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this player in world space.
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
        /// Constructors a new player.
        /// </summary>
        public Player(Level level, Vector2 position)
        {
            this.level = level;
            isAlive = true;
            health = 100;
            LoadContent();

            score = 0;
            lives = 3;

            bullets = new List<Bullet>();
            bullets.Add(new Bullet(level.Content)); //standard bullet
            guns = new List<Gun>();
            guns.Add(new Gun(level));//and standard gun
            equippedGun = 0; //currently equipped gun (index for guns)
            equippedBullet = 0; //currently equipped bullet (index for bullets)
            
            Reset(position);
        }

        /// <summary>
        /// Loads the player sprite sheet and sounds.
        /// </summary>
        public void LoadContent()
        {
            // Load animated textures.
            idleAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Player/PlayerIdle"), 0.1f, true);
            runAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Player/PlayerIdle"), 0.1f, true);
            jumpAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Player/PlayerJump"), 0.1f, false);
            celebrateAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Player/PlayerIdle"), 0.1f, false);
            dieAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Player/PlayerDead"), 0.1f, false);
            aimDiagDown = new Animation(level.Content.Load<Texture2D>("Sprites/Player/PlayerDiagDown"), 0.1f, true);
            aimDown = new Animation(level.Content.Load<Texture2D>("Sprites/Player/PlayerDown"), 0.1f, true);
            aimDiagUp = new Animation(level.Content.Load<Texture2D>("Sprites/Player/PlayerDiagUp"), 0.1f, true);
            aimUp = new Animation(level.Content.Load<Texture2D>("Sprites/Player/PlayerUp"), 0.1f, true);
            jumpAimDiagDown = new Animation(level.Content.Load<Texture2D>("Sprites/Player/PlayerJumpDiagDown"), 0.1f, true);
            jumpAimDown = new Animation(level.Content.Load<Texture2D>("Sprites/Player/PlayerJumpDown"), 0.1f, true);
            jumpAimDiagUp = new Animation(level.Content.Load<Texture2D>("Sprites/Player/PlayerJumpDiagUp"), 0.1f, true);
            jumpAimUp = new Animation(level.Content.Load<Texture2D>("Sprites/Player/PlayerJumpUp"), 0.1f, true);

            sprite.PlayAnimation(idleAnimation);


            // Calculate bounds within texture size.  TODO, do this better          
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Load sounds.            
            killedSound = level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
            jumpSound = level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = level.Content.Load<SoundEffect>("Sounds/PlayerFall");
            hurtSound = level.Content.Load<SoundEffect>("Sounds/PlayerHurt");
        }

        /// <summary>
        /// Resets the player to life.
        /// </summary>
        /// <param name="position">The position to come to life at.</param>
        public void ReloadGuns()
        {
            foreach (Gun gun in guns)
                gun.ReloadContent(level);
        }

        /// <summary>
        /// Resets the player to life.
        /// </summary>
        /// <param name="position">The position to come to life at.</param>
        public void Reset(Vector2 position)
        {
            if (!isAlive)
            {
                invulnerableTime = 2500;
                invulnerable = true;
            }
            health = 100;
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
        }

        /// <summary>
        /// The player gets hit, plays a hurt sound and becomes invincible for a short time while blinking red
        /// </summary>
        /// <param name="damage">The amount fo damage the player should take.</param>
        public void Hit(int damage)
        {
            invulnerableTime = 1000;
            invulnerable = true;
            health -= damage;
            hurtSound.Play();

        }

        /// <summary>
        /// Handles input, performs physics, and animates the player sprite.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            GetInput();

            ApplyPhysics(gameTime);

            if (health <= 0) isAlive = false;

            if (IsAlive && IsOnGround)
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                {
                    if (aimY == 0)
                    {
                        sprite.PlayAnimation(runAnimation);
                    }
                    else if (aimY > 0)
                    {
                        sprite.PlayAnimation(aimDiagDown);
                    }
                    else if (aimY < 0)
                    {
                        sprite.PlayAnimation(aimDiagUp);
                    }
                }
                else
                {
                    if (aimY == 0)
                    {
                        sprite.PlayAnimation(idleAnimation);
                    }
                    else if (aimY > 0)
                    {
                        sprite.PlayAnimation(aimDown);
                    }
                    else if (aimY < 0)
                    {
                        sprite.PlayAnimation(aimUp);
                    }
                }
            }
            else if (IsAlive)//show aiming while jumping
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                {
                    if (aimY == 0)
                    {
                        sprite.PlayAnimation(jumpAnimation);
                    }
                    else if (aimY > 0)
                    {
                        sprite.PlayAnimation(jumpAimDiagDown);
                    }
                    else if (aimY < 0)
                    {
                        sprite.PlayAnimation(jumpAimDiagUp);
                    }
                }
                else
                {
                    if (aimY == 0)
                    {
                        sprite.PlayAnimation(jumpAnimation);
                    }
                    else if (aimY > 0)
                    {
                        sprite.PlayAnimation(jumpAimDown);
                    }
                    else if (aimY < 0)
                    {
                        sprite.PlayAnimation(jumpAimUp);
                    }
                }
            }

            //update gun delay
            if(guns[equippedGun].Delay > 0)
                guns[equippedGun].Delay -= gameTime.ElapsedGameTime.Milliseconds;

            //Make the player invulnerable for 2 seconds when he spawns
            if (invulnerableTime > 0)
            {
                invulnerableTime -= gameTime.ElapsedGameTime.Milliseconds;
            }
            else if(invulnerable == true)
            {
                invulnerable = false;
            }

            // Clear input.
            movement = 0.0f;
            isJumping = false;
        }
        
        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        private void GetInput()
        {
            // Get input state.
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();

            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;
            if (movement > 0)
            {
                direction = 1;
            }
            else if (movement < 0)
            {
                direction = -1;
            }

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f;
                direction = -1;
            }
            else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
                direction = 1;
            }

            // Check verticle aim
            if (gamePadState.IsButtonDown(Buttons.DPadUp) ||
                 keyboardState.IsKeyDown(Keys.Up) ||
                 gamePadState.ThumbSticks.Left.Y >= 1f ||
                 keyboardState.IsKeyDown(Keys.W))
            {
                aimY = -1;
            }
            else if (gamePadState.IsButtonDown(Buttons.DPadDown) ||
                     keyboardState.IsKeyDown(Keys.Down) ||
                    gamePadState.ThumbSticks.Left.Y <= -1f ||
                     keyboardState.IsKeyDown(Keys.S))
            {
                aimY = 1;
            }
            else
            {
                aimY = 0;
            }

            
            //fire bullets
            if (((gamePadState.IsButtonDown(Buttons.X) && (!previousGamePadState.IsButtonDown(Buttons.X) || guns[equippedGun].IsAutomatic)) ||
                    (keyboardState.IsKeyDown(Keys.J) && (!previousKeyboardState.IsKeyDown(Keys.J) || guns[equippedGun].IsAutomatic ))) && guns[equippedGun].Delay <= 0)
            {
                FireBullets();
            }
            

            // Check if the player wants to jump.
            isJumping =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.K);

            if ((keyboardState.IsKeyDown(Keys.Q) && !previousKeyboardState.IsKeyDown(Keys.Q)) || ((gamePadState.IsButtonDown(Buttons.LeftShoulder) && (!previousGamePadState.IsButtonDown(Buttons.LeftShoulder)))))
            {
                SwitchGuns(-1);
            }

            if ((keyboardState.IsKeyDown(Keys.E) && !previousKeyboardState.IsKeyDown(Keys.E)) || ((gamePadState.IsButtonDown(Buttons.RightShoulder) && (!previousGamePadState.IsButtonDown(Buttons.RightShoulder)))))
            {
                SwitchGuns(1);
            }


            if ((keyboardState.IsKeyDown(Keys.Z) && !previousKeyboardState.IsKeyDown(Keys.Z)) /*|| ((gamePadState.IsButtonDown(Buttons.LeftShoulder) && (!previousGamePadState.IsButtonDown(Buttons.LeftShoulder)))*/)
            {
                SwitchBullets(-1);
            }
// TODO bullets scroll other direction
//            if ((keyboardState.IsKeyDown(Keys.E) && !previousKeyboardState.IsKeyDown(Keys.E)) /*|| ((gamePadState.IsButtonDown(Buttons.RightShoulder) && (!previousGamePadState.IsButtonDown(Buttons.RightShoulder))))*/)
//            {
//                SwitchBullets(1);
//            }


            //set previous states
            previousGamePadState = gamePadState;
            previousKeyboardState = keyboardState;
        }

        //fires a bullet
        public void FireBullets()
        {
            Double bulletAngle;
            if (velocity.X == 0 && aimY != 0)
                bulletAngle = (Math.PI / 2) * aimY;
            else
                bulletAngle = Math.Atan2((double)aimY, (double)direction);

            if (guns[equippedGun].Energy > bullets[equippedBullet].EnergyCost)
                guns[equippedGun].Fire(bullets[equippedBullet], bulletAngle, muzzlePos(bullets[equippedBullet]));
            else
            {
                guns.RemoveAt(equippedGun);
                SwitchGuns(1); //goto to next gun
            }   
        }

        //find the right position to start the bullet from
        public Vector2 muzzlePos(Bullet bullet){
            Vector2 muzzle = new Vector2(position.X - bullet.center.X, position.Y - bullet.center.Y);
            if (aimY == 0)//lined up with barrel of sprite for regular shots
            {
                muzzle.Y -= (sprite.Animation.FrameHeight / 3) - 2;
                muzzle.X += ((direction * sprite.Animation.FrameWidth) / 3) + (direction * 5);
            }
            else if (velocity.X == 0)//shots for standing still aiming up/down
            {
                if (aimY < 0)//lined up with barrel of sprite for straight up
                {
                    muzzle.Y -= (sprite.Animation.FrameHeight) - 10;
                    muzzle.X += ((direction * sprite.Animation.FrameWidth) / 10);
                }
                else if (aimY > 0)//lined up with barrel of sprite for straight down
                {
                    muzzle.Y += (sprite.Animation.FrameHeight / 15) - 10;
                    muzzle.X += ((direction * sprite.Animation.FrameWidth) / 3) - (direction * 5);
                }

            }
            else //diagonal shots
            {
                if (aimY < 0)//lined up with barrel of sprite for diagonal up
                {
                    muzzle.Y -= (sprite.Animation.FrameHeight / 2) + 10;
                    muzzle.X += ((direction * sprite.Animation.FrameWidth) / 3) + (direction * 3);
                }
                else if (aimY > 0)//lined up with barrel of sprite for diagonal down
                {
                    muzzle.Y -= (sprite.Animation.FrameHeight / 2) - 20;
                    muzzle.X += ((direction * sprite.Animation.FrameWidth) / 3) + (direction * 5);
                }
            }

            muzzle.Y += bullets[equippedBullet].center.Y;
            muzzle.X += bullets[equippedBullet].center.X;
            return muzzle;
        }

        public void AddGun(Gun gun)
        {
            foreach (Gun g in guns) // if its there add energy
            {
                if (g.GetType() == gun.GetType())
                { // add energy
                    g.Energy += gun.Energy;
                    if (g.Energy > g.MaxEnergy)
                        g.Energy = g.MaxEnergy;
                    return;
                }
            }
            //otherwise add the gun and equip it
            guns.Add(gun);
            equippedGun = guns.Count - 1;
        }

        public void AddBullet(Bullet bullet)
        {
            foreach (Bullet b in bullets) // if its there add score
            {
                if (b.GetType() == bullet.GetType())
                { // add ammo
                    this.score += 30;//TODO: score here
                    return;
                }
            }
            //otherwise add the bullet and equip it
            bullets.Add(bullet);
            equippedBullet = bullets.Count - 1;
        }

        //<summary>
        //switch equpped gun. Pass in 1 or -1 to scroll forward or backward.
        //</summary>
        public void SwitchGuns(int i)
        {
            equippedGun += i; //move guns

            //loop over either end
            if (equippedGun >= guns.Count)
                equippedGun = 0;
            if (equippedGun < 0)
                equippedGun = guns.Count - 1;

            //check if there is ammo
            if (guns[equippedGun].Energy <= 0 && guns[equippedGun].GetType() != typeof(Gun))
            {
                guns.RemoveAt(equippedGun);
                SwitchGuns(i); //find a working gun
            }

            //equip normal bullets
            equippedBullet = 0;
        }

        //<summary>
        //switch equpped bullet. Pass in 1 or -1 to scroll forward or backward.
        //</summary>
        public void SwitchBullets(int i)
        {
            equippedBullet += i; //move Bullets

            //loop over either end
            if (equippedBullet >= bullets.Count)
                equippedBullet = 0;
            if (equippedBullet < 0)
                equippedBullet = bullets.Count - 1;
        }

        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

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
                    if (jumpTime == 0.0f)
                        jumpSound.Play();

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sprite.PlayAnimation(jumpAnimation);
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

        /// <summary>
        /// Called when the player has been killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This parameter is null if the player was
        /// not killed by an enemy (fell into a hole).
        /// </param>
        public void OnKilled(Enemy killedBy)
        {
            isAlive = false;

            if (killedBy != null)
                killedSound.Play();
            else
                fallSound.Play();

            sprite.PlayAnimation(dieAnimation);
        }

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            sprite.PlayAnimation(celebrateAnimation);
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            color = Color.White; //Change color based on gun later!

            // Flip the sprite to face the way we are moving.
            if (Velocity.X < 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X > 0)
                flip = SpriteEffects.None;

            //is he invulnerable? from just spawning
            //Blink
            if (invulnerable)
            {
                if (invulnerableTime % 10 > 0)
                    color = Color.DarkGray;
                else
                    color = Color.LightGray;
            }
           
            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, Position, flip, color);
        }
    }
}
