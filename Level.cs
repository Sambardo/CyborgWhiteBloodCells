using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace ImagineCup
{
    /// <summary>
    /// A uniform grid of tiles with collections of GunPowerups and enemies.
    /// The level owns the player and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    class Level : IDisposable
    {
        // Physical structure of the level.
        private Tile[,] tiles;
        private Layer[] layers;

        private Queue<Enemy> EnemySpawnQueue = new Queue<Enemy>();

        // The layer which entities are drawn on top of.
        private const int EntityLayer = 3;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;

        private List<GunPowerup> GunPowerups = new List<GunPowerup>();

        private List<BulletPowerup> BulletPowerups = new List<BulletPowerup>();

        public List<Enemy> Enemies
        {
            get { return enemies; }
        }
        private List<Enemy> enemies = new List<Enemy>();

        public List<Event> Events
        {
            get { return events; }
        }
        private List<Event> events = new List<Event>();
        

        public List<Bullet> PlayerBullets
        {
            get { return playerBullets; }
        }
        private List<Bullet> playerBullets = new List<Bullet>();

        public List<Bullet> EnemyBullets
        {
            get { return enemyBullets; }
        }
        private List<Bullet> enemyBullets = new List<Bullet>();

        private Queue<Bullet> pBulletRemoveQueue = new Queue<Bullet>();
        private Queue<Bullet> eBulletRemoveQueue = new Queue<Bullet>();

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);
        

        // Level game state.
        public float cameraPosition; //used for scrolling, left most point
        private Rectangle currentScreen = new Rectangle();
        private Random random = new Random(354668); // Arbitrary, but constant seed

        public int Score
        {
            get { return score; }
            set
            {
                score = value;
            }
        }
        int score;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        private const int PointsPerSecond = 5;

        //Used for input
        GamePadState previousGamePadState = GamePad.GetState(PlayerIndex.One);
        KeyboardState previousKeyboardState = Keyboard.GetState();

        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        private SoundEffect exitReachedSound;

        #region Loading

        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="path">
        /// The absolute path to the level file to be loaded.
        /// </param>
        public Level(GameplayScreen gscreen, string path, Player player)
        {
            // Create a new content manager to load content used just by this level.
            content = gscreen.Content;
            this.player = player;

            LoadTiles(path);

            // Load background layer textures. For now, all levels must
            // use the same backgrounds. It will scroll through them, looping over.
            //Layers go from back to front and scroll at different speeds.
            layers = new Layer[4];
            layers[0] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
            layers[1] = new Layer(Content, "Backgrounds/Layer1", 0.4f);
            layers[2] = new Layer(Content, "Backgrounds/Layer2", 0.6f);
            layers[3] = new Layer(Content, "Backgrounds/Layer3", 0.8f);
         
            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
        }

        /// <summary>
        /// Iterates over every tile in the structure file and loads its
        /// appearance and behavior. This method also validates that the
        /// file is well-formed with a player start point, exit, etc.
        /// </summary>
        /// <param name="path">
        /// The absolute path to the level file to be loaded.
        /// </param>
        private void LoadTiles(string path)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            if (exit == InvalidPosition)
                throw new NotSupportedException("A level must have an exit.");

        }

        /// <summary>
        /// Loads an individual tile's appearance and behavior.
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Gem
                case 'G':
                   
                    return LoadPowerupBulletTile(x, y, new RocketBullet(this.Content));

                // Automatic Power Up
                case 'M':
                    return LoadPowerupGunTile(x, y, new MachineGun(Alignment.player,this));

                // Spreead Shot Power Up
                case 'S':
                    return LoadPowerupGunTile(x, y, new SpreadGun(Alignment.player, this));

                // Floating platform
                case '-':
                    return LoadTile("Platform", TileCollision.Platform);

                // Various enemies
                case 'A': //stand
                    return LoadEnemyTile(new SpikeEnemy(this, RectangleExtensions.GetBottomCenter(GetBounds(x, y))));
                case 'B': //pace
                    return LoadEnemyTile(new PacingEnemy(this,RectangleExtensions.GetBottomCenter(GetBounds(x, y)), "Spike" ));
                case 'C': //jump
                    return LoadEnemyTile(new PacingShootingEnemy(this, RectangleExtensions.GetBottomCenter(GetBounds(x, y)), new MachineGun(Alignment.enemy, this)));
                case 'D': //jump and shoot!
                    return LoadEnemyTile(new BossEnemy(this, RectangleExtensions.GetBottomCenter(GetBounds(x, y))));
                case 'E': //float
                    //return LoadEnemyTile(x, y, "MonsterD", 4);
                case 'F': //fly
                    return LoadEventTile(new FlyingSpawner(this, RectangleExtensions.GetBottomCenter(GetBounds(x, y))));

                // Platform block
                case '~':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Platform);

                // Passable block
                case ':':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Passable);

                // Player 1 start point
                case '1':
                    return LoadStartTile(x, y);

                // Impassable block
                case '#':
                    return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        /// <summary>
        /// Creates a new tile. The other tile loading methods typically chain to this
        /// method after performing their special logic.
        /// </summary>
        /// <param name="name">
        /// Path to a tile texture relative to the Content/Tiles directory.
        /// </param>
        /// <param name="collision">
        /// The tile collision type for the new tile.
        /// </param>
        /// <returns>The new tile.</returns>
        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }


        /// <summary>
        /// Loads a tile with a random appearance.
        /// </summary>
        /// <param name="baseName">
        /// The content name prefix for this group of tile variations. Tile groups are
        /// name LikeThis0.png and LikeThis1.png and LikeThis2.png.
        /// </param>
        /// <param name="variationCount">
        /// The number of variations in this group.
        /// </param>
        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = random.Next(variationCount);
            return LoadTile(baseName + index, collision);
        }


        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadStartTile(int x, int y)
        {
            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            if (Player != null)
            {
                player.Position = start;
                player.Level = this;
                player.ReloadGuns();
            }
            else
                player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        public Tile LoadEnemyTile(Enemy en)
        {
            enemies.Add(en);
            //TODO: etype in block of ifs? can I pass a class type or something?
            return new Tile(null, TileCollision.Passable);
        }
        public Tile LoadEventTile(Event en)
        {
            events.Add(en);
            //TODO: etype in block of ifs? can I pass a class type or something?
            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates a gem and puts it in the level.
        /// </summary>
        private Tile LoadPowerupGunTile(int x, int y, Gun g)
        {
            Point position = GetBounds(x, y).Center;
            GunPowerups.Add(new GunPowerup(this, new Vector2(position.X, position.Y), g));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadPowerupBulletTile(int x, int y, Bullet b)
        {
            Point position = GetBounds(x, y).Center;
            BulletPowerups.Add(new BulletPowerup(this, new Vector2(position.X, position.Y), b));

            return new Tile(null, TileCollision.Passable);
        }


        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            //Content.Unload(); TODO memory leak?
        }

        #endregion

        #region Bounds and collision

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        public TileCollision EnemyGetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Offscreen;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            //take in input
            GetInput();

            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);
            }
            else if (ReachedExit)
            {
                //DO SOMETHING POINT RELATED
            }
            else
            {
                Player.Update(gameTime);

                UpdatePlayerBullets(gameTime);
                UpdateEnemyBullets(gameTime);


                UpdateGems(gameTime);

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    OnPlayerKilled(null);

                UpdateEnemies(gameTime);

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the GunPowerups.
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }
        }


        /// <summary>
        /// Animates each gem and checks to allows the player to collect them.
        /// </summary>
        private void UpdateGems(GameTime gameTime)
        {
            for (int i = 0; i < GunPowerups.Count; ++i)
            {
                GunPowerup gem = GunPowerups[i];

                gem.Update(gameTime);

                if (gem.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    GunPowerups.RemoveAt(i--);
                    OnGunCollected(gem, Player);
                }
            }


            for (int i = 0; i < BulletPowerups.Count; ++i)
            {
                BulletPowerup gem = BulletPowerups[i];

                gem.Update(gameTime);

                if (gem.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    BulletPowerups.RemoveAt(i--);
                    OnBulletCollected(gem, Player);
                }
            }
        }

        /// <summary>
        /// Updates the playerBullets AND checks for collision with enemies
        /// </summary>
        private void UpdatePlayerBullets(GameTime gameTime)
        {
            foreach (Bullet bullet in playerBullets)
                {
                    //check if bullets are on the screen
                    if (!isOnScreen(bullet.position))
                        bullet.isAlive = false;

                    if (bullet.isAlive)
                        bullet.Update();

                    foreach (Enemy enemy in enemies)
                        if (bullet.BoundingRectangle.Intersects(enemy.BoundingRectangle) && enemy.IsAlive)
                        {
                            bullet.isAlive = false;
                            enemy.Hit(bullet.Damage);
                            if (!enemy.IsAlive) //TODO
                                player.Score += enemy.Score;
                        }
                
                        if(!bullet.isAlive)
                            pBulletRemoveQueue.Enqueue(bullet);
                }
            RemoveBullets(playerBullets, pBulletRemoveQueue);
        }

        /// <summary>
        /// Updates the enemy bullets AND checks for collision with player
        /// </summary>
        private void UpdateEnemyBullets(GameTime gameTime)
        {
            foreach (Bullet bullet in enemyBullets)
            {
                //check if bullets are on the screen
                if (!isOnScreen(bullet.position))
                    bullet.isAlive = false;

                if (bullet.isAlive) //update and check collision
                {
                    bullet.Update();
                    if (bullet.BoundingRectangle.Intersects(player.BoundingRectangle)) //player collision
                    {
                        bullet.isAlive = false;
                        player.Hit(bullet.Damage);
                    }
                }
               //TODO player collision

                if (!bullet.isAlive)
                    eBulletRemoveQueue.Enqueue(bullet);
            }
            RemoveBullets(enemyBullets, eBulletRemoveQueue);
        }

        /// <summary>
        /// enqueues enemies to be deleted/added based on the isalive inside them
        /// </summary>
        /// <param name="enemy"></param>
        public void QueueEnemyChange(Enemy enemy) {
            EnemySpawnQueue.Enqueue(enemy);
        }


        public void AddAndRemoveEnemies()
        {

            while (EnemySpawnQueue.Count > 0)
            {
                Enemy en = EnemySpawnQueue.Dequeue();
                if (en.IsAlive) LoadEnemyTile(en);
                else enemies.Remove(en);
            }
        }

        public void RemoveBullets(List<Bullet> bList, Queue<Bullet> bRemoveQueue)
        {
            while (bRemoveQueue.Count > 0)
            {
                Bullet bullet = bRemoveQueue.Dequeue();
                bList.Remove(bullet);
            }
        }

        /// <summary>
        /// Animates each enemy and allow them to kill the player.
        /// </summary>
        private void UpdateEnemies(GameTime gameTime)
        {

            AddAndRemoveEnemies();

            foreach (Event even in events) {
                even.Update();
            }

            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);

                // Touching an enemy instantly kills the player
                if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle) && enemy.IsAlive && !player.Invulnerable)
                {
                    player.Hit(enemy.CollisionDamage);
                }
            }
        }

        


        //takes in input, used to pause and bring up menues
        private void GetInput()
        {
            // Get input state.
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();

            previousKeyboardState = keyboardState;
            previousGamePadState = gamePadState;
  
        }

        /// <summary>
        /// Called when a gem is collected.
        /// </summary>
        /// <param name="gem">The gem that was collected.</param>
        /// <param name="collectedBy">The player who collected this gem.</param>
        private void OnGunCollected(GunPowerup gem, Player collectedBy)
        {
            score += gem.PointValue;

            gem.OnCollected(collectedBy);
        }

        private void OnBulletCollected(BulletPowerup gem, Player collectedBy)
        {
            score += gem.PointValue;

            gem.OnCollected(collectedBy);
        }

        /// <summary>
        /// Called when the player is killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This is null if the player was not killed by an
        /// enemy, such as when a player falls into a hole.
        /// </param>
        private void OnPlayerKilled(Enemy killedBy)
        {
            Player.OnKilled(killedBy);
        }

        /// <summary>
        /// Called when the player reaches the level's exit.
        /// </summary>
        private void OnExitReached()
        {
            Player.OnReachedExit();
            exitReachedSound.Play(); 
            reachedExit = true;
        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Vector2 Drop = new Vector2(Player.Position.X, currentScreen.Top);//drops player in from top of screen
            Player.Reset(Drop);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            for (int i = 0; i <= EntityLayer; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, cameraTransform);

            DrawTiles(spriteBatch);

            foreach (GunPowerup gem in GunPowerups)
                gem.Draw(gameTime, spriteBatch);

            foreach (BulletPowerup gem in BulletPowerups)
                gem.Draw(gameTime, spriteBatch);

            foreach (Bullet bullet in playerBullets)
                bullet.Draw(gameTime, spriteBatch);

            foreach (Bullet bullet in enemyBullets)
                bullet.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);
            foreach (Event ev in events) {
                if (ev.Draws) ((VisibleEvent)ev).Draw(gameTime,spriteBatch);//THIS IS VERY BAD
            }

            spriteBatch.End();

            //currently unused, draws a foreground layer which would obscure player and enemies.
            //Remove or Use later.
            spriteBatch.Begin();
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();
        }


        //Scrolls the camera position to keep moving forward.
        //The camera position will always be the origin of the screen.
        //Viewmargin's are different on the zune, which has a wide, narrow screen
        //This is also where the update needs to occur to keep track of screen edges
        private void ScrollCamera(Viewport viewport)
        {
#if ZUNE
            const float ViewMargin = 0.45f;
#else
            const float ViewMargin = 0.35f;
#endif

            // Calculate the edges of the screen.
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPosition + marginWidth;
            float marginRight = cameraPosition + viewport.Width - marginWidth;

            // Calculate how far to scroll when the player is near the edges of the screen.
            float cameraMovement = 0.0f;
            if (Player.Position.X < marginLeft)
                cameraMovement = Player.Position.X - marginLeft;
            else if (Player.Position.X > marginRight)
                cameraMovement = Player.Position.X - marginRight;

            // Update the camera position, but prevent scrolling off the ends of the level.
            float maxCameraPosition = Tile.Width * Width - viewport.Width;
            cameraPosition = MathHelper.Clamp(cameraPosition + cameraMovement, 0.0f, maxCameraPosition);

            //update the rectangle that contains the whole screen
            currentScreen = new Rectangle((int)Math.Ceiling(cameraPosition), 0, viewport.Width, viewport.Height); 
        }

        //pass in a vector position and find out of its on the screen (TRUE) or off (FALSE)
        public bool isOnScreen(Vector2 position)
        {
            if (currentScreen.Contains((int)Math.Floor(position.X), (int)Math.Ceiling(position.Y)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // Calculate the visible range of tiles.
            int left = (int)Math.Floor(cameraPosition / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, Width - 1);

            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x <= right; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.Red);//change back to no tint when actual blocks are added in
                    }
                }
            }
        }

        #endregion
    }
}
