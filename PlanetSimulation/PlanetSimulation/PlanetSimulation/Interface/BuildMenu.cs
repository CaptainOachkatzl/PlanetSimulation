using InputFunctionality.KeyboardAdapter;
using InputFunctionality.MouseAdapter;
using Microsoft.Xna.Framework;
using RenderingFunctionality.Camera;

namespace PlanetSimulation.Interface
{
    class BuildMenu
    {
        PlanetSim Parent { get; set; }
        
        Planet m_newPlanet;
        bool IsCreating { get { return m_newPlanet != null; } }

        Planet m_editPlanet;
        bool EditPlanetWasSelected { get; set; }
        bool IsEditing { get { return m_editPlanet != null; } }
        Vector2 m_editPlanetOldDirection;

        MouseAdvanced InputMouse { get { return Parent.InputMouse; } }
        KeyboardAdvanced InputKeyboard { get { return Parent.InputKeyboard; } }
        Camera Camera { get { return CurrentUniverse.Camera; } }

        Vector2 RelativeMousePosition { get; set; }

        Universe CurrentUniverse { get { return Parent.CurrentUniverse; } }

        bool ForcePlanetAtMouseUpdate { get; set; }
        Planet m_planetMousePos;
        Planet PlanetAtMousePosition
        {
            get
            {
                if (ForcePlanetAtMouseUpdate)
                {
                    m_planetMousePos = CurrentUniverse.GetPlanetAtPosition(RelativeMousePosition);
                    ForcePlanetAtMouseUpdate = false;
                }

                return m_planetMousePos;
            }
        }
        

        public BuildMenu(PlanetSim parent)
        {
            Parent = parent;
            Reset();
        }

        public void Reset()
        {
            m_newPlanet = null;
            m_editPlanet = null;
            m_planetMousePos = null;
            ForcePlanetAtMouseUpdate = true;
            RelativeMousePosition = Vector2.Zero;
        }

        public void Update()
        {
            ForcePlanetAtMouseUpdate = true;
            RelativeMousePosition = Camera.CalculateRelative2DPosition(InputMouse.Position);

            if (InputMouse.LeftClicked)
                CheckLeftClickActions();
            else if(InputMouse.LeftReleased)
                CheckLeftReleasedAction();

            if(InputMouse.RightClicked)
                CheckRightClickActions();

            if(InputMouse.LeftHold)
                CheckLeftHoldActions();

            if(InputMouse.MouseWheelClicked)
                CheckWheelClickedActions();
        }

        private void CheckLeftClickActions()
        {
            if (IsCreating || IsEditing)
                return;

            if (EditPlanetAtMousePosition())
                return;

            CreatePlanet();
        }

        private bool EditPlanetAtMousePosition()
        {
            m_editPlanet = PlanetAtMousePosition;

            if (IsEditing)
            {
                EditPlanetWasSelected = CurrentUniverse.IsPlanetSelected(m_editPlanet);
                m_editPlanetOldDirection = m_editPlanet.Direction;
                CurrentUniverse.SelectPlanet(m_editPlanet);
            }

            return IsEditing;
        }

        private void CreatePlanet()
        {
            if (IsCreating)
                return;

            CreateNewPlanetAtMousePosition();
        }

        private void CreateNewPlanetAtMousePosition()
        {
            m_newPlanet = new Planet(CurrentUniverse);
            m_newPlanet.Position = RelativeMousePosition;
            CurrentUniverse.AddPlanet(m_newPlanet);
            CurrentUniverse.TogglePlanetSelection(m_newPlanet);
        }

        private void CheckLeftReleasedAction()
        {
            if (IsCreating)
            {
                CurrentUniverse.UnselectPlanet(m_newPlanet);
                m_newPlanet = null;
            }

            if (IsEditing)
            {
                if (EditPlanetWasSelected)
                    CurrentUniverse.SelectPlanet(m_editPlanet);
                else
                    CurrentUniverse.UnselectPlanet(m_editPlanet);

                if (m_editPlanet == PlanetAtMousePosition)
                    CurrentUniverse.TogglePlanetSelection(m_editPlanet);

                m_editPlanet = null;
            }
        }

        private void CheckRightClickActions()
        {
            if (IsCreating)
                CancelNewPlanetCreation();

            else if (IsEditing)
                CancelEditing();

            else
                RemovePlanetAtMousePosition();
        }

        private void CancelNewPlanetCreation()
        {
            CurrentUniverse.RemovePlanet(m_newPlanet);

            m_newPlanet = null;
        }

        private void CancelEditing()
        {
            m_editPlanet.Direction = m_editPlanetOldDirection;
            m_editPlanet = null;
        }

        private void RemovePlanetAtMousePosition()
        {
            CurrentUniverse.RemovePlanetAtPosition(RelativeMousePosition);
        }

        private void CheckLeftHoldActions()
        {
            NewPlanetAdjustingChecks();
            EditPlanetAdjustingChecks();
        }

        private void NewPlanetAdjustingChecks()
        {
            if (!IsCreating)
                return;

            AdjustNewPlanetRadius();
            AdjustNewPlanetMass();
        }

        private void AdjustNewPlanetRadius()
        {
            float oldRadius = m_newPlanet.Radius;

            m_newPlanet.Radius = (m_newPlanet.Position - RelativeMousePosition).Length();
            if (m_newPlanet.Radius < 2)
                m_newPlanet.Radius = 2;

            if (CurrentUniverse.IsPlanetCollidingWithAnyOtherPlanet(m_newPlanet))
                m_newPlanet.Radius = oldRadius;         
        }

        private void AdjustNewPlanetMass()
        {
            m_newPlanet.Density = GameGlobals.GenericDensity;
        }

        private void EditPlanetAdjustingChecks()
        {
            if (!IsEditing)
                return;

            AdjustEditPlanetDirection();
        }

        private void AdjustEditPlanetDirection()
        {
            Vector2 distanceVector = RelativeMousePosition - m_editPlanet.Position;
            if(distanceVector.LengthSquared() < m_editPlanet.Radius * m_editPlanet.Radius)
            {
                m_editPlanet.Direction = m_editPlanetOldDirection;
                return;
            }

            m_editPlanet.Direction = distanceVector;
            distanceVector.Normalize();
            m_editPlanet.Direction -= distanceVector * m_editPlanet.Radius;
            m_editPlanet.Direction *= GameGlobals.SimulationSpeedInverse;
        }

        private void CheckWheelClickedActions()
        {
            if (PlanetAtMousePosition != null)
                CurrentUniverse.FocusPlanet(PlanetAtMousePosition);
        }
    }
}
