using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

namespace Madingley
{
    /// <summary>
    /// Methods to convert between SDS objects and values, vectors and arrays
    /// <todoT>1. Write code to make sure that the dimensions and the grid sizes correspond</todoT>
    /// <todo>2. Write debug assertions for as many as evantualities as can be thought of</todo>
    /// <todo>3. Decide whether to exit with error if variable already exists</todo>
    /// <todo>4. IMPORTANT - dimensions values need to refer to cell centres. Need to think through implications of this</todo>
    /// </summary>
    public class ArraySDSConvert
    {
        /// <summary>
        /// Adds a one-dimensional variable to the specified SDS object with floating point dimension data
        /// </summary>
        /// <param name="SDSObject">A reference to an SDS object</param>
        /// <param name="variableName">The name of the variable to create</param>
        /// <param name="units">Units of the data</param>
        /// <param name="numDimensions">The number of dimensions for the new variable</param>
        /// <param name="namesDimensions">A vector of names of the dimensions for the variable</param>
        /// <param name="missingValue">The missing value to apply to the new variable</param>
        /// <param name="dimension1Data">A vector of values of the first dimension</param>
        public void AddVariable(DataSet SDSObject, string variableName, string units, int numDimensions, string[] namesDimensions, double missingValue, float[] dimension1Data)
        {
            //If the wrong number of dimension names have been provided, then return error
            if (namesDimensions.Length != numDimensions)
                Debug.Fail("Error: you have provided the wrong number of dimension names");
            
            //Since this overload method deals with one-dimensional variables, return an error if this is not the case
            if (numDimensions != 1)
                Debug.Fail("Error: you have provided data for the wrong number of dimensions");

            //If the variable already exists in the SDS, then take no action, otherwise create new variable
            if (SDSObject.Variables.Contains(variableName))
            {
                Console.WriteLine("SDS object already contains a variable with that name. Skipping...");
            }
            else
            {
                //For each dimension, if it already exists in the SDS then take no action, otherwise create a new variable and populate it with the provided data
                if (SDSObject.Variables.Contains(namesDimensions[0]))
                {
                }
                else
                {
                    SDSObject.AddVariable<float>(namesDimensions[0], dimension1Data, namesDimensions[0]);
                }
                
                //If the variable is the same as one of the entered dimensions, then take no action, otherwise create the new variable and populate it with missing values
                if (SDSObject.Variables.Contains(variableName))
                {
                }
                else
                {
                    //Create array of missing values of the correct dimensions
                    double[] tempOutData = new double[dimension1Data.Length];

                    for (int ii = 0; ii < dimension1Data.Length; ii++)
                    {
                        tempOutData[ii] = missingValue;
                    }


                    //Add variable to SDS
                    var testOut = SDSObject.Add<double[]>(variableName, units, missingValue, tempOutData, namesDimensions);

                    //Metadata required by SDS
                    testOut.Metadata["DisplayName"] = variableName;
                    testOut.Metadata["MissingValue"] = missingValue;
                    
                    //Commit changes
                    SDSObject.Commit();

                }
            }

        }

        /// <summary>
        /// Adds a one-dimensional variable to the specified SDS object with string dimension data
        /// </summary>
        /// <param name="SDSObject">A reference to an SDS object</param>
        /// <param name="variableName">The name of the variable to create</param>
        /// <param name="numDimensions">The number of dimensions for the new variable</param>
        /// <param name="namesDimensions">A vector of names of the dimensions for the variable</param>
        /// <param name="missingValue">The missing value to apply to the new variable</param>
        /// <param name="dimension1Data">A string vector of values of the first dimension</param>
        public void AddVariable(DataSet SDSObject, string variableName, int numDimensions, string[] namesDimensions, double missingValue, string[] dimension1Data)
        {
            //If the wrong number of dimension names have been provided, then return error
            if (namesDimensions.Length != numDimensions)
                Debug.Fail("Error: you have provided the wrong number of dimension names");
            
            //Since this overload method deals with one-dimensional variables, return an error if this is not the case
            if (numDimensions != 1)
                Debug.Fail("Error: you have provided data for the wrong number of dimensions");

            //If the variable already exists in the SDS, then return, otherwise create new variable
            if (SDSObject.Variables.Contains(variableName))
            {
                Console.WriteLine("SDS object already contains a variable with that name. Skipping...");
            }
            else
            {
                //For each dimension, if it already exists in the SDS then no action, otherwise create a new variable and populate it with the provided data
                if (SDSObject.Variables.Contains(namesDimensions[0]))
                {
                }
                else
                {
                    SDSObject.AddVariable<string>(namesDimensions[0], dimension1Data, namesDimensions[0]);
                }

                //If the variable is the same as one of the entered dimensions, then take no action, otherwise create the new variable and populate it with missing values
                if (SDSObject.Variables.Contains(variableName))
                {
                }
                else
                {
                    //Create array of missing values of the correct dimensions
                    double[] tempOutData = new double[dimension1Data.Length];

                    for (int ii = 0; ii < dimension1Data.Length; ii++)
                    {
                        tempOutData[ii] = missingValue;
                    }


                    //Add variable to SDS
                    var testOut = SDSObject.AddVariable<double>(variableName, tempOutData, namesDimensions);

                    //Metadata required by SDS
                    testOut.Metadata["DisplayName"] = variableName;
                    testOut.Metadata["MissingValue"] = missingValue;

                    //Commit changes
                    SDSObject.Commit();

                }
            }

        }


