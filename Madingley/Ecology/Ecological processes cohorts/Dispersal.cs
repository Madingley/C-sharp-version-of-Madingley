










using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// Performs dispersal
    /// </summary>
    public class Dispersal : IEcologicalProcessAcrossGridCells
    {
        /// <summary>
        /// The available implementations of the dispersal process
        /// </summary>
        private SortedList<string, IDispersalImplementation> Implementations;

        /// <summary>
        /// Threshold (g) below which a marine individual is considered to be planktonic (i.e. cannot swim against the currents). Currently set to 10mg.
        /// </summary>
        private double PlanktonThreshold;

        /// <summary>
        /// Constructor for Dispersal: fills the list of available implementations of dispersal
        /// </summary>
        public Dispersal(Boolean DrawRandomly, string globalModelTimeStepUnit, MadingleyModelInitialisation modelInitialisation)
        {
            // Initialise the list of dispersal implementations
            Implementations = new SortedList<string, IDispersalImplementation>();

            // Add the basic advective dispersal implementation to the list of implementations
            AdvectiveDispersal AdvectiveDispersalImplementation = new AdvectiveDispersal(globalModelTimeStepUnit, DrawRandomly);
            Implementations.Add("basic advective dispersal", AdvectiveDispersalImplementation);

            // Add the basic advective dispersal implementation to the list of implementations
            DiffusiveDispersal DiffusiveDispersalImplementation = new DiffusiveDispersal(globalModelTimeStepUnit, DrawRandomly);
            Implementations.Add("basic diffusive dispersal", DiffusiveDispersalImplementation);

            // Add the basic advective dispersal implementation to the list of implementations
            ResponsiveDispersal ResponsiveDispersalImplementation = new ResponsiveDispersal(globalModelTimeStepUnit, DrawRandomly);
            Implementations.Add("basic responsive dispersal", ResponsiveDispersalImplementation);

            // Get the weight threshold below which organisms are dispersed planktonically
            PlanktonThreshold = modelInitialisation.PlanktonDispersalThreshold;
        }

        /// <summary>
        /// Run dispersal
        /// </summary>
        public void RunCrossGridCellEcologicalProcess(uint[] cellIndex, ModelGrid gridForDispersal, bool dispersalOnly, FunctionalGroupDefinitions madingleyCohortDefinitions, FunctionalGroupDefinitions madingleyStockDefinitions, uint currentMonth)
        {
        
            // Create a temporary handler for grid cell cohorts
            GridCellCohortHandler WorkingGridCellCohorts;

            // Get the lat and lon indices
            uint ii = cellIndex[0];
            uint jj = cellIndex[1];

            // A boolean to check that the environmental layer exists
            bool varExists;
            
            // Check to see if the cell is marine
            double CellRealm = gridForDispersal.GetEnviroLayer("Realm", 0, ii, jj, out varExists);

            // Go through all of the cohorts in turn and see if they disperse
            WorkingGridCellCohorts = gridForDispersal.GetGridCellCohorts(ii, jj);
                    
            // Loop through cohorts, and perform dispersal according to cohort type and status
            for (int kk = 0; kk < WorkingGridCellCohorts.Count; kk++)
            {
                // Work through the list of cohorts
                for (int ll = 0; ll < WorkingGridCellCohorts[kk].Count; ll++)
                {
                    // Check to see if the cell is marine and the cohort type is planktonic
                    if (CellRealm == 2.0 && 
                        ((madingleyCohortDefinitions.GetTraitNames("Mobility", WorkingGridCellCohorts[kk][ll].FunctionalGroupIndex) == "planktonic") || (WorkingGridCellCohorts[kk][ll].IndividualBodyMass <= PlanktonThreshold)))                    
                    {   
                        // Run advective dispersal
                        Implementations["basic advective dispersal"].RunDispersal(cellIndex, gridForDispersal, WorkingGridCellCohorts[kk][ll], kk, ll, currentMonth);
                        
                    }
                    // Otherwise, if mature do responsive dispersal
                    else if (WorkingGridCellCohorts[kk][ll].MaturityTimeStep < uint.MaxValue)
                    {
                        // Run diffusive dispersal 
                        Implementations["basic responsive dispersal"].RunDispersal(cellIndex, gridForDispersal, WorkingGridCellCohorts[kk][ll], kk, ll, currentMonth);
                    }
                    // If the cohort is immature, run diffusive dispersal
                    else
                    {
                        Implementations["basic diffusive dispersal"].RunDispersal(cellIndex, gridForDispersal, WorkingGridCellCohorts[kk][ll], kk, ll, currentMonth);
                    }
                }
            }
        }
    }
}