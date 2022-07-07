using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    /// <summary>
    /// Builds a path for the room, from the startGridPosition to the endGridPosition, and adds movement steps to the returned Stack. Returns null if no path is found
    /// 시작지점 (GridPosition) 에서 끝지점 (GridPosition) 까지 이르는 방의 경로를 작성하고 반환된 스택에 이동 단계를 추가합니다. 경로를 찾을 수 없으면 null을 반환합니다.
    /// </summary>
    public static Stack<Vector3> BuildPath(Room room, Vector3Int startGridPosition, Vector3Int endGridPosition)
    {
        // Adjust position by lower bounds
        // 그리드 위치가 반드시 0을 기준으로 하는것은 아님 - 방을 기준으로 해서 방의 하한점을 잡는다
        startGridPosition -= (Vector3Int)room.templateLowerBounds;
        endGridPosition -= (Vector3Int)room.templateLowerBounds;

        // Create open list and closed hashset
        List<Node> openNodeList = new List<Node>();
        HashSet<Node> closeNodeHashSet = new HashSet<Node>();

        // Create gridnodes for path finding
        GridNodes gridNodes = new GridNodes(room.templateUpperBounds.x - room.templateLowerBounds.x + 1, room.templateUpperBounds.y - room.templateLowerBounds.y + 1);

        // 시작 노드, 타겟 노드
        Node startNode = gridNodes.GetGridNode(startGridPosition.x, startGridPosition.y);
        Node targetNode = gridNodes.GetGridNode(endGridPosition.x, endGridPosition.y);

        // FindShortestPath 이름을 지정하지만 실제로는 A* 알고리즘을 실행함 - 끝 경로 노드를 반환
        Node endPathNode = FindShortestPath(startNode, targetNode, gridNodes, openNodeList, closeNodeHashSet, room.instantiatedRoom);

        // endPathNode 가 null 이 아니라면 == 경로를 찾았다면
        if (endPathNode != null)
        {
            // 경로의 그리드 위치를 World 위치로 변환후 반환
            return CreatePathStack(endPathNode, room);
        }

        return null;

    }

    /// <summary>
    /// Find the shortest path - returns the end Node if a path has been found, else returns null
    /// 최단경로를 찾는다 - 경로를 찾았다면 마지막 노드를 반환하고 그렇지 않다면 null 반환
    /// </summary>
    private static Node FindShortestPath(Node startNode, Node targetNode, GridNodes gridNodes, List<Node> openNodeList, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        // Add start node to open list
        // 열린 노드 목록에 시작 노드를 추가한다
        openNodeList.Add(startNode);

        // Loop through open node list until empty
        // 열린 노드 목록이 비어있을때 까지
        while (openNodeList.Count > 0)
        {
            // Sort List
            openNodeList.Sort();

            // current node = the node in the open list with the lowest fCost
            // 현재 노드 = 열린 목록의 노드 중 fCost가 가장 낮은 노드 (0 번째 노드)
            // 열린 목록의 노드 중 fCost가 가장 낮은 노드 (0 번째 노드) 가 현재 노드가 되었으니 0 번째 노드 삭제
            Node currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);

            // if the current node = target node then finish
            // 현재 노드가 타겟 노드라면 경로를 찾은것이므로 반환후 종료
            if (currentNode == targetNode)
            {
                return currentNode;
            }

            // add current node to the closed list
            // 경로를 찾지 못했다면 현재 노드를 닫힌 노드 목록에 추가 - 해시집합에 add 함
            closedNodeHashSet.Add(currentNode);

            // evaluate fCost for each neighbor of the current node
            // 현재 노드에 대해 모든 인접 노드의 FCost를 평가
            EvaluateCurrentNodeNeightbors(currentNode, targetNode, gridNodes, openNodeList, closedNodeHashSet, instantiatedRoom);

        }

        return null;

    }

    /// <summary>
    /// Create a Stack<Vector3> containing the movement path
    /// </summary>
    private static Stack<Vector3> CreatePathStack(Node targetNode, Room room)
    {
        Stack<Vector3> movementPathStack = new Stack<Vector3>();

        Node nextNode = targetNode;

        // Get mid point of cell
        Vector3 cellMidPoint = room.instantiatedRoom.grid.cellSize * 0.5f;
        cellMidPoint.z = 0f;

        // 모든 부모 노드에 대해 부모를 다음 노드로 설정
        // world 공간에 추가하고 스택에 추가
        while (nextNode != null)
        {
            // Convert grid position to world position
            // 그리드 위치를 world 위치로 변환
            Vector3 worldPosition = room.instantiatedRoom.grid.CellToWorld(new Vector3Int(nextNode.gridPosition.x + room.templateLowerBounds.x,
                nextNode.gridPosition.y + room.templateLowerBounds.y, 0));

            // Set the world Position to the middle of the grid cell
            worldPosition += cellMidPoint;

            movementPathStack.Push(worldPosition);

            nextNode = nextNode.parentNode;

        }

        return movementPathStack;
    }

    /// <summary>
    /// Evaluate neighbor nodes
    /// 인접 노드 평가
    /// </summary>
    private static void EvaluateCurrentNodeNeightbors(Node currentNode, Node targetNode, GridNodes gridNodes, List<Node> openNodeList, 
        HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;

        Node validNeighborNode;

        // Loop through all directions
        // 모든 방향에 대해 반복
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                // ( i == 0 && j == 0 일때는 현재노드의 위치 ) -> 현재 노드라면 제외
                if (i == 0 && j == 0)
                    continue;

                // 인접노드가 유효하면 인접노드를 전달 - 인접노드 체크
                validNeighborNode = GetValidNodeNeighbor(currentNodeGridPosition.x + i, currentNodeGridPosition.y + j, gridNodes, closedNodeHashSet, instantiatedRoom);

                if (validNeighborNode != null)
                {
                    // Calculate new gCost for neighbor
                    // 인접노드에 대해 새로운 gCost 를 계산
                    int newCostToNeighbor;

                    // Get the movement penalty
                    // Unwalkable paths have a value of 0. Default movement penalty is set in Settings and applies to other grid squares
                    // 지나갈수 없는 경로는 0 값을 가지고 기본 이동 패널티는 Settings 에서 설정되며 다른 그리드 사각형에 적용된다
                    int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[validNeighborNode.gridPosition.x, validNeighborNode.gridPosition.y];

                    // 현재 노드와 유효한 인접 노드 사이의 거리
                    // currentNode 는 인접노드를 평가하는 부모 노드
                    newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, validNeighborNode) + movementPenaltyForGridSpace;

                    bool isValidNeighborNodeInOpenList = openNodeList.Contains(validNeighborNode);

                    // 새로 계산한 새로운 비용이 인접 비용보다 작거나 노드가 아직 열린 목록에 없다면
                    if (newCostToNeighbor < validNeighborNode.gCost || !isValidNeighborNodeInOpenList)
                    {
                        // 노드의 gCost 와 hCost 를 업데이트
                        // 유효한 이웃노드와 타겟 사이의 거리 (H 값은 그 지점에서부터 도착지점까지의 거리)
                        // 현재노드는 유효한 이웃노드의 부모노드로 업데이트
                        validNeighborNode.gCost = newCostToNeighbor;
                        validNeighborNode.hCost = GetDistance(validNeighborNode, targetNode);
                        validNeighborNode.parentNode = currentNode;

                        // 이웃노드가 열린목록에 없으면
                        if (!isValidNeighborNodeInOpenList)
                        {
                            // 열린 목록에 이웃노드 추가
                            openNodeList.Add(validNeighborNode);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns the distance int between nodeA and nodeB
    /// </summary>
    private static int GetDistance(Node nodeA, Node nodeB)
    {
        // X 거리의 절댓값 ,  Y 거리의 절댓값
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        if (dstX > dstY)
        {
            // 10 used instead of 1, and 14 is a pythagoras approximation SQRT( 10*10 + 10*10 ) -- to avoid using floats
            // (거리 단위로) 1 대신에 10 을 사용, 14 는 SQRT(10^2 + 10^2) (루트 (10^2 + 10^2)) 의 근삿값 -- 실수 사용을 피하기 위함
            return 14 * dstY + 10 * (dstX - dstY);
        }
        return 14 * dstX + 10 * (dstY - dstX);

    }

    /// <summary>
    /// Evaluate a neighbor node at neighborNodeXPosition, neighborNodeYPosition, using the specified gridNodes, 
    /// closedNodeHashSet, and instantiated room. Returns null if the node isn't valid
    /// 지정된 gridNodes, closedNodeHashSet 및 인스턴스화된 룸을 사용하여 neighborNodeXPosition 및 neighborNodeYPosition 인접 노드를 평가
    /// 노드가 올바르지 않으면 null을 반환
    /// </summary>
    private static Node GetValidNodeNeighbor(int neighborNodeXPosition, int neighborNodeYPosition, GridNodes gridNodes,
        HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        // If neighbor node position is beyond grid then return null
        // 이웃노드 위치가 그리드를 넘었다면 null 반환
        if (neighborNodeXPosition >= instantiatedRoom.room.templateUpperBounds.x - instantiatedRoom.room.templateLowerBounds.x || neighborNodeXPosition < 0
            || neighborNodeYPosition >= instantiatedRoom.room.templateUpperBounds.y - instantiatedRoom.room.templateLowerBounds.y || neighborNodeYPosition < 0)
        {
            return null;
        }

        // Get neighbor node
        Node neighborNode = gridNodes.GetGridNode(neighborNodeXPosition, neighborNodeYPosition);

        // check for obstacle at that position
        // 그 위치가 장애물인지 체크
        int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[neighborNodeXPosition, neighborNodeYPosition];

        // if neighbor is an obstacle or neighbor is in the closed list then skip
        // 이웃이 장애물이거나 이웃이 닫힌 목록에 있다면 스킵
        if (movementPenaltyForGridSpace == 0 || closedNodeHashSet.Contains(neighborNode))
        {
            return null;
        }
        // 닫힌 목록에 없다면 이웃노드 반환
        else
        {
            return neighborNode;
        }

    }

}
