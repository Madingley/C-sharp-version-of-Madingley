using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

namespace Madingley
{
    /// <summary>
    /// Calculates derived climate variables for which no input data exist
    /// </summary>
    public class ClimateVariablesCalculator
    {
        /// <summary>
        /// Constructor the climate variables calculator
        /// </summary>
        public ClimateVariablesCalculator()
        {
        }

        /// <summary>
        /// Calculates monthly water balance variables: actual evapotranspiration, soil water deficit,
        /// and the an approximation for the length of the fire season.
        /// Actual Evapotranspiration and soil moisture was calculated by following Prentice et al (1993)
        /// "A simulation model for the transient effects of climate change on forest landscapes",
        /// Ecological Modelling, 65, 51-70, but using potential evapotranspiration rates calculated elsewhere
        /// (normally the Penman Monteith equation).
        /// The approximate length of the fire season was calculated using equations (2) and (4) of 
        /// Thonicke et al. (2001). "The role of fire disturbance for global vegetation dynamics: coupling
        /// fire into a Dynamic Global Vegetation Model". Global Ecology and Biogeography, 10, 661-677.
        /// </summary>
        /// <param name="AvailableWaterCapacity">The available water capacity of the soil (mm)</param>
        /// <param name="Precipitation">Mean monthly precipitation (mm)</param>
        /// <param name="MonthlyTemperatures">Mean monthly temperatures, degrees celcius</param>
        /// <returns>A Tuple containing i) monthly actual evapotranspiration (mm), ii) soil water deficit (mm) and, iii) fire season length (between 0 and 360 days) </returns>
        public Tuple<double[], double, double> MonthlyActualEvapotranspirationSoilMoisture(double AvailableWaterCapacity, double[] Precipitation, double[] MonthlyTemperatures)
        {
            // Vector to hold potential evapotranspiration data
            double[] PotentialEvapotranspiration = new double[12];

            // Loop over months and calculate actual evapotranspiration
            for (int i = 0; i < 12; i++)
            {
                PotentialEvapotranspiration[i] = this.CalculatePotentialEvapotranspiration(MonthlyTemperatures[i]);
            }

            // This algorithm sets the soil water content at field capacity and simulates 10 years of
            // soil water dynamics, which in testing has been long enough for the annual soil water dynamics to 
            // settle on an equilibrium cycle.
            int RunYears = 10; //Number of years to simulate
            double SoilWaterPast = AvailableWaterCapacity; // Initialise the past soil water content to be field capacity
            double[] ActualEvapotranspiration = new double[12]; // Will store monthly actual avapotranspiration (mm)
            double[] SoilWater = new double[12]; // Will store montly soil water
            double[] DailyAET = new double[30]; // Temporary store for daily actual evapotranspiration
            double[] DailySWC = new double[30]; // Temporary store for daily soil water content
            double[] MidMonthDailyPET = new double[12]; // The daily PET at the middle of each month
            double[] MidMonthDailyPPT = new double[12]; // The daily PPT at the middle of each month
            double SoilMoistureFireThreshold = 0.3;

            for (int jj = 0; jj < 12; jj++) // for each month work out the mid point values for PPT and PET
            {
                MidMonthDailyPET[jj] = PotentialEvapotranspiration[jj] / 30; // Approximating 30 days per month
                MidMonthDailyPPT[jj] = Precipitation[jj] / 30; // Approximating 30 days per month
            }

            int PrevMonth = 11; // used to store the index of the previous month
            int NextMonth = 1; // used to store the index of the next month
            double PET = 0; // tracks the daily potential evapotranspiration rate (mm day-1)
            double PPT = 0; // tracks the dail predicipitation rate (mm day-1)
            double TMP = 0;

            double LengthOfFireSeason = 0; // The length of the fire season is the fraction of the year that the soil moisture status is below a critical value: indicative of fire risk

            for (int ii = 0; ii < RunYears; ii++) // for each of the simulated years
            {
                for (int jj = 0; jj < 12; jj++) // for each month
                {
                    PrevMonth = (jj == 0) ? 11 : jj - 1; // work out the index of the previous month
                    NextMonth = (jj == 11) ? 0 : jj + 1; // work out the index of the next month  

                    for (int kk = 0; kk < 30; kk++) // for each day in the month
                    {
                        if (kk < 15) // if we are less than half way through the month then linearly interpolate from the previous month
                        {
                            PET = MidMonthDailyPET[PrevMonth] + ((MidMonthDailyPET[jj] - MidMonthDailyPET[PrevMonth]) / (double)15) * (double)kk; //basically divide the difference by fifteen steps and multiply by the number of steps forward
                            PPT = MidMonthDailyPPT[PrevMonth] + ((MidMonthDailyPPT[jj] - MidMonthDailyPPT[PrevMonth]) / (double)15) * (double)kk;
                        }
                        else // if we are more than half way through the month then linearly interpolate forwards
                        {
                            PET = MidMonthDailyPET[jj] + ((MidMonthDailyPET[NextMonth] - MidMonthDailyPET[jj]) / (double)15) * (double)(kk - 15);
                            PPT = MidMonthDailyPPT[jj] + ((MidMonthDailyPPT[NextMonth] - MidMonthDailyPPT[jj]) / (double)15) * (double)(kk - 15);
                        }
                        DailyAET[kk] = PET * (SoilWaterPast / AvailableWaterCapacity); // this is the potential evapotranspiration rates scaled by how dry the soil is. The further the soil water is from field capacity the less the evapotranspiration rate is.
                        DailySWC[kk] = Math.Min(Math.Max((SoilWaterPast + PPT - DailyAET[kk]), 0), AvailableWaterCapacity); //Soil water content is then updated
                        SoilWaterPast = DailySWC[kk]; // update the previous soil water content
                    } // end of day loop
                    if (ii == (RunYears - 1)) // if we are in the last year of simulation then we also want to record the monthly values
                    {
                        for (int kk = 0; kk < 30; kk++) // for each day in the month
                        {
                            if (kk < 15) // if we are less than half way through the month then linearly interpolate from the previous month
                            {
                                TMP = MonthlyTemperatures[PrevMonth] + ((MonthlyTemperatures[jj] - MonthlyTemperatures[PrevMonth]) / (double)15) * (double)kk;
                            }
                            else // if we are more than half way through the month then linearly interpolate forwards
                            {
                                TMP = MonthlyTemperatures[jj] + ((MonthlyTemperatures[NextMonth] - MonthlyTemperatures[jj]) / (double)15) * (double)(kk - 15);
                            }
                            ActualEvapotranspiration[jj] += DailyAET[kk]; // Add up the actual evapotranspiration
                            SoilWater[jj] += DailySWC[kk]; // Add up the soil water contents (we'll take an average)
                            double SoilMoistureContent = DailySWC[kk] / AvailableWaterCapacity;
                            if (TMP > 0 && SoilMoistureContent < SoilMoistureFireThreshold)
                            {
                                LengthOfFireSeason += (Math.Exp((-Math.PI) * Math.Pow(((DailySWC[kk] / AvailableWaterCapacity) / 0.3), 2))); // work out the length of the fire season
                            }
                        }
                    }
                }
            }
            var OutputData = new Tuple<double[], double, double>(ActualEvapotranspiration, SoilWaterPast, LengthOfFireSeason); // return the collection of results
            return OutputData;
        }


