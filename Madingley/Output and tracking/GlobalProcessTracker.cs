using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// Tracks ecological processes
    /// </summary>
    public class GlobalProcessTracker
    {
        /// <summary>
        /// Whether to track processes
        /// </summary>
        private Boolean _TrackProcesses;
        /// <summary>
        /// Get or set whether to track processes
        /// </summary>
        public Boolean TrackProcesses
        {
            get { return _TrackProcesses; }
            set { _TrackProcesses = value; }
        }

        /// <summary>
        /// An instance of the global NPP tracker
        /// </summary>
        private GlobalNPPTracker _TrackNPP;

        /// <summary>
        /// Get and set the instance of the global NPP tracker
        /// </summary>
        public GlobalNPPTracker TrackNPP
        {
            get { return _TrackNPP; }
            set { _TrackNPP = value; }
        }

        /// <summary>
        /// Constructor for process tracker: Initialises the trackers for individual processes
        /// </summary>
        /// <param name="numTimesteps">The number of time steps in the model</param>
        /// <param name="lats">The latitudes of active grid cells in the model</param>
        /// <param name="lons">The longitudes of active grid cells in the model</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="Filenames">The filenames of the output files to write the tracking results to</param>
        /// <param name="trackProcesses">Whether to track processes</param>
        /// <param name="cohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="missingValue">The missing value to use in process tracking output files</param>
        /// <param name="outputFileSuffix">The suffix to be applied to output files from process tracking</param>
        /// <param name="outputPath">The path to the folder to be used for process tracking outputs</param>
        /// <param name="trackerMassBins">The mass bins to use for categorising output data in the process trackers</param>
        /// <param name="specificLocations">Whether the model is being run for specific locations</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="latCellSize">The size of grid cells latitudinally</param>
        /// <param name="lonCellSize">The size of grid cells longitudinally</param>
        public GlobalProcessTracker(uint numTimesteps,
            float[] lats, float[] lons,
            List<uint[]> cellIndices,
            SortedList<string, string> Filenames,
            Boolean trackProcesses,
            FunctionalGroupDefinitions cohortDefinitions,
            FunctionalGroupDefinitions stockDefinitions,
            double missingValue,
            string outputFileSuffix,
            string outputPath, MassBinsHandler trackerMassBins,
            Boolean specificLocations,
            MadingleyModelInitialisation initialisation,
            float latCellSize,
            float lonCellSize)
        {
            // Initialise trackers for ecological processes
            _TrackProcesses = trackProcesses;

            if (_TrackProcesses)
            {
                _TrackNPP = new GlobalNPPTracker(outputPath, lats.Length, lons.Length, lats, lons, latCellSize, lonCellSize,
                    (int)numTimesteps,stockDefinitions.GetNumberOfFunctionalGroups(),outputFileSuffix);

            }
        }

        /// <summary>
        /// Record a flow of biomass to plants through net primary production
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="val">The NPP value</param>
        public void RecordNPP(uint latIndex, uint lonIndex, uint stock, double val)
        {
            _TrackNPP.RecordNPPValue(latIndex, lonIndex, stock, val);
        }

        /// <summary>
        /// Write out the NPP-tracking data to file
        /// </summary>
        /// <param name="t">The current time step</param>
        public void StoreNPPGrid(uint t, uint stock)
        {
            _TrackNPP.StoreNPPGrid(t,stock);
        }

        /// <summary>
        /// Record a flow of biomass to plants through net primary production
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="val">The HANPP value</param>
        public void RecordHANPP(uint latIndex, uint lonIndex, uint stock, double val)
        {
            _TrackNPP.RecordHANPPValue(latIndex, lonIndex, stock, val);
        }

        /// <summary>
        /// Write out the HANPP-tracking data to file
        /// </summary>
        /// <param name="t">The current time step</param>
        public void StoreHANPPGrid(uint t, uint stock)
        {
            _TrackNPP.StoreHANPPGrid(t, stock);
        }

        /// <summary>
        /// Close the connection to the file for outputting NPP data
        /// </summary>
        public void CloseNPPFile()
        {
            _TrackNPP.CloseNPPFile();

        }

    }
}
