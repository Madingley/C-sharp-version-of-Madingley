using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using Microsoft.Research.Science.Data;


namespace Madingley
{
    /// <summary>
    /// Initialization information for Madingley model simulations
    /// </summary>
    public class MadingleyModelInitialisation
    {
        /// <summary>
        /// String identifying time step units to be used by the simulations
        /// </summary>
        private string _GlobalModelTimeStepUnit;
        /// <summary>
        /// Get and set the string identifying time step units to be used by the simulations
        /// </summary>
        public string GlobalModelTimeStepUnit
        {
            get { return _GlobalModelTimeStepUnit; }
            set { _GlobalModelTimeStepUnit = value; }
        }

        /// <summary>
        /// The number of time steps to be run in the simulations
        /// </summary>
        private uint _NumTimeSteps;
        /// <summary>
        /// Get and set the number of time steps to be run in the simulations
        /// </summary>
        public uint NumTimeSteps
        {
            get { return _NumTimeSteps; }
            set { _NumTimeSteps = value; }
        }

        /// <summary>
        /// The number of time steps to run the model for before any impacts are applied
        /// </summary>
        private uint _BurninTimeSteps;
        /// <summary>
        /// Get and set the number of time steps to run the model for before any impacts are applied
        /// </summary>
        public uint BurninTimeSteps
        {
            get { return _BurninTimeSteps; }
            set { _BurninTimeSteps = value; }
        }

        /// <summary>
        /// For scenarios with temporary impacts, the number of time steps to apply the impact for
        /// </summary>
        private uint _ImpactTimeSteps;

        /// <summary>
        /// Get and set the number of time steps to apply the impact for, for scenarios with temporary impacts
        /// </summary>
        public uint ImpactTimeSteps
        {
            get { return _ImpactTimeSteps; }
            set { _ImpactTimeSteps = value; }
        }


        /// <summary>
        /// For scenarios with temporary impacts, the number of time steps to apply the impact for
        /// </summary>
        private uint _RecoveryTimeSteps;

        /// <summary>
        /// Get and set the number of time steps to apply the impact for, for scenarios with temporary impacts
        /// </summary>
        public uint RecoveryTimeSteps
        {
            get { return _RecoveryTimeSteps; }
            set { _RecoveryTimeSteps = value; }
        }

        /// <summary>
        /// For scenarios with instantaneous impacts, the time step in which to apply the impact
        /// </summary>
        private uint _InstantaneousTimeStep;

        /// <summary>
        /// Get and set the time step in which to apply the impact, for scenarios with instantaneous impacts
        /// </summary>
        public uint InstantaneousTimeStep
        {
            get { return _InstantaneousTimeStep; }
            set { _InstantaneousTimeStep = value; }
        }


        /// <summary>
        /// For scenarios with instantaneous impacts, the number of time steps to apply the impact for
        /// </summary>
        private uint _NumInstantaneousTimeStep;

        /// <summary>
        /// Get and set the number of time steps to apply the impact for, for scenarios with instantaneous impacts
        /// </summary>
        public uint NumInstantaneousTimeStep
        {
            get { return _NumInstantaneousTimeStep; }
            set { _NumInstantaneousTimeStep = value; }
        }

        /// <summary>
        /// The size of cells to be used in the model grid
        /// </summary>
        private double _CellSize;
        /// <summary>
        /// Get and set the size of cells to be used in the model grid
        /// </summary>
        public double CellSize
        {
            get { return _CellSize; }
            set { _CellSize = value; }
        }

        /// <summary>
        /// The lowest extent of the model grid in degrees
        /// </summary>
        private float _BottomLatitude;
        /// <summary>
        /// Get and set the lowest extent of the model grid in degrees
        /// </summary>
        public float BottomLatitude
        {
            get { return _BottomLatitude; }
            set { _BottomLatitude = value; }
        }

        /// <summary>
        /// The uppermost extent of the model grid in degrees
        /// </summary>
        private float _TopLatitude;
        /// <summary>
        /// Get and set the uppermost extent of the model grid in degrees
        /// </summary>
        public float TopLatitude
        {
            get { return _TopLatitude; }
            set { _TopLatitude = value; }
        }

        /// <summary>
        /// The leftmost extent of the model grid in degrees
        /// </summary>
        private float _LeftmostLongitude;
        /// <summary>
        /// Get and set the leftmost extent of the model grid in degrees
        /// </summary>
        public float LeftmostLongitude
        {
            get { return _LeftmostLongitude; }
            set { _LeftmostLongitude = value; }
        }

        /// <summary>
        /// The rightmost extent of the model grid in degrees
        /// </summary>
        private float _RightmostLongitude;
        /// <summary>
        /// Get and set the rightmost extent of the model grid in degrees
        /// </summary>
        public float RightmostLongitude
        {
            get { return _RightmostLongitude; }
            set { _RightmostLongitude = value; }
        }


        /// <summary>
        /// Whether to run the model for different grid cells in parallel
        /// </summary>
        private Boolean _RunInParallel = false;
        /// <summary>
        /// Get and set whether to run the model for different grid cells in parallel
        /// </summary>
        public Boolean RunInParallel
        {
            get { return _RunInParallel; }
            set { _RunInParallel = value; }
        }


        /// <summary>
        /// Whether to run the model for different grid cells in parallel
        /// </summary>
        private Boolean _RunCellsInParallel = false;
        /// <summary>
        /// Get and set whether to run the model for different grid cells in parallel
        /// </summary>
        public Boolean RunCellsInParallel
        {
            get { return _RunCellsInParallel; }
            set { _RunCellsInParallel = value; }
        }

