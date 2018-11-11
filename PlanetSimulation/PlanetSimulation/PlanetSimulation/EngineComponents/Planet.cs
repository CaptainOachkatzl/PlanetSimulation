using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace PlanetSimulation
{
    public class Planet : IDisposable
    {
        #region Properties

        Universe ParentUniverse { get; set; }

        float m_mass;
        public float Mass
        {
            get { return m_mass; }
            set
            {
                m_mass = value;
                PlanetColor = CalculateColor();
            }
        }
        public float Density
        {
            get
            {
                if (Volume <= 0)
                    return 0;

                return Mass / Volume;
            }
            set { Mass = Volume * value; }
        }

        public Vector2 Position { get; set; }

        private Vector2 m_direction;
        public Vector2 Direction
        {
            get { return m_direction; }
            set
            {
                m_direction = value;
                DirectionNextFrameNeedsUpdate = true;
            }
        }

        private Vector2 m_directionNextFrame;
        public Vector2 DirectionNextFrame
        {
            get
            {
                if (DirectionNextFrameNeedsUpdate)
                {
                    m_directionNextFrame = Direction * GameGlobals.SimulationSpeedMuliplicator * (float)ParentUniverse.CurrentGameTime.ElapsedGameTime.TotalSeconds;
                    DirectionNextFrameNeedsUpdate = false;
                }

                return m_directionNextFrame;
            }
        }


        public bool DirectionNextFrameNeedsUpdate { get; set; }

        private float m_radius;
        public float Radius
        {
            get { return m_radius; }
            set
            {
                m_radius = value;
                m_volume = 4F / 3F * (float)Math.PI * (float)Math.Pow(value, 3);
            }
        }

        private float m_volume;
        public float Volume { get { return m_volume; } }

        public float BouncingFactor { get; set; }
        public Texture2D SpritePlanet { get; private set; }
        public Texture2D SpriteSelection { get; private set; }
        private Color PlanetColor { get; set; }
        public bool Selected
        {
            get { return ParentUniverse.IsPlanetSelected(this); }
            set
            {
                if (value)
                    ParentUniverse.SelectPlanet(this);
                else
                    ParentUniverse.UnselectPlanet(this);
            }
        }

        public int PosXint
        {
            get { return (int)(Position.X + 0.5); }
        }
        public int PosYint
        {
            get { return (int)(Position.Y + 0.5); }
        }

        public string DataString { get { return CreateStringFromData(); } }
        #endregion

        public Planet(Universe universe)
        {
            ParentUniverse = universe;
            Radius = 10;
            Mass = 1;
            BouncingFactor = 1;
            Position = Vector2.Zero;
            Direction = Vector2.Zero;
            Selected = false;
            DirectionNextFrameNeedsUpdate = true;
        }

        public Planet Copy(Universe universe)
        {
            Planet copy = new Planet(universe);

            copy.Position = this.Position;
            copy.Direction = this.Direction;
            copy.Radius = this.Radius;
            copy.Mass = this.Mass;
            copy.BouncingFactor = this.BouncingFactor;
            copy.Selected = this.Selected;
            copy.SpritePlanet = this.SpritePlanet;
            copy.SpriteSelection = this.SpriteSelection;

            return copy;
        }

        public void Dispose()
        {
            //SafeDisposeResource(Sprite);
        }

        private void SafeDisposeResource(IDisposable disposable)
        {
            if (disposable != null)
                disposable.Dispose();

            disposable = null;
        }

        public void LoadContent(ContentManager content)
        {
            SpritePlanet = content.Load<Texture2D>("PlanetHighRes");
            SpriteSelection = content.Load<Texture2D>("PlanetSelection");
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, bool focused)
        {
            Rectangle targetRect = new Rectangle(Generics.RoundFloat(Position.X - Radius), Generics.RoundFloat(Position.Y - Radius), Generics.RoundFloat(Radius * 2), Generics.RoundFloat(Radius * 2));

            spriteBatch.Draw(SpritePlanet, targetRect, null, PlanetColor, 0, Vector2.Zero, SpriteEffects.None, 0);

            if (focused)
                spriteBatch.Draw(SpriteSelection, targetRect, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);
        }

        private Color CalculateColor()
        {
            int factor = 15000;

            if (Mass <= 0)
                return new Color(255, 255, 255);

            int red = 255;
            int green = 255;
            int blue = 255;

            int factorIndex = 1;
            if(Mass < Math.Pow(factor, factorIndex))
            {
                green = 255 - CalculateSingleColorValue(factor,factorIndex);
                blue = 255 - CalculateSingleColorValue(factor, factorIndex);

                return new Color(red, green, blue);
            }

            factorIndex++;
            if (Mass < Math.Pow(factor, factorIndex))
            {
                green = CalculateSingleColorValue(factor, factorIndex);
                blue = 0;

                return new Color(red, green, blue);
            }

            factorIndex++;
            if (Mass < Math.Pow(factor, factorIndex))
            {
                blue = 0;
                red = 255 - CalculateSingleColorValue(factor, factorIndex);

                return new Color(red, green, blue);
            }

            factorIndex++;
            if (Mass < Math.Pow(factor, factorIndex))
            {
                blue = CalculateSingleColorValue(factor, factorIndex);
                red = 0;

                return new Color(red, green, blue);
            }

            factorIndex++;
            if (Mass < Math.Pow(factor, factorIndex))
            {
                green = 255 - CalculateSingleColorValue(factor, factorIndex);
                red = 0;

                return new Color(red, green, blue);
            }

            factorIndex++;
            double test = Math.Pow(factor, factorIndex);
            if (Mass < Math.Pow(factor, factorIndex))
            {
                blue = 255 - CalculateSingleColorValue(factor, factorIndex);
                red = 0;
                green = 0;

                return new Color(red, green, blue);
            }

            return Color.Black;
        }

        private int CalculateSingleColorValue(double factor, int power)
        {
            double prefactor = Math.Pow(factor, power - 1);

            double difference = Mass - prefactor;
            double relativeValue = difference / prefactor;

            return (int)(relativeValue / factor * 0xFF);
        }

        public void ApplyAcceleration()
        {
            Position += DirectionNextFrame;
        }

        public bool IsPointInPlanet(Vector2 point)
        {
            return (point - Position).LengthSquared() < Radius * Radius;
        }

        private string CreateStringFromData()
        {
            string speed = "Speed: " + Generics.CutDecimals(Direction.Length(), 3) + " " + Units.Speed;
            string mass = "Mass: " + Generics.CutDecimals(Mass, 2) + " " + Units.Weight;
            string radius = "Radius: " + Generics.CutDecimals(Radius, 2) + " " + Units.Length;
            string volume = "Volume: " + Generics.CutDecimals(Volume, 2) + " " + Units.Volume;
            string density = "Density: " + Generics.CutDecimals(Density, 2) + " " + Units.Density;

            return speed + "\n" + mass + "\n" + radius + "\n" + volume + "\n" + density;
        }
    }
}