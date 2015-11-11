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
    public partial class BackgroundMortality : IMortalityImplementation
    {
        
        /// <summary>
        /// Scalar to convert from the time step units used by this mortality implementation to global model time step units
        /// </summary>
        private double _DeltaT;
        /// <summary>
        /// Get the scalar to convert from the time step units used by this mortality implementation to global model time step units
        /// </summary>
        public double DeltaT { get { return _DeltaT; } }

        # region Methods

        /// <summary>
        /// Constructor for background mortality: assigns all parameter values
        /// </summary>
        public BackgroundMortality(string globalModelTimeStepUnit)
        {

            InitialiseParametersBackgroundMortality();

            // Initialise the utility functions
            UtilityFunctions Utilities = new UtilityFunctions();

            // Calculate the scalar to convert from the time step units used by this implementation of mortality to the global model time step units
            _DeltaT = Utilities.ConvertTimeUnits(globalModelTimeStepUnit, _TimeUnitImplementation);
        }

        # endregion

    }
}
