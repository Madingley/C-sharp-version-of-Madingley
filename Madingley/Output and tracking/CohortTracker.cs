using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


using System.Diagnostics;

namespace Madingley
{
    public class CohortTracker
    {
        /// <summary>
        /// File to write data on cohort biomass abundance and to
        /// </summary>
        string CohortFilename;

        /// <summary>
        /// A streamwriter for writing out data on cohorts 
        /// </summary>
        private StreamWriter CohortWriter;

        /// <summary>
        /// Thread-safe text-writer to output cohort data
        /// </summary>
        private TextWriter SyncCohortWriter;

        public CohortTracker(string cohortFilename, 
            string outputFilesSuffix, string outputPath)
        {
            CohortFilename = cohortFilename;

            // Initialise streamwriter to output biomasses eaten data
            CohortWriter = new StreamWriter(outputPath + cohortFilename + outputFilesSuffix + ".txt");
            SyncCohortWriter = TextWriter.Synchronized(CohortWriter);
            SyncCohortWriter.WriteLine("Latitude\tLongitude\ttime_step\tfunctional_group\tCurrent_body_mass_g\tAbundance\tJuvenile_mass_g\tAdult_mass_g");

        }

        public void RecordCohorts(uint lat, uint lon, uint currentTimeStep, GridCellCohortHandler cohorts)
        {
            foreach (var fg in cohorts)
            {
                foreach (Cohort c in fg)
                {
                    SyncCohortWriter.WriteLine(
                        Convert.ToString(lat) + '\t' +
                        Convert.ToString(lon) + '\t' +
                        Convert.ToString(currentTimeStep) + '\t' +
                        Convert.ToString(c.FunctionalGroupIndex) + '\t' +
                        Convert.ToString(c.IndividualBodyMass) + '\t' +
                        Convert.ToString(c.CohortAbundance) + '\t' +
                        Convert.ToString(c.JuvenileMass) + '\t' +
                        Convert.ToString(c.AdultMass));
                }
                
            }
        }

        public void CloseStreams()
        {
            SyncCohortWriter.Dispose();
            CohortWriter.Dispose();
        }

    }
}
