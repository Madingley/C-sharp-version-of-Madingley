using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Science.Data;
using System.IO;


namespace Madingley
{
    /// <summary>
    /// Revised version of Matt Smith's terrestrial carbon model
    /// </summary>
    public class RevisedTerrestrialPlantModel
    {


        /// <summary>
        /// The maximum poossible NPP (kg C per m2 per year)
        /// </summary>
        private double max_NPP;

        /// <summary>
        /// First constant in the logistic function relating NPP to temperature in the Miami NPP model
        /// </summary>
        private double t1_NPP;

        /// <summary>
        /// Second constant in the logistic function relating NPP to temperature in the Miami NPP model 
        /// </summary>
        private double t2_NPP;

        /// <summary>
        /// Constant in the saturating function relating NPP to precipitation in the Miami NPP model
        /// </summary>
        private double p_NPP;

        /// <summary>
        /// Scalar relating the fraction of NPP devoted to structural tissue to the total amount of NPP
        /// </summary>
        private double FracStructScalar;

        /// <summary>
        /// Coefficient for the quadratic term in the function relating fractional allocation in evergreen leaf matter to fraction of the year experiencing frost
        /// </summary>
        private double a_FracEvergreen;

        /// <summary>
        /// Coefficient for the linear term in the function relating fractional allocation in evergreen leaf matter to fraction of the year experiencing frost
        /// </summary>
        private double b_FracEvergreen;

        /// <summary>
        /// Intercept in the function relating fractional allocation in evergreen leaf matter to fraction of the year experiencing frost
        /// </summary>
        private double c_FracEvergreen;

        /// <summary>
        /// The slope of the relationship between temperature and evergreen leaf mortality rate
        /// </summary>
        private double m_EGLeafMortality;

        /// <summary>
        /// The intercept of the relationship between temperature and evergreen leaf mortality rate
        /// </summary>
        private double c_EGLeafMortality;

        /// <summary>
        /// The minimum rate of evergreen leaf mortality
        /// </summary>
        private double er_min;

        /// <summary>
        /// The maximum rate of evergreen leaf mortality
        /// </summary>
        private double er_max;

        /// <summary>
        /// The slope of the relationship between temperature and deciduous leaf mortality rate
        /// </summary>
        private double m_DLeafMortality;

        /// <summary>
        /// The intercept of the relationship between temperature and deciduous leaf mortality rate
        /// </summary>
        private double c_DLeafMortality;

        /// <summary>
        /// The minimum rate of deciduous leaf mortality
        /// </summary>
        private double dr_min;

        /// <summary>
        /// The maximum rate of deciduous leaf mortality
        /// </summary>
        private double dr_max;

        /// <summary>
        /// The slope of the relationship between fine root mortality rate and temperature
        /// </summary>
        private double m_FRootMort;

        /// <summary>
        /// The intercept of the relationship between fine root mortality rate and temperature
        /// </summary>
        private double c_FRootMort;

        /// <summary>
        /// The minimum rate of fine root mortality
        /// </summary>
        private double frm_min;

        /// <summary>
        /// The maximum rate of fine root mortality
        /// </summary>
        private double frm_max;

        /// <summary>
        /// Scalar relating fire mortality rate to NPP
        /// </summary>
        private double NPPScalar_Fire;

        /// <summary>
        /// NPP at which fire mortality reaches half its maximum rate
        /// </summary>
        private double NPPHalfSaturation_Fire;

        /// <summary>
        /// Scalar relating fire mortality rate to the fractional fire season length
        /// </summary>
        private double LFSScalar_Fire;

        /// <summary>
        /// The fractional fire season length at which fire mortality reaches half its maximum rate
        /// </summary>
        private double LFSHalfSaturation_Fire;

        /// <summary>
        /// Base scalar for the fire mortality function
        /// </summary>
        private double BaseScalar_Fire;

        /// <summary>
        /// Minimum fire return interval
        /// </summary>
        private double MinReturnInterval;

        /// <summary>
        /// Second parameter in the structural mortality function
        /// </summary>
        private double p2_StMort;

