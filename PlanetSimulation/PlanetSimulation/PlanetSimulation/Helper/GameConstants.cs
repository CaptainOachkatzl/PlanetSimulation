namespace PlanetSimulation
{
    class GameGlobals
    {
        public const float YearInSeconds = 3.154e+7F;
        public const float DayInSeconds = 86400;

        public const float MultiplierUnit = DayInSeconds;

        private static float m_simSpeed;
        public static float SimulationSpeedMuliplicator
        {
            get { return m_simSpeed; }
            set
            {
                m_simSpeed = value;
                SimulationSpeedInverse = 1 / m_simSpeed;
            }
        }
        public static float SimulationSpeedInverse;

        public const float SimulationSpeedStep = MultiplierUnit / 10;
        public const float GenericDensity = 1000000000F;
        public const int PlanetDataListInspacer = 20;
        public const int MaxUniverseCount = 9;

        public const float MinZoom = 0.000001F;
        public const float MaxZoom = 1F;

        public static int MaximumProcessorsUsed = 0;

        static GameGlobals()
        {
            SimulationSpeedMuliplicator = MultiplierUnit;
        }
    }

    class Units
    {
        public const string Length = "km";
        public const string Time = "s";
        public const string SimulationSpeed = "days/s";
        public const string Weight = "t";
        public const string Speed = Length + "/" + Time;
        public const string Volume = Length + "³";
        public const string Density = Weight + "/" + Volume;
    }
}