        /// <summary>
        /// Whether to run the model for different simulations in parallel
        /// </summary>
        private Boolean _RunSimulationsInParallel = false;
        /// <summary>
        /// Get and set whether to run the model for different grid cells in parallel
        /// </summary>
        public Boolean RunSimulationsInParallel
        {
            get { return _RunSimulationsInParallel; }
            set { _RunSimulationsInParallel = value; }
        }

        /// <summary>
        /// Which realm to run the model for
        /// </summary>
        private string _RunRealm;
        /// <summary>
        /// Get and set which realm to run the model for
        /// </summary>
        public string RunRealm
        {
            get { return _RunRealm; }
            set { _RunRealm = value; }
        }


        /// <summary>
        /// Whether to draw cohort properties randomly when seeding them, and whether cohorts will undergo ecological processes in a random order
        /// </summary>
        /// <remarks>Value should be set in initialization file, but default value is true</remarks>
        private Boolean _DrawRandomly = true;
        /// <summary>
        /// Get and set whether to draw cohort properties randomly when seeding them, and whether cohorts will undergo ecological processes in a random order
        /// </summary>
        public Boolean DrawRandomly
        {
            get { return _DrawRandomly; }
            set { _DrawRandomly = value; }
        }

        /// <summary>
        /// The threshold abundance below which cohorts will be made extinct
        /// </summary>
        private double _ExtinctionThreshold;
        /// <summary>
        /// Get and set the threshold abundance below which cohorts will be made extinct
        /// </summary>
        public double ExtinctionThreshold
        {
            get { return _ExtinctionThreshold; }
            set { _ExtinctionThreshold = value; }
        }

        /// <summary>
        /// The threshold difference between cohorts, within which they will be merged
        /// </summary>
        private double _MergeDifference;
        /// <summary>
        /// Get and set the threshold difference between cohorts, within which they will be merged
        /// </summary>
        public double MergeDifference
        {
            get { return _MergeDifference; }
            set { _MergeDifference = value; }
        }

        /// <summary>
        /// The maximum number of cohorts to be in the model, per grid cell, when it is running
        /// </summary>
        private int _MaxNumberOfCohorts;

        /// <summary>
        ///  Get and set the maximum number of cohorts per grid cell
        /// </summary>
        public int MaxNumberOfCohorts
        {
            get { return _MaxNumberOfCohorts; }
            set { _MaxNumberOfCohorts = value; }
        }


        /// <summary>
        /// Whether to run only dispersal (i.e. turn all other ecological processes off, and set dispersal probability to one temporarily)
        /// </summary>
        private Boolean _DispersalOnly = false;
        /// <summary>
        /// Get and set whether to run dispersal only
        /// </summary>
        public Boolean DispersalOnly
        {
            get { return _DispersalOnly; }
            set { _DispersalOnly = value; }
        }

        /// <summary>
        /// The weight threshold (grams) below which marine organisms that are not obligate zooplankton will be dispersed planktonically
        /// </summary>
        private double _PlanktonDispersalThreshold;
        /// <summary>
        /// Get and set the weight threshold (grams) below which marine organisms that are not obligate zooplankton will be dispersed planktonically
        /// </summary>
        public double PlanktonDispersalThreshold
        {
            get { return _PlanktonDispersalThreshold; }
            set { _PlanktonDispersalThreshold = value; }
        }

        /// <summary>
        /// Information from the initialization file
        /// </summary>
        private SortedList<string, string> _InitialisationFileStrings = new SortedList<string, string>();
        /// <summary>
        /// Get and set information from the initialization file
        /// </summary>
        public SortedList<string, string> InitialisationFileStrings
        {
            get { return _InitialisationFileStrings; }
            set { _InitialisationFileStrings = value; }
        }

        /// <summary>
        /// The functional group definitions of cohorts in the model
        /// </summary>
        private FunctionalGroupDefinitions _CohortFunctionalGroupDefinitions;
        /// <summary>
        /// Get and set the functional group definitions of cohorts in the model
        /// </summary>
        public FunctionalGroupDefinitions CohortFunctionalGroupDefinitions
        {
            get { return _CohortFunctionalGroupDefinitions; }
            set { _CohortFunctionalGroupDefinitions = value; }
        }

        /// <summary>
        /// The functional group definitions of stocks in the model
        /// </summary>
        private FunctionalGroupDefinitions _StockFunctionalGroupDefinitions;
        /// <summary>
        /// Get and set the functional group definitions of stocks in the model
        /// </summary>
        public FunctionalGroupDefinitions StockFunctionalGroupDefinitions
        {
            get { return _StockFunctionalGroupDefinitions; }
            set { _StockFunctionalGroupDefinitions = value; }
        }

        /// <summary>
        /// The environmental layers for use in the model
        /// </summary>
        private SortedList<string, EnviroData> _EnviroStack = new SortedList<string, EnviroData>();
        /// <summary>
        /// Get and set the environmental layers for use in the model
        /// </summary>
        public SortedList<string, EnviroData> EnviroStack
        {
            get { return _EnviroStack; }
            set { _EnviroStack = value; }
        }

        /// <summary>
        /// The environmental layers for use in the model
        /// </summary>
        private SortedList<string, EnviroDataTemporal> _EnviroStackTemporal = new SortedList<string, EnviroDataTemporal>();
        /// <summary>
        /// Get and set the environmental layers for use in the model
        /// </summary>
        public SortedList<string, EnviroDataTemporal> EnviroStackTemporal
        {
            get { return _EnviroStackTemporal; }
            set { _EnviroStackTemporal = value; }
        }


