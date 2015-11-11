using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// Performs metabolism
    /// </summary>
    public class Metabolism : IEcologicalProcessWithinGridCell
    {
        /// <summary>
        /// The available implementations of the metabolism process
        /// </summary>
        private SortedList<string, IMetabolismImplementation> Implementations;

        /// <summary>
        /// Constructor Metabolism: fills the list of available implementations of metabolism
        /// </summary>
        public Metabolism(string globalModelTimeStepUnit)
        {
            // Initialise the list of metabolism implementations
            Implementations = new SortedList<string, IMetabolismImplementation>();

            // Add the basic endotherm metabolism implementation to the list of implementations
            MetabolismEndotherm MetabolismEndothermImplementation = new MetabolismEndotherm(globalModelTimeStepUnit);
            Implementations.Add("basic endotherm", MetabolismEndothermImplementation);

            // Add the basic ectotherm metabolism implementation to the list of implementations
            MetabolismEctotherm MetabolismEctothermImplementation = new MetabolismEctotherm(globalModelTimeStepUnit);
            Implementations.Add("basic ectotherm", MetabolismEctothermImplementation);
        }

        /// <summary>
        /// Initializes an implementation of metabolism
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock  functional groups in the model</param>
        /// <param name="implementationKey">The name of the implementation of metabolism to initialize</param>
        public void InitializeEcologicalProcess(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks,
            FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions,
            string implementationKey)
        {

        }

        /// <summary>
        /// Run metabolism
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock functional groups in the model</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="trackProcesses">An instance of ProcessTracker to hold diagnostics for metabolism</param>
        /// <param name="partial">Thread-locked variables</param>
        /// <param name="specificLocations">Whether the model is being run for specific locations</param>
        /// <param name="outputDetail">The level of output detail being used for the current model run</param>
        /// <param name="currentMonth">The current model month</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        public void RunEcologicalProcess(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, 
            int[] actingCohort, SortedList<string, double[]> cellEnvironment, Dictionary<string, Dictionary<string, double>> deltas, 
            FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, 
            uint currentTimestep, ProcessTracker trackProcesses, ref ThreadLockedParallelVariables partial,
            Boolean specificLocations, string outputDetail, uint currentMonth, MadingleyModelInitialisation initialisation)
        {
            double Realm = cellEnvironment["Realm"][0];
            if (madingleyCohortDefinitions.GetTraitNames("Heterotroph/Autotroph", gridCellCohorts[actingCohort].FunctionalGroupIndex) == "heterotroph")
            {
                if (madingleyCohortDefinitions.GetTraitNames("Endo/Ectotherm", gridCellCohorts[actingCohort].FunctionalGroupIndex) == "endotherm")
                {

                        Implementations["basic endotherm"].RunMetabolism(gridCellCohorts, gridCellStocks, actingCohort, cellEnvironment, deltas, madingleyCohortDefinitions, madingleyStockDefinitions, currentTimestep, currentMonth);
                }
                else
                {
                        Implementations["basic ectotherm"].RunMetabolism(gridCellCohorts, gridCellStocks, actingCohort, cellEnvironment, deltas, madingleyCohortDefinitions, madingleyStockDefinitions, currentTimestep, currentMonth);

                }

            }

            // If the process tracker is on and output detail is set to high and this cohort has not been merged yet, then record
            // the number of individuals that have died
            if (trackProcesses.TrackProcesses && (outputDetail == "high"))
            {

                trackProcesses.TrackTimestepMetabolism((uint)cellEnvironment["LatIndex"][0],
                                                (uint)cellEnvironment["LonIndex"][0],
                                                currentTimestep,
                                                gridCellCohorts[actingCohort].IndividualBodyMass,
                                                actingCohort[0],
                                                cellEnvironment["Temperature"][currentMonth],
                                                deltas["biomass"]["metabolism"]);

            }
        }


    }
}
