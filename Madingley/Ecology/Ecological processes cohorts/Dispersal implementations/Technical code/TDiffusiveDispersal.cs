using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// A formulation of the process of dispersal
    /// </summary>
    public partial class DiffusiveDispersal : IDispersalImplementation
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

        #region Methods

        /// <summary>
        /// Constructor for dispersal: assigns all parameter values
        /// </summary>
        public DiffusiveDispersal(string globalModelTimeStepUnit, Boolean DrawRandomly)
        {
            InitialiseParametersDiffusiveDispersal();

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
        /// Run diffusive dispersal
        /// </summary>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="gridForDispersal">The model grid to run dispersal for</param>
        /// <param name="cohortToDisperse">The cohort for which to run the dispersal process for</param>
        /// <param name="actingCohortFunctionalGroup">The functional group index of the acting cohort</param>
        /// <param name="actingCohortNumber">The position of the cohort within the functional group in the array of grid cell cohorts</param>
        /// <param name="currentMonth">The current model month</param>
        public void RunDispersal(uint[] cellIndices, ModelGrid gridForDispersal, Cohort cohortToDisperse, 
            int actingCohortFunctionalGroup, int actingCohortNumber, uint currentMonth)
        {
            // Calculate dispersal speed for the cohort         
            double DispersalSpeed = CalculateDispersalSpeed(cohortToDisperse.IndividualBodyMass);

            // A double to indicate whether or not the cohort has dispersed, and if it has dispersed, where to
            double CohortDispersed = 0;

            // Temporary variables to keep track of directions in which cohorts enter/exit cells during the multiple advection steps per time step
            uint ExitDirection = new uint();
            uint EntryDirection = new uint();
            ExitDirection = 9999;
            
            // Get the probability of dispersal
            double[] DispersalArray = CalculateDispersalProbability(gridForDispersal, cellIndices[0], cellIndices[1], DispersalSpeed);
            
            // Check to see if it does disperse
            CohortDispersed = CheckForDispersal(DispersalArray[0]);

            // If it does, check to see where it will end up
            if (CohortDispersed > 0)
            {
                // Check to see if the direction is actually dispersable
                uint[] DestinationCell = CellToDisperseTo(gridForDispersal, cellIndices[0], cellIndices[1], DispersalArray, CohortDispersed, DispersalArray[4], DispersalArray[5], ref ExitDirection, ref EntryDirection);

                if (DestinationCell[0] < 999999)
                {
                    // Update the delta array of cohorts
                    gridForDispersal.DeltaFunctionalGroupDispersalArray[cellIndices[0], cellIndices[1]].Add((uint)actingCohortFunctionalGroup);
                    gridForDispersal.DeltaCohortNumberDispersalArray[cellIndices[0], cellIndices[1]].Add((uint)actingCohortNumber);

                    // Update the delta array of cells to disperse to
                    gridForDispersal.DeltaCellToDisperseToArray[cellIndices[0], cellIndices[1]].Add(DestinationCell);

                    // Update the delta array of exit and entry directions
                    gridForDispersal.DeltaCellExitDirection[cellIndices[0], cellIndices[1]].Add(ExitDirection);
                    gridForDispersal.DeltaCellEntryDirection[cellIndices[0], cellIndices[1]].Add(EntryDirection);
                }
            }
        }

        #endregion
    }
}