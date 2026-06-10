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
}