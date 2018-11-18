using Cloo;
using System;
using System.IO;

namespace PlanetSimulation.OpenCL
{
    class KernelModule
    {
        public ComputeKernel Program { get; private set; }
        public ComputeCommandQueue Queue { get; private set; }

        ComputePlatform m_platform;
        ComputeContext m_context;
        ComputeDevice m_graphicsCard;

        public void Load(string kernelPath, string function)
        {
            m_platform = ComputePlatform.Platforms[0];

            m_context = new ComputeContext(ComputeDeviceTypes.Gpu,
            new ComputeContextPropertyList(m_platform), null, IntPtr.Zero);

            m_graphicsCard = m_context.Devices[0];

            Queue = new ComputeCommandQueue(m_context, m_graphicsCard, ComputeCommandQueueFlags.None);

            ComputeProgram program = new ComputeProgram(m_context, GetKernelSource(kernelPath));
            try
            {
                program.Build(null, null, null, IntPtr.Zero);
            }
            catch
            {
                string error = program.GetBuildLog(m_graphicsCard);
                throw new Exception(error);
            }

            Program = program.CreateKernel(function);
        }

        private string GetKernelSource(string kernelPath)
        {
            // load opencl source
            StreamReader streamReader = new StreamReader(kernelPath);
            string clSource = streamReader.ReadToEnd();
            streamReader.Close();
            return clSource;
        }
    }
}
