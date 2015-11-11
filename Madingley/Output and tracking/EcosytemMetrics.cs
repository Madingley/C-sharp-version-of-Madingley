using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

// using RDotNet;
// using RserveCli;

namespace Madingley
{
    /// <summary>
    /// Calculates ecosystem-level metrics
    /// </summary>
    public class EcosytemMetrics
    {
        /// <summary>
        /// The trophic level values associated with each of the trophic level bins
        /// </summary>
        private float[] _TrophicIndexBinValues;
        /// <summary>
        /// Get and set the trophic level values associated with each of the trophic level bins
        /// </summary>
        public float[] TrophicIndexBinValues
        {
            get { return _TrophicIndexBinValues; }
            set { _TrophicIndexBinValues = value; }
        }

        /// <summary>
        /// The number of trophic level bins to use in calculating ecosystem-level metrics
        /// </summary>
        private int _NumberTrophicBins;

        /// <summary>
        /// Get and set the number of trophic level bins to use in calculating ecosystem-level metrics
        /// </summary>
        public int NumberTrophicBins
        {
            get { return _NumberTrophicBins; }
            set { _NumberTrophicBins = value; }
        }

        /// <summary>
        /// Instance of the connection to R
        /// </summary>
        private Process _RServeProcess;


        //Define upp and lower limits for trophic index
        private double MaxTI = 40.0;
        private double MinTI = 1.0;
       
        /// <summary>
        /// Constructor for the ecosystem metrics class: sets up trophic level bins
        /// </summary>
        public EcosytemMetrics()
        {

            float TrophicIndexBinWidth = 0.2f;
            float LowestTophicIndex = 1.0f;
            float HighestTrophicIndex = 5.0f;
            NumberTrophicBins = (int)((HighestTrophicIndex - LowestTophicIndex) / TrophicIndexBinWidth);
            _TrophicIndexBinValues = new float[NumberTrophicBins];

            for (int i = 0; i < TrophicIndexBinValues.Length; i++)
            {
                _TrophicIndexBinValues[i] = LowestTophicIndex + (TrophicIndexBinWidth * i);
            }

            /*Console.WriteLine("Opening R connection");

            
            int port = int.Parse((6).ToString() + (2*scenarioIndex).ToString() + (simulationNumber).ToString() + cellIndex.ToString());

            ProcessStartInfo StartInfo = new ProcessStartInfo();
            StartInfo.CreateNoWindow = true;
            StartInfo.UseShellExecute = false;
            StartInfo.FileName = "C:/Users/mikeha/Documents/R/win-library/2.14/Rserve/libs/x64/RServe.exe";
            StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            StartInfo.Arguments = "--RS-port " + port.ToString();
            
            //Start an instance of RServe to talk with R
            _RServeProcess = Process.Start(StartInfo);

            R = new RConnection(new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 }),port);
            //RConn.VoidEval("install.packages(/"FD/")");
            R.VoidEval("library(FD)");
            */
        }

        /// <summary>
        /// Closes the connection to R (currently disabled)
        /// </summary>
        public void CloseRserve()
        {
            //_RServeProcess.Kill();
        }

        /// <summary>
        /// Calculates the mean trophic level of all individual organisms in a grid cell
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cellIndices">The list of cell indices in the current model simulation</param>
        /// <param name="cellIndex">The index of the current cell in the list of cells to run</param>
        /// <returns>The mean trophic level of individuals in the grid cell</returns>
        public double CalculateMeanTrophicLevelCell(ModelGrid ecosystemModelGrid,List<uint[]> cellIndices, int cellIndex)
        {
            //Get the cohorts for the specified cell
            GridCellCohortHandler CellCohorts = ecosystemModelGrid.GetGridCellCohorts(cellIndices[cellIndex][0], cellIndices[cellIndex][1]);
            double BiomassWeightedTI = 0.0;
            double TotalBiomass = 0.0;
            double CohortBiomass = 0.0;

            foreach (var CohortList in CellCohorts)
            {
                foreach (Cohort c in CohortList)
                {
                    CohortBiomass = (c.IndividualBodyMass + c.IndividualReproductivePotentialMass) * c.CohortAbundance;
                    BiomassWeightedTI += CohortBiomass * c.TrophicIndex;
                    TotalBiomass += CohortBiomass;
                }
            }

            return BiomassWeightedTI/TotalBiomass;
        }

