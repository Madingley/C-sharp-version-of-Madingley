using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using System.IO;

namespace Madingley
{
    /// <summary>
    /// A revised version of the predation process, written November 2011 
    /// </summary>
    public partial class RevisedPredation: IEatingImplementation
    {

        /// <summary>
        /// The time unit associated with this particular implementation of predation and its parameters
        /// </summary>
        private string _TimeUnitImplementation;
        /// <summary>
        /// Get the time unit associated with this particular implementation of predation and its parameters
        /// </summary>
        public string TimeUnitImplementation { get { return _TimeUnitImplementation; } }
       
        /// <summary>
        /// The assimilation efficiency of eaten prey mass into predator body mass
        /// </summary>
        private double _AssimilationEfficiency;
        /// <summary>
        /// Get and set the assimilation efficiency of eaten prey mass into predator body mass
        /// </summary>
        public double AssimilationEfficiency 
        { 
            get { return _AssimilationEfficiency; }
            set { _AssimilationEfficiency = value; }
        }
        
        /// <summary>
        /// The scalar of the relationship between handling time and the function of predator and prey masses for terrestrial animals
        /// </summary>
        private double _HandlingTimeScalarTerrestrial;
        /// <summary>
        /// Get the scalar of the relationship between handling time and the function of predator and prey masses
        /// </summary>
        public double HandlingTimeScalarTerrestrial { get { return _HandlingTimeScalarTerrestrial; } }

        /// <summary>
        /// The exponent applied to predator mass in the handling time relationship for terrestrial animals
        /// </summary>
        private double _HandlingTimeExponentTerrestrial;
        /// <summary>
        /// Get the exponent applied to predator mass in the handling time relationship
        /// </summary>
        public double HandlingTimeExponentTerrestrial { get { return _HandlingTimeExponentTerrestrial; } }

        /// <summary>
        /// The scalar of the relationship between handling time and the function of predator and prey masses for terrestrial animals
        /// </summary>
        private double _HandlingTimeScalarMarine;
        /// <summary>
        /// Get the scalar of the relationship between handling time and the function of predator and prey masses
        /// </summary>
        public double HandlingTimeScalarMarine  { get { return _HandlingTimeScalarMarine; } }

        /// <summary>
        /// The exponent applied to predator mass in the handling time relationship for terrestrial animals
        /// </summary>
        private double _HandlingTimeExponentMarine;
        /// <summary>
        /// Get the exponent applied to predator mass in the handling time relationship
        /// </summary>
        public double HandlingTimeExponentMarine { get { return _HandlingTimeExponentMarine; } }

        private double _ReferenceMass;
        /// <summary>
        /// Get and set the reference mass property
        /// </summary>
        public double ReferenceMass { get { return _ReferenceMass; } }
        
        /// <summary>
        /// Pre-calculate the specific predator handling time scaling to prevent having to do it for every prey cohort
        /// </summary>
        private double _SpecificPredatorHandlingTimeScaling;
        /// <summary>
        /// Get the pre-calculated specific predator handling time scaling to prevent having to do it for every prey cohort
        /// </summary>
        public double SpecificPredatorHandlingTimeScaling
        {
            get { return _SpecificPredatorHandlingTimeScaling; }
            set { _SpecificPredatorHandlingTimeScaling = value; }
        }
        
        /// <summary>
        /// The maximum kill rate for a predator of 1 g on prey of an optimal size
        /// </summary>
        private double _KillRateConstant;
        /// <summary>
        /// Get the maximum kill rate for a predator of 1 g on prey of an optimal size
        /// </summary>
        public double KillRateConstant { get { return _KillRateConstant; } }


        /// <summary>
        /// Pre-calculate the maximum kill rate for a specific predator of 1 g on prey of an optimal size
        /// </summary>
        private double _SpecificPredatorKillRateConstant;

        /// <summary>
        /// Get the pre-calculated maximum kill rate for a specific predator of 1 g on prey of an optimal size
        /// </summary>
        public double SpecificPredatorKillRateConstant
        {
            get { return _SpecificPredatorKillRateConstant; }
            set { _SpecificPredatorKillRateConstant = value; }
        }

        /// <summary>
        /// The optimal ratio of prey to predator body masses for terrestrial animals
        /// </summary>
        private double _OptimalPreyPredatorMassRatioTerrestrial;
        /// <summary>
        /// Get and set the optimal ratio of prey to predator body masses for terrestrial animals
        /// </summary>
        public double OptimalPreyPredatorMassRatioTerrestrial
        {
            get { return _OptimalPreyPredatorMassRatioTerrestrial; }
            set { _OptimalPreyPredatorMassRatioTerrestrial = value; }
        }

