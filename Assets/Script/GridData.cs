using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Script
{
    [Serializable]
    public class GridData
    {
        public const int GridSize = 10; // 칸 수
        /**
         * 벽 포함 실제 데이터 배열 크기
         *  짝수: 벽 / 홀수: 기물
         */
        public const int DataSize = 21;

        public enum CellType { Empty, Wall, Piece }
        
        public CellType[,] Content;

        public List<char> unplacedWalls = new() { 'F', 'I', 'L', 'N', 'P', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        public List<char> placedWalls = new();

        public static readonly int[] TargetYs = { 19, 1 };

        [NonSerialized]
        public Vector2Int[] PiecePositions = { new(9, 1), new(11, 19) };

        public GridData()
        {
            ResetGrid();
        }

        public GridData Clone()
        {
            using MemoryStream ms = new();
            BinaryFormatter formatter = new();
            
            formatter.Serialize(ms, this);
            ms.Position = 0;
            var ret = (GridData)formatter.Deserialize(ms);
            ms.Close();

            ret.PiecePositions = PiecePositions.Clone() as Vector2Int[];
            return ret;
        }

        public void ResetGrid()
        {
            Content = new CellType[DataSize, DataSize];
            
            for (int i = 0; i < DataSize; i++)
            {
                for (int j = 0; j < DataSize; j++)
                {
                    Content[i, j] = CellType.Empty;
                }
            }
        }
        
        public bool CanPlaceWall(WallData wallData, Vector2Int basePos)
        {
            foreach(var offset in wallData.OccupiedOffsets)
            {
                Vector2Int target = basePos + offset;

                // 보드 범위 초과
                if(target.x < 0 || target.x >= DataSize || target.y < 0 || target.y >= DataSize)
                {
                    //Debug.Log("보드 범위 초과");
                    return false;
                }

                // 이미 벽이 있으면 설치 불가
                if (Content[target.x, target.y] == CellType.Wall)
                {
                    //Debug.Log("이미 벽이 있음");
                    return false;
                }
            }
            
            return IsNotBlockingGoal(wallData, basePos); //길을 모두 막는 것은 불가능
        }
        
        public void PlaceWallData(WallData wallData, Vector2Int basePos)
        {
            foreach(var offset in wallData.OccupiedOffsets)
            {
                int targetX = basePos.x + offset.x;
                int targetY = basePos.y + offset.y;

                if(0 <= targetX && targetX < DataSize &&
                   0 <= targetY && targetY < DataSize)
                {
                    Content[targetX, targetY] = CellType.Wall;
                    //Debug.Log($"Place wall at [{targetX}, {targetY}]");
                }            
            }

            unplacedWalls.Remove(wallData.pieceChar);
            placedWalls.Add(wallData.pieceChar);
        }
        
        
        //길을 막는지 판단
        public bool IsNotBlockingGoal(WallData wallToTest, Vector2Int basePos)
        {
            var offsets = wallToTest.OccupiedOffsets;
            foreach (var offset in offsets)
            {
                Content[basePos.x + offset.x, basePos.y + offset.y] = CellType.Wall;
            }

            bool p0CanGo = CanReachGoal(0);
            bool p1CanGo = CanReachGoal(1);

            foreach(var offset in offsets)
            {
                Content[basePos.x + offset.x, basePos.y + offset.y] = CellType.Empty;
            }

            //Debug.Log($"p1: {p0CanGo}, p2: {p1CanGo}");

            return p0CanGo && p1CanGo; 
        }
        
        
        //BFS로 길 탐색
        public bool CanReachGoal(int pieceID)
        {
            Vector2Int startPos = PiecePositions[pieceID];
            int targetY = TargetYs[pieceID];
                
            bool[] visited = new bool[DataSize * DataSize];
            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            queue.Enqueue(startPos);
            visited[startPos.x + startPos.y * DataSize] = true;

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                if (current.y == targetY) return true; //목적지 도착

                //상하좌우 탐색
                Vector2Int[] dirs = { Vector2Int.up * 2, Vector2Int.down * 2, Vector2Int.left * 2, Vector2Int.right * 2 };
                foreach (var dir in dirs)
                {
                    Vector2Int next = current + dir; //이동 경로
                    Vector2Int wall = current + (dir / 2); //이동 경로의 벽 좌표

                    if (next.x >= 0 && next.x < DataSize && next.y >= 0 && next.y < DataSize)
                    {
                        if (!visited[next.x + next.y * DataSize] && Content[wall.x, wall.y] != CellType.Wall)
                        {
                            visited[next.x + next.y * DataSize] = true;
                            queue.Enqueue(next);
                        }
                    }
                }
            }
            return false;
        }
        

        public void RemoveWall(WallData wallData, Vector2Int basePos)
        {
            if (unplacedWalls.Contains(wallData.pieceChar)) return;
            
            foreach(var offset in wallData.OccupiedOffsets)
            {
                int targetX = basePos.x + offset.x;
                int targetY = basePos.y + offset.y;

                if(0 <= targetX && targetX < DataSize &&
                   0 <= targetY && targetY < DataSize)
                {
                    Content[targetX, targetY] = CellType.Empty;
                    //Debug.Log($"Remove wall at [{targetX}, {targetY}]");
                }            
            }
            
            unplacedWalls.Add(wallData.pieceChar);
            placedWalls.Remove(wallData.pieceChar);
        }
        
        
        // Returns true if out of bounds or wall is present
        public bool IsWall(Vector2Int pos)
        {
            if (pos.x < 0 || pos.y < 0 || pos.x >= DataSize || pos.y >= DataSize) return true;

            return Content[pos.x, pos.y] == CellType.Wall;
        }
        
        
        public bool IsPiece(Vector2Int pos)
        {
            return PiecePositions.Contains(pos);
        }
        

        public bool CanMovePieceTo(int pieceID, Vector2Int targetPos)
        {
            Vector2Int startPos = PiecePositions[pieceID];

            int dx = targetPos.x - startPos.x;
            int dy = targetPos.y - startPos.y;
            int absDx = Mathf.Abs(dx);
            int absDy = Mathf.Abs(dy);


            //보드 범위 초과
            if (targetPos.x < 0 || targetPos.y < 0 ||
                targetPos.x >= DataSize || targetPos.y >= DataSize) return false;

            //Debug.Log($"Start: {startPos}, Target: {targetPos}, dx: {dx}, dy: {dy}");


            //목표에 말이 있다면 이동 불가
            if (Content[targetPos.x, targetPos.y] == CellType.Piece)
            {
                //Debug.Log("경로에 이미 말이 있음");
                return false;
            }


            // 상하좌우 한 칸 이동
            if (absDx + absDy == 2 && absDx * absDy == 0)
            {
                Vector2Int mid = (startPos + targetPos) / 2;

                return !IsWall(mid);
            }


            // 상하좌우 두 칸 이동
            if (absDx == 4 || absDy == 4)
            {
                Vector2Int opponentPos = (startPos + targetPos) / 2;

                if (Content[opponentPos.x, opponentPos.y] == CellType.Piece)
                {
                    Vector2Int wall1 = (startPos + opponentPos) / 2; //나와 적 사이의 벽
                    Vector2Int wall2 = (targetPos + opponentPos) / 2; //적과 도착 지점 사이의 벽

                    if (!IsWall(wall1) && !IsWall(wall2))
                    {
                        return true;
                    }
                }
            }


            // 대각선 이동 (적 뒤에 벽이 있을 경우)
            if (absDx == 2 && absDy == 2)
            {
                //가로의 적
                Vector2Int opponentSideX = new Vector2Int(startPos.x + dx, startPos.y);

                if (IsPiece(opponentSideX))
                {
                    Vector2Int wallBehindOpponent = new Vector2Int(opponentSideX.x + dx, opponentSideX.y);
                    Vector2Int wallBetween = new Vector2Int(startPos.x + dx / 2, startPos.y);
                    Vector2Int wallToTarget = new Vector2Int(opponentSideX.x, opponentSideX.y + dy / 2);

                    if (IsWall(wallBehindOpponent)
                        && !IsWall(wallBetween)
                        && !IsWall(wallToTarget))
                    {
                        return true;
                    }
                }

                //세로의 적
                Vector2Int opponentSideY = new Vector2Int(startPos.x, startPos.y + dy);

                if (IsPiece(opponentSideY))
                {
                    Vector2Int wallBehindOpponent = new Vector2Int(opponentSideY.x, opponentSideY.y + dy / 2);
                    Vector2Int wallBetween = new Vector2Int(startPos.x, startPos.y + dy / 2);
                    Vector2Int wallToTarget = new Vector2Int(opponentSideY.x + dx / 2, opponentSideY.y);

                    if (IsWall(wallBehindOpponent)
                        && !IsWall(wallBetween)
                        && !IsWall(wallToTarget))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        
        public void MovePieceData(int pieceID, Vector2Int targetPos)
        {
            Content[PiecePositions[pieceID].x, PiecePositions[pieceID].y] = CellType.Empty;
            Content[targetPos.x, targetPos.y] = CellType.Piece;
            
            PiecePositions[pieceID] = targetPos;
        }
        
        
        public void MovePieceData(Vector2Int originalPos, Vector2Int targetPos)
        {
            int pieceID = PiecePositions[0] == originalPos ? 0 : 1;
            
            MovePieceData(pieceID, targetPos);
        }
    }
}
