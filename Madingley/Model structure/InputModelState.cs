using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

namespace Madingley
{
    public class InputModelState
    {

        /// <summary>
        /// The handler for the cohorts in this grid cell
        /// </summary>
        private GridCellCohortHandler[,] _GridCellCohorts;
        /// <summary>
        /// Get or set the cohorts in this grid cell
        /// </summary>
        public GridCellCohortHandler[,] GridCellCohorts
        {
            get { return _GridCellCohorts; }
            set { _GridCellCohorts = value; }
        }

        /// <summary>
        /// The handler for the stocks in this grid cell
        /// </summary>
        private GridCellStockHandler[,] _GridCellStocks;
        /// <summary>
        /// Get or set the stocks in this grid cell
        /// </summary>
        public GridCellStockHandler[,] GridCellStocks
        {
            get { return _GridCellStocks; }
            set { _GridCellStocks = value; }
        }

        private Boolean _InputState = false;

        public Boolean InputState
        {
            get { return _InputState; }
            set { _InputState = value; }
        }
        
        /// <summary>
        /// A streamwriter instance for outputting data on interactions between cohorts
        /// </summary>
        private StreamReader StateReader;
        /// <summary>
        /// A synchronized version of the streamwriter for outuputting data on the interactions between cohorts
        /// </summary>
        private TextReader SyncStateReader;