        /// <summary>
        /// The full path for the output files for a set of simulations
        /// </summary>
        private string _OutputPath;
        /// <summary>
        /// Get and set the full path for the output files for a set of simulations
        /// </summary>
        public string OutputPath
        {
            get { return _OutputPath; }
            set { _OutputPath = value; }
        }

        /// <summary>
        /// Whether to output detailed diagnostics for the ecological processes
        /// </summary>
        private Boolean _TrackProcesses = false;
        /// <summary>
        /// Get and set whether to output detailed diagnostics for the ecological processes
        /// </summary>
        public Boolean TrackProcesses
        {
            get { return _TrackProcesses; }
            set { _TrackProcesses = value; }
        }

        /// <summary>
        /// Whether to output detailed diagnostics for the cross cell ecological processes
        /// </summary>
        private Boolean _TrackCrossCellProcesses = false;
        /// <summary>
        /// Get and set whether to output detailed diagnostics for the cross cell ecological processes
        /// </summary>
        public Boolean TrackCrossCellProcesses
        {
            get { return _TrackCrossCellProcesses; }
            set { _TrackCrossCellProcesses = value; }
        }

        /// <summary>
        /// Whether to output detailed diagnostics for the ecological processes
        /// </summary>
        private Boolean _TrackGlobalProcesses = false;
        /// <summary>
        /// Get and set whether to output detailed diagnostics for the ecological processes
        /// </summary>
        public Boolean TrackGlobalProcesses
        {
            get { return _TrackGlobalProcesses; }
            set { _TrackGlobalProcesses = value; }
        }

        /// <summary>
        /// The paths and filenames for the diagnostics for the ecological processes
        /// </summary>
        private SortedList<string, string> _ProcessTrackingOutputs = new SortedList<string, string>();
        /// <summary>
        /// Get and set the paths and filenames for the diagnostics for the ecological processes
        /// </summary>
        public SortedList<string, string> ProcessTrackingOutputs
        {
            get { return _ProcessTrackingOutputs; }
            set { _ProcessTrackingOutputs = value; }
        }

        /// <summary>
        /// The string values for the units of each environmental data layer
        /// </summary>
        private SortedList<string, string> _Units = new SortedList<string, string>();
        /// <summary>
        /// Get and set the unit strings
        /// </summary>
        public SortedList<string, string> Units
        {
            get { return _Units; }
            set { _Units = value; }
        }

        /// <summary>
        /// An instance of the mass bin handler for the current model run
        /// </summary>
        private MassBinsHandler _ModelMassBins;
        /// <summary>
        /// Get the instance of the mass bin handler for the current model run
        /// </summary>
        public MassBinsHandler ModelMassBins
        { get { return _ModelMassBins; } }


        /// <summary>
        /// Whether to display live outputs using Dataset Viewer during the model runs
        /// </summary>
        private Boolean _LiveOutputs;

        /// <summary>
        /// Get and set whether to display live outputs using Dataset Viewer during the model runs
        /// </summary>
        public Boolean LiveOutputs
        {
            get { return _LiveOutputs; }
            set { _LiveOutputs = value; }
        }

        /// <summary>
        /// Whether or not to track trophic level biomass and flow information specific to the marine realm
        /// </summary>
        private Boolean _TrackMarineSpecifics;
        /// <summary>
        /// Get and set whether or not to track trophic level biomass and flow information specific to the marine realm
        /// </summary>
        public Boolean TrackMarineSpecifics
        {
            get { return _TrackMarineSpecifics; }
            set { _TrackMarineSpecifics = value; }
        }

        /// <summary>
        /// Whether to output ecosystem-level functional metrics
        /// </summary>
        private Boolean _OutputMetrics;
        /// <summary>
        /// Get and set whether to output ecosystem-level functional metrics
        /// </summary>
        public Boolean OutputMetrics
        {
            get { return _OutputMetrics; }
            set { _OutputMetrics = value; }
        }

        private List<uint> _ImpactCellIndices = new List<uint>();

        public List<uint> ImpactCellIndices
        {
            get { return _ImpactCellIndices; }
            set { _ImpactCellIndices = value; }
        }

        private Boolean _ImpactAll = false;

        public Boolean ImpactAll
        {
            get { return _ImpactAll; }
            set { _ImpactAll = value; }
        }


        private List<uint> _OutputStateTimestep = new List<uint>();

        public List<uint> OutputStateTimestep
        {
            get { return _OutputStateTimestep; }
            set { _OutputStateTimestep = value; }
        }

        /// <summary>
        /// Instance of Utilities for timestep conversions
        /// </summary>
        private UtilityFunctions Utilities = new UtilityFunctions();

        //proportion of the model grid that is fragmented
        private float _FragmentProportion;
        
        private Boolean _InputState = false;

        public Boolean InputState
        {
            get { return _InputState; }
            set { _InputState = value; }
        }

        /// <summary>
        /// Pairs of longitude and latitude indices for all active cells in the model grid
        /// </summary>
        private List<uint[]> _CellList;
        public List<uint[]> CellList
        {
            get { return _CellList; }
            set { _CellList = value; }
        }

        //Indicates if specific locations have been specified
        private Boolean _SpecificLocations = false;

        public Boolean SpecificLocations
        {
            get { return _SpecificLocations; }
            set { _SpecificLocations = value; }
        }

        private List<string> _ModelStatePath;

        public List<string> ModelStatePath
        {
            get { return _ModelStatePath; }
            set { _ModelStatePath = value; }
        }

