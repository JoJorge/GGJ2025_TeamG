public interface ILeastSquaresSolver
{
    void SolveAxEqB(int iMatrixRow,
                    int iMatrixCol,
                    double[] pdMatrixA, // should not be modified
                    double[] pdVectorB, // should not be modified
                    double[] pdVectorX,
                    bool bRowMajor);
}