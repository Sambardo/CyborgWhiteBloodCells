using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace ImagineCup
{
    class ShootingEnemy : Enemy
    {
        public Vector2 MuzzlePosition{
            get { return muzzlePosition; }
            set { muzzlePosition = value; }
        }Vector2 muzzlePosition;
        public Gun Gun
        {
            get { return gun; }
            set { gun = value; }
        }Gun gun;


        public ShootingEnemy(Level level, Vector2 Position, String SpriteSet, Gun gu)
            : base(level, Position, SpriteSet)
        {
            Gun = gu;
            Health = 10;
            base.CollisionDamage = 20;
            muzzlePosition = Position;//TODO: change
        }

        public override void Update(GameTime gametime)
        {
            if(Level.isOnScreen(MuzzlePosition)){
                gun.Delay -= gametime.ElapsedGameTime.Milliseconds;
                if(gun.Delay <= 0 && IsAlive)
                    FireGun();
            }
        }
        
        public virtual void FireGun() { 
            double x = Level.Player.Center.X - (this.Position.X + muzzlePosition.X);
            double y = Level.Player.Center.Y - (this.Position.Y + muzzlePosition.Y );
            double angle = Math.Atan2(y, x);
            gun.Fire(new Bullet(Level.Content), angle, muzzlePosition);
            gun.Delay *= 5;
        }

    }
}