using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerPiece : MonoBehaviour
{
    public int ownerPlayerID;

    //public static PlayerPiece selectedPiece; //선택된 말
    private SpriteRenderer SpriteRenderer;
    private Vector3 originalPos;

    private bool isDragging = false;
    private bool justPicked = false;


    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        if (GameManager.Instance.IsMyTurn(ownerPlayerID))
        {
            if (!isDragging)
            {
                isDragging = true;
                justPicked = true;
                originalPos = transform.position;

                GameManager.Instance.selectedPiece = this.gameObject;
                Debug.Log("말 클릭");
            }
        }
    }

    private void Update()
    {
        if (!isDragging) return;

        //마우스 좌표
        Vector3 targetPos = Input.mousePosition;
        targetPos.z = 10f;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(targetPos);
        Vector2Int currentGrid = BoardManager.Instance.WorldToGrid(worldMousePos);

        //transform.position = BoardManager.Instance.GridToWorld(currentGrid);

        bool canMove = CanMove(currentGrid);

        if (canMove)
        {
            transform.position = BoardManager.Instance.GridToWorld(currentGrid);

            if (SpriteRenderer != null)
            {
                SpriteRenderer.color = canMove ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
            }
        }
        else
        {
            transform.position = originalPos;

            if (SpriteRenderer != null)
            {
                SpriteRenderer.color = canMove ? new Color(1, 0, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
            }
        }

        //esc키 다운 시 드래그 취소
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelDragging();
            return;
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

            if (canMove)
            {
                MoveTo(currentGrid);
            }
            else
            {
                Debug.Log("설치 실패");
            }
        }
    }

    public void MoveTo(Vector2Int targetGrid)
    {
        isDragging = false;
        transform.position = BoardManager.Instance.GridToWorld(targetGrid);
        
        //색 원상복구
        if (SpriteRenderer != null)
        {
            SpriteRenderer.color = Color.white;
        }

        GameManager.Instance.selectedPiece = null;
        GameManager.Instance.IsPlayerAction();
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

        GameManager.Instance.selectedPiece = null;
    }

    public bool CanMove(Vector2Int targetGrid)
    {
        Vector2Int startGrid = BoardManager.Instance.WorldToGrid(originalPos);

        //거리 계산
        int distance = Mathf.Abs(targetGrid.x - startGrid.x) + Mathf.Abs(targetGrid.y - startGrid.y);
        
        //보드의 범위 내에서 이동 가능
        if(targetGrid.x < 0 || targetGrid.x >= BoardManager.Instance.boardSize || targetGrid.y >= BoardManager.Instance.boardSize)
        {
            Debug.Log("보드 범위 밖 이동 불가");
            return false;
        }

        //한 칸씩 이동 가능
        if (distance != 1)
        {
            Debug.Log("이동 범위 초과");
            return false;
        }

        //벽 너머로 이동 불가
        Vector2Int midPoint = new Vector2Int((startGrid.x + targetGrid.x) / 2, (startGrid.y + targetGrid.y) / 2);
        if (BoardManager.Instance.IsBlocked(midPoint.x, midPoint.y))
        {
            Debug.Log("벽 너머로 이동 불가");
            return false;
        }

        //상대방 말 점프
    
        return true;
    }
}

