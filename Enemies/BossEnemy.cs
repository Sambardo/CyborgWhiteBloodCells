using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace ImagineCup
{
    class BossEnemy : ShootingEnemy
    {

        public BossEnemy(Level level, Vector2 Position)
            : base(level, Position, "ChestShooterBoss", new SpreadShotHoleGun(Alignment.enemy, level))
        {
            base.CollisionDamage = 100;
            Health = 250;
            Score = 150;
            MuzzlePosition = Position - new Vector2(100,120);//TODO: change
        }

        


        public override void FireGun()
        {

            Double angle = (int)Direction * Math.PI;
            Gun.Fire(new BigBullet(Level.Content), angle, MuzzlePosition);//TODO: change normal bllet

        }
        public override void SetBounds()
        {



            int width = (int)(base.IdleAnimation.FrameWidth * .75);//TODO: why idleAnimation? we might be loading a different animation
            int left = 40;
            int height = (int)(base.IdleAnimation.FrameWidth * .95);//half the height
            int top = base.IdleAnimation.FrameWidth - height;//moved down half a height

            LocalBounds = new Rectangle(left, top, width, height);
        }
    }
}
