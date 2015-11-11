using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// Interface for implementations of the ecological process of reproduction
    /// </summary>
    public interface IReproductionImplementation
    {
        #region Property and field definitions
        
        /// <summary>
        /// Time units associated with the formulation of reproduction
        /// </summary>
        string TimeUnitImplementation { get; }

        /// <summary>
        /// Scalar to convert from the time units associated with reproduction to the global model time step unit
        /// </summary>
        double DeltaT { get; }
        
        #endregion

        /// <summary>
        /// Generate new cohorts from reproductive potential mass
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock functional groups in the model</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="trackProcesses">An instance of ProcessTracker to hold diagnostics for eating</param>
        /// <param name="partial">Thread-locked variables</param>
        /// <param name="iteroparous">Whether the acting cohort is iteroparous, as opposed to semelparous</param>
        /// <param name="currentMonth">The current model month</param>
        void RunReproductionEvents(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, int[] actingCohort, 
            SortedList<string, double[]> cellEnvironment, Dictionary<string, Dictionary<string, double>> deltas, FunctionalGroupDefinitions 
            madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, uint currentTimestep, ProcessTracker trackProcesses, 
            ref ThreadLockedParallelVariables partial, bool iteroparous, uint currentMonth);
        
        /// <summary>
        /// Assigns surplus body mass to reproductive potential mass
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock functional groups in the model</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="trackProcesses">An instance of ProcessTracker to hold diagnostics for reproduction</param>
        void RunReproductiveMassAssignment(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, int[] actingCohort, SortedList<string, double[]> cellEnvironment, Dictionary<string, Dictionary<string, double>> deltas, FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, uint currentTimestep, ProcessTracker trackProcesses);
    }
}
