using UnityEngine;

namespace Script
{
    public class WallPiece : MonoBehaviour
    {
        //public int ownerPlayerID;

        private SpriteRenderer SpriteRenderer;

        [SerializeField] private char _wallChar;
        [HideInInspector] public WallData wallData;
        private bool isPlaced = false; //설치 상태
        private bool isDragging = false; //드래그 상태
        private bool justPicked = false; //방금 집었는지 확인
        private Vector3 originalPos;

        private void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            wallData = new WallData(_wallChar);
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
                //Debug.Log("벽 클릭");
            }
        }

        private void Update()
        {
            if (isPlaced || !isDragging) return;

            float cellSize = BoardManager.Instance.gridSize;
            //마우스 좌표
            Vector3 targetPos = Input.mousePosition;
            targetPos.z = 10f;
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(targetPos);
            worldMousePos += new Vector3(cellSize, cellSize, 0f);
            
            //짝수 좌표로 변환
            float rawX = (worldMousePos.x - BoardManager.Instance.boardStartPos.x) / cellSize;
            float rawY = (worldMousePos.y - BoardManager.Instance.boardStartPos.y) / cellSize;

            Vector2Int snapGrid = new Vector2Int(
                Mathf.FloorToInt(rawX / 2f) * 2,
                Mathf.FloorToInt(rawY / 2f) * 2
            );

            SetWorldPositionFromGrid(snapGrid);

            bool canPlace = BoardManager.Instance.CanPlaceWall(wallData, snapGrid);

            //설치 가능 여부에 따른 색 변경
            if (SpriteRenderer)
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
                wallData.Rotate(1);
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

                if (canPlace) PlaceWall(snapGrid);
                //else Debug.Log("설치 실패");
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                var segmentOffsets = wallData.OccupiedOffsets.Clone() as Vector2Int[];
                for (int i = 0; i < segmentOffsets.Length; i++)
                {
                    segmentOffsets[i] += snapGrid;
                }
                Debug.Log($"pos: {snapGrid}, rot: {wallData.Rotation}\nsegments: {string.Join(", ", segmentOffsets)}");
            }
        }

        private void CancelDragging()
        {
            isDragging = false;
            justPicked = false;
            transform.position = originalPos;
            transform.rotation = Quaternion.identity;
            wallData.Rotation = 0;

            //색 원상복구
            if (SpriteRenderer)
            {
                SpriteRenderer.color = Color.white;
            }

            //Debug.Log("벽 설치 취소");
        }

        public void PlaceWall(Vector2Int targetPos)
        {
            isPlaced = true;
            isDragging = false;

            //색 원상복구
            if (SpriteRenderer)
            {
                SpriteRenderer.color = Color.white;
            }
            
            SetWorldPositionFromGrid(targetPos);

            BoardManager.Instance.grid.PlaceWallData(wallData, targetPos);
            GameManager.Instance.EndTurn();
        }

        public void SetWorldPositionFromGrid(Vector2Int gridPos)
        {
            transform.position = BoardManager.Instance.GridToWorld(gridPos);
            transform.rotation = Quaternion.Euler(0, 0, -90f * wallData.Rotation);
        }
    }
}
