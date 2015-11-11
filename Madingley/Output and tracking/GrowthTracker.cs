using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Madingley
{
    /// <summary>
    /// Track the growth of a cohort in a time step
    /// </summary>
    public class GrowthTracker
    {
        /// <summary>
        /// File to write data on growth to
        /// </summary>
        string GrowthFilename;

        /// <summary>
        /// A streamwriter for writing out data on growth
        /// </summary>
        private StreamWriter GrowthWriter;
        private TextWriter SyncGrowthWriter;

        /// <summary>
        /// Set up the tracker for outputing the growth of cohorts each time step
        /// </summary>
        /// <param name="numTimeSteps">The total number of timesteps for this simulation</param>
        /// <param name="numLats">The number of latitudes in the model grid</param>
        /// <param name="numLons">The number of longitudes in the model grid</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="growthFilename">The name of the file to write information about growth to</param>
        /// <param name="outputFilesSuffix">The suffix to apply to output files from this simulation</param>
        /// <param name="outputPath">The file path to write all outputs to</param>
        /// <param name="cellIndex">The index of the current cell in the list of all cells in this simulation</param>
        public GrowthTracker(uint numTimeSteps, uint numLats, uint numLons, List<uint[]> cellIndices, string growthFilename,
            string outputFilesSuffix, string outputPath, int cellIndex)
        {
            GrowthFilename = growthFilename;

            // Initialise streamwriter to output growth data
            GrowthWriter = new StreamWriter(outputPath + GrowthFilename + outputFilesSuffix + "_Cell" + cellIndex + ".txt");
            SyncGrowthWriter = TextWriter.Synchronized(GrowthWriter);
            SyncGrowthWriter.WriteLine("Latitude\tLongitude\ttime_step\tCurrent_body_mass_g\tfunctional_group\tgrowth_g\tmetabolism_g\tpredation_g\therbivory_g");

        }

        /// <summary>
        /// Record the growth of the individuals in a cohort in the current time step
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="timeStep">The current time step</param>
        /// <param name="currentBodyMass">The current body mass of individuals in the cohort</param>
        /// <param name="functionalGroup">The index of the functional group that the cohort belongs to</param>
        /// <param name="netGrowth">The net growth of individuals in the cohort this time step</param>
        /// <param name="metabolism">The biomass lost by individuals in this cohort through metabolism</param>
        /// <param name="predation">The biomass gained by individuals in this cohort through predation</param>
        /// <param name="herbivory">The biomass gained by individuals in this cohort through herbivory</param>
        public void RecordGrowth(uint latIndex, uint lonIndex, uint timeStep, double currentBodyMass, int functionalGroup, 
            double netGrowth, double metabolism, double predation, double herbivory)
        {
            SyncGrowthWriter.WriteLine(Convert.ToString(latIndex) + '\t' + Convert.ToString(lonIndex) + '\t' + Convert.ToString(timeStep) +
                '\t' + Convert.ToString(currentBodyMass) + '\t' + Convert.ToString(functionalGroup) + '\t' + Convert.ToString(netGrowth)+ '\t' + Convert.ToString(metabolism)+ '\t' + Convert.ToString(predation)+ '\t' + Convert.ToString(herbivory));
        }

        /// <summary>
        /// Closes streams for writing growth data
        /// </summary>
        public void CloseStreams()
        {

            SyncGrowthWriter.Dispose();
            GrowthWriter.Dispose();
        }

    }
}
