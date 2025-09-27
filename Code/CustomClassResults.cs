using System;
using System.Collections.Generic;
using System.Linq;
using irsdkSharp.Serialization.Models.Session.SessionInfo;

namespace iRacingTVController;

public class CustomClassResults
{
	private IReadOnlyCollection<PositionModel>? cachedResults;
	private List<PositionModel>? sortedResults;
	
	private CustomClassSystem classSystem;
	private CustomPointsSystem pointsSystem;

	public enum OutReason : int
	{
		Running = 0,
		Disconnected = 32,
		Disqualified = 29,
		
		Unknown = int.MaxValue
	}

	internal struct ResultCar
	{
		public required int classPosition;
		public required NormalizedCar normCar;
		public required PositionModel posCar;
	}

	private Dictionary<CustomClassSystem.CarClass, List<ResultCar>>? classResults;
	
	internal CustomClassResults(CustomClassSystem classes)
	{
		classSystem = classes;
		pointsSystem = new CustomPointsSystem(this, classSystem);
	}
	
	public void UpdateFromPositionModel(in IReadOnlyCollection<PositionModel> results)
	{
		if (Equals(cachedResults, results))
		{
			return;
		}

		sortedResults = new List<PositionModel>(results);
		sortedResults.Sort(SortResults);

		classResults = new Dictionary<CustomClassSystem.CarClass, List<ResultCar>>(classSystem.GetClassCount());

		foreach (PositionModel positionModel in sortedResults)
		{
			var normCar = IRSDK.normalizedData.normalizedCars.FirstOrDefault(n => n.carIdx == positionModel.CarIdx);
			if (normCar == null)
			{
				Console.WriteLine("MISS MATCH BETWEEN CARS!!!!");
				continue;
			}

			CustomClassSystem.CarClass? thisClass = classSystem.GetClassForCar(normCar);
			if (thisClass == null)
			{
				Console.WriteLine("NO CLASS!!!!");
				continue;
			}

			if (!classResults.TryGetValue(thisClass, out List<ResultCar>? cars))
			{
				cars = new List<ResultCar>(thisClass.CarNums.Count);
				classResults.Add(thisClass, cars);
			}

			int myPosInClass = cars.Count + 1;
			ResultCar resultCar = new ResultCar()
			{
				classPosition = myPosInClass,
				normCar = normCar,
				posCar = positionModel
			};

			cars.Add(resultCar);
		}
		
		pointsSystem.UpdateFromRaceResults();
	}

	public bool IsClassLeader(NormalizedCar car)
	{
		if (!GetResultsForClass(car, out ResultCar? resultCar, out List<ResultCar>? cars) || cars == null || resultCar == null)
			return false;

		return resultCar.Value.normCar == cars[0].normCar;
	}

	public int? GetCarClassPosition(NormalizedCar car)
	{
		return !TryGetResultCarFromNormalizedCar(car, out ResultCar? resultCar) ? null : resultCar?.classPosition;
	}

	public OutReason GetOutReason(NormalizedCar car)
	{
		if (!TryGetResultCarFromNormalizedCar(car, out ResultCar? resultCar) || resultCar == null)
		{
			return 0;
		}

		return (OutReason)resultCar.Value.posCar.ReasonOutId;
	}

	public int? GetCarLapsBehindClassLeader(NormalizedCar car)
	{
		if (!GetResultsForClass(car, out ResultCar? resultCar, out List<ResultCar>? cars) || cars == null || resultCar == null)
			return null;

		var leader = cars[0];

		return leader.posCar.LapsComplete - resultCar.Value.posCar.LapsComplete;
	}

	public float? GetCarTimeBehindClassLeader(NormalizedCar car)
	{
		if (!GetResultsForClass(car, out ResultCar? resultCar, out List<ResultCar>? cars) || cars == null || resultCar == null)
			return null;
		
		var leader = cars[0];
		
		return resultCar.Value.posCar.Time - leader.posCar.Time;
	}

	internal List<ResultCar>? GetClassResults(CustomClassSystem.CarClass carClass)
	{
		if (classResults == null || !classResults.TryGetValue(carClass, out List<ResultCar>? results))
		{
			return null;
		}
		
		return results;
	}

	public CustomPointsSystem.CarPoints GetPointsForCar(NormalizedCar car)
	{
		return pointsSystem.GetPointsForCar(car);
	}

	private bool TryGetResultCarFromNormalizedCar(NormalizedCar car, out ResultCar? resultCar)
	{
		if (!GetResultsForClass(car, out resultCar, out List<ResultCar>? cars) || cars == null) 
			return false;

		foreach (ResultCar rc in cars)
		{
			if (rc.normCar == car)
			{
				resultCar = rc;
				return true;
			}
		}

		return false;
	}

	private bool GetResultsForClass(NormalizedCar car, out ResultCar? resultCar, out List<ResultCar>? allCarsInClass)
	{
		resultCar = null;
		var carClass = classSystem.GetClassForCar(car);
		
		if (carClass == null || classResults == null || !classResults.TryGetValue(carClass, out allCarsInClass) || allCarsInClass == null)
		{
			allCarsInClass = null;
			return false;
		}

		resultCar = allCarsInClass.Find(m => m.normCar == car);
		return resultCar != null;
	}

	private int SortResults(PositionModel x, PositionModel y)
	{
		return x.Position.CompareTo(y.Position);
	}
}