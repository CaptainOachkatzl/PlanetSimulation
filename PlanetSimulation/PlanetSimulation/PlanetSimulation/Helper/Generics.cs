using System.Globalization;

namespace PlanetSimulation
{
    class Generics
    {
        static char[] DecimalIdentifier = new char[2] { ',', '.' };

        static public int RoundFloat(float toRound)
        {
            return (int)(toRound + 0.5F);
        }

        static public string CutDecimals(float toCut, int decimals)
        {
            try
            {
                string result = toCut.ToString("E" + decimals.ToString(), CultureInfo.InvariantCulture);

                int powerIndex = result.IndexOf('E');

                string power = "";
                if (powerIndex > 0)
                {
                    power = result.Substring(powerIndex + 1);
                }

                power = CutLeadingZeros(power);

                if (power.Length <= 0)
                    return result.Substring(0, powerIndex);

                return (result.Substring(0, powerIndex) + " E" + power).Trim();
            }
            catch
            {
                return toCut.ToString();
            }
        }

        static public string CutLeadingZeros(string toCut)
        {
            if (toCut.Length < 2)
                return "";

            string ident = toCut.Substring(0, 1);

            for (int zeroCounter = 1; zeroCounter < toCut.Length; zeroCounter++)
            {
                if (toCut.Substring(zeroCounter, 1) != "0")
                    return ident + toCut.Substring(zeroCounter);
            }

            return "";
        }
    }
}
