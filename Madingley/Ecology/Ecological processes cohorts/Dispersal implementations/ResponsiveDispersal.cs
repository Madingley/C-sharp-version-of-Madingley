using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Madingley
{
    /// <summary>
    /// A formulation of the process of dispersal
    /// </summary>
    public partial class ResponsiveDispersal : CommonDispersalMethods , IDispersalImplementation
    {
        /// <summary>
        /// The time units associated with this implementation of dispersal
        /// </summary>
        private string _TimeUnitImplementation;
        /// <summary>
        /// Get the time units associated with this implementation of dispersal
        /// </summary>
        public string TimeUnitImplementation { get { return _TimeUnitImplementation; } }

        /// <summary>
        /// Density threshold below which adult individuals may move to look for other adults of the same cohort
        /// </summary>
        /// <remarks>The density scales with cohort weight via: Min Density = DensityThresholdScaling / MatureMass (g)</remarks>
        private double _DensityThresholdScaling;
        /// <summary>
        /// Get the density threshold below which adult individuals may move to look for other adults of the same cohort
        /// </summary>
        public double DensityThresholdScaling { get { return _DensityThresholdScaling; } }

        /// <summary>
        /// The proportion of body mass loss at which the cohort will try to disperse every time during a time step
        /// </summary>
        private double _StarvationDispersalBodyMassThreshold;
        /// <summary>
        /// Get the proportion of body mass loss at which the cohort will try to disperse every time during a time step
        /// </summary>
        public double StarvationDispersalBodyMassThreshold { get { return _StarvationDispersalBodyMassThreshold; } }

        /// <summary>
        /// Scalar relating dispersal speed to individual body mass
        /// </summary>
        private double _DispersalSpeedBodyMassScalar;
        /// <summary>
        /// Get the scalar relating dispersal speed to individual body mass
        /// </summary>
        public double DispersalSpeedBodyMassScalar { get { return _DispersalSpeedBodyMassScalar; } }


        /// <summary>
        /// Body-mass exponent of the relationship between disperal speed and individual body mass
        /// </summary>
        private double _DispersalSpeedBodyMassExponent;
        /// <summary>
        /// Get the body-mass exponent of the relationship between disperal speed and individual body mass
        /// </summary>
        public double DispersalSpeedBodyMassExponent { get { return _DispersalSpeedBodyMassExponent; } }

        /// <summary>
        /// Calculate the average diffusive dispersal speed of individuals in a cohort given their body mass
        /// </summary>
        /// <param name="bodyMass">The current body mass of an individual in the cohort</param>
        /// <returns>The (average) dispersal speed in kilometres per month</returns>
        protected double CalculateDispersalSpeed(double bodyMass)
        {
            return _DispersalSpeedBodyMassScalar * Math.Pow(bodyMass, _DispersalSpeedBodyMassExponent);
        }


        public void InitialiseParametersResponsiveDispersal()
        {
            _TimeUnitImplementation =
                EcologicalParameters.TimeUnits[(int)EcologicalParameters.Parameters["Dispersal.Responsive.TimeUnitImplementation"]];
            _DensityThresholdScaling = EcologicalParameters.Parameters["Dispersal.Responsive.DensityThresholdScaling"];
            _StarvationDispersalBodyMassThreshold = EcologicalParameters.Parameters["Dispersal.Responsive.StarvationDispersalBodyMassThreshold"];
            _DispersalSpeedBodyMassScalar = EcologicalParameters.Parameters["Dispersal.Responsive.DispersalSpeedBodyMassScalar"];
            _DispersalSpeedBodyMassExponent = EcologicalParameters.Parameters["Dispersal.Responsive.DispersalSpeedBodyMassExponent"];
        }


        /// <summary>
        /// Write out the values of the parameters to an output file
        /// </summary>
        /// <param name="sw">A streamwriter object to write the parameter values to</param>
        public void WriteOutParameterValues(StreamWriter sw)
        {
            // Write out parameters
            sw.WriteLine("Responsive Dispersal\tTimeUnitImplementation\t" + Convert.ToString(_TimeUnitImplementation));
            sw.WriteLine("Responsive Dispersal\t_DispersalSpeedBodyMassScalar\t" + Convert.ToString(DispersalSpeedBodyMassScalar));
            sw.WriteLine("Responsive Dispersal\t_DispersalSpeedBodyMassExponent\t" + Convert.ToString(DispersalSpeedBodyMassExponent));
            sw.WriteLine("Responsive Dispersal\tTDensityThresholdScaling\t" + Convert.ToString(_DensityThresholdScaling));
            sw.WriteLine("Responsive Dispersal\tStarvationDispersalBodyMassThreshold\t" + Convert.ToString(_StarvationDispersalBodyMassThreshold));
        }

        private bool CheckStarvationDispersal(ModelGrid gridForDispersal, uint latIndex, uint lonIndex, Cohort cohortToDisperse, int functionalGroup, int cohortNumber)
        {
            // A boolean to check whether a cohort has dispersed
            bool CohortHasDispersed = false;

            // Check for starvation driven dispersal
            // What is the present body mass of the cohort?
            // Note that at present we are just tracking starvation for adults
            double IndividualBodyMass = cohortToDisperse.IndividualBodyMass;
            double AdultMass = cohortToDisperse.AdultMass;

            // Temporary variables to keep track of directions in which cohorts enter/exit cells during the multiple advection steps per time step
            uint ExitDirection = new uint();
            uint EntryDirection = new uint();
            ExitDirection = 9999;

            // Assume a linear relationship between probability of dispersal and body mass loss, up to _StarvationDispersalBodyMassThreshold
            // at which point the cohort will try to disperse every time step
            if (IndividualBodyMass < AdultMass)
            {
                double ProportionalPresentMass = IndividualBodyMass / AdultMass;

                // If the body mass loss is greater than the starvation dispersal body mass threshold, then the cohort tries to disperse
                if (ProportionalPresentMass < _StarvationDispersalBodyMassThreshold)
                {
                    // Cohort tries to disperse
                    double[] DispersalArray = CalculateDispersalProbability(gridForDispersal, latIndex, lonIndex, CalculateDispersalSpeed(AdultMass));
                    double CohortDispersed = CheckForDispersal(DispersalArray[0]);
                    if (CohortDispersed > 0)
                    {
                        uint[] DestinationCell = CellToDisperseTo(gridForDispersal, latIndex, lonIndex, DispersalArray, CohortDispersed, DispersalArray[4], DispersalArray[5], ref ExitDirection, ref EntryDirection);
                        
                        // Update the delta array of cells to disperse to, if the cohort moves
                        if (DestinationCell[0] < 999999)
                        {
                            // Update the delta array of cohorts
                            gridForDispersal.DeltaFunctionalGroupDispersalArray[latIndex, lonIndex].Add((uint)functionalGroup);
                            gridForDispersal.DeltaCohortNumberDispersalArray[latIndex, lonIndex].Add((uint)cohortNumber);
                        
                            // Update the delta array of cells to disperse to
                            gridForDispersal.DeltaCellToDisperseToArray[latIndex, lonIndex].Add(DestinationCell);

                            // Update the delta array of exit and entry directions
                            gridForDispersal.DeltaCellExitDirection[latIndex, lonIndex].Add(ExitDirection);
                            gridForDispersal.DeltaCellEntryDirection[latIndex, lonIndex].Add(EntryDirection);
                        }
                    }

                    // Note that regardless of whether or not it succeeds, if a cohort tries to disperse, it is counted as having dispersed for 
                    // the purposes of not then allowing it to disperse based on its density.
                    CohortHasDispersed = true;
                }

                // Otherwise, the cohort has a chance of trying to disperse proportional to its mass loss
                else
                {
                    // Cohort tries to disperse with a particular probability
                    // Draw a random number
                    if (((1.0 - ProportionalPresentMass) / (1.0 - _StarvationDispersalBodyMassThreshold)) > RandomNumberGenerator.GetUniform())
                    {
                        // Cohort tries to disperse
                        double[] DispersalArray = CalculateDispersalProbability(gridForDispersal, latIndex, lonIndex, CalculateDispersalSpeed(AdultMass));
                        double CohortDispersed = CheckForDispersal(DispersalArray[0]);
                        if (CohortDispersed > 0)
                        {
                            uint[] DestinationCell = CellToDisperseTo(gridForDispersal, latIndex, lonIndex, DispersalArray, CohortDispersed, DispersalArray[4], DispersalArray[5], ref ExitDirection, ref EntryDirection);

                            // Update the delta array of cells to disperse to, if the cohort moves
                            if (DestinationCell[0] < 999999)
                            {
                                // Update the delta array of cohorts
                                gridForDispersal.DeltaFunctionalGroupDispersalArray[latIndex, lonIndex].Add((uint)functionalGroup);
                                gridForDispersal.DeltaCohortNumberDispersalArray[latIndex, lonIndex].Add((uint)cohortNumber);

                                // Update the delta array of cells to disperse to
                                gridForDispersal.DeltaCellToDisperseToArray[latIndex, lonIndex].Add(DestinationCell);

                                // Update the delta array of exit and entry directions
                                gridForDispersal.DeltaCellExitDirection[latIndex, lonIndex].Add(ExitDirection);
                                gridForDispersal.DeltaCellEntryDirection[latIndex, lonIndex].Add(EntryDirection);
                            }
                        }

                        CohortHasDispersed = true;
                    }
                }

            }
            return CohortHasDispersed;
        }

        private void CheckDensityDrivenDispersal(ModelGrid gridForDispersal, uint latIndex, uint lonIndex, Cohort cohortToDisperse, int functionalGroup, int cohortNumber)
        {
            // Check the population density
            double NumberOfIndividuals = cohortToDisperse.CohortAbundance;

            // Get the cell area, in kilometres squared
            double CellArea = gridForDispersal.GetCellEnvironment(latIndex, lonIndex)["Cell Area"][0];

            // If below the density threshold
            if ((NumberOfIndividuals / CellArea) < _DensityThresholdScaling / cohortToDisperse.AdultMass)
            {
                // Temporary variables to keep track of directions in which cohorts enter/exit cells during the multiple advection steps per time step
                uint ExitDirection = new uint();
                uint EntryDirection = new uint();
                ExitDirection = 9999;

                // Check to see if it disperses (based on the same movement scaling as used in diffusive movement)
                // Calculate dispersal speed for that cohort
                double DispersalSpeed = CalculateDispersalSpeed(cohortToDisperse.IndividualBodyMass);

                // Cohort tries to disperse
                double[] DispersalArray = CalculateDispersalProbability(gridForDispersal, latIndex, lonIndex, CalculateDispersalSpeed(cohortToDisperse.AdultMass));
                
                double CohortDispersed = CheckForDispersal(DispersalArray[0]);
                
                if (CohortDispersed > 0)
                {
                    uint[] DestinationCell = CellToDisperseTo(gridForDispersal, latIndex, lonIndex, DispersalArray, CohortDispersed, DispersalArray[4], DispersalArray[5], ref ExitDirection, ref EntryDirection);

                    // Update the delta array of cells to disperse to, if the cohort moves
                    if (DestinationCell[0] < 999999)
                    {
                        // Update the delta array of cohorts
                        gridForDispersal.DeltaFunctionalGroupDispersalArray[latIndex, lonIndex].Add((uint)functionalGroup);
                        gridForDispersal.DeltaCohortNumberDispersalArray[latIndex, lonIndex].Add((uint)cohortNumber);

                        // Update the delta array of cells to disperse to
                        gridForDispersal.DeltaCellToDisperseToArray[latIndex, lonIndex].Add(DestinationCell);

                        // Update the delta array of exit and entry directions
                        gridForDispersal.DeltaCellExitDirection[latIndex, lonIndex].Add(ExitDirection);
                        gridForDispersal.DeltaCellEntryDirection[latIndex, lonIndex].Add(EntryDirection);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the probability of responsive dispersal given average individual dispersal speed and grid cell
        /// </summary>
        /// <param name="madingleyGrid">The model grid</param>
        /// <param name="latIndex">The latitude index of the grid cell to check for dispersal</param>
        /// <param name="lonIndex">The longitude index of the grid cell to check for dispersal</param>
        /// <param name="dispersalSpeed">The average dispersal speed of individuals in the acting cohort</param>
        /// <returns>A six element array. 
        /// The first element is the probability of dispersal.
        /// The second element is the probability of dispersing in the u (longitudinal) direction
        /// The third element is the probability of dispersing in the v (latitudinal) direction
        /// The fourth element is the probability of dispersing in the diagonal direction
        /// The fifth element is the u velocity
        /// The sixth element is the v velocity
        /// Note that the second, third, and fourth elements are always positive; thus, they do not indicate 'direction' in terms of dispersal.</returns>
        protected double[] CalculateDispersalProbability(ModelGrid madingleyGrid, uint latIndex, uint lonIndex, double dispersalSpeed)
        {
            double LatCellLength = madingleyGrid.CellHeightsKm[latIndex];
            double LonCellLength = madingleyGrid.CellWidthsKm[latIndex];

            // Pick a direction at random
            double RandomDirection = RandomNumberGenerator.GetUniform() * 2 * Math.PI;

            // Calculate the u and v components given the dispersal speed
            double uSpeed = dispersalSpeed * Math.Cos(RandomDirection);
            double vSpeed = dispersalSpeed * Math.Sin(RandomDirection);

            // Check that the whole cell hasn't moved out (i.e. that dispersal speed is not greater than cell length). 
            // This could happen if dispersal speed was high enough; indicates a need to adjust the time step, or to slow dispersal
            if ((uSpeed > LonCellLength) || (vSpeed > LatCellLength))
            {
                Debug.Fail("Dispersal probability should always be <= 1");
            }

            // Calculate the area of the grid cell that is now outside in the diagonal direction
            double AreaOutsideBoth = Math.Abs(uSpeed * vSpeed);

            // Calculate the area of the grid cell that is now outside in the u direction (not including the diagonal)
            double AreaOutsideU = Math.Abs(uSpeed * LatCellLength) - AreaOutsideBoth;

            // Calculate the proportion of the grid cell that is outside in the v direction (not including the diagonal
            double AreaOutsideV = Math.Abs(vSpeed * LonCellLength) - AreaOutsideBoth;

            // Get the cell area, in kilometres squared
            double CellArea = madingleyGrid.GetCellEnvironment(latIndex, lonIndex)["Cell Area"][0];

            // Convert areas to a probability
            double DispersalProbability = (AreaOutsideU + AreaOutsideV + AreaOutsideBoth) / CellArea;

            // Check that we don't have any issues
            if (DispersalProbability > 1)
            {
                //Debug.Fail("Dispersal probability should always be <= 1");
                DispersalProbability = 1.0;
            }

            double[] NewArray = { DispersalProbability, AreaOutsideU / CellArea, AreaOutsideV / CellArea, AreaOutsideBoth / CellArea, uSpeed, vSpeed };

            return NewArray;
        }

  

    }
}
