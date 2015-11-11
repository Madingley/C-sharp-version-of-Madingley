using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using System.Threading;
using System.Threading.Tasks;


namespace Madingley
{
    /// <summary>
    /// A revised version of the predation process, written November 2011
    /// </summary>
    public partial class RevisedPredation : IEatingImplementation
    {

        /// <summary>
        /// Holds the thread-local variables to track numbers of extinctions and productions of cohorts
        /// </summary>
        /// <todoD>Needs a little tidying and checking of access levels</todoD>
        public class ThreadLockedParallelVariables
        {
            /// <summary>
            /// Thread-local variables to track numbers of cohort extinctions and productions
            /// </summary>
            public int Extinctions, Productions;
        }

        /// <summary>
        /// Scalar to convert from the time step units used by this predation implementation to global model time step units
        /// </summary>
        private double _DeltaT;
        /// <summary>
        /// Get the scalar to convert from the time step units used by this predation implementation to global model time step units
        /// </summary>
        public double DeltaT { get { return _DeltaT; } }

        /// <summary>
        /// The proportion of time that a predator cohort spends eating
        /// </summary>
        private double _ProportionOfTimeEating;
        /// <summary>
        /// Get and set the proportion of time that a predator cohort spends eating
        /// </summary>
        public double ProportionTimeEating
        {
            get { return _ProportionOfTimeEating; }
            set { _ProportionOfTimeEating = value; }
        }

        /// <summary>
        /// Jagged array mirroring the grid cell cohorts to store the abundance gained from predation on each cohort
        /// </summary>
        private double[][] _AbundancesEaten;
        /// <summary>
        /// Return the jagged array mirroring the grid cell cohorts to store the abundance gained from predation on each cohort
        /// </summary>
        public double[][] AbundancesEaten
        { get { return _AbundancesEaten; } }

        /// <summary>
        /// Jagged array mirroring the grid cell cohorts to store the potential abundance gained (given the number of encounters) from predation on each cohort
        /// </summary>
        private double[][] _PotentialAbundanceEaten;
        /// <summary>
        /// Get the jagged array mirroring the grid cell cohorts to store the potential abundance gained from predation on each cohort
        /// </summary>
        public double[][] PotentialAbundanceEaten
        { get { return _PotentialAbundanceEaten; } }

        /// <summary>
        /// List of cohort functional group indices ot be eaten in predation
        /// </summary>
        private int[] _FunctionalGroupIndicesToEat;
        /// <summary>
        /// Get the list of cohort functional group indices ot be eaten in predation
        /// </summary>
        public int[] FunctionalGroupIndicesToEat
        { get { return _FunctionalGroupIndicesToEat; } }

        /// <summary>
        ///The total biomass eaten by the acting cohort 
        /// </summary>
        private double _TotalBiomassEatenByCohort;
        /// <summary>
        /// Get the total biomass eaten by the acting cohort
        /// </summary>
        public double TotalBiomassEatenByCohort
        { get { return _TotalBiomassEatenByCohort; } }

        /// <summary>
        /// Cumulative number of time units to handle all of the potential kills from all cohorts
        /// </summary>
        private double _TimeUnitsToHandlePotentialFoodItems;
        /// <summary>
        /// Get and set the cumulative number of time units to handle all of the potential kills from all cohorts
        /// </summary>
        public double TimeUnitsToHandlePotentialFoodItems
        {
            get { return _TimeUnitsToHandlePotentialFoodItems; }
            set { _TimeUnitsToHandlePotentialFoodItems = value; }

        }

        // Cell area for the cell within which this predation object is instantiated. Extracted once to speed up calculations.
        /// <summary>
        /// The area (in square km) of the grid cell
        /// </summary>
        private double _CellArea;
        /// <summary>
        /// Get and set the area of the grid cell
        /// </summary>
        public double CellArea 
        { 
            get { return _CellArea; } 
            set { _CellArea = value; } 
        }

        /// <summary>
        /// The area of the current grid cell in hectares
        /// </summary>
        private double _CellAreaHectares;
        /// <summary>
        /// Get and set the area of the current grid cell in hectares
        /// </summary>
        public double CellAreaHectares
        {
            get { return _CellAreaHectares; }
            set { _CellAreaHectares = value; }
        }

