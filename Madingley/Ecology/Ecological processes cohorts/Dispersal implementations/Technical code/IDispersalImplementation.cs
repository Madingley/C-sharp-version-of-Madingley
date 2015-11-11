using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// Interface for implementations of the ecological process of dispersal
    /// </summary>
    public interface IDispersalImplementation
    {
        /// <summary>
        /// Time units associated with the formulation of dispersal
        /// </summary>
        string TimeUnitImplementation { get; }
        
        /// <summary>
        /// Scalar to convert from time units associated with dispersal to the global model time step unit
        /// </summary>
        double DeltaT { get ; } 

        /// <summary>
        /// Run the dispersal implementation
        /// </summary>
        void RunDispersal(uint[] cellIndex, ModelGrid gridForDispersal, Cohort cohortToDisperse, int actingCohortFunctionalGroup, int actingCohortNumber, uint currentMonth);

    }
}
