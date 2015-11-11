using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;


namespace Madingley
{
    /// <summary>
    /// Handles the stocks in a grid cell
    /// </summary>
    /// <todoD>CAN USE COLLECTIONBASE SYNTAX TO ELIMINATE A LOT OF CODE HERE</todoD>
    /// <todo>Create a wrapper class to handle our array of lists of gridCellStocks within an individual grid cell</todo>
    public class GridCellStockHandler : IList<List<Stock>>, IEnumerable<List<Stock>>
    {
        /// <summary>
        /// A vector (with elements correpsonding to functional groups) of lists of stocks in the current grid cell
        /// </summary>
        private List<Stock>[] GridCellStocks;

        /// <summary>
        /// Overloaded constructor for the grid cell stock handler: initialises a new vector of lists of stocks
        /// </summary>
        public GridCellStockHandler()
        {
            GridCellStocks = new List<Stock>[0];
        }

        /// <summary>
        /// Overloaded constructor for the grid cell stock handler: initialises a new vector of lists of stocks with number of elements equal to the number of functional groups 
        /// </summary>
        /// <param name="NumFunctionalGroups">The number of stock functional groups in the model</param>
        public GridCellStockHandler(int NumFunctionalGroups)
        {
            GridCellStocks = new List<Stock>[NumFunctionalGroups];
        }

        /// <summary>
        /// Overloaded constructor for the grid cell stock handler: update the grid cell stocks with the a set of existing stocks
        /// </summary>
        /// <param name="ExistingStocks"></param>
        public GridCellStockHandler(List<Stock>[] ExistingStocks)
        {
            GridCellStocks = ExistingStocks;
        }

        /// <summary>
        /// Get or set the list of stocks for a specified functional group index
        /// </summary>
        /// <param name="index">The functional group index</param>
        /// <returns>The list of stocks from the specified functional group index</returns>
        public List<Stock> this[int index]
        {
            get { return GridCellStocks[index]; }
            set
            {
                GridCellStocks[index] = value;
            }
        }

        // Gets of sets a Stock within the array of lists of gridCellStocks where the first element of the 2-element vector passed in is the array index and the second element is the list index
        /// <summary>
        /// Get or set the stock at a specified position within a specified functional group index
        /// </summary>
        /// <param name="index">Pair of values corresponding to the functional group index and the position of the stock within this functional group</param>
        /// <returns>The stock at the specified position</returns>
        public Stock this[int[] index]
        {
            get { return GridCellStocks[index[0]][index[1]]; }
            set { GridCellStocks[index[0]][index[1]] = value; }
        }

        /// <summary>
        /// Get the functional group index of the passed list of stocks
        /// </summary>
        /// <param name="item">The list of stocks to get the functional group index for</param>
        /// <returns>The functional group index of the passed list of stocks</returns>
        public int IndexOf(List<Stock> item)
        {
            return ((IList<List<Stock>>)GridCellStocks).IndexOf(item);
        }

        /// <summary>
        /// NOT CURRENTLY USED
        /// </summary>
        /// <param name="index">NOT CURRENTLY USED</param>
        /// <param name="item">NOT CURRENTLY USED</param>
        public void Insert(int index, List<Stock> item)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// NOT CURRENTLY USED
        /// </summary>
        /// <param name="index">NOT CURRENTLY USED</param>
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NOT CURRENTLY USED
        /// </summary>
        /// <param name="item">NOT CURRENTLY USED</param>
        public void Add(List<Stock> item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NOT CURRENTLY USED
        /// </summary>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NOT CURRENTLY USED
        /// </summary>
        /// <param name="item">NOT CURRENTLY USED</param>
        /// <returns>NOT CURRENTLY USED</returns>
        public bool Contains(List<Stock> item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NOT CURRENTLY USED
        /// </summary>
        /// <param name="array">NOT CURRENTLY USED</param>
        /// <param name="arrayIndex">NOT CURRENTLY USED</param>
        public void CopyTo(List<Stock>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the number of stock functional groups
        /// </summary>
        public int Count
        {
            get { return GridCellStocks.Count(); }
        }

        /// <summary>
        /// NOT CURRENTLY USED
        /// </summary>
        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// NOT CURRENTLY USED
        /// </summary>
        /// <param name="item">NOT CURRENTLY USED</param>
        /// <returns>NOT CURRENTLY USED</returns>
        public bool Remove(List<Stock> item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return the grid cell stocks as an IEnumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<List<Stock>> GetEnumerator()
        {
            return new GridCellStocksEnum(GridCellStocks);
        }

        /// <summary>
        /// Return an IEnumerable as an IEnumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
    }

    /// <summary>
    /// IEnumerator for the grid cell stocks
    /// </summary>
    public class GridCellStocksEnum : IEnumerator<List<Stock>>
    {
        // The array of lists of gridCellStocks
        /// <summary>
        /// The grid cell stocks as a vector (with elements corresponding to functional groups) of lists of stocks
        /// </summary>
        public List<Stock>[] GridCellStocks;

        /// <summary>
        /// Current position in the vector of lists of stocks
        /// </summary>
        int position = -1;

        /// <summary>
        /// Assign the passed set of grid cell stocks to the internal vector of lists of stocks 
        /// </summary>
        /// <param name="list"></param>
        public GridCellStocksEnum(List<Stock>[] list)
        {
            GridCellStocks = list;
        }

        /// <summary>
        /// Move to the next element in the vector of lists of stocks
        /// </summary>
        /// <returns>True if the end of the list had not been reached</returns>
        public bool MoveNext()
        {
            position++;
            return (position < GridCellStocks.Length);
        }

        /// <summary>
        /// Move back to the first element in the vector of lists of stocks
        /// </summary>
        public void Reset()
        {
            position = -1;
        }

        /// <summary>
        /// Returns the list of stocks for the current position (i.e. functional group) in the vector of lists of stocks
        /// </summary>
        object System.Collections.IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        /// <summary>
        /// Get the list of stocks for the current position (i.e. functional group) in the vector of lists of stocks
        /// </summary>
        public List<Stock> Current
        {
            get
            {
                try
                {
                    return GridCellStocks[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        /// <summary>
        /// Destructor for the grid cell stocks enumerator
        /// </summary>
        public void Dispose()
        {
        }
    }

}