        private List<string> _ModelStateFilename;

        public List<string> ModelStateFilename
        {
            get { return _ModelStateFilename; }
            set { _ModelStateFilename = value; }
        }


        private List<string> _ModelStateType;

        public List<string> ModelStateType
        {
            get { return _ModelStateType; }
            set { _ModelStateType = value; }
        }
        
        /// <summary>
        /// Reads the initalization file to get information for the set of simulations to be run
        /// </summary>
        /// <param name="initialisationFile">The name of the initialization file with information on the simulations to be run</param>
        /// <param name="outputPath">The path to folder in which outputs will be stored</param>
        public MadingleyModelInitialisation(string simulationInitialisationFilename, string definitionsFilename, string outputsFilename, string outputPath)
        {
            // Write to console
            Console.WriteLine("Initializing model...\n");

            // Initialize the mass bins to be used during the model run
            _ModelMassBins = new MassBinsHandler();

            // Initialize the lists for storing information about model states
            _ModelStatePath = new List<string>();
            _ModelStateFilename = new List<string>();
            _ModelStateType = new List<string>();

            // Read the intialisation files and copy them to the output directory
            ReadAndCopyInitialisationFiles(simulationInitialisationFilename, definitionsFilename, outputsFilename, outputPath);

            // Copy parameter values to an output file
            //Don't do this now as the parameter values are read in from file and this file is copied to the output directory
            //CopyParameterValues(outputPath);



        }

