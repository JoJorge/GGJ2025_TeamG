using System;
using System.Runtime.InteropServices;

public class MatlabLeastSquaresSolverWrapper : ILeastSquaresSolver
{
    [DllImport("LeastSquaresSolver", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr CreateMatlabLeastSquaresSolver();
    [DllImport("LeastSquaresSolver", CallingConvention = CallingConvention.Cdecl)]
    private static extern void DisposeMatlabLeastSquaresSolver(IntPtr instance);

    /** \brief Solve Ax = b
	 * 
	 * \aram iMatrixRow The number of rows of Matrix A
	 * \param iMatrixCol The number of columns of Matrix A
	 * \param pdMatrixA The data of matrix A in column major 1D array
	 * \param pdVectorB The data of vector b
	 * \param pdVectorX The output space of vector x
	 * \param bRowMajor Is the data of pdMatrixA in row major?
	 */
    [DllImport("LeastSquaresSolver", CallingConvention = CallingConvention.Cdecl)]
    private static extern void SolveAxEqBMatlab(IntPtr instance,
                                int iMatrixRow,
                                int iMatrixCol,
                                double[] pdMatrixA, // should not be modified
                                double[] pdVectorB, // should not be modified
                                double[] pdVectorX,
                                bool bRowMajor);

    /**
	 * C and cpp use row major to store 2d matrix into a 1d array,
	 * but some library for math use column major.
	 * I provide a switch function to change that.
	 */
    [DllImport("LeastSquaresSolver", CallingConvention = CallingConvention.Cdecl)]
    private static extern void RowMajorToColumnMajorMatlab(IntPtr instance, int iMatrixRow, int iMatrixCol, double[] pdMatrix);
    [DllImport("LeastSquaresSolver", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ColumnMajorToRowMajorMatlab(IntPtr instance, int iMatrixRow, int iMatrixCol, double[] pdMatrix);

    /// <summary>
    /// The instance of real executor.
    /// </summary>
    private IntPtr instance;

    public MatlabLeastSquaresSolverWrapper()
    {
        instance = CreateMatlabLeastSquaresSolver();
    }

    ~MatlabLeastSquaresSolverWrapper()
    {
        DisposeMatlabLeastSquaresSolver(instance);
    }

    public void SolveAxEqB(int iMatrixRow,
                           int iMatrixCol,
                           double[] pdMatrixA, // should not be modified
                           double[] pdVectorB, // should not be modified
                           double[] pdVectorX,
                           bool bRowMajor)
    {
        SolveAxEqBMatlab(instance, iMatrixRow, iMatrixCol, pdMatrixA, pdVectorB, pdVectorX, bRowMajor);
    }

    public void RowMajorToColumnMajorMatlab(int iMatrixRow, int iMatrixCol, double[] pdMatrix)
    {
        RowMajorToColumnMajorMatlab(instance, iMatrixRow, iMatrixCol, pdMatrix);
    }

    public void ColumnMajorToRowMajorMatlab(int iMatrixRow, int iMatrixCol, double[] pdMatrix)
    {
        ColumnMajorToRowMajorMatlab(instance, iMatrixRow, iMatrixCol, pdMatrix);
    }
}
