using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Script
{
	[Serializable]
	public class WallData
	{
		public char pieceChar;
		[SerializeField] private Vector2Int[] _occupiedOffsets;
		public Vector2Int[] OccupiedOffsets // 벽이 보드에서 차지하는 좌표
		{
			get
			{
				var ret = _occupiedOffsets.Clone() as Vector2Int[];
				Assert.IsNotNull(ret);
				for (int t = 0; t < Rotation; t++) {
					for (int i = 0; i < _occupiedOffsets.Length; i++)
					{
						int x = ret[i].x;
						int y = ret[i].y;
						ret[i] = new Vector2Int(y, -x);
					}
				}
			
				return ret; 
			}
			set => _occupiedOffsets = value;
		}

		private static readonly Dictionary<char, Vector2Int[]> BaseOffsets = new()
		{
			{'F', new Vector2Int[] { new(-1, 0), new(0, -1), new(0, 1), new(1, 2) }},
			{'I', new Vector2Int[] { new(0, 1), new(0, 3), new(0, -1), new(0, -3) }},
			{'L', new Vector2Int[] { new(1, 0), new(0, 1), new(0, 3), new(0, 5) }},
			{'N', new Vector2Int[] { new(0, -1), new(0, -3), new(1, 0), new(2, 1) }},
			{'P', new Vector2Int[] { new(1, 0), new(1, 2), new(0, 1), new(0, -1), new(2, 1) }},
			{'T', new Vector2Int[] { new(0, -1), new(0, 1), new(-1, 2), new(1, 2) }},
			{'U', new Vector2Int[] { new(-1, 0), new(-2, 1), new(1, 0), new(2, 1) }},
			{'V', new Vector2Int[] { new(1, 0), new(3, 0), new(0, 1), new(0, 3) }},
			{'W', new Vector2Int[] { new(-1, 0), new(-2, 1), new(0, -1), new(1, -2) }},
			{'X', new Vector2Int[] { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) }},
			{'Y', new Vector2Int[] { new(-1, 0), new(0, 1), new(0, -1), new(0, -3) }},
			{'Z', new Vector2Int[] { new(-1, 2), new(0, 1), new(0, -1), new(1, -2) }}
		};
		
		private int _rotation;
		public int Rotation
		{
			get => _rotation;
			set => _rotation = value % 4;
		}

		// Rotate occupiedOffsets by 90 degrees "amount" times
		public void Rotate(int amount)
		{
			Rotation += amount;
		}
		
		public WallData(char pieceChar, int rotation = 0)
		{
			this.pieceChar = pieceChar;
			OccupiedOffsets = BaseOffsets[pieceChar].Clone() as Vector2Int[];
			
			Rotate(rotation);
		}
	}
}