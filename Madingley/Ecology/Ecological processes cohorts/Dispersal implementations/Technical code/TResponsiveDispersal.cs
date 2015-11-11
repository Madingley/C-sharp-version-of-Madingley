using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// A formulation of the process of responsive dispersal
    /// </summary>
    public partial class ResponsiveDispersal : IDispersalImplementation
    {
                
        /// <summary>
        /// Scalar to convert from the time step units used by this formulation of dispersal to global model time step units
        /// </summary>
        private double _DeltaT;
        /// <summary>
        /// Get the scalar to convert from the time step units used by this formulation of dispersal to global model time step units
        /// </summary>
        public double DeltaT { get { return _DeltaT; } }

        /// <summary>
        /// An instance of the simple random number generator class
        /// </summary>
        private NonStaticSimpleRNG RandomNumberGenerator = new NonStaticSimpleRNG();

        /// <summary>
        /// Assigns all parameter values for repsonsive dispersal
        /// </summary>
        public ResponsiveDispersal(string globalModelTimeStepUnit, Boolean DrawRandomly)
        {
            InitialiseParametersResponsiveDispersal();

            // Initialise the utility functions
            UtilityFunctions Utilities = new UtilityFunctions();

            // Calculate the scalar to convert from the time step units used by this implementation of dispersal to the global model time step units
            _DeltaT = Utilities.ConvertTimeUnits(globalModelTimeStepUnit, _TimeUnitImplementation);
            
            // Set the seed for the random number generator
            RandomNumberGenerator = new NonStaticSimpleRNG();
            if (DrawRandomly)
            {
                RandomNumberGenerator.SetSeedFromSystemTime();
            }
            else
            {
                RandomNumberGenerator.SetSeed(14141);
            }
        }

        /// <summary>
        /// Run responsive dispersal
        /// </summary>
        /// <param name="cellIndices">The longitudinal and latitudinal indices of the current grid cell</param>
        /// <param name="gridForDispersal">The model grid to run dispersal for</param>
        /// <param name="cohortToDisperse">The cohort for which to apply the dispersal process</param>
        /// <param name="actingCohortFunctionalGroup">The functional group index of the acting cohort</param>
        /// <param name="actingCohortNumber">The position of the acting cohort within the functional group in the array of grid cell cohorts</param>
        /// <param name="currentMonth">The current model month</param>
        public void RunDispersal(uint[] cellIndices, ModelGrid gridForDispersal, Cohort cohortToDisperse, 
            int actingCohortFunctionalGroup, int actingCohortNumber, uint currentMonth)
        {
            // Starvation driven dispersal takes precedence over density driven dispersal (i.e. a cohort can't do both). Also, the delta 
            // arrays only allow each cohort to perform one type of dispersal each time step
            bool CohortDispersed = false;

            // Check for starvation-driven dispersal
            CohortDispersed = CheckStarvationDispersal(gridForDispersal, cellIndices[0], cellIndices[1], cohortToDisperse, actingCohortFunctionalGroup, actingCohortNumber);

            if (!CohortDispersed)
            {
                // Check for density driven dispersal
                CheckDensityDrivenDispersal(gridForDispersal, cellIndices[0], cellIndices[1], cohortToDisperse, actingCohortFunctionalGroup, actingCohortNumber);
            }
        }

    }
}
