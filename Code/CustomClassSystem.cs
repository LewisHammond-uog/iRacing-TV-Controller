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
	public static float rs = 0f;
	public class CarClass
	{
		public string ClassName;
		public List<string> CarNums = new();
		public Unity.Color Colour;
		public float RelativeSpeed = 0;
	}

	private HashSet<CarClass> allClasses;
	private Dictionary<string , CarClass> carsToClasses;
	private Dictionary<string, CarClass> nameToClasses;
		
	private Dictionary<CarClass, List<string>> classLeaderboards;

	private static CustomClassSystem? _instance;
	public static CustomClassSystem Instance
	{
		get
		{
			if (_instance == null)
				_instance = new CustomClassSystem(Program.documentsFolder + "data/cars.csv",
					Program.documentsFolder + "data/classes.csv");

			return _instance;
		}
	}

	private CarClass defaultClass;
	
	
	public CustomClassSystem(string classCSV, string classToColourCSV)
	{
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

		defaultClass = new CarClass()
		{
			ClassName = "Default",
			CarNums = new List<string>(),
			Colour = Unity.Color.white,
			RelativeSpeed = 0f,
		};
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

	public CarClass? GetClassForCar(NormalizedCar car)
	{
		return GetClassForCar(car.carNumber);
	}

	public CarClass? GetClassForCar(string carNum)
	{
		return carsToClasses.GetValueOrDefault(carNum, defaultClass);
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

	public Unity.Color GetColourForClass(string className)
	{
		if (string.IsNullOrEmpty(className))
		{
			return Unity.Color.white;
		}
		
		if (!nameToClasses.ContainsKey(className))
		{
			return Unity.Color.white;
		}
		
		var carClass = nameToClasses[className];


		return carClass.Colour;
	}

	public int GetPositionInClass(NormalizedCar car)
	{
		string carNum = car.carNumber;
		if (!carsToClasses.ContainsKey(carNum))
		{
			return 1;
		}
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

	private void CreateClass(string name, Unity.Color colour, int relativeSpeed)
	{
		float rs = 0f;
		if (name == "Pro")
		{
			rs = 1f;
		}else if (name == "Pro-Am")
		{
			rs = 0.5f;
		}else if (name == "Am")
		{
			rs = 0.1f;
		}
		
		
		
		
		var newClass = new CarClass()
		{
			ClassName = name,
			Colour = colour,
			CarNums = new List<string>(),
			RelativeSpeed = rs
		};
		
		allClasses.Add(newClass);
		nameToClasses.Add(name, newClass);
		
	}

	private void LoadClassesToColours(string classToColourCSV)
	{
		var records = CustomClassCsvLoader.LoadClassesToColours(classToColourCSV);

		foreach (var record in records)
		{
			CreateClass(record.ClassName, new Unity.Color(record.Hex),  (int)record.relativeSeepd);
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