        /// <summary>
        /// Adds a two-dimensional variable to the specified SDS object with floating point data for both dimensions
        /// </summary>
        /// <param name="SDSObject">A reference to an SDS object</param>
        /// <param name="variableName">The name of the variable to create</param>
        /// <param name="numDimensions">The number of dimensions for the new variable</param>
        /// <param name="namesDimensions">A vector of names of the dimensions for the variable</param>
        /// <param name="missingValue">The missing value to apply to the new variable</param>
        /// <param name="dimension1Data">A vector of values of the first dimension</param>
        /// <param name="dimension2Data">A vector of values of the second dimension</param>
        public void AddVariable(DataSet SDSObject, string variableName, int numDimensions, string[] namesDimensions, double missingValue, float[] dimension1Data, float[] dimension2Data)
        {
            //If the wrong number of dimension names have been provided, then return error
            if (namesDimensions.Length != numDimensions)
                Debug.Fail("Error: you have provided the wrong number of dimension names");
            
            //Since this overload method deals with two-dimensional variables, return an error if this is not the case
            if (numDimensions != 2)
                Debug.Fail("Error: you have provided data for the wrong number of dimensions");

            //If the variable already exists in the SDS, then return, otherwise create new variable
            if (SDSObject.Variables.Contains(variableName))
            {
                Console.WriteLine("SDS object already contains a variable with that name. Skipping...");
            }
            else
            {
                //For each dimension, if it already exists in the SDS then no action, otherwise create a new variable and populate it with the provided data
                if (SDSObject.Variables.Contains(namesDimensions[0]))
                {
                }
                else
                {
                    SDSObject.AddVariable<float>(namesDimensions[0], dimension1Data, namesDimensions[0]);
                }
                if (SDSObject.Variables.Contains(namesDimensions[1]))
                {
                }
                else
                {
                    SDSObject.AddVariable<float>(namesDimensions[1], dimension2Data, namesDimensions[1]);
                }

                //If the variable is the same as one of the entered dimensions, then take no action, otherwise create the new variable and populate it with missing values
                if (SDSObject.Variables.Contains(variableName))
                {
                }
                else
                {
                    //Create array of missing values of the correct dimensions
                    double[,] tempOutData = new double[dimension1Data.Length,dimension2Data.Length];

                    for (int ii = 0; ii < dimension1Data.Length; ii++)
                    {
                        for (int jj = 0; jj < dimension2Data.Length; jj++)
                        {
                            tempOutData[ii,jj] = missingValue;
                        }
                    }


                    //Add variable to SDS
                    var testOut = SDSObject.AddVariable<double>(variableName, tempOutData, namesDimensions);

                    //Metadata required by SDS
                    testOut.Metadata["DisplayName"] = variableName;
                    testOut.Metadata["MissingValue"] = missingValue;

                    //Commit changes
                    SDSObject.Commit();

                }
            }

        }

