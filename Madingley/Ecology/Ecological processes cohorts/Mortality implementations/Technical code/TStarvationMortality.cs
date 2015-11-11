using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Madingley
{    
    /// <summary>
    /// A formulation of the process of starvation mortality
    /// </summary>
    public partial class StarvationMortality : IMortalityImplementation
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
        /// Constructor for starvation mortality: assigns all parameter values
        /// 
        /// </summary>
        public StarvationMortality(string globalModelTimeStepUnit)
        {

            InitialiseParametersStarvationMortality();

            // Initialise the utility functions
            UtilityFunctions Utilities = new UtilityFunctions();

            // Calculate the scalar to convert from the time step units used by this implementation of mortality to the global  model time step units
            _DeltaT = Utilities.ConvertTimeUnits(globalModelTimeStepUnit, _TimeUnitImplementation);
        }

    }
}