        /// <summary>
        /// The proportion of biomass eaten assimilated by predators
        /// </summary>
        private double _PredatorAssimilationEfficiency;
        
        /// <summary>
        /// The proportion of biomass eaten not assimilated by predators
        /// </summary>
        private double _PredatorNonAssimilation;

        /// <summary>
        /// Boolean to indicate if the diet of marine species is "allspecial"
        /// </summary>
        private Boolean _DietIsAllSpecial;

        /// <summary>
        /// Double to hold the log optimal prey body size ratio for the acting predator cohort
        /// </summary>
        private double _PredatorLogOptimalPreyBodySizeRatio;

        /// <summary>
        /// Individual body mass of the prey cohort
        /// </summary>
        private double _BodyMassPrey;
       
        /// <summary>
        /// Individual body mass of the acting (predator) cohort
        /// </summary>
        private double _BodyMassPredator;

        /// <summary>
        /// The logarithm of the predator boy mass plus the logarithm of the optimal prey body size ratio
        /// </summary>
        double LogPredatorMassPlusPredatorLogOptimalPreyBodySizeRatio;

        /// <summary>
        /// Abundance of the acting (predator) cohort
        /// </summary>
        private double _AbundancePredator;

        private double _ReferenceMassRatioScalingTerrestrial;
        private double _ReferenceMassRatioScalingMarine;

        private double _PredatorAbundanceMultipliedByTimeEating;

        /// <summary>
        /// Identifies which functional groups are carnivores
        /// </summary>
        private Boolean[] _CarnivoreFunctionalGroups;

        private Boolean[] _OmnivoreFunctionalGroups;

        private Boolean[] _PlanktonFunctionalGroups;

        /// <summary>
        /// A boolean which monitors whether or not to track individual cohorts
        /// </summary>
        private Boolean Track;
        /// <summary>
        /// Number of cohorts in each functional group that were present in the grid cell before this time step's new cohorts were created
        /// </summary>
        private int[] NumberCohortsPerFunctionalGroupNoNewCohorts;

        // The matrix to hold the abundance of prey items in each functional group and 
        // weight bin
        private double[,] BinnedPreyDensities;

        // The number of bins in which to combine prey cohorts
        // THIS SHOULD ALWAYS BE AN EVEN VALUE
        private int NumberOfBins;
        private int HalfNumberOfBins;

        // The mass bin number of an individual prey cohort
        private int PreyMassBinNumber;

        // Temporary value to hold calculations
        private double TempDouble;

        /// <summary>
        /// Instance of the class to perform general functions
        /// </summary>
        private UtilityFunctions Utilities;

        /// <summary>
        /// An instance of the simple random number generator class
        /// </summary>
        private NonStaticSimpleRNG RandomNumberGenerator = new NonStaticSimpleRNG();


        /// <summary>
        /// Constructor for predation: assigns all parameter values
        /// </summary>
        /// <param name="cellArea">The area (in square km) of the grid cell</param>
        /// <param name="globalModelTimeStepUnit">The time step unit used in the model</param>
        public RevisedPredation(double cellArea, string globalModelTimeStepUnit)
        {

            InitialiseParametersPredation();

            // Initialise the utility functions
            Utilities = new UtilityFunctions();

            // Calculate the scalar to convert from the time step units used by this implementation of predation to the global model time step units
            _DeltaT = Utilities.ConvertTimeUnits(globalModelTimeStepUnit, _TimeUnitImplementation);

            // Store the specified cell area in this instance of this predation implementation
            _CellArea = cellArea;
            _CellAreaHectares = cellArea * 100;
            HalfNumberOfBins = NumberOfBins / 2;
            FeedingPreferenceHalfStandardDeviation = _FeedingPreferenceStandardDeviation * 0.5;
        }

