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
    public partial class AdvectiveDispersal : CommonDispersalMethods , IDispersalImplementation
    {
        /// <summary>
        /// The horizontal diffusivity parameter (m^2/s)
        /// </summary>
        private double _HorizontalDiffusivity;
        /// <summary>
        /// Get the horizontal diffusivity parameter (m^2/s)
        /// </summary>
        public double HorizontalDiffusivity { get { return _HorizontalDiffusivity; } }
        
        /// <summary>
        /// The length of the time-step for advective dispersal, in hours
        /// </summary>
        private uint _AdvectiveModelTimeStepLengthHours;
        /// <summary>
        /// Get the length in hours of the time-step for advective dispersal
        /// </summary>
        public uint AdvectiveModelTimeStepLengthHours { get { return _AdvectiveModelTimeStepLengthHours; } }

        /// <summary>
        /// Horizontal diffusivity in km^2/advective-dispersal-time-step
        /// </summary>
        private double _HorizontalDiffusivityKmSqPerADTimeStep;
        /// <summary>
        /// Get the horizontal diffusivity in km^2/advective-dispersal-time-step
        /// </summary>
        public double HorizontalDiffusivityKmSqPerADTimeStep { get { return _HorizontalDiffusivityKmSqPerADTimeStep; } }

        /// <summary>
        /// Time unit scalar to apply to advective dispersal
        /// </summary>
        private double _AdvectionTimeStepsPerModelTimeStep;
      
        /// <summary>
        /// Get the time unit scalar for advective dispersal
        /// </summary>
        public double AdvectionTimeStepsPerModelTimeStep { get { return _AdvectionTimeStepsPerModelTimeStep; } }

        /// <summary>
        /// The time units associated with this implementation of dispersal
        /// </summary>
        private string _TimeUnitImplementation;
        /// <summary>
        /// Get the time units associated with this implementation of dispersal
        /// </summary>
        public string TimeUnitImplementation { get { return _TimeUnitImplementation; } }    

        /// <summary>
        /// Factor to convert velocity from m/s to km/month
        /// </summary>
        private static double VelocityUnitConversion;

        /// <summary>
        /// Initialises the parameters used for advective dispersal
        /// </summary>
        public void InitialiseParatemersAdvectiveDispersal()
        {
            _TimeUnitImplementation = 
                EcologicalParameters.TimeUnits[(int)EcologicalParameters.Parameters["Dispersal.Advective.TimeUnitImplementation"]];
            _HorizontalDiffusivity = EcologicalParameters.Parameters["Dispersal.Advective.HorizontalDiffusivity"];
            _AdvectiveModelTimeStepLengthHours = (uint)EcologicalParameters.Parameters["Dispersal.Advective.AdvectiveModelTimeStepLengthHours"];

            _HorizontalDiffusivityKmSqPerADTimeStep = _HorizontalDiffusivity / (1000 * 1000) * 60 * 60 * _AdvectiveModelTimeStepLengthHours;
        }


        /// <summary>
        /// Write out the values of the parameters to an output file
        /// </summary>
        /// <param name="sw">A streamwriter object to write the parameter values to</param>
        public void WriteOutParameterValues(StreamWriter sw)
        {
            // Write out parameters
            sw.WriteLine("Advective Dispersal\tTimeUnitImplementation\t" + Convert.ToString(_TimeUnitImplementation));
            sw.WriteLine("Advective Dispersal\tHorizontalDiffusivity\t" + Convert.ToString(_HorizontalDiffusivity));
            sw.WriteLine("Advective Dispersal\tAdvectivedispersalTemporalScaling\t" + Convert.ToString(_AdvectionTimeStepsPerModelTimeStep));
            sw.WriteLine("Advective Dispersal\tVelocityUnitConversion\t" + Convert.ToString(VelocityUnitConversion));
        }


        /// <summary>
        /// Convert dispersal speed from m per second to km per dispersal time step (currently 18h)
        /// </summary>
        /// <param name="dispersalSpeed">The dispersal speed in m per second</param>
        /// <returns>The dispersal speed in kilometres per time step</returns>
        private double RescaleDispersalSpeed(double dispersalSpeed)
        {
            // Units are metres per second; need to convert to kilometres per global time step (currently one month) - use VelocityUnitConversion for this.
            // Also rescale based on the time step of the advective dispersal model - currently 18h
            return dispersalSpeed * VelocityUnitConversion / _AdvectionTimeStepsPerModelTimeStep;
        }
        
        /// <summary>
        /// Calculates the probability of advective dispersal given the grid cell
        /// </summary>
        /// <param name="madingleyGrid">The model grid</param>
        /// <param name="latIndex">The latitude index of the grid cell to check for dispersal</param>
        /// <param name="lonIndex">The longitude index of the grid cell to check for dispersal</param>
        /// <param name="currentMonth">The current model month</param>
        /// <returns>A six element array. 
        /// The first element is the probability of dispersal.
        /// The second element is the probability of dispersing in the u (longitudinal) direction
        /// The third element is the probability of dispersing in the v (latitudinal) direction
        /// The fourth element is the probability of dispersing in the diagonal direction
        /// The fifth element is the distance travelled in the u direction (u velocity modified by the random diffusion component)
        /// The sixth element is the distance travelled in the v direction (v velocity modified by the random diffusion component)
        /// Note that the second, third, and fourth elements are always positive; thus, they do not indicate 'direction' in terms of dispersal.</returns>
        private double[] CalculateDispersalProbability(ModelGrid madingleyGrid, uint latIndex, uint lonIndex, uint currentMonth, double rescaleduSpeed, double rescaledvSpeed)
        {

        // Distance travelled in u (longitudinal) direction
         double uDistanceTravelled;

        // Distance travelled in v (latitudinal) direction
         double vDistanceTravelled;

        // U and V components of the diffusive velocity
        double[] DiffusiveUandVComponents = new double[2];

         // Length in km of a cell boundary latitudinally
         double LatCellLength;

         // Length in km of a cell boundary longitudinally
         double LonCellLength;
         
         // Area of the grid cell that is outside in the diagonal direction after dispersal, in kilometres squared
         double AreaOutsideBoth;

         // Area of the grid cell that is  outside in the u (longitudinal) direction after dispersal, in kilometres squared
         double AreaOutsideU;

         // Area of the grid cell that is  outside in the v (latitudinal) direction after dispersal, in kilometres squared
         double AreaOutsideV;
            
         // Cell area, in kilometres squared
         double CellArea;

        // Probability of dispersal
         double DispersalProbability;
            
            // Calculate the diffusive movement speed, with a direction chosen at random
            DiffusiveUandVComponents = CalculateDiffusion();

            // Calculate the distance travelled in this dispersal (not global) time step. both advective and diffusive speeds need to have been converted to km / advective model time step
            uDistanceTravelled = rescaleduSpeed + DiffusiveUandVComponents[0];
            vDistanceTravelled = rescaledvSpeed + DiffusiveUandVComponents[1];

            // Check that the u distance travelled and v distance travelled are not greater than the cell length
            LatCellLength = madingleyGrid.CellHeightsKm[latIndex];
            LonCellLength = madingleyGrid.CellWidthsKm[latIndex];

            if (Math.Abs(uDistanceTravelled) >= LonCellLength)
            {
                Debug.Fail("u velocity greater than cell width");

            }
            if (Math.Abs(vDistanceTravelled) >= LatCellLength)
            {
                Debug.Fail("v velocity greater than cell width");
            }

            // We assume that the whole grid cell moves at the given velocity and calculate the area that is then outside the original grid cell location. 
            // This then becomes the probability of dispersal
            
            // Calculate the area of the grid cell that is now outside in the diagonal direction. 
            AreaOutsideBoth = Math.Abs(uDistanceTravelled * vDistanceTravelled);

            // Calculate the area of the grid cell that is now outside in the u (longitudinal) direction (not including the diagonal)
            AreaOutsideU = Math.Abs(uDistanceTravelled * LatCellLength) - AreaOutsideBoth;
            
            // Calculate the proportion of the grid cell that is outside in the v (latitudinal) direction (not including the diagonal)
            AreaOutsideV = Math.Abs(vDistanceTravelled * LonCellLength) - AreaOutsideBoth;

            // Get the cell area, in kilometres squared
            CellArea = madingleyGrid.GetCellEnvironment(latIndex, lonIndex)["Cell Area"][0];
            
            // Convert areas to a probability
            DispersalProbability = (AreaOutsideU + AreaOutsideV + AreaOutsideBoth) / CellArea;

            // Check that the whole cell hasn't moved out. Could this happen for the fastest currents in a month? Definitely, 
            // if current speeds were not constrained
            if (DispersalProbability >= 1)
            {
                Debug.Fail("Dispersal probability in advection should always be <= 1");
            }

            double[] NewArray = { DispersalProbability, AreaOutsideU / CellArea, AreaOutsideV / CellArea, AreaOutsideBoth / CellArea, uDistanceTravelled, vDistanceTravelled };
            return NewArray;
        }

        /// <summary>
        /// Get a randomly directed diffusion vector. This is derived from the LTRANS model formulation, which itself is derived from Visser 1997 (MEPS)
        /// We assume that the standard deviation of the random draw is 1.0
        /// </summary>
        /// <returns>A two element array, where the first element is the diffusion component in the u direction, and the second component is the
        /// diffusion component in the v direction</returns>
        private double[] CalculateDiffusion()
        {
            // Create the array with which to send the output
            double[] UandVOutputs = new double[2];

            // Note that this formulation drops the delta t because we set the horizontal diffusivity to be at the same temporal
            // scale as the time step
            UandVOutputs[0] = RandomNumberGenerator.GetNormal() * Math.Sqrt((2.0 * _HorizontalDiffusivityKmSqPerADTimeStep));
            UandVOutputs[1] = RandomNumberGenerator.GetNormal() * Math.Sqrt((2.0 * _HorizontalDiffusivityKmSqPerADTimeStep));

            return UandVOutputs;
        }
           
    }
}