        /// <summary>
        /// Return the distribution of biomasses among trophic level bins
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cellIndices">The list of cell indices to be run in the current model simulation</param>
        /// <param name="cellIndex">The index of the current cell in the list of cells to be run</param>
        /// <returns>The distribution of biomasses among trophic level bins</returns>
        public double[] CalculateTrophicDistribution(ModelGrid ecosystemModelGrid, List<uint[]> cellIndices, int cellIndex)
        {
            //Get the cohorts for the specified cell
            GridCellCohortHandler CellCohorts = ecosystemModelGrid.GetGridCellCohorts(cellIndices[cellIndex][0], cellIndices[cellIndex][1]);
            double[] TrophicIndexBinMasses = new double[NumberTrophicBins];
            int BinIndex;


            foreach (var CohortList in CellCohorts)
            {
                foreach (Cohort c in CohortList)
                {
                    BinIndex = _TrophicIndexBinValues.ToList().IndexOf(_TrophicIndexBinValues.Last(x => x < c.TrophicIndex));
                    TrophicIndexBinMasses[BinIndex] += (c.IndividualBodyMass + c.IndividualReproductivePotentialMass) * c.CohortAbundance;
                }
            }

            return TrophicIndexBinMasses;
        }


        public double[] CalculateFunctionalRichness(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortDefinitions, 
            List<uint[]> cellIndices, int cellIndex, string trait)
        {

            //Get the cohorts for the specified cell
            GridCellCohortHandler CellCohorts = ecosystemModelGrid.GetGridCellCohorts(cellIndices[cellIndex][0], cellIndices[cellIndex][1]);
            double MinCurrentTraitValue = double.MaxValue;
            double MaxCurrentTraitValue = double.MinValue;
            double MinModelTraitValue = 0.0;
            double MaxModelTraitValue = 0.0;

            switch (trait.ToLower())
            {
                case "biomass":

                    foreach (var CohortList in CellCohorts)
                    {
                        foreach (var cohort in CohortList)
                        {

                            if (cohort.IndividualBodyMass < MinCurrentTraitValue) MinCurrentTraitValue = cohort.IndividualBodyMass;

                            if (cohort.IndividualBodyMass > MaxCurrentTraitValue) MaxCurrentTraitValue = cohort.IndividualBodyMass;

                        }
                    }


                    //Define upper and lower limits for body mass
                    MinModelTraitValue = cohortDefinitions.GetBiologicalPropertyAllFunctionalGroups("minimum mass").Min();
                    MaxModelTraitValue = cohortDefinitions.GetBiologicalPropertyAllFunctionalGroups("maximum mass").Max();
                    break;
                case "trophic index":
                    foreach (var CohortList in CellCohorts)
                    {
                        foreach (var cohort in CohortList)
                        {

                            if (cohort.TrophicIndex < MinCurrentTraitValue) MinCurrentTraitValue = cohort.TrophicIndex;

                            if (cohort.TrophicIndex > MaxCurrentTraitValue) MaxCurrentTraitValue = cohort.TrophicIndex;

                        }
                    }


                    //Define upper and lower limits for body mass
                    MinModelTraitValue = MinTI;
                    MaxModelTraitValue = MaxTI;

                    break;
                default:
                    Debug.Fail("Trait not recognised in calculation of ecosystem metrics: " + trait);
                    break;
            }

            Debug.Assert((MaxModelTraitValue - MinModelTraitValue) > 0.0, "Division by zero or negative model trait values in calculation of functional richness");

            double[] NewArray = {(MaxCurrentTraitValue-MinCurrentTraitValue)/(MaxModelTraitValue-MinModelTraitValue),MinCurrentTraitValue,MaxCurrentTraitValue};

            return NewArray;
        }