        /// <summary>
        /// The optimal ratio of prey to predator body masses for marine animals
        /// </summary>
        private double _OptimalPreyPredatorMassRatioMarine;
        /// <summary>
        /// Get and set the optimal ratio of prey to predator body masses for marine animals
        /// </summary>
        public double OptimalPreyPredatorMassRatioMarine
        {
            get { return _OptimalPreyPredatorMassRatioMarine; }
            set { _OptimalPreyPredatorMassRatioMarine = value; }
        }

        private double RelativeFeedingPreference;

        /// <summary>
        /// Pre-calculate the proportion of time spent eating (in appropriate time units for this class) for a specific predator
        /// </summary>
        private double _SpecificPredatorTimeUnitsEatingPerGlobalTimeStep;

        /// <summary>
        /// Get the pre-calculated proportion of time spent eating (in appropriate time units for this class) for a specific predator
        /// </summary>
        public double SpecificPredatorTimeUnitsEatingPerGlobalTimeStep
        {
            get { return _SpecificPredatorTimeUnitsEatingPerGlobalTimeStep; }
            set { _SpecificPredatorTimeUnitsEatingPerGlobalTimeStep = value; }
        }
        

        /// <summary>
        /// The standard deviation in attack rates around the optimal prey to predator mass ratio
        /// </summary>
        private double _FeedingPreferenceStandardDeviation;
        
        /// <summary>
        /// Get the standard deviation in attack rates around the optimal prey to predator mass ratio
        /// </summary>
        public double FeedingPreferenceStandardDeviation { get { return _FeedingPreferenceStandardDeviation; } }
        private double FeedingPreferenceHalfStandardDeviation;
        
        // Killing rate of an individual predator per unit prey density per hectare per time unit
        double Alphaij;

        //Variable to hold the instantaneous fraction of the prey cohort that is eaten
        //double InstantFractionKilled;

        // Fraction of of the prey cohort remaining given the proportion of time that the predator cohort spends eating
        //double FractionRemaining;

        /// <summary>
        /// The exponent on body mass in the relationship between body mass and attack rate
        /// </summary>
        private double _KillRateConstantMassExponent;
        /// <summary>
        /// Get and set the exponent on body mass in the relationship between body mass and attack rate
        /// </summary>
        public double KillRateConstantMassExponent { get { return _KillRateConstantMassExponent; } }


        public void InitialiseParametersPredation()
        {
            _TimeUnitImplementation =
                EcologicalParameters.TimeUnits[(int)EcologicalParameters.Parameters["Predation.RevisedPredation.TimeUnitImplementation"]];
            _HandlingTimeScalarTerrestrial = EcologicalParameters.Parameters["Predation.RevisedPredation.Terrestrial.HandlingTimeScalar"];
            _HandlingTimeExponentTerrestrial = EcologicalParameters.Parameters["Predation.RevisedPredation.Terrestrial.HandlingTimeExponent"];
            _HandlingTimeScalarMarine = EcologicalParameters.Parameters["Predation.RevisedPredation.Marine.HandlingTimeScalar"];
            _HandlingTimeExponentMarine = EcologicalParameters.Parameters["Predation.RevisedPredation.Marine.HandlingTimeExponent"];
            _ReferenceMass = EcologicalParameters.Parameters["Predation.RevisedPredation.HandlingTimeReferenceMass"];
            _KillRateConstant = EcologicalParameters.Parameters["Predation.RevisedPredation.KillRateConstant"];
            _KillRateConstantMassExponent = EcologicalParameters.Parameters["Predation.RevisedPredation.KillRateConstantMassExponent"];
            _FeedingPreferenceStandardDeviation = EcologicalParameters.Parameters["Predation.RevisedPredation.FeedingPreferenceStandardDeviation"];
            NumberOfBins = (int)EcologicalParameters.Parameters["Predation.RevisedPredation.NumberOfMassAggregationBins"];
        }


