using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// Interface for cross grid-cell ecological process code
    /// </summary>
    public interface IEcologicalProcessAcrossGridCells
    {
        /// <summary>
        /// Run the cross-grid-cell ecological process
        /// </summary>
        /// <param name="cellIndex">The cell index for the active cell in the model grid</param>
        /// <param name="gridForDispersal">The model grid to run the process for</param>
        /// <param name="dispersalOnly">Whether we are running dispersal only</param>
        /// <param name="madingleyCohortDefinitions">The functional group definitions for cohorts in the model</param>
        /// <param name="madingleyStockDefinitions">The functional group definitions for stocks in the model</param>
        /// <param name="currentMonth">The current model month</param>
        void RunCrossGridCellEcologicalProcess(uint[] cellIndex, ModelGrid gridForDispersal, bool dispersalOnly, 
            FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, 
            uint currentMonth);
    }
}
