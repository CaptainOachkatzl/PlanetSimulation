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

        ComputeBuffer<int> m_matrixBuffer;
        ComputeBuffer<float> m_position;
        ComputeBuffer<float> m_direction;
        ComputeBuffer<float> m_mass;

        public override int CoreCount => 2;

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
            m_position = new ComputeBuffer<float>(Kernel.Context, ComputeMemoryFlags.UseHostPointer, new float[2]);
            m_direction = new ComputeBuffer<float>(Kernel.Context, ComputeMemoryFlags.UseHostPointer, new float[2]);
            m_mass = new ComputeBuffer<float>(Kernel.Context, ComputeMemoryFlags.UseHostPointer, new float[1]);

            Kernel.Program.SetMemoryArgument(3, m_position);
            Kernel.Program.SetMemoryArgument(4, m_direction);
            Kernel.Program.SetMemoryArgument(5, m_mass);
            Kernel.Program.SetValueArgument(6, 1 /*elements.GetLength(0)*/);
            Kernel.Program.SetValueArgument(7, elapsedSeconds);
            Kernel.Program.SetValueArgument(8, GameGlobals.SimulationSpeedMuliplicator);
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

        }

        public override void Dispose()
        {
            base.Dispose();

            m_matrixBuffer.Dispose();
        }
    }
}
