using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlanetSimulation
{
    public class PlanetCollection
    {
        public void Clear()
        {
            Changed = true;
            m_planets.Clear();
        }

        public bool Contains(Planet planet)
        {
            return m_planets.Contains(planet);
        }

        public void CopyTo(Planet[] planets, int index)
        {
            Changed = true;
            m_planets.CopyTo(planets, index);
        }

        public Planet[] ToArray()
        {
            return m_planets.ToArray();
        }


        public bool Changed { get; private set; }

        List<Planet> m_planets = new List<Planet>();

        public Planet this[int key]
        {
            get { return m_planets[key]; }
            set
            {
                Changed = true;
                m_planets[key] = value;
            }
        }

        public int Count => m_planets.Count;
        public bool IsReadOnly => false;

        public PlanetCollection()
        {
            Changed = true;
        }

        public void ClearChangedFlag()
        {
            Changed = false;
        }

        public void Add(Planet planet)
        {
            Changed = true;
            m_planets.Add(planet);
        }

        public bool Remove(Planet planet)
        {
            bool removed = m_planets.Remove(planet);
            Changed |= removed;
            return removed;
        }

    }
}
