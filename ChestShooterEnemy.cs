using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace ImagineCup
{
    class ChestShooterEnemy : ShootingEnemy
    {
        public ChestShooterEnemy(Level level, Vector2 Position, Gun gu)
            : base(level, Position, "ChestShooter", gu) { 
        }
    }
}
