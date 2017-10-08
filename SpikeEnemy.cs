using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace ImagineCup
{
    class SpikeEnemy : Enemy
    {
        private const double g = 980;
        private double Vy = 8;

        public SpikeEnemy(Level level, Vector2 position)
            : base(level, position, "Spike")
        {
            Health = 10;
            base.CollisionDamage = 50;
        }

        public override void Update(GameTime gameTime)
        {
            ApplyPhysics(gameTime);
        }

        public override void ApplyPhysics(GameTime gameTime)
        {
            //TODO: copy player physics update into here, trim the fat
            Vector2 previousPosition = base.Position;
            //double time = ((double)gameTime.ElapsedGameTime.Milliseconds / 1000.0);

            float time = (float)gameTime.ElapsedGameTime.TotalSeconds;

          
            Vy += g * time;

            double y = Vy * time;//what y have we fallen
               // y = 1.1;
            base.Position = new Vector2((float)Math.Round(base.Position.X), (float)Math.Round((base.Position.Y + y)));//new position
            

            base.HandleCollisions();

            if (base.Position.Y == previousPosition.Y)
                Vy = 0;

        }


        public override void SetBounds()
        {



            int width = (int)(base.IdleAnimation.FrameWidth*.90);//TODO: why idleAnimation? we might be loading a different animation
            int left = base.IdleAnimation.FrameWidth - width - 5;//minus 5 percent, 2.5 on each side
            int height = (int)(base.IdleAnimation.FrameWidth*.60);//half the height
            int top = (int)(base.IdleAnimation.FrameWidth * .40);//moved down half a height

            LocalBounds = new Rectangle(left, top, width, height);
        }
    }
}