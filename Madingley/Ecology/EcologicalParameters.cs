using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Madingley
{
    public static class EcologicalParameters
    {

        public static Dictionary<string, double> Parameters;
        public static string[] TimeUnits = { "day", "month", "year" };

        public static void ReadEcologicalParameters(string parametersFile, string outputPath)
        {
            //Copy the parameter values to the output directory
            System.IO.File.Copy("input/Model setup/Ecological definition files/" + parametersFile, outputPath + parametersFile, true);

            //Now read the parameter values into a dictionary
            Parameters = new Dictionary<string, double>();
            StreamReader r_env = new StreamReader("input/Model setup/Ecological definition files/" + parametersFile);
            string l;
            char[] comma = ",".ToCharArray();
            
            string[] f;
            
            l = r_env.ReadLine();
            while (!r_env.EndOfStream)
            {
                l = r_env.ReadLine();
                // Split fields by commas
                f = l.Split(comma);
                //First column is the parameter name
                //2nd column is the parameter value

                // Lists of the different fields
                Parameters.Add(f[0], Convert.ToDouble(f[1]));
            }
        }



        public static void WriteEcologicalParameters(string outputPath)
        {
        }

    }
}
