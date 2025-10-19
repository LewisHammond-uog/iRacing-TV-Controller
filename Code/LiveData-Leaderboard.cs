using System;
using System.Linq;
using irsdkSharp.Serialization.Enums.Fastest;

namespace iRacingTVController;

public partial class LiveData
{
	public void UpdateLeaderboard(ref LiveDataLeaderboard[]? liveDataLeaderboards, bool splitLeaderboard)
	{
		//Make slot count be total, regarless of how many classes
		int slotCount = (int) Math.Floor((double) Settings.overlay.leaderboardSlotCount /
		                                 (double) IRSDK.normalizedData.numLeaderboardClasses);


		// save setting

		this.splitLeaderboard = splitLeaderboard;

		// allocate leaderboards

		if ((liveDataLeaderboards == null) ||
		    (liveDataLeaderboards.Length != IRSDK.normalizedData.numLeaderboardClasses))
		{
			liveDataLeaderboards = new LiveDataLeaderboard[IRSDK.normalizedData.numLeaderboardClasses];

			for (var leaderboardIndex = 0; leaderboardIndex < liveDataLeaderboards.Length; leaderboardIndex++)
			{
				liveDataLeaderboards[leaderboardIndex] = new LiveDataLeaderboard();
			}
		}

		var leaderboardOffset = Unity.Vector2.zero;

#if USE_CUSTOM_CLASSES
		if (UseCustomClassSystem)
		{
			classSystem.Update(IRSDK.normalizedData.leaderboardSortedNormalizedCars);
		}
#endif


		// go through each car class


		int numClasses = 0;
		if (UseCustomClassSystem)
		{
			numClasses = classSystem.GetClassCount();
		}
		else
		{
			numClasses = IRSDK.normalizedData.numLeaderboardClasses;
		}


		for (var classIndex = 0; classIndex < IRSDK.normalizedData.numLeaderboardClasses; classIndex++)
		{
			var currentLiveDataLeaderboard = liveDataLeaderboards[classIndex];
			var currentLeaderboardClass = IRSDK.normalizedData.leaderboardClass[classIndex];
			var currentClassID = currentLeaderboardClass.classID;

			// leaderboard splits

			var bottomSplitSlotCount = slotCount / 2;
			var bottomSplitLastSlotIndex = slotCount;

			if (!IRSDK.normalizedSession.isInQualifyingSession)
			{
				if (bottomSplitSlotCount > 0)
				{
					foreach (var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars)
					{
						if (!normalizedCar.includeInLeaderboard)
						{
							break;
						}

						if (!Settings.overlay.leaderboardSeparateBoards ||
						    (classSystem.GetClassForCar(normalizedCar)?.ClassName == currentClassID))
						{
							if (normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx)
							{
								if (normalizedCar.displayedPosition > bottomSplitLastSlotIndex)
								{
									while (bottomSplitLastSlotIndex < normalizedCar.displayedPosition)
									{
										bottomSplitLastSlotIndex += bottomSplitSlotCount;
									}

									if (bottomSplitLastSlotIndex > currentLeaderboardClass.numDrivers)
									{
										bottomSplitLastSlotIndex = currentLeaderboardClass.numDrivers;
									}

									break;
								}
							}
						}
					}
				}
			}

			var topSplitFirstSlotIndex = 1;
			var topSplitLastSlotIndex = slotCount - bottomSplitSlotCount;
			var bottomSplitFirstSlotIndex = bottomSplitLastSlotIndex - bottomSplitSlotCount + 1;

			if (!splitLeaderboard)
			{
				topSplitLastSlotIndex = MaxNumDrivers;
				bottomSplitFirstSlotIndex = MaxNumDrivers + 1;
				bottomSplitLastSlotIndex = MaxNumDrivers + 1;
			}

			// leaderboard

			Unity.Color color;

			var myClass = CustomClassSystem.Instance.GetClasses().FirstOrDefault(c => c.ClassName == currentClassID);

			currentLiveDataLeaderboard.show = false;
			currentLiveDataLeaderboard.classColor = classSystem.GetColourForClass(currentLeaderboardClass.name);
			currentLiveDataLeaderboard.textLayer1 = GetTextContent(out currentLiveDataLeaderboard.classColor,
				"LeaderboardTextLayer1", null, myClass);
			currentLiveDataLeaderboard.textLayer2 = GetTextContent(out currentLiveDataLeaderboard.classColor,
				"LeaderboardTextLayer2", null, myClass);

			normalizedCarClassLeader = null;
			normalizedCarInFront = null;
			classLeaderBestLapTime = 0.0f;

			var carsShown = 0;

			// reset leaderboard slots to be hidden

			foreach (var liveDataLeaderboardSlot in currentLiveDataLeaderboard.liveDataLeaderboardSlots)
			{
				liveDataLeaderboardSlot.show = false;
			}

			// go through cars for this class

			foreach (var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars)
			{
				// skip cars with wrong car class

				if (Settings.overlay.leaderboardSeparateBoards && (normalizedCar.classID != currentClassID))
				{
					continue;
				}

				// get slot

				LiveDataLeaderboardSlot liveDataLeaderboardSlot =
					currentLiveDataLeaderboard.liveDataLeaderboardSlots[normalizedCar.carIdx];

				// skip pace car and spectators

				if (normalizedCar.includeInLeaderboard)
				{
					// class leader best lap time

					if (normalizedCarClassLeader == null)
					{
						normalizedCarClassLeader = normalizedCar;

						classLeaderBestLapTime = normalizedCar.bestLapTime;
					}

					// check if the car is visible on the leaderboard

					liveDataLeaderboardSlot.show =
						(((normalizedCar.displayedPosition >= topSplitFirstSlotIndex) &&
						  (normalizedCar.displayedPosition <= topSplitLastSlotIndex)) ||
						 ((normalizedCar.displayedPosition >= bottomSplitFirstSlotIndex) &&
						  (normalizedCar.displayedPosition <= bottomSplitLastSlotIndex)));

					//Set Red


					// hide cars that have not qualified yet (only during qualifying)

					if (IRSDK.normalizedSession.isInQualifyingSession)
					{
						if (normalizedCar.bestLapTime == 0)
						{
							liveDataLeaderboardSlot.show = false;
						}
					}
				}

				// skip cars not visible on the leaderboard

				if (!liveDataLeaderboardSlot.show)
				{
					if (splitLeaderboard)
					{
						normalizedCar.wasVisibleOnLeaderboard = false;
					}
				}
				else
				{
					// at least one car is visible so we want to show the leaderboard

					currentLiveDataLeaderboard.show = true;

					carsShown++;

					// slot index

					var slotIndex = normalizedCar.displayedPosition -
					                ((normalizedCar.displayedPosition >= bottomSplitFirstSlotIndex)
						                ? bottomSplitFirstSlotIndex - topSplitLastSlotIndex
						                : topSplitFirstSlotIndex);

					// compute slot offset

					if (splitLeaderboard)
					{
						var resetSlotOffset =
							((lastFrameBottomSplitFirstPosition[classIndex] != bottomSplitFirstSlotIndex) &&
							 (normalizedCar.displayedPosition >= bottomSplitFirstSlotIndex));

						var targetSlotOffset =
							new Unity.Vector2(Settings.overlay.leaderboardSlotSpacing.x,
								-Settings.overlay.leaderboardSlotSpacing.y) * slotIndex +
							new Unity.Vector2(Settings.overlay.leaderboardFirstSlotPosition.x,
								-Settings.overlay.leaderboardFirstSlotPosition.y);

						if (normalizedCar.wasVisibleOnLeaderboard && !resetSlotOffset)
						{
							normalizedCar.leaderboardSlotOffset +=
								(targetSlotOffset - normalizedCar.leaderboardSlotOffset) * 0.15f;
						}
						else
						{
							normalizedCar.leaderboardSlotOffset = targetSlotOffset;
						}

						liveDataLeaderboardSlot.offset = normalizedCar.leaderboardSlotOffset;
					}

					//


					liveDataLeaderboardSlot.textLayer1 = GetTextContent(out liveDataLeaderboardSlot.textLayer1Color,
						"LeaderboardPositionTextLayer1", normalizedCar, normalizedCar.carClass);
					liveDataLeaderboardSlot.textLayer2 = GetTextContent(out liveDataLeaderboardSlot.textLayer2Color,
						"LeaderboardPositionTextLayer2", normalizedCar, normalizedCar.carClass);
					liveDataLeaderboardSlot.textLayer3 = GetTextContent(out liveDataLeaderboardSlot.textLayer3Color,
						"LeaderboardPositionTextLayer3", normalizedCar, normalizedCar.carClass);
					liveDataLeaderboardSlot.textLayer4 = GetTextContent(out liveDataLeaderboardSlot.textLayer4Color,
						"LeaderboardPositionTextLayer4", normalizedCar, normalizedCar.carClass);

					if (normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx)
					{
						liveDataLeaderboardSlot.textLayer1Color = Unity.Color.red;
					}


					//any pens?
					liveDataLeaderboardSlot.penaltyFlag =
						normalizedCar.sessionFlags.HasAnyFlag(SessionFlags.Black | SessionFlags.Crossed |
						                                      SessionFlags.Disqualify);
					liveDataLeaderboardSlot.slowDownFlag = normalizedCar.sessionFlags.HasAnyFlag(SessionFlags.Furled);
					liveDataLeaderboardSlot.meatballFlag = normalizedCar.sessionFlags.HasAnyFlag(SessionFlags.Repair);
					liveDataLeaderboardSlot.finished = normalizedCar.hasCrossedFinishLine;


					//put pit / outlap
					liveDataLeaderboardSlot.textLayer5 = GetTextContent(out liveDataLeaderboardSlot.textLayer4Color,
						"LeaderboardPositionTextLayer5", normalizedCar, normalizedCar.carClass);


					// preferred driver

					liveDataLeaderboardSlot.showPreferredCar = normalizedCar.isPreferredCar;

					//

					if (splitLeaderboard)
					{
						normalizedCar.wasVisibleOnLeaderboard = true;
					}

					normalizedCarInFront = normalizedCar;
				}
			}

			if (splitLeaderboard)
			{
				// leaderboard offset and background and splitter

				currentLiveDataLeaderboard.offset = new Unity.Vector2(leaderboardOffset.x, leaderboardOffset.y);
				currentLiveDataLeaderboard.backgroundSize =
					Settings.overlay.leaderboardSlotSpacing * Math.Min(carsShown, slotCount);
				currentLiveDataLeaderboard.showSplitter = ((topSplitLastSlotIndex + 1) != bottomSplitFirstSlotIndex);
				currentLiveDataLeaderboard.splitterPosition = Settings.overlay.leaderboardFirstSlotPosition +
				                                              Settings.overlay.leaderboardSlotSpacing *
				                                              topSplitLastSlotIndex;

				if (currentLiveDataLeaderboard.show)
				{
					if (Settings.overlay.leaderboardMultiClassOffsetType == 0)
					{
						leaderboardOffset.y += currentLiveDataLeaderboard.backgroundSize.y;
					}

					leaderboardOffset += Settings.overlay.leaderboardMultiClassOffset;
				}

				// remember the bottom split first position

				lastFrameBottomSplitFirstPosition[classIndex] = bottomSplitFirstSlotIndex;
			}
		}
	}
}