        /// <summary>
        /// Calculates the monthly potential evapotranspiration according to
        /// Malmstrom VH (1969) A new approach to the classification of climate. J Geog 68:351–357.
        /// </summary>
        /// <param name="Temperature">Mean monthly temperature, degrees Celsius</param>
        /// <returns>Potential Monthly Evapotranspiration, mm</returns>
        public double CalculatePotentialEvapotranspiration(double Temperature)
        {
            double ps0 = 610.78; // The saturation vapour pressure at zero degrees C (pascals)
            double psa = 0.0; // The saturation vapour pressure at another temperature

            // This then predicts vapur pressure as a function of temperature,
            // With different functions depending on whether the temperaure is above or below 0 degrees C
            psa = (Temperature < 0.0) ? Math.Exp((-6140.4 / (273 + Temperature)) + 28.916) : 610.78 * Math.Exp((Temperature / (Temperature + 238.3)) * 17.2694);

            // The Potential Evapotranspiration is then approximated as a linear
            // function of the ratio of the saturation vapour pressure at the given temperature to the pressure at zero.
            return ((psa / ps0) * 25);
        }

        
        /// <summary>
        /// Estimates the fraction of the year in which the temperature drops below zero at some time in the day
        /// according to the the CRU CL 2.0 gridded climate dataset (For details of this dataset see CRU2p0Dataset.txt)
        /// </summary>
        /// <param name="monthlyFrostDays">A vector containing the number of frost days each month</param>
        /// <param name="monthlyTemperature">A vector containing average temperatures for each month</param>
        /// <param name="missingValue">The missing value used in the the environmental datasets</param>
        /// <returns>The fraction of the year in which temperature drops below zero at some point in the day</returns>
        public double GetNDF(double[] monthlyFrostDays, double[] monthlyTemperature, double missingValue)
        {
            double DataToReturn = 0.0;

            if (monthlyFrostDays[0] > missingValue)
            {
                double NumMonthsFrost = 0; // will monitor the integrated number of frost months (a continuous variable)
                int prevmonth;
                int nextmonth;

                // We classify a complete "frost month" if we have more than 15 days in the month with frost 
                for (int jj = 0; jj < 12; jj++)
                {
                    prevmonth = (jj == 0) ? 11 : jj - 1;
                    nextmonth = (jj == 11) ? 0 : jj + 1;
                    // We classify a complete "frost month" if we have more than 15 days in the month with frost 
                    if (monthlyFrostDays[jj] > 15)
                    {
                        NumMonthsFrost++;
                    }
                    // However, if there are less than 15 days in the month with frost then we first of all
                    // work out if that month came from a previous month with more than 15 frost days
                    // If that is the case then we interpolate forwards and have a fraction of a month that is frost
                    else if (monthlyFrostDays[prevmonth] > 15)
                    {
                        NumMonthsFrost += (double)monthlyFrostDays[jj] / 15;

                    }
                    // Otherwise if there are more than 15 days frost in the next month then we are going into the winter season
                    // and we make an interpolation
                    else if (monthlyFrostDays[nextmonth] > 15)
                    {
                        NumMonthsFrost += (double)monthlyFrostDays[jj] / 15;
                    }
                }

                    DataToReturn = NumMonthsFrost / 12; // convert to a fraction of a year
            }
            else
            {
                ApproximateNDF(monthlyTemperature);
            }


            return DataToReturn;
        }
        

