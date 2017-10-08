using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace ImagineCup
{
    class Event
    {
        public bool Draws {
            get { return draws; }
            set { draws = value; }
        }bool draws;
        public Vector2 Position {
            get { return position; }
            set { position = value; }
        } Vector2 position;

        public bool IsAlive//this name exists outside of the class; is a method
        {
            get { return isAlive; }//gets the var inside the class; note uncapped
            set { isAlive = value; }
        }
        bool isAlive;//this is the variable we're referring to

        public Level Level
        {
            get { return level; }
            set { level = value; }
        }
        Level level;

        public virtual void Update() { 
        
        
        }

        public Event(Level level, Vector2 position) {
            this.level = level;
            this.position = position;
            this.isAlive = true;
            this.draws = false;
        }
    }
}
