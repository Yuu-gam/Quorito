using System;
using NUnit.Framework;
using UnityEngine;

namespace Script
{
	[Serializable]
	public class WallData
	{
		public char pieceChar;
		
		public Vector2Int[] occupiedOffsets; // 벽이 보드에서 차지하는 좌표

		// Rotate occupiedOffsets by 90 degrees "amount" times
		public void Rotate(int amount)
		{
			for (int t = 0; t < amount; t++) {
				for (int i = 0; i < occupiedOffsets.Length; i++)
				{
					int x = occupiedOffsets[i].x;
					int y = occupiedOffsets[i].y;
					occupiedOffsets[i] = new Vector2Int(y, -x);
				}
			}
		}
		
		public WallData(char pieceChar)
		{
			this.pieceChar = pieceChar;
			switch (pieceChar)
			{
				case 'F':
					occupiedOffsets = new Vector2Int[] { new(-1, 0), new(0, -1), new(0, 1), new(1, 2) };
					break;
				case 'I':
					occupiedOffsets = new Vector2Int[] { new(0, 1), new(0, 3), new(0, -1), new(0, -3) };
					break;
				case 'L':
					occupiedOffsets = new Vector2Int[] { new(1, 0), new(0, 1), new(0, 3), new(0, 5) };
					break;
				case 'N':
					occupiedOffsets = new Vector2Int[] { new(0, -1), new(0, -3), new(1, 0), new(2, 1) };
					break;
				case 'P':
					occupiedOffsets = new Vector2Int[] { new(1, 0), new(1, 2), new(0, 1), new(0, -1), new(2, 1) };
					break;
				case 'T':
					occupiedOffsets = new Vector2Int[] { new(0, -1), new(0, 1), new(-1, 2), new(1, 2) };
					break;
				case 'U':
					occupiedOffsets = new Vector2Int[] { new(-1, 0), new(-2, 1), new(1, 0), new(2, 1) };
					break;
				case 'V':
					occupiedOffsets = new Vector2Int[] { new(1, 0), new(3, 0), new(0, 1), new(0, 3) };
					break;
				case 'W':
					occupiedOffsets = new Vector2Int[] { new(-1, 0), new(-2, 1), new(0, -1), new(1, -2) };
					break;
				case 'X':
					occupiedOffsets = new Vector2Int[] { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };
					break;
				case 'Y':
					occupiedOffsets = new Vector2Int[] { new(-1, 0), new(0, 1), new(0, -1), new(0, -3) };
					break;
				case 'Z':
					occupiedOffsets = new Vector2Int[] { new(-1, 2), new(0, 1), new(0, -1), new(1, -2) };
					break;
				default:
					Assert.Fail($"Unknown piece char '{pieceChar}' during WallData construction");
					break;
			}
		}
	}
}