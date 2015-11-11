using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    public class ImpactsSpatialHandler
    {

        private List<uint> _SpecificImpactCellIndices;

        public List<uint> SpecificImpactCellIndices
        {
            get { return _SpecificImpactCellIndices; }
            set { _SpecificImpactCellIndices = value; }
        }

        private int _NumLatCells;
        private int _NumLonCells;

        private int NumCells;

        private Tuple<string, double> SpatialImpactsScenario;

        /// <summary>
        /// Instance of Utilities for timestep conversions
        /// </summary>
        private UtilityFunctions Utilities = new UtilityFunctions();
        
        //public ImpactsSpatialHandler(MadingleyModelInitialisation initialisation, 
        //    ScenarioParameterInitialisation scenarioParameters, int scenarioIndex,Boolean specificLocations)
        //{

        //    if (specificLocations)
        //    {
        //        _SpecificImpactCellIndices = new List<uint>();
        //        _SpecificImpactCellIndices.Add(0);
        //    }
        //    else
        //    {

        //        int NumLatCells = (int)((initialisation.TopLatitude - initialisation.BottomLatitude) / initialisation.CellSize);
        //        int NumLonCells = (int)((initialisation.RightmostLongitude - initialisation.LeftmostLongitude) / initialisation.CellSize);

        //        _NumLatCells = NumLatCells;
        //        _NumLonCells = NumLonCells;

        //        NumCells = (int)(NumLatCells * NumLonCells / initialisation.CellRarefaction);

        //        SpatialImpactsScenario = scenarioParameters.scenarioParameters.ElementAt(scenarioIndex).Item3["spatial"];

        //        switch (SpatialImpactsScenario.Item1)
        //        {
        //            case "no":
        //                _SpecificImpactCellIndices = new List<uint>();
        //                break;
        //            case "random":
        //                this.CalculateRandomImpactedCells(SpatialImpactsScenario.Item2);
        //                break;
        //            case "continuous":
        //                this.CalculateContinuousImpactedCells(SpatialImpactsScenario.Item2);
        //                break;
        //            case "division":
        //                this.CalculateMaximumDivisionsOfImpactedCells(SpatialImpactsScenario.Item2);
        //                break;
        //            case "distributed":
        //                this.CalculateMaximallyDistributedImpactedCells(SpatialImpactsScenario.Item2);
        //                break;
        //        }
        //    }





        //}


        private void CalculateMaximallyDistributedImpactedCells(double fragmentProportion)
        {



        }


        /// <summary>
        /// Calculates the indices of impacted cells that give maximum division of the model grid for the given fragmentation proportion
        /// </summary>
        /// <param name="fragmentProportion">Proportion of grid impacted</param>
        private void CalculateMaximumDivisionsOfImpactedCells(double fragmentProportion)
        {
            _SpecificImpactCellIndices = new List<uint>();

            bool[,] ImpactCellGrid = new bool[_NumLatCells, _NumLonCells];

            // Create a temporary grid of cells
            for (int ii = 0; ii < _NumLatCells; ii++)
            {
                for (int jj = 0; jj < _NumLonCells; jj++)
                {
                    ImpactCellGrid[ii, jj] = false;
                }
            }

            // Calculate number of cells impacted
            int NumberImpactedCells = (int)(NumCells * fragmentProportion);

            // Initial number of slices
            int NumberSlices = NumberImpactedCells / _NumLatCells;

            // Divide slices between latitudinal and longitudinal
            int LatSlices = NumberSlices / 2;
            int LonSlices = NumberSlices - LatSlices;

            // Number of intersections gives the number of cells that are not contributing to the impacted area - these need to be accounted for
            int Intersections = LatSlices * LonSlices;

            // Number of cells remaining to be allocated after the current slices are assigned
            int RemainingCells = NumberImpactedCells - (LatSlices * _NumLatCells) - (LonSlices * _NumLonCells) + Intersections;


            // Assign the index values for each latitudinal and longitudinal slice
            List<int> LatSliceIndices = new List<int>();
            List<int> LonSliceIndices = new List<int>();

            for (int i = 0; i < LatSlices; i++)
            {
                LatSliceIndices.Add((int)Math.Floor((double)(_NumLatCells * (i + 1) / (LatSlices + 1))) - 1);
            }

            for (int i = 0; i < LonSlices; i++)
            {
                LonSliceIndices.Add((int)Math.Floor((double)(_NumLonCells * (i + 1) / (LonSlices + 1))) - 1);
            }

            int NewIntersections = 0;

            // While there are still impacted cells remaining to be assigned to the grid
            while (RemainingCells > 0)
            {
                //Check which orientation to start slicing
                if (LatSlices <= LonSlices)
                {
                    NewIntersections = 0;

                    //Check if the remaining cells intersect with any slices in the perpendicular direction
                    for (int s = 0; s < LonSliceIndices.Count; s++)
                    {
                        if (RemainingCells >= LonSliceIndices[s])
                            NewIntersections = s;
                    }

                    // Add any intersections to the remaining cells to be accounted for
                    RemainingCells += NewIntersections;
                    LatSlices += 1;

                    if (RemainingCells >= _NumLatCells)
                    {
                        RemainingCells -= _NumLatCells;
                    }
                    else
                    {
                        RemainingCells -= RemainingCells;
                    }

                    //Reassign index values
                    LatSliceIndices = new List<int>();

                    for (int i = 0; i < LatSlices; i++)
                    {
                        LatSliceIndices.Add((int)Math.Floor((double)(_NumLatCells * (i + 1)/ (LatSlices + 1))) - 1);
                    }

                }
                else
                {

                    NewIntersections = 0;

                    for (int s = 0; s < LatSliceIndices.Count; s++)
                    {
                        if (RemainingCells >= LatSliceIndices[s])
                            NewIntersections = s;
                    }

                    RemainingCells += NewIntersections;
                    LonSlices += 1;

                    if (RemainingCells >= _NumLonCells)
                    {
                        RemainingCells -= _NumLonCells;
                    }
                    else
                    {
                        RemainingCells -= RemainingCells;
                    }


                    LonSliceIndices = new List<int>();
                    for (int i = 0; i < LonSlices; i++)
                    {
                        LonSliceIndices.Add((int)Math.Floor((double)(_NumLonCells * (i + 1) / (LonSlices + 1))) - 1);
                    }

                }

            }


            //Given those indices for slices calculate the list of impacted cell indices for use in the rest of the model
            int ImpactCellCounter = 0;
            uint IndexCounter = 0;

            for (int ii = 0; ii < _NumLatCells; ii++)
            {
                for (int jj = 0; jj < _NumLonCells; jj++)
                {

                    if (LatSliceIndices.Contains(jj) || LonSliceIndices.Contains(ii))
                    {
                        ImpactCellCounter += 1;
                        if (ImpactCellCounter <= NumberImpactedCells)
                        {
                            SpecificImpactCellIndices.Add(IndexCounter);
                            IndexCounter += 1;
                        }
                        else
                        {
                            break;
                        }

                    }
                }

            }




        }



        private void CalculateContinuousImpactedCells(double fragmentProportion)
        {

            _SpecificImpactCellIndices = new List<uint>();

            int NumberImpactedCells = (int)(NumCells * fragmentProportion);


            for (uint ii = 0; ii < NumberImpactedCells; ii++)
            {
                _SpecificImpactCellIndices.Add(ii);  
            }

        }


        private void CalculateRandomImpactedCells(double fragmentProportion)
        {
            _SpecificImpactCellIndices = new List<uint>();

            int NumberImpactedCells = (int)(NumCells * fragmentProportion);

            uint[] RandomCellIndices = Utilities.RandomlyOrderedIndices((uint)NumCells);
            for (int ii = 0; ii < NumberImpactedCells; ii++)
            {
                _SpecificImpactCellIndices.Add(RandomCellIndices[ii]);
            }

        }

    }
}
