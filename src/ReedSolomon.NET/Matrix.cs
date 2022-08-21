// Matrix Algebra over an 8-bit Galois Field
// Copyright © 2022 Kodjo Laurent Egbakou
// Copyright 2015, Backblaze, Inc.

using System;
using System.Linq;
using System.Text;

namespace ReedSolomon.NET
{
    /// <summary>
    /// A matrix over the 8-bit Galois field.
    /// This class is not performance-critical, so the implementations
    /// are simple and straightforward.
    /// </summary>
    public class Matrix
    {
        /// <summary>
        /// The number of columns in the matrix.
        /// </summary>
        private readonly int _columns;

        /// <summary>
        /// The data in the matrix, in row major form.
        /// To get element (r, c): data[r][c]
        /// Because this this is computer science, and not math, the indices for both the row and column start at 0.
        /// </summary>
        private readonly byte[][] _data;

        /// <summary>
        /// The number of rows in the matrix.
        /// </summary>
        private readonly int _rows;

        /// <summary>
        /// Initialize a matrix of zeros.
        /// </summary>
        /// <param name="rows">The number of rows in the matrix.</param>
        /// <param name="columns">The number of columns in the matrix.</param>
        /// <exception cref="ArgumentException"></exception>
        public Matrix(int rows, int columns)
        {
            if (rows <= 0)
                throw new ArgumentException("Number of rows must be positive.", nameof(rows));
            if (columns <= 0)
                throw new ArgumentException("Number of columns must be positive.", nameof(columns));

            _rows = rows;
            _columns = columns;
            _data = new byte[rows][];
            for (var r = 0; r < rows; r++)
            {
                _data[r] = new byte[columns];
            }
        }

        /// <summary>
        /// Initializes a matrix with the given row-major data.
        /// </summary>
        /// <param name="data">The initial data.</param>
        public Matrix(byte[][] data)
        {
            _rows = data.Length;
            if (_rows <= 0)
                throw new ArgumentException("Invalid input data.", nameof(data));
            _columns = data[0].Length;
            if (_columns <= 0)
                throw new ArgumentException("Invalid input data.", nameof(data));

            _data = new byte[_rows][];
            for (var r = 0; r < _rows; r++)
            {
                if (data[r].Length != _columns)
                    throw new ArgumentException("All rows must have the same number of columns.");

                _data[r] = new byte[_columns];
                data[r].CopyTo(_data[r], 0);
                /*for (var c = 0; c < _columns; c++)
                {
                    _data[r][c] = data[r][c];
                }*/
                // Array.Copy(data[r], 0, _data[r], 0, _columns);
                // Buffer.BlockCopy(data[r], 0, _data[r], 0, _columns);
            }
        }

        /// <summary>
        /// Gets the number of rows in the matrix.
        /// </summary>
        /// <returns>The number of rows in the matrix.</returns>
        public int GetRowCount() => _rows;

        /// <summary>
        /// Gets the number of columns in the matrix.
        /// </summary>
        /// <returns>The number of columns in the matrix.</returns>
        public int GetColumnCount() => _columns;

        /// <summary>
        /// Identity Matrix returns an identity matrix of the given size.
        /// </summary>
        /// <param name="size">The size of the identity matrix.</param>
        /// <returns>The identity matrix of the given size.</returns>
        public static Matrix Identity(int size)
        {
            var identityMatrix = new Matrix(size, size);
            for (var i = 0; i < size; i++)
            {
                identityMatrix.Set(i, i, 1);
            }

            return identityMatrix;
        }

        /// <summary>
        /// Returns the value at row r, column c.
        /// </summary>
        /// <param name="row">The row number.</param>
        /// <param name="column">The colum number.</param>
        /// <returns>The value at [row][col].</returns>
        public byte Get(int row, int column)
        {
            if (row < 0 || row >= _rows)
                throw new ArgumentException("Row index out of range.", nameof(row));

            if (column < 0 || column >= _columns)
                throw new ArgumentException("Column index out of range.", nameof(column));

            return _data[row][column];
        }

        /// <summary>
        /// Sets the value at row r, column c.
        /// </summary>
        /// <param name="row">The row number.</param>
        /// <param name="column">The colum number.</param>
        /// <param name="value">the value to insert at [row][column].</param>
        public void Set(int row, int column, byte value)
        {
            if (row < 0 || row >= _rows)
                throw new ArgumentException("Row index out of range.", nameof(row));

            if (column < 0 || column >= _columns)
                throw new ArgumentException("Column index out of range.", nameof(column));

            _data[row][column] = value;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is Matrix matrix))
                return false;