        /// <summary>
        /// Write out the values of the parameters to an output file
        /// </summary>
        /// <param name="sw">A streamwriter object to write the parameter values to</param>
        public void WriteOutParameterValues(StreamWriter sw)
        {
            // Write out parameters
            sw.WriteLine("Predation\tTimeUnitImplementation\t" + Convert.ToString(_TimeUnitImplementation));
            sw.WriteLine("Predation\tReferenceMass_g\t" + Convert.ToString(_ReferenceMass));
            sw.WriteLine("Predation\tHandlingTimeScalarTerrestrial\t" + Convert.ToString(_HandlingTimeScalarTerrestrial));
            sw.WriteLine("Predation\tHandlingTimeExponentTerrestrial\t" + Convert.ToString(_HandlingTimeExponentTerrestrial));
            sw.WriteLine("Predation\tHandlingTimeScalarMarine\t" + Convert.ToString(_HandlingTimeScalarMarine));
            sw.WriteLine("Predation\tHandlingTimeExponentMarine\t" + Convert.ToString(_HandlingTimeExponentMarine));
            sw.WriteLine("Predation\tKillRateConstant\t" + Convert.ToString(_KillRateConstant));
            sw.WriteLine("Predation\tFeedingPreferenceStandardDeviation\t" + Convert.ToString(_FeedingPreferenceStandardDeviation));
            sw.WriteLine("Predation\tKillRateConstantMassExponent\t" + Convert.ToString(_KillRateConstantMassExponent));

        }


        /// <summary>
        /// Calculate the potential number of individuals in a prey cohort eaten by an acting predator cohort given the number of prey detections
        /// </summary>
        /// <param name="preyAbundance">The number of individuals in the prey cohort</param>
        /// <param name="preyIndividualMass">The body mass of prey individuals</param>
        /// <param name="preyMassBinNumber">The mass bin of the prey</param>
        /// <param name="preyFunctionalGroup">The functional group index of the prey</param>
        /// <param name="predatorIndividualMass">The body mass of predator individuals</param>
        /// <param name="preyIsCarnivore">Whether the prey cohort is a carnivore cohort</param>
        /// <param name="preyIsOmnivore">Whether the prey cohort is an omnivore cohort</param>
        /// <param name="predatorIsOmnivore">Whether the predator cohort is an omnivore cohort</param>
        /// <param name="logOptimalPreyPredatorMassRatio">The log ratio of optimal prey body mass to predator body mass</param>
        /// <returns>The potential number of individuals in a prey cohort eaten by an acting predator cohort</returns>
        private double CalculateExpectedNumberKilledTerrestrial(double preyAbundance, double preyIndividualMass, int preyMassBinNumber, 
            int preyFunctionalGroup, double predatorIndividualMass, Boolean preyIsCarnivore, Boolean preyIsOmnivore, Boolean predatorIsOmnivore, 
            double logOptimalPreyPredatorMassRatio)
            
    {
            // Calculate the killing rate of an individual predator per unit prey density per hectare per time unit
        Alphaij = CalculateIndividualKillingRatePerHectare(preyIndividualMass, preyMassBinNumber, preyFunctionalGroup, predatorIndividualMass, logOptimalPreyPredatorMassRatio);
                        
            // Calculate the potential number of prey killed given the number of prey detections
            return Alphaij * preyAbundance / _CellAreaHectares;
        }

        /// <summary>
        /// Calculate the potential number of individuals in a prey cohort eaten by an acting predator cohort given the number of prey detections
        /// </summary>
        /// <param name="preyAbundance">The number of individuals in the prey cohort</param>
        /// <param name="preyIndividualMass">The body mass of prey individuals</param>
        /// <param name="preyMassBinNumber">The mass bin of the prey</param>
        /// <param name="preyFunctionalGroup">The functional group index of the prey</param>
        /// <param name="predatorIndividualMass">The body mass of predator individuals</param>
        /// <param name="preyIsCarnivore">Whether the prey cohort is a carnivore cohort</param>
        /// <param name="preyIsOmnivore">Whether the prey cohort is an omnivore cohort</param>
        /// <param name="predatorIsOmnivore">Whether the predator cohort is am omnivore cohort</param>
        /// <param name="logOptimalPreyPredatorMassRatio">The log ratio of optimal prey body mass to predator body mass</param>
        /// <returns>The potential number of individuals in a prey cohort eaten by an acting predator cohort</returns>
        private double CalculateExpectedNumberKilledMarine(double preyAbundance, double preyIndividualMass, int preyMassBinNumber, 
            int preyFunctionalGroup, double predatorIndividualMass, Boolean preyIsCarnivore, Boolean preyIsOmnivore, Boolean predatorIsOmnivore,
            double logOptimalPreyPredatorMassRatio)
        {
            // Calculate the killing rate of an individual predator per unit prey density per hectare per time unit
            Alphaij = CalculateIndividualKillingRatePerHectare(preyIndividualMass, preyMassBinNumber, preyFunctionalGroup, predatorIndividualMass, logOptimalPreyPredatorMassRatio);
            
            // Calculate the potential number of prey killed given the number of prey detections
            return Alphaij * preyAbundance / _CellAreaHectares;
        }