        /// <summary>
        /// Reads in all initialisation files and copies them to the output directory for future reference
        /// </summary>
        /// <param name="initialisationFile">The name of the initialization file with information on the simulations to be run</param>
        /// <param name="outputPath">The path to folder in which outputs will be stored</param>
        /// <todo>Need to adjust this file to deal with incorrect inputs, extra columns etc by throwing an error</todo>
        /// <todo>Also need to strip leading spaces</todo>
        public void ReadAndCopyInitialisationFiles(string simulationInitialisationFilename, string definitionsFilename, string outputsFilename, string outputPath)
        {
            // Construct file names
            string SimulationFileString = "msds:csv?file=input/Model setup/" + simulationInitialisationFilename + "&openMode=readOnly";
            string DefinitionsFileString = "msds:csv?file=input/Model setup/" + definitionsFilename + "&openMode=readOnly";
            string OutputsFileString = "msds:csv?file=input/Model setup/" + outputsFilename + "&openMode=readOnly";

            // Copy the initialisation files to the output directory
            System.IO.File.Copy("input/Model setup/" + simulationInitialisationFilename, outputPath + simulationInitialisationFilename, true);
            System.IO.File.Copy("input/Model setup/" + definitionsFilename, outputPath + definitionsFilename, true);
            System.IO.File.Copy("input/Model setup/" + outputsFilename, outputPath + outputsFilename, true);

            // Read in the simulation data
            DataSet InternalData = DataSet.Open(SimulationFileString);

            // Get the names of parameters in the initialization file
            var VarParameters = InternalData.Variables[1].GetData();

            // Get the values for the parameters
            var VarValues = InternalData.Variables[0].GetData();

            // Loop over the parameters
            for (int row = 0; row < VarParameters.Length; row++)
            {
                // Switch based on the name of the parameter, and write the value to the appropriate field
                switch (VarParameters.GetValue(row).ToString().ToLower())
                {
                    case "timestep units":
                        _GlobalModelTimeStepUnit = VarValues.GetValue(row).ToString();
                        break;
                    case "length of simulation (years)":
                        _NumTimeSteps = (uint)Utilities.ConvertTimeUnits("year", _GlobalModelTimeStepUnit) * Convert.ToUInt32(VarValues.GetValue(row));
                        break;
                    case "burn-in (years)":
                        _BurninTimeSteps = (uint)Utilities.ConvertTimeUnits("year", _GlobalModelTimeStepUnit) * Convert.ToUInt32(VarValues.GetValue(row));
                        break;
                    case "impact duration (years)":
                        _ImpactTimeSteps = (uint)Utilities.ConvertTimeUnits("year", _GlobalModelTimeStepUnit) * Convert.ToUInt32(VarValues.GetValue(row));
                        break;
                    case "recovery duration (years)":
                        _RecoveryTimeSteps = (uint)Utilities.ConvertTimeUnits("year", _GlobalModelTimeStepUnit) * Convert.ToUInt32(VarValues.GetValue(row));
                        break;
                    case "number timesteps":
                        _NumTimeSteps = Convert.ToUInt32(VarValues.GetValue(row));
                        break;
                    case "grid cell size":
                        _CellSize = Convert.ToDouble(VarValues.GetValue(row));
                        break;
                    case "bottom latitude":
                        _BottomLatitude = Convert.ToSingle(VarValues.GetValue(row));
                        break;
                    case "top latitude":
                        _TopLatitude = Convert.ToSingle(VarValues.GetValue(row));
                        break;
                    case "leftmost longitude":
                        _LeftmostLongitude = Convert.ToSingle(VarValues.GetValue(row));
                        break;
                    case "rightmost longitude":
                        _RightmostLongitude = Convert.ToSingle(VarValues.GetValue(row));
                        break;
                    case "run cells in parallel":
                        switch (VarValues.GetValue(row).ToString().ToLower())
                        {
                            case "yes":
                                _RunCellsInParallel = true;
                                break;
                            case "no":
                                _RunCellsInParallel = false;
                                break;
                        }
                        break;
                    case "run simulations in parallel":
                        switch (VarValues.GetValue(row).ToString().ToLower())
                        {
                            case "yes":
                                _RunSimulationsInParallel = true;
                                break;
                            case "no":
                                _RunSimulationsInParallel = false;
                                break;
                        }
                        break;
                    case "run single realm":
                        _RunRealm = VarValues.GetValue(row).ToString().ToLower();
                        break;
                    case "draw randomly":

                        switch (VarValues.GetValue(row).ToString().ToLower())
                        {
                            case "yes":
                                _DrawRandomly = true;
                                break;
                            case "no":
                                _DrawRandomly = false;
                                break;
                        }
                        break;
                    case "extinction threshold":
                        _ExtinctionThreshold = Convert.ToDouble(VarValues.GetValue(row));
                        break;
                    case "merge difference":
                        _MergeDifference = Convert.ToDouble(VarValues.GetValue(row));
                        break;
                    case "maximum number of cohorts":
                        _MaxNumberOfCohorts = Convert.ToInt32(VarValues.GetValue(row));
                        break;
                    case "read state":
                        if (VarValues.GetValue(row).ToString() != "")
                        {
                            _InputState = true;
                            this.ModelStatesInitialisation(VarValues.GetValue(row).ToString(), outputPath);
                        }
                        break;
                    case "specific location file":
                        if (VarValues.GetValue(row).ToString() != "")
                        {
                            _InitialisationFileStrings.Add("Locations", VarValues.GetValue(row).ToString());
                            _SpecificLocations = true;
                            // Copy the initialisation file to the output directory
                            System.IO.File.Copy("input/Model setup/Initial Model State Setup/" + _InitialisationFileStrings["Locations"], outputPath + _InitialisationFileStrings["Locations"], true);
                            ReadSpecificLocations(_InitialisationFileStrings["Locations"], outputPath);
                        }
                        break;
                    case "impact cell index":
                        if (VarValues.GetValue(row).ToString() != "")
                        {
                            if (VarValues.GetValue(row).ToString().ToLower() == "all")
                            {
                                ImpactAll = true;
                            }
                            else
                            {

                                string[] temp = VarValues.GetValue(row).ToString().Split(new char[] { ';' });
                                foreach (string t in temp)
                                {
                                    if (t.Split(new char[] { '-' }).Length > 1)
                                    {
                                        string[] range = t.Split(new char[] { '-' });
                                        for (uint i = Convert.ToUInt32(range[0]); i <= Convert.ToUInt32(range[1]); i++)
                                        {
                                            _ImpactCellIndices.Add(i);
                                        }
                                    }
                                    else
                                    {
                                        _ImpactCellIndices.Add(Convert.ToUInt32(Convert.ToInt32(t)));
                                    }

                                }
                            }
                        }
                        break;
                    case "dispersal only":
                        if (VarValues.GetValue(row).ToString() == "yes")
                            _DispersalOnly = true;
                        else _DispersalOnly = false;
                        break;
                    case "dispersal only type":
                        _InitialisationFileStrings.Add("DispersalOnlyType", VarValues.GetValue(row).ToString());
                        break;
                    case "plankton size threshold":
                        _PlanktonDispersalThreshold = Convert.ToDouble(VarValues.GetValue(row));
                        break;
                }
            }


            InternalData.Dispose();

            // Read in the definitions data
            InternalData = DataSet.Open(DefinitionsFileString);

            // Get the names of parameters in the initialization file
            VarParameters = InternalData.Variables[1].GetData();

            // Get the values for the parameters
            VarValues = InternalData.Variables[0].GetData();

            // Loop over the parameters
            for (int row = 0; row < VarParameters.Length; row++)
            {
                // Switch based on the name of the parameter, and write the value to the appropriate field
                switch (VarParameters.GetValue(row).ToString().ToLower())
                {
                    case "mass bin filename":
                        // Set up the mass bins as specified in the initialization file
                        _ModelMassBins.SetUpMassBins(VarValues.GetValue(row).ToString(), outputPath);
                        break;
                    case "environmental data file":
                        _InitialisationFileStrings.Add("Environmental", VarValues.GetValue(row).ToString());
                        // Read environmental data layers
                        this.ReadEnvironmentalLayers(VarValues.GetValue(row).ToString(), outputPath);
                        break;
                    case "cohort functional group definitions file":
                        Console.WriteLine("Reading functional group definitions...\n");
                        _InitialisationFileStrings.Add("CohortFunctional", VarValues.GetValue(row).ToString());
                        // Open a the specified csv file and set up the cohort functional group definitions
                        _CohortFunctionalGroupDefinitions = new FunctionalGroupDefinitions(VarValues.GetValue(row).ToString(), outputPath);
                        break;
                    case "stock functional group definitions file":
                        _InitialisationFileStrings.Add("StockFunctional", VarValues.GetValue(row).ToString());
                        // Open a the specified csv file and set up the stock functional group definitions
                        _StockFunctionalGroupDefinitions = new FunctionalGroupDefinitions(VarValues.GetValue(row).ToString(), outputPath);
                        break;
                    case "ecological parameters file":
                        EcologicalParameters.ReadEcologicalParameters(VarValues.GetValue(row).ToString(), outputPath);
                        break;

                }
            }

            InternalData.Dispose();

            // Read in the outputs data
            InternalData = DataSet.Open(OutputsFileString);

            // Get the names of parameters in the initialization file
            VarParameters = InternalData.Variables[1].GetData();

            // Get the values for the parameters
            VarValues = InternalData.Variables[0].GetData();

            // Loop over the parameters
            for (int row = 0; row < VarParameters.Length; row++)
            {
                // Switch based on the name of the parameter, and write the value to the appropriate field
                switch (VarParameters.GetValue(row).ToString().ToLower())
                {
                    case "track processes":
                        switch (VarValues.GetValue(row).ToString().ToLower())
                        {
                            case "yes":
                                _TrackProcesses = true;
                                break;
                            case "no":
                                _TrackProcesses = false;
                                break;
                        }
                        break;
                    case "track cross cell processes":
                        switch (VarValues.GetValue(row).ToString().ToLower())
                        {
                            case "yes":
                                _TrackCrossCellProcesses = true;
                                break;
                            case "no":
                                _TrackCrossCellProcesses = false;
                                break;
                        }
                        break;
                    case "track global processes":
                        switch (VarValues.GetValue(row).ToString().ToLower())
                        {
                            case "yes":
                                _TrackGlobalProcesses = true;
                                break;
                            case "no":
                                _TrackGlobalProcesses = false;
                                break;
                        }
                        break;
                    case "new cohorts filename":
                        _ProcessTrackingOutputs.Add("NewCohortsOutput", VarValues.GetValue(row).ToString());
                        break;
                    case "maturity filename":
                        _ProcessTrackingOutputs.Add("MaturityOutput", VarValues.GetValue(row).ToString());
                        break;
                    case "biomasses eaten filename":
                        _ProcessTrackingOutputs.Add("BiomassesEatenOutput", VarValues.GetValue(row).ToString());
                        break;
                    case "trophic flows filename":
                        _ProcessTrackingOutputs.Add("TrophicFlowsOutput", VarValues.GetValue(row).ToString());
                        break;
                    case "growth filename":
                        _ProcessTrackingOutputs.Add("GrowthOutput", VarValues.GetValue(row).ToString());
                        break;
                    case "metabolism filename":
                        _ProcessTrackingOutputs.Add("MetabolismOutput", VarValues.GetValue(row).ToString());
                        break;
                    case "npp output filename":
                        _ProcessTrackingOutputs.Add("NPPOutput", VarValues.GetValue(row).ToString());
                        break;
                    case "predation flows filename":
                        _ProcessTrackingOutputs.Add("PredationFlowsOutput", VarValues.GetValue(row).ToString());
                        break;
                    case "herbivory flows filename":
                        _ProcessTrackingOutputs.Add("HerbivoryFlowsOutput", VarValues.GetValue(row).ToString());
                        break;
                    case "mortality filename":
                        _ProcessTrackingOutputs.Add("MortalityOutput", VarValues.GetValue(row).ToString());
                        break;
                    case "extinction filename":
                        _ProcessTrackingOutputs.Add("ExtinctionOutput", VarValues.GetValue(row).ToString());
                        break;
                    case "output detail":
                        _InitialisationFileStrings.Add("OutputDetail", VarValues.GetValue(row).ToString());
                        break;
                    case "live outputs":
                        if (VarValues.GetValue(row).ToString() == "yes")
                            _LiveOutputs = true;
                        else _LiveOutputs = false;
                        break;
                    case "track marine specifics":
                        if (VarValues.GetValue(row).ToString() == "yes")
                            _TrackMarineSpecifics = true;
                        else _TrackMarineSpecifics = false;
                        break;
                    case "output metrics":
                        if (VarValues.GetValue(row).ToString() == "yes")
                            _OutputMetrics = true;
                        else _OutputMetrics = false;
                        break;
                    case "output model state timesteps":

                        if (VarValues.GetValue(row).ToString() != "no")
                        {
                            string[] OutputStateTimesSteps = VarValues.GetValue(row).ToString().Split(new char[] { ';' });
                            foreach (string t in OutputStateTimesSteps)
                            {
                                if (t.Split(new char[] { '-' }).Length > 1)
                                {
                                    string[] range = t.Split(new char[] { '-' });
                                    for (uint i = Convert.ToUInt32(range[0]); i <= Convert.ToUInt32(range[1]); i++)
                                    {
                                        _OutputStateTimestep.Add(i);
                                    }
                                }
                                else
                                {
                                    _OutputStateTimestep.Add(Convert.ToUInt32(Convert.ToInt32(t)));
                                }
                            }
                        }

                        break;
                }
            }

            InternalData.Dispose();

        }

