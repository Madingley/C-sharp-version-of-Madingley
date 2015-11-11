using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timing;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
// using RDotNet;
// using RserveCli;

namespace Madingley
{
    /// <summary>
    /// Runs simulations of the Madingley model
    /// </summary>
    class RunSimulations
    {
        
        /// <summary>
        /// Runs the specified number of simulations for each of the specified scenarios
        /// </summary>
        /// <param name="simulationInitialisationFilename">Filename of the file from which to read initialisation information</param>
        /// <param name="scenarios">Contains scenario information for this set of simulations</param>
        /// <param name="outputPath">The path to which outputs should be written</param>
        public void RunAllSimulations(string simulationInitialisationFilename, string definitionsFilename, string outputsFilename, ScenarioParameterInitialisation scenarios, string outputPath)
        {
            // Declare an instance of the class for initializing the Madingley model
            MadingleyModelInitialisation InitialiseMadingley = new MadingleyModelInitialisation(simulationInitialisationFilename, definitionsFilename, outputsFilename, outputPath);
            // Specify the output path in this instance
            InitialiseMadingley.OutputPath = outputPath;

            // List to hold the names of the scenarios to run
            List<string> ScenarioNames = new List<string>();
            // String variable to hold the index suffix to apply to output files for a given simulation
            string OutputFilesSuffix;

            // Loop over scenario names and add the name of the scenario to the list of scenarion names
            foreach (var scenario in scenarios.scenarioParameters)
            {
                ScenarioNames.Add(scenario.Item1);
            }
            
            // Check whether there is only one simulation to run
            if (scenarios.scenarioNumber == 1 && scenarios.scenarioParameters.ElementAt(scenarios.scenarioNumber-1).Item2==1)
            {
                // For a single simulation

                // Set-up the suffix for the output files
                OutputFilesSuffix = "_";
                
                // Loop over the parameters for this scenario
                for (int i = 0; i < ScenarioNames.Count; i++)
                {
                    // Add the parameter information to the suffix for this simulation
                    OutputFilesSuffix +=  ScenarioNames[0] + "_";
                }
                // Add a zero index to the end of the suffix
                OutputFilesSuffix += "0";

                //Run the simulation
                RunSimulation(scenarios, 0, InitialiseMadingley, OutputFilesSuffix, 0);

            }
            else
            {

                if (InitialiseMadingley.RunSimulationsInParallel)
                {
                    // Loop over specified scenarios iteratively
                    for (int ScenarioIndex = 0; ScenarioIndex < scenarios.scenarioNumber; ScenarioIndex++)
                    {
                        //Create an array of new MadingleyModel instances for simulations under this scenario combination
                        MadingleyModel[] MadingleyEcosystemModels = new MadingleyModel
                            [scenarios.scenarioParameters.ElementAt(ScenarioIndex).Item2];

                        for (int simulation = 0; simulation < scenarios.scenarioParameters.ElementAt(ScenarioIndex).Item2; simulation++)
                        {
                            // Set up the suffix for the output files
                            OutputFilesSuffix = "_";

                            // Add the scenario label to the suffix for the output files
                            OutputFilesSuffix += ScenarioNames[ScenarioIndex] + "_";

                            // Add the simulation index number to the suffix
                            OutputFilesSuffix += simulation.ToString();

                            // Initialize the instance of MadingleyModel
                            MadingleyEcosystemModels[simulation] = new MadingleyModel(InitialiseMadingley, scenarios, ScenarioIndex, OutputFilesSuffix,
                                InitialiseMadingley.GlobalModelTimeStepUnit, simulation);
                        }

                        // Loop over the specified number of simulations for each scenario
                        //for (int simulation = 0; simulation<  scenarios.scenarioSimulationsNumber[ScenarioIndex]; simulation++)
                        Parallel.For(0, scenarios.scenarioParameters.ElementAt(ScenarioIndex).Item2, simulation =>
                        {
                            // Declare and start a timer
                            StopWatch s = new StopWatch();
                            s.Start();

                            // Run the simulation
                            MadingleyEcosystemModels[simulation].RunMadingley(InitialiseMadingley);

                            // Stop the timer and write out the time taken to run this simulation
                            s.Stop();
                            Console.WriteLine("Model run finished");
                            Console.WriteLine("Total elapsed time was {0} seconds", s.GetElapsedTimeSecs());

                        });
                    }
                }
                else
                {
                    //Run simulations sequentially

                    // Loop over specified scenarios
                    for (int ScenarioIndex = 0; ScenarioIndex < scenarios.scenarioNumber; ScenarioIndex++)
                    {
                        // Loop over the specified number of simulations for each scenario
                        for (int simulation = 0; simulation < scenarios.scenarioParameters.ElementAt(ScenarioIndex).Item2; simulation++)
                        {
                            // Set up the suffix for the output files
                            OutputFilesSuffix = "_";

                            // Add the scenario label to the suffix for the output files
                            OutputFilesSuffix += ScenarioNames[ScenarioIndex] + "_";

                            // Add the simulation index number to the suffix
                            OutputFilesSuffix += simulation.ToString();

                            // Run the current simulation
                            RunSimulation(scenarios, ScenarioIndex, InitialiseMadingley, OutputFilesSuffix, simulation);
                        }
                    }
                }
               
            }

        }

        /// <summary>
        /// Runs a single simulation of the Madingley model
        /// </summary>
        /// <param name="scenarios">Parameter information and simulation number for all scenarios to be run</param>
        /// <param name="scenarioIndex">The index of the scenario to be run in this simulation</param>
        /// <param name="initialiseMadingley">Model initialization information for all simulations</param>
        /// <param name="outputFileSuffix">Suffix to be applied to the names of files written out by this simulation</param>
        /// <param name="simulation">The index of the simulation being run</param>
        public void RunSimulation(ScenarioParameterInitialisation scenarios, int scenarioIndex, MadingleyModelInitialisation initialiseMadingley, 
            string outputFileSuffix, int simulation)
        {
            // Declare an instance of the class that runs a Madingley model simulation
            MadingleyModel MadingleyEcosystemModel;
            
            // Declare and start a timer
            StopWatch s = new StopWatch();
            s.Start();
            StopWatch t = new StopWatch();
            t.Start();

            // Initialize the instance of MadingleyModel
            MadingleyEcosystemModel = new MadingleyModel(initialiseMadingley, scenarios, scenarioIndex, outputFileSuffix, 
                initialiseMadingley.GlobalModelTimeStepUnit,simulation);
            t.Stop();

            // Run the simulation
            MadingleyEcosystemModel.RunMadingley(initialiseMadingley);

            // Stop the timer and write out the time taken to run this simulation
            s.Stop();
            Console.WriteLine("Model run finished");
            Console.WriteLine("Total elapsed time was {0} seconds", s.GetElapsedTimeSecs());
            Console.WriteLine("Model setup time was {0} seconds", t.GetElapsedTimeSecs());
            Console.WriteLine("Model run time was {0} seconds", s.GetElapsedTimeSecs() - t.GetElapsedTimeSecs());
        }

    }
}
