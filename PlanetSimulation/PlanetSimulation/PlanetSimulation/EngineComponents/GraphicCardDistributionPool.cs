//using System;
//using Microsoft.Xna.Framework;
//using Cloo;
//using XSLibrary.MultithreadingPatterns.UniquePair;
//using System.IO;
//using System.Threading;

//namespace PlanetSimulation.EngineComponents
//{
//    class GraphicCardDistributionPool : CorePool<Planet, GameTime>
//    {
//        const string FUNCTION_NAME = "CalculateGravity";

//        SharedMemoryStackCalculation<Planet, GameTime> m_stackCalculation;

//        ComputePlatform m_platform;
//        ComputeContext m_context;
//        ComputeDevice m_graphicsCard;
//        ComputeKernel m_kernelProgram;
//        ComputeCommandQueue m_queue;

//        ManualResetEvent m_resetEvent;

//        ComputeBuffer<float> m_inputBuffer;
//        ComputeBuffer<float> m_outputBuffer;

//        public override int CoreCount
//        {
//            get
//            {
//                return 2; //(int)(m_graphicsCard.MaxComputeUnits * m_graphicsCard.MaxWorkGroupSize);
//            }
//        }

//        public GraphicCardDistributionPool()
//        {
//            m_resetEvent = new ManualResetEvent(false);

//            m_platform = ComputePlatform.Platforms[0];

//            m_context = new ComputeContext(ComputeDeviceTypes.Gpu,
//            new ComputeContextPropertyList(m_platform), null, IntPtr.Zero);

//            m_graphicsCard = m_context.Devices[0];

//            m_queue = new ComputeCommandQueue(m_context, m_graphicsCard, ComputeCommandQueueFlags.None);

//            ComputeProgram program = new ComputeProgram(m_context, GetKernelSource());
//            program.Build(null, null, null, IntPtr.Zero);

//            m_kernelProgram = program.CreateKernel(FUNCTION_NAME);
//        }

//        private string GetKernelSource()
//        {
//            // load opencl source
//            StreamReader streamReader = new StreamReader("../../../../../../kernel.cl");
//            string clSource = streamReader.ReadToEnd();
//            streamReader.Close();
//            return clSource;
//        }

//        public override void DistributeCalculation(int coreIndex, PairingData<Planet, GameTime> calculationPair)
//        {
//            m_resetEvent.Reset();

//            //new Thread(() => {

//            InitializeData(calculationPair);

//            if (calculationPair.CalculateInternally)
//            {
//                for (int i = 0; i < calculationPair.Stack1.Length; i++)
//                    for (int j = i + 1; j < calculationPair.Stack1.Length; j++)
//                        CalculateSinglePair(i, j);

//                for (int i = 0; i < calculationPair.Stack2.Length; i++)
//                    for (int j = i + 1; j < calculationPair.Stack2.Length; j++)
//                        CalculateSinglePair(calculationPair.Stack1.Length + i, calculationPair.Stack1.Length + j);
//            }

//            for (int i = 0; i < calculationPair.Stack1.Length; i++)
//            {
//                for (int j = 0; j < calculationPair.Stack2.Length; j++)
//                {
//                    CalculateSinglePair(i, calculationPair.Stack1.Length + j);
//                }
//            }

//            ApplyResult(calculationPair);

//            CleanUp();

//            m_resetEvent.Set();

//            //}).Start();
//        }

//        private void InitializeData(PairingData<Planet, GameTime> calculationPair)
//        {
//            int size = calculationPair.Stack1.Length + calculationPair.Stack2.Length;
//            InputTransformationArray input = new InputTransformationArray(size, 3);
//            OutputTransformationArray output = new OutputTransformationArray(size, 2);

//            for (int i = 0; i < calculationPair.Stack1.Length; i++)
//            {
//                input.SetPlanet(i, calculationPair.Stack1[i]);
//                output.SetPlanet(i, calculationPair.Stack1[i]);
//            }

//            for (int i = 0; i < calculationPair.Stack2.Length; i++)
//            {
//                input.SetPlanet(i + calculationPair.Stack1.Length, calculationPair.Stack2[i]);
//                output.SetPlanet(i + calculationPair.Stack1.Length, calculationPair.Stack2[i]);
//            }

//            m_inputBuffer = new ComputeBuffer<float>(m_context, ComputeMemoryFlags.UseHostPointer, input.GetDataArray());
//            m_outputBuffer = new ComputeBuffer<float>(m_context, ComputeMemoryFlags.UseHostPointer, output.GetDataArray());

//            m_kernelProgram.SetMemoryArgument(0, m_outputBuffer);
//            m_kernelProgram.SetMemoryArgument(1, m_inputBuffer);
//            m_kernelProgram.SetValueArgument(2, GameGlobals.SimulationSpeedMuliplicator);
//            m_kernelProgram.SetValueArgument(3, (float)calculationPair.GlobalData.ElapsedGameTime.TotalSeconds);
//        }

//        private void CalculateSinglePair(int index1, int index2)
//        {
//            m_kernelProgram.SetValueArgument(4, index1);
//            m_kernelProgram.SetValueArgument(5, index2);

//            m_queue.ExecuteTask(m_kernelProgram, null);
//        }

//        private void ApplyResult(PairingData<Planet, GameTime> calculationPair)
//        {
//            int size = calculationPair.Stack1.Length + calculationPair.Stack2.Length;

//            float[] result = new float[2 * size];

//            m_queue.Finish();

//            m_queue.ReadFromBuffer(m_outputBuffer, ref result, true, null);

//            for (int i = 0; i < calculationPair.Stack1.Length; i++)
//            {
//                calculationPair.Stack1[i].Direction = new Vector2(result[2 * i], result[2 * i + 1]);
//            }

//            for (int i = 0; i < calculationPair.Stack2.Length; i++)
//            {
//                calculationPair.Stack2[i].Direction = 
//                    new Vector2(
//                        result[2 * calculationPair.Stack1.Length + (2 * i)], 
//                        result[2 * calculationPair.Stack1.Length + (2 * i + 1)]);
//            }
//        }

//        private void CleanUp()
//        {
//            m_inputBuffer.Dispose();
//            m_outputBuffer.Dispose();
//        }

//        public override void Synchronize()
//        {
//            m_resetEvent.WaitOne();
//        }

//        public override void Synchronize(int nodeIndex)
//        {
//            // TODO
//            m_resetEvent.WaitOne();
//        }

//        public override void Dispose()
//        {
//        }
//    }
//}
