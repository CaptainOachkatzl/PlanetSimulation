using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlanetSimulation.CameraComponents;
using PlanetSimulation.EngineComponents;
using PlanetSimulation.PhysicHandler;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PlanetSimulation
{
    public class Universe : DrawableGameComponent
    {
        public PlanetSim Parent { get { return Game as PlanetSim; } }
        public GameTime CurrentGameTime { get { return Parent.CurrentGameTime; } }
        public List<Planet> Planets { get; set; }
        public int ID { get; private set; }
        public List<Planet> SelectedPlanets { get; private set; }
        public UniverseCamera Camera { get; private set; }
        private MultiProcessingUnit MultiProcessing { get; set; }
        public CollisionHandling CollisionHandler { get; private set; }
        public GravityHandling GravityHandler { get; private set; }
        private SpriteFont DataFont { get { return Parent.DataFont; } }

        public Universe (PlanetSim parent, int id) : base(parent)
        {
            Planets = new List<Planet>();
            SelectedPlanets = new List<Planet>();
            Camera = new UniverseCamera(Parent.GraphicsDevice.Viewport);
            GravityHandler = parent.GravityHandler;
            CollisionHandler = parent.CollisionHandler;
            MultiProcessing = parent.MultiProcessing;
            ID = id;

            Reset();
        }


        public Universe Copy(int newID)
        {
            Universe copy = new Universe(Parent, newID);
            copy.Camera.Position2D = this.Camera.Position2D;
            copy.Camera.Zoom = this.Camera.Zoom;

            foreach (Planet planet in Planets)
            {
                Planet planetCopy = planet.Copy(copy);

                copy.Planets.Add(planetCopy);

                if (IsPlanetSelected(planet))
                    copy.SelectedPlanets.Add(planetCopy);

                if (planet == Camera.FocusPlanet)
                    copy.FocusPlanet(planetCopy);
            }

            return copy;
        }

        public void UpdateUniverse()
        {
            MultiProcessing.CalculatePlanetMovement(Planets, CurrentGameTime);

            foreach (Planet planet in Planets)
            {
                planet.ApplyAcceleration();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Parent.SpriteBatch.Begin(SpriteSortMode.FrontToBack,
            BlendState.AlphaBlend,
            null,
            null,
            null,
            null,
            Camera.GetTranslationMatrix());

            foreach (Planet planet in Planets)
            {
                planet.Draw(gameTime, Parent.SpriteBatch, false /*planet == Camera.FocusPlanet*/);
            }
            Parent.SpriteBatch.End();

            Parent.SpriteBatch.Begin();
            foreach (Planet selectedPlanet in SelectedPlanets)
            {
                DrawPlanetData(selectedPlanet);
            }
        }

        private void DrawPlanetData(Planet planet)
        {
            Vector2 topLeft = planet.Position;
            topLeft.X += planet.Radius + (GameGlobals.PlanetDataListInspacer / Camera.Zoom);
            topLeft.Y -= planet.Radius;
            topLeft = Camera.CalculateAbsolute2DPosition(topLeft);

            Parent.SpriteBatch.DrawString(DataFont, planet.DataString, topLeft, Color.White);
        }

        public int GetPhysicalCoreCount()
        {
            return MultiProcessing.PhysicalCoreCount;
        }

        public int GetLogicalCoreCount()
        {
            return MultiProcessing.LogicalCoreCount;
        }

        public int UsedProcessorCoreCount()
        {
            return Parent.MultiProcessing.UsedCores;
        }

        public void AddPlanet(Planet planet)
        {
            if (planet != null)
            {
                planet.LoadContent(Parent.Content);
                Planets.Add(planet);
            }
        }

        public void ClearPlanets()
        {
            for (int i = Planets.Count - 1; i > -1; i--)
            {
                RemovePlanet(Planets[i]);
            }

            Camera.ClearFocus();
        }

        public void RemovePlanet(Planet planet)
        {
            if (planet != null)
            {
                if (SelectedPlanets.Contains(planet))
                    SelectedPlanets.Remove(planet);

                if (planet == Camera.FocusPlanet)
                    Camera.ClearFocus();

                Planets.Remove(planet);
                planet.Dispose();
            }
        }

        public bool IsPlanetCollidingWithAnyOtherPlanet(Planet planet)
        {
            foreach (Planet otherPlanet in Planets)
            {
                if (planet == otherPlanet)
                    continue;

                if (CollisionHandler.IsColliding(planet, otherPlanet))
                    return true;
            }

            return false;
        }
        
        public void TogglePlanetSelection(Planet planet)
        {
            if (IsPlanetSelected(planet))
                UnselectPlanet(planet);
            else
                SelectPlanet(planet);
        }

        public void SelectPlanet(Planet planet)
        {
            if (!IsPlanetSelected(planet))
                SelectedPlanets.Add(planet);
        }

        public void UnselectPlanet(Planet planet)
        {
            SelectedPlanets.Remove(planet);
        }

        public bool IsPlanetSelected(Planet planet)
        {
            return SelectedPlanets.Contains(planet);
        }

        public void RemovePlanetAtPosition(Vector2 position)
        {
            Planet planetAtPosition = GetPlanetAtPosition(position);

            if(planetAtPosition != null)
                RemovePlanet(planetAtPosition);
        }

        public Planet GetPlanetAtPosition(Vector2 position)
        {
            foreach (Planet planet in Planets)
            {
                if (planet.IsPointInPlanet(position))
                    return planet;
            }

            return null;
        }

        public void FocusPlanet(Planet planet)
        {
            Camera.FocusPlanet = planet;
        }

        public void StopFocusPlanet()
        {
            Camera.ClearFocus();
        }

        public void Reset()
        {
            ClearPlanets();
            Camera.ResetCamera();
            Camera.Zoom = 0.3F;
        }

        public void CreateRandomField()
        {
            Random randomizer = new Random();
            for (int i = 0; i < 50; i++)
            {
                Vector2 position = new Vector2(Camera.Position.X + randomizer.Next(-1000, 1000), Camera.Position.Y + randomizer.Next(-1000, 1000));

                Vector2 direction = Vector2.Zero; // new Vector2(randomizer.Next(-100, 100) / 100, randomizer.Next(-100, 100) / 100);

                Planet newPlanet = new Planet(this);
                newPlanet.Position = position;
                newPlanet.Direction = direction;
                newPlanet.Radius = randomizer.Next(2, 10);
                newPlanet.Density = GameGlobals.GenericDensity;
                AddPlanet(newPlanet);
            }
        }
    }
}
