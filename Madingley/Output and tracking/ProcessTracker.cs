using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// Tracks diagnostics about the ecological processes
    /// </summary>
    public class ProcessTracker
    {
        /// <summary>
        /// Whether to track processes
        /// </summary>
        private Boolean _TrackProcesses;
        /// <summary>
        /// Get or set whether to track processes
        /// </summary>
        public Boolean TrackProcesses
        {
            get { return _TrackProcesses; }
            set { _TrackProcesses = value; }
        }
        
        /// <summary>
        /// Instance of the reproduction tracker within the process tracker
        /// </summary>
        private ReproductionTracker  _TrackReproduction;
        /// <summary>
        /// Get and set the reproduction tracker
        /// </summary>
        public ReproductionTracker  TrackReproduction
        {
            get { return _TrackReproduction; }
            set { _TrackReproduction = value; }
        }

        /// <summary>
        /// Instance of predation tracker
        /// </summary>
        private PredationTracker  _TrackPredation;
        /// <summary>
        /// Get and set the predation tracker
        /// </summary>
        public PredationTracker  TrackPredation
        {
            get { return _TrackPredation; }
            set { _TrackPredation = value; }
        }

        /// <summary>
        /// Instance of the eating tracker
        /// </summary>
        private EatingTracker _TrackEating;
        /// <summary>
        /// Get and set the eating tracker
        /// </summary>
        public EatingTracker TrackEating
        {
            get { return _TrackEating; }
            set { _TrackEating = value; }
        }

        /// <summary>
        /// Instance of the growth tracker
        /// </summary>
        private GrowthTracker _TrackGrowth;
        /// <summary>
        /// Get and set the growth tracker
        /// </summary>
        public GrowthTracker TrackGrowth
        {
            get { return _TrackGrowth; }
            set { _TrackGrowth = value; }
        }

        /// <summary>
        /// Instance of the mortality tracker
        /// </summary>
        private MortalityTracker _TrackMortality;
        /// <summary>
        /// Get and set the mortality tracker
        /// </summary>
        public MortalityTracker TrackMortality
        {
            get { return _TrackMortality; }
            set { _TrackMortality = value; }
        }

        /// <summary>
        /// An instance of the extinction tracker
        /// </summary>
        private ExtinctionTracker _TrackExtinction;
        /// <summary>
        /// Get and set the instance of the extinction tracker
        /// </summary>
        public ExtinctionTracker TrackExtinction
        {
            get { return _TrackExtinction; }
            set { _TrackExtinction = value; }
        }

        /// <summary>
        /// An instance of the metabolism tracker
        /// </summary>
        private MetabolismTracker _TrackMetabolism;
        /// <summary>
        /// Get and set the instance of the metabolism tracker
        /// </summary>
        public MetabolismTracker TrackMetabolism
        {
            get { return _TrackMetabolism; }
            set { _TrackMetabolism = value; }
        }
       


        /// <summary>
        /// Constructor for process tracker: Initialises the trackers for individual processes
        /// </summary>
        /// <param name="numTimesteps">The number of time steps in the model</param>
        /// <param name="lats">The latitudes of active grid cells in the model</param>
        /// <param name="lons">The longitudes of active grid cells in the model</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="Filenames">The filenames of the output files to write the tracking results to</param>
        /// <param name="trackProcesses">Whether to track processes</param>
        /// <param name="cohortDefinitions">The definitions for cohort functional groups in the model</param>
        /// <param name="missingValue">The missing value to use in process tracking output files</param>
        /// <param name="outputFileSuffix">The suffix to be applied to output files from process tracking</param>
        /// <param name="outputPath">The path to the folder to be used for process tracking outputs</param>
        /// <param name="trackerMassBins">The mass bins to use for categorising output data in the process trackers</param>
        /// <param name="specificLocations">Whether the model is being run for specific locations</param>
        /// <param name="cellIndex">The index of the current cell in the list of all cells to run the model for</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        /// <param name="latCellSize">The size of grid cells in the latitudinal direction</param>
        /// <param name="lonCellSize">The size of grid cells in the longitudinal direction</param>
        public ProcessTracker(uint numTimesteps,
            float[] lats, float[] lons, 
            List<uint[]> cellIndices,
            SortedList<string,string> Filenames, 
            Boolean trackProcesses, 
            FunctionalGroupDefinitions cohortDefinitions, 
            double missingValue,
            string outputFileSuffix,
            string outputPath, MassBinsHandler trackerMassBins,
            Boolean specificLocations,
            int cellIndex,
            MadingleyModelInitialisation initialisation,
            bool marineCell,
            float latCellSize,
            float lonCellSize)
        {
            // Initialise trackers for ecological processes
            _TrackProcesses = trackProcesses;

            if (_TrackProcesses)
            {
                _TrackReproduction = new ReproductionTracker(numTimesteps, (uint)lats.Length, (uint)lons.Length, cellIndices, Filenames["NewCohortsOutput"], Filenames["MaturityOutput"], outputFileSuffix, outputPath, cellIndex);
                _TrackEating = new EatingTracker((uint)lats.Length, (uint)lons.Length, Filenames["TrophicFlowsOutput"], outputFileSuffix, outputPath, cellIndex, initialisation, marineCell);
                _TrackGrowth = new GrowthTracker(numTimesteps, (uint)lats.Length, (uint)lons.Length, cellIndices, Filenames["GrowthOutput"], outputFileSuffix, outputPath, cellIndex);
                _TrackMortality = new MortalityTracker(numTimesteps, (uint)lats.Length, (uint)lons.Length, cellIndices, Filenames["MortalityOutput"], outputFileSuffix, outputPath, cellIndex);
                _TrackExtinction = new ExtinctionTracker(Filenames["ExtinctionOutput"], outputPath, outputFileSuffix, cellIndex);
                _TrackMetabolism = new MetabolismTracker(Filenames["MetabolismOutput"], outputPath, outputFileSuffix, cellIndex);

                // Initialise the predation and herbivory trackers only for runs with specific locations
                if (specificLocations == true)
                {
                    _TrackPredation = new PredationTracker( numTimesteps, cellIndices, Filenames["PredationFlowsOutput"], cohortDefinitions,
                        missingValue, outputFileSuffix, outputPath, trackerMassBins, cellIndex);
                }
            }
        }

        /// <summary>
        /// Record a new cohort in the reproduction tracker
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="timestep">The current model time step</param>
        /// <param name="offspringCohortAbundance">The number of individuals in the new cohort</param>
        /// <param name="parentCohortAdultMass">The adult body mass of the parent cohort</param>
        /// <param name="functionalGroup">The functional group that the parent and offspring cohorts belong to</param>
        /// <param name="parentCohortIDs">All cohort IDs associated with the acting parent cohort</param>
        /// <param name="offspringCohortID">The cohort ID that has been assigned to the produced offspring cohort</param>
        public void RecordNewCohort(uint latIndex, uint lonIndex, uint timestep, double offspringCohortAbundance, 
            double parentCohortAdultMass, int functionalGroup, List<uint> parentCohortIDs, uint offspringCohortID)
        {
            _TrackReproduction.RecordNewCohort(latIndex, lonIndex, timestep, offspringCohortAbundance, parentCohortAdultMass, 
                functionalGroup,parentCohortIDs,offspringCohortID);
        }

        /// <summary>
        /// Track the maturity of a cohort in the reproduction tracker
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="timestep">The current model time step</param>
        /// <param name="birthTimestep">The birth time step of the cohort reaching maturity</param>
        /// <param name="juvenileMass">The juvenile mass of the cohort reaching maturity</param>
        /// <param name="adultMass">The adult mass of the cohort reaching maturity</param>
        /// <param name="functionalGroup">The functional group of the cohort reaching maturity</param>
        public void TrackMaturity(uint latIndex, uint lonIndex, uint timestep, uint birthTimestep, double juvenileMass, double adultMass, int functionalGroup)
        {
            _TrackReproduction.TrackMaturity(latIndex,lonIndex,timestep,birthTimestep,juvenileMass,adultMass,functionalGroup);
        }

        /// <summary>
        /// Track the flow of mass between trophic levels during a predation event
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="fromFunctionalGroup">The index of the functional group being eaten</param>
        /// <param name="toFunctionalGroup">The index of the functional group that the predator belongs to</param>
        /// <param name="cohortFunctionalGroupDefinitions">The functional group definitions of cohorts in the model</param>
        /// <param name="massEaten">The mass eaten during the predation event</param>
        /// <param name="predatorBodyMass">The body mass of the predator doing the eating</param>
        /// <param name="preyBodyMass">The body mass of the prey doing the eating</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        public void TrackPredationTrophicFlow(uint latIndex, uint lonIndex, int fromFunctionalGroup, int toFunctionalGroup,
            FunctionalGroupDefinitions cohortFunctionalGroupDefinitions, double massEaten, double predatorBodyMass, double preyBodyMass,
            MadingleyModelInitialisation initialisation, Boolean marineCell)
        {
            _TrackEating.RecordPredationTrophicFlow(latIndex, lonIndex, fromFunctionalGroup, toFunctionalGroup, cohortFunctionalGroupDefinitions, massEaten, predatorBodyMass, preyBodyMass, initialisation, marineCell);
        }

        /// <summary>
        /// Track the flow of mass between trophic levels during a herbivory event
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="toFunctionalGroup">The index of the functional group that the predator belongs to</param>
        /// <param name="cohortFunctionalGroupDefinitions">The functional group definitions of cohorts in the model</param>
        /// <param name="massEaten">The mass eaten during the herbivory event</param>
        /// <param name="predatorBodyMass">The body mass of the predator doing the eating</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        public void TrackHerbivoryTrophicFlow(uint latIndex, uint lonIndex, int toFunctionalGroup, 
            FunctionalGroupDefinitions cohortFunctionalGroupDefinitions, double massEaten, double predatorBodyMass, 
            MadingleyModelInitialisation initialisation, Boolean marineCell)
        {
            _TrackEating.RecordHerbivoryTrophicFlow(latIndex, lonIndex, toFunctionalGroup, cohortFunctionalGroupDefinitions, massEaten, predatorBodyMass, initialisation, marineCell);
        }

        /// <summary>
        /// Track the flow of mass between trophic levels during primary production of autotrophs
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="massEaten">The mass gained through primary production</param>
        public void TrackPrimaryProductionTrophicFlow(uint latIndex, uint lonIndex, double massEaten)
        {
            _TrackEating.RecordPrimaryProductionTrophicFlow(latIndex, lonIndex, massEaten);
        }

        /// <summary>
        /// Write trophic flow data from the current time step to file 
        /// </summary>
        /// <param name="currentTimeStep">The current model time step</param>
        /// <param name="numLats">The number of grid cells, latitudinally, in the simulation</param>
        /// <param name="numLons">The number of grid cells, longitudinally, in the simulation</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        public void WriteTimeStepTrophicFlows(uint currentTimeStep,uint numLats,uint numLons, MadingleyModelInitialisation initialisation, 
            Boolean marineCell)
        {
            _TrackEating.WriteTrophicFlows(currentTimeStep, numLats, numLons, initialisation, marineCell);
        }

        /// <summary>
        /// Track growth of individuals in a cohort using the growth tracker
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="timeStep">The current model time step</param>
        /// <param name="currentBodyMass">The current body mass of individuals in the cohort</param>
        /// <param name="functionalGroup">The funcitonal group of the cohort being tracked</param>
        /// <param name="netGrowth">The net growth of individuals in the cohort this time step</param>
        /// <param name="metabolism">The mass lost to indivduals in the cohort through metabolism</param>
        /// <param name="predation">The mass gained by individuals in the cohort through predation</param>
        /// <param name="herbivory">The mass gained by individuals in the cohort through herbivory</param>
        public void TrackTimestepGrowth(uint latIndex, uint lonIndex, uint timeStep, double currentBodyMass, 
            int functionalGroup, double netGrowth, double metabolism, double predation, double herbivory)
        {
            _TrackGrowth.RecordGrowth(latIndex, lonIndex, timeStep, currentBodyMass, functionalGroup, netGrowth, metabolism, predation,herbivory);
        }

        /// <summary>
        /// Records the flow of mass between a prey and its predator during a predation event
        /// </summary>
        /// <param name="currentTimeStep">The current model time step</param>
        /// <param name="preyBodyMass">The individual body mass of the prey</param>
        /// <param name="predatorBodyMass">The individual body mass of the predator</param>
        /// <param name="massFlow">The flow of mass between predator and prey</param>
        public void RecordPredationMassFlow(uint currentTimeStep, double preyBodyMass, double predatorBodyMass, double massFlow)
        {
            _TrackPredation.RecordFlow(currentTimeStep, preyBodyMass, predatorBodyMass, massFlow);
        }

        /// <summary>
        /// Adds the mass flows from predation in the current time step to the output file and then resets the mass flow tracker
        /// </summary>
        /// <param name="currentTimeStep">The current model time step</param>
        public void EndTimeStepPredationTracking(uint currentTimeStep)
        {
            _TrackPredation.AddTimestepFlows((int)currentTimeStep);
            _TrackPredation.ResetPredationTracker();
        }

        /// <summary>
        /// Records the flow of mass between primary producers and herbivores during a herbivory event
        /// </summary>
        /// <param name="currentTimeStep">The current model time step</param>
        /// <param name="herbivoreBodyMass">The individual body mass of the herbivore</param>
        /// <param name="massFlow">The flow of mass between the primary producer and the herbivore</param>
        public void RecordHerbivoryMassFlow(uint currentTimeStep, double herbivoreBodyMass, double massFlow)
        {
            //_TrackHerbivory.RecordFlow(currentTimeStep, herbivoreBodyMass, massFlow);
        }

        /// <summary>
        /// Adds the mass flows from herbivory in the current time step to the output file and then resets the mass flow tracker
        /// </summary>
        /// <param name="currentTimeStep"></param>
        public void EndTimeStepHerbvioryTracking(uint currentTimeStep)
        {
            //_TrackHerbivory.AddTimestepFlows((int)currentTimeStep);
            //_TrackHerbivory.ResetHerbivoryTracker();
        }

        /// <summary>
        /// Record an instance of mortality in the output file
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="birthTimeStep">The time step in which this cohort was born</param>
        /// <param name="timeStep">The current model time step</param>
        /// <param name="currentMass">The current body mass of individuals in the cohort</param>
        /// <param name="adultMass">The adult mass of individuals in the cohort</param>
        /// <param name="functionalGroup">The functional group of the cohort suffering mortality</param>
        /// <param name="cohortID">The ID of the cohort suffering mortality</param>
        /// <param name="numberDied">The number of individuals dying in this mortality event</param>
        /// <param name="mortalitySource">The type of mortality causing the individuals to die</param>
        public void RecordMortality(uint latIndex, uint lonIndex, uint birthTimeStep, uint timeStep, double currentMass, double adultMass, uint functionalGroup, uint cohortID, 
            double numberDied,string mortalitySource)
        {
            _TrackMortality.RecordMortality(latIndex, lonIndex, birthTimeStep,
                timeStep, currentMass, adultMass, functionalGroup, cohortID, numberDied, mortalitySource);
        }

        /// <summary>
        /// Output the mortality profile of a cohort becoming extinct
        /// </summary>
        /// <param name="cohortID">The ID of the cohort becoming extinct</param>
        public void OutputMortalityProfile(uint cohortID)
        {
            _TrackMortality.OutputMortalityProfile(cohortID);
        }

        /// <summary>
        /// Record the extinction of a cohort
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="currentTimeStep">THe current time step</param>
        /// <param name="merged">Whether the cohort becoming extinct has ever been merged</param>
        /// <param name="cohortIDs">The IDs of all cohorts that have contributed individuals to the cohort going extinct</param>
        public void RecordExtinction(uint latIndex, uint lonIndex,uint currentTimeStep,bool merged,List<uint>cohortIDs)
        {
            _TrackExtinction.RecordExtinction(latIndex, lonIndex, currentTimeStep, merged, cohortIDs);
        }

        /// <summary>
        /// Tracks the mass lost by individuals in a cohort in a time step through metabolism
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="timeStep">The current model time step</param>
        /// <param name="currentBodyMass">The body mass of individuals in the acting cohort</param>
        /// <param name="functionalGroup">The functional group index of the acting cohort</param>
        /// <param name="temperature">The ambient temperature in the grid cell</param>
        /// <param name="metabolicLoss">The mass lost by individuals through metabolism</param>
        public void TrackTimestepMetabolism(uint latIndex, uint lonIndex, uint timeStep, double currentBodyMass, 
            int functionalGroup, double temperature, double metabolicLoss)
        {
            _TrackMetabolism.RecordMetabolism(latIndex, lonIndex, timeStep, currentBodyMass, functionalGroup, temperature, metabolicLoss);
        }


        /// <summary>
        /// Close all tracker streams
        /// </summary>
		public void CloseStreams(Boolean SpecificLocations)
        {
            _TrackReproduction.CloseStreams();
            _TrackEating.CloseStreams();
            _TrackGrowth.CloseStreams();
            _TrackMetabolism.CloseStreams();
            //_TrackNPP.CloseStreams();
            if (SpecificLocations == true)
            {
                _TrackPredation.CloseStreams();
            }
        }
    }
}