            for (var r = 0; r < _rows; r++)
            {
                if (!_data[r].SequenceEqual(matrix._data[r]))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(_columns, _data, _rows);
        }

        /// <summary>
        /// Custom Equals.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwose, false.</returns>
        protected bool Equals(Matrix other)
        {
            return _columns == other._columns && _rows == other._rows && _data.Equals(other._data);
        }

        /// <summary>
        /// Return a human-readable representation of the matrix.
        /// Example:<br/>
        /// [1, 2, 3] <br/>
        /// [4, 5, 6] <br/>
        /// [7, 8, 9]
        /// </summary>
        /// <returns>A human-readable representation of the matrix.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var r = 0; r < _rows; r++)
            {
                sb.Append("[");
                for (var c = 0; c < _columns; c++)
                {
                    sb.Append(_data[r][c] & 0xFF);
                    if (c < _columns - 1)
                        sb.Append(", ");
                }

                sb.Append("]\n");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Return a human-readable representation of the matrix.
        /// Example:[[1, 2, 3], [4, 5, 6], [7, 8, 9]]
        /// </summary>
        /// <returns>A human-readable representation of the matrix.</returns>
        public string ToOneLineString()
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (var r = 0; r < _rows; r++)
            {
                sb.Append("[");
                for (var c = 0; c < _columns; c++)
                {
                    sb.Append(_data[r][c] & 0xFF);
                    if (c < _columns - 1)
                        sb.Append(", ");
                }

                sb.Append("]");
                if (r < _rows - 1)
                    sb.Append(", ");
            }

            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Multiplies this matrix (the one on the left) by another matrix (the one on the right).
        /// </summary>
        /// <param name="right">The another matrix.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The number of columns in first matrix must match number
        /// of rows in second matrix.</exception>
        public Matrix Multiply(Matrix right)
        {
            if (_columns != right.GetRowCount())
                throw new ArgumentException(
                    "Number of columns in first matrix must match number of rows in second matrix.");

            var result = new Matrix(_rows, right.GetColumnCount());

            for (var r = 0; r < _rows; r++)
            {
                for (var c = 0; c < right.GetColumnCount(); c++)
                {
                    byte value = 0;
                    for (var i = 0; i < _columns; i++)
                    {
                        value ^= Galois.Multiply(_data[r][i], right.Get(i, c));
                    }

                    result.Set(r, c, value);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the concatenation of this matrix and the matrix on the right.
        /// </summary>
        /// <param name="right">The another matrix.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The number of rows in the two matrices must be the same.</exception>
        public Matrix Augment(Matrix right)
        {
            if (_rows != right.GetRowCount())
                throw new ArgumentException(
                    "Number of rows in first matrix must match number of rows in second matrix.");
            var result = new Matrix(_rows, _columns + right.GetColumnCount());
            for (var r = 0; r < _rows; r++)
            {
                for (var c = 0; c < _columns; c++)
                {
                    result._data[r][c] = _data[r][c];
                    // result.Set(r, c, _data[r][c]);
                }

                for (var c = 0; c < right.GetColumnCount(); c++)
                {
                    result._data[r][_columns + c] = right._data[r][c];
                    // result.Set(r, c + _columns, right.Get(r, c));
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a part of the current matrix.
        /// For example, calling SubMatrix(0, 1, 1, 2) on a the bellow 3x3: <br/>
        /// [1, 2, 3] <br/>
        /// [4, 5, 6] <br/>
        /// [7, 8, 9] <br/>
        /// will return a 2x2 matrix: <br/>
        /// [2, 3] <br/>
        /// [5, 6] <br/>
        /// </summary>
        /// <param name="rowMin">The row min number where to start extracting the sub-matrix.</param>
        /// <param name="columnMin">The column min number where to start extract the sub-matrix.</param>
        /// <param name="rowMax">The row max number where to stop the extraction of the sub-matrix.</param>
        /// <param name="columnMax">The column max number where to stop the extraction of the sub-matrix.</param>
        /// <returns></returns>
        public Matrix SubMatrix(int rowMin, int columnMin, int rowMax, int columnMax)
        {
            var result = new Matrix(rowMax - rowMin, columnMax - columnMin);
            for (var r = rowMin; r < rowMax; r++)
            {
                for (var c = columnMin; c < columnMax; c++)
                {
                    result._data[r - rowMin][c - columnMin] = _data[r][c];
                    // result.Set(r - rowMin, c - columnMin, _data[r][c]);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns one row of the matrix as a byte array.
        /// ~6x faster than the Java implementation.
        /// </summary>
        /// <param name="rowNumber">The row number.</param>
        /// <returns>The row at position rowNumber.</returns>
        /// <exception cref="ArgumentException">Occurs when the rowNumber is less than 0
        /// or greater than the number of rows in the matrix.</exception>
        public byte[] GetRow(int rowNumber)
        {
            if (rowNumber < 0 || rowNumber >= _rows)
                throw new ArgumentException("Row number is out of bounds.");

            var result = new byte[_columns];
            _data[rowNumber].CopyTo(result, 0);
            // Array.Copy(_data[rowNumber], result, _columns);
            return result;
        }

        /// <summary>
        /// Exchanges two rows in the matrix.
        /// </summary>
        /// <param name="row1">The first row.</param>
        /// <param name="row2">The second row.</param>
        /// <exception cref="ArgumentException">Occurs when the row numbers is out of bounds.</exception>
        public void SwapRows(int row1, int row2)
        {
            if (row1 < 0 || row1 >= _rows || row2 < 0 || row2 >= _rows)
                throw new ArgumentException("Row number is out of bounds.");

            (_data[row1], _data[row2]) = (_data[row2], _data[row1]);
        }

        /// <summary>
        /// Check if the matrix is square.
        /// </summary>
        /// <returns></returns>
        public bool IsSquare() => _rows == _columns;


        /// <summary>
        /// Create a Vandermonde matrix, which is guaranteed to have the property
        /// that any subset of rows that forms a square matrix is invertible.
        /// </summary>
        /// <param name="rows">The number of rows in the vandermonde matrix.</param>
        /// <param name="columns">The number of columns in the vandermonde matrix.</param>
        /// <returns>A Vandermonde matrix.</returns>
        public static Matrix Vandermonde(int rows, int columns)
        {
            var result = new Matrix(rows, columns);
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < columns; c++)
                {
                    result.Set(r, c, Galois.Exp((byte)r, c));
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the inverse of this matrix.
        /// </summary>
        /// <returns>The inverse of the current matrix.</returns>
        /// <exception cref="InvalidOperationException">Occurs when the matrix is singular
        /// and doesn't have an inverse.</exception>
        public Matrix Invert()
        {
            if (!IsSquare())
                throw new InvalidOperationException("Only square matrices can be inverted.");

            // Create a working matrix by augmenting this one with an identity matrix on the right.
            var work = Augment(Identity(_rows));

            // Do Gaussian elimination to transform the left half into an identity matrix.
            work.GaussianElimination();

            // The right half is now the inverse.
            return work.SubMatrix(0, _rows, _columns, _columns * 2);
        }

        /// <summary>
        /// Does the work of matrix inversion. Assumes that this is an r by 2r matrix
        /// </summary>
        /// <exception cref="InvalidOperationException">Occurs when the matrix is singular.</exception>
        private void GaussianElimination()
        {
            // Clear out the part below the main diagonal and scale the main
            // diagonal to be 1.
            for (var r = 0; r < _rows; r++)
            {
                // If the element on the diagonal is 0, find a row below
                // that has a non-zero and swap them.
                if (_data[r][r] == 0)
                {
                    for (var rowBelow = r + 1; rowBelow < _rows; rowBelow++)
                    {
                        if (_data[rowBelow][r] == 0)
                            continue;
                        SwapRows(r, rowBelow);
                        break;
                    }
                }

                // If we couldn't find one, the matrix is singular.
                if (_data[r][r] == 0)
                    throw new InvalidOperationException("Matrix is singular.");

                // Scale the row to be 1.
                if (_data[r][r] != 1)
                {
                    var scale = Galois.Divide(1, _data[r][r]);
                    for (var c = 0; c < _columns; c++)
                    {
                        _data[r][c] = Galois.Multiply(_data[r][c], scale);
                    }
                }

                // Make everything below the 1 be a 0 by subtracting
                // a multiple of it.  (Subtraction and addition are
                // both exclusive or in the Galois field.)
                for (var rowBelow = r + 1; rowBelow < _rows; rowBelow++)
                {
                    if (_data[rowBelow][r] == 0)
                        continue;
                    var scale = _data[rowBelow][r];
                    for (var c = 0; c < _columns; c++)
                    {
                        _data[rowBelow][c] ^= Galois.Multiply(scale, _data[r][c]);
                    }
                }
            }

            // Now clear the part above the main diagonal.
            for (var d = 0; d < _rows; d++)
            {
                for (var rowAbove = 0; rowAbove < d; rowAbove++)
                {
                    if (_data[rowAbove][d] == 0)
                        continue;
                    var scale = _data[rowAbove][d];
                    for (var c = 0; c < _columns; c++)
                    {
                        _data[rowAbove][c] ^= Galois.Multiply(scale, _data[d][c]);
                    }
                }
            }
        }
    }
}
