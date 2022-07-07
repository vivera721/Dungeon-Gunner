using UnityEngine;

public class GridNodes
{
    private int width;
    private int height;

    private Node[,] gridNode;

    // 생성자
    // 2차원 배열에 대한 노드 생성
    public GridNodes(int width, int height)
    {
        this.width = width;
        this.height = height;

        gridNode = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridNode[x, y] = new Node(new Vector2Int(x, y));
            }
        }

    }

    public Node GetGridNode(int xPosition, int yPosition)
    {
        if (xPosition < width && yPosition < height)
        {
            return gridNode[xPosition, yPosition];
        }
        else
        {
            Debug.Log("Requested grid node is out of range");
            Debug.Log("요청된 그리드 노드가 범위를 벗어남");
            return null;
        }
    }


}