using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.CSV;
using Microsoft.Research.Science.Data.Imperative;


namespace Madingley
{
    /// <summary>
    /// Reads in and performs look-ups on functional group definitions
    /// </summary>
    /// <remarks>Mass bins values currently defined as middle of each mass bins</remarks>
    /// <todoM>Throw error if there are any blanks in csv file</todoM>
    public class FunctionalGroupDefinitions
    {
        /// <summary>
        /// An internal version of the dataset to query if necessary
        /// </summary>
        private DataSet InternalData;
        //A lookup device: sorted dictionary keyed by Functional Trait and valued by a sorted dictionary itself keyed by Unique Functional Trait Values and 
        //valued by an integer array of functional group indices corresponding to each functional trait value
        /// <summary>
        /// A dictionary to allow functional group indices to be looked up based on trait values
        /// </summary>
        private SortedDictionary<string, SortedDictionary<string, int[]>> IndexLookupFromTrait = new SortedDictionary<string, SortedDictionary<string, int[]>>();

        /// <summary>
        /// A sorted list of all of the properties of functional groups and their values
        /// </summary>
        private SortedList<string,double[]> _FunctionalGroupProperties;
        /// <summary>
        /// Get and set the sorted list of all of the properties of functional groups and their values
        /// </summary>
        public SortedList<string,double[]> FunctionalGroupProperties
        {
            get { return _FunctionalGroupProperties; }
            set { _FunctionalGroupProperties = value; }
        }

        /// <summary>
        /// Dictionary to allow traits of functional groups to be looked up based on the functional group index
        /// </summary>
        private SortedDictionary<string, string[]> TraitLookupFromIndex = new SortedDictionary<string, string[]>();

        /// <summary>
        /// A list of the indices of all functional groups in the model
        /// </summary>
        private int[] _AllFunctionalGroupsIndex;
        /// <summary>
        /// Get the list of the indices of all functional groups in the model
        /// </summary>
        public int[] AllFunctionalGroupsIndex { get { return _AllFunctionalGroupsIndex; } }
        

        /// <summary>
        /// Constructor for the functional group definitions: reads in the specified functional group definition file, 
        /// constructs lookup tables, mass ranges and initial cohort numbers in each functional group
        /// </summary>
        /// <param name="fileName">The name of the functional group definition file to be read in</param>
        /// <param name="outputPath">The path to the output folder, in which to copy the functional group definitions file</param>
        public FunctionalGroupDefinitions(string fileName, string outputPath)
        {
            // Construct the URI for the functional group definition file
            string FileString = "msds:csv?file=input/Model setup/ecological definition files/" + fileName + "&openMode=readOnly";

            // Copy the Function group definitions file to the output directory
            System.IO.File.Copy("input/Model setup/ecological definition files/" + fileName, outputPath + fileName, true);

            // Read in the data
            InternalData = DataSet.Open(FileString);

            // Initialise the lists
            _AllFunctionalGroupsIndex = new int[InternalData.Dimensions[0].Length];
            _FunctionalGroupProperties = new SortedList<string, double[]>();

            // Loop over columns in the functional group definitions file
            foreach (Variable v in InternalData.Variables)
            {
                // Get the column header
                string TraitName = v.Name.Split('_')[1].ToLower();
                // Get the values in this column
                var TempValues = v.GetData();

                // For functional group definitions
                if (System.Text.RegularExpressions.Regex.IsMatch(v.Name, "DEFINITION_"))
                {
                    // Declare a sorted dictionary to hold the index values for each unique trait value
                    SortedDictionary<string, int[]> TraitIndexValuesList = new SortedDictionary<string, int[]>();
                    // Create a string array with the values of this trait
                    string[] TempString = new string[TempValues.Length];
                    for (int nn = 0; nn < TempValues.Length; nn++)
                    {
                        TempString[nn] = TempValues.GetValue(nn).ToString().ToLower();

                        // Add the functional group index to the list of all indices
                        _AllFunctionalGroupsIndex[nn] = nn;
                    }
                    // Add the trait values to the trait-value lookup list
                    TraitLookupFromIndex.Add(TraitName, TempString);

                    // Get the unique values for this trait
                    var DistinctValues = TempString.Distinct().ToArray();
                    //Loop over the unique values for this trait and list all the functional group indices with the value
                    foreach (string DistinctTraitValue in DistinctValues.ToArray())
                    {
                        List<int> FunctionalGroupIndex = new List<int>();
                        //Loop over the string array associated with this trait and add the index values of matching string to a list
                        for (int kk = 0; kk < TempString.Length; kk++)
                        {
                            if (TempString[kk].Equals(DistinctTraitValue))
                            {
                                FunctionalGroupIndex.Add(kk);
                            }
                        }
                        //Add the unique trait value and the functional group indices to the temporary list
                        TraitIndexValuesList.Add(DistinctTraitValue, FunctionalGroupIndex.ToArray());
                    }
                    // Add the unique trait values and corresponding functional group indices to the functional group index lookup
                    IndexLookupFromTrait.Add(TraitName, TraitIndexValuesList);
                }
                // For functional group properties
                else if (System.Text.RegularExpressions.Regex.IsMatch(v.Name, "PROPERTY_"))
                {
                    // Get the values for this property
                    double[] TempDouble = new double[TempValues.Length];
                    for (int nn = 0; nn < TempValues.Length; nn++)
                    {
                        TempDouble[nn] = Convert.ToDouble(TempValues.GetValue(nn));
                    }
                    // Add the values to the list of functional group properties
                    _FunctionalGroupProperties.Add(TraitName, TempDouble);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(v.Name, "NOTES_"))
                {
                    // Ignore
                }
                // Otherwise, throw an error
                else
                {
                    Debug.Fail("All functional group data must be prefixed by DEFINITTION OR PROPERTY");
                }


            }

        }

