using UnityEngine;

namespace Script
{
    public class PlayerPiece : MonoBehaviour
    {
        [Header("Player Data")]
        public int playerID;
        public int targetY; //승리 지점
        public Vector2Int currentGridPos; //현재 위치

        private SpriteRenderer SpriteRenderer;
        private Vector3 originalPos;
        private Vector2Int startGrid;

        private bool justPicked = false;
        private bool isDragging = false;

        private void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (!isDragging) return;

            //마우스 좌표
            Vector3 targetPos = Input.mousePosition;
            targetPos.z = 10f;
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(targetPos);
            
            //홀수 좌표로 변환
            float rawX = (worldMousePos.x - BoardManager.Instance.boardStartPos.x) / BoardManager.Instance.gridSize;
            float rawY = (worldMousePos.y - BoardManager.Instance.boardStartPos.y) / BoardManager.Instance.gridSize;

            Vector2Int snapGrid = new Vector2Int(
                Mathf.FloorToInt(rawX / 2f ) * 2 + 1, 
                Mathf.FloorToInt(rawY / 2f) * 2 + 1);


            bool canPlace = BoardManager.Instance.CanPlacePiece(playerID, snapGrid);

            if (canPlace)
            {
                transform.position = BoardManager.Instance.GridToWorld(snapGrid);

                if (SpriteRenderer)
                {
                    SpriteRenderer.color = new Color(0, 1, 0, 0.5f);
                }
            }
            else
            {
                transform.position = originalPos;

                if (SpriteRenderer)
                {
                    SpriteRenderer.color = new Color(1, 0, 0, 0.5f);
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

                if (canPlace)
                {
                    PlacePiece(playerID, snapGrid);
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
            if (SpriteRenderer)
            {
                SpriteRenderer.color = Color.white;
            }

            GameManager.Instance.selectedPiece = null;
        }


        public void PickUp()
        {
            if (!isDragging)
            {
                isDragging = true;
                justPicked = true;
                originalPos = transform.position;
                startGrid = BoardManager.Instance.WorldToGrid(originalPos);

                GameManager.Instance.selectedPiece = this;
                Debug.Log($"말 클릭: {startGrid}");
            }
        }

        public void PlacePiece(int id, Vector2Int targetGrid)
        {
            if(!BoardManager.Instance.CanPlacePiece(playerID, targetGrid))
            {
                CancelDragging();
                return;
            }

            //dataSize에서 말은 홀수 좌표
            if (targetGrid.x % 2 == 0 || targetGrid.y % 2 == 0) return;

            BoardManager.Instance.UpdatePieceData(startGrid, targetGrid);
            isDragging = false;
            transform.position = BoardManager.Instance.GridToWorld(targetGrid);

            //게임 승리 판정
            GameManager.Instance.IsEnd(playerID, targetGrid);

            //색 원상복구
            if (SpriteRenderer)
            {
                SpriteRenderer.color = Color.white;
            }

            //위치값 업데이트
            GameManager.Instance.players[id].currentGridPos = targetGrid;

            GameManager.Instance.selectedPiece = null;
            GameManager.Instance.EndTurn();
            Debug.Log($"말 설치 : x({targetGrid.x}), y({targetGrid.y})");
        }
    }
}

