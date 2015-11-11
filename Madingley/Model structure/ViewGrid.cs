using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.CSV;
using Microsoft.Research.Science.Data.Imperative;
using Microsoft.Research.Science.Data.Utilities;

namespace Madingley
{
    /// <summary>
    /// This class is for viewing gridded data such as environmental data layers or state variables; it pauses program execution while the viewer is open
    /// </summary>
    public class ViewGrid
    {
        /// <summary>
        /// An instance of the class to convert data between arrays and SDS objects
        /// </summary>
        private ArraySDSConvert DataConverter;

        /// <summary>
        /// Constructor for the grid viewer: initialses relevant objects
        /// </summary>
        public ViewGrid()
        {
            DataConverter = new ArraySDSConvert();
        }

        /// <summary>
        /// Copy an georeferenced array (should be by reference!) to a grid in order to view it, then spawn the data set viewer
        /// <param name="gridToView">The grid to be viewed</param>
        /// <param name="variableName">The name of the variable to be viewed</param>
        /// <param name="lats">A vector of latitudes associated with the grid</param>
        /// <param name="lons">A vector of longitudes associated with the grid</param>
        /// <param name="gridMissingValue">The missing value for the grid to view</param>
        /// </summary>
        public void PauseProgram(ref double[,] gridToView, string variableName, float[] lats, float[] lons, double gridMissingValue)
        {
            // Create a new data set, set it to commit changes manually
            var DataSetToView = DataSet.Open("msds:memory");
            DataSetToView.IsAutocommitEnabled = false;

            // Convert the grid to an SDS structure
            //ConvertSDSArray.GridToSDS(ref gridToView, variableName, lats, lons, gridMissingValue, ref DataSetToView, false);
            DataConverter.Array2DToSDS2D(gridToView, variableName, lats, lons, gridMissingValue, DataSetToView);

            // Open the viewer
            DataSetToView.View();

            // Remove the temporary data set from memory
            DataSetToView.Dispose();
        }

        /// <summary>
        /// Provides a snapshot view of an SDS
        /// </summary>
        /// <param name="DataSetToView">The name of the SDS to view</param>
        /// <param name="handle">An object handle for the viewer instance; send the same handle to prevent multiple instances of SDS viewer opening</param>
        /// <todoD>Need to update to be able to select which variable to view</todoD>
        /// <todoD>Pass sleep length</todoD>
        public void SnapshotView(ref DataSet DataSetToView, ref object handle)
        {
            // Open the snapshot viewer
            handle = DataSetToView.ViewSnapshot("", handle);
            
            // Slow down computation
            System.Threading.Thread.Sleep(250);

        }

        /// <summary>
        /// Asynchronously views an SDS
        /// </summary>
        /// <param name="DataSetToView">The name of the SDS to view</param>
        /// <param name="viewingParameters">A string of viewing parameters ('hints') to pass to SDS viewer</param>
        /// <todoD>Need to update to be able to select which variable to view</todoD>
        /// <todoD>Pass sleep length</todoD>
        /// <todoD>UPdate title on each timestep</todoD>
        public void AsynchronousView(ref DataSet DataSetToView, string viewingParameters)
        {
            DataSetToView.SpawnViewer(viewingParameters);
            // Slow down computation
            //System.Threading.Thread.Sleep(10);

        }

        /// <summary>
        /// Asynchronously views an SDS
        /// </summary>
        /// <param name="DataSetToView">The name of the SDS to view</param>
        /// <todoD>Need to update to be able to select which variable to view</todoD>
        /// <todoD>Pass sleep length</todoD>
        /// <todoD>UPdate title on each timestep</todoD>
        public void AsynchronousView(ref DataSet DataSetToView)
        {
            DataSetToView.SpawnViewer();
            // Slow down computation
            //System.Threading.Thread.Sleep(10);

        }


    }
}
