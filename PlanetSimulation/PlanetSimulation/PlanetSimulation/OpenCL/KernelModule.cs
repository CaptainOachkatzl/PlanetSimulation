using Cloo;
using System;
using System.IO;

namespace PlanetSimulation.OpenCL
{
    class KernelModule
    {
        public ComputeKernel Program { get; private set; }
        public ComputeCommandQueue Queue { get; private set; }
        public ComputeContext Context { get; private set; }

        ComputePlatform m_platform;
        ComputeDevice m_graphicsCard;

        public void Load(string directory, string kernelFileName, string function)
        {
            m_platform = ComputePlatform.Platforms[0];

            Context = new ComputeContext(ComputeDeviceTypes.Gpu,
            new ComputeContextPropertyList(m_platform), null, IntPtr.Zero);

            m_graphicsCard = Context.Devices[0];

            Queue = new ComputeCommandQueue(Context, m_graphicsCard, ComputeCommandQueueFlags.None);

            ComputeProgram program = new ComputeProgram(Context, GetKernelSource(directory + "/" + kernelFileName));
            try
            {
                program.Build(null, "-I " + directory, null, IntPtr.Zero);
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