        /// <summary>
        /// First parameter in the structural mortality function
        /// </summary>
        private double p1_StMort;

        /// <summary>
        /// Maximum rate of structural mortality
        /// </summary>
        private double stm_max;

        /// <summary>
        /// Minimum rate of structural mortality
        /// </summary>
        private double stm_min;

        /// <summary>
        /// The maximum fraction of productivity that can be allocated to structural tissue
        /// </summary>
        private double MaxFracStruct;

        /// <summary>
        /// Scalar to convert between mass of carbon and mass of leaf dry matter
        /// </summary>
        private double MassCarbonPerMassLeafDryMatter;

        /// <summary>
        /// Scalar to convert between mass of lead dry and mass of leaf wet matter
        /// </summary>
        private double MassLeafDryMatterPerMassLeafWetMatter;

        /// <summary>
        /// Constant to convert from m2 to km2
        /// </summary>
        private double m2Tokm2Conversion;

        /// <summary>
        /// Instance of the class to perform general functions
        /// </summary>
        private UtilityFunctions Utilities;


        #region Methods

        /// <summary>
        /// Constructor for the plant model
        /// </summary>
        public RevisedTerrestrialPlantModel()
        {
            // Initialise parameters
            InitialisePlantModelParameters();

            // Initialise the utility functions
            Utilities = new UtilityFunctions();

        }

        /// <summary>
        /// Initialise parameters for the plant model
        /// </summary>
        public void InitialisePlantModelParameters()
        {
            // Assign the parameters for the plant model
            max_NPP = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.max_NPP"];
            t1_NPP = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.t1_NPP"];
            t2_NPP = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.t2_NPP"]; ;
            p_NPP = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.p_NPP"]; ;
            FracStructScalar = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.FracStructScalar"]; ;
            a_FracEvergreen = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.a_FracEvergreen"]; ;
            b_FracEvergreen = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.b_FracEvergreen"]; ;
            c_FracEvergreen = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.c_FracEvergreen"]; ;
            m_EGLeafMortality = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.m_EGLeafMortality"];
            c_EGLeafMortality = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.c_EGLeafMortality"];
            m_DLeafMortality = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.m_DLeafMortality"];
            c_DLeafMortality = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.c_DLeafMortality"];
            m_FRootMort = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.m_FRootMort"];
            c_FRootMort = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.c_FRootMort"];
            p2_StMort = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.p2_StMort"];
            p1_StMort = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.p1_StMort"];
            MaxFracStruct = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.MaxFracStruct"];
            LFSHalfSaturation_Fire = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.LFSHalfSaturation_Fire"];
            LFSScalar_Fire = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.LFSScalar_Fire"];
            NPPHalfSaturation_Fire = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.NPPHalfSaturation_Fire"];
            NPPScalar_Fire = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.NPPScalar_Fire"];
            er_min = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.er_min"];
            er_max = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.er_max"];
            dr_min = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.dr_min"];
            dr_max = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.dr_max"];
            frm_min = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.frm_min"];
            frm_max = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.frm_max"];
            stm_max = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.stm_max"];
            stm_min = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.stm_min"];
            BaseScalar_Fire = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.BaseScalar_Fire"];
            MinReturnInterval = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.MinReturnInterval"];

            // mass of Leaf C per g leaf dry matter = 0.4761 g g-1 (from Kattge et al. (2011), TRY- A global database of plant traits, Global Change Biology)
            MassCarbonPerMassLeafDryMatter = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.MassCarbonPerMassLeafDryMatter"];
            // mass of leaf dry matter per g leaf wet matter = 0.213 g g-1 (from Kattge et al. (2011), TRY- A global database of plant traits, Global Change Biology)
            MassLeafDryMatterPerMassLeafWetMatter = EcologicalParameters.Parameters["RevisedTerrestrialPlantModel.MassLeafDryMatterPerMassLeafWetMatter"];


            m2Tokm2Conversion = 1000000.0;
        }

