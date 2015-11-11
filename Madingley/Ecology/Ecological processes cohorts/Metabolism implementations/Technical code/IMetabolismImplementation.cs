using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// Interface for implementations of the ecological process of metabolism
    /// </summary>
    public interface IMetabolismImplementation
    {
        /// <summary>
        /// Time units associated with the formulation of metabolism
        /// </summary>
        string TimeUnitImplementation {get;}

        /// <summary>
        /// Scalar to convert from time units associated with metabolism to the global model time step unit
        /// </summary>
        double DeltaT {get;}


        /// <summary>
        /// Calculate the biomass lost through metabolism and update the relevant deltas for the acting cohort
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock functional groups in the model</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="currentMonth">The current month in the model</param>
        void RunMetabolism(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, int[] actingCohort, 
            SortedList<string, double[]> cellEnvironment, Dictionary<string, Dictionary<string, double>> deltas, 
            FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, 
            uint currentTimestep, uint currentMonth);
    }
}
