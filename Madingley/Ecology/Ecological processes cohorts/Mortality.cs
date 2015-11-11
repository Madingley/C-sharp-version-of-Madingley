using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// Performs mortality
    /// </summary>
    class Mortality : IEcologicalProcessWithinGridCell
    {
        #region Declare properties and fields

        /// <summary>
        /// The available implementations of the mortality process
        /// </summary>
        private SortedList<string, IMortalityImplementation> Implementations;
        
        #endregion

        /// <summary>
        /// Constructor for Mortality: fills the list with available implementations of mortality
        /// <param name="globalModelTimeStepUnit">The time step for the global model</param>
        /// </summary>
        public Mortality(string globalModelTimeStepUnit)
        {
            // Initialize the list of mortality implementations
            Implementations = new SortedList<string, IMortalityImplementation>();

            // Add the background mortality implementation to the list of implementations
            BackgroundMortality BackgroundMortalityImplementation = new BackgroundMortality(globalModelTimeStepUnit);
            Implementations.Add("basic background mortality", BackgroundMortalityImplementation);

            // Add the senescence mortality implementation to the list of implementations
            SenescenceMortality SenescenceMortalityImplementation = new SenescenceMortality(globalModelTimeStepUnit);
            Implementations.Add("basic senescence mortality", SenescenceMortalityImplementation);

            // Add the starvation mortality implementation to the list of implementations
            StarvationMortality StarvationMortalityImplementation = new StarvationMortality(globalModelTimeStepUnit);
            Implementations.Add("basic starvation mortality", StarvationMortalityImplementation);           
       
        }

        /// <summary>
        /// Initialize an implementation of mortality. This is only in here to satisfy the requirements of IEcologicalProcessAcrossGridCells
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock functional groups in the model</param>
        /// <param name="implementationKey">The name of the implementation of mortality to initialize</param>
        public void InitializeEcologicalProcess(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks,
            FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, 
            string implementationKey)
        {

        }

        /// <summary>
        /// Run mortality
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock functional groups in the model</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="trackProcesses">An instance of ProcessTracker to hold diagnostics for mortality</param>
        /// <param name="partial">Thread-locked variables</param>
        /// <param name="specificLocations">Whether the model is being run for specific locations</param>
        /// <param name="outputDetail">The level output detail being used for the current model run</param>
        /// <param name="currentMonth">The current model month</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        public void RunEcologicalProcess(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, 
            int[] actingCohort, SortedList<string, double[]> cellEnvironment, Dictionary<string, Dictionary<string, double>> deltas,
            FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, 
            uint currentTimestep, ProcessTracker trackProcesses, ref ThreadLockedParallelVariables partial,
            Boolean specificLocations, string outputDetail, uint currentMonth, MadingleyModelInitialisation initialisation)
        {
            
        // Variables to hold the mortality rates
        double MortalityRateBackground;
        double MortalityRateSenescence;
        double MortalityRateStarvation;

        // Variable to hold the total abundance lost to all forms of mortality
        double MortalityTotal;

        // Individual body mass including change this time step as a result of other ecological processes
        double BodyMassIncludingChangeThisTimeStep;

        // Individual reproductive mass including change this time step as a result of other ecological processes
        double ReproductiveMassIncludingChangeThisTimeStep;

            // Calculate the body mass of individuals in this cohort including mass gained through eating this time step, up to but not exceeding adult body mass for this cohort. 
            // Should be fine because these deductions are made in the reproduction implementation, but use Math.Min to double check.

            BodyMassIncludingChangeThisTimeStep = 0.0;

            // Loop over all items in the biomass deltas
            foreach (var Biomass in deltas["biomass"])
            {
                // Add the delta biomass to net biomass
                BodyMassIncludingChangeThisTimeStep += Biomass.Value;
            }
            BodyMassIncludingChangeThisTimeStep = Math.Min(gridCellCohorts[actingCohort].AdultMass, BodyMassIncludingChangeThisTimeStep + gridCellCohorts[actingCohort].IndividualBodyMass);
            
            // Temporary variable to hold net reproductive biomass change of individuals in this cohort as a result of other ecological processes
            ReproductiveMassIncludingChangeThisTimeStep = 0.0;

            // Loop over all items in the biomass deltas
            foreach (var Biomass in deltas["reproductivebiomass"])
            {
                // Add the delta biomass to net biomass
                ReproductiveMassIncludingChangeThisTimeStep += Biomass.Value;
            }

            ReproductiveMassIncludingChangeThisTimeStep += gridCellCohorts[actingCohort].IndividualReproductivePotentialMass;

            // Check to see if the cohort has already been killed by predation etc
            if ((BodyMassIncludingChangeThisTimeStep).CompareTo(0.0) <= 0)
            {
                // If individual body mass is not greater than zero, then all individuals become extinct
                MortalityTotal = gridCellCohorts[actingCohort].CohortAbundance;
            }
            else
            {
                // Calculate background mortality rate
                MortalityRateBackground = Implementations["basic background mortality"].CalculateMortalityRate(gridCellCohorts,
                    actingCohort, BodyMassIncludingChangeThisTimeStep, deltas, currentTimestep);

                // If the cohort has matured, then calculate senescence mortality rate, otherwise set rate to zero
                if (gridCellCohorts[actingCohort].MaturityTimeStep != uint.MaxValue)
                {
                    MortalityRateSenescence = Implementations["basic senescence mortality"].CalculateMortalityRate(gridCellCohorts,
                        actingCohort, BodyMassIncludingChangeThisTimeStep, deltas, currentTimestep);
                }
                else
                {
                    MortalityRateSenescence = 0.0;
                }

                // Calculate the starvation mortality rate based on individual body mass and maximum body mass ever
                // achieved by this cohort
                MortalityRateStarvation = Implementations["basic starvation mortality"].CalculateMortalityRate(gridCellCohorts, actingCohort, BodyMassIncludingChangeThisTimeStep, deltas, currentTimestep);

                // Calculate the number of individuals that suffer mortality this time step from all sources of mortality
                MortalityTotal = (1 - Math.Exp(-MortalityRateBackground - MortalityRateSenescence -
                    MortalityRateStarvation)) * gridCellCohorts[actingCohort].CohortAbundance;
            }

            // If the process tracker is on and output detail is set to high and this cohort has not been merged yet, then record
            // the number of individuals that have died
            if (trackProcesses.TrackProcesses && (outputDetail == "high") && (!gridCellCohorts[actingCohort].Merged))
            {
                trackProcesses.RecordMortality((uint)cellEnvironment["LatIndex"][0], (uint)cellEnvironment["LonIndex"][0],gridCellCohorts[actingCohort].BirthTimeStep, 
                    currentTimestep, gridCellCohorts[actingCohort].IndividualBodyMass, gridCellCohorts[actingCohort].AdultMass, 
                    gridCellCohorts[actingCohort].FunctionalGroupIndex,
                    gridCellCohorts[actingCohort].CohortID[0], MortalityTotal, "sen/bg/starv");
            }

            // Remove individuals that have died from the delta abundance for this cohort
            deltas["abundance"]["mortality"] = -MortalityTotal;

            // Add the biomass of individuals that have died to the delta biomass in the organic pool (including reproductive 
            // potential mass, and mass gained through eating, and excluding mass lost through metabolism)
            deltas["organicpool"]["mortality"] = MortalityTotal * (BodyMassIncludingChangeThisTimeStep + ReproductiveMassIncludingChangeThisTimeStep);
        }
    }
}
