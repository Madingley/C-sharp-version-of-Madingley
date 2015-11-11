using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Madingley
{
    /// <summary>
    /// Calculates the relative activity rate of a cohort
    /// </summary>
    public class Activity
    {

        /// <summary>
        /// The distance of the maximum critical temperature from the ambient temperature
        /// </summary>
        private double WarmingTolerance;
        /// <summary>
        /// Distance of the optimal performance temperature from the ambient temperature
        /// </summary>
        private double ThermalSafetyMargin;
        /// <summary>
        /// The optimal performance temperature
        /// </summary>
        private double Topt;
        /// <summary>
        /// The maximum critical temperature
        /// </summary>
        private double CTmax;
        /// <summary>
        /// The minimum critical temperature
        /// </summary>
        private double CTmin;
        /// <summary>
        /// The ambient temperature
        /// </summary>
        private double AmbientTemp;
        /// <summary>
        /// The diurnal temperature range
        /// </summary>
        private double DTR;

        /// <summary>
        /// Intercept of the linear relationship between warming tolerance of terrestrial ectotherms and annual temperature variability
        /// </summary>
        double TerrestrialWarmingToleranceIntercept;
        /// <summary>
        /// Slope of the linear relationship between warming tolerance of terrestrial ectotherms and annual temperature variability
        /// </summary>
        double TerrestrialWarmingToleranceSlope;
        /// <summary>
        /// Intercept of the linear relationship between terrestrial safety margin of terrestrial ectotherms and annual temperature variability
        /// </summary>
        double TerrestrialTSMIntercept;
        /// <summary>
        /// Slope of the linear relationship between terrestrial safety margin of terrestrial ectotherms and annual temperature variability
        /// </summary>
        double TerrestrialTSMSlope;
        
        /// <summary>
        /// Constructor for the Activity class: assigns parameter values
        /// </summary>
        public Activity()
        {
            // Initialise ecological parameters for predation
            InitialiseActivityParameters();
        }

        /// <summary>
        /// Initialise parameters related to the activity of cohorts
        /// </summary>
        private void InitialiseActivityParameters()
        {
             // Source: Deutsch et al (2008), Impacts of climate warming on terrestrial ecototherms across latitude, PNAS.
             TerrestrialWarmingToleranceIntercept = EcologicalParameters.Parameters["Activity.Terrestrial.WarmingToleranceIntercept"];
             TerrestrialWarmingToleranceSlope = EcologicalParameters.Parameters["Activity.Terrestrial.WarmingToleranceSlope"];
             TerrestrialTSMIntercept = EcologicalParameters.Parameters["Activity.Terrestrial.TSMIntercept"];
             TerrestrialTSMSlope = EcologicalParameters.Parameters["Activity.Terrestrial.TSMSlope"];


             // Source: Sunday et al (2010), Global analysis of thermal tolerance and latitude in ectotherms, Proc R Soc B.
             /*MarineUpperToleranceIntercept = 43.2;
             MarineUpperToleranceSlope = -0.14;
             MarineRangeIntercept = 31.2;
             MarineRangeSlope = -0.13;*/
        }

        /// <summary>
        /// Write out the values of the parameters to an output file
        /// </summary>
        /// <param name="sw">A streamwriter object to write the parameter values to</param>
        public void WriteOutParameterValues(StreamWriter sw)
        {
            // Initialise the parameters
            InitialiseActivityParameters();

            // Write out parameters
            sw.WriteLine("Activity\tTerrestrialWarmingToleranceIntercept\t" + Convert.ToString(TerrestrialWarmingToleranceIntercept));
            sw.WriteLine("Activity\tTerrestrialWarmingToleranceSlope\t" + Convert.ToString(TerrestrialWarmingToleranceSlope));
            sw.WriteLine("Activity\tTerrestrialTSMIntercept\t" + Convert.ToString(TerrestrialTSMIntercept));
            sw.WriteLine("Activity\tTerrestrialTSMSlope\t" + Convert.ToString(TerrestrialTSMSlope));
            /*sw.WriteLine("Activity\tMarineUpperToleranceIntercept\t" + Convert.ToString(MarineUpperToleranceIntercept));
            sw.WriteLine("Activity\tMarineUpperToleranceSlope\t" + Convert.ToString(MarineUpperToleranceSlope));
            sw.WriteLine("Activity\tMarineRangeIntercept\t" + Convert.ToString(MarineRangeIntercept));
            sw.WriteLine("Activity\tMarineRangeSlope\t" + Convert.ToString(MarineRangeSlope));*/

        }

        /// <summary>
        /// Calculate the proportion of time for which this cohort could be active and assign it to the cohort's properties
        /// </summary>
        /// <param name="actingCohort">The Cohort for which proportion of time active is being calculated</param>
        /// <param name="cellEnvironment">The environmental information for current grid cell</param>
        /// <param name="madingleyCohortDefinitions">Functional group definitions and code to interrogate the cohorts in current grid cell</param>
        /// <param name="currentTimestep">Current timestep index</param>
        /// <param name="currentMonth">Current month</param>
        public void AssignProportionTimeActive(Cohort actingCohort, SortedList<string, double[]> cellEnvironment,
            FunctionalGroupDefinitions madingleyCohortDefinitions,uint currentTimestep, uint currentMonth)
        {
            double Realm = cellEnvironment["Realm"][0];

            //Only work on heterotroph cohorts
            if (madingleyCohortDefinitions.GetTraitNames("Heterotroph/Autotroph", actingCohort.FunctionalGroupIndex) == "heterotroph")
            {
                //Check if this is an endotherm or ectotherm
                Boolean Endotherm = madingleyCohortDefinitions.GetTraitNames("Endo/Ectotherm", actingCohort.FunctionalGroupIndex) == "endotherm";
                if (Endotherm)
                {
                    //Assumes the whole timestep is suitable for endotherms to be active - actual time active is therefore the proportion specified for this functional group.
                    actingCohort.ProportionTimeActive = madingleyCohortDefinitions.GetBiologicalPropertyOneFunctionalGroup("proportion suitable time active", actingCohort.FunctionalGroupIndex);
                }
                else
                {
                    //If ectotherm then use realm specific function
                    if (Realm == 1.0)
                    {
                        actingCohort.ProportionTimeActive = CalculateProportionTimeSuitableTerrestrial(cellEnvironment, currentMonth, Endotherm) *
                            madingleyCohortDefinitions.GetBiologicalPropertyOneFunctionalGroup("proportion suitable time active", actingCohort.FunctionalGroupIndex);
                    }
                    else
                    {
                        actingCohort.ProportionTimeActive = CalculateProportionTimeSuitableMarine(cellEnvironment, currentMonth, Endotherm) *
                            madingleyCohortDefinitions.GetBiologicalPropertyOneFunctionalGroup("proportion suitable time active", actingCohort.FunctionalGroupIndex);
                    }

                }

            }
            
        }

        /// <summary>
        /// Calculate the proportion of each timestep for which this cohort is active
        /// For ectotherms: is a function of the critical max and min temperatures for this ectotherm cohort and also the ambient temperature and diurnal variation in this cell
        /// Assumes that the diurnal temperature range is symmetrical around the monthly mean temperature
        /// Alse assumes that the diurnal temperature profile is approximated by a sinusoidal time-series
        /// Source: Deutsch et al (2008), Impacts of climate warming on terrestrial ecototherms across latitude, PNAS.
        /// </summary>
        /// <param name="cellEnvironment">The environment for this grid cell</param>
        /// <param name="currentMonth">Currnent month in the model</param>
        /// <param name="endotherm">Boolean indicating if cohort is endotherm or ectotherm (true if endotherm)</param>
        /// <returns>The proportion of the timestep for which this cohort could be active</returns>
        private double CalculateProportionTimeSuitableTerrestrial(SortedList<string, double[]> cellEnvironment, uint currentMonth, Boolean endotherm)
        {


                AmbientTemp = cellEnvironment["Temperature"][currentMonth];
                DTR = cellEnvironment["DiurnalTemperatureRange"][currentMonth];

                //Calculate the Warming tolerance and thermal safety margin given standard deviation of monthly temperature
                WarmingTolerance = TerrestrialWarmingToleranceSlope * cellEnvironment["SDTemperature"][0] + TerrestrialWarmingToleranceIntercept;
                ThermalSafetyMargin = TerrestrialTSMSlope * cellEnvironment["SDTemperature"][0] + TerrestrialTSMIntercept;

                Topt = ThermalSafetyMargin + cellEnvironment["AnnualTemperature"][0];
                CTmax = WarmingTolerance + cellEnvironment["AnnualTemperature"][0];


                double PerformanceStandardDeviation = (CTmax - Topt) / 12;

                CTmin = Topt - 4 * PerformanceStandardDeviation;

                return ProportionDaySuitable();
            
        }

        /// <summary>
        /// Calculate the proportion of each timestep for which this cohort is active
        /// For ectotherms: Is a function of the critical max and min temperatures for this ectotherm cohort and also the ambient temperature and diurnal variation in this cell
        /// Assumes that the diurnal temperature range is symmetrical around the monthly mean temperature
        /// Alse assumes that the diurnal temperature profile is approximated by a sinusoidal time-series
        /// Source: Sunday et al (2010), Global analysis of thermal tolerance and latitude in ectotherms, Proc R Soc B.
        /// </summary>
        /// <param name="cellEnvironment">The environment for this grid cell</param>
        /// <param name="currentMonth">Currnent month in the model</param>
        /// <param name="endotherm">Boolean indicating if cohort is endotherm or ectotherm (true if endotherm)</param>
        /// <returns>The proportion of the timestep for which this cohort could be active</returns>
        private double CalculateProportionTimeSuitableMarine(SortedList<string, double[]> cellEnvironment, uint currentMonth, Boolean endotherm)
        {

            return 1.0;
            /*double Latitude = Math.Abs(cellEnvironment["Latitude"][0]);


            CTmax = MarineUpperToleranceIntercept + (Latitude * MarineUpperToleranceSlope);
            CTmin = CTmax - (MarineRangeIntercept + (Latitude * MarineRangeSlope));

            AmbientTemp = cellEnvironment["Temperature"][currentMonth];
            DTR = cellEnvironment["DiurnalTemperatureRange"][currentMonth];

            return ProportionDaySuitable();
            */
        }

        /// <summary>
        /// Calculate the proportion of the current timestep that this cohort is active for
        /// Is a function of the critical max and min temperatures for this ectotherm cohort and also the ambient temperature and diurnal variation in this cell
        /// Assumes that the diurnal temperature range is symmetrical around the monthly mean temperature
        /// Alse assumes that the diurnal temperature profile is approximated by a sinusoidal time-series
        /// Sin of form:
        ///T(h)=Ambient+ [DTR*(0.5*sin(omega*(h-6)))]
        /// </summary>
        /// <returns>The proportion of the day that temperatures are between CTmin and CTmax</returns>
        public double ProportionDaySuitable()
        {
            double ProportionOfDaySuitable;


            //Calculate the diurnal maximum in the current month
            double DTmax = AmbientTemp + (0.5 * DTR);
            double DTmin = AmbientTemp - (0.5 * DTR);

            
            //Proportion of time for which ambient temperatures are greater than the critical upper temperature
            double POver;
            //Proportion of time for which ambient temperatures are below the critical lower temperature
            double PBelow;
            double temp;

            if(CTmax - DTmax > 0.0)
            {
                temp = 1.0;
            }
            else if (CTmax - DTmin < 0.0)
            {
                temp = -1.0;
            }
            else
            {
                temp = 2 * (CTmax - AmbientTemp) / DTR;
            }
            POver = ((Math.PI / 2.0) - Math.Asin(temp))/Math.PI;

            if (CTmin - DTmax > 0.0)
            {
                temp = 1.0;
            }
            else if (CTmin - DTmin < 0.0)
            {
                temp = -1.0;
            }
            else
            {
                temp = 2 * (CTmin - AmbientTemp) / DTR;
            }
            PBelow = 1 - ((Math.PI / 2.0) - Math.Asin(temp)) / Math.PI;

            ProportionOfDaySuitable = 1 - (POver + PBelow);

            

            return ProportionOfDaySuitable;
        }

    }
}
