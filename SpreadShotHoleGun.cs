using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace ImagineCup
{
    class SpreadShotHoleGun : Gun
    {
        public SpreadShotHoleGun(Alignment alignment, Level level)
            : base(alignment, false, 2000, level, level.Content.Load<SoundEffect>("Sounds/Spreadshot"),4,4000)//this is a boss weapon; maybe add disable energy?
        {
            Energy = 10;
            //handled by gun constructor
        }

    


    override public void Fire(Bullet bullet, Double direction, Vector2 muzzle)
        {
            Delay = delayTime;
            base.shootSound.Play(volume, 0.0f, 0.0f);

            int spacing = 10;
            int number_bullets = 6;

            Random rand = new Random();
            int rando = rand.Next(4) - (number_bullets/2);
            for (int i = -number_bullets/2 * spacing; i < number_bullets/2 * spacing; i += spacing)
            {
                if (i != rando * spacing)bullets.Add(bullet.clone(new Vector2((float)Math.Cos(direction - MathHelper.ToRadians(i)) * bullet.Speed, (float)Math.Sin(direction - MathHelper.ToRadians( i)) * bullet.Speed), muzzle + new Vector2(0,i/spacing*5)));
            }
            }
    }
}
