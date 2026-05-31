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

        private bool isDragging = false;

        private void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (!isDragging) return;

            Vector2Int snapGrid = BoardManager.Instance.CurrentMouseGrid;
            bool canPlace = BoardManager.Instance.CanMovePieceTo(playerID, snapGrid);

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
        }

        private void CancelDragging()
        {
            isDragging = false;
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
                originalPos = transform.position;
                startGrid = BoardManager.Instance.WorldToGrid(originalPos);

                GameManager.Instance.selectedPiece = this;
                //Debug.Log($"말 클릭: {startGrid}");
            }
        }

        public void OnPiecePlace(Vector2Int targetPos)
        {
            if(!BoardManager.Instance.CanMovePieceTo(playerID, targetPos))
            {
                CancelDragging();
                return;
            }

            //dataSize에서 말은 홀수 좌표
            if (targetPos.x % 2 == 0 || targetPos.y % 2 == 0) return;
            
            BoardManager.Instance.MovePieceTo(playerID, targetPos);

            isDragging = false;
            transform.position = BoardManager.Instance.GridToWorld(targetPos);

            if (SpriteRenderer)
            {
                SpriteRenderer.color = Color.white;
            }
        }
    }
}