        /// <summary>
        /// Calculates the number of days frost using an alternative algorithm to that in ClimateLookup
        /// that is based on mean annual temperature data alone. This will probably be a coarse representation
        /// of the number of frost days but will do for now.
        /// </summary>
        /// <param name="MATData">Mean monthly temperatures, degrees celcius</param>
        /// <returns>Fraction of the year that experiences frost</returns>
        private double ApproximateNDF(double[] MATData)
        {
            int PrevMonth;
            int NextMonth;
            double NDF = 0;

            for (int jj = 0; jj < 12; jj++)
            {
                PrevMonth = (jj == 0) ? 11 : jj - 1; // work out the index of the previous month
                NextMonth = (jj == 11) ? 0 : jj + 1; // work out the index of the next month 
                double TempStart = (MATData[PrevMonth] + MATData[jj]) / 2.0;
                double TempEnd = (MATData[NextMonth] + MATData[jj]) / 2.0;
                if (TempStart < 0.0)
                {
                    if (MATData[jj] < 0.0)
                    {
                        NDF += 15.0;
                    }
                    else
                    {
                        NDF += (MATData[jj] / (MATData[jj] - TempStart)) * 15;
                    }
                }
                else
                {
                    if (MATData[jj] < 0.0)
                    {
                        NDF += (TempStart / (TempStart - MATData[jj])) * 15;
                    }
                }
                if (MATData[jj] < 0.0)
                {
                    if (TempEnd < 0.0)
                    {
                        NDF += 15.0;
                    }
                    else
                    {
                        NDF += (TempEnd / (TempEnd - MATData[jj])) * 15;
                    }
                }
                else
                {
                    if (TempEnd < 0.0)
                    {
                        NDF += (MATData[jj] / (MATData[jj] - TempEnd)) * 15;
                    }
                }
            }
            return NDF / 360;
        }
    }
}
