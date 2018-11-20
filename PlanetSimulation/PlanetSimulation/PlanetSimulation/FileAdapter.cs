using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace PlanetSimulation
{
    class FileAdapter
    {
        private const string FilePath = "\\PlanetSimulation\\";
        private const string FileName = "Save";
        private const string FileEnding = ".txt";

        private const string UniverseHeader = "#startuniverse";
        private const string UniverseZoom = "zoom";
        private const string UniverseCamPositionX = "camposX";
        private const string UniverseCamPositionY = "camposY";
        private const string UniverseParameterEnd = "#parameterenduniverse";
        private const string UniverseEnd = "#enduniverse";

        private const string PlanetHeader = "#startplanet";
        private const string PlanetEnd = "#endplanet";

        private const string PlanetPositionX = "positionX";
        private const string PlanetPositionY = "positionY";

        private const string PlanetDirectionX = "directionX";
        private const string PlanetDirectionY = "directionY";

        private const string PlanetRadius = "radius";
        private const string PlanetMass = "mass";
        private const string PlanetBounce = "bounce";
        private const string PlanetSelected = "selected";
        private const string PlanetFocus = "focus";

        private const string NewLine = "\r\n";

        public static Universe ReadUniverseFromFile(PlanetSim parent, int id)
        {
            try
            {
                using (StreamReader reader = new StreamReader(CreateFileName(id)))
                {
                    Universe createdUniverse = new Universe(parent, id);

                    string[] source = reader.ReadToEnd().Split('\n');
                    for (int i = 0; i < source.Length;i ++)
                        source[i] = source[i].Trim();

                    if (source[0].IndexOf(UniverseHeader) > 0)
                        return null;

                    ReadUniverseParameters(createdUniverse, source);
                    ReadPlanets(createdUniverse, source);

                    return createdUniverse;
                }
            }
            catch { }

            return null;
        }

        private static void ReadUniverseParameters(Universe universe, string[] source)
        {
            int end = 0;
            string[] parameters = GetAllEntriesFromTo(UniverseHeader, UniverseParameterEnd, source, ref end);
            if (parameters == null)
                return;

            try
            {
                universe.Camera.Zoom = Convert.ToSingle(ReadKeyValue(UniverseZoom, parameters));
                universe.Camera.Position2D = new Vector2(
                    Convert.ToSingle(ReadKeyValue(UniverseCamPositionX, parameters)), 
                    Convert.ToSingle(ReadKeyValue(UniverseCamPositionY, parameters)));
            }
            catch { }
        }

        private static void ReadPlanets(Universe universe, string[] source)
        {
            string[] parameters;

            int start = 0;
            while ((parameters = GetAllEntriesFromTo(PlanetHeader, PlanetEnd, source, ref start)) != null)
            {
                Planet createdPlanet = new Planet(universe);
                universe.AddPlanet(createdPlanet);

                try
                {
                    createdPlanet.Position = new Vector2(
                    Convert.ToSingle(ReadKeyValue(PlanetPositionX, parameters)),
                    Convert.ToSingle(ReadKeyValue(PlanetPositionY, parameters)));

                    createdPlanet.Direction = new Vector2(
                    Convert.ToSingle(ReadKeyValue(PlanetDirectionX, parameters)),
                    Convert.ToSingle(ReadKeyValue(PlanetDirectionY, parameters)));

                    createdPlanet.Mass = Convert.ToSingle(ReadKeyValue(PlanetMass, parameters));
                    createdPlanet.Radius = Convert.ToSingle(ReadKeyValue(PlanetRadius, parameters));
                    createdPlanet.BouncingFactor = Convert.ToSingle(ReadKeyValue(PlanetBounce, parameters));

                    if (ReadKeyValue(PlanetSelected, parameters) == "1")
                        universe.SelectPlanet(createdPlanet);

                    if (ReadKeyValue(PlanetFocus, parameters) == "1")
                        universe.FocusPlanet(createdPlanet);

                }
                catch { }
            }
            
        }

        private static string[] GetAllEntriesFromTo(string from, string to, string[] source, ref int skippedEntries)
        {
            string[] cutSource = GetAllEntries(skippedEntries, source);

            int start = FindStringInArray(from, cutSource, false);
            if (start < 0)
                return null;

            start++;

            int end = FindStringInArray(to, cutSource);
            if (end < 0)
                return null;

            skippedEntries += end + 1;

            return GetAllEntries(start, cutSource, end); ;
        }

        private static string[] GetAllEntries(int start, string[] source, int end = 0)
        {
            if (start < 0 || source.Length <= end)
                return source;

            if (end == 0)
                end = source.Length - 1;

            string[] entries = new string[end - start];
            for (int i = 0; i < end - start; i++)
            {
                entries[i] = source[i + start];
            }

            return entries;
        }

        private static int FindStringInArray(string search, string[] source, bool exact = true)
        {
            int index = 0;
            foreach (string entry in source)
            {
                if (exact)
                {
                    if (entry == search)
                        return index;
                }
                else
                {
                    if (entry.IndexOf(search) > -1)
                        return index;
                }

                index++;
            }

            return -1;
        }

        private static string ReadKeyValue(string key, string[] source)
        {
            foreach (string entry in source)
            {
                string[] split = entry.Split(' ');

                if (split.Length < 2 || split[0] != key)
                    continue;

                return split[1];
            }

            return "";
        }




        public static void WriteUniverseToFile (Universe universe)
        {
            
            using (StreamWriter writer = new StreamWriter(CreateFileName(universe.ID)))
            {
                writer.Write(WriteUniverseFileString(universe));
            }
        }

        private static string CreateFileName(int id)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + FilePath;
            Directory.CreateDirectory(path);

            path += FileName + id.ToString() + FileEnding;
            return path;
        }

        private static string WriteUniverseFileString(Universe universe)
        {
            string fileString = UniverseHeader + " " + universe.ID.ToString() + NewLine;
            fileString += WriteUniverseParameterString(universe);

            int planetIndex = 0;
            foreach(Planet planet in universe.Planets.ToArray())
            {
                fileString += WritePlanetString(planet, planetIndex, universe.IsPlanetSelected(planet), universe.Camera.FocusPlanet == planet);
                planetIndex++;
            }

            return fileString + UniverseEnd + NewLine;
        }

        private static string WriteUniverseParameterString(Universe universe)
        {
            string universeParameter = KeyValueString(UniverseZoom, universe.Camera.Zoom.ToString());
            universeParameter += KeyValueString(UniverseCamPositionX, universe.Camera.Position.X.ToString());
            universeParameter += KeyValueString(UniverseCamPositionY, universe.Camera.Position.Y.ToString());

            return universeParameter + UniverseParameterEnd + NewLine;
        }

        private static string WritePlanetString(Planet planet, int index, bool selected, bool focus)
        {
            string planetString = PlanetHeader + " " + index.ToString() + NewLine;

            planetString += KeyValueString(PlanetPositionX, planet.Position.X.ToString());
            planetString += KeyValueString(PlanetPositionY, planet.Position.Y.ToString());
            planetString += KeyValueString(PlanetDirectionX, planet.Direction.X.ToString());
            planetString += KeyValueString(PlanetDirectionY, planet.Direction.Y.ToString());
            planetString += KeyValueString(PlanetRadius, planet.Radius.ToString());
            planetString += KeyValueString(PlanetMass, planet.Mass.ToString());
            planetString += KeyValueString(PlanetBounce, planet.BouncingFactor.ToString());
            planetString += KeyValueString(PlanetSelected, selected ? "1" : "0");
            planetString += KeyValueString(PlanetFocus, focus ? "1" : "0");

            return planetString + PlanetEnd + NewLine;
        }

        private static string KeyValueString(string key, string value)
        {
            return key + " " + value + NewLine;
        }
        
    }
}
