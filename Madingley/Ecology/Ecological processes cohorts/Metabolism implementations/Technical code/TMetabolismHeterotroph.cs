using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// A formulation of the metabolism process
    /// </summary>
    /// <remarks>Functional form and parameters taken from fitted relationship in Brown's (2004) Metabolic Theory of Ecology.
    /// Currently mass assigned to reproductive potential is not metabolised</remarks>
    public partial class MetabolismHeterotroph : IMetabolismImplementation
    {

        /// <summary>
        /// Scalar to convert from the time units used by this metabolism implementation to the global model time step units
        /// </summary>
        private double _DeltaT;
        /// <summary>
        /// Get the scalar to convert from the time units used by this metabolism implementation to the global model time step units
        /// </summary>
        public double DeltaT  { get  {  return _DeltaT;  } }

        /// <summary>
        /// Constant to convert temperature in degrees Celsius to temperature in Kelvin
        /// </summary>
        private double _TemperatureUnitsConvert;

        /// <summary>
        /// Instance of the class to perform general functions
        /// </summary>
        private UtilityFunctions Utilities;

        /// <summary>
        /// Constructor for metabolism: assigns all parameter values
        /// </summary>
        public MetabolismHeterotroph(string globalModelTimeStepUnit)
        {
            // Initialise ecological parameters for metabolism
            InitialiseMetabolismParameters();

            // Initialise the utility functions
            Utilities = new UtilityFunctions();

            // Calculate the scalar to convert from the time step units used by this implementation of metabolism to the global  model time step units
            _DeltaT = Utilities.ConvertTimeUnits(globalModelTimeStepUnit, _TimeUnitImplementation);
            
            // Set the constant to convert temperature in degrees Celsius to Kelvin
            _TemperatureUnitsConvert = 273.0;
        }

        /// <summary>
        /// Run metabolism for the acting cohort
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for the stock functional groups in the model</param>
        /// <param name="currentTimestep">The current model time step</param>
        public void RunMetabolism(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, 
            int[] actingCohort, SortedList<string, double[]> cellEnvironment, Dictionary<string, Dictionary<string, double>> 
            deltas, FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, 
            uint currentTimestep, uint currentMonth)
        {
            // Calculate metabolic loss for an individual and add the value to the delta biomass for metabolism
            deltas["biomass"]["metabolism"] = -CalculateIndividualMetabolicRate(gridCellCohorts[actingCohort].IndividualBodyMass,
                cellEnvironment["Temperature"][currentMonth] + _TemperatureUnitsConvert) * _DeltaT;

            // If metabolic loss is greater than individual body mass after herbivory and predation, then set equal to individual body mass
            deltas["biomass"]["metabolism"] = Math.Max(deltas["biomass"]["metabolism"],-(gridCellCohorts[actingCohort].IndividualBodyMass + deltas["biomass"]["predation"] + deltas["biomass"]["herbivory"]));

            // Add total metabolic loss for all individuals in the cohort to delta biomass for metabolism in the respiratory CO2 pool
            deltas["respiratoryCO2pool"]["metabolism"] = -deltas["biomass"]["metabolism"] * gridCellCohorts[actingCohort].CohortAbundance;
        }

        
    }
}
