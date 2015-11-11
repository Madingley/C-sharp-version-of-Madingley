using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// Tracks diagnostics about the ecological processes
    /// </summary>
    public class CrossCellProcessTracker
    {
        /// <summary>
        /// Whether to track cross-cell processes
        /// </summary>
        private Boolean _TrackCrossCellProcesses;
        /// <summary>
        /// Get or set whether to track cross-cell processes
        /// </summary>
        public Boolean TrackCrossCellProcesses
        {
            get { return _TrackCrossCellProcesses; }
            set { _TrackCrossCellProcesses = value; }
        }
        
        /// <summary>
        /// Instance of the dispersal tracker within the cross-cell tracker
        /// </summary>
        private DispersalTracker  _TrackDispersal;
        /// <summary>
        /// Get and set the reproduction tracker
        /// </summary>
        public DispersalTracker  TrackDispersal
        {
            get { return _TrackDispersal; }
            set { _TrackDispersal = value; }
        }

        /// <summary>
        /// Constructor for cross cell process tracker: Initialises the trackers for individual processes
        /// </summary>
        /// <param name="trackCrossCellProcesses">Whether to track cross-grid-cell ecological processes</param>
        /// <param name="filename">The name of the file to output data to</param>
        /// <param name="outputPath">The path to write the file to</param>
        /// <param name="outputFileSuffix">The suffix to apply to the output filename</param>
        public CrossCellProcessTracker(Boolean trackCrossCellProcesses, string filename, string outputPath, string outputFileSuffix)
        {
            // Initialise cross-cell trackers for ecological processes
            // Note that results go into a single file irrespective of how many cells are being tracked; that is, it automatically adjusts to either specific locations or the whole
            // grid
            _TrackCrossCellProcesses = trackCrossCellProcesses;

            if (_TrackCrossCellProcesses)
            {
                _TrackDispersal = new DispersalTracker(filename, outputPath, outputFileSuffix);
            }
        }

        /// <summary>
        /// Record dispersal events in the dispersal tracker
        /// </summary>
        /// <param name="inboundCohorts">The cohorts arriving in a cell in the current time step</param>
        /// <param name="outboundCohorts">The cohorts leaving a cell in the current time step</param>
        /// <param name="outboundCohortWeights">The body masses of cohorts leaving the cell in the current time step</param>
        /// <param name="timestep">The current model time step</param>
        /// <param name="madingleyModelGrid">The model grid</param>
        public void RecordDispersalForACell(uint[, ,] inboundCohorts, uint[, ,] outboundCohorts, List<double>[,] outboundCohortWeights, uint timestep, ModelGrid madingleyModelGrid)
        {
            _TrackDispersal.RecordDispersal(inboundCohorts, outboundCohorts, outboundCohortWeights, timestep, madingleyModelGrid);
        }

       


        /// <summary>
        /// Close all tracker streams
        /// </summary>
		public void CloseStreams()
        {
            _TrackDispersal.CloseStreams();
        }
    }
}
