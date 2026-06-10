using UnityEngine;

namespace Script
{
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance;

        private float ObjectWidth => GetComponent<Renderer>().bounds.size.x; //실제 보드 크기
    
        public GridData grid; //해당 좌표에 무엇이 있는지 판단

        [HideInInspector]
        public float gridSize; //세계 좌표 기준 격자 한 칸 크기

        [HideInInspector]
        public Vector2 boardStartPos; //보드 왼쪽 아래 꼭짓점 좌표

        public Vector2Int CurrentMouseGrid { get; private set; } //현재 마우스 좌표는 BoardManager에서만 계산할 것

        private Camera _camera;


        void Awake()
        {
            Instance = this;
            
            grid = new GridData();
            gridSize = ObjectWidth / (GridData.DataSize - 1);
            float halfBoard = ObjectWidth / 2f;
            boardStartPos = new Vector2(-halfBoard, -halfBoard);
        }

        private void Start()
        {
            _camera = Camera.main;
            ResetBoard();
        }

        public void ResetBoard()
        {
            foreach(var player in GameManager.Instance.players)
            {
                grid.content[player.currentGridPos.x + player.currentGridPos.y * GridData.DataSize] = GridData.CellType.Piece;
            }
        }


        private void Update()
        {
            int id = GameManager.Instance.CurrentTurnID;
            PlayerPiece player = GameManager.Instance.players[id];

            Vector2 mousePos = GetMousePos();

            //홀수 좌표로 변환
            float rawX = (mousePos.x - boardStartPos.x) / gridSize;
            float rawY = (mousePos.y - boardStartPos.y) / gridSize;

            CurrentMouseGrid = new Vector2Int(
                Mathf.FloorToInt(rawX / 2f) * 2 + 1,
                Mathf.FloorToInt(rawY / 2f) * 2 + 1);


            if (Input.GetMouseButtonDown(0))
            {

                if (CurrentMouseGrid == player.currentGridPos && player.controllable)
                {
                    player.PickUp();
                    GameManager.Instance.selectedPiece = player;

                    return;
                }
                
                GameManager.Instance.selectedPiece?.OnPiecePlace(CurrentMouseGrid);
            }
        }

        private Vector2 GetMousePos()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = 10f;
            return _camera.ScreenToWorldPoint(mousePoint);
        }

        //좌표를 격자 번호로 변환
        public Vector2Int WorldToGrid(Vector2 worldPos)
        {
            int x = Mathf.RoundToInt((worldPos.x - boardStartPos.x) / gridSize);
            int y = Mathf.RoundToInt((worldPos.y - boardStartPos.y) / gridSize);

            x = Mathf.Clamp(x, 0, GridData.DataSize - 1);
            y = Mathf.Clamp(y, 0, GridData.DataSize - 1);

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
        public bool CanMovePieceTo(int pieceID, Vector2Int targetPos)
        {
            return grid.CanMovePieceTo(pieceID, targetPos);
        }
        
        public void MovePieceTo(int pieceID, Vector2Int targetPos)
        {
            //dataSize에서 말은 홀수 좌표
            if (targetPos.x % 2 == 0 || targetPos.y % 2 == 0) return;

            grid.MovePieceData(pieceID, targetPos);

            //게임 승리 판정
            if (targetPos.y == GameManager.Instance.players[pieceID].targetY)
            {
                GameManager.Instance.OnGoalReached(pieceID, targetPos);
            }

            //위치값 업데이트
            GameManager.Instance.players[pieceID].currentGridPos = targetPos;

            GameManager.Instance.selectedPiece = null;
            _ = GameManager.Instance.EndTurn();
            //Debug.Log($"말 설치 : x({targetGrid.x}), y({targetGrid.y})");
        }


        //벽을 설치할 수 있는지 판단
        public bool CanPlaceWall(WallData wallData, Vector2Int basePos)
        {
            if (GameManager.Instance.CurrentTurnID != 0)
            {
                return false;
            }
            return grid.CanPlaceWall(wallData, basePos);
        }


        public WallPiece FindWallPiece(byte wallChar)
        {
            var wallPieces = FindObjectsByType<WallPiece>(FindObjectsSortMode.None);
            foreach (var wall in wallPieces)
            {
                if (wall.wallData.pieceChar == wallChar)  return wall;
            }
            return null;
        }
    }
}
