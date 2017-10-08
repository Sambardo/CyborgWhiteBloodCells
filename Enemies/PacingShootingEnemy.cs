using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;


namespace ImagineCup
{
    class PacingShootingEnemy : PacingEnemy
    {

            
        private Vector2 muzzlePosition;
        public Gun Gun
        {
            get { return gun; }
            set { gun = value; }
        }Gun gun;


        public PacingShootingEnemy(Level level, Vector2 Position, Gun gu)
            : base(level, Position, "ChestShooter")
        {
            Gun = gu;
            muzzlePosition = new Vector2(-15, -45);
        }

        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
            bool lookright = (this.Position.X < Level.Player.Position.X && (this.Direction == FaceDirection.Right));
            bool lookleft = (this.Position.X > Level.Player.Position.X && (this.Direction == FaceDirection.Left));//TODO: WTF
            if (Level.isOnScreen(Position))
            {
                gun.Delay -= gametime.ElapsedGameTime.Milliseconds;
                if (gun.Delay <= 0 && IsAlive)
                {
                    if ( lookright || lookleft)
                        FireGun();
                }
            }
        }
        
        public void FireGun() {
            Vector2 muzzle = new Vector2(Position.X + muzzlePosition.X, Position.Y + muzzlePosition.Y);
            double x = Level.Player.Center.X -  muzzle.X;
            double y = Level.Player.Center.Y - muzzle.Y;
            double angle = Math.Atan2(y, x);
            gun.Fire(new Bullet(Level.Content), angle, muzzle);//TODO
            gun.Delay *= 5;
        }

        public override void SetBounds()
        {



            int width = (int)(base.IdleAnimation.FrameWidth * .75);//TODO: why idleAnimation? we might be loading a different animation
            int left = 4;
            int height = (int)(base.IdleAnimation.FrameWidth * .95);//half the height
            int top = base.IdleAnimation.FrameWidth - height;//moved down half a height

            LocalBounds = new Rectangle(left, top, width, height);
        }

    }

    }

