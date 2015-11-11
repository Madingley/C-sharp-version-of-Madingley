using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Madingley
{
    class InputCatchData
    {

        //Define a grid to hold the catch data on the grid resolution and extent that the model will use
        private double[,] _ModelGridCatchTotal;
        public double[,] ModelGridCatchTotal
        {
            get { return _ModelGridCatchTotal; }
            set { _ModelGridCatchTotal = value; }
        }

        private double[,,] _ModelGridCatch;
        public double[,,] ModelGridCatch
        {
            get { return _ModelGridCatch; }
            set { _ModelGridCatch = value; }
        }

        private double[,] _CatchTotal;
        public double[,] CatchTotal
        {
            get { return _CatchTotal; }
            set { _CatchTotal = value; }
        }

        private double[,,] _CatchBinned;
        public double[,,] CatchBinned
        {
            get { return _CatchBinned; }
            set { _CatchBinned = value; }
        }

        private float[] _CatchLats;
        public float[] CatchLats
        {
            get { return _CatchLats; }
            set { _CatchLats = value; }
        }

        private float[] _CatchLons;
        public float[] CatchLons
        {
            get { return _CatchLons; }
            set { _CatchLons = value; }
        }

        int _CatchNumLats;
        int _CatchNumLons;

        //The bin upper bounds into which catch should be aggregated
        private double[] _MassBins;
        public double[] MassBins
        {
            get { return _MassBins; }
            set { _MassBins = value; }
        }

        private int _UnknownMassBinIndex;
        public int UnknownMassBinIndex
        {
            get { return _UnknownMassBinIndex; }
            set { _UnknownMassBinIndex = value; }
        }
        

        public List<string>[,] UnknownTaxa;

        //Instance of class to hold the fish traits data
        FishTraits Traits;

        


        UtilityFunctions Utilities = new UtilityFunctions();

        public InputCatchData(float[] modelLats, float[] modelLons, float cellSize)
        {
            StreamReader r_ht = new StreamReader("input\\data\\Fisheries\\catchratesyr2000HT.csv");
            StreamReader r = new StreamReader("input\\data\\Fisheries\\catchrateyr2000.csv");

            //Read trait data
             Traits = new FishTraits();
            //Retrieve the Max Mass range from the trait data
            var temp = Traits.MassRange();
            double[] MaxMassRange = temp.Item1;
            string[] MaxMassRangeSp = temp.Item2;
            
            
            //Calculate a set of mass bins to be used for removing fisheries catches from binned Madingley biomasses 
            //TO DO: make these bins flexible and user defined
            int MinMassbinMax = Convert.ToInt32(Math.Ceiling(Math.Log10(MaxMassRange[0])));
            int MaxMassbinMax = Convert.ToInt32(Math.Ceiling(Math.Log10(MaxMassRange[1])));

            int NumBins = (MaxMassbinMax - MinMassbinMax) + 1;

            _MassBins = new double[NumBins];
            for (int i = 0; i < NumBins-1; i++)
            {
                _MassBins[i] = Math.Pow(10,MinMassbinMax + i);
            }
            _UnknownMassBinIndex = NumBins - 1;


            string l;
            char[] comma = ",".ToCharArray();

            string[] f;
            
            List<int> year_ht = new List<int>();
            List<int> cell_ht = new List<int>();
            List<double> catchRate_ht = new List<Double>();
            List<string> taxa_ht = new List<string>();

            List<int> year = new List<int>();
            List<int> cell = new List<int>();
            List<double> catchRate = new List<Double>();
            List<string> taxa = new List<string>();

            //Read the Higher Taxonomy file
            while(! r_ht.EndOfStream)
            {                
                l = r_ht.ReadLine();
                // Split fields by commas
                f = l.Split(comma);

                // Lists of the different fields
                year_ht.Add(Convert.ToInt32(f[0]));
                cell_ht.Add(Convert.ToInt32(f[1]));
                catchRate_ht.Add(Convert.ToDouble(f[2]));
                taxa_ht.Add(f[3]);
            }

            //Read the species catch file
            while (!r.EndOfStream)
            {
                l = r.ReadLine();
                // Split fields by commas
                f = l.Split(comma);

                // Lists of the different fields
                year.Add(Convert.ToInt32(f[0]));
                cell.Add(Convert.ToInt32(f[1]));
                catchRate.Add(Convert.ToDouble(f[2]));
                taxa.Add(f[3]);
            }

            float MinLon = -179.75f;
            float MaxLon = 179.75f;
            float MaxLat = 89.75f;
            float MinLat = -89.75f;

            _CatchNumLats = (int)((MaxLat - MinLat) / 0.5) + 1;
            _CatchNumLons = (int)((MaxLon - MinLon) / 0.5) + 1;

            _CatchTotal = new double[_CatchNumLats, _CatchNumLons];
            _CatchBinned = new double[_CatchNumLats, _CatchNumLons, NumBins];

            UnknownTaxa = new List<string>[_CatchNumLats, _CatchNumLons];
            for (int i = 0; i < UnknownTaxa.GetLength(0); i++)
            {
                for (int j = 0; j < UnknownTaxa.GetLength(1); j++)
                {
                    UnknownTaxa[i, j] = new List<string>();
                }
            }


            _CatchLats = new float[_CatchNumLats];
            _CatchLons = new float[_CatchNumLons];

            int[] Index;

            // Match lon index to lon
            for (int i = 0; i < _CatchNumLons; i++)
            {
                _CatchLons[i] = MinLon + (i * 0.5f);
            }

            // Match lat index to lat
            for (int i = 0; i < _CatchNumLats; i++)
            {
                _CatchLats[i] = MaxLat - (i * 0.5f);
            }

            //Will hold the mass bin index for the catch data
            int mb = 0;
            //Allocate the species level catch to cells and mass bins
            for (int i = 0; i < catchRate.Count; i++)
            {
                Index = IndexLookup(cell[i]);

                //Need to convert to size bins
                mb = AssignCatchToMassBin(taxa[i]);

                _CatchTotal[Index[0], Index[1]] += catchRate[i] * 1E6;
                _CatchBinned[Index[0], Index[1], mb] += catchRate[i] * 1E6;

                //If the taxa does not have trait data then list this taxa
                if(mb == UnknownMassBinIndex) UnknownTaxa[Index[0], Index[1]].Add(taxa[i]);
            }
            //Allocate the higher taxa level catch to cells and mass bins
            for (int i = 0; i < catchRate_ht.Count; i++)
            {
                Index = IndexLookup(cell_ht[i]);

                //Need to convert to size bins
                mb = AssignCatchToMassBin(taxa_ht[i]);

                _CatchTotal[Index[0], Index[1]] += catchRate_ht[i] * 1E6;
                _CatchBinned[Index[0], Index[1], mb] += catchRate_ht[i] * 1E6;

                //If the taxa does not have trait data then list this taxa
                if (mb == UnknownMassBinIndex) UnknownTaxa[Index[0], Index[1]].Add(taxa_ht[i]);
            }

            //foreach (var u in UnknownTaxa)
            //{
            //    if (u.Count > 0.0) Console.WriteLine(u.Count);
            //}

            double CumulativeCatch = 0.0;
            for (int i = 0; i < _CatchTotal.GetLength(0); i++)
            {
                for (int j = 0; j < _CatchTotal.GetLength(1); j++)
			    {
			       CumulativeCatch += _CatchTotal[i,j];
			    }
            }

            Console.WriteLine(CumulativeCatch);

            AggregateCatchData(modelLats,modelLons, cellSize);


        }


        private int AssignCatchToMassBin(string t)
        {
            int mb = 0;
            double m = 0.0;
            if (Traits.MaxMasses.ContainsKey(t))
            {
                //If the key is found then assign this mass to the appropriate mass bin
                m = Traits.MaxMasses[t];
                mb = MassBins.ToList().FindIndex(a => a >= m);
                if (mb < 0) mb = UnknownMassBinIndex - 1;
            }
            else
            {
                //if we don't have this taxa in the trait database then assign the biomass to the unknown mass bin
                mb = UnknownMassBinIndex;
            }
            return (mb);
        }

        private void AggregateCatchData(float[] modelLats,float[] modelLons, float cellSize)
        {

            int NumModelLats = modelLats.Length;
            int NumModelLons = modelLons.Length;

            
            //Dimension the model grid catch data
            _ModelGridCatchTotal = new double[NumModelLats,NumModelLons];
            _ModelGridCatch = new double[NumModelLats, NumModelLons,MassBins.Length];

            List<int> LatIndexes;
            List<int> LonIndexes;

            double Area = 0.0;
            double CumulativeArea = 0.0;

            //Loop over each model grid cell,
            for (int i = 0; i < NumModelLats; i++)
            {
                for (int j = 0; j < NumModelLons; j++)
                {

                    LatIndexes = new List<int>();
                    LonIndexes = new List<int>();

                    //Find the indexes of those fine resolution cells that are within the model grid cell
                    for (int ci = 0; ci < _CatchNumLats; ci++)
                    {
                        if (CatchLats[ci] >= (modelLats[i] - cellSize / 2) && CatchLats[ci] <= (modelLats[i] + cellSize / 2))
                        {
                            LatIndexes.Add(ci);
                        }
                    }
                    for (int cj = 0; cj < _CatchNumLons; cj++)
                    {
                        if (CatchLons[cj] >= (modelLons[j] - cellSize / 2) && CatchLons[cj] <= (modelLons[j] + cellSize / 2))
                        {
                            LonIndexes.Add(cj);
                        }
                    }

                    CumulativeArea = 0.0;


                    //Calculate the total catch for the model grid cell
                    //Original data was in units of t km-2,
                    // Above catch rate is multiplied by 1E6 to convert to g.
                    // therefore multiplication by area in km2
                    //gives the catch in g for each model cell
                    foreach (int ci in LatIndexes)
                    {
                        foreach (int cj in LonIndexes)
	                    {
                            Area = Utilities.CalculateGridCellArea(_CatchLats[ci], 0.5, 0.5);
                            _ModelGridCatchTotal[i, j] += _CatchTotal[ci, cj] * Area;
                            CumulativeArea += Area;
                            for (int mb = 0; mb < MassBins.Length; mb++)
                            {
                                _ModelGridCatch[i, j, mb] += _CatchBinned[ci, cj, mb] * Area;
                            }
	                    }
                    }

                    //_ModelGridCatch[i, j] /= CumulativeArea;

                }

            }

        } 

        private int[] IndexLookup(int i)
        {
            int[] indexes = new int[2];
            double t = (i - 1) / _CatchNumLons;

            //lat index
            indexes[0] = (int)Math.Floor(t);

            //lon index
            indexes[1] = (i - 1) % _CatchNumLons;

            return indexes;
        }

    }
}
