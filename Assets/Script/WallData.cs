using System;
using NUnit.Framework;
using UnityEngine;

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
				Assert.NotNull(ret);
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
			switch (pieceChar)
			{
				case 'F':
					OccupiedOffsets = new Vector2Int[] { new(-1, 0), new(0, -1), new(0, 1), new(1, 2) };
					break;
				case 'I':
					OccupiedOffsets = new Vector2Int[] { new(0, 1), new(0, 3), new(0, -1), new(0, -3) };
					break;
				case 'L':
					OccupiedOffsets = new Vector2Int[] { new(1, 0), new(0, 1), new(0, 3), new(0, 5) };
					break;
				case 'N':
					OccupiedOffsets = new Vector2Int[] { new(0, -1), new(0, -3), new(1, 0), new(2, 1) };
					break;
				case 'P':
					OccupiedOffsets = new Vector2Int[] { new(1, 0), new(1, 2), new(0, 1), new(0, -1), new(2, 1) };
					break;
				case 'T':
					OccupiedOffsets = new Vector2Int[] { new(0, -1), new(0, 1), new(-1, 2), new(1, 2) };
					break;
				case 'U':
					OccupiedOffsets = new Vector2Int[] { new(-1, 0), new(-2, 1), new(1, 0), new(2, 1) };
					break;
				case 'V':
					OccupiedOffsets = new Vector2Int[] { new(1, 0), new(3, 0), new(0, 1), new(0, 3) };
					break;
				case 'W':
					OccupiedOffsets = new Vector2Int[] { new(-1, 0), new(-2, 1), new(0, -1), new(1, -2) };
					break;
				case 'X':
					OccupiedOffsets = new Vector2Int[] { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };
					break;
				case 'Y':
					OccupiedOffsets = new Vector2Int[] { new(-1, 0), new(0, 1), new(0, -1), new(0, -3) };
					break;
				case 'Z':
					OccupiedOffsets = new Vector2Int[] { new(-1, 2), new(0, 1), new(0, -1), new(1, -2) };
					break;
				default:
					Assert.Fail($"Unknown piece char '{pieceChar}' during WallData construction");
					break;
			}
			
			Rotate(rotation);
		}
	}
}