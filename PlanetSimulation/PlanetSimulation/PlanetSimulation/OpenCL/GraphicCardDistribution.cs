using XSLibrary.MultithreadingPatterns.UniquePair;
using Microsoft.Xna.Framework;
using System;
using Cloo;

namespace PlanetSimulation.OpenCL
{
    class GraphicCardDistribution : UniquePairDistribution<Planet, GameTime>
    {
        RRTPairing RRTMatrix { get; set; }
        KernelModule Kernel { get; set; } = new KernelModule();

        float[] m_planetData;

        ComputeBuffer<int> m_matrixBuffer;
        ComputeBuffer<float> m_planetDataBuffer;

        public override int CoreCount => 2;

        bool fired = false;

        public GraphicCardDistribution()
        {
            string kernelDirectory = AppDomain.CurrentDomain.BaseDirectory + "XSGravitonCL";
            Kernel.Load(kernelDirectory, "gravitonkernel.cl", "Calculate");

            RRTMatrix = new RRTPairing();
            RRTMatrix.GenerateMatrix(CoreCount);
            m_matrixBuffer = new ComputeBuffer<int>(Kernel.Context, ComputeMemoryFlags.UseHostPointer, RRTMatrix.ToArray());
        }

        public override void SetCalculationFunction(PairCalculationFunction function)
        {
            // having generic c# functions executed on a graphics card seems a little 3018 to me
        }

        public override void Calculate(Planet[] elements, GameTime globalData)
        {
            if (fired || elements.GetLength(0) <= 0)
                return;

            fired = true;

            // move RRT matrix to graphics card (probably only once cause core count stays equal)
            WriteRRTMatrix();

            // move planet data to graphics card
            WritePlanetData(elements, (float)globalData.ElapsedGameTime.TotalSeconds);

            // graphics card does the calculation
            CalculateOnGraphicsCard();

            // some sort of synchronization probably
            Synchronize();

            // read planet data from graphics card
            ReadPlanetData();
        }

        private void WritePlanetData(Planet[] elements, float elapsedSeconds)
        {
            m_planetData = CreatePlanetArray(elements);
            m_planetDataBuffer = new ComputeBuffer<float>(Kernel.Context, ComputeMemoryFlags.UseHostPointer, m_planetData);

            Kernel.Program.SetMemoryArgument(3, m_planetDataBuffer);
            Kernel.Program.SetValueArgument(4, elements.GetLength(0));
            Kernel.Program.SetValueArgument(5, elapsedSeconds);
            Kernel.Program.SetValueArgument(6, GameGlobals.SimulationSpeedMuliplicator);
        }

        const int PLANET_DATA_SIZE = 6;
        private float[] CreatePlanetArray(Planet[] elements)
        {
            float[] planetData = new float[elements.GetLength(0) * PLANET_DATA_SIZE];

            for (int i = 0; i < elements.GetLength(0); i++)
            {
                planetData[i * PLANET_DATA_SIZE] = elements[i].Position.X;
                planetData[i * PLANET_DATA_SIZE + 1] = elements[i].Position.Y;
                planetData[i * PLANET_DATA_SIZE + 2] = elements[i].Direction.X;
                planetData[i * PLANET_DATA_SIZE + 3] = elements[i].Direction.Y;
                planetData[i * PLANET_DATA_SIZE + 4] = elements[i].Mass;
            }

            return planetData;
        }

        private void WriteRRTMatrix()
        {
            Kernel.Program.SetMemoryArgument(0, m_matrixBuffer);
            Kernel.Program.SetValueArgument(1, RRTMatrix.StepCount);
            Kernel.Program.SetValueArgument(2, CoreCount);
        }

        private void CalculateOnGraphicsCard()
        {
            Kernel.Queue.ExecuteTask(Kernel.Program, null);
        }

        private void Synchronize()
        {
            Kernel.Queue.Finish();
            
        }

        private void ReadPlanetData()
        {
            Kernel.Queue.ReadFromBuffer(m_planetDataBuffer, ref m_planetData, true, null);
        }

        public override void Dispose()
        {
            base.Dispose();

            m_matrixBuffer.Dispose();
        }
    }
}