        // Original
        /// <summary>
        /// Calculates the killing rate of an individual predator per unit prey density per hectare per time unit 
        /// </summary>
        /// <param name="preyIndividualMass">The body mass of individuals in the prey cohort</param>
        /// <param name="preyMassBinNumber">The mass bin index of the prey</param>
        /// <param name="preyFunctionalGroup">The functional group index of the prey</param>
        /// <param name="predatorIndividualMass">The body mass of individuals in the predator cohort</param>
        /// <param name="logOptimalPreyPredatorMassRatio">The log ratio of optimal prey body mass to predator body mass</param>
        /// <returns>The killing rate of an individual predator per unit prey density per hectare per time unit</returns>
        private double CalculateIndividualKillingRatePerHectare(double preyIndividualMass, int preyMassBinNumber, int preyFunctionalGroup, 
            double predatorIndividualMass, double logOptimalPreyPredatorMassRatio)
        {
            //int PreyBinNumber;

            // Calculate the relative feeding preference from a log-normal distribution with mean equal to the optimal 
            // prey to predator ratio and standard deviation as specified
            RelativeFeedingPreference = Math.Exp(-(Math.Pow
                    (((Math.Log(preyIndividualMass / predatorIndividualMass) - logOptimalPreyPredatorMassRatio) /
                    _FeedingPreferenceStandardDeviation), 2)));

            // Calculate the individual killing rate
            return _SpecificPredatorKillRateConstant * RelativeFeedingPreference * BinnedPreyDensities[preyFunctionalGroup, preyMassBinNumber];
        }     

        /// <summary>
        /// Calculates the time for an individual predator to handle an individual prey in the terrestrial realm
        /// </summary>
        /// <param name="preyIndividualMass">The body mass of prey individuals</param>
        /// <returns>The time for an individual predator to handle an individual prey</returns>
        private double CalculateHandlingTimeTerrestrial(double preyIndividualMass)
        {
            return _ReferenceMassRatioScalingTerrestrial * preyIndividualMass;
        }


        /// <summary>
        /// Calculates the time for an individual predator to handle an individual prey in the marine realm
        /// </summary>
        /// <param name="preyIndividualMass">The body mass of prey individuals</param>
        /// <returns>The time for an individual predator to handle an individual prey</returns>
        private double CalculateHandlingTimeMarine(double preyIndividualMass)
        {
            return _ReferenceMassRatioScalingMarine * preyIndividualMass;
        }

        /// <summary>
        /// Calculate the actual abundance of a prey cohort eaten by a predator cohort
        /// </summary>
        /// <param name="potentialKills">The potential abundance of the prey cohort eaten by the predator cohort given the number of detections</param>
        /// <param name="totalHandlingTimePlusOne">The total time that would be taken to eat all detected prey individuals in all prey cohorts plus one</param>
        /// <param name="predatorAbundanceMultipliedByTimeEating">The abundance in the predator cohort</param>
        /// <param name="preyAbundance">The abundance in the prey cohort</param>
        /// <returns>The actual abundance of a prey cohort eaten by a predator cohort</returns>
        private double CalculateAbundanceEaten(double potentialKills, double predatorAbundanceMultipliedByTimeEating, double totalHandlingTimePlusOne, double preyAbundance)
        {
            // This is the more explicit but slower version
            // Check whether there are any individuals in the prey cohort
            /*if (preyAbundance > 0.0)
            {
                // Calculate the instantaneous fraction of the prey cohort eaten
                InstantFractionKilled = predatorAbundance * ((potentialKills / totalHandlingTimePlusOne) / preyAbundance);
            }
            else
            {
                // Set the instantaneous fraction of the prey cohort eaten to zero
                InstantFractionKilled = 0.0;
            }
            

            // Calculate the fraction of of the prey cohort remaining given the proportion of time that the predator cohort spends eating
            FractionRemaining = Math.Exp(-InstantFractionKilled * _SpecificPredatorProportionTimeEating);

            //Return the abundance of prey cohort eaten
            return preyAbundance * (1.0 - FractionRemaining);
          */

            // Optimized for speed; check for zero abundance prey moved to the calling function
            return preyAbundance * (1.0 - Math.Exp(-(predatorAbundanceMultipliedByTimeEating * ((potentialKills / totalHandlingTimePlusOne) / preyAbundance))));
      }

        /// <summary>
        /// Calculate the visibility of the prey cohort (currently set to 1)
        /// </summary>
        /// <param name="preyAbundance">The abundance in the prey cohort</param>
        /// <returns>The visibility of the prey cohort</returns>
        private double CalculateVisibility(double preyAbundance)
        {
            return Math.Pow(preyAbundance, 0);
        }

    }
}