using XSLibrary.MultithreadingPatterns.UniquePair;
using Microsoft.Xna.Framework;

namespace PlanetSimulation.OpenCL
{
    class GraphicCardDistribution : UniquePairDistribution<Planet, GameTime>
    {
        RRTPairing RRTMatrix { get; set; }

        public GraphicCardDistribution()
        {
            GraphicCardDistributionPool pool = new GraphicCardDistributionPool();

            RRTMatrix = new RRTPairing();
            RRTMatrix.GenerateMatrix(CoreCount);
        }

        public override void SetCalculationFunction(PairCalculationFunction function)
        {
            // having generic c# functions executed on a graphics card seems a little 3018 to me
        }

        public override int CoreCount => 2;

        public override void Calculate(Planet[] elements, GameTime globalData)
        {
            
            RRTMatrix.GenerateMatrix(elements.GetLength(0));

            // move planet data to graphics card
            WritePlanetData();

            // move RRT matrix to graphics card (probably only once cause core count stays equal)
            WriteRRTMatrix();

            // graphics card does the calculation
            CalculateOnGraphicsCard();

            // some sort of synchronization probably
            Synchronize();

            // read planet data from graphics card
            ReadPlanetData();
        }

        private void WritePlanetData()
        {

        }

        private void WriteRRTMatrix()
        {

        }

        private void CalculateOnGraphicsCard()
        {

        }

        private void Synchronize()
        {

        }

        private void ReadPlanetData()
        {

        }
    }
}
