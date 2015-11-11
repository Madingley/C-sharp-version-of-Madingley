using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

namespace Madingley
{
    /// <summary>
    /// Tracks the herbivory ecological process
    /// </summary>
    public class HerbivoryTracker
    {
        /// <summary>
        /// The flow of mass between prey and predator
        /// </summary>
        private double[] _MassFlows;
        /// <summary>
        /// Get and set the flow of mass between prey and predator
        /// </summary>
        public double[] MassFlows
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
        public double MissingValue { get { return _MissingValue; } set { _MissingValue = value; } }


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
        /// The time steps to be run in the current simulation
        /// </summary>
        private float[] TimeSteps;

        /// <summary>
        /// Set up the herbivory tracker
        /// </summary>
        /// <param name="numTimeSteps">The total number of timesteps for this simulation</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="massFlowsFilename">Filename for outputs of the flows of mass between predators and prey</param>
        /// <param name="cohortDefinitions">The functional group definitions for cohorts in the model</param>
        /// <param name="missingValue">The missing value to be used in the output file</param>
        /// <param name="outputFileSuffix">The suffix to be applied to the output file</param>
        /// <param name="outputPath">The path to write the output file to</param>
        /// <param name="trackerMassBins">The mass bin handler containing the mass bins to be used for predation tracking</param>
        public HerbivoryTracker(uint numTimeSteps,
            List<uint[]> cellIndices,
            string massFlowsFilename,
            FunctionalGroupDefinitions cohortDefinitions,
            double missingValue,
            string outputFileSuffix,
            string outputPath, MassBinsHandler trackerMassBins)
        {
            // Assign the missing value
            _MissingValue = missingValue;

            // Get the mass bins to use for the predation tracker and the number of mass bins that this correpsonds to
            _MassBins = trackerMassBins.GetSpecifiedMassBins();
            _NumMassBins = trackerMassBins.NumMassBins;

            // Initialise the array to hold data on mass flows between mass bins
            _MassFlows = new double[_NumMassBins];

            // Define the model time steps to be used in the output file
            TimeSteps = new float[numTimeSteps];
            for (int i = 1; i <= numTimeSteps; i++)
            {
                TimeSteps[i - 1] = i;
            }

            // Initialise the data converter
            DataConverter = new ArraySDSConvert();

            // Initialise the SDS object creator
            SDSCreator = new CreateSDSObject();

            // Create an SDS object to hold the predation tracker data
            MassFlowsDataSet = SDSCreator.CreateSDS("netCDF", massFlowsFilename + outputFileSuffix, outputPath);

            // Define the dimensions to be used in the predation tracker output file
            string[] dimensions = { "Time step", "Herbivore mass bin" };

            // Add the mass flow variable to the predation tracker
            DataConverter.AddVariable(MassFlowsDataSet, "Log mass (g)", 2, dimensions, _MissingValue, TimeSteps, _MassBins);

            
        }

        /// <summary>
        /// Record mass flow in an eating event
        /// </summary>
        /// <param name="timestep">The current model time step</param>
        /// <param name="herbivoreBiomass">The individual body mass of the herbivore</param>
        /// <param name="massFlow">The amount of mass consumed in the predation event</param>
        public void RecordFlow(uint timestep, double herbivoreBiomass, double massFlow)
        {

            // Find the appropriate mass bin for the cohort
            int HerbivoreMassBin = 0;
            do
            {
                HerbivoreMassBin++;
            } while (HerbivoreMassBin < (_MassBins.Length - 1) && herbivoreBiomass > _MassBins[HerbivoreMassBin]);

            _MassFlows[HerbivoreMassBin] += massFlow;

        }

        /// <summary>
        /// Add the mass flows from the current timestep to the dataset
        /// </summary>
        /// <param name="timeStep">the current timestep</param>
        public void AddTimestepFlows(int timeStep)
        {
            // Define the dimensions of the output data
            string[] dimensions = { "Time step", "Herbivore mass bin" };

            // Log all values of the mass flow
            for (int i = 0; i < _NumMassBins; i++)
            {
                if (_MassFlows[i] > 0) _MassFlows[i] = Math.Log(_MassFlows[i]);
                else _MassFlows[i] = _MissingValue;
            }
            // Add the mass flows data to the output file
            DataConverter.VectorToSDS2D(_MassFlows, "Log mass (g)", dimensions, TimeSteps, _MassBins, _MissingValue, MassFlowsDataSet, timeStep);

        }

        /// <summary>
        /// Resets the mass flows data array
        /// </summary>
        public void ResetHerbivoryTracker()
        {
            _MassFlows = new double[_NumMassBins];
        }

        /// <summary>
        /// Close the herbivory tracker
        /// </summary>
        public void CloseStreams()
        {
            MassFlowsDataSet.Dispose();
        }
    }
}
