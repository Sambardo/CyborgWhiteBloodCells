using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace ImagineCup
{
   class Bullet
    {
        public Texture2D Sprite {
            get { return sprite; }
            set { sprite = value; }

        }

        public String Name {
            get { return name;}
            set { name = value;}
        }private String name;

        public String PowerupTexture
        {
            get { return poweruptexture; }
            set { poweruptexture = value; }

        }private String poweruptexture;


        public String PowerupSound
        {
            get { return powerupsound; }
            set { powerupsound = value; }

        }private String powerupsound;


        private Texture2D sprite;
        public Vector2 position;
        public float rotation;
        public Vector2 center;
        public Vector2 velocity;
        public bool isAlive;
        private SpriteEffects flip = SpriteEffects.None;

        public int Damage
        {
            get { return damage; }
        }
        private int damage;

        public int Speed
        {
            get { return speed; }
            set { speed = value; }
        }
        private int speed;

        public Color Color
        {
            get { return color; }
        }
        private Color color;

        public float EnergyCost
        {
            get { return energycost; }

        }private float energycost;


        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(position.X - center.X);
                int top = (int)Math.Round(position.Y - center.Y);

                return new Rectangle(left, top, sprite.Width, sprite.Height);
            }
        }


        protected Bullet(Texture2D loadedTexture, int damage, int speed, Color color, float cost, String poweruptexture, String powerupsound, String name)
        {
            this.damage = damage;
            this.speed = speed;
            rotation = 0.0f;
            position = Vector2.Zero;
            sprite = loadedTexture;
            center = new Vector2(sprite.Width / 2, sprite.Height / 2);
            velocity = Vector2.Zero;
            isAlive = true;
            this.color = color;
            this.energycost = cost;
            this.powerupsound = powerupsound;
            this.poweruptexture = poweruptexture;
            this.name = name;
        }

        /*public Bullet(Texture2D loadedTexture, int damage, int speed) : this(loadedTexture, damage, speed, Color.White)
        { 
        }*/


       public Bullet(ContentManager content)//for spawning a normal bullet, with base characteristics
            : this(content.Load<Texture2D>("Sprites\\bullet"), 10, 10, Color.AliceBlue, 1, "Sprites\\bullet","Sounds\\Shoot", "Aspirin")
        {
        }



        public Bullet(Bullet bullet) : this(bullet.sprite, bullet.damage, bullet.speed, bullet.Color,bullet.EnergyCost, bullet.PowerupTexture,bullet.PowerupSound, bullet.Name)
        {
        }

        public virtual Bullet clone(Vector2 velocity, Vector2 position)
        {
            Bullet bullet = new Bullet(this);
            bullet.velocity = velocity;
            bullet.position = position;
            return bullet;
            
        }

        virtual public void Update()
        {
            position += velocity;
            rotation = VectorToAngle(velocity);

        }

        float VectorToAngle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }



        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            if (isAlive)
                while (rotation < 0) rotation += 2 * (float)Math.PI;
              // Flip the sprite to face the way we are moving.
            if (rotation > Math.PI / 2 && rotation < Math.PI * 1.5)
                flip = SpriteEffects.FlipVertically;

            else
                flip = SpriteEffects.None;
                 
             //spriteBatch.Draw(sprite, position, null, color, 0, center, 1, flip, 1);
             spriteBatch.Draw(sprite, new Rectangle((int)position.X, (int)position.Y, sprite.Width, sprite.Height), new Rectangle(0, 0, sprite.Width, sprite.Height), Color.White, rotation, center, flip, 0);
             //Console.WriteLine(rotation);
        }
    }
}
