using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using System.IO;

using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

using Timing;

namespace Madingley
{

    /// <summary>
    /// A class to perform all operations involved in outputting the results to console, screen or file
    /// </summary>
    class OutputCell
    {
        /// <summary>
        /// Designates the level of output detail
        /// </summary>
        private enum OutputDetailLevel { Low, Medium, High };

        /// <summary>
        /// An instance of the enumerator to designate output detail level
        /// </summary>
        OutputDetailLevel ModelOutputDetail;

        /// <summary>
        /// A dataset to store the live screen view
        /// </summary>
        private DataSet DataSetToViewLive;
        
        /// <summary>
        /// A version of the basic outputs dataset to hold data for output in memory while running the model
        /// </summary>
        private DataSet BasicOutputMemory;

        /// <summary>
        /// A memory version of the mass bins output to store data during the model run
        /// </summary>
        private DataSet MassBinsOutputMemory;

        /// <summary>
        /// A memory version of the tracked cohorts output to hold data during the model run
        /// </summary>
        private DataSet TrackedCohortsOutputMemory;

        /// <summary>
        /// Holds a list of the functional group indices correpsonding to each unique cohort trait
        /// </summary>
        private SortedList<string, int[]> CohortTraitIndices = new SortedList<string, int[]>();

        /// <summary>
        /// Holds a list of the functional group indices correpsonding to each unique cohort trait in the marine realm
        /// </summary>
        private SortedList<string, int[]> CohortTraitIndicesMarine = new SortedList<string, int[]>();
        
        /// <summary>
        /// Holds a list of the functional group indices corresponding to each unique stock trait
        /// </summary>
        private SortedList<string, int[]> StockTraitIndices = new SortedList<string, int[]>();

        /// <summary>
        /// Holds a list of the functional group indices corresponding to each unique stock trait
        /// </summary>
        private SortedList<string, int[]> StockTraitIndicesMarine = new SortedList<string, int[]>();

        /// <summary>
        /// The total living biomass in the model
        /// </summary>
        private double TotalLivingBiomass;

        /// <summary>
        /// The total living biomass in the model
        /// </summary>
        private double TotalLivingBiomassDensity;

        /// <summary>
        /// The total heterotroph biomass in the model
        /// </summary>
        private double TotalHeterotrophBiomassDensity;

        /// <summary>
        /// The total heterotroph abundance in the model
        /// </summary>
        private double TotalHeterotrophAbundanceDensity;


        /// <summary>
        /// Total NPP incoming from the marine model
        /// </summary>
        private double TotalIncomingNPP;

        /// <summary>
        /// Total densities of all cohorts within each combination of cohort traits
        /// </summary>
        private SortedList<string, double> TotalDensitiesOut = new SortedList<string, double>();
        
        /// <summary>
        /// Total densities of all cohorts within each combination of cohort traits (marine)
        /// </summary>
        private SortedList<string, double> TotalDensitiesMarineOut = new SortedList<string, double>();

        /// <summary>
        /// Total biomass densities of all cohorts within each combination of cohort traits
        /// </summary>
        private SortedList<string, double> TotalBiomassDensitiesOut = new SortedList<string, double>();
        
        /// <summary>
        /// Total biomass densities of all cohorts within each combination of cohort traits
        /// </summary>
        private SortedList<string, double> TotalBiomassDensitiesMarineOut = new SortedList<string, double>();


        /// <summary>
        /// List of vectors of abundances in mass bins corresponding with each unique trait value
        /// </summary>
        private SortedList<string, double[]> AbundancesInMassBins = new SortedList<string, double[]>();

        /// <summary>
        /// List of vectors of biomasses in mass bins corresponding with each unique trait value
        /// </summary>
        private SortedList<string, double[]> BiomassesInMassBins = new SortedList<string, double[]>();

        /// <summary>
        /// List of arrays of abundance in juvenile vs. adult mass bins correpsonding with each unique trait value
        /// </summary>
        private SortedList<string, double[,]> AbundancesInJuvenileAdultMassBins = new SortedList<string, double[,]>();

        /// <summary>
        /// List of arrays of biomass in juvenile vs. adult mass bins correpsonding with each unique trait value
        /// </summary>
        private SortedList<string, double[,]> BiomassesInJuvenileAdultMassBins = new SortedList<string, double[,]>();

        /// <summary>
        /// The number of mass bins to use in model outputs
        /// </summary>
        private int MassBinNumber;

        /// <summary>
        /// The mass bins to use in model outputs
        /// </summary>
        private float[] MassBins;

        /// <summary>
        /// The mass bin handler for the mass bins to use in the model output
        /// </summary>
        private MassBinsHandler _MassBinHandler;

        /// <summary>
        /// The upper limit for the y-axis of the live output
        /// </summary>
        private double MaximumYValue;

        /// <summary>
        /// The time steps in this model simulation
        /// </summary>
        private float[] TimeSteps;     
        
        /// <summary>
        /// List to hold cohort IDs of tracked cohorts
        /// </summary>
        List<uint> TrackedCohorts;

        /// <summary>
        /// The path to the output folder
        /// </summary>
        private string _OutputPath;
        /// <summary>
        /// Get the path to the output folder
        /// </summary>
        public string OutputPath { get { return _OutputPath; } }

        /// <summary>
        /// The suffix to apply to all outputs from this grid cell
        /// </summary>
        private string _OutputSuffix;
        /// <summary>
        /// Get the suffix for ouputs for this grid cell
        /// </summary>
        public string OutputSuffix
        { get { return _OutputSuffix; }}
        

        /// <summary>
        /// The cohort traits to be considered in the outputs
        /// </summary>
        private string[] CohortTraits;

        /// <summary>
        /// All unique values of the traits to be considered in outputs (terrestrial only)
        /// </summary>
        private SortedDictionary<string, string[]> CohortTraitValues;

        /// <summary>
        /// All unique values of the traits to be considered in marine outputs
        /// </summary>
        private SortedDictionary<string, string[]> CohortTraitValuesMarine;

        /// <summary>
        /// The stock traits to be considered in the outputs
        /// </summary>
        private string[] StockTraits;

        /// <summary>
        /// The marine stock traits to be considered in the outputs
        /// </summary>
        private string[] StockTraitsMarine;

        /// <summary>
        /// All unique values of the traits to be considered in the outputs
        /// </summary>
        private SortedDictionary<string, string[]> StockTraitValues;

        /// <summary>
        /// All unique values of the traits to be considered in the marine outputs
        /// </summary>
        private SortedDictionary<string, string[]> StockTraitValuesMarine;

        /// <summary>
        /// Vector of individual body masses of the tracked cohorts
        /// </summary>
        private double[] TrackedCohortIndividualMasses;

        /// <summary>
        /// Vector of abundances of the tracked cohorts
        /// </summary>
        private double[] TrackedCohortAbundances;

        /// <summary>
        /// Instance of the class to convert data between arrays and SDS objects
        /// </summary>
        private ArraySDSConvert DataConverter;

        /// <summary>
        /// Intance of the class to create SDS objects
        /// </summary>
        private CreateSDSObject SDSCreator;

        /// <summary>
        /// An instance of the class to view grid results
        /// </summary>
        private ViewGrid GridViewer;

        /// <summary>
        /// Whether to display live outputs during the model run
        /// </summary>
        private Boolean LiveOutputs;

        /// <summary>
        ///  Track marine specific functional groups (i.e. plankton, baleen whales)
        /// </summary>
        private Boolean TrackMarineSpecifics;

        /// <summary>
        /// The size threshold for determining whether an organism is planktonic
        /// </summary>
        private double PlanktonSizeThreshold;


        /// <summary>
        /// Indicates whether to output metric information
        /// </summary>
        private Boolean OutputMetrics;

        /// <summary>
        /// Instance of the class to calculate ecosystem metrics
        /// </summary>
        private EcosytemMetrics Metrics;


        /// <summary>
        /// Constructor for the cell output class
        /// </summary>
        /// <param name="outputDetail">The level of detail to include in the ouputs: 'low', 'medium' or 'high'</param>
        /// <param name="modelInitialisation">Model initialisation object</param>
        /// <param name="cellIndex">The index of the current cell in the list of all cells to run the model for</param>
        public OutputCell(string outputDetail, MadingleyModelInitialisation modelInitialisation, 
            int cellIndex)
        {
            // Set the output path
            _OutputPath = modelInitialisation.OutputPath;

            // Set the initial maximum value for the y-axis of the live display
            MaximumYValue = 1000000;

            // Set the local copy of the mass bin handler from the model initialisation
            _MassBinHandler = modelInitialisation.ModelMassBins;

            // Get the number of mass bins to be used
            MassBinNumber = _MassBinHandler.NumMassBins;

            // Get the specified mass bins
            MassBins = modelInitialisation.ModelMassBins.GetSpecifiedMassBins();

            // Set the output detail level
            if (outputDetail == "low")
                ModelOutputDetail = OutputDetailLevel.Low;
            else if (outputDetail == "medium")
                ModelOutputDetail = OutputDetailLevel.Medium;
            else if (outputDetail == "high")
                ModelOutputDetail = OutputDetailLevel.High;
            else
                Debug.Fail("Specified output detail level is not valid, must be 'low', 'medium' or 'high'");

            // Get whether to track marine specifics
            TrackMarineSpecifics = modelInitialisation.TrackMarineSpecifics;

            // Get whether to track marine specifics
            OutputMetrics = modelInitialisation.OutputMetrics;

            //Initialise the EcosystemMetrics class
            Metrics = new EcosytemMetrics();

            // Initialise the data converter
            DataConverter = new ArraySDSConvert();

            // Initialise the SDS object creator
            SDSCreator = new CreateSDSObject();

            // Initialise the grid viewer
            GridViewer = new ViewGrid();

            // Set the local variable designating whether to display live outputs
            if (modelInitialisation.LiveOutputs)
                LiveOutputs = true;

        }

        /// <summary>
        /// Spawn dataset viewer for the live outputs
        /// </summary>
        /// <param name="NumTimeSteps">The number of time steps in the model run</param>
        public void SpawnDatasetViewer(uint NumTimeSteps)
        {
            Console.WriteLine("Spawning Dataset Viewer\n");

            // Intialise the SDS object for the live view
            DataSetToViewLive = SDSCreator.CreateSDSInMemory(true);

            // Check the output detail level
            if (ModelOutputDetail == OutputDetailLevel.Low)
            {
                // For low detail level, just show total living biomass
                DataSetToViewLive.Metadata["VisualHints"] = "\"Total living biomass\"[Time step]; Style:Polyline; Visible: 0,1," +
                    NumTimeSteps.ToString() + "," + MaximumYValue.ToString() +
                    "; LogScale:Y; Stroke:#D95F02; Thickness:3; Title:\"Total Biomass" + "\"";
            }
            else
            {
                // For medium and high detail levels, show biomass by trophic level
                DataSetToViewLive.Metadata["VisualHints"] = "\"autotroph biomass\"[Time step]; Style:Polyline; Visible: 0,1,"
                    + NumTimeSteps.ToString() + ","
                    + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FF008040;Thickness:3;;\"carnivore biomass\"[Time step] ; Style:Polyline; Visible: 0,1,"
                    + NumTimeSteps.ToString() + ","
                    + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FFFF0000;Thickness:3;;\"herbivore biomass\"[Time step] ; Style:Polyline; Visible: 0,1,"
                    + NumTimeSteps.ToString() + ","
                    + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FF00FF00;Thickness:3;;\"omnivore biomass\"[Time step] ; Style:Polyline; Visible: 0,1,"
                    + NumTimeSteps.ToString() + ","
                    + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FF0000FF;Thickness:3; Title:\"Biomass Densities";
            }


            // Start viewing
            GridViewer.AsynchronousView(ref DataSetToViewLive, "");

        }