        /// <summary>
        /// Initialises predation implementation each time step
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohorts in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stocks in the model</param>
        /// <remarks>This only works if: a) predation is initialised in every grid cell; and b) if parallelisation is done by latitudinal strips
        /// It is critical to run this every time step</remarks>
        public void InitializeEatingPerTimeStep(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions)
        {
            // Get the functional group indices of all heterotroph cohorts (i.e. potential prey)
            _FunctionalGroupIndicesToEat = madingleyCohortDefinitions.GetFunctionalGroupIndex("Heterotroph/Autotroph", "heterotroph", false);
                     
            // Initialise the vector to hold the number of cohorts in each functional group at the start of the time step
            NumberCohortsPerFunctionalGroupNoNewCohorts = new int[gridCellCohorts.Count];

            // Initialise the jagged arrays to hold the potential and actual numbers of prey eaten in each of the grid cell cohorts
            _AbundancesEaten = new double[gridCellCohorts.Count][];
            _PotentialAbundanceEaten = new double[gridCellCohorts.Count][];

            // Initialise the vector to identify carnivore cohorts
            _CarnivoreFunctionalGroups = new Boolean[_FunctionalGroupIndicesToEat.Length];
            _OmnivoreFunctionalGroups = new Boolean[_FunctionalGroupIndicesToEat.Length];
            _PlanktonFunctionalGroups = new Boolean[_FunctionalGroupIndicesToEat.Length];

            // Loop over rows in the jagged arrays, initialise each vector within the jagged arrays, and calculate the current number of cohorts in 
            // each functional group
            for (int i = 0; i < gridCellCohorts.Count; i++)
            {
                // Calculate the current number of cohorts in this functional group
                int NumCohortsThisFG = gridCellCohorts[i].Count;
                NumberCohortsPerFunctionalGroupNoNewCohorts[i] = NumCohortsThisFG;
                // Initialise the jagged arrays
                _AbundancesEaten[i] = new double[NumberCohortsPerFunctionalGroupNoNewCohorts[i]];
                _PotentialAbundanceEaten[i] = new double[NumberCohortsPerFunctionalGroupNoNewCohorts[i]];
            }

            // Loop over functional groups that are potential prey and determine which are carnivores
            foreach (int FunctionalGroup in FunctionalGroupIndicesToEat)
                _CarnivoreFunctionalGroups[FunctionalGroup] = madingleyCohortDefinitions.GetTraitNames("Nutrition source", FunctionalGroup) == "carnivore";

            foreach (int FunctionalGroup in FunctionalGroupIndicesToEat)
                _OmnivoreFunctionalGroups[FunctionalGroup] = madingleyCohortDefinitions.GetTraitNames("Nutrition source", FunctionalGroup) == "omnivore";

            foreach (int FunctionalGroup in FunctionalGroupIndicesToEat)
                _PlanktonFunctionalGroups[FunctionalGroup] = madingleyCohortDefinitions.GetTraitNames("Mobility", FunctionalGroup) == "planktonic";

        
        }