        /// <summary>
        /// Return the value of a biological parameter for a given parameter and functional group
        /// </summary>
        /// <param name="propertyName">The name of the biological parameter</param>
        /// <param name="functionalGroup">Functional group index</param>
        /// <returns>The value of the biological parameter for the specified functional group</returns>
        public double GetBiologicalPropertyOneFunctionalGroup(string propertyName, int functionalGroup)
        {
            return FunctionalGroupProperties[propertyName.ToLower()][functionalGroup];
        }

        /// <summary>
        /// Get values of a functional group property for all functional groups
        /// </summary>
        /// <param name="propertyName">The name of the property to get values for</param>
        /// <returns>The values of a functional group property for all functional groups</returns>
        public double[] GetBiologicalPropertyAllFunctionalGroups(string propertyName)
        {
            return FunctionalGroupProperties[propertyName.ToLower()];
        }

        /// <summary>
        /// Retrieves the values for all traits defined in the model
        /// </summary>
        /// <returns>String array of traits defined for the model</returns>
        public string[] GetTraits()
        {
            List<string> Traits = new List<string>();

            foreach (var key in TraitLookupFromIndex.Keys)
            {
                Traits.Add(key);
            }

            return Traits.ToArray();
        }

        /// <summary>
        /// Retrieves the trait values for all traits defined in the model
        /// </summary>
        /// <param name="Trait">The trait for which trait values are to be found</param>
        /// <returns>String array of trait values for the specifiec trait</returns>
        public string[] GetUniqueTraitValues(string Trait)
        {
            List<string> TraitValues = new List<string>();
            SortedDictionary<string, int[]> temp = IndexLookupFromTrait[Trait.ToLower()];

            foreach (var key in temp.Keys)
            {
                if(!TraitValues.Contains(key)) TraitValues.Add(key);
            }

            return TraitValues.ToArray();
        }


        /// <summary>
        /// Returns a string of Trait Names associated with the specified search trait for all functional group index value
        /// </summary>
        /// <param name="searchTrait">The name of the trait to get values for</param>
        /// <returns>The value of the specified trait for the specified functional group</returns>
        public string[] GetTraitValuesAllFunctionalGroups(string searchTrait)
        {
            return TraitLookupFromIndex[searchTrait.ToLower()];
        }

        /// <summary>
        /// Returns a string of Trait Names associated with the specified search trait and functional group index value
        /// </summary>
        /// <param name="searchTrait">The name of the trait to get values for</param>
        /// <param name="functionalGroupIndex">The functional group index to return the trait value for</param>
        /// <returns>The value of the specified trait for the specified functional group</returns>
        public string GetTraitNames(string searchTrait, int functionalGroupIndex)
        {
             return TraitLookupFromIndex[searchTrait.ToLower()].GetValue(functionalGroupIndex).ToString();   
        }

        /// <summary>
        /// Gets a list of unique trait values for all functional groups
        /// </summary>
        /// <returns>A list of unique trait values</returns>
        public string[] GetAllTraitNames()
        {
            return IndexLookupFromTrait.Keys.Distinct().ToArray();
        }

        /// <summary>
        /// Get the values of a set of specified traits for a specified functional group
        /// </summary>
        /// <param name="searchTraits">A vector of trait names to get values for</param>
        /// <param name="functionalGroupIndex">The functional group index to return trait values for</param>
        /// <returns>A vector of values of the specified traits for a specified functional group</returns>
        public string[] GetTraitValues(string[] searchTraits, int functionalGroupIndex)
        {
            string[] TraitNames = new string[searchTraits.Length];

            for (int nn = 0; nn < searchTraits.Length; nn++)
            {
                TraitNames[nn] = TraitLookupFromIndex[searchTraits[nn].ToLower()].GetValue(functionalGroupIndex).ToString();
            }

            return TraitNames;
        }



