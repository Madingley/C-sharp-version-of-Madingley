using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using System.Diagnostics;


namespace Madingley
{
    /// <summary>
    /// Handles the cohorts in a grid cell
    /// </summary>
    /// <todoD>NOTE TO DT: CAN USE COLLECTIONBASE SYNTAX TO ELIMINATE A LOT OF CODE HERE</todoD>
    /// <todo>Create a wrapper class to handle our array of lists of gridCellCohorts within an individual grid cell</todo>
    public class GridCellCohortHandler : IList<List<Cohort>>, IEnumerable<List<Cohort>>
    {
        /// <summary>
        /// A list of cohorts in the grid cell
        /// </summary>
        private List<Cohort> [] GridCellCohorts;

        /// <summary>
        /// Create a new list of cohorts for the grid cell
        /// </summary>
        public GridCellCohortHandler()
        {
            GridCellCohorts = new List<Cohort>[0];
        }

        /// <summary>
        /// Create a new list of cohorts of specified length corresponding to the number of functional groups
        /// </summary>
        /// <param name="NumFunctionalGroups">The number of functional groups for which there will be cohorts in this grid cell</param>
        public GridCellCohortHandler(int NumFunctionalGroups)
        {
            GridCellCohorts = new List<Cohort>[NumFunctionalGroups];
        }

        /// <summary>
        /// Update grid cell cohorts with a specified list of cohorts
        /// </summary>
        /// <param name="ExistingCohorts">A list of cohorts to update the grid cell cohorts with</param>
        public GridCellCohortHandler(List<Cohort>[] ExistingCohorts)
        {
            GridCellCohorts = ExistingCohorts;
        }

        /// <summary>
        /// Get or set the list of cohorts for a specified functional group index
        /// </summary>
        /// <param name="functionalGroupIndex">The index of the functional group to get or set the list of cohorts for</param>
        /// <returns>The list of cohorts in the specified functional group</returns>
        public List<Cohort> this[int functionalGroupIndex]
        {
            get { return GridCellCohorts[functionalGroupIndex]; }
            set
            {
                GridCellCohorts[functionalGroupIndex] = value;
            }
        }

        /// <summary>
        /// Gets or sets a particular cohort within the grid cell cohorts
        /// </summary>
        /// <param name="index">A vector of two values corresponding to the functional group index and the index of the desired cohort within this functional group</param>
        /// <returns>The specified cohort</returns>
        public Cohort this[int[] index]
        {
            get { return GridCellCohorts[index[0]][index[1]]; }
            set { GridCellCohorts[index[0]][index[1]] = value; }
        }

         // Gets of sets a cohort within the array of lists of gridCellCohorts where the first element of the 2-element vector passed in is the array index and the second element is the list index
        /// <summary>
        /// Gets or sets a particular cohort within the grid cell cohorts
        /// </summary>
        /// <param name="functionalGroupIndex">The functional group index of the desired cohort</param>
        /// <param name="cohortIndex">The index of the cohort within the specified functional group</param>
        /// <returns>The specified cohort</returns>
        public Cohort this[int functionalGroupIndex, int cohortIndex]
        {
            get { return GridCellCohorts[functionalGroupIndex][cohortIndex]; }
            set
            {
                if (GridCellCohorts[functionalGroupIndex] == null) GridCellCohorts[functionalGroupIndex] = new List<Cohort>();
                GridCellCohorts[functionalGroupIndex].Add(value);
            }
        }

        
        /// <summary>
        /// Get the functional group index a specified cohort
        /// </summary>
        /// <param name="cohort">The cohort to return the functional group index for</param>
        /// <returns>The functional group index of the specified cohort</returns>
        public int IndexOf(List<Cohort> cohort)
        {
            return ((IList<List<Cohort>>)GridCellCohorts).IndexOf(cohort);
        }

        /// <summary>
        /// Inserts a new list of cohorts at a specified functional group index - CURRENTLY  NOT SUPPORTED
        /// </summary>
        /// <param name="index">The index in the list of functional groups to insert the list of cohorts in</param>
        /// <param name="listOfCohorts">The list of cohorts to insert</param>
        public void Insert(int index, List<Cohort> listOfCohorts)
        {
            Debug.Fail("The model does not currently support the addition of functional groups");
            ((IList<List<Cohort>>)GridCellCohorts).Insert(index, listOfCohorts);
        }

