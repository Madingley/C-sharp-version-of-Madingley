using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    //Instantiate one of these for each grid cell
    class ApplyFishingCatches
    {

        List<Tuple<int[], double>>[] BinnedCohorts;
        double[] BinnedTotalModelBiomass;
        double[] DefecitCatch;

        double AdultMassProportionFished;

        public ApplyFishingCatches(InputCatchData fishCatch)
        {
            BinnedTotalModelBiomass = new double[fishCatch.MassBins.Length];
            DefecitCatch = new double[fishCatch.MassBins.Length];

            BinnedCohorts = new List<Tuple<int[], double>>[BinnedTotalModelBiomass.Length];
            AdultMassProportionFished = 0.5;
        }

        //Function to bin cohorts according to the mass bins defined for the catch data
        /// <summary>
        /// Bin cohorts according to the mass bins defined for the catch data
        /// Constructs a list of functional group and cohort indices falling within each mass bin
        /// as well as the total biomass available to be fished in each
        /// </summary>
        /// <param name="c">The grid cell cohorts</param>
        /// <param name="fishCatch">Fisheries catch data</param>
        public void BinCohorts(GridCellCohortHandler c, InputCatchData fishCatch, FunctionalGroupDefinitions cohortFGs)
        {
            int mb = 0;

            int[] FishFGs = cohortFGs.GetFunctionalGroupIndex("Endo/Ectotherm", "Ectotherm",false);

            for (int i = 0; i < BinnedCohorts.Length; i++)
            {
                BinnedCohorts[i] = new List<Tuple<int[], double>>();
            }

            foreach (int fg in FishFGs)
	        {
                for (int i = 0; i < c[fg].Count(); i++)
                {
                    //Find the mass bin for this cohort
                    mb = fishCatch.MassBins.ToList().FindIndex(a => a >= c[fg,i].AdultMass);
                    if (mb < 0) mb = fishCatch.UnknownMassBinIndex - 1;
                    
                    //Check if the current bodymass is greater than the proportion of the adult mass
                    if (c[fg, i].IndividualBodyMass >= c[fg, i].AdultMass * AdultMassProportionFished)
                    {
                        //Calculate the total biomass of this cohort
                        double CohortBiomass = (c[fg, i].IndividualBodyMass + c[fg, i].IndividualReproductivePotentialMass) *
                                                    c[fg, i].CohortAbundance;
                        //Add the indices and total biomass to the bins
                        BinnedCohorts[mb].Add(new Tuple<int[], double>(new int[] { fg, i }, CohortBiomass));
                        BinnedTotalModelBiomass[mb] += CohortBiomass;
                    }
                }
            }
        }


        public void ApplyCatches(GridCellCohortHandler c, InputCatchData fishCatch, int latIndex, int lonIndex)
        {
            //Hold the total catch in each mass bin for this cell
            double[] BinnedCellCatch = new double[fishCatch.MassBins.Length];

            //TO DO: make the time division flexible according to the model timestep
            for (int mb = 0; mb < BinnedCellCatch.Length; mb++)
            {
                BinnedCellCatch[mb] = fishCatch.ModelGridCatch[latIndex, lonIndex, mb]/12.0;

                if (BinnedCellCatch[mb] > 0)
                {

                    if (BinnedTotalModelBiomass[mb] <= BinnedCellCatch[mb])
                    {
                        DefecitCatch[mb] = BinnedCellCatch[mb] - BinnedTotalModelBiomass[mb];
                        BinnedCellCatch[mb] = BinnedTotalModelBiomass[mb];
                    }

                    foreach (var v in BinnedCohorts[mb])
                    {
                        double Contribution = v.Item2 / BinnedTotalModelBiomass[mb];
                        double AbundanceCaught = Contribution * BinnedCellCatch[mb] / (c[v.Item1].IndividualBodyMass + c[v.Item1].IndividualReproductivePotentialMass);
                        c[v.Item1].CohortAbundance -= AbundanceCaught;
                    }
                }

            }

        }


    }
}
