namespace PlanetSimulation.EngineComponents
{
    abstract class DataTransformationArray
    {
        protected float[] m_dataArray;
        protected int m_argumentCount;

        public DataTransformationArray(int size, int argumentCount)
        {
            m_argumentCount = argumentCount;
            m_dataArray = new float[size * argumentCount];
        }

        public float[] GetDataArray()
        {
            return m_dataArray;
        }

        public abstract void SetPlanet(int index, Planet planet);

    }

    class InputTransformationArray : DataTransformationArray
    {
        public InputTransformationArray(int size, int argumentCount) : base(size, argumentCount)
        {

        }

        public override void SetPlanet(int index, Planet planet)
        {
            int startIndex = index * m_argumentCount;
            m_dataArray[startIndex] = planet.Position.X;
            m_dataArray[startIndex + 1] = planet.Position.Y;
            m_dataArray[startIndex + 2] = planet.Mass;
        }
    }

    class OutputTransformationArray : DataTransformationArray
    {
        public OutputTransformationArray(int size, int argumentCount) : base(size, argumentCount)
        {

        }

        public override void SetPlanet(int index, Planet planet)
        {
            int startIndex = index * m_argumentCount;
            m_dataArray[startIndex] = planet.Direction.X;
            m_dataArray[startIndex + 1] = planet.Direction.Y;
        }
    }
}


