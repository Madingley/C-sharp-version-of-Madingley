using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace Madingley
{
    /// <summary>
    /// Tracks results associated with cohort extinction
    /// </summary>
    public class ExtinctionTracker
    {
        string ExtinctionFilename;

        private StreamWriter ExtinctionWriter;

        private TextWriter SyncedExtinctionWriter;

        /// <summary>
        /// Constructor for the eating tracker: sets up output file
        /// </summary>
        /// <param name="extinctionFilename">The filename for the output file</param>
        /// <param name="outputPath">The path to the output directory</param>
        /// <param name="outputFilesSuffix">The suffix to be applied to all outputs from this model simulation</param>
        /// <param name="cellIndex">The index of the current cell within the list of cells in this simulation</param>
        public ExtinctionTracker(string extinctionFilename, string outputPath, string outputFilesSuffix, int cellIndex)
        {
            ExtinctionFilename = extinctionFilename;

            // Initialise streamwriter to output properties and ids of extinct cohorts
            ExtinctionWriter = new StreamWriter(outputPath + extinctionFilename + outputFilesSuffix + "_Cell" + cellIndex + ".txt");
            // Create a threadsafe textwriter to write outputs to the ExtinctionWriter stream
            SyncedExtinctionWriter = TextWriter.Synchronized(ExtinctionWriter);
            SyncedExtinctionWriter.WriteLine("Latitude\tLongitude\ttime_step\tmerged\tcohortID");

        }

        /// <summary>
        /// Record the extinction of a cohort in the output file
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="currentTimeStep">The current model time step</param>
        /// <param name="merged">Whether the cohort going extinct has ever been merged with another cohort</param>
        /// <param name="cohortID">The ID of the cohort going extinct</param>
        public void RecordExtinction(uint latIndex, uint lonIndex,uint currentTimeStep,bool merged,List<uint> cohortID)
        {
            string newline = Convert.ToString(latIndex) + '\t' + Convert.ToString(lonIndex) + '\t' +
                Convert.ToString(currentTimeStep) + '\t' + Convert.ToString(merged) + '\t' +
                Convert.ToString(cohortID[0]);

            SyncedExtinctionWriter.WriteLine(newline);
        }



    }
}
