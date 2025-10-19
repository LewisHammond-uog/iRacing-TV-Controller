using System;
using StreamDeckCommunicator;

namespace iRacingTVController;

public partial class LiveData
{
	private void SendLiveSessionReplayEvent(bool prevLiveSessionReplay)
	{
		if (prevLiveSessionReplay != isLiveSessionReplay)
		{
			var send = ServerMessagePipe.Instance!.SendMessageAsync(Events.IsInReplay);
			send.Wait(20);
		}
	}

	private void UpdateLapTimeComparision()
	{
		var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx(IRSDK.normalizedData.camCarIdx);
		if (normalizedCar == null)
		{
			return;
		}

		string FormName(NormalizedCar? car)
		{
			return car == null ? "" : $"{car.familyName} #{car.carNumber}";
		}

		string GetTimeDiff(NormalizedCar? me, NormalizedCar? comparision, int lapNum)
		{
			if (me == null || comparision == null)
			{
				return "NO LAP";
			}

			float meLap = me.GetTimeOnExactLap(lapNum);
			float compLap = comparision.GetTimeOnExactLap(lapNum);

			if (meLap <= 0 || compLap <= 0)
			{
				return "NO LAP";
			}

			float diff = meLap - compLap;

			string pmString = diff > 0 ? "+" : "-";
			string timeString = Program.GetTimeString(MathF.Abs(diff), true);

			return $"{pmString}{timeString}";
		}

		NormalizedCar? GetCarInFrontForPosition(NormalizedCar me)
		{
			var inFrontRoad = me.normalizedCarInFront;
			while (inFrontRoad != null && inFrontRoad != me)
			{
				if (inFrontRoad.overallPosition < me.overallPosition)
				{
					return inFrontRoad;
				}

				inFrontRoad = inFrontRoad.normalizedCarInFront;
			}

			return null;
		}

		NormalizedCar? GetCarBehindForPosition(NormalizedCar me)
		{
			var behindRoad = me.normalizedCarBehind;
			while (behindRoad != null && behindRoad != me)
			{
				if (behindRoad.overallPosition > me.overallPosition)
				{
					return behindRoad;
				}

				behindRoad = behindRoad.normalizedCarBehind;
			}

			return null;
		}

		liveDataLapComp.Clear();

		//TODO Find for position (overall) comparisions

		var inFront = GetCarInFrontForPosition(normalizedCar);
		var behind = GetCarBehindForPosition(normalizedCar);

		liveDataLapComp.aheadCarIdX = inFront?.carIdx ?? -1;
		liveDataLapComp.aheadName = FormName(inFront);

		liveDataLapComp.behindCarIdX = behind?.carIdx ?? -1;
		liveDataLapComp.behindName = FormName(behind);

		liveDataLapComp.currentIdX = normalizedCar?.carIdx ?? -1;
		liveDataLapComp.currentName = FormName(normalizedCar);

		for (int i = 0; i < LiveDataLapComp.historyCount; i++)
		{
			int lap = normalizedCar.lapCompletedLastFrame - i;

			float thisLapTime = normalizedCar.GetTimeOnExactLap(lap);

			liveDataLapComp.carBehindLastLapsDiff[i] =
				GetTimeDiff(normalizedCar, normalizedCar.normalizedCarBehind, lap);
			liveDataLapComp.carAheadLastLapsDiff[i] =
				GetTimeDiff(normalizedCar, normalizedCar.normalizedCarInFront, lap);
			liveDataLapComp.thisCarLaps[i] = thisLapTime > 0 ? Program.GetTimeString(thisLapTime, true) : "";

			if (lap >= 1)
			{
				liveDataLapComp.lapNums[i] = $"LAP {lap}";
			}
			else
			{
				liveDataLapComp.lapNums[i] = "";
			}
		}
	}
}