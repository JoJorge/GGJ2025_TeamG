using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LeastSquaresSolverLineTest : MonoBehaviour {
    public GameObject samplePointPrefab;
    public LineRenderer lineRenderer;
    public float lineStartX;
    public float lineEndX;

    public List<double> A = new List<double>();
    public List<double> B = new List<double>();
    public List<double> X = new List<double>();

    public double[] XArray;

    MathDotNetLeastSquaresSolver solver = new MathDotNetLeastSquaresSolver();

    // Use this for initialization
    void Start () {
        X.Add(0.0f);
        X.Add(0.0f);
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)){
                Instantiate(samplePointPrefab, hit.point, Quaternion.identity);
                A.Add(hit.point.x);
                A.Add(1.0f);
                B.Add(hit.point.z);
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            XArray = (double[])X.ToArray().Clone();
            solver.SolveAxEqB(A.Count / 2, 2, A.ToArray(), B.ToArray(), XArray, true);

            print("Solved: Y = " + XArray[0] + "X" + " + " + XArray[1]);

            Vector3[] points = new Vector3[2];
            points[0] = new Vector3(lineStartX, 1.0f, lineStartX * (float)XArray[0] + (float)XArray[1]);
            points[1] = new Vector3(lineEndX, 1.0f, lineEndX * (float)XArray[0] + (float)XArray[1]);
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(points);
        }

        if (Input.GetMouseButtonUp(2))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