        public void InputModelStateNCDF(string inputPath, string filename, ModelGrid ecosystemModelGrid, List<uint[]> cellList, bool tracking)
        {

            //Set the input state flag to be true
            _InputState = true;
                                    
            // Construct the string required to access the file using Scientific Dataset
            string _ReadFileString = "msds:nc?file=" + inputPath + filename +".nc&openMode=readOnly";

            // Open the data file using Scientific Dataset
            DataSet StateDataSet = DataSet.Open(_ReadFileString);


            float[] Latitude = StateDataSet.GetData<float[]>("Latitude");
            float[] Longitude = StateDataSet.GetData<float[]>("Longitude");
            float[] CohortFunctionalGroup = StateDataSet.GetData<float[]>("Cohort Functional Group");
            float[] Cohort = StateDataSet.GetData<float[]>("Cohort");

            float[] StockFunctionalGroup = StateDataSet.GetData<float[]>("Stock Functional Group");
            float[] Stock = StateDataSet.GetData<float[]>("Stock");


            // Check that the longitudes and latitudes in the input state match the cell environment
            for (int la = 0; la < Latitude.Length; la++)
            {
                Debug.Assert(ecosystemModelGrid.GetCellEnvironment((uint)la, 0)["Latitude"][0] == Latitude[la],
                    "Error: input-state grid doesn't match current model grid");
            }
            for (int lo = 0; lo < Longitude.Length; lo++)
            {
                Debug.Assert(ecosystemModelGrid.GetCellEnvironment(0, (uint)lo)["Longitude"][0] == Longitude[lo],
                    "Error: input-state grid doesn't match current model grid");
            }
            
            List<double[,,]> CohortJuvenileMass = new List<double[,,]>();
            List<double[,,]> CohortAdultMass = new List<double[,,]>();
            List<double[,,]> CohortIndividualBodyMass = new List<double[,,]>();
            List<double[,,]> CohortCohortAbundance = new List<double[,,]>();
            List<double[,,]> CohortLogOptimalPreyBodySizeRatio = new List<double[,,]>();
            List<double[,,]> CohortBirthTimeStep = new List<double[,,]>();
            List<double[,,]> CohortProportionTimeActive = new List<double[,,]>();
            List<double[,,]> CohortTrophicIndex = new List<double[,,]>();

            float[,,,] tempData = new float[Latitude.Length, Longitude.Length,
                CohortFunctionalGroup.Length, Cohort.Length];

            tempData = StateDataSet.GetData<float[,,,]>("CohortJuvenileMass");
            
            
            for (int la = 0; la < Latitude.Length; la++)
            {
                CohortJuvenileMass.Add(new double[Longitude.Length, CohortFunctionalGroup.Length, Cohort.Length]);
            }

            foreach (uint[] cell in cellList)
            {
                for (int fg = 0; fg < CohortFunctionalGroup.Length; fg++)
                {
                    for (int c = 0; c < Cohort.Length; c++)
                    {
                        CohortJuvenileMass[(int)cell[0]][cell[1], fg, c] = tempData[
                            cell[0], cell[1], fg, c];
                    }
                }
            }
            
            
            tempData = StateDataSet.GetData<float[,,,]>("CohortAdultMass");

            for (int la = 0; la < Latitude.Length; la++)
            {
                CohortAdultMass.Add(new double[Longitude.Length, CohortFunctionalGroup.Length, Cohort.Length]);
            }

            foreach (uint[] cell in cellList)
            {
                for (int fg = 0; fg < CohortFunctionalGroup.Length; fg++)
                {
                    for (int c = 0; c < Cohort.Length; c++)
                    {
                        CohortAdultMass[(int)cell[0]][cell[1], fg, c] = tempData[
                            cell[0], cell[1], fg, c];
                    }
                }
            }

            tempData = StateDataSet.GetData<float[,,,]>("CohortIndividualBodyMass");

            for (int la = 0; la < Latitude.Length; la++)
            {
                CohortIndividualBodyMass.Add(new double[Longitude.Length, CohortFunctionalGroup.Length, Cohort.Length]);
            }

            foreach (uint[] cell in cellList)
            {
                for (int fg = 0; fg < CohortFunctionalGroup.Length; fg++)
                {
                    for (int c = 0; c < Cohort.Length; c++)
                    {
                        CohortIndividualBodyMass[(int)cell[0]][cell[1], fg, c] = tempData[
                            cell[0], cell[1], fg, c];
                    }
                }
            }

            tempData = StateDataSet.GetData<float[,,,]>("CohortCohortAbundance");

            for (int la = 0; la < Latitude.Length; la++)
            {
                CohortCohortAbundance.Add(new double[Longitude.Length, CohortFunctionalGroup.Length, Cohort.Length]);
            }

            foreach (uint[] cell in cellList)
            {
                for (int fg = 0; fg < CohortFunctionalGroup.Length; fg++)
                {
                    for (int c = 0; c < Cohort.Length; c++)
                    {
                        CohortCohortAbundance[(int)cell[0]][cell[1], fg, c] = tempData[
                            cell[0], cell[1], fg, c];
                    }
                }
            }

            tempData = StateDataSet.GetData<float[,,,]>("CohortLogOptimalPreyBodySizeRatio");

            for (int la = 0; la < Latitude.Length; la++)
            {
                CohortLogOptimalPreyBodySizeRatio.Add(new double[Longitude.Length, CohortFunctionalGroup.Length, Cohort.Length]);
            }

            foreach (uint[] cell in cellList)
            {
                for (int fg = 0; fg < CohortFunctionalGroup.Length; fg++)
                {
                    for (int c = 0; c < Cohort.Length; c++)
                    {
                        CohortLogOptimalPreyBodySizeRatio[(int)cell[0]][cell[1], fg, c] = tempData[
                            cell[0], cell[1], fg, c];
                    }
                }
            }

            tempData = StateDataSet.GetData<float[,,,]>("CohortBirthTimeStep");

            for (int la = 0; la < Latitude.Length; la++)
            {
                CohortBirthTimeStep.Add(new double[Longitude.Length, CohortFunctionalGroup.Length, Cohort.Length]);
            }

            foreach (uint[] cell in cellList)
            {
                for (int fg = 0; fg < CohortFunctionalGroup.Length; fg++)
                {
                    for (int c = 0; c < Cohort.Length; c++)
                    {
                        CohortBirthTimeStep[(int)cell[0]][cell[1], fg, c] = tempData[
                            cell[0], cell[1], fg, c];
                    }
                }
            }

            tempData = StateDataSet.GetData<float[,,,]>("CohortProportionTimeActive");

            for (int la = 0; la < Latitude.Length; la++)
            {
                CohortProportionTimeActive.Add(new double[Longitude.Length, CohortFunctionalGroup.Length, Cohort.Length]);
            }

            foreach (uint[] cell in cellList)
            {
                for (int fg = 0; fg < CohortFunctionalGroup.Length; fg++)
                {
                    for (int c = 0; c < Cohort.Length; c++)
                    {
                        CohortProportionTimeActive[(int)cell[0]][cell[1], fg, c] = tempData[
                            cell[0], cell[1], fg, c];
                    }
                }
            }

            tempData = StateDataSet.GetData<float[,,,]>("CohortTrophicIndex");

            for (int la = 0; la < Latitude.Length; la++)
            {
                CohortTrophicIndex.Add(new double[Longitude.Length, CohortFunctionalGroup.Length, Cohort.Length]);
            }

            foreach (uint[] cell in cellList)
            {
                for (int fg = 0; fg < CohortFunctionalGroup.Length; fg++)
                {
                    for (int c = 0; c < Cohort.Length; c++)
                    {
                        CohortTrophicIndex[(int)cell[0]][cell[1], fg, c] = tempData[
                            cell[0], cell[1], fg, c];
                    }
                }
            }

            _GridCellCohorts = new GridCellCohortHandler[Latitude.Length, Longitude.Length];

            long temp = 0;

            for (int cell = 0; cell < cellList.Count; cell++)
            {
                _GridCellCohorts[cellList[cell][0], cellList[cell][1]] = new GridCellCohortHandler(CohortFunctionalGroup.Length);

                for (int fg = 0; fg < CohortFunctionalGroup.Length; fg++)
                {
                    _GridCellCohorts[cellList[cell][0], cellList[cell][1]][fg] = new List<Cohort>();
                    for (int c = 0; c < Cohort.Length; c++)
                    {
                        if (CohortCohortAbundance[(int)cellList[cell][0]][cellList[cell][1],fg,c] > 0.0)
                        {
                            Cohort TempCohort = new Cohort(
                                (byte)fg,
                                CohortJuvenileMass[(int)cellList[cell][0]][cellList[cell][1], fg, c],
                                CohortAdultMass[(int)cellList[cell][0]][cellList[cell][1], fg, c],
                                CohortIndividualBodyMass[(int)cellList[cell][0]][cellList[cell][1], fg, c],
                                CohortCohortAbundance[(int)cellList[cell][0]][cellList[cell][1], fg, c],
                                Math.Exp(CohortLogOptimalPreyBodySizeRatio[(int)cellList[cell][0]][cellList[cell][1], fg, c]),
                                Convert.ToUInt16(CohortBirthTimeStep[(int)cellList[cell][0]][cellList[cell][1], fg, c]),
                                CohortProportionTimeActive[(int)cellList[cell][0]][cellList[cell][1], fg, c], ref temp,
                                CohortTrophicIndex[(int)cellList[cell][0]][cellList[cell][1], fg, c],
                                tracking);

                            _GridCellCohorts[cellList[cell][0], cellList[cell][1]][fg].Add(TempCohort);
                        }
                    }
                }

            }

            CohortJuvenileMass.RemoveRange(0, CohortJuvenileMass.Count);
            CohortAdultMass.RemoveRange(0, CohortAdultMass.Count);
            CohortIndividualBodyMass.RemoveRange(0, CohortIndividualBodyMass.Count);
            CohortCohortAbundance.RemoveRange(0, CohortCohortAbundance.Count);
            CohortLogOptimalPreyBodySizeRatio.RemoveRange(0, CohortLogOptimalPreyBodySizeRatio.Count);
            CohortBirthTimeStep.RemoveRange(0, CohortBirthTimeStep.Count);
            CohortProportionTimeActive.RemoveRange(0, CohortProportionTimeActive.Count);
            CohortTrophicIndex.RemoveRange(0, CohortTrophicIndex.Count);

            List<double[,,]> StockIndividualBodyMass = new List<double[,,]>();
            List<double[,,]> StockTotalBiomass = new List<double[,,]>();

            float[,,,] tempData2 = new float[Latitude.Length, Longitude.Length,
                StockFunctionalGroup.Length,Stock.Length];

            tempData2 = StateDataSet.GetData<float[,,,]>("StockIndividualBodyMass");

            for (int la = 0; la < Latitude.Length; la++)
            {
                StockIndividualBodyMass.Add(new double[Longitude.Length, StockFunctionalGroup.Length, Stock.Length]);
            }

            foreach (uint[] cell in cellList)
            {
                for (int fg = 0; fg < StockFunctionalGroup.Length; fg++)
                {
                    for (int c = 0; c < Stock.Length; c++)
                    {
                        StockIndividualBodyMass[(int)cell[0]][cell[1], fg, c] = tempData2[
                            cell[0], cell[1], fg, c];
                    }
                }
            }

            tempData2 = StateDataSet.GetData<float[,,,]>("StockTotalBiomass");

            for (int la = 0; la < Latitude.Length; la++)
            {
                StockTotalBiomass.Add(new double[Longitude.Length, StockFunctionalGroup.Length, Stock.Length]);
            }

            foreach (uint[] cell in cellList)
            {
                for (int fg = 0; fg < StockFunctionalGroup.Length; fg++)
                {
                    for (int c = 0; c < Stock.Length; c++)
                    {
                        StockTotalBiomass[(int)cell[0]][cell[1], fg, c] = tempData2[
                            cell[0], cell[1], fg, c];
                    }
                }
            }

            _GridCellStocks = new GridCellStockHandler[Latitude.Length, Longitude.Length];
            
            for (int cell = 0; cell < cellList.Count; cell++)
            {
                _GridCellStocks[cellList[cell][0], cellList[cell][1]] = new GridCellStockHandler(StockFunctionalGroup.Length);
                
                for (int fg = 0; fg < StockFunctionalGroup.Length; fg++)
                {
                    _GridCellStocks[cellList[cell][0], cellList[cell][1]][fg] = new List<Stock>();
                    for (int c = 0; c < Stock.Length; c++)
                    {
                        if (StockTotalBiomass[(int)cellList[cell][0]][cellList[cell][1], fg, c] > 0.0)
                        {
                            Stock TempStock = new Stock(
                                (byte)fg, 
                                StockIndividualBodyMass[(int)cellList[cell][0]][cellList[cell][1], fg, c],
                                StockTotalBiomass[(int)cellList[cell][0]][cellList[cell][1], fg, c]);

                            _GridCellStocks[cellList[cell][0], cellList[cell][1]][fg].Add(TempStock);
                        }
                    }
                }

            }
            

        }


