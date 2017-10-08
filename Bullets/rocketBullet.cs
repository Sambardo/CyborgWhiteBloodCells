using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace ImagineCup
{
    class RocketBullet : Bullet
    {

        public RocketBullet(ContentManager content)
            : base(content.Load<Texture2D>("Sprites\\Bullets\\rocketBullet"), 25, 10, Color.White, 5, "Sprites\\Bullets\\rocketBullet", "Sounds\\Shoot", "Big Bertha")//placeholder for now, customize later
        {
            
        }


    }
}
