using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace iRacingTVController;

[Serializable]
public class LiveDataLapComp
{
	[XmlIgnore, NonSerialized] public const int historyCount = 5;
	
	[JsonInclude] public string[] carAheadLastLapsDiff = new string[historyCount];
	[JsonInclude] public string[] carBehindLastLapsDiff= new string[historyCount];
	[JsonInclude] public string[] thisCarLaps= new string[historyCount];
	[JsonInclude] public string[] lapNums = new string[historyCount];
	
	[JsonInclude] public int aheadCarIdX = 0;
	[JsonInclude] public string aheadName = String.Empty;

	[JsonInclude] public int currentIdX = 0;
	[JsonInclude] public string currentName = string.Empty;

	[JsonInclude] public int behindCarIdX = 0;
	[JsonInclude] public string behindName = String.Empty;
	

	public void Clear()
	{
		carAheadLastLapsDiff = new string[historyCount];
		carBehindLastLapsDiff = new string[historyCount];
		thisCarLaps = new string[historyCount];
	}
}