        /// <summary>
        /// Adds a three-dimensional variable to the specified SDS object with floating point data for all three dimensions
        /// </summary>
        /// <param name="SDSObject">A reference to an SDS object</param>
        /// <param name="variableName">The name of the variable to create</param>
        /// <param name="numDimensions">The number of dimensions for the new variable</param>
        /// <param name="namesDimensions">A vector of names of the dimensions for the variable</param>
        /// <param name="missingValue">The missing value to apply to the new variable</param>
        /// <param name="dimension1Data">A vector of values of the first dimension</param>
        /// <param name="dimension2Data">A vector of values of the second dimension</param>
        /// <param name="dimension3Data">A vector of values of the third dimension</param>
        public void AddVariable(DataSet SDSObject, string variableName, int numDimensions, string[] namesDimensions, double missingValue, float[] dimension1Data, float[] dimension2Data, float[] dimension3Data)
        {
            //If the wrong number of dimension names have been provided, then return error
            if (namesDimensions.Length != numDimensions)
                Debug.Fail("Error: you have provided the wrong number of dimension names");
            
            //Since this overload method deals with three-dimensional variables, return an error if this is not the case
            if (numDimensions != 3)
                Debug.Fail("Error: you have provided data for the wrong number of dimensions");

            //If the variable already exists in the SDS, then return, otherwise create new variable
            if (SDSObject.Variables.Contains(variableName))
            {
                Console.WriteLine("SDS object already contains a variable with that name. Skipping...");
            }
            else
            {
                //For each dimension, if it already exists in the SDS then no action, otherwise create a new variable and populate it with the provided data
                if (SDSObject.Variables.Contains(namesDimensions[0]))
                {
                }
                else
                {
                    SDSObject.AddVariable<float>(namesDimensions[0], dimension1Data, namesDimensions[0]);
                }
                if (SDSObject.Variables.Contains(namesDimensions[1]))
                {
                }
                else
                {
                    SDSObject.AddVariable<float>(namesDimensions[1], dimension2Data, namesDimensions[1]);
                }
                if (SDSObject.Variables.Contains(namesDimensions[2]))
                {
                }
                else
                {
                    SDSObject.AddVariable<float>(namesDimensions[2], dimension3Data, namesDimensions[2]);
                }

                //If the variable is the same as one of the entered dimensions, then take no action, otherwise create the new variable and populate it with missing values
                if (SDSObject.Variables.Contains(variableName))
                {
                }
                else
                {
                    //Create array of missing values of the correct dimensions
                    double[, ,] tempOutData = new double[dimension1Data.Length, dimension2Data.Length, dimension3Data.Length];

                    for (int ii = 0; ii < dimension1Data.Length; ii++)
                    {
                        for (int jj = 0; jj < dimension2Data.Length; jj++)
                        {
                            for (int kk = 0; kk < dimension3Data.Length; kk++)
                            {
                                tempOutData[ii, jj, kk] = missingValue;
                            }
                        }
                    }


                    //Add variable to SDS
                    var testOut = SDSObject.AddVariable<double>(variableName, tempOutData, namesDimensions);

                    //Metadata required by SDS
                    testOut.Metadata["DisplayName"] = variableName;
                    testOut.Metadata["MissingValue"] = missingValue;


                    //Commit changes
                    SDSObject.Commit();

                }
            }

        }