        /// <summary>
        /// Set up all outputs (live, console and file) prior to the model run
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid that output data will be derived from</param>
        /// <param name="cohortFunctionalGroupDefinitions">The definitions for cohort functional groups</param>
        /// <param name="stockFunctionalGroupDefinitions">The definitions for stock functional groups</param>
        /// <param name="numTimeSteps">The number of time steps in the model run</param>
        /// <param name="outputFilesSuffix">The suffix to be applied to all output files from the current model run</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="cellIndex">The number of the current grid cell in the list of indices of active cells</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        public void SetUpOutputs(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortFunctionalGroupDefinitions,
            FunctionalGroupDefinitions stockFunctionalGroupDefinitions, uint numTimeSteps, string outputFilesSuffix, List<uint[]> cellIndices,
            int cellIndex, Boolean marineCell)
        {
            Console.WriteLine("Setting up grid cell outputs...\n");

            // Set the suffix for all output files
            _OutputSuffix = outputFilesSuffix + "_Cell" + cellIndex;

            // Create vector to hold the values of the time dimension
            TimeSteps = new float[numTimeSteps + 1];

            // Set the first value to be -1 (this will hold initial outputs)
            TimeSteps[0] = 0;

            // Fill other values from 0 (this will hold outputs during the model run)
            for (int i = 1; i < numTimeSteps + 1; i++)
            {
                TimeSteps[i] = i;
            }

            // Initialise the trait based outputs
            InitialiseTraitBasedOutputs(cohortFunctionalGroupDefinitions, stockFunctionalGroupDefinitions, marineCell);

            // Setup low-level outputs
            SetUpLowLevelOutputs(numTimeSteps, ecosystemModelGrid);

            if ((ModelOutputDetail == OutputDetailLevel.Medium) || (ModelOutputDetail == OutputDetailLevel.High))
            {
                // Setup medium-level outputs
                SetupMediumLevelOutputs(ecosystemModelGrid, marineCell);

                if (ModelOutputDetail == OutputDetailLevel.High)
                {                   
                    // Setup high-level outputs
                    SetUpHighLevelOutputs(ecosystemModelGrid, cellIndices,cellIndex, cohortFunctionalGroupDefinitions, marineCell);
                }
            }


        }

