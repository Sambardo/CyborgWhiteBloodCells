using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace ImagineCup
{
     class ProgressBar 
    {
        public Vector2 Position { get { return position; } set { position = value; } }private Vector2 position;
        public Texture2D BarImg { get { return barimg; } set { barimg = value; } }private Texture2D barimg;
        public Texture2D BorderImg { get { return borderimg; } set { borderimg = value; } }private Texture2D borderimg;
        public Texture2D EndBorderImg { get { return endborderimg; } set { endborderimg = value; } }private Texture2D endborderimg;
        public Color FullColor { get { return fullcolor; } set { fullcolor = value; } }private Color fullcolor;
        public Color EmptyColor { get { return emptycolor; } set { emptycolor = value; } }private Color emptycolor;
        public int MaxLength { get{return maxlength;} set {maxlength = value;} } private int maxlength;//really? reserving maxlength and maxlen? and maximumlen?

        
        
        private const int BARHEIGHT = 25;
        private const int BORDERTHICKNESS = (int)(.20 * BARHEIGHT);
        private const int BORDERHEIGHT = BARHEIGHT + 2 * BORDERTHICKNESS;
        private const int BORDERWIDTH = BORDERTHICKNESS;

        public ProgressBar(Level level, Vector2 position, int maxlen,String barimglocation, String borderimglocation, String endborderimglocation, Color full, Color empty) {
            barimg = level.Content.Load<Texture2D>(barimglocation);
            borderimg = level.Content.Load<Texture2D>(borderimglocation);
            endborderimg = level.Content.Load<Texture2D>(endborderimglocation);
            this.fullcolor = full;
            this.emptycolor = empty;
            this.position = position;
            maxlength = maxlen;
        }



         public ProgressBar (Level level, Vector2 position, int maxlen) : this (level, position, maxlen, "bg","bg", "bg", Color.Purple,Color.Blue)//
        {

           
        }

         


        public void Draw(SpriteBatch spritebatch, float percent) 
        {

            

            int length = (int)(percent*maxlength);

            Rectangle firstborderRectangle = new Rectangle((int)(position.X - BORDERWIDTH), (int)(position.Y - BORDERTHICKNESS), BORDERWIDTH, BORDERHEIGHT);

            spritebatch.Draw(endborderimg, firstborderRectangle,Color.White);


            Rectangle topborderstretch = new Rectangle((int)(position.X), (int)(position.Y - BORDERTHICKNESS),MaxLength,BORDERTHICKNESS);

            spritebatch.Draw(borderimg, topborderstretch, Color.White);

            Rectangle bottomborderstretch = new Rectangle((int)(position.X), (int)(position.Y + BARHEIGHT), maxlength, BORDERTHICKNESS);

            spritebatch.Draw(borderimg, bottomborderstretch, Color.White);


            Rectangle endborderRectangle = new Rectangle((int)(position.X + maxlength), (int)(position.Y - BORDERTHICKNESS), BORDERWIDTH, BORDERHEIGHT);
      
            spritebatch.Draw(endborderimg, endborderRectangle, Color.White);



            Rectangle bar = new Rectangle((int)position.X, (int)position.Y, length, BARHEIGHT);

            Color barcolor = new Color((float)(fullcolor.R/255.0 * percent +(emptycolor.R/255.0 * (1.0-percent))),(float)(fullcolor.G/255.0 * percent + (emptycolor.G/255.0 * (1.0-percent))),(float)(fullcolor.B/255.0 * percent + (emptycolor.B/255.0 * (1.0-percent))));
           

            spritebatch.Draw(barimg, bar, barcolor);
        
        }

    }
}