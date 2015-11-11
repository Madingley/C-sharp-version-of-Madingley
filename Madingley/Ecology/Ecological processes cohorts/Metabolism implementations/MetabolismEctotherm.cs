using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Madingley
{
    /// <summary>
    /// A formulation of the metabolism process for Ectothermic organisms
    /// </summary>
    /// <remarks>Functional form Brown (2004) Metabolic Theory of Ecology.
    /// Parameters from fitted relationship in Dillon et al (2010) Global Metabolic impacts of recent climate warming, Nature
    /// Currently mass assigned to reproductive potential is not metabolised
    /// Assumes that ectothermic organisms have a body temperature equal to the ambient temperature,
    /// therefore metabolising at that ambient temperature</remarks>
    public partial class MetabolismEctotherm : IMetabolismImplementation
    {
        # region Declare properties and fields

        /// <summary>
        /// The time unit for this metabolism implementation and its parameters
        /// </summary>
        private string _TimeUnitImplementation;
        /// <summary>
        /// Get the time unit for this metabolism implementation and its parameters
        /// </summary>
        public string TimeUnitImplementation { get { return _TimeUnitImplementation; } }

        /// <summary>
        /// Exponent describing the mass-dependency of field metabolic rate
        /// </summary>
        private double _MetabolismMassExponent;

        /// <summary>
        /// Exponent describing the mass-dependency of basal metabolic rate
        /// </summary>
        private double _BasalMetabolismMassExponent;

        /// <summary>
        /// Normalization constant for field metabolic rate  (independent of mass and temperature)
        /// </summary>
        private double _NormalizationConstant;

        /// <summary>
        /// Normalization constatnt for basal metabolic rate  (independent of mass and temperature)
        /// </summary>
        private double _NormalizationConstantBMR;

        /// <summary>
        /// The activation energy of metabolism
        /// </summary>
        private double _ActivationEnergy;

        /// <summary>
        /// Boltzmann's constant
        /// </summary>
        private double _BoltzmannConstant;

        /// <summary>
        /// Scalar to convert energy in kJ to energy in grams mass
        /// </summary>
        private double _EnergyScalar;

        # endregion

        /// <summary>
        /// Initialises values for all ecological parameters for metabolism
        /// </summary>
        /// <remarks>
        /// Metabolism exponent and normalization constant calculated based on Nagy et al (1999) field metabolic rates.
        /// Use the Brown (2004) functional form and take the activation energy for metabolism from there
        /// The scalar to convert kJ to grams mass currently a very rough estimate based on the calorific values
        /// of fat, protein and carbohydrate
        /// </remarks>
        public void InitialiseMetabolismParameters()
        {
            _TimeUnitImplementation =
                EcologicalParameters.TimeUnits[(int)EcologicalParameters.Parameters["Metabolism.Ectotherm.TimeUnitImplementation"]];

            // Parameters from fitting to Nagy 1999 Field Metabolic Rates for reptiles - assumes that reptile FMR was measured with animals at their optimal temp of 30degC
            _MetabolismMassExponent = EcologicalParameters.Parameters["Metabolism.Ectotherm.MetabolismMassExponent"];
            _NormalizationConstant = EcologicalParameters.Parameters["Metabolism.Ectotherm.NormalizationConstant"];
            _ActivationEnergy = EcologicalParameters.Parameters["Metabolism.Ectotherm.ActivationEnergy"]; // includes endotherms in hibernation and torpor
            _BoltzmannConstant = EcologicalParameters.Parameters["BoltzmannConstant"];

            // BMR normalisation constant from Brown et al 2004 - original units of J/s so scale to kJ/d
            _NormalizationConstantBMR = EcologicalParameters.Parameters["Metabolism.Ectotherm.NormalizationConstantBMR"];
            _BasalMetabolismMassExponent = EcologicalParameters.Parameters["Metabolism.Ectotherm.BasalMetabolismMassExponent"];

            // Currently a very rough estimate based on calorific values of fat, protein and carbohydrate - assumes organism is metabolising mass of 1/4 protein, 1/4 carbohydrate and 1/2 fat 
            _EnergyScalar = EcologicalParameters.Parameters["Metabolism.EnergyScalar"];


            // Set the constant to convert temperature in degrees Celsius to Kelvin
            _TemperatureUnitsConvert = 273.0;

        }

        /// <summary>
        /// Write out the values of the parameters to an output file
        /// </summary>
        /// <param name="sw">A streamwriter object to write the parameter values to</param>
        public void WriteOutParameterValues(StreamWriter sw)
        {
            // Initialise the parameters
            InitialiseMetabolismParameters();

            // Write out parameters
            sw.WriteLine("Ectothermic Metabolism\tTimeUnitImplementation\t" + Convert.ToString(_TimeUnitImplementation));
            sw.WriteLine("Ectothermic Metabolism\tMetabolismMassExponent\t" + Convert.ToString(_MetabolismMassExponent));
            sw.WriteLine("Ectothermic Metabolism\tNormalizationConstant\t" + Convert.ToString(_NormalizationConstant));
            sw.WriteLine("Ectothermic Metabolism\tActivationEnergy_eV\t" + Convert.ToString(_ActivationEnergy));
            sw.WriteLine("Ectothermic Metabolism\tBoltzmannConstant_eVperK\t" + Convert.ToString(_BoltzmannConstant));
            sw.WriteLine("Ectothermic Metabolism\tEnergyScalar_kJ_to_g\t" + Convert.ToString(_EnergyScalar));
            sw.WriteLine("Ectothermic Metabolism\tNormalizationConstantBMR\t" + Convert.ToString(_NormalizationConstantBMR));
            sw.WriteLine("Ectothermic Metabolism\tBasalMetabolismMassExponent\t" + Convert.ToString(_BasalMetabolismMassExponent));
            
            

        }


        /// <summary>
        /// Calculate metabolic loss in grams for an individual
        /// </summary>
        /// <param name="individualBodyMass">The body mass of individuals in the acting cohort</param>
        /// <param name="temperature">The ambient temperature, in degrees Kelvin</param>
        /// <param name="proportionTimeActive">The proportion of time that the cohort is active for</param>
        /// <returns>The metabolic loss for an individual</returns>
        public double CalculateIndividualMetabolicRate(double individualBodyMass, double temperature, double proportionTimeActive)
        {
            // Calculate field metabolic loss in kJ
            double FieldMetabolicLosskJ = _NormalizationConstant * Math.Pow(individualBodyMass, _MetabolismMassExponent) *
                Math.Exp(-(_ActivationEnergy / (_BoltzmannConstant * temperature)));

            double BasalMetabolicLosskJ = _NormalizationConstantBMR * Math.Pow(individualBodyMass, _BasalMetabolismMassExponent) *
                Math.Exp(-(_ActivationEnergy / (_BoltzmannConstant * temperature)));

            // Return metabolic loss in grams
            return ((proportionTimeActive * FieldMetabolicLosskJ) + ((1 - proportionTimeActive) * (BasalMetabolicLosskJ))) * _EnergyScalar;
            //return FieldMetabolicLosskJ * _EnergyScalar;
        }

    }
}
