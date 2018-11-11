using InputFunctionality.KeyboardAdapter;
using InputFunctionality.MouseAdapter;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlanetSimulation.CameraComponents;

namespace PlanetSimulation.Interface
{
    public class CameraControl
    {
        private PlanetSim Parent { get; set; }
        public UniverseCamera Camera { get; set; }
        MouseAdvanced InputMouse { get { return Parent.InputMouse; } }
        KeyboardAdvanced InputKeyboard { get { return Parent.InputKeyboard; } }
        Viewport Viewport { get { return Camera.Viewport; } }
        public Matrix TranslationMatrix { get { return Camera.GetTranslationMatrix(); } }

        public CameraControl(PlanetSim parent)
        {
            Parent = parent;
        }

        public void Update()
        {
            if (Camera == null)
                return;

            CameraAdjustingChecks();
        }

        private void CameraAdjustingChecks()
        {
            CheckCameraZoom();

            CheckCameraPosition();
        }

        private void CheckCameraZoom()
        {
            int wheelValue = InputMouse.MouseWheelValue;
            if (wheelValue != 0)
            {
                Camera.Zoom *= 1 + (float)InputMouse.MouseWheelValue / 120 / 10;

                if (Camera.Zoom <= GameGlobals.MinZoom)
                    Camera.Zoom = GameGlobals.MinZoom;

                if (Camera.Zoom > GameGlobals.MaxZoom)
                    Camera.Zoom = GameGlobals.MaxZoom;
            }
        }

        private void CheckCameraPosition()
        {
            float speed = (float)Parent.CurrentGameTime.ElapsedGameTime.TotalMilliseconds / Camera.Zoom;

            if (InputKeyboard.KeyCurrentlyPressed(Keys.A))
                ApplyCameraChanges(new Vector2(-speed, 0));

            if (InputKeyboard.KeyCurrentlyPressed(Keys.D))
                ApplyCameraChanges(new Vector2(speed, 0));

            if (InputKeyboard.KeyCurrentlyPressed(Keys.W))
                ApplyCameraChanges(new Vector2(0, -speed));

            if (InputKeyboard.KeyCurrentlyPressed(Keys.S))
                ApplyCameraChanges(new Vector2(0, speed));
        }

        private void ApplyCameraChanges(Vector2 direction)
        {
            Camera.Position2D += direction;
            Camera.ClearFocus();
        }
    }
}
