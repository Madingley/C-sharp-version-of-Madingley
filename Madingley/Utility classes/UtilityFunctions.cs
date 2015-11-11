using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// Generic functions
    /// </summary>
    public class UtilityFunctions
    {
        /// <summary>
        /// If longitudinal cell coordinates run from 0 to 360, the convert to -180 to 180 values
        /// </summary>
        /// <param name="lons">The longitudinal coorindates of the cells in the model grid</param>
        public void ConvertToM180To180(double[] lons)
        {
            // Loop over longitudinal coordinates of the model grid cells
            for (int jj = 0; jj < lons.Length; jj++)
            {
                // If longitudinal coorindates exceed 180, then subtrarct 360 to correct the coorindates
                if (lons[jj] >= 180.0)
                {
                    lons[jj] -= 360.0;
                }
            }
            // Re-sort the longitudinal coordinates
            Array.Sort(lons);
        }

        /// <summary>
        /// Generate a random order in which cohorts will be subjected to ecological processes
        /// </summary>
        /// <param name="numberIndices">The number of cohorts in the current grid cell</param>
        /// <returns>A vector of randomly ordered integers corresponding to the cohorts in the grid cell</returns>
        public uint[] RandomlyOrderedIndices(uint numberIndices)
        {
            // A vector to hold indices of cohorts in order
            uint[] OrderedIndices = new uint[numberIndices];
            // A vector to hold indices of cohorts in random order
            uint[] RandomOrderIndices = new uint[numberIndices];

            // Fill the ordered vector with incremental integer indices up to the total number of cohorts
            for (uint ii = 0; ii < numberIndices; ii++)
            {
                OrderedIndices[ii] = ii;
            }

            // Copy the ordered vector of cohorts to the vector for the randomly ordered cohorts
            OrderedIndices.CopyTo(RandomOrderIndices, 0);
            // Declare and initialise an instance of Ranodm for random number generation
            Random random = new Random();

            // Loop over cohorts
            for (int ii = 0; ii < OrderedIndices.Length; ii++)
            {
                // Generate a random integer to swap this cohort index with
                int SwapIndex = random.Next(ii, OrderedIndices.Length);
                // If the cohort index to swap is not the same as the active cohort index, then swap the values
                if (SwapIndex != ii)
                {
                    uint Temp = RandomOrderIndices[ii];
                    RandomOrderIndices[ii] = RandomOrderIndices[SwapIndex];
                    RandomOrderIndices[SwapIndex] = Temp;
                }
            }
            
            // Return the randomly ordered vector of cohort indices
            return RandomOrderIndices;
          
        }

        /// <summary>
        /// Generate a non-random order in which cohorts will be subjected to ecological processes
        /// </summary>
        /// <param name="cohortNumber">The number of cohorts in the current grid cell</param>
        /// <param name="currentTimeStep">The current time step of the model</param>
        /// <returns>A vector of non-randomly ordered integers corresponding to the cohorts in the grid cell</returns>
        public uint[] NonRandomlyOrderedCohorts(uint cohortNumber, uint currentTimeStep)
        {

            // A vector to hold indices of cohorts in order
            uint[] OrderedCohorts = new uint[cohortNumber];
            // A vector to hold indices of cohorts in random order
            uint[] RandomOrderCohorts = new uint[cohortNumber];

            // Fill the ordered vector with incremental integer indices up to the total number of cohorts
            for (uint ii = 0; ii < cohortNumber; ii++)
            {
                OrderedCohorts[ii] = ii;
            }
            
            // Copy the ordered vector of cohorts to the vector for the non-randomly reordered cohorts
            OrderedCohorts.CopyTo(RandomOrderCohorts, 0);
            // Declare and initialise an instance of Ranodm for random number generation, using the current time step as a deterministic seed to 
            // ensure a non-random order of cohorts
            Random random = new Random((int)currentTimeStep);

            // Loop over cohorts
            for (int ii = 0; ii < OrderedCohorts.Length; ii++)
            {
                // Generate a pseudo-random integer to swap this cohort index with
                int SwapIndex = random.Next(ii, OrderedCohorts.Length);
                // If the cohort index to swap is not the same as the active cohort index, then swap the values
                if (SwapIndex != ii)
                {
                    uint Temp = RandomOrderCohorts[ii];
                    RandomOrderCohorts[ii] = RandomOrderCohorts[SwapIndex];
                    RandomOrderCohorts[SwapIndex] = Temp;
                }
            }

            // Return the deterministically ordered vector of cohort indices
            return RandomOrderCohorts;

         }

        /// <summary>
        /// Get the month corresponding to the current time step
        /// </summary>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="modelTimestepUnits">The time step units</param>
        /// <returns>The month corresponding to the current time step</returns>
        public uint GetCurrentMonth(uint currentTimestep, string modelTimestepUnits)
        {
            uint Month;
            
            double DaysInYear = 360.0;
            double MonthsInYear = 12.0;
            double DaysInWeek = 7.0;

            switch (modelTimestepUnits.ToLower())
            {
                case "year":
                    Month = 0;
                    break;
                case "month":
                    Month = currentTimestep % 12;
                    break;
                case "week":
                    Month = (uint)Math.Floor(currentTimestep / ((DaysInYear/MonthsInYear)/DaysInWeek)) % 12;
                    break;
                case "day":
                    Month = (uint)Math.Floor(currentTimestep / (DaysInYear / MonthsInYear)) % 12;
                    break;
                default:
                    Debug.Fail("Requested model time units not currently supported");
                    Month =  100;
                    break;

            }

            return Month;

        }

        /// <summary>
        /// Calculates factors to convert between different time units
        /// </summary>
        /// <param name="fromUnit">Time unit to convert from</param>
        /// <param name="toUnit">Time unit to convert to</param>
        /// <returns>Factor to convert between time units</returns>
        public double ConvertTimeUnits(string fromUnit, string toUnit)
        {
            // Variable to hold the conversion factor
            double ConversionValue;
            double DaysInYear = 360.0;
            double MonthsInYear = 12.0;
            double DaysInWeek = 7.0;

            // Determine which combination of time units is being requested and return the appropriate scaling factor
            switch (fromUnit.ToLower())
            {
                case "year":
                    switch (toUnit.ToLower())
                    {
                        case "year":
                            ConversionValue =  1.0;
                            break;
                        case "month":
                            ConversionValue =  MonthsInYear;
                            break;
                        case "bimonth":
                            ConversionValue = MonthsInYear*2;
                            break;
                        case "week":
                            ConversionValue = DaysInYear/DaysInWeek;
                            break;
                        case "day":
                            ConversionValue =  DaysInYear;
                            break;
                        default:
                            Debug.Fail("Requested combination of time units not currently supported");
                            ConversionValue =  0;
                            break;
                    }
                    break;
                case "month":
                    switch (toUnit.ToLower())
                    {
                        case "year":
                            ConversionValue =  1.0 / MonthsInYear;
                            break;
                        case "month":
                            ConversionValue =  1.0;
                            break;
                        case "bimonth":
                            ConversionValue = 2.0;
                            break;
                        case "week":
                            ConversionValue = (DaysInYear/MonthsInYear)/DaysInWeek;
                            break;
                        case "day":
                            ConversionValue = (DaysInYear / MonthsInYear);
                            break;
                        case "second":
                            ConversionValue = (DaysInYear / MonthsInYear) * 24.0 * 60.0 * 60.0;
                            break;
                        default:
                            Debug.Fail("Requested combination of time units not currently supported");
                            ConversionValue =  0;
                            break;
                    }
                    break;
                case "bimonth":
                    switch (toUnit.ToLower())
                    {
                        case "year":
                            ConversionValue = 1.0 / (MonthsInYear*2);
                            break;
                        case "month":
                            ConversionValue = 1 / 2.0;
                            break;
                        case "bimonth":
                            ConversionValue = 1.0;
                            break;
                        case "week":
                            ConversionValue = (DaysInYear / (MonthsInYear * 2)) / DaysInWeek;
                            break;
                        case "day":
                            ConversionValue = (DaysInYear / (MonthsInYear * 2));
                            break;
                        case "second":
                            ConversionValue = (DaysInYear / (MonthsInYear * 2)) * 24.0 * 60.0 * 60.0;
                            break;
                        default:
                            Debug.Fail("Requested combination of time units not currently supported");
                            ConversionValue = 0;
                            break;
                    }
                    break;

                case "week":
                    switch (toUnit.ToLower())
                    {
                        case "year":
                            ConversionValue = DaysInWeek/DaysInYear;
                            break;
                        case "month":
                            ConversionValue = DaysInWeek/(DaysInYear / MonthsInYear);
                            break;
                        case "bimonth":
                            ConversionValue = DaysInWeek / (DaysInYear / (MonthsInYear*2));
                            break;
                        case "week":
                            ConversionValue = 1.0;
                            break;
                        case "day":
                            ConversionValue = DaysInWeek;
                            break;
                        case "second":
                            ConversionValue = DaysInWeek * 24.0 * 60.0 * 60.0;
                            break;
                        default:
                            Debug.Fail("Requested combination of time units not currently supported");
                            ConversionValue = 0;
                            break;
                    }
                    break;
                case "day":
                    switch (toUnit.ToLower())
                    {
                        case "year":
                            ConversionValue =  1.0 / DaysInYear;
                            break;
                        case "month":
                            ConversionValue = 1.0 / (DaysInYear / MonthsInYear);
                            break;
                        case "bimonth":
                            ConversionValue = 1.0 / (DaysInYear / (MonthsInYear * 2));
                            break;
                        case "week":
                            ConversionValue = 1.0 / DaysInWeek;
                            break;
                        case "day":
                            ConversionValue =  1.0;
                            break;
                        default:
                            Debug.Fail("Requested combination of time units not currently supported");
                            ConversionValue =  0;
                            break;
                    }
                    break;
                default:
                    Debug.Fail("Requested combination of time units not currently supported");
                    ConversionValue =  0;
                    break;
            }

            // Return the conversion factor
            return ConversionValue;
        }


        /// <summary>
        /// For a given cohort index, return a vector pair of values corresponding to the cohort's location in the jagged array of grid cell cohorts
        /// </summary>
        /// <param name="valueToFind">The index of the cohort (values range between zero and the number of cohorts in the jagged arrray)</param>
        /// <param name="arrayToSearch">The jaggged array of cohorts, where rows correspond to functional groups, and columns to cohorts within functional groups</param>
        /// <param name="totalNumberOfCohorts">The total number of cohorts in the grid cell</param>
        /// <returns>The position of the specified cohort in the jagged array of grid cell cohorts, where the first value is the row index (functional group) and the second value is the column index (position within functional group)</returns>
        public int[] FindJaggedArrayIndex(uint valueToFind, uint[][] arrayToSearch, uint totalNumberOfCohorts)
        {
            // Create a vector to hold the location of the cohort in the jagged array
            int[] ValueLocation = new int[2];

            // Check to make sure that specified cohort index is not greater than the total number of cohorts
            Debug.Assert(valueToFind <= totalNumberOfCohorts, "Value searched for in jagged array is bigger than the biggest value in the jagged array");

            // Variables to hold the row and colum indices of the cohort in the jaggged array
            int RowIndex = 0;
            int ColumnIndex = 0;

            // Loop over rows (functional groups) and locate the one in which the specified cohort is located
            while (arrayToSearch[RowIndex].Length == 0 || valueToFind > arrayToSearch[RowIndex][arrayToSearch[RowIndex].Length - 1])
            {
                RowIndex++;
            }

            // Add the located row to the vector of values to return
            ValueLocation[0] = RowIndex;

            // Loop over columns (cohorts within the functional group) and locate the one in which the specified cohort is located
            while (valueToFind != arrayToSearch[RowIndex][ColumnIndex])
            {
                ColumnIndex++;
            }

            // Add the located column to the vector of values to return
            ValueLocation[1] = ColumnIndex;

            // Return the vector of two values correpsonding to the located position in the jagged array of grid cell cohorts
            return ValueLocation;

        }
        
        /// <summary>Converts values per square km to per square degree, given cell latitude</summary>
        /// <param name="valueToConvert">The value per square km</param>
        /// <param name="latitude">The latitude of the grid cell</param>
        /// <returns>The specified value converted to per square degree </returns>
        public double ConvertSqMToSqDegrees(double valueToConvert, double latitude)
        {
            // Convert the value to per sqaure degree using the cosine of latitude and assuming cell dimensions of 110km by 110km at the Equator
            return valueToConvert * 110000.0 * 110000.0 * Math.Cos(DegreesToRadians(latitude));
        }
        
        /// <summary>
        /// Calculates the probability of a particular value under a log-normal distribution with specified mean and standard deviation
        /// </summary>
        /// <param name="xValue">The value to return the probability of under the log-normal distribtuion, in identity space</param>
        /// <param name="meanIdentity">The mean of the log-normal distribution, in identity space</param>
        /// <param name="standardDeviation">The standard deviation of the log-normal distribution, in log space</param>
        /// <returns>The probability of the specified value under the specified log-normal distribution</returns>
        public double LogNormalPDF(double xValue, double meanIdentity, double standardDeviation)
        {
            // Calculate the mean of the log-normal distribution in log space
            double meanLog = Math.Log(meanIdentity);
            // Calculate and return the probability of the specified value under the specified log-normal distribution
            return (1 / Math.Sqrt(2*Math.PI * Math.Pow(standardDeviation,2)))*Math.Exp(-(Math.Pow(Math.Log(xValue)-meanLog,2)/(2*Math.Pow(standardDeviation,2))));
        }

        /// <summary>
        /// Calculates the probability of a particular value under a normal distribution with specified mean and standard deviation
        /// </summary>
        /// <param name="xValue">The value to return the probability of under the normal distribtuion</param>
        /// <param name="meanValue">The mean of the normal distribution</param>
        /// <param name="standardDeviation">The standard deviation of the normal distribution</param>
        /// <returns>The probability of the specified value under the specified normal distribution</returns>
        public double NormalPDF(double xValue, double meanValue, double standardDeviation)
        {
            // Calculate and return the probability of the specified value under the specified normal distribution
            return (1 / Math.Sqrt(2 * Math.PI * Math.Pow(standardDeviation, 2))) * Math.Exp(-(Math.Pow(xValue - meanValue, 2) / (2 * Math.Pow(standardDeviation, 2))));
        }

        /// <summary>
        /// Calculate the area of a grid cell in square km, given its dimensions and geographical position
        /// </summary>
        /// <param name="latitude">The latitude of the bottom-left corner of the grid cell</param>
        /// <param name="lonCellSize">The longitudinal dimension of the grid cell</param>
        /// <param name="latCellSize">The latitudinal dimension of the grid cell</param>
        /// <returns>The area in square km of the grid cell</returns>
        public double CalculateGridCellArea(double latitude, double lonCellSize, double latCellSize)
        {
            // Convert from degrees to radians
            double latitudeRad = DegreesToRadians(latitude);

            // Equatorial radius in metres
            double EquatorialRadius = 6378137;

            // Polar radius in metres
            double PolarRadius = 6356752.3142;

            // Angular eccentricity
            double AngularEccentricity = Math.Acos(DegreesToRadians(PolarRadius / EquatorialRadius));

            // First eccentricity squared
            double ESquared = Math.Pow(Math.Sin(DegreesToRadians(AngularEccentricity)), 2);

            // Flattening
            double Flattening = 1 - Math.Cos(DegreesToRadians(AngularEccentricity));

            // Temporary value to save computations
            double TempVal = Math.Pow((EquatorialRadius * Math.Cos(latitudeRad)),2) + Math.Pow((PolarRadius * Math.Sin(latitudeRad)),2);

            // Meridional radius of curvature
            double MPhi = Math.Pow(EquatorialRadius * PolarRadius,2) / Math.Pow(TempVal,1.5);
            
            // Normal radius of curvature
            double NPhi = Math.Pow(EquatorialRadius,2) / Math.Sqrt(TempVal);

            // Length of latitude (km)
            double LatitudeLength = Math.PI / 180 * MPhi / 1000;

            // Length of longitude (km)
            double LongitudeLength = Math.PI / 180 * Math.Cos(latitudeRad) * NPhi / 1000;

            // Return the cell area in km^2
            return LatitudeLength * latCellSize * LongitudeLength * lonCellSize;
        }

        /// <summary>
        /// Calculate the length of a degree of latitude at a particular latitude
        /// </summary>
        /// <param name="latitude">The latitude of the bottom-left corner of the grid cell</param>
        /// <returns>The length of a degree of latitude in kilometres</returns>
        public double CalculateLengthOfDegreeLatitude(float latitude)
        {
            // Convert from degrees to radians
            double latitudeRad = DegreesToRadians(latitude);

            // Equatorial radius in metres
            double EquatorialRadius = 6378137;

            // Polar radius in metres
            double PolarRadius = 6356752.3142;

            // Angular eccentricity
            double AngularEccentricity = Math.Acos(DegreesToRadians(PolarRadius / EquatorialRadius));

            // First eccentricity squared
            double ESquared = Math.Pow(Math.Sin(DegreesToRadians(AngularEccentricity)), 2);

            // Flattening
            double Flattening = 1 - Math.Cos(DegreesToRadians(AngularEccentricity));

            // Temporary value to save computations
            double TempVal = Math.Pow((EquatorialRadius * Math.Cos(latitudeRad)), 2) + Math.Pow((PolarRadius * Math.Sin(latitudeRad)), 2);

            // Meridional radius of curvature
            double MPhi = Math.Pow(EquatorialRadius * PolarRadius, 2) / Math.Pow(TempVal, 1.5);

            // Length of latitude (km)
            return Math.PI / 180 * MPhi / 1000;
        }

        /// <summary>
        /// Calculate the length of a degree of longitude at a particular latitude
        /// </summary>
        /// <param name="latitude">The latitude of the bottom-left corner of the grid cell</param>
        /// <returns>The length of a degree of longitude in kilometres</returns>
        public double CalculateLengthOfDegreeLongitude(float latitude)
        {
            // Convert from degrees to radians
            double latitudeRad = DegreesToRadians(latitude);

            // Equatorial radius in metres
            double EquatorialRadius = 6378137;

            // Polar radius in metres
            double PolarRadius = 6356752.3142;

            // Temporary value to save computations
            double TempVal = Math.Pow((EquatorialRadius * Math.Cos(latitudeRad)), 2) + Math.Pow((PolarRadius * Math.Sin(latitudeRad)), 2);

            // Normal radius of curvature
            double NPhi = Math.Pow(EquatorialRadius, 2) / Math.Sqrt(TempVal);

            // Length of longitude (km)
            return Math.PI / 180 * Math.Cos(latitudeRad) * NPhi / 1000;
        }

        /// <summary>
        /// Convert from degrees to radians
        /// </summary>
        /// <param name="degrees">The value in degrees to convert</param>
        /// <returns>The value converted to radians</returns>
        public double DegreesToRadians(double degrees)
        {
            return (degrees * Math.PI / 180.0);
        }

    }
}
