using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class WallPiece : MonoBehaviour
{
    //public int ownerPlayerID;

    private SpriteRenderer SpriteRenderer;

    public Vector2Int[] occupiedOffsets; //벽이 보드에서 차지하는 좌표
    private bool isPlaced = false; //설치 상태
    private bool isDragging = false; //드래그 상태
    private bool justPicked = false; //방금 집었는지 확인

    private Vector3 originalPos;

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    //벽을 클릭하면 마우스를 따라옴
    private void OnMouseDown()
    {
        if (isPlaced) return;

        if (!isDragging)
        {
                isDragging = true;
                justPicked = true;
                originalPos = transform.position; //벽을 들어올린 위치 기억
                Debug.Log("벽 클릭");
            
        }
    }

    private void Update() //벽은 드래그로 구현
    {
        if (isPlaced || !isDragging) return;

        //마우스 좌표
        Vector3 targetPos = Input.mousePosition;
        targetPos.z = 10f;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(targetPos);
        Vector2Int currentGrid = BoardManager.Instance.WorldToGrid(worldMousePos);

        transform.position = AdjustPos(currentGrid);

        bool canPlace = BoardManager.Instance.CanPlaceWall(occupiedOffsets, currentGrid);

        //설치 가능 여부에 따른 색 변경
        if (SpriteRenderer != null)
        {
            SpriteRenderer.color = canPlace ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        }

        //esc키 다운 시 드래그 취소
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            CancelDragging();
            return;
        }

        //우클릭 시 회전
        if(Input.GetMouseButtonDown(1))
        {
            transform.Rotate(0, 0, -90f);
        }

        //좌클릭 시 설치
        if (Input.GetMouseButtonDown(0))
        {
            //잡고 있는 상태인지 확인
            if (justPicked)
            {
                justPicked = false;
                return;
            }

            if (canPlace)
            {
                PlaceWall(currentGrid);
            }
            else
            {
                Debug.Log("설치 실패");
            }
        }
    }

    //차지하는 칸의 개수가 짝수일 때, 오프셋을 더하여 계산
    private Vector3 AdjustPos(Vector2Int gridPos)
    {
        Vector3 conPos = BoardManager.Instance.GridToWorld(gridPos);
        float gridSize = BoardManager.Instance.gridSize;

        //벽의 월드 크기를 격자 수로 변환 및 계산
        Vector3 spriteSize = SpriteRenderer.bounds.size;
        int width = Mathf.RoundToInt(spriteSize.x / gridSize);
        int height = Mathf.RoundToInt(spriteSize.y / gridSize);

        //짝수일 경우 0.5만큼 이동
        if (width % 2 == 0) conPos.x += gridSize * 0.5f;
        if (height % 2 == 0) conPos.y += gridSize * 0.5f;

        return conPos;
    }

    private void CancelDragging()
    {
        isDragging = false;
        justPicked = false;
        transform.position = originalPos;

        //색 원상복구
        if (SpriteRenderer != null)
        {
            SpriteRenderer.color = Color.white;
        }

        Debug.Log("벽 설치 취소");
    }

    public void PlaceWall(Vector2Int baseGridPos)
    {
        isPlaced = true;
        isDragging = false;

        //색 원상복구
        if (SpriteRenderer != null)
        {
            SpriteRenderer.color = Color.white;
        }

        BoardManager.Instance.PlaceWallData(occupiedOffsets, baseGridPos);
        GameManager.Instance.IsPlayerAction();
    }
}
