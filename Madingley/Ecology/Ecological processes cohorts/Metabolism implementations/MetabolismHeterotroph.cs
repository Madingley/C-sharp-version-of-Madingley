using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// A formulation of the metabolism process
    /// </summary>
    /// <remarks>Functional form and parameters taken from fitted relationship in Brown's (2004) Metabolic Theory of Ecology.
    /// Currently mass assigned to reproductive potential is not metabolised</remarks>
    public partial class MetabolismHeterotroph: IMetabolismImplementation
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
        /// Exponent describing the mass-dependency of metabolic rate
        /// </summary>
        private double _MetabolismMassExponent;

        /// <summary>
        /// Normalization constatnt for metabolic rate  (independent of mass and temperature)
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

        # endregion

        /// <summary>
        /// Initialises values for all ecological parameters for metabolism
        /// </summary>
        /// <remarks>Most parameters currently drawn from Brown's (2004) Metabolic Theory of Ecology
        /// The scalar to convert kJ to grams mass currently a very rough estimate based on the calorific values
        /// of fat, protein and carbohydrate</remarks>
        public void InitialiseMetabolismParameters()
        {
            _TimeUnitImplementation = "second";
            
            // Paramters from Brown's (2004) Metabolic Theory of Ecology
            _MetabolismMassExponent = 0.71;
            _NormalizationConstant = Math.Exp(20);
            _ActivationEnergy = 0.69;
            _BoltzmannConstant = 8.617e-5;

            // Currently a very rough estimate based on calorific values of fat, protein and carbohydrate
            _EnergyScalar = 1.0 / 20000.0;

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
                Math.Exp(-(_ActivationEnergy / (_BoltzmannConstant * temperature)));

            // Return metabolic loss in grams
            return MetabolicLosskJ * _EnergyScalar;

        }
    }
}