        /// <summary>
        /// Write out the values of the parameters to an output file
        /// </summary>
        /// <param name="sw">A streamwriter object to write the parameter values to</param>
        public void WriteOutParameterValues(StreamWriter sw)
        {
            // Initialise the parameters
            InitialisePlantModelParameters();

            // Write out parameters
            sw.WriteLine("Terrestrial Plant Model\tmax_NPP\t" + Convert.ToString(max_NPP));
            sw.WriteLine("Terrestrial Plant Model\tt1_NPP\t" + Convert.ToString(t1_NPP));
            sw.WriteLine("Terrestrial Plant Model\tt2_NPP\t" + Convert.ToString(t2_NPP));
            sw.WriteLine("Terrestrial Plant Model\tp_NPP\t" + Convert.ToString(p_NPP));
            sw.WriteLine("Terrestrial Plant Model\tFracStructScalar\t" + Convert.ToString(FracStructScalar));
            sw.WriteLine("Terrestrial Plant Model\ta_FracEvergreen\t" + Convert.ToString(a_FracEvergreen));
            sw.WriteLine("Terrestrial Plant Model\tb_FracEvergreen\t" + Convert.ToString(b_FracEvergreen));
            sw.WriteLine("Terrestrial Plant Model\tc_Frac_Evergreen\t" + Convert.ToString(c_FracEvergreen));
            sw.WriteLine("Terrestrial Plant Model\tm_EGLeafMortality\t" + Convert.ToString(m_EGLeafMortality));
            sw.WriteLine("Terrestrial Plant Model\tc_EGLeafMortality\t" + Convert.ToString(c_EGLeafMortality));
            sw.WriteLine("Terrestrial Plant Model\tm_DLeafMortality\t" + Convert.ToString(m_DLeafMortality));
            sw.WriteLine("Terrestrial Plant Model\tc_DLeafMortality\t" + Convert.ToString(c_DLeafMortality));
            sw.WriteLine("Terrestrial Plant Model\tm_FRootMort\t" + Convert.ToString(m_FRootMort));
            sw.WriteLine("Terrestrial Plant Model\tc_FRootMort\t" + Convert.ToString(c_FRootMort));
            sw.WriteLine("Terrestrial Plant Model\tp2_StMort\t" + Convert.ToString(p2_StMort));
            sw.WriteLine("Terrestrial Plant Model\tp1_StMort\t" + Convert.ToString(p1_StMort));
            sw.WriteLine("Terrestrial Plant Model\tMaxFracStruct\t" + Convert.ToString(MaxFracStruct));
            sw.WriteLine("Terrestrial Plant Model\tLFSHalfSaturation_Fire\t" + Convert.ToString(LFSHalfSaturation_Fire));
            sw.WriteLine("Terrestrial Plant Model\tLFSScalar_Fire\t" + Convert.ToString(LFSScalar_Fire));
            sw.WriteLine("Terrestrial Plant Model\tNPPHalfSaturation_Fire\t" + Convert.ToString(NPPHalfSaturation_Fire));
            sw.WriteLine("Terrestrial Plant Model\tNPPScalar_Fire\t" + Convert.ToString(NPPScalar_Fire));
            sw.WriteLine("Terrestrial Plant Model\ter_min\t" + Convert.ToString(er_min));
            sw.WriteLine("Terrestrial Plant Model\ter_max\t" + Convert.ToString(er_max));
            sw.WriteLine("Terrestrial Plant Model\tdr_min\t" + Convert.ToString(dr_min));
            sw.WriteLine("Terrestrial Plant Model\tdr_max\t" + Convert.ToString(dr_max));
            sw.WriteLine("Terrestrial Plant Model\tfrm_min\t" + Convert.ToString(frm_min));
            sw.WriteLine("Terrestrial Plant Model\tfrm_max\t" + Convert.ToString(frm_max));
            sw.WriteLine("Terrestrial Plant Model\tstm_max\t" + Convert.ToString(stm_max));
            sw.WriteLine("Terrestrial Plant Model\tstm_min\t" + Convert.ToString(stm_min));
            sw.WriteLine("Terrestrial Plant Model\tBaseScalar_Fire\t" + Convert.ToString(BaseScalar_Fire));
            sw.WriteLine("Terrestrial Plant Model\tMinReturnInterval\t" + Convert.ToString(MinReturnInterval));
            sw.WriteLine("Terrestrial Plant Model\tCarbonToLeafDryMatterScalar\t" + Convert.ToString(MassCarbonPerMassLeafDryMatter));
            sw.WriteLine("Terrestrial Plant Model\tLeafDryMatterToLeafWetMatterScalar\t" + Convert.ToString(MassLeafDryMatterPerMassLeafWetMatter));


        }


