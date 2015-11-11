using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// Class for applying changes from the ecological processes to the properties of the acting cohort and to the environment
    /// </summary>
    public class ApplyEcology
    {
        /// <summary>
        /// Apply all updates from the ecological processes to the properties of the acting cohort and to the environment
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="actingCohort">The location of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="cellEnvironment">The environment in the current gird cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="tracker">A process tracker</param>
        public void UpdateAllEcology(GridCellCohortHandler gridCellCohorts, int[] actingCohort, SortedList<string, double[]> cellEnvironment, Dictionary<string, Dictionary<string, double>> 
            deltas, uint currentTimestep, ProcessTracker tracker)
        {
           // Apply cohort abundance changes
            UpdateAbundance(gridCellCohorts, actingCohort, deltas); 
            // Apply cohort biomass changes
            UpdateBiomass(gridCellCohorts, actingCohort, deltas, currentTimestep, tracker, cellEnvironment);
            // Apply changes to the environmental biomass pools
            UpdatePools(cellEnvironment, deltas);
        }

        /// <summary>
        /// Update the abundance of the acting cohort according to the delta abundances from the ecological processes
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="actingCohort">The location of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        private void UpdateAbundance(GridCellCohortHandler gridCellCohorts, int[] actingCohort, Dictionary<string,Dictionary<string, double>> deltas)
        {
            // Extract the abundance deltas from the sorted list of all deltas
            Dictionary<string, double> deltaAbundance = deltas["abundance"];
            
            // Get all keys from the abundance deltas sorted list
            string[] KeyStrings = deltaAbundance.Keys.ToArray();
            
            // Variable to calculate net abundance change to check that cohort abundance will not become negative
            double NetAbundanceChange = 0.0;

            // Loop over all abundance deltas
            foreach (var key in KeyStrings)
            {
                // Update net abundance change
                NetAbundanceChange += deltaAbundance[key];
            }
            // Check that cohort abundance will not become negative
            Debug.Assert((gridCellCohorts[actingCohort].CohortAbundance + NetAbundanceChange).CompareTo(0.0) >= 0, "Cohort abundance < 0");

            //Loop over all keys in the abundance deltas sorted list
            foreach (var key in KeyStrings)
            {
                // Update the abundance of the acting cohort
                gridCellCohorts[actingCohort].CohortAbundance += deltaAbundance[key];
                // Reset the current delta abundance to zero
                deltaAbundance[key] = 0.0;
            }

        }

        /// <summary>
        /// Update the individual and reproductive body masses of the acting cohort according to the delta biomasses from the ecological processes
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="tracker">A process tracker</param>
        /// <param name="cellEnvironment">The cell environment</param>
        private void UpdateBiomass(GridCellCohortHandler gridCellCohorts, int[] actingCohort, Dictionary<string,Dictionary<string, double>> deltas, 
            uint currentTimestep, ProcessTracker tracker, SortedList<string,double[]> cellEnvironment)
        {
            // Extract the biomass deltas from the sorted list of all deltas
            Dictionary<string, double> deltaBiomass = deltas["biomass"];

            if (tracker.TrackProcesses)
            {
                // Calculate net growth of individuals in this cohort
                double growth = deltaBiomass["predation"] + deltaBiomass["herbivory"] + deltaBiomass["metabolism"];
                tracker.TrackTimestepGrowth((uint)cellEnvironment["LatIndex"][0], (uint)cellEnvironment["LonIndex"][0], currentTimestep,
                    gridCellCohorts[actingCohort].IndividualBodyMass, gridCellCohorts[actingCohort].FunctionalGroupIndex, growth, deltaBiomass["metabolism"],deltaBiomass["predation"],deltaBiomass["herbivory"]);
                  
            }

            // Get all keys from the biomass deltas sorted list
            string[] KeyStrings = deltaBiomass.Keys.ToArray();

            // Variable to calculate net biomass change to check that cohort individual body mass will not become negative
            double NetBiomass = 0.0;

            // Loop over all biomass deltas
            foreach (string key in KeyStrings)
            {
                // Update net biomass change
                NetBiomass += deltaBiomass[key];
            }

            double BiomassCheck=0.0;
            Boolean NetToBeApplied = true;
            // If cohort abundance is greater than zero, then check that the calculated net biomas will not make individual body mass become negative
            if (gridCellCohorts[actingCohort].CohortAbundance.CompareTo(0.0) > 0)
            {
                string output = "Biomass going negative, acting cohort: " + actingCohort[0].ToString() + ", " + actingCohort[1].ToString();
                BiomassCheck = gridCellCohorts[actingCohort].IndividualBodyMass + NetBiomass;
                Debug.Assert((BiomassCheck).CompareTo(0.0) >= 0, output);
            }

            //Loop over all keys in the abundance deltas sorted list
            foreach (string key in KeyStrings)
            {
                // If cohort abundance is zero, then set cohort individual body mass to zero and reset the biomass delta to zero, 
                // otherwise update cohort individual body mass and reset the biomass delta to zero
                if (gridCellCohorts[actingCohort].CohortAbundance.CompareTo(0.0) == 0)
                {
                    gridCellCohorts[actingCohort].IndividualBodyMass = 0.0;
                    deltaBiomass[key] = 0.0;
                }
                else
                {
                    if (NetToBeApplied)
                    {
                        gridCellCohorts[actingCohort].IndividualBodyMass += NetBiomass;
                        NetToBeApplied = false;
                    }

                    //gridCellCohorts[actingCohort].IndividualBodyMass += deltaBiomass[key];
                    deltaBiomass[key] = 0.0;
                }
            }

            // Check that individual body mass is still greater than zero
            Debug.Assert(gridCellCohorts[actingCohort].IndividualBodyMass.CompareTo(0.0) >= 0, "biomass < 0");

            // If the current individual body mass is the largest that has been achieved by this cohort, then update the maximum achieved
            // body mass tracking variable for the cohort
            if (gridCellCohorts[actingCohort].IndividualBodyMass > gridCellCohorts[actingCohort].MaximumAchievedBodyMass)
                gridCellCohorts[actingCohort].MaximumAchievedBodyMass = gridCellCohorts[actingCohort].IndividualBodyMass;

            // Extract the reproductive biomass deltas from the sorted list of all deltas
            Dictionary<string, double> deltaReproductiveBiomass = deltas["reproductivebiomass"];

            // Get all keys from the biomass deltas sorted list
            string[] KeyStrings2 = deltas["reproductivebiomass"].Keys.ToArray();

            // Variable to calculate net reproductive biomass change to check that cohort individual body mass will not become negative
            double NetReproductiveBiomass = 0.0;

            // Loop over all reproductive biomass deltas
            foreach (string key in KeyStrings2)
            {
                // Update net reproductive biomass change
                NetReproductiveBiomass += deltaReproductiveBiomass[key];
            }

            //Loop over all keys in the abundance deltas sorted list
            foreach (string key in KeyStrings2)
            {
                // If cohort abundance is zero, then set cohort reproductive body mass to zero and reset the biomass delta to zero, 
                // otherwise update cohort reproductive body mass and reset the biomass delta to zero
                if (gridCellCohorts[actingCohort].CohortAbundance.CompareTo(0.0) == 0)
                {
                    gridCellCohorts[actingCohort].IndividualReproductivePotentialMass = 0.0;
                    deltaReproductiveBiomass[key] = 0.0;
                }
                else
                {
                    gridCellCohorts[actingCohort].IndividualReproductivePotentialMass += deltaReproductiveBiomass[key];
                    deltaReproductiveBiomass[key] = 0.0;
                }
            }
            
            // Note that maturity time step is set in TReproductionBasic

        }



        /// <summary>
        /// Update the organic and respiratory biomass pools according to the relevant deltas from the ecological processes
        /// </summary>
        /// <param name="cellEnvironment">The environment of the current gird cell</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        private void UpdatePools(SortedList<string, double[]> cellEnvironment, Dictionary<string,Dictionary<string,double>> deltas)
        {
            // Extract the organic pool deltas from the sorted list of all deltas
            Dictionary<string, double> DeltaOrganicPool = deltas["organicpool"];

            // Get all the keys from the oragnic pool deltas sorted list
            string[] KeyStrings = DeltaOrganicPool.Keys.ToArray();

            // Loop over all keys in the organic pool deltas sorted list
            foreach (var key in KeyStrings)
            {
                // Check that the delta value is not negative
                //Debug.Assert(DeltaOrganicPool[key] >= 0.0, "A delta value for the organic pool is negative");
                // Update the organic pool biomass
                cellEnvironment["Organic Pool"][0] += DeltaOrganicPool[key];
                //Reset the delta value to zero
                DeltaOrganicPool[key] = 0.0;

            }

            // Extract the respiratory pool deltas from the sorted list of all deltas
            Dictionary<string, double> DeltaRespiratoryPool = deltas["respiratoryCO2pool"];

            // Get all the keys from the respiratory pool deltas sorted list
            KeyStrings = DeltaRespiratoryPool.Keys.ToArray();

            // Loop over all keys in the respiratory pool deltas sorted list
            foreach (var key in KeyStrings)
            {
                // Check that the delta value is not negative
                Debug.Assert(DeltaRespiratoryPool[key] >= 0.0, "A delta value for the respiratory CO2 pool is negative");
                // Update the respiratory CO2 pool
                cellEnvironment["Respiratory CO2 Pool"][0] += DeltaRespiratoryPool[key];
                // Reset the delta value to zero
                DeltaRespiratoryPool[key] = 0.0;

            }
        }

    }
}