        /// <summary>
        /// Adds a four-dimensional variable to the specified SDS object with floating point data for all four dimensions
        /// </summary>
        /// <param name="SDSObject">A reference to an SDS object</param>
        /// <param name="variableName">The name of the variable to create</param>
        /// <param name="numDimensions">The number of dimensions for the new variable</param>
        /// <param name="namesDimensions">A vector of names of the dimensions for the variable</param>
        /// <param name="missingValue">The missing value to apply to the new variable</param>
        /// <param name="dimension1Data">A vector of values for the first dimension</param>
        /// <param name="dimension2Data">A vector of values for the second dimension</param>
        /// <param name="dimension3Data">A vector of values for the third dimension</param>
        /// <param name="dimension4Data">A vector of values for the fourth dimension</param>
        public void AddVariable(DataSet SDSObject, string variableName, int numDimensions, string[] namesDimensions, double missingValue, float[] dimension1Data, float[] dimension2Data, float[] dimension3Data, float[] dimension4Data)
        {
            //If the wrong number of dimension names have been provided, then return error
            if (namesDimensions.Length != numDimensions)
                Debug.Fail("Error: you have provided the wrong number of dimension names");

            //Since this overload method deals with three-dimensional variables, return an error if this is not the case
            if (numDimensions != 4)
                Debug.Fail("Error: you have provided data for the wrong number of dimensions");

            //If the variable already exists in the SDS, then return, otherwise create new variable
            if (SDSObject.Variables.Contains(variableName))
            {
                Console.WriteLine("SDS object already contains a variable with that name. Skipping...");
            }
            else
            {
                //For each dimension, if it already exists in the SDS then no action, otherwise create a new variable and populate it with the provided data
                if (SDSObject.Variables.Contains(namesDimensions[0]))
                {
                }
                else
                {
                    SDSObject.AddVariable<float>(namesDimensions[0], dimension1Data, namesDimensions[0]);
                }
                if (SDSObject.Variables.Contains(namesDimensions[1]))
                {
                }
                else
                {
                    SDSObject.AddVariable<float>(namesDimensions[1], dimension2Data, namesDimensions[1]);
                }
                if (SDSObject.Variables.Contains(namesDimensions[2]))
                {
                }
                else
                {
                    SDSObject.AddVariable<float>(namesDimensions[2], dimension3Data, namesDimensions[2]);
                }
                if (SDSObject.Variables.Contains(namesDimensions[3]))
                {
                }
                else
                {
                    SDSObject.AddVariable<float>(namesDimensions[3], dimension4Data, namesDimensions[3]);
                }

                //If the variable is the same as one of the entered dimensions, then take no action, otherwise create the new variable and populate it with missing values
                if (SDSObject.Variables.Contains(variableName))
                {
                }
                else
                {
                    //Create array of missing values of the correct dimensions
                    double[, , ,] tempOutData = new double[dimension1Data.Length, dimension2Data.Length, dimension3Data.Length, dimension4Data.Length];

                    for (int ii = 0; ii < dimension1Data.Length; ii++)
                    {
                        for (int jj = 0; jj < dimension2Data.Length; jj++)
                        {
                            for (int kk = 0; kk < dimension3Data.Length; kk++)
                            {
                                for (int ll = 0; ll < dimension4Data.Length; ll++)
                                {
                                    tempOutData[ii, jj, kk, ll] = missingValue;
                                }
                            }
                        }
                    }


                    //Add variable to SDS
                    var testOut = SDSObject.AddVariable<double>(variableName, tempOutData, namesDimensions);

                    //Metadata required by SDS
                    testOut.Metadata["DisplayName"] = variableName;
                    testOut.Metadata["MissingValue"] = missingValue;


                    //Commit changes
                    SDSObject.Commit();

                }
            }

        }


        /// <summary>
        /// Adds a double value to an one-dimensional variable in an SDS object at the specified offset in the dimension
        /// </summary>
        /// <param name="dataToConvert">The value to add to the SDS object</param>
        /// <param name="outputVariableName">The name of the variable to add the data to</param>
        /// <param name="dimensionName">The name of the dimension variable of the output variable</param>
        /// <param name="missingValue">The value used for missing data</param>
        /// <param name="SDSObject">The SDS object to write to</param>
        /// <param name="dimensionOffset">The required offset in the dimension variable</param>
        public void ValueToSDS1D(double dataToConvert, string outputVariableName, string dimensionName, double missingValue, 
            DataSet SDSObject, int dimensionOffset)
        {
            // Check that the dimension variables and the output variable have been defined already
            Debug.Assert(SDSObject.Variables.Contains(dimensionName), 
                "Error: where an offset is included, dimension information must be defined before adding data");
            Debug.Assert(SDSObject.Variables.Contains(outputVariableName), 
                "Error: where an offset is included, the variable must be created before adding data");

            // Add the data to the SDS object
            SDSObject.PutData<double>(outputVariableName, dataToConvert, DataSet.ReduceDim(dimensionOffset));
            
            // Commit the SDS object
            SDSObject.Commit();

        }

