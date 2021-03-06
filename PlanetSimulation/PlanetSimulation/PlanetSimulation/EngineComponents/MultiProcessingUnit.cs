﻿using System;
using PlanetSimulation.PhysicHandler;
using Microsoft.Xna.Framework;
using System.Management;
using XSLibrary.MultithreadingPatterns.UniquePair;
using PlanetSimulation.OpenCL;

namespace PlanetSimulation.EngineComponents
{
    public class MultiProcessingUnit
    {
        public enum DistributionMode
        {
            Sequence,
            ParallelLoop,
            Modulo,
            LockedRRT,
            SyncedRRT,
            OpenCL
        }

        DistributionMode m_distributionMode;
        public DistributionMode Distribution
        {
            get { return m_distributionMode; }
            set
            {
                m_distributionMode = value;
                ChangeDistribution();
            }
        }

        bool DistributionChanged { get; set; } = true;

        private GravityHandling GravityHandler { get; set; }
        private CollisionHandling CollisionHandler { get; set; }
        private GraphicCardDistribution m_graphicCardDistribution = new GraphicCardDistribution();

        private UniquePairDistribution<Planet, GameTime> m_pairDistribution;

        public int CoreCount { get; private set; }
        public int UsedCores
        {
            get { return m_pairDistribution.CoreCount; }
        }

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
            GravityHandler = parent.GravityHandler;
            CollisionHandler = parent.CollisionHandler;

            CoreCount = GetCoreCount();

            Distribution = DistributionMode.OpenCL;
        }

        public void Close()
        {
            m_pairDistribution.Dispose();
        }

        public void CalculatePlanetMovement(PlanetCollection allPlanets, GameTime currentGameTime)
        {
            m_pairDistribution.DataChanged = allPlanets.Changed || DistributionChanged;
            DistributionChanged = false;
            allPlanets.ClearChangedFlag();

            Planet[] planets = allPlanets.ToArray();

            if (IsStandaloneDistribution())
            {
                m_pairDistribution.Calculate(planets, currentGameTime);
            }
            else
            {
                // gravity
                m_pairDistribution.SetCalculationFunction(GravityHandler.CalculateGravity);
                m_pairDistribution.Calculate(planets, currentGameTime);

                // collisions
                m_pairDistribution.SetCalculationFunction(CollisionHandler.CalculateCollision);
                m_pairDistribution.Calculate(planets, currentGameTime);

                ApplyAccelaration(planets);
            }
        }

        private void ApplyAccelaration(Planet[] allPlanets)
        {
            foreach (Planet planet in allPlanets)
            {
                planet.ApplyAcceleration();
            }
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

        private bool IsStandaloneDistribution()
        {
            return Distribution == DistributionMode.OpenCL;
        }

        private void ChangeDistribution()
        {
            switch (Distribution)
            {
                case DistributionMode.Sequence:
                    m_pairDistribution = new SingleThreadReference<Planet, GameTime>();
                    break;
                case DistributionMode.ParallelLoop:
                    m_pairDistribution = new ParallelLoopDistribution<Planet, GameTime>(CoreCount);
                    break;
                case DistributionMode.Modulo:
                    m_pairDistribution = new EvenlyLockedDistribution<Planet, GameTime>(CoreCount);
                    break;
                case DistributionMode.LockedRRT:
                    m_pairDistribution = new LockedRRTDistribution<Planet, GameTime>(CoreCount);
                    break;
                case DistributionMode.SyncedRRT:
                    m_pairDistribution = new SynchronizedRRTDistribution<Planet, GameTime>(new SystemHandledThreadPool<Planet, GameTime>(CoreCount));
                    break;
                case DistributionMode.OpenCL:
                    m_pairDistribution = m_graphicCardDistribution;
                    break;
            }

            DistributionChanged = true;
        }
    }
}