        /// <summary>
        /// Calculate the potential number of prey that could be gained through predation on each cohort in the grid cell
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The acting cohort</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="madingleyCohortDefinitions">The functional group definitions for cohorts in the model</param>
        /// <param name="madingleyStockDefinitions">The functional group definitions for stocks  in the model</param>
        public void GetEatingPotentialMarine(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, int[] actingCohort, 
            SortedList<string, double[]> cellEnvironment, FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions 
            madingleyStockDefinitions)
        {

            BinnedPreyDensities = new double[gridCellCohorts.Count, NumberOfBins];

            // Set the total eaten by the acting cohort to zero
            _TotalBiomassEatenByCohort = 0.0;

            // Set the total number of units to handle all potential prey individuals eaten to zero
            _TimeUnitsToHandlePotentialFoodItems = 0.0;

            // Get the individual body mass of the acting (predator) cohort
            _BodyMassPredator = gridCellCohorts[actingCohort].IndividualBodyMass;

            // Get the abundance of the acting (predator) cohort
            _AbundancePredator = gridCellCohorts[actingCohort].CohortAbundance;

            // Pre-calculate individual values for this predator to speed things up
            _SpecificPredatorKillRateConstant = _KillRateConstant * Math.Pow(_BodyMassPredator, (_KillRateConstantMassExponent));
            _SpecificPredatorTimeUnitsEatingPerGlobalTimeStep = _DeltaT * _ProportionOfTimeEating;
            _PredatorAssimilationEfficiency = _AssimilationEfficiency;
            _PredatorNonAssimilation = (1 - _AssimilationEfficiency);

            _DietIsAllSpecial = madingleyCohortDefinitions.GetTraitNames("Diet", actingCohort[0]) == "allspecial";

            _PredatorLogOptimalPreyBodySizeRatio = gridCellCohorts[actingCohort[0]][actingCohort[1]].LogOptimalPreyBodySizeRatio;

            // If a filter feeder, then optimal body size is a value not a ratio: convert it to a ratio to ensure that all calculations work correctly
            if (_DietIsAllSpecial)
            {
                // Optimal body size is actually a value, not a ratio, so convert it to a ratio based on the present body size
                _PredatorLogOptimalPreyBodySizeRatio = Math.Log(
                    Math.Exp(gridCellCohorts[actingCohort[0]][actingCohort[1]].LogOptimalPreyBodySizeRatio) / gridCellCohorts[actingCohort[0]][actingCohort[1]].IndividualBodyMass);
            }
            

            // Calculate the reference mass scaling ratio
            _ReferenceMassRatioScalingMarine = HandlingTimeScalarMarine * Math.Pow(_ReferenceMass / _BodyMassPredator, _HandlingTimeExponentMarine);
                              
            _PredatorAbundanceMultipliedByTimeEating = _AbundancePredator * _SpecificPredatorTimeUnitsEatingPerGlobalTimeStep;

            LogPredatorMassPlusPredatorLogOptimalPreyBodySizeRatio = Math.Log(_BodyMassPredator) + _PredatorLogOptimalPreyBodySizeRatio;

            // Calculate the abundance of prey in each of the prey mass bins
            PopulateBinnedPreyAbundance(gridCellCohorts, actingCohort, FunctionalGroupIndicesToEat, LogPredatorMassPlusPredatorLogOptimalPreyBodySizeRatio);

            // Loop over potential prey functional groups
            foreach (int FunctionalGroup in FunctionalGroupIndicesToEat)
            {
                // Eating operates differently for planktivores
                // This can certainly be sped up
                if (_DietIsAllSpecial)
                {
                    // Loop over cohorts within the functional group
                    for (int i = 0; i < NumberCohortsPerFunctionalGroupNoNewCohorts[FunctionalGroup]; i++)
                    {
                        // Get the body mass of individuals in this cohort
                        _BodyMassPrey = gridCellCohorts[FunctionalGroup][i].IndividualBodyMass;
 
                        // Get the bin number of this prey cohort
                        PreyMassBinNumber = GetBinNumber(_BodyMassPrey, LogPredatorMassPlusPredatorLogOptimalPreyBodySizeRatio);


                        // Check whether 
                        // The prey cohort is within the feeding range of the predator
                        // the prey cohort still exists in the model (i.e. body mass > 0)   
                        // Currently having whales etc eat everything, but preferentially feed on very small things (i.e. filter feeders)
                    if ((_PlanktonFunctionalGroups[FunctionalGroup]) && (0 < PreyMassBinNumber) &&
                        (PreyMassBinNumber < NumberOfBins) && (_BodyMassPrey > 0))
                        {
                            // Calculate the potential abundance from this cohort eaten by the acting cohort
                            _PotentialAbundanceEaten[FunctionalGroup][i] = CalculateExpectedNumberKilledMarine(
                                gridCellCohorts[FunctionalGroup][i].CohortAbundance, _BodyMassPrey, PreyMassBinNumber, FunctionalGroup,
                                _BodyMassPredator, _CarnivoreFunctionalGroups[FunctionalGroup], _OmnivoreFunctionalGroups[FunctionalGroup],
                                _OmnivoreFunctionalGroups[actingCohort[0]], _PredatorLogOptimalPreyBodySizeRatio);
                            
                            // Add the time required to handle the potential abundance eaten from this cohort to the cumulative total for all cohorts
                            _TimeUnitsToHandlePotentialFoodItems += _PotentialAbundanceEaten[FunctionalGroup][i] *
                                CalculateHandlingTimeMarine(_BodyMassPrey);
                        }
                        else
                        {
                            // Assign a potential abundance eaten of zero
                            _PotentialAbundanceEaten[FunctionalGroup][i] = 0.0;
                        }
                    }
                }
                else
                {

                    // Loop over cohorts within the functional group
                    for (int i = 0; i < NumberCohortsPerFunctionalGroupNoNewCohorts[FunctionalGroup]; i++)
                    {
                        // Get the body mass of individuals in this cohort
                        _BodyMassPrey = gridCellCohorts[FunctionalGroup][i].IndividualBodyMass;

                        // Get the bin number of this prey cohort
                        PreyMassBinNumber = GetBinNumber(_BodyMassPrey, LogPredatorMassPlusPredatorLogOptimalPreyBodySizeRatio);

                        // Check whether 
                        // The prey cohort is within the feeding range of the predator
                        // the prey cohort still exists in the model (i.e. body mass > 0)   
                        if ((0 < PreyMassBinNumber) && (PreyMassBinNumber < NumberOfBins) && (_BodyMassPrey > 0))
                        {
                            // Calculate the potential abundance from this cohort eaten by the acting cohort
                            _PotentialAbundanceEaten[FunctionalGroup][i] = CalculateExpectedNumberKilledMarine(
                               gridCellCohorts[FunctionalGroup][i].CohortAbundance, _BodyMassPrey, PreyMassBinNumber, FunctionalGroup,
                               _BodyMassPredator, _CarnivoreFunctionalGroups[FunctionalGroup], _OmnivoreFunctionalGroups[FunctionalGroup],
                               _OmnivoreFunctionalGroups[actingCohort[0]], _PredatorLogOptimalPreyBodySizeRatio);
                            
                            // Add the time required to handle the potential abundance eaten from this cohort to the cumulative total for all cohorts
                            _TimeUnitsToHandlePotentialFoodItems += _PotentialAbundanceEaten[FunctionalGroup][i] *
                                CalculateHandlingTimeMarine(_BodyMassPrey);
                        }
                        else
                        {
                            // Assign a potential abundance eaten of zero
                            _PotentialAbundanceEaten[FunctionalGroup][i] = 0.0;
                        }                  
                    }
                }

            }
                       
            // No cannibalism; do this outside the loop to speed up the calculations
            _TimeUnitsToHandlePotentialFoodItems -= PotentialAbundanceEaten[actingCohort[0]][actingCohort[1]] *
                CalculateHandlingTimeMarine(_BodyMassPredator);
            PotentialAbundanceEaten[actingCohort[0]][actingCohort[1]] = 0.0;
        }

