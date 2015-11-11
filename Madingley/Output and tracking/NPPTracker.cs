using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace Madingley
{
    /// <summary>
    /// Tracks primary productivity
    /// </summary>
    public class NPPTracker
    {
        /// <summary>
        /// The filename for the NPP data
        /// </summary>
        string NPPFileName;

        /// <summary>
        /// Stream-writer to output NPP data
        /// </summary>
        private StreamWriter NPPWriter;

        /// <summary>
        /// Thread-safe text-writer to output NPP data
        /// </summary>
        private TextWriter SyncedNPPWriter;

        /// <summary>
        /// Set up the tracker for outputting NPP data to file
        /// </summary>
        /// <param name="nppFilename">The name of the file to write information on NPP to</param>
        /// <param name="outputPath">The file path to write all outputs to</param>
        /// <param name="outputFilesSuffix">The suffix to apply to output files from this simulation</param>
        public NPPTracker(string nppFilename, string outputPath, string outputFilesSuffix)
        {
            NPPFileName = nppFilename;

            // Initialise stream-writers to output NPP data
            NPPWriter = new StreamWriter(outputPath + NPPFileName + outputFilesSuffix + ".txt");
            SyncedNPPWriter = TextWriter.Synchronized(NPPWriter);
            SyncedNPPWriter.WriteLine("Latitude\tLongitude\ttime_step\tcell_area\ttotal_cell_productivity_g_per_month");

        }

        /// <summary>
        /// Record the total primary productivity in the current cell in the current time step
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current cell</param>
        /// <param name="lonIndex">The longitudinal index of the current cell</param>
        /// <param name="timeStep">The current model time step</param>
        /// <param name="cellArea">The area of the current grid cell</param>
        /// <param name="cellNPP">The total primary productivity in the cell this time step</param>
        public void RecordNPP(uint latIndex, uint lonIndex, uint timeStep, double cellArea, double cellNPP)
        {
            SyncedNPPWriter.WriteLine(Convert.ToString(latIndex) + '\t' + Convert.ToString(lonIndex) + '\t' + Convert.ToString(timeStep) +
                '\t' + Convert.ToString(cellArea) + '\t' + Convert.ToString(cellNPP));

        }

        /// <summary>
        /// Close the streams for writing NPP data
        /// </summary>
        public void CloseStreams()
        {
            SyncedNPPWriter.Dispose();
            NPPWriter.Dispose();
        }
        
    }
}
