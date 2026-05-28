using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Script
{
    public class Move
    {
        public enum MoveType
        {
            MovePiece,
            PlaceWall,
        }

        public MoveType Type;
        public Vector2Int Position;

        public Move(MoveType type, Vector2Int position)
        {
            if (type == MoveType.MovePiece)
            {
                Assert.IsTrue(position.x % 2 == 1 || position.y % 2 == 1);
            }
            else
            {
                Assert.IsTrue(position.x % 2 == 0 || position.y % 2 == 0);
            }
            
            Type = type;
            Position = position;
        }
    }

    public class AlphaBetaAI : MonoBehaviour
    {
        const int MaxDepth = 5; // 탐색 깊이

        private List<Vector2Int> _optimalPath0 = new(); // 플레이어 0의 최적 경로
        private List<Vector2Int> _optimalPath1 = new(); // 플레이어 1의 최적 경로

        private GridData _grid; // 계산용 로컬 그리드
        
        private char[] _availableWalls = {'F', 'I', 'L', 'N', 'P', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
        
        // Return: path length
        int BFS(int playerID, out List<Vector2Int> path)
        {
            GridData grid = BoardManager.Instance.grid;

            // flip: 5 - current
            const int zero = 0;
            const int up = 1;
            const int left = 2;
            const int right = 3;
            const int down = 4;
            const int start = 5;
        
            Vector2Int[] dirs =
            {
                Vector2Int.zero, Vector2Int.up * 2, Vector2Int.left * 2,
                Vector2Int.right * 2, Vector2Int.down * 2, Vector2Int.one,
            };

            short[,] cameFrom = new short[GridData.DataSize, GridData.DataSize];
            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            Vector2Int currentPos = GameManager.Instance.players[playerID].currentGridPos;
            queue.Enqueue(currentPos);
            cameFrom[currentPos.x, currentPos.y] = start;
            
            // BFS copied from BoardManager.CanReachGoal
            while (queue.Count > 0)
            {
                currentPos = queue.Dequeue();

                if (currentPos.y == GameManager.Instance.players[playerID].targetY)
                    break; //목적지 도착

                //상좌우하 탐색
                for (short i = 1; i <= 4; i++)
                {
                    Vector2Int dir = dirs[i];
                    Vector2Int next = currentPos + dir; //이동 경로
                    Vector2Int wall = currentPos + (dir / 2); //이동 경로의 벽 좌표

                    if (next.x < 0 || next.x >= GridData.DataSize || next.y < 0 || next.y >= GridData.DataSize) continue;
                    if (cameFrom[next.x, next.y] != zero) continue;
                    if (grid.Content[wall.x, wall.y] == GridData.CellType.Wall) continue;
                    cameFrom[next.x, next.y] = (short)(5 - i);
                    queue.Enqueue(next);
                }
            }
            
            // Path reconstruction
            path = new List<Vector2Int>();
            while (cameFrom[currentPos.x, currentPos.y] != start)
            {
                path.Add(currentPos);
                currentPos += dirs[cameFrom[currentPos.x, currentPos.y]];
            }
            
            return path.Count;
        }

        int EvaluateBoard()
        {
            // 현재 보드 상태를 평가하여 점수 반환
            int dist0 = BFS(0, out _);
            int dist1 = BFS(1, out _);
            int score = dist1 - dist0; // 플레이어 0이 유리할수록 점수가 높음
            
            return score;
        }

        int AlphaBeta(int alpha, int beta, int maximizingPlayer, int depth = MaxDepth)
        {
            if (depth == MaxDepth)
                _grid = BoardManager.Instance.grid.Clone();
            
            if (depth == 0)
            {
                return EvaluateBoard(); // 현재 보드 상태 평가
            }
            
            if (maximizingPlayer == 1)
            {
                int maxEval = int.MinValue;
                foreach (var move in GetPossibleMoves())
                {
                    MakeMove(_grid, move);
                    int eval = AlphaBeta(alpha, beta, 0, depth - 1);
                    UndoMove(_grid);
                    maxEval = Mathf.Max(maxEval, eval);
                    alpha = Mathf.Max(alpha, eval);
                    if (beta <= alpha)
                        break; // 베타 컷오프
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (var move in GetPossibleMoves())
                {
                    MakeMove(_grid, move);
                    int eval = AlphaBeta(alpha, beta, 1, depth - 1);
                    UndoMove(_grid);
                    minEval = Mathf.Min(minEval, eval);
                    beta = Mathf.Min(beta, eval);
                    if (beta <= alpha)
                        break; // 알파 컷오프
                }
                return minEval;
            }
        }

        private void MakeMove(GridData grid, Move move)
        {
            throw new NotImplementedException();
        }
        
        private void UndoMove(GridData grid)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<Move> GetPossibleMoves()
        {
            List<Move> moves = new();
            
            // Get possible piece move
            
            
            // Get possible wall placements
            
            
            return moves;
        }
    }
}