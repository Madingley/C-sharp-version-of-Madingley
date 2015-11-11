using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Madingley
{
    /// <summary>
    /// A formulation of the process of senescence mortality
    /// </summary>
    public partial class SenescenceMortality: IMortalityImplementation
    {
        # region Define properties and fields

        /// <summary>
        /// The time unit associated with this senescence mortality implementation and its parameters
        /// </summary>
        private string _TimeUnitImplementation;
        /// <summary>
        /// Get the time unit associated with this senescence mortality implementation and its parameters
        /// </summary>
        public string TimeUnitImplementation { get { return _TimeUnitImplementation; } }

        /// <summary>
        /// Cohort senescence mortality rate scalar: the rate of individuals dying in a time step when they reach maturity
        /// </summary>
        private double _MortalityRate;
        /// <summary>
        /// Get the cohort senescence mortality rate scalar
        /// </summary>       
        public double MortalityRate { get { return _MortalityRate; } }

        # endregion


        public void InitialiseParametersSenescenceMortality()
        {
            _TimeUnitImplementation =
                EcologicalParameters.TimeUnits[(int)EcologicalParameters.Parameters["Mortality.Senescence.TimeUnitImplementation"]];
            _MortalityRate = EcologicalParameters.Parameters["Mortality.Senescence.MortalityRate"];
        }

        /// <summary>
        /// Write out the parameter values to an output file
        /// </summary>
        /// <param name="sw">A streamwriter object to write the parameter values to</param>
        public void WriteOutParameterValues(StreamWriter sw)
        {
            // Write out parameters
            sw.WriteLine("Senescence Mortality\tTimeUnitImplementation\t" + Convert.ToString(_TimeUnitImplementation));
            sw.WriteLine("Senescence Mortality\tMortalityRate\t" + Convert.ToString(_MortalityRate));
        }

        /// <summary>
        /// Calculate the rate of individuals in a cohort that die from senescence mortality in a model time step
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="bodyMassIncludingChangeThisTimeStep">The body mass of individuals in the acting cohort, including body mass change this time step through eating and mortality</param>
        /// <param name="deltas">The sorted list to track changes in biomass and abundance of the acting cohort in this grid cell</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <returns>The rate of individuals in the cohort that die from senescence mortality</returns>
        public double CalculateMortalityRate(GridCellCohortHandler gridCellCohorts, int[] actingCohort, 
            double bodyMassIncludingChangeThisTimeStep, Dictionary<string, Dictionary<string, double>> deltas, uint currentTimestep)
        {
            // Calculate the age (in model time steps) that the cohort reached maturity
            double TimeToMaturity = gridCellCohorts[actingCohort].MaturityTimeStep - gridCellCohorts[actingCohort].BirthTimeStep;
            
            // Calculate how many model time steps since the cohort reached maturity
            double AgePostMaturity = currentTimestep - gridCellCohorts[actingCohort].MaturityTimeStep;
            
            // Calculate the time since maturity as a fraction of the time that it took the cohort to reach maturity
            double FractionalAgePostMaturity = AgePostMaturity/(TimeToMaturity+1);

            // Calculate the mortality rate per mortality formulation time step as a function of the exponential of the previous fraction
            double AgeRelatedMortalityRate = _MortalityRate * Math.Exp(FractionalAgePostMaturity);

            // Convert the mortality rate from formulation time step units to model time step units
            return AgeRelatedMortalityRate * DeltaT;
        }        
    }
}