        public void InputModelStateTxt(string inputPath, string filename, ModelGrid ecosystemModelGrid,
            FunctionalGroupDefinitions cohortFunctionalGroupDefinitions, FunctionalGroupDefinitions stockFunctionalGroupDefinitions, bool tracking)
        {
            //Set the input state flag to be true
            _InputState = true;
            
            char[] tab = "\t".ToCharArray();

            string l;
            

            StateReader = new StreamReader(inputPath + filename + ".txt");
            // Create a threadsafe textwriter to write outputs to the Maturity stream
            SyncStateReader = TextReader.Synchronized(StateReader);
            l = SyncStateReader.ReadLine();
            string[] header = l.Split(tab);

            double Latitude;
            double Longitude;
            int Fg;
            double N;
            double Bm;
            double J_bm;
            double A_bm;
            double TI;
            double LogOptPreySize;
            uint BirthTimestep;
            double PropTimeActive;
            uint MaturityTimestep;
            double MaxBm;

            int SFG;
            double SIM;
            double STM;

            uint lat_ind;
            uint lon_ind;

            _GridCellCohorts = new GridCellCohortHandler[ecosystemModelGrid.NumLatCells, ecosystemModelGrid.NumLonCells];
            _GridCellStocks = new GridCellStockHandler[ecosystemModelGrid.NumLatCells, ecosystemModelGrid.NumLonCells];

            long cid = 0;

            while(StateReader.Peek() != -1)
            {
                l = SyncStateReader.ReadLine();
                string[] vals = l.Split(tab);

                Latitude = Convert.ToDouble(vals[1]);
                Longitude = Convert.ToDouble(vals[2]);

                lat_ind = ecosystemModelGrid.GetLatIndex(Latitude);
                lon_ind = ecosystemModelGrid.GetLonIndex(Longitude);

                if (_GridCellCohorts[lat_ind, lon_ind] == null) _GridCellCohorts[lat_ind, lon_ind] = new GridCellCohortHandler(cohortFunctionalGroupDefinitions.GetNumberOfFunctionalGroups());
                if (_GridCellStocks[lat_ind, lon_ind] == null) _GridCellStocks[lat_ind, lon_ind] = new GridCellStockHandler(stockFunctionalGroupDefinitions.GetNumberOfFunctionalGroups());

                if (vals[4].Contains("S")) //Check if this is a stock
                {
                    //Expects the file to have timestep, latitude, longitude, Cohort ID. So start reading the values at column index 4.
                    int i = 4;
                    SFG = Convert.ToInt32(vals[i].Trim(new Char[] { 'S' }));
                    //Jump to the IndividualBodyMass column
                    i = 7;
                    SIM = Convert.ToDouble(vals[i++]);
                    STM = Convert.ToDouble(vals[i++]);
                    
                    Stock NewStock = new Stock((byte)SFG, SIM, STM);

                    if (_GridCellStocks[lat_ind, lon_ind][SFG] == null) _GridCellStocks[lat_ind, lon_ind][SFG] = new List<Stock>();
                    _GridCellStocks[lat_ind, lon_ind].Add(SFG, NewStock);
                }
                else // is a cohort
                {
                    //Expects the file to have timestep, latitude, longitude, Cohort ID. So start reading the values at column index 4.
                    int i = 4;
                    Fg = Convert.ToInt32(vals[i++]);
                    J_bm = Convert.ToDouble(vals[i++]);
                    A_bm = Convert.ToDouble(vals[i++]);
                    Bm = Convert.ToDouble(vals[i++]);
                    N = Convert.ToDouble(vals[i++]);
                    BirthTimestep = Convert.ToUInt32(vals[i++]);
                    MaturityTimestep = Convert.ToUInt32(vals[i++]);
                    LogOptPreySize = Convert.ToDouble(vals[i++]);
                    MaxBm = Convert.ToDouble(vals[i++]);
                    TI = Convert.ToDouble(vals[i++]);
                    PropTimeActive = Convert.ToDouble(vals[i++]);

                    //Instantiate the list for this functional group

                    if(_GridCellCohorts[lat_ind, lon_ind][Fg] == null) _GridCellCohorts[lat_ind, lon_ind][Fg] = new List<Cohort>();

                    Cohort NewCohort = new Cohort((byte) Fg, J_bm, A_bm, Bm, N, LogOptPreySize,MaxBm, (ushort)BirthTimestep, (ushort)MaturityTimestep, PropTimeActive, ref cid, TI, tracking);
                    _GridCellCohorts[lat_ind, lon_ind].Add(Fg, NewCohort);

                }

                
            }

        }

    }
}
