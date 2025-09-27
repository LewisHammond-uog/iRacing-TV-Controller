using System.Collections.Generic;

namespace iRacingTVController;

public partial class CustomPointsSystem
{
	private static class PointsAllocation
	{
		private static readonly Dictionary<string, List<int>> pointsDictonary = new Dictionary<string, List<int>>()
		{
			{"Pro", [32, 25, 22, 19, 17, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1]},
			{"Pro-Am", [32, 25, 22, 19, 17, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1]},
			{"Am", [32, 25, 22, 19, 17, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1]},
			{"Default", [32, 25, 22, 19, 17, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1]},
		};

		public static int fastestLapBounusPts => 1;
		public static int zeroIncidentsBounusPts => 1;

		public static int GetPointsForPosition(int pos, CustomClassSystem.CarClass carClass)
		{
			if (!pointsDictonary.TryGetValue(carClass.ClassName, out List<int>? points))
			{
				return 0;
			}

			int index = pos - 1;

			if (points == null || index >= points.Count || index < 0)
				return 0;

			return points[index];
		}
	}
}
