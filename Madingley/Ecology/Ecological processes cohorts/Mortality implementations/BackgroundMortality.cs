using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using System.IO;

namespace Madingley
{
    /// <summary>
    /// A formulation of the process of background mortality, i.e. mortality from disease, accidents and other random events
    /// </summary>
    public partial class BackgroundMortality: IMortalityImplementation
    {   
        /// <summary>
        /// The time units associated with this background mortality implementation and its parameters
        /// </summary>
        private string _TimeUnitImplementation;

        /// <summary>
        /// Get the time units associated with this background mortality implementation and its parameters
        /// </summary>
        public string TimeUnitImplementation { get { return _TimeUnitImplementation; } }

        /// <summary>
        /// Cohort background mortality rate - the proportion of individuals dying in a time step
        /// </summary>
        private double _MortalityRate;
        /// <summary>
        /// Get the cohort background mortality rate
        /// </summary>
        public double MortalityRate { get { return _MortalityRate; } }


        public void InitialiseParametersBackgroundMortality()
        {
            _TimeUnitImplementation =
                EcologicalParameters.TimeUnits[(int)EcologicalParameters.Parameters["Mortality.Background.TimeUnitImplementation"]];
            _MortalityRate = EcologicalParameters.Parameters["Mortality.Background.MortalityRate"];
        }


        /// <summary>
        /// Write out the values of the parameters to an output file
        /// </summary>
        /// <param name="sw">A streamwriter object to write the parameter values to</param>
        public void WriteOutParameterValues(StreamWriter sw)
        {
            // Write out parameters
            sw.WriteLine("Background Mortality\tTimeUnitImplementation\t" + Convert.ToString(_TimeUnitImplementation));
            sw.WriteLine("Background Mortality\tMortalityRate\t" + Convert.ToString(_MortalityRate));
        }
        
        /// <summary>
        /// Calculate the rate of individuals in a cohort that die from background mortality in a model time step
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="bodyMassIncludingChangeThisTimeStep">The body mass of individuals in the acting cohort, including body mass change this time step through eating and mortality</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <returns>The rate of individuals in the cohort that die from background mortality</returns>
        public double CalculateMortalityRate(GridCellCohortHandler gridCellCohorts, int[] actingCohort, 
            double bodyMassIncludingChangeThisTimeStep, Dictionary<string, Dictionary<string, double>> deltas, uint currentTimestep)
        {
            // Convert from mortality rate per mortality formulation time step to mortality rate per model time step
            return _MortalityRate * DeltaT;
        }
    }
}
