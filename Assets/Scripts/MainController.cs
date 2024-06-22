using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{

    public GameObject symbolPrefab;
    public int rows = 5;
    public int cols = 5;

    public float XOffset;
    public float YOffset;

    public float gridSpacing;

    public float gridZ;

    protected GameObject[,] symbolInstanceGrid;

    // Start is called before the first frame update
    void Start()
    {
        // instantiate the grid
        symbolInstanceGrid = new GameObject[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                GameObject symbolAnchor = Instantiate(symbolPrefab);
                symbolAnchor.transform.position = new Vector3((i - XOffset) * gridSpacing, (j - YOffset) * gridSpacing, gridZ);

                symbolInstanceGrid[i, j] = symbolAnchor;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
