using Microsoft.Xna.Framework;

namespace PlanetSimulation.PhysicHandler
{
    public class GravityHandling
    {
        const float GravityConstant = 6.67408E-11F;

        const float GravityConstantKilometerTonns = GravityConstant / 1000000;

        public void CalculateGravity(Planet planet1, Planet planet2, GameTime gameTime)
        {
            Vector2 distanceVector = Measurements.GetDistanceVector(planet1, planet2);

            float distanceSqared = distanceVector.LengthSquared();

            if (distanceSqared == 0)
                return;

            float acceleration = GravityConstantKilometerTonns * GameGlobals.SimulationSpeedMuliplicator / distanceSqared * (float)gameTime.ElapsedGameTime.TotalSeconds;

            distanceVector.Normalize();
            distanceVector *= acceleration;

            planet1.Direction += distanceVector * planet2.Mass;
            planet2.Direction -= distanceVector * planet1.Mass;
        }
    }
}
