using Microsoft.Xna.Framework;

namespace PlanetSimulation.PhysicHandler
{
    public class CollisionHandling
    {
        public void CalculateCollision(Planet planet1, Planet planet2, GameTime gameTime)
        {
            CalculateCollision(planet1, planet2);
        }

        public void CalculateCollision(Planet planet1, Planet planet2)
        {
            Vector2 distanceVectorToOtherPlanet;

            if (!IsColliding(planet1, planet2, out distanceVectorToOtherPlanet))
                return;

            CalculateCollisionReaction(planet1, planet2, distanceVectorToOtherPlanet);
        }

        public bool IsColliding(Planet planet1, Planet planet2, out Vector2 distanceVector)
        {
            distanceVector = Measurements.GetDistanceVector(planet1, planet2);
            float distance = distanceVector.Length();

            Vector2 coveredDistance = planet2.DirectionNextFrame - planet1.DirectionNextFrame;
            float skalar = Vector2.Dot(coveredDistance, distanceVector);
            float movementDistance = skalar / distance;

            float totalDistance = distance + movementDistance;

            return planet1.Radius + planet2.Radius >= totalDistance;
        }

        public bool IsColliding(Planet planet1, Planet planet2)
        {
            Vector2 dummy = new Vector2();
            return IsColliding(planet1, planet2, out dummy);
        }

        private void CalculateCollisionReaction(Planet planet1, Planet planet2, Vector2 distanceVector)
        {
            // Source http://www.gamasutra.com/view/feature/131424/pool_hall_lessons_fast_accurate_.php?page=3

            // First, find the normalized vector n from the center of 
            // circle1 to the center of circle2
            distanceVector.Normalize();
            // Find the length of the component of each of the movement
            // vectors along n. 
            // a1 = v1 . n
            // a2 = v2 . n
            float acceleration1 = Vector2.Dot(planet1.Direction, distanceVector); // v1.dot(n);
            float acceleration2 = Vector2.Dot(planet2.Direction, distanceVector);

            // Using the optimized version, 
            // optimizedP =  2(a1 - a2)
            //              -----------
            //                m1 + m2
            Vector2 optimizedP = (2.0F * (acceleration1 - acceleration2)) / (planet1.Mass + planet2.Mass) * distanceVector;

            // v1' = v1 - optimizedP * m2 * n
            planet1.Direction -= optimizedP * planet2.Mass;

            // v2' = v2 + optimizedP * m1 * n
            planet2.Direction += optimizedP * planet1.Mass;
        }

        private float CalculateVelocityFactor(Planet planet1, Planet planet2)
        {
            return 2 * ((planet1.Mass * planet1.Direction.Length() + planet2.Mass * planet2.Direction.Length()) / (planet1.Mass + planet2.Mass));
        }

        private Vector2 CalculateAfterCollisionDirection(Vector2 direction, Vector2 normalizedDistance, float velocity)
        {
            direction = Vector2.Reflect(direction, normalizedDistance);
            direction.Normalize();
            return direction *= velocity;
        }

        private float CalculateEnergySum(Planet planet1, Planet planet2)
        {
            return CalculateCurrentEnergy(planet1.Mass, planet1.Direction.Length()) + CalculateCurrentEnergy(planet2.Mass, planet2.Direction.Length());
        }

        private float CalculateCurrentEnergy(float velocity, float mass)
        {
            return velocity * mass;
        }
    }
}
