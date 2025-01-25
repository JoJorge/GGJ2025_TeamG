using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CliffLeeCL
{
    public class RoomGenerator : MonoBehaviour
    {
        public int xNum = 4;
        public int yNum = 3;
        public Vector2 roomSize = Vector2.one;
        public bool canShowGrid = true;
        public List<GameObject> roomPrefab = new List<GameObject>();

        GridData grid;

        void Start()
        {
            InitGrid();
            GenerateRoom();
        }

        void InitGrid()
        {
            grid = new GridData(xNum, yNum, new Vector2(xNum * roomSize.x, yNum * roomSize.y), transform.position);
        }

        void GenerateRoom()
        {
            GameObject roomObj = new GameObject("Room");

            for (int i = 0; i < grid.nodeList.Count; i++)
            {
                for (int j = 0; j < grid.nodeList[i].Count; j++)
                {
                    GameObject room;
                    int randomIndex = Random.Range(0, roomPrefab.Count);

                    room = Instantiate(roomPrefab[randomIndex], grid.nodeList[i][j].position, Quaternion.identity, roomObj.transform);
                    room.name += " (" + i + ", " + j + ")";
                }
            }
        }

        void OnDrawGizmos()
        {
            if (canShowGrid)
            {
                InitGrid();

                Gizmos.color = Color.cyan;
                for (int i = 0; i < grid.nodeList.Count; i++)
                {
                    for (int j = 0; j < grid.nodeList[i].Count; j++)
                    {
                        Gizmos.DrawWireCube(grid.nodeList[i][j].position, roomSize);
                        Gizmos.DrawWireSphere(grid.nodeList[i][j].position, 0.1f);
                    }
                }
            }
        }

    }
}
