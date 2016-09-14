using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.CSV;
using Microsoft.Research.Science.Data.Imperative;
using Microsoft.Research.Science.Data.Utilities;
using System.Diagnostics;

using System.IO;

namespace Madingley
{
    public class EnviroDataTemporal
    {

                /// Number of latitudinal cells
        /// </summary>
        private uint _NumLats;
        /// <summary>
        /// Get number of latitudinal cells
        /// </summary>
        public uint NumLats { get { return _NumLats; } }

        /// <summary>
        /// Number of longitudinal cells
        /// </summary>
        private uint _NumLons;
        /// <summary>
        /// Get number of longitudinal cells
        /// </summary>
        public uint NumLons { get { return _NumLons; } }

        /// <summary>
        /// Number of time intervals encompassed by the environmental variable
        /// </summary>
        private uint _NumTimes;
        /// <summary>
        /// Get the number of time intervals encompassed by the environmental variable
        /// </summary>
        public uint NumTimes
        { get { return _NumTimes; } }

        /// <summary>
        /// Latitude of the bottom edge of the sothernmost grid cell
        /// </summary>
        private double _LatMin;
        /// <summary>
        /// Get latitude of the bottom edge of the sothernmost grid cell
        /// </summary>
        public double LatMin { get { return _LatMin; } }

        /// <summary>
        /// Latitude of the left edge of the most western grid cell
        /// </summary>
        private double _LonMin;
        /// <summary>
        /// Get latitude of the left edge of the most western grid cell
        /// </summary>
        public double LonMin { get { return _LonMin; } }

        /// <summary>
        /// Value used to denote missing data for this environmental variable
        /// </summary>
        private double _MissingValue;
        /// <summary>
        /// Get value used to denote missing data for this environmental variable
        /// </summary>
        public double MissingValue { get { return _MissingValue; } }

        /// <summary>
        /// Latitudinal distance between adjacent cells
        /// </summary>
        private double _LatStep;
        /// <summary>
        /// Get latitudinal distance between adjacent cells
        /// </summary>
        public double LatStep { get { return _LatStep; } }

        /// <summary>
        /// Longitudinal distance between adjacent cells
        /// </summary>
        private double _LonStep;
        /// <summary>
        /// Get longitudinal distance between adjacent cells
        /// </summary>
        public double LonStep { get { return _LonStep; } }

        /// <summary>
        /// Vector of latitudes of the bottom edges of grid cells
        /// </summary>
        private double[] _Lats;
        /// <summary>
        /// Get vector of latitudes of the bottom edges of grid cells
        /// </summary>
        public double[] Lats { get { return _Lats; } }

        /// <summary>
        /// Vector of longitudes of the left edges of grid cells
        /// </summary>
        private double[] _Lons;
        /// <summary>
        /// Get vector of longitudes of the left edges of grid cells
        /// </summary>
        public double[] Lons { get { return _Lons; } }

        /// <summary>
        /// Vector containing values of the time dimension of the environmental variable
        /// </summary>
        private double[] _Times;
        /// <summary>
        /// Get vector containing values of the time dimension of the environmental variable
        /// </summary>
        public double[] Times { get { return _Times; } }

        /// <summary>
        /// The string required to read the file with the environmental data
        /// </summary>
        private string _ReadFileString;
        /// <summary>
        /// Get the string required to read the file with the environmental data
        /// </summary>
        public string ReadFileString { get { return _ReadFileString; } }

        /// <summary>
        /// The units of the environmental variable
        /// </summary>
        private string _Units;
        /// <summary>
        /// Gets the units of the environmental variable
        /// </summary>
        public string Units
        { get { return _Units; } }

        /// <summary>
        /// Tracks the number of environmental data layers opened
        /// </summary>
        private uint _NumEnviroLayers;
        /// <summary>
        /// Returns the number of environmental data layers opened
        /// </summary>
        public uint NumEnviroLayers { get { return _NumEnviroLayers; } }

        /// <summary>
        /// Instance of the class to perform general functions
        /// </summary>
        private UtilityFunctions Utilities;

        DataSet _InternalData;

        string _DataName;

        bool LatInverted;
        bool LongInverted;

        string _DataResolution;

