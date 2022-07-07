using System;
using UnityEngine;

// iComparable 인터페이스 사용 - 상속받은 클래스의 인스턴스를 정렬함 - 비교대상 메서드 필요
public class Node : IComparable<Node>
{
    // G Cost (G 값) = Distance From Start (시작지점으로부터 거리)
    // F Cost (F 값) = G Cost + H Cost (G 값 + H 값)
    // H Cost (H 값) = Distance To Finish (도착지점까지 거리)

    public Vector2Int gridPosition;
    public int gCost = 0;
    public int hCost = 0;
    public Node parentNode;

    public Node(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;

        parentNode = null;
    }

    public int FCost
    {
        get
        {
            return gCost + hCost;
        }
    }


    // 비교 대상 메서드는 클래스 인스턴스가 어떤것인지 결정, 비교할 노드 전달
    // 반환값은 비교할 노드들의 순서에 따라 다름
    public int CompareTo(Node nodeToCompare)
    {
        // Compare will be ( less than ) < 0 if this instance Fcost is less than nodeToCompare.FCost
        // Compare will be ( greater than ) > 0 if this instance Fcost is greater than nodeToCompare.FCost
        // Compare woo; be == 0 if the value are the same

        int compare = FCost.CompareTo(nodeToCompare.FCost);

        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return compare;


    }

}