        /// <summary>
        /// Estimate the mass of leaves in a specified stock in the specified grid cell at equilibrium, given current environmental conditions
        /// </summary>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="deciduous">Whether the leaves in the specified stock are deciduous</param>
        /// <returns>The equilibrium mass of leaves in the specified stock</returns>
        public double CalculateEquilibriumLeafMass(SortedList<string, double[]> cellEnvironment, bool deciduous)
        {
            // Calculate annual average temperature
            double MeanTemp = cellEnvironment["Temperature"].Average();

            // Calculate total annual precipitation
            double TotalPrecip = cellEnvironment["Precipitation"].Sum();

            // Calculate total annual AET
            double TotalAET = cellEnvironment["AET"].Sum();

            // Calculate NPP using the Miami model
            double NPP = this.CalculateMiamiNPP(MeanTemp, TotalPrecip);


            // Calculate fractional allocation to structural tissue
            double FracStruct = this.CalculateFracStruct(NPP);

            // Calculate the fractional allocation of NPP to evergreen plant matter
            double FracEvergreen = this.CalculateFracEvergreen(cellEnvironment["Fraction Year Frost"][0]);

            // Calculate the fire mortality rate
            double FireMortRate = this.CalculateFireMortalityRate(NPP, cellEnvironment["Fraction Year Fire"][0]);

            // Update NPP depending on whether the acting stock is deciduous or evergreen
            if (deciduous)
            {
                NPP *= (1 - FracEvergreen);
            }
            else
            {
                NPP *= FracEvergreen;
            }

            // Calculate fine root mortality rate
            double FRootMort = this.CalculateFineRootMortalityRate(MeanTemp);

            
            // Calculate the structural mortality rate
            double StMort = this.CalculateStructuralMortality(TotalAET);

            double LeafMortRate;

            if (deciduous)
            {
                // Calculate deciduous leaf mortality
                LeafMortRate = this.CalculateDeciduousAnnualLeafMortality(MeanTemp);

            }
            else
            {
                // Calculate evergreen leaf mortality
                LeafMortRate = this.CalculateEvergreenAnnualLeafMortality(MeanTemp);
            }

            // Calculate the fractional mortality of leaves
            double LeafMortFrac = this.CalculateLeafFracAllocation(LeafMortRate, FRootMort);

            // Calculate leaf C fixation
            double LeafCFixation = NPP * (1 - FracStruct) * LeafMortFrac;

            // Calculate leaf carbon mortality
            double LeafCMortality = LeafMortRate + FireMortRate + StMort;

            // Calculate equilibrium leaf carbon in kg C per m2
            double EquilibriumLeafCarbon = LeafCFixation / LeafCMortality;

            // Convert to equilibrium leaf wet matter content
            double LeafWetMatter = this.ConvertToLeafWetMass(EquilibriumLeafCarbon, cellEnvironment["Cell Area"][0]);

            return LeafWetMatter;
        }

