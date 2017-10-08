 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace ImagineCup
{
    class SpreadGun : Gun
    {
        public SpreadGun(Alignment alignment, Level level)
            : base(alignment, false, 1000, level, level.Content.Load<SoundEffect>("Sounds/Spreadshot"),5,100.0F)
        {
            //handled by gun constructor
            base.Energy = 100;
            base.DisplaySprite = level.Content.Load<Texture2D>("Sprites/Powerups/Hud/SpreadShotHUD");//TODO: add these to the default constructors' list of shit, they will be loaded for every gun
            base.PowerupSound = "Sounds/SpreadShotPowerup";
            base.PowerupTexture = "Sprites/Powerups/Guns/SpreadShot";
        }

        override public void Fire(Bullet bullet, Double direction, Vector2 muzzle)
        {
            Delay = delayTime;
            base.shootSound.Play(volume, 0.0f, 0.0f);
            double thirty = Math.PI / 6;
            double fifteen = Math.PI / 12;
            Energy -= bullet.EnergyCost * EnergyCostModifier;
            bullets.Add((bullet.clone(new Vector2((float)Math.Cos(direction - thirty) * bullet.Speed, (float)Math.Sin(direction - thirty) * bullet.Speed), muzzle)));
            bullets.Add((bullet.clone(new Vector2((float)Math.Cos(direction - fifteen) * bullet.Speed, (float)Math.Sin(direction - fifteen) * bullet.Speed), muzzle)));
            bullets.Add((bullet.clone(new Vector2((float)Math.Cos(direction) * bullet.Speed, (float)Math.Sin(direction) * bullet.Speed), muzzle)));
            bullets.Add((bullet.clone(new Vector2((float)Math.Cos(direction + fifteen) * bullet.Speed, (float)Math.Sin(direction + fifteen) * bullet.Speed), muzzle)));
            bullets.Add((bullet.clone(new Vector2((float)Math.Cos(direction + thirty) * bullet.Speed, (float)Math.Sin(direction + thirty) * bullet.Speed), muzzle)));
        }
    }
}