        /// <summary>
        /// Removes a list of cohorts in a specified functional group - CURRENTLY NOT SUPPORTED
        /// </summary>
        /// <param name="functionalGroupIndex">The index of the functional group to remove the list of cohorts for</param>
        public void RemoveAt(int functionalGroupIndex)
        {
            Debug.Fail("The model does not currently support the removal of functional groups");
            ((IList<List<Cohort>>)GridCellCohorts).RemoveAt(functionalGroupIndex);
        }

        /// <summary>
        /// Adds a list of cohorts at the end of the functional group indices - CURRENTLY NOT SUPPORTED
        /// </summary>
        /// <param name="listOfCohorts">The list of cohorts to add</param>
        public void Add(List<Cohort> listOfCohorts)
        {
            Debug.Fail("The model does not currently support the addition of functional groups");
            ((IList<List<Cohort>>)GridCellCohorts).Add(listOfCohorts);
        }

        /// <summary>
        /// Currently not implemented
        /// </summary>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Currently not implemented
        /// </summary>
        /// <param name="item">NA</param>
        /// <returns>NA</returns>
        public bool Contains(List<Cohort> item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Currently not implemented
        /// </summary>
        /// <param name="array">NA</param>
        /// <param name="arrayIndex">NA</param>
        public void CopyTo(List<Cohort>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the number of functional groups in the grid cell cohorts
        /// </summary>
        public int Count
        {
            get { return GridCellCohorts.Count(); }
        }

        /// <summary>
        /// Currently not implemented
        /// </summary>
        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Currently not implemented
        /// </summary>
        /// <param name="item">NA</param>
        /// <returns>NA</returns>
        public bool Remove(List<Cohort> item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an the grid cell cohorts as an IEnumerator 
        /// </summary>
        /// <returns>The grid cell cohorts as an IEnumerator</returns>
        public IEnumerator<List<Cohort>> GetEnumerator()
        {
            return new GridCellCohortsEnum(GridCellCohorts);
        }

        /// <summary>
        /// Return an IEnumerable as an IEnumerator
        /// </summary>
        /// <returns>The IEnumerable as an IEnumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        /// <summary>
        /// Gets the number of cohorts in this grid cell
        /// </summary>
        public int GetNumberOfCohorts()
        {

            int sum = 0;
            for (int ii = 0; ii < GridCellCohorts.Count(); ii++)
            {
                sum += GridCellCohorts[ii].Count();
            }

            return sum;
        }

    }

    /// <summary>
    /// IEnumerator for the grid cell cohorts
    /// </summary>
    public class GridCellCohortsEnum : IEnumerator<List<Cohort>>
    {
        /// <summary>
        /// The grid cell cohorts as a vector (with elements corresponding to functional groups) of lists of cohorts
        /// </summary>
        public List<Cohort>[] GridCellCohorts;

        /// <summary>
        /// Current position in the vector of lists of cohorts
        /// </summary>
        int position = -1;

        /// <summary>
        /// Assign the passed set of grid cell cohorts to the internal vector of lists of cohorts 
        /// </summary>
        /// <param name="list"></param>
        public GridCellCohortsEnum(List<Cohort>[] list)
        {
            GridCellCohorts = list;
        }

        /// <summary>
        /// Move to the next element in the vector of lists of cohorts
        /// </summary>
        /// <returns>True if the end of the list had not been reached</returns>
        public bool MoveNext()
        {
            position++;
            return (position <  GridCellCohorts.Length);
        }

        /// <summary>
        /// Move back to the first element in the vector of lists of cohorts
        /// </summary>
        public void Reset()
        {
            position = -1;
        }

        /// <summary>
        /// Returns the list of cohorts for the current position (i.e. functional group) in the vector of lists of cohorts
        /// </summary>
        object System.Collections.IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        /// <summary>
        /// Get the list of cohorts for the current position (i.e. functional group) in the vector of lists of cohorts
        /// </summary>
        public List<Cohort> Current
        {
            get
            {
                try
                {
                    return GridCellCohorts[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        /// <summary>
        /// Destructor for the grid cell cohorts enumerator
        /// </summary>
        public void Dispose()
        {
        }

    }

}
