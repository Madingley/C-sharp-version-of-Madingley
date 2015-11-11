using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Madingley
{

    /// <summary>
    /// A formulation of the metabolism process for Endothermic organisms
    /// </summary>
    /// <remarks>Functional form and parameters taken from fitted relationship in Brown's (2004) Metabolic Theory of Ecology.
    /// Currently mass assigned to reproductive potential is not metabolised
    /// Assumes that endothermic organisms metabolise at 37degC, and that they can adapt physiologicaly to do this without extra costs</remarks>
    public partial class MetabolismEndotherm : IMetabolismImplementation
    {

        /// <summary>
        /// The time unit for this metabolism implementation and its parameters
        /// </summary>
        private string _TimeUnitImplementation;
        /// <summary>
        /// Get the time unit for this metabolism implementation and its parameters
        /// </summary>
        public string TimeUnitImplementation { get { return _TimeUnitImplementation; } }

        /// <summary>
        /// Exponent describing the mass-dependency of metabolic rate
        /// </summary>
        private double _MetabolismMassExponent;

        /// <summary>
        /// Normalization constant for field metabolic rate  (independent of mass and temperature)
        /// </summary>
        private double _NormalizationConstant;

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

        /// <summary>
        /// Scalar value for endotherm body temperature
        /// </summary>
        private double _EndothermBodyTemperature;


        

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
                EcologicalParameters.TimeUnits[(int)EcologicalParameters.Parameters["Metabolism.Endotherm.TimeUnitImplementation"]];

            // Parameters from fitting to Nagy 1999 Field Metabolic Rates for mammals and birds, and assuming that these endotherms are metabolising with a body temperature of 310K (37C)
            _MetabolismMassExponent = EcologicalParameters.Parameters["Metabolism.Endotherm.MetabolismMassExponent"];
            _NormalizationConstant = EcologicalParameters.Parameters["Metabolism.Endotherm.NormalizationConstant"];
            _ActivationEnergy = EcologicalParameters.Parameters["Metabolism.Endotherm.ActivationEnergy"]; // includes endotherms in hibernation and torpor
            _BoltzmannConstant = EcologicalParameters.Parameters["BoltzmannConstant"];

            // Currently a very rough estimate based on calorific values of fat, protein and carbohydrate - assumes organism is metabolising mass of 1/4 protein, 1/4 carbohydrate and 1/2 fat 
            _EnergyScalar = EcologicalParameters.Parameters["Metabolism.EnergyScalar"];


            // Set the constant to convert temperature in degrees Celsius to Kelvin
            _TemperatureUnitsConvert = 273.0;

            // Assume all endotherms have a constant body temperature of 37degC
            _EndothermBodyTemperature = 37.0 + _TemperatureUnitsConvert;

            

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
            sw.WriteLine("Endothermic Metabolism\tTimeUnitImplementation\t" + Convert.ToString(_TimeUnitImplementation));
            sw.WriteLine("Endothermic Metabolism\tMetabolsimMassExponent\t" + Convert.ToString(_MetabolismMassExponent));
            sw.WriteLine("Endothermic Metabolism\tNormalizationConstant\t" + Convert.ToString(_NormalizationConstant));
            sw.WriteLine("Endothermic Metabolism\tActivationEnergy_eV\t" + Convert.ToString(_ActivationEnergy));
            sw.WriteLine("Endothermic Metabolism\tBoltzmannConstant_eV_per_K\t" + Convert.ToString(_BoltzmannConstant));
            sw.WriteLine("Endothermic Metabolism\tEnergyScalar_kJ_to_g\t" + Convert.ToString(_EnergyScalar));
            sw.WriteLine("Endothermic Metabolism\tBodyTemperature_K\t" + Convert.ToString(_EndothermBodyTemperature));
            

        }


        /// <summary>
        /// Calculate metabolic loss in grams for an individual
        /// </summary>
        /// <param name="individualBodyMass">The body mass of individuals in the acting cohort</param>
        /// <param name="temperature">The ambient temperature, in degrees Kelvin</param>
        /// <returns>The metabolic loss for an individual</returns>
        public double CalculateIndividualMetabolicRate(double individualBodyMass, double temperature)
        {
            // Calculate metabolic loss in kJ
            double MetabolicLosskJ = _NormalizationConstant * Math.Pow(individualBodyMass, _MetabolismMassExponent) *
                Math.Exp(-(_ActivationEnergy / (_BoltzmannConstant * _EndothermBodyTemperature)));

            // Return metabolic loss in grams
            return MetabolicLosskJ * _EnergyScalar;

        }

    }
}
