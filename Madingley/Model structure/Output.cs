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
    class Output
    {
        // An instance of the output class to deal with textual outputs
        private TextualOutput TextOutput;

        // Instances of SDS datasets for the screen and file outputs respectively
        private DataSet DataSetToViewLive;
        private DataSet DataSetForFileOutput;

        // Arrays to hold the indices of different functional groups
        private int[] HerbivoreIndices;
        private int[] CarnivoreIndices;
        private int[] OmnivoreIndices;
        private int[] AutotrophIndices;

        // Variables to track total densities across the model grid
        private double HerbivoreDensityOut;
        private double CarnivoreDensityOut;
        private double OmnivoreDensityOut;
        private double TotalDensityOut;

        // Variables to track total abundances across the model grid
        private double HerbivoreAbundanceOut;
        private double CarnivoreAbundanceOut;
        private double OmnivoreAbundanceOut;
        private double TotalAbundanceOut;

        // Variables to track total biomass densities across the model grid
        private double HerbivoreBiomassDensityOut;
        private double CarnivoreBiomassDensityOut;
        private double OmnivoreBiomassDensityOut;
        private double AutotrophBiomassDensityOut;

        // Variables to track total biomasses across the model grid
        private double HerbivoreBiomassOut;
        private double CarnivoreBiomassOut;
        private double OmnivoreBiomassOut;
        private double AutotrophBiomassOut;

        // Pools
        private double OrganicPoolOut;
        private double RespiratoryPoolOut;

        // Retain total abundances
        private double TotalLivingBiomassOut;
        private double TotalBiomassOut;
        
        // Two-dimensional arrays to hold biomasses in individual grid cells
        private double[,] LogBiomassDensityGridCohorts;
        private double[,] LogBiomassDensityGridStocks;
        private double[,] LogBiomassDensityGrid;

        // Two-dimensional arrays to hold densities in individual grid cells
        private double[,] LogAbundanceDensityGridCohorts;
        
        // Vectors to hold abundances in individual body mass bins
        private double[] CarnivoreAbundanceInMassBins;
        private double[] HerbivoreAbundanceInMassBins;
        //Vectors to hold biomasses in individual body mass bins
        private double[] CarnivoreBiomassInMassBins;
        private double[] HerbivoreBiomassInMassBins;

        //Arrays to hold abundances against Juvenile and adult biomasses
        private double[,] CarnivoreAbundanceVsJuvenileAdultMass;
        private double[,] HerbivoreAbundanceVsJuvenileAdultMass;
        //Arrays to hold biomasses against Juvenile and adult biomasses
        private double[,] CarnivoreBiomassVsJuvenileAdultMass;
        private double[,] HerbivoreBiomassVsJuvenileAdultMass;

        // Variables to hold numbers of stocks and cohorts
        private double TotalNumberOfCohorts;
        private double TotalNumberOfStocks;
        private double NumberOfCohortsExtinct;
        private double NumberOfCohortsProduced;
        private double NumberOfCohortsCombined;

        // Number of mass bins to use in the final output
        private int MassBinNumber;

        // Maximum Y value for the live output when the visualisation is 2D
        private double MaximumYValue;

        // An instance of the grid cell cohort handler to store a temporary copy of the cohorts in the grid cell for output purposes
        private GridCellCohortHandler TempCohorts;

        // Vectors of time steps and mass bins to use as dimensions in the output file
        private float[] OutTimes;
        private float[] OutMassBins;

        // Boolean indicating if the live output should include the full model grid (the alternative is a graph of total biomasses and abundances)
        private bool GridView;

        /// <summary>
        /// Constructor for the output class
        /// </summary>
        /// <param name="textDetail">The level of detail required in the textual output: 'Low', 'Medium' or 'High'</param>
        public Output(string textDetail, bool gridView)
        {
            // Create an instance of the class for text output
            TextOutput = new TextualOutput(textDetail);

            // Set the maximum value for the y-axes in the case of a 2D plot
            MaximumYValue = 1000000;

            // Set the number of individual body mass bins to use in the output
            MassBinNumber = 100;

            // Set the boolean indicating whether live output will be grid view
            if (gridView)
            {
                GridView = true;
            }
            else
            {
                GridView = false;
            }

        }

        /// <summary>
        /// Spawn dataset viewer for the live outputs
        /// </summary>
        /// <param name="NumTimeSteps">The number of time steps in the model run</param>
        /// <param name="gridView">Whether to launch dataset viewer in full model grid view (otherwise in graph view)</param>
        public void SpawnDatasetViewer(uint NumTimeSteps)
        {
            Console.WriteLine("Spawning Dataset Viewer");

            // Intialise the SDS object for the live view
            DataSetToViewLive = CreateSDSObject.CreateSDSInMemory(true);
            
            // Set up the grid viewer asynchronously
            // If the grid view option is selected, then live output will be the full model grid, otherwise the output will be a graph with total
            // carnivore and herbivore abundance
            if (GridView)
            {
                DataSetToViewLive.Metadata["VisualHints"] = 
                    "\"Biomass density\"(Longitude,Latitude) Style:Colormap; Palette:#000040=0,Blue=0.1,Green=0.2661,#FA8000=0.76395,Red; Transparency:0.38";
            }
            else
            {
                DataSetToViewLive.Metadata["VisualHints"] = "\"Carnivore density\"[Time step]; Style:Polyline; Visible: 0,1," + 
                    NumTimeSteps.ToString() + "," + MaximumYValue.ToString() +
                    "; LogScale:Y;  Stroke:#D95F02; Thickness:3;; \"Herbivore density\"[Time step]; Style:Polyline; Visible: 0,1," +
                    NumTimeSteps.ToString() + "," + MaximumYValue.ToString() +
                    "; LogScale:Y;  Stroke:#1B9E77; Thickness:3;; \"Omnivore density\"[Time step]; Style:Polyline; Visible: 0,1," +
                    NumTimeSteps.ToString() + "," + MaximumYValue.ToString() +
                    "; LogScale:Y;  Stroke:#7570B3;Thickness:3; Title:\"Heterotroph Densities"
                    + "\"";
            }

            
            // Start viewing
            ViewGrid.AsynchronousView(ref DataSetToViewLive, "");
        
        }

        /// <summary>
        /// Set up the file, screen and live outputs prior to the model run
        /// </summary>
        /// <param name="EcosystemModelGrid">The model grid that output data will be derived from</param>
        /// <param name="CohortFunctionalGroupDefinitions">The definitions for cohort functional groups</param>
        /// <param name="StockFunctionalGroupDefinitions">The definitions for stock functional groups</param>
        /// <param name="NumTimeSteps">The number of time steps in the model run</param>
        public void SetUpOutputs(ModelGrid EcosystemModelGrid, FunctionalGroupDefinitions CohortFunctionalGroupDefinitions, 
            FunctionalGroupDefinitions StockFunctionalGroupDefinitions, uint NumTimeSteps, string FileOutputs)
        {
            // Get the functional group indices of herbivore, carnivore and omnivore cohorts, and autotroph stocks
            string[] Trait = { "Nutrition source" };
            string[] Trait2 = { "Heterotroph/Autotroph" };
            string[] TraitValue1 = { "Herbivory" };
            string[] TraitValue2 = { "Carnivory" };
            string[] TraitValue3 = { "Omnivory" };
            string[] TraitValue4 = { "Autotroph" };

            HerbivoreIndices = CohortFunctionalGroupDefinitions.GetFunctionalGroupIndex(Trait, TraitValue1, false);
            CarnivoreIndices = CohortFunctionalGroupDefinitions.GetFunctionalGroupIndex(Trait, TraitValue2, false);
            OmnivoreIndices = CohortFunctionalGroupDefinitions.GetFunctionalGroupIndex(Trait, TraitValue3, false);
            AutotrophIndices = StockFunctionalGroupDefinitions.GetFunctionalGroupIndex(Trait2, TraitValue4, false);

            // Set up vectors to hold dimension data for the output variables
            float[] outLats = new float[EcosystemModelGrid.NumLatCells];
            float[] outLons = new float[EcosystemModelGrid.NumLonCells];
            float[] IdentityMassBins;

            // Populate the dimension variable vectors with cell centre latitude and longitudes
            for (int ii = 0; ii < EcosystemModelGrid.NumLatCells; ii++)
            {
                outLats[ii] = EcosystemModelGrid.Lats[ii] + (EcosystemModelGrid.LatCellSize / 2);
            }

            for (int jj = 0; jj < EcosystemModelGrid.NumLonCells; jj++)
            {
                outLons[jj] = EcosystemModelGrid.Lons[jj] + (EcosystemModelGrid.LonCellSize / 2);
            }

            // Create vector to hold the values of the time dimension
            OutTimes = new float[NumTimeSteps + 1];
            // Set the first value to be -1 (this will hold initial outputs)
            OutTimes[0] = -1;
            // Fill other values from 0 (this will hold outputs during the model run)
            for (int ii = 1; ii < NumTimeSteps + 1; ii++)
            {
                OutTimes[ii] = ii + 1;
            }

            // Set up a vector to hold (log) individual body mass bins
            OutMassBins = new float[MassBinNumber];
            IdentityMassBins = new float[MassBinNumber];

            // Get the (log) minimum and maximum possible (log) masses across all functional groups combined, start with default values of
            // Infinity and -Infinity
            float MaximumMass = -1 / 0F;
            float MinimumMass = 1 / 0F;
            foreach (int FunctionalGroupIndex in CohortFunctionalGroupDefinitions.AllFunctionalGroupsIndex)
            {
                MinimumMass = (float)Math.Min(MinimumMass, Math.Log(CohortFunctionalGroupDefinitions.GetBiologicalPropertyOneFunctionalGroup("minimum mass", FunctionalGroupIndex)));
                MaximumMass = (float)Math.Max(MaximumMass, Math.Log(CohortFunctionalGroupDefinitions.GetBiologicalPropertyOneFunctionalGroup("maximum mass", FunctionalGroupIndex)));
            }

            // Get the interval required to span the range between the minimum and maximum values in 100 steps
            float MassInterval = (MaximumMass - MinimumMass) / MassBinNumber;

            // Fill the vector of output mass bins with (log) body masses spread evenly between the minimum and maximum values
            for (int ii = 0; ii < MassBinNumber; ii++)
            {
                OutMassBins[ii] = MinimumMass + ii * MassInterval;
                IdentityMassBins[ii] = Convert.ToSingle(Math.Exp(Convert.ToDouble(OutMassBins[ii])));
            }

            // Create file for model outputs
            DataSetForFileOutput = CreateSDSObject.CreateSDS("netCDF", FileOutputs);

            // Add three-dimensional variables to output file, dimensioned by latitude, longtiude and time
            string[] dimensions3D = { "Latitude", "Longitude", "Time step" };
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Biomass density", 3, dimensions3D, 0, outLats, outLons, OutTimes);


            dimensions3D = new string[] { "Adult Mass bin", "Juvenile Mass bin", "Time step" };
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Log Carnivore abundance in juvenile vs adult bins", 3, dimensions3D,Math.Log(0), OutMassBins, OutMassBins, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Log Herbivore abundance in juvenile vs adult bins", 3, dimensions3D, Math.Log(0), OutMassBins, OutMassBins, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Log Carnivore biomass in juvenile vs adult bins", 3, dimensions3D, Math.Log(0), OutMassBins, OutMassBins, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Log Herbivore biomass in juvenile vs adult bins", 3, dimensions3D, Math.Log(0), OutMassBins, OutMassBins, OutTimes);

            // Add two-dimensional variables to output file, dimensioned by mass bins and time
            string[] dimensions2D = { "Time step", "Mass bin" };
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Log Carnivore abundance in mass bins", 2, dimensions2D, Math.Log(0), OutTimes, OutMassBins);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Log Herbivore abundance in mass bins", 2, dimensions2D, Math.Log(0), OutTimes, OutMassBins);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Log Carnivore biomass in mass bins", 2, dimensions2D, Math.Log(0), OutTimes, OutMassBins);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Log Herbivore biomass in mass bins", 2, dimensions2D, Math.Log(0), OutTimes, OutMassBins);


            // Add one-dimensional variables to the output file, dimensioned by time
            string[] dimensions1D = { "Time step" };
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Herbivore density", "Individuals / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Herbivore abundance", "Individuals", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Herbivore biomass", "Kg / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Carnivore density", "Individuals / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Carnivore abundance", "Individuals", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes); 
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Carnivore biomass", "Kg / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Omnivore density", "Individuals / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Omnivore abundance", "Individuals", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes); 
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Omnivore biomass", "Kg / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Autotroph biomass", "Kg / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Organic matter pool", "Kg / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Respiratory CO2 pool", "Kg / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Number of cohorts extinct", "", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Number of cohorts produced", "", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Number of cohorts combined", "", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Number of cohorts in model", "", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
            ArraySDSConvert.AddVariable(DataSetForFileOutput, "Number of stocks in model", "", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);

            // Add one-dimensional variables to the output file, dimensioned by mass bin index
            // To enable outputs to be visualised against mass instead of index



            // Initialise the arrays that will be used for the grid-based outputs
            LogBiomassDensityGridCohorts = new double[EcosystemModelGrid.NumLatCells, EcosystemModelGrid.NumLonCells];
            LogBiomassDensityGridStocks = new double[EcosystemModelGrid.NumLatCells, EcosystemModelGrid.NumLonCells];
            LogBiomassDensityGrid = new double[EcosystemModelGrid.NumLatCells, EcosystemModelGrid.NumLonCells];

        }

        /// <summary>
        /// Calculates the variables to output
        /// </summary>
        /// <param name="EcosystemModelGrid">The model grid to get output data from</param>
        /// <param name="CohortFunctionalGroupDefinitions">Definitions of the cohort functional groups in the model</param>
        /// <param name="StockFunctionalGroupDefinitions">Definitions of the stock functional groups in the model</param>
        /// <param name="_LatCellIndices">A vector of the latitudinal indices of live cells in the model grid</param>
        /// <param name="_LonCellIndices">A vector of the longitudinal indices of live cells in the model grid</param>
        /// <param name="GlobalDiagnosticVariables">The sorted list of global diagnostic variables in the model</param>
        public void CalculateOutputs(ModelGrid EcosystemModelGrid, FunctionalGroupDefinitions CohortFunctionalGroupDefinitions, 
            FunctionalGroupDefinitions StockFunctionalGroupDefinitions, uint[] _LatCellIndices, uint[] _LonCellIndices, SortedList<string, double> 
            GlobalDiagnosticVariables)
        {
            
            # region Get biomasses and abundances from model grid
                        
            // Get grids of the total biomass densities of all stocks and all cohorts in each grid cell
            LogBiomassDensityGridCohorts = EcosystemModelGrid.GetStateVariableGridDensityPerSqKm("Biomass", CohortFunctionalGroupDefinitions.
                AllFunctionalGroupsIndex, _LatCellIndices, _LonCellIndices, "cohort");
            LogBiomassDensityGridStocks = EcosystemModelGrid.GetStateVariableGridDensityPerSqKm("Biomass", StockFunctionalGroupDefinitions.
                AllFunctionalGroupsIndex, _LatCellIndices, _LonCellIndices, "stock");

            // Convert biomass densities from grams to kilograms
            for (int i = 0; i < _LatCellIndices.Length; i++)
            {
                for (int j = 0; j < _LonCellIndices.Length; j++)
                {
                    if ((i != j) && (MadingleyModel.SpecificLocations == true))
                    {
                        break;
                    }
                    LogBiomassDensityGridCohorts[i, j] /= 1000;
                    LogBiomassDensityGridStocks[i, j] /= 1000;
                }
            }

            // Get grids of total abundance densities of all stocks and all cohorts in each grid cell
            LogAbundanceDensityGridCohorts = EcosystemModelGrid.GetStateVariableGridLogDensityPerSqKm("Abundance", CohortFunctionalGroupDefinitions.
                 AllFunctionalGroupsIndex, _LatCellIndices, _LonCellIndices, "cohort");
                        
            // Loop over grid cells and add stock and cohort biomass density to get the total of all biomass densities
            for (int ii = 0; ii < EcosystemModelGrid.NumLatCells; ii++)
            {
                for (int jj = 0; jj < EcosystemModelGrid.NumLonCells; jj++)
                {
                    LogBiomassDensityGrid[ii, jj] = Math.Log(Math.Exp(LogBiomassDensityGridCohorts[ii, jj]) + Math.Exp(LogBiomassDensityGridStocks[ii, jj]));
                }
            }

            // Get total herbivore biomass and abundance
            HerbivoreBiomassOut = EcosystemModelGrid.StateVariableGridTotal("Biomass", HerbivoreIndices, _LatCellIndices, _LonCellIndices,
            "cohort");
            HerbivoreAbundanceOut = EcosystemModelGrid.StateVariableGridTotal("Abundance", HerbivoreIndices, _LatCellIndices, _LonCellIndices,
                "cohort");
            HerbivoreBiomassDensityOut = EcosystemModelGrid.StateVariableGridMeanDensity("Biomass", HerbivoreIndices, _LatCellIndices, _LonCellIndices, 
                "cohort");
            HerbivoreDensityOut = EcosystemModelGrid.StateVariableGridMeanDensity("Abundance", HerbivoreIndices, _LatCellIndices, _LonCellIndices, 
                "cohort");

            // Get total carnivore biomass
            CarnivoreBiomassOut = EcosystemModelGrid.StateVariableGridTotal("Biomass", CarnivoreIndices, _LatCellIndices, _LonCellIndices,
            "cohort");
            CarnivoreAbundanceOut = EcosystemModelGrid.StateVariableGridTotal("Abundance", CarnivoreIndices, _LatCellIndices, _LonCellIndices,
                "cohort");
            CarnivoreBiomassDensityOut = EcosystemModelGrid.StateVariableGridMeanDensity("Biomass", CarnivoreIndices, _LatCellIndices, _LonCellIndices, 
                "cohort");
            CarnivoreDensityOut = EcosystemModelGrid.StateVariableGridMeanDensity("Abundance", CarnivoreIndices, _LatCellIndices, _LonCellIndices, 
                "cohort");

            // Get total omnivore biomass
            OmnivoreBiomassOut = EcosystemModelGrid.StateVariableGridTotal("Biomass", OmnivoreIndices, _LatCellIndices, _LonCellIndices,
                "cohort"); 
            OmnivoreAbundanceOut = EcosystemModelGrid.StateVariableGridTotal("Abundance", OmnivoreIndices, _LatCellIndices, _LonCellIndices,
                "cohort"); 
            OmnivoreBiomassDensityOut = EcosystemModelGrid.StateVariableGridMeanDensity("Biomass", OmnivoreIndices, _LatCellIndices, _LonCellIndices, 
                "cohort");
            OmnivoreDensityOut = EcosystemModelGrid.StateVariableGridMeanDensity("Abundance", OmnivoreIndices, _LatCellIndices, _LonCellIndices, 
                "cohort");

            // Get total autotroph biomass
            AutotrophBiomassOut = EcosystemModelGrid.StateVariableGridTotal("Biomass", AutotrophIndices, _LatCellIndices, _LonCellIndices,
                "stock"); 
            AutotrophBiomassDensityOut = EcosystemModelGrid.StateVariableGridMeanDensity("Biomass", AutotrophIndices, _LatCellIndices, _LonCellIndices, 
                "stock");

            // Convert biomass densities from grams to kilograms
            for (int i = 0; i < _LatCellIndices.Length; i++)
            {
                for (int j = 0; j < _LonCellIndices.Length; j++)
                {
                    if ((i != j) && (MadingleyModel.SpecificLocations == true))
                    {
                        break;
                    }
                    HerbivoreBiomassDensityOut/= 1000;
                    CarnivoreBiomassDensityOut /= 1000;
                    OmnivoreBiomassDensityOut /= 1000;
                    AutotrophBiomassDensityOut /= 1000;
                }
            }

            // Get total living biomass
            TotalLivingBiomassOut = HerbivoreBiomassOut + CarnivoreBiomassOut + OmnivoreBiomassOut + AutotrophBiomassOut;

            // Get total respiratory pool biomass
            RespiratoryPoolOut = EcosystemModelGrid.GetEnviroGridTotal("Respiratory CO2 Pool", 0, _LatCellIndices, _LonCellIndices);

            // Get total organic pool biomass
            OrganicPoolOut = EcosystemModelGrid.GetEnviroGridTotal("Organic Pool", 0, _LatCellIndices, _LonCellIndices);

            // Get total of all biomass
            TotalBiomassOut = TotalLivingBiomassOut + RespiratoryPoolOut + OrganicPoolOut;

            // Get total density
            TotalDensityOut = HerbivoreDensityOut + CarnivoreDensityOut + OmnivoreDensityOut;

            // Get number of cohorts and stocks
            TotalNumberOfCohorts = GlobalDiagnosticVariables["NumberOfCohortsInModel"];
            TotalNumberOfStocks = GlobalDiagnosticVariables["NumberOfStocksInModel"];

            // Get numbers of cohort extinctions and productions
            NumberOfCohortsExtinct = GlobalDiagnosticVariables["NumberOfCohortsExtinct"];
            NumberOfCohortsProduced = GlobalDiagnosticVariables["NumberOfCohortsProduced"];
            NumberOfCohortsCombined = GlobalDiagnosticVariables["NumberOfCohortsCombined"];

            # endregion

            # region Abundances in Mass Bins

            
            // Initialise the vector of abundances in mass bins
            CarnivoreAbundanceInMassBins = new double[MassBinNumber];
            // Initialise the vector of biomasses in mass bins
            CarnivoreBiomassInMassBins = new double[MassBinNumber];

            // Initialise the array of abundances vs juvenile and adult mass bins
            CarnivoreAbundanceVsJuvenileAdultMass = new double[MassBinNumber, MassBinNumber];
            // Initialise the array of biomasses vs Juvenile and adult mass bins
            CarnivoreBiomassVsJuvenileAdultMass = new double[MassBinNumber, MassBinNumber];

            // Get indices of carnivore functional groups
            int[] CarnivoreFunctionalGroupIndices = CohortFunctionalGroupDefinitions.GetFunctionalGroupIndex(new string[1] { "Nutrition source" },
                new string[1] { "Carnivory" }, false);

            // Loop over cells in the model grid
            foreach (uint ii in _LatCellIndices)
            {
                foreach (uint jj in _LonCellIndices)
                {
                    // Create a temporary local copy of the cohorts in this grid cell
                    TempCohorts = EcosystemModelGrid.GetGridCellCohorts(ii, jj);

                    // Loop over functional  groups
                    foreach (int CarnivoreIndex in CarnivoreFunctionalGroupIndices)
                    {
                        // Loop over all cohorts in this funcitonal  group
                        for (int cc = 0; cc < TempCohorts[CarnivoreIndex].Count; cc++)
                        {
                            // Find the appropriate mass bin for the cohort
                            int mb = 0;
                            do
                            {
                                mb++;
                            } while (mb < (OutMassBins.Length - 1) && Math.Log(TempCohorts[CarnivoreIndex][cc].IndividualBodyMass) > OutMassBins[mb]);

                            // Add the cohort's abundance to the approriate mass bin
                            CarnivoreAbundanceInMassBins[mb - 1] += TempCohorts[CarnivoreIndex][cc].CohortAbundance;
                            // Add the cohort's biomass to the approriate mass bin
                            CarnivoreBiomassInMassBins[mb - 1] += TempCohorts[CarnivoreIndex][cc].CohortAbundance * TempCohorts[CarnivoreIndex][cc].IndividualBodyMass;

                            int j = 0;
                            do
                            {
                                j++;
                            } while (j < (OutMassBins.Length - 1) && Math.Log(TempCohorts[CarnivoreIndex][cc].JuvenileMass) > OutMassBins[j]);
                            int a = 0;
                            do
                            {
                                a++;
                            } while (a < (OutMassBins.Length - 1) && Math.Log(TempCohorts[CarnivoreIndex][cc].AdultMass) > OutMassBins[a]);

                            // Add the cohort's abundance to this adult vs juvenile mass bin
                            CarnivoreAbundanceVsJuvenileAdultMass[a - 1, j - 1] += TempCohorts[CarnivoreIndex][cc].CohortAbundance;
                            // Add the cohort's biomass to this adult vs juvenile mass bin
                            CarnivoreBiomassVsJuvenileAdultMass[a - 1, j - 1] += TempCohorts[CarnivoreIndex][cc].CohortAbundance * TempCohorts[CarnivoreIndex][cc].IndividualBodyMass;

                        }
                    }
                }
            }


            // Initialise the vector of abundances in mass bins
            HerbivoreAbundanceInMassBins = new double[MassBinNumber];
            // Initialise the vector of biomasses in mass bins
            HerbivoreBiomassInMassBins = new double[MassBinNumber];

            // Initialise the array of abundances vs juvenile and adult mass bins
            HerbivoreAbundanceVsJuvenileAdultMass = new double[MassBinNumber, MassBinNumber];
            // Initialise the array of biomasses vs Juvenile and adult mass bins
            HerbivoreBiomassVsJuvenileAdultMass = new double[MassBinNumber, MassBinNumber];

            // Get indices of carnivore functional groups
            int[] HerbivoreFunctionalGroupIndices = CohortFunctionalGroupDefinitions.GetFunctionalGroupIndex(new string[1] { "Nutrition source" }, new string[1] { "Herbivory" }, false);

            // Loop over cells in the model grid
            foreach (uint ii in _LatCellIndices)
            {
                foreach (uint jj in _LonCellIndices)
                {
                    // Create a temporary local copy of the cohorts in this grid cell
                    TempCohorts = EcosystemModelGrid.GetGridCellCohorts(ii, jj);

                    // Loop over functional  groups
                    foreach (int HerbivoreIndex in HerbivoreFunctionalGroupIndices)
                    {
                        // Loop over all cohorts in this funcitonal  group
                        for (int cc = 0; cc < TempCohorts[HerbivoreIndex].Count; cc++)
                        {
                            // Find the appropriate mass bin for the cohort
                            int mb = 0;
                            do
                            {
                                mb++;
                            } while (mb < (OutMassBins.Length - 1) && Math.Log(TempCohorts[HerbivoreIndex][cc].IndividualBodyMass) > OutMassBins[mb]);

                            // Add the cohort's abundance to the approriate mass bin
                            HerbivoreAbundanceInMassBins[mb - 1] +=TempCohorts[HerbivoreIndex][cc].CohortAbundance;
                            // Add the cohort's biomass to the approriate mass bin
                            HerbivoreBiomassInMassBins[mb - 1] += TempCohorts[HerbivoreIndex][cc].CohortAbundance * TempCohorts[HerbivoreIndex][cc].IndividualBodyMass;

                            int j = 0;
                            do
                            {
                                j++;
                            } while (j < (OutMassBins.Length - 1) && Math.Log(TempCohorts[HerbivoreIndex][cc].JuvenileMass) > OutMassBins[j]);
                            int a = 0;
                            do
                            {
                                a++;
                            } while (a < (OutMassBins.Length - 1) && Math.Log(TempCohorts[HerbivoreIndex][cc].AdultMass) > OutMassBins[a]);

                            // Add the cohort's abundance to this adult vs juvenile mass bin
                            HerbivoreAbundanceVsJuvenileAdultMass[a - 1, j - 1] += TempCohorts[HerbivoreIndex][cc].CohortAbundance;
                            // Add the cohort's biomass to this adult vs juvenile mass bin
                            HerbivoreBiomassVsJuvenileAdultMass[a - 1, j - 1] += TempCohorts[HerbivoreIndex][cc].CohortAbundance * TempCohorts[HerbivoreIndex][cc].IndividualBodyMass;
                        }
                    }
                }
            }

            for (int i = 0; i < OutMassBins.Length; i++)
            {
                CarnivoreAbundanceInMassBins[i] = Math.Log(CarnivoreAbundanceInMassBins[i]);
                CarnivoreBiomassInMassBins[i] = Math.Log(CarnivoreBiomassInMassBins[i]);
                HerbivoreAbundanceInMassBins[i] = Math.Log(HerbivoreAbundanceInMassBins[i]);
                HerbivoreBiomassInMassBins[i] = Math.Log(HerbivoreBiomassInMassBins[i]);

                for (int j = 0; j < OutMassBins.Length; j++)
                {
                    CarnivoreAbundanceVsJuvenileAdultMass[i, j] = Math.Log(CarnivoreAbundanceVsJuvenileAdultMass[i, j]);
                    CarnivoreBiomassVsJuvenileAdultMass[i, j] = Math.Log(CarnivoreBiomassVsJuvenileAdultMass[i, j]);
                    HerbivoreAbundanceVsJuvenileAdultMass[i, j] = Math.Log(HerbivoreAbundanceVsJuvenileAdultMass[i, j]);
                    HerbivoreBiomassVsJuvenileAdultMass[i, j] = Math.Log(HerbivoreBiomassVsJuvenileAdultMass[i, j]);
                }
            }

            # endregion

        }

        /// <summary>
        /// Write to the output file values of the output variables before the first time step
        /// </summary>
        /// <param name="EcosystemModelGrid">The model grid to get data from</param>
        /// <param name="CohortFunctionalGroupDefinitions">The definitions of cohort functional groups in the model</param>
        /// <param name="StockFunctionalGroupDefinitions">The definitions of stock functional groups in the model</param>
        /// <param name="_LatCellIndices">The latitudinal indices of live cells in the model</param>
        /// <param name="_LonCellIndices">The longitudinal indices of live cells in the model</param>
        /// <param name="GlobalDiagnosticVariables">List of global diagnostic variables</param>
        /// <param name="NumTimeSteps">The number of time steps in the model run</param>
        public void InitialOutputs(ModelGrid EcosystemModelGrid,  FunctionalGroupDefinitions CohortFunctionalGroupDefinitions, 
            FunctionalGroupDefinitions StockFunctionalGroupDefinitions, uint[] _LatCellIndices, uint[] _LonCellIndices, SortedList<string, double> 
            GlobalDiagnosticVariables, uint NumTimeSteps)
        {
            // Calculate values of the output variables to be used
            this.CalculateOutputs(EcosystemModelGrid, CohortFunctionalGroupDefinitions, StockFunctionalGroupDefinitions, _LatCellIndices, 
                _LonCellIndices, GlobalDiagnosticVariables);

            # region Live View

            if (!GridView)
            // For live view as graph
            {
                // Create vector to hold the values of the time dimension
                OutTimes = new float[NumTimeSteps + 1];
                // Set the first value to be 0
                OutTimes[0] = 0;
                // Fill other values from 0 (this will hold outputs during the model run)
                for (int ii = 1; ii < NumTimeSteps + 1; ii++)
                {
                    OutTimes[ii] = ii + 1;
                }

                // Create a string holding the name of the x-axis variable
                string[] dimensions1D = { "Time step" };

                // Add the x-axis to the plots (time step)
                DataSetToViewLive.AddAxis("Time step", "Month", OutTimes);

                // Add in the carnivore and herbivore abundance variables
                ArraySDSConvert.AddVariable(DataSetToViewLive, "Carnivore density", "Individuals / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
                ArraySDSConvert.AddVariable(DataSetToViewLive, "Herbivore density", "Individuals / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
                ArraySDSConvert.AddVariable(DataSetToViewLive, "Omnivore density", "Individuals / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes); 

                // Add in the initial values of carnivore and herbivore abundance
                ArraySDSConvert.ValueToSDS1D(ref CarnivoreDensityOut, "Carnivore density", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
                ArraySDSConvert.ValueToSDS1D(ref HerbivoreDensityOut, "Herbivore density", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
                ArraySDSConvert.ValueToSDS1D(ref OmnivoreDensityOut, "Omnivore density", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);

                // Add in the carnivore and herbivore biomass variables
                ArraySDSConvert.AddVariable(DataSetToViewLive, "Carnivore biomass", "Kg / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
                ArraySDSConvert.AddVariable(DataSetToViewLive, "Herbivore biomass", "Kg / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);
                ArraySDSConvert.AddVariable(DataSetToViewLive, "Omnivore biomass", "Kg / km^2", 1, dimensions1D, EcosystemModelGrid.GlobalMissingValue, OutTimes);

                // Add in the initial values of carnivore and herbivore abundance
                ArraySDSConvert.ValueToSDS1D(ref CarnivoreBiomassDensityOut, "Carnivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
                ArraySDSConvert.ValueToSDS1D(ref HerbivoreBiomassDensityOut, "Herbivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
                ArraySDSConvert.ValueToSDS1D(ref OmnivoreBiomassDensityOut, "Omnivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
            }
            else
            // Whole grid
            {
                // Add the three-dimensional biomass grid to the live-view dataset
                ArraySDSConvert.Array2DToSDS2D(LogBiomassDensityGridCohorts, "Log(Biomass density)", EcosystemModelGrid.Lats, EcosystemModelGrid.Lons, EcosystemModelGrid.GlobalMissingValue, DataSetToViewLive);
            }

            # endregion

            # region Console Outputs

            // Write out initial total biomass, abundance and number of cohorts
            TextOutput.WriteOutput("Initial total biomass (all) = " + String.Format("{0:N}", TotalBiomassOut / 1000) + " kg", "high", ConsoleColor.White);
            TextOutput.WriteOutput("Initial living biomass = " + String.Format("{0:N}", TotalLivingBiomassOut / 1000) + " kg", "low", ConsoleColor.White);
            TextOutput.WriteOutput("Initial respiratory pool biomass = " + String.Format("{0:N}", RespiratoryPoolOut / 1000) + " kg", "high", ConsoleColor.White);
            TextOutput.WriteOutput("Initial organic pool biomass = " + String.Format("{0:N}", OrganicPoolOut / 1000) + " kg", "high", ConsoleColor.White);
            TextOutput.WriteOutput("Initial total density  = " + String.Format("{0:N}", TotalDensityOut) + " inds / km^2", "low", ConsoleColor.White);
            TextOutput.WriteOutput("Initial total number of cohorts = " + TotalNumberOfCohorts, "medium", ConsoleColor.White);
            TextOutput.WriteOutput("Initial number of stocks = " + TotalNumberOfStocks, "medium", ConsoleColor.White);
            TextOutput.WriteOutput(" ", "low", ConsoleColor.White);

            # endregion

            # region File Outputs

            // Write densities, biomasses and abundances in different functional groups to the relevant one-dimensional output variables
            ArraySDSConvert.ValueToSDS1D(ref HerbivoreDensityOut, "Herbivore density", "Time step", EcosystemModelGrid.GlobalMissingValue, 
                DataSetForFileOutput, 0);
            ArraySDSConvert.ValueToSDS1D(ref HerbivoreBiomassDensityOut, "Herbivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue, 
                DataSetForFileOutput, 0);
            ArraySDSConvert.ValueToSDS1D(ref CarnivoreDensityOut, "Carnivore density", "Time step", EcosystemModelGrid.GlobalMissingValue, 
                DataSetForFileOutput, 0);
            ArraySDSConvert.ValueToSDS1D(ref CarnivoreBiomassDensityOut, "Carnivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue, 
                DataSetForFileOutput, 0);
            ArraySDSConvert.ValueToSDS1D(ref OmnivoreDensityOut, "Omnivore density", "Time step", EcosystemModelGrid.GlobalMissingValue, 
                DataSetForFileOutput, 0);
            ArraySDSConvert.ValueToSDS1D(ref OmnivoreBiomassDensityOut, "Omnivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue, 
                DataSetForFileOutput, 0);
            ArraySDSConvert.ValueToSDS1D(ref AutotrophBiomassDensityOut, "Autotroph biomass", "Time step", EcosystemModelGrid.GlobalMissingValue, 
                DataSetForFileOutput, 0);

            ArraySDSConvert.ValueToSDS1D(ref CarnivoreAbundanceOut, "Carnivore abundance", "Time step", EcosystemModelGrid.GlobalMissingValue, 
                DataSetForFileOutput, 0);
            ArraySDSConvert.ValueToSDS1D(ref HerbivoreAbundanceOut, "Herbivore abundance", "Time step", EcosystemModelGrid.GlobalMissingValue,
                DataSetForFileOutput, 0);

            // Write out total biomass in the organic and respiratory pools to the relevant one-dimensional output variables
            ArraySDSConvert.ValueToSDS1D(ref OrganicPoolOut, "Organic matter pool", "Time step", EcosystemModelGrid.GlobalMissingValue, 
                DataSetForFileOutput, 0);
            ArraySDSConvert.ValueToSDS1D(ref RespiratoryPoolOut, "Respiratory CO2 pool", "Time step", EcosystemModelGrid.GlobalMissingValue, 
                DataSetForFileOutput, 0);

            // Write out the total number of cohorts and stocks to the relevant one-dimensional output variables
            ArraySDSConvert.ValueToSDS1D(ref TotalNumberOfCohorts, "Number of cohorts in model", "Time step", EcosystemModelGrid.
                GlobalMissingValue, DataSetForFileOutput, 0);
            ArraySDSConvert.ValueToSDS1D(ref TotalNumberOfStocks, "Number of stocks in model", "Time step", EcosystemModelGrid.
                GlobalMissingValue, DataSetForFileOutput, 0);

            //Write out the number of cohorts produced, extinct and combined to the relevant 1D output variables
            ArraySDSConvert.ValueToSDS1D(ref NumberOfCohortsProduced, "Number of cohorts produced", "Time step", EcosystemModelGrid.
    GlobalMissingValue, DataSetForFileOutput, 0);
            ArraySDSConvert.ValueToSDS1D(ref NumberOfCohortsExtinct, "Number of cohorts extinct", "Time step", EcosystemModelGrid.
GlobalMissingValue, DataSetForFileOutput, 0);
            ArraySDSConvert.ValueToSDS1D(ref NumberOfCohortsCombined, "Number of cohorts combined", "Time step", EcosystemModelGrid.
GlobalMissingValue, DataSetForFileOutput, 0);

            // Write out abundances in each of the mass bins to the relevant two-dimensional output variables
            ArraySDSConvert.VectorToSDS2D(ref CarnivoreAbundanceInMassBins, "Log Carnivore abundance in mass bins",
                new string[2] { "Time step", "Mass bin" }, OutTimes, OutMassBins, Math.Log(0), DataSetForFileOutput, 0);
            ArraySDSConvert.VectorToSDS2D(ref HerbivoreAbundanceInMassBins, "Log Herbivore abundance in mass bins",
                new string[2] { "Time step", "Mass bin" }, OutTimes, OutMassBins, Math.Log(0), DataSetForFileOutput, 0);

            // Write out abundances in each of the mass bins to the relevant two-dimensional output variables
            ArraySDSConvert.VectorToSDS2D(ref CarnivoreBiomassInMassBins, "Log Carnivore biomass in mass bins",
                new string[2] { "Time step", "Mass bin" }, OutTimes, OutMassBins, Math.Log(0), DataSetForFileOutput, 0);
            ArraySDSConvert.VectorToSDS2D(ref HerbivoreBiomassInMassBins, "Log Herbivore biomass in mass bins",
                new string[2] { "Time step", "Mass bin" }, OutTimes, OutMassBins, Math.Log(0), DataSetForFileOutput, 0);

            // Write out abundances in each of the mass bins to the relevant two-dimensional output variables
            ArraySDSConvert.Array2DToSDS3D(CarnivoreAbundanceVsJuvenileAdultMass,
                "Log Carnivore abundance in juvenile vs adult bins",
                EcosystemModelGrid.Lats,EcosystemModelGrid.Lons,
                "Time step",
                0,
                Math.Log(0),
                DataSetForFileOutput);

            // Write out abundances in each of the mass bins to the relevant two-dimensional output variables
            ArraySDSConvert.Array2DToSDS3D(HerbivoreAbundanceVsJuvenileAdultMass,
                "Log Herbivore abundance in juvenile vs adult bins",
                EcosystemModelGrid.Lats, EcosystemModelGrid.Lons,
                "Time step",
                0,
                Math.Log(0),
                DataSetForFileOutput);

            // Write out abundances in each of the mass bins to the relevant two-dimensional output variables
            ArraySDSConvert.Array2DToSDS3D(CarnivoreBiomassVsJuvenileAdultMass,
                "Log Carnivore biomass in juvenile vs adult bins",
                EcosystemModelGrid.Lats, EcosystemModelGrid.Lons,
                "Time step",
                0,
                Math.Log(0),
                DataSetForFileOutput);

            // Write out abundances in each of the mass bins to the relevant two-dimensional output variables
            ArraySDSConvert.Array2DToSDS3D(HerbivoreBiomassVsJuvenileAdultMass,
                "Log Herbivore biomass in juvenile vs adult bins",
                EcosystemModelGrid.Lats, EcosystemModelGrid.Lons,
                "Time step",
                0,
                Math.Log(0),
                DataSetForFileOutput);


            // Add the three-dimensional biomass grid to the file dataset
            ArraySDSConvert.Array2DToSDS3D(LogBiomassDensityGridCohorts, "Biomass density", EcosystemModelGrid.Lats, EcosystemModelGrid.Lons, "Time step",0, EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput);

            # endregion
        }

        /// <summary>
        /// Write to the output file values of the output variables during the model time steps
        /// </summary>
        /// <param name="EcosystemModelGrid">The model grid to get data from</param>
        /// <param name="CohortFunctionalGroupDefinitions">The definitions of the cohort functional groups in the model</param>
        /// <param name="StockFunctionalGroupDefinitions">The definitions of the stock  functional groups in the model</param>
        /// <param name="_LatCellIndices">The latitudinal indices of live cells in the model</param>
        /// <param name="_LonCellIndices">The longitudinal indices of live cells in the model</param>
        /// <param name="GlobalDiagnosticVariables">List of global diagnostic variables</param>
        /// <param name="TimeStepTimer">Stopwatch instance for calculating the time to run each time step</param>
        /// <param name="NumTimeSteps">The number of time steps in the model run</param>
        public void TimeStepOutputs(ModelGrid EcosystemModelGrid, FunctionalGroupDefinitions CohortFunctionalGroupDefinitions, FunctionalGroupDefinitions
            StockFunctionalGroupDefinitions, uint[] _LatCellIndices, uint[] _LonCellIndices, SortedList<string, double> GlobalDiagnosticVariables,
            StopWatch TimeStepTimer, uint NumTimeSteps, uint currentTimestep)
        {
            // Calculate values of the output variables to be used
            this.CalculateOutputs(EcosystemModelGrid, CohortFunctionalGroupDefinitions, StockFunctionalGroupDefinitions, _LatCellIndices, _LonCellIndices, GlobalDiagnosticVariables);


            # region Live View

            if (GridView)
            {
                // Add the three-dimensional biomass grid to the live-view dataset
                //ArraySDSConvert.Array2DToSDS2D(LogBiomassGridCohorts, "Log(Biomass)", EcosystemModelGrid.Lats, EcosystemModelGrid.Lons, 0, DataSetToView);


                // Add the three-dimensional biomass grid to the live-view dataset
                ArraySDSConvert.Array2DToSDS2D(LogBiomassDensityGridCohorts, "Log(Biomass density)", EcosystemModelGrid.Lats, EcosystemModelGrid.Lons, 0, DataSetToViewLive);

                // Output the carnivore and herbivore biomasses
                //ArraySDSConvert.ValueToSDS1D(ref CarnivoreBiomassDensityOut, "Carnivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue,
                //    DataSetToViewLive, (int)currentTimestep + 1);
                //ArraySDSConvert.ValueToSDS1D(ref HerbivoreBiomassDensityOut, "Herbivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue,
                //    DataSetToViewLive, (int)currentTimestep + 1);

            }
            else
            {
                // Rescale the y-axis appropriately
                if (HerbivoreDensityOut > MaximumYValue)
                {
                    MaximumYValue = HerbivoreDensityOut * 1.1;
                    DataSetToViewLive.Metadata["VisualHints"] = "\"Carnivore density\"[Time step]; Style:Polyline; Visible: 0,1," 
                        + NumTimeSteps.ToString() + ","
                        + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#D95F02;Thickness:3;;\"Herbivore density\"[Time step] ; Style:Polyline; Visible: 0,1,"
                        + NumTimeSteps.ToString() + ","
                        + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#1B9E77;Thickness:3;;\"Omnivore density\"[Time step] ; Style:Polyline; Visible: 0,1,"
                        + NumTimeSteps.ToString() + ","
                        + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#7570B3;Thickness:3; Title:\"Heterotroph Densities";
                }

                // Output the total carnivore, herbivore and omnivore abundances
                ArraySDSConvert.ValueToSDS1D(ref CarnivoreDensityOut, "Carnivore density", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetToViewLive,
                    (int)currentTimestep + 1);
                ArraySDSConvert.ValueToSDS1D(ref HerbivoreDensityOut, "Herbivore density", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetToViewLive,
                    (int)currentTimestep + 1);
                ArraySDSConvert.ValueToSDS1D(ref OmnivoreDensityOut, "Omnivore density", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetToViewLive,
                    (int)currentTimestep + 1);

                // Output the carnivore, herbivore and omnivore biomasses
                ArraySDSConvert.ValueToSDS1D(ref CarnivoreBiomassDensityOut, "Carnivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue,
                    DataSetToViewLive, (int)currentTimestep + 1);
                ArraySDSConvert.ValueToSDS1D(ref HerbivoreBiomassDensityOut, "Herbivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue,
                    DataSetToViewLive, (int)currentTimestep + 1);
                ArraySDSConvert.ValueToSDS1D(ref OmnivoreBiomassDensityOut, "Omnivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue,
                    DataSetToViewLive, (int)currentTimestep + 1);

            }

            
            
            # endregion

            # region Console Outputs

            // Write out initial total biomass, abundance and number of cohorts
            TextOutput.WriteOutput("Completed time step " + currentTimestep, "low", ConsoleColor.Green);
            TextOutput.WriteOutput("Time step corresponds to month " + currentTimestep%12, "high", ConsoleColor.White);
            TextOutput.WriteOutput("Elapsed time in seconds this time step: " + TimeStepTimer.GetElapsedTimeSecs(), "low", ConsoleColor.White);
            TextOutput.WriteOutput("Total biomass (all) = " + String.Format("{0:N}", TotalBiomassOut / 1000) + " kg", "medium", ConsoleColor.White);
            TextOutput.WriteOutput("Living biomass = " + String.Format("{0:N}", TotalLivingBiomassOut / 1000) + " kg", "low", ConsoleColor.White);
            TextOutput.WriteOutput("Respiratory pool biomass = " + String.Format("{0:N}", RespiratoryPoolOut / 1000) + " kg", "high", ConsoleColor.White);
            TextOutput.WriteOutput("Organic pool biomass = " + String.Format("{0:N}", OrganicPoolOut / 1000) + " kg", "high", ConsoleColor.White);
            TextOutput.WriteOutput("Total density  = " + String.Format("{0:N}", TotalDensityOut) + " inds / km^2", "low", ConsoleColor.White);
            TextOutput.WriteOutput("Total number of cohorts = " + TotalNumberOfCohorts, "medium", ConsoleColor.White);
            TextOutput.WriteOutput("Total number of stocks = " + TotalNumberOfStocks, "medium", ConsoleColor.White);
            TextOutput.WriteOutput("Number of cohorts extinct = " + NumberOfCohortsExtinct, "medium", ConsoleColor.White);
            TextOutput.WriteOutput("Number of cohorts produced = " + NumberOfCohortsProduced, "medium", ConsoleColor.White);
            TextOutput.WriteOutput("Number of cohorts combined = " + NumberOfCohortsCombined, "medium", ConsoleColor.White); 
            TextOutput.WriteOutput(" ", "low", ConsoleColor.White);

            // Check that total living biomass is a number
            Debug.Assert(TotalLivingBiomassOut != Double.NaN, "NANS!!!! PANIC!!!");

            # endregion


            # region File Outputs

            ArraySDSConvert.ValueToSDS1D(ref HerbivoreDensityOut, "Herbivore density", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref HerbivoreBiomassDensityOut, "Herbivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref CarnivoreDensityOut, "Carnivore density", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref CarnivoreBiomassDensityOut, "Carnivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref OmnivoreDensityOut, "Omnivore density", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref OmnivoreBiomassDensityOut, "Omnivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref AutotrophBiomassDensityOut, "Autotroph biomass", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref OrganicPoolOut, "Organic matter pool", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref RespiratoryPoolOut, "Respiratory CO2 pool", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref TotalNumberOfCohorts, "Number of cohorts in model", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref TotalNumberOfStocks, "Number of stocks in model", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref NumberOfCohortsExtinct, "Number of cohorts extinct", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref NumberOfCohortsProduced, "Number of cohorts produced", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref NumberOfCohortsCombined, "Number of cohorts combined", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);


            ArraySDSConvert.ValueToSDS1D(ref CarnivoreAbundanceOut, "Carnivore abundance", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref HerbivoreAbundanceOut, "Herbivore abundance", "Time step", EcosystemModelGrid.GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);

            // Add the three-dimensional biomass grid to the file dataset
            ArraySDSConvert.Array2DToSDS3D(LogBiomassDensityGridCohorts, "Biomass density", EcosystemModelGrid.Lats, EcosystemModelGrid.Lons, "Time step",
                (int)currentTimestep, 0, DataSetForFileOutput);

            // Write densities, biomasses and abundances in different functional groups to the relevant one-dimensional output variables
            ArraySDSConvert.ValueToSDS1D(ref HerbivoreDensityOut, "Herbivore density", "Time step", EcosystemModelGrid.GlobalMissingValue,
                DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref HerbivoreBiomassDensityOut, "Herbivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue,
                DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref CarnivoreDensityOut, "Carnivore density", "Time step", EcosystemModelGrid.GlobalMissingValue,
                DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref CarnivoreBiomassDensityOut, "Carnivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue,
                DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref OmnivoreDensityOut, "Omnivore density", "Time step", EcosystemModelGrid.GlobalMissingValue,
                DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref OmnivoreBiomassDensityOut, "Omnivore biomass", "Time step", EcosystemModelGrid.GlobalMissingValue,
                DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref AutotrophBiomassDensityOut, "Autotroph biomass", "Time step", EcosystemModelGrid.GlobalMissingValue,
                DataSetForFileOutput, (int)currentTimestep + 1);
           
            ArraySDSConvert.ValueToSDS1D(ref CarnivoreAbundanceOut, "Carnivore abundance", "Time step", EcosystemModelGrid.GlobalMissingValue,
                DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref HerbivoreAbundanceOut, "Herbivore abundance", "Time step", EcosystemModelGrid.GlobalMissingValue,
                DataSetForFileOutput, (int)currentTimestep + 1);

            // Write out total biomass in the organic and respiratory pools to the relevant one-dimensional output variables
            ArraySDSConvert.ValueToSDS1D(ref OrganicPoolOut, "Organic matter pool", "Time step", EcosystemModelGrid.GlobalMissingValue,
                DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref RespiratoryPoolOut, "Respiratory CO2 pool", "Time step", EcosystemModelGrid.GlobalMissingValue,
                DataSetForFileOutput, (int)currentTimestep + 1);

            // Write out the total number of cohorts and stocks to the relevant one-dimensional output variables
            ArraySDSConvert.ValueToSDS1D(ref TotalNumberOfCohorts, "Number of cohorts in model", "Time step", EcosystemModelGrid.
                GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref TotalNumberOfStocks, "Number of stocks in model", "Time step", EcosystemModelGrid.
                GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            
            // Write out numbers of cohorts that have been produced and gone extinct in this time step
            ArraySDSConvert.ValueToSDS1D(ref NumberOfCohortsExtinct, "Number of cohorts extinct", "Time step", EcosystemModelGrid.
                GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.ValueToSDS1D(ref NumberOfCohortsProduced, "Number of cohorts produced", "Time step", EcosystemModelGrid.
                GlobalMissingValue, DataSetForFileOutput, (int)currentTimestep + 1);

            // Write out abundances in each of the mass bins to the relevant two-dimensional output variables
            ArraySDSConvert.VectorToSDS2D(ref CarnivoreAbundanceInMassBins, "Log Carnivore abundance in mass bins",
                new string[2] { "Time step", "Mass bin" }, OutTimes, OutMassBins, Math.Log(0), DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.VectorToSDS2D(ref HerbivoreAbundanceInMassBins, "Log Herbivore abundance in mass bins",
                new string[2] { "Time step", "Mass bin" }, OutTimes, OutMassBins, Math.Log(0), DataSetForFileOutput, (int)currentTimestep + 1);

            // Write out abundances in each of the mass bins to the relevant two-dimensional output variables
            ArraySDSConvert.VectorToSDS2D(ref CarnivoreBiomassInMassBins, "Log Carnivore biomass in mass bins",
                new string[2] { "Time step", "Mass bin" }, OutTimes, OutMassBins, Math.Log(0), DataSetForFileOutput, (int)currentTimestep + 1);
            ArraySDSConvert.VectorToSDS2D(ref HerbivoreBiomassInMassBins, "Log Herbivore biomass in mass bins",
                new string[2] { "Time step", "Mass bin" }, OutTimes, OutMassBins, Math.Log(0), DataSetForFileOutput, (int)currentTimestep + 1);

            // Write out abundances in each of the mass bins to the relevant two-dimensional output variables
            ArraySDSConvert.Array2DToSDS3D(CarnivoreAbundanceVsJuvenileAdultMass,
                "Log Carnivore abundance in juvenile vs adult bins",
                EcosystemModelGrid.Lats, EcosystemModelGrid.Lons,
                "Time step",
                (int)currentTimestep+1,
                Math.Log(0),
                DataSetForFileOutput);

            // Write out abundances in each of the mass bins to the relevant two-dimensional output variables
            ArraySDSConvert.Array2DToSDS3D(HerbivoreAbundanceVsJuvenileAdultMass,
                "Log Herbivore abundance in juvenile vs adult bins",
                EcosystemModelGrid.Lats, EcosystemModelGrid.Lons,
                "Time step",
                (int)currentTimestep+1,
                Math.Log(0),
                DataSetForFileOutput);

            // Write out abundances in each of the mass bins to the relevant two-dimensional output variables
            ArraySDSConvert.Array2DToSDS3D(CarnivoreBiomassVsJuvenileAdultMass,
                "Log Carnivore biomass in juvenile vs adult bins",
                EcosystemModelGrid.Lats, EcosystemModelGrid.Lons,
                "Time step",
                (int)currentTimestep+1,
                Math.Log(0),
                DataSetForFileOutput);

            // Write out abundances in each of the mass bins to the relevant two-dimensional output variables
            ArraySDSConvert.Array2DToSDS3D(HerbivoreBiomassVsJuvenileAdultMass,
                "Log Herbivore biomass in juvenile vs adult bins",
                EcosystemModelGrid.Lats, EcosystemModelGrid.Lons,
                "Time step",
                (int)currentTimestep+1,
                Math.Log(0),
                DataSetForFileOutput);
            
            # endregion
        }

        /// <summary>
        /// Write to the output file values of the output variables at the end of the model run
        /// </summary>
        /// <param name="EcosystemModelGrid">The model grid to get data from</param>
        /// <param name="CohortFunctionalGroupDefinitions">Definitions of the cohort functional groups in the model</param>
        /// <param name="StockFunctionalGroupDefinitions">Definitions of the stock functional groups in the model</param>
        /// <param name="_LatCellIndices">The latitudinal indices of live cells in the model</param>
        /// <param name="_LonCellIndices">The longitudinal indices of live cells in the model</param>
        /// <param name="GlobalDiagnosticVariables">List of global diagnostic variables</param>
        public void FinalOutputs(ModelGrid EcosystemModelGrid, FunctionalGroupDefinitions CohortFunctionalGroupDefinitions, FunctionalGroupDefinitions
            StockFunctionalGroupDefinitions, uint[] _LatCellIndices, uint[] _LonCellIndices, SortedList<string, double> GlobalDiagnosticVariables)
        {
            // Calculate output variables
            CalculateOutputs(EcosystemModelGrid, CohortFunctionalGroupDefinitions, StockFunctionalGroupDefinitions, _LatCellIndices, _LonCellIndices, GlobalDiagnosticVariables);

            // Write out final total biomass to the console
            Console.WriteLine("Final total biomass = {0}", TotalBiomassOut);

            // Dispose of the dataset objects
            DataSetForFileOutput.Dispose();
            DataSetToViewLive.Dispose();

        }

    }
}