        /// <summary>
        /// Get the functional group indices that have specified values of specified traits
        /// </summary>
        /// <param name="searchTraits">Vector of trait names to search for</param>
        /// <param name="searchTraitValues">Vector of trait values to search for</param>
        /// <param name="intersection">Whether the intersection of the indices for the traits should be returned, otherwise return the union of the indices</param>
        /// <returns>A vector of functional group indices with the specified values of the specified traits</returns>
        public int[] GetFunctionalGroupIndex(string[] searchTraits, string[] searchTraitValues, Boolean intersection)
        {
            // Check that the numbers of traits and of trait values specified are equal
            Debug.Assert((searchTraits.Length == searchTraitValues.Length), "Unequal search string arrays");

            // List to hold the functional group indices for each trait-trait value pair
            List<int[]> IndexList = new List<int[]>();
            
            int[] TempIndexList;
            
            //Sorted dictionary to hold the trait value index list sorted dictionary from the lookup table
			SortedDictionary<string, int[]> TraitIndexList;

            //Loop over the number of trait name and trait value pairs
            for (int nn = 0; nn < searchTraits.Length; nn++)
			{ 
                //Check if the trait name is in the lookup table and if so pull out the <trait value, index vector> sorted dictionary for it
                if (IndexLookupFromTrait.TryGetValue(searchTraits[nn].ToLower(), out TraitIndexList))
                {
                    //Check if the trait value string is found in the lookup table and if found pull out the index vector for it
                    //and add it to the List of these for processing - intersection of union
                    if (TraitIndexList.TryGetValue(searchTraitValues[nn], out TempIndexList))
                    {
                        IndexList.Add(TempIndexList);
                    }
                    //If trait value string not found then show error message
                    else
                    {
                        Debug.Fail("Trait Value to search for not found in lookup tables");
                    }
                }
                //If trait name string not found then show error message
                else
                {
                    Debug.Fail("Trait to search for not found in lookup tables");
                }
			}

            //If we are only searching for one traitname and trait value pair then return the index vector
            if (searchTraits.Length == 1)
            {
                return IndexList[0];
            }
            //Otherwise process the List of index vectors
            else
            {
                //Object to hold the array of index values found by the intersection method
                IEnumerable<int> ReturnList;
                //If intersection true then find the index values common to all traitname and trait value pairs
                if (intersection)
                {
                    ReturnList = IndexList[0].Intersect(IndexList[1]);

                    for (int nn = 2; nn < IndexList.Count; nn++)
                    {
                        ReturnList = ReturnList.Intersect(IndexList[nn]);
                    }
                }
                //If intersection false then return all the index values found above
                else
                {
                    ReturnList = IndexList[0].Union(IndexList[1]);

                    for (int nn = 2; nn < IndexList.Count; nn++)
                    {
                        ReturnList = ReturnList.Union(IndexList[nn]);
                    }

                }
                return ReturnList.ToArray();
            }
        }

        /// <summary>
        /// Function to return the integer index values for functional groups corresponding to given trait and trait value pair combinations.
        /// Overloaded to accept a single string rather than an array in the traits to search and the trait values - both must be single strings
        /// </summary>
        /// <param name="searchTraits">String of Trait names to search for trait values within</param>
        /// <param name="searchTraitValues">String of string Trait Values to find functional group indices for</param>
        /// <param name="intersection">Boolean statement indicating if you want the intersection of the indices. Only valid if more than one Trait and Trait Value pair.
        /// True means give intersection. False means give the union of indices</param>
        /// <returns>Int array containing functional group indices corresponding to the given search conditions</returns>
        public int[] GetFunctionalGroupIndex(string searchTraits, string searchTraitValues, Boolean intersection)
        {
            
            //List to hold the index vectors for each trait trait value pair
            int[] IndexList;

            //Sorted dictionary to hold the trait value index list sorted dictionary from the lookup table
            SortedDictionary<string, int[]> TraitIndexList;

            //Check if the trait name is in the lookup table and if so pull out the <trait value, index vector> sorted dictionary for it
            if (IndexLookupFromTrait.TryGetValue(searchTraits.ToLower(), out TraitIndexList))
            {
                //Check if the trait value string is found in the lookup table and if found pull out the index vector for it
                //and add it to the List of these for processing - intersection of union
                if (TraitIndexList.TryGetValue(searchTraitValues.ToLower(), out IndexList))
                {
                    ;
                }
                //If trait value string not found then show error message
                else
                {
                    IndexList = null;
                    Debug.Print("Trait Value to search for not found in lookup tables");
                }
            }
            //If trait name string not found then show error message
            else
            {
                IndexList = null;
                Debug.Print("Trait to search for not found in lookup tables");
            }

            return IndexList;
        }

        /// <summary>
        /// Returns number of functional groups
        /// </summary>
        /// <returns>Number of functional groups</returns>
        public int GetNumberOfFunctionalGroups()
        {
            return (_AllFunctionalGroupsIndex.Length);
        }
    }
}
