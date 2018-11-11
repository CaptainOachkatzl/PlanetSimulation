using Microsoft.Xna.Framework;

namespace PlanetSimulation
{
    class Measurements
    {
        static public Vector2 GetDistanceVector(Planet planet1, Planet planet2)
        {
            return planet2.Position - planet1.Position;
        }
    }
}