        /// <summary>
        /// Adds a double value to a two-dimensional variable in an SDS object at the specified offsets in both dimensions
        /// </summary>
        /// <param name="dataToConvert">The value to add to the SDS object</param>
        /// <param name="outputVariableName">The name of the variable to add the data to</param>
        /// <param name="dimensionNames">A vector containing the names of the dimensions of the output variable</param>
        /// <param name="missingValue">The value to be used for missing data</param>
        /// <param name="SDSObject">The SDS object to write to</param>
        /// <param name="dimension1Offset">The required offset in the first dimension</param>
        /// <param name="dimension2Offset">The required offset in the second dimension</param>
        public void ValueToSDS2D(double dataToConvert, string outputVariableName, string[] dimensionNames, double missingValue, DataSet SDSObject, int dimension1Offset, int dimension2Offset)
        {
            // Check that the length of the vector of dimension names equals the number of dimensions
            Debug.Assert(dimensionNames.Length == 2, "There should be two dimension names passed to this method");

            // Check that the dimension information has been defined already
            foreach (string dimensionName in dimensionNames)
            {
                Debug.Assert(SDSObject.Variables.Contains(dimensionName), "Error: where an offset is included, dimension information must be defined before adding data");
            }

            // Check that the output variable has been defined already
            Debug.Assert(SDSObject.Variables.Contains(outputVariableName), "Error: where an offset is included, the variable must be created before adding data");

            // Add the data to the SDS object
            SDSObject.PutData<double>(outputVariableName, dataToConvert, DataSet.ReduceDim(dimension1Offset), DataSet.ReduceDim(dimension2Offset));

            // Commit the changes
            SDSObject.Commit();

        }

        /// <summary>
        /// Adds a double value to a three-dimensional variable in an SDS object at the specified offsets in all three dimensions
        /// </summary>
        /// <param name="dataToConvert">The value to add to the SDS object</param>
        /// <param name="outputVariableName">The name of the variable to add the data to</param>
        /// <param name="dimensionNames">A vector containing the names of the dimensions of the output variable</param>
        /// <param name="missingValue">The value to be used for missing data</param>
        /// <param name="SDSObject">The SDS object to write to</param>
        /// <param name="dimension1Offset">The required offset in the first dimension</param>
        /// <param name="dimension2Offset">The required offset in the second dimension</param>
        /// <param name="dimension3Offset">The required offset in the third dimension</param>
        public void ValueToSDS3D(double dataToConvert, string outputVariableName, string[] dimensionNames, double missingValue, DataSet SDSObject, int dimension1Offset, int dimension2Offset, int dimension3Offset)
        {
            // Check that the length of the vector of dimension names equals the number of dimensions
            Debug.Assert(dimensionNames.Length == 3, "There should be three dimension names passed to this method");

            // Check that the dimension information has been defined already
            foreach (string dimensionName in dimensionNames)
            {
                Debug.Assert(SDSObject.Variables.Contains(dimensionName), "Error: where an offset is included, dimension information must be defined before adding data");
            }

            // Check that the output variable has been defined already
            Debug.Assert(SDSObject.Variables.Contains(outputVariableName), "Error: where an offset is included, the variable must be created before adding data");

            // Add the data to the SDS object
            SDSObject.PutData<double>(outputVariableName, dataToConvert, DataSet.ReduceDim(dimension1Offset), DataSet.ReduceDim(dimension2Offset),DataSet.ReduceDim(dimension3Offset));

            // Commit the changes
            SDSObject.Commit();

        }

        /// <summary>
        /// Adds a vector of double values to a one-dimensional variable in an SDS object
        /// </summary>
        /// <param name="dataToConvert">The vector of values to add</param>
        /// <param name="outputVariableName">The name of the variable to write to</param>
        /// <param name="dimensionName">The name of the dimension variable of the output variable</param>
        /// <param name="dimensionValues">The values of the dimension variable</param>
        /// <param name="missingValue">The value used for missing data</param>
        /// <param name="SDSObject">The SDS object to write to</param>
        public void VectorToSDS1D(double[] dataToConvert, string outputVariableName, string dimensionName, float[] dimensionValues, double missingValue, DataSet SDSObject)
        {
            // If not already contained in the SDS object, add the dimension variable
            if (SDSObject.Variables.Contains(dimensionName))
            {
            }
            else
            {
                SDSObject.AddVariable<float>(dimensionName, dimensionValues, dimensionName);
            }

            // If not already contained in the SDS object, add the output variable
            if (SDSObject.Variables.Contains(outputVariableName))
            {
                SDSObject.PutData<double[]>(outputVariableName, dataToConvert);
                SDSObject.Commit();
            }
            else
            {
                // Set up the dimensions and add the gridded data
                string[] dimensions = { dimensionName };
                var DataGrid = SDSObject.AddVariable<double>(outputVariableName, dataToConvert, dimensions);

                // Add appropriate metadata (including missing values)
                DataGrid.Metadata["DisplayName"] = outputVariableName;
                DataGrid.Metadata["MissingValue"] = (double)missingValue;

                // Commit changes
                SDSObject.Commit();
            }

        }

