using Microsoft.Xna.Framework;

namespace PlanetSimulation.Interface
{
    class StatusText
    {
        public bool DrawStatus { get; set; }
        public bool DrawControls { get; set; }

        PlanetSim Parent { get; set; }
        Universe CurrentUniverse { get { return Parent.CurrentUniverse; } }

        string SimSpeed { get; set; }
        string FPSString { get; set; }
        string UniverseID { get; set; }
        string Zoom { get; set; }
        string PlanetCount { get; set; }
        string PhysCoreCount { get; set; }
        string LogCoreCount { get; set; }
        string UsedCores { get; set; }
        string Distribution { get; set; }

        int FPS { get; set; }
        int FrameCounter { get; set; }
        float TimeCounter { get; set; }

        public StatusText(PlanetSim parent)
        {
            Parent = parent;

            FrameCounter = 0;
            TimeCounter = 0;
            FPS = 0;

            DrawStatus = true;
        }

        public void Update()
        {
            if (DrawStatus)
            {
#if DEBUG
                CalculateFPS((float)Parent.CurrentGameTime.ElapsedGameTime.TotalSeconds);
#endif
                UpdateDataString();

            }
        }

        public void DrawStatusText()
        {
            if (DrawStatus || DrawControls)
            {
                string test = CreateStatusText();
                Parent.SpriteBatch.DrawString(Parent.DataFont, test, new Vector2(GameGlobals.PlanetDataListInspacer, GameGlobals.PlanetDataListInspacer), Color.White);
            }
        }

        private string CreateStatusText()
        {
            if (DrawStatus && DrawControls)
                return CreateDataString() + "\n\n\n" + CreateControlsString();
            else if (DrawStatus)
                return CreateDataString();
            else
                return CreateControlsString();
        }

        private string CreateDataString()
        {
            return SimSpeed + "\n" +
                UniverseID + "\n" +
                PlanetCount + "\n" +
                FPSString + "\n" +
                Zoom + "\n" +
                PhysCoreCount + "\n" +
                LogCoreCount + "\n" +
                UsedCores + "\n" +
                Distribution;
        }

        private void UpdateDataString()
        {
            SimSpeed = "Simulation speed: " + Generics.CutDecimals(GameGlobals.SimulationSpeedMuliplicator / GameGlobals.MultiplierUnit, 2) + " " + Units.SimulationSpeed + (Parent.Pause ? " (Paused)" : "");
            FPSString = "FPS: ";
            UniverseID = "Universe: " + (CurrentUniverse.ID + 1);
            Zoom = "Zoom: " + Generics.CutDecimals(Parent.CamControl.Camera.Zoom, 2);
#if !DEBUG
            FPS = Generics.RoundFloat(1F / (float)Parent.CurrentGameTime.ElapsedGameTime.TotalSeconds);
#endif

            FPSString += FPS;
            PlanetCount = "Planet count: " + CurrentUniverse.Planets.Count;
            PhysCoreCount = "Physical cores: " + CurrentUniverse.GetPhysicalCoreCount().ToString();
            LogCoreCount = "Logical cores: " + CurrentUniverse.GetLogicalCoreCount().ToString();
            UsedCores = "Used cores: " + CurrentUniverse.UsedProcessorCoreCount().ToString();
            Distribution = "Distribution: " + Parent.GetDistributionName(); 
        }

        private void CalculateFPS(float elapsedTime)
        {
            FrameCounter++;
            TimeCounter += elapsedTime;
            if (TimeCounter > 1)
            {
                FPS = FrameCounter;
                TimeCounter = 0;
                FrameCounter = 0;
            }
        }

        private string CreateControlsString()
        {
            return "Toggle pause.. Space\n" +
                "Simulation speed.. + / -\n\n" +
                "Create new planet.. Click empty space and drag\n" +
                "Delete planet.. Rightclick planet\n" +
                "Create planet field.. F\n" +
                "Set planet speed.. Click planet and drag\n" +
                "Toggle planet data.. Click planet\n\n" +
                "Switch universe.. 1 - 9\n" +
                "Save universe.. P\n" +
                "Load universe.. L\n" +
                "Reset universe.. R\n\n" +
                "Move camera.. WASD\n" +
                "Zoom.. Mousewheel\n" +
                "Focus planet.. Middle mouse button\n\n" +
                "Toggle multithreading.. M\n\n" + 
                "Toggle status text.. I\n" +
                "Toggle controls text.. C";
        }
    }
}