        /// <summary>
        /// Reads in and holds a model state from file
        /// </summary>
        /// <param name="modelStateFileSpecification">Filename of the setup file specifying the model state datafile</param>
        private void ModelStatesInitialisation(string modelStateFileSpecification, string outputPath)
        {
            
            string FileString = "msds:csv?file=input/Model setup/Initial model state setup/" + modelStateFileSpecification + "&openMode=readOnly";

            //Copy the file containing the list of environmental layers to the output directory
            System.IO.File.Copy("input/Model setup/Initial model state setup/" + modelStateFileSpecification, outputPath + modelStateFileSpecification, true);

            // Read in the data
            DataSet InternalData = DataSet.Open(FileString);

            // Loop over the parameters associated with the list of environmental layers

            foreach (Variable v in InternalData.Variables)
            {

                // Get the name of the parameter
                string HeaderName = v.Name;

                // Create a local copy of all of the values associated with this parameter
                var TempValues = v.GetData();

                // Switch based on the name of the parameter, and store the parameter values in the appropriate list
                switch (HeaderName.ToLower())
                {
                    case "path":
                        for (int ii = 0; ii < TempValues.Length; ii++) _ModelStatePath.Add(TempValues.GetValue(ii).ToString());
                        break;
                    case "filename":
                        for (int ii = 0; ii < TempValues.Length; ii++) _ModelStateFilename.Add(TempValues.GetValue(ii).ToString());
                        break;
                    case "filetype":
                        for (int ii = 0; ii < TempValues.Length; ii++) _ModelStateType.Add(TempValues.GetValue(ii).ToString());
                        break;
                }
            }

            /*
            for (int i = 0; i < ModelStatePath.Count; i++)
            {
                _ModelStates.Add(new InputModelState(ModelStatePath[i], ModelStateFilename[i]));
            }
            */
            //uint[] RandomCellIndices = Utilities.RandomlyOrderedIndices((uint)ModelStatePath.Count);

            ////Randomise the state ordering
            //foreach (int i in RandomCellIndices)
            //{
            //    _ModelStates.Add(new InputModelState(ModelStatePath[i], ModelStateFilename[i]));
            //}
        }

        
        /// <summary>
        /// Copy parameter values to a text file in the specified output directory
        /// </summary>
        /// <param name="outputDirectory">THe directory for outputs</param>
        public void CopyParameterValues(string outputDirectory)
        {
            // Create a stream write object to write the parameter values to
            StreamWriter sw = new StreamWriter(outputDirectory + "Parameters.txt");

            // Write out the column headings
            sw.WriteLine("Ecological process\tParameter name\tParameter value");

            // Create dummy instances of the ecological processes
            RevisedHerbivory DummyHerbivory = new RevisedHerbivory(0.0, _GlobalModelTimeStepUnit);
            RevisedPredation DummyPredation = new RevisedPredation(0.0, _GlobalModelTimeStepUnit);
            MetabolismEndotherm DummyEndoMetabolism = new MetabolismEndotherm(_GlobalModelTimeStepUnit);
            MetabolismEctotherm DummyEctoMetabolism = new MetabolismEctotherm(_GlobalModelTimeStepUnit);
            BackgroundMortality DummyBackgroundMortality = new BackgroundMortality(_GlobalModelTimeStepUnit);
            SenescenceMortality DummySenescenceMortality = new SenescenceMortality(_GlobalModelTimeStepUnit);
            StarvationMortality DummyStarvationMortality = new StarvationMortality(_GlobalModelTimeStepUnit);
            ReproductionBasic DummyReproduction = new ReproductionBasic(_GlobalModelTimeStepUnit, _DrawRandomly);
            DiffusiveDispersal DummyDiffusiveDispersal = new DiffusiveDispersal(_GlobalModelTimeStepUnit, _DrawRandomly);
            RevisedTerrestrialPlantModel DummyPlantModel = new RevisedTerrestrialPlantModel();
            Activity DummyActivityModel = new Activity();


            // Call the methods in these processes that write the parameter values out
            DummyHerbivory.WriteOutParameterValues(sw);
            DummyPredation.WriteOutParameterValues(sw);
            DummyEndoMetabolism.WriteOutParameterValues(sw);
            DummyEctoMetabolism.WriteOutParameterValues(sw);
            DummyBackgroundMortality.WriteOutParameterValues(sw);
            DummySenescenceMortality.WriteOutParameterValues(sw);
            DummyStarvationMortality.WriteOutParameterValues(sw);
            DummyReproduction.WriteOutParameterValues(sw);
            DummyDiffusiveDispersal.WriteOutParameterValues(sw);
            DummyPlantModel.WriteOutParameterValues(sw);
            DummyActivityModel.WriteOutParameterValues(sw);


            sw.Dispose();

        }


