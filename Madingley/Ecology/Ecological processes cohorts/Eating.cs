using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Timing;

namespace Madingley
{
    /// <summary>
    /// Performs eating
    /// </summary>
    public class Eating: IEcologicalProcessWithinGridCell
    {
        /// <summary>
        /// The available implementations of the eating process
        /// </summary>
        SortedList<string, IEatingImplementation> Implementations;

        /// <summary>
        /// Tracks the total time to handle all potential food for omnivores
        /// </summary>
        double TotalTimeToEatForOmnivores = new double();

        /// <summary>
        /// An instance of the simple random number generator class
        /// </summary>
        private NonStaticSimpleRNG RandomNumberGenerator = new NonStaticSimpleRNG();

        /// <summary>
        /// Holds the trophic index of the acting cohort from the previous timestep
        /// </summary>
        private double PreviousTrophicIndex;

        /// <summary>
        /// Constructor for Eating: fills the list of available implementations of eating
        /// </summary>
        public Eating(double cellArea, string globalModelTimeStepUnit)
        {
            // Initialize the list of eating implementations
            Implementations = new SortedList<string, IEatingImplementation>();

            // Add the revised herbivory implementation to the list of implementations
            RevisedHerbivory RevisedHerbivoryImplementation = new RevisedHerbivory(cellArea, globalModelTimeStepUnit);
            Implementations.Add("revised herbivory", RevisedHerbivoryImplementation);

            //Add the revised predation implementation to the list of implementations
            RevisedPredation RevisedPredationImplementation = new RevisedPredation(cellArea, globalModelTimeStepUnit);
            Implementations.Add("revised predation", RevisedPredationImplementation);
        }

        /// <summary>
        /// Initializes an implementation of eating
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock functional groups in the model</param>
        /// <param name="implementationKey">The name of the implementation of eating to initialize</param>
        /// <remarks>Eating needs to be initialized every time step</remarks>
        public void InitializeEcologicalProcess(GridCellCohortHandler gridCellCohorts, GridCellStockHandler gridCellStocks, 
            FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, 
            string implementationKey)
        {
            // Initialize the implementation of the eating process
            Implementations[implementationKey].InitializeEatingPerTimeStep(gridCellCohorts, gridCellStocks, 
                madingleyCohortDefinitions, madingleyStockDefinitions);
        }

