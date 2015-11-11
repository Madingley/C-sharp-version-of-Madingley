using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// Interface for implementations of the ecological process of eating
    /// </summary>
    public interface IEatingImplementation
    {
        /// <summary>
        /// Time units associated with the formulation of eating
        /// </summary>
        string TimeUnitImplementation { get ; }
        
        /// <summary>
        /// Scalar to convert from time units associated with eating to the global model time step unit
        /// </summary>
        double DeltaT { get ; } 

        /// <summary>
        /// Assimilation efficiency of food mass into acting cohort mass
        /// </summary>
        double AssimilationEfficiency
        {
            get;
            set;
        }

        /// <summary>
        /// Proportion of time spent eating
        /// </summary>
        double ProportionTimeEating
        {
            get;
            set;
        }

        /// <summary>
        /// Time to handle all prey cohorts or plant mass encountered
        /// </summary>
        double TimeUnitsToHandlePotentialFoodItems
        {
            get;
            set;
        }
        
        /// <summary>
        /// List of functional group indices to act on
        /// </summary>
        int[] FunctionalGroupIndicesToEat
        { get; }

        /// <summary>
        /// The total biomass eaten by the acting cohort 
        /// </summary>
        double TotalBiomassEatenByCohort
        { get; }       

        /// <summary>
        /// Initialises eating implementation each time step
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohorts in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stocks in the model</param>
        void InitializeEatingPerTimeStep(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions);
        
        /// <summary>
        /// Calculate the potential biomass that could be gained through eating for marine cells
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of cohorts</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohorts in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stocks in the model</param>
        void GetEatingPotentialMarine(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, int[] actingCohort, 
            SortedList<string, double[]> cellEnvironment, FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions 
            madingleyStockDefinitions);

        /// <summary>
        /// Calculate the potential biomass that could be gained through eating for terrestrial cells
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of cohorts</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohorts in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stocks in the model</param>
        void GetEatingPotentialTerrestrial(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, int[] actingCohort,
            SortedList<string, double[]> cellEnvironment, FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions
            madingleyStockDefinitions);


        /// <summary>
        /// Calculate the actual biomass eaten from each cohort or sotck, apply changes from eating to the cohorts or stocks eaten, and update deltas for the acting cohort
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock functional groups in the model</param>
        /// <param name="trackProcesses">An instance of ProcessTracker to hold diagnostics for eating</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="specificLocations">Whether the model is being run for specific locations</param>
        /// <param name="outputDetail">The level of output detail being used in this model run</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        void RunEating(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, 
            int[] actingCohort, SortedList<string, double[]> cellEnvironment, 
            Dictionary<string, Dictionary<string, double>> deltas, 
            FunctionalGroupDefinitions madingleyCohortDefinitions, 
            FunctionalGroupDefinitions madingleyStockDefinitions, 
            ProcessTracker trackProcesses, uint currentTimestep,
            Boolean specificLocations, string outputDetail, MadingleyModelInitialisation initialisation);

    }
}
