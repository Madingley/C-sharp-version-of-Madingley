using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// A formulation of the process of senescence mortality
    /// </summary>
    public partial class SenescenceMortality : IMortalityImplementation
    {
        
        /// <summary>
        /// Scalar to convert from the time step units used by this mortality implementation to global model time step units
        /// </summary>
        private double _DeltaT;
        /// <summary>
        /// Get the scalar to convert from the time step units used by this mortality implementation to global model time step units
        /// </summary>
        public double DeltaT { get { return _DeltaT; } }

        /// <summary>
        /// Instance of the class to perform general functions
        /// </summary>
        private UtilityFunctions Utilities;

        /// <summary>
        /// Constructor for senscence mortality: assigns all parameter values
        /// </summary>
        public SenescenceMortality(string globalModelTimeStepUnit)
        {
            InitialiseParametersSenescenceMortality();

            // Initialise the utility functions
            Utilities = new UtilityFunctions();

            // Calculate the scalar to convert from the time step units used by this implementation of mortality to the global model time step units
            _DeltaT = Utilities.ConvertTimeUnits(globalModelTimeStepUnit, _TimeUnitImplementation);
        }
    }
}