        /// <summary>
        /// Create the matrix of prey abundances in each weight bin
        /// </summary>
        /// <param name="gridCellCohorts">Cohorts in this grid cell</param>
        /// <param name="actingCohort">The predator cohort</param>
        /// <param name="functionalGroupIndicesToEat">The functional groups which this predator eats</param>
        /// <param name="logOptimalPreyBodySizeRatio">The (log-transformed) optimal ratio of prey to predator body mass</param>
        private void PopulateBinnedPreyAbundance(GridCellCohortHandler gridCellCohorts, int[] actingCohort, int[] functionalGroupIndicesToEat,
            double logPredatorMassPlusLogPredatorOptimalBodySizeRatio)
        {
            int BinNumber = 0;

            // Loop through prey functional groups
            foreach (var fg in functionalGroupIndicesToEat)
            {
                foreach (var cohort in gridCellCohorts[fg])
                {
                    // Calculate the difference between the actual body size ratio and the optimal ratio, 
                    // and then divide by the standard deviation in log ratio space to determine in 
                    // which bin to assign the prey item.
                    BinNumber = GetBinNumber(cohort.IndividualBodyMass, logPredatorMassPlusLogPredatorOptimalBodySizeRatio);

                    if((0 < BinNumber) && (BinNumber < NumberOfBins))
                    {
                        BinnedPreyDensities[fg, BinNumber] += cohort.CohortAbundance / _CellAreaHectares;
                    }
                }
            }

        }

        // Get the bin number for a prey of a particular body mass
        private int GetBinNumber(double preyMass, double logPredatorMassPlusLogPredatorOptimalBodySizeRatio)
        
        //private int GetBinNumber(double preyMass, double logPredatorMassPlusPredatorLogOptimalPreyBodySizeRatio)
        {
            return (int)(GetBinNumberFractional(preyMass, logPredatorMassPlusLogPredatorOptimalBodySizeRatio) + HalfNumberOfBins);
       
            //return (int)(GetBinNumberFractional(preyMass, logPredatorMassPlusPredatorLogOptimalPreyBodySizeRatio) + HalfNumberOfBins);
        }

