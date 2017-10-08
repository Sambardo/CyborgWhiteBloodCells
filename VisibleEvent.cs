using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace ImagineCup
{
    class VisibleEvent : Event
    {
        protected Animation IdleAnimation { 
            get {return idleAnimation;}
            set { idleAnimation = value; }
        } Animation idleAnimation;

        protected AnimationPlayer Sprite {
            get { return sprite; }
            set { sprite = value; }
        } 
        public AnimationPlayer sprite;

        public VisibleEvent(Level level, Vector2 position, String spriteSet) : base(level, position) {
            Draws = true;
            LoadContent(spriteSet);
        }

        public virtual void LoadContent(string spriteSet)
        {
            // Load animations.
            spriteSet = "Sprites/" + spriteSet + "/";
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            sprite.PlayAnimation(idleAnimation);

        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            

            //maybe make a DWID function here so things dont have to override draw and redo the code every time
            sprite.PlayAnimation(idleAnimation);
            

            // Draw facing the way the enemy is moving.
            SpriteEffects flip = 1 > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }

        public override void Update()
        {
            
        }


    }
}