        /// <summary>
        /// Update the leaf stock during a time step given the environmental conditions in the grid cell
        /// </summary>
        /// <param name="cellEnvironment">The environment in the current grid cell</param>
        /// <param name="gridCellStocks">The stocks in the current grid cell</param>
        /// <param name="actingStock">The position of the acting stock in the array of grid cell stocks</param>
        /// <param name="currentTimeStep">The current model time step</param>
        /// <param name="deciduous">Whether the acting stock consists of deciduous leaves</param>
        /// <param name="GlobalModelTimeStepUnit">The time step unit used in the model</param>
        /// <param name="tracker">Whether to track properties of the ecological processes</param>
        /// <param name="globalTracker">Whether to output data describing the global environment</param>
        /// <param name="currentMonth">The current model month</param>
        /// <param name="outputDetail">The level of detail to use in model outputs</param>
        /// <param name="specificLocations">Whether the model is being run for specific locations</param>
        public double UpdateLeafStock(SortedList<string, double[]> cellEnvironment, GridCellStockHandler gridCellStocks, int[] actingStock,
            uint currentTimeStep, bool deciduous, string GlobalModelTimeStepUnit, ProcessTracker tracker, GlobalProcessTracker globalTracker, 
            uint currentMonth, string outputDetail, bool specificLocations)
        {


            // ESTIMATE ANNUAL LEAF CARBON FIXATION ASSUMING ENVIRONMENT THROUGHOUT THE YEAR IS THE SAME AS IN THIS MONTH

            // Calculate annual NPP
            double NPP = this.CalculateMiamiNPP(cellEnvironment["Temperature"].Average(), cellEnvironment["Precipitation"].Sum());

            // Calculate fractional allocation to structural tissue
            double FracStruct = this.CalculateFracStruct(NPP);

            // Estimate monthly NPP based on seasonality layer
            NPP *= cellEnvironment["Seasonality"][currentMonth];


            // Calculate leaf mortality rates
            double AnnualLeafMortRate;
            double MonthlyLeafMortRate;
            double TimeStepLeafMortRate;

            if (deciduous)
            {
                // Calculate annual deciduous leaf mortality
                AnnualLeafMortRate = this.CalculateDeciduousAnnualLeafMortality(cellEnvironment["Temperature"].Average());

                // For deciduous plants monthly leaf mortality is weighted by temperature deviance from the average, to capture seasonal patterns
                double[] ExpTempDev = new double[12];
                double SumExpTempDev = 0.0;
                double[] TempDev = new double[12];
                double Weight;
                for (int i = 0; i < 12; i++)
                {
                    TempDev[i] = cellEnvironment["Temperature"][i] - cellEnvironment["Temperature"].Average();
                    ExpTempDev[i] = Math.Exp(-TempDev[i] / 3);
                    SumExpTempDev += ExpTempDev[i];
                }
                Weight = ExpTempDev[currentMonth] / SumExpTempDev;
                MonthlyLeafMortRate = AnnualLeafMortRate * Weight;
                TimeStepLeafMortRate = MonthlyLeafMortRate * Utilities.ConvertTimeUnits(GlobalModelTimeStepUnit, "month");
            }
            else
            {
                // Calculate annual evergreen leaf mortality
                AnnualLeafMortRate = this.CalculateEvergreenAnnualLeafMortality(cellEnvironment["Temperature"].Average());

                // For evergreen plants, leaf mortality is assumed to be equal throughout the year
                MonthlyLeafMortRate = AnnualLeafMortRate * (1.0 / 12.0);
                TimeStepLeafMortRate = MonthlyLeafMortRate * Utilities.ConvertTimeUnits(GlobalModelTimeStepUnit, "month");
            }

            // Calculate fine root mortality rate
            double AnnualFRootMort = this.CalculateFineRootMortalityRate(cellEnvironment["Temperature"][currentMonth]);

            // Calculate the NPP allocated to non-structural tissues
            double FracNonStruct = (1 - FracStruct);

            // Calculate the fractional allocation to leaves
            double FracLeaves = FracNonStruct * this.CalculateLeafFracAllocation(AnnualLeafMortRate, AnnualFRootMort);

            // Calculate the fractional allocation of NPP to evergreen plant matter
            double FracEvergreen = this.CalculateFracEvergreen(cellEnvironment["Fraction Year Frost"][0]);

            // Update NPP depending on whether the acting stock is deciduous or evergreen
            if (deciduous)
            {
                NPP *= (1 - FracEvergreen);
            }
            else
            {
                NPP *= FracEvergreen;
            }
          
            // Calculate the fire mortality rate
            double FireMortRate = this.CalculateFireMortalityRate(NPP, cellEnvironment["Fraction Year Fire"][0]);

            // Calculate the structural mortality rate
            double StMort = this.CalculateStructuralMortality(cellEnvironment["AET"][currentMonth] * 12);

            // Calculate leaf C fixation
            double LeafCFixation = NPP * FracLeaves;

            // Convert from carbon to leaf wet matter
            double WetMatterIncrement = this.ConvertToLeafWetMass(LeafCFixation, cellEnvironment["Cell Area"][0]);

            // Convert from the monthly time step used for this process to the global model time step unit
            WetMatterIncrement *= Utilities.ConvertTimeUnits(GlobalModelTimeStepUnit, "month");




            // Add the leaf wet matter to the acting stock
            //gridCellStocks[actingStock].TotalBiomass += Math.Max(-gridCellStocks[actingStock].TotalBiomass, WetMatterIncrement);
            double NPPWetMatter = Math.Max(-gridCellStocks[actingStock].TotalBiomass, WetMatterIncrement);


            // If the processer tracker is enabled and output detail is high and the model is being run for specific locations, then track the biomass gained through primary production
            if (tracker.TrackProcesses && (outputDetail == "high") && specificLocations)
            {
                tracker.TrackPrimaryProductionTrophicFlow((uint)cellEnvironment["LatIndex"][0], (uint)cellEnvironment["LonIndex"][0],
                    Math.Max(-gridCellStocks[actingStock].TotalBiomass, WetMatterIncrement));

            }

            if (globalTracker.TrackProcesses)
            {
                globalTracker.RecordNPP((uint)cellEnvironment["LatIndex"][0], (uint)cellEnvironment["LonIndex"][0],(uint)actingStock[0],
                    this.ConvertToLeafWetMass(NPP, cellEnvironment["Cell Area"][0]) * 
                    Utilities.ConvertTimeUnits(GlobalModelTimeStepUnit, "month")/cellEnvironment["Cell Area"][0]);
            }
            // Calculate fractional leaf mortality
            double LeafMortFrac = 1 - Math.Exp(-TimeStepLeafMortRate);

            // Update the leaf stock biomass owing to the leaf mortality
            gridCellStocks[actingStock].TotalBiomass *= (1 - LeafMortFrac);
            NPPWetMatter *= (1 - LeafMortFrac);

            return (NPPWetMatter);
            
        }

