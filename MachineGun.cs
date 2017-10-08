using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace ImagineCup
{
    class MachineGun : Gun
    {
        public MachineGun(Alignment alignment, Level level)
            : base(alignment, true, 200, level, level.Content.Load<SoundEffect>("Sounds/Shoot"),1,50)
        {
            //handled by gun constructor
            Energy = 50.00F;
            base.DisplaySprite = level.Content.Load<Texture2D>("Sprites/Powerups/Hud/MachineGunHUD");
            base.PowerupSound = "Sounds/MachineGunPowerup";
            base.PowerupTexture = "Sprites/Powerups/Guns/MachineGun";
        }

        override public void Fire(Bullet bullet, Double direction, Vector2 muzzle)
        {
            Delay = delayTime;
            Energy -= bullet.EnergyCost;
            base.shootSound.Play(volume, 0.0f, 0.0f);
            bullets.Add((bullet.clone(new Vector2((float)Math.Cos(direction) * bullet.Speed, (float)Math.Sin(direction) * bullet.Speed), muzzle)));
        }
    }
}
