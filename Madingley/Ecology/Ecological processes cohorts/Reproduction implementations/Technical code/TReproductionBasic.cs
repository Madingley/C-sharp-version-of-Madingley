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
    public partial class ReproductionBasic : IReproductionImplementation
    {
        #region Declare properties and fields

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

        /// <summary>
        /// Constructor for reproduction: assigns all parameter values
        /// <param name="globalModelTimeStepUnit">The time step of the global model</param>
        /// <param name="drawRandomly">Indicates whether to draw values randomly</param>
        /// </summary>
        public ReproductionBasic(string globalModelTimeStepUnit, Boolean drawRandomly)
        {

            // Initialise ecological parameters for reproduction
            InitialiseReproductionParameters();

            // Initialise the utility class
            UtilityFunctions Utilities = new UtilityFunctions();

            // Calculate the scalar to convert from the time step units used by this implementation of herbivory to the global model time step units
            _DeltaT = Utilities.ConvertTimeUnits(globalModelTimeStepUnit, _TimeUnitImplementation);

            // Instantiate the random number generator
            RandomNumberGenerator = new NonStaticSimpleRNG();

            // Set the seed for the random number generator
            if (drawRandomly)
            {
                RandomNumberGenerator.SetSeedFromSystemTime();
            }
            else
            {
                RandomNumberGenerator.SetSeed(4000);
            }
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
        /// <param name="iteroparous">Whether the acting cohort is iteroparous, as opposed to semelparous</param>
        /// <param name="currentMonth">The current model month</param>
        public void RunReproductionEvents(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, 
            int[] actingCohort, SortedList<string, double[]> cellEnvironment, Dictionary<string, Dictionary<string, double>> 
            deltas, FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, 
            uint currentTimestep, ProcessTracker tracker, ref ThreadLockedParallelVariables partial, bool iteroparous, uint currentMonth)
        {
            // Adult non-reproductive biomass lost by semelparous organisms
            double AdultMassLost;

            // Offspring cohort abundance
            double _OffspringCohortAbundance;

            // Mass ratio of body mass + reproductive mass to adult body mass
            double CurrentMassRatio;

            // Individual body mass including change this time step as a result of other ecological processes
            double BodyMassIncludingChangeThisTimeStep;

            // Offspring juvenile and adult body masses
            double[] OffspringJuvenileAndAdultBodyMasses = new double[2];

            // Offspring cohort
            Cohort OffspringCohort;

            // Individual reproductive mass including change this time step as a result of other ecological processes
            double ReproductiveMassIncludingChangeThisTimeStep;


            // Calculate the biomass of an individual in this cohort including changes this time step from other ecological processes  
            BodyMassIncludingChangeThisTimeStep = 0.0;
            
            foreach (var Biomass in deltas["biomass"])
            {
                // Add the delta biomass to net biomass
                BodyMassIncludingChangeThisTimeStep += Biomass.Value;

            }

            BodyMassIncludingChangeThisTimeStep += gridCellCohorts[actingCohort].IndividualBodyMass;

            // Calculate the reproductive biomass of an individual in this cohort including changes this time step from other ecological processes  
            ReproductiveMassIncludingChangeThisTimeStep = 0.0;

            foreach (var ReproBiomass in deltas["reproductivebiomass"])
            {
		        // Add the delta reproductive biomass to net biomass
                ReproductiveMassIncludingChangeThisTimeStep += ReproBiomass.Value;
            }

            ReproductiveMassIncludingChangeThisTimeStep += gridCellCohorts[actingCohort].IndividualReproductivePotentialMass;

            // Get the current ratio of total individual mass (including reproductive potential) to adult body mass
            CurrentMassRatio = (BodyMassIncludingChangeThisTimeStep + ReproductiveMassIncludingChangeThisTimeStep) / gridCellCohorts[actingCohort].AdultMass;
            
            // Must have enough mass to hit reproduction threshold criterion, and either (1) be in breeding season, or (2) be a marine cell (no breeding season in marine cells)
            if ((CurrentMassRatio > _MassRatioThreshold) && ((cellEnvironment["Breeding Season"][currentMonth] == 1.0) || ((cellEnvironment["Realm"][0] == 2.0))))
            {
                // Iteroparous and semelparous organisms have different strategies
                if (iteroparous)
                {
                    // Iteroparous organisms do not allocate any of their current non-reproductive biomass to reproduction
                    AdultMassLost = 0.0;

                    // Calculate the number of offspring that could be produced given the reproductive potential mass of individuals
                    _OffspringCohortAbundance = gridCellCohorts[actingCohort].CohortAbundance * ReproductiveMassIncludingChangeThisTimeStep /
                        gridCellCohorts[actingCohort].JuvenileMass;
                }
                else
                {
                    // Semelparous organisms allocate a proportion of their current non-reproductive biomass (including the effects of other ecological processes) to reproduction
                    AdultMassLost = _SemelparityAdultMassAllocation * BodyMassIncludingChangeThisTimeStep;

                    // Calculate the number of offspring that could be produced given the reproductive potential mass of individuals
                    _OffspringCohortAbundance = gridCellCohorts[actingCohort].CohortAbundance * (AdultMassLost + ReproductiveMassIncludingChangeThisTimeStep) /
                        gridCellCohorts[actingCohort].JuvenileMass;
                }

                // Check that the abundance in the cohort to produce is greater than or equal to zero
                Debug.Assert(_OffspringCohortAbundance >= 0.0, "Offspring abundance < 0");

                // Get the adult and juvenile masses of the offspring cohort
                OffspringJuvenileAndAdultBodyMasses = GetOffspringCohortProperties(gridCellCohorts, actingCohort, madingleyCohortDefinitions);
                
                // Update cohort abundance in case juvenile mass has been altered through 'evolution'
                _OffspringCohortAbundance = (_OffspringCohortAbundance * gridCellCohorts[actingCohort].JuvenileMass) / OffspringJuvenileAndAdultBodyMasses[0];

                double TrophicIndex;
                switch (madingleyCohortDefinitions.GetTraitNames("nutrition source", actingCohort[0]))
                {
                    case "herbivore":
                        TrophicIndex = 2;
                        break;
                    case "omnivore":
                        TrophicIndex = 2.5;
                        break;
                    case "carnivore":
                        TrophicIndex = 3;
                        break;
                    default:
                        Debug.Fail("Unexpected nutrition source trait value when assigning trophic index");
                        TrophicIndex = 0.0;
                        break;
                }

                // Create the offspring cohort
                OffspringCohort = new Cohort((byte)actingCohort[0], OffspringJuvenileAndAdultBodyMasses[0], OffspringJuvenileAndAdultBodyMasses[1], OffspringJuvenileAndAdultBodyMasses[0],
                                                    _OffspringCohortAbundance, Math.Exp(gridCellCohorts[actingCohort].LogOptimalPreyBodySizeRatio),
                                                    (ushort)currentTimestep, gridCellCohorts[actingCohort].ProportionTimeActive, ref partial.NextCohortIDThreadLocked,TrophicIndex, tracker.TrackProcesses);
                
                // Add the offspring cohort to the grid cell cohorts array
                gridCellCohorts[actingCohort[0]].Add(OffspringCohort);

                // If track processes has been specified then add the new cohort to the process tracker 
                if (tracker.TrackProcesses)
                    tracker.RecordNewCohort((uint)cellEnvironment["LatIndex"][0],  (uint)cellEnvironment["LonIndex"][0], 
                        currentTimestep, _OffspringCohortAbundance, gridCellCohorts[actingCohort].AdultMass, gridCellCohorts[actingCohort].FunctionalGroupIndex,
                        gridCellCohorts[actingCohort].CohortID, (uint)partial.NextCohortIDThreadLocked);

                // Subtract all of the reproductive potential mass of the parent cohort, which has been used to generate the new
                // cohort, from the delta reproductive potential mass and delta adult body mass
                deltas["reproductivebiomass"]["reproduction"] -= ReproductiveMassIncludingChangeThisTimeStep;
                deltas["biomass"]["reproduction"] -= AdultMassLost;

            }
            else
            {
                // Organism is not large enough, or it is not the breeding season, so take no action
            }

        }

        /// <summary>
        /// Assigns ingested biomass from other ecological processes to reproductive potential mass
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
        public void RunReproductiveMassAssignment(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, 
            int[] actingCohort, SortedList<string, double[]> cellEnvironment, Dictionary<string, Dictionary<string, double>> deltas, 
            FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, 
            uint currentTimestep, ProcessTracker tracker)
        {
            // Biomass per individual in each cohort to be assigned to reproductive potential
            double _BiomassToAssignToReproductivePotential;

            // Net biomass change from other ecological functions this time step
            double NetBiomassFromOtherEcologicalFunctionsThisTimeStep;

            // Reset variable holding net biomass change of individuals in this cohort as a result of other ecological processes
            NetBiomassFromOtherEcologicalFunctionsThisTimeStep = 0.0;

            // Loop over all items in the biomass deltas
            foreach (var Biomass in deltas["biomass"])
            {
                // Add the delta biomass to net biomass
                NetBiomassFromOtherEcologicalFunctionsThisTimeStep += Biomass.Value;
            }

            // If individual body mass after the addition of the net biomass from processes this time step will yield a body mass 
            // greater than the adult body mass for this cohort, then assign the surplus to reproductive potential
            if ((gridCellCohorts[actingCohort].IndividualBodyMass + NetBiomassFromOtherEcologicalFunctionsThisTimeStep) > gridCellCohorts[actingCohort].AdultMass)
            {                
                // Calculate the biomass for each individual in this cohort to be assigned to reproductive potential
                _BiomassToAssignToReproductivePotential = gridCellCohorts[actingCohort].IndividualBodyMass + NetBiomassFromOtherEcologicalFunctionsThisTimeStep -
                    gridCellCohorts[actingCohort].AdultMass;

                // Check that a positive biomass is to be assigned to reproductive potential
                Debug.Assert(_BiomassToAssignToReproductivePotential >= 0.0, "Assignment of negative reproductive potential mass");

                // If this is the first time reproductive potential mass has been assigned for this cohort, 
                // then set the maturity time step for this cohort as the current model time step
                if (gridCellCohorts[actingCohort].MaturityTimeStep == uint.MaxValue)
                {
                    gridCellCohorts[actingCohort].MaturityTimeStep = currentTimestep;

                    // Track the generation length for this cohort
                    if (tracker.TrackProcesses && (!gridCellCohorts[actingCohort].Merged))
                        tracker.TrackMaturity((uint)cellEnvironment["LatIndex"][0], (uint)cellEnvironment["LonIndex"][0],
                            currentTimestep, gridCellCohorts[actingCohort].BirthTimeStep, gridCellCohorts[actingCohort].JuvenileMass,
                            gridCellCohorts[actingCohort].AdultMass, gridCellCohorts[actingCohort].FunctionalGroupIndex);
                }

                // Assign the specified mass to reproductive potential mass and remove it from individual biomass
                deltas["reproductivebiomass"]["reproduction"] += _BiomassToAssignToReproductivePotential;
                deltas["biomass"]["reproduction"] -= _BiomassToAssignToReproductivePotential;

            }
            else
            {
                // Cohort has not gained sufficient biomass to assign any to reproductive potential, so take no action
            }
        }
    }
}
