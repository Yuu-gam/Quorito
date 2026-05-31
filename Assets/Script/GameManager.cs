using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script
{
    public class GameManager: MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Player Data")]
        public PlayerPiece[] players = new PlayerPiece[2];

        public PlayerPiece selectedPiece;
        

        [Header("Game State")]
        public int turnCount = 0;
        public int currentTurnID => turnCount % 2;

        [SerializeField] private TMP_Text text;
        
        private bool hasActed = false; //행동 여부

        [Header("AI")]
        [SerializeField] private AlphaBetaAI ai;
        
        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if(currentTurnID == 0){
                text.text = $"Turn : {turnCount}\n<color=#FF6B6B>Player {currentTurnID+1}</color>";    
            }else{
                text.text = $"Turn : {turnCount}\n<color=#4CAF50>Player {currentTurnID+1}</color>";
            }
        }
        

        //턴 변경
        public void EndTurn()
        {
            turnCount++;
            hasActed = false;
            //Debug.Log($"턴 수:{turnCount}, 현재: Player{currentTurnID + 1}");
            
            // AI
            if (currentTurnID == 0) return;
            
            var eval = ai.AlphaBeta(int.MinValue, int.MaxValue, 1, ai.maxDepth, out var bestMove);

            if (bestMove is PieceMoveData pieceMove)
            {
                players[currentTurnID].OnPiecePlace(pieceMove.TargetPosition);
                //Debug.Log($"Move player at ({pieceMove.OriginalPosition}) to ({pieceMove.TargetPosition})"); 
            }
            else if (bestMove is WallMoveData wallMove)
            {
                var wallPiece = BoardManager.Instance.FindWallPiece(wallMove.WallData.pieceChar);
                if (wallPiece)
                {
                    wallPiece.wallData = wallMove.WallData;
                    wallPiece.PlaceWall(wallMove.TargetPosition);
                }
                //Debug.Log($"Place wall '{wallMove.WallData.pieceChar}-{wallMove.WallData.Rotation}' at  ({wallMove.TargetPosition})"); 
            }
            else
            {
                //Debug.Log("No playable move returned");
            }
            Debug.Log($"eval: {eval}");
        }


        //내 턴인지 확인
        public bool IsMyTurn(int ownerPlayerID)
        {
            if (hasActed) return false;

            return currentTurnID == ownerPlayerID;
        }
        

        //게임이 끝났는지 판단
        public void OnGoalReached(int id, Vector2Int currentGrid)
        {
            //Debug.Log($"게임 승리자: Player{id}");

            //승리자에 따라 다른 게임 결과 창(Scene) 로드
            if(id == 0)
            {
                P1Win();
            }
            else
            {
                P2Win();
            }
        }


        //게임 리셋 : 현재 화면을 재로드
        public void ResetGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        //플레이어 1 우승 결과 씬
        public void P1Win()
        {
            SceneManager.LoadScene("Player1Win");
        }
        
        //플레이어 2 우승 결과 씬
        public void P2Win()
        {
            SceneManager.LoadScene("Player2Win");
        }
    }
}
