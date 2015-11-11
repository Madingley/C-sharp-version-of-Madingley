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
    public partial class AdvectiveDispersal : IDispersalImplementation
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

        Boolean varExists;

        #region Methods

        /// <summary>
        /// Constructor for dispersal: assigns all parameter values
        /// </summary>
        public AdvectiveDispersal(string globalModelTimeStepUnit, Boolean DrawRandomly)
        {
            InitialiseParatemersAdvectiveDispersal();

            // Initialise the utility functions
            UtilityFunctions Utilities = new UtilityFunctions();

            // Calculate the scalar to convert from the time step units used by this implementation of dispersal to the global model time step units
            _DeltaT = Utilities.ConvertTimeUnits(globalModelTimeStepUnit, _TimeUnitImplementation);

            // Initialise the advective dispersal temporal scaling to adjust between time steps appropriately
            _AdvectionTimeStepsPerModelTimeStep = Utilities.ConvertTimeUnits(globalModelTimeStepUnit, "day") * 24 / _AdvectiveModelTimeStepLengthHours;

            // Convert velocity from m/s to km/month. Note that if the _TimeUnitImplementation changes, this will also have to change.
            VelocityUnitConversion = 60 * 60 * 24 * Utilities.ConvertTimeUnits(globalModelTimeStepUnit, "day") * _DeltaT / 1000;
       
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
        /// Run advective dispersal
        /// </summary>
        /// <param name="cellIndex">The longitudinal and latitudinal indices of the focal grid cell</param>
        /// <param name="gridForDispersal">The model grid to run dispersal for</param>
        /// <param name="cohortToDisperse">The cohort to run dispersal for</param>
        /// <param name="actingCohortFunctionalGroup">The functional group index of the acting cohort</param>
        /// <param name="actingCohortNumber">The position of the acting cohort wihtin the functional group in the array of grid cell cohorts</param>
        /// <param name="currentMonth">The current model month</param>
        public void RunDispersal(uint[] cellIndex, ModelGrid gridForDispersal, Cohort cohortToDisperse, int actingCohortFunctionalGroup, 
            int actingCohortNumber, uint currentMonth)
        {
            // An array to hold the dispersal information
            double[] DispersalArray = new double[6];
            
            // A double to indicate whether or not the cohort has dispersed, and if it has dispersed, where to
            double CohortDispersed = 0;

            // Temporary variables to keep track of directions in which cohorts enter/exit cells during the multiple advection steps per time step
            uint ExitDirection = new uint();
            uint EntryDirection = new uint();
            ExitDirection = 9999;

            // An array to hold the present cohort location for the intermediate steps that occur before the final dispersal this time step
            uint[] PresentLocation = { cellIndex[0], cellIndex[1] };

            // Get the u speed and the v speed from the cell data
            double uAdvectiveSpeed = gridForDispersal.GetEnviroLayer("uVel", currentMonth, PresentLocation[0], PresentLocation[1], 
                out varExists);
            Debug.Assert(uAdvectiveSpeed != -9999);

            double vAdvectiveSpeed = gridForDispersal.GetEnviroLayer("vVel", currentMonth, PresentLocation[0], PresentLocation[1], 
                out varExists);
            Debug.Assert(vAdvectiveSpeed != -9999);         

            uAdvectiveSpeed = RescaleDispersalSpeed(uAdvectiveSpeed);
            vAdvectiveSpeed = RescaleDispersalSpeed(vAdvectiveSpeed);


            // Loop through a number of times proportional to the rescaled dispersal
            for (int mm = 0; mm < _AdvectionTimeStepsPerModelTimeStep; mm++)
            {
                // Get the probability of dispersal
                DispersalArray = CalculateDispersalProbability(gridForDispersal, PresentLocation[0], PresentLocation[1], 
                    currentMonth, uAdvectiveSpeed, vAdvectiveSpeed);

                // Check to see if it does disperse
                CohortDispersed = CheckForDispersal(DispersalArray[0]);

                // If it does, check to see where it will end up
                if (CohortDispersed > 0)
                {
                    // Check to see if the direction is actually dispersable
                    uint[] DestinationCell = CellToDisperseTo(gridForDispersal, PresentLocation[0], PresentLocation[1], 
                        DispersalArray, CohortDispersed, DispersalArray[4], DispersalArray[5], ref ExitDirection, 
                        ref EntryDirection);

                    // If it is, go ahead and update the cohort location
                    if (DestinationCell[0] < 999999)
                    {
                        PresentLocation = DestinationCell;

                        // Get the u speed and the v speed from the cell data
                        uAdvectiveSpeed = gridForDispersal.GetEnviroLayer("uVel", currentMonth, PresentLocation[0], 
                            PresentLocation[1], out varExists);
                        vAdvectiveSpeed = gridForDispersal.GetEnviroLayer("vVel", currentMonth, PresentLocation[0], 
                            PresentLocation[1], out varExists);
                        uAdvectiveSpeed = RescaleDispersalSpeed(uAdvectiveSpeed);
                        vAdvectiveSpeed = RescaleDispersalSpeed(vAdvectiveSpeed);
                    }
                }
            }


            // Update the dipersal deltas for this cohort, if necessary
            if ((cellIndex[0] != PresentLocation[0]) || (cellIndex[1] != PresentLocation[1]))
            {
                // Update the delta array of cohorts
                gridForDispersal.DeltaFunctionalGroupDispersalArray[cellIndex[0], cellIndex[1]].Add((uint)actingCohortFunctionalGroup);
                gridForDispersal.DeltaCohortNumberDispersalArray[cellIndex[0], cellIndex[1]].Add((uint)actingCohortNumber);

                // Update the delta array of cells to disperse to
                gridForDispersal.DeltaCellToDisperseToArray[cellIndex[0], cellIndex[1]].Add(PresentLocation);

                // Update the delta array of exit and entry directions
                gridForDispersal.DeltaCellExitDirection[cellIndex[0], cellIndex[1]].Add(ExitDirection);
                gridForDispersal.DeltaCellEntryDirection[cellIndex[0], cellIndex[1]].Add(EntryDirection);
            }
        }
                
        #endregion
    }
}
