using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public Vector2 boardStartPos = new Vector2(-2.65f, -2.65f); //보드 왼쪽 아래 첫 칸 좌표
    public int boardSize = 10; //10^10

    public int dataSize => boardSize * 2 - 1; //벽+격자 크기
    public bool[,] gridData; //벽이 있는지 판단
    public float gridSize => (Mathf.Abs(boardStartPos.x) * 2f) / (dataSize - 1); //격자 간 거리 계산

    void Awake()
    {
        Instance = this;
        gridData = new bool[dataSize, dataSize];
    }

    private void Start()
    {
        WallPiece[] allWalls = Object.FindObjectsByType<WallPiece>(FindObjectsSortMode.None);
        
        foreach(var wall in allWalls)
        {
            Vector2Int pos = WorldToGrid(wall.transform.position);
        }
    }

    void OnMouseDown()
    {
        //플레이어의 말이 선택 되었을 때 이동
        if(GameManager.Instance.selectedPiece != null)
        {
            PlayerPiece selectedPlayer = GameManager.Instance.selectedPiece.GetComponent<PlayerPiece>();

            if(selectedPlayer != null)
            {
                Vector2Int targetPos = WorldToGrid(GetMousePos());
                selectedPlayer.MoveTo(targetPos);
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

            //이미 사용 중이면 설치 불가
            if (gridData[target.x, target.y])
            {
                Debug.Log("이미 벽이 있음");
                return false;
            }
        }
        return true;
    }

    public void PlaceWallData(Vector2Int[] offsets, Vector2Int currentGrid)
    {
        foreach(var offset in offsets)
        {
            int targetX = currentGrid.x + offset.x;
            int targetY = currentGrid.y + offset.y;

            if(targetX >= 0 && targetX < dataSize && targetY >= 0 && targetY < dataSize)
            {
                gridData[targetX, targetY] = true;
                Debug.Log($"벽 데이터 입력: [{targetX}, {targetY}]");
            }            
        }
    }

    public bool IsBlocked(int x, int y)
    {
        if (x < 0 || y < 0 || x >= dataSize || y >= dataSize) return true;

        return gridData[x, y];
    }
}
