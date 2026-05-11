using Unity.Burst.CompilerServices;
using Unity.Mathematics;
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

        //짝수 좌표로 변환
        Vector2Int snapGrid = new Vector2Int(
            Mathf.RoundToInt(currentGrid.x / 2f ) * 2, 
            Mathf.RoundToInt(currentGrid.y / 2f) * 2
        );

        bool canMove = CanMove(snapGrid);

        if (canMove)
        {
            transform.position = BoardManager.Instance.GridToWorld(snapGrid);

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
                MoveTo(snapGrid);
            }
            else
            {
                Debug.Log("설치 실패");
            }
        }
    }

    public void MoveTo(Vector2Int targetGrid)
    {
        //dataSize에서 말은 짝수 단위로 움직여야함
        if (targetGrid.x % 2 != 0 || targetGrid.y % 2 != 0) return;

        if (!CanMove(targetGrid))
        {
            return;
        }

        isDragging = false;
        transform.position = BoardManager.Instance.GridToWorld(targetGrid);

        //게임 승리 판정
        GameManager.Instance.IsEnd(this);

        //색 원상복구
        if (SpriteRenderer != null)
        {
            SpriteRenderer.color = Color.white;
        }

        GameManager.Instance.selectedPiece = null;
        GameManager.Instance.IsPlayerAction();
        Debug.Log($"말 설치 : x({targetGrid.x}), y({targetGrid.y})");
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

        int dx = Mathf.Abs(targetGrid.x - startGrid.x);
        int dy = Mathf.Abs(targetGrid.y - startGrid.y);
        int distance = Mathf.Abs(dx) + Mathf.Abs(dy);

        bool isStraightOne = (dx == 2 && dy == 0) || (dx == 0 && dy == 2);

        //한 칸씩 이동 가능, 대각선 이동 불가
        if (!isStraightOne)
        {
            Debug.Log("이동 범위 초과");
            return false;
        }

        //벽 너머로 이동 불가
        Vector2Int midPoint = (startGrid + targetGrid) / 2;
        if (BoardManager.Instance.IsBlocked(midPoint.x, midPoint.y))
        {
            Debug.Log("벽 너머로 이동 불가");
            return false;
        }

        //상대방 말 점프
    
        return true;
    }
}

