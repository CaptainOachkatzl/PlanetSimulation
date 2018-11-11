using System;

namespace PlanetSimulation
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            int screenWidth = 1920;
            int screenHeight = 1080;
            int maxProcessorsUsed = 0;
            bool freeFPS = false;

            try
            {
                if (args.Length > 0)
                {
                    maxProcessorsUsed = Convert.ToInt32(args[0]);
                }
            }
            catch
            {
                maxProcessorsUsed = 0;
            }

            if (maxProcessorsUsed < 0)
                maxProcessorsUsed = 0;

            try
            {
                if (args.Length > 2)
                {
                    screenWidth = Convert.ToInt32(args[1]);
                    screenHeight = Convert.ToInt32(args[2]);
                }
            }
            catch
            {
                screenWidth = 1920;
                screenHeight = 1080;
            }

            try
            {
                if (args.Length > 3)
                {
                    freeFPS = Convert.ToInt32(args[3]) > 0;
                }
            }
            catch
            {
                freeFPS = false;
            }

            using (PlanetSim game = new PlanetSim(maxProcessorsUsed, screenWidth, screenHeight, freeFPS))
            {
                game.Run();
            }
        }
    }
}

