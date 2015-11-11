using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Timing;

namespace Madingley
{
    /// <summary>
    /// A class to specify, initalise and run ecological processes pertaining to cohorts
    /// </summary>
    public class EcologyCohort
    {
        # region Properties and Fields

        /// <summary>
        /// A vector of stopwatch objects for timing the ecological processes
        /// </summary>
        public StopWatch[] s2;
               
        /// <summary>
        /// A sorted list of formulations of metabolism
        /// </summary>
        private SortedList<string, IEcologicalProcessWithinGridCell> _MetabolismFormulations;
        /// <summary>
        /// Get the sorted list of metabolism formulations
        /// </summary>
        public SortedList<string, IEcologicalProcessWithinGridCell> MetabolismFormulations
	    {
		    get { return _MetabolismFormulations;}
        }

        /// <summary>
        /// A sorted list of formulations of eating
        /// </summary>
        private SortedList<string, IEcologicalProcessWithinGridCell> _EatingFormulations;
        /// <summary>
        /// Get the sorted list of eating formulations
        /// </summary>
        public SortedList<string, IEcologicalProcessWithinGridCell> EatingFormulations
	    {
		    get { return _EatingFormulations;}
        }
	
        /// <summary>
        /// A sorted list of formulations of mortality
        /// </summary>
        private SortedList<string, IEcologicalProcessWithinGridCell> _MortalityFormulations;
        /// <summary>
        /// Get the sorted list of mortality formulations
        /// </summary>
        public SortedList<string, IEcologicalProcessWithinGridCell> MortalityFormulations
	    {
		    get { return _MortalityFormulations;}
        }
       
        /// <summary>
        /// A sorted list of formulations of reproduction
        /// </summary>
        private SortedList<string, IEcologicalProcessWithinGridCell> _ReproductionFormulations;
        /// <summary>
        /// Get the sorted list of reproduction formulations
        /// </summary>
        public SortedList<string, IEcologicalProcessWithinGridCell> Reproductions
	    {
		    get { return _ReproductionFormulations;}
        }

        /// <summary>
        /// An instance of apply ecology
        /// </summary>
        ApplyEcology ApplyEcologicalProcessResults;


        # endregion

        /// <summary>
        /// Initalise the ecological processes
        /// </summary>
        public void InitializeEcology(double cellArea, string globalModelTimeStepUnit, Boolean drawRandomly)
        {
            // Initialise eating formulations
            _EatingFormulations = new SortedList<string, IEcologicalProcessWithinGridCell>();
            // Declare and attach eating formulations
            Eating EatingFormulation = new Eating(cellArea, globalModelTimeStepUnit);
            _EatingFormulations.Add("Basic eating", EatingFormulation);

            // Initialise metabolism formulations
            _MetabolismFormulations = new SortedList<string, IEcologicalProcessWithinGridCell>();
            // Declare and attach metabolism formulations
            Metabolism MetabolismFormulation = new Metabolism(globalModelTimeStepUnit);
            _MetabolismFormulations.Add("Basic metabolism", MetabolismFormulation);

            // Initialise mortality formulations
            _MortalityFormulations = new SortedList<string, IEcologicalProcessWithinGridCell>();
            // Declare and attach mortality formulations
            Mortality MortalityFormulation = new Mortality(globalModelTimeStepUnit);
            _MortalityFormulations.Add("Basic mortality", MortalityFormulation);

            // Initialise reproduction formulations
            _ReproductionFormulations = new SortedList<string, IEcologicalProcessWithinGridCell>();
            // Declare and attach mortality formulations
            Reproduction ReproductionFormulation = new Reproduction(globalModelTimeStepUnit, drawRandomly);
            _ReproductionFormulations.Add("Basic reproduction", ReproductionFormulation);

            // Initialise apply ecology
            ApplyEcologicalProcessResults = new ApplyEcology();


        }

        /// <summary>
        /// Run ecological processes that operate on cohorts within a single grid cell
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The acting cohort</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="deltas">A sorted list of deltas to track changes in abundances and biomasses during the ecological processes</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock functional groups in the model</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="trackProcesses">An instance of the process tracker</param>
        /// <param name="partial">Thread-locked local variables</param>
        /// <param name="specificLocations">Whether the model is being run for specific locations</param>
        /// <param name="outputDetail">The level of output detail being used for this model run</param>
        /// <param name="currentMonth">The current model month</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        public void RunWithinCellEcology(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, int[] actingCohort, 
            SortedList<string, double[]> cellEnvironment, Dictionary<string, Dictionary<string, double>> deltas, FunctionalGroupDefinitions 
            madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, uint currentTimestep, ProcessTracker trackProcesses, 
            ref ThreadLockedParallelVariables partial, Boolean specificLocations,string outputDetail, uint currentMonth, 
            MadingleyModelInitialisation initialisation)
        {

            // RUN EATING
            _EatingFormulations["Basic eating"].RunEcologicalProcess(gridCellCohorts, gridCellStocks, actingCohort, cellEnvironment,
                deltas, madingleyCohortDefinitions, madingleyStockDefinitions, currentTimestep, trackProcesses, ref partial,
                specificLocations, outputDetail, currentMonth, initialisation);

            
            // RUN METABOLISM - THIS TIME TAKE THE METABOLIC LOSS TAKING INTO ACCOUNT WHAT HAS BEEN INGESTED THROUGH EATING
            _MetabolismFormulations["Basic metabolism"].RunEcologicalProcess(gridCellCohorts, gridCellStocks, actingCohort,
                cellEnvironment, deltas, madingleyCohortDefinitions, madingleyStockDefinitions, currentTimestep, trackProcesses, ref partial,
                specificLocations, outputDetail, currentMonth, initialisation);
              
           
            // RUN REPRODUCTION - TAKING INTO ACCOUNT NET BIOMASS CHANGES RESULTING FROM EATING AND METABOLISING
            _ReproductionFormulations["Basic reproduction"].RunEcologicalProcess(gridCellCohorts, gridCellStocks, actingCohort,
                cellEnvironment, deltas, madingleyCohortDefinitions, madingleyStockDefinitions, currentTimestep, trackProcesses, ref partial,
                specificLocations, outputDetail, currentMonth, initialisation);
            
              
            // RUN MORTALITY - TAKING INTO ACCOUNT NET BIOMASS CHANGES RESULTING FROM EATING, METABOLISM AND REPRODUCTION
            _MortalityFormulations["Basic mortality"].RunEcologicalProcess(gridCellCohorts, gridCellStocks, actingCohort,
                cellEnvironment, deltas, madingleyCohortDefinitions, madingleyStockDefinitions, currentTimestep, trackProcesses, ref partial,
                specificLocations, outputDetail, currentMonth, initialisation);
        }

        /// <summary>
        /// Update the properties of the acting cohort and of the environmental biomass pools after running the ecological processes for a cohort
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The acting cohort</param>
        /// <param name="cellEnvironment">The environment of the current grid cell</param>
        /// <param name="deltas">The sorted list of deltas for the current grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock functional groups in the model</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="tracker">A process tracker</param>
        public void UpdateEcology(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, int[] actingCohort, 
            SortedList<string, double[]> cellEnvironment, Dictionary<string, Dictionary<string, double>> deltas, FunctionalGroupDefinitions 
            madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, uint currentTimestep, ProcessTracker tracker)
        {
            // Apply the results of within-cell ecological processes
            ApplyEcologicalProcessResults.UpdateAllEcology(gridCellCohorts, actingCohort, cellEnvironment, deltas, currentTimestep, tracker);

        }
    }
}
