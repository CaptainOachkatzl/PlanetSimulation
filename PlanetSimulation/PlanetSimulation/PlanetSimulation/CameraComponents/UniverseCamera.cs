using Microsoft.Xna.Framework.Graphics;
using RenderingFunctionality.Camera;
using Microsoft.Xna.Framework;

namespace PlanetSimulation.CameraComponents
{
    public class UniverseCamera : Camera2D
    {
        public Planet FocusPlanet { get; set; }
        public bool IsFocusing { get { return FocusPlanet != null; } }

        public override Vector3 Position
        {
            get
            {
                if (FocusPlanet != null)
                    return new Vector3(FocusPlanet.Position.X, FocusPlanet.Position.Y, 0);

                return base.Position;
            }
        }

        public UniverseCamera(Viewport viewport) : base(viewport)
        {
            ClearFocus();
        }

        public void ClearFocus()
        {
            if (FocusPlanet != null)
            {
                Position2D = FocusPlanet.Position;
                FocusPlanet = null;
            }
        }
    }
}
