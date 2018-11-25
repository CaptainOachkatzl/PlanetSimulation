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
        int[] m_matrixData;

        ComputeBuffer<int> m_matrixBuffer;
        ComputeBuffer<float> m_planetDataBuffer;

        int m_previouslyUsedCores = -1;
        public override int CoreCount => 64;

        public GraphicCardDistribution()
        {
            string kernelDirectory = AppDomain.CurrentDomain.BaseDirectory + "XSGravitonCL";
            Kernel.Load(kernelDirectory, "gravitonkernel.cl", "Calculate");

            RRTMatrix = new RRTPairing();
        }

        public override void SetCalculationFunction(PairCalculationFunction function)
        {
            // having generic c# functions executed on a graphics card seems a little 3018 to me
        }

        public override void Calculate(Planet[] elements, GameTime globalData)
        {
            if (elements.Length < 2)
                return;

            Kernel.Program.SetValueArgument(7, (float)globalData.ElapsedGameTime.TotalSeconds);
            Kernel.Program.SetValueArgument(8, GameGlobals.SimulationSpeedMuliplicator);

            int usableCores = CalculateUsableCoreCount(elements.Length);
            // move RRT matrix to graphics card (probably only once cause core count stays equal)
            if (usableCores != m_previouslyUsedCores)
            {
                m_previouslyUsedCores = usableCores;
                WriteRRTMatrix(usableCores);
            }

            if (DataChanged)
            {
                // move planet data to graphics card
                WritePlanetData(elements);
            }

            // graphics card does the calculation
            CalculateOnGraphicsCard(usableCores);

            // some sort of synchronization probably
            Synchronize();

            // read planet data from graphics card
            ReadPlanetData(elements);
        }

        private int CalculateUsableCoreCount(int elementCount)
        {
            return Math.Min(CoreCount, elementCount / 2);
        }

        private void WritePlanetData(Planet[] elements)
        {
            if (m_planetDataBuffer != null)
                m_planetDataBuffer.Dispose();

            m_planetData = CreatePlanetArray(elements);
            m_planetDataBuffer = new ComputeBuffer<float>(Kernel.Context, ComputeMemoryFlags.UseHostPointer, m_planetData);

            Kernel.Program.SetMemoryArgument(1, m_planetDataBuffer);
            Kernel.Program.SetLocalArgument(5, m_planetData.Length * 4);
            Kernel.Program.SetValueArgument(6, elements.Length);
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
                planetData[i * PLANET_DATA_SIZE + 5] = elements[i].Radius;
            }

            return planetData;
        }

        private void WriteRRTMatrix(int usedCores)
        {
            if (m_matrixBuffer != null)
                m_matrixBuffer.Dispose();

            RRTMatrix.GenerateMatrix(usedCores * 2);
            m_matrixData = RRTMatrix.ToArray();
            m_matrixBuffer = new ComputeBuffer<int>(Kernel.Context, ComputeMemoryFlags.UseHostPointer, m_matrixData);

            Kernel.Program.SetMemoryArgument(0, m_matrixBuffer);
            Kernel.Program.SetLocalArgument(2, m_matrixData.Length * 4); // int = 4 bytes
            Kernel.Program.SetValueArgument(3, RRTMatrix.StepCount);
            Kernel.Program.SetValueArgument(4, usedCores);
        }

        private void CalculateOnGraphicsCard(int usedCores)
        {
            long[] offset = new long[usedCores];
            for (int i = 0; i < offset.Length; i++)
                offset[i] = i;

            Kernel.Queue.Execute(Kernel.Program, offset, new long[1] { usedCores }, new long[1] { usedCores }, null);
        }

        private void Synchronize()
        {
            Kernel.Queue.Finish();
        }

        private void ReadPlanetData(Planet[] elements)
        {
            Kernel.Queue.ReadFromBuffer(m_planetDataBuffer, ref m_planetData, true, null);

            for (int i = 0; i < elements.Length; i++)
            {
                Vector2 data = new Vector2(m_planetData[i * PLANET_DATA_SIZE], m_planetData[i * PLANET_DATA_SIZE + 1]);
                elements[i].Position = data;
                data = new Vector2(m_planetData[i * PLANET_DATA_SIZE + 2], m_planetData[i * PLANET_DATA_SIZE + 3]);
                elements[i].Direction = data;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if(m_matrixBuffer != null)
                m_matrixBuffer.Dispose();

            if (m_planetDataBuffer != null)
                m_planetDataBuffer.Dispose();
        }
    }
}
