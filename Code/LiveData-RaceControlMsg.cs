using System;
using System.Collections.Generic;
using irsdkSharp.Serialization.Enums.Fastest;

namespace iRacingTVController;

public partial class LiveData
{
	private Dictionary<NormalizedCar, double> carsOffTrack = new Dictionary<NormalizedCar, double>();

	private const float minSpdKphForYellow = 20f;
	private const double minForYellow = 2f;

	private int currentSessionIDLastFrame = -1;

	[Flags]
	public enum SectorFlag
	{
		Sector1 = 1,
		Sector2 = 1 << 1,
		Sector3 = 1 << 2,
	}
	
	
	public void UpdateYellowFlags()
	{
		if (IRSDK.normalizedSession.sessionNumber != currentSessionIDLastFrame)
		{
			carsOffTrack.Clear();
			currentSessionIDLastFrame = IRSDK.normalizedSession.sessionNumber;
		}

		liveDataRaceStatus.yellowSectors = 0;
		
		
		foreach (NormalizedCar car in IRSDK.normalizedData.normalizedCars)
		{
			if (!car.includeInLeaderboard)
				continue;
			
			
			int carIdx = car.carIdx;
			if (IRSDK.data == null) continue;
			TrackSurface surface = (TrackSurface)IRSDK.data.Cars[carIdx].CarIdxTrackSurface;
			bool isOffTrack = surface == TrackSurface.OffTrack 
			                  || (car.speedInKph < minSpdKphForYellow && IRSDK.normalizedData.sessionState == SessionState.StateRacing && surface is TrackSurface.OnTrack or TrackSurface.OffTrack);


			if (isOffTrack)
			{
				carsOffTrack.TryAdd(car, IRSDK.normalizedData.sessionTime);
            }
			else
			{
				carsOffTrack.Remove(car);
			}

		}

		if (carsOffTrack.Count == 0)
		{
			return;
		}

		bool[] yellowsThisUpdate = [false, false, false];
		foreach (KeyValuePair<NormalizedCar, double> carOffTrack in carsOffTrack)
		{
			int sector = carOffTrack.Key.currentFakeSector;
			bool timeInYellow =  MathF.Abs((float) (IRSDK.normalizedData.sessionTime - carOffTrack.Value)) >= minForYellow;
			Console.WriteLine($"Time for Yellow in {sector} :: {carOffTrack.Key.carNumber} :: {timeInYellow}s");

			if (timeInYellow)
			{
				yellowsThisUpdate[sector] = true;
				Console.WriteLine($"YELLOW in {sector}");
			}
		}
		
		var yFlags = (SectorFlag)0;
		if (yellowsThisUpdate[0])
		{
			yFlags |= SectorFlag.Sector1;
		}
		
		if (yellowsThisUpdate[1])
		{
			yFlags |= SectorFlag.Sector2;
		}
		
		if (yellowsThisUpdate[2])
		{
			yFlags |= SectorFlag.Sector3;
		}

		liveDataRaceStatus.showYellowLight = yFlags != 0;
		liveDataRaceStatus.yellowSectors = (int)yFlags;

	}
	
	
}