        /// <summary>
        /// Read in the specified locations in which to run the model
        /// </summary>
        /// <param name="specificLocationsFile">The name of the file with specific locations information</param>
        /// <param name="outputPath">The path to the output folder in which to copy the specific locations file</param>
        public void ReadSpecificLocations(string specificLocationsFile, string outputPath)
        {
            _CellList = new List<uint[]>();

            List<double> LatitudeList = new List<double>();
            List<double> LongitudeList = new List<double>();

            Console.WriteLine("Reading in specific location data");
            Console.WriteLine("");

            // construct file name
            string FileString = "msds:csv?file=input/Model setup/Initial Model State Setup/" + specificLocationsFile + "&openMode=readOnly";

            // Read in the data
            DataSet InternalData = DataSet.Open(FileString);

            foreach (Variable v in InternalData.Variables)
            {
                //Get the name of the variable currently referenced in the dataset
                string HeaderName = v.Name;
                //Copy the values for this variable into an array
                var TempValues = v.GetData();

                switch (HeaderName.ToLower())
                {
                    // Add the latitude and longitude values to the appropriate list
                    case "latitude":
                        for (int ii = 0; ii < TempValues.Length; ii++) LatitudeList.Add(Convert.ToDouble(TempValues.GetValue(ii).ToString()));
                        break;
                    case "longitude":
                        for (int ii = 0; ii < TempValues.Length; ii++) LongitudeList.Add(Convert.ToDouble(TempValues.GetValue(ii).ToString()));
                        break;
                    default:
                        Console.WriteLine("Variable defined in the specific location file but not processed: ", HeaderName);
                        break;
                }
            }

            // Loop over cells defined in the specific locations file
            for (int ii = 0; ii < LatitudeList.Count; ii++)
            {
                // Define a vector to hold the longitude and latitude index for this cell
                uint[] cellIndices = new uint[2];

                // Get the longitude and latitude indices for the current grid cell
                cellIndices[0] = (uint)Math.Floor((LatitudeList.ElementAt(ii) - BottomLatitude) / CellSize);
                cellIndices[1] = (uint)Math.Floor((LongitudeList.ElementAt(ii) - LeftmostLongitude) / CellSize);

                // Add these indices to the list of active cells
                _CellList.Add(cellIndices);
            }


        }


