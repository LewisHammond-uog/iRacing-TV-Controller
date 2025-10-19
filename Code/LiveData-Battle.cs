using System;
using irsdkSharp.Serialization.Enums.Fastest;

namespace iRacingTVController;

public partial class LiveData
{
	public void UpdateBattleChyron()
	{
		liveDataBattleChyron.show = false;

		if (!IRSDK.normalizedSession.isInRaceSession)
		{
			return;
		}

		if ((IRSDK.normalizedData.sessionState < SessionState.StateRacing) || ((IRSDK.normalizedData.sessionFlags &
			                                                                       ((uint) SessionFlags.CautionWaving |
				                                                                       (uint) SessionFlags.Caution |
				                                                                       (uint) SessionFlags
					                                                                       .GreenHeld)) !=
		                                                                       0))
		{
			battleChyronTimer = Settings.overlay.battleChyronDelay;

			return;
		}

		if (battleChyronTimer > 0)
		{
			battleChyronTimer -= Program.deltaTime;

			if (battleChyronTimer > 0)
			{
				return;
			}
		}

		if ((IRSDK.currentCameraType == SettingsDirector.CameraType.Inside) ||
		    (IRSDK.currentCameraType == SettingsDirector.CameraType.Close))
		{
			var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx(IRSDK.normalizedData.camCarIdx);

			if ((normalizedCar != null) && normalizedCar.includeInLeaderboard && !normalizedCar.isOnPitRoad &&
			    Director.showChyron &&
			    (!liveDataControlPanel.voiceOfOn || (IRSDK.normalizedData.radioTransmitCarIdx == -1)))
			{
				var nearestDeltaLapPosition = float.MaxValue;
				NormalizedCar? nearestNormalizedCar = null;

				if (normalizedCar.normalizedCarInFront != null)
				{
					var deltaLapPosition =
						Math.Abs(normalizedCar.lapPosition - normalizedCar.normalizedCarInFront.lapPosition);

					if (deltaLapPosition < 0.5f)
					{
						nearestDeltaLapPosition = deltaLapPosition;
						nearestNormalizedCar = normalizedCar.normalizedCarInFront;
					}
				}

				if (nearestNormalizedCar != null)
				{
					var distanceInMeters = nearestDeltaLapPosition * IRSDK.normalizedSession.trackLengthInMeters;

					if (distanceInMeters <= Settings.overlay.battleChyronDistance)
					{
						Unity.Color color;

						liveDataBattleChyron.show = true;

						liveDataBattleChyron.textLayer1 =
							GetTextContent(out color, "BattleChyronTextLayer1", nearestNormalizedCar);
						liveDataBattleChyron.textLayer2 =
							GetTextContent(out color, "BattleChyronTextLayer2", nearestNormalizedCar);
						liveDataBattleChyron.textLayer3 =
							GetTextContent(out color, "BattleChyronTextLayer3", nearestNormalizedCar);
						liveDataBattleChyron.textLayer4 =
							GetTextContent(out color, "BattleChyronTextLayer4", nearestNormalizedCar);
						liveDataBattleChyron.textLayer5 =
							GetTextContent(out color, "BattleChyronTextLayer5", nearestNormalizedCar);
						liveDataBattleChyron.textLayer6 =
							GetTextContent(out color, "BattleChyronTextLayer6", nearestNormalizedCar);
						liveDataBattleChyron.textLayer7 =
							GetTextContent(out color, "BattleChyronTextLayer7", nearestNormalizedCar);
						liveDataBattleChyron.textLayer8 =
							GetTextContent(out color, "BattleChyronTextLayer8", nearestNormalizedCar);
						liveDataBattleChyron.textLayer9 =
							GetTextContent(out color, "BattleChyronTextLayer9", nearestNormalizedCar);
						liveDataBattleChyron.textLayer10 =
							GetTextContent(out color, "BattleChyronTextLayer10", nearestNormalizedCar);
						liveDataBattleChyron.textLayer11 =
							GetTextContent(out color, "BattleChyronTextLayer11", nearestNormalizedCar);
						liveDataBattleChyron.textLayer12 =
							GetTextContent(out color, "BattleChyronTextLayer12", nearestNormalizedCar);
						liveDataBattleChyron.textLayer13 =
							GetTextContent(out color, "BattleChyronTextLayer13", nearestNormalizedCar);
						liveDataBattleChyron.textLayer14 =
							GetTextContent(out color, "BattleChyronTextLayer14", nearestNormalizedCar);
						liveDataBattleChyron.textLayer15 =
							GetTextContent(out color, "BattleChyronTextLayer15", nearestNormalizedCar);

						liveDataBattleChyron.carIdx = nearestNormalizedCar.carIdx;
					}
				}
			}
		}
	}
}