        /// <summary>
        /// Calculate NPP in kg C per m2
        /// </summary>
        /// <param name="temperature">Current temperature, in degrees Celsius</param>
        /// <param name="precipitation">Current precipitation, in mm</param>
        /// <returns></returns>
        public double CalculateMiamiNPP(double temperature, double precipitation)
        {
            // Calculate the maximum annual NPP that could be sustained if average temperature were equal to this month's temperature
            double NPPTemp = max_NPP / (1 + Math.Exp(t1_NPP - t2_NPP * temperature));

            // Calculate theC:\madingley-ecosystem-model\Madingley\Ecological processes cohorts\Reproduction implementations\ReproductionBasic.cs maximum annual NPP that could be sustained if precipitation in every other month was equal to this month's precipitation
            double NPPPrecip = max_NPP * (1 - Math.Exp(-p_NPP * precipitation));

            // Calculate the maximum annual NPP that could be sustained based on temperature and precipitation
            return Math.Min(NPPTemp, NPPPrecip);

        }

        /// <summary>
        /// Calculate the fractional allocation of productivity to structural tissue
        /// </summary>
        /// <param name="NPP">Net primary productivity</param>
        /// <returns>The fractional allocation of productivity to structural tissue</returns>
        public double CalculateFracStruct(double NPP)
        {
            double MinFracStruct = 0.01; // This prevents the prediction becoming zero (makes likelihood calculation difficult)
            double FracStruc = MinFracStruct * (Math.Exp(FracStructScalar * NPP) / (1 + MinFracStruct * (Math.Exp(FracStructScalar * NPP) - 1.0)));
            if (FracStruc > 0.99) FracStruc = 1 - MinFracStruct;
            FracStruc *= MaxFracStruct;
            return FracStruc;

        }

