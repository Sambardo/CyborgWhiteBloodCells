using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace ImagineCup
{
    class FlyingSpawner : Event
    {
        public bool Happened
        {
        get {return happened;}
        }bool happened;

        public FlyingSpawner(Level level, Vector2 position) : base(level,position){
        happened = false;
        }

        public override void  Update()
        {
           if (this.Position.X < Level.Player.Position.X + 500&& !happened){
               Level.QueueEnemyChange(new FlyingEnemy(Level,Position));
               happened = true;
           }
        }


    }
}