        /// <summary>
        /// Adds a vector of values to a two-dimensional variable in an SDS object at the specified offset in the first dimension
        /// </summary>
        /// <param name="dataToConvert">The vector of values to add</param>
        /// <param name="outputVariableName">The name of the variable to write to</param>
        /// <param name="dimensionNames">A vector containing the names of the dimension variables</param>
        /// <param name="dimension1Data">The values of the first dimension variable</param>
        /// <param name="dimension2Data">The values of the second dimension</param>
        /// <param name="missingValue">The value used for missing data</param>
        /// <param name="SDSObject">The SDS object to write to</param>
        /// <param name="dimension1Offset">The required offset in the first dimension</param>
        public void VectorToSDS2D(double[] dataToConvert, string outputVariableName, string[] dimensionNames, float[] dimension1Data, float[] dimension2Data, double missingValue, DataSet SDSObject, int dimension1Offset)
        {
            // Check that the length of the vector of dimension names equals the number of dimensions
            Debug.Assert(dimensionNames.Length == 2, "There should be two dimension names passed to this method");

            // Check that the dimension information has been defined already
            foreach (string dimensionName in dimensionNames)
            {
                Debug.Assert(SDSObject.Variables.Contains(dimensionName), "Error: where an offset is included, dimension information must be defined before adding data");
            }
            
            // Check that the output variable has been defined already
            Debug.Assert(SDSObject.Variables.Contains(outputVariableName), "Error: where an offset is included, the variable must be created before adding data");

            // Add the data to the SDS object
            SDSObject.PutData<double[]>(outputVariableName, dataToConvert, DataSet.ReduceDim(dimension1Offset), DataSet.FromToEnd(0));
            
            // Commit the SDS object
            SDSObject.Commit();
           
        }

        /// <summary>
        /// Adds a geographical array of values to a two-dimensional variable in an SDS object
        /// </summary>
        /// <param name="dataToConvert">The array of values to add</param>
        /// <param name="ouputVariableName">The name of the variable to write to</param>
        /// <param name="lats">The values of the latitude dimension variable</param>
        /// <param name="lons">The values of the longitude dimension variable</param>
        /// <param name="missingValue">The value used for missing data</param>
        /// <param name="SDSObject">The SDS object to write to</param>
        public void Array2DToSDS2D(double[,] dataToConvert, string ouputVariableName, float[] lats, float[] lons, double missingValue, DataSet SDSObject)
        {
            // If not already contained in the SDS, add the dimension information (geographic coordinates)
            if (SDSObject.Variables.Contains("Latitude"))
            {
            }
            else
            {
                SDSObject.AddVariable<float>("Latitude", lats, "Lat");
            }
            if (SDSObject.Variables.Contains("Longitude"))
            {
            }
            else
            {
                SDSObject.AddVariable<float>("Longitude", lons, "Lon");
            }

            // If the SDS object contains the variable to write to, then simply add the data, otherwise add a new variable and then add the data
            if (SDSObject.Variables.Contains(ouputVariableName))
            {
                // Add the data
                SDSObject.PutData<double[,]>(ouputVariableName, dataToConvert);

                // Commit the changes
                SDSObject.Commit();

            }
            else
            {
                // Set up the dimensions and add the gridded data
                string[] dimensions = { "Lat", "Lon" };
                var DataGrid = SDSObject.AddVariable<double>(ouputVariableName, dataToConvert, dimensions);

                // Add appropriate metadata (including missing values)
                DataGrid.Metadata["DisplayName"] = ouputVariableName;
                DataGrid.Metadata["MissingValue"] = (double)missingValue;

                // Commit changes to update data set
                SDSObject.Commit();
            }
        }

