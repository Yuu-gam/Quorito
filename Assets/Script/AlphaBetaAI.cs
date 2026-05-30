using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Script
{
    public class MoveData
    {
        public Vector2Int TargetPosition;
    }

    public class PieceMoveData : MoveData
    {
        public Vector2Int OriginalPosition;

        public PieceMoveData(Vector2Int originalPosition, Vector2Int targetPosition)
        {
            Assert.IsTrue(targetPosition.x % 2 == 1 || targetPosition.y % 2 == 1);

            TargetPosition = targetPosition;
            OriginalPosition = originalPosition;
        }
    }

    public class WallMoveData : MoveData
    {
        public WallData WallData;

        public WallMoveData(WallData wallData, Vector2Int targetPosition)
        {
            Assert.IsTrue(targetPosition.x % 2 == 0 || targetPosition.y % 2 == 0);
            
            TargetPosition = targetPosition;
            WallData = wallData;
        }
    }

    public class AlphaBetaAI : MonoBehaviour
    {
        const int MaxDepth = 5; // 탐색 깊이

        private List<Vector2Int> _optimalPath0 = new(); // 플레이어 0의 최적 경로
        private List<Vector2Int> _optimalPath1 = new(); // 플레이어 1의 최적 경로

        private GridData _grid; // 계산용 로컬 그리드
        
        private Stack<MoveData> _moveStack = new();
        
        // flip: 5 - current
        private const int Zero = 0;
        private const int Up = 1;
        private const int Left = 2;
        private const int Right = 3;
        private const int Down = 4;
        private const int Start = 5;
        
        private readonly Vector2Int[] Dirs =
        {
            Vector2Int.zero, Vector2Int.up * 2, Vector2Int.left * 2,
            Vector2Int.right * 2, Vector2Int.down * 2, Vector2Int.one,
        };
        
        // Return: path length
        int BFS(int playerID, out List<Vector2Int> path)
        {
            GridData grid = BoardManager.Instance.grid;

            short[,] cameFrom = new short[GridData.DataSize, GridData.DataSize];
            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            Vector2Int currentPos = GameManager.Instance.players[playerID].currentGridPos;
            queue.Enqueue(currentPos);
            cameFrom[currentPos.x, currentPos.y] = Start;
            
            // BFS copied from BoardManager.CanReachGoal
            while (queue.Count > 0)
            {
                currentPos = queue.Dequeue();

                if (currentPos.y == GameManager.Instance.players[playerID].targetY)
                    break; //목적지 도착

                //상좌우하 탐색
                for (short i = 1; i <= 4; i++)
                {
                    Vector2Int dir = Dirs[i];
                    Vector2Int next = currentPos + dir; //이동 경로
                    Vector2Int wall = currentPos + (dir / 2); //이동 경로의 벽 좌표

                    if (next.x < 0 || next.x >= GridData.DataSize || next.y < 0 || next.y >= GridData.DataSize) continue;
                    if (cameFrom[next.x, next.y] != Zero) continue;
                    if (grid.Content[wall.x, wall.y] == GridData.CellType.Wall) continue;
                    cameFrom[next.x, next.y] = (short)(5 - i);
                    queue.Enqueue(next);
                }
            }
            
            // Reconstruct path
            path = new List<Vector2Int>();
            while (cameFrom[currentPos.x, currentPos.y] != Start)
            {
                path.Add(currentPos);
                currentPos += Dirs[cameFrom[currentPos.x, currentPos.y]];
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
            
            if (maximizingPlayer == 0)
            {
                int maxEval = int.MinValue;
                foreach (var move in GetPossibleMoves(0))
                {
                    MakeMove(move);
                    int eval = AlphaBeta(alpha, beta, 0, depth - 1);
                    UndoMove();
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
                foreach (var move in GetPossibleMoves(1))
                {
                    MakeMove(move);
                    int eval = AlphaBeta(alpha, beta, 1, depth - 1);
                    UndoMove();
                    minEval = Mathf.Min(minEval, eval);
                    beta = Mathf.Min(beta, eval);
                    if (beta <= alpha)
                        break; // 알파 컷오프
                }
                return minEval;
            }
        }

        private void MakeMove(MoveData move)
        {
            _moveStack.Push(move);

            if (move is PieceMoveData pieceMove)
            {
                _grid.MovePieceData(pieceMove.OriginalPosition, pieceMove.TargetPosition);
            } 
            else if (move is WallMoveData wallMove)
            {
                _grid.PlaceWallData(wallMove.WallData, wallMove.TargetPosition);
            }
        }
        
        private void UndoMove()
        {
            MoveData move = _moveStack.Pop();

            if (move is PieceMoveData pieceMove)
            {
                _grid.MovePieceData(pieceMove.TargetPosition, pieceMove.OriginalPosition);
            }
            else if (move is WallMoveData wallMove)
            {
                _grid.RemoveWall(wallMove.WallData, wallMove.TargetPosition);
            }
        }

        private IEnumerable<MoveData> GetPossibleMoves(int playerID)
        {
            List<MoveData> moves = new();

            // Get possible piece move
            Vector2Int[] targetPosCandidates =
            {
                Dirs[Up], Dirs[Down], Dirs[Left], Dirs[Right],
                Dirs[Up] * 2, Dirs[Down] * 2, Dirs[Left] * 2, Dirs[Right] * 2,
                new(2, 2), new(2, -2), new(-2, 2), new(-2, -2)
            };

            var currentPos = GameManager.Instance.players[playerID].currentGridPos;
            
            foreach (var pos in targetPosCandidates)
            {
                if (_grid.CanMovePieceTo(playerID, pos))
                    moves.Add(new PieceMoveData(currentPos, pos));
            }

            Dictionary<char, short> uniqueRotations = new()
            {
                { 'F', 4 },
                { 'I', 2 },
                { 'L', 4 },
                { 'N', 4 },
                { 'P', 4 },
                { 'T', 4 },
                { 'U', 4 },
                { 'V', 4 },
                { 'W', 4 },
                { 'X', 1 },
                { 'Y', 4 },
                { 'Z', 2 }
            };
            
            // Get possible wall placements
            foreach (var wall in _grid.unplacedWalls)
            {
                for (int y = 0; y < GridData.DataSize; y += 2)
                {
                    for (int x = 0; x < GridData.DataSize; x += 2)
                    {
                        //_grid.CanPlaceWall();
                    }
                }
            }
            
            return moves;
        }
    }
}