using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

namespace Madingley
{
    /// <summary>
    /// Tracks the predation ecological process
    /// </summary>
    public class PredationTracker
    {
        /// <summary>
        /// The flow of mass between prey and predator
        /// </summary>
        private double[,] _MassFlows;
        /// <summary>
        /// Get and set the flow of mass between prey and predator
        /// </summary>
        public double[,] MassFlows
        {
            get { return _MassFlows; }
            set { _MassFlows = value; }
        }


        /// <summary>
        /// Vector of mass bins to be used in the predation tracker
        /// </summary>
        private float[] _MassBins;

        /// <summary>
        /// The number of mass bins to track predation for
        /// </summary>
        private int _NumMassBins;


        /// <summary>
        /// Missing data value to be used in the mass flows output
        /// </summary>
        private double _MissingValue;
    	/// <summary>
        /// Get and set the missing data value to be used in the mass flows output
    	/// </summary>
        public double MissingValue {get { return _MissingValue;} set { _MissingValue = value;} }
	

        /// <summary>
        /// Dataset to output the Massflows data
        /// </summary>
        private DataSet MassFlowsDataSet;


        /// <summary>
        /// An instance of the class to convert data between arrays and SDS objects
        /// </summary>
        private ArraySDSConvert DataConverter;

        /// <summary>
        /// Instance of the class to create SDS objects
        /// </summary>
        private CreateSDSObject SDSCreator;

        /// <summary>
        /// Set up the predation tracker
        /// </summary>_
        /// <param name="numTimeSteps">The total number of timesteps for this simulation</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="massFlowsFilename">Filename for outputs of the flows of mass between predators and prey</param>
        /// <param name="cohortDefinitions">The functional group definitions for cohorts in the model</param>
        /// <param name="missingValue">The missing value to be used in the output file</param>
        /// <param name="outputFileSuffix">The suffix to be applied to the output file</param>
        /// <param name="outputPath">The path to write the output file to</param>
        /// <param name="trackerMassBins">The mass bin handler containing the mass bins to be used for predation tracking</param>
        /// <param name="cellIndex">The index of the current cell in the list of all cells to run the model for</param>
        public PredationTracker(uint numTimeSteps,
            List<uint[]> cellIndices, 
            string massFlowsFilename, 
            FunctionalGroupDefinitions cohortDefinitions, 
            double missingValue,
            string outputFileSuffix,
            string outputPath, MassBinsHandler trackerMassBins, int cellIndex)
        {
            // Assign the missing value
            _MissingValue = missingValue;

            // Get the mass bins to use for the predation tracker and the number of mass bins that this correpsonds to
            _MassBins = trackerMassBins.GetSpecifiedMassBins();
            _NumMassBins = trackerMassBins.NumMassBins;

            // Initialise the array to hold data on mass flows between mass bins
            _MassFlows = new double[_NumMassBins, _NumMassBins];
            
            // Define the model time steps to be used in the output file
            float[] TimeSteps = new float[numTimeSteps];
            for (int i = 1; i <= numTimeSteps; i++)
            {
                TimeSteps[i-1] = i;
            }

            // Initialise the data converter
            DataConverter = new ArraySDSConvert();

            // Initialise the SDS object creator
            SDSCreator = new CreateSDSObject();

            // Create an SDS object to hold the predation tracker data
            MassFlowsDataSet = SDSCreator.CreateSDS("netCDF", massFlowsFilename + outputFileSuffix + "_Cell" + cellIndex, outputPath);

            // Define the dimensions to be used in the predation tracker output file
            string[] dimensions = { "Predator mass bin", "Prey mass bin", "Time steps" };

            // Add the mass flow variable to the predation tracker
            DataConverter.AddVariable(MassFlowsDataSet, "Log mass (g)", 3, dimensions, _MissingValue, _MassBins, _MassBins, TimeSteps);    
        }

        /// <summary>
        /// Record mass flow in an eating event
        /// </summary>
        /// <param name="timestep">The current model time step</param>
        /// <param name="preyBiomass">The individual body mass of the prey</param>
        /// <param name="predatorBiomass">The individual body mass of the predator</param>
        /// <param name="massFlow">The amount of mass consumed in the predation event</param>
        public void RecordFlow(uint timestep, double preyBiomass, double predatorBiomass, double massFlow)
        {
            
            // Find the appropriate mass bin for the cohort
            int PredatorMassBin = 0;
            do
            {
                PredatorMassBin++;
            } while (PredatorMassBin < (_MassBins.Length - 1) && predatorBiomass > _MassBins[PredatorMassBin]);

            // Find the appropriate mass bin for the cohort
            int PreyMassBin = 0;
            do
            {
                PreyMassBin++;
            } while (PreyMassBin < (_MassBins.Length - 1) && preyBiomass > _MassBins[PreyMassBin]);

            _MassFlows[PredatorMassBin,PreyMassBin] += massFlow;

        }

        /// <summary>
        /// Add the mass flows from the current timestep to the dataset
        /// </summary>
        /// <param name="timeStep">the current timestep</param>
        public void AddTimestepFlows(int timeStep)
        {
            // Define the dimensions of the output data
            string[] dimensions = { "Predator mass bin", "Prey mass bin", "Time steps" };

            // Log all values of the mass flow
            for (int i = 0; i < _NumMassBins; i++)
            {
                for (int j = 0; j < _NumMassBins; j++)
                {
                    if (_MassFlows[i, j] > 0) _MassFlows[i, j] = Math.Log(_MassFlows[i, j]);
                    else _MassFlows[i, j] = _MissingValue;
                }
            }
            // Add the mass flows data to the output file
            DataConverter.Array2DToSDS3D(_MassFlows, "Log mass (g)", dimensions, timeStep, _MissingValue, MassFlowsDataSet);

        }

        /// <summary>
        /// Resets the mass flows data array
        /// </summary>
        public void ResetPredationTracker()
        {
            _MassFlows = new double[_NumMassBins, _NumMassBins];
        }


        /// <summary>
        /// Close the predation tracker
        /// </summary>
        public void CloseStreams()
        {
            MassFlowsDataSet.Dispose();
        }
    }
}
