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
    /// <summary>
    /// Imports environmental data from ASCII and NetCDF files
    /// </summary>
    /// <todoT>No error-trapping as yet</todoT>
    /// <todoT>Rewrite to use the ArraySDSConvert class</todoT>
    /// <todoD>Need  to go through code and rewrite e.g. change method to overloaded to prevent passing variable name and file name for ESRI grids</todoD>
    /// <remarks>Currently assumes that cells are evenly spaced in latitude and longitude</remarks>
    public class EnviroData
    {
        /// <summary>
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
        /// List of arrays of values of the environmental variable
        /// </summary>
        private List<double[,]> _DataArray;
        /// <summary>
        /// Get list of arrays of values of the environmental variable
        /// </summary>
        public List<double[,]> DataArray { get { return _DataArray; } }

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

        /// <summary>
        /// Overloaded constructor to fetch climate information from the cloud using FetchClimate
        /// </summary>
        /// <param name="dataName">Name of the the climate variable to be fetched</param>
        /// <param name="dataResolution">Time resolution requested</param>
        /// <param name="latMin">Bottom latitude</param>
        /// <param name="lonMin">Leftmost longitude</param>
        /// <param name="latMax">Maximum latitude</param>
        /// <param name="lonMax">Maximum longitude</param>
        /// <param name="cellSize">Size of each grid cell</param>
        /// <param name="FetchClimateDataSource">Data source from which to fetch environmental data</param>
        public EnviroData(string dataName, string dataResolution, double latMin, double lonMin, double latMax, double lonMax, double cellSize,
            EnvironmentalDataSource FetchClimateDataSource)
        {
            Console.WriteLine("Fetching environmental data for: " + dataName + " with resolution " + dataResolution);

            // Initialise the utility functions
            Utilities = new UtilityFunctions();

            _NumLats = Convert.ToUInt32((latMax - latMin) / cellSize);
            _NumLons = Convert.ToUInt32((lonMax - lonMin) / cellSize);
            _LatMin = latMin;
            _LonMin = lonMin;

            _Lats = new double[_NumLats];
            _Lons = new double[_NumLons];

            for (int ii = 0; ii < _NumLats; ii++)
            {
                _Lats[ii] = Math.Round(_LatMin + (ii * cellSize), 2);
            }
            for (int jj = 0; jj < _NumLons; jj++)
            {
                _Lons[jj] = Math.Round(_LonMin + (jj * cellSize), 2);
            }

            _LatStep = Math.Round(Lats[1] - _Lats[0], 2);
            _LonStep = Math.Round(Lons[1] - Lons[0], 2);

            //Declare a dataset to perform the fetch
            var ds = DataSet.Open("msds:memory2");
            //Add lat and lon information to the dataset
            ds.AddAxisCells("Latitude", "degrees", _LatMin, latMax + cellSize, cellSize); //copying Latitude and Longitude variables into new dataset            
            ds.AddAxisCells("Longitude", "degrees", _LonMin, lonMax + cellSize, cellSize);

            //Add the required time dimension to the dataset
            switch (dataResolution)
            {
                case "year":
                    ds.AddClimatologyAxisYearly(yearmin: 1961, yearmax: 1990, yearStep: 30);
                    break;
                case "month":
                    ds.AddClimatologyAxisMonthly();
                    break;
                default:
                    break;
            }

            double[, ,] temp = null;
            _DataArray = new List<double[,]>();

            //Fetch for the required data
            switch (dataName.ToLower())
            {
                case "land_dtr":
                    ds.Fetch(ClimateParameter.FC_LAND_DIURNAL_TEMPERATURE_RANGE, "landdtr", dataSource: FetchClimateDataSource); //this call will create 2D variable on dimensions records and months and fill it with a FetchClimate
                    //int NumberOfRecords = ds.Dimensions["RecordNumber"].Length; // get number of records     
                    temp = (double[, ,])ds.Variables["landdtr"].GetData();
                    _MissingValue = (double)ds.Variables["landdtr"].GetMissingValue();
                    break;
                case "temperature":
                    ds.Fetch(ClimateParameter.FC_TEMPERATURE, "airt", dataSource: FetchClimateDataSource); //this call will create 2D variable on dimensions records and months and fill it with a FetchClimate
                    //int NumberOfRecords = ds.Dimensions["RecordNumber"].Length; // get number of records     
                    temp = (double[, ,])ds.Variables["airt"].GetData();
                    _MissingValue = (double)ds.Variables["airt"].GetMissingValue();
                    break;
                // Commenting out ocean air temperature because it is running too slow when using FetchClimate
                case "temperature_ocean":
                    ds.Fetch(ClimateParameter.FC_OCEAN_AIR_TEMPERATURE, "oceanairt", dataSource: FetchClimateDataSource); //this call will create 2D variable on dimensions records and months and fill it with a FetchClimate
                    //int NumberOfRecords = ds.Dimensions["RecordNumber"].Length; // get number of records     
                    temp = (double[, ,])ds.Variables["oceanairt"].GetData();
                    _MissingValue = (double)ds.Variables["oceanairt"].GetMissingValue();
                    break;
                case "precipitation":
                    ds.Fetch(ClimateParameter.FC_PRECIPITATION, "precip", dataSource: FetchClimateDataSource); //this call will create 2D variable on dimensions records and months and fill it with a FetchClimate
                    //int NumberOfRecords = ds.Dimensions["RecordNumber"].Length; // get number of records     
                    temp = (double[, ,])ds.Variables["precip"].GetData();
                    _MissingValue = (double)ds.Variables["precip"].GetMissingValue();
                    break;
                case "frost":
                    ds.Fetch(ClimateParameter.FC_LAND_FROST_DAY_FREQUENCY, "frost", dataSource: FetchClimateDataSource);
                    temp = (double[, ,])ds.Variables["frost"].GetData();
                    _MissingValue = (double)ds.Variables["frost"].GetMissingValue();
                    break;
                default:
                    Debug.Fail("No Enviro data read in for " + dataName);
                    break;
            }

            _NumTimes = (uint)ds.Dimensions["time"].Length;

            //Add the fetched data to the Envirodata array
            for (int tt = 0; tt < _NumTimes; tt++)
            {
                double[,] TempArray = new double[_NumLats, _NumLons];
                for (int ii = 0; ii < _NumLats; ii++)
                {
                    for (int jj = 0; jj < _NumLons; jj++)
                    {
                        // Currently FetchClimate returns longitudes as the last array dimension
                        TempArray[ii, jj] = temp[tt, ii, jj];
                    }
                }
                _DataArray.Add(TempArray);
            }

            //DataSet Out = ds.Clone("output/" + dataName + ".nc");
            //Out.Dispose();
            ds.Dispose();

        }



        /// <summary>
        /// Overloaded constructor to fetch climate information from the cloud using FetchClimate for specific locations
        /// </summary>
        /// <param name="dataName">Name of the the climate variable to be fetched</param>
        /// <param name="dataResolution">Time resolution requested</param>
        /// <param name="latMin">Bottom latitude</param>
        /// <param name="lonMin">Leftmost longitude</param>
        /// <param name="latMax">Maximum latitude</param>
        /// <param name="lonMax">Maximum longitude</param>
        /// <param name="cellSize">Size of each grid cell</param>
        /// <param name = "cellList">List of cells to be fetched</param>
        /// <param name="FetchClimateDataSource">Data source from which to fetch environmental data</param>
        public EnviroData(string dataName, string dataResolution, double latMin, double lonMin, double latMax, double lonMax, double cellSize,
            List<uint[]> cellList,
            EnvironmentalDataSource FetchClimateDataSource)
        {
            Console.WriteLine("Fetching environmental data for: " + dataName + " with resolution " + dataResolution);

            // Initialise the utility functions
            Utilities = new UtilityFunctions();

            _NumLats = Convert.ToUInt32((latMax - latMin) / cellSize);
            _NumLons = Convert.ToUInt32((lonMax - lonMin) / cellSize);
            _LatMin = latMin;
            _LonMin = lonMin;

            _Lats = new double[_NumLats];
            _Lons = new double[_NumLons];

            for (int ii = 0; ii < _NumLats; ii++)
            {
                _Lats[ii] = Math.Round(_LatMin + (ii * cellSize), 2);
            }
            for (int jj = 0; jj < _NumLons; jj++)
            {
                _Lons[jj] = Math.Round(_LonMin + (jj * cellSize), 2);
            }

            _LatStep = Math.Round(Lats[1] - _Lats[0], 2);
            _LonStep = Math.Round(Lons[1] - Lons[0], 2);

            //Declare a dataset to perform the fetch
            var ds = DataSet.Open("msds:memory2");

            _DataArray = new List<double[,]>();

            //Add the required time dimension to the dataset
            switch (dataResolution)
            {
                case "year":
                    ds.AddClimatologyAxisYearly(yearmin: 1961, yearmax: 1990, yearStep: 30);
                    break;
                case "month":
                    ds.AddClimatologyAxisMonthly();
                    break;
                default:
                    break;
            }

            //Add lat and lon information to the dataset
            for (int ii = 0; ii < cellList.Count; ii++)
            {
                ds.AddAxisCells("longitude", "degrees_east", _Lons[cellList[ii][1]], Lons[cellList[ii][1]] + cellSize, cellSize);
                ds.AddAxisCells("latitude", "degrees_north", _Lats[cellList[ii][0]], _Lats[cellList[ii][0]] + cellSize, cellSize);

                double[, ,] temp = null;


                //Fetch for the required data
                switch (dataName.ToLower())
                {
                    case "land_dtr":
                        ds.Fetch(ClimateParameter.FC_LAND_DIURNAL_TEMPERATURE_RANGE, "landdtr", dataSource: FetchClimateDataSource); //this call will create 2D variable on dimensions records and months and fill it with a FetchClimate
                        //int NumberOfRecords = ds.Dimensions["RecordNumber"].Length; // get number of records     
                        temp = (double[, ,])ds.Variables["landdtr"].GetData();
                        _MissingValue = (double)ds.Variables["landdtr"].GetMissingValue();
                        break;
                    case "temperature":
                        ds.Fetch(ClimateParameter.FC_TEMPERATURE, "airt", dataSource: FetchClimateDataSource); //this call will create 2D variable on dimensions records and months and fill it with a FetchClimate
                        //int NumberOfRecords = ds.Dimensions["RecordNumber"].Length; // get number of records     
                        temp = (double[, ,])ds.Variables["airt"].GetData();
                        _MissingValue = (double)ds.Variables["airt"].GetMissingValue();
                        break;
                    // Commenting out ocean air temperature because it is running too slow when using FetchClimate
                    case "temperature_ocean":
                        ds.Fetch(ClimateParameter.FC_OCEAN_AIR_TEMPERATURE, "oceanairt", dataSource: FetchClimateDataSource); //this call will create 2D variable on dimensions records and months and fill it with a FetchClimate
                        //int NumberOfRecords = ds.Dimensions["RecordNumber"].Length; // get number of records     
                        temp = (double[, ,])ds.Variables["oceanairt"].GetData();
                        _MissingValue = (double)ds.Variables["oceanairt"].GetMissingValue();
                        break;
                    case "precipitation":
                        ds.Fetch(ClimateParameter.FC_PRECIPITATION, "precip", dataSource: FetchClimateDataSource); //this call will create 2D variable on dimensions records and months and fill it with a FetchClimate
                        //int NumberOfRecords = ds.Dimensions["RecordNumber"].Length; // get number of records     
                        temp = (double[, ,])ds.Variables["precip"].GetData();
                        _MissingValue = (double)ds.Variables["precip"].GetMissingValue();
                        break;
                    case "frost":
                        ds.Fetch(ClimateParameter.FC_LAND_FROST_DAY_FREQUENCY, "frost", dataSource: FetchClimateDataSource);
                        temp = (double[, ,])ds.Variables["frost"].GetData();
                        _MissingValue = (double)ds.Variables["frost"].GetMissingValue();
                        break;
                    default:
                        Debug.Fail("No Enviro data read in for " + dataName);
                        break;
                }

                _NumTimes = (uint)ds.Dimensions["time"].Length;

                //Add the fetched data to the Envirodata array
                for (int tt = 0; tt < _NumTimes; tt++)
                {
                    double[,] TempArray;
                    if (_DataArray.Count > tt)
                    {
                        TempArray = _DataArray[tt];
                    }
                    else
                    {
                        TempArray = new double[NumLats, NumLons];
                    }

                    // Currently FetchClimate returns longitudes as the last array dimension
                    TempArray[cellList[ii][0], cellList[ii][1]] = temp[tt, 0, 0];

                    if (_DataArray.Count > tt)
                    {
                        _DataArray.RemoveAt(tt);
                        _DataArray.Insert(tt, TempArray);
                    }
                    else
                    {
                        _DataArray.Add(TempArray);
                    }

                }
            }

            //DataSet Out = ds.Clone("output/" + dataName + ".nc");
            //Out.Dispose();
            ds.Dispose();

        }


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
        public EnviroData(string fileName, string dataName, string dataType, string dataResolution, string units)
        {
            // Initialise the utility functions
            Utilities = new UtilityFunctions();

            // Temporary array to hold environmental data
            double[,] tempDoubleArray;

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

            // Intialise the list of arrays to hold the values of the environmental data
            _DataArray = new List<double[,]>();

            // Construct the string required to access the file using Scientific Dataset
            _ReadFileString = "msds:" + dataType + "?file=" + fileName + "&openMode=readOnly";

            // Open the data file using Scientific Dataset
            DataSet internalData = DataSet.Open(_ReadFileString);

            // Store the specified units
            _Units = units;

            // Switch based on the tempeoral resolution and data type
            switch (dataResolution)
            {
                case "year":
                    switch (dataType)
                    {
                        case "esriasciigrid":
                            // Extract the number of latidudinal and longitudinal cells in the file
                            _NumLats = (uint)internalData.Dimensions["x"].Length;
                            _NumLons = (uint)internalData.Dimensions["y"].Length;
                            // Set number of time intervals equal to 1
                            _NumTimes = 1;
                            // Initialise the vector of time steps with length 1
                            _Times = new double[1];
                            // Assign the single value of the time step dimension to be equal to 1
                            _Times[0] = 1;
                            // Get the value used for missing data in this environmental variable
                            _MissingValue = internalData.GetAttr<double>(1, "NODATA_value");
                            // Get the latitudinal and longitudinal sizes of grid cells
                            _LatStep = internalData.GetAttr<double>(1, "cellsize");
                            _LonStep = _LatStep;
                            // Get longitudinal 'x' and latitudinal 'y' corners of the bottom left of the data grid
                            _LatMin = internalData.GetAttr<double>(1, "yllcorner");
                            _LonMin = internalData.GetAttr<double>(1, "xllcorner");
                            // Create vectors holding the latitudes and longitudes of the bottom-left corners of the grid cells
                            _Lats = new double[NumLats];
                            for (int ii = 0; ii < NumLats; ii++)
                            {
                                _Lats[NumLats - 1 - ii] = LatMin + ii * _LatStep;
                            }
                            _Lons = new double[NumLons];
                            for (int ii = 0; ii < NumLons; ii++)
                            {
                                _Lons[ii] = LonMin + ii * _LonStep;
                            }
                            //Fill in the two-dimensional environmental data array
                            // Note: currently assumes  Lats (x), Lons (y) in SDS - this is different to ESRI ASCII
                            tempDoubleArray = new double[NumLats, NumLons];
                            tempDoubleArray = internalData.GetData<double[,]>(dataName);
                            _DataArray.Add(tempDoubleArray);
                            break;
                        case "nc":
                            // Loop over possible names for the latitude dimension until a match in the data file is found
                            kk = 0;
                            while ((kk < LatSearchStrings.Length) && (!internalData.Variables.Contains(LatSearchStrings[kk]))) kk++;

                            // If a match for the latitude dimension has been found then read in the data, otherwise throw an error
                            if (kk < LatSearchStrings.Length)
                            {
                                // Get number of latitudinal cells in the file
                                _NumLats = (uint)internalData.Dimensions[LatSearchStrings[kk]].Length;
                                // Read in the values of the latitude dimension from the file
                                // Check which format the latitude dimension data are in; if unrecognized, then throw an error
                                if (internalData.Variables[LatSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "single")
                                {
                                    // Read the latitude dimension data to a temporary vector
                                    tempSingleVector = internalData.GetData<Single[]>(LatSearchStrings[kk]);
                                    // Convert the dimension data to double format and add to the vector of dimension values
                                    _Lats = new double[tempSingleVector.Length];
                                    for (int jj = 0; jj < tempSingleVector.Length; jj++)
                                    {
                                        _Lats[jj] = (double)tempSingleVector[jj];
                                    }
                                }
                                else if (internalData.Variables[LatSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "double")
                                {
                                    // Read the dimension data directly into the vector of dimension values
                                    _Lats = internalData.GetData<double[]>(LatSearchStrings[kk]);
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
                            while ((kk < LonSearchStrings.Length) && (!internalData.Variables.Contains(LonSearchStrings[kk]))) kk++;

                            // If a match for the longitude dimension has been found then read in the data, otherwise throw an error
                            if (kk < LonSearchStrings.Length)
                            {
                                // Get number of longitudinal cells in the file
                                _NumLons = (uint)internalData.Dimensions[LonSearchStrings[kk]].Length;
                                // Read in the values of the longitude dimension from the file
                                // Check which format the longitude dimension data are in; if unrecognized, then throw an error
                                if (internalData.Variables[LonSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "single")
                                {
                                    // Read the longitude dimension data to a temporary vector
                                    tempSingleVector = internalData.GetData<Single[]>(LonSearchStrings[kk]);
                                    // Convert the dimension data to double format and add to the vector of dimension values
                                    _Lons = new double[tempSingleVector.Length];
                                    for (int jj = 0; jj < tempSingleVector.Length; jj++)
                                    {
                                        _Lons[jj] = (double)tempSingleVector[jj];
                                    }
                                }
                                else if (internalData.Variables[LonSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "double")
                                {
                                    // Read the dimension data directly into the vector of dimension values
                                    _Lons = internalData.GetData<double[]>(LonSearchStrings[kk]);
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
                            // Set number of time intervals equal to 1
                            _NumTimes = 1;
                            // Initialise the vector of time steps with length 1
                            _Times = new double[1];
                            // Assign the single value of the time step dimension to be equal to 1
                            _Times[0] = 1;
                            // Get the latitudinal and longitudinal sizes of grid cells
                            _LatStep = (_Lats[1] - _Lats[0]);
                            _LonStep = (_Lons[1] - _Lons[0]);
                            // Convert vectors of latitude and longutiude dimension data from cell-centre references to bottom-left references
                            //if LatStep is positive then subtract the step to convert to the bottom  left corner of the cell,
                            // else if LatStep is negative, then need to add the step to convert to the bottom left
                            for (int ii = 0; ii < _Lats.Length; ii++)
                            {
                                _Lats[ii] = (_LatStep.CompareTo(0.0) > 0) ? _Lats[ii] - (_LatStep / 2) : _Lats[ii] + (_LatStep / 2);
                            }
                            for (int jj = 0; jj < _Lons.Length; jj++)
                            {
                                _Lons[jj] = (_LonStep.CompareTo(0.0) > 0) ? _Lons[jj] - (_LonStep / 2) : _Lons[jj] + (_LonStep / 2);
                            }
                            // Check whether latitudes and longitudes are inverted in the NetCDF file
                            bool LatInverted = (_Lats[1] < _Lats[0]);
                            bool LongInverted = (_Lons[1] < _Lons[0]);
                            // Run method to read in the environmental data and the dimension data from the NetCDF
                            EnvironmentListFromNetCDF(internalData, dataName, LatInverted, LongInverted);
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
                            while ((kk < LatSearchStrings.Length) && (!internalData.Variables.Contains(LatSearchStrings[kk]))) kk++;

                            // If a match for the latitude dimension has been found then read in the data, otherwise throw an error
                            if (kk < LatSearchStrings.Length)
                            {
                                // Get number of latitudinal cells in the file
                                _NumLats = (uint)internalData.Dimensions[LatSearchStrings[kk]].Length;
                                // Read in the values of the latitude dimension from the file
                                // Check which format the latitude dimension data are in; if unrecognized, then throw an error
                                if (internalData.Variables[LatSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "single")
                                {
                                    // Read the latitude dimension data to a temporary vector
                                    tempSingleVector = internalData.GetData<Single[]>(LatSearchStrings[kk]);
                                    // Convert the dimension data to double format and add to the vector of dimension values
                                    _Lats = new double[tempSingleVector.Length];
                                    for (int jj = 0; jj < tempSingleVector.Length; jj++)
                                    {
                                        _Lats[jj] = (double)tempSingleVector[jj];
                                    }
                                }
                                else if (internalData.Variables[LatSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "double")
                                {
                                    // Read the dimension data directly into the vector of dimension values
                                    _Lats = internalData.GetData<double[]>(LatSearchStrings[kk]);
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
                            while ((kk < LonSearchStrings.Length) && (!internalData.Variables.Contains(LonSearchStrings[kk]))) kk++;

                            // If a match for the longitude dimension has been found then read in the data, otherwise throw an error
                            if (kk < LonSearchStrings.Length)
                            {
                                // Get number of longitudinal cells in the file
                                _NumLons = (uint)internalData.Dimensions[LonSearchStrings[kk]].Length;
                                // Read in the values of the longitude dimension from the file
                                // Check which format the longitude dimension data are in; if unrecognized, then throw an error
                                if (internalData.Variables[LonSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "single")
                                {
                                    // Read the longitude dimension data to a temporary vector
                                    tempSingleVector = internalData.GetData<Single[]>(LonSearchStrings[kk]);
                                    // Convert the dimension data to double format and add to the vector of dimension values
                                    _Lons = new double[tempSingleVector.Length];
                                    for (int jj = 0; jj < tempSingleVector.Length; jj++)
                                    {
                                        _Lons[jj] = (double)tempSingleVector[jj];
                                    }
                                }
                                else if (internalData.Variables[LonSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "double")
                                {
                                    // Read the dimension data directly into the vector of dimension values
                                    _Lons = internalData.GetData<double[]>(LonSearchStrings[kk]);
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
                            while ((kk < MonthSearchStrings.Length) && (!internalData.Variables.Contains(MonthSearchStrings[kk]))) kk++;

                            // Of a match for the monthly temporal dimension has been found then read in the data, otherwise thrown an error
                            if (internalData.Variables.Contains(MonthSearchStrings[kk]))
                            {
                                // Get the number of months in the temporal dimension
                                _NumTimes = (uint)internalData.Dimensions[MonthSearchStrings[kk]].Length;
                                // Check that the number of months is 12
                                Debug.Assert(_NumTimes == 12, "Number of time intervals in an environmental data file with specified monthly temporal resolution is not equal to 12");
                                // Read in the values of the temporal dimension from the file
                                // Check which format the temporal dimension data are in; if unrecognized, then throw an error
                                if (internalData.Variables[MonthSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "single")
                                {
                                    // Read the temporal dimension data to a temporary vector
                                    tempSingleVector = internalData.GetData<Single[]>(MonthSearchStrings[kk]);
                                    // Convert the dimension data to double format and add to the vector of dimension values
                                    _Times = new double[_NumTimes];
                                    for (int hh = 0; hh < tempSingleVector.Length; hh++)
                                    {
                                        _Times[hh] = (double)tempSingleVector[hh];
                                    }
                                }
                                else if (internalData.Variables[MonthSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "double")
                                {
                                    // Read the dimension data directly into the vector of dimension values
                                    _Times = internalData.GetData<double[]>(MonthSearchStrings[kk]);
                                }
                                else if (internalData.Variables[MonthSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "int32")
                                {
                                    // Read the temporal dimension data to a temporary vector
                                    tempInt32Vector = internalData.GetData<Int32[]>(MonthSearchStrings[kk]);
                                    // Convert the dimension data to double format and add to the vector of dimension values
                                    _Times = new double[_NumTimes];
                                    for (int hh = 0; hh < tempInt32Vector.Length; hh++)
                                    {
                                        _Times[hh] = (double)tempInt32Vector[hh];
                                    }
                                }
                                else if (internalData.Variables[MonthSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "int16")
                                {
                                    // Read the temporal dimension data to a temporary vector
                                    tempInt16Vector = internalData.GetData<Int16[]>(MonthSearchStrings[kk]);
                                    // Convert the dimension data to double format and add to the vector of dimension values
                                    _Times = new double[_NumTimes];
                                    for (int hh = 0; hh < tempInt16Vector.Length; hh++)
                                    {
                                        _Times[hh] = (double)tempInt16Vector[hh];
                                    }
                                }
                                else if (internalData.Variables[MonthSearchStrings[kk]].TypeOfData.Name.ToString().ToLower() == "string")
                                {
                                    // Convert the dimension data to double format and add to the vector of dimension values
                                    _Times = new double[_NumTimes];
                                    for (int hh = 0; hh < _NumTimes; hh++)
                                    {
                                        _Times[hh] = (double)hh;
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
                            bool LatInverted = (_Lats[1] < _Lats[0]);
                            bool LongInverted = (_Lons[1] < _Lons[0]);
                            // Run method to read in the environmental data and the dimension data from the NetCDF
                            EnvironmentListFromNetCDF3D(internalData, dataName, LatInverted, LongInverted);
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

            //Close the environmental data file
            internalData.Dispose();
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
        public double GetValue(double lat, double lon, uint timeInterval, out Boolean missingValue, double latCellSize, double lonCellSize)
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
                        if (_DataArray[(int)timeInterval][ii, jj].CompareTo(_MissingValue) != 0.0)
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
                        if (_DataArray[(int)timeInterval][ii, jj].CompareTo(_MissingValue) != 0.0)
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
                            if (_DataArray[(int)timeInterval][ii, jj].CompareTo(_MissingValue) != 0)
                            {
                                WeightedValue += _DataArray[(int)timeInterval][ii, jj] * OverlapAreas[ii - ClosestLowerLatIndex, jj - closestLeftmostLonIndex] / CumulativeNonMissingValueOverlapArea;
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
                            if (_DataArray[(int)timeInterval][ii, jj].CompareTo(_MissingValue) != 0)
                            {
                                WeightedValue += _DataArray[(int)timeInterval][ii, jj] * OverlapAreas[ClosestLowerLatIndex - ii, jj - closestLeftmostLonIndex] / CumulativeNonMissingValueOverlapArea;
                            }
                        }
                    }
                }
            }

            //Return the weighted average
            return WeightedValue;
        }



        /// <summary>
        /// Reads in two-dimensional environmental data from a NetCDF and stores them in the array of values within this instance of EnviroData
        /// </summary>
        /// <param name="internalData">The SDS object to get data from</param>
        /// <param name="dataName">The name of the variable within the NetCDF file</param>
        /// <param name="latInverted">Whether the latitude values are inverted in the NetCDF file (i.e. large to small values)</param>
        /// <param name="longInverted">Whether the longitude values are inverted in the NetCDF file (i.e. large to small values)</param>
        private void EnvironmentListFromNetCDF(DataSet internalData, string dataName, bool latInverted, bool longInverted)
        {
            // Vector two hold the position in the dimensions of the NetCDF file of the latitude and longitude dimensions
            int[] positions = new int[2];

            // Array to store environmental data with latitude as the first dimension and longitude as the second dimension
            double[,] LatLongArrayUnsorted = new double[_NumLats, _NumLons];
            // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
            double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

            Console.WriteLine(dataName);
            // Check that the requested variable exists in the NetCDF file
            Debug.Assert(internalData.Variables.Contains(dataName), "Requested variable does not exist in the specified file");

            // Check that the environmental variable in the NetCDF file has two dimensions
            Debug.Assert(internalData.Variables[dataName].Dimensions.Count == 2, "The specified variable in the NetCDF file does not have two dimensions, which is the required number for this method");

            // Possible names for the missing value metadata in the NetCDF file
            string[] SearchStrings = { "missing_value", "MissingValue" };

            // Loop over possible names for the missing value metadata until a match is found in the NetCDF file
            int kk = 0;
            while ((kk < SearchStrings.Length) && (!internalData.Variables[dataName].Metadata.ContainsKey(SearchStrings[kk]))) kk++;

            // If a match is found, then set the missing data field equal to the value in the file, otherwise throw an error
            if (kk < SearchStrings.Length)
            {
                _MissingValue = Convert.ToDouble(internalData.Variables[dataName].Metadata[SearchStrings[kk]]);
            }
            else
            {
                Console.WriteLine("No missing data value found for this variable: assigning a value of -9999");
                _MissingValue = -9999;
            }

            // Possible names for the latitude dimension in the NetCDF file
            SearchStrings = new string[] { "lat", "Lat", "latitude", "Latitude", "lats", "Lats", "latitudes", "Latitudes", "y", "Y" };
            // Check which position the latitude dimension is in in the NetCDF file and add this to the vector of positions. If the latitude dimension cannot be
            // found then throw an error
            if (SearchStrings.Contains(internalData.Dimensions[0].Name.ToString()))
            {
                positions[0] = 1;
            }
            else if (SearchStrings.Contains(internalData.Dimensions[1].Name.ToString()))
            {
                positions[1] = 1;
            }
            else
            {
                Debug.Fail("Cannot find a latitude dimension");
            }

            // Possible names for the longitude dimension in the netCDF file
            SearchStrings = new string[] { "lon", "Lon", "longitude", "Longitude", "lons", "Lons", "long", "Long", "longs", "Longs", "longitudes", "Longitudes", "x", "X" };
            // Check which position the latitude dimension is in in the NetCDF file and add this to the vector of positions. If the latitude dimension cannot be
            // found then throw an error
            if (SearchStrings.Contains(internalData.Dimensions[0].Name.ToString()))
            {
                positions[0] = 2;
            }
            else if (SearchStrings.Contains(internalData.Dimensions[1].Name.ToString()))
            {
                positions[1] = 2;
            }
            else
            {
                Debug.Fail("Cannot find a longitude dimension");
            }

            // Check the format of the specified environmental variable
            if (internalData.Variables[dataName].TypeOfData.Name.ToString().ToLower() == "single")
            {
                // Read the environmental data into a temporary array
                Single[,] TempArray;
                TempArray = internalData.GetData<Single[,]>(dataName);
                // Convert the data to double format and add to the unsorted array of values, transposing the data to be dimensioned by latitude first and longitude second
                for (int ii = 0; ii < _NumLats; ii++)
                {
                    for (int jj = 0; jj < _NumLons; jj++)
                    {
                        LatLongArrayUnsorted[ii, jj] = TempArray[(int)(ii * Convert.ToDouble(positions[0] == 1) + jj * Convert.ToDouble(positions[0] == 2)),
                            (int)(ii * Convert.ToDouble(positions[1] == 1) + jj * Convert.ToDouble(positions[1] == 2))];
                    }
                }
            }
            else if (internalData.Variables[dataName].TypeOfData.Name.ToString().ToLower() == "double")
            {
                // Read the environmental data into a temporary array
                double[,] TempArray;
                TempArray = internalData.GetData<double[,]>(dataName);
                // Add the data to the unsorted array of values, transposing the data to be dimensioned by latitude first and longitude second
                for (int ii = 0; ii < _NumLats; ii++)
                {
                    for (int jj = 0; jj < _NumLons; jj++)
                    {
                        LatLongArrayUnsorted[ii, jj] = TempArray[(int)(ii * Convert.ToDouble(positions[0] == 1) + jj * Convert.ToDouble(positions[0] == 2)), (int)(ii * Convert.ToDouble(positions[1] == 1) + jj * Convert.ToDouble(positions[1] == 2))];
                    }
                }
            }
            else if (internalData.Variables[dataName].TypeOfData.Name.ToString().ToLower() == "int32")
            {
                // Read the environmental data into a temporary array
                Int32[,] TempArray;
                TempArray = internalData.GetData<Int32[,]>(dataName);
                // Convert the data to double format and add to the unsorted array of values, transposing the data to be dimensioned by latitude first and longitude second
                for (int ii = 0; ii < _NumLats; ii++)
                {
                    for (int jj = 0; jj < _NumLons; jj++)
                    {
                        LatLongArrayUnsorted[ii, jj] = TempArray[(int)(ii * Convert.ToDouble(positions[0] == 1) + jj * Convert.ToDouble(positions[0] == 2)), (int)(ii * Convert.ToDouble(positions[1] == 1) + jj * Convert.ToDouble(positions[1] == 2))];
                    }
                }
            }
            else
            {
                // Format of environmental data not recognized so throw an error
                Debug.Fail("Environmental data are in an unrecognized format");
            }

            // Transpose the environmental data so that they are in ascending order of both latitude and longitude
            for (int ii = 0; ii < _NumLats; ii++)
            {
                for (int jj = 0; jj < _NumLons; jj++)
                {
                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[(int)((1 - Convert.ToDouble(latInverted)) * ii + (Convert.ToDouble(latInverted) * (LatLongArrayUnsorted.GetLength(0) - 1 - ii))), (int)((1 - Convert.ToDouble(longInverted)) * jj + (Convert.ToDouble(longInverted) * (LatLongArrayUnsorted.GetLength(1) - 1 - jj)))];
                }
            }
            // Add the final array to the class field for environmental data values
            _DataArray.Add(LatLongArraySorted);

            // If either latitude or longitude were inverted, then reverse their values in the class fields
            if (latInverted)
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
            if (longInverted)
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

        }


        /// <summary>
        /// Reads in three-dimensional environmental data from a NetCDF and stores them in the array of values within this instance of EnviroData
        /// </summary>
        /// <param name="internalData">The SDS object to get data from</param>
        /// <param name="dataName">The name of the variable within the NetCDF file</param>
        /// <param name="latInverted">Whether the latitude values are inverted in the NetCDF file (i.e. large to small values)</param>
        /// <param name="longInverted">Whether the longitude values are inverted in the NetCDF file (i.e. large to small values)</param>
        private void EnvironmentListFromNetCDF3D(DataSet internalData, string dataName, bool latInverted, bool longInverted)
        {
            // Vector to hold the position in the dimensions of the NetCDF file of the latitude, longitude and third dimensions
            int[] positions = new int[3];

            // Array to store environmental data with latitude as the first dimension and longitude as the second dimension
            double[,] LatLongArrayUnsorted = new double[_NumLats, _NumLons];

            // Check that the requested variable exists in the NetCDF file
            Debug.Assert(internalData.Variables.Contains(dataName), "Requested variable does not exist in the specified file");

            // Check that the environmental variable in the NetCDF file has three dimensions
            Debug.Assert(internalData.Variables[dataName].Dimensions.Count == 3, "The specified variable in the NetCDF file does not have three dimensions, which is the required number for this method");

            // Possible names for the missing value metadata in the NetCDF file
            string[] SearchStrings = { "missing_value", "MissingValue" };

            // Loop over possible names for the missing value metadata until a match is found in the NetCDF file
            int kk = 0;
            while ((kk < SearchStrings.Length) && (!internalData.Variables[dataName].Metadata.ContainsKey(SearchStrings[kk]))) kk++;

            // If a match is found, then set the missing data field equal to the value in the file, otherwise throw an error
            if (kk < SearchStrings.Length)
            {
                _MissingValue = Convert.ToDouble(internalData.Variables[dataName].Metadata[SearchStrings[kk]]);
            }
            else
            {
                //Debug.Fail("No missing data value found for environmental data file: " + internalData.Name.ToString());
                Console.WriteLine("No missing data value found for this variable: assigning a value of -9999");
                _MissingValue = -9999;
            }

            // Possible names for the latitude dimension in the NetCDF file
            SearchStrings = new string[] { "lat", "Lat", "latitude", "Latitude", "lats", "Lats", "latitudes", "Latitudes", "y" };
            // Check which position the latitude dimension is in in the NetCDF file and add this to the vector of positions. If the latitude dimension cannot be
            // found then throw an error
            if (SearchStrings.Contains(internalData.Dimensions[0].Name.ToString()))
            {
                positions[0] = 1;
            }
            else if (SearchStrings.Contains(internalData.Dimensions[1].Name.ToString()))
            {
                positions[1] = 1;
            }
            else if (SearchStrings.Contains(internalData.Dimensions[2].Name.ToString()))
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
            if (SearchStrings.Contains(internalData.Dimensions[0].Name.ToString()))
            {
                positions[0] = 2;
            }
            else if (SearchStrings.Contains(internalData.Dimensions[1].Name.ToString()))
            {
                positions[1] = 2;
            }
            else if (SearchStrings.Contains(internalData.Dimensions[2].Name.ToString()))
            {
                positions[2] = 2;
            }
            else
            {
                Debug.Fail("Cannot find a longitude dimension");
            }

            // Possible names for the monthly temporal dimension in the netCDF file
            SearchStrings = new string[] { "time", "month", "Month", "months", "Months" };
            // Check which position the temporal dimension is in in the NetCDF file and add this to the vector of positions. If the temporal dimension cannot be
            // found then throw an error
            if (SearchStrings.Contains(internalData.Dimensions[0].Name.ToString()))
            {
                positions[0] = 3;
            }
            else if (SearchStrings.Contains(internalData.Dimensions[1].Name.ToString()))
            {
                positions[1] = 3;
            }
            else if (SearchStrings.Contains(internalData.Dimensions[2].Name.ToString()))
            {
                positions[2] = 3;
            }

            // Check the format of the specified environmental variable
            if (internalData.Variables[dataName].TypeOfData.Name.ToString().ToLower() == "single")
            {
                // Read the environmental data into a temporary array
                Single[, ,] TempArray;
                TempArray = internalData.GetData<Single[, ,]>(dataName);

                // Revised for speed
                switch (positions[0])
                {
                    case 1:
                        switch (positions[1])
                        {
                            case 2:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

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
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

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
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

                                }
                                break;
                            case 2:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

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
            else if (internalData.Variables[dataName].TypeOfData.Name.ToString().ToLower() == "double")
            {
                // Read the environmental data into a temporary array
                double[, ,] TempArray;
                TempArray = internalData.GetData<double[, ,]>(dataName);
                // Revised for speed
                switch (positions[0])
                {
                    case 1:
                        switch (positions[1])
                        {
                            case 2:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

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
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

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
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

                                }
                                break;
                            case 2:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

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
            else if (internalData.Variables[dataName].TypeOfData.Name.ToString().ToLower() == "int32")
            {
                // Read the environmental data into a temporary array
                Int32[, ,] TempArray;
                TempArray = internalData.GetData<Int32[, ,]>(dataName);
                // Revised for speed
                switch (positions[0])
                {
                    case 1:
                        switch (positions[1])
                        {
                            case 2:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

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
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

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
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

                                }
                                break;
                            case 2:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

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
            else if (internalData.Variables[dataName].TypeOfData.Name.ToString().ToLower() == "int16")
            {
                // Read the environmental data into a temporary array
                Int16[, ,] TempArray;
                TempArray = internalData.GetData<Int16[, ,]>(dataName);

                // Revised for speed
                switch (positions[0])
                {
                    case 1:
                        switch (positions[1])
                        {
                            case 2:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

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
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

                                }
                                break;
                            case 3:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

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
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

                                }
                                break;
                            case 2:
                                // Loop over time steps
                                for (int hh = 0; hh < _NumTimes; hh++)
                                {
                                    // Array to store environmental data as above, but with data in ascending order of both latitude and longitude
                                    double[,] LatLongArraySorted = new double[_NumLats, _NumLons];

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
                                    if (latInverted)
                                    {
                                        if (longInverted)
                                        {
                                            // Both dimensions inverted
                                            int LatLengthMinusOne = LatLongArrayUnsorted.GetLength(0) - 1;
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, LongLengthMinusOne - jj];
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
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[LatLengthMinusOne - ii, jj];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (longInverted)
                                        {
                                            // Longitude only inverted
                                            int LongLengthMinusOne = LatLongArrayUnsorted.GetLength(1) - 1;
                                            for (int ii = 0; ii < _NumLats; ii++)
                                            {
                                                for (int jj = 0; jj < _NumLons; jj++)
                                                {
                                                    LatLongArraySorted[ii, jj] = LatLongArrayUnsorted[ii, LongLengthMinusOne - jj];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LatLongArraySorted = (double[,])LatLongArrayUnsorted.Clone();
                                        }
                                    }
                                    // Add the final array to the class field for environmental data values
                                    _DataArray.Add(LatLongArraySorted);

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
            if (latInverted)
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
            if (longInverted)
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

        }


        /// <summary>
        /// Dispose of an Envirodata instance
        /// </summary>
        ~EnviroData()
        {
            // Clear the data in the Envirodata instance
            DataArray.Clear();
        }

    }
}