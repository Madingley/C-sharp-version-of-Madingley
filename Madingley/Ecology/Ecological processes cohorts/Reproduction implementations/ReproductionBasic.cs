using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Madingley
{
    /// <summary>
    /// A formulation of the process of reproduction
    /// </summary>
    public partial class ReproductionBasic: IReproductionImplementation
    {
        #region Declare fields and properties

        /// <summary>
        /// The time units associated with this implementation of reproduction
        /// </summary>
        private string _TimeUnitImplementation;
        /// <summary>
        /// Get the time units associated with this implementation of reproduction
        /// </summary>
        public string TimeUnitImplementation { get { return _TimeUnitImplementation; } }
           
        /// <summary>
        /// The per individual ratio of (adult body mass + reproductive potential mass) to adult body mass above which reproduction is possible
        /// </summary>
        private double _MassRatioThreshold;
        /// <summary>
        /// Get the per individual ratio of (adult body mass + reproductive potential mass) to adult body mass above which reproduction is possible
        /// </summary>
        public double MassRatioThreshold { get { return _MassRatioThreshold; } }

        /// <summary>
        /// The probability that random draws above which result in offspring cohorts with 
        /// evolved juvenile and adult masses
        /// </summary>
        private double _MassEvolutionProbabilityThreshold;
        /// <summary>
        /// Get the probability threshold for evolution of juvenuile and adult masses of offspring cohorts
        /// </summary>
        public double MassEvolutionProbabilityThreshold { get {return _MassEvolutionProbabilityThreshold; } }

        /// <summary>
        /// The standard deviation around the parent cohort's adult and juvenile masses to apply when drawing offspring
        /// adult and juvenile masses (when mass evolution occurs)
        /// </summary>
        private double _MassEvolutionStandardDeviation;
        /// <summary>
        /// Get the standard deviation to apply to offspring cohort masses around the parent cohort's masses
        /// </summary>
        public double MassEvolutionStandardDeviation { get { return _MassEvolutionStandardDeviation; } }

        // The proportion of adult (non-reproductive) biomass allocated to offspring by semelparous organisms
        /// <summary>
        /// The proportion of adult (non-reproductive) biomass allocated to offspring during a reproductive event by semelparous organisms
        /// </summary>
        private double _SemelparityAdultMassAllocation;
        /// <summary>
        /// Get the proportion of adult biomass allocated to offspring by semelparous organisms
        /// </summary>
        public double SemelparityAdultMassAllocation { get { return _SemelparityAdultMassAllocation; } }

        #endregion

        public void InitialiseReproductionParameters()
        {
            _TimeUnitImplementation = 
            EcologicalParameters.TimeUnits[(int)EcologicalParameters.Parameters["Reproduction.Basic.TimeUnitImplementation"]];
            _MassEvolutionProbabilityThreshold = EcologicalParameters.Parameters["Reproduction.Basic.MassEvolutionProbabilityThreshold"];
            _MassRatioThreshold = EcologicalParameters.Parameters["Reproduction.Basic.MassRatioThreshold"];
            _MassEvolutionStandardDeviation = EcologicalParameters.Parameters["Reproduction.Basic.MassEvolutionStandardDeviation"];
            _SemelparityAdultMassAllocation = EcologicalParameters.Parameters["Reproduction.Basic.SemelparityAdultMassAllocation"];
        }

        /// <summary>
        /// Write out the values of the parameters to an output file
        /// </summary>
        /// <param name="sw">A streamwriter object to write the parameter values to</param>
        public void WriteOutParameterValues(StreamWriter sw)
        {   
            // Write out parameters
            sw.WriteLine("Reproduction\tTimeUnitImplementation\t" + Convert.ToString(_TimeUnitImplementation));
            sw.WriteLine("Reproduction\tMassRatioThreshold\t" + Convert.ToString(_MassRatioThreshold));
            sw.WriteLine("Reproduction\tMassEvolutionProbability\t" + Convert.ToString(_MassEvolutionProbabilityThreshold));
            sw.WriteLine("Reproduction\tMassEvolutionStandardDeviation\t" + Convert.ToString(_MassEvolutionStandardDeviation));
            sw.WriteLine("Reproduction\tSemelparityAdultMassAllocation\t" + Convert.ToString(_SemelparityAdultMassAllocation));
        }

        /// <summary>
        /// Assign the juvenile and adult masses of the new cohort to produce
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <param name="actingCohort">The position of the acting cohort in the jagged array of grid cell cohorts</param>
        /// <param name="madingleyCohortDefinitions">The definitions of cohort functional groups in the model</param>
        /// <returns>A vector containing the juvenile and adult masses of the cohort to be produced</returns>
        private double[] GetOffspringCohortProperties(GridCellCohortHandler gridCellCohorts, int[] actingCohort, FunctionalGroupDefinitions madingleyCohortDefinitions)
        {
                    // A two-element vector holding adult and juvenile body masses in elements zero and one respectively
         double[] _CohortJuvenileAdultMasses = new double[2];

            // Determine whether offspring cohort 'evolves' in terms of adult and juvenile body masses
            if (RandomNumberGenerator.GetUniform() > _MassEvolutionProbabilityThreshold)
            {
                // Determine the new juvenile body mass
                _CohortJuvenileAdultMasses[0] = Math.Max(RandomNumberGenerator.GetNormal(gridCellCohorts[actingCohort].JuvenileMass, _MassEvolutionStandardDeviation * gridCellCohorts[actingCohort].JuvenileMass), 
                    madingleyCohortDefinitions.GetBiologicalPropertyOneFunctionalGroup("Minimum mass",actingCohort[0]));

                // Determine the new adult body mass
                _CohortJuvenileAdultMasses[1] = Math.Min(RandomNumberGenerator.GetNormal(gridCellCohorts[actingCohort].AdultMass, _MassEvolutionStandardDeviation * gridCellCohorts[actingCohort].AdultMass),
                    madingleyCohortDefinitions.GetBiologicalPropertyOneFunctionalGroup("Maximum mass", actingCohort[0]));                
            }
            // If not, it just gets the same values as the parent cohort
            else
            {
                // Assign masses to the offspring cohort that are equal to those of the parent cohort
                _CohortJuvenileAdultMasses[0] = gridCellCohorts[actingCohort].JuvenileMass;
                _CohortJuvenileAdultMasses[1] = gridCellCohorts[actingCohort].AdultMass;
            }

            // Return the vector of adult and juvenile masses
            return _CohortJuvenileAdultMasses;
        }
    }
}