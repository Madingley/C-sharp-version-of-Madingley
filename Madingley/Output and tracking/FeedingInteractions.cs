using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;


namespace Madingley
{
    public class FeedingInteractions
    {

        List<Tuple<int, int, double, double>>[,] FeedingInteractionsMatrixPredation;
        List<Tuple<int, int, double, double>>[,] FeedingInteractionsMatrixHerbivory;

        /// <summary>
        /// Dataset to output the Massflows data
        /// </summary>
        private DataSet _FeedingInteractionsDS;

        /// <summary>
        /// An instance of the class to convert data between arrays and SDS objects
        /// </summary>
        private ArraySDSConvert DataConverter;

        /// <summary>
        /// Instance of the class to create SDS objects
        /// </summary>
        private CreateSDSObject SDSCreator;

        private string _OutputFileSuffix;
        private string _OutputPath;
        private string _Filename;
        private double _MV;


        /// <summary>
        /// A streamwriter instance for outputting data on interactions between cohorts
        /// </summary>
        private StreamWriter InteractionWriter;
        /// <summary>
        /// A synchronized version of the streamwriter for outuputting data on the interactions between cohorts
        /// </summary>
        private TextWriter SyncInteractionWriter;

        public FeedingInteractions(
            string filename,
            double missingValue,
            string outputFileSuffix,
            string outputPath,
            int cellIndex)
        {

            _Filename = filename;
            _OutputFileSuffix = outputFileSuffix;
            _OutputPath = outputPath;
            _MV = missingValue;


            InteractionWriter = new StreamWriter(outputPath + "Interactions" + outputFileSuffix + "_Cell" + cellIndex.ToString() + ".txt");
            // Create a threadsafe textwriter to write outputs to the Maturity stream
            SyncInteractionWriter = TextWriter.Synchronized(InteractionWriter);
            SyncInteractionWriter.WriteLine("time_step\tPred_ID\tPrey_ID\tBiomass_Assimilated\tBiomassIngested");
        }

        /// <summary>
        /// Initialises the feeding interactions matrix each timestep
        /// </summary>
        public void InitialiseInteractionsMatrix(GridCellCohortHandler gridCellCohorts)
        {


            int MaxCohorts = 0;
            foreach (var CohortList in gridCellCohorts)
            {
                if (CohortList.Count > MaxCohorts) MaxCohorts = CohortList.Count;
            }

            FeedingInteractionsMatrixPredation = new List<Tuple<int, int, double, double>>[gridCellCohorts.Count, MaxCohorts];
            FeedingInteractionsMatrixHerbivory = new List<Tuple<int, int, double, double>>[gridCellCohorts.Count, MaxCohorts];

            for (int i = 0; i < gridCellCohorts.Count; i++)
            {
                for (int c = 0; c < MaxCohorts; c++)
                {
                    FeedingInteractionsMatrixPredation[i, c] = new List<Tuple<int, int, double, double>>();
                    FeedingInteractionsMatrixHerbivory[i, c] = new List<Tuple<int, int, double, double>>();
                }
            }


        }

        /// <summary>
        /// Record a cohort specific feeding interaction
        /// </summary>
        public void RecordFeedingInteraction(string feedingMode, int actingFG, int actingC, int preyFG, int preyI, double biomassAssimilated, double biomassIngested)
        {
            if (feedingMode == "predation")
            {
                FeedingInteractionsMatrixPredation[actingFG, actingC].Add(new Tuple<int, int, double, double>(preyFG, preyI, biomassAssimilated, biomassIngested));
            }
            else
            {
                FeedingInteractionsMatrixHerbivory[actingFG, actingC].Add(new Tuple<int, int, double, double>(preyFG, preyI, biomassAssimilated, biomassIngested));
            }


        }

        /// <summary>
        /// Record a cohort specific feeding interaction
        /// </summary>
        public void RecordFeedingInteraction(string feedingMode, uint timestep, long actingID, long preyID, double biomassAssimilated, double biomassIngested)
        {
            string newline = Convert.ToString(timestep) + "\t" +
                    Convert.ToString(actingID) + "\t";

            if (feedingMode == "predation")
            {
                newline +=  Convert.ToString(preyID) + "\t" +
                                 Convert.ToString(biomassAssimilated) + "\t" +
                                 Convert.ToString(biomassIngested);

            }
            else
            {
                newline += "S" + Convert.ToString(preyID) + "\t" +
                                 Convert.ToString(biomassAssimilated) + "\t" +
                                 Convert.ToString(biomassIngested);
            }

            SyncInteractionWriter.WriteLine(newline);

        }