        /// <summary>
        /// Reads the environmental layers listed in the specified file containing a list of environmental layers
        /// </summary>
        /// <param name="environmentalLayerFile">The name of the file containing the list of environmental layers</param>
        /// <param name="outputPath">The path to folder in which outputs will be stored</param>
        public void ReadEnvironmentalLayers(string environmentalLayerFile, string outputPath)
        {
            Console.WriteLine("Reading in environmental data:");

            // Declare lists to hold the information required to read the environmental layers
            List<string> Sources = new List<string>();
            List<string> Folders = new List<string>();
            List<string> Filenames = new List<string>();
            List<string> DatasetNames = new List<string>();
            List<string> FileTypes = new List<string>();
            List<string> LayerName = new List<string>();
            List<string> StaticLayer = new List<string>();
            List<string> Extensions = new List<string>();
            List<string> Resolutions = new List<string>();
            List<string> MethodUnits = new List<string>();

            // Variable to store the file name of the environmental data files
            string TempFilename;

            // Construct the full URI for the file  containing the list of environmental layers
            string FileString = "msds:csv?file=input/Model setup/Environmental data layer list/" + environmentalLayerFile + "&openMode=readOnly";

            //Copy the file containing the list of environmental layers to the output directory
            System.IO.File.Copy("input/Model setup/Environmental data layer list/" + environmentalLayerFile, outputPath + environmentalLayerFile, true);

            StreamReader r_env = new StreamReader("input/Model setup/Environmental data layer list/" + environmentalLayerFile);
            string l;
            char[] comma = ",".ToCharArray();

            string[] f;
            int col;
            // Read in the data
            DataSet InternalData = DataSet.Open(FileString);
            l = r_env.ReadLine();
            while (!r_env.EndOfStream)
            {
                l = r_env.ReadLine();
                // Split fields by commas
                f = l.Split(comma);
                //zero the column index
                col = 0;
                // Lists of the different fields
                Sources.Add(f[col++]);
                Folders.Add(f[col++]);
                Filenames.Add(f[col++]);
                Extensions.Add(f[col++]);
                DatasetNames.Add(f[col++]);
                FileTypes.Add(f[col++]);
                LayerName.Add(f[col++]);
                StaticLayer.Add(f[col++]);
                Resolutions.Add(f[col++]);
                MethodUnits.Add(f[col++]);
            }


            for (int ii = 0; ii < MethodUnits.Count; ii++)
            {
                Units.Add(LayerName[ii], MethodUnits[ii]);
            }

            // Check that there are the same number of values for all parameters
            Debug.Assert(Folders.Count() == Filenames.Count() && Filenames.Count() == DatasetNames.Count() && DatasetNames.Count() == FileTypes.Count() && FileTypes.Count() == LayerName.Count(),
                "Error in Environmental Data Layer import lists - unequal number of filenames, dataset names, filetypes and datalayer names");

            // Loop over parameter values
            for (int ii = 0; ii < Filenames.Count(); ii++)
            {
                Console.Write("\r{0} Variable {1} of {2}: {3}\n", Sources[ii], ii + 1, Filenames.Count, Filenames[ii]);
                
                if (Sources[ii].ToLower().Equals("local"))
                {
                    // For layers where the file format is ESRI ASCII grid, the dataset name is the same as the file name
                    if (FileTypes[ii].ToLower().Equals("esriasciigrid"))
                    {
                        DatasetNames[ii] = Filenames[ii];
                    }
                    // Generate the appropriate file name for the environmental data layer
                    if (Folders[ii].ToLower().Equals("input"))
                    {
                        TempFilename = "input/Data/" + Filenames[ii];
                    }
                    else
                    {
                        TempFilename = Folders[ii] + "/" + Filenames[ii];
                    }
                    Filenames[ii] = TempFilename + Extensions[ii];
                    // Read in and store the environmental data
                    if (StaticLayer[ii] == "Y")
                    {
                        EnviroStack.Add(LayerName[ii], new EnviroData(Filenames[ii], DatasetNames[ii], FileTypes[ii], Resolutions[ii], MethodUnits[ii]));
                    }
                    else
                    {
                        EnviroStackTemporal.Add(LayerName[ii], new EnviroDataTemporal(Filenames[ii], DatasetNames[ii], FileTypes[ii], Resolutions[ii], MethodUnits[ii]));
                    }
                }
                else if (Sources[ii].ToLower().Equals("fetchclimate"))
                {

                    if (!EnviroStack.ContainsKey(LayerName[ii]))
                        if (_SpecificLocations)
                        {
                            EnviroStack.Add(LayerName[ii], new EnviroData(DatasetNames[ii], Resolutions[ii], (double)BottomLatitude, (double)LeftmostLongitude, (double)TopLatitude, (double)RightmostLongitude, (double)CellSize, _CellList, EnvironmentalDataSource.ANY));
                        }
                        else
                        {
                            EnviroStack.Add(LayerName[ii], new EnviroData(DatasetNames[ii], Resolutions[ii], (double)BottomLatitude, (double)LeftmostLongitude, (double)TopLatitude, (double)RightmostLongitude, (double)CellSize, EnvironmentalDataSource.ANY));
                        }

                }
            }
            Console.WriteLine("\n\n");
        }

    }



}
