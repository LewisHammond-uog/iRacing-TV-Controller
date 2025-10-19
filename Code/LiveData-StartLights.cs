using irsdkSharp.Serialization.Enums.Fastest;

namespace iRacingTVController;

public partial class LiveData
{
	public void UpdateStartLights()
	{
		liveDataStartLights.showReady = false;
		liveDataStartLights.showSet = false;
		liveDataStartLights.showGo = false;

		if ((IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.StartGo) != 0)
		{
			liveDataStartLights.showGo = true;
		}
		else if ((IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.StartSet) != 0)
		{
			liveDataStartLights.showSet = true;
		}
		else if ((IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.StartReady) != 0)
		{
			if ((IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.OneLapToGreen) != 0)
			{
				if ((IRSDK.normalizedData.paceCar == null) || (IRSDK.normalizedData.paceCar.isOnPitRoad &&
				                                               (IRSDK.normalizedData.paceCar.lapDistPct > 0.5f)))
				{
					liveDataStartLights.showReady = true;
				}
			}
			else if ((IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.StartHidden) == 0)
			{
				liveDataStartLights.showReady = true;
			}
		}
	}
}