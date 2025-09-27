using System.Collections.Generic;
using Aydsko.iRacingData.Leagues;

namespace iRacingTVController;

public partial class CustomPointsSystem
{
	private CustomClassResults resultsSystem;
	private CustomClassSystem classSystem;

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

	private Dictionary<NormalizedCar, CarPoints> carToPoints;
	
	public CustomPointsSystem(CustomClassResults resultsSystem, CustomClassSystem classSystem)
	{
		this.resultsSystem = resultsSystem;
		this.classSystem = classSystem;

		carToPoints = new Dictionary<NormalizedCar, CarPoints>();
	}

	public void UpdateFromRaceResults()
	{
		carToPoints.Clear();
		
		var allClasses = classSystem.GetClasses();
		(float time, NormalizedCar? car) fastestLap = (float.MaxValue, null);
		foreach (CustomClassSystem.CarClass carClass in allClasses)
		{
			var classResults = resultsSystem.GetClassResults(carClass);

			int currentPosForPoints = 1; //not the same as pos because DNF, DNQ do not count for points. Start at 1 for 1st
			
			foreach (CustomClassResults.ResultCar car in classResults)
			{
				if(resultsSystem.GetOutReason(car.normCar) != CustomClassResults.OutReason.Running) 
					continue;
				
				int basePoints = PointsAllocation.GetPointsForPosition(currentPosForPoints, car.normCar.carClass);
				CarPoints pts = new CarPoints(basePoints);

				pts.zeroXPoints = car.posCar.Incidents == 0 ? PointsAllocation.zeroIncidentsBounusPts : 0;

				if (car.posCar.FastestLap < fastestLap.time)
				{
					fastestLap = (car.posCar.FastestLap, car.normCar);
				}
				
				carToPoints.Add(car.normCar, pts);
				currentPosForPoints++;
			}
		}

		if (fastestLap.car != null)
		{
			carToPoints[fastestLap.car].flPoints = PointsAllocation.fastestLapBounusPts;
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