        /// <summary>
        /// Extract an array of values from a state variable in a model grid and add to a two-dimensional variable in an SDS object
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid to extract data from</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="gridVariableName">The name of the state variable in the model grid</param>
        /// <param name="traitValue">The trait value of the functional groups to get data for</param>
        /// <param name="variableType">The type of the state variable: 'stock' or 'cohort'</param>
        /// <param name="outputVariableName">The name of the variable to write to</param>
        /// <param name="SDSObject">The SDS object to write to</param>
        /// <param name="functionalGroupHandler">The functional group handler corresponding to cohorts or stocks</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        public void Array2DToSDS2D(ModelGrid ecosystemModelGrid, List<uint[]> cellIndices, string gridVariableName, string traitValue, 
            string variableType, string outputVariableName, DataSet SDSObject, FunctionalGroupDefinitions functionalGroupHandler, 
            MadingleyModelInitialisation initialisation)
        {
            // Get the missing value from the model grid
            double MissingValue = ecosystemModelGrid.GlobalMissingValue;

            // create an array to hold the data to output
            double[,] dataToConvert = new double[ecosystemModelGrid.NumLatCells, ecosystemModelGrid.NumLonCells];

            // generate arrays to hold latitudes and longitudes
            float[] lats = new float[ecosystemModelGrid.NumLatCells];
            float[] lons = new float[ecosystemModelGrid.NumLonCells];
            
            // Populate arrays of latitudes and longitudes, converting from bottom left cell references as used in the model grid
            // to cell centre references as required by SDS
            for (uint ii = 0; ii < ecosystemModelGrid.NumLatCells; ii++)
            {
                lats[ii] = ecosystemModelGrid.Lats[ii] + (ecosystemModelGrid.LatCellSize / 2);
                
            }

            for (uint jj = 0; jj < ecosystemModelGrid.NumLonCells; jj++)
            {
                lons[jj] = ecosystemModelGrid.Lons[jj] + (ecosystemModelGrid.LonCellSize / 2);    
            }

            // Get the required data from the model grid
            dataToConvert = ecosystemModelGrid.GetStateVariableGrid(gridVariableName, traitValue, functionalGroupHandler.AllFunctionalGroupsIndex, cellIndices,
                variableType, initialisation);

            // If not already contained in the SDS, add the dimension information (geographic coordinates)
            if (SDSObject.Variables.Contains("Latitude"))
            {
            }
            else
            {
                SDSObject.AddVariable<float>("Latitude", lats, "Lat");
            }
            if (SDSObject.Variables.Contains("Longitude"))
            {
            }
            else
            {
                SDSObject.AddVariable<float>("Longitude", lons, "Lon");
            }

            // If the SDS object already contains the output variable, then add the data. Otherwise, define the variable and then add the data
            if (SDSObject.Variables.Contains(outputVariableName))
            {
                SDSObject.PutData<double[,]>(outputVariableName, dataToConvert);
                // Commit the changes
                SDSObject.Commit();
            }
            else
            {
                // Set up the dimensions and add the gridded data
                string[] dimensions = { "Lat", "Lon" };
                var DataGrid = SDSObject.AddVariable<double>(outputVariableName, dataToConvert, dimensions);

                // Add appropriate metadata (including missing values)
                DataGrid.Metadata["DisplayName"] = outputVariableName;
                DataGrid.Metadata["MissingValue"] = (double)MissingValue;

                // Commit changes to update data set
                SDSObject.Commit();
            }

           

        }

        /// <summary>
        /// Outputs a two-dimensional array to a three-dimensional variable in an SDS object, with specified offset for the third dimension
        /// </summary>
        /// <param name="dataToConvert">An array of values to output</param>
        /// <param name="newVariableName">The name of the variable to be created or written to</param>
        /// <param name="dimensionNames">A vector containing the names of the dimensions of the output variable</param>
        /// <param name="thirdDimensionOffset">The offset to be applied in the third dimension</param>
        /// <param name="missingValue">The missing value to be used</param>
        /// <param name="SDSObject">A reference to an SDS object</param>
        public void Array2DToSDS3D(double[,] dataToConvert, string newVariableName, string[] dimensionNames, int thirdDimensionOffset, double missingValue, DataSet SDSObject)
        {
            // Check that the length of the vector of dimension names equals the number of dimensions
            Debug.Assert(dimensionNames.Length == 3, "There should be three dimension names passed to this method");

            // Check that the dimension information has been defined already
            foreach (string dimensionName in dimensionNames)
            {
                Debug.Assert(SDSObject.Variables.Contains(dimensionName), "Error: where an offset is included, dimension information must be defined before adding data");
            }

            Debug.Assert(SDSObject.Variables.Contains(newVariableName), "Error: where an offset is included, target variable must be created before adding data");

            SDSObject.PutData<double[,]>(newVariableName, dataToConvert, DataSet.FromToEnd(0), DataSet.FromToEnd(0), DataSet.ReduceDim(thirdDimensionOffset));

            SDSObject.Commit();


        }
    }
}