        // Get the fractional bin number for a prey of a particular body mass
        private double GetBinNumberFractional(double preyMass, double logPredatorMassPlusLogPredatorOptimalBodySizeRatio)
        
        //private double GetBinNumberFractional(double preyMass, double logPredatorMassPlusPredatorLogOptimalPreyBodySizeRatio)
        {
            //return (Math.Log(preyMass) - logPredatorMassPlusPredatorLogOptimalPreyBodySizeRatio) / FeedingPreferenceHalfStandardDeviation;
            
            // Below is the original. Have passed log values directly in order to speed it up
            return (Math.Log(preyMass) - logPredatorMassPlusLogPredatorOptimalBodySizeRatio) / 
                FeedingPreferenceHalfStandardDeviation;

        }

        /// <summary>
        /// Calculate the potential number of prey that could be gained through predation on each cohort in the grid cell
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The acting cohort</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="madingleyCohortDefinitions">The functional group definitions for cohorts in the model</param>
        /// <param name="madingleyStockDefinitions">The functional group definitions for stocks  in the model</param>
        public void GetEatingPotentialTerrestrial(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, int[] actingCohort,
            SortedList<string, double[]> cellEnvironment, FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions
            madingleyStockDefinitions)
        {

            BinnedPreyDensities = new double[gridCellCohorts.Count, NumberOfBins];

            // Set the total eaten by the acting cohort to zero
            _TotalBiomassEatenByCohort = 0.0;

            // Set the total number of units to handle all potential prey individuals eaten to zero
            _TimeUnitsToHandlePotentialFoodItems = 0.0;

            // Get the individual body mass of the acting (predator) cohort
            _BodyMassPredator = gridCellCohorts[actingCohort].IndividualBodyMass;

            // Get the abundance of the acting (predator) cohort
            _AbundancePredator = gridCellCohorts[actingCohort].CohortAbundance;

            // Pre-calculate individual values for this predator
            _SpecificPredatorKillRateConstant = _KillRateConstant * Math.Pow(_BodyMassPredator, (_KillRateConstantMassExponent));
            _SpecificPredatorTimeUnitsEatingPerGlobalTimeStep = _DeltaT * _ProportionOfTimeEating;
            _PredatorAssimilationEfficiency = _AssimilationEfficiency;
            _PredatorNonAssimilation = (1 - _AssimilationEfficiency);

            // When body sizes are less than one gram, we have a flat handling time relationship to stop small things have extraordinarily short handling times
          //  if (_BodyMassPredator > 1.0)
          //  {
                _ReferenceMassRatioScalingTerrestrial = HandlingTimeScalarTerrestrial * Math.Pow(_ReferenceMass / _BodyMassPredator, _HandlingTimeExponentTerrestrial);
          //  }
          //  else
          //  {
          //      _ReferenceMassRatioScalingTerrestrial = HandlingTimeScalarTerrestrial * _ReferenceMass / _BodyMassPredator;

//            }
                _PredatorAbundanceMultipliedByTimeEating = _AbundancePredator * _SpecificPredatorTimeUnitsEatingPerGlobalTimeStep;


                _PredatorLogOptimalPreyBodySizeRatio = gridCellCohorts[actingCohort[0]][actingCohort[1]].LogOptimalPreyBodySizeRatio;

            
            LogPredatorMassPlusPredatorLogOptimalPreyBodySizeRatio = Math.Log(_BodyMassPredator) + _PredatorLogOptimalPreyBodySizeRatio;

            // Calculate the abundance of prey in each of the prey mass bins
            PopulateBinnedPreyAbundance(gridCellCohorts, actingCohort, FunctionalGroupIndicesToEat, LogPredatorMassPlusPredatorLogOptimalPreyBodySizeRatio);

            // Loop over potential prey functional groups
            foreach (int FunctionalGroup in FunctionalGroupIndicesToEat)
            {

                // Loop over cohorts within the functional group
                for (int i = 0; i < NumberCohortsPerFunctionalGroupNoNewCohorts[FunctionalGroup]; i++)
                {
                    // Get the body mass of individuals in this cohort
                    _BodyMassPrey = gridCellCohorts[FunctionalGroup][i].IndividualBodyMass;

                    // Get the bin number of this prey cohort
                    PreyMassBinNumber = GetBinNumber(_BodyMassPrey, LogPredatorMassPlusPredatorLogOptimalPreyBodySizeRatio);

                    // Check whether the prey cohort still exists in the model (i.e. body mass > 0)            
                    if ((0 < PreyMassBinNumber) && (PreyMassBinNumber < NumberOfBins) && (_BodyMassPrey > 0))
                    {
                        // Calculate the potential abundance from this cohort eaten by the acting cohort
                        _PotentialAbundanceEaten[FunctionalGroup][i] = CalculateExpectedNumberKilledTerrestrial(
                            gridCellCohorts[FunctionalGroup][i].CohortAbundance, _BodyMassPrey, PreyMassBinNumber, FunctionalGroup,
                            _BodyMassPredator, _CarnivoreFunctionalGroups[FunctionalGroup], _OmnivoreFunctionalGroups[FunctionalGroup],
                            _OmnivoreFunctionalGroups[actingCohort[0]], _PredatorLogOptimalPreyBodySizeRatio);
                        
                        // Add the time required to handle the potential abundance eaten from this cohort to the cumulative total for all cohorts
                        _TimeUnitsToHandlePotentialFoodItems += _PotentialAbundanceEaten[FunctionalGroup][i] *
                            CalculateHandlingTimeTerrestrial(_BodyMassPrey);
                    }
                    else
                    {
                        // Assign a potential abundance eaten of zero
                        _PotentialAbundanceEaten[FunctionalGroup][i] = 0.0;
                    }
                }
            }

            // No cannibalism; do this outside the loop to speed up the calculations
            _TimeUnitsToHandlePotentialFoodItems -= PotentialAbundanceEaten[actingCohort[0]][actingCohort[1]] *
                CalculateHandlingTimeTerrestrial(_BodyMassPredator);
            PotentialAbundanceEaten[actingCohort[0]][actingCohort[1]] = 0.0;
        }


