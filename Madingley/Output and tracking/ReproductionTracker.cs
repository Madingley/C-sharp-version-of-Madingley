using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

namespace Madingley
{
    /// <summary>
    /// Tracks results associated with the reproduction process
    /// </summary>
    public class ReproductionTracker
    {
        /// <summary>
        /// File to write data on newly produced cohorts to
        /// </summary>
        string NewCohortsFilename;

        /// <summary>
        /// File to write data on maturity of cohorts to
        /// </summary>
        string MaturityFilename;

        /// <summary>
        /// A streamwriter instance for outputting data on newly produced cohorts
        /// </summary>
        private StreamWriter NewCohortWriter;
        /// <summary>
        /// Synchronized version of the streamwriter for outputting data on newly produced cohorts
        /// </summary>
        private TextWriter SyncNewCohortWriter;
        /// <summary>
        /// A streamwriter instance for outputting data on maturity of cohorts
        /// </summary>
        private StreamWriter MaturityWriter;
        /// <summary>
        /// A synchronized version of the streamwriter for outuputting data on the maturity of cohorts
        /// </summary>
        private TextWriter SyncMaturityWriter;

        /// <summary>
        /// Sets up properties of the reproduction tracker
        /// </summary>
        /// <param name="numTimeSteps">The total number of timesteps for this simulation</param>
        /// <param name="numLats">The number of latitudes in the model grid</param>
        /// <param name="numLons">The number of longitudes in the model grid</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="newCohortsFilename">The filename to write information about new cohorts to</param>
        /// <param name="maturityFilename">The filename to write information about cohorts reaching maturity</param>
        /// <param name="outputFileSuffix">The suffix to apply to all output files from this model run</param>
        /// <param name="outputPath">The path to write all output files to</param>
        /// <param name="cellIndex">The index of the current cell in the list of all cells to run the model for</param>
        public ReproductionTracker(uint numTimeSteps,
            uint numLats, uint numLons, 
            List<uint[]> cellIndices, 
            string newCohortsFilename, 
            string maturityFilename,
            string outputFileSuffix,
            string outputPath, int cellIndex)
        {

            NewCohortsFilename = newCohortsFilename;
            MaturityFilename = maturityFilename;
            

            // Initialise streamwriter to output abundance of newly produced cohorts to a text file
            NewCohortWriter = new StreamWriter(outputPath + newCohortsFilename + outputFileSuffix + "_Cell" + cellIndex + ".txt");
            // Create a threadsafe textwriter to write outputs to the NewCohortWriter stream
            SyncNewCohortWriter = TextWriter.Synchronized(NewCohortWriter);
            SyncNewCohortWriter.WriteLine("Latitude\tLongitude\ttime_step\tabundance\tfunctional group\tadult mass\tparent cohort IDs\toffspring cohort ID");

            MaturityWriter = new StreamWriter(outputPath + maturityFilename + outputFileSuffix + "_Cell" + cellIndex + ".txt");
            // Create a threadsafe textwriter to write outputs to the Maturity stream
            SyncMaturityWriter = TextWriter.Synchronized(MaturityWriter);
            SyncMaturityWriter.WriteLine("Latitude\tLongitude\ttime_step\tbirth_step\tjuvenile Mass\tadult mass\tfunctional group");
            
        }

        /// <summary>
        /// Records information about new cohorts spawned in the model
        /// </summary>
        /// <param name="latIndex">The latitude index of the grid cell in which the cohort was spawned</param>
        /// <param name="lonIndex">The longitude index of the grid cell in which the cohort was spawned</param>
        /// <param name="timestep">The model timestep in which the spawning happened</param>
        /// <param name="offspringCohortAbundance">The abundance of the offspring cohort</param>
        /// <param name="parentCohortAdultMass">The adult mass of the parent cohort</param>
        /// <param name="functionalGroup">The functional group of the offspring cohort</param>
        /// <param name="parentCohortIDs">The cohort IDs associated with the parent cohort</param>
        /// <param name="offspringCohortID">The cohort ID used for the new offspring cohort</param>
        public void RecordNewCohort(uint latIndex, uint lonIndex, uint timestep, double offspringCohortAbundance, double parentCohortAdultMass, 
            int functionalGroup, List<uint> parentCohortIDs,uint offspringCohortID)
        {
            double[] NewCohortRecords = new double[3];
            NewCohortRecords[0] = offspringCohortAbundance;
            NewCohortRecords[1] = parentCohortAdultMass;
            NewCohortRecords[2] = (double)functionalGroup;

            string AllCohortIDs = Convert.ToString(parentCohortIDs[0]);
            if (parentCohortIDs.Count > 1)
            {
                for (int i = 1; i < parentCohortIDs.Count; i++)
                {
                    AllCohortIDs = AllCohortIDs + "; " + Convert.ToString(parentCohortIDs[i]);
                }
            }

            // Write the time step and the abundance of the new cohort to the output file for diagnostic purposes
            string newline = Convert.ToString(latIndex) + '\t' + Convert.ToString(lonIndex) + '\t' +
                Convert.ToString(timestep) + '\t' + Convert.ToString(offspringCohortAbundance) + '\t' +
                Convert.ToString(functionalGroup) + '\t' + Convert.ToString(parentCohortAdultMass) + '\t' + AllCohortIDs +
                '\t' + Convert.ToString(offspringCohortID);
            SyncNewCohortWriter.WriteLine(newline);
        }

        /// <summary>
        /// Record information about cohorts reaching maturity in the model
        /// </summary>
        /// <param name="latIndex">The latitude index of the grid cell in which the cohort was spawned</param>
        /// <param name="lonIndex">The longitude index of the grid cell in which the cohort was spawned</param>
        /// <param name="timestep">The model timestep in which the spawning happened</param>
        /// <param name="birthTimestep">The timestep in which the cohort was born</param>
        /// <param name="juvenileMass">The mass at which the cohort was born</param>
        /// <param name="adultMass">The maturity mass of the cohort</param>
        /// <param name="functionalGroup">The functional group of the cohort</param>
        public void TrackMaturity(uint latIndex, uint lonIndex, uint timestep, uint birthTimestep, double juvenileMass, double adultMass, int functionalGroup)
        {
            //Record data on this cohort reaching maturity

            double[] MaturityRecords = new double[4];
            MaturityRecords[0] = (double)birthTimestep;
            MaturityRecords[1] = juvenileMass;
            MaturityRecords[2] = adultMass;
            MaturityRecords[3] = (double)functionalGroup;

            //_Maturity[latIndex, lonIndex,timestep].Add(MaturityRecords);

            // Write the time step and the abundance of the new cohort to the output file for diagnostic purposes
            string newline = Convert.ToString(latIndex) +'\t'+ Convert.ToString(lonIndex)+'\t'+
                Convert.ToString(timestep) + '\t' + Convert.ToString(birthTimestep) + '\t' + Convert.ToString(juvenileMass) + '\t'+
                Convert.ToString(adultMass) + '\t' + Convert.ToString(functionalGroup);
            SyncMaturityWriter.WriteLine(newline);
        }

        /// <summary>
        /// Close the output streams for the reproduction tracker
        /// </summary>
        public void CloseStreams()
        {
            SyncMaturityWriter.Close();
            MaturityWriter.Close();
            SyncNewCohortWriter.Close();
            NewCohortWriter.Close();
        }

    }
}
