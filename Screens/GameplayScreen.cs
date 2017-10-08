#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
#region Using Statements
using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Media;
#endregion
//Note: Change to YOUR NAME SPACE
namespace ImagineCup
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields
        public ContentManager Content
        {
            get { return content; }

        }
        ContentManager content;
        SpriteFont gameFont;

        // Resources for drawing.
        private SpriteBatch spriteBatch;

        // Global content.
        private SpriteFont hudFont;

        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;
        private Texture2D gameOverOverlay;

        // Meta-level game state.
        private int levelIndex = -1;
        private Level level;
        private bool wasContinuePressed;
        private Player player;
        private Texture2D lifeSprite;
        private bool gameOver = false;

        private ProgressBar energyBar;
        private ProgressBar healthBar;

        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

#if ZUNE
        private const int TargetFrameRate = 30;        
        private const int BackBufferWidth = 240;
        private const int BackBufferHeight = 320;
        private const Buttons ContinueButton = Buttons.B;        
#else
        private const int TargetFrameRate = 60;
        private const int BackBufferWidth = 1280;
        private const int BackBufferHeight = 720;
        private const Buttons ContinueButton = Buttons.A;
#endif
        #endregion
        #region Initialization
        //DEBUG
        Texture2D rectTex; 
        /// <summary>
        /// Constructor.
        /// </summary>
        /// 
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }
        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");
            gameFont = content.Load<SpriteFont>("gamefont");

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);

            // Load fonts
            hudFont = content.Load<SpriteFont>("Fonts/Hud");

            // Load overlay textures
            winOverlay = content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = content.Load<Texture2D>("Overlays/you_died");
            gameOverOverlay = content.Load<Texture2D>("Overlays/gameover_temp");

            lifeSprite = content.Load<Texture2D>("Sprites/lives");

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f;
            MediaPlayer.Play(content.Load<Song>("Sounds/music1"));

            //DEBUG
            rectTex = new Texture2D(ScreenManager.GraphicsDevice,1, 1);
            Color[] data = {  
            // I like transparent magenta, but any color will do.  
            new Color(1f, 0f, 1f, .5f),  
            };
            rectTex.SetData<Color>(data);  

            LoadNextLevel();

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();


            energyBar = new ProgressBar(level, new Vector2(0, 100), 255, "bg", "bg", "bg", Color.Turquoise, Color.DarkBlue);
            healthBar = new ProgressBar(level, new Vector2(0, 30), 255, "bg", "bg", "bg", Color.Red, Color.Maroon);
        }
        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }
        #endregion
        #region Update and Draw
        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
 bool coveredByOtherScreen)
        {
            if (gameOver && wasContinuePressed)
            {
                LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen()); 
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            if (IsActive)
            {
                HandleGameInput();
                level.Update(gameTime);
            }
        }
        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;
            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];
            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
            input.GamePadWasConnected[playerIndex];
            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
            }
        }


        private void HandleGameInput()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamepadState = GamePad.GetState(PlayerIndex.One);

            bool continuePressed =
                keyboardState.IsKeyDown(Keys.Space) ||
                gamepadState.IsButtonDown(ContinueButton);

            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)//if the player is dead take a life and respawn him
                {
                    if (level.Player.Lives > 0)
                    {
                        level.Player.Lives--;
                        level.StartNewLife();
                    }
                 /*   else //continue from begining of level
                    {
                        lives = 3;
                        ReloadCurrentLevel();
                        health = level.Player.Health;
                    } */
                }

                if (level.ReachedExit)
                {
                    player = level.Player;
                    LoadNextLevel();
                }
            }
            wasContinuePressed = continuePressed;
        }

        private void LoadNextLevel()
        {
            // Find the path of the next level.
            string levelPath;

            // Loop here so we can try again when we can't find a level.
            while (true)
            {
                // Try to find the next level. They are sequentially numbered txt files.
                levelPath = String.Format("Levels/{0}.txt", ++levelIndex);
                levelPath = Path.Combine(StorageContainer.TitleLocation, "Content/" + levelPath);
                if (File.Exists(levelPath))
                    break;

                // If there isn't even a level 0, something has gone wrong.
                if (levelIndex == 0)
                    throw new Exception("No levels found.");

                // Whenever we can't find a level, start over again at 0.
                //CHANGE TO WIN SCREEN
                levelIndex = -1;
            }

          /*  // Unloads the content for the current level before loading the next one.
            if (level != null)
                level.Dispose(); */ //TODO is this a memory leak?

            // Load the level.
            level = new Level(this, levelPath, player);
        }

        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
            Color.CornflowerBlue, 0, 0);
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            level.Draw(gameTime, spriteBatch);

            DrawHud();

            base.Draw(gameTime);

            //DebugCol();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }
        #endregion

        private void DebugCol()
        {
            //Debug
            Matrix cameraTransform = Matrix.CreateTranslation(-level.cameraPosition, 0.0f, 0.0f);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, cameraTransform);

            //draw bounding rectangles 
            foreach (Enemy enemy in level.Enemies)
            {

                if (enemy != null && enemy.IsAlive)
                {
                    spriteBatch.Draw(rectTex, enemy.BoundingRectangle, null, Color.White);
                }
            }

            spriteBatch.Draw(rectTex, level.Player.BoundingRectangle, null, Color.White);

            spriteBatch.End();

        }


        private void Debug(String str) {
            spriteBatch.DrawString(hudFont, str, new Vector2(200, 20), Color.Black);
        }

 /*       private void drawAmmo() {

            Rectangle titleSafeArea = ScreenManager.GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);

            Gun gun = level.Player.EquippedGun;
            Bullet bullet = level.Player.EquippedBullet;
            int numbullets = (int)(gun.Energy / (gun.EnergyCostModifier * bullet.EnergyCost));
            float energyPercentage = gun.Energy / gun.MaxEnergy;

            Color fullColor = energyBar.FullColor;
            Color emptyColor = energyBar.EmptyColor;
            float percent = energyPercentage;

            //DEbug, TODO remove
 //            Color derp = new Color ((float)(fullColor.R / 255.0 * percent ), (float)(fullColor.G / 255.0 * percent ), (float)(fullColor.B / 255.0 * percent ));
 //            Debug(derp.ToString());


            energyBar.Draw(spriteBatch, energyPercentage);

            //TODO remove
            //int percentcolor = (int)(energyPercentage * 255);
            //spriteBatch.Draw(level.Content.Load<Texture2D>("blank"), new Rectangle((int)(hudLocation.X), (int)(150 + hudLocation.Y), (int)(100*energyPercentage), 25), new Color(energyPercentage,0.0F,1.0F));
            //spriteBatch.DrawString(hudFont, "               " +((numbullets-1) / 60 ).ToString(), new Vector2(hudLocation.X, hudLocation.Y+60), Color.Aqua);
           for (i = 0; i < ((numbullets %60 == 0? 60 : numbullets % 60)) / 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    spriteBatch.Draw(bullet.Sprite, new Rectangle((int)(hudLocation.X + j * 10), (int)(150 + hudLocation.Y + i * 10), 10, 10 ), Color.White);
                }
            }           
            for (int k = 0; k < numbullets %6; k++)
            {
                spriteBatch.Draw(bullet.Sprite, new Rectangle((int)(hudLocation.X + k * 10), (int)(150 + hudLocation.Y + i * 10), 10, 10), Color.White);
            }
       
        } */

     private void DrawHud()
        {
            spriteBatch.Begin();

            Rectangle titleSafeArea = ScreenManager.GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);
        
            // Draw score
            DrawShadowedString(hudFont, "SCORE: " + level.Player.Score.ToString(), hudLocation, Color.Yellow);
            float stringHeight = hudFont.MeasureString(level.Player.Score.ToString()).Y;

            //Draw Health and lives
            healthBar.Draw(spriteBatch, level.Player.Health / 100.0f);
            DrawShadowedString(hudFont,level.Player.Health.ToString(), hudLocation + new Vector2(10.0f, stringHeight * 1.1f), Color.Red);
            DrawShadowedString(hudFont, "x" + level.Player.Lives.ToString(), hudLocation + new Vector2(65.0f, stringHeight * 1.1f), Color.YellowGreen);
            spriteBatch.Draw(lifeSprite, hudLocation + new Vector2(45.0f, stringHeight * 1.1f), Color.White);
            
            //Draw gun and ammo
            Gun gun = level.Player.EquippedGun;
            Bullet bullet = level.Player.EquippedBullet;
            string ammo;
            ammo = "Unlimited";
            int numbullets = 0;
            float energyPercentage = gun.Energy / gun.MaxEnergy;
            if (!(gun.GetType() == typeof(Gun) && bullet.GetType() == typeof(Bullet)))
            {
                numbullets = (int)(gun.Energy / (gun.EnergyCostModifier * bullet.EnergyCost)); 
                ammo = numbullets.ToString();
            }

            energyBar.Draw(spriteBatch, energyPercentage);
            DrawShadowedString(hudFont, "Shots: " + ammo, hudLocation + new Vector2(0.0f, 103), Color.YellowGreen);
       
            spriteBatch.Draw(gun.DisplaySprite, hudLocation + new Vector2(0.0f, stringHeight * 2.2f), Color.White);
            spriteBatch.Draw(bullet.Sprite, hudLocation + new Vector2(gun.DisplaySprite.Width + 5, (stringHeight * 2.2f)), Color.White);
            DrawShadowedString(hudFont, bullet.Name, hudLocation + new Vector2(gun.DisplaySprite.Width + bullet.Sprite.Width + 5, (stringHeight * 2.2f)), Color.YellowGreen);

            // Determine the status overlay message to show.
            Texture2D status = null;
            if (level.ReachedExit)
            {
                    status = winOverlay;
            }
            else if (!level.Player.IsAlive)
            {
                if (level.Player.Lives > 0)
                {
                    status = diedOverlay;
                }
                else
                {
                    status = gameOverOverlay;
                    gameOver = true;
                }
            }

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }

            spriteBatch.End();
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }
    }
}
