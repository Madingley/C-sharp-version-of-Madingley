using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// Performs reproduction
    /// </summary>
    public class Reproduction : IEcologicalProcessWithinGridCell
    {

        /// <summary>
        /// The available implementations of the reproduction process
        /// </summary>
        private SortedList<string, IReproductionImplementation> Implementations;
                
        /// <summary>
        /// Constructor for Reproduction: fills the list of available implementations of reproduction
        /// </summary>
        public Reproduction(string globalModelTimeStepUnit, Boolean drawRandomly)
        {
            // Initialize the list of reproduction implementations
            Implementations = new SortedList<string, IReproductionImplementation>();
            
            // Add the basic reproduction implementation to the list of implementations
            ReproductionBasic ReproductionImplementation = new ReproductionBasic(globalModelTimeStepUnit, drawRandomly);
            Implementations.Add("reproduction basic", ReproductionImplementation);
        }

        /// <summary>
        /// Initialize an implementation of reproduction. This is only in here to satisfy the requirements of IEcologicalProcessWithinGridCells
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock functional groups in the model</param>
        /// <param name="implementationKey">The name of the reproduction implementation to initialize</param>
        public void InitializeEcologicalProcess(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, string implementationKey)
        {

        }

        /// <summary>
        /// Run reproduction
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock functional groups in the model</param>
        /// <param name="currentTimeStep">The current model time step</param>
        /// <param name="processTracker">An instance of ProcessTracker to hold diagnostics for eating</param>
        /// <param name="partial">Thread-locked variables for the parallelised version</param>
        /// <param name="specificLocations">Whether the model is being run for specific locations</param>
        /// <param name="outputDetail">The level of output detail being used for this model run</param>
        /// <param name="currentMonth">The current model month</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        public void RunEcologicalProcess(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, 
            int[] actingCohort, SortedList<string, double[]> cellEnvironment, Dictionary<string,Dictionary<string,double>> deltas , 
            FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions,
            uint currentTimeStep, ProcessTracker processTracker, ref ThreadLockedParallelVariables partial,
            Boolean specificLocations, string outputDetail, uint currentMonth, MadingleyModelInitialisation initialisation)
        {

                    // Holds the reproductive strategy of a cohort
        bool _Iteroparous = madingleyCohortDefinitions.GetTraitNames("reproductive strategy", actingCohort[0])=="iteroparity";

            // Assign mass to reproductive potential
            Implementations["reproduction basic"].RunReproductiveMassAssignment(gridCellCohorts, gridCellStocks, actingCohort, cellEnvironment, deltas,
                madingleyCohortDefinitions, madingleyStockDefinitions, currentTimeStep, processTracker);

            // Run reproductive events. Note that we can't skip juveniles here as they could conceivably grow to adulthood and get enough biomass to reproduce in a single time step
            // due to other ecological processes
            Implementations["reproduction basic"].RunReproductionEvents(gridCellCohorts, gridCellStocks, actingCohort, cellEnvironment,
                    deltas, madingleyCohortDefinitions, madingleyStockDefinitions, currentTimeStep, processTracker, ref partial, _Iteroparous, currentMonth);
        }
    }
}