        /// <summary>
        /// Set up the necessary architecture for generating outputs arranged by trait value
        /// </summary>
        /// <param name="cohortFunctionalGroupDefinitions">Functional group definitions for cohorts in the model</param>
        /// <param name="stockFunctionalGroupDefinitions">Functional group definitions for stocks in the model</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void InitialiseTraitBasedOutputs(FunctionalGroupDefinitions cohortFunctionalGroupDefinitions, FunctionalGroupDefinitions 
            stockFunctionalGroupDefinitions, Boolean marineCell)
        {
            // Define the cohort traits that will be used to separate outputs
            CohortTraits = new string[2] { "Nutrition source", "Endo/Ectotherm"};

            // Declare a sorted dictionary to hold all unique trait values
            CohortTraitValues = new SortedDictionary<string, string[]>();
            
            // Declare a sorted dictionary to hold all unique trait values for marine systems
            CohortTraitValuesMarine = new SortedDictionary<string, string[]>();

            // Get the list of functional group indices corresponding to each unique trait value
            if (marineCell)
            {
                // Add all unique trait values to the sorted dictionary
                foreach (string Trait in CohortTraits)
                {
                    CohortTraitValuesMarine.Add(Trait, cohortFunctionalGroupDefinitions.GetUniqueTraitValues(Trait));
                }
                
                foreach (string Trait in CohortTraits)
                {
                    foreach (string TraitValue in CohortTraitValuesMarine[Trait])
                    {
                        // Only add indices of marine functional groups
                        int[] TempIndices = cohortFunctionalGroupDefinitions.GetFunctionalGroupIndex(Trait, TraitValue, false);
                        Boolean[] TempIndices2 = new Boolean[TempIndices.GetLength(0)];
                        for (int ii = 0; ii < TempIndices.GetLength(0); ii++)
                        {
                            if (cohortFunctionalGroupDefinitions.GetTraitNames("Realm", TempIndices[ii]).Equals("Marine", StringComparison.OrdinalIgnoreCase))
                            {
                                TempIndices2[ii] = true;
                            }
                        }

                        // Extract only the indices which are marine 
                        int[] TempIndices3 = Enumerable.Range(0, TempIndices2.Length).Where(i => TempIndices2[i]).ToArray();

                        if (TempIndices3.Length > 0)
                        {
                            // Extract the values at these indices
                            for (int ii = 0; ii < TempIndices3.Length; ii++)
                            {
                                TempIndices3[ii] = TempIndices[TempIndices3[ii]];
                            }

                            // Add in the indices for this functional group and this realm
                            CohortTraitIndicesMarine.Add(TraitValue, TempIndices3);
                        }   
                    }
                }

                if (TrackMarineSpecifics)
                {
                    // Add in the specific classes of zooplankton and baleen whales

                    // There are functional groups representing obligate zooplankton
                    string[] TempString = new string[1] { "Obligate zooplankton" };
                    CohortTraitValuesMarine.Add("Obligate zooplankton", TempString);
                    CohortTraitIndicesMarine.Add("Obligate zooplankton", cohortFunctionalGroupDefinitions.GetFunctionalGroupIndex("Mobility", "planktonic", false));

                    // Whales have a special dietary index
                    TempString = new string[1] { "Baleen whales" };
                    CohortTraitValuesMarine.Add("Baleen whales", TempString);
                    CohortTraitIndicesMarine.Add("Baleen whales", cohortFunctionalGroupDefinitions.GetFunctionalGroupIndex("Diet", "allspecial", false));

                    // But we also want all zooplankton, including larval/juvenile stages of other cohorts
                    int[] ZooplanktonIndices1 = cohortFunctionalGroupDefinitions.GetFunctionalGroupIndex("Mobility", "planktonic", false);

                    // Then there are all of the other groups which may have planktonic juveniles. In the ModelGrid.cs class, these cohorts are checked to see 
                    // if they have a weight of less than the planktonic dispersal threshold.
                    TempString = new string[3] { "Realm", "Endo/Ectotherm", "Mobility" };
                    string[] TempString2 = new string[3] { "marine", "ectotherm", "mobile" };
                    int[] ZooplanktonIndices2 = cohortFunctionalGroupDefinitions.GetFunctionalGroupIndex(TempString, TempString2, true);
                    CohortTraitIndicesMarine.Add("Zooplankton (all)", ZooplanktonIndices2.Concat(ZooplanktonIndices1).ToArray());
                }

                // Add unique trait values to each of the lists that will contain output data arranged by trait value
                foreach (string TraitValue in CohortTraitIndicesMarine.Keys)
                {
                    TotalBiomassDensitiesMarineOut.Add(TraitValue, 0.0);
                    TotalDensitiesMarineOut.Add(TraitValue, 0.0);
                }             
            }
            else
            {
                // Add all unique trait values to the sorted dictionary
                foreach (string Trait in CohortTraits)
                {
                    CohortTraitValues.Add(Trait, cohortFunctionalGroupDefinitions.GetUniqueTraitValues(Trait));
                }

                foreach (string Trait in CohortTraits)
                {
                    foreach (string TraitValue in CohortTraitValues[Trait])
                    {
                        // Only add indices of terrestrial functional groups
                        int[] TempIndices = cohortFunctionalGroupDefinitions.GetFunctionalGroupIndex(Trait, TraitValue, false);
                        Boolean[] TempIndices2 = new Boolean[TempIndices.GetLength(0)];
                        for (int ii = 0; ii < TempIndices.GetLength(0); ii++)
                        {
                            if (cohortFunctionalGroupDefinitions.GetTraitNames("Realm", TempIndices[ii]).Equals("Terrestrial", StringComparison.OrdinalIgnoreCase))
                            {
                                TempIndices2[ii] = true;
                            }        
                        }

                        // Extract only the indices which are terrestrial 
                        int[] TempIndices3 = Enumerable.Range(0, TempIndices2.Length).Where(i => TempIndices2[i]).ToArray();
                        
                        if (TempIndices3.Length > 0)
                        {
                            // Extract the values at these indices
                            for (int ii = 0; ii < TempIndices3.Length; ii++)
                            {
                                TempIndices3[ii] = TempIndices[TempIndices3[ii]];
                            }

                            // Add in the indices for this functional group and this realm
                            CohortTraitIndices.Add(TraitValue, TempIndices3);
                        }   
                    }
                }

                // Add unique trait values to each of the lists that will contain output data arranged by trait value
                foreach (string TraitValue in CohortTraitIndices.Keys)
                {
                    TotalBiomassDensitiesOut.Add(TraitValue, 0.0);
                    TotalDensitiesOut.Add(TraitValue, 0.0);
                }
            }

            if (marineCell)
            {
                // Define the stock traits that will be used to separate outputs
                StockTraitsMarine = new string[1] { "Heterotroph/Autotroph"};

                // Re-initialise the sorted dictionary to hold all unique trait values
                StockTraitValuesMarine = new SortedDictionary<string, string[]>();
                
                // Add all unique stock trait values to the sorted dictionary
                foreach (string Trait in StockTraitsMarine)
                {
                    StockTraitValuesMarine.Add(Trait, stockFunctionalGroupDefinitions.GetUniqueTraitValues(Trait));
                }
                
                // Get the list of functional group indices corresponding to each unique marine trait value
                foreach (string Trait in StockTraitsMarine)
                {
                    foreach (string TraitValue in StockTraitValuesMarine[Trait])
                    {
                        StockTraitIndicesMarine.Add(TraitValue, stockFunctionalGroupDefinitions.GetFunctionalGroupIndex(Trait, TraitValue, false));
                    }
                }

                // Add unique trait values to each of the lists that will contain output data arranged by trait value
                foreach (string TraitValue in StockTraitIndicesMarine.Keys)
                {
                    TotalBiomassDensitiesOut.Add(TraitValue, 0.0);
                }
            }
            else
            {
                // Define the stock traits that will be used to separate outputs
                StockTraits = new string[2] { "Heterotroph/Autotroph", "Leaf strategy" };

                // Re-initialise the sorted dictionary to hold all unique trait values
                StockTraitValues = new SortedDictionary<string, string[]>();
                
                // Add all unique marine stock trait values to the sorted dictionary
                foreach (string Trait in StockTraits)
                {
                    StockTraitValues.Add(Trait, stockFunctionalGroupDefinitions.GetUniqueTraitValues(Trait));
                }

                // Get the list of functional group indices corresponding to each unique  trait value
                foreach (string Trait in StockTraits)
                {
                    foreach (string TraitValue in StockTraitValues[Trait])
                    {
                        StockTraitIndices.Add(TraitValue, stockFunctionalGroupDefinitions.GetFunctionalGroupIndex(Trait, TraitValue, false));
                    }
                }

                // Add unique trait values to each of the lists that will contain output data arranged by trait value
                foreach (string TraitValue in StockTraitIndices.Keys)
                {
                    TotalBiomassDensitiesOut.Add(TraitValue, 0.0);
                }
            }  
        }


        /// <summary>
        /// Sets up the outputs associated with all levels of output detail
        /// </summary>
        /// <param name="numTimeSteps">The number of time steps in the model run</param>
        /// <param name="ecosystemModelGrid">The model grid</param>
        private void SetUpLowLevelOutputs(uint numTimeSteps, ModelGrid ecosystemModelGrid)
        {
            // Create an SDS object to hold total abundance and biomass data
            // BasicOutput = SDSCreator.CreateSDS("netCDF", "BasicOutputs" + _OutputSuffix, _OutputPath);
            BasicOutputMemory = SDSCreator.CreateSDSInMemory(true);
            string[] TimeDimension = { "Time step" };
            DataConverter.AddVariable(BasicOutputMemory, "Total Biomass density", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
            DataConverter.AddVariable(BasicOutputMemory, "Heterotroph Abundance density", "Individuals / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
            DataConverter.AddVariable(BasicOutputMemory, "Heterotroph Biomass density", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
        }

        /// <summary>
        /// Sets up the outputs associated with medium and high levels of output detail
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void SetupMediumLevelOutputs(ModelGrid ecosystemModelGrid, Boolean marineCell)
        {

            string[] TimeDimension = { "Time step" };

            if (OutputMetrics)
            {
                DataConverter.AddVariable(BasicOutputMemory, "Mean Trophic Level", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Trophic Evenness", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Biomass Evenness", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Functional Richness", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Rao Functional Evenness", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Biomass Richness", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Trophic Richness", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);

                DataConverter.AddVariable(BasicOutputMemory, "Max Bodymass", "g", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Min Bodymass", "g", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Max Trophic Index", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Min Trophic Index", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Geometric Mean Bodymass", "g", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Arithmetic Mean Bodymass", "g", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
            }

            if (marineCell)
            {
                foreach (string TraitValue in CohortTraitIndicesMarine.Keys)
                {

                    DataConverter.AddVariable(BasicOutputMemory, TraitValue + " density", "Individuals / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                    DataConverter.AddVariable(BasicOutputMemory, TraitValue + " biomass density", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                }
                
                foreach (string TraitValue in StockTraitIndicesMarine.Keys)
                {
                    DataConverter.AddVariable(BasicOutputMemory, TraitValue + " biomass density", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                }

                if (TrackMarineSpecifics)
                {
                    // Add a variable to keep track of the NPP incoming from the VGPM model
                    DataConverter.AddVariable(BasicOutputMemory, "Incoming NPP", "gC / m^2 / day", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                }
            }
            else
            {
                foreach (string TraitValue in CohortTraitIndices.Keys)
                {

                    DataConverter.AddVariable(BasicOutputMemory, TraitValue + " density", "Individuals / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                    DataConverter.AddVariable(BasicOutputMemory, TraitValue + " biomass density", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                }
                
                foreach (string TraitValue in StockTraitIndices.Keys)
                {
                    DataConverter.AddVariable(BasicOutputMemory, TraitValue + " biomass density", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                }
            }
        }

        /// <summary>
        /// Sets up the outputs associated with the high level of output detail
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cellIndices">The indices of active cells in the model grid</param>
        /// <param name="cellNumber">The index of the current cell in the list of active cells</param>
        /// <param name="cohortFunctionalGroupDefinitions">The functional group definitions for cohorts in the model</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void SetUpHighLevelOutputs(ModelGrid ecosystemModelGrid, List<uint[]> cellIndices, int cellNumber,
            FunctionalGroupDefinitions cohortFunctionalGroupDefinitions, Boolean marineCell)
        {
            // Create an SDS object for outputs by mass bin
            // MassBinsOutput = SDSCreator.CreateSDS("netCDF", "MassBins" + _OutputSuffix, _OutputPath);
            MassBinsOutputMemory = SDSCreator.CreateSDSInMemory(true);

            // Add relevant output variables to the mass bin output file
            string[] MassBinDimensions = { "Time step", "Mass bin" };
            string[] DoubleMassBinDimensions = new string[] { "Adult Mass bin", "Juvenile Mass bin", "Time step" };


            if (OutputMetrics)
            {
                DataConverter.AddVariable(MassBinsOutputMemory, "Trophic Index Distribution", 2, new string[] {"Time step","Trophic Index Bins"}, ecosystemModelGrid.GlobalMissingValue, TimeSteps, Metrics.TrophicIndexBinValues);
            }

            if (marineCell)
            {
                foreach (string TraitValue in CohortTraitIndicesMarine.Keys)
                {
                    DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValue + " abundance in mass bins", 2, MassBinDimensions, ecosystemModelGrid.GlobalMissingValue, TimeSteps, MassBins);
                    DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValue + " biomass in mass bins", 2, MassBinDimensions, ecosystemModelGrid.GlobalMissingValue, TimeSteps, MassBins);
                    DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValue + " abundance in juvenile vs adult bins", 3, DoubleMassBinDimensions, ecosystemModelGrid.GlobalMissingValue, MassBins, MassBins, TimeSteps);
                    DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValue + " biomass in juvenile vs adult bins", 3, DoubleMassBinDimensions, ecosystemModelGrid.GlobalMissingValue, MassBins, MassBins, TimeSteps);
                }
            }
            else
            {
                foreach (string TraitValue in CohortTraitIndices.Keys)
                {
                    DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValue + " abundance in mass bins", 2, MassBinDimensions, ecosystemModelGrid.GlobalMissingValue, TimeSteps, MassBins);
                    DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValue + " biomass in mass bins", 2, MassBinDimensions, ecosystemModelGrid.GlobalMissingValue, TimeSteps, MassBins);
                    DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValue + " abundance in juvenile vs adult bins", 3, DoubleMassBinDimensions, ecosystemModelGrid.GlobalMissingValue, MassBins, MassBins, TimeSteps);
                    DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValue + " biomass in juvenile vs adult bins", 3, DoubleMassBinDimensions, ecosystemModelGrid.GlobalMissingValue, MassBins, MassBins, TimeSteps);
                }
            }

            // Create an SDS object in memory for tracked cohorts outputs
            // TrackedCohortsOutput = SDSCreator.CreateSDS("netCDF", "TrackedCohorts" + _OutputSuffix, _OutputPath);
            TrackedCohortsOutputMemory = SDSCreator.CreateSDSInMemory(true);
            
            // Initialise list to hold tracked cohorts
            TrackedCohorts = new List<uint>();

            // Identify cohorts to track
            GridCellCohortHandler TempCohorts = null;
            bool FoundCohorts = false;
            
            // Get a local copy of the cohorts in the grid cell
            TempCohorts = ecosystemModelGrid.GetGridCellCohorts(cellIndices[cellNumber][0], cellIndices[cellNumber][1]);

            // Loop over functional groups and check whether any cohorts exist in this grid cell
            foreach (var CohortList in TempCohorts)
            {
                if (CohortList.Count > 0)
                {
                    FoundCohorts = true;
                    break;
                }
            }

            
            // If there are some cohorts in the grid cell, then setup the tracked cohorts
            if (FoundCohorts)
            {
                // Initialise stream writer to hold details of tracked cohorts
                StreamWriter sw = new StreamWriter(_OutputPath + "TrackedCohortProperties" + _OutputSuffix + ".txt");
                sw.WriteLine("Output ID\tCohort ID\tFunctional group index\tNutrition source\tDiet\tRealm\tMobility\tJuvenile mass\tAdult mass");

                // Counter for tracked cohorts
                int TrackedCohortCounter = 0;

                for (int i = 0; i < TempCohorts.Count; i++)
                {
                    if (TempCohorts[i].Count > 0)
                    {
                        for (int j = 0; j < TempCohorts[i].Count; j++)
                        {
                            // Write out properties of the selected cohort
                            sw.WriteLine(Convert.ToString(TrackedCohortCounter) + '\t' + Convert.ToString(TempCohorts[i][j].CohortID[0]) + '\t' + i + '\t' +
                                cohortFunctionalGroupDefinitions.GetTraitNames("Nutrition source", i) + '\t' + cohortFunctionalGroupDefinitions.
                                GetTraitNames("Diet", i) + '\t' + cohortFunctionalGroupDefinitions.GetTraitNames("Realm", i) + '\t' +
                                cohortFunctionalGroupDefinitions.GetTraitNames("Mobility", i) + '\t' + TempCohorts[i][j].JuvenileMass + '\t' +
                                TempCohorts[i][j].AdultMass);

                            // Add the ID of the cohort to the list of tracked cohorts
                            TrackedCohorts.Add(TempCohorts[i][j].CohortID[0]);

                            // Increment the counter of tracked cohorts
                            TrackedCohortCounter++;
                        }
                    }
                }

                // Generate an array of floating points to index the tracked cohorts in the output file
                float[] OutTrackedCohortIDs = new float[TrackedCohortCounter];
                for (int i = 0; i < TrackedCohortCounter; i++)
                {
                    OutTrackedCohortIDs[i] = i;
                }

                // Set up outputs for tracked cohorts
                string[] TrackedCohortsDimensions = { "Time step", "Cohort ID" };


                // Add output variables for the tracked cohorts output
                DataConverter.AddVariable(TrackedCohortsOutputMemory, "Individual body mass", 2, TrackedCohortsDimensions,
                ecosystemModelGrid.GlobalMissingValue, TimeSteps, OutTrackedCohortIDs);
                DataConverter.AddVariable(TrackedCohortsOutputMemory, "Number of individuals", 2, TrackedCohortsDimensions,
                ecosystemModelGrid.GlobalMissingValue, TimeSteps, OutTrackedCohortIDs);

                // Dispose of the streamwriter
                sw.Dispose();
            }
            
                // Get a list of all possible combinations of trait values as a jagged array
            string[][] TraitValueSearch;
            
                if (marineCell)
                    TraitValueSearch = CalculateAllCombinations(CohortTraitValuesMarine[CohortTraits[0]], CohortTraitValuesMarine[CohortTraits[1]]);
                else
                    TraitValueSearch = CalculateAllCombinations(CohortTraitValues[CohortTraits[0]], CohortTraitValues[CohortTraits[1]]);

                
                // Add the functional group indices of these trait combinations to the list of indices of the trait values to consider, 
                // keyed with a concatenated version of the trait values
                string TraitValueJoin = "";
                string[] TimeDimension = { "Time step" };
                for (int i = 0; i < TraitValueSearch.Count(); i++)
                {
                    TraitValueJoin = "";
                    foreach (string TraitValue in TraitValueSearch[i])
                    {
                        TraitValueJoin += TraitValue + " ";
                    }

                    if (marineCell)
                    {
                        // Only add indices of marine functional groups
                        int[] TempIndices = cohortFunctionalGroupDefinitions.GetFunctionalGroupIndex(CohortTraits, TraitValueSearch[i], true);
                        Boolean[] TempIndices2 = new Boolean[TempIndices.GetLength(0)];
                        for (int ii = 0; ii < TempIndices.GetLength(0); ii++)
                        {
                            if (cohortFunctionalGroupDefinitions.GetTraitNames("Realm", TempIndices[ii]).Equals("Marine", StringComparison.OrdinalIgnoreCase))
                            {
                                TempIndices2[ii] = true;
                            }
                        }

                        // Extract only the indices which are marine 
                        int[] TempIndices3 = Enumerable.Range(0, TempIndices2.Length).Where(zz => TempIndices2[zz]).ToArray();

                        if (TempIndices3.Length > 0)
                        {
                            // Extract the values at these indices
                            for (int ii = 0; ii < TempIndices3.Length; ii++)
                            {
                                TempIndices3[ii] = TempIndices[TempIndices3[ii]];
                            }

                            // Add in the indices for this functional group and this realm
                            CohortTraitIndices.Add(TraitValueJoin, TempIndices3);

                            DataConverter.AddVariable(BasicOutputMemory, TraitValueJoin + " density", "Individuals / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                            DataConverter.AddVariable(BasicOutputMemory, TraitValueJoin + " biomass density", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                            DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValueJoin + " abundance in mass bins", 2, MassBinDimensions, ecosystemModelGrid.GlobalMissingValue, TimeSteps, MassBins);
                            DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValueJoin + " biomass in mass bins", 2, MassBinDimensions, ecosystemModelGrid.GlobalMissingValue, TimeSteps, MassBins);
                            DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValueJoin + " abundance in juvenile vs adult bins", 3, DoubleMassBinDimensions, ecosystemModelGrid.GlobalMissingValue, MassBins, MassBins, TimeSteps);
                            DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValueJoin + " biomass in juvenile vs adult bins", 3, DoubleMassBinDimensions, ecosystemModelGrid.GlobalMissingValue, MassBins, MassBins, TimeSteps);

                            TotalBiomassDensitiesOut.Add(TraitValueJoin, 0.0);
                            TotalDensitiesOut.Add(TraitValueJoin, 0.0);
                        }       
                    }
                    else
                    {
                        // Only add indices of terrestrial functional groups
                        int[] TempIndices = cohortFunctionalGroupDefinitions.GetFunctionalGroupIndex(CohortTraits, TraitValueSearch[i], true);
                        Boolean[] TempIndices2 = new Boolean[TempIndices.GetLength(0)];
                        for (int ii = 0; ii < TempIndices.GetLength(0); ii++)
                        {
                            if (cohortFunctionalGroupDefinitions.GetTraitNames("Realm", TempIndices[ii]).Equals("Terrestrial", StringComparison.OrdinalIgnoreCase))
                            {
                                TempIndices2[ii] = true;
                            }
                        }

                        // Extract only the indices which are terrestrial 
                        int[] TempIndices3 = Enumerable.Range(0, TempIndices2.Length).Where(zz => TempIndices2[zz]).ToArray();

                        if (TempIndices3.Length > 0)
                        {
                            // Extract the values at these indices
                            for (int ii = 0; ii < TempIndices3.Length; ii++)
                            {
                                TempIndices3[ii] = TempIndices[TempIndices3[ii]];
                            }

                            // Add in the indices for this functional group and this realm
                            CohortTraitIndices.Add(TraitValueJoin, TempIndices3);

                            DataConverter.AddVariable(BasicOutputMemory, TraitValueJoin + " density", "Individuals / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                            DataConverter.AddVariable(BasicOutputMemory, TraitValueJoin + " biomass density", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                            DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValueJoin + " abundance in mass bins", 2, MassBinDimensions, ecosystemModelGrid.GlobalMissingValue, TimeSteps, MassBins);
                            DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValueJoin + " biomass in mass bins", 2, MassBinDimensions, ecosystemModelGrid.GlobalMissingValue, TimeSteps, MassBins);
                            DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValueJoin + " abundance in juvenile vs adult bins", 3, DoubleMassBinDimensions, ecosystemModelGrid.GlobalMissingValue, MassBins, MassBins, TimeSteps);
                            DataConverter.AddVariable(MassBinsOutputMemory, "Log " + TraitValueJoin + " biomass in juvenile vs adult bins", 3, DoubleMassBinDimensions, ecosystemModelGrid.GlobalMissingValue, MassBins, MassBins, TimeSteps);

                            TotalBiomassDensitiesOut.Add(TraitValueJoin, 0.0);
                            TotalDensitiesOut.Add(TraitValueJoin, 0.0);
                        }       
                    }
                }
       
        }

        /// <summary>
        /// Calculates the variables to output
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid to get output data from</param>
        /// <param name="cohortFunctionalGroupDefinitions">Definitions of the cohort functional groups in the model</param>
        /// <param name="stockFunctionalGroupDefinitions">Definitions of the stock functional groups in the model</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="cellNumber">The number of the current cell in the list of indices of active cells</param>
        /// <param name="globalDiagnosticVariables">The sorted list of global diagnostic variables in the model</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="month">The current month in the model run</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void CalculateOutputs(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortFunctionalGroupDefinitions,
            FunctionalGroupDefinitions stockFunctionalGroupDefinitions, List<uint[]> cellIndices, int cellNumber, SortedList<string, double>
            globalDiagnosticVariables, MadingleyModelInitialisation initialisation, uint month, Boolean marineCell)
        {            
            // Calculate low-level outputs
            CalculateLowLevelOutputs(ecosystemModelGrid, cellIndices, cellNumber, globalDiagnosticVariables, cohortFunctionalGroupDefinitions,
                stockFunctionalGroupDefinitions, initialisation, month, marineCell);
            
            if (ModelOutputDetail == OutputDetailLevel.High)
            {
                // Calculate high-level outputs
                CalculateHighLevelOutputs(ecosystemModelGrid, cellIndices, cellNumber, marineCell);
            }


        }

        /// <summary>
        /// Calculate outputs associated with low-level outputs
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cellIndices">The list of indices of active cells in the model grid</param>
        /// <param name="cellIndex">The position of the current cell in the list of active cells</param>
        /// <param name="globalDiagnosticVariables">The global diagnostic variables for this model run</param>
        /// <param name="cohortFunctionalGroupDefinitions">The functional group definitions of cohorts in the model</param>
        /// <param name="stockFunctionalGroupDefinitions">The functional group definitions of stocks in the model</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="month">The current month in the model run</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        private void CalculateLowLevelOutputs(ModelGrid ecosystemModelGrid, List<uint[]> cellIndices, int cellIndex, 
            SortedList<string,double> globalDiagnosticVariables, FunctionalGroupDefinitions cohortFunctionalGroupDefinitions,
            FunctionalGroupDefinitions stockFunctionalGroupDefinitions, MadingleyModelInitialisation initialisation, uint month, 
            Boolean MarineCell)
        {
            // Reset the total living biomass
            TotalLivingBiomass = 0.0;

            string[] Keys;

            if (MarineCell)
            {
                // Get the list of cohort trait combinations to consider
                Keys = CohortTraitIndicesMarine.Keys.ToArray();

                // Get biomass, abundance and densities for each of the trait combinations. Note that the GetStateVariableDensity function deals with the assessment of whether cohorts contain individuals
                // of low enough mass to be considered zooplankton in the marine realm
                foreach (string TraitValue in Keys)
                {
                    // Biomass density
                    TotalBiomassDensitiesOut[TraitValue] = ecosystemModelGrid.GetStateVariableDensity("Biomass", TraitValue, CohortTraitIndicesMarine[TraitValue], cellIndices[cellIndex][0], cellIndices[cellIndex][1], "cohort", initialisation) / 1000.0;

                    // Density
                    TotalDensitiesOut[TraitValue] = ecosystemModelGrid.GetStateVariableDensity("Abundance", TraitValue, CohortTraitIndicesMarine[TraitValue], cellIndices[cellIndex][0], cellIndices[cellIndex][1], "cohort", initialisation);
                }
            }
            else
            {
                // Get the list of cohort trait combinations to consider
                Keys = CohortTraitIndices.Keys.ToArray();

                // Get biomass, abundance and densities for each of the trait combinations
                foreach (string TraitValue in Keys)
                {
                    // Biomass density
                    TotalBiomassDensitiesOut[TraitValue] = ecosystemModelGrid.GetStateVariableDensity("Biomass", TraitValue, CohortTraitIndices[TraitValue], cellIndices[cellIndex][0], cellIndices[cellIndex][1], "cohort", initialisation) / 1000.0;

                    // Density
                    TotalDensitiesOut[TraitValue] = ecosystemModelGrid.GetStateVariableDensity("Abundance", TraitValue, CohortTraitIndices[TraitValue], cellIndices[cellIndex][0], cellIndices[cellIndex][1], "cohort", initialisation);
                }
            }



            // Add the total biomass of all cohorts to the total living biomass variable
            TotalLivingBiomass += ecosystemModelGrid.GetStateVariable("Biomass", "NA", cohortFunctionalGroupDefinitions.AllFunctionalGroupsIndex,
                cellIndices[cellIndex][0], cellIndices[cellIndex][1], "cohort", initialisation);
            TotalLivingBiomassDensity = ecosystemModelGrid.GetStateVariableDensity("Biomass", "NA", cohortFunctionalGroupDefinitions.AllFunctionalGroupsIndex, cellIndices[cellIndex][0], cellIndices[cellIndex][1], "cohort", initialisation) / 1000.0;
            TotalHeterotrophAbundanceDensity = ecosystemModelGrid.GetStateVariableDensity("Abundance", "NA", cohortFunctionalGroupDefinitions.AllFunctionalGroupsIndex, cellIndices[cellIndex][0], cellIndices[cellIndex][1], "cohort", initialisation);
            TotalHeterotrophBiomassDensity = TotalLivingBiomassDensity;


            if (MarineCell)
            {
                // Get the list of stock trait combinations to consider
                Keys = StockTraitIndicesMarine.Keys.ToArray();

                // Get biomass and biomass density for each of the trait combinations
                foreach (string TraitValue in Keys)
                {
                    // Density
                    TotalBiomassDensitiesOut[TraitValue] = ecosystemModelGrid.GetStateVariableDensity("Biomass", TraitValue, StockTraitIndicesMarine[TraitValue], cellIndices[cellIndex][0], cellIndices[cellIndex][1], "stock", initialisation) / 1000.0;
                }
            }
            else
            {
                // Get the list of stock trait combinations to consider
                Keys = StockTraitIndices.Keys.ToArray();

                // Get biomass and biomass density for each of the trait combinations
                foreach (string TraitValue in Keys)
                {
                    // Density
                    TotalBiomassDensitiesOut[TraitValue] = ecosystemModelGrid.GetStateVariableDensity("Biomass", TraitValue, StockTraitIndices[TraitValue], cellIndices[cellIndex][0], cellIndices[cellIndex][1], "stock", initialisation) / 1000.0;
                }
            }
            
            // Add the total biomass of all stocks to the total living biomass variable
            TotalLivingBiomass += ecosystemModelGrid.GetStateVariable("Biomass", "NA", stockFunctionalGroupDefinitions.AllFunctionalGroupsIndex,
                cellIndices[cellIndex][0], cellIndices[cellIndex][1], "stock", initialisation);
            TotalLivingBiomassDensity += ecosystemModelGrid.GetStateVariableDensity("Biomass", "NA", stockFunctionalGroupDefinitions.AllFunctionalGroupsIndex, cellIndices[cellIndex][0], cellIndices[cellIndex][1], "stock", initialisation) / 1000.0;


            if (TrackMarineSpecifics && MarineCell)
            {
                bool varExists;
                TotalIncomingNPP = ecosystemModelGrid.GetEnviroLayer("NPP", month, cellIndices[cellIndex][0], cellIndices[cellIndex][1], out varExists);
            }
        }

        /// <summary>
        /// Calculate outputs associated with high-level outputs
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="cellIndex">The number of the current cell in the list of active cells</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void CalculateHighLevelOutputs(ModelGrid ecosystemModelGrid, List<uint[]> cellIndices, int cellIndex, Boolean marineCell)
        {

            // Calcalate the outputs arranged by mass bin
            CalculateMassBinOutputs(ecosystemModelGrid, cellIndices, cellIndex, marineCell);

            // Declare vectors to hold properties of tracked cohorts
            TrackedCohortIndividualMasses = new double[TrackedCohorts.Count];
            TrackedCohortAbundances = new double[TrackedCohorts.Count];

            // Get a temporary local copy of the cohorts in the grid cell
            GridCellCohortHandler TempCohorts = ecosystemModelGrid.GetGridCellCohorts(cellIndices[cellIndex][0], cellIndices[cellIndex][1]);

            // Loop over functional groups in the grid cell cohorts
            for (int j = 0; j < TempCohorts.Count; j++)
            {
                // Loop over cohorts within this functional group
                for (int k = 0; k < TempCohorts[j].Count; k++)
                {
                    // Loop over all cohort IDs 
                    foreach (uint CohortID in TempCohorts[j][k].CohortID)
                    {
                        // Check whether the cohort ID corresponds to a tracked cohort
                        if (TrackedCohorts.Contains(CohortID))
                        {
                            // Get the position of the cohort in the list of tracked cohorts
                            int position = TrackedCohorts.FindIndex(
                                delegate(uint i)
                                {
                                    return i == CohortID;
                                });
                            // Add the body mass and abundance of the tracked cohort to the appropriate position in the output vector
                            TrackedCohortIndividualMasses[position] = TempCohorts[j][k].IndividualBodyMass;
                            TrackedCohortAbundances[position] = TempCohorts[j][k].CohortAbundance;
                        }
                    }
                }
            }


        }

        /// <summary>
        /// Write to the output file values of the output variables before the first time step
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid to get data from</param>C:\madingley-ecosystem-model\Madingley\Output and tracking\PredationTracker.cs
        /// <param name="cohortFunctionalGroupDefinitions">The definitions of cohort functional groups in the model</param>
        /// <param name="stockFunctionalGroupDefinitions">The definitions of stock functional groups in the model</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="cellNumber">The number of the current cell in the list of indices of active cells</param>
        /// <param name="globalDiagnosticVariables">List of global diagnostic variables</param>
        /// <param name="numTimeSteps">The number of time steps in the model run</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="month">The current month in the model run</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        public void InitialOutputs(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortFunctionalGroupDefinitions,
            FunctionalGroupDefinitions stockFunctionalGroupDefinitions, List<uint[]> cellIndices, int cellNumber,
            SortedList<string, double> globalDiagnosticVariables, uint numTimeSteps, MadingleyModelInitialisation initialisation, 
            uint month, Boolean marineCell)
        {

            // Calculate values of the output variables to be used
            CalculateOutputs(ecosystemModelGrid, cohortFunctionalGroupDefinitions, stockFunctionalGroupDefinitions, cellIndices, cellNumber, globalDiagnosticVariables, initialisation, month, marineCell);

            // Generate the intial live outputs
            if (LiveOutputs)
            {
                InitialLiveOutputs(ecosystemModelGrid, marineCell);
            }
            // Generate the intial file outputs
            InitialFileOutputs(ecosystemModelGrid, cohortFunctionalGroupDefinitions, marineCell,cellIndices,cellNumber);

        }

        /// <summary>
        /// Generates the intial output to the live dataset view
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void InitialLiveOutputs(ModelGrid ecosystemModelGrid, Boolean marineCell)
        {
            // Create a string holding the name of the x-axis variable
            string[] TimeDimension = { "Time step" };

            // Add the x-axis to the plots (time step)
            DataSetToViewLive.AddAxis("Time step", "Month", TimeSteps);

            // Add the relevant output variables depending on the specified level of detail
            if (ModelOutputDetail == OutputDetailLevel.Low)
            {
                // Add the variable for total living biomass
                DataConverter.AddVariable(DataSetToViewLive, "Total living biomass", "kg", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);

                // Add the initial value of total living biomass
                DataConverter.ValueToSDS1D(TotalLivingBiomass, "Total living biomass", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
            }
            else
            {
               if (marineCell)
                {
                    foreach (string TraitValue in CohortTraitIndicesMarine.Keys)
                    {
                        // Add in the carnivore and herbivore abundance variables
                        DataConverter.AddVariable(DataSetToViewLive, TraitValue + " density", "Individuals / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                        // Add in the initial values of carnivore and herbivore abundance
                        DataConverter.ValueToSDS1D(TotalDensitiesOut[TraitValue], TraitValue + " density", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
                        // Add in the carnivore and herbivore biomass variables
                        DataConverter.AddVariable(DataSetToViewLive, TraitValue + " biomass", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                        // Add in the initial values of carnivore and herbivore abundance
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
                    }

                    foreach (string TraitValue in StockTraitIndicesMarine.Keys)
                    {
                        // Add in the stock biomass variables
                        DataConverter.AddVariable(DataSetToViewLive, TraitValue + " biomass", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                        // Add in the initial value
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
                    }
                }
                else
                {
                    foreach (string TraitValue in CohortTraitIndices.Keys)
                    {
                        // Add in the carnivore and herbivore abundance variables
                        DataConverter.AddVariable(DataSetToViewLive, TraitValue + " density", "Individuals / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                        // Add in the initial values of carnivore and herbivore abundance
                        DataConverter.ValueToSDS1D(TotalDensitiesOut[TraitValue], TraitValue + " density", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
                        // Add in the carnivore and herbivore biomass variables
                        DataConverter.AddVariable(DataSetToViewLive, TraitValue + " biomass", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                        // Add in the initial values of carnivore and herbivore abundance
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
                    }

                    foreach (string TraitValue in StockTraitIndices.Keys)
                    {
                        // Add in the stock biomass variables
                        DataConverter.AddVariable(DataSetToViewLive, TraitValue + " biomass", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                        // Add in the initial value
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
                    }
                }

                

            }
        }

        

        /// <summary>
        /// Generates the initial file outputs
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cohortFunctionalGroupDefinitions">The functional group definitions for cohorts in the model</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        /// <param name="cellIndices">The list of all cells to run the model for</param>
        /// <param name="cellIndex">The index of the current cell in the list of all cells to run the model for</param>
        private void InitialFileOutputs(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortFunctionalGroupDefinitions, 
            Boolean MarineCell, List<uint[]> cellIndices, int cellIndex)
        {
            Console.WriteLine("Writing initial grid cell outputs to memory...");

            //Write the low level outputs first
            DataConverter.ValueToSDS1D(TotalLivingBiomassDensity, "Total Biomass density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, 0);
            DataConverter.ValueToSDS1D(TotalHeterotrophAbundanceDensity, "Heterotroph Abundance density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, 0);
            DataConverter.ValueToSDS1D(TotalHeterotrophBiomassDensity, "Heterotroph Biomass density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, 0);

            // File outputs for medium and high detail levels
            if ((ModelOutputDetail == OutputDetailLevel.Medium) || (ModelOutputDetail == OutputDetailLevel.High))
            {
                

                if (MarineCell)
                {
                    foreach (string TraitValue in CohortTraitIndicesMarine.Keys)
                    {
                        // Write densities, biomasses and abundances in different functional groups to the relevant one-dimensional output variables
                        DataConverter.ValueToSDS1D(TotalDensitiesOut[TraitValue], TraitValue + " density", "Time step", 
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, 0);
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass density", "Time step", 
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, 0);
                    }

                    foreach (string TraitValue in StockTraitIndicesMarine.Keys)
                    {
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass density", "Time step", 
                            ecosystemModelGrid.GlobalMissingValue,
                        BasicOutputMemory, 0);
                    }
                }
                else
                {
                    foreach (string TraitValue in CohortTraitIndices.Keys)
                    {
                        // Write densities, biomasses and abundances in different functional groups to the relevant one-dimensional output variables
                        DataConverter.ValueToSDS1D(TotalDensitiesOut[TraitValue], TraitValue + " density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, 0);
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass density", "Time step", 
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, 0);
                    }
                    
                    foreach (string TraitValue in StockTraitIndices.Keys)
                    {
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass density", "Time step",
                           ecosystemModelGrid.GlobalMissingValue,
                        BasicOutputMemory, 0);
                    }
                }

                if (OutputMetrics)
                {
                    DataConverter.ValueToSDS1D(Metrics.CalculateMeanTrophicLevelCell(ecosystemModelGrid,cellIndices,cellIndex),
                                                "Mean Trophic Level", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalEvennessRao(ecosystemModelGrid, cohortFunctionalGroupDefinitions,cellIndices, cellIndex,"trophic index"),
                                                "Trophic Evenness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalEvennessRao(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "biomass"),
                                                "Biomass Evenness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);

                    double[] FunctionalDiversity = Metrics.CalculateFunctionalDiversity(ecosystemModelGrid, cohortFunctionalGroupDefinitions, 
                        cellIndices, cellIndex);

                    DataConverter.ValueToSDS1D(FunctionalDiversity[0],
                                                 "Functional Richness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                 BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(FunctionalDiversity[1],
                                                "Rao Functional Evenness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);

                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Biomass")[0],
                                                "Biomass Richness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Biomass")[1],
                                                "Min Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Biomass")[2],
                                                "Max Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);

                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Trophic index")[0],
                                                "Trophic Richness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Trophic index")[1],
                                                "Min Trophic Index", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Trophic index")[2],
                                                "Max Trophic Index", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);

                    DataConverter.ValueToSDS1D(Metrics.CalculateArithmeticCommunityMeanBodyMass(ecosystemModelGrid, cellIndices, cellIndex),
                                                "Arithmetic Mean Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(Metrics.CalculateGeometricCommunityMeanBodyMass(ecosystemModelGrid, cellIndices, cellIndex),
                                                "Geometric Mean Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);

                }

                if (MarineCell && TrackMarineSpecifics)
                {
                    DataConverter.ValueToSDS1D(TotalIncomingNPP, "Incoming NPP", "Time step", ecosystemModelGrid.GlobalMissingValue,
                        BasicOutputMemory, 0);
                }

                // File outputs for high detail level
                if (ModelOutputDetail == OutputDetailLevel.High)
                {
                    if (OutputMetrics)
                    {
                        DataConverter.VectorToSDS2D(Metrics.CalculateTrophicDistribution(ecosystemModelGrid,cellIndices,cellIndex), "Trophic Index Distribution",
                        new string[2] { "Time step", "Trophic Index Bins" }, TimeSteps, Metrics.TrophicIndexBinValues, ecosystemModelGrid.GlobalMissingValue, MassBinsOutputMemory, 0);
                    }

                    foreach (var TraitValue in AbundancesInMassBins.Keys)
                    {
                        DataConverter.VectorToSDS2D(AbundancesInMassBins[TraitValue], "Log " + TraitValue + " abundance in mass bins",
                        new string[2] { "Time step", "Mass bin" }, TimeSteps, MassBins, ecosystemModelGrid.GlobalMissingValue, MassBinsOutputMemory, 0);
                        DataConverter.VectorToSDS2D(BiomassesInMassBins[TraitValue], "Log " + TraitValue + " biomass in mass bins",
                        new string[2] { "Time step", "Mass bin" }, TimeSteps, MassBins, ecosystemModelGrid.GlobalMissingValue, MassBinsOutputMemory, 0);
                        // Write out abundances in combinations of juvenile and adult mass bins to the relevant three-dimensional output variables
                        DataConverter.Array2DToSDS3D(AbundancesInJuvenileAdultMassBins[TraitValue],
                            "Log " + TraitValue + " abundance in juvenile vs adult bins",
                            new string[] { "Adult Mass bin", "Juvenile Mass bin", "Time step" },
                            0,
                            ecosystemModelGrid.GlobalMissingValue,
                            MassBinsOutputMemory);
                        DataConverter.Array2DToSDS3D(BiomassesInJuvenileAdultMassBins[TraitValue],
                            "Log " + TraitValue + " biomass in juvenile vs adult bins",
                            new string[] { "Adult Mass bin", "Juvenile Mass bin", "Time step" },
                            0,
                            ecosystemModelGrid.GlobalMissingValue,
                            MassBinsOutputMemory);
                    }


                    // Write outputs for tracking individual cohorts

                    // Loop over tracked cohorts
                    for (int i = 0; i < TrackedCohorts.Count; i++)
                    {
                        // Add the individual body mass of the tracked cohort to the output dataset
                        string[] TrackedCohortDimensions = new string[] { "Time step", "Cohort ID" };
                        DataConverter.ValueToSDS2D(TrackedCohortIndividualMasses[i], "Individual body mass", TrackedCohortDimensions,
                            ecosystemModelGrid.GlobalMissingValue, TrackedCohortsOutputMemory, 0, i);
                        DataConverter.ValueToSDS2D(TrackedCohortAbundances[i], "Number of individuals", TrackedCohortDimensions,
                            ecosystemModelGrid.GlobalMissingValue, TrackedCohortsOutputMemory, 0, i);
                    }



                }

            }

        }

        /// <summary>
        /// Write to the output file values of the output variables during the model time steps
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid to get data from</param>
        /// <param name="cohortFunctionalGroupDefinitions">The definitions of the cohort functional groups in the model</param>
        /// <param name="stockFunctionalGroupDefinitions">The definitions of the stock  functional groups in the model</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="cellNumber">The number of the current cell in the list of indices of active cells</param>
        /// <param name="globalDiagnosticVariables">List of global diagnostic variables</param>
        /// <param name="timeStepTimer">The timer for the current time step</param>
        /// <param name="numTimeSteps">The number of time steps in the model run</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="month">The current month in the model run</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        public void TimeStepOutputs(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortFunctionalGroupDefinitions, 
            FunctionalGroupDefinitions stockFunctionalGroupDefinitions, List<uint[]> cellIndices, int cellNumber,
            SortedList<string, double> globalDiagnosticVariables, StopWatch timeStepTimer, uint numTimeSteps, uint currentTimestep, 
            MadingleyModelInitialisation initialisation, uint month, Boolean marineCell)
        {

            // Calculate values of the output variables to be used
            CalculateOutputs(ecosystemModelGrid, cohortFunctionalGroupDefinitions, stockFunctionalGroupDefinitions, cellIndices, cellNumber, globalDiagnosticVariables, initialisation, month, marineCell);

            // Generate the live outputs for this time step
            if (LiveOutputs)
            {
                TimeStepLiveOutputs(numTimeSteps, currentTimestep, ecosystemModelGrid, marineCell);
            }

            // Generate the console outputs for the current time step
            TimeStepConsoleOutputs(currentTimestep, timeStepTimer);

            // Generate the file outputs for the current time step
            TimeStepFileOutputs(ecosystemModelGrid, cohortFunctionalGroupDefinitions, currentTimestep, marineCell, cellIndices,cellNumber);

        }

        /// <summary>
        /// Generate the live outputs for the current time step
        /// </summary>
        /// <param name="numTimeSteps">The number of time steps in the model run</param>
        /// <param name="currentTimeStep">The current time step</param>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void TimeStepLiveOutputs(uint numTimeSteps, uint currentTimeStep, ModelGrid ecosystemModelGrid, Boolean marineCell)
        {
            
            // Output to the live graph view according to the specified level of detail
            if (ModelOutputDetail == OutputDetailLevel.Low)
            {
                // Rescale the y-axis if necessary
                if (TotalLivingBiomass > MaximumYValue)
                {
                    MaximumYValue = TotalLivingBiomass * 1.1;
                    DataSetToViewLive.Metadata["VisualHints"] = "\"Total living biomass\"[Time step]; Style:Polyline; Visible: 0,1," +
                    numTimeSteps.ToString() + "," + MaximumYValue.ToString() +
                    "; LogScale:Y; Stroke:#D95F02; Thickness:3; Title:\"Total Biomass" + "\"";
                }
                // Write out total living biomass
                DataConverter.ValueToSDS1D(TotalLivingBiomass, "Total living biomass", "Time step",
                    ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, (int)currentTimeStep + 1);

            }
            else
            {
                //Find the max value in the TotalBiomassDensities
                double MaxVal = 0.0;
                foreach (var KVPair in TotalBiomassDensitiesOut)
                {
                    if (KVPair.Value > MaxVal) MaxVal = KVPair.Value;
                }
                // Rescale the y-axis if necessary
                if (MaxVal > MaximumYValue)
                {
                    MaximumYValue = MaxVal * 1.1;
                    DataSetToViewLive.Metadata["VisualHints"] = "\"autotroph biomass\"[Time step]; Style:Polyline; Visible: 0,1,"
                        + numTimeSteps.ToString() + ","
                        + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FF008040;Thickness:3;;\"carnivore biomass\"[Time step] ; Style:Polyline; Visible: 0,1,"
                        + numTimeSteps.ToString() + ","
                        + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FFFF0000;Thickness:3;;\"herbivore biomass\"[Time step] ; Style:Polyline; Visible: 0,1,"
                        + numTimeSteps.ToString() + ","
                        + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FF00FF00;Thickness:3;;\"omnivore biomass\"[Time step] ; Style:Polyline; Visible: 0,1,"
                        + numTimeSteps.ToString() + ","
                        + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FF0000FF;Thickness:3; Title:\"Biomass Densities";
                }

                if (marineCell)
                {
                    foreach (string TraitValue in CohortTraitIndicesMarine.Keys)
                    {
                        // Output the total carnivore, herbivore and omnivore abundances
                        DataConverter.ValueToSDS1D(TotalDensitiesOut[TraitValue], TraitValue + " density", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, (int)currentTimeStep + 1);
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, (int)currentTimeStep + 1);
                    }
                    
                    foreach (string TraitValue in StockTraitIndicesMarine.Keys)
                    {
                        // Add in the initial values of stock biomass density
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, (int)currentTimeStep + 1);
                    }
                }
                else
                {
                    foreach (string TraitValue in CohortTraitIndices.Keys)
                    {
                        // Output the total carnivore, herbivore and omnivore abundances
                        DataConverter.ValueToSDS1D(TotalDensitiesOut[TraitValue], TraitValue + " density", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, (int)currentTimeStep + 1);
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, (int)currentTimeStep + 1);
                    }

                    foreach (string TraitValue in StockTraitIndices.Keys)
                    {
                        // Add in the initial values of stock biomass density
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, (int)currentTimeStep + 1);
                    }
                }
            }


        }

        /// <summary>
        /// Generates the console outputs for the current time step
        /// </summary>
        /// <param name="currentTimeStep">The current time step</param>
        /// <param name="timeStepTimer">The timer for the current time step</param>
        private void TimeStepConsoleOutputs(uint currentTimeStep, StopWatch timeStepTimer)
        {
           
        }

        /// <summary>
        /// Generate file outputs for the current time step
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cohortFunctionalGroupDefinitions">The functional group definitions for cohorts in the model</param>
        /// <param name="currentTimeStep">The current time step</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        /// <param name="cellIndices">The list of all cells to run the model for</param>
        /// <param name="cellIndex">The index of the current cell in the list of all cells to run the model for</param>
        private void TimeStepFileOutputs(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortFunctionalGroupDefinitions, 
            uint currentTimeStep, Boolean MarineCell, List<uint[]> cellIndices, int cellIndex)
        {
            Console.WriteLine("Writing grid cell ouputs to file...\n");
            //Write the low level outputs first
            DataConverter.ValueToSDS1D(TotalLivingBiomassDensity, "Total Biomass density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, (int)currentTimeStep + 1);
            DataConverter.ValueToSDS1D(TotalHeterotrophAbundanceDensity, "Heterotroph Abundance density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, (int)currentTimeStep + 1);
            DataConverter.ValueToSDS1D(TotalHeterotrophBiomassDensity, "Heterotroph Biomass density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, (int)currentTimeStep + 1);
            // File outputs for medium and high detail levels
            if ((ModelOutputDetail == OutputDetailLevel.Medium) || (ModelOutputDetail == OutputDetailLevel.High))
            {
                if (MarineCell)
                {
                    // Loop over all cohort trait value combinations and output abundances, densities and biomasses
                    foreach (string TraitValue in CohortTraitIndicesMarine.Keys)
                    {
                        DataConverter.ValueToSDS1D(TotalDensitiesOut[TraitValue], TraitValue + " density", "Time step", ecosystemModelGrid.GlobalMissingValue, BasicOutputMemory, (int)currentTimeStep + 1);
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass density", "Time step", ecosystemModelGrid.GlobalMissingValue, BasicOutputMemory, (int)currentTimeStep + 1);
                    }

                    // Loop over all stock trait value combinations and output biomasses
                    foreach (string TraitValue in StockTraitIndicesMarine.Keys)
                    {
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue, BasicOutputMemory, (int)currentTimeStep + 1);
                    }
                }
                else
                {
                    // Loop over all cohort trait value combinations and output abudnances, densities and biomasses
                    foreach (string TraitValue in CohortTraitIndices.Keys)
                    {
                        DataConverter.ValueToSDS1D(TotalDensitiesOut[TraitValue], TraitValue + " density", "Time step", ecosystemModelGrid.GlobalMissingValue, BasicOutputMemory, (int)currentTimeStep + 1);
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass density", "Time step", ecosystemModelGrid.GlobalMissingValue, BasicOutputMemory, (int)currentTimeStep + 1);
                    }
                    
                    // Loop over all stock trait value combinations and output biomasses
                    foreach (string TraitValue in StockTraitIndices.Keys)
                    {
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[TraitValue], TraitValue + " biomass density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue, BasicOutputMemory, (int)currentTimeStep + 1);
                    }
                }

                // If ouputting ecosystem metrics has been specified then add these metrics to the output
                if (OutputMetrics)
                {
                    DataConverter.ValueToSDS1D(Metrics.CalculateMeanTrophicLevelCell(ecosystemModelGrid, cellIndices, cellIndex),
                                                "Mean Trophic Level", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalEvennessRao(ecosystemModelGrid,cohortFunctionalGroupDefinitions, cellIndices, cellIndex,"trophic index"),
                                                "Trophic Evenness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalEvennessRao(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "biomass"),
                                                "Biomass Evenness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    double[] FunctionalDiversity = Metrics.CalculateFunctionalDiversity(ecosystemModelGrid, cohortFunctionalGroupDefinitions, 
                        cellIndices, cellIndex);
                    DataConverter.ValueToSDS1D(FunctionalDiversity[0],
                                                 "Functional Richness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                 BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(FunctionalDiversity[1],
                                                "Rao Functional Evenness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);

                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Biomass")[0],
                                                "Biomass Richness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Biomass")[1],
                                                "Min Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Biomass")[2],
                                                "Max Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);

                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Trophic index")[0],
                                                "Trophic Richness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Trophic index")[1],
                                                "Min Trophic Index", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Trophic index")[2],
                                                "Max Trophic Index", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);

                    DataConverter.ValueToSDS1D(Metrics.CalculateArithmeticCommunityMeanBodyMass(ecosystemModelGrid, cellIndices, cellIndex),
                                                "Arithmetic Mean Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(Metrics.CalculateGeometricCommunityMeanBodyMass(ecosystemModelGrid, cellIndices, cellIndex),
                                                "Geometric Mean Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                }

                if (TrackMarineSpecifics && MarineCell)
                {
                    DataConverter.ValueToSDS1D(TotalIncomingNPP, "Incoming NPP", "Time step", ecosystemModelGrid.GlobalMissingValue,
                        BasicOutputMemory, (int)currentTimeStep + 1);
                }


                if (currentTimeStep % 600 == 0 && currentTimeStep > 0)
                {
                    BasicOutputMemory.Clone("msds:nc?file=" + _OutputPath + "BasicOutputs" + _OutputSuffix + ".nc&openMode=create");
                    Console.WriteLine("Cloning grid cell ouputs to file...\n");
                }
                


                // File outputs for high detail level
                if (ModelOutputDetail == OutputDetailLevel.High)
                {

                    if (OutputMetrics)
                    {
                        DataConverter.VectorToSDS2D(Metrics.CalculateTrophicDistribution(ecosystemModelGrid,cellIndices,cellIndex), "Trophic Index Distribution",
                            new string[2] { "Time step", "Trophic Index Bins" }, TimeSteps, Metrics.TrophicIndexBinValues, ecosystemModelGrid.GlobalMissingValue, MassBinsOutputMemory, (int)currentTimeStep + 1);
                    }

                    // Loop over trait combinations in the mass bin outputs
                    foreach (var TraitValue in AbundancesInMassBins.Keys)
                    {
                        // Write out abundances in each of the mass bins to the relevant two-dimensional output variables
                        DataConverter.VectorToSDS2D(AbundancesInMassBins[TraitValue], "Log " + TraitValue + " abundance in mass bins",
                            new string[2] { "Time step", "Mass bin" }, TimeSteps, MassBins, ecosystemModelGrid.GlobalMissingValue, MassBinsOutputMemory, (int)currentTimeStep + 1);
                        DataConverter.VectorToSDS2D(BiomassesInMassBins[TraitValue], "Log " + TraitValue + " biomass in mass bins",
                            new string[2] { "Time step", "Mass bin" }, TimeSteps, MassBins, ecosystemModelGrid.GlobalMissingValue, MassBinsOutputMemory, (int)currentTimeStep + 1);
                        // Write out abundances in combinations of juvenile and adult mass bins to the relevant three-dimensional output variable
                        DataConverter.Array2DToSDS3D(AbundancesInJuvenileAdultMassBins[TraitValue], "Log " + TraitValue + " abundance in juvenile vs adult bins",
                            new string[] { "Adult Mass bin", "Juvenile Mass bin", "Time step" },
                            (int)currentTimeStep + 1,
                            ecosystemModelGrid.GlobalMissingValue,
                            MassBinsOutputMemory);
                        // Write out biomasses in combinations of juvenile and adult mass bins to the relevant three-dimensional output variable
                        DataConverter.Array2DToSDS3D(BiomassesInJuvenileAdultMassBins[TraitValue], "Log " + TraitValue + " biomass in juvenile vs adult bins",
                            new string[] { "Adult Mass bin", "Juvenile Mass bin", "Time step" },
                            (int)currentTimeStep + 1,
                            ecosystemModelGrid.GlobalMissingValue,
                            MassBinsOutputMemory);
                    }

                    // Loop over tracked cohorts
                    for (int i = 0; i < TrackedCohorts.Count; i++)
                    {
                        // Add the individual body mass and abundance of the tracked cohort to the output dataset
                        string[] TrackedCohortDimensions = new string[] { "Time step", "Cohort ID" };
                        DataConverter.ValueToSDS2D(TrackedCohortIndividualMasses[i], "Individual body mass", TrackedCohortDimensions,
                            ecosystemModelGrid.GlobalMissingValue, TrackedCohortsOutputMemory, (int)currentTimeStep + 1, i);
                        DataConverter.ValueToSDS2D(TrackedCohortAbundances[i], "Number of individuals", TrackedCohortDimensions,
                            ecosystemModelGrid.GlobalMissingValue, TrackedCohortsOutputMemory, (int)currentTimeStep + 1, i);
                    }

                    if (currentTimeStep % 600 ==0 && currentTimeStep > 0)
                    {
                        MassBinsOutputMemory.Clone("msds:nc?file=" + _OutputPath + "MassBinsOutputs" + _OutputSuffix + ".nc&openMode=create");

                        TrackedCohortsOutputMemory.Clone("msds:nc?file=" + _OutputPath + "TrackedCohortsOutputs" + _OutputSuffix + ".nc&openMode=create");

                    }

                }

            }




        }

        /// <summary>
        /// Write to the output file values of the output variables at the end of the model run
        /// </summary>
        /// <param name="EcosystemModelGrid">The model grid to get data from</param>
        /// <param name="CohortFunctionalGroupDefinitions">Definitions of the cohort functional groups in the model</param>
        /// <param name="StockFunctionalGroupDefinitions">Definitions of the stock functional groups in the model</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="cellNumber">The number of the current cell in the list of indices of active cells</param>
        /// <param name="GlobalDiagnosticVariables">List of global diagnostic variables</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="month">The current month in the model run</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        public void FinalOutputs(ModelGrid EcosystemModelGrid, FunctionalGroupDefinitions CohortFunctionalGroupDefinitions, 
            FunctionalGroupDefinitions StockFunctionalGroupDefinitions, List<uint[]> cellIndices, int cellNumber, 
            SortedList<string, double> GlobalDiagnosticVariables, MadingleyModelInitialisation initialisation, uint month, Boolean marineCell)
        {
            // Calculate output variables
            CalculateOutputs(EcosystemModelGrid, CohortFunctionalGroupDefinitions, StockFunctionalGroupDefinitions, cellIndices,cellNumber, GlobalDiagnosticVariables, initialisation, month, marineCell);

            // Dispose of the dataset objects  
            BasicOutputMemory.Clone("msds:nc?file="+ _OutputPath + "BasicOutputs" + _OutputSuffix + ".nc&openMode=create");
            BasicOutputMemory.Dispose();

            if (LiveOutputs)
            {
                DataSetToViewLive.Dispose();
            }

            if (ModelOutputDetail == OutputDetailLevel.High)
            {
                // Dispose of the dataset objects for high detail level outputs
                MassBinsOutputMemory.Clone("msds:nc?file="+ _OutputPath + "MassBinsOutputs" + _OutputSuffix + ".nc&openMode=create");
                MassBinsOutputMemory.Dispose();

                TrackedCohortsOutputMemory.Clone("msds:nc?file="+ _OutputPath + "TrackedCohortsOutputs" + _OutputSuffix + ".nc&openMode=create");
                TrackedCohortsOutputMemory.Dispose();

            }

            Metrics.CloseRserve();

        }

        /// <summary>
        /// Calculates the abundances and biomasses within mass bins for all functional groups in the cohort indices array
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="cellIndex">The number of the current cell in the list of indices of active cells</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void CalculateMassBinOutputs(ModelGrid ecosystemModelGrid, List<uint[]> cellIndices, int cellIndex, Boolean marineCell)
        {
            string[] Keys;
            if (marineCell)
            {
                // Get the cohort trait combinations to consider
                Keys = CohortTraitIndicesMarine.Keys.ToArray();
            }
            else
            {
                // Get the cohort trait combinations to consider
                Keys = CohortTraitIndices.Keys.ToArray();
            }
            // Loop over trait combinations
            foreach (var TraitValue in Keys)
            {
                // Declare vectors to hold abundance and biomass in mass bins for this trait combination
                double[] WorkingAbundanceInMassBins = new double[MassBinNumber];
                double[] WorkingBiomassInMassBins = new double[MassBinNumber];

                // Declare arrays to hold abundance and biomass in juvenile vs. adult mass bins for this trait combination
                double[,] WorkingAbundanceJuvenileAdultMassBins = new double[MassBinNumber, MassBinNumber];
                double[,] WorkingBiomassJuvenileAdultMassBins = new double[MassBinNumber, MassBinNumber];

                // Create a temporary local copy of the cohorts in this grid cell
                GridCellCohortHandler TempCohorts = ecosystemModelGrid.GetGridCellCohorts(cellIndices[cellIndex][0],cellIndices[cellIndex][1]);

                if (marineCell)
                {
                    // Loop over functional  groups
                    foreach (int FunctionalGroupIndex in CohortTraitIndicesMarine[TraitValue])
                    {
                        // Loop over all cohorts in this functional  group
                        for (int cohort = 0; cohort < TempCohorts[FunctionalGroupIndex].Count; cohort++)
                        {
                            // Find the appropriate mass bin for the cohort
                            int mb = 0;
                            do
                            {
                                mb++;
                            } while (mb < (MassBins.Length - 1) && TempCohorts[FunctionalGroupIndex][cohort].IndividualBodyMass > MassBins[mb]);

                            // Add the cohort's abundance to the approriate mass bin. Note that we have to differentiate here for non-obligate zooplankton, because they are only added in if they are 
                            // below the zooplankton mass threshold (or an obligate zooplankton)
                            if (String.Equals(TraitValue, "Zooplankton (all)"))
                            {
                                if (TempCohorts[FunctionalGroupIndex][cohort].IndividualBodyMass < PlanktonSizeThreshold || CohortTraitIndicesMarine["Obligate zooplankton"].Contains(FunctionalGroupIndex))
                                {
                                    WorkingAbundanceInMassBins[mb - 1] += TempCohorts[FunctionalGroupIndex][cohort].CohortAbundance;

                                    // Add the cohort's biomass to the approriate mass bin
                                    WorkingBiomassInMassBins[mb - 1] += TempCohorts[FunctionalGroupIndex][cohort].CohortAbundance * TempCohorts[FunctionalGroupIndex][cohort].IndividualBodyMass;

                                    // Find the mass bin appropriate for this cohort's juvenile mass
                                    int j = 0;
                                    do
                                    {
                                        j++;
                                    } while (j < (MassBins.Length - 1) && TempCohorts[FunctionalGroupIndex][cohort].JuvenileMass > MassBins[j]);
                                    // Find the mass bin appropriate for this cohort's adult mass
                                    int a = 0;
                                    do
                                    {
                                        a++;
                                    } while (a < (MassBins.Length - 1) && TempCohorts[FunctionalGroupIndex][cohort].AdultMass > MassBins[a]);
                                        
                                   // Add the cohort's abundance to this adult vs juvenile mass bins
                                    WorkingAbundanceJuvenileAdultMassBins[a - 1, j - 1] += TempCohorts[FunctionalGroupIndex][cohort].CohortAbundance;
                                    // Add the cohort's biomass to this adult vs juvenile mass bins
                                    WorkingBiomassJuvenileAdultMassBins[a - 1, j - 1] += TempCohorts[FunctionalGroupIndex][cohort].CohortAbundance * TempCohorts[FunctionalGroupIndex][cohort].IndividualBodyMass;       
                                }
                            }
                            else
                            {
                                WorkingAbundanceInMassBins[mb - 1] += TempCohorts[FunctionalGroupIndex][cohort].CohortAbundance;

                                // Add the cohort's biomass to the approriate mass bin
                                WorkingBiomassInMassBins[mb - 1] += TempCohorts[FunctionalGroupIndex][cohort].CohortAbundance * TempCohorts[FunctionalGroupIndex][cohort].IndividualBodyMass;


                                // Find the mass bin appropriate for this cohort's juvenile mass
                                int j = 0;
                                do
                                {
                                    j++;
                                } while (j < (MassBins.Length - 1) && TempCohorts[FunctionalGroupIndex][cohort].JuvenileMass > MassBins[j]);
                                // Find the mass bin appropriate for this cohort's adult mass
                                int a = 0;
                                do
                                {
                                    a++;
                                } while (a < (MassBins.Length - 1) && TempCohorts[FunctionalGroupIndex][cohort].AdultMass > MassBins[a]);

                                // Add the cohort's abundance to this adult vs juvenile mass bins
                                WorkingAbundanceJuvenileAdultMassBins[a - 1, j - 1] += TempCohorts[FunctionalGroupIndex][cohort].CohortAbundance;
                                // Add the cohort's biomass to this adult vs juvenile mass bins
                                WorkingBiomassJuvenileAdultMassBins[a - 1, j - 1] += TempCohorts[FunctionalGroupIndex][cohort].CohortAbundance * TempCohorts[FunctionalGroupIndex][cohort].IndividualBodyMass;
                            }
                        }
                    }

                    //Copy the working vectors and arrays to the sortedlist for the current key value
                    AbundancesInMassBins[TraitValue] = WorkingAbundanceInMassBins;
                    BiomassesInMassBins[TraitValue] = WorkingBiomassInMassBins;
                    AbundancesInJuvenileAdultMassBins[TraitValue] = WorkingAbundanceJuvenileAdultMassBins;
                    BiomassesInJuvenileAdultMassBins[TraitValue] = WorkingBiomassJuvenileAdultMassBins;
                }
                else
                {
                    // Loop over functional  groups
                    foreach (int FunctionalGroupIndex in CohortTraitIndices[TraitValue])
                    {
                        // Loop over all cohorts in this functional  group
                        for (int cohort = 0; cohort < TempCohorts[FunctionalGroupIndex].Count; cohort++)
                        {
                            // Find the appropriate mass bin for the cohort
                            int mb = 0;
                            do
                            {
                                mb++;
                            } while (mb < (MassBins.Length - 1) && TempCohorts[FunctionalGroupIndex][cohort].IndividualBodyMass > MassBins[mb]);

                            // Add the cohort's abundance to the approriate mass bin
                            WorkingAbundanceInMassBins[mb - 1] += TempCohorts[FunctionalGroupIndex][cohort].CohortAbundance;
                            // Add the cohort's biomass to the approriate mass bin
                            WorkingBiomassInMassBins[mb - 1] += TempCohorts[FunctionalGroupIndex][cohort].CohortAbundance * TempCohorts[FunctionalGroupIndex][cohort].IndividualBodyMass;

                            // Find the mass bin appropriate for this cohort's juvenile mass
                            int j = 0;
                            do
                            {
                                j++;
                            } while (j < (MassBins.Length - 1) && TempCohorts[FunctionalGroupIndex][cohort].JuvenileMass > MassBins[j]);
                            // Find the mass bin appropriate for this cohort's adult mass
                            int a = 0;
                            do
                            {
                                a++;
                            } while (a < (MassBins.Length - 1) && TempCohorts[FunctionalGroupIndex][cohort].AdultMass > MassBins[a]);

                            // Add the cohort's abundance to this adult vs juvenile mass bins
                            WorkingAbundanceJuvenileAdultMassBins[a - 1, j - 1] += TempCohorts[FunctionalGroupIndex][cohort].CohortAbundance;
                            // Add the cohort's biomass to this adult vs juvenile mass bins
                            WorkingBiomassJuvenileAdultMassBins[a - 1, j - 1] += TempCohorts[FunctionalGroupIndex][cohort].CohortAbundance * TempCohorts[FunctionalGroupIndex][cohort].IndividualBodyMass;

                        }
                    }

                    //Copy the working vectors and arrays to the sortedlist for the current key value
                    AbundancesInMassBins[TraitValue] = WorkingAbundanceInMassBins;
                    BiomassesInMassBins[TraitValue] = WorkingBiomassInMassBins;
                    AbundancesInJuvenileAdultMassBins[TraitValue] = WorkingAbundanceJuvenileAdultMassBins;
                    BiomassesInJuvenileAdultMassBins[TraitValue] = WorkingBiomassJuvenileAdultMassBins;
                }
            }

            // Loop over trait combinations and log abundances and body masses
            foreach (var TraitValue in Keys)
            {
                for (int i = 0; i < MassBins.Length; i++)
                {
                    AbundancesInMassBins[TraitValue][i] = (AbundancesInMassBins[TraitValue][i] > 0) ? Math.Log(AbundancesInMassBins[TraitValue][i]) : ecosystemModelGrid.GlobalMissingValue;
                    BiomassesInMassBins[TraitValue][i] = (BiomassesInMassBins[TraitValue][i] > 0) ? Math.Log(BiomassesInMassBins[TraitValue][i]) : ecosystemModelGrid.GlobalMissingValue;
                    for (int j = 0; j < MassBins.Length; j++)
                    {
                        AbundancesInJuvenileAdultMassBins[TraitValue][i, j] = (AbundancesInJuvenileAdultMassBins[TraitValue][i, j] > 0) ? Math.Log(AbundancesInJuvenileAdultMassBins[TraitValue][i, j]) : ecosystemModelGrid.GlobalMissingValue;
                        BiomassesInJuvenileAdultMassBins[TraitValue][i, j] = (BiomassesInJuvenileAdultMassBins[TraitValue][i, j] > 0) ? Math.Log(BiomassesInJuvenileAdultMassBins[TraitValue][i, j]) : ecosystemModelGrid.GlobalMissingValue;
                    }
                }

            }
        }

        /// <summary>
        /// Returns an array of strings for all unique combinations of the strings where strings from R1 vector appear in the first column.
        /// </summary>
        /// <param name="R1"></param>
        /// <param name="R2"></param>
        /// <returns>Vector of vector of string combinations</returns>
        private string[][] CalculateAllCombinations(string[] R1, string[] R2)
        {
            string[][] AllCombinations = new string[R1.Length * R2.Length][];
            int ii = 0;
            foreach (string a in R1)
            {
                foreach (string b in R2)
                {
                    AllCombinations[ii] = new string[2];
                    AllCombinations[ii][0] = a;
                    AllCombinations[ii][1] = b;
                    ii++;
                }
            }

            return AllCombinations;
        }

        /// <summary>
        /// Overloaded method that returns an array of strings for all unique combinations of the strings where strings from R1 vector appear in the first column.
        /// </summary>
        /// <param name="R1"></param>
        /// <param name="R2"></param>
        /// <param name="R3"></param>
        /// <returns>Vector of vector of string combinations</returns>
        private string[][] CalculateAllCombinations(string[] R1, string[] R2, string[] R3)
        {
            string[][] AllCombinations = new string[R1.Length * R2.Length * R3.Length][];
            int ii = 0;
            foreach (string a in R1)
            {
                foreach (string b in R2)
                {
                    foreach (string c in R3)
                    {

                        AllCombinations[ii] = new string[3];
                        AllCombinations[ii][0] = a;
                        AllCombinations[ii][1] = b;
                        AllCombinations[ii][2] = c;
                        ii++;
                    }
                }
            }

            return AllCombinations;
        }

    }
}
