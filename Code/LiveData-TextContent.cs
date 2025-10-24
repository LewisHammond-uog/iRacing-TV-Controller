using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace iRacingTVController;

public partial class LiveData
{
	public string GetTextContent(out Unity.Color color, string key, NormalizedCar? normalizedCar = null,
		CustomClassSystem.CarClass? leaderboardClass = null, int extraInt = 0)
	{
		var settingsText = Settings.overlay.textSettingsDataDictionary[key];

		var session = IRSDK.normalizedSession.sessionNumber;
		var results = CustomClassSystem.Instance.Results;

		color = GetTextColor(settingsText, normalizedCar);

		const float msToMph = 3.6f;
		switch (settingsText.content)
		{
			case SettingsText.Content.None:

				return "";

			case SettingsText.Content.Driver_CarNumber:

				return normalizedCar?.carNumber ?? "";

			case SettingsText.Content.Driver_CsvProperty:

				return GetCsvProperty(settingsText, normalizedCar);

			case SettingsText.Content.Driver_CarBehind_CarNumber:

				return (normalizedCar?.normalizedCarBehind != null)
					? $"#{normalizedCar.normalizedCarBehind.carNumber}"
					: "";

			case SettingsText.Content.Driver_CarBehind_CsvProperty:

				return GetCsvProperty(settingsText, normalizedCar?.normalizedCarBehind);

			case SettingsText.Content.Driver_CarBehind_GapTime:
			{
				var text = "-.--";

				if (normalizedCar != null)
				{
					if (normalizedCar.normalizedCarBehind != null)
					{
						text = $"{normalizedCar.gapTimeBack:0.00}";

						var deltaLapPositionRelativeToClassLeader =
							normalizedCar.normalizedCarBehind.lapPositionRelativeToClassLeader -
							normalizedCar.lapPositionRelativeToClassLeader;

						if (deltaLapPositionRelativeToClassLeader < -0.5f)
						{
							color = new Unity.Color(1.0f, 0.3f, 0.3f, color.a);
						}
						else if (deltaLapPositionRelativeToClassLeader > 0.5f)
						{
							color = new Unity.Color(0.4f, 0.4f, 1.0f, color.a);
						}
					}
				}

				return text;
			}

			case SettingsText.Content.Driver_CarBehind_Name:

				return normalizedCar?.normalizedCarBehind?.displayedName ?? "";

			case SettingsText.Content.Driver_CarBehind_Position:

				return (normalizedCar?.normalizedCarBehind?.displayedPosition >= 1)
					? "P" + normalizedCar.normalizedCarBehind.displayedPosition.ToString()
					: "";

			case SettingsText.Content.Driver_Sectors:

				if (IRSDK.normalizedSession.isInRaceSession || IsLiveSessionInReplayMode() || normalizedCar == null)
				{
					return "";
				}

				if (normalizedCar.isOnPitRoad || normalizedCar.isOutOfCar)
				{
					return "PIT";
				}

				if (normalizedCar.isOutLap)
				{
					return "OUT LAP";
				}

				StringBuilder sectorTimes = new StringBuilder();
				;
				List<SectorLapStatus> list = normalizedCar.GetCurrentLapSectorStatuses(fake: true);
				for (int index = 0; index < list.Count; index++)
				{
					SectorLapStatus? sector = list[index];
					GetSectorStatusColour(sector, sectorTimes);
					sectorTimes.Append(sector.Time.ToString("00.00</color>"));

					if (index < list.Count - 1)
					{
						sectorTimes.Append(" | ");
					}
				}


				return sectorTimes.ToString();

			case SettingsText.Content.Track_Name:

				return IRSDK.session?.WeekendInfo.TrackDisplayShortName + " - " +
				       IRSDK.session?.WeekendInfo.TrackConfigName;

			case SettingsText.Content.Track_CityCounty:

				return IRSDK.session?.WeekendInfo.TrackCity + ", " + IRSDK.session?.WeekendInfo.TrackCountry;
			case SettingsText.Content.Track_ExtraInfo:

				StringBuilder extraInfo = new StringBuilder();
				var km = IRSDK.normalizedSession.trackLengthInMeters / 1000.0f;

				extraInfo.AppendLine($"<b>Length</b>:<pos=60%> {km:0.00} KM");
				extraInfo.AppendLine($"<b>Turns</b>:<pos=60%> {IRSDK.session?.WeekendInfo.TrackNumTurns} ");
				extraInfo.AppendLine(
					$"<b>Altitude</b>:<pos=60%> {FormatMeasurementAsInt(IRSDK.session?.WeekendInfo.TrackAltitude)} ");
				extraInfo.AppendLine(
					$"<b>Pit Speed</b>:<pos=60%> {FormatMeasurementAsInt(IRSDK.session?.WeekendInfo.TrackPitSpeedLimit)} ");
				extraInfo.AppendLine(
					$"<b>Track Temp</b>:<pos=60%> {FormatMeasurementAsInt(IRSDK.session?.WeekendInfo.TrackSurfaceTemp)} ");
				extraInfo.AppendLine(
					$"<b>Air Temp</b>:<pos=60%> {FormatMeasurementAsInt(IRSDK.session?.WeekendInfo.TrackAirTemp)} ");
				extraInfo.AppendLine($"<b>Fog</b>:<pos=60%> {IRSDK.session?.WeekendInfo.TrackFogLevel} ");
				var windDirRaw = IRSDK.session?.WeekendInfo.TrackWindDir ?? string.Empty;
				double windDirRad = 0;
				if (!string.IsNullOrWhiteSpace(windDirRaw))
				{
					var numeric = windDirRaw.Replace("rad", "", StringComparison.OrdinalIgnoreCase).Trim();
					double.TryParse(numeric, NumberStyles.Float, CultureInfo.InvariantCulture, out windDirRad);
				}

				var windDirCard = RadiansToCompassFromEastCCW(windDirRad);
				string windSpd = FormatWindVelocityAsKph(IRSDK.session?.WeekendInfo.TrackWindVel);
				extraInfo.AppendLine($"<b>Wind</b>:<pos=60%> {windSpd} - {windDirCard}");
				extraInfo.AppendLine($"<b>Humidity</b>:<pos=60%> {IRSDK.session?.WeekendInfo.TrackRelativeHumidity}");

				return extraInfo.ToString();

			case SettingsText.Content.Driver_CarBehind_UserID:

				return normalizedCar?.normalizedCarBehind?.userId.ToString() ?? "";

			case SettingsText.Content.Driver_CarBehind_Rating:

				return normalizedCar?.normalizedCarBehind?.iRating.ToString() ?? "";

			case SettingsText.Content.Driver_CarInFront_CarNumber:

				return (normalizedCar?.normalizedCarInFront != null)
					? $"#{normalizedCar.normalizedCarInFront.carNumber}"
					: "";

			case SettingsText.Content.Driver_CarInFront_CsvProperty:

				return GetCsvProperty(settingsText, normalizedCar?.normalizedCarInFront);

			case SettingsText.Content.Driver_CarInFront_GapTime:
			{
				var text = "-.--";

				if (normalizedCar != null)
				{
					if (normalizedCar.normalizedCarInFront != null)
					{
						text = $"{normalizedCar.gapTimeFront:0.00}";

						var deltaLapPositionRelativeToClassLeader =
							normalizedCar.normalizedCarInFront.lapPositionRelativeToClassLeader -
							normalizedCar.lapPositionRelativeToClassLeader;

						if (deltaLapPositionRelativeToClassLeader < -0.5f)
						{
							color = new Unity.Color(1.0f, 0.3f, 0.3f, color.a);
						}
						else if (deltaLapPositionRelativeToClassLeader > 0.5f)
						{
							color = new Unity.Color(0.4f, 0.4f, 1.0f, color.a);
						}
					}
				}

				return text;
			}

			case SettingsText.Content.Driver_CarInFront_Name:

				return normalizedCar?.normalizedCarInFront?.displayedName ?? "";

			case SettingsText.Content.Driver_CarInFront_Position:

				return (normalizedCar?.normalizedCarInFront?.displayedPosition >= 1)
					? "P" + normalizedCar.normalizedCarInFront.displayedPosition.ToString()
					: "";

			case SettingsText.Content.Driver_CarInFront_UserID:

				return normalizedCar?.normalizedCarInFront?.userId.ToString() ?? "";

			case SettingsText.Content.Driver_CarInFront_LapTimeDiff:
			{
				if (normalizedCar == null)
				{
					return String.Empty;
				}

				float? rawTime = GetLapTimeComparisionExactLap(normalizedCar, ComparisionMode.Ahead, extraInt);
				if (rawTime == null)
				{
					return "NO LAP";
				}

				string plusMinus = rawTime.Value > 0 ? "+" : "-";
				color = rawTime.Value > 0 ? Unity.Color.red : Unity.Color.green;
				string str = $"{plusMinus}{Program.GetTimeString(rawTime.Value, true)}";

				return str;
			}


			case SettingsText.Content.Driver_CarBehind_LapTimeDiff:
			{
				if (normalizedCar == null)
				{
					return String.Empty;
				}

				float? rawTime = GetLapTimeComparisionExactLap(normalizedCar, ComparisionMode.Behind, extraInt);
				if (rawTime == null)
				{
					return "NO LAP";
				}

				string plusMinus = rawTime.Value > 0 ? "+" : "-";
				color = rawTime.Value > 0 ? Unity.Color.red : Unity.Color.green;
				string str = $"{plusMinus}{Program.GetTimeString(rawTime.Value, true)}";

				return str;
			}

			case SettingsText.Content.Driver_CarInFront_Rating:

				return normalizedCar?.normalizedCarInFront?.iRating.ToString() ?? "";

			case SettingsText.Content.Driver_FamilyName:

				return normalizedCar?.familyName ?? "";

			case SettingsText.Content.Driver_FullName:

				return normalizedCar?.userName ?? "";

			case SettingsText.Content.Driver_Gear:
			{
				if (normalizedCar != null)
				{
					if (normalizedCar.gear == -1)
					{
						color = new Unity.Color(1, 0.25f, 0.25f, 1);

						return "R";
					}
					else if (normalizedCar.gear == 0)
					{
						color = new Unity.Color(1, 1, 0.25f, 1);

						return "N";
					}
					else
					{
						return normalizedCar.gear.ToString();
					}
				}
				else
				{
					return "";
				}
			}

			case SettingsText.Content.Driver_GivenName:

				return normalizedCar?.givenName ?? "";

			case SettingsText.Content.Driver_LapDelta:
			{
				var text = " -.-- | -.--";

				if (normalizedCar != null)
				{
					text =
						$"{normalizedCar.interpolatedDeltaTime:+0.00;-0.00; 0.00} | {normalizedCar.interpolatedDeltaInterpolatedDeltaTime:+0.00;-0.00; 0.00}";

					if (normalizedCar.interpolatedDeltaInterpolatedDeltaTime <= 0)
					{
						color = Unity.Color.Lerp(Unity.Color.white, green,
							(float) Math.Min(1, -normalizedCar.interpolatedDeltaInterpolatedDeltaTime));
					}
					else
					{
						color = Unity.Color.Lerp(Unity.Color.white, red,
							(float) Math.Min(1, normalizedCar.interpolatedDeltaInterpolatedDeltaTime));
					}
				}

				return text;
			}

			case SettingsText.Content.Driver_LapsBehindClassLeader:
			{
				var text = "";

				if (normalizedCar != null)
				{
					if (IRSDK.normalizedSession.isInRaceSession && (normalizedCar.lapPositionRelativeToClassLeader > 0))
					{
						text = $"{normalizedCar.lapPositionRelativeToClassLeader:0.000}";
					}
				}

				return text;
			}

			case SettingsText.Content.Driver_LapsLed:
			{
				if ((normalizedCar != null) && (normalizedCar.lapsLed > 0))
				{
					return normalizedCar.lapsLed.ToString();
				}
				else
				{
					return "";
				}
			}

			case SettingsText.Content.Driver_LapTime_Current:

				if (normalizedCar != null)
				{
					if (IRSDK.normalizedSession.isInRaceSession)
					{
						return $"";
					}

					if (IRSDK.normalizedSession.isInQualifyingSession || IRSDK.normalizedSession.isInQualifyingSession)
					{
						if (normalizedCar.isOnPitRoad == true)
						{
							return "PIT LANE";
						}

						if (normalizedCar.lastPitLap == normalizedCar.currentLap)
						{
							return "OUT LAP";
						}
					}


					if ((normalizedCar.currentLapTime < 5) && (normalizedCar.lastLapTime > 0))
					{
						return Program.GetTimeString(normalizedCar.lastLapTime, true);
					}
					else if (normalizedCar.currentLapTime > 0)
					{
						return Program.GetTimeString(normalizedCar.currentLapTime, true);
					}
					else
					{
						return "--.---";
					}
				}
				else
				{
					return "--.---";
				}

			case SettingsText.Content.Driver_LapTime_LastLap:

				if ((normalizedCar != null) && (normalizedCar.lastLapTime > 0))
				{
					return Program.GetTimeString(normalizedCar.lastLapTime, true);
				}
				else
				{
					return "--.---";
				}

			case SettingsText.Content.Driver_License:
			{
				if (normalizedCar != null)
				{
					color = new Unity.Color(normalizedCar.licenseColor);

					return normalizedCar.license;
				}
				else
				{
					return "";
				}
			}

			case SettingsText.Content.Driver_Name:

				return normalizedCar?.displayedName ?? "";

			case SettingsText.Content.Driver_Position:

				return (normalizedCar?.displayedPosition >= 1) ? normalizedCar.displayedPosition.ToString() : "";

			case SettingsText.Content.Driver_Position_WithP:

				return (normalizedCar?.displayedPosition >= 1) ? "P" + normalizedCar.displayedPosition.ToString() : "";

			case SettingsText.Content.Driver_Position_Ordinal:

				return (normalizedCar?.displayedPosition >= 1) ? GetOrdinal(normalizedCar.displayedPosition) : "";

			case SettingsText.Content.Driver_Position_FinalResults:
				if (normalizedCar == null)
				{
					return ReturnErrorInDebugOrBlankInRelease();
				}

				var pos = CustomClassSystem.Instance?.Results?.GetCarClassPosition(normalizedCar);
				return pos == null ? ReturnErrorInDebugOrBlankInRelease() : GetOrdinal(pos.Value);

			case SettingsText.Content.Driver_QualifyLapTime_1:

				return (normalizedCar?.qualifyingLapTimes[0] > 0)
					? $"{normalizedCar.qualifyingLapTimes[0]:0.000}"
					: "--.---";

			case SettingsText.Content.Driver_QualifyLapTime_2:

				return (normalizedCar?.qualifyingLapTimes[1] > 0)
					? $"{normalizedCar.qualifyingLapTimes[1]:0.000}"
					: "--.---";

			case SettingsText.Content.Driver_QualifyLapTime_3:

				return (normalizedCar?.qualifyingLapTimes[2] > 0)
					? $"{normalizedCar.qualifyingLapTimes[2]:0.000}"
					: "--.---";

			case SettingsText.Content.Driver_QualifyLapTime_4:

				return (normalizedCar?.qualifyingLapTimes[3] > 0)
					? $"{normalizedCar.qualifyingLapTimes[3]:0.000}"
					: "--.---";

			case SettingsText.Content.Driver_QualifyPosition:

				return (normalizedCar?.qualifyingClassPosition >= 1)
					? normalizedCar.qualifyingClassPosition.ToString()
					: "";

			case SettingsText.Content.Driver_QualifyPosition_WithP:

				return (normalizedCar?.qualifyingClassPosition >= 1)
					? "P" + classSystem.GetPositionInClass(normalizedCar).ToString()
					: "";

			case SettingsText.Content.Driver_QualifyPosition_Ordinal:

				return (normalizedCar?.qualifyingClassPosition >= 1)
					? GetOrdinal(normalizedCar.qualifyingClassPosition)
					: "";

			case SettingsText.Content.Driver_OverallQualityPosition_WithP:

				return (normalizedCar?.qualifyingPosition >= 1)
					? "P" + normalizedCar.qualifyingPosition.ToString()
					: "";

			case SettingsText.Content.Driver_QualifyPosition_Class:

				if (normalizedCar == null)
				{
					return "";
				}

				//Calculate Class Quali Position
				int localPos = 0;
				var allPositions = IRSDK.normalizedData.GetCarsInQualifyingOverallOrder(true);
				foreach (var car in allPositions)
				{
					if (car.classID == normalizedCar.classID)
					{
						localPos++;
					}

					if (car == normalizedCar)
					{
						break;
					}
				}

				return (localPos >= 1) ? $"P{localPos} ({normalizedCar.classID})" : "";

			case SettingsText.Content.Driver_QualifyTime:
			{
				if (normalizedCar != null)
				{
					if (normalizedCar.qualifyingTime < 0)
					{
						return Settings.overlay.translationDictionary["DidNotQualify"].translation;
					}
					else if (normalizedCar.qualifyingTime == 0)
					{
						return "";
					}
					else
					{
						return Program.GetTimeString(normalizedCar.qualifyingTime, true);
					}
				}
				else
				{
					return "";
				}
			}

			case SettingsText.Content.Driver_Rating:

				if (normalizedCar != null)
				{
					return normalizedCar.iRating.ToString();
				}
				else
				{
					return "";
				}

			case SettingsText.Content.Driver_RPM:
			{
				if (normalizedCar != null)
				{
					return $"{normalizedCar.rpm:0}";
				}
				else
				{
					return "";
				}
			}

			case SettingsText.Content.Driver_Speed:

				if (normalizedCar == null)
				{
					return "";
				}
				else
				{
					return
						$"{Math.Abs(normalizedCar.speedInMetersPerSecond) * (IRSDK.normalizedData.displayIsMetric ? msToMph : 2.23694f):0} {(IRSDK.normalizedData.displayIsMetric ? Settings.overlay.translationDictionary["KPH"].translation : Settings.overlay.translationDictionary["MPH"].translation)}";
				}

			case SettingsText.Content.Driver_Telemetry:
			{
				if (normalizedCar == null)
				{
					return "";
				}
				else
				{
					var sign = Settings.overlay.telemetryShowAsNegativeNumbers ? "-" : "+";

					var text = string.Empty;

					if (IRSDK.normalizedSession.isInPracticeSession || IRSDK.normalizedSession.isInQualifyingSession)
					{
						if (normalizedCar.bestLapTime > 0)
						{
							if (classLeaderBestLapTime == normalizedCar.bestLapTime)
							{
								text = Program.GetTimeString(classLeaderBestLapTime, true);
							}
							else
							{
								var deltaTime = normalizedCar.bestLapTime - classLeaderBestLapTime;

								text = $"{sign}{deltaTime:0.000}";
							}
						}

						normalizedCar.checkpointTime = 0;
					}
					else if (normalizedCar.isOnPitRoad)
					{
						text = Settings.overlay.translationDictionary["Pit"].translation;
						color = Settings.overlay.telemetryPitColor;

						normalizedCar.checkpointTime = 0;
					}
					else if (normalizedCar.isOutOfCar && normalizedCar.outOfCarTimer > 5f)
					{
						text = Settings.overlay.translationDictionary["Out"].translation;
						color = Settings.overlay.telemetryOutColor;

						normalizedCar.checkpointTime = 0;
					}
					else if (IRSDK.normalizedSession.isInRaceSession)
					{
						if (normalizedCar.hasCrossedStartLine)
						{
							if (normalizedCar.lapPositionRelativeToClassLeader >= 1.0f)
							{
								var wholeLapsDown = Math.Floor(normalizedCar.lapPositionRelativeToClassLeader);

								text =
									$"-{wholeLapsDown:0} {Settings.overlay.translationDictionary["LapsAbbreviation"].translation}";

								normalizedCar.checkpointTime = 0;
							}
							else if (normalizedCarInFront == null) //No Car in front = leader!
							{
								text = "LEADER";

								if (Settings.overlay.telemetryIsBetweenCars)
								{
									text = "INTERVAL";
								}
								else
								{
									text = "GAP";
								}
							}
							else if (!IRSDK.normalizedData.isUnderCaution && (normalizedCarInFront != null))
							{
								if (!normalizedCar.hasCrossedFinishLine && !normalizedCarInFront.hasCrossedFinishLine)
								{
									var lapPosition = Settings.overlay.telemetryIsBetweenCars
										? (normalizedCarInFront.lapPosition - normalizedCar.lapPosition)
										: normalizedCar.lapPositionRelativeToClassLeader;

									if (Settings.overlay.telemetryMode == 0)
									{
										text =
											$"{sign}{lapPosition:0.000} {Settings.overlay.translationDictionary["LapsAbbreviation"].translation}";
									}
									else if (Settings.overlay.telemetryMode == 1)
									{
										var distance = lapPosition * IRSDK.normalizedSession.trackLengthInMeters;

										if (IRSDK.normalizedData.displayIsMetric)
										{
											var distanceString = $"{distance:0}";

											if (distanceString != "0")
											{
												text =
													$"{sign}{distanceString} {Settings.overlay.translationDictionary["MetersAbbreviation"].translation}";
											}
										}
										else
										{
											distance *= 3.28084f;

											var distanceString = $"{distance:0}";

											if (distanceString != "0")
											{
												text =
													$"{sign}{distanceString} {Settings.overlay.translationDictionary["FeetAbbreviation"].translation}";
											}
										}
									}
									else
									{
										if (Settings.overlay.telemetryIsBetweenCars)
										{
											if (normalizedCarInFront.sessionTimeCheckpoints
												    [normalizedCar.checkpointIdx] > 0)
											{
												if (!splitLeaderboard)
												{
													var checkpointTime =
														Math.Abs(
															(float) (normalizedCar.sessionTimeCheckpoints[
																         normalizedCar.checkpointIdx] -
															         normalizedCarInFront.sessionTimeCheckpoints[
																         normalizedCar.checkpointIdx]));

													if ((normalizedCar.checkpointTime != 0) &&
													    (normalizedCar.normalizedCarForTelemetry != null) &&
													    (normalizedCarInFront.carIdx ==
													     normalizedCar.normalizedCarForTelemetry.carIdx))
													{
														normalizedCar.checkpointTime =
															normalizedCar.checkpointTime * 0.95f +
															checkpointTime * 0.05f;
													}
													else
													{
														normalizedCar.normalizedCarForTelemetry = normalizedCarInFront;

														normalizedCar.checkpointTime = checkpointTime;
													}
												}

												text = $"{sign}{normalizedCar.checkpointTime:0.00}";
											}
										}
										else if (normalizedCarClassLeader != null)
										{
											if (normalizedCarClassLeader.sessionTimeCheckpoints[
												    normalizedCar.checkpointIdx] > 0)
											{
												if (!splitLeaderboard)
												{
													var checkpointTime =
														Math.Abs(
															(float) (normalizedCar.sessionTimeCheckpoints[
																         normalizedCar.checkpointIdx] -
															         normalizedCarClassLeader.sessionTimeCheckpoints[
																         normalizedCar.checkpointIdx]));

													if ((normalizedCar.checkpointTime != 0) &&
													    (normalizedCar.normalizedCarForTelemetry != null) &&
													    (normalizedCarClassLeader.carIdx ==
													     normalizedCar.normalizedCarForTelemetry.carIdx))
													{
														normalizedCar.checkpointTime =
															normalizedCar.checkpointTime * 0.95f +
															checkpointTime * 0.05f;
													}
													else
													{
														normalizedCar.normalizedCarForTelemetry =
															normalizedCarClassLeader;

														normalizedCar.checkpointTime = checkpointTime;
													}
												}

												text = $"{sign}{normalizedCar.checkpointTime:0.00}";
											}
										}
									}

									if (text == string.Empty)
									{
										normalizedCar.checkpointTime = 0;
									}
								}
							}
						}
						else
						{
							normalizedCar.checkpointTime = 0;
						}
					}

					return text;
				}
			}

			case SettingsText.Content.Driver_UserID:

				return normalizedCar?.userId.ToString() ?? "";

			case SettingsText.Content.Leaderboard_ClassName:
			{
				if (splitLeaderboard == false)
				{
					return "All Classes";
				}

				color = CustomClassSystem.Instance.GetColourForClass(leaderboardClass?.ClassName);

				return leaderboardClass?.ClassName ?? "(error)";
			}

			case SettingsText.Content.Leaderboard_ClassNameShort:
			{
				if (splitLeaderboard == false)
				{
					return "All";
				}

				return leaderboardClass?.ClassName ?? "(error)";
			}

			case SettingsText.Content.ThisCar_LeaderboardClass:
			{
				if (splitLeaderboard == false)
				{
					return "All";
				}

				if (normalizedCar == null)
				{
					return "";
				}

				int carClassIndex = normalizedCar.leaderboardClassIndex;
				var carClass = CustomClassSystem.Instance.GetClassForCar(normalizedCar);

				color = carClass.Colour;


				return carClass.ClassName.ToUpper();
				break;
			}

			case SettingsText.Content.Player_FuelRemainingInLaps:
			{
				var text = "-.--";

				if (IRSDK.normalizedData.highestLapFuelLevelDelta > 0)
				{
					var fuelLapsRemaining =
						IRSDK.normalizedData.fuelLevel / IRSDK.normalizedData.highestLapFuelLevelDelta;

					text = $"{fuelLapsRemaining:0.00}";

					if ((IRSDK.normalizedData.isInTimedRace && (fuelLapsRemaining <= 2.0f)) ||
					    (!IRSDK.normalizedData.isInTimedRace &&
					     (fuelLapsRemaining < IRSDK.normalizedData.sessionLapsRemaining)))
					{
						color = new Unity.Color(1, 0.25f, 0.25f, 1);
					}
				}

				return text;
			}

			case SettingsText.Content.Player_RPM:
			{
				var text = "----";

				if (normalizedCar != null)
				{
					var steppedRPM = (int) Math.Floor(normalizedCar.rpm / 10) * 10;

					text = steppedRPM.ToString();

					if (normalizedCar.gear < IRSDK.normalizedSession.numForwardGears)
					{
						if (normalizedCar.rpm >= IRSDK.normalizedSession.blinkRpm)
						{
							color = new Unity.Color(1, 0.2f, 0.2f, 1);
						}
						else if (normalizedCar.rpm >= IRSDK.normalizedSession.redlineRpm)
						{
							color = new Unity.Color(1, 1, 0.2f, 1);
						}
						else if (normalizedCar.rpm >= IRSDK.normalizedSession.shiftRpm)
						{
							color = new Unity.Color(0.2f, 1, 0.2f, 1);
						}
					}
				}

				return text;
			}

			case SettingsText.Content.Session_CurrentLap:
			{
				if (IRSDK.normalizedData.isInTimedRace || !IRSDK.normalizedSession.isInRaceSession)
				{
					return Program.GetTimeString(
						Math.Floor(IRSDK.normalizedData.sessionTimeTotal - IRSDK.normalizedData.sessionTimeRemaining),
						false) + " | " + Program.GetTimeString(IRSDK.normalizedData.sessionTimeTotal, false);
				}
				else
				{
					return IRSDK.normalizedData.lapNumber.ToString() + " | " +
					       IRSDK.normalizedData.sessionLapsTotal.ToString();
				}
			}

			case SettingsText.Content.Session_LapsRemaining:
			{
				if (IRSDK.normalizedData.isInTimedRace || !IRSDK.normalizedSession.isInRaceSession)
				{
					return Program.GetTimeString(Math.Ceiling(IRSDK.normalizedData.sessionTimeRemaining), false);
				}
				else if (IRSDK.normalizedData.sessionLapsRemaining == 1)
				{
					return Settings.overlay.translationDictionary["FinalLap"].translation;
				}
				else
				{
					var lapsRemaining = Math.Min(IRSDK.normalizedData.sessionLapsTotal,
						IRSDK.normalizedData.sessionLapsRemaining);

					return lapsRemaining.ToString() + " " + Settings.overlay.translationDictionary["ToGo"].translation;
				}
			}

			case SettingsText.Content.RacePoints:
				if (normalizedCar == null)
				{
					return ReturnErrorInDebugOrBlankInRelease();
				}

				if (!normalizedCar.includeInLeaderboard)
				{
					return "";
				}

				CustomClassSystem.Instance.Results?.UpdateFromPositionModel(IRSDK.session.SessionInfo.Sessions[session]
					.ResultsPositions);

				int pts = results.GetPointsForCar(normalizedCar).TotalPoints;
				if (normalizedCar.carNumberRaw is 30 or 13)
				{
					pts += 1;
				}

				return results == null ? "" : $"{pts} PTS";


				break;

			case SettingsText.Content.RaceBounusPoints:
				CustomClassSystem.Instance.Results?.UpdateFromPositionModel(IRSDK.session.SessionInfo.Sessions[session]
					.ResultsPositions);

				bool hasFl = results.GetPointsForCar(normalizedCar).hasFastestLap;
				bool hasZeroX = results.GetPointsForCar(normalizedCar).hasZeroX;

				if (normalizedCar.carNumberRaw is 30 or 13)
				{
					hasZeroX = true;
				}


				string bonus = String.Empty;

				const string flString = "<color=purple>FL</color>";
				const string zeroXString = "<color=green>0x</color>";

				switch (hasFl)
				{
					case true when hasZeroX:
						return $"{flString} | {zeroXString}";
					case true:
						return flString;
				}

				return hasZeroX ? zeroXString : string.Empty;

			case SettingsText.Content.FinishTime_Classsed:

				if (normalizedCar == null)
				{
					return ReturnErrorInDebugOrBlankInRelease();
				}

				if (!normalizedCar.includeInLeaderboard)
				{
					return "";
				}


				if (results == null)
					return ReturnErrorInDebugOrBlankInRelease();

				CustomClassSystem.Instance.Results?.UpdateFromPositionModel(IRSDK.session.SessionInfo.Sessions[session]
					.ResultsPositions);

				CustomClassResults.OutReason outReason = results.GetOutReason(normalizedCar);
				if (outReason != CustomClassResults.OutReason.Running)
				{
					switch (outReason)
					{
						case CustomClassResults.OutReason.Running:
							return ReturnErrorInDebugOrBlankInRelease();
							break;
						case CustomClassResults.OutReason.Disconnected:
							return "DNF";
							break;
						case CustomClassResults.OutReason.Disqualified:
							return "DSQ";
							break;
						case CustomClassResults.OutReason.Unknown:
							return "NC (UNK)";
							break;
						default:
							return "NC";
					}
				}

				if (results.IsClassLeader(normalizedCar))
				{
					return "";
				}

				int? lapsBehind = results.GetCarLapsBehindClassLeader(normalizedCar);
				if (lapsBehind is > 0)
				{
					return $"+ {lapsBehind} LAPS";
				}

				float? timeBehind = results.GetCarTimeBehindClassLeader(normalizedCar);
				if (timeBehind is > 0)
				{
					double tb = (double) timeBehind;
					return $"+ {Program.GetTimeString(tb, true)}";
				}

				break;


			//Extra info decided for the driver. In quali this is OUTLAP / PIT. 
			case SettingsText.Content.Driver_ExtraInfo:

				if (normalizedCar == null)
				{
					return "";
				}

				if (IRSDK.normalizedSession.isInQualifyingSession)
				{
					if (normalizedCar.isOnPitRoad || normalizedCar.isOutOfCar)
					{
						return "PIT";
					}
					else if (normalizedCar.lastPitLap == normalizedCar.currentLap)
					{
						return "OUT LAP";
					}
					else
					{
						var sectors = normalizedCar.GetCurrentLapSectorStatuses(fake: true);
						StringBuilder sb = new StringBuilder();
						foreach (var sector in sectors)
						{
							GetSectorStatusColorBlob(sector, sb);
						}

						return sb.ToString();
					}

					return "";
				}

				break;


			case SettingsText.Content.Session_Name:
			{
				if (Settings.overlay.translationDictionary.ContainsKey(IRSDK.normalizedSession.sessionName))
				{
					return Settings.overlay.translationDictionary[IRSDK.normalizedSession.sessionName].translation;
				}
				else
				{
					return IRSDK.normalizedSession.sessionName;
				}
			}

			case SettingsText.Content.Translation_Gear:

				return Settings.overlay.translationDictionary["Gear"].translation;

			case SettingsText.Content.Translation_License:

				return Settings.overlay.translationDictionary["License"].translation;

			case SettingsText.Content.Translation_Rating:

				return Settings.overlay.translationDictionary["iRating"].translation;

			case SettingsText.Content.Translation_RPM:

				return Settings.overlay.translationDictionary["RPM"].translation;

			case SettingsText.Content.Translation_Speed:

				return Settings.overlay.translationDictionary["Speed"].translation;

			case SettingsText.Content.Translation_Units:
			{
				if (IRSDK.normalizedData.isInTimedRace || !IRSDK.normalizedSession.isInRaceSession)
				{
					return Settings.overlay.translationDictionary["Time"].translation;
				}
				else
				{
					return Settings.overlay.translationDictionary["Lap"].translation;
				}
			}

			case SettingsText.Content.Translation_VoiceOf:

				return Settings.overlay.translationDictionary["VoiceOf"].translation;
		}

		return "";
	}
}