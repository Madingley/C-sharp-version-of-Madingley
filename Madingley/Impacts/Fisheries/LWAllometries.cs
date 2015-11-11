using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Madingley
{
    class LWAllometries
    {
        //A sorted dictionary to hold the trait data by taxon
        public SortedDictionary<string, double[]> AllometricParameters;


        public LWAllometries()
        {
            StreamReader r = new StreamReader("input\\data\\Fisheries\\taxonLW.csv");

            string l;
            char[] comma = ",".ToCharArray();

            string[] f;

            //Get the column names
            if (!r.EndOfStream)
            {
                l = r.ReadLine();
            }

            AllometricParameters = new SortedDictionary<string, double[]>();

            //Read the taxon trait data file
            while (!r.EndOfStream)
            {

                l = r.ReadLine();
                // Split fields by commas
                f = l.Split(comma);
                string[] f1 = f.Skip(1).ToArray();
                double[] temp = new double[f1.Length];
                for (int i = 0; i < f1.Length; i++)
                {
                    temp[i] = Convert.ToDouble(f1[i]);
                }

                //Add this taxon's trait data to the sorted dictionary
                AllometricParameters.Add(f[0], temp);
            }

        }


    }
}
