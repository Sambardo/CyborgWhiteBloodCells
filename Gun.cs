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
    enum Alignment
    {
        player = 0,
        enemy = 1,
    }

    class Gun
    {
        protected List<Bullet> bullets;
        private Level level;

        public String PowerupSound {
            get { return powerupsound; }
            set { powerupsound = value; }

        }private String powerupsound;

        public String PowerupTexture {
            get { return poweruptexture; }
            set { poweruptexture = value; }

        }private String poweruptexture;


        protected SoundEffect shootSound;
        protected float volume; //makes enemy bullets quiet

        public Texture2D DisplaySprite {
            get { return displaysprite; }
            set { displaysprite = value; }

        }private Texture2D displaysprite;

        public float Energy
        {
            get { return energy; }

            set { energy = value; }
        }
        private float energy;


        public float MaxEnergy
        {
            get { return maxenergy; }

            set { maxenergy = value; }
        }
        private float maxenergy;


        public float EnergyCostModifier
        {
            get { return energycostmodifier; }

            set { energycostmodifier = value; }
        }


        private float energycostmodifier;

        public bool IsAutomatic
        {
            get { return isAutomatic; }

            set{ isAutomatic = value;}
        }
        private bool isAutomatic;

        public int Delay
        {
            get { return delay; }

            set{ delay = value;}
        }
        private int delay;

        protected int delayTime; //amount of time enforced between shots.

        private Alignment alignment;

        protected Gun(Alignment alignment, bool auto, int delayTime,  Level level, SoundEffect shootSound, float ecost, float maxenergy)
        {
            this.level = level;

           isAutomatic = auto;
           this.delayTime = delayTime; //the standard gun should be called with 0 delay time
           this.delay = 0;
           energy = 500; //this standard gun as infinite ammo

           displaysprite = level.Content.Load<Texture2D>("Sprites/Powerups/Hud/GunHUD");

           this.shootSound = shootSound;

           this.alignment = alignment;
           if(alignment == Alignment.player){
                this.bullets = level.PlayerBullets;
                volume = 1.0f;
            }
            else{
                this.bullets = level.EnemyBullets;
                volume = 0.5f;
            }
           energycostmodifier = ecost;
           this.maxenergy = maxenergy;
        }

        public Gun(Level level)//TODO: should use this
            :this (Alignment.player, false, 0, level, level.Content.Load<SoundEffect>("Sounds/Shoot"),1,1000)
        { 
        }

        

        


        public void ReloadContent(Level level)
        {
            this.level = level;
            if (alignment == Alignment.player)
                this.bullets = level.PlayerBullets;
            else
                this.bullets = level.EnemyBullets;
        }

        //<summary>
        //Fires the gun with the provided bullet type.
        // bullet - an instance ofi the bullet you want the gun to fire
        // direction - angle in radians for bullet directioon
        // muzzle - position of the muzzle of your sprites gun.
        // inherited functions must implement this and use ammo/delay time when needed.
        //</summary>
        virtual public void Fire(Bullet bullet, Double direction, Vector2 muzzle)
        {
            if (bullet.GetType() != typeof(Bullet)) {
                energy -= energycostmodifier * bullet.EnergyCost;
            }
            shootSound.Play(volume,0.0f,0.0f);
            bullets.Add((bullet.clone(new Vector2((float)(Math.Cos(direction) * bullet.Speed), (float)(Math.Sin(direction) * bullet.Speed)), muzzle)));
        }
    }
}
