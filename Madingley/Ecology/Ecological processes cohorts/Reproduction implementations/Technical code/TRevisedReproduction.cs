using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// A formulation of the process of reproduction
    /// </summary>
    public partial class RevisedReproduction : IReproductionImplementation
    {
        #region Declare variables and parameters


        /// <summary>
        /// Scalar to convert from the time step units used by this formulation of reproduction to global model time step units
        /// </summary>
        private double _DeltaT;
        /// <summary>
        /// Get the scalar to convert from the time step units used by this formulation of reproduction to global model time step units
        /// </summary>
        public double DeltaT { get { return _DeltaT; } }

        /// <summary>
        /// An instance of the simple random number generator class
        /// </summary>
        private NonStaticSimpleRNG RandomNumberGenerator = new NonStaticSimpleRNG();

        #endregion

        #region Methods

        /// <summary>
        /// Constructor for reproduction: assigns all parameter values
        /// </summary>
        public RevisedReproduction()
        {
            // Initialise ecological parameters for reproduction
            InitializeReproductionParameters();

            // Calculate the scalar to convert from the time step units used by this implementation of herbivory to the global model time step units
            _DeltaT = UtilityFunctions.ConvertTimeUnits(MadingleyModel.GlobalModelTimeStepUnit, _TimeUnitImplementation);
        }

        /// <summary>
        /// Generate new cohorts from reproductive potential mass
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="cellEnvironment">The environment of the current grid cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions of cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions of stock functional groups in the model</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="tracker">An instance of ProcessTracker to hold diagnostics for reproduction</param>
        /// <param name="partial">Thread-locked variables</param>
        public void RunReproduction(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks,
            int[] actingCohort, SortedList<string, double[]> cellEnvironment, Dictionary<string, Dictionary<string, double>>
            deltas, FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions,
            uint currentTimestep, ProcessTracker tracker, ref ThreadLockedParallelVariables partial)
        {
            // Check that the abundance in the cohort to produce is greater than or equal to zero
            Debug.Assert(_OffspringCohortAbundance >= 0.0, "Offspring abundance < 0");

            // Get the adult and juvenile masses of the cohort to produce
            double[] OffspringProperties = GetOffspringCohortProperties(gridCellCohorts, actingCohort,
                madingleyCohortDefinitions);

            // Update cohort abundance in case juvenile mass has been altered
            _OffspringCohortAbundance = (_OffspringCohortAbundance * gridCellCohorts[actingCohort].JuvenileMass) /
                OffspringProperties[0];

            //Create the offspring cohort
            Cohort OffspringCohort = new Cohort((byte)actingCohort[0],
                                                OffspringProperties[0],
                                                OffspringProperties[1],
                                                OffspringProperties[0],
                                                _OffspringCohortAbundance,
                                                (ushort)currentTimestep, ref partial.NextCohortIDThreadLocked);

            // Add the offspring cohort to the grid cell cohorts array
            gridCellCohorts[actingCohort[0]].Add(OffspringCohort);

            // If the cohort has never been merged with another cohort, then add it to the tracker for output as diagnostics
            if ((!gridCellCohorts[actingCohort].Merged) && tracker.TrackProcesses)
                tracker.RecordNewCohort((uint)cellEnvironment["LatIndex"][0],
                    (uint)cellEnvironment["LonIndex"][0], currentTimestep, _OffspringCohortAbundance,
                    gridCellCohorts[actingCohort].AdultMass, gridCellCohorts[actingCohort].FunctionalGroupIndex);

            // Subtract all of the reproductive potential mass of the parent cohort, which has been used to generate the new
            // cohort, from the delta reproductive potential mass
            deltas["reproductivebiomass"]["reproduction"] -= (gridCellCohorts[actingCohort].IndividualReproductivePotentialMass);

        }

        /// <summary>
        /// Assigns biomass from body mass to reproductive potential mass
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions of cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions of stock functional groups in the model</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="tracker">An instance of ProcessTracker to hold diagnostics for reproduction</param>
        public void AssignMassToReproductivePotential(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks,
            int[] actingCohort, SortedList<string, double[]> cellEnvironment, Dictionary<string, Dictionary<string, double>> deltas,
            FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions,
            uint currentTimestep, ProcessTracker tracker)
        {
            // If this is the first time reproductive potential mass has been assigned for this cohort, 
            // then set the maturity time step for this cohort as the current model time step
            if (gridCellCohorts[actingCohort].MaturityTimeStep == uint.MaxValue)
            {
                gridCellCohorts[actingCohort].MaturityTimeStep = currentTimestep;

                // Track the generation length for this cohort
                if ((!gridCellCohorts[actingCohort].Merged) && tracker.TrackProcesses)
                    tracker.TrackMaturity((uint)cellEnvironment["LatIndex"][0], (uint)cellEnvironment["LonIndex"][0],
                        currentTimestep, gridCellCohorts[actingCohort].BirthTimeStep, gridCellCohorts[actingCohort].JuvenileMass,
                        gridCellCohorts[actingCohort].AdultMass, gridCellCohorts[actingCohort].FunctionalGroupIndex);
            }

            // Assign the specified mass to reproductive potential mass and remove it from individual biomass
            deltas["reproductivebiomass"]["reproduction"] += _BiomassToAssignToReproductivePotential;
            deltas["biomass"]["reproduction"] -= _BiomassToAssignToReproductivePotential;

        }

        #endregion
    }
}
