using System;
using PlanetSimulation.PhysicHandler;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Management;
using XSLibrary.MultithreadingPatterns.UniquePair;

namespace PlanetSimulation.EngineComponents
{
    public class MultiProcessingUnit
    {
        private GravityHandling GravityHandler { get; set; }
        private CollisionHandling CollisionHandler { get; set; }

        private UniquePairThreading<Planet, GameTime> m_pairThreading;

        public int CoreCount { get; private set; }

        int m_phyisicalCoreCount = 0;
        public int PhysicalCoreCount
        {
            get
            {
                if (m_phyisicalCoreCount < 1)
                    m_phyisicalCoreCount = GetPhysicalCoreCount();

                return m_phyisicalCoreCount;
            }
        }

        public int LogicalCoreCount { get { return Environment.ProcessorCount; } }

        public MultiProcessingUnit(PlanetSim parent)
        {
            CoreCount = GetCoreCount();
            m_pairThreading = new UniquePairThreading<Planet, GameTime>(CoreCount, false);

            GravityHandler = parent.GravityHandler;
            CollisionHandler = parent.CollisionHandler;
        }
        public void Close()
        {
            m_pairThreading.Dispose();
        }

        public void CalculatePlanetMovement(List<Planet> allPlanets, GameTime currentGameTime)
        {
            m_pairThreading.Calculate(allPlanets.ToArray(), currentGameTime, GravityHandler.CalculateGravity);
            m_pairThreading.Calculate(allPlanets.ToArray(), currentGameTime, CollisionHandler.CalculateCollision);
        }

        private int GetCoreCount()
        {
            if (GameGlobals.MaximumProcessorsUsed > 0)
                return GameGlobals.MaximumProcessorsUsed;

            return LogicalCoreCount;
        }

        private int GetLogicalCoreCount()
        {
            return Environment.ProcessorCount;
        }

        private int GetPhysicalCoreCount()
        {
            int coreCount = 0;
            foreach (ManagementBaseObject item in new ManagementObjectSearcher("Select NumberOfCores from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            return coreCount;
        }
    }
}