        /// <summary>
        /// Calculate trophic evenness using the Rao Index
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cellIndices">The list of indices of cells to be run in the current simulation</param>
        /// <param name="cellIndex">The index of the current cell within the list of cells to be run</param>
        /// <returns>Trophic evenness</returns>
        public double CalculateFunctionalEvennessRao(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortDefinitions,
            List<uint[]> cellIndices, int cellIndex, string trait)
        {
            //Get the cohorts for the specified cell
            GridCellCohortHandler CellCohorts = ecosystemModelGrid.GetGridCellCohorts(cellIndices[cellIndex][0], cellIndices[cellIndex][1]);

            double[] EvennessValues = new double[2];

            double[,] Distances = new double[CellCohorts.GetNumberOfCohorts(), CellCohorts.GetNumberOfCohorts()];

            double[] FunctionalTrait = new double[CellCohorts.GetNumberOfCohorts()];
            double MaxModelTraitValue=0;
            double MinModelTraitValue=0;

            // Construct a vector of cohort biomass (in case we want to weight by them)
            double[] CohortTotalBiomasses = new double[CellCohorts.GetNumberOfCohorts()];


            int CohortNumberCounter = 0;
            switch (trait.ToLower())
            {
                case "biomass":
                    for (int fg = 0; fg < CellCohorts.Count; fg++)
                    {
                        foreach (Cohort c in CellCohorts[fg])
                        {

                            FunctionalTrait[CohortNumberCounter] = c.IndividualBodyMass;
                            CohortTotalBiomasses[CohortNumberCounter] = (c.IndividualBodyMass + c.IndividualReproductivePotentialMass) * c.CohortAbundance;

                            CohortNumberCounter++;
                        }
                    }

                    //Define upper and lower limits for body mass
                    MinModelTraitValue = cohortDefinitions.GetBiologicalPropertyAllFunctionalGroups("minimum mass").Min();
                    MaxModelTraitValue = cohortDefinitions.GetBiologicalPropertyAllFunctionalGroups("maximum mass").Max();
                    break;
                case "trophic index":
                    for (int fg = 0; fg < CellCohorts.Count; fg++)
                    {
                        foreach (Cohort c in CellCohorts[fg])
                        {

                            FunctionalTrait[CohortNumberCounter] = c.IndividualBodyMass;
                            CohortTotalBiomasses[CohortNumberCounter] = (c.IndividualBodyMass + c.IndividualReproductivePotentialMass) * c.CohortAbundance;

                            CohortNumberCounter++;
                        }
                    }
                    MinModelTraitValue = MinTI;
                    MaxModelTraitValue = MaxTI;
                    break;
            }


            Distances = CalculateDistanceMatrix(FunctionalTrait, MaxModelTraitValue, MinModelTraitValue);

            return RaoEntropy(Distances, CohortTotalBiomasses);

        }

        /// <summary>
        /// Calculates the arithmetic community weighted mean body mass
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cellIndices">The list of indices of cells to be run in the current model simulation</param>
        /// <param name="cellIndex">The index of the current cell within the list of cells to be run</param>
        /// <returns>arithmetic community weighted mean body mass</returns>
        public double CalculateArithmeticCommunityMeanBodyMass(ModelGrid ecosystemModelGrid, List<uint[]> cellIndices, int cellIndex)
        {

            //Get the cohorts for the specified cell
            GridCellCohortHandler CellCohorts = ecosystemModelGrid.GetGridCellCohorts(cellIndices[cellIndex][0], cellIndices[cellIndex][1]);
            double CumulativeAbundance = 0.0;
            double CumulativeBiomass = 0.0;

            //Retrieve the biomass
            foreach (var CohortList in CellCohorts)
            {
                foreach (Cohort c in CohortList)
                {
                    CumulativeBiomass += (c.IndividualBodyMass + c.IndividualReproductivePotentialMass) * c.CohortAbundance;
                    CumulativeAbundance += c.CohortAbundance;
                }
            }

            double CWAMBM = (CumulativeBiomass / CumulativeAbundance);

            return (CWAMBM);

        }

