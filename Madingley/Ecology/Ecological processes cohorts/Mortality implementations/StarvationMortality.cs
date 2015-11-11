using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using System.IO;

namespace Madingley
{
    /// <summary>
    /// A formulation of the process of starvation mortality
    /// </summary>
    public partial class StarvationMortality: IMortalityImplementation
    {
        #region Define properties and fields

        /// <summary>
        /// The time unit associated with this starvation mortality implementation and its parameters
        /// </summary>
        private string _TimeUnitImplementation;
        /// <summary>
        /// Get the time unit associated with this starvation mortality implementation and its parameters
        /// </summary>
        public string TimeUnitImplementation { get { return _TimeUnitImplementation; } }

        /// <summary>
        /// The inflection point of the curve describing the relationship between body mass and mortality rate
        /// </summary>
        private double _LogisticInflectionPoint;
        /// <summary>
        /// Get the inflection point of the curve describing the relationship between body mass and mortality rate
        /// </summary>
        public double LogisticInflectionPoint { get { return _LogisticInflectionPoint; } }
        
        /// <summary>
        /// The steepness of the curve describing the relationship between body mass and mortality rate
        /// </summary>
        private double _LogisticScalingParameter;
        /// <summary>
        /// Get the steepness of the curve describing the relationship between body mass and mortality rate
        /// </summary>
        public double LogisticScalingParameter { get { return _LogisticScalingParameter; } }

        /// <summary>
        /// The asymptote of the curve describing the relationship between body mass and mortality rate
        /// </summary>
        private double _MaximumStarvationRate;
        /// <summary>
        /// Get the asymptote of the curve describing the relationship between body mass and mortality rate
        /// </summary>
        public double MaximumStarvationRate { get { return _MaximumStarvationRate; } }

        # endregion


        public void InitialiseParametersStarvationMortality()
        {
            _TimeUnitImplementation =
                EcologicalParameters.TimeUnits[(int)EcologicalParameters.Parameters["Mortality.Starvation.TimeUnitImplementation"]];
            _LogisticInflectionPoint = EcologicalParameters.Parameters["Mortality.Starvation.LogisticInflectionPoint"];
            _LogisticScalingParameter = EcologicalParameters.Parameters["Mortality.Starvation.LogisticScalingParameter"];
            _MaximumStarvationRate = EcologicalParameters.Parameters["Mortality.Starvation.MaximumStarvationRate"];
        }


        /// <summary>
        /// Write out the values of the parameters to an output file
        /// </summary>
        /// <param name="sw">A streamwriter object to write the parameter values to</param>
        public void WriteOutParameterValues(StreamWriter sw)
        {
            // Write out parameters
            sw.WriteLine("Starvation Mortality\tTimeUnitImplementation\t" + Convert.ToString(_TimeUnitImplementation));
            sw.WriteLine("Starvation Mortality\tLogisticInflectionPoint\t" + Convert.ToString(_LogisticInflectionPoint));
            sw.WriteLine("Starvation Mortality\tMaximumStarvationRate\t" + Convert.ToString(_MaximumStarvationRate));
            sw.WriteLine("Starvation Mortality\tLogisticScalingParameter\t" + Convert.ToString(_LogisticScalingParameter));
        }


        /// <summary>
        /// Calculate the proportion of individuals in a cohort that die from starvation mortality each time step
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts  in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="bodyMassIncludingChangeThisTimeStep">Body mass including change from other ecological functions this time step; should not exceed adult mass</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <returns>The proportion of individuals in the cohort that die from starvation mortality</returns>
        public double CalculateMortalityRate(GridCellCohortHandler gridCellCohorts, int[] actingCohort, double bodyMassIncludingChangeThisTimeStep, Dictionary<string, Dictionary<string, double>> deltas, uint currentTimestep)
        {
            // Calculate the starvation rate of the cohort given individual body masses compared to the maximum body
            // mass ever achieved
            double _MortalityRate = CalculateStarvationRate(gridCellCohorts, actingCohort, bodyMassIncludingChangeThisTimeStep, deltas);

            // Convert the mortality rate from formulation time step units to model time step units
            return _MortalityRate * DeltaT;
        }

        /// <summary>
        /// Calculates the rate of starvation mortality given current body mass and the maximum body mass ever achieved. Note that metabolic costs are already included in the deltas passed in
        /// the body mass including change this time step, so no change in body mass should mean no starvation (as metabolic costs have already been met)
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="bodyMassIncludingChangeThisTimeStep">Body mass including change from other ecological functions this time step; should not exceed adult mass</param>
        /// <returns>The starvation mortality rate in mortality formulation time step units</returns>
        private double CalculateStarvationRate(GridCellCohortHandler gridCellCohorts, int[] actingCohort, double bodyMassIncludingChangeThisTimeStep, Dictionary<string, Dictionary<string, double>> deltas)
        {
            if (bodyMassIncludingChangeThisTimeStep < gridCellCohorts[actingCohort].MaximumAchievedBodyMass)
            {
                // Calculate the first part of the relationship between body mass and mortality rate
                double k = -(bodyMassIncludingChangeThisTimeStep - _LogisticInflectionPoint * gridCellCohorts[actingCohort].
                    MaximumAchievedBodyMass) / (_LogisticScalingParameter * gridCellCohorts[actingCohort].MaximumAchievedBodyMass);

                // Calculate mortality rate
                return _MaximumStarvationRate / (1 + Math.Exp(-k));
            }
            else
                return 0;
        }
    }
}
