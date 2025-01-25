using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CliffLeeCL
{
    public class GridData
    {
        public List<List<Node>> nodeList = new List<List<Node>>();

        public GridData(int xNum, int yNum, Vector2 gridSize, Vector2 centerPosition)
        {
            Vector2 startPosition = centerPosition - (gridSize / 2.0f);
            Vector2 offset = new Vector2(gridSize.x / xNum, gridSize.y / yNum);

            for (int i = 0; i < xNum; i++)
            {
                List<Node> xList = new List<Node>();
                for(int j = 0; j < yNum; j++)
                {
                    Node node = new Node
                    {
                        xIndex = i,
                        yIndex = j,
                        position = startPosition + new Vector2(offset.x * i + (offset.x / 2.0f), offset.y * j + (offset.y / 2.0f))
                    };
                    xList.Add(node);
                }
                nodeList.Add(xList);
            }
        }
    }

    public class Node
    {
        public int xIndex = 0;
        public int yIndex = 0;
        public Vector3 position = Vector3.zero;
    }
}