        /// <summary>
        /// Calculates the geometric community weighted mean body mass
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cellIndices">The list of indices of cells to be run in the current model simulation</param>
        /// <param name="cellIndex">The index of the current cell within the list of cells to be run</param>
        /// <returns>geometric community weighted mean body mass</returns>
        public double CalculateGeometricCommunityMeanBodyMass(ModelGrid ecosystemModelGrid, List<uint[]> cellIndices, int cellIndex)
        {

            //Get the cohorts for the specified cell
            GridCellCohortHandler CellCohorts = ecosystemModelGrid.GetGridCellCohorts(cellIndices[cellIndex][0], cellIndices[cellIndex][1]);
            double CumulativeAbundance = 0.0;
            double CumulativeLogBiomass = 0.0;
            
            //Retrieve the biomass
            foreach (var CohortList in CellCohorts)
            {
                foreach (Cohort c in CohortList)
                {
                    CumulativeLogBiomass += Math.Log(c.IndividualBodyMass + c.IndividualReproductivePotentialMass) * c.CohortAbundance;
                    CumulativeAbundance += c.CohortAbundance;
                }
            }

            double CWGMBM = Math.Exp(CumulativeLogBiomass / CumulativeAbundance);

            return (CWGMBM);

        }

        /// <summary>
        /// Calculates trophic evenness using the FRO Index of Mouillot et al.
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cellIndices">The list of indices of cells to be run in the current model simulation</param>
        /// <param name="cellIndex">The index of the current cell within the list of cells to be run</param>
        /// <returns>Trophic evenness</returns>
        /// <remarks>From Mouillot et al (2005) Functional regularity: a neglected aspect of functional diversity, Oecologia</remarks>
        public double CalculateTrophicEvennessFRO(ModelGrid ecosystemModelGrid, List<uint[]> cellIndices, int cellIndex)
        {

            //Get the cohorts for the specified cell
            GridCellCohortHandler CellCohorts = ecosystemModelGrid.GetGridCellCohorts(cellIndices[cellIndex][0], cellIndices[cellIndex][1]);
            List<double[]> TrophicIndexBiomassDistribution = new List<double[]>();
            double[] TIBiomass;
            double[] EW;

            foreach (var CohortList in CellCohorts)
            {
                foreach (Cohort c in CohortList)
                {
                    TIBiomass = new double[2];
                    TIBiomass[0] = c.TrophicIndex;
                    TIBiomass[1] = (c.IndividualBodyMass + c.IndividualReproductivePotentialMass) * c.CohortAbundance;
                    TrophicIndexBiomassDistribution.Add(TIBiomass);
                }
            }

            TrophicIndexBiomassDistribution = TrophicIndexBiomassDistribution.OrderBy(x => x[0]).ToList();


            //Use the Mouillot Evenness index - Functional Regularity Index or FRO
            //From Mouillot et al (2005) Functional regularity: a neglected aspect of functional diversity, Oecologia

            EW = new double[TrophicIndexBiomassDistribution.Count];
            double TotalEW = 0.0 ;

            for (int ii = 0; ii < TrophicIndexBiomassDistribution.Count-1; ii++)
            {
                EW[ii] = (TrophicIndexBiomassDistribution[ii + 1][0] - TrophicIndexBiomassDistribution[ii][0]) / (TrophicIndexBiomassDistribution[ii + 1][1] + TrophicIndexBiomassDistribution[ii][1]);
                TotalEW += EW[ii];
            }

            double FRO = 0.0;

            for (int ii = 0; ii < TrophicIndexBiomassDistribution.Count - 1; ii++)
            {
                FRO += Math.Min(EW[ii]/TotalEW,1.0/(TrophicIndexBiomassDistribution.Count-1));
            }

            return FRO;
        }

