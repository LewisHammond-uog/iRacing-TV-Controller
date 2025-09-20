using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace iRacingTVController;

public class CustomClassSystem
{
	public class CarClass
	{
		public string ClassName;
		public List<string> CarNums = new();
		public Unity.Color Colour;
		public float RelativeSpeed;
	}

	private HashSet<CarClass> allClasses;
	private Dictionary<string , CarClass> carsToClasses;
	private Dictionary<string, CarClass> nameToClasses;
		
	private Dictionary<CarClass, List<string>> classLeaderboards;

	public static CustomClassSystem Instance;
	
	
	public CustomClassSystem(string classCSV, string classToColourCSV)
	{
		Instance = this;
		carsToClasses = new Dictionary<string, CarClass>();
		allClasses =  new HashSet<CarClass>();
		nameToClasses = new Dictionary<string, CarClass>(StringComparer.OrdinalIgnoreCase);
		LoadClassesToColours(classToColourCSV);
		LoadCars(classCSV);

		classLeaderboards = new Dictionary<CarClass, List<string>>();
		foreach (CarClass carclass in allClasses)
		{
			classLeaderboards.Add(carclass, new List<string>(carclass.CarNums.Count));
		}
	}

	public void Update(in List<NormalizedCar> sortedLeaderboardClass)
	{
		foreach (NormalizedCar car in sortedLeaderboardClass)
		{
			if (car == null || car.includeInLeaderboard == false)
			{
				continue;
			}
			
			string carNumber = car.carNumber;
			if (!carsToClasses.ContainsKey(carNumber))
			{
				continue;
			}
			
			
			var carClass = carsToClasses[carNumber];
			classLeaderboards[carClass].Add(carNumber);
		}
	}

	public HashSet<CarClass> GetClasses()
	{
		return allClasses;
	}

	public int GetClassCount()
	{
		return allClasses.Count;
	}

	public CarClass? GetClassForCar(string carNum)
	{
		if (!carsToClasses.ContainsKey(carNum))
		{
			return null;
		}
		
		return carsToClasses[carNum];
	}

	public CarClass GetClassForCar(NormalizedCar car)
	{
		return GetClassForCar(car.carNumber);
	}
	
	public bool IsCarInClass(CarClass carClass, string carNum)
	{
		return carClass.CarNums.Contains(carNum);
	}

	public List<string> GetClassLeaderBoard(CarClass carClass)
	{
		return classLeaderboards[carClass];
	}

	public List<NormalizedCar> GetClassLeaderBoardAsNormalisedCar(CarClass carClass)
	{
		List<NormalizedCar> carsLeaderboard = new List<NormalizedCar>(classLeaderboards[carClass].Count);

		foreach (var car in classLeaderboards[carClass])
		{
			NormalizedCar? thisNormCar = null;
			foreach (NormalizedCar normCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars)
			{
				if (normCar.carNumber == car)
				{
					thisNormCar = normCar;
					break;
				}
			}

			if (thisNormCar == null)
			{
				throw new InvalidOperationException("Failed to find car for class leaderboard");
			}
			
			carsLeaderboard.Add(thisNormCar);
		}

		return carsLeaderboard;
	}

	public Unity.Color GetColourForClass(CarClass carClass)
	{
		return carClass.Colour;
	}

	public int GetPositionInClass(NormalizedCar car)
	{
		string carNum = car.carNumber;
		var carClass = carsToClasses[carNum];
		var leaderboard = classLeaderboards[carClass];
		return leaderboard.IndexOf(carNum) + 1;
	}
	
	//Output functions
	//Get normalized car from number STRING

	private void AddCarToClass(string carNum, string className)
	{
		if (nameToClasses.TryGetValue(className, out var carClass))
		{
			AddCarToClass(carNum, carClass);
		}
		else
		{
			throw new ArgumentException("Class " + className + " not found");
		}
	}

	private void AddCarToClass(string carNum, CarClass carClass)
	{
		carClass.CarNums.Add(carNum);
		carsToClasses.Add(carNum, carClass);
	}

	private void CreateClass(string name, Unity.Color colour)
	{
		var newClass = new CarClass()
		{
			ClassName = name,
			Colour = colour,
			CarNums = new List<string>()
		};
		
		allClasses.Add(newClass);
		nameToClasses.Add(name, newClass);
	}

	private void LoadClassesToColours(string classToColourCSV)
	{
		var records = CustomClassCsvLoader.LoadClassesToColours(classToColourCSV);

		foreach (var record in records)
		{
			CreateClass(record.ClassName, new Unity.Color(record.Hex));
		}
	}


	private void LoadCars(string classCSV)
	{
		var records = CustomClassCsvLoader.LoadCars(classCSV);

		foreach (var record in records)
		{
			AddCarToClass(record.CarNumber, record.ClassName);
		}
	}
}