        /// <summary>
        /// Apply the changes from predation to prey cohorts, and update deltas for the predator cohort
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The acting cohort</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="madingleyCohortDefinitions">The functional group definitions for cohorts in the model</param>
        /// <param name="madingleyStockDefinitions">The functional group definitions for stocks in the model</param>
        /// <param name="trackProcesses">An instance of ProcessTracker to hold diagnostics for predation</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="specificLocations">Whether the model is being run for specific locations</param>
        /// <param name="outputDetail">The level of output detail used in this model run</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        public void RunEating(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, int[] actingCohort, SortedList<string, double[]>
            cellEnvironment, Dictionary<string, Dictionary<string, double>> deltas, FunctionalGroupDefinitions madingleyCohortDefinitions,
            FunctionalGroupDefinitions madingleyStockDefinitions, ProcessTracker trackProcesses, uint currentTimestep, Boolean specificLocations,
            string outputDetail, MadingleyModelInitialisation initialisation)
        {
            if (trackProcesses.TrackProcesses)
            {
                Track = (RandomNumberGenerator.GetUniform() > 0.975) ? true : false;
            }
            

            TempDouble = 0.0;

            // Temporary variable to hold the total time spent eating + 1. Saves an extra calculation in CalculateAbundanceEaten
            double TotalTimeUnitsToHandlePlusOne = TimeUnitsToHandlePotentialFoodItems + 1;

            // Loop over potential prey functional groups
            foreach (int FunctionalGroup in _FunctionalGroupIndicesToEat)
            {
                
                // Loop over cohorts within the functional group
                for (int i = 0; i < NumberCohortsPerFunctionalGroupNoNewCohorts[FunctionalGroup]; i++)
                {
                    // Get the individual body mass of this cohort
                    _BodyMassPrey = gridCellCohorts[FunctionalGroup][i].IndividualBodyMass;

                     // Calculate the actual abundance of prey eaten from this cohort
                    if (gridCellCohorts[FunctionalGroup][i].CohortAbundance > 0)
                    {
                     
                        
                     // Calculate the actual abundance of prey eaten from this cohort
                    _AbundancesEaten[FunctionalGroup][i] = CalculateAbundanceEaten(_PotentialAbundanceEaten[FunctionalGroup][i], _PredatorAbundanceMultipliedByTimeEating,
                        TotalTimeUnitsToHandlePlusOne, gridCellCohorts[FunctionalGroup][i].CohortAbundance);

                    }
                    else
                        _AbundancesEaten[FunctionalGroup][i] = 0;

                    // Remove number of prey eaten from the prey cohort
                    gridCellCohorts[FunctionalGroup][i].CohortAbundance -= _AbundancesEaten[FunctionalGroup][i];

                    gridCellCohorts[actingCohort].TrophicIndex += (_BodyMassPrey + gridCellCohorts[FunctionalGroup][i].IndividualReproductivePotentialMass) * _AbundancesEaten[FunctionalGroup][i] * gridCellCohorts[FunctionalGroup][i].TrophicIndex;

                    // If the process tracker is set and output detail is set to high and the prey cohort has never been merged,
                    // then track its mortality owing to predation
                    if (trackProcesses.TrackProcesses)
                    {
                        if ((outputDetail == "high") && (gridCellCohorts[FunctionalGroup][i].CohortID.Count == 1)
                        && AbundancesEaten[FunctionalGroup][i] > 0)
                        {
                            trackProcesses.RecordMortality((uint)cellEnvironment["LatIndex"][0], (uint)cellEnvironment["LonIndex"][0], gridCellCohorts
                                [FunctionalGroup][i].BirthTimeStep, currentTimestep, gridCellCohorts[FunctionalGroup][i].IndividualBodyMass,
                                gridCellCohorts[FunctionalGroup][i].AdultMass, gridCellCohorts[FunctionalGroup][i].FunctionalGroupIndex,
                                gridCellCohorts[FunctionalGroup][i].CohortID[0], AbundancesEaten[FunctionalGroup][i], "predation");
                        }

                        // If the model is being run for specific locations and if track processes has been specified, then track the mass flow between
                        // prey and predator
                        if (specificLocations)
                        {
                            trackProcesses.RecordPredationMassFlow(currentTimestep, _BodyMassPrey, _BodyMassPredator, _BodyMassPrey *
                                _AbundancesEaten[FunctionalGroup][i]);
                        
                        if (outputDetail == "high")
                            trackProcesses.TrackPredationTrophicFlow((uint)cellEnvironment["LatIndex"][0], (uint)cellEnvironment["LonIndex"][0],
                                gridCellCohorts[FunctionalGroup][i].FunctionalGroupIndex, gridCellCohorts[actingCohort].FunctionalGroupIndex,
                                madingleyCohortDefinitions, (_AbundancesEaten[FunctionalGroup][i] * _BodyMassPrey), _BodyMassPredator, _BodyMassPrey, 
                                initialisation, cellEnvironment["Realm"][0] == 2.0);

                        }

                    }


                    // Check that the abundance eaten from this cohort is not negative
                    // Commented out for the purposes of speed
                    //Debug.Assert( _AbundancesEaten[FunctionalGroup][i].CompareTo(0.0) >= 0,
                    //     "Predation negative for this prey cohort" + actingCohort);
        
                    // Create a temporary value to speed up the predation function
                    // This is equivalent to the body mass of the prey cohort including reproductive potential mass, times the abundance eaten of the prey cohort,
                    // divided by the abundance of the predator
                    TempDouble += (_BodyMassPrey + gridCellCohorts[FunctionalGroup][i].IndividualReproductivePotentialMass) * _AbundancesEaten[FunctionalGroup][i] / _AbundancePredator;


                }
            }

            

            // Add the biomass eaten and assimilated by an individual to the delta biomass for the acting (predator) cohort
            deltas["biomass"]["predation"] = TempDouble * _PredatorAssimilationEfficiency;

            // Move the biomass eaten but not assimilated by an individual into the organic matter pool
            deltas["organicpool"]["predation"] = TempDouble * _PredatorNonAssimilation * _AbundancePredator;

            // Check that the delta biomass from eating for the acting cohort is not negative
            //Debug.Assert(deltas["biomass"]["predation"] >= 0, "Predation yields negative biomass");

            // Calculate the total biomass eaten by the acting (predator) cohort
            _TotalBiomassEatenByCohort = deltas["biomass"]["predation"] * _AbundancePredator;
        }

    }

}
