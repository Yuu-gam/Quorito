using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace Script
{
    public abstract class MoveData
    {
        public Vector2Int TargetPosition;

        public abstract MoveData Clone();
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

        public override MoveData Clone()
        {
            return new PieceMoveData(OriginalPosition, TargetPosition);
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

        public override MoveData Clone()
        {
            return new WallMoveData(WallData, TargetPosition);
        }
    }

    public class AlphaBetaAI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] public int maxDepth = 3;
        [SerializeField] private int evalLimit = 10000;
        
        [Header("Debug")]
        [SerializeField] private bool enableLogging;
        [SerializeField] private bool logEvalResults;
        [SerializeField] private bool logBestMove;
        [SerializeField] private bool writeScores;
        
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
        
        private static readonly Vector2Int[] Dirs =
        {
            Vector2Int.zero, Vector2Int.up * 2, Vector2Int.left * 2,
            Vector2Int.right * 2, Vector2Int.down * 2, Vector2Int.one,
        };

        private static readonly Vector2Int[] MoveDirs = {
            Dirs[Up], Dirs[Down], Dirs[Left], Dirs[Right],
            Dirs[Up] * 2, Dirs[Down] * 2, Dirs[Left] * 2, Dirs[Right] * 2,
            new(2, 2), new(2, -2), new(-2, 2), new(-2, -2)
        }; 
        
        private int _evaluatedMoves;
        private List<int> _scores = new();
        
        // Return: path length
        private int BFS(int pieceID, out List<Vector2Int> path)
        {
            short[] cameFrom = new short[GridData.DataSize * GridData.DataSize];
            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            Vector2Int currentPos = _grid.PiecePositions[pieceID];
            queue.Enqueue(currentPos);
            cameFrom[currentPos.x + currentPos.y * GridData.DataSize] = Start;
            
            // BFS copied from BoardManager.CanReachGoal
            while (queue.Count > 0)
            {
                currentPos = queue.Dequeue();

                if (currentPos.y == GridData.TargetYs[pieceID])
                    break; //목적지

                //상좌우하 탐색
                for (short i = 1; i <= 4; i++)
                {
                    Vector2Int dir = Dirs[i];
                    Vector2Int next = currentPos + dir; //이동 경로
                    Vector2Int wall = currentPos + (dir / 2); //이동 경로의 벽 좌표

                    if (next.x < 0 || next.x >= GridData.DataSize || next.y < 0 || next.y >= GridData.DataSize) continue;
                    if (cameFrom[next.x + next.y * GridData.DataSize] != Zero) continue;
                    if (_grid.content[wall.x + wall.y * GridData.DataSize] == GridData.CellType.Wall) continue;
                    cameFrom[next.x + next.y * GridData.DataSize] = (short)(5 - i);
                    queue.Enqueue(next);
                }
            }
            
            // Reconstruct path
            path = new List<Vector2Int>();
            while (cameFrom[currentPos.x + currentPos.y * GridData.DataSize] != Start)
            {
                path.Add(currentPos);
                currentPos += Dirs[cameFrom[currentPos.x + currentPos.y * GridData.DataSize]];
            }
            
            return path.Count;
        }

        public int EvaluateBoard()
        {
            // 현재 보드 상태를 평가하여 점수 반환
            int dist0 = BFS(0, out _);
            int dist1 = BFS(1, out _);
            int score = dist1 - dist0; // 플레이어 0이 유리할수록 점수가 높음

            _evaluatedMoves++;
            if (writeScores) _scores.Add(score);
            return score;
        }
        
        
        public void LogResult(int eval, MoveData bestMove)
        {
            if (!enableLogging) return;
            
            if (logEvalResults)
                Debug.Log($"Minimax ended with {_evaluatedMoves} evaluated moves\neval: {eval}");

            if (logBestMove)
            {
                switch (bestMove)
                {
                    case PieceMoveData pieceMove:
                        Debug.Log($"Move player at ({pieceMove.OriginalPosition}) to ({pieceMove.TargetPosition})");
                        break;
                    case WallMoveData wallMove:
                        Debug.Log($"Place wall '{wallMove.WallData.pieceChar}-{wallMove.WallData.Rotation}' at  ({wallMove.TargetPosition})");
                        break;
                    case null:
                        Debug.Log("Error: bestMove is null");
                        break;
                    default:
                        Debug.Log($"Error: Invalid move");
                        break;
                }
            }

            if (writeScores)
            {
                if (!Directory.Exists("Assets/Log")) //Log폴더 확인 후 생성
                {
                    Directory.CreateDirectory("Assets/Log");
                }

                var sw = new StreamWriter("Assets/Log/scores.txt");
                sw.WriteLine(string.Join(", ", _scores));
                sw.Flush();
                sw.Close();
            }
        }

        public struct AlphaBetaResult
        {
            public int Eval;
            public MoveData BestMove;

            public AlphaBetaResult(int eval)
            {
                Eval = eval;
                BestMove = null;
            }

            public AlphaBetaResult(int eval, MoveData bestMove)
            {
                Eval = eval;
                BestMove = bestMove;
            }
        }

        public int AlphaBeta(int alpha, int beta, int maximizingPlayer, int depth, out MoveData bestMove)
        {
            bestMove = null;
            
            if (depth == maxDepth)
            {
                _grid = BoardManager.Instance.grid.Clone();
                _evaluatedMoves = 0;
                _scores.Clear();
            }
            
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
                    int eval = AlphaBeta(alpha, beta, 1, depth - 1, out _);
                    UndoMove();
                    if (eval > maxEval)
                    {
                        maxEval = eval;
                        bestMove = move;
                    }
                    alpha = Mathf.Max(alpha, eval);
                    if (beta <= alpha)
                        break; // 베타 컷오프
                }
                return maxEval;
            }

            int minEval = int.MaxValue;
            foreach (var move in GetPossibleMoves(1))
            {
                MakeMove(move);
                int eval = AlphaBeta(alpha, beta, 0, depth - 1, out _);
                UndoMove();
                if (eval < minEval)
                {
                    minEval = eval;
                    bestMove = move;
                }
                beta = Mathf.Min(beta, eval);
                if (beta <= alpha)
                    break; // 알파 컷오프
            }
            return minEval;
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
            

            var currentPos = _grid.PiecePositions[playerID];
            
            foreach (var offset in MoveDirs)
            {
                var targetPos = currentPos + offset;
                if (_grid.CanMovePieceTo(playerID, targetPos))
                    moves.Add(new PieceMoveData(currentPos, targetPos));
            }

            List<WallData> wallCandidates = new(41);

            foreach (var wallChar in _grid.unplacedWalls)
            {
                switch (wallChar)
                {
                    case (byte)'X':
                        wallCandidates.Add(new WallData(wallChar));
                        break;
                    case (byte)'I': case (byte)'Z':
                        wallCandidates.Add(new WallData(wallChar));
                        wallCandidates.Add(new WallData(wallChar, 1));
                        break;
                    default:
                        wallCandidates.Add(new WallData(wallChar));
                        wallCandidates.Add(new WallData(wallChar, 1));
                        wallCandidates.Add(new WallData(wallChar, 2));
                        wallCandidates.Add(new WallData(wallChar, 3));
                        break;
                }
            }
            
            // Get possible wall placements
            foreach (WallData wallData in wallCandidates)
            {
                for (int y = 0; y < GridData.DataSize; y += 2)
                {
                    for (int x = 0; x < GridData.DataSize; x += 2)
                    {
                        if (_grid.CanPlaceWall(wallData, new Vector2Int(x, y)))
                            moves.Add(new WallMoveData(wallData,  new Vector2Int(x, y)));
                        
                        if (moves.Count > evalLimit && evalLimit > 0)
                            return moves;
                    }
                }
            }
            
            return moves;
        }
    }
}