        /// <summary>
        /// Calculate the fractional allocation of productivity to evergreen plant matter
        /// </summary>
        /// <param name="NDF">The proportion of the current year subject to frost</param>
        /// <returns>The fractional allocation of productivity to evergreen plant matter</returns>
        public double CalculateFracEvergreen(double NDF)
        {
            double imed1 = a_FracEvergreen * NDF * NDF + b_FracEvergreen * NDF + c_FracEvergreen;
            if (imed1 < 0) imed1 = 0;
            if (imed1 > 1) imed1 = 1;
            return imed1;
        }

        /// <summary>
        /// Calculate the mortality rate of evergreen leaves
        /// </summary>
        /// <param name="temperature">Current temperature, in degrees Celsius</param>
        /// <returns>The mortality rate of evergreen leaves</returns>
        public double CalculateEvergreenAnnualLeafMortality(double temperature)
        {
            double EstimatedRate = Math.Exp(m_EGLeafMortality * temperature - c_EGLeafMortality);
            if (EstimatedRate > er_max) EstimatedRate = er_max;
            if (EstimatedRate < er_min) EstimatedRate = er_min;
            return EstimatedRate;
        }

        /// <summary>
        /// Calculate the mortality rate of deciduous leaves
        /// </summary>
        /// <param name="temperature">Current temperature, in degrees Celsius</param>
        /// <returns>The mortality rate of deciduous leaves</returns>
        public double CalculateDeciduousAnnualLeafMortality(double temperature)
        {
            double EstimatedRate = Math.Exp(-(m_DLeafMortality * temperature + c_DLeafMortality));
            if (EstimatedRate > dr_max) EstimatedRate = dr_max;
            if (EstimatedRate < dr_min) EstimatedRate = dr_min;
            return EstimatedRate;
        }

        /// <summary>
        /// Calculate the fraction of NPP allocated to non-structural tissue that is allocated to leaves
        /// </summary>
        /// <param name="LeafMortRate">The mortality rate of leaves</param>
        /// <param name="FRootMort">The mortality rate of fine roots</param>
        /// <returns>The fractional mortality of leaves</returns>
        public double CalculateLeafFracAllocation(double LeafMortRate, double FRootMort)
        {
            return LeafMortRate / (LeafMortRate + FRootMort);
        }

        /// <summary>
        /// Calculate the mortality rate of fine roots
        /// </summary>
        /// <param name="temperature">Current temperature, in degrees Celsius</param>
        /// <returns>The mortality rate of fine roots</returns>
        public double CalculateFineRootMortalityRate(double temperature)
        {
            double EstimatedRate = Math.Exp(m_FRootMort * temperature + c_FRootMort);
            if (EstimatedRate > frm_max) EstimatedRate = frm_max;
            if (EstimatedRate < frm_min) EstimatedRate = frm_min;
            return EstimatedRate;
        }

        /// <summary>
        /// Calculate the rate of plant mortality to fire
        /// </summary>
        /// <param name="NPP">Net Primary Productivity, in kg C per m2</param>
        /// <param name="FractionYearFireSeason">The fraction of the year subject to fires</param>
        /// <returns>The rate of plant mortality to fire</returns>
        public double CalculateFireMortalityRate(double NPP, double FractionYearFireSeason)
        {
            double NPPFunction = (1.0 / (1.0 + Math.Exp(-NPPScalar_Fire * (NPP - NPPHalfSaturation_Fire))));
            double LFSFunction = (1.0 / (1.0 + Math.Exp(-LFSScalar_Fire * (FractionYearFireSeason - LFSHalfSaturation_Fire))));
            double TempRate = BaseScalar_Fire * NPPFunction * LFSFunction;
            if (TempRate > 1.0) TempRate = 1.0;
            double Rate = (TempRate <= MinReturnInterval) ? MinReturnInterval : TempRate;
            return Rate;
        }

        /// <summary>
        /// Calculate the mortality rate of plant structural tissue
        /// </summary>
        /// <param name="AET">Actual evapotranspiration, in mm</param>
        /// <returns>The mortality rate of plant structural tissue</returns>
        public double CalculateStructuralMortality(double AET)
        {
            double EstimatedRate = Math.Exp(p2_StMort * AET / 1000 + p1_StMort);
            if (EstimatedRate > stm_max) EstimatedRate = stm_max;
            if (EstimatedRate < stm_min) EstimatedRate = stm_min;
            return EstimatedRate;
        }

