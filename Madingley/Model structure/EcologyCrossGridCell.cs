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
    /// A class to specify, initalise and run ecological processes across grid cells
    /// </summary>
    public class EcologyCrossGridCell
    {
        # region Properties and Fields

        /// <summary>
        /// A vector of stopwatch objects for timing the ecological processes
        /// </summary>
        public StopWatch[] s2;
               
        /// <summary>
        /// A sorted list of formulations of dispersal
        /// </summary>
        private SortedList<string, IEcologicalProcessAcrossGridCells> _DispersalFormulations;
        /// <summary>
        /// Get the sorted list of dispersal formulations
        /// </summary>
        public SortedList<string, IEcologicalProcessAcrossGridCells> DispersalFormulations
	    {
            get { return _DispersalFormulations; }
        }
	
        /// <summary>
        /// An instance of apply cross grid cell ecology
        /// </summary>
        ApplyCrossGridCellEcology ApplyCrossGridCellEcologicalProcessResults;


        # endregion

        /// <summary>
        /// Initalise the ecological processes
        /// </summary>
        public void InitializeCrossGridCellEcology(string globalModelTimeStepUnit, Boolean drawRandomly, MadingleyModelInitialisation modelInitialisation)
        {
            // Initialise dispersal formulations
            _DispersalFormulations = new SortedList<string, IEcologicalProcessAcrossGridCells>();

            // Declare and attach dispersal formulations
            Dispersal DispersalFormulation = new Dispersal(drawRandomly, globalModelTimeStepUnit, modelInitialisation);
            _DispersalFormulations.Add("Basic dispersal", DispersalFormulation);

            // Initialise apply ecology
            ApplyCrossGridCellEcologicalProcessResults = new ApplyCrossGridCellEcology();
        }

        /// <summary>
        /// Run ecological processes that operate across grid cells, for a particular grid cell. These should always occur after the within grid cell processes
        /// </summary>

        public void RunCrossGridCellEcology(uint[] cellIndex, ModelGrid gridForDispersal, bool dispersalOnly, FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, uint currentMonth)
        {
            // RUN DISPERSAL
            _DispersalFormulations["Basic dispersal"].RunCrossGridCellEcologicalProcess(cellIndex, gridForDispersal, dispersalOnly, madingleyCohortDefinitions, madingleyStockDefinitions, currentMonth);       
        }

        /// <summary>
        /// Update the properties of all cohorts across all grid cells
        /// </summary>

        public void UpdateCrossGridCellEcology(ModelGrid madingleyModelGrid, ref uint dispersalCounter, CrossCellProcessTracker trackCrossCellProcesses, uint currentTimeStep)
        {
            // Apply the results of cross-cell ecological processes
            ApplyCrossGridCellEcologicalProcessResults.UpdateAllCrossGridCellEcology(madingleyModelGrid, ref dispersalCounter, trackCrossCellProcesses, currentTimeStep);

        }
    }
}
