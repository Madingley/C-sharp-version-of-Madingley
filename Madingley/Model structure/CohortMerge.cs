using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// Merges cohorts with similar properties
    /// </summary>
    public class CohortMerge
    {
        /// <summary>
        /// An instance of the simple random number generator
        /// </summary>
        private NonStaticSimpleRNG RandomNumberGenerator = new NonStaticSimpleRNG();

        /// <summary>
        /// Constructor for CohortMerge: sets the seed for the random number generator
        /// </summary>
        /// <param name="DrawRandomly"></param>
        public CohortMerge(Boolean DrawRandomly)
        {
            // Seed the random number generator
            // Set the seed for the random number generator
            RandomNumberGenerator = new NonStaticSimpleRNG();
            if (DrawRandomly)
            {
                RandomNumberGenerator.SetSeedFromSystemTime();
            }
            else
            {
                RandomNumberGenerator.SetSeed(4000);
            }
        }

        /// <summary>
        /// Calculate the distance between two cohorts in multi-dimensional trait space (body mass, adult mass, juvenile mass)
        /// </summary>
        /// <param name="Cohort1">The first cohort to calculate distance to</param>
        /// <param name="Cohort2">The cohort to compare to</param>
        /// <returns>The relative distance in trait space</returns>
        public double CalculateDistance(Cohort Cohort1, Cohort Cohort2)
        {
            double AdultMassDistance = Math.Abs(Cohort1.AdultMass - Cohort2.AdultMass)/Cohort1.AdultMass;
            double JuvenileMassDistance = Math.Abs(Cohort1.JuvenileMass - Cohort2.JuvenileMass)/Cohort1.JuvenileMass;
            double CurrentMassDistance = Math.Abs(Cohort1.IndividualBodyMass - Cohort2.IndividualBodyMass)/Cohort1.IndividualBodyMass;

            return Math.Sqrt((AdultMassDistance * AdultMassDistance) + (JuvenileMassDistance * JuvenileMassDistance) +
                (CurrentMassDistance * CurrentMassDistance));

        }

        /// <summary>
        /// Merge cohorts until below a specified threshold number of cohorts in each grid cell
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts within this grid cell</param>
        /// <param name="TotalNumberOfCohorts">The total number of cohorts in this grid cell</param>
        /// <param name="TargetCohortThreshold">The target threshold to reduce the number of cohorts to</param>
        /// <returns>The number of cohorts that have been merged</returns>
        public int MergeToReachThreshold(GridCellCohortHandler gridCellCohorts, int TotalNumberOfCohorts, int TargetCohortThreshold)
        {

                // A list of shortest distances between pairs of cohorts
                List<Tuple<double, int, int[]>> ShortestDistances = new List<Tuple<double,int,int[]>>();

                // A holding list
           //     List<Tuple<double, int, int[]>> HoldingList = new List<Tuple<double, int, int[]>>();

                // Vector of lists of shortest distances in each functional group
           //     List<Tuple<double, int, int[]>>[] ShortestDistancesPerFunctionalGroup = new List<Tuple<double, int, int[]>>[gridCellCohorts.Count];
    
                
                // Temporary
                List<Tuple<double, int, int[]>>[] ShortestDistancesPerFunctionalGroup2 = new List<Tuple<double, int, int[]>>[gridCellCohorts.Count];

                // How many cohorts to remove to hit the threshold
                int NumberToRemove = TotalNumberOfCohorts - TargetCohortThreshold;

                // Holds the pairwise distances between two cohorts; the functional group of the cohorts; the cohort IDs of each cohort
                Tuple<double, int, int[]> PairwiseDistance;

                // Loop through functional groups
                for (int ff = 0; ff < gridCellCohorts.Count; ff++)
                {
                   
                    // Temporary
                    ShortestDistancesPerFunctionalGroup2[ff] = new List<Tuple<double, int, int[]>>();

                    // Loop through cohorts within functional groups
                    for (int cc = 0; cc < gridCellCohorts[ff].Count - 1; cc++)
                    {

                        // Loop through comparison cohorts
                        for (int dd = cc + 1; dd < gridCellCohorts[ff].Count; dd++)
                        {
                            // Randomly select which cohort is to be merge to & calculate distance between cohort pair
                            if (RandomNumberGenerator.GetUniform() < 0.5)
                            {
                                PairwiseDistance = new Tuple<double, int, int[]>(CalculateDistance(gridCellCohorts[ff][cc], gridCellCohorts[ff][dd]), ff, new int[] { cc, dd });
                            }
                            else
                            {
                                PairwiseDistance = new Tuple<double, int, int[]>(CalculateDistance(gridCellCohorts[ff][cc], gridCellCohorts[ff][dd]), ff, new int[] { dd, cc });
                            }

                            // Temporary
                            ShortestDistancesPerFunctionalGroup2[ff].Add(PairwiseDistance);
                        }

                       
                    }
                       

                    // Temporary
                    ShortestDistancesPerFunctionalGroup2[ff] = ShortestDistancesPerFunctionalGroup2[ff].OrderBy(x => x.Item1).ToList();
                }


                // Hold the current position in the shortest distance list
                int CurrentListPosition = 0;
                List<int> IndicesToRemove;

                // Now that the shortest distances have been calculated, do the merging execution                
                int FunctionalGroup;
                int CohortToMergeFrom;
                int CohortToMergeTo;
            
                // Temporary
                for (int gg = 0; gg < gridCellCohorts.Count; gg++)
                {
                    IndicesToRemove = new List<int>();
                    CurrentListPosition = 0;
                    while (CurrentListPosition < ShortestDistancesPerFunctionalGroup2[gg].Count)
                    {
                        CohortToMergeFrom = ShortestDistancesPerFunctionalGroup2[gg][CurrentListPosition].Item3[1];
                        CohortToMergeTo = ShortestDistancesPerFunctionalGroup2[gg][CurrentListPosition].Item3[0];

                        for (int cc = ShortestDistancesPerFunctionalGroup2[gg].Count - 1; cc > CurrentListPosition; cc--)
                        {
                            if (ShortestDistancesPerFunctionalGroup2[gg][cc].Item3[0] == CohortToMergeFrom ||
                                ShortestDistancesPerFunctionalGroup2[gg][cc].Item3[1] == CohortToMergeFrom)
                            {
                                ShortestDistancesPerFunctionalGroup2[gg].RemoveAt(cc);
                            }

                        }

                        CurrentListPosition++;
                    }
                }
                     

                // Compile all shortest distances into a single list for merging purposes - note that we only need to do a limited number of merges
                for (int gg = 0; gg < gridCellCohorts.Count; gg++)
                {
                    foreach (var distance in ShortestDistancesPerFunctionalGroup2[gg])
                    { 
                        ShortestDistances.Add(distance);
                    }
                }
                
                ShortestDistances = ShortestDistances.OrderBy(x => x.Item1).ToList();


                // Counts the number of merges that have happened
                int MergeCounter = 0;
                CurrentListPosition = 0;

                // While merging does not reach threshold, and while there are still elements in the list
                while((MergeCounter < NumberToRemove) && (CurrentListPosition < ShortestDistances.Count))
                {
                    // Get pairwise traits
                    FunctionalGroup = ShortestDistances[CurrentListPosition].Item2;
                    CohortToMergeFrom = ShortestDistances[CurrentListPosition].Item3[1];
                    CohortToMergeTo = ShortestDistances[CurrentListPosition].Item3[0];

                    // Check whether either cohort has already merged to something else this timestep merge
                    // execution, and hence is empty
          //          if ((gridCellCohorts[FunctionalGroup][CohortToMergeFrom].CohortAbundance.CompareTo(0.0) > 0) ||
          //              (gridCellCohorts[FunctionalGroup][CohortToMergeTo].CohortAbundance.CompareTo(0.0) > 0))
          //          {

                        // Add the abundance of the second cohort to that of the first
                        gridCellCohorts[FunctionalGroup][CohortToMergeTo].CohortAbundance += (gridCellCohorts[FunctionalGroup][CohortToMergeFrom].CohortAbundance * gridCellCohorts[FunctionalGroup][CohortToMergeFrom].IndividualBodyMass) / gridCellCohorts[FunctionalGroup][CohortToMergeTo].IndividualBodyMass;
                        // Add the reproductive potential mass of the second cohort to that of the first
                        gridCellCohorts[FunctionalGroup][CohortToMergeTo].IndividualReproductivePotentialMass += (gridCellCohorts[FunctionalGroup][CohortToMergeFrom].IndividualReproductivePotentialMass * gridCellCohorts[FunctionalGroup][CohortToMergeFrom].CohortAbundance) / gridCellCohorts[FunctionalGroup][CohortToMergeTo].CohortAbundance;
                        // Set the abundance of the second cohort to zero
                        gridCellCohorts[FunctionalGroup][CohortToMergeFrom].CohortAbundance = 0.0;
                        // Designate both cohorts as having merged
                        gridCellCohorts[FunctionalGroup][CohortToMergeTo].Merged = true;
                        gridCellCohorts[FunctionalGroup][CohortToMergeFrom].Merged = true;

                        MergeCounter++;
                        CurrentListPosition++;

             //       }
                 //   else
                 //   {
                //        CurrentListPosition++;
              //      }
                }

                return MergeCounter;

        }


        /// <summary>
        /// Merge cohorts until below a specified threshold number of cohorts in each grid cell
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts within this grid cell</param>
        /// <param name="TotalNumberOfCohorts">The total number of cohorts in this grid cell</param>
        /// <param name="TargetCohortThreshold">The target threshold to reduce the number of cohorts to</param>
        /// <returns>The number of cohorts that have been merged</returns>
        public int MergeToReachThresholdFast(GridCellCohortHandler gridCellCohorts, int TotalNumberOfCohorts, int TargetCohortThreshold)
        {

            // A list of shortest distances between pairs of cohorts
            List<Tuple<double, int, int[]>> ShortestDistances = new List<Tuple<double, int, int[]>>();

            // A holding list
            List<Tuple<double, int, int[]>> HoldingList = new List<Tuple<double, int, int[]>>();

            // Vector of lists of shortest distances in each functional group
            List<Tuple<double, int, int[]>>[] ShortestDistancesPerFunctionalGroup = new List<Tuple<double, int, int[]>>[gridCellCohorts.Count];
            
            // How many cohorts to remove to hit the threshold
            int NumberToRemove = TotalNumberOfCohorts - TargetCohortThreshold;

            // Holds the pairwise distances between two cohorts; the functional group of the cohorts; the cohort IDs of each cohort
            Tuple<double, int, int[]> PairwiseDistance;

            // Loop through functional groups
            for (int ff = 0; ff < gridCellCohorts.Count; ff++)
            {
                ShortestDistancesPerFunctionalGroup[ff] = new List<Tuple<double, int, int[]>>();
                
                // Loop through cohorts within functional groups
                for (int cc = 0; cc < gridCellCohorts[ff].Count - 1; cc++)
                {
                    // Reset the holding list
                    HoldingList = new List<Tuple<double, int, int[]>>();

                    // Loop through comparison cohorts
                    for (int dd = cc + 1; dd < gridCellCohorts[ff].Count; dd++)
                    {
                        // Randomly select which cohort is to be merge to & calculate distance between cohort pair
                        if (RandomNumberGenerator.GetUniform() < 0.5)
                        {
                            PairwiseDistance = new Tuple<double, int, int[]>(CalculateDistance(gridCellCohorts[ff][cc], gridCellCohorts[ff][dd]), ff, new int[] { cc, dd });
                        }
                        else
                        {
                            PairwiseDistance = new Tuple<double, int, int[]>(CalculateDistance(gridCellCohorts[ff][cc], gridCellCohorts[ff][dd]), ff, new int[] { dd, cc });
                        }

                        HoldingList.Add(PairwiseDistance);

                    }

                    HoldingList = HoldingList.OrderBy(x => x.Item1).ToList();

                    // Sort through and only keep those cohorts which are necessary

                    // The value to which to compare
                    int ValueToCompareTo = cc;

                    ShortestDistancesPerFunctionalGroup[ff].Add(HoldingList.ElementAt(0));

                    // Only add to main list those that are valid. Note that this doesn't catch everything (because we don't yet know the full ordering), 
                    // but clears out a lot of redundant information from the list
                    int position = 0;                                           

                    while (position < HoldingList.Count)
                    {
                        if (HoldingList.ElementAt(position).Item3[1] == ValueToCompareTo)
                        {
                            ShortestDistancesPerFunctionalGroup[ff].Add(HoldingList.ElementAt(position));
                            break;
                        }
                        else
                            ShortestDistancesPerFunctionalGroup[ff].Add(HoldingList.ElementAt(position));

                        position++;
                    }


                }
                ShortestDistancesPerFunctionalGroup[ff] = ShortestDistancesPerFunctionalGroup[ff].OrderBy(x => x.Item1).ToList();

            }


            // Hold the current position in the shortest distance list
            int CurrentListPosition = 0;
            List<int> IndicesToRemove;
          
            int FunctionalGroup;
            int CohortToMergeFrom;
            int CohortToMergeTo;

            for (int ff = 0; ff < gridCellCohorts.Count; ff++)
            {
                IndicesToRemove = new List<int>();
                CurrentListPosition = 0;
                while(CurrentListPosition < ShortestDistancesPerFunctionalGroup[ff].Count)
                {
                    CohortToMergeFrom = ShortestDistancesPerFunctionalGroup[ff][CurrentListPosition].Item3[1];
                    CohortToMergeTo = ShortestDistancesPerFunctionalGroup[ff][CurrentListPosition].Item3[0];

                    for (int cc = ShortestDistancesPerFunctionalGroup[ff].Count-1; cc > CurrentListPosition; cc--)
                    {
                        if (ShortestDistancesPerFunctionalGroup[ff][cc].Item3[0] == CohortToMergeFrom || 
                            ShortestDistancesPerFunctionalGroup[ff][cc].Item3[1] == CohortToMergeFrom)
                        {
                            ShortestDistancesPerFunctionalGroup[ff].RemoveAt(cc);
                        }

                    }

                    CurrentListPosition++;
                }
            }
            
                 

            // Compile all shortest distances into a single list for merging purposes - note that we only need to do a limited number of merges
            for (int ff = 0; ff < gridCellCohorts.Count; ff++)
            {
                foreach (var distance in ShortestDistancesPerFunctionalGroup[ff])
                {
                    ShortestDistances.Add(distance);
                }
            }
                
            ShortestDistances = ShortestDistances.OrderBy(x => x.Item1).ToList();


            // Counts the number of merges that have happened
            int MergeCounter = 0;
            CurrentListPosition = 0;

            // While merging does not reach threshold, and while there are still elements in the list
            while ((MergeCounter < NumberToRemove) && (CurrentListPosition < ShortestDistances.Count))
            {
                // Get pairwise traits
                FunctionalGroup = ShortestDistances[CurrentListPosition].Item2;
                CohortToMergeFrom = ShortestDistances[CurrentListPosition].Item3[1];
                CohortToMergeTo = ShortestDistances[CurrentListPosition].Item3[0];

                // Check whether either cohort has already merged to something else this timestep merge
                // execution, and hence is empty
                //          if ((gridCellCohorts[FunctionalGroup][CohortToMergeFrom].CohortAbundance.CompareTo(0.0) > 0) ||
                //              (gridCellCohorts[FunctionalGroup][CohortToMergeTo].CohortAbundance.CompareTo(0.0) > 0))
                //          {

                // Add the abundance of the second cohort to that of the first
                gridCellCohorts[FunctionalGroup][CohortToMergeTo].CohortAbundance += (gridCellCohorts[FunctionalGroup][CohortToMergeFrom].CohortAbundance * gridCellCohorts[FunctionalGroup][CohortToMergeFrom].IndividualBodyMass) / gridCellCohorts[FunctionalGroup][CohortToMergeTo].IndividualBodyMass;
                // Add the reproductive potential mass of the second cohort to that of the first
                gridCellCohorts[FunctionalGroup][CohortToMergeTo].IndividualReproductivePotentialMass += (gridCellCohorts[FunctionalGroup][CohortToMergeFrom].IndividualReproductivePotentialMass * gridCellCohorts[FunctionalGroup][CohortToMergeFrom].CohortAbundance) / gridCellCohorts[FunctionalGroup][CohortToMergeTo].CohortAbundance;
                // Set the abundance of the second cohort to zero
                gridCellCohorts[FunctionalGroup][CohortToMergeFrom].CohortAbundance = 0.0;
                // Designate both cohorts as having merged
                gridCellCohorts[FunctionalGroup][CohortToMergeTo].Merged = true;
                gridCellCohorts[FunctionalGroup][CohortToMergeFrom].Merged = true;

                MergeCounter++;
                CurrentListPosition++;

                //       }
                //   else
                //   {
                //        CurrentListPosition++;
                //      }
            }

            return MergeCounter;

        }




        /// <summary>
        /// Merge cohorts for responsive dispersal only; merges identical cohorts, no matter how many times they have been merged before
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <returns>Number of cohorts merged</returns>
        public int MergeForResponsiveDispersalOnly(GridCellCohortHandler gridCellCohorts)
        {
            // Variable to track the total number of cohorts merged
            int NumberCombined = 0;

            //Loop over all functional groups
            for (int i = 0; i < gridCellCohorts.Count; i++)
            {
                // Loop over each cohort in each functional group
                for (int j = 0; j < gridCellCohorts[i].Count; j++)
                {
                    // If that cohort has abundance greater than zero  then check if there are similar cohorts that could be merged with it
                    if (gridCellCohorts[i][j].CohortAbundance > 0)
                    {
                        // Loop over all cohorts above the jth in the cohort list
                        for (int k = j + 1; k < gridCellCohorts[i].Count; k++)
                        {
                            // Check that kth cohort has abunance and that the two cohorts being compared do not represent a juvenile adult pairing
                            if (gridCellCohorts[i][k].CohortAbundance > 0 &&
                                ((gridCellCohorts[i][j].MaturityTimeStep == uint.MaxValue && gridCellCohorts[i][k].MaturityTimeStep == uint.MaxValue) ||
                                 (gridCellCohorts[i][j].MaturityTimeStep < uint.MaxValue && gridCellCohorts[i][k].MaturityTimeStep < uint.MaxValue)))
                            {
                                //Check that the individual masses are widentical
                                if (gridCellCohorts[i][j].IndividualBodyMass == gridCellCohorts[i][k].IndividualBodyMass)
                                {
                                    //Check that the adult masses are similar
                                    if (gridCellCohorts[i][j].AdultMass == gridCellCohorts[i][k].AdultMass)
                                    {
                                        //Check that the juvenile masses are similar
                                        if (gridCellCohorts[i][j].JuvenileMass == gridCellCohorts[i][k].JuvenileMass)
                                        {
                                            //Check that the Maximum achieved mass is similar
                                            if (gridCellCohorts[i][j].MaximumAchievedBodyMass == gridCellCohorts[i][k].MaximumAchievedBodyMass)
                                            {
                                                // In half of cases, add the abundance of the second cohort to that of the first and maintain the properties of the first
                                                if (RandomNumberGenerator.GetUniform() < 0.5)
                                                {
                                                    // Add the abundance of the second cohort to that of the first
                                                    gridCellCohorts[i][j].CohortAbundance += (gridCellCohorts[i][k].CohortAbundance * gridCellCohorts[i][k].IndividualBodyMass) / gridCellCohorts[i][j].IndividualBodyMass;
                                                    // Set the abundance of the second cohort to zero
                                                    gridCellCohorts[i][k].CohortAbundance = 0.0;
                                                    // Add the reproductive potential mass of the second cohort to that of the first
                                                    gridCellCohorts[i][j].IndividualReproductivePotentialMass += (gridCellCohorts[i][k].IndividualReproductivePotentialMass * gridCellCohorts[i][k].CohortAbundance) / gridCellCohorts[i][j].CohortAbundance;
                                                    // Designate both cohorts as having merged
                                                    gridCellCohorts[i][j].Merged = true;
                                                    gridCellCohorts[i][k].Merged = true;
                                                }
                                                // In all other cases, add the abundance of the first cohort to that of the second and maintain the properties of the second
                                                else
                                                {
                                                    // Add the abundance of the first cohort to that of the second
                                                    gridCellCohorts[i][k].CohortAbundance += (gridCellCohorts[i][j].CohortAbundance * gridCellCohorts[i][j].IndividualBodyMass) / gridCellCohorts[i][k].IndividualBodyMass;
                                                    // Set the abundance of the second cohort to zero
                                                    gridCellCohorts[i][j].CohortAbundance = 0.0;
                                                    // Add the reproductive potential mass of the second cohort to that of the first
                                                    gridCellCohorts[i][k].IndividualReproductivePotentialMass += (gridCellCohorts[i][j].IndividualReproductivePotentialMass * gridCellCohorts[i][j].CohortAbundance) / gridCellCohorts[i][k].CohortAbundance;
                                                    // Designate both cohorts as having merged
                                                    gridCellCohorts[i][j].Merged = true;
                                                    gridCellCohorts[i][k].Merged = true;
                                                }
                                                // Increment the number of cohorts combined
                                                NumberCombined += 1;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return NumberCombined;

        }

    }
}
