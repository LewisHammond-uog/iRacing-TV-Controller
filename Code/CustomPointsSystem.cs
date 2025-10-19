using System.Collections.Generic;
using Aydsko.iRacingData.Leagues;

namespace iRacingTVController;

public partial class CustomPointsSystem(CustomClassResults resultsSystem, CustomClassSystem classSystem)
{
	public class CarPoints
	{
		private readonly int basePoints;
		internal int flPoints = 0;
		internal int zeroXPoints = 0;
		
		public bool hasFastestLap => flPoints > 0;
		public bool hasZeroX => zeroXPoints > 0;
		public int TotalPoints => basePoints + flPoints + zeroXPoints;

		public CarPoints(int basePoints)
		{
			this.basePoints = basePoints;
		}

		public static implicit operator CarPoints(int pts)
		{
			return new CarPoints(pts);
		}
	}

	private Dictionary<NormalizedCar, CarPoints> carToPoints = new Dictionary<NormalizedCar, CarPoints>();


	public void UpdateFromRaceResults()
	{
		carToPoints.Clear();
		
		var allClasses = classSystem.GetClasses();
		foreach (CustomClassSystem.CarClass carClass in allClasses)
		{
			(float time, NormalizedCar? car) fastestLap = (float.MaxValue, null);
			var classResults = resultsSystem.GetClassResults(carClass);
			
			if(classResults == null)
			{
				continue;
			}

			int currentPosForPoints = 1; //not the same as pos because DNF, DNQ do not count for points. Start at 1 for 1st
			
			foreach (CustomClassResults.ResultCar car in classResults)
			{
				if(resultsSystem.GetOutReason(car.normCar) != CustomClassResults.OutReason.Running) 
					continue;
				
				int basePoints = PointsAllocation.GetPointsForPosition(currentPosForPoints, car.normCar.carClass);
				CarPoints pts = new CarPoints(basePoints);

				pts.zeroXPoints = car.posCar.Incidents == 0 ? PointsAllocation.zeroIncidentsBounusPts : 0;

				if (car.posCar.FastestTime < fastestLap.time && car.posCar.FastestTime > 0)
				{
					fastestLap = (car.posCar.FastestTime, car.normCar);
				}
				
				carToPoints.Add(car.normCar, pts);
				currentPosForPoints++;
			}
			
			if (fastestLap.car != null)
			{
				carToPoints[fastestLap.car].flPoints = PointsAllocation.fastestLapBounusPts;
			}
		}


	}

	public CarPoints GetPointsForCar(NormalizedCar car)
	{
		if (!carToPoints.TryGetValue(car, out CarPoints? pts))
		{
			return 0;
		}

		return pts ?? 0;
	}
}