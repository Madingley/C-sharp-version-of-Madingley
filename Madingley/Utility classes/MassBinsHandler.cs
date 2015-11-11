using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Science.Data;

namespace Madingley
{
    /// <summary>
    /// Handles the mass bins to be used in model outputs
    /// </summary>
    public class MassBinsHandler
    {
        /// <summary>
        /// The number of mass bins to be used for outputs
        /// </summary>
        public int NumMassBins = 50;
        
        /// <summary>
        /// A vector containing the masses correpsonding to the mass bins
        /// </summary>
        private float[] MassBins;

        /// <summary>
        /// Sets up mass bins based on an input file
        /// </summary>
        /// <param name="massBinsFile">The filename containing the mass bin information</param>
        /// <param name="outputPath">The path to the output folder to copy the mass bins definition file to</param>
        public void SetUpMassBins(string massBinsFile, string outputPath)
        {
            // Construct file name
            string FileString = "msds:csv?file=input/Model setup/Ecological Definition Files/" + massBinsFile + "&openMode=readOnly";

            //Copy the Mass bin definitions file to the output directory
            if(System.IO.File.Exists(outputPath+ massBinsFile))
                System.IO.File.Copy("input/Model setup/" + massBinsFile, outputPath + massBinsFile, true);

            // Read in the data
            DataSet InternalData = DataSet.Open(FileString);

            //Copy the values for this variable into an array
            var TempValues = InternalData.Variables[0].GetData();
            NumMassBins = TempValues.Length;
            MassBins = new float[TempValues.Length];

            for (int i = 0; i < TempValues.Length; i++)
            {
                MassBins[i] = Convert.ToSingle(TempValues.GetValue(i));
            }

            // Sort the array of mass bins
            Array.Sort(MassBins);
        }

        /// <summary>
        /// Returns the mass bins copied from file
        /// </summary>
        /// <returns>the mass bins copied from file</returns>
        public float[] GetSpecifiedMassBins()
        {
            return MassBins;
        }

    }
}
