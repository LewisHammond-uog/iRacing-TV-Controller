
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Aydsko.iRacingData.Leagues;
using irsdkSharp.Serialization.Enums.Fastest;
using irsdkSharp.Serialization.Models.Session.SessionInfo;
using StreamDeckCommunicator;
using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public partial class LiveData
	{
		public const bool UseCustomClassSystem = true;
		
		public const int MaxNumDrivers = 64;
		public const int MaxNumClasses = 8;
		public const int MaxNumCustom = 6;

		public static LiveData Instance { get; private set; }

		[JsonInclude] public bool isConnected = false;
		public string systemMessage = string.Empty;

		public LiveDataSteamVr liveDataSteamVr = new();
		public LiveDataControlPanel liveDataControlPanel = new();
		public LiveDataDriver[] liveDataDrivers = new LiveDataDriver[ MaxNumDrivers ];
		[JsonInclude] public LiveDataRaceStatus liveDataRaceStatus = new();
		[JsonInclude, XmlIgnore] public LiveDataLeaderboard[]? liveDataLeaderboardsWebPage = null;
		public LiveDataLeaderboard[]? liveDataLeaderboards = null;
		public LiveDataRaceResult liveDataRaceResult = new();
		public LiveDataVoiceOf liveDataVoiceOf = new();
		public LiveDataChyron liveDataChyron = new();
		public LiveDataBattleChyron liveDataBattleChyron = new();
		public LiveDataSubtitle liveDataSubtitle = new();
		public LiveDataIntro liveDataIntro = new();
		public LiveDataStartLights liveDataStartLights = new();
		[JsonInclude] public LiveDataTrackMap liveDataTrackMap = new();
		public LiveDataPitLane liveDataPitLane = new();
		[JsonInclude, XmlIgnore] public LiveDataEventLog liveDataEventLog = new();
		[XmlIgnore] public LiveDataHud liveDataHud = new();
		[XmlIgnore]public LiveDataTrainer liveDataTrainer = new();
		[XmlIgnore]public LiveDataWebcamStreaming liveDataWebcamStreaming = new();
		public LiveDataCustom[] liveDataCustom = new LiveDataCustom[ MaxNumCustom ];
		
		[JsonInclude] public bool isLiveSessionReplay = false;
		[JsonInclude] public int champResultCurrentPage = 0;
		[JsonInclude] public LiveDataLapComp liveDataLapComp = new LiveDataLapComp();

		public string seriesLogoTextureUrl = string.Empty;
		public string trackLogoTextureUrl = string.Empty;
		public string trackTextureUrl = string.Empty;

		[NonSerialized, XmlIgnore] public int[] lastFrameBottomSplitFirstPosition = new int[ MaxNumClasses ];

		[NonSerialized, XmlIgnore] public float speechToTextTimer = 0;

		[NonSerialized, XmlIgnore] public Dictionary<string, string> chyronRandomItemNames;
		[NonSerialized, XmlIgnore] public string[] chyronAvailableItemLabels;
		[NonSerialized, XmlIgnore] public string[] chyronAvailableItemValues;

		[NonSerialized, XmlIgnore] public int trackIdLastFrame = 0;
		[NonSerialized, XmlIgnore] public bool pitLaneTouched = false;
		[NonSerialized, XmlIgnore] public float pitLaneMinLapDistPct = 0;
		[NonSerialized, XmlIgnore] public float pitLaneMaxLapDistPct = 0;

		[NonSerialized, XmlIgnore] public float paceCarDistPct = 0;

		[NonSerialized, XmlIgnore] public Color red = new( 1, 0.35f, 0.35f, 1 );
		[NonSerialized, XmlIgnore] public Color green = new( 0.2f, 1, 0.2f, 1 );

		[NonSerialized, XmlIgnore] public float classLeaderBestLapTime = 0.0f;
		[NonSerialized, XmlIgnore] NormalizedCar? normalizedCarClassLeader = null;
		[NonSerialized, XmlIgnore] NormalizedCar? normalizedCarInFront = null;
		[NonSerialized, XmlIgnore] bool splitLeaderboard = false;

		[NonSerialized, XmlIgnore] float battleChyronTimer = 0;

		[NonSerialized, XmlIgnore] public bool forceShowRaceResult = false;
		[NonSerialized, XmlIgnore] public int raceResultPageCount = 0;
		[NonSerialized, XmlIgnore] public int raceResultCurrentPage = 0;
		[NonSerialized, XmlIgnore] float raceResultTimer = 0;

		[NonSerialized, XmlIgnore] public int introCarIdx = 0;

		private CustomClassSystem classSystem;

		static LiveData()
		{
			Instance = new LiveData();
		}

		private LiveData()
		{
			Instance = this;

			for ( var driverIndex = 0; driverIndex < liveDataDrivers.Length; driverIndex++ )
			{
				liveDataDrivers[ driverIndex ] = new LiveDataDriver();
			}

			chyronRandomItemNames = new Dictionary<string, string>
			{
				{ "FAV_REAL_TRACKS", "Favorite tracks" },
				{ "FAV_REAL_CARS", "Favorite cars" },
				{ "FAV_MOVIES", "Favorite movies" },
				{ "FAV_HOBBIES", "Hobbies" },
				{ "FAV_GAMES", "Favorite games" },
				{ "FAV_MUSIC", "Favorite music" },
				{ "FAV_TV_SHOWS", "Favorite TV shows" },
				{ "FAV_BOOKS", "Favorite books" },
				{ "FAV_QUOTATION", "Favorite saying" },
				{ "FAV_SPORTS", "Favorite sports" }
			};

			chyronAvailableItemLabels = new string[ chyronRandomItemNames.Count ];
			chyronAvailableItemValues = new string[ chyronRandomItemNames.Count ];

			for ( var i = 0; i < MaxNumCustom; i++ )
			{
				liveDataCustom[ i ] = new LiveDataCustom();
			}

			if (UseCustomClassSystem)
			{
				classSystem = CustomClassSystem.Instance;
			}
			
		}

		public void Update()
		{
			isConnected = IRSDK.isConnected;

			if ( Controller.currentMode == Controller.Mode.None )
			{
				systemMessage = string.Empty;
			}
			else
			{
				switch ( Controller.currentMode )
				{
					case Controller.Mode.Width:
						systemMessage = "Adjusting SteamVR Overlay Width";
						break;

					case Controller.Mode.PositionXY:
						systemMessage = "Adjusting SteamVR Overlay Position (X/Y)";
						break;

					case Controller.Mode.PositionZ:
						systemMessage = "Adjusting SteamVR Overlay Position (Z)";
						break;

					case Controller.Mode.Curvature:
						systemMessage = "Adjusting SteamVR Overlay Curvature";
						break;
				}
			}

			Settings.UpdateCombinedOverlay();

			UpdateSteamVr();
			UpdateControlPanel();
			UpdateDrivers();
			UpdateRaceStatus();
			UpdateLeaderboard( ref liveDataLeaderboardsWebPage, false );
			UpdateLeaderboard( ref liveDataLeaderboards, true );
			UpdateRaceResult();
			UpdateTrackMap();
			UpdatePitLane();
			UpdateVoiceOf();
			UpdateChyron();
			UpdateBattleChyron();
			UpdateSubtitle();
			UpdateIntro();
			UpdateStartLights();
			UpdateEventLog();
			UpdateHud();
			UpdateTrainer();
			UpdateWebcamStreaming();
			UpdateCustom();
			UpdateLapTimeComparision();

			seriesLogoTextureUrl = IRSDK.normalizedSession.seriesLogoTextureUrl;
			trackLogoTextureUrl = IRSDK.normalizedSession.trackLogoTextureUrl;
			trackTextureUrl = IRSDK.normalizedSession.trackMapTextureUrl;

			bool prevLiveSessionReplay = isLiveSessionReplay;
			isLiveSessionReplay = IsLiveSessionInReplayMode();
			SendLiveSessionReplayEvent(prevLiveSessionReplay);

			IPC.readyToSendLiveData = true;
		}


		public void UpdateSteamVr()
		{
			liveDataSteamVr.enabled = Settings.editor.editorSteamVrEnabled;
			liveDataSteamVr.width = Settings.editor.editorSteamVrWidth;
			liveDataSteamVr.position = Settings.editor.editorSteamVrPosition;
			liveDataSteamVr.curvature = Settings.editor.editorSteamVrCurvature;
		}

		public void UpdateControlPanel()
		{
			liveDataControlPanel.masterOn = MainWindow.Instance.masterOn;
			liveDataControlPanel.raceStatusOn = MainWindow.Instance.raceStatusOn;
			liveDataControlPanel.leaderboardOn = MainWindow.Instance.leaderboardOn;
			liveDataControlPanel.raceResultOn = MainWindow.Instance.raceResultOn;
			liveDataControlPanel.trackMapOn = MainWindow.Instance.trackMapOn;
			liveDataControlPanel.pitLaneOn = MainWindow.Instance.pitLaneOn;
			liveDataControlPanel.startLightsOn = MainWindow.Instance.startLightsOn;
			liveDataControlPanel.voiceOfOn = MainWindow.Instance.voiceOfOn;
			liveDataControlPanel.chyronOn = MainWindow.Instance.chyronOn;
			liveDataControlPanel.battleChyronOn = MainWindow.Instance.battleChyronOn;
			liveDataControlPanel.subtitlesOn = MainWindow.Instance.subtitlesOn;
			liveDataControlPanel.introOn = MainWindow.Instance.introOn;
			liveDataControlPanel.customLayerOn = MainWindow.Instance.customLayerOn;
			liveDataControlPanel.customLayerOn[3] = IsLiveSessionInReplayMode();
		}

		public void UpdateDrivers()
		{
			foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
			{
				liveDataDrivers[ normalizedCar.carIdx ].carLogoTextureUrl = normalizedCar.carLogoTextureUrl;
				liveDataDrivers[ normalizedCar.carIdx ].carNumberTextureUrl = normalizedCar.carNumberTextureUrl;
				liveDataDrivers[ normalizedCar.carIdx ].carTextureUrl = normalizedCar.carTextureUrl;
				liveDataDrivers[ normalizedCar.carIdx ].driverTextureUrl = normalizedCar.driverTextureUrl;
				liveDataDrivers[ normalizedCar.carIdx ].helmetTextureUrl = normalizedCar.helmetTextureUrl;
				liveDataDrivers[ normalizedCar.carIdx ].memberClubRegionTextureUrl = normalizedCar.memberClubTextureUrl;
				liveDataDrivers[ normalizedCar.carIdx ].memberIdTextureUrl_A = normalizedCar.memberIdTextureUrl_A;
				liveDataDrivers[ normalizedCar.carIdx ].memberIdTextureUrl_B = normalizedCar.memberIdTextureUrl_B;
				liveDataDrivers[ normalizedCar.carIdx ].memberIdTextureUrl_C = normalizedCar.memberIdTextureUrl_C;
			}
		}

		public void UpdateRaceStatus()
		{
			// lights

			liveDataRaceStatus.showBlackLight = false;
			liveDataRaceStatus.showGreenLight = false;
			liveDataRaceStatus.showWhiteLight = false;
			liveDataRaceStatus.showYellowLight = false;

			if ( IRSDK.normalizedData.isUnderCaution )
			{
				liveDataRaceStatus.showYellowLight = true;
			}
			else if ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedData.sessionState != SessionState.StateRacing ) )
			{
				liveDataRaceStatus.showBlackLight = true;
			}
			else if ( IRSDK.normalizedSession.isInRaceSession && ( ( IRSDK.normalizedData.sessionLapsRemaining == 1 ) || ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.White ) != 0 ) ) )
			{
				liveDataRaceStatus.showWhiteLight = true;
			}
			else
			{
				liveDataRaceStatus.showGreenLight = true;
			}

			Color color;

			liveDataRaceStatus.textLayer1 = GetTextContent( out color, "RaceStatusTextLayer1" );
			liveDataRaceStatus.textLayer2 = GetTextContent( out color, "RaceStatusTextLayer2" );
			//liveDataRaceStatus.textLayer3 = GetTextContent( out color, "RaceStatusTextLayer3" );
			//liveDataRaceStatus.textLayer4 = GetTextContent( out color, "RaceStatusTextLayer4" );

			// flags

			liveDataRaceStatus.showGreenFlag = false;
			liveDataRaceStatus.showYellowFlag = false;
			liveDataRaceStatus.showCheckeredFlag = false;

			if ( IRSDK.normalizedSession.isInRaceSession )
			{
				if ( IRSDK.normalizedData.sessionState >= SessionState.StateCheckered )
				{
					liveDataRaceStatus.showCheckeredFlag = true;
				}
				else if ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint)SessionFlags.Yellow | (uint)SessionFlags.YellowWaving ) ) != 0 )
				{
					liveDataRaceStatus.showYellowFlag = true;
				}
				else if ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.StartGo ) != 0 )
				{
					liveDataRaceStatus.showGreenFlag = true;
				}
			}

			if (IRSDK.normalizedSession.isInPracticeSession || IRSDK.normalizedSession.isInQualifyingSession)
			{
				if ( IRSDK.normalizedData.sessionState >= SessionState.StateCheckered )
				{
					liveDataRaceStatus.showCheckeredFlag = true;
				}
			}

			// one to green
			liveDataRaceStatus.showOneToGreen =
				(IRSDK.normalizedData.sessionFlags &
				 ((uint)(SessionFlags.OneLapToGreen | SessionFlags.StartSet | SessionFlags.StartReady | SessionFlags.StartHidden))) != 0;
		}

		public void UpdateTrackMap()
		{
			Color color;

			if ( TrackMap.initialized )
			{
				liveDataTrackMap.show = true;
				liveDataTrackMap.showPaceCar = false;
				liveDataTrackMap.trackID = TrackMap.trackID;
				liveDataTrackMap.width = TrackMap.width;
				liveDataTrackMap.height = TrackMap.height;
				liveDataTrackMap.startFinishLine = TrackMap.fullVectorList[ ( TrackMap.startFinishOffset + Settings.overlay.trackMapStartFinishOffset ) % TrackMap.numVectors ];
				liveDataTrackMap.drawVectorList = TrackMap.drawVectorList;

				foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars )
				{
					var liveDataTrackMapCar = liveDataTrackMap.liveDataTrackMapCars[ normalizedCar.carIdx ];

					liveDataTrackMapCar.show = normalizedCar.includeInLeaderboard && !normalizedCar.isOnPitRoad && !normalizedCar.isOutOfCar;
					liveDataTrackMapCar.offset = TrackMap.GetPosition( normalizedCar.lapDistPct );
					liveDataTrackMapCar.textLayer1 = GetTextContent( out color, "TrackMapCarTextLayer1", normalizedCar );
					liveDataTrackMapCar.showHighlight = ( normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx );

					if ( normalizedCar.isPaceCar )
					{
						if ( !normalizedCar.isOnPitRoad )
						{
							var lapDistPctDelta = normalizedCar.lapDistPct - paceCarDistPct;

							var distanceMovedInMeters = lapDistPctDelta * IRSDK.normalizedSession.trackLengthInMeters;
							var speedInMetersPerSecond = distanceMovedInMeters / (float) IRSDK.normalizedData.sessionTimeDelta;

							if ( speedInMetersPerSecond >= 5 )
							{
								liveDataTrackMap.showPaceCar = true;
								liveDataTrackMap.paceCarOffset = TrackMap.GetPosition( normalizedCar.lapDistPct );
							}
						}

						paceCarDistPct = normalizedCar.lapDistPct;
					}
				}
			}
			else
			{
				liveDataTrackMap.show = false;
				liveDataTrackMap.showPaceCar = false;
				liveDataTrackMap.trackID = 0;
				liveDataTrackMap.width = 0;
				liveDataTrackMap.height = 0;
				liveDataTrackMap.startFinishLine = Vector3.zero;
				liveDataTrackMap.drawVectorList = null;
			}
		}

		public void UpdateVoiceOf()
		{
			liveDataVoiceOf.show = false;

			if ( IRSDK.normalizedData.radioTransmitCarIdx != -1 )
			{
				liveDataVoiceOf.show = true;

				var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.normalizedData.radioTransmitCarIdx );

				if ( normalizedCar != null )
				{
					Color color;

					liveDataVoiceOf.textLayer1 = GetTextContent( out color, "VoiceOfTextLayer1", normalizedCar );
					liveDataVoiceOf.textLayer2 = GetTextContent( out color, "VoiceOfTextLayer2", normalizedCar );

					liveDataVoiceOf.carIdx = IRSDK.normalizedData.radioTransmitCarIdx;
				}

				if ( liveDataControlPanel.voiceOfOn )
				{
					Director.chyronTimer = 0;
				}
			}
		}

		public void UpdateSubtitle()
		{
			var subtitleData = SubtitlePlayback.GetCurrentSubtitleData();

			liveDataSubtitle.text = ( subtitleData == null ) ? string.Empty : subtitleData.Text;
		}

		public void UpdateIntro()
		{
			Color color;

			if ( IRSDK.normalizedData.sessionTimeDelta < 0 )
			{
				liveDataIntro.show = false;
			}
			else if ( IRSDK.normalizedData.sessionTimeDelta >= 0 )
			{
				liveDataIntro.show = false;

				if ( IRSDK.normalizedSession.isInRaceSession )
				{
					var numRows = (int) Math.Ceiling( IRSDK.normalizedData.numLeaderboardCars / 2.0 );

					var animationDuration = Settings.overlay.introInTime + Settings.overlay.introHoldTime + Settings.overlay.introOutTime;

					var introStartTime = Math.Min( Settings.overlay.introLeftStartTime, Settings.overlay.introRightStartTime );
					var introEndTime = Math.Max( Settings.overlay.introLeftStartTime, Settings.overlay.introRightStartTime ) + ( numRows - 1 ) * Settings.overlay.introStartInterval + animationDuration;

					if ( ( IRSDK.normalizedData.sessionTime >= introStartTime ) && ( IRSDK.normalizedData.sessionTime < introEndTime ) )
					{
						liveDataIntro.show = true;

						if ( IRSDK.normalizedData.numLeaderboardCars > 0 )
						{
							var timePerCar = Settings.overlay.introStartInterval / 2.0;
							var currentTime = ( IRSDK.normalizedData.sessionTime - introStartTime ) - ( Settings.overlay.introInTime + ( Settings.overlay.introHoldTime - Settings.overlay.introStartInterval ) );
							var driverIndex = Math.Max( 0, (int) Math.Round( currentTime / timePerCar ) );

							if ( driverIndex < IRSDK.normalizedData.numLeaderboardCars )
							{
								var normalizedCar = IRSDK.normalizedData.leaderboardSortedNormalizedCars[ driverIndex ];

								introCarIdx = normalizedCar.carIdx;
							}
						}

						for ( var driverIndex = 0; driverIndex < liveDataIntro.liveDataIntroDrivers.Length; driverIndex++ )
						{
							var liveDataIntroDriver = liveDataIntro.liveDataIntroDrivers[ driverIndex ];

							var normalizedCar = IRSDK.normalizedData.leaderboardSortedNormalizedCars[ driverIndex ];

							if ( normalizedCar.includeInLeaderboard && ( normalizedCar.qualifyingPosition < MaxNumDrivers ) )
							{
								var rowNumber = Math.Floor( driverIndex / 2.0 );
								var driverStartTime = ( ( ( driverIndex & 1 ) == 0 ) ? Settings.overlay.introLeftStartTime : Settings.overlay.introRightStartTime ) + rowNumber * Settings.overlay.introStartInterval;
								var driverEndTime = driverStartTime + animationDuration;

								liveDataIntroDriver.show = ( IRSDK.normalizedData.sessionTime >= driverStartTime ) && ( IRSDK.normalizedData.sessionTime < driverEndTime );
								liveDataIntroDriver.carIdx = normalizedCar.carIdx;
								liveDataIntroDriver.textLayer1 = GetTextContent( out color, "IntroDriverTextLayer1", normalizedCar );
								liveDataIntroDriver.textLayer2 = GetTextContent( out color, "IntroDriverTextLayer2", normalizedCar );
								liveDataIntroDriver.textLayer3 = GetTextContent( out color, "IntroDriverTextLayer3", normalizedCar );
								liveDataIntroDriver.textLayer4 = GetTextContent( out color, "IntroDriverTextLayer4", normalizedCar );
								liveDataIntroDriver.textLayer5 = GetTextContent( out color, "IntroDriverTextLayer5", normalizedCar );
								liveDataIntroDriver.textLayer6 = GetTextContent( out color, "IntroDriverTextLayer6", normalizedCar );
							}
							else
							{
								liveDataIntroDriver.show = false;
							}
						}
					}
				}
			}
		}

		public void UpdateEventLog()
		{
			liveDataEventLog.messages = EventLog.messages;
		}

		public void UpdateTrainer()
		{
			liveDataTrainer.message = Trainer.message;
			liveDataTrainer.countdown = Trainer.countdown;
		}

		public void UpdateWebcamStreaming()
		{
			liveDataWebcamStreaming.enabled = Settings.editor.editorWebcamStreamingEnabled;
			liveDataWebcamStreaming.webserverURL = Settings.editor.editorWebcamStreamingWebserverURL;
			liveDataWebcamStreaming.roomCode = Settings.editor.editorWebcamStreamingRoomCode;
		}

		public void UpdateCustom()
		{
			
			for ( var i = 0; i < MaxNumCustom; i++ )
			{
				var layerNumber = i + 1;

				var custom = liveDataCustom[ i ];
        
				// Only set custom properties if the corresponding layer is actually on
				if (!MainWindow.Instance.customLayerOn[i]) 
				{
					continue;
				}

				var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.normalizedData.camCarIdx );

				if ( normalizedCar != null )
				{
					custom.carIdx = normalizedCar.carIdx;

					custom.textLayer1 = GetTextContent( out custom.textLayer1Color, $"Custom{layerNumber}TextLayer1", normalizedCar );
					custom.textLayer2 = GetTextContent( out custom.textLayer2Color, $"Custom{layerNumber}TextLayer2", normalizedCar );
					custom.textLayer3 = GetTextContent( out custom.textLayer3Color, $"Custom{layerNumber}TextLayer3", normalizedCar );
				}
			}
		}


		private enum ComparisionMode
		{
			Ahead,
			Behind
		}
		
		private static float? GetLapTimeComparisionExactLap(NormalizedCar car, ComparisionMode m, int exactLap)
		{
			NormalizedCar? target;
			switch (m)
			{
				case ComparisionMode.Ahead:
					target = car.normalizedCarInFront;
					break;
				case ComparisionMode.Behind:
					target = car.normalizedCarBehind;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(m), m, null);
			}

			if (target == null)
			{
				return null;
			}

			float myLap = car.GetCurrentLapMinusNLapTime(exactLap);
			float targetLap = target.GetCurrentLapMinusNLapTime(exactLap);

			if (myLap < 0 || targetLap < 0)
			{
				return null;
			}


			return targetLap - myLap;
		}
		
		private static float? GetLapTimeComparision(NormalizedCar car, ComparisionMode m, int minsNLaps)
		{
			NormalizedCar? target;
			switch (m)
			{
				case ComparisionMode.Ahead:
					target = car.normalizedCarInFront;
					break;
				case ComparisionMode.Behind:
					target = car.normalizedCarBehind;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(m), m, null);
			}

			if (target == null)
			{
				return null;
			}

			float myLap = car.GetCurrentLapMinusNLapTime(minsNLaps);
			float targetLap = target.GetCurrentLapMinusNLapTime(minsNLaps);

			if (myLap < 0 || targetLap < 0)
			{
				return null;
			}


			return targetLap - myLap;
		}
		
		/// <summary>
		/// Checks if the current session is live but temporarily replaying a section
		/// </summary>
		/// <returns>True if it's a live session in replay mode, false otherwise</returns>
		public static bool IsLiveSessionInReplayMode()
		{
			// First make sure iRacing is connected
			if (!IRSDK.isConnected || IRSDK.data == null)
				return false;

			// Check if we're in a live session (not a full replay)
			bool isLiveSession = !IRSDK.normalizedSession.isReplay;

			bool rplay = Math.Abs(IRSDK.data.ReplaySessionTime - IRSDK.data.SessionTime) > 5f;
			
			// Return true if it's a live session with an active replay
			return isLiveSession && rplay;
		}

		private static string FormatMeasurementAsInt(string? measurement)
		{
			if (string.IsNullOrWhiteSpace(measurement))
				return string.Empty;
    
			// Find the first non-digit, non-decimal character
			int unitStartIndex = -1;
			for (int i = 0; i < measurement.Length; i++)
			{
				if (!char.IsDigit(measurement[i]) && measurement[i] != '.')
				{
					unitStartIndex = i;
					break;
				}
			}
    
			// If no unit part found, just try to parse and round the whole string
			if (unitStartIndex == -1)
			{
				if (double.TryParse(measurement, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
				{
					return ((int)Math.Round(value)).ToString();
				}
				return measurement;
			}
    
			// Extract numeric part and unit part
			string numericPart = measurement.Substring(0, unitStartIndex).Trim();
			string unitPart = measurement.Substring(unitStartIndex).Trim();
    
			// Parse and round the numeric part
			if (double.TryParse(numericPart, NumberStyles.Float, CultureInfo.InvariantCulture, out double numValue))
			{
				return $"{(int)Math.Round(numValue)} {unitPart}";
			}
    
			// Return original if parsing fails
			return measurement;
		}


		private static string ReturnErrorInDebugOrBlankInRelease()
		{
#if DEBUG
			return "error";
#else
						return "";
#endif
		}

		private static void GetSectorStatusColorBlob(SectorLapStatus sector, StringBuilder sb)
		{
			GetSectorStatusColour(sector, sb);
			sb.Append("I</color>");
		}

		private static void GetSectorStatusColour(SectorLapStatus sector, StringBuilder sb)
		{
			switch (sector.Status)
			{
				case SectorStatus.NotCompleted:
					sb.Append("<color=grey>");
					break;
				case SectorStatus.Regular:
					sb.Append("<color=white>");
					break;
				case SectorStatus.PersonalBest:
					sb.Append("<color=green>");
					break;
				case SectorStatus.SessionBestInClass:
					sb.Append("<color=purple>");
					break;
				case SectorStatus.SessionBestOverall:
					const string pink = "#FF00CB";
					sb.Append($"<color={pink}>");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		// iRacing wind dir is in radians, 0 at East, increasing CCW.
		// Map to 16-point compass (E, ENE, NE, ..., ESE).
		private static string RadiansToCompassFromEastCCW(double radians)
		{
			var twoPi = Math.PI * 2.0;
			radians %= twoPi;
			if (radians < 0) radians += twoPi;

			var degrees = radians * 180.0 / Math.PI;

			string[] dirs = {
				"E","ENE","NE","NNE","N","NNW","NW","WNW",
				"W","WSW","SW","SSW","S","SSE","SE","ESE"
			};

			int index = (int)Math.Round(degrees / 22.5) % 16;
			return dirs[index];
		}

		public static Color GetTextColor( SettingsText settingsText, NormalizedCar? normalizedCar )
		{
			var tintColor = settingsText.tintColor;

			if ( settingsText.useClassColors )
			{
				if ( normalizedCar != null )
				{
					tintColor = Color.Lerp( tintColor, normalizedCar.classColor, settingsText.classColorStrength );
				}
			}

			return tintColor;
		}

		public static string GetCsvProperty( SettingsText settingsText, NormalizedCar? normalizedCar )
		{
			if ( ( IRSDK.driverCsvFile != null ) && ( normalizedCar != null ) )
			{
				if ( IRSDK.driverCsvFile.ContainsKey( normalizedCar.userId ) )
				{
					var record = IRSDK.driverCsvFile[ normalizedCar.userId ];

					if ( record != null )
					{
						if ( record.ContainsKey( settingsText.csvProperty ) )
						{
							var value = record[ settingsText.csvProperty ];

							if ( value != null )
							{
								return (string) value;
							}
						}
						else
						{
							return "(key not found)";
						}
					}
				}
			}

			return string.Empty;
		}

		public static string ReplaceString( string targetString )
		{
			if ( IRSDK.stringsCsvFile != null )
			{
				if ( IRSDK.stringsCsvFile.ContainsKey( targetString ) )
				{
					return IRSDK.stringsCsvFile[ targetString ];
				}
			}

			return targetString;
		}
		
		/// <summary>
		/// Converts wind velocity from m/s to kph and formats it as a string
		/// </summary>
		/// <param name="windVelString">Wind velocity string in format "X.XX m/s"</param>
		/// <returns>Formatted string in kph, rounded to nearest whole number</returns>
		public static string FormatWindVelocityAsKph(string windVelString)
		{
			if (string.IsNullOrWhiteSpace(windVelString))
				return string.Empty;
    
			// Extract numeric value from "X.XX m/s" format
			var numericPart = windVelString.Replace("m/s", "", StringComparison.OrdinalIgnoreCase).Trim();
			if (double.TryParse(numericPart, NumberStyles.Float, CultureInfo.InvariantCulture, out double windVelMps))
			{
				// Convert m/s to kph (1 m/s = 3.6 kph) and round to nearest integer
				int windVelKph = (int)Math.Round(windVelMps * 3.6);
				return $"{windVelKph} kph";
			}
    
			// Return original if parsing fails
			return windVelString;
		}


		public static string GetOrdinal( int number )
		{
			if ( number <= 0 )
			{
				return number.ToString();
			}

			switch ( number % 100 )
			{
				case 11:
				case 12:
				case 13:
					return number + "th";
			}

			switch ( number % 10 )
			{
				case 1:
					return number + "st";
				case 2:
					return number + "nd";
				case 3:
					return number + "rd";
				default:
					return number + "th";
			}
		}
	}


}