        /// <summary>
        /// Calculate leaf carbon, in kg C per m2
        /// </summary>
        /// <param name="NPP">Net Primary Productivity, in kg C per m2</param>
        /// <param name="FracStruct">Fractional allocation to structural tissue</param>
        /// <param name="LeafMortFrac">Fractional mortality of leaves</param>
        /// <param name="LeafMortRate">Rate of mortality of leaves</param>
        /// <param name="FireMortRate">Rate of mortality to fire</param>
        /// <param name="StMort">Rate of mortality of structural tissue</param>
        /// <returns>Leaf carbon, in kg C per m2</returns>
        public double CalculateLeafCarbon(double NPP, double FracStruct, double LeafMortFrac, double LeafMortRate, double FireMortRate, double StMort)
        {
            double LeafCFixation = this.CalculateLeafCFixation(NPP, FracStruct, LeafMortFrac);
            double LeafCMortality = LeafMortRate + FireMortRate + StMort;
            return LeafCFixation / LeafCMortality;
        }

        /// <summary>
        /// Calculate the carbon fixed by leaves, in kg C per m2
        /// </summary>
        /// <param name="NPP">Net Primary Productivity, in kg C per m2</param>
        /// <param name="FracStruct">Fractional allocation to structural tissue</param>
        /// <param name="LeafMortFrac">Fractional mortality of leaves</param>
        /// <returns>The carbon fixed by leaves, in kg C per m2</returns>
        public double CalculateLeafCFixation(double NPP, double FracStruct, double LeafMortFrac)
        {
            return NPP * (1 - FracStruct * MaxFracStruct) * LeafMortFrac;
        }

        /// <summary>
        /// Convert from kg C per m2 to g of leaf wet matter in the entire grid cell
        /// </summary>
        /// <param name="kgCarbon">Value to convert, in kg C per m2</param>
        /// <param name="cellArea">The area of the grid cell</param>
        /// <returns>Value in g of wet matter in the grid cell</returns>
        public double ConvertToLeafWetMass(double kgCarbon, double cellArea)
        {
            // Convert from kg to g
            double gCarbonPerM2 = kgCarbon * 1000;

            // Convert from m2 to km2
            double gCarbonPerKm2 = gCarbonPerM2 * m2Tokm2Conversion;

            // Convert from km2 to cell area
            double gCarbonPerCell = gCarbonPerKm2 * cellArea;

            // Convert from g C to g dry matter
            // mass of Leaf C per g leaf dry matter = 0.4761 g g-1 (from Kattge et al. (2011), TRY- A global database of plant traits, Global Change Biology)
            double LeafDryMatter = gCarbonPerCell / MassCarbonPerMassLeafDryMatter;

            // Convert from dry matter to wet matter
            // mass of leaf dry matter per g leaf wet matter = 0.213 g g-1 (from Kattge et al. (2011), TRY- A global database of plant traits, Global Change Biology)
            return LeafDryMatter / MassLeafDryMatterPerMassLeafWetMatter;

        }

        /// <summary>
        /// Convert from g of plant wet matter in the entire grid cell to kg C per m2
        /// </summary>
        /// <param name="leafWetMatter">The value to convert as total g wet matter in the grid cell</param>
        /// <param name="cellArea">The area of the grid cell</param>
        /// <returns>Value in kg C per m2</returns>
        public double ConvertToKgCarbonPerM2(double leafWetMatter, double cellArea)
        {
            // Convert from wet matter to dry matter
            double LeafDryMatter = leafWetMatter / 2;

            // Convert from dry matter to g C per grid cell
            double gCarbonPerCell = LeafDryMatter * 2;

            // Convert from cell area to km2
            double gCarbonPerKm2 = gCarbonPerCell / cellArea;

            // Convert from km2 to m2
            double gCarbonPerM2 = gCarbonPerKm2 / m2Tokm2Conversion;

            // Convert from g carbon to kg carbon
            return gCarbonPerM2 / 1000;





        }

        #endregion




    }
}