        /// <summary>
        /// Run eating
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="madingleyCohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="madingleyStockDefinitions">The definitions for stock functional groups in the model</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="trackProcesses">An instance of ProcessTracker to hold diagnostics for eating</param>
        /// <param name="partial">Thread-locked variables</param>
        /// <param name="specificLocations">Whether the model is being run for specific locations</param>
        /// <param name="outputDetail">The level of output detail being used for the current model run</param>
        /// <param name="currentMonth">The current model month</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        public void RunEcologicalProcess(GridCellCohortHandler gridCellCohorts, 
            GridCellStockHandler gridCellStocks, int[] actingCohort, 
            SortedList<string, double[]> cellEnvironment, 
            Dictionary<string, Dictionary<string, double>> deltas, 
            FunctionalGroupDefinitions madingleyCohortDefinitions, 
            FunctionalGroupDefinitions madingleyStockDefinitions, 
            uint currentTimestep, ProcessTracker trackProcesses, 
            ref ThreadLockedParallelVariables partial, Boolean specificLocations,
            string outputDetail, uint currentMonth, MadingleyModelInitialisation initialisation)
        {

            PreviousTrophicIndex = gridCellCohorts[actingCohort].TrophicIndex;
            //Reset this cohort's trohic index ready for calculation across its feeding this timetsstep
            gridCellCohorts[actingCohort].TrophicIndex = 0.0;

            // Get the nutrition source (herbivory, carnivory or omnivory) of the acting cohort
            string NutritionSource = madingleyCohortDefinitions.GetTraitNames("Nutrition source", gridCellCohorts[actingCohort].FunctionalGroupIndex);
            
            // Switch to the appropriate eating process(es) given the cohort's nutrition source
            switch (NutritionSource)
            {
                case "herbivore":
                  
                    // Get the assimilation efficiency for herbivory for this cohort from the functional group definitions
                    Implementations["revised herbivory"].AssimilationEfficiency = 
                        madingleyCohortDefinitions.GetBiologicalPropertyOneFunctionalGroup
                        ("herbivory assimilation", gridCellCohorts[actingCohort].FunctionalGroupIndex);

                    // Get the proportion of time spent eating for this cohort from the functional group definitions
                    Implementations["revised herbivory"].ProportionTimeEating = gridCellCohorts[actingCohort].ProportionTimeActive;

                    // Calculate the potential biomass available from herbivory
                    if (cellEnvironment["Realm"][0] == 2.0)
                        Implementations["revised herbivory"].GetEatingPotentialMarine
                        (gridCellCohorts, gridCellStocks, actingCohort, 
                        cellEnvironment, madingleyCohortDefinitions, madingleyStockDefinitions);
                    else

                        Implementations["revised herbivory"].GetEatingPotentialTerrestrial
                        (gridCellCohorts, gridCellStocks, actingCohort,
                        cellEnvironment, madingleyCohortDefinitions, madingleyStockDefinitions);

                    // Run herbivory to apply changes in autotroph biomass from herbivory and add biomass eaten to the delta arrays
                    Implementations["revised herbivory"].RunEating
                        (gridCellCohorts, gridCellStocks, actingCohort, 
                        cellEnvironment, deltas, madingleyCohortDefinitions, 
                        madingleyStockDefinitions, trackProcesses, 
                        currentTimestep, specificLocations,outputDetail, initialisation);

                    break;

                case "carnivore":

                    // Get the assimilation efficiency for predation for this cohort from the functional group definitions
                    Implementations["revised predation"].AssimilationEfficiency = 
                        madingleyCohortDefinitions.GetBiologicalPropertyOneFunctionalGroup
                        ("carnivory assimilation", gridCellCohorts[actingCohort].FunctionalGroupIndex);

                    Implementations["revised predation"].ProportionTimeEating = gridCellCohorts[actingCohort].ProportionTimeActive;

                    // Calculate the potential biomass available from predation
                    if (cellEnvironment["Realm"][0] == 2.0) 
                        Implementations["revised predation"].GetEatingPotentialMarine
                        (gridCellCohorts, gridCellStocks, actingCohort, 
                        cellEnvironment, madingleyCohortDefinitions, madingleyStockDefinitions);
                    else
                        Implementations["revised predation"].GetEatingPotentialTerrestrial
                        (gridCellCohorts, gridCellStocks, actingCohort,
                        cellEnvironment, madingleyCohortDefinitions, madingleyStockDefinitions);
                    // Run predation to apply changes in prey biomass from predation and add biomass eaten to the delta arrays
                    Implementations["revised predation"].RunEating
                        (gridCellCohorts, gridCellStocks, actingCohort, cellEnvironment, deltas, 
                        madingleyCohortDefinitions, madingleyStockDefinitions, trackProcesses, 
                        currentTimestep, specificLocations,outputDetail, initialisation);


                    break;

                case "omnivore":

                    // Get the assimilation efficiency for predation for this cohort from the functional group definitions 
                    Implementations["revised predation"].AssimilationEfficiency = 
                        madingleyCohortDefinitions.GetBiologicalPropertyOneFunctionalGroup
                        ("carnivory assimilation", gridCellCohorts[actingCohort].FunctionalGroupIndex);

                    // Get the assimilation efficiency for herbivory for this cohort from the functional group definitions
                    Implementations["revised herbivory"].AssimilationEfficiency = 
                        madingleyCohortDefinitions.GetBiologicalPropertyOneFunctionalGroup
                        ("herbivory assimilation", gridCellCohorts[actingCohort].FunctionalGroupIndex);

                    // Get the proportion of time spent eating and assign to both the herbivory and predation implementations
                    double ProportionTimeEating = gridCellCohorts[actingCohort].ProportionTimeActive;
                    Implementations["revised predation"].ProportionTimeEating = ProportionTimeEating;
                    Implementations["revised herbivory"].ProportionTimeEating = ProportionTimeEating;

                    // Calculate the potential biomass available from herbivory
                    if (cellEnvironment["Realm"][0] == 2.0) 
                        Implementations["revised herbivory"].GetEatingPotentialMarine
                        (gridCellCohorts, gridCellStocks, actingCohort, 
                        cellEnvironment, madingleyCohortDefinitions, 
                        madingleyStockDefinitions);
                    else
                        Implementations["revised herbivory"].GetEatingPotentialTerrestrial
                        (gridCellCohorts, gridCellStocks, actingCohort,
                        cellEnvironment, madingleyCohortDefinitions,
                        madingleyStockDefinitions);

                    // Calculate the potential biomass available from predation
                    if (cellEnvironment["Realm"][0] == 2.0) 
                        Implementations["revised predation"].GetEatingPotentialMarine
                        (gridCellCohorts, gridCellStocks, actingCohort, 
                        cellEnvironment, madingleyCohortDefinitions, 
                        madingleyStockDefinitions);
                    else
                        Implementations["revised predation"].GetEatingPotentialTerrestrial
                        (gridCellCohorts, gridCellStocks, actingCohort,
                        cellEnvironment, madingleyCohortDefinitions,
                        madingleyStockDefinitions);

                    // Calculate the total handling time for all expected kills from predation and expected plant matter eaten in herbivory
                    TotalTimeToEatForOmnivores = 
                        Implementations["revised herbivory"].TimeUnitsToHandlePotentialFoodItems + 
                        Implementations["revised predation"].TimeUnitsToHandlePotentialFoodItems;

                    // Assign this total time to the relevant variables in both herbviory and predation, so that actual amounts eaten are calculated correctly
                    Implementations["revised herbivory"].TimeUnitsToHandlePotentialFoodItems = TotalTimeToEatForOmnivores;
                    Implementations["revised predation"].TimeUnitsToHandlePotentialFoodItems = TotalTimeToEatForOmnivores;

                    // Run predation to update prey cohorts and delta biomasses for the acting cohort
                    Implementations["revised predation"].RunEating
                        (gridCellCohorts, gridCellStocks, actingCohort, 
                        cellEnvironment, deltas, madingleyCohortDefinitions, 
                        madingleyStockDefinitions, trackProcesses, 
                        currentTimestep, specificLocations,outputDetail, initialisation);

                    // Run herbivory to update autotroph biomass and delta biomasses for the acting cohort
                    Implementations["revised herbivory"].RunEating
                        (gridCellCohorts, gridCellStocks, actingCohort, 
                        cellEnvironment, deltas, madingleyCohortDefinitions, 
                        madingleyStockDefinitions, trackProcesses, 
                        currentTimestep, specificLocations,outputDetail, initialisation);

                    break;

                default:

                    // For nutrition source that are not supported, throw an error
                    Debug.Fail("The model currently does not contain an eating model for nutrition source:" + NutritionSource);

                    break;

            }

            // Check that the biomasses from predation and herbivory in the deltas is a number
            Debug.Assert(!double.IsNaN(deltas["biomass"]["predation"]), "BiomassFromEating is NaN");
            Debug.Assert(!double.IsNaN(deltas["biomass"]["herbivory"]), "BiomassFromEating is NaN");

            double biomassEaten = 0.0;
            if (madingleyCohortDefinitions.GetBiologicalPropertyOneFunctionalGroup("carnivory assimilation",
                gridCellCohorts[actingCohort].FunctionalGroupIndex) > 0)
            {
                biomassEaten += (deltas["biomass"]["predation"] / madingleyCohortDefinitions.GetBiologicalPropertyOneFunctionalGroup("carnivory assimilation",
                gridCellCohorts[actingCohort].FunctionalGroupIndex));
            }
            if (madingleyCohortDefinitions.GetBiologicalPropertyOneFunctionalGroup("herbivory assimilation",
                gridCellCohorts[actingCohort].FunctionalGroupIndex) > 0)
            {
                biomassEaten += (deltas["biomass"]["herbivory"]/madingleyCohortDefinitions.GetBiologicalPropertyOneFunctionalGroup("herbivory assimilation",
                gridCellCohorts[actingCohort].FunctionalGroupIndex));
            }

            if (biomassEaten > 0.0)
            {
                gridCellCohorts[actingCohort].TrophicIndex = 1 +
                    (gridCellCohorts[actingCohort].TrophicIndex / (biomassEaten * gridCellCohorts[actingCohort].CohortAbundance));
            }
            else
            {
                gridCellCohorts[actingCohort].TrophicIndex = PreviousTrophicIndex;
            }


        }


    }
}