        /// <summary>
        /// Calculates functional diversity of cohorts in a grid cell as functional richness and functional diveregence (using the Rao Index)
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cohortDefinitions">The functional group definitions for cohorts in the model</param>
        /// <param name="cellIndices">The list of cell indices in the current model simulation</param>
        /// <param name="cellIndex">The index of the current cell within the list of cells to run</param>
        /// <returns>A pair of values representing the functional richness and functional divergence (functional richness currently disabled!)</returns>
        public double[] CalculateFunctionalDiversity(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortDefinitions, 
            List<uint[]> cellIndices, int cellIndex)
        {
            //Get the cohorts for the specified cell
            GridCellCohortHandler CellCohorts = ecosystemModelGrid.GetGridCellCohorts(cellIndices[cellIndex][0], cellIndices[cellIndex][1]);

            //Variable to hold the functional richness value for the current cohorts
            double FunctionalRichness;
            //Variable to hold the functional divergence value for the current cohorts
            double RaoFunctionalDivergence = 0.0;
            double[,] Distances= new double[CellCohorts.GetNumberOfCohorts(), CellCohorts.GetNumberOfCohorts()];

            List<string> AllTraitNames = cohortDefinitions.GetAllTraitNames().ToList();

            AllTraitNames.Remove("realm");
            AllTraitNames.Remove("heterotroph/autotroph");
            AllTraitNames.Remove("diet");
            string[] TraitNames = AllTraitNames.ToArray();


            //Define upper and lower limits for body mass
            double MinMass = cohortDefinitions.GetBiologicalPropertyAllFunctionalGroups("minimum mass").Min();
            double MaxMass = cohortDefinitions.GetBiologicalPropertyAllFunctionalGroups("maximum mass").Max();
            //Define upp and lower limits for trophic index
            double MaxTI = 40.0;
            double MinTI = 1.0;

            // Construct an array of functional trait values for each cohort
            // Rows are specific cohorts
            // Columns are the functional traits (these include different types:
            //      quantative: current mass, trophic index
            //      nominal: diet, reproductive strategy, mobility, metabolism
            Tuple<double[], string[]>[] CohortFunctionalTraits = new Tuple<double[], string[]>[CellCohorts.GetNumberOfCohorts()];
            double[] IndividualBodyMasses = new double[CellCohorts.GetNumberOfCohorts()];
            double[] TrophicIndex = new double[CellCohorts.GetNumberOfCohorts()];
            string[][] CohortNominalTraitValues= new string[TraitNames.Length][];

            for (int i = 0; i < TraitNames.Length; i++)
			{
			    CohortNominalTraitValues[i] = new string[CellCohorts.GetNumberOfCohorts()];
			}

            // Construct a vector of cohort biomass (in case we want to weight by them)
            double[] CohortTotalBiomasses = new double[CellCohorts.GetNumberOfCohorts()];

            
            string[] TraitValues = new string[TraitNames.Length];
            double[] QuantitativeTraitValues= new double[2];
            int CohortNumberCounter = 0;
            for (int fg = 0; fg < CellCohorts.Count; fg++)
			{
                foreach (Cohort c in CellCohorts[fg])
                {
                    TraitValues = cohortDefinitions.GetTraitValues(TraitNames, fg);
                    for (int ii = 0; ii < TraitValues.Length; ii++)
                    {
			            CohortNominalTraitValues[ii][CohortNumberCounter] = TraitValues[ii];
                    }


                    IndividualBodyMasses[CohortNumberCounter] = c.IndividualBodyMass;
                    TrophicIndex[CohortNumberCounter] = c.TrophicIndex;
 
                    QuantitativeTraitValues[0] = c.IndividualBodyMass;
                    QuantitativeTraitValues[1] = c.TrophicIndex;

                    CohortFunctionalTraits[CohortNumberCounter] = new Tuple<double[], string[]>(QuantitativeTraitValues, TraitValues);
                    
                    CohortTotalBiomasses[CohortNumberCounter] = (c.IndividualBodyMass + c.IndividualReproductivePotentialMass) * c.CohortAbundance;
                    
                    CohortNumberCounter++;
                }
            }
            
            List<double[,]> DistanceList = new List<double[,]>();

            DistanceList.Add(CalculateDistanceMatrix(IndividualBodyMasses, MaxMass, MinMass));
            DistanceList.Add(CalculateDistanceMatrix(TrophicIndex, MaxTI, MinTI));
            foreach (string[] t in CohortNominalTraitValues)
            {
                DistanceList.Add(CalculateDistanceMatrix(t));
            }

            Distances = CalculateAggregateDistance(DistanceList);

            RaoFunctionalDivergence = RaoEntropy(Distances, CohortTotalBiomasses);

            return new double[] {0.0,RaoFunctionalDivergence};
            

        }

