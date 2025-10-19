namespace iRacingTVController;

public partial class LiveData
{
	public void UpdatePitLane()
	{
		Unity.Color color;

		if (trackIdLastFrame != IRSDK.normalizedSession.trackID)
		{
			trackIdLastFrame = IRSDK.normalizedSession.trackID;

			pitLaneTouched = false;

			pitLaneMinLapDistPct = 0;
			pitLaneMaxLapDistPct = 0;
		}

		if (!pitLaneTouched)
		{
			foreach (var normalizedCar in IRSDK.normalizedData.normalizedCars)
			{
				if (normalizedCar.includeInLeaderboard && normalizedCar.isOnPitRoad)
				{
					pitLaneTouched = true;

					pitLaneMinLapDistPct = normalizedCar.lapDistPct;
					pitLaneMaxLapDistPct = normalizedCar.lapDistPct;

					break;
				}
			}
		}

		liveDataPitLane.show = false;

		if (pitLaneTouched)
		{
			foreach (var normalizedCar in IRSDK.normalizedData.normalizedCars)
			{
				if (normalizedCar.includeInLeaderboard && normalizedCar.isOnPitRoad)
				{
					liveDataPitLane.show = true;

					var lapDistPct = normalizedCar.lapDistPct;

					var deltaLapDistPct = lapDistPct - pitLaneMinLapDistPct;

					if (deltaLapDistPct <= -0.5)
					{
						lapDistPct += 1;
					}
					else if (deltaLapDistPct >= 0.5)
					{
						lapDistPct -= 1;
					}

					if (lapDistPct < pitLaneMinLapDistPct)
					{
						pitLaneMinLapDistPct = lapDistPct;
					}

					lapDistPct = normalizedCar.lapDistPct;

					deltaLapDistPct = lapDistPct - pitLaneMaxLapDistPct;

					if (deltaLapDistPct <= -0.5)
					{
						lapDistPct += 1;
					}
					else if (deltaLapDistPct >= 0.5)
					{
						lapDistPct -= 1;
					}

					if (lapDistPct > pitLaneMaxLapDistPct)
					{
						pitLaneMaxLapDistPct = lapDistPct;
					}
				}
			}

			var adjustedMaxLapDistPct = pitLaneMaxLapDistPct;

			if (adjustedMaxLapDistPct < pitLaneMinLapDistPct)
			{
				adjustedMaxLapDistPct += 1;
			}

			var length = adjustedMaxLapDistPct - pitLaneMinLapDistPct;

			foreach (var normalizedCar in IRSDK.normalizedData.normalizedCars)
			{
				var liveDataPitLaneCar = liveDataPitLane.liveDataPitLaneCars[normalizedCar.carIdx];

				if (normalizedCar.includeInLeaderboard && normalizedCar.isOnPitRoad && (length > 0))
				{
					liveDataPitLaneCar.show = true;
					liveDataPitLaneCar.showHighlight = (normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx);

					var lapDistPct = normalizedCar.lapDistPct;

					var deltaLapDistPct = lapDistPct - pitLaneMinLapDistPct;

					if (deltaLapDistPct <= -0.5)
					{
						lapDistPct += 1;
					}
					else if (deltaLapDistPct >= 0.5)
					{
						lapDistPct -= 1;
					}

					var offset = Settings.overlay.pitLaneLength * ((lapDistPct - pitLaneMinLapDistPct) / length);

					liveDataPitLaneCar.offset = new Unity.Vector3(offset, 0, 0);
					liveDataPitLaneCar.textLayer1 = GetTextContent(out color, "PitLaneCarTextLayer1", normalizedCar);
				}
				else
				{
					liveDataPitLaneCar.show = false;
				}
			}
		}
	}
}