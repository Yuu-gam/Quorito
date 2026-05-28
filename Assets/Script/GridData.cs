using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Script
{
    [Serializable]
    public class GridData
    {
        public const int GridSize = 10; // 칸 수
        /**
         * 벽 포함 실제 데이터 배열 크기
         * 홀수: 기물 / 짝수: 벽
         */
        public const int DataSize = 21;

        public enum CellType { Empty, Wall, Piece }
        public CellType[,] Content;

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
    }
}
