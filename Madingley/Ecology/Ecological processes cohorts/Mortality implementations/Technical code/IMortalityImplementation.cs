using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// Interface for implementations of the ecological process of mortality
    /// </summary>
    public interface IMortalityImplementation
    {

        /// <summary>
        /// Time units associated with the formulation of mortality
        /// </summary>
        string TimeUnitImplementation { get; }

        /// <summary>
        /// Scalar to convert from time units associated with mortality to the global model time step unit
        /// </summary>
        double DeltaT { get; } 

        /// <summary>
        /// Calculate the proportion of individuals in a cohort that die through a particular type of mortality in a model time step
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="bodyMassIncludingChangeThisTimeStep">The body mass that individuals in this cohort will have at the end of this time step</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <returns>The number of individuals lost to a cohort through mortality</returns>
        double CalculateMortalityRate(GridCellCohortHandler gridCellCohorts, int[] actingCohort, double bodyMassIncludingChangeThisTimeStep, Dictionary<string, Dictionary<string, double>> deltas, uint currentTimestep);
    }
}
