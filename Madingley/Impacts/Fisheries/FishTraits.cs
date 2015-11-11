using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Madingley
{
    class FishTraits
    {
        //A sorted dictionary to hold the trait data by taxon
        public SortedDictionary<string, string[]> TraitData;

        public SortedDictionary<string, double> MaxMasses;
        /// <summary>
        /// Value of trophic group for each taxa: 2 = herbivore (value = 1), 2-3.5 = Omnivore (value = 2), >3.5 = Carnivore (value = 3)
        /// </summary>
        public SortedDictionary<string, int> TrophicLevel;
        public SortedDictionary<string, Boolean> DeepSea;


        string[] header;
        //Read allometries data
        LWAllometries Allometries;

        public FishTraits()
        {
            StreamReader r = new StreamReader("input\\data\\Fisheries\\taxonlist.csv");

            Allometries = new LWAllometries();

            string[] RequiredFields = {"slmax","tl","setl","reef" ,"pelagic","demersal","deepsea","sea grass","mangrove"};     
            string l;
            char[] comma = ",".ToCharArray();

            string[] f;

            //Get the column names
            if (!r.EndOfStream)
            {
                l = r.ReadLine();
                // Split fields by commas
                header = l.ToLower().Split(comma);
                header = header.Skip(1).ToArray();
            }
            else
            {
                header = new string[RequiredFields.Length];
            }

            TraitData = new SortedDictionary<string, string[]>();

            //Read the taxon trait data file
            while (!r.EndOfStream)
            {
                l = r.ReadLine();
                // Split fields by commas
                f = l.Split(comma);
                
                //Add this taxon's trait data to the sorted dictionary
                TraitData.Add(f[0],f.Skip(1).ToArray());
            }

            //Convert max body lengths to max body masses
            ConvertLengthToMass();
            //Convert max TL to a discrete TL
            ConvertTLToDiscrete();
            //Create a boolean dictionary for deep sea or not
            AssignDeepSea();

        }


        //Convert body lengths to body masses using LW allometries from fishbase:
        // Uses eqn BM = A*(L)^B
        private void ConvertLengthToMass()
        {
            MaxMasses = new SortedDictionary<string, double>();

            int TraitCol = 0;
            for (int i = 0; i < header.Length; i++)
            {
                if (header[i] == "slmax") TraitCol = i;
            }

            foreach (var item in TraitData)
            {
                double[] parameters;
                if (Allometries.AllometricParameters.ContainsKey(item.Key))
                {
                    parameters = Allometries.AllometricParameters[item.Key];
                }
                else
                {
                    parameters = new double[] {0.01,3};
                }
                double l = Convert.ToDouble(item.Value[TraitCol]);
                MaxMasses.Add(item.Key,parameters[0]*Math.Pow(l,parameters[1]));
            }
        }

        private void ConvertTLToDiscrete()
        {
            TrophicLevel = new SortedDictionary<string, int>();

            int TraitCol = 0;
            for (int i = 0; i < header.Length; i++)
            {
                if (header[i] == "tl") TraitCol = i;
            }

            foreach (var item in TraitData)
            {
                double tl = Convert.ToDouble(item.Value[TraitCol]);

                if(tl == 2)
                {
                   TrophicLevel.Add(item.Key,1);
                }
                else if(tl > 2 && tl <= 3.5)
                {
                    TrophicLevel.Add(item.Key, 2);
                }
                else
                {
                    TrophicLevel.Add(item.Key, 3);
                }

            }
        }

        private void AssignDeepSea()
        {
            DeepSea = new SortedDictionary<string, bool>();

            int TraitCol1 = 0;
            int TraitCol2 = 0;
            for (int i = 0; i < header.Length; i++)
            {
                if (header[i] == "demersal") TraitCol1 = i;
                if(header[i] == "deepsea")  TraitCol2 = i;
            }

            foreach (var item in TraitData)
            {
                DeepSea.Add(item.Key, (item.Value[TraitCol1] == "1" || item.Value[TraitCol2] == "1"));
            }
        }

        public double[] TraitRange(string trait)
        {
            double min = 0;
            double max = double.MaxValue;

            int TraitCol = 0;
            for (int i = 0; i < header.Length; i++)
			{
			    if(header[i] == trait.ToLower()) TraitCol = i;
			}

            foreach (var item in TraitData.Values)
            {
                double t = Convert.ToDouble(item[TraitCol]);
                if (t < min) min = t;
                if (t > max) max = t;
            }
            
            double[] ret = {min,max};

            return (ret);
        }

        public Tuple<double[],string[]> MassRange()
        {
            double MinMass = double.MaxValue;
            string MinSp="";
            double MaxMass = 0;
            string MaxSp="";
            
            //Find the max and min species and their max lengths
            foreach (var t in MaxMasses)
            {
                if (t.Value < MinMass)
                {
                    MinMass = t.Value;
                    MinSp = t.Key;
                }
                if (t.Value > MaxMass)
                {
                    MaxMass = t.Value;
                    MaxSp = t.Key;
                }
            }

            return (new Tuple<double[],string[]> (new double[] {MinMass,MaxMass},new string[] {MinSp,MaxSp}));
        }


    }
}
