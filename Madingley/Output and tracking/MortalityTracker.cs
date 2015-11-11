using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace Madingley
{
    /// <summary>
    /// Track results associated with mortality
    /// </summary>
    public class MortalityTracker
    {
        /// <summary>
        /// Name of the file to write data on mortality to
        /// </summary>
        string MortalityFilename;

        /// <summary>
        /// A streamwriter instance to output data on mortality
        /// </summary>
        private StreamWriter MortalityWriter;

        /// <summary>
        /// Synchronized version of the streamwriter to output mortality data
        /// </summary>
        private TextWriter SyncMortalityWriter;

        /// <summary>
        /// List of mortality events for each cohort (keyed by cohort ID)
        /// </summary>
        private SortedList<string,List<string>> MortalityList;


        /// <summary>
        /// An instance of the simple random number generator
        /// </summary>
        private NonStaticSimpleRNG RandomNumberGenerator = new NonStaticSimpleRNG();

        /// <summary>
        /// Set up properties of the mortality tracker
        /// </summary>
        /// <param name="numTimeSteps">The total number of time steps for this simulation</param>
        /// <param name="numLats">The number of latitudinal cells in the model grid</param>
        /// <param name="numLons">The number of longitudinal cells in the model grid</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="mortalityFilename">The filename to write data on mortality to</param>
        /// <param name="outputFileSuffix">The suffix to apply to all output files from this model run</param>
        /// <param name="outputPath">The path to write all output files to</param>
        /// <param name="cellIndex">The index of the current cell in the list of all cells to run the model for</param>
        public MortalityTracker(uint numTimeSteps,
            uint numLats, uint numLons,
            List<uint[]> cellIndices,
            string mortalityFilename,
            string outputFileSuffix,
            string outputPath, int cellIndex)
        {
            MortalityFilename = mortalityFilename;

            // Initialise streamwriter to output mortality of cohorts
            MortalityWriter = new StreamWriter(outputPath + MortalityFilename + outputFileSuffix + "_Cell" + cellIndex + ".txt");
            // Create a threadsafe textwriter to write outputs to the NewCohortWriter stream
            SyncMortalityWriter = TextWriter.Synchronized(MortalityWriter);
            SyncMortalityWriter.WriteLine("Latitude\tLongitude\tbirth_step\ttime_step\tcurrent mass\tadult mass\tfunctional group\tcohort id\tnumber died\tmortality source");

            MortalityList = new SortedList<string,List<string>>();

        
        }

        /// <summary>
        /// Record a mortality event associated with a cohort to memory
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="birthStep">The time step that the cohort came into existence</param>
        /// <param name="timestep">The current time step</param>
        /// <param name="currentMass">The current body mass of individuals in the cohort with dying individuals</param>
        /// <param name="adultMass">The adult body mass of individuals in the cohort with dying individuals</param>
        /// <param name="functionalGroup">The index of the functional group that the cohort belongs to</param>
        /// <param name="cohortID">The unique ID of the cohort</param>
        /// <param name="numberDied">The number of individuals </param>
        /// <param name="mortalitySource"></param>
        public void RecordMortality(uint latIndex, uint lonIndex,
            uint birthStep, uint timestep, double currentMass,double adultMass, uint functionalGroup, 
            uint cohortID, double numberDied, string mortalitySource)
        {
            // Write the time step and the abundance of the new cohort to the output file for diagnostic purposes
            string newline = Convert.ToString(latIndex + "\t" + lonIndex + "\t" + birthStep + "\t" + timestep + "\t" + currentMass + "\t" + adultMass + "\t" +
            functionalGroup + "\t" + cohortID + "\t" + numberDied + "\t" + mortalitySource);

            string CohortIDString = Convert.ToString(cohortID);

            if (MortalityList.ContainsKey(CohortIDString))
            {
                MortalityList[CohortIDString].Add(newline);
            }
            else
            {
                MortalityList.Add(CohortIDString, new List<string>() { newline });
            }

            /*
            string StringCID = Convert.ToString(cohortID);

            lock (MortalityList.SyncRoot)
            {
                if (MortalityList.ContainsKey(StringCID))
                {
                    //Copy the current contents for this cohortID to a variable
                    List<object> objects = ((IEnumerable)MortalityList[StringCID]).Cast<object>().ToList();
                    List<string> strings = (from o in objects select o.ToString()).ToList();

                    //Add to the list the newline
                    strings.Add(newline);
                    //Remove the current MortalityList entry for cohortID
                    MortalityList.Remove(StringCID);
                    //Add the new lines to MortalityList for this cohortID
                    MortalityList.Add(StringCID, strings);

                }
                else
                {
                    if (RandomNumberGenerator.GetUniform() > 0.99)
                    {
                        //Add a new MortalityList entry for cohortID
                        List<string> strings = new List<string>();
                        strings.Add(newline);

                        MortalityList.Add(StringCID, strings);
                    }
                }
            }
            */
        }

        /// <summary>
        /// Output the mortality profile of a cohort becoming extinct
        /// </summary>
        /// <param name="cohortID">The ID of the cohort becoming extinct</param>
        public void OutputMortalityProfile(uint cohortID)
        {
            
            string CohortIDString = Convert.ToString(cohortID);

            if (MortalityList.ContainsKey(CohortIDString))
            {
                if (RandomNumberGenerator.GetUniform() > 0.95)
                {
                    var Lines = ((IEnumerable)MortalityList[CohortIDString]).Cast<object>().ToList();
                    foreach (var Line in Lines)
                    {
                        SyncMortalityWriter.WriteLine(Line);
                    }
                }
                MortalityList.Remove(CohortIDString);
            }

            /*
            if (MortalityList.ContainsKey(StringCID))
            {
                lock (MortalityList.SyncRoot)
                {
                    var Lines = ((IEnumerable)MortalityList[StringCID]).Cast<object>().ToList();
                    // Loop over mortality events for the specified cohort and write these to the output file
                    foreach (var Line in Lines)
                    {
                        SyncMortalityWriter.WriteLine(Line);
                    }

                    // Remove the cohort from the list of mortality profiles
                    MortalityList.Remove(Convert.ToString(cohortID));
                }
            }
            */
        }


    }
}