        public void WriteFeedingInteractions(uint timestep)
        {
            int nrows = FeedingInteractionsMatrixPredation.GetLength(0);
            int ncols = FeedingInteractionsMatrixPredation.GetLength(1);

            int MaxLengthPredation = 0;

            for (int i = 0; i < nrows; i++)
            {
                for (int j = 0; j < ncols; j++)
                {
                    if (FeedingInteractionsMatrixPredation[i, j].Count > MaxLengthPredation) MaxLengthPredation = FeedingInteractionsMatrixPredation[i, j].Count;
                }
            }

            double[, ,] FGIndicesPredation = new double[nrows, ncols, MaxLengthPredation];
            double[, ,] FGIndicesHerbivory = new double[nrows, ncols, 2];

            double[, ,] CIndicesPredation = new double[nrows, ncols, MaxLengthPredation];
            double[, ,] CIndicesHerbivory = new double[nrows, ncols, 2];

            double[, ,] BiomassAssimilatedPredation = new double[nrows, ncols, MaxLengthPredation];
            double[, ,] BiomassAssimilatedHerbivory = new double[nrows, ncols, 2];

            double[, ,] BiomassIngestedPredation = new double[nrows, ncols, MaxLengthPredation];
            double[, ,] BiomassIngestedHerbivory = new double[nrows, ncols, 2];


            float[] FGIndices = new float[nrows];
            float[] CIndices = new float[ncols];
            float[] PredationIndices = new float[MaxLengthPredation];
            float[] HerbivoryIndices = new float[2] {0,1};

            for (int i = 0; i < nrows; i++)
            {
                FGIndices[i] = i;
            }

            for (int i = 0; i < ncols; i++)
            {
                CIndices[i] = i;
            }

            for (int i = 0; i < MaxLengthPredation; i++)
            {
                PredationIndices[i] = i;
            }

            for (int f = 0; f < nrows; f++)
            {
                for (int c = 0; c < ncols; c++)
                {

                    //Populate output data matrices for predation feeding events
                    for (int i = 0; i < FeedingInteractionsMatrixPredation[f, c].Count; i++)
                    {
                        FGIndicesPredation[f, c, i] = Convert.ToDouble(FeedingInteractionsMatrixPredation[f, c].ElementAt(i).Item1);
                        CIndicesPredation[f, c, i] = Convert.ToDouble(FeedingInteractionsMatrixPredation[f, c].ElementAt(i).Item2);
                        BiomassAssimilatedPredation[f, c, i] = (FeedingInteractionsMatrixPredation[f, c].ElementAt(i).Item3);
                        BiomassIngestedPredation[f, c, i] = (FeedingInteractionsMatrixPredation[f, c].ElementAt(i).Item4);
                    }

                    //Populate output data matrices for herbivory feeding events
                    for (int i = 0; i < FeedingInteractionsMatrixHerbivory[f,c].Count; i++)
                    {
                        FGIndicesHerbivory[f, c, i] = Convert.ToDouble(FeedingInteractionsMatrixHerbivory[f, c].ElementAt(i).Item1);
                        CIndicesHerbivory[f, c, i] = Convert.ToDouble(FeedingInteractionsMatrixHerbivory[f, c].ElementAt(i).Item2);
                        BiomassAssimilatedHerbivory[f, c, i] = (FeedingInteractionsMatrixHerbivory[f, c].ElementAt(i).Item3);
                        BiomassIngestedHerbivory[f, c, i] = (FeedingInteractionsMatrixHerbivory[f, c].ElementAt(i).Item4);

                    }
                }
            }




            // Initialise the data converter
            DataConverter = new ArraySDSConvert();

            // Initialise the SDS object creator
            SDSCreator = new CreateSDSObject();

            // Create an SDS object to hold the predation tracker data
            _FeedingInteractionsDS = SDSCreator.CreateSDS("netCDF", _Filename + _OutputFileSuffix + timestep, _OutputPath);

            string[] PredationDimensions = new string[3] {"Functional Group Indices","Cohort Indices","Predation Interaction Dimension"};
            string[] HerbivoryDimensions = new string[3] { "Functional Group Indices", "Cohort Indices", "Herbivory Interaction Dimension" };

            string[] PredD1 = new string [1] { PredationDimensions[0] };
            string[] PredD2 = new string[1] { PredationDimensions[1] };
            string[] PredD3 = new string[1] { PredationDimensions[2] };
            string[] HerbD3 = new string[1] { HerbivoryDimensions[2] };

            DataConverter.AddVariable(_FeedingInteractionsDS, PredationDimensions[0], "index", 1, PredD1, _MV, FGIndices);
            DataConverter.AddVariable(_FeedingInteractionsDS, PredationDimensions[1], "index", 1, PredD2, _MV, CIndices);
            DataConverter.AddVariable(_FeedingInteractionsDS, PredationDimensions[2], "index", 1, PredD3, _MV, PredationIndices);
            DataConverter.AddVariable(_FeedingInteractionsDS, HerbivoryDimensions[2], "index", 1, HerbD3, _MV, HerbivoryIndices);

            /*DataConverter.AddVariable(_FeedingInteractionsDS, "Predation Interactions Functional Groups", 3, PredationDimensions, _MV, FGIndices, CIndices, PredationIndices);
            DataConverter.AddVariable(_FeedingInteractionsDS, "Predation Interactions Cohort Index", 3, PredationDimensions, _MV, FGIndices, CIndices, PredationIndices);
            DataConverter.AddVariable(_FeedingInteractionsDS, "Predation Interactions Biomass Assimilated", 3, PredationDimensions, _MV, FGIndices, CIndices, PredationIndices);
            DataConverter.AddVariable(_FeedingInteractionsDS, "Predation Interactions Biomass Ingested", 3, PredationDimensions, _MV, FGIndices, CIndices, PredationIndices);

            DataConverter.AddVariable(_FeedingInteractionsDS, "Herbivory Interactions Functional Groups", 3, HerbivoryDimensions, _MV, FGIndices, CIndices, HerbivoryIndices);
            DataConverter.AddVariable(_FeedingInteractionsDS, "Herbivory Interactions Stock Index", 3, HerbivoryDimensions, _MV, FGIndices, CIndices, HerbivoryIndices);
            DataConverter.AddVariable(_FeedingInteractionsDS, "Herbivory Interactions Biomass Assimilated", 3, HerbivoryDimensions, _MV, FGIndices, CIndices, HerbivoryIndices);
            DataConverter.AddVariable(_FeedingInteractionsDS, "Herbivory Interactions Biomass Ingested", 3, HerbivoryDimensions, _MV, FGIndices, CIndices, HerbivoryIndices);*/

            //Add variable to SDS
            var FGIPredOut = _FeedingInteractionsDS.AddVariable<double>("Predation Interactions Functional Groups", FGIndicesPredation, PredationDimensions);
            FGIPredOut.Metadata["DisplayName"] = "Predation Interactions Functional Groups";
            FGIPredOut.Metadata["MissingValue"] = _MV;

            //Add variable to SDS
            var CIPredOut = _FeedingInteractionsDS.AddVariable<double>("Predation Interactions Cohort Index", CIndicesPredation, PredationDimensions);
            CIPredOut.Metadata["DisplayName"] = "Predation Interactions Cohort Index";
            CIPredOut.Metadata["MissingValue"] = _MV;

            //Add variable to SDS
            var BAPredOut = _FeedingInteractionsDS.AddVariable<double>("Predation Interactions Biomass Assimilated", BiomassAssimilatedPredation, PredationDimensions);
            BAPredOut.Metadata["DisplayName"] = "Predation Interactions Biomass Assimilated";
            BAPredOut.Metadata["MissingValue"] = _MV;

            //Add variable to SDS
            var BIPredOut = _FeedingInteractionsDS.AddVariable<double>("Predation Interactions Biomass Ingested", BiomassIngestedPredation, PredationDimensions);
            BIPredOut.Metadata["DisplayName"] = "Predation Interactions Biomass Ingested";
            BIPredOut.Metadata["MissingValue"] = _MV;


            //Add variable to SDS
            var FGIHerbOut = _FeedingInteractionsDS.AddVariable<double>("Herbivory Interactions Functional Groups", FGIndicesHerbivory, HerbivoryDimensions);
            FGIHerbOut.Metadata["DisplayName"] = "Herbivory Interactions Functional Groups";
            FGIHerbOut.Metadata["MissingValue"] = _MV;

            //Add variable to SDS
            var CIHerbOut = _FeedingInteractionsDS.AddVariable<double>("Herbivory Interactions Cohort Index", CIndicesHerbivory, HerbivoryDimensions);
            CIHerbOut.Metadata["DisplayName"] = "Herbivory Interactions Cohort Index";
            CIHerbOut.Metadata["MissingValue"] = _MV;

            //Add variable to SDS
            var BAHerbOut = _FeedingInteractionsDS.AddVariable<double>("Herbivory Interactions Biomass Assimilated", BiomassAssimilatedHerbivory, HerbivoryDimensions);
            BAHerbOut.Metadata["DisplayName"] = "Herbivory Interactions Biomass Assimilated";
            BAHerbOut.Metadata["MissingValue"] = _MV;

            //Add variable to SDS
            var BIHerbOut = _FeedingInteractionsDS.AddVariable<double>("Herbivory Interactions Biomass Ingested", BiomassIngestedHerbivory, HerbivoryDimensions);
            BIHerbOut.Metadata["DisplayName"] = "Herbivory Interactions Biomass Ingested";
            BIHerbOut.Metadata["MissingValue"] = _MV;


            //Commit changes
            _FeedingInteractionsDS.Commit();

        }


        /// <summary>
        /// Close the output streams for the reproduction tracker
        /// </summary>
        public void CloseStreams()
        {
            SyncInteractionWriter.Close();
            InteractionWriter.Close();
        }

    }
}
