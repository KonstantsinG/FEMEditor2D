using System;
using System.Collections.Generic;

namespace FiniteElements.MathPhys
{
    public class FEMMethod
    {
        private List<Point> _points;
        private List<FiniteElement> _elements;
        private List<Force> _forces;
        private double[][] _displacements = null;

        public List<FiniteElement> Elements
        {
            get { return _elements; }
        }

        public double[][] Displacements
        {
            get
            {
                if (_displacements == null)
                    CalculateFEM();

                return _displacements;
            }
        }


        public FEMMethod(List<FiniteElement> elements, List<Point> points, List<Force> forces)
        {
            _elements = elements;
            _points = points;
            _forces = forces;
        }

        /// <summary>
        /// Calculate Finite Elements Method
        /// </summary>
        public void CalculateFEM()
        {
            // Global stiffness matrix
            double[][] globalStiffness = GetGlobalStiffness();
            // Global forces matrix
            double[] globalForces = GetGlobalForces();

            // Global displacements vector
            // (Solve globalStiffness * displacementsVector = globalForces)
            double[] displacementsVector = SolveGauss(globalStiffness, globalForces);
            // Local displacements matrix (X annd Y for each point)
            double[][] displacements = GetPointsDisplacements(displacementsVector);

            _displacements = displacements;
        }

        /// <summary>
        /// Get local displacements matrix
        /// </summary>
        /// <param name="displMatrix"> Global displacements matrix </param>
        /// <returns> Local displacements matrix </returns>
        private double[][] GetPointsDisplacements(double[] displMatrix)
        {
            double[][] displacements = new double[_points.Count][];
            for (int i = 0; i < _points.Count; i++)
                displacements[i] = new double[] {0, 0};

            for (int i = 0; i < _points.Count; i++)
            {
                if (_points[i].IsFixed)
                    continue;

                // X displacement
                displacements[i][0] = displMatrix[i * 2];
                // Y displacement
                displacements[i][1] = displMatrix[i * 2 + 1];
            }

            return displacements;
        }

        /// <summary>
        /// Solve LinAlgSystem using Gauss Method
        /// </summary>
        /// <param name="coefficients"> Coefficients matrix </param>
        /// <param name="rightSide"> Right side vector </param>
        /// <returns> LinAlgSystem solution vector </returns>
        public static double[] SolveGauss(double[][] coefficients, double[] rightSide)
        {
            // Matrix with coefficients and right side
            double[,] matrix = new double[coefficients.Length, coefficients.Length + 1];

            for (int i = 0; i < coefficients.Length; i++)
            {
                for (int j = 0; j < coefficients[i].Length; j++)
                    matrix[i, j] = coefficients[i][j];

                matrix[i, coefficients.Length] = rightSide[i];
            }

            int n = matrix.GetLength(0);
            double[,] matrixClone = new double[n, n + 1];
            double coeff;

            for (int i = 0; i < n; i++)
                for (int j = 0; j < n + 1; j++)
                    matrixClone[i, j] = matrix[i, j];

            // Forvard feed (Zeroing left-down corner)
            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n + 1; i++)
                    // Dividing the k-row by the first term !=0 to convert it to one
                    matrixClone[k, i] = matrixClone[k, i] / matrix[k, k];

                // i - next row after k
                for (int i = k + 1; i < n; i++)
                {
                    // Koefficient
                    coeff = matrixClone[i, k] / matrixClone[k, k];

                    // j - column after k
                    for (int j = 0; j < n + 1; j++)
                        // Zeroing out matrix elements below the first term converted to one
                        matrixClone[i, j] = matrixClone[i, j] - matrixClone[k, j] * coeff;
                }

