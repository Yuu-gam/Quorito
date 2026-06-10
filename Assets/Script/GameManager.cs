using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Script
{
    public class GameManager: MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Player Data")]
        public PlayerPiece[] players = new PlayerPiece[2];
        public PlayerPiece selectedPiece;


        private int _turnCount = 0;
        [Header("Game State")]
        public int TurnCount
        {
            get => _turnCount;
            set
            {
                _turnCount = value;
                UpdateStatusMessage();
            }
        }
        public int CurrentTurnID => TurnCount % 2;

        [SerializeField] private TMP_Text text;

        
        [Header("AI")]
        [SerializeField] private AlphaBetaAI ai;


        private bool _isCalculating;

        public bool IsCalculating
        {
            get => _isCalculating;
            set
            {
                _isCalculating = value;
                UpdateStatusMessage();
            }
        }
        private bool _isGameEnded = false;

        void Awake()
        {
            Instance = this;
            UpdateStatusMessage();
        }

        void UpdateStatusMessage()
        {
            string calculatingMessage = IsCalculating ? "(...)" : "";
            if(CurrentTurnID == 0){
                text.text = $"Turn : {TurnCount}\n<color=#FF6B6B>Player {CurrentTurnID+1} {calculatingMessage}</color>";  
            }else{
                text.text = $"Turn : {TurnCount}\n<color=#4CAF50>Player {CurrentTurnID+1} {calculatingMessage}</color>";
            }
        }

        //턴 변경
        public async Task EndTurn()
        {
            IsCalculating = true;
            
            TurnCount++;
            Debug.Log($"턴 수:{TurnCount}, 현재: Player{CurrentTurnID + 1}");
            
            // AI
            if (CurrentTurnID == 0) return;
            
            var bestMove = await CalculateBestMove();
            
            Assert.IsNotNull(bestMove);

            if (bestMove is PieceMoveData pieceMove)
            {
                players[CurrentTurnID].OnPiecePlace(pieceMove.TargetPosition);
            }
            else if (bestMove is WallMoveData wallMove)
            {
                var wallPiece = BoardManager.Instance.FindWallPiece(wallMove.WallData.pieceChar);
                if (wallPiece)
                {
                    wallPiece.wallData = wallMove.WallData;
                    wallPiece.PlaceWall(wallMove.TargetPosition);
                }
            }
            
            IsCalculating = false;
        }


        //게임이 끝났는지 판단
        public void OnGoalReached(int id, Vector2Int currentGrid)
        {
            if (_isGameEnded) return;
            _isGameEnded = true;
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


        //플레이어 1 우승 결과 씬
        private void P1Win()
        {
            SceneManager.LoadScene("Player1Win");
        }
        
        //플레이어 2 우승 결과 씬
        private void P2Win()
        {
            SceneManager.LoadScene("Player2Win");
        }
        
        private async Task<MoveData> CalculateBestMove()
        {
            MoveData bestMove = null;
            CancellationTokenSource cts = new();

            int eval = int.MinValue;
            await Task.Run(() =>
            {
                eval = ai.AlphaBeta(int.MinValue, int.MaxValue, 1, ai.maxDepth, out bestMove);
            }, cts.Token);
            
            ai.LogResult(eval, bestMove);
            
            return bestMove;
        }
    }
}
