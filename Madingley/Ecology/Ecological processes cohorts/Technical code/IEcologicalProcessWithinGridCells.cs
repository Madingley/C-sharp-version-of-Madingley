using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// Interface for ecological process code
    /// </summary>
    public interface IEcologicalProcessWithinGridCell
    {
        /// <summary>
        /// Run the ecological process
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="madingleyCohortHandler">The definitions of cohort functional groups in the model</param>
        /// <param name="madingleyStockHandler">The definitions of stock functional groups in the model</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="trackProcesses">An instance of ProcessTracker to hold diagnostics for this ecological process</param>
        /// <param name="partial">Thread-locked variables</param>
        /// <param name="specificLocations">Whether the model is being run for specific locations</param>
        /// <param name="outputDetail">The level of output detail used for this model simulation</param>
        /// <param name="currentMonth">The current model month</param>
        /// <param name="initialisation">The instance of the MadingleyModelInitialisation class for this simulation</param>
        void RunEcologicalProcess(GridCellCohortHandler gridCellCohorts, 
            GridCellStockHandler gridCellStocks, 
            int[] actingCohort, SortedList<string, 
            double[]> cellEnvironment, 
            Dictionary<string,Dictionary<string, double>> deltas,
            FunctionalGroupDefinitions madingleyCohortHandler, 
            FunctionalGroupDefinitions madingleyStockHandler,
            uint currentTimestep,
            ProcessTracker trackProcesses,
            ref ThreadLockedParallelVariables partial,
            Boolean specificLocations, string outputDetail, uint currentMonth, MadingleyModelInitialisation initialisation);

        /// <summary>
        /// Initialises an implementation of the ecological process
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock functional groups in the model</param>
        /// <param name="implementationKey">The name of the specific implementation of this process to initialize</param>
        void InitializeEcologicalProcess(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, string implementationKey);
        

    }
}