                // Updating init matrix
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n + 1; j++)
                        matrix[i, j] = matrixClone[i, j];
            }

            // Backward feed (Zeroing right-top corner)
            for (int k = n - 1; k > -1; k--)
            {
                for (int i = n; i > -1; i--)
                    matrixClone[k, i] = matrixClone[k, i] / matrix[k, k];

                // i - next row after k
                for (int i = k - 1; i > -1; i--)
                {
                    coeff = matrixClone[i, k] / matrixClone[k, k];

                    // j - next row after k
                    for (int j = n; j > -1; j--)
                        matrixClone[i, j] = matrixClone[i, j] - matrixClone[k, j] * coeff;
                }
            }

            // Extract solution
            double[] result = new double[n];
            for (int i = 0; i < n; i++)
                result[i] = matrixClone[i, n];

            return result;
        }

        /// <summary>
        /// Get global forces matrix
        /// </summary>
        /// <returns> Global forces matrix </returns>
        private double[] GetGlobalForces()
        {
            double[] globalForces = new double[_points.Count * 2];
            int pt;
            
            foreach (Force f in _forces)
            {
                if (f.Magnitude <= 0)
                    continue;

                // Set X and Y force to nearest point
                pt = GetNearestPointIndex(f.Position);
                globalForces[pt * 2] += f.DirectionX;
                globalForces[pt * 2 + 1] += f.DirectionY;
            }

            globalForces = CrossOutFixedPoints(globalForces);
            
            return globalForces;
        }

        /// <summary>
        /// Get nearest point to given point
        /// </summary>
        /// <param name="p1"> Target point </param>
        /// <returns> Nearest point </returns>
        private int GetNearestPointIndex(Point p1)
        {
            double minDst = double.MaxValue;
            int minIndex = 0;
            double dst;

            foreach (Point p2 in _points)
            {
                dst = Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));

                if (dst < minDst)
                {
                    minDst = dst;
                    minIndex = GetPointIndex(p2);
                }
            }

            return minIndex;
        }

        /// <summary>
        /// Get Global Stiffness matrix
        /// </summary>
        /// <returns> Global stiffness matrix </returns>
        public double[][] GetGlobalStiffness()
        {
            int globalRow;
            int globalCol;
            double[][] globalMatrix = GetZeroMatrix(_points.Count * 2, _points.Count * 2);

            foreach (FiniteElement e in Elements)
            {
                for (int row = 0; row < e.Stiffness.Length; row++)
                {
                    // Get row index for local matrix in global matrix ( 4 x 4 fragment )
                    globalRow = 2 * GetPointIndex(e.Triangle[row / 2]) + row % 2;

                    for (int col = 0; col < e.Stiffness[0].Length; col++)
                    {
                        // Get column index for local matrix in global matrix ( 4 x 4 fragment )
                        globalCol = 2 * GetPointIndex(e.Triangle[col / 2]) + (col % 2);

                        globalMatrix[globalRow][globalCol] += e.Stiffness[row][col];
                    }
                }
            }

            globalMatrix = CrossOutFixedPoints(globalMatrix);

            return globalMatrix;
        }

        /// <summary>
        /// Cross out fixed points from matrix
        /// </summary>
        /// <param name="stiffnessMatrix"> Global Stiffness matrix </param>
        /// <returns> Changed Global Stiffness matrix </returns>
        private double[][] CrossOutFixedPoints(double[][] stiffnessMatrix)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                // Cross out rows and columns by zeros in global matrix for fixed points
                if (_points[i].IsFixed)
                {
                    for (int j = 0; j < stiffnessMatrix.Length; j++)
                    {
                        // Cross out for X
                        // If there is matrix diagonal - set 1 instead of 0
                        if (i * 2 == j)
                        {
                            stiffnessMatrix[i * 2][j] = 1;
                            stiffnessMatrix[j][i * 2] = 1;
                        }
                        else
                        {
                            stiffnessMatrix[i * 2][j] = 0;
                            stiffnessMatrix[j][i * 2] = 0;
                        }

                        // Cross out for Y
                        // If there is matrix diagonal - set 1 instead of 0
                        if (i * 2 + 1 == j)
                        {
                            stiffnessMatrix[i * 2 + 1][j] = 1;
                            stiffnessMatrix[j][i * 2 + 1] = 1;
                        }
                        else
                        {
                            stiffnessMatrix[i * 2 + 1][j] = 0;
                            stiffnessMatrix[j][i * 2 + 1] = 0;
                        }
                    }
                }
            }

            return stiffnessMatrix;
        }

        /// <summary>
        /// Cross out fixed points from vector
        /// </summary>
        /// <param name="forces"> Global forces vector </param>
        /// <returns> Changed global forces vector </returns>
        private double[] CrossOutFixedPoints(double[] forces)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                if (_points[i].IsFixed)
                {
                    forces[i * 2] = 0;
                    forces[i * 2 + 1] = 0;
                }
            }

            return forces;
        }

        /// <summary>
        /// Get index of given point
        /// </summary>
        /// <param name="p"> Target point </param>
        /// <returns> Index of given point </returns>
        private int GetPointIndex(Point p)
        {
            return _points.IndexOf(p);
        }

        /// <summary>
        /// Get zeros matrix
        /// </summary>
        /// <param name="n"> Rows count </param>
        /// <param name="m"> Columns count </param>
        /// <returns> Zeros matrix </returns>
        private double[][] GetZeroMatrix(int n, int m)
        {
            double[][] matr = new double[n][];

            for (int i = 0; i < n; i++)
            {
                matr[i] = new double[m];

                for (int j = 0; j < m; j++)
                    matr[i][j] = 0.0;
            }

            return matr;
        }


        /// <summary>
        /// Get distance between points
        /// </summary>
        /// <param name="p1"> First point </param>
        /// <param name="p2"> Second point </param>
        /// <returns> Distance between points </returns>
        public static double[] GetDistance(Point p1, Point p2)
        {
            return new double[] { p1.X - p2.X, p1.Y - p2.Y };
        }

        /// <summary>
        /// Copy Source matrix into Destination matrix
        /// </summary>
        /// <param name="source"> Source matrix </param>
        /// <param name="destination"> Destination matrix </param>
        public static void CopyMatrix(double[][] source, ref double[][] destination)
        {
            int rows = source.Length;
            int cols = source[0].Length;
            destination = new double[rows][];

            for (int i = 0; i < rows; i++)
            {
                destination[i] = new double[cols];
                Array.Copy(source[i], destination[i], cols);
            }
        }

        /// <summary>
        /// Multiply matrix by number
        /// </summary>
        /// <param name="matr"> Matrix </param>
        /// <param name="num"> Number </param>
        /// <returns> Result of multiplication </returns>
        public static double[][] MatrixMul(double[][] matr, double num)
        {
            for (int i = 0; i < matr.Length; i++)
            {
                for (int j = 0; j < matr[i].Length; j++)
                    matr[i][j] *= num;
            }

            return matr;
        }

        /// <summary>
        /// Multiply matrix by matrix
        /// </summary>
        /// <param name="matr1"> First matrix </param>
        /// <param name="matr2"> Second matrix </param>
        /// <returns> Result of multiplication </returns>
        /// <exception cref="ArgumentException"> First matrix columns must be equal to second matrix rows. </exception>
        public static double[][] MatrixMul(double[][] matr1, double[][] matr2)
        {
            int rows1 = matr1.Length;
            int cols1 = matr1[0].Length;
            int rows2 = matr2.Length;
            int cols2 = matr2[0].Length;

            if (cols1 != rows2)
                throw new ArgumentException("Matrix A columns must be equal to Matrix B rows.");

            if (rows1 == cols2 && cols1 > rows1)
                return MatrixMul(matr2, matr1);

            double[][] result = new double[rows1][];

            for (int i = 0; i < rows1; i++)
            {
                result[i] = new double[cols2];

                for (int j = 0; j < cols2; j++)
                {
                    for (int k = 0; k < cols1; k++)
                    {
                        result[i][j] += matr1[i][k] * matr2[k][j];
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Transpose matrix
        /// </summary>
        /// <param name="matr"> Matrix </param>
        /// <returns> Transposed matrix </returns>
        public static double[][] TransposeMatrix(double[][] matr)
        {
            double[][] transposedMatrix = new double[matr[0].Length][];

            for (int i = 0; i < matr[0].Length; ++i)
                transposedMatrix[i] = new double[matr.Length];

            for (int i = 0; i < matr.Length; i++)
            {
                for (int j = 0; j < matr[i].Length; j++)
                    transposedMatrix[j][i] = matr[i][j];
            }

            return transposedMatrix;
        }
    }
}
