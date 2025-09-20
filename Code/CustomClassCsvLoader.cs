using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace iRacingTVController;

internal static class CustomClassCsvLoader
{
	// Private DTOs used only for CSV mapping
	private sealed class ClassToColourCsv
	{
		[Name("ClassName")] public string ClassName { get; set; }
		[Name("Colour")] public string Hex { get; set; }
	}

	private sealed class CarNumToClassCsv
	{
		[Name("Car Number")] public string CarNumber { get; set; }
		[Name("Driver Name")] public string DriverName { get; set; }
		[Name("Class Name")] public string ClassName { get; set; }
	}

	public static List<(string ClassName, string Hex)> LoadClassesToColours(string path)
	{
		var config = new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			PrepareHeaderForMatch = args => args.Header?.Trim(),
		};

		using var reader = new StreamReader(path);
		using var csv = new CsvReader(reader, config);
		return csv.GetRecords<ClassToColourCsv>()
		          .Select(r => (r.ClassName, r.Hex))
		          .ToList();
	}

	public static List<(string CarNumber, string DriverName, string ClassName)> LoadCars(string path)
	{
		using var reader = new StreamReader(path);
		using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
		return csv.GetRecords<CarNumToClassCsv>()
		          .Select(r => (r.CarNumber, r.DriverName, r.ClassName))
		          .ToList();
	}
}
