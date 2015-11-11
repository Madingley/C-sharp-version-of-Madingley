using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Madingley
{
    /// <summary>
    /// An abstract class to implement common methods that are used across all dispersal classes.
    /// </summary>
    public abstract class CommonDispersalMethods
    {

        /// <summary>
        /// An instance of the simple random number generator class
        /// </summary>
        private NonStaticSimpleRNG RandomNumberGenerator = new NonStaticSimpleRNG();


        /// <summary>
        /// Generate a random value to see if a cohort disperses
        /// </summary>
        /// <param name="dispersalProbability">The probability of dispersal</param>
        /// <returns>Returns either the random value, if it less than dispersal probability, or -1</returns>
        protected double CheckForDispersal(double dispersalProbability)
        {
            // Randomly check to see if dispersal occurs
            double RandomValue = RandomNumberGenerator.GetUniform();
            if (dispersalProbability >= RandomValue)
            {
                return RandomValue;
            }
            else
            {
                return -1.0;
            }
        }

        // Determine to which cell the cohort disperses
        /// <summary>
        /// Determines the cell to which a cohort disperses
        /// </summary>
        /// <param name="madingleyGrid">The ecosystem model grid</param>
        /// <param name="latIndex">The latitudinal index of the cell being run</param>
        /// <param name="lonIndex">The longitudinal index of the cell being run</param>
        /// <param name="dispersalArray"></param>
        /// <param name="RandomValue"></param>
        /// <param name="uSpeedIncDiffusion"></param>
        /// <param name="vSpeedIncDiffusion"></param>
        /// <param name="exitDirection"></param>
        /// <param name="entryDirection"></param>
        /// <returns></returns>
        protected uint[] CellToDisperseTo(ModelGrid madingleyGrid, uint latIndex, uint lonIndex, double[] dispersalArray, 
            double RandomValue, double uSpeedIncDiffusion, double vSpeedIncDiffusion, ref uint exitDirection, 
            ref uint entryDirection)
        {
            uint[] DestinationCell;

            // Check to see in which axis the cohort disperses

            // Note that the values in the dispersal array are the proportional area moved outside the grid cell in each direction; we simply compare the random draw to this
            // to determine the direction in which the cohort moves probabilistically

            // Longitudinally
            if (RandomValue <= dispersalArray[1])
            {
                // Work out whether dispersal is to the cell to the E or the W
                if (uSpeedIncDiffusion > 0)
                {

                    DestinationCell = madingleyGrid.CheckDispersalEast(latIndex, lonIndex);

                        // Record entry and exit directions. Exit direction is only recorded the first time it happens during a (model) timestep, not each advection time step.
                        if (exitDirection == 9999)
                            exitDirection = 2;
                        entryDirection = 6;



                }
                else
                {
                    DestinationCell = madingleyGrid.CheckDispersalWest(latIndex, lonIndex);

                    // Record entry and exit directions. Exit direction is only recorded the first time it happens during a (model) timestep, not each advection time step.
                    if (exitDirection == 9999)
                        exitDirection = 6;
                    entryDirection = 2;
                }

            }
            else
            {
                // Latitudinally
                if (RandomValue <= (dispersalArray[1] + dispersalArray[2]))
                {
                    // Work out whether dispersal is to the cell to the N or the S
                    if (vSpeedIncDiffusion > 0)
                    {
                        DestinationCell = madingleyGrid.CheckDispersalNorth(latIndex, lonIndex);

                        // Record entry and exit directions. Exit direction is only recorded the first time it happens during a (model) timestep, not each advection time step.
                        if (exitDirection == 9999)
                            exitDirection = 0;
                        entryDirection = 4;

                    }
                    else
                    {
                        DestinationCell = madingleyGrid.CheckDispersalSouth(latIndex, lonIndex);

                        // Record entry and exit directions. Exit direction is only recorded the first time it happens during a (model) timestep, not each advection time step.
                        if (exitDirection == 9999)
                            exitDirection = 4;
                        entryDirection = 0;
                    }

                }
                else
                {
                    // Diagonally. Note that DispersalArray[0] is equal to dispersalArray[1] + dispersalArray[2] + dispersalArray[3], but it 
                    // is both faster to compare and also avoids any rounding errors.
                    if (RandomValue <= (dispersalArray[0]))
                    {
                        // Work out to which cell dispersal occurs
                        if (uSpeedIncDiffusion > 0)
                        {
                            if (vSpeedIncDiffusion > 0)
                            {
                                DestinationCell = madingleyGrid.CheckDispersalNorthEast(latIndex, lonIndex);

                                // Record entry and exit directions. Exit direction is only recorded the first time it happens during a (model) timestep, not each advection time step.
                                if (exitDirection == 9999)
                                    exitDirection = 1;
                                entryDirection = 5;
                            }
                            else
                            {
                                DestinationCell = madingleyGrid.CheckDispersalSouthEast(latIndex, lonIndex);

                                // Record entry and exit directions. Exit direction is only recorded the first time it happens during a (model) timestep, not each advection time step.
                                if (exitDirection == 9999)
                                    exitDirection = 5;
                                entryDirection = 1;
                            }

                        }
                        else
                        {
                            if (vSpeedIncDiffusion > 0)
                            {
                                DestinationCell = madingleyGrid.CheckDispersalNorthWest(latIndex, lonIndex);

                                // Record entry and exit directions. Exit direction is only recorded the first time it happens during a (model) timestep, not each advection time step.
                                if (exitDirection == 9999)
                                    exitDirection = 7;
                                entryDirection = 3;
                            }
                            else
                            {
                                DestinationCell = madingleyGrid.CheckDispersalSouthWest(latIndex, lonIndex);

                                // Record entry and exit directions. Exit direction is only recorded the first time it happens during a (model) timestep, not each advection time step.
                                if (exitDirection == 9999)
                                    exitDirection = 3;
                                entryDirection = 7;
                            }
                        }
                    }
                    else
                    {
                        // This should never happen. Means that the random number indicates dispersal by being lower than the probability, but
                        // that in the comparison abive, it is higher than the probability.
                        Debug.Fail("Error when determining which cell to disperse to");
                        Console.WriteLine("Error when determining which cell to disperse to");
                        Console.ReadKey();
                        DestinationCell = new uint[2] { 9999999, 9999999 };
                    }
                }

            }
            return DestinationCell;
        }
    }
}
