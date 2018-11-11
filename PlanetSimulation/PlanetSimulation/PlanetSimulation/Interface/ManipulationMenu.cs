using InputFunctionality.KeyboardAdapter;
using InputFunctionality.MouseAdapter;
using Microsoft.Xna.Framework.Input;

namespace PlanetSimulation.Interface
{
    class ManipulationMenu
    {
        PlanetSim Parent { get; set; }

        MouseAdvanced InputMouse { get { return Parent.InputMouse; } }
        KeyboardAdvanced InputKeyboard { get { return Parent.InputKeyboard; } }
        Universe CurrentUniverse {get { return Parent.CurrentUniverse; } }

        public ManipulationMenu(PlanetSim parent)
        {
            Parent = parent;
        }

        public void Update()
        {
            DistributionSelection();
            UniverseSaveLoad();
            UniverseManipulation();
        }

        private void UniverseSaveLoad()
        {
            for (int i = 0; i < GameGlobals.MaxUniverseCount; i++)
            {
                if (InputKeyboard.KeyNowPressed(Keys.D1 + i))
                {
                    if (InputKeyboard.KeyCurrentlyPressed(Keys.LeftControl))
                        Parent.CopyUniverse(CurrentUniverse, i);
                    else
                        Parent.QuickLoadUniverse(i);
                }
            }

            if (InputKeyboard.KeyNowPressed(Keys.P))
                FileAdapter.WriteUniverseToFile(CurrentUniverse);

            if (InputKeyboard.KeyNowPressed(Keys.L))
            {
                Parent.FileLoadUniverse(CurrentUniverse.ID);
            }
        }

        private void UniverseManipulation()
        {
            if (InputKeyboard.KeyNowPressed(Keys.Add))
                GameGlobals.SimulationSpeedMuliplicator += GameGlobals.SimulationSpeedStep;
            if (InputKeyboard.KeyNowPressed(Keys.Subtract))
                GameGlobals.SimulationSpeedMuliplicator -= GameGlobals.SimulationSpeedStep;

            if (GameGlobals.SimulationSpeedMuliplicator < 1F)
                GameGlobals.SimulationSpeedMuliplicator = 1F;

            if (InputKeyboard.KeyNowPressed(Keys.R))
                Parent.ResetCurrentUniverse();

            if (InputKeyboard.KeyNowPressed(Keys.Space))
            {
                Parent.Pause = !Parent.Pause;

                if (Parent.Pause)
                    Parent.ResetBuildMenu();
            }

            if (InputKeyboard.KeyNowPressed(Keys.F))
                CurrentUniverse.CreateRandomField();
        }

        private void DistributionSelection()
        {
            if (InputKeyboard.KeyNowPressed(Keys.Y))
                Parent.MultiProcessing.Distribution = EngineComponents.MultiProcessingUnit.DistributionMode.Sequence;
            else if (InputKeyboard.KeyNowPressed(Keys.X))
                Parent.MultiProcessing.Distribution = EngineComponents.MultiProcessingUnit.DistributionMode.ParallelLoop;
            else if (InputKeyboard.KeyNowPressed(Keys.C))
                Parent.MultiProcessing.Distribution = EngineComponents.MultiProcessingUnit.DistributionMode.Modulo;
            else if (InputKeyboard.KeyNowPressed(Keys.V))
                Parent.MultiProcessing.Distribution = EngineComponents.MultiProcessingUnit.DistributionMode.LockedRRT;
            else if (InputKeyboard.KeyNowPressed(Keys.B))
                Parent.MultiProcessing.Distribution = EngineComponents.MultiProcessingUnit.DistributionMode.SyncedRRT;
        }

    }
}
