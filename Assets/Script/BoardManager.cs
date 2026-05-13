using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public int boardSize; //10^10
    public float boardWidth => GetComponent<Renderer>().bounds.size.x; //실제 보드 크기

    public int dataSize => boardSize * 2 + 1; //벽+격자 크기
    
    //해당 좌표에 무엇이 있는지 판단
    public enum GridContent { Empty, Wall, Piece}
    public GridContent[,] gridData; 
    
    [HideInInspector] public float gridSize; //격자 간 거리 계산
    [HideInInspector] public Vector2 boardStartPos; //보드 왼쪽 아래 꼭짓점 좌표

    void Awake()
    {
        Instance = this;

        gridData = new GridContent[dataSize, dataSize];
        gridSize = boardWidth / (dataSize - 1);
        float halfBoard = boardWidth / 2f;
        boardStartPos = new Vector2(-halfBoard, -halfBoard);
    }

   
    private void Start()
    {
        ResetBoard();
    }

    public void ResetBoard()
    {
        for(int i = 0; i < dataSize; i++)
        {
            for(int j = 0; j < dataSize; j++)
            {
                gridData[i, j] = GridContent.Empty;
            }
        }

        foreach(var player in GameManager.Instance.players)
        {
            gridData[player.currentGridPos.x, player.currentGridPos.y] = GridContent.Piece;
        }
    }


    private void Update()
    {
        int id = GameManager.Instance.currentTurnID;
        PlayerData player = GameManager.Instance.players[id];

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = GetMousePos();
            Vector2Int targetGrid = WorldToGrid(mousePos);

            if(targetGrid == player.currentGridPos)
            {
                player.playerPiece.GetComponent<PlayerPiece>().PickUp();
                GameManager.Instance.selectedPiece = player.playerPiece;

                return;
            }
            else if(GameManager.Instance.selectedPiece != null)
            {
                GameManager.Instance.selectedPiece.GetComponent<PlayerPiece>().PlacePiece(id, targetGrid);
            }
        }
    }

    private Vector2 GetMousePos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 10f;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    //좌표를 격자 번호로 변환
    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - boardStartPos.x) / gridSize);
        int y = Mathf.RoundToInt((worldPos.y - boardStartPos.y) / gridSize);

        x = Mathf.Clamp(x, 0, dataSize - 1);
        y = Mathf.Clamp(y, 0, dataSize - 1);

        return new Vector2Int(x, y);
    }

    //격자 번호를 좌표로 변환
    public Vector3 GridToWorld(Vector2 gridPos)
    {
        float x = boardStartPos.x + (gridPos.x * gridSize);
        float y = boardStartPos.y + (gridPos.y * gridSize);

        return new Vector3(x, y, 0);
    }


    //말을 설치할 수 있는지 판단
    public bool CanPlacePiece(int id, Vector2Int targetGrid)
    {
        PlayerData data = GameManager.Instance.players[id];
        Vector2Int startGrid = data.currentGridPos;

        int dx = targetGrid.x - startGrid.x;
        int dy = targetGrid.y - startGrid.y;
        int absDx = Mathf.Abs(dx);
        int absDy = Mathf.Abs(dy);


        //보드 범위 초과
        if (targetGrid.x < 0 || targetGrid.y < 0 ||
        targetGrid.x >= dataSize || targetGrid.y >= dataSize) return false;

        //Debug.Log($"Start: {startGrid}, Target: {targetGrid}, dx: {dx}, dy: {dy}");


        //목표에 말이 있다면 이동 불가
        if (gridData[targetGrid.x, targetGrid.y] == GridContent.Piece)
        {
            Debug.Log("경로에 이미 말이 있음");
            return false;
        }


        //한 칸 이동
        if (absDx + absDy == 2)
        {
            if (absDx != 0 && absDy != 0) return false; //대각선 이동 방지

            Vector2Int mid = (startGrid + targetGrid) / 2;

            return !IsBlocked(mid.x, mid.y);
        }


        //두 칸 이동
        if (absDx == 4 || absDy == 4)
        {
            Vector2Int enemyPos = (startGrid + targetGrid) / 2;

            if (gridData[enemyPos.x, enemyPos.y] == GridContent.Piece)
            {
                Vector2Int wall1 = (startGrid + enemyPos) / 2; //나와 적 사이의 벽
                Vector2Int wall2 = (targetGrid + enemyPos) / 2; //적과 도착 지점 사이의 벽

                if (!IsBlocked(wall1.x, wall1.y) && !IsBlocked(wall2.x, wall2.y))
                {
                    return true;
                }
            }
        }


        //대각선 이동(적 뒤에 벽이 있을 경우)
        if (absDx == 2 && absDy == 2)
        {
            //가로의 적
            Vector2Int enemySideX = new Vector2Int(startGrid.x + dx, startGrid.y);

            if (HasPiece(enemySideX.x, enemySideX.y))
            {
                Vector2Int wallBehindEnemy = new Vector2Int(enemySideX.x + dx, enemySideX.y);
                Vector2Int wallBetween = new Vector2Int(startGrid.x + dx / 2, startGrid.y);
                Vector2Int wallToTarget = new Vector2Int(enemySideX.x, enemySideX.y + dy / 2);

                if (IsBlocked(wallBehindEnemy.x, wallBehindEnemy.y)
                    && !IsBlocked(wallBetween.x, wallBetween.y)
                    && !IsBlocked(wallToTarget.x, wallToTarget.y))
                {
                    return true;
                }
            }

            //세로의 적
            Vector2Int enemySideY = new Vector2Int(startGrid.x, startGrid.y + dy);

            if (HasPiece(enemySideY.x, enemySideY.y))
            {
                Vector2Int wallBehindEnemy = new Vector2Int(enemySideY.x, enemySideY.y + dy / 2);
                Vector2Int wallBetween = new Vector2Int(startGrid.x, startGrid.y + dy / 2);
                Vector2Int wallToTarget = new Vector2Int(enemySideY.x + dx / 2, enemySideY.y);

                if (IsBlocked(wallBehindEnemy.x, wallBehindEnemy.y)
                    && !IsBlocked(wallBetween.x, wallBetween.y)
                    && !IsBlocked(wallToTarget.x, wallToTarget.y))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void UpdatePieceData(Vector2Int oldGrid, Vector2Int newGrid)
    {
        if (oldGrid.x >= 0 && oldGrid.y >= 0)
        {
            gridData[oldGrid.x, oldGrid.y] = GridContent.Empty;
        }

        gridData[newGrid.x, newGrid.y] = GridContent.Piece;
    }

    public bool HasPiece(int x, int y)
    {
        if (x < 0 || y < 0 || x >= dataSize || y >= dataSize) return false;

        return gridData[x, y] == GridContent.Piece;
    }


    //벽을 설치할 수 있는지 판단
    public bool CanPlaceWall(Vector2Int[] offsets, Vector2Int basePos)
    {
        foreach(var offset in offsets)
        {
            Vector2Int target = basePos + offset;

            //보드 범위 초과
            if(target.x < 0 || target.x >= dataSize || target.y < 0 || target.y >= dataSize)
            {
                Debug.Log("보드 범위 초과");
                return false;
            }

            //이미 벽이 있으면 설치 불가
            if (gridData[target.x, target.y] == GridContent.Wall)
            {
                Debug.Log("이미 벽이 있음");
                return false;
            }
        }
        return CheckWall(offsets, basePos); //길을 모두 막는 것은 불가능
    }

    public void PlaceWallData(Vector2Int[] offsets, Vector2Int currentGrid)
    {
        foreach(var offset in offsets)
        {
            int targetX = currentGrid.x + offset.x;
            int targetY = currentGrid.y + offset.y;

            if(targetX >= 0 && targetX < dataSize && targetY >= 0 && targetY < dataSize)
            {
                gridData[targetX, targetY] = GridContent.Wall;
                Debug.Log($"벽 데이터 입력: [{targetX}, {targetY}]");
            }            
        }
    }

    public bool IsBlocked(int x, int y)
    {
        if (x < 0 || y < 0 || x >= dataSize || y >= dataSize) return true;

        return gridData[x, y] == GridContent.Wall;
    }


    //길을 막는지 판단
    public bool CheckWall(Vector2Int[] offsets, Vector2Int basePos)
    {
        foreach (var offset in offsets)
        {
            gridData[basePos.x + offset.x, basePos.y + offset.y] = GridContent.Wall;
        }

        bool p1CanGo = CanReachGoal(GameManager.Instance.player1.currentGridPos, GameManager.Instance.player1.targetY);
        bool p2CanGo = CanReachGoal(GameManager.Instance.player2.currentGridPos, GameManager.Instance.player2.targetY);

        foreach(var offset in offsets)
        {
            gridData[basePos.x + offset.x, basePos.y + offset.y] = GridContent.Empty;
        }

        Debug.Log($"p1: {p1CanGo}, p2: {p2CanGo}");

        return p1CanGo && p2CanGo; 
    }

    //BFS로 길 탐색
    public bool CanReachGoal(Vector2Int startPos, int targetY)
    {
        bool[,] visited = new bool[dataSize, dataSize];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(startPos);
        visited[startPos.x, startPos.y] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current.y == targetY) return true; //목적지 도착

            //상하좌우 탐색
            Vector2Int[] dirs = { Vector2Int.up * 2, Vector2Int.down * 2, Vector2Int.left * 2, Vector2Int.right * 2 };
            foreach (var dir in dirs)
            {
                Vector2Int next = current + dir; //이동 경로
                Vector2Int wall = current + (dir / 2); //이동 경로의 벽 좌표

                if (next.x >= 0 && next.x < dataSize && next.y >= 0 && next.y < dataSize)
                {
                    if (!visited[next.x, next.y] && gridData[wall.x, wall.y] != GridContent.Wall)
                    {
                        visited[next.x, next.y] = true;
                        queue.Enqueue(next);
                    }
                }
            }
        }
        return false;
    }
}
