using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// Tracks results associated with the eating process
    /// </summary>
    public class EatingTracker
    {
        
        string TrophicFlowsFilename;

        private StreamWriter TrophicFlowsWriter;

        private TextWriter SyncedTrophicFlowsWriter;

        /// <summary>
        /// Array to hold flows of mass among trophic levels. Order is:
        /// Lat, Lon, From (group), To (group)
        /// </summary>
        private double[,,,] TrophicMassFlows;

        /// <summary>
        /// Set up the tracker for outputing properties of the eating process
        /// </summary>
        /// <param name="numLats">The number of latitudes in the model grid</param>
        /// <param name="numLons">The number of longitudes in the model grid</param>
        /// <param name="trophicFlowsFilename">The filename to write data on trophic flows to</param>
        /// <param name="outputFilesSuffix">The suffix to apply to output files from this simulation</param>
        /// <param name="outputPath">The file path to write all outputs to</param>
        /// <param name="cellIndex">The index of the current cell within the list of all grid cells in this simulation</param>
        /// <param name="initialisation">The instance of the MadingleyModelInitialisation class for this simulation</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        public EatingTracker(uint numLats, uint numLons, string trophicFlowsFilename, string outputFilesSuffix, string outputPath, 
            int cellIndex, MadingleyModelInitialisation initialisation, Boolean MarineCell)
        {
            TrophicFlowsFilename = trophicFlowsFilename;

            TrophicFlowsWriter = new StreamWriter(outputPath + TrophicFlowsFilename + outputFilesSuffix + "_Cell" + cellIndex + ".txt");
            SyncedTrophicFlowsWriter = TextWriter.Synchronized(TrophicFlowsWriter);
            SyncedTrophicFlowsWriter.WriteLine("Latitude\tLongitude\ttime_step\tfromIndex\ttoIndex\tmass_eaten_g");


            // Initialise array to hold mass flows among trophic levels
            if (initialisation.TrackMarineSpecifics && MarineCell)
                // 0 = autotrophs, 1 = non-planktonic herbivores, 2 = non-planktonic omnivores, 3 = non-planktonic carnivores, 4 = obligate zooplankton, 5 = non-obligate zooplankton, 6 = baleen whales
                TrophicMassFlows = new double[numLats, numLons, 7, 7];
            else
                TrophicMassFlows = new double[numLats, numLons, 4, 4];
        }

        /// <summary>
        /// Record the flow of biomass between trophic levels during predation
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="fromFunctionalGroup">The index of the functional group that the biomass is flowing from (i.e. the prey)</param>
        /// <param name="toFunctionalGroup">The index of the functional group that the biomass is flowing to (i.e. the predator)</param>
        /// <param name="cohortFunctionalGroupDefinitions">The functional group definitions of cohorts in the model</param>
        /// <param name="massEaten">The total biomass eaten by the predator cohort</param>
        /// <param name="predatorBodyMass">The body mass of the predator doing the eating</param>
        /// <param name="preyBodyMass">The body mass of the prey being eaten</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        public void RecordPredationTrophicFlow(uint latIndex, uint lonIndex, int fromFunctionalGroup, int toFunctionalGroup,
            FunctionalGroupDefinitions cohortFunctionalGroupDefinitions, double massEaten, double predatorBodyMass, double preyBodyMass, 
            MadingleyModelInitialisation initialisation, Boolean MarineCell)
        {
            int fromIndex = 0;
            int toIndex = 0;
            if (initialisation.TrackMarineSpecifics && MarineCell)
            {
                // Get the trophic level index of the functional group that mass is flowing from
                switch (cohortFunctionalGroupDefinitions.GetTraitNames("nutrition source", fromFunctionalGroup))
                {
                    case "herbivore":
                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("mobility", fromFunctionalGroup))
                        {
                            case "planktonic":
                                fromIndex = 4;
                                break;
                            default:
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", fromFunctionalGroup))
                                {
                                    case "endotherm":
                                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", fromFunctionalGroup))
                                        {
                                            case "allspecial":
                                                fromIndex = 6;
                                                break;
                                            default:
                                                fromIndex = 1;
                                                break;    
                                        }
                                        break;
                                    default:
                                        if (preyBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            fromIndex = 5;
                                        else
                                            fromIndex = 1;
                                    break;
                                }
                                break;
                        }
                        break;
                    case "omnivore":
                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("mobility", fromFunctionalGroup))
                        {
                            case "planktonic":
                                fromIndex = 4;
                                break;
                            default:
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", fromFunctionalGroup))
                                {
                                    case "endotherm":
                                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", fromFunctionalGroup))
                                        {
                                            case "allspecial":
                                                fromIndex = 6;
                                                break;
                                            default:
                                                fromIndex = 2;
                                                break;    
                                        }
                                        break;
                                    default:
                                        if (preyBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            fromIndex = 5;
                                        else
                                            fromIndex = 2;
                                    break;
                                }
                                break;
                        }
                        break;
                    case "carnivore":
                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("mobility", fromFunctionalGroup))
                        {
                            case "planktonic":
                                fromIndex = 4;
                                break;
                            default:
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", fromFunctionalGroup))
                                {
                                    case "endotherm":
                                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", fromFunctionalGroup))
                                        {
                                            case "allspecial":
                                                fromIndex = 6;
                                                break;
                                            default:
                                                fromIndex = 3;
                                                break;    
                                        }
                                        break;
                                    default:
                                        if (preyBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            fromIndex = 5;
                                        else
                                            fromIndex = 3;
                                    break;
                                }
                                break;
                        }
                        break;
                    default:
                        Debug.Fail("Specified nutrition source is not supported");
                        break;
                }

                // Get the trophic level index of the functional group that mass is flowing to
                switch (cohortFunctionalGroupDefinitions.GetTraitNames("nutrition source", toFunctionalGroup))
                {
                    case "omnivore":
    switch (cohortFunctionalGroupDefinitions.GetTraitNames("mobility", toFunctionalGroup))
                        {
                            case "planktonic":
                                toIndex = 4;
                                break;
                            default:
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", toFunctionalGroup))
                                {
                                    case "endotherm":
                                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", toFunctionalGroup))
                                        {
                                            case "allspecial":
                                                toIndex = 6;
                                                break;
                                            default:
                                                toIndex = 2;
                                                break;    
                                        }
                                        break;
                                    default:
                                        if (predatorBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            toIndex = 5;
                                        else
                                            toIndex = 2;
                                    break;
                                }
                                break;
                        }
                        break;
                    case "carnivore":
    switch (cohortFunctionalGroupDefinitions.GetTraitNames("mobility", toFunctionalGroup))
                        {
                            case "planktonic":
                                toIndex = 4;
                                break;
                            default:
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", toFunctionalGroup))
                                {
                                    case "endotherm":
                                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", toFunctionalGroup))
                                        {
                                            case "allspecial":
                                                toIndex = 6;
                                                break;
                                            default:
                                                toIndex = 3;
                                                break;    
                                        }
                                        break;
                                    default:
                                        if (predatorBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            toIndex = 5;
                                        else
                                            toIndex = 3;
                                    break;
                                }
                                break;
                        }
                        break;
                    default:
                        Debug.Fail("Specified nutrition source is not supported");
                        break;
                }
            }
            else
            {
                // Get the trophic level index of the functional group that mass is flowing from
                switch (cohortFunctionalGroupDefinitions.GetTraitNames("nutrition source", fromFunctionalGroup))
                {
                    case "herbivore":
                        fromIndex = 1;
                        break;
                    case "omnivore":
                        fromIndex = 2;
                        break;
                    case "carnivore":
                        fromIndex = 3;
                        break;
                    default:
                        Debug.Fail("Specified nutrition source is not supported");
                        break;
                }

                // Get the trophic level index of the functional group that mass is flowing to
                switch (cohortFunctionalGroupDefinitions.GetTraitNames("nutrition source", toFunctionalGroup))
                {
                    case "herbivore":
                        toIndex = 1;
                        break;
                    case "omnivore":
                        toIndex = 2;
                        break;
                    case "carnivore":
                        toIndex = 3;
                        break;
                    default:
                        Debug.Fail("Specified nutrition source is not supported");
                        break;
                }
            }

            // Add the flow of matter to the matrix of mass flows
            TrophicMassFlows[latIndex, lonIndex, fromIndex, toIndex] += massEaten;

        }

        /// <summary>
        /// Record the flow of biomass between trophic levels during herbivory
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="toFunctionalGroup">The index of the functional group that the biomass is flowing to (i.e. the herbivore)</param>
        /// <param name="cohortFunctionalGroupDefinitions">The functional group definitions of cohorts in the model</param>
        /// <param name="massEaten">The total biomass eaten by the herbivore cohort</param>
        /// <param name="predatorBodyMass">The mass of the predator doing the eating</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        public void RecordHerbivoryTrophicFlow(uint latIndex, uint lonIndex, int toFunctionalGroup, FunctionalGroupDefinitions
            cohortFunctionalGroupDefinitions, double massEaten, double predatorBodyMass, MadingleyModelInitialisation initialisation, 
            Boolean MarineCell)
        {
            // For herbivory the trophic level index that mass flows from is 0
            int fromIndex = 0;
            // Get the trophic level index of the functional group that mass is flowing to
            int toIndex = 0;

            if (initialisation.TrackMarineSpecifics && MarineCell)
            {
                // Get the trophic level index of the functional group that mass is flowing to
                switch (cohortFunctionalGroupDefinitions.GetTraitNames("nutrition source", toFunctionalGroup))
                {
                    case "herbivore":
                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("mobility", toFunctionalGroup))
                        {
                            case "planktonic":
                                toIndex = 4;
                                break;
                            default:
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", toFunctionalGroup))
                                {
                                    case "endotherm":
                                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", toFunctionalGroup))
                                        {
                                            case "allspecial":
                                                toIndex = 6;
                                                break;
                                            default:
                                                toIndex = 1;
                                                break;
                                        }
                                        break;
                                    default:
                                        if (predatorBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            toIndex = 5;
                                        else
                                            toIndex = 1;
                                        break;
                                }
                                break;
                        }
                        break;
                    case "omnivore":
                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("mobility", toFunctionalGroup))
                        {
                            case "planktonic":
                                toIndex = 4;
                                break;
                            default:
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", toFunctionalGroup))
                                {
                                    case "endotherm":
                                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", toFunctionalGroup))
                                        {
                                            case "allspecial":
                                                toIndex = 6;
                                                break;
                                            default:
                                                toIndex = 2;
                                                break;
                                        }
                                        break;
                                    default:
                                        if (predatorBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            toIndex = 5;
                                        else
                                            toIndex = 2;
                                        break;
                                }
                                break;
                        }
                        break;
                    default:
                        Debug.Fail("Specified nutrition source is not supported");
                        break;
                }
            }
            else
            {
                // Get the trophic level index of the functional group that mass is flowing to
                switch (cohortFunctionalGroupDefinitions.GetTraitNames("nutrition source", toFunctionalGroup))
                {
                    case "herbivore":
                        toIndex = 1;
                        break;
                    case "omnivore":
                        toIndex = 2;
                        break;
                    case "carnivore":
                        toIndex = 3;
                        break;
                    default:
                        Debug.Fail("Specified nutrition source is not supported");
                        break;
                }
            }

            // Add the flow of matter to the matrix of mass flows
            TrophicMassFlows[latIndex, lonIndex, fromIndex, toIndex] += massEaten;
        }

        /// <summary>
        /// Record the flow of biomass into the autotroph trophic level as a result of primary production
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="massEaten">The total biomass gained by the autotroph stock</param>
        public void RecordPrimaryProductionTrophicFlow(uint latIndex, uint lonIndex, double massEaten)
        {
            // Add the flow of matter to the matrix of mass flows
            TrophicMassFlows[latIndex, lonIndex, 0, 0] += massEaten;
        }

        /// <summary>
        /// Write flows of matter among trophic levels to the output file at the end of the time step
        /// </summary>
        /// <param name="currentTimeStep">The current time step</param>
        /// <param name="numLats">The latitudinal dimension of the model grid in number of cells</param>
        /// <param name="numLons">The longitudinal dimension of the model grid in number of cells</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        public void WriteTrophicFlows(uint currentTimeStep,uint numLats,uint numLons, MadingleyModelInitialisation initialisation, 
            Boolean MarineCell)
        {
            for (int lat = 0; lat < numLats; lat++)
            {
                for (int lon = 0; lon < numLons; lon++)
                {
                    for (int i = 0; i < TrophicMassFlows.GetLength(2); i++)
                    {
                        for (int j = 0; j < TrophicMassFlows.GetLength(3); j++)
                        {
                            if (TrophicMassFlows[lat, lon, i, j] > 0)
                            {
                                SyncedTrophicFlowsWriter.WriteLine(Convert.ToString(lat) + '\t' + Convert.ToString(lon) + '\t' + Convert.ToString(currentTimeStep) +
                                    '\t' + Convert.ToString(i) + '\t' + Convert.ToString(j) + '\t' + Convert.ToString(TrophicMassFlows[lat, lon, i, j]));
                            }
                        }
                    }
                }
            }

            // Initialise array to hold mass flows among trophic levels
            if (initialisation.TrackMarineSpecifics && MarineCell)
                TrophicMassFlows = new double[numLats, numLons, 7, 7];
            else
                TrophicMassFlows = new double[numLats, numLons, 4, 4];


            
        }

        /// <summary>
        /// Close the streams for writing eating data
        /// </summary>
        public void CloseStreams()
        {
            TrophicFlowsWriter.Dispose();
        }
    }
}
