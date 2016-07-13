using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// Class for converting primary productivity estimates to autotroph biomass
    /// </summary>
    public class AutotrophProcessor
    {
        /// <summary>
        /// Instance of the class to perform general functions
        /// </summary>
        private UtilityFunctions Utilities;

        // Conversion ratio for phytoplankton from grams carbon to grams wet weight
        /// <summary>
        /// Factor to convert phytoplankton biomass from grams carbon to grams wet weight
        /// </summary>
        /// <remarks>Currently derived from Ho et al. (2003) J. Phycol., Dalsgaard and Pauly (1997) and Strickland (1966)</remarks>
        private double _PhytoplanktonConversionRatio;
        /// <summary>
        /// Get the conversion ratio for phytoplankton from grams carbon to grams wet weight
        /// </summary>
        public double PhytoplanktonConversionRatio { get {return _PhytoplanktonConversionRatio; } }

        /// <summary>
        /// Factor to convert NPP from units per m^2 to units per km^2
        /// </summary>
        private const double _MsqToKmSqConversion = 1000000.0;
        /// <summary>
        /// Get the factor to convert NPP from units per m^2 to units per km^2
        /// </summary>
        public double MsqToKmSqConversion { get { return _MsqToKmSqConversion; } }

        /// <summary>
        /// Constructor for the autotroph processor: initialises necessary classes
        /// </summary>
        public AutotrophProcessor()
        {

            _PhytoplanktonConversionRatio = EcologicalParameters.Parameters["AutotrophProcessor.ConvertNPPtoAutotroph.PhytoplanktonConversionRatio"];

            // Initialise the utility functions
            Utilities = new UtilityFunctions();
        }

        /// <summary>
        /// Convert NPP estimate into biomass of an autotroph stock
        /// </summary>
        /// <param name="cellEnvironment">The environment of the current grid cell</param>
        /// <param name="gridCellStockHandler">The stock handler for the current stock</param>
        /// <param name="actingStock">The location of the stock to add biomass to</param>
        /// <param name="terrestrialNPPUnits">The units of the terrestrial NPP data</param>
        /// <param name="oceanicNPPUnits">The units of the oceanic NPP data</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="GlobalModelTimeStepUnit">The time step unit used in the model</param>
        /// <param name="trackProcesses">Whether to output data describing the ecological processes</param>
        /// <param name="globalTracker">Whether to output data describing the global-scale environment</param>
        /// <param name="outputDetail">The level of output detail to use for the outputs</param>
        /// <param name="specificLocations">Whether the model is being run for specific locations</param>
        /// <param name="currentMonth">The current month in the model run</param>
        public void ConvertNPPToAutotroph(SortedList<string,double[]> cellEnvironment, GridCellStockHandler gridCellStockHandler, int[] 
            actingStock, string terrestrialNPPUnits, string oceanicNPPUnits, uint currentTimestep, string GlobalModelTimeStepUnit,
            ProcessTracker trackProcesses, GlobalProcessTracker globalTracker, string outputDetail, bool specificLocations,uint currentMonth)
        {

            // Get NPP from the cell environment
            double NPP = cellEnvironment["NPP"][currentMonth];

            // If NPP is a mssing value then set to zero
            if (NPP == cellEnvironment["Missing Value"][0]) NPP = 0.0;

            // Check that this is an ocean cell
            if (cellEnvironment["Realm"][0] == 2.0)
            {
                // Check that the units of oceanic NPP are gC per m2 per day
                Debug.Assert(oceanicNPPUnits == "gC/m2/day", "Oceanic NPP data are not in the correct units for this formulation of the model");

                //If units are moles per meter squared per second then convert to gC m-2 d-1
                if(oceanicNPPUnits == "mol C m-2 s-1")
                {
                    NPP *= 12 * 86400;
                }

                // Convert to g/cell/month
                NPP *= _MsqToKmSqConversion;

                // Multiply by cell area to get g/cell/day
                NPP *= cellEnvironment["Cell Area"][0];

                // Convert to g wet matter, assuming carbon content of phytoplankton is 10% of wet matter
                NPP *= _PhytoplanktonConversionRatio;

                // Finally convert to g/cell/month and add to the stock totalbiomass
                NPP *= Utilities.ConvertTimeUnits(GlobalModelTimeStepUnit, "day");
                gridCellStockHandler[actingStock].TotalBiomass += NPP;

                if (trackProcesses.TrackProcesses && (outputDetail == "high") && specificLocations)
                {
                    trackProcesses.TrackPrimaryProductionTrophicFlow((uint)cellEnvironment["LatIndex"][0], (uint)cellEnvironment["LonIndex"][0],
                        NPP);
                }

                if (globalTracker.TrackProcesses)
                {
                    globalTracker.RecordNPP((uint)cellEnvironment["LatIndex"][0], (uint)cellEnvironment["LonIndex"][0],(uint)actingStock[0],
                            NPP / cellEnvironment["Cell Area"][0]);
                }

                // If the biomass of the autotroph stock has been made less than zero (i.e. because of negative NPP) then reset to zero
                if (gridCellStockHandler[actingStock].TotalBiomass < 0.0)
                    gridCellStockHandler[actingStock].TotalBiomass = 0.0;
            }
            // Else if neither on land or in the ocean
            else
            {
                Debug.Fail("This is not a marine cell!");
                // Set the autotroph biomass to zero
                gridCellStockHandler[actingStock].TotalBiomass = 0.0;
            }
            Debug.Assert(gridCellStockHandler[actingStock].TotalBiomass >= 0.0, "stock negative");
        }

    }
}
