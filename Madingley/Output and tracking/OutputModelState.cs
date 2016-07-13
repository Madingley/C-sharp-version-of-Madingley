using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

namespace Madingley
{
    public class OutputModelState
    {
        /// <summary>
        /// The path to the output folder
        /// </summary>
        private string _OutputPath;
        /// <summary>
        /// Get the path to the output folder
        /// </summary>
        public string OutputPath { get { return _OutputPath; } }

        /// <summary>
        /// Dataset object to handle grid-based outputs
        /// </summary>
        private DataSet StateOutput;

        /// <summary>
        /// An instance of the class to convert data between arrays and SDS objects
        /// </summary>
        private ArraySDSConvert DataConverter;

        /// <summary>
        /// Instance of the class to create SDS objects
        /// </summary>
        private CreateSDSObject SDSCreator;

        /// <summary>
        /// A streamwriter instance for outputting data on interactions between cohorts
        /// </summary>
        private StreamWriter StateWriter;
        /// <summary>
        /// A synchronized version of the streamwriter for outuputting data on the interactions between cohorts
        /// </summary>
        private TextWriter SyncStateWriter;

        private int Simulation;

        public OutputModelState(MadingleyModelInitialisation modelInitialisation, string suffix, int simulation)
        {
            //Initialise output path and variables
            // Set the output path
            _OutputPath = modelInitialisation.OutputPath;

            // Initialise the data converter
            DataConverter = new ArraySDSConvert();

            // Initialise the SDS object creator
            SDSCreator = new CreateSDSObject();

            StateWriter = new StreamWriter(_OutputPath + "State" + suffix + simulation.ToString() + ".txt");
            // Create a threadsafe textwriter to write outputs to the Maturity stream
            SyncStateWriter = TextWriter.Synchronized(StateWriter);
            SyncStateWriter.WriteLine("TimeStep\tLatitude\tLongitude\tID" +
            "\tFunctionalGroup\tJuvenileMass\tAdultMass\tIndividualBodyMass\tCohortAbundance\tBirthTimeStep" +
                "\tMaturityTimeStep\tLogOptimalPreyBodySizeRatio\tMaximumAchievedBodyMass\tTrophicIndex\tProportionTimeActive");

            Simulation = simulation;

        }


        public void OutputCurrentModelState(ModelGrid currentModelGrid, List<uint[]> cellIndices, uint currentTimestep)
        {

            GridCellCohortHandler TempCohorts;
            GridCellStockHandler TempStocks;

            string context;
            string organism;

            context = Convert.ToString(currentTimestep) + "\t";

            foreach (uint[] cell in cellIndices)
            {
                context = Convert.ToString(currentTimestep) + "\t" +
                        Convert.ToString(currentModelGrid.GetCellLatitude(cell[0])) + "\t" +
                            Convert.ToString(currentModelGrid.GetCellLongitude(cell[1])) + "\t";

                TempStocks = currentModelGrid.GetGridCellStocks(cell[0], cell[1]);
                TempCohorts = currentModelGrid.GetGridCellCohorts(cell[0], cell[1]);

                foreach (List<Stock> ListS in TempStocks)
                {
                    foreach (Stock S in ListS)
                    {
                        organism = "-999\tS" + Convert.ToString(S.FunctionalGroupIndex) + "\t" +
                                    "-999\t-999\t" + Convert.ToString(S.IndividualBodyMass) + "\t" +
                                    Convert.ToString(S.TotalBiomass) + "\t" +
                                    "-999\t-999\t-999\t-999\t-999\t-999";
                        SyncStateWriter.WriteLine(context + organism);
                    }
                }

                foreach (List<Cohort> ListC in TempCohorts)
                {
                    foreach (Cohort C in ListC)
                    {
                        organism = Convert.ToString(C.CohortID) + "\t" +
                            Convert.ToString(C.FunctionalGroupIndex) + "\t" +
                                    Convert.ToString(C.JuvenileMass) + "\t" +
                                    Convert.ToString(C.AdultMass) + "\t" +
                                    Convert.ToString(C.IndividualBodyMass) + "\t" +
                                    Convert.ToString(C.CohortAbundance) + "\t" +
                                    Convert.ToString(C.BirthTimeStep) + "\t" +
                                    Convert.ToString(C.MaturityTimeStep) + "\t" +
                                    Convert.ToString(C.LogOptimalPreyBodySizeRatio) + "\t" +
                                    Convert.ToString(C.MaximumAchievedBodyMass) + "\t" +
                                    Convert.ToString(C.TrophicIndex) + "\t" +
                                    Convert.ToString(C.ProportionTimeActive);

                        SyncStateWriter.WriteLine(context + organism);

                    }
                }

            }

        }


