using System;
using System.Linq;
using irsdkSharp.Serialization.Enums.Fastest;

namespace iRacingTVController;

public partial class LiveData
{
	public void UpdateRaceResult()
	{
		// figure out how many race result pages we have

		raceResultPageCount = 0;

		var classes = CustomClassSystem.Instance.GetClasses().ToList();
		for (var i = 0; i < classes.Count; i++)
		{
			raceResultPageCount += 1 + ((classes[i].CarNums.Count - 1) / Settings.overlay.raceResultSlotCount);
		}

		// don't show race result until it is time

		if (!forceShowRaceResult)
		{
			liveDataRaceResult.show = false;
			return;
			if (!IRSDK.normalizedSession.isInRaceSession ||
			    (IRSDK.normalizedData.sessionState < SessionState.StateCoolDown))
			{
				raceResultCurrentPage = 0;
				raceResultTimer = 0;

				liveDataRaceResult.show = false;

				return;
			}

			// run the timer

			raceResultTimer += (float) IRSDK.normalizedData.sessionTimeDelta;

			// calculate total duration of race results (0 if manual)

			var totalDuration = raceResultPageCount * Settings.overlay.raceResultInterval;

			// is it time to show the first page?

			if (raceResultTimer < Settings.overlay.raceResultStartTime)
			{
				liveDataRaceResult.show = false;

				return;
			}

			// are we done showing the race results?

			var timeOffset = raceResultTimer - Settings.overlay.raceResultStartTime;

			if ((totalDuration > 0) && (timeOffset > totalDuration))
			{
				liveDataRaceResult.show = false;

				return;
			}

			// figure out which page we are on

			if (totalDuration > 0)
			{
				raceResultCurrentPage = (int) Math.Floor(timeOffset / Settings.overlay.raceResultInterval);
			}
		}

		// set up page metadata

		var pageClassIndex = new int[raceResultPageCount];
		var pageSlotIndex = new int[raceResultPageCount];

		var slotIndex = 0;
		var pageIndex = 0;

		for (var i = 0; i < CustomClassSystem.Instance.GetClassCount(); i++)
		{
			slotIndex = 0;

			var pagesThisClass = 1 + ((classes[i].CarNums.Count - 1) / Settings.overlay.raceResultSlotCount);

			for (var j = 0; j < pagesThisClass; j++)
			{
				pageClassIndex[pageIndex] = i;
				pageSlotIndex[pageIndex] = slotIndex;

				slotIndex += Settings.overlay.raceResultSlotCount;

				pageIndex++;
			}
		}

		// build the race result

		Unity.Color color;

		CustomClassSystem.CarClass raceResultClass = classes[pageClassIndex[raceResultCurrentPage]];

		liveDataRaceResult.show = true;

		liveDataRaceResult.backgroundSize =
			Settings.overlay.raceResultSlotSpacing * Settings.overlay.raceResultSlotCount;
		liveDataRaceResult.classColor = raceResultClass.Colour;
		liveDataRaceResult.textLayer1 = GetTextContent(out color, "RaceResultTextLayer1", null, raceResultClass);
		liveDataRaceResult.textLayer2 = GetTextContent(out color, "RaceResultTextLayer2", null, raceResultClass);

		// clear out the race result slots

		for (var i = 0; i < liveDataRaceResult.liveDataRaceResultSlots.Length; i++)
		{
			var liveDataRaceResultSlot = liveDataRaceResult.liveDataRaceResultSlots[i];

			liveDataRaceResultSlot.show = false;
		}

		// build the race result slotss

		slotIndex = 0;

		var numSlots = Math.Min(Settings.overlay.raceResultSlotCount,
			(raceResultClass.CarNums.Count - pageSlotIndex[raceResultCurrentPage]));

		if (IRSDK.normalizedSession.sessionNumber < 0)
		{
			return;
		}

		if (IRSDK.session.SessionInfo.Sessions[IRSDK.normalizedSession.sessionNumber].ResultsPositions == null)
		{
			return;
		}

		foreach (var posCar in IRSDK.session.SessionInfo.Sessions[IRSDK.normalizedSession.sessionNumber]
			         .ResultsPositions)
		{
			var normalizedCar = IRSDK.normalizedData.normalizedCars.First(nc => nc.carIdx == posCar.CarIdx);

			if (!normalizedCar.includeInLeaderboard)
			{
				continue;
			}

			if (normalizedCar.classID != raceResultClass.ClassName)
			{
				continue;
			}

			if (slotIndex >= pageSlotIndex[raceResultCurrentPage])
			{
				var liveDataRaceResultSlot = liveDataRaceResult.liveDataRaceResultSlots[normalizedCar.carIdx];

				liveDataRaceResultSlot.show = true;

				liveDataRaceResultSlot.showPreferredCar = false;
				liveDataRaceResultSlot.offset =
					new Unity.Vector2(Settings.overlay.raceResultSlotSpacing.x,
						-Settings.overlay.raceResultSlotSpacing.y) *
					(slotIndex - pageSlotIndex[raceResultCurrentPage]) + new Unity.Vector2(
						Settings.overlay.raceResultFirstSlotPosition.x,
						-Settings.overlay.raceResultFirstSlotPosition.y);
				liveDataRaceResultSlot.textLayer1 = GetTextContent(out liveDataRaceResultSlot.textLayer1Color,
					"RaceResultPositionTextLayer1", normalizedCar, normalizedCar.carClass);
				liveDataRaceResultSlot.textLayer2 = GetTextContent(out liveDataRaceResultSlot.textLayer2Color,
					"RaceResultPositionTextLayer2", normalizedCar, normalizedCar.carClass);
				liveDataRaceResultSlot.textLayer3 = GetTextContent(out liveDataRaceResultSlot.textLayer3Color,
					"RaceResultPositionTextLayer3", normalizedCar, normalizedCar.carClass);
				liveDataRaceResultSlot.textLayer4 = GetTextContent(out liveDataRaceResultSlot.textLayer4Color,
					"RaceResultPositionTextLayer4", normalizedCar, normalizedCar.carClass);
				liveDataRaceResultSlot.textLayer5 = GetTextContent(out liveDataRaceResultSlot.textLayer5Color,
					"RaceResultPositionTextLayer5", normalizedCar, normalizedCar.carClass);
				liveDataRaceResultSlot.textLayer6 = GetTextContent(out liveDataRaceResultSlot.textLayer6Color,
					"RaceResultPositionTextLayer6", normalizedCar, normalizedCar.carClass);

				numSlots--;

				if (numSlots == 0)
				{
					break;
				}
			}

			slotIndex++;
		}
	}
}