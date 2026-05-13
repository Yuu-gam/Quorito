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

        //짝수 좌표로 변환
        float rawX = (worldMousePos.x - BoardManager.Instance.boardStartPos.x) / BoardManager.Instance.gridSize;
        float rawY = (worldMousePos.y - BoardManager.Instance.boardStartPos.y) / BoardManager.Instance.gridSize;

        Vector2Int snapGrid = new Vector2Int(
            Mathf.FloorToInt(rawX / 2f ) * 2,
            Mathf.FloorToInt(rawY / 2f) * 2
        );

        transform.position = BoardManager.Instance.GridToWorld(snapGrid);

        bool canPlace = BoardManager.Instance.CanPlaceWall(occupiedOffsets, snapGrid);

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

            for (int i = 0; i < occupiedOffsets.Length; i++)
            {
                int x = occupiedOffsets[i].x;
                int y = occupiedOffsets[i].y;
                occupiedOffsets[i] = new Vector2Int(y, -x);
            }
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
                PlaceWall(snapGrid);
            }
            else
            {
                Debug.Log("설치 실패");
            }
        }
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

    public void PlaceWall(Vector2Int currentGrid)
    {
        isPlaced = true;
        isDragging = false;

        //색 원상복구
        if (SpriteRenderer != null)
        {
            SpriteRenderer.color = Color.white;
        }

        BoardManager.Instance.PlaceWallData(occupiedOffsets, currentGrid);
        GameManager.Instance.EndTurn();
    }
}