        private double[,] CalculateDistanceMatrix(double[] continuousTrait, double traitMaxVal, double traitMinVal)
        {
            double[,] D = new double[continuousTrait.Length, continuousTrait.Length];
            double Range = traitMaxVal - traitMinVal;

            for (int ii = 0; ii < continuousTrait.Length; ii++)
            {
                for (int jj = ii; jj < continuousTrait.Length; jj++)
                {
                    D[ii, jj] = Math.Abs(continuousTrait[ii] - continuousTrait[jj]) / Range;
                    D[jj, ii] = D[ii, jj];
                }
            }

            return D;
        }

        private double[,] CalculateDistanceMatrix(string[] nominalTrait)
        {
            int NumberOfCohorts = nominalTrait.Length;
            double[,] D = new double[NumberOfCohorts, NumberOfCohorts];

            for (int ii = 0; ii < NumberOfCohorts; ii++)
            {
                for (int jj = ii; jj < NumberOfCohorts; jj++)
                {
                    if (nominalTrait[ii] == nominalTrait[jj])
                    {
                        D[ii, jj] = 1.0;
                        D[jj, ii] = 1.0;
                    }
                    else
                    {
                        D[ii, jj] = 0.0;
                        D[jj, ii] = 0.0;
                    }
                }
            }

            return D;
        }


        private double[,] CalculateAggregateDistance(List<double[,]> distanceList)
        {
            int NumberOfTraits = distanceList.Count();
            int NumberOfCohorts = distanceList[0].GetLength(0);
            double[,] D = new double[NumberOfCohorts, NumberOfCohorts];

            foreach (double[,] d in distanceList)
            {
                for (int ii = 0; ii < d.GetLength(0); ii++)
                {
                    for (int jj = ii; jj < d.GetLength(1); jj++)
                    {
                        D[ii, jj] += d[ii, jj] / NumberOfTraits;
                        D[jj, ii] = D[ii, jj];
                    }
                }
            }

            return D;
        }

        private double RaoEntropy(double[,] d, double[] b)
        {
            double TotalB = 0.0;
            double R = 0.0;

            for (int ii = 0; ii < b.Length; ii++)
            {
                TotalB += b[ii];
            }

            for (int ii = 0; ii < d.GetLength(0)-1; ii++)
            {
                for (int jj = 1; jj < d.GetLength(0); jj++)
                {
                    R += d[ii,jj]*b[ii]*b[jj]/(TotalB*TotalB);
                }   
            }

            return R;

        }

    }
}
