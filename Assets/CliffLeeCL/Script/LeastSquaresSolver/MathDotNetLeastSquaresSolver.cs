using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class MathDotNetLeastSquaresSolver : ILeastSquaresSolver
{
    /** \brief Solve Ax = b
	 * 
	 * \aram iMatrixRow The number of rows of Matrix A
	 * \param iMatrixCol The number of columns of Matrix A
	 * \param pdMatrixA The data of matrix A in column major 1D array
	 * \param pdVectorB The data of vector b
	 * \param pdVectorX The output space of vector x
	 * \param bRowMajor Is the data of pdMatrixA in row major?
	 */
    public void SolveAxEqB(int iMatrixRow, 
                            int iMatrixCol, 
                            double[] pdMatrixA, // should not be modified
                            double[] pdVectorB, // should not be modified
                            double[] pdVectorX, 
                            bool bRowMajor)
    {
        Matrix<double> matA;
        Matrix<double> matB;
        Matrix<double> matX;

        if (bRowMajor)
        {
            matA = Matrix<double>.Build.DenseOfRowMajor(iMatrixRow, iMatrixCol, pdMatrixA);
            matB = Matrix<double>.Build.DenseOfRowMajor(iMatrixRow, 1, pdVectorB);
        }
        else
        {
            matA = Matrix<double>.Build.DenseOfColumnMajor(iMatrixRow, iMatrixCol, pdMatrixA);
            matB = Matrix<double>.Build.DenseOfColumnMajor(iMatrixRow, 1, pdVectorB);
        }

        matX = matA.Solve(matB);
        matX.ToRowMajorArray().CopyTo(pdVectorX, 0);
    }
}
