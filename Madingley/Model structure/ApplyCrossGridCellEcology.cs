using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// Class for applying changes from the cross-grid cell ecological processes. These are held in matrices of lists in the modelgrid structure.
    /// We simply loop through each cell, and check to see if there are any cohorts flagged as needing to be dispersed. If so, we point the cohort list
    /// in the new grid cell to this cohort, we delete the pointer to it and to the new grid cell in the model grid delta structures, and we delete the pointer 
    /// to it in the original cell
    /// 
    /// We can also output diagnostics here (temporarily) as the whole grid needs to be completed before dispersal is enacted.
    /// </summary>
    public class ApplyCrossGridCellEcology
    {
        /// <summary>
        /// Apply all updates from the ecological processes to the properties of the acting cohort and to the environment
        /// </summary>
        public void UpdateAllCrossGridCellEcology(ModelGrid madingleyModelGrid, ref uint dispersalCounter, CrossCellProcessTracker trackCrossCellProcesses, uint currentTimeStep)
        {
                // Create an array to hold the number of cohorts dispersing in each direction from each grid cell
                uint[, ,] InboundCohorts = new uint[madingleyModelGrid.DeltaFunctionalGroupDispersalArray.GetLength(0), madingleyModelGrid.DeltaFunctionalGroupDispersalArray.GetLength(1), 8];

                // Create an array to hold the number of cohorts dispersing in each direction to each grid cell
                uint[, ,] OutboundCohorts = new uint[madingleyModelGrid.DeltaFunctionalGroupDispersalArray.GetLength(0), madingleyModelGrid.DeltaFunctionalGroupDispersalArray.GetLength(1), 8];

                // Create an list array to hold the weights of cohorts dispersing from grid cell. Dimensions are: num grid cells lon, num grid cells lat, num cohorts dispersing
                List<double>[,] OutboundCohortWeights = new List<double>[madingleyModelGrid.DeltaFunctionalGroupDispersalArray.GetLength(0), madingleyModelGrid.DeltaFunctionalGroupDispersalArray.GetLength(1)];

                for (uint ii = 0; ii < madingleyModelGrid.DeltaFunctionalGroupDispersalArray.GetLength(0); ii++)
                {
                    for (uint jj = 0; jj < madingleyModelGrid.DeltaFunctionalGroupDispersalArray.GetLength(1); jj++)
                    {
                        OutboundCohortWeights[ii,jj] = new List<double>();
                    }
                }


            // Loop through the delta array that holds the grid cells of the cohorts that are flagged as needing to be moved
            for (uint ii = 0; ii < madingleyModelGrid.DeltaFunctionalGroupDispersalArray.GetLength(0) ; ii++)
            {
                for (uint jj = 0; jj < madingleyModelGrid.DeltaFunctionalGroupDispersalArray.GetLength(1); jj++)
                {
                    if (madingleyModelGrid.DeltaFunctionalGroupDispersalArray[ii, jj] != null)
                    {

                        // No cohorts to move if there are none in the delta dispersal array
                        if (madingleyModelGrid.DeltaFunctionalGroupDispersalArray[ii, jj].Count == 0)
                        {
                        }
                        // Otherwise, loop through the cohorts and change the pointers/references to them one-by-one
                        else
                        {

                            for (int kk = 0; kk < madingleyModelGrid.DeltaFunctionalGroupDispersalArray[ii, jj].Count; kk++)
                            {
                                // Find out which grid cell it is going to
                                uint[] CellToDisperseTo = madingleyModelGrid.DeltaCellToDisperseToArray[ii, jj].ElementAt(kk);

                                // Functional group is identified by the first array
                                uint CohortToDisperseFG = madingleyModelGrid.DeltaFunctionalGroupDispersalArray[ii, jj].ElementAt(kk);

                                // Cohort number is identified by the second array
                                uint CohortToDisperseNum = madingleyModelGrid.DeltaCohortNumberDispersalArray[ii, jj].ElementAt(kk);

                                // If track processes is on, add this to the list of inbound and outbound cohorts
                                if (trackCrossCellProcesses.TrackCrossCellProcesses)
                                {
                                    WriteOutCrossGridCell(madingleyModelGrid, CellToDisperseTo, InboundCohorts, OutboundCohorts, OutboundCohortWeights, ii, jj,
                                        CohortToDisperseFG, CohortToDisperseNum, madingleyModelGrid.DeltaCellExitDirection[ii, jj].ElementAt(kk),
                                        madingleyModelGrid.DeltaCellEntryDirection[ii, jj].ElementAt(kk));
                                }

                                // Simmply add it to the existing cohorts in that FG in the grid cell to disperse to
                                madingleyModelGrid.AddNewCohortToGridCell(CellToDisperseTo[0], CellToDisperseTo[1], (int)CohortToDisperseFG, madingleyModelGrid.GetGridCellIndividualCohort(ii, jj, (int)CohortToDisperseFG, (int)CohortToDisperseNum));

                                // Update the dispersal counter
                                dispersalCounter++;

                                // So now there is a pointer in the grid cell to which it is going. We have to delete the pointers in the original cell and in the
                                // delta array, but we need to do this without messing with the list structure; i.e. wait until all cohorts have been moved
                            }
                        }
                    }
                    
                }
            }


            // Reset the delta arrays and remove the pointers to the cohorts in the original list
            for (uint ii = 0; ii < madingleyModelGrid.DeltaFunctionalGroupDispersalArray.GetLength(0); ii++)
            {
                for (uint jj = 0; jj < madingleyModelGrid.DeltaFunctionalGroupDispersalArray.GetLength(1); jj++)
                {
                    if (madingleyModelGrid.DeltaFunctionalGroupDispersalArray[ii, jj] != null)
                    {
                        // No cohorts to move if there are none in the delta dispersal array
                        if (madingleyModelGrid.DeltaFunctionalGroupDispersalArray[ii, jj].Count == 0)
                        {
                        }
                        // Otherwise, loop through the cohorts and change the pointers/references to them one-by-one
                        else
                        {
                            // Delete the cohorts from the original grid cell. Note that this needs to be done carefully to ensure that the correct ones 
                            // are deleted (lists shift about when an internal element is deleted.
                            madingleyModelGrid.DeleteGridCellIndividualCohorts(ii, jj, madingleyModelGrid.DeltaFunctionalGroupDispersalArray[ii, jj], madingleyModelGrid.DeltaCohortNumberDispersalArray[ii, jj]);

                            // Reset the lists in the delta dispersal arrays
                            madingleyModelGrid.DeltaFunctionalGroupDispersalArray[ii, jj] = new List<uint>();
                            madingleyModelGrid.DeltaCohortNumberDispersalArray[ii, jj] = new List<uint>();

                            // Reset the list in the grid cells to disperse to array
                            madingleyModelGrid.DeltaCellToDisperseToArray[ii, jj] = new List<uint[]>();

                            // Reset the lists in the delta dispersal arrays
                            madingleyModelGrid.DeltaCellExitDirection[ii, jj] = new List<uint>();
                            madingleyModelGrid.DeltaCellEntryDirection[ii, jj] = new List<uint>();
                        }
                    }

                }
            }

            if (trackCrossCellProcesses.TrackCrossCellProcesses)
            {
                // If we are tracking dispersal, then write out how many cohorts have moved to a file
                trackCrossCellProcesses.RecordDispersalForACell(InboundCohorts, OutboundCohorts, OutboundCohortWeights, currentTimeStep, madingleyModelGrid);
            }
        }

        // If we are tracking processes, this method writes out the relevant information
        void WriteOutCrossGridCell(ModelGrid madingleyModelGrid, uint[] cellToDisperseTo, uint[, ,] inboundCohorts, uint[, ,] outboundCohorts, 
            List<double>[,] outboundCohortWeights, uint xCellToDisperseFrom, uint yCellToDisperseFrom, uint cohortToDisperseFG, uint cohortToDisperseNum,
            uint exitDirection, uint entryDirection)
        {
            // Add the weight of the outbound cohort to the weights array
            outboundCohortWeights[xCellToDisperseFrom, yCellToDisperseFrom].Add(madingleyModelGrid.GetGridCellIndividualCohort(xCellToDisperseFrom, yCellToDisperseFrom,
                (int)cohortToDisperseFG, (int)cohortToDisperseNum).IndividualBodyMass);
            
            // Record the cohort as leaving the outbound cell
            outboundCohorts[xCellToDisperseFrom, yCellToDisperseFrom, exitDirection]++;

            // Record the cohort as incoming in the inbound cell
            inboundCohorts[cellToDisperseTo[0], cellToDisperseTo[1], entryDirection]++;
        }

    }
}
