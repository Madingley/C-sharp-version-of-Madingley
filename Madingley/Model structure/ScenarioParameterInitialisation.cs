using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

using Microsoft.Research.Science.Data;

namespace Madingley
{
    /// <summary>
    /// Reads the file specifying which scenarios will be run, and stores this information
    /// </summary>
    public class ScenarioParameterInitialisation
    {
        /// <summary>
        /// The number of scenarios to be run
        /// </summary>
        private int _scenarioNumber;
        /// <summary>
        /// Get the number of scenarios to be run
        /// </summary>
        public int scenarioNumber
        { get { return _scenarioNumber; } }

        /// <summary>
        /// Parameters for all scenarios in the model run
        /// </summary>
        private List<Tuple<string,int,SortedList<string,Tuple<string,double, double>>>> _scenarioParameters;
        /// <summary>
        /// Get the parameters for all scenarios in the model run
        /// </summary>
        public List<Tuple<string,int, SortedList<string, Tuple<string, double, double>>>> scenarioParameters
        { get { return _scenarioParameters; } }
        
        /// <summary>
        /// Constructor for ScenarioParameterInitialisation: reads in scenario parameters from a specified file
        /// </summary>
        /// <param name="scenarioParameterFile">The name of the scenario parameters file, which must be in the 'Model setup' directory</param>
        /// <param name="outputPath">The directory to write output files to</param>
        public ScenarioParameterInitialisation(string scenarioParameterFile, string outputPath)
        {
            Console.WriteLine("Reading scenario parameters file...\n");

            // Construct file name
            string FileString = "msds:csv?file=input/Model setup/Initial model state setup/" + scenarioParameterFile + "&openMode=readOnly";

            //Copy the scenarioParameterFile to the output directory
            System.IO.File.Copy("input/Model setup/Initial model state setup/" + scenarioParameterFile, outputPath + scenarioParameterFile, true);

            // Read in the data
            DataSet InternalData = DataSet.Open(FileString);

            // Get the number of scenarios to be run based on the number of lines in the first variable in the input file
            _scenarioNumber = InternalData.Variables[0].GetData().Length;

            // Intialise sorted lists for parameter combinations and simulations numbers
            _scenarioParameters = new List<Tuple<string,int, SortedList<string, Tuple<string, double, double>>>>();
            
            // Temporary vector to hold parameter information
            string[] TempExtractionParameters = new string[_scenarioNumber];

            // Find the 'label'  and 'simulation number' columns in the scenarios file 
            // and create a corresponding items in the sorted list
            // First, check that the scenarios file contains columns called 'label' and 'simulation number'
            Debug.Assert(InternalData.Variables.Contains("label"), 
                "The scenario file must contain a column called 'label'");
            Debug.Assert(InternalData.Variables.Contains("simulation number"),
                "The scenario file must contain a column called 'simulation number'");
            // Get values from the columns called 'label' and 'simulation number'
            var TempValues = InternalData.Variables["label"].GetData();
            var TempValues2 = InternalData.Variables["simulation number"].GetData();
            // Loop over scenarios and add labels to sorted list
            for (int i = 0; i < _scenarioNumber; i++)
            {
                string label = TempValues.GetValue(i).ToString();
                _scenarioParameters.Add(new Tuple<string, int, SortedList<string, Tuple<string, double, double>>>
                    (TempValues.GetValue(i).ToString(), Convert.ToInt16(TempValues2.GetValue(i).ToString()),
                        new SortedList<string, Tuple<string, double, double>>()));
            }

            // Loop over columns in the scenarios file again, and populate the sorted list with
            // scenario information
            foreach (Variable v in InternalData.Variables)
            {
                //Get the name of the variable currently referenced in the dataset
                string HeaderName = v.Name;
                //Copy the values for this variable into an array
                TempValues = v.GetData();

                switch (HeaderName.ToLower())
                {
                    case "npp":
                        // Loop over scenarios and extract the npp parameters for each
                        for (int i = 0; i < _scenarioNumber; i++)
                        {
                            string[] pair = TempValues.GetValue(i).ToString().Split(' ');

                            if (pair.Length > 2)
                            {
                                scenarioParameters.ElementAt(i).Item3.Add
                                    ("npp", new Tuple<string, double, double>(pair[0], Convert.ToDouble(pair[1]), Convert.ToDouble(pair[2])));
                            }
                            else
                            {
                                scenarioParameters.ElementAt(i).Item3.Add
                                    ("npp", new Tuple<string, double, double>(pair[0], Convert.ToDouble(pair[1]), -999));
                            }
                        }
                        break;
                    case "temperature":
                        // Loop over scenarios and extract the temperature parameters for each
                        for (int i = 0; i < _scenarioNumber; i++)
                        {
                            string[] pair = TempValues.GetValue(i).ToString().Split(' ');

                            if (pair.Length > 2)
                            {
                                _scenarioParameters.ElementAt(i).Item3.Add
                                    ("temperature", new Tuple<string, double, double>(pair[0], Convert.ToDouble(pair[1]), Convert.ToDouble(pair[2])));
                            }
                            else
                            {
                                _scenarioParameters.ElementAt(i).Item3.Add
                                    ("temperature", new Tuple<string, double, double>(pair[0], Convert.ToDouble(pair[1]), -999));
                            }
                        }
                        break;
                    case "harvesting":
                        // Loop over scenarios and exract the harvesting parameters for each
                        for (int i = 0; i < _scenarioNumber; i++)
                        {
                            string[] pair = TempValues.GetValue(i).ToString().Split(' ');
                            if (pair.Length > 2)
                            {
                                _scenarioParameters.ElementAt(i).Item3.Add
                                    ("harvesting", new Tuple<string, double, double>(pair[0], Convert.ToDouble(pair[1]), Convert.ToDouble(pair[2])));
                            }
                            else
                            {
                                _scenarioParameters.ElementAt(i).Item3.Add
                                    ("harvesting", new Tuple<string, double, double>(pair[0], Convert.ToDouble(pair[1]), -999));
                            }
                        }
                        break;
                    default:
                        break;
                }
 
            }


        }

    }
}
