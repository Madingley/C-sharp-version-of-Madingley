using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

namespace Madingley
{
    /// <summary>
    /// Tracks flows of biomass into plant matter through net primary production
    /// </summary>
    public class GlobalNPPTracker
    {
        /// <summary>
        /// An array to hold gridded NPP values
        /// </summary>
        double[,,] NPP;

        /// <summary>
        /// An array to hold gridded NPP values
        /// </summary>
        double[,,] HANPP;

        /// <summary>
        /// An instance of the class to convert data between arrays and SDS objects
        /// </summary>
        private ArraySDSConvert DataConverter;

        /// <summary>
        /// Instance of the class to create SDS objects
        /// </summary>
        private CreateSDSObject SDSCreator;

        /// <summary>
        /// A dataset to store the NPP outputs to file
        /// </summary>
        private DataSet NPPOutput;

        /// <summary>
        /// A dataset to store the NPP outputs to file
        /// </summary>
        private DataSet HANPPOutput;

        private int _NumLats;
        private int _NumLons;

        /// <summary>
        /// Constructor for the global NPP tracker: sets up the output file and the data arrays
        /// </summary>
        /// <param name="outputPath">Path to the file to output NPP data to</param>
        /// <param name="numLats">The number of cells latitudinally in the NPP output grid</param>
        /// <param name="numLons">The number of cells longitudinally in the NPP output grid</param>
        /// <param name="lats">The latitudes of cells in the grid to output</param>
        /// <param name="lons">The longitudes of cells in the grid to output</param>
        /// <param name="latCellSize">The latitudinal cell size of the grid to output</param>
        /// <param name="lonCellSize">The longitudinal cell size of the grid to output</param>
        /// <param name="numTimeSteps">The number of time steps to output NPP data for</param>
        /// <param name="outputFileSuffix">The suffix for output filename: scenario label + simulation number</param>
        public GlobalNPPTracker(string outputPath, int numLats, int numLons, float[] lats, float[] lons, float latCellSize, float lonCellSize,
             int numTimeSteps, int numStocks,string outputFileSuffix)

        {
            _NumLats = numLats;
            _NumLons = numLons;

            // Initialise the data converter
            DataConverter = new ArraySDSConvert();

            // Initialise the SDS object creator
            SDSCreator = new CreateSDSObject();

            // Create an SDS object to hold NPP data
            NPPOutput = SDSCreator.CreateSDS("netCDF", "NPP_Output" + outputFileSuffix, outputPath);

            // Create an SDS object to hold total abundance and biomass data
            HANPPOutput = SDSCreator.CreateSDS("netCDF", "HANPP_Output" + outputFileSuffix, outputPath);

            // Create vector to hold the values of the time dimension
            float[] TimeSteps = new float[numTimeSteps];



            // Fill other values from 0 (this will hold outputs during the model run)
            for (int i = 0; i < numTimeSteps; i++)
            {
                TimeSteps[i] = i;
            }

            // Declare vectors for geographical dimension data
            float[] outLats = new float[numLats];
            float[] outLons = new float[numLons];

            // Populate the dimension variable vectors with cell centre latitude and longitudes
            for (int i = 0; i < numLats; i++)
            {
                outLats[i] = lats[i] + (latCellSize / 2);
            }

            for (int jj = 0; jj < numLons; jj++)
            {
                outLons[jj] = lons[jj] + (lonCellSize / 2);
            }


            // Add output variables that are dimensioned geographically and temporally to grid output file
            string[] GeographicalDimensions = { "Latitude", "Longitude", "Time step" };
            for (int ii = 0; ii < numStocks; ii++)
            {
                DataConverter.AddVariable(NPPOutput, "NPP_" + ii.ToString(), 3, GeographicalDimensions, -9999.0, outLats, outLons, TimeSteps);
                DataConverter.AddVariable(HANPPOutput, "HANPP_"+ii.ToString(), 3, GeographicalDimensions, -9999.0, outLats, outLons, TimeSteps);
            }
            
            NPP = new double[numLats, numLons, numStocks];
            HANPP = new double[numLats, numLons, numStocks];

            for (int ii = 0; ii < numLats; ii++)
            {
                for (int jj = 0; jj < numLons; jj++)
                {
                    for (int kk = 0; kk < numStocks; kk++)
                    {
                        NPP[ii, jj, kk] = -9999.0;
                        HANPP[ii, jj, kk] = -9999.0;
                    }
                }
                
            }

        }

        /// <summary>
        /// Add the NPP value for this grid cell
        /// </summary>
        /// <param name="latIndex">The latitude index of the grid cell</param>
        /// <param name="lonIndex">The longitude index of the grid cell</param>
        /// <param name="val">The NPP value to be recorded</param>
        public void RecordNPPValue(uint latIndex,uint lonIndex, uint stock, double val)
        {
            NPP[latIndex, lonIndex, stock] = val;
        }

        /// <summary>
        /// Add the HANPP value for this grid cell
        /// </summary>
        /// <param name="latIndex">The latitude index of the grid cell</param>
        /// <param name="lonIndex">The longitude index of the grid cell</param>
        /// <param name="val">The HANPP value to be recorded</param>
        public void RecordHANPPValue(uint latIndex, uint lonIndex, uint stock, double val)
        {
            HANPP[latIndex, lonIndex, stock] = val;
        }

        /// <summary>
        /// Add the filled NPP grid the memory dataset ready to be written to file
        /// </summary>
        /// <param name="t">The current time step</param>
        public void StoreNPPGrid(uint t,uint stock)
        {
            double[,] NPPout;

            NPPout = new double[_NumLats, _NumLons];
            for (int ii = 0; ii < _NumLats; ii++)
            {
                for (int jj = 0; jj < _NumLons; jj++)
                {
                    NPPout[ii, jj] = NPP[ii, jj, stock];
                }
            }

            DataConverter.Array2DToSDS3D(NPPout, "NPP_"+stock.ToString(), new string[] { "Latitude", "Longitude", "Time step" },
                                        (int)t, 0, NPPOutput);


            for (int ii = 0; ii < _NumLats; ii++)
            {
                for (int jj = 0; jj < _NumLons; jj++)
                {
                    
                    NPP[ii, jj, stock] = -9999.0;
                }

            }
        }


        /// <summary>
        /// Add the filled NPP grid the memory dataset ready to be written to file
        /// </summary>
        /// <param name="t">The current time step</param>
        public void StoreHANPPGrid(uint t, uint stock)
        {
            double[,] HANPPout;

            HANPPout = new double[_NumLats, _NumLons];
            for (int ii = 0; ii < _NumLats; ii++)
            {
                for (int jj = 0; jj < _NumLons; jj++)
                {
                    HANPPout[ii, jj] = HANPP[ii, jj, stock];
                }
            }

            DataConverter.Array2DToSDS3D(HANPPout, "HANPP_" + stock.ToString(), new string[] { "Latitude", "Longitude", "Time step" },
                                        (int)t, 0, HANPPOutput);


            for (int ii = 0; ii < _NumLats; ii++)
            {
                for (int jj = 0; jj < _NumLons; jj++)
                {

                    HANPP[ii, jj, stock] = -9999.0;
                }

            }
        }

        /// <summary>
        /// Close the connection to the file for outputting NPP flows
        /// </summary>
        public void CloseNPPFile()
        {
            NPPOutput.Dispose();
        }

    }
}