        /// <summary>
        /// Constructor for EnviroData
        /// </summary>
        /// <param name="fileName">Filename (including extension)</param>
        /// <param name="dataName">The name of the variable that contains the data within the specified file</param>
        /// <param name="dataType">Type of data, nc = NetCDF, ascii = ESRI ASCII)</param>
        /// <param name="dataResolution">The temporal resolution of the environmental variable</param>
        /// <param name="units">The units of the data</param>
        /// <todo>Check whether lat/lon or 0/1 are fixed for all NetCDFs</todo>
        /// <todo>CHECK IF DIMENSIONS HAVE TO BE THE SAME FOR ALL VARIABLES IN A NETCDF AND HOW TO EXTRACT DIMENSIONS FOR A SINGLE VARIABLE IF NECESSARY</todo>
        /// <todo>Write code to check for equal cell sizes in NetCDFs</todo>
        public EnviroDataTemporal(string fileName, string dataName, string dataType, string dataResolution, string units)
        {
            // Initialise the utility functions
            Utilities = new UtilityFunctions();

            // Temporary vectors to hold dimension data
            Single[] tempSingleVector;
            Int32[] tempInt32Vector;
            Int16[] tempInt16Vector;

            // Vectors of possible names of the dimension variables to search in the files for
            string[] LonSearchStrings = new string[] { "lon", "Lon", "longitude", "Longitude", "lons", "Lons", "long", "Long", "longs", "Longs", "longitudes", "Longitudes", "x", "X" };
            string[] LatSearchStrings = new string[] { "lat", "Lat", "latitude", "Latitude", "lats", "Lats", "latitudes", "Latitudes", "y", "Y" };
            string[] MonthSearchStrings = new string[] { "month", "Month", "months", "Months", "Time", "time" };

            //Integer counter for iterating through search strings
            int kk = 0;

            // Construct the string required to access the file using Scientific Dataset
            _ReadFileString = "msds:" + dataType + "?file=" + fileName + "&openMode=readOnly";

            // Open the data file using Scientific Dataset
            _InternalData = DataSet.Open(_ReadFileString);

            _DataName = dataName;

            // Store the specified units
            _Units = units;

            _DataResolution = dataResolution;

            // Switch based on the tempeoral resolution and data type
            switch (dataResolution)
            {
                //case "year":
                //    switch (dataType)
                //    {
                //        case "esriasciigrid":
                //            // Extract the number of latidudinal and longitudinal cells in the file
                //            _NumLats = (uint)_InternalData.Dimensions["x"].Length;
                //            _NumLons = (uint)_InternalData.Dimensions["y"].Length;
                //            // Set number of time intervals equal to 1
                //            _NumTimes = 1;
                //            // Initialise the vector of time steps with length 1
                //            _Times = new double[1];
                //            // Assign the single value of the time step dimension to be equal to 1
                //            _Times[0] = 1;
                //            // Get the value used for missing data in this environmental variable
                //            _MissingValue = _InternalData.GetAttr<double>(1, "NODATA_value");
                //            // Get the latitudinal and longitudinal sizes of grid cells
                //            _LatStep = _InternalData.GetAttr<double>(1, "cellsize");
                //            _LonStep = _LatStep;
                //            // Get longitudinal 'x' and latitudinal 'y' corners of the bottom left of the data grid
                //            _LatMin = _InternalData.GetAttr<double>(1, "yllcorner");
                //            _LonMin = _InternalData.GetAttr<double>(1, "xllcorner");
                //            // Create vectors holding the latitudes and longitudes of the bottom-left corners of the grid cells
                //            _Lats = new double[NumLats];
                //            for (int ii = 0; ii < NumLats; ii++)
                //            {
                //                _Lats[NumLats - 1 - ii] = LatMin + ii * _LatStep;
                //            }
                //            _Lons = new double[NumLons];
                //            for (int ii = 0; ii < NumLons; ii++)
                //            {
                //                _Lons[ii] = LonMin + ii * _LonStep;
                //            }
                //            break;
                //        case "nc":
                //            // Loop over possible names for the latitude dimension until a match in the data file is found
                //            kk = 0;
                //            while ((kk < LatSearchStrings.Length) && (!_InternalData.Variables.Contains(LatSearchStrings[kk]))) kk++;

                //            // If a match for the latitude dimension has been found then read in the data, otherwise throw an error
                //            if (kk < LatSearchStrings.Length)
                //            {
                //                // Get number of latitudinal cells in the file
                //                _NumLats = (uint)_InternalData.Dimensions[LatSearchStrings[kk]].Length;
                //                // Read in the values of the latitude dimension from the file
                //                // Check which format the latitude dimension data are in; if unrecognized, then throw an error
                //                if (_InternalData.Variables[LatSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "single")
                //                {
                //                    // Read the latitude dimension data to a temporary vector
                //                    tempSingleVector = _InternalData.GetData<Single[]>(LatSearchStrings[kk]);
                //                    // Convert the dimension data to double format and add to the vector of dimension values
                //                    _Lats = new double[tempSingleVector.Length];
                //                    for (int jj = 0; jj < tempSingleVector.Length; jj++)
                //                    {
                //                        _Lats[jj] = (double)tempSingleVector[jj];
                //                    }
                //                }
                //                else if (_InternalData.Variables[LatSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "double")
                //                {
                //                    // Read the dimension data directly into the vector of dimension values
                //                    _Lats = _InternalData.GetData<double[]>(LatSearchStrings[kk]);
                //                }
                //                else
                //                {
                //                    // Data format unrecognized, so throw an error
                //                    Debug.Fail("Unrecognized data format for latitude dimension");
                //                }
                //            }
                //            else
                //            {
                //                // Didn't find a plausible match for latitude dimension data, so throw an error
                //                Debug.Fail("Cannot find any variables that look like latitude dimensions");
                //            }

                //            // Loop over possible names for the latitude dimension until a match in the data file is found
                //            kk = 0;
                //            while ((kk < LonSearchStrings.Length) && (!_InternalData.Variables.Contains(LonSearchStrings[kk]))) kk++;

                //            // If a match for the longitude dimension has been found then read in the data, otherwise throw an error
                //            if (kk < LonSearchStrings.Length)
                //            {
                //                // Get number of longitudinal cells in the file
                //                _NumLons = (uint)_InternalData.Dimensions[LonSearchStrings[kk]].Length;
                //                // Read in the values of the longitude dimension from the file
                //                // Check which format the longitude dimension data are in; if unrecognized, then throw an error
                //                if (_InternalData.Variables[LonSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "single")
                //                {
                //                    // Read the longitude dimension data to a temporary vector
                //                    tempSingleVector = _InternalData.GetData<Single[]>(LonSearchStrings[kk]);
                //                    // Convert the dimension data to double format and add to the vector of dimension values
                //                    _Lons = new double[tempSingleVector.Length];
                //                    for (int jj = 0; jj < tempSingleVector.Length; jj++)
                //                    {
                //                        _Lons[jj] = (double)tempSingleVector[jj];
                //                    }
                //                }
                //                else if (_InternalData.Variables[LonSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "double")
                //                {
                //                    // Read the dimension data directly into the vector of dimension values
                //                    _Lons = _InternalData.GetData<double[]>(LonSearchStrings[kk]);
                //                }
                //                else
                //                {
                //                    // Data format unrecognized, so throw an error
                //                    Debug.Fail("Unrecognized data format for longitude dimension");
                //                }
                //            }
                //            else
                //            {
                //                // Didn't find a plausible match for longitude dimension data, so throw an error
                //                Debug.Fail("Cannot find any variables that look like longitude dimensions");
                //            }
                //            // Set number of time intervals equal to 1
                //            _NumTimes = 1;
                //            // Initialise the vector of time steps with length 1
                //            _Times = new double[1];
                //            // Assign the single value of the time step dimension to be equal to 1
                //            _Times[0] = 1;
                //            // Get the latitudinal and longitudinal sizes of grid cells
                //            _LatStep = (_Lats[1] - _Lats[0]);
                //            _LonStep = (_Lons[1] - _Lons[0]);
                //            // Convert vectors of latitude and longutiude dimension data from cell-centre references to bottom-left references
                //            //if LatStep is positive then subtract the step to convert to the bottom  left corner of the cell,
                //            // else if LatStep is negative, then need to add the step to convert to the bottom left
                //            for (int ii = 0; ii < _Lats.Length; ii++)
                //            {
                //                _Lats[ii] = (_LatStep.CompareTo(0.0) > 0) ? _Lats[ii] - (_LatStep / 2) : _Lats[ii] + (_LatStep / 2);
                //            }
                //            for (int jj = 0; jj < _Lons.Length; jj++)
                //            {
                //                _Lons[jj] = (_LonStep.CompareTo(0.0) > 0) ? _Lons[jj] - (_LonStep / 2) : _Lons[jj] + (_LonStep / 2);
                //            }
                //            // Check whether latitudes and longitudes are inverted in the NetCDF file
                //            LatInverted = (_Lats[1] < _Lats[0]);
                //            LongInverted = (_Lons[1] < _Lons[0]);
                            
                //            // Get longitudinal 'x' and latitudinal 'y' corners of the bottom left of the data grid
                //            _LatMin = _Lats[0];
                //            _LonMin = _Lons[0];
                //            break;
                //        default:
                //            // Data type not recognized, so throw an error
                //            Debug.Fail("Data type not supported");
                //            break;
                //    }
                //    break;
                case "month":
                    switch (dataType)
                    {
                        case "esriasciigrid":
                            // This combination does not work in the model, so throw an error
                            Debug.Fail("Variables at monthly temporal resolution must be stored as three-dimensional NetCDFs");
                            break;
                        case "nc":
                            // Loop over possible names for the latitude dimension until a match in the data file is found
                            kk = 0;
                            while ((kk < LatSearchStrings.Length) && (!_InternalData.Variables.Contains(LatSearchStrings[kk]))) kk++;

                            // If a match for the latitude dimension has been found then read in the data, otherwise throw an error
                            if (kk < LatSearchStrings.Length)
                            {
                                // Get number of latitudinal cells in the file
                                _NumLats = (uint)_InternalData.Dimensions[LatSearchStrings[kk]].Length;
                                // Read in the values of the latitude dimension from the file
                                // Check which format the latitude dimension data are in; if unrecognized, then throw an error
                                if (_InternalData.Variables[LatSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "single")
                                {
                                    // Read the latitude dimension data to a temporary vector
                                    tempSingleVector = _InternalData.GetData<Single[]>(LatSearchStrings[kk]);
                                    // Convert the dimension data to double format and add to the vector of dimension values
                                    _Lats = new double[tempSingleVector.Length];
                                    for (int jj = 0; jj < tempSingleVector.Length; jj++)
                                    {
                                        _Lats[jj] = (double)tempSingleVector[jj];
                                    }
                                }
                                else if (_InternalData.Variables[LatSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "double")
                                {
                                    // Read the dimension data directly into the vector of dimension values
                                    _Lats = _InternalData.GetData<double[]>(LatSearchStrings[kk]);
                                }
                                else
                                {
                                    // Data format unrecognized, so throw an error
                                    Debug.Fail("Unrecognized data format for latitude dimension");
                                }
                            }
                            else
                            {
                                // Didn't find a plausible match for latitude dimension data, so throw an error
                                Debug.Fail("Cannot find any variables that look like latitude dimensions");
                            }

                            // Loop over possible names for the latitude dimension until a match in the data file is found
                            kk = 0;
                            while ((kk < LonSearchStrings.Length) && (!_InternalData.Variables.Contains(LonSearchStrings[kk]))) kk++;

                            // If a match for the longitude dimension has been found then read in the data, otherwise throw an error
                            if (kk < LonSearchStrings.Length)
                            {
                                // Get number of longitudinal cells in the file
                                _NumLons = (uint)_InternalData.Dimensions[LonSearchStrings[kk]].Length;
                                // Read in the values of the longitude dimension from the file
                                // Check which format the longitude dimension data are in; if unrecognized, then throw an error
                                if (_InternalData.Variables[LonSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "single")
                                {
                                    // Read the longitude dimension data to a temporary vector
                                    tempSingleVector = _InternalData.GetData<Single[]>(LonSearchStrings[kk]);
                                    // Convert the dimension data to double format and add to the vector of dimension values
                                    _Lons = new double[tempSingleVector.Length];
                                    for (int jj = 0; jj < tempSingleVector.Length; jj++)
                                    {
                                        _Lons[jj] = (double)tempSingleVector[jj];
                                    }
                                }
                                else if (_InternalData.Variables[LonSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "double")
                                {
                                    // Read the dimension data directly into the vector of dimension values
                                    _Lons = _InternalData.GetData<double[]>(LonSearchStrings[kk]);
                                }
                                else
                                {
                                    // Data format unrecognized, so throw an error
                                    Debug.Fail("Unrecognized data format for longitude dimension");
                                }
                            }
                            else
                            {
                                // Didn't find a plausible match for longitude dimension data, so throw an error
                                Debug.Fail("Cannot find any variables that look like longitude dimensions");
                            }

                            // Loop over possible names for the monthly temporal dimension until a match in the data file is found
                            kk = 0;
                            while ((kk < MonthSearchStrings.Length) && (!_InternalData.Variables.Contains(MonthSearchStrings[kk]))) kk++;

                            // Of a match for the monthly temporal dimension has been found then read in the data, otherwise thrown an error
                            if (_InternalData.Variables.Contains(MonthSearchStrings[kk]))
                            {
                                // Get the number of months in the temporal dimension
                                _NumTimes = (uint)_InternalData.Dimensions[MonthSearchStrings[kk]].Length;
                                
                                // Read in the values of the temporal dimension from the file
                                // Check which format the temporal dimension data are in; if unrecognized, then throw an error
                                if (_InternalData.Variables[MonthSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "single")
                                {
                                    // Read the temporal dimension data to a temporary vector
                                    tempSingleVector = _InternalData.GetData<Single[]>(MonthSearchStrings[kk]);
                                    // Convert the dimension data to double format and add to the vector of dimension values
                                    _Times = new double[_NumTimes];
                                    for (int hh = 0; hh < tempSingleVector.Length; hh++)
                                    {
                                        _Times[hh] = (double)tempSingleVector[hh];
                                    }
                                }
                                else if (_InternalData.Variables[MonthSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "double")
                                {
                                    // Read the dimension data directly into the vector of dimension values
                                    _Times = _InternalData.GetData<double[]>(MonthSearchStrings[kk]);
                                }
                                else if (_InternalData.Variables[MonthSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "int32")
                                {
                                    // Read the temporal dimension data to a temporary vector
                                    tempInt32Vector = _InternalData.GetData<Int32[]>(MonthSearchStrings[kk]);
                                    // Convert the dimension data to double format and add to the vector of dimension values
                                    _Times = new double[_NumTimes];
                                    for (int hh = 0; hh < tempInt32Vector.Length; hh++)
                                    {
                                        _Times[hh] = (double)tempInt32Vector[hh];
                                    }
                                }
                                else if (_InternalData.Variables[MonthSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "int16")
                                {
                                    // Read the temporal dimension data to a temporary vector
                                    tempInt16Vector = _InternalData.GetData<Int16[]>(MonthSearchStrings[kk]);
                                    // Convert the dimension data to double format and add to the vector of dimension values
                                    _Times = new double[_NumTimes];
                                    for (int hh = 0; hh < tempInt16Vector.Length; hh++)
                                    {
                                        _Times[hh] = (double)tempInt16Vector[hh];
                                    }
                                }
                                else
                                {
                                    // Data format unrecognized, so throw an error
                                    Debug.Fail("Unrecognized data format for time dimension");
                                }
                            }
                            else
                            {
                                // Didn't find a plausible match for temporal dimension data, so throw an error
                                Debug.Fail("Cannot find any variables that look like a monthly temporal dimension");
                            }

                            // Convert the values of the time dimension to equal integers between 1 and 12 (the format currently recognized by the model)
                            for (int hh = 0; hh < _NumTimes; hh++)
                            {
                                if (_Times[hh] != hh + 1) _Times[hh] = hh + 1;
                            }
                            // Get the latitudinal and longitudinal sizes of grid cells
                            _LatStep = (_Lats[1] - _Lats[0]);
                            _LonStep = (_Lons[1] - _Lons[0]);
                            // Convert vectors of latitude and longutiude dimension data from cell-centre references to bottom-left references
                            for (int ii = 0; ii < _Lats.Length; ii++)
                            {
                                _Lats[ii] = _Lats[ii] - (_LatStep / 2);
                            }
                            for (int jj = 0; jj < _Lons.Length; jj++)
                            {
                                _Lons[jj] = _Lons[jj] - (_LonStep / 2);
                            }
                            // Check whether latitudes and longitudes are inverted in the NetCDF file
                            LatInverted = (_Lats[1] < _Lats[0]);
                            LongInverted = (_Lons[1] < _Lons[0]);
                           
                            // Get longitudinal 'x' and latitudinal 'y' corners of the bottom left of the data grid
                            _LatMin = _Lats[0];
                            _LonMin = _Lons[0];
                            break;
                        default:
                            // Data type not recognized, so throw an error
                            Debug.Fail("Data type not supported");
                            break;
                    }
                    break;
                default:
                    // The model currently only supports variables with temporal resolution 'year' or 'month', so throw an error
                    Debug.Fail("Temporal resolution not supported");
                    break;
            }

            // Check to see whether the environmental variable has longitude values from 0 to 360, instead of -180 to 180 (which the model currently recognizes)
            if (_LonMin + (_NumLons * _LonStep) > 180.0)
            {
                // Convert the longitude values to be -180 to 180, instead of 0 to 360
                Utilities.ConvertToM180To180(_Lons);
                // Update the minimum longitude value accordingly
                _LonMin = _Lons.Min();
            }

            // Update the variable keeping track of the number of environmental data layers
            _NumEnviroLayers = _NumEnviroLayers + 1;

        }


        /// <summary>
        /// Reads in one year's worth of data from the file and copies the values to the grid cell environment
        /// </summary>
        /// <param name="gridCells"></param>
        /// <param name="cellList"></param>
        /// <param name="internalLayerName"></param>
        /// <param name="TimestepElapsed"></param>
        /// <param name="LatCellSize"></param>
        /// <param name="LonCellSize"></param>
        public void GetTemporalEnvironmentListofCells(GridCell[,] gridCells, List<uint[]> cellList, string internalLayerName, uint TimestepElapsed,
            float LatCellSize, float LonCellSize)
        {

            double[,,] DataArray  = EnvironmentListFromNetCDF3D((int)TimestepElapsed);
            bool data_missing;

            foreach (var c in cellList)
            {
                double[] TempData = new double[12];
                for (uint i = TimestepElapsed; i < TimestepElapsed+12; i++)
			    {
                    data_missing = false;
			    
                    TempData[i-TimestepElapsed] = GetValue(DataArray,gridCells[c[0],c[1]].CellEnvironment["Latitude"][0],
                    gridCells[c[0],c[1]].CellEnvironment["Longitude"][0],i-TimestepElapsed,out data_missing,
                    (double)LatCellSize,(double)LonCellSize);
                    if (data_missing) TempData[i] = _MissingValue;
			    }

                gridCells[c[0], c[1]].CellEnvironment[internalLayerName] = TempData;
            }
        }


        public void GetTemporalEnvironmentListofCells(GridCell gridCell, List<uint[]> cellList, string internalLayerName, uint TimestepElapsed,
            float LatCellSize, float LonCellSize)
        {

            double[, ,] DataArray = EnvironmentListFromNetCDF3D((int)TimestepElapsed);
            bool data_missing;

            foreach (var c in cellList)
            {
                double[] TempData = new double[12];
                for (uint i = TimestepElapsed; i < TimestepElapsed + 12; i++)
                {
                    data_missing = false;

                    TempData[i - TimestepElapsed] = GetValue(DataArray, gridCell.CellEnvironment["Latitude"][0],
                    gridCell.CellEnvironment["Longitude"][0], i - TimestepElapsed, out data_missing,
                    (double)LatCellSize, (double)LonCellSize);
                    if (data_missing) TempData[i] = _MissingValue;
                }

                gridCell.CellEnvironment[internalLayerName] = TempData;
            }
        }


        /// <summary>
        /// A method to extract the area weighted value of an environmental variable from the envirodata cells overlapped by the cell specified by lat and lon
        /// </summary>
        /// <param name="lat">Bottom latitude of cell to get value from</param>
        /// <param name="lon">Leftmost longitude of cell to get value from</param>
        /// <param name="timeInterval">The time interval to get the value from (i.e. the month, or 0 for yearly variables)</param>
        /// <param name="missingValue">Boolean to indicate whether the returned value is a missing value</param>
        /// <param name="latCellSize">The latitudinal size of cells in the model grid</param>
        /// <param name="lonCellSize">The longitudinal size of cells in the model grid</param>
        /// <returns>The area weighted value of an environmental variable from the envirodata cells overlapped by the cell specified by lat and lon</returns>
        private double GetValue(double[,,] dataArray, double lat, double lon, uint timeInterval, out Boolean missingValue, double latCellSize, double lonCellSize)
        {
            // Check that the requested latitude and longitude are within the scope of the environmental variable
            Debug.Assert(lat >= LatMin && lat < LatMin + (NumLats * LatStep), "Requested latitude is outside dataset latitude range: " + _ReadFileString);
            Debug.Assert(lon >= LonMin && lon < LonMin + (NumLons * LonStep), "Requested longitude is outside dataset longitude range: " + _ReadFileString);

            // TEMPP DT
            Boolean invertedLat = false;

            // Temporary variable for finding the shortest distance between the bottom latitude of the requested model grid cell and the latitude of cells in the environmental variable
            double ShortestLowerLatDistance = double.MinValue;
            // Variable to store the latitude index of the cell in the environmental variable that is the closest to the bottom latitude of the requested model grid cell
            int ClosestLowerLatIndex = -1;
            // Temporary variable for finding the shortest distance between the top latitude of the requested model grid cell and the latitude of cells in the environmental variable
            double ShortestUpperLatDistance = double.MinValue;
            // Variable to store the latitude index of the cell in the environmental variable that is the closest to the top latitude of the requested model grid cell
            int ClosestUpperLatIndex = -1;
            // A temporary variable for storing latitudinal distances between the requested latitude and grid cell latitudes
            double tempVal;

            if (_Lats[0] < _Lats[_Lats.Count() - 1]) invertedLat = true;

            if (invertedLat)
            {
                // Loop over latitude values of the environmental variable
                for (int ii = 0; ii != _Lats.Length; ++ii)
                {
                    // Get the distance between the latitude of this cell and the requested latitude
                    tempVal = lat - _Lats[ii];
                    // If this is shorter than the shortest distance that has been found so far, then store this cell as the closest to the requested latitude
                    if ((tempVal > ShortestLowerLatDistance) && (tempVal < 0.0))
                    {
                        // Update the shortest distance
                        ShortestLowerLatDistance = (lat - _Lats[ii]);
                        // Store this latitude index as being the closest to the latitude requested
                        ClosestLowerLatIndex = ii - 1;
                    }
                    //Get the distance between the top latitude of the requested model grid cell latitude and this EnviroData layer grid cell
                    tempVal = lat + latCellSize - _Lats[ii];
                    if ((tempVal > ShortestUpperLatDistance) && (tempVal < 0.0))
                    {
                        //Update the shortest upper distance
                        ShortestUpperLatDistance = (lat + latCellSize - _Lats[ii]);
                        //store this latitude index
                        ClosestUpperLatIndex = ii - 1;
                    }
                }


                //If haven't found the ClosestLowerLatIndex then this is because the grid cell starts at or beyond the lower limit of the highest enviro grid
                //So set the ClosestLowerLatIndex equal to the last index
                if (ClosestLowerLatIndex == -1)
                {
                    // and set the ClosestUpperLatIndex equal to the the last index (the highest latitude cell)
                    ClosestLowerLatIndex = _Lats.Length - 1;
                }

                //If haven't found the ClosestUpperLatIndex then this is because the grid cell extends to or beyond the upper limit of the enviro grid
                //So set the ClosestUpperLatIndex equal to the last index
                if (ClosestUpperLatIndex == -1)
                {
                    // and set the ClosestUpperLatIndex equal to the the last index (the highest latitude cell)
                    ClosestUpperLatIndex = _Lats.Length - 1;
                }


            }
            else
            {
                // Loop over latitude values of the environmental variable
                for (int ii = 0; ii != _Lats.Length; ++ii)
                {
                    // Get the distance between the latitude of this cell and the requested latitude
                    tempVal = lat - _Lats[ii];
                    // If this is shorter than the shortest distance that has been found so far, then store this cell as the closest to the requested latitude
                    if ((tempVal < ShortestLowerLatDistance) && (tempVal > 0.0))
                    {
                        // Update the shortest distance
                        ShortestLowerLatDistance = (lat - _Lats[ii]);
                        // Store this latitude index as being the closest to the latitude requested
                        ClosestLowerLatIndex = ii - 1;
                    }
                    //Get the distance between the top latitude of the requested model grid cell latitude and this EnviroData layer grid cell
                    tempVal = lat + latCellSize - _Lats[ii];
                    if ((tempVal < ShortestUpperLatDistance) && (tempVal > 0.0))
                    {
                        //Update the shortest upper distance
                        ShortestUpperLatDistance = (lat + latCellSize - _Lats[ii]);
                        //store this latitude index
                        ClosestUpperLatIndex = ii - 1;
                    }
                }


                //If haven't found the ClosestLowerLatIndex then this is because the grid cell extends to or beyond the lower limit of the enviro grid
                //So set the ClosestUpperLatIndex equal to the index of the highest latitude
                if (ClosestLowerLatIndex == -1)
                {
                    // So take the ShortestUpperLatDistance as 0 
                    ShortestLowerLatDistance = 0.0;
                    // and set the ClosestLowerLatIndex equal to the last index value (the lowest latitude cell)
                    ClosestLowerLatIndex = _Lats.Length - 1;
                }

                //If haven't found the ClosestUpperLatIndex then this is because the grid cell extends to or beyond the upper limit of the enviro grid
                //So set the ClosestUpperLatIndex equal to the last index
                if (ClosestUpperLatIndex == -1)
                {
                    // and set the ClosestUpperLatIndex equal to the the last index (the highest latitude cell)
                    ClosestUpperLatIndex = _Lats.Length - 1;
                }

            }


            // Adjust to correct for potential problems with ESRI ASCII grids starting latitudes at -90 and going upwards,
            // instead of netCDFs, which do the inverse
            //if (closestUpperLatIndex < closestLowerLatIndex)
            //{
            //    invertedLat = 1;
            //}

            //Calculate the number of EnviroData cells that are overlapped by the requested model grid cell
            int NumOverlappedLatCells = Math.Abs(ClosestUpperLatIndex - ClosestLowerLatIndex) + 1;


            // Temporary variable for finding the shortest distance between the leftmost longitude of requested model grid cell and the longitude of cells in the environmental variable
            double shortestLeftmostLonDistance = double.MaxValue;
            // Variable to store the longitude index of the cell in the environmental variable that is the closest to the leftmost longitude of the requested model grid cell
            int closestLeftmostLonIndex = 0;
            // Temporary variable for finding the shortest distance between the rightmost longitude of requested model grid cell and the longitude of cells in the environmental variable
            double shortestRightmostLonDistance = double.MaxValue;
            // Variable to store the longitude index of the cell in the environmental variable that is the closest to the rightmost longitude of the requested model grid cell
            int closestRightmostLonIndex = 0;
            // Loop over longitude values of the environmental variable
            for (int ii = 0; ii != _Lons.Length; ++ii)
            {
                // Get the distance between the longitude of this cell and the requested longitude
                tempVal = lon - _Lons[ii];
                // If this is shorter than the shortest distance that has been found so far, then store this cell as the closest to the requested longitude
                if ((tempVal < shortestLeftmostLonDistance) && (tempVal >= 0.0))
                {
                    // Update the shortest distance
                    shortestLeftmostLonDistance = (lon - _Lons[ii]);
                    // Store this latitude index as being the closest to the latitude requested
                    closestLeftmostLonIndex = ii;
                }
                //Get the distance between the leftmost longitude of the model grid cell requested and this envirodata cell leftmost longitude
                tempVal = lon + lonCellSize - _Lons[ii];
                if ((tempVal < shortestRightmostLonDistance) && (tempVal >= 0.0))
                {
                    shortestRightmostLonDistance = (lon + lonCellSize - _Lons[ii]);
                    closestRightmostLonIndex = ii;
                }
            }


            //Calculate the number of EnviroData cells that are overlapped by the requested model grid cell
            int NumOverlappedLonCells = Math.Abs(closestRightmostLonIndex - closestLeftmostLonIndex) + 1;

            //Array to hold the area of each of this EnviroData layer cell that is overlapped by the requested model grid cell
            double[,] OverlapAreas = new double[NumOverlappedLatCells, NumOverlappedLonCells];

            //Variable to hold the cumulative area of overlapping region
            Double CumulativeNonMissingValueOverlapArea = 0.0;
            //Variable to hold the bottom lat overlap for each grid cell
            double CellBottomLatOverlap = 0.0;
            //Variable to hold the leftmost lon overlap for each grid cell
            double CellLeftmostLonOverlap = 0.0;
            //Variable to hold the upper lat overlap for each grid cell
            double CellTopLatOverlap = 0.0;
            //Variable to hold the rightmost lon overlap for each grid cell
            double CellRightmostLonOverlap = 0.0;
            //Variable to hold the longitudinal overlap size
            double OverlapLonSize;
            //Variable to hold the latitudinal overlap size
            double OverlapLatSize;


            if (invertedLat)
            {
                for (int ii = ClosestLowerLatIndex; ii <= ClosestUpperLatIndex; ii++)
                {
                    if (ii == ClosestLowerLatIndex)
                    {
                        //The lower lat overlap for this band of enviro data cells is the bottom of the requested model grid cell
                        CellBottomLatOverlap = lat;
                    }
                    else
                    {
                        //the enviro data cells fall totally within the footprint of the requested model grid cell so use the latitude extents of the current band of cells
                        CellBottomLatOverlap = _Lats[ii];
                    }
                    if (ii == ClosestUpperLatIndex)
                    {
                        //The upper lat overlap for this band of enviro data cells is the uppermost extent of the requested model grid cell
                        CellTopLatOverlap = lat + latCellSize;
                    }
                    else
                    {
                        //The upper lat for this band of envirodata cells is the top of the current enviro data cells
                        CellTopLatOverlap = _Lats[ii] + LatStep;
                    }


                    for (int jj = closestLeftmostLonIndex; jj <= closestRightmostLonIndex; jj++)
                    {
                        if (jj == closestLeftmostLonIndex)
                        {
                            //The leftmost lon overlap for this cell is the leftmost extent of the requested model grid cell
                            CellLeftmostLonOverlap = lon;
                        }
                        else
                        {
                            //The leftmost lat overlap for this cell is the leftmost extent of this enviro data cell
                            CellLeftmostLonOverlap = _Lons[jj];
                        }
                        if (jj == closestRightmostLonIndex)
                        {
                            //The rightmost lat overlap for this cell is the rightmost extent of this requested model grid cell
                            CellRightmostLonOverlap = lon + lonCellSize;
                        }
                        else
                        {
                            //The rightmost lat overlap for this cell is the rightmost extent of this enviro data cell
                            CellRightmostLonOverlap = _Lons[jj] + LonStep;
                        }

                        OverlapLonSize = CellRightmostLonOverlap - CellLeftmostLonOverlap;
                        OverlapLatSize = CellTopLatOverlap - CellBottomLatOverlap;

                        //Given the lat and lon extents of the overlapping region of this grid cell then calculate the area of this grid cell that is overlapped
                        OverlapAreas[ii - ClosestLowerLatIndex, jj - closestLeftmostLonIndex] = Utilities.CalculateGridCellArea(CellBottomLatOverlap, OverlapLonSize, OverlapLatSize);
                        if (dataArray[ii, jj,(int)timeInterval].CompareTo(_MissingValue) != 0.0)
                            CumulativeNonMissingValueOverlapArea += OverlapAreas[ii - ClosestLowerLatIndex, jj - closestLeftmostLonIndex];
                    }
                }
            }
            else
            {
                for (int ii = ClosestUpperLatIndex; ii <= ClosestLowerLatIndex; ii++)
                {
                    if (ii == ClosestLowerLatIndex)
                    {
                        //The lower lat overlap for this band of enviro data cells is the bottom of the requested model grid cell
                        CellBottomLatOverlap = lat;
                    }
                    else
                    {
                        //the enviro data cells fall totally within the footprint of the requested model grid cell so use the latitude extents of the current band of cells
                        CellBottomLatOverlap = _Lats[ii];
                    }
                    if (ii == ClosestUpperLatIndex)
                    {
                        //The upper lat overlap for this band of enviro data cells is the uppermost extent of the requested model grid cell
                        CellTopLatOverlap = lat + latCellSize;
                    }
                    else
                    {
                        //The upper lat for this band of envirodata cells is the top of the current enviro data cells
                        CellTopLatOverlap = _Lats[ii] + LatStep;
                    }


                    for (int jj = closestLeftmostLonIndex; jj <= closestRightmostLonIndex; jj++)
                    {
                        if (jj == closestLeftmostLonIndex)
                        {
                            //The leftmost lon overlap for this cell is the leftmost extent of the requested model grid cell
                            CellLeftmostLonOverlap = lon;
                        }
                        else
                        {
                            //The leftmost lat overlap for this cell is the leftmost extent of this enviro data cell
                            CellLeftmostLonOverlap = _Lons[jj];
                        }
                        if (jj == closestRightmostLonIndex)
                        {
                            //The rightmost lat overlap for this cell is the rightmost extent of this requested model grid cell
                            CellRightmostLonOverlap = lon + lonCellSize;
                        }
                        else
                        {
                            //The rightmost lat overlap for this cell is the rightmost extent of this enviro data cell
                            CellRightmostLonOverlap = _Lons[jj] + LonStep;
                        }

                        OverlapLonSize = CellRightmostLonOverlap - CellLeftmostLonOverlap;
                        OverlapLatSize = CellTopLatOverlap - CellBottomLatOverlap;

                        //Given the lat and lon extents of the overlapping region of this grid cell then calculate the area of this grid cell that is overlapped
                        OverlapAreas[ClosestLowerLatIndex - ii, jj - closestLeftmostLonIndex] = Utilities.CalculateGridCellArea(CellBottomLatOverlap, OverlapLonSize, OverlapLatSize);
                        if (dataArray[ii, jj,(int)timeInterval].CompareTo(_MissingValue) != 0.0)
                            CumulativeNonMissingValueOverlapArea += OverlapAreas[ClosestLowerLatIndex - ii, jj - closestLeftmostLonIndex];
                    }
                }
            }
            //Variable to hold the weighted average value of the enviro data cells overlapped by the requested model grid cell
            double WeightedValue = 0.0;
            missingValue = false;

            if (invertedLat)
            {
                if (CumulativeNonMissingValueOverlapArea.CompareTo(0.0) == 0)
                {
                    missingValue = true;
                    WeightedValue = this.MissingValue;
                }
                else
                {
                    for (int ii = ClosestLowerLatIndex; ii <= ClosestUpperLatIndex; ii++)
                    {
                        for (int jj = closestLeftmostLonIndex; jj <= closestRightmostLonIndex; jj++)
                        {
                            if (dataArray[ii, jj,(int)timeInterval].CompareTo(_MissingValue) != 0)
                            {
                                WeightedValue += dataArray[ii, jj,(int)timeInterval] * OverlapAreas[ii - ClosestLowerLatIndex, jj - closestLeftmostLonIndex] / CumulativeNonMissingValueOverlapArea;
                            }
                        }
                    }
                }
            }
            else
            {
                if (CumulativeNonMissingValueOverlapArea.CompareTo(0.0) == 0)
                {
                    missingValue = true;
                    WeightedValue = this.MissingValue;
                }
                else
                {
                    for (int ii = ClosestUpperLatIndex; ii <= ClosestLowerLatIndex; ii++)
                    {
                        for (int jj = closestLeftmostLonIndex; jj <= closestRightmostLonIndex; jj++)
                        {
                            if (dataArray[ii, jj,(int)timeInterval].CompareTo(_MissingValue) != 0)
                            {
                                WeightedValue += dataArray[ii, jj,(int)timeInterval] * OverlapAreas[ClosestLowerLatIndex - ii, jj - closestLeftmostLonIndex] / CumulativeNonMissingValueOverlapArea;
                            }
                        }
                    }
                }
            }

            //Return the weighted average
            return WeightedValue;
        }


        /// <summary>
        /// Reads in three-dimensional environmental data from a NetCDF and stores them in the array of values within this instance of EnviroData
        /// NEED TO MAKE THIS WORK FOR TEMPORAL RESOLUTIONS DIFFERENT FROM MONTH!!
        /// </summary>
        /// <param name="_InternalData">The SDS object to get data from</param>
        /// <param name="dataName">The name of the variable within the NetCDF file</param>
        /// <param name="LatInverted">Whether the latitude values are inverted in the NetCDF file (i.e. large to small values)</param>
        /// <param name="LongInverted">Whether the longitude values are inverted in the NetCDF file (i.e. large to small values)</param>
        private double[,,] EnvironmentListFromNetCDF3D(int timestep_elapsed)
        {
            
            // Array to store environmental data from netcdf, but with data in ascending order of both latitude and longitude
            int number_time_steps_per_year = 12;
            switch(_DataResolution)
            {
                case "year":
                    number_time_steps_per_year = 1;
                    break;
                case "month":
                    number_time_steps_per_year = 12;
                    break;
            }
            double[,,] LatLongArraySorted = new double[_NumLats, _NumLons,number_time_steps_per_year];

            // Vector to hold the position in the dimensions of the NetCDF file of the latitude, longitude and third dimensions
            int[] positions = new int[3];

            // Array to store environmental data with latitude as the first dimension and longitude as the second dimension
            double[,] LatLongArrayUnsorted = new double[_NumLats, _NumLons];

            // Check that the requested variable exists in the NetCDF file
            Debug.Assert(_InternalData.Variables.Contains(_DataName), "Requested variable does not exist in the specified file");

            // Check that the environmental variable in the NetCDF file has three dimensions
            Debug.Assert(_InternalData.Variables[_DataName].Dimensions.Count == 3, "The specified variable in the NetCDF file does not have three dimensions, which is the required number for this method");

            // Possible names for the missing value metadata in the NetCDF file
            string[] SearchStrings = { "missing_value", "MissingValue", "_FillValue" };

            // Loop over possible names for the missing value metadata until a match is found in the NetCDF file
            int kk = 0;
            while ((kk < SearchStrings.Length) & (!_InternalData.Variables[_DataName].Metadata.ContainsKey(SearchStrings[kk]))) kk++;

            // If a match is found, then set the missing data field equal to the value in the file, otherwise throw an error
            if (kk < SearchStrings.Length)
            {
                _MissingValue = Convert.ToDouble(_InternalData.Variables[_DataName].Metadata[SearchStrings[kk]]);
            }
            else
            {
                Debug.Fail("No missing data value found for environmental data file: " + _InternalData.Name.ToString());
            }

            // Possible names for the latitude dimension in the NetCDF file
            SearchStrings = new string[] { "lat", "Lat", "latitude", "Latitude", "lats", "Lats", "latitudes", "Latitudes", "y" };
            // Check which position the latitude dimension is in in the NetCDF file and add this to the vector of positions. If the latitude dimension cannot be
            // found then throw an error
            if (SearchStrings.Contains(_InternalData.Dimensions[0].Name.ToString()))
            {
                positions[0] = 1;
            }
            else if (SearchStrings.Contains(_InternalData.Dimensions[1].Name.ToString()))
            {
                positions[1] = 1;
            }
            else if (SearchStrings.Contains(_InternalData.Dimensions[2].Name.ToString()))
            {
                positions[2] = 1;
            }
            else
            {
                Debug.Fail("Cannot find a latitude dimension");
            }

            // Possible names for the longitude dimension in the netCDF file
            SearchStrings = new string[] { "lon", "Lon", "longitude", "Longitude", "lons", "Lons", "long", "Long", "longs", "Longs", "longitudes", "Longitudes", "x" };
            // Check which position the latitude dimension is in in the NetCDF file and add this to the vector of positions. If the latitude dimension cannot be
            // found then throw an error
            if (SearchStrings.Contains(_InternalData.Dimensions[0].Name.ToString()))
            {
                positions[0] = 2;
            }
            else if (SearchStrings.Contains(_InternalData.Dimensions[1].Name.ToString()))
            {
                positions[1] = 2;
            }
            else if (SearchStrings.Contains(_InternalData.Dimensions[2].Name.ToString()))
            {
                positions[2] = 2;
            }
            else
            {
                Debug.Fail("Cannot find a longitude dimension");
            }

            // Possible names for the monthly temporal dimension in the netCDF file
            SearchStrings = new string[] { "time","month", "Month", "months", "Months" };
            // Check which position the temporal dimension is in in the NetCDF file and add this to the vector of positions. If the temporal dimension cannot be
            // found then throw an error
            if (SearchStrings.Contains(_InternalData.Dimensions[0].Name.ToString()))
            {
                positions[0] = 3;
            }
            else if (SearchStrings.Contains(_InternalData.Dimensions[1].Name.ToString()))
            {
                positions[1] = 3;
            }
            else if (SearchStrings.Contains(_InternalData.Dimensions[2].Name.ToString()))
            {
                positions[2] = 3;
            }

            // Check the format of the specified environmental variable
            if (_InternalData.Variables[_DataName].TypeOfData.Name.ToString().ToLower() == "single")
            {
                Single[, ,] TempArray;
                // Read the environmental data into a temporary array
                if(_InternalData.Variables[_DataName].Dimensions.Count  == 4)
                {
                    Single[,,,] TempArray4;
                    TempArray4 = _InternalData.GetData<Single[,,,]>(_DataName);
                    TempArray = new Single[TempArray4.GetLength(0), TempArray4.GetLength(2), TempArray4.GetLength(3)];
                    for (int i = 0; i < TempArray4.GetLength(0); i++)
                    {
                        for (int j = 0; j < TempArray4.GetLength(2); j++)
                        {
                            for (int k = 0; k < TempArray4.GetLength(3); k++)
                            {
                                TempArray[i, j, k] = TempArray4[i, 0, j, k];
                            }
                        }
                    }
                    
                }
                else
                {
                    TempArray = _InternalData.GetData<Single[, ,]>(_DataName);
                }


                // Revised for speed
                switch (positions[0])
                {
                    case 1:
                        switch (positions[1])
                        {
                            case 2:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[ii, jj, hh];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    
                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[ii, hh, jj];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    
                                }
                                break;
                            default:
                                Debug.Fail("Failure detecting latitude dimension");
                                break;
                        }
                        break;
                    case 2:
                        switch (positions[1])
                        {
                            case 1:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[jj, ii, hh];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, jj];
                                                }
                                            }
                                        }
                                    }

                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[jj, hh, ii];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    
                                }
                                break;
                            default:
                                Debug.Fail("Failure detecting latitude dimension");
                                break;
                        }
                        break;
                    case 3:
                        switch (positions[1])
                        {
                            case 1:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[hh, ii, jj];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, jj];
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case 2:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[hh, jj, ii];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, jj];
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                Debug.Fail("Failure detecting latitude dimension");
                                break;
                        }
                        break;
                    default:
                        Debug.Fail("Failure detecting latitude dimension");
                        break;
                }

            }
            else if (_InternalData.Variables[_DataName].TypeOfData.Name.ToString().ToLower() == "double")
            {
                // Read the environmental data into a temporary array
                double[, ,] TempArray;
                TempArray = _InternalData.GetData<double[, ,]>(_DataName);
                // Revised for speed
                switch (positions[0])
                {
                    case 1:
                        switch (positions[1])
                        {
                            case 2:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = TempArray[ii, jj, hh];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }

                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = TempArray[ii, hh, jj];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                Debug.Fail("Failure detecting latitude dimension");
                                break;
                        }
                        break;
                    case 2:
                        switch (positions[1])
                        {
                            case 1:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = TempArray[jj, ii, hh];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }

                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = TempArray[jj, hh, ii];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                Debug.Fail("Failure detecting latitude dimension");
                                break;
                        }
                        break;
                    case 3:
                        switch (positions[1])
                        {
                            case 1:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = TempArray[hh, ii, jj];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }

                                }
                                break;
                            case 2:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = TempArray[hh, jj, ii];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                Debug.Fail("Failure detecting latitude dimension");
                                break;
                        }
                        break;
                    default:
                        Debug.Fail("Failure detecting latitude dimension");
                        break;
                }
            }
            else if (_InternalData.Variables[_DataName].TypeOfData.Name.ToString().ToLower() == "int32")
            {
                // Read the environmental data into a temporary array
                Int32[, ,] TempArray;
                TempArray = _InternalData.GetData<Int32[, ,]>(_DataName);
                  // Revised for speed
                switch (positions[0])
                {
                    case 1:
                        switch (positions[1])
                        {
                            case 2:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[ii, jj, hh];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }
                                    
                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[ii, hh, jj];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }
                                    
                                }
                                break;
                            default:
                                Debug.Fail("Failure detecting latitude dimension");
                                break;
                        }
                        break;
                    case 2:
                        switch (positions[1])
                        {
                            case 1:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[jj, ii, hh];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }

                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[jj, hh, ii];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }
                                    
                                }
                                break;
                            default:
                                Debug.Fail("Failure detecting latitude dimension");
                                break;
                        }
                        break;
                    case 3:
                        switch (positions[1])
                        {
                            case 1:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[hh, ii, jj];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case 2:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[hh, jj, ii];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                Debug.Fail("Failure detecting latitude dimension");
                                break;
                        }
                        break;
                    default:
                        Debug.Fail("Failure detecting latitude dimension");
                        break;
                }

            }
            else if (_InternalData.Variables[_DataName].TypeOfData.Name.ToString().ToLower() == "int16")
            {
                // Read the environmental data into a temporary array
                Int16[, ,] TempArray;
                TempArray = _InternalData.GetData<Int16[, ,]>(_DataName);

                  // Revised for speed
                switch (positions[0])
                {
                    case 1:
                        switch (positions[1])
                        {
                            case 2:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[ii, jj, hh];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }
                                    
                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[ii, hh, jj];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }
                                    
                                }
                                break;
                            default:
                                Debug.Fail("Failure detecting latitude dimension");
                                break;
                        }
                        break;
                    case 2:
                        switch (positions[1])
                        {
                            case 1:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[jj, ii, hh];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }

                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[jj, hh, ii];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }
                                    
                                }
                                break;
                            default:
                                Debug.Fail("Failure detecting latitude dimension");
                                break;
                        }
                        break;
                    case 3:
                        switch (positions[1])
                        {
                            case 1:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[hh, ii, jj];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case 2:
                                // Loop over time steps
                                for (int hh = timestep_elapsed; hh < timestep_elapsed+12; hh++)
                                {
                                    // Add to the unsorted array of values, transposing the data to be dimensioned by latitude first and 
                                    // longitude second
                                    for (int ii = 0; ii < _NumLats; ii++)
                                    {
                                        for (int jj = 0; jj < _NumLons; jj++)
                                        {
                                            LatLongArrayUnsorted[ii, jj] = (double)TempArray[hh, jj, ii];
                                        }
                                    }

                                    // Transpose the environmental data so that they are in ascending order of both latitude and longitude
                                    if (LatInverted)
                                    {
                                        if (LongInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }

                                        else
                                        {
                                            // Latitude only inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (LongInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj,hh-timestep_elapsed] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                Debug.Fail("Failure detecting latitude dimension");
                                break;
                        }
                        break;
                    default:
                        Debug.Fail("Failure detecting latitude dimension");
                        break;
                }

            }
            else
            {
                // Format of environmental data not recognized so throw an error
                Debug.Fail("Environmental data are in an unrecognized format");
            }

            // If either latitude or longitude were inverted, then reverse their values in the class fields
            if (LatInverted)
            {
                // Temporary vector to store inverted latitude values
                double[] tempLats = new double[_NumLats];
                // Loop over latitude values
                for (int ii = 0; ii < _NumLats; ii++)
                {
                    // Invert the values in the temporary vector
                    tempLats[ii] = _Lats[_Lats.Length - 1 - ii];
                }
                // Overwrite the old vector of latitude values with the inverted values
                _Lats = tempLats;
                // Reverse the sign on the difference in latitude values between adjacent cells
                _LatStep = -_LatStep;
            }
            if (LongInverted)
            {
                // Temporary vector to store inverted longitude values
                double[] tempLongs = new double[_NumLons];
                // Loop over longitude values
                for (int jj = 0; jj < _NumLons; jj++)
                {
                    // Invert the values in the temporary vector
                    tempLongs[jj] = _Lons[_Lons.Length - 1 - jj];
                }
                // Overwrite the old vector of longitude values with the inverted values
                _Lons = tempLongs;
                // Reverse the sign on the difference in longitude values between adjacent cells
                _LonStep = -_LonStep;
            }

            // Check that the increment in both latitudes and longitudes between consecutive grid cells is now positive
            Debug.Assert(_LatStep > 0.0, "Latitudes are still inverted in an environmental variable stored in EnviroData");
            Debug.Assert(_LonStep > 0.0, "Longitudes are still inverted in an environmental variable stored in EnviroData");

            return(LatLongArraySorted);

        }

        /// <summary>
        /// Dispose of an Envirodata instance
        /// </summary>
        ~EnviroDataTemporal()
        {
            
        }

    }
}