        public void OutputCurrentModelState(ModelGrid currentModelGrid,FunctionalGroupDefinitions functionalGroupHandler, List<uint[]> cellIndices, uint currentTimestep, int maximumNumberOfCohorts,string filename)
        {
            
            float[] Latitude = currentModelGrid.Lats;

            float[] Longitude = currentModelGrid.Lons;
            
            float[] CohortFunctionalGroup = new float[functionalGroupHandler.GetNumberOfFunctionalGroups()];
            for (int fg = 0; fg < CohortFunctionalGroup.Length; fg++)
            {
                CohortFunctionalGroup[fg] = fg;
            }

            int CellCohortNumber = 0;
            GridCellCohortHandler CellCohorts;
            for (int cellIndex = 0; cellIndex < cellIndices.Count; cellIndex++)
            {
                CellCohorts = currentModelGrid.GetGridCellCohorts(cellIndices[cellIndex][0], cellIndices[cellIndex][1]);
                for (int i = 0; i < CellCohorts.Count; i++)
                {
                    if (CellCohorts[i].Count > CellCohortNumber) CellCohortNumber = CellCohorts[i].Count;
                }
            }

            int MaxNumberCohorts = Math.Min(CellCohortNumber, maximumNumberOfCohorts);

            float[] Cohort = new float[MaxNumberCohorts];
            for (int c = 0; c < Cohort.Length; c++)
            {
                Cohort[c] = c;
            }

            //Define an array for stock functional group - there are only three currently
            float[] StockFunctionalGroup = new float[] { 1, 2, 3 };

            //Define an array for index of stocks - there is only one currently
            float[] Stock = new float[] { 1};

            string Filename = filename + "_" + currentTimestep.ToString() + Simulation.ToString() ;

            StateOutput = SDSCreator.CreateSDS("netCDF", Filename, _OutputPath);

            //Define the cohort properties for output
            string[] CohortProperties = new string[]
            {"JuvenileMass", "AdultMass", "IndividualBodyMass", "CohortAbundance",
             "BirthTimeStep", "MaturityTimeStep", "LogOptimalPreyBodySizeRatio",
             "MaximumAchievedBodyMass","Merged","TrophicIndex","ProportionTimeActive"};

            //define the dimensions for cohort outputs
            string[] dims = new string[] { "Latitude", "Longitude", "Cohort Functional Group", "Cohort" };

            // Add the variables for each cohort property
            // Then calculate the state for this property and put the data to this variable
            foreach (string v in CohortProperties)
            {
                DataConverter.AddVariable(StateOutput,"Cohort" + v,4,
                dims,currentModelGrid.GlobalMissingValue,Latitude,
                Longitude,CohortFunctionalGroup,Cohort);

                StateOutput.PutData<float[,,,]>("Cohort" + v,
                    CalculateCurrentCohortState(currentModelGrid,v,Latitude.Length,Longitude.Length,CohortFunctionalGroup.Length,Cohort.Length,cellIndices));

               StateOutput.Commit();
            }

            //Define the stock properties for output
            string[] StockProperties = new string[] { "IndividualBodyMass", "TotalBiomass"};


            //define the dimensions for cohort outputs
            dims = new string[] { "Latitude", "Longitude", "Stock Functional Group", "Stock" };

            // Add the variables for each stock property
            // Then calculate the state for this property and put the data to this variable
            foreach (string v in StockProperties)
            {
                DataConverter.AddVariable(StateOutput,"Stock" + v, 4,
                dims, currentModelGrid.GlobalMissingValue, Latitude,
                Longitude, StockFunctionalGroup, Stock);

                StateOutput.PutData<float[, , ,]>("Stock" + v,
                    CalculateCurrentStockState(currentModelGrid, v, Latitude.Length, Longitude.Length, StockFunctionalGroup.Length, Stock.Length, cellIndices));

                StateOutput.Commit();
            }

            //Close this data set
            StateOutput.Dispose();
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentModelState"></param>
        /// <param name="variableName"></param>
        /// <param name="numLats"></param>
        /// <param name="numLons"></param>
        /// <param name="numFG"></param>
        /// <param name="numCohorts"></param>
        /// <param name="cellList"></param>
        /// <returns>float to reduce memory associated with the outputting of global states</returns>
        private float[, , ,] CalculateCurrentCohortState(ModelGrid currentModelState, string variableName, int numLats, int numLons, int numFG, int numCohorts, List<uint[]> cellList)
        {
            //Calculate the cohort state
            float[, , ,] State = new float[numLats, numLons, numFG, numCohorts];
            GridCellCohortHandler CellCohorts;

            for (int cellIndex = 0; cellIndex < cellList.Count; cellIndex++)
            {

                CellCohorts = currentModelState.GetGridCellCohorts(cellList[cellIndex][0], cellList[cellIndex][1]);

                for (int functionalGroupIndex = 0; functionalGroupIndex < CellCohorts.Count; functionalGroupIndex++)
                {
                    for (int cohortIndex = 0; cohortIndex < CellCohorts[functionalGroupIndex].Count; cohortIndex++)
                    {
                        switch (variableName)
                        {
                            case "JuvenileMass":
                                State[cellList[cellIndex][0], cellList[cellIndex][1], functionalGroupIndex, cohortIndex] = (float)CellCohorts[functionalGroupIndex][cohortIndex].JuvenileMass;
                                break;
                            case "AdultMass":
                                State[cellList[cellIndex][0], cellList[cellIndex][1], functionalGroupIndex, cohortIndex] = (float)CellCohorts[functionalGroupIndex][cohortIndex].AdultMass;
                                break;
                            case "IndividualBodyMass":
                                State[cellList[cellIndex][0], cellList[cellIndex][1], functionalGroupIndex, cohortIndex] = (float)CellCohorts[functionalGroupIndex][cohortIndex].IndividualBodyMass;
                                break;
                            case "CohortAbundance":
                                State[cellList[cellIndex][0], cellList[cellIndex][1], functionalGroupIndex, cohortIndex] = (float)CellCohorts[functionalGroupIndex][cohortIndex].CohortAbundance;
                                break;
                            case "BirthTimeStep":
                                State[cellList[cellIndex][0], cellList[cellIndex][1], functionalGroupIndex, cohortIndex] = (float)CellCohorts[functionalGroupIndex][cohortIndex].BirthTimeStep;
                                break;
                            case "MaturityTimeStep":
                                State[cellList[cellIndex][0], cellList[cellIndex][1], functionalGroupIndex, cohortIndex] = (float)CellCohorts[functionalGroupIndex][cohortIndex].MaturityTimeStep;
                                break;
                            case "LogOptimalPreyBodySizeRatio":
                                State[cellList[cellIndex][0], cellList[cellIndex][1], functionalGroupIndex, cohortIndex] = (float)CellCohorts[functionalGroupIndex][cohortIndex].LogOptimalPreyBodySizeRatio;
                                break;
                            case "MaximumAchievedBodyMass":
                                State[cellList[cellIndex][0], cellList[cellIndex][1], functionalGroupIndex, cohortIndex] = (float)CellCohorts[functionalGroupIndex][cohortIndex].MaximumAchievedBodyMass;
                                break;
                            case "Merged":
                                State[cellList[cellIndex][0], cellList[cellIndex][1], functionalGroupIndex, cohortIndex] = Convert.ToSingle(CellCohorts[functionalGroupIndex][cohortIndex].Merged);
                                break;
                            case "TrophicIndex":
                                State[cellList[cellIndex][0], cellList[cellIndex][1], functionalGroupIndex, cohortIndex] = (float)CellCohorts[functionalGroupIndex][cohortIndex].TrophicIndex;
                                break;
                            case "ProportionTimeActive":
                                State[cellList[cellIndex][0], cellList[cellIndex][1], functionalGroupIndex, cohortIndex] = (float)CellCohorts[functionalGroupIndex][cohortIndex].ProportionTimeActive;
                                break;
                        }
                    }

                }
            }

            return State;
        }

        private float[, , ,] CalculateCurrentStockState(ModelGrid currentModelState, string variableName, int numLats, int numLons, int numFG, int numStocks, List<uint[]> cellList)
        {
            //Calculate the cohort state
            float[, , ,] State = new float[numLats, numLons, numFG, numStocks];
            GridCellStockHandler CellStocks;

            for (int cellIndex = 0; cellIndex < cellList.Count; cellIndex++)
            {

                CellStocks = currentModelState.GetGridCellStocks(cellList[cellIndex][0], cellList[cellIndex][1]);

                for (int functionalGroupIndex = 0; functionalGroupIndex < CellStocks.Count; functionalGroupIndex++)
                {
                    for (int stockIndex = 0; stockIndex < CellStocks[functionalGroupIndex].Count; stockIndex++)
                    {
                        switch (variableName)
                        {
                            case "IndividualBodyMass":
                                State[cellList[cellIndex][0], cellList[cellIndex][1], functionalGroupIndex, stockIndex] = (float)CellStocks[functionalGroupIndex][stockIndex].IndividualBodyMass;
                                break;
                            case "TotalBiomass":
                                State[cellList[cellIndex][0], cellList[cellIndex][1], functionalGroupIndex, stockIndex] = (float)CellStocks[functionalGroupIndex][stockIndex].TotalBiomass;
                                break;
                        }
                    }

                }
            }

            return State;
        }

        public void CloseStreams()
        {
            StateWriter.Close();
            SyncStateWriter.Close();
        }


    }
}
