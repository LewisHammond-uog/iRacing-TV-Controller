
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using System.Globalization;
using System.Linq;
using Aydsko.iRacingData.Leagues;
using irsdkSharp.Serialization.Enums.Fastest;
using irsdkSharp.Serialization.Models.Session.SessionInfo;
using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveData
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

			isLiveSessionReplay = IsLiveSessionInReplayMode();

			IPC.readyToSendLiveData = true;
		}

		private void UpdateLapTimeComparision()
		{
			var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.normalizedData.camCarIdx );
			if (normalizedCar == null)
			{
				return;
			}

			string FormName(NormalizedCar? car)
			{
				return car == null ? "" : $"{car.familyName} #{car.carNumber}";
			}
			
			liveDataLapComp.Clear();
			
			liveDataLapComp.aheadCarIdX = normalizedCar?.normalizedCarInFront?.carIdx ?? -1;
			liveDataLapComp.aheadName = FormName(normalizedCar?.normalizedCarBehind);
			
			liveDataLapComp.behindCarIdX = normalizedCar?.normalizedCarBehind?.carIdx ?? -1;
			liveDataLapComp.behindName = FormName(normalizedCar?.normalizedCarInFront);
			
			liveDataLapComp.currentIdX = normalizedCar?.carIdx ?? -1;
			liveDataLapComp.currentName = FormName(normalizedCar);

			for (int i = 0; i < LiveDataLapComp.historyCount; i++)
			{
				liveDataLapComp.carBehindLastLapsDiff[i] = GetTextContent(out _, "BehindGapOverLaps", normalizedCar, extraInt: i);
				liveDataLapComp.carAheadLastLapsDiff[i] = GetTextContent(out _, "AheadGapOverLaps", normalizedCar, extraInt: i);
				liveDataLapComp.thisCarLaps[i] = Program.GetTimeString(normalizedCar.GetCurrentLapMinusNLapTime(i), true);

				if (IRSDK.normalizedData.lapNumber - i > 1)
				{
					liveDataLapComp.lapNums[i] = $"LAP {IRSDK.normalizedData.lapNumber - i}" ;
				}
				else
				{
					liveDataLapComp.lapNums[i] = "";
				}

			}
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

		public void UpdateLeaderboard( ref LiveDataLeaderboard[]? liveDataLeaderboards, bool splitLeaderboard )
		{
			//Make slot count be total, regarless of how many classes
			int slotCount = (int)Math.Floor((double) Settings.overlay.leaderboardSlotCount /
			                            (double) IRSDK.normalizedData.numLeaderboardClasses);
			
			
			// save setting

			this.splitLeaderboard = splitLeaderboard;

			// allocate leaderboards

			if ( ( liveDataLeaderboards == null ) || ( liveDataLeaderboards.Length != IRSDK.normalizedData.numLeaderboardClasses ) )
			{
				liveDataLeaderboards = new LiveDataLeaderboard[ IRSDK.normalizedData.numLeaderboardClasses ];

				for ( var leaderboardIndex = 0; leaderboardIndex < liveDataLeaderboards.Length; leaderboardIndex++ )
				{
					liveDataLeaderboards[ leaderboardIndex ] = new LiveDataLeaderboard();
				}
			}

			var leaderboardOffset = Vector2.zero;

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
			
			
			for ( var classIndex = 0; classIndex < IRSDK.normalizedData.numLeaderboardClasses; classIndex++ )
			{
				var currentLiveDataLeaderboard = liveDataLeaderboards[ classIndex ];
				var currentLeaderboardClass = IRSDK.normalizedData.leaderboardClass[ classIndex ];
				var currentClassID = currentLeaderboardClass.classID;

				// leaderboard splits

				var bottomSplitSlotCount = slotCount / 2;
				var bottomSplitLastSlotIndex = slotCount;

				if ( !IRSDK.normalizedSession.isInQualifyingSession )
				{
					if ( bottomSplitSlotCount > 0 )
					{
						foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars)
						{
							if ( !normalizedCar.includeInLeaderboard )
							{
								break;
							}

							if ( !Settings.overlay.leaderboardSeparateBoards || (classSystem.GetClassForCar(normalizedCar)?.ClassName == currentClassID ) )
							{
								if ( normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx )
								{
									if ( normalizedCar.displayedPosition > bottomSplitLastSlotIndex )
									{
										while ( bottomSplitLastSlotIndex < normalizedCar.displayedPosition )
										{
											bottomSplitLastSlotIndex += bottomSplitSlotCount;
										}

										if ( bottomSplitLastSlotIndex > currentLeaderboardClass.numDrivers )
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

				if ( !splitLeaderboard )
				{
					topSplitLastSlotIndex = MaxNumDrivers;
					bottomSplitFirstSlotIndex = MaxNumDrivers + 1;
					bottomSplitLastSlotIndex = MaxNumDrivers + 1;
				}

				// leaderboard

				Color color;

				var myClass = CustomClassSystem.Instance.GetClasses().FirstOrDefault(c => c.ClassName == currentClassID) ;

				currentLiveDataLeaderboard.show = false;
				currentLiveDataLeaderboard.classColor = classSystem.GetColourForClass(currentLeaderboardClass.name);
				currentLiveDataLeaderboard.textLayer1 = GetTextContent( out currentLiveDataLeaderboard.classColor, "LeaderboardTextLayer1", null, myClass );
				currentLiveDataLeaderboard.textLayer2 = GetTextContent( out currentLiveDataLeaderboard.classColor, "LeaderboardTextLayer2", null, myClass );

				normalizedCarClassLeader = null;
				normalizedCarInFront = null;
				classLeaderBestLapTime = 0.0f;

				var carsShown = 0;

				// reset leaderboard slots to be hidden

				foreach ( var liveDataLeaderboardSlot in currentLiveDataLeaderboard.liveDataLeaderboardSlots )
				{
					liveDataLeaderboardSlot.show = false;
				}

				// go through cars for this class

				foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars )
				{
					// skip cars with wrong car class

					if ( Settings.overlay.leaderboardSeparateBoards && ( normalizedCar.classID != currentClassID ) )
					{
						continue;
					}

					// get slot

					LiveDataLeaderboardSlot liveDataLeaderboardSlot = currentLiveDataLeaderboard.liveDataLeaderboardSlots[ normalizedCar.carIdx ];

					// skip pace car and spectators

					if ( normalizedCar.includeInLeaderboard )
					{
						// class leader best lap time

						if ( normalizedCarClassLeader == null )
						{
							normalizedCarClassLeader = normalizedCar;

							classLeaderBestLapTime = normalizedCar.bestLapTime;
						}

						// check if the car is visible on the leaderboard

						liveDataLeaderboardSlot.show = ( ( ( normalizedCar.displayedPosition >= topSplitFirstSlotIndex ) && ( normalizedCar.displayedPosition <= topSplitLastSlotIndex ) ) || ( ( normalizedCar.displayedPosition >= bottomSplitFirstSlotIndex ) && ( normalizedCar.displayedPosition <= bottomSplitLastSlotIndex ) ) );

						//Set Red
						
						
						// hide cars that have not qualified yet (only during qualifying)

						if ( IRSDK.normalizedSession.isInQualifyingSession )
						{
							if ( normalizedCar.bestLapTime == 0)
							{
								liveDataLeaderboardSlot.show = false;
							}
						}
					}

					// skip cars not visible on the leaderboard

					if ( !liveDataLeaderboardSlot.show )
					{
						if ( splitLeaderboard )
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

						var slotIndex = normalizedCar.displayedPosition - ( ( normalizedCar.displayedPosition >= bottomSplitFirstSlotIndex ) ? bottomSplitFirstSlotIndex - topSplitLastSlotIndex : topSplitFirstSlotIndex );

						// compute slot offset

						if ( splitLeaderboard )
						{
							var resetSlotOffset = ( ( lastFrameBottomSplitFirstPosition[ classIndex ] != bottomSplitFirstSlotIndex ) && ( normalizedCar.displayedPosition >= bottomSplitFirstSlotIndex ) );

							var targetSlotOffset = new Vector2( Settings.overlay.leaderboardSlotSpacing.x, -Settings.overlay.leaderboardSlotSpacing.y ) * slotIndex + new Vector2( Settings.overlay.leaderboardFirstSlotPosition.x, -Settings.overlay.leaderboardFirstSlotPosition.y );

							if ( normalizedCar.wasVisibleOnLeaderboard && !resetSlotOffset )
							{
								normalizedCar.leaderboardSlotOffset += ( targetSlotOffset - normalizedCar.leaderboardSlotOffset ) * 0.15f;
							}
							else
							{
								normalizedCar.leaderboardSlotOffset = targetSlotOffset;
							}

							liveDataLeaderboardSlot.offset = normalizedCar.leaderboardSlotOffset;
						}

						//
						

						liveDataLeaderboardSlot.textLayer1 = GetTextContent( out liveDataLeaderboardSlot.textLayer1Color, "LeaderboardPositionTextLayer1", normalizedCar, normalizedCar.carClass );
						liveDataLeaderboardSlot.textLayer2 = GetTextContent( out liveDataLeaderboardSlot.textLayer2Color, "LeaderboardPositionTextLayer2", normalizedCar, normalizedCar.carClass );
						liveDataLeaderboardSlot.textLayer3 = GetTextContent( out liveDataLeaderboardSlot.textLayer3Color, "LeaderboardPositionTextLayer3", normalizedCar, normalizedCar.carClass );
						liveDataLeaderboardSlot.textLayer4 = GetTextContent( out liveDataLeaderboardSlot.textLayer4Color, "LeaderboardPositionTextLayer4", normalizedCar, normalizedCar.carClass );

						if (normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx)
						{
							liveDataLeaderboardSlot.textLayer1Color = Color.red;
						}
						
						
						
						//any pens?
						liveDataLeaderboardSlot.penaltyFlag = normalizedCar.sessionFlags.HasAnyFlag(SessionFlags.Black | SessionFlags.Crossed | SessionFlags.Disqualify);
						liveDataLeaderboardSlot.slowDownFlag = normalizedCar.sessionFlags.HasAnyFlag(SessionFlags.Furled);
						liveDataLeaderboardSlot.meatballFlag = normalizedCar.sessionFlags.HasAnyFlag(SessionFlags.Repair);
						liveDataLeaderboardSlot.finished = normalizedCar.hasCrossedFinishLine;
						
						
						//put pit / outlap
						liveDataLeaderboardSlot.textLayer5 = GetTextContent(out liveDataLeaderboardSlot.textLayer4Color, "LeaderboardPositionTextLayer5", normalizedCar, normalizedCar.carClass);
						

						// preferred driver

						liveDataLeaderboardSlot.showPreferredCar = normalizedCar.isPreferredCar;

						//

						if ( splitLeaderboard )
						{
							normalizedCar.wasVisibleOnLeaderboard = true;
						}

						normalizedCarInFront = normalizedCar;
					}
				}

				if ( splitLeaderboard )
				{
					// leaderboard offset and background and splitter

					currentLiveDataLeaderboard.offset = new Vector2( leaderboardOffset.x, leaderboardOffset.y );
					currentLiveDataLeaderboard.backgroundSize = Settings.overlay.leaderboardSlotSpacing * Math.Min( carsShown, slotCount );
					currentLiveDataLeaderboard.showSplitter = ( ( topSplitLastSlotIndex + 1 ) != bottomSplitFirstSlotIndex );
					currentLiveDataLeaderboard.splitterPosition = Settings.overlay.leaderboardFirstSlotPosition + Settings.overlay.leaderboardSlotSpacing * topSplitLastSlotIndex;

					if ( currentLiveDataLeaderboard.show )
					{
						if ( Settings.overlay.leaderboardMultiClassOffsetType == 0 )
						{
							leaderboardOffset.y += currentLiveDataLeaderboard.backgroundSize.y;
						}

						leaderboardOffset += Settings.overlay.leaderboardMultiClassOffset;
					}

					// remember the bottom split first position

					lastFrameBottomSplitFirstPosition[ classIndex ] = bottomSplitFirstSlotIndex;
				}
			}
		}

		public void UpdateRaceResult()
		{
			// figure out how many race result pages we have

			raceResultPageCount = 0;

			var classes = CustomClassSystem.Instance.GetClasses().ToList();
			for ( var i = 0; i < classes.Count; i++ )
			{
				raceResultPageCount += 1 + ( ( classes[ i ].CarNums.Count - 1 ) / Settings.overlay.raceResultSlotCount );
			}

			// don't show race result until it is time

			if ( !forceShowRaceResult )
			{
				liveDataRaceResult.show = false;
				return;
				if ( !IRSDK.normalizedSession.isInRaceSession || ( IRSDK.normalizedData.sessionState < SessionState.StateCoolDown ) )
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

				if ( raceResultTimer < Settings.overlay.raceResultStartTime )
				{
					liveDataRaceResult.show = false;

					return;
				}

				// are we done showing the race results?

				var timeOffset = raceResultTimer - Settings.overlay.raceResultStartTime;

				if ( ( totalDuration > 0 ) && ( timeOffset > totalDuration ) )
				{
					liveDataRaceResult.show = false;

					return;
				}

				// figure out which page we are on

				if ( totalDuration > 0 )
				{
					raceResultCurrentPage = (int) Math.Floor( timeOffset / Settings.overlay.raceResultInterval );
				}
			}

			// set up page metadata

			var pageClassIndex = new int[ raceResultPageCount ];
			var pageSlotIndex = new int[ raceResultPageCount ];

			var slotIndex = 0;
			var pageIndex = 0;
			
			for ( var i = 0; i < CustomClassSystem.Instance.GetClassCount(); i++ )
			{
				slotIndex = 0;

				var pagesThisClass = 1 + ( ( classes[i].CarNums.Count - 1 ) / Settings.overlay.raceResultSlotCount );

				for ( var j = 0; j < pagesThisClass; j++ )
				{
					pageClassIndex[ pageIndex ] = i;
					pageSlotIndex[ pageIndex ] = slotIndex;

					slotIndex += Settings.overlay.raceResultSlotCount;

					pageIndex++;
				}
			}

			// build the race result

			Color color;

			CustomClassSystem.CarClass raceResultClass = classes[ pageClassIndex[ raceResultCurrentPage ] ];

			liveDataRaceResult.show = true;

			liveDataRaceResult.backgroundSize = Settings.overlay.raceResultSlotSpacing * Settings.overlay.raceResultSlotCount;
			liveDataRaceResult.classColor = raceResultClass.Colour;
			liveDataRaceResult.textLayer1 = GetTextContent( out color, "RaceResultTextLayer1", null, raceResultClass );
			liveDataRaceResult.textLayer2 = GetTextContent( out color, "RaceResultTextLayer2", null, raceResultClass );

			// clear out the race result slots

			for ( var i = 0; i < liveDataRaceResult.liveDataRaceResultSlots.Length; i++ )
			{
				var liveDataRaceResultSlot = liveDataRaceResult.liveDataRaceResultSlots[ i ];

				liveDataRaceResultSlot.show = false;
			}

			// build the race result slotss

			slotIndex = 0;

			var numSlots = Math.Min( Settings.overlay.raceResultSlotCount, ( raceResultClass.CarNums.Count - pageSlotIndex[ raceResultCurrentPage ] ) );

			if (IRSDK.normalizedSession.sessionNumber < 0)
			{
				return;
			}
			
			if(IRSDK.session.SessionInfo.Sessions[IRSDK.normalizedSession.sessionNumber].ResultsPositions == null)
			{
				return;
			}
			
			foreach ( var posCar in IRSDK.session.SessionInfo.Sessions[IRSDK.normalizedSession.sessionNumber].ResultsPositions )
			{
				var normalizedCar = IRSDK.normalizedData.normalizedCars.First(nc => nc.carIdx == posCar.CarIdx);
				
				if ( !normalizedCar.includeInLeaderboard )
				{
					continue;
				}

				if ( normalizedCar.classID != raceResultClass.ClassName )
				{
					continue;
				}

				if ( slotIndex >= pageSlotIndex[ raceResultCurrentPage ] )
				{
					var liveDataRaceResultSlot = liveDataRaceResult.liveDataRaceResultSlots[ normalizedCar.carIdx ];

					liveDataRaceResultSlot.show = true;

					liveDataRaceResultSlot.showPreferredCar = false;
					liveDataRaceResultSlot.offset = new Vector2( Settings.overlay.raceResultSlotSpacing.x, -Settings.overlay.raceResultSlotSpacing.y ) * ( slotIndex - pageSlotIndex[ raceResultCurrentPage ] ) + new Vector2( Settings.overlay.raceResultFirstSlotPosition.x, -Settings.overlay.raceResultFirstSlotPosition.y );
					liveDataRaceResultSlot.textLayer1 = GetTextContent( out liveDataRaceResultSlot.textLayer1Color, "RaceResultPositionTextLayer1", normalizedCar, normalizedCar.carClass );
					liveDataRaceResultSlot.textLayer2 = GetTextContent( out liveDataRaceResultSlot.textLayer2Color, "RaceResultPositionTextLayer2", normalizedCar, normalizedCar.carClass );
					liveDataRaceResultSlot.textLayer3 = GetTextContent( out liveDataRaceResultSlot.textLayer3Color, "RaceResultPositionTextLayer3", normalizedCar, normalizedCar.carClass );
					liveDataRaceResultSlot.textLayer4 = GetTextContent( out liveDataRaceResultSlot.textLayer4Color, "RaceResultPositionTextLayer4", normalizedCar, normalizedCar.carClass );
					liveDataRaceResultSlot.textLayer5 = GetTextContent( out liveDataRaceResultSlot.textLayer5Color, "RaceResultPositionTextLayer5", normalizedCar, normalizedCar.carClass );
					liveDataRaceResultSlot.textLayer6 = GetTextContent( out liveDataRaceResultSlot.textLayer6Color, "RaceResultPositionTextLayer6", normalizedCar, normalizedCar.carClass );

					numSlots--;

					if ( numSlots == 0 )
					{
						break;
					}
				}

				slotIndex++;
			}
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

		public void UpdatePitLane()
		{
			Color color;

			if ( trackIdLastFrame != IRSDK.normalizedSession.trackID )
			{
				trackIdLastFrame = IRSDK.normalizedSession.trackID;

				pitLaneTouched = false;

				pitLaneMinLapDistPct = 0;
				pitLaneMaxLapDistPct = 0;
			}

			if ( !pitLaneTouched )
			{
				foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
				{
					if ( normalizedCar.includeInLeaderboard && normalizedCar.isOnPitRoad )
					{
						pitLaneTouched = true;

						pitLaneMinLapDistPct = normalizedCar.lapDistPct;
						pitLaneMaxLapDistPct = normalizedCar.lapDistPct;

						break;
					}
				}
			}

			liveDataPitLane.show = false;

			if ( pitLaneTouched )
			{
				foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
				{
					if ( normalizedCar.includeInLeaderboard && normalizedCar.isOnPitRoad )
					{
						liveDataPitLane.show = true;

						var lapDistPct = normalizedCar.lapDistPct;

						var deltaLapDistPct = lapDistPct - pitLaneMinLapDistPct;

						if ( deltaLapDistPct <= -0.5 )
						{
							lapDistPct += 1;
						}
						else if ( deltaLapDistPct >= 0.5 )
						{
							lapDistPct -= 1;
						}

						if ( lapDistPct < pitLaneMinLapDistPct )
						{
							pitLaneMinLapDistPct = lapDistPct;
						}

						lapDistPct = normalizedCar.lapDistPct;

						deltaLapDistPct = lapDistPct - pitLaneMaxLapDistPct;

						if ( deltaLapDistPct <= -0.5 )
						{
							lapDistPct += 1;
						}
						else if ( deltaLapDistPct >= 0.5 )
						{
							lapDistPct -= 1;
						}

						if ( lapDistPct > pitLaneMaxLapDistPct )
						{
							pitLaneMaxLapDistPct = lapDistPct;
						}
					}
				}

				var adjustedMaxLapDistPct = pitLaneMaxLapDistPct;

				if ( adjustedMaxLapDistPct < pitLaneMinLapDistPct )
				{
					adjustedMaxLapDistPct += 1;
				}

				var length = adjustedMaxLapDistPct - pitLaneMinLapDistPct;

				foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
				{
					var liveDataPitLaneCar = liveDataPitLane.liveDataPitLaneCars[ normalizedCar.carIdx ];

					if ( normalizedCar.includeInLeaderboard && normalizedCar.isOnPitRoad && ( length > 0 ) )
					{
						liveDataPitLaneCar.show = true;
						liveDataPitLaneCar.showHighlight = ( normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx );

						var lapDistPct = normalizedCar.lapDistPct;

						var deltaLapDistPct = lapDistPct - pitLaneMinLapDistPct;

						if ( deltaLapDistPct <= -0.5 )
						{
							lapDistPct += 1;
						}
						else if ( deltaLapDistPct >= 0.5 )
						{
							lapDistPct -= 1;
						}

						var offset = Settings.overlay.pitLaneLength * ( ( lapDistPct - pitLaneMinLapDistPct ) / length );

						liveDataPitLaneCar.offset = new Vector3( offset, 0, 0 );
						liveDataPitLaneCar.textLayer1 = GetTextContent( out color, "PitLaneCarTextLayer1", normalizedCar );
					}
					else
					{
						liveDataPitLaneCar.show = false;
					}
				}
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

		public void UpdateChyron()
		{
			var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.normalizedData.camCarIdx );

			if ( ( normalizedCar != null ) && normalizedCar.includeInLeaderboard && Director.showChyron && ( !liveDataControlPanel.voiceOfOn || ( IRSDK.normalizedData.radioTransmitCarIdx == -1 ) ) )
			{
				Color color;

				liveDataChyron.show = true;

				liveDataChyron.textLayer1 = GetTextContent( out color, "ChyronTextLayer1", normalizedCar );
				liveDataChyron.textLayer2 = GetTextContent( out color, "ChyronTextLayer2", normalizedCar );
				liveDataChyron.textLayer3 = GetTextContent( out color, "ChyronTextLayer3", normalizedCar );
				liveDataChyron.textLayer4 = GetTextContent( out color, "ChyronTextLayer4", normalizedCar );
				liveDataChyron.textLayer5 = GetTextContent( out color, "ChyronTextLayer5", normalizedCar );
				liveDataChyron.textLayer6 = GetTextContent( out color, "ChyronTextLayer6", normalizedCar );
				liveDataChyron.textLayer7 = GetTextContent( out color, "ChyronTextLayer7", normalizedCar );
				liveDataChyron.textLayer8 = GetTextContent( out color, "ChyronTextLayer8", normalizedCar );
				liveDataChyron.textLayer9 = GetTextContent( out color, "ChyronTextLayer9", normalizedCar );
				liveDataChyron.textLayer10 = GetTextContent( out color, "ChyronTextLayer10", normalizedCar );
				liveDataChyron.textLayer11 = GetTextContent( out color, "ChyronTextLayer11", normalizedCar );
				liveDataChyron.textLayer12 = GetTextContent( out color, "ChyronTextLayer12", normalizedCar );
				liveDataChyron.textLayer13 = GetTextContent( out color, "ChyronTextLayer13", normalizedCar );
				liveDataChyron.textLayer14 = GetTextContent( out color, "ChyronTextLayer14", normalizedCar );
				liveDataChyron.textLayer15 = GetTextContent( out color, "ChyronTextLayer15", normalizedCar );

				liveDataChyron.carIdx = normalizedCar.carIdx;
			}
			else
			{
				liveDataChyron.show = false;
			}
		}

		public void UpdateBattleChyron()
		{
			liveDataBattleChyron.show = false;

			if ( !IRSDK.normalizedSession.isInRaceSession )
			{
				return;
			}

			if ( ( IRSDK.normalizedData.sessionState < SessionState.StateRacing ) || ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.Caution | (uint) SessionFlags.GreenHeld ) ) != 0 ) )
			{
				battleChyronTimer = Settings.overlay.battleChyronDelay;

				return;
			}

			if ( battleChyronTimer > 0 )
			{
				battleChyronTimer -= Program.deltaTime;

				if ( battleChyronTimer > 0 )
				{
					return;
				}
			}

			if ( ( IRSDK.currentCameraType == SettingsDirector.CameraType.Inside ) || ( IRSDK.currentCameraType == SettingsDirector.CameraType.Close ) )
			{
				var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.normalizedData.camCarIdx );

				if ( ( normalizedCar != null ) && normalizedCar.includeInLeaderboard && !normalizedCar.isOnPitRoad && Director.showChyron && ( !liveDataControlPanel.voiceOfOn || ( IRSDK.normalizedData.radioTransmitCarIdx == -1 ) ) )
				{
					var nearestDeltaLapPosition = float.MaxValue;
					NormalizedCar? nearestNormalizedCar = null;

					if ( normalizedCar.normalizedCarInFront != null )
					{
						var deltaLapPosition = Math.Abs( normalizedCar.lapPosition - normalizedCar.normalizedCarInFront.lapPosition );

						if ( deltaLapPosition < 0.5f )
						{
							nearestDeltaLapPosition = deltaLapPosition;
							nearestNormalizedCar = normalizedCar.normalizedCarInFront;
						}
					}

					if ( nearestNormalizedCar != null )
					{
						var distanceInMeters = nearestDeltaLapPosition * IRSDK.normalizedSession.trackLengthInMeters;

						if ( distanceInMeters <= Settings.overlay.battleChyronDistance )
						{
							Color color;

							liveDataBattleChyron.show = true;

							liveDataBattleChyron.textLayer1 = GetTextContent( out color, "BattleChyronTextLayer1", nearestNormalizedCar );
							liveDataBattleChyron.textLayer2 = GetTextContent( out color, "BattleChyronTextLayer2", nearestNormalizedCar );
							liveDataBattleChyron.textLayer3 = GetTextContent( out color, "BattleChyronTextLayer3", nearestNormalizedCar );
							liveDataBattleChyron.textLayer4 = GetTextContent( out color, "BattleChyronTextLayer4", nearestNormalizedCar );
							liveDataBattleChyron.textLayer5 = GetTextContent( out color, "BattleChyronTextLayer5", nearestNormalizedCar );
							liveDataBattleChyron.textLayer6 = GetTextContent( out color, "BattleChyronTextLayer6", nearestNormalizedCar );
							liveDataBattleChyron.textLayer7 = GetTextContent( out color, "BattleChyronTextLayer7", nearestNormalizedCar );
							liveDataBattleChyron.textLayer8 = GetTextContent( out color, "BattleChyronTextLayer8", nearestNormalizedCar );
							liveDataBattleChyron.textLayer9 = GetTextContent( out color, "BattleChyronTextLayer9", nearestNormalizedCar );
							liveDataBattleChyron.textLayer10 = GetTextContent( out color, "BattleChyronTextLayer10", nearestNormalizedCar );
							liveDataBattleChyron.textLayer11 = GetTextContent( out color, "BattleChyronTextLayer11", nearestNormalizedCar );
							liveDataBattleChyron.textLayer12 = GetTextContent( out color, "BattleChyronTextLayer12", nearestNormalizedCar );
							liveDataBattleChyron.textLayer13 = GetTextContent( out color, "BattleChyronTextLayer13", nearestNormalizedCar );
							liveDataBattleChyron.textLayer14 = GetTextContent( out color, "BattleChyronTextLayer14", nearestNormalizedCar );
							liveDataBattleChyron.textLayer15 = GetTextContent( out color, "BattleChyronTextLayer15", nearestNormalizedCar );

							liveDataBattleChyron.carIdx = nearestNormalizedCar.carIdx;
						}
					}
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

		public void UpdateStartLights()
		{
			liveDataStartLights.showReady = false;
			liveDataStartLights.showSet = false;
			liveDataStartLights.showGo = false;

			if ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.StartGo ) != 0 )
			{
				liveDataStartLights.showGo = true;
			}
			else if ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.StartSet ) != 0 )
			{
				liveDataStartLights.showSet = true;
			}
			else if ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.StartReady ) != 0 )
			{
				if ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.OneLapToGreen ) != 0 )
				{
					if ( ( IRSDK.normalizedData.paceCar == null ) || ( IRSDK.normalizedData.paceCar.isOnPitRoad && ( IRSDK.normalizedData.paceCar.lapDistPct > 0.5f ) ) )
					{
						liveDataStartLights.showReady = true;
					}
				}
				else if ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.StartHidden ) == 0 )
				{
					liveDataStartLights.showReady = true;
				}
			}
		}

		public void UpdateEventLog()
		{
			liveDataEventLog.messages = EventLog.messages;
		}

		public void UpdateHud()
		{
			if ( IRSDK.data == null )
			{
				return;
			}

			// player car

			var normalizedCar = IRSDK.normalizedData.normalizedCars[ IRSDK.data.PlayerCarIdx ];

			liveDataHud.textLayer1 = GetTextContent( out liveDataHud.textLayer1Color, "HudTextLayer1", normalizedCar );
			liveDataHud.textLayer2 = GetTextContent( out liveDataHud.textLayer2Color, "HudTextLayer2", normalizedCar );
			liveDataHud.textLayer3 = GetTextContent( out liveDataHud.textLayer3Color, "HudTextLayer3", normalizedCar );
			liveDataHud.textLayer4 = GetTextContent( out liveDataHud.textLayer4Color, "HudTextLayer4", normalizedCar );
			liveDataHud.textLayer5 = GetTextContent( out liveDataHud.textLayer5Color, "HudTextLayer5", normalizedCar );
			liveDataHud.textLayer6 = GetTextContent( out liveDataHud.textLayer6Color, "HudTextLayer6", normalizedCar );
			liveDataHud.textLayer7 = GetTextContent( out liveDataHud.textLayer7Color, "HudTextLayer7", normalizedCar );
			liveDataHud.textLayer8 = GetTextContent( out liveDataHud.textLayer8Color, "HudTextLayer8", normalizedCar );
			liveDataHud.textLayer9 = GetTextContent( out liveDataHud.textLayer9Color, "HudTextLayer9", normalizedCar );
			liveDataHud.textLayer10 = GetTextContent( out liveDataHud.textLayer10Color, "HudTextLayer10", normalizedCar );
			liveDataHud.textLayer11 = GetTextContent( out liveDataHud.textLayer11Color, "HudTextLayer11", normalizedCar );
			liveDataHud.textLayer12 = GetTextContent( out liveDataHud.textLayer12Color, "HudTextLayer12", normalizedCar );
			liveDataHud.textLayer13 = GetTextContent( out liveDataHud.textLayer13Color, "HudTextLayer13", normalizedCar );
			liveDataHud.textLayer14 = GetTextContent( out liveDataHud.textLayer14Color, "HudTextLayer14", normalizedCar );
			liveDataHud.textLayer15 = GetTextContent( out liveDataHud.textLayer15Color, "HudTextLayer15", normalizedCar );
			liveDataHud.textLayer16 = GetTextContent( out liveDataHud.textLayer16Color, "HudTextLayer16", normalizedCar );

			// speech to text

			var recognizedString = SpeechToText.GetRecognizingString();

			if ( recognizedString == string.Empty )
			{
				if ( speechToTextTimer > 0 )
				{
					speechToTextTimer -= Program.deltaTime;

					if ( speechToTextTimer <= 0.0f )
					{
						speechToTextTimer = 0;

						liveDataHud.speechToText = string.Empty;
					}
				}
			}
			else
			{
				liveDataHud.speechToText = recognizedString;

				speechToTextTimer = 15.0f;
			}

			// spotter indicators

			var carsLeft = 0;
			var carsRight = 0;

			switch ( (CarLeftRight) IRSDK.data.CarLeftRight )
			{
				case CarLeftRight.LRCarLeft:
					carsLeft = 1;
					break;
				case CarLeftRight.LR2CarsLeft:
					carsLeft = 2;
					break;
				case CarLeftRight.LRCarRight:
					carsRight = 1;
					break;
				case CarLeftRight.LR2CarsRight:
					carsRight = 2;
					break;
				case CarLeftRight.LRCarLeftRight:
					carsLeft = 2;
					carsRight = 2;
					break;
			}

			if ( carsLeft == 1 )
			{
				liveDataHud.showLeftSpotterIndicator = true;
			}
			else if ( carsLeft == 2 )
			{
				liveDataHud.showLeftSpotterIndicator = ( Program.elapsedMilliseconds % 250 ) >= 100;
			}
			else
			{
				liveDataHud.showLeftSpotterIndicator = false;
			}

			if ( carsRight == 1 )
			{
				liveDataHud.showRightSpotterIndicator = true;
			}
			else if ( carsRight == 2 )
			{
				liveDataHud.showRightSpotterIndicator = ( Program.elapsedMilliseconds % 250 ) >= 100;
			}
			else
			{
				liveDataHud.showRightSpotterIndicator = false;
			}
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

		public string GetTextContent( out Color color, string key, NormalizedCar? normalizedCar = null, CustomClassSystem.CarClass? leaderboardClass = null, int extraInt = 0 )
		{
			var settingsText = Settings.overlay.textSettingsDataDictionary[ key ];
			
			var session = IRSDK.normalizedSession.sessionNumber;
			var results = CustomClassSystem.Instance.Results;

			color = GetTextColor( settingsText, normalizedCar );

			switch ( settingsText.content )
			{
				case SettingsText.Content.None:

					return "";

				case SettingsText.Content.Driver_CarNumber:

					return normalizedCar?.carNumber ?? "";

				case SettingsText.Content.Driver_CsvProperty:

					return GetCsvProperty( settingsText, normalizedCar );

				case SettingsText.Content.Driver_CarBehind_CarNumber:

					return ( normalizedCar?.normalizedCarBehind != null ) ? $"#{normalizedCar.normalizedCarBehind.carNumber}" : "";

				case SettingsText.Content.Driver_CarBehind_CsvProperty:

					return GetCsvProperty( settingsText, normalizedCar?.normalizedCarBehind );

				case SettingsText.Content.Driver_CarBehind_GapTime:
				{
					var text = "-.--";

					if ( normalizedCar != null )
					{
						if ( normalizedCar.normalizedCarBehind != null )
						{
							text = $"{normalizedCar.gapTimeBack:0.00}";

							var deltaLapPositionRelativeToClassLeader = normalizedCar.normalizedCarBehind.lapPositionRelativeToClassLeader - normalizedCar.lapPositionRelativeToClassLeader;

							if ( deltaLapPositionRelativeToClassLeader < -0.5f )
							{
								color = new Color( 1.0f, 0.3f, 0.3f, color.a );
							}
							else if ( deltaLapPositionRelativeToClassLeader > 0.5f )
							{
								color = new Color( 0.4f, 0.4f, 1.0f, color.a );
							}
						}
					}

					return text;
				}

				case SettingsText.Content.Driver_CarBehind_Name:

					return normalizedCar?.normalizedCarBehind?.displayedName ?? "";

				case SettingsText.Content.Driver_CarBehind_Position:

					return ( normalizedCar?.normalizedCarBehind?.displayedPosition >= 1 ) ? "P" + normalizedCar.normalizedCarBehind.displayedPosition.ToString() : "";

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
					
					StringBuilder sectorTimes = new StringBuilder();;
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

					return IRSDK.session?.WeekendInfo.TrackDisplayShortName + " - " + IRSDK.session?.WeekendInfo.TrackConfigName;
				
				case SettingsText.Content.Track_CityCounty:

					return IRSDK.session?.WeekendInfo.TrackCity + ", " +  IRSDK.session?.WeekendInfo.TrackCountry;
				case SettingsText.Content.Track_ExtraInfo:

					StringBuilder extraInfo = new StringBuilder();
					var km = IRSDK.normalizedSession.trackLengthInMeters / 1000.0f;
					
					extraInfo.AppendLine($"<b>Length</b>:<pos=60%> {km:0.00} KM");
					extraInfo.AppendLine($"<b>Turns</b>:<pos=60%> {IRSDK.session?.WeekendInfo.TrackNumTurns} ");
					extraInfo.AppendLine($"<b>Altitude</b>:<pos=60%> {FormatMeasurementAsInt(IRSDK.session?.WeekendInfo.TrackAltitude)} ");
					extraInfo.AppendLine($"<b>Pit Speed</b>:<pos=60%> {FormatMeasurementAsInt(IRSDK.session?.WeekendInfo.TrackPitSpeedLimit)} ");
					extraInfo.AppendLine($"<b>Track Temp</b>:<pos=60%> {FormatMeasurementAsInt(IRSDK.session?.WeekendInfo.TrackSurfaceTemp)} ");
					extraInfo.AppendLine($"<b>Air Temp</b>:<pos=60%> {FormatMeasurementAsInt(IRSDK.session?.WeekendInfo.TrackAirTemp)} ");
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

					return ( normalizedCar?.normalizedCarInFront != null ) ? $"#{normalizedCar.normalizedCarInFront.carNumber}" : "";

				case SettingsText.Content.Driver_CarInFront_CsvProperty:

					return GetCsvProperty( settingsText, normalizedCar?.normalizedCarInFront );

				case SettingsText.Content.Driver_CarInFront_GapTime:
				{
					var text = "-.--";

					if ( normalizedCar != null )
					{
						if ( normalizedCar.normalizedCarInFront != null )
						{
							text = $"{normalizedCar.gapTimeFront:0.00}";

							var deltaLapPositionRelativeToClassLeader = normalizedCar.normalizedCarInFront.lapPositionRelativeToClassLeader - normalizedCar.lapPositionRelativeToClassLeader;

							if ( deltaLapPositionRelativeToClassLeader < -0.5f )
							{
								color = new Color( 1.0f, 0.3f, 0.3f, color.a );
							}
							else if ( deltaLapPositionRelativeToClassLeader > 0.5f )
							{
								color = new Color( 0.4f, 0.4f, 1.0f, color.a );
							}
						}
					}

					return text;
				}

				case SettingsText.Content.Driver_CarInFront_Name:

					return normalizedCar?.normalizedCarInFront?.displayedName ?? "";

				case SettingsText.Content.Driver_CarInFront_Position:

					return ( normalizedCar?.normalizedCarInFront?.displayedPosition >= 1 ) ? "P" + normalizedCar.normalizedCarInFront.displayedPosition.ToString() : "";

				case SettingsText.Content.Driver_CarInFront_UserID:

					return normalizedCar?.normalizedCarInFront?.userId.ToString() ?? "";

				case SettingsText.Content.Driver_CarInFront_LapTimeDiff:
				{
					if (normalizedCar == null)
					{
						return String.Empty;
					}

					float? rawTime = GetLapTimeComparision(normalizedCar, ComparisionMode.Ahead, extraInt);
					if (rawTime == null)
					{
						return "NO LAP";
					}

					string plusMinus = rawTime.Value > 0 ? "+" : "-";
					color = rawTime.Value > 0 ? Color.red : Color.green; 
					string str = $"{plusMinus}{Program.GetTimeString(rawTime.Value, true)}";
					
					return str;
				}

				
				case SettingsText.Content.Driver_CarBehind_LapTimeDiff:
				{
					if (normalizedCar == null)
					{
						return String.Empty;
					}
					
					float? rawTime = GetLapTimeComparision(normalizedCar, ComparisionMode.Behind, extraInt);
					if (rawTime == null)
					{
						return "NO LAP";
					}
					
					string plusMinus = rawTime.Value > 0 ? "+" : "-";
					color = rawTime.Value > 0 ? Color.red : Color.green; 
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
					if ( normalizedCar != null )
					{
						if ( normalizedCar.gear == -1 )
						{
							color = new Color( 1, 0.25f, 0.25f, 1 );

							return "R";
						}
						else if ( normalizedCar.gear == 0 )
						{
							color = new Color( 1, 1, 0.25f, 1 );

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

					if ( normalizedCar != null )
					{
						text = $"{normalizedCar.interpolatedDeltaTime:+0.00;-0.00; 0.00} | {normalizedCar.interpolatedDeltaInterpolatedDeltaTime:+0.00;-0.00; 0.00}";

						if ( normalizedCar.interpolatedDeltaInterpolatedDeltaTime <= 0 )
						{
							color = Color.Lerp( Color.white, green, (float) Math.Min( 1, -normalizedCar.interpolatedDeltaInterpolatedDeltaTime ) );
						}
						else
						{
							color = Color.Lerp( Color.white, red, (float) Math.Min( 1, normalizedCar.interpolatedDeltaInterpolatedDeltaTime ) );
						}
					}

					return text;
				}

				case SettingsText.Content.Driver_LapsBehindClassLeader:
				{
					var text = "";

					if ( normalizedCar != null )
					{
						if ( IRSDK.normalizedSession.isInRaceSession && ( normalizedCar.lapPositionRelativeToClassLeader > 0 ) )
						{
							text = $"{normalizedCar.lapPositionRelativeToClassLeader:0.000}";
						}
					}

					return text;
				}

				case SettingsText.Content.Driver_LapsLed:
				{
					if ( ( normalizedCar != null ) && ( normalizedCar.lapsLed > 0 ) )
					{
						return normalizedCar.lapsLed.ToString();
					}
					else
					{
						return "";
					}
				}

				case SettingsText.Content.Driver_LapTime_Current:

					if ( normalizedCar != null )
					{
						if (IRSDK.normalizedSession.isInRaceSession)
						{
							return "";
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
						
						
						if ( ( normalizedCar.currentLapTime < 5 ) && ( normalizedCar.lastLapTime > 0 ) )
						{
							return Program.GetTimeString( normalizedCar.lastLapTime, true );
						}
						else if ( normalizedCar.currentLapTime > 0 )
						{
							return Program.GetTimeString( normalizedCar.currentLapTime, true );
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

					if ( ( normalizedCar != null ) && ( normalizedCar.lastLapTime > 0 ) )
					{
						return Program.GetTimeString( normalizedCar.lastLapTime, true );
					}
					else
					{
						return "--.---";
					}

				case SettingsText.Content.Driver_License:
				{
					if ( normalizedCar != null )
					{
						color = new Color( normalizedCar.licenseColor );

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

					return ( normalizedCar?.displayedPosition >= 1 ) ? normalizedCar.displayedPosition.ToString() : "";

				case SettingsText.Content.Driver_Position_WithP:

					return ( normalizedCar?.displayedPosition >= 1 ) ? "P" + normalizedCar.displayedPosition.ToString() : "";

				case SettingsText.Content.Driver_Position_Ordinal:

					return ( normalizedCar?.displayedPosition >= 1 ) ? GetOrdinal( normalizedCar.displayedPosition ) : "";
				
				case SettingsText.Content.Driver_Position_FinalResults:
					if (normalizedCar == null)
					{
						return ReturnErrorInDebugOrBlankInRelease();
					}

					var pos = CustomClassSystem.Instance?.Results?.GetCarClassPosition(normalizedCar);
					return pos == null ? ReturnErrorInDebugOrBlankInRelease() : GetOrdinal(pos.Value);

				case SettingsText.Content.Driver_QualifyLapTime_1:

					return ( normalizedCar?.qualifyingLapTimes[ 0 ] > 0 ) ? $"{normalizedCar.qualifyingLapTimes[ 0 ]:0.000}" : "--.---";

				case SettingsText.Content.Driver_QualifyLapTime_2:

					return ( normalizedCar?.qualifyingLapTimes[ 1 ] > 0 ) ? $"{normalizedCar.qualifyingLapTimes[ 1 ]:0.000}" : "--.---";

				case SettingsText.Content.Driver_QualifyLapTime_3:

					return ( normalizedCar?.qualifyingLapTimes[ 2 ] > 0 ) ? $"{normalizedCar.qualifyingLapTimes[ 2 ]:0.000}" : "--.---";

				case SettingsText.Content.Driver_QualifyLapTime_4:

					return ( normalizedCar?.qualifyingLapTimes[ 3 ] > 0 ) ? $"{normalizedCar.qualifyingLapTimes[ 3 ]:0.000}" : "--.---";

				case SettingsText.Content.Driver_QualifyPosition:

					return ( normalizedCar?.qualifyingClassPosition >= 1 ) ? normalizedCar.qualifyingClassPosition.ToString() : "";

				case SettingsText.Content.Driver_QualifyPosition_WithP:

					return ( normalizedCar?.qualifyingClassPosition >= 1 ) ? "P" + classSystem.GetPositionInClass(normalizedCar).ToString() : "";

				case SettingsText.Content.Driver_QualifyPosition_Ordinal:

					return ( normalizedCar?.qualifyingClassPosition >= 1 ) ? GetOrdinal( normalizedCar.qualifyingClassPosition ) : "";
				
				case SettingsText.Content.Driver_OverallQualityPosition_WithP:
					
					return ( normalizedCar?.qualifyingPosition >= 1 ) ? "P" + normalizedCar.qualifyingPosition.ToString() : "";
				
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
					
					return ( localPos >= 1 ) ? $"P{localPos} ({normalizedCar.classID})" : ""; 

				case SettingsText.Content.Driver_QualifyTime:
				{
					if ( normalizedCar != null )
					{
						if ( normalizedCar.qualifyingTime < 0 )
						{
							return Settings.overlay.translationDictionary[ "DidNotQualify" ].translation;
						}
						else if ( normalizedCar.qualifyingTime == 0 )
						{
							return "";
						}
						else
						{
							return Program.GetTimeString( normalizedCar.qualifyingTime, true );
						}
					}
					else
					{
						return "";
					}
				}

				case SettingsText.Content.Driver_Rating:

					if ( normalizedCar != null )
					{
						return normalizedCar.iRating.ToString();
					}
					else
					{
						return "";
					}

				case SettingsText.Content.Driver_RPM:
				{
					if ( normalizedCar != null )
					{
						return $"{normalizedCar.rpm:0}";
					}
					else
					{
						return "";
					}
				}

				case SettingsText.Content.Driver_Speed:

					if ( normalizedCar == null )
					{
						return "";
					}
					else
					{
						return $"{Math.Abs( normalizedCar.speedInMetersPerSecond ) * ( IRSDK.normalizedData.displayIsMetric ? 3.6f : 2.23694f ):0} {( IRSDK.normalizedData.displayIsMetric ? Settings.overlay.translationDictionary[ "KPH" ].translation : Settings.overlay.translationDictionary[ "MPH" ].translation )}";
					}

				case SettingsText.Content.Driver_Telemetry:
				{
					if ( normalizedCar == null )
					{
						return "";
					}
					else
					{
						var sign = Settings.overlay.telemetryShowAsNegativeNumbers ? "-" : "+";

						var text = string.Empty;

						if ( IRSDK.normalizedSession.isInPracticeSession || IRSDK.normalizedSession.isInQualifyingSession )
						{
							if ( normalizedCar.bestLapTime > 0 )
							{
								if ( classLeaderBestLapTime == normalizedCar.bestLapTime )
								{
									text = Program.GetTimeString( classLeaderBestLapTime, true );
								}
								else
								{
									var deltaTime = normalizedCar.bestLapTime - classLeaderBestLapTime;

									text = $"{sign}{deltaTime:0.000}";
								}
							}

							normalizedCar.checkpointTime = 0;
						}
						else if ( normalizedCar.isOnPitRoad )
						{
							text = Settings.overlay.translationDictionary[ "Pit" ].translation;
							color = Settings.overlay.telemetryPitColor;

							normalizedCar.checkpointTime = 0;
						}
						else if ( normalizedCar.isOutOfCar && normalizedCar.outOfCarTimer > 5f)
						{
							text = Settings.overlay.translationDictionary[ "Out" ].translation;
							color = Settings.overlay.telemetryOutColor;

							normalizedCar.checkpointTime = 0;
						}
						else if ( IRSDK.normalizedSession.isInRaceSession )
						{
							if ( normalizedCar.hasCrossedStartLine )
							{
								if ( normalizedCar.lapPositionRelativeToClassLeader >= 1.0f )
								{
									var wholeLapsDown = Math.Floor( normalizedCar.lapPositionRelativeToClassLeader );

									text = $"-{wholeLapsDown:0} {Settings.overlay.translationDictionary[ "LapsAbbreviation" ].translation}";

									normalizedCar.checkpointTime = 0;
								}else if (normalizedCarInFront == null) //No Car in front = leader!
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
								else if ( !IRSDK.normalizedData.isUnderCaution && ( normalizedCarInFront != null ) )
								{
									if ( !normalizedCar.hasCrossedFinishLine && !normalizedCarInFront.hasCrossedFinishLine )
									{
										var lapPosition = Settings.overlay.telemetryIsBetweenCars ? ( normalizedCarInFront.lapPosition - normalizedCar.lapPosition ) : normalizedCar.lapPositionRelativeToClassLeader;

										if ( Settings.overlay.telemetryMode == 0 )
										{
											text = $"{sign}{lapPosition:0.000} {Settings.overlay.translationDictionary[ "LapsAbbreviation" ].translation}";
										}
										else if ( Settings.overlay.telemetryMode == 1 )
										{
											var distance = lapPosition * IRSDK.normalizedSession.trackLengthInMeters;

											if ( IRSDK.normalizedData.displayIsMetric )
											{
												var distanceString = $"{distance:0}";

												if ( distanceString != "0" )
												{
													text = $"{sign}{distanceString} {Settings.overlay.translationDictionary[ "MetersAbbreviation" ].translation}";
												}
											}
											else
											{
												distance *= 3.28084f;

												var distanceString = $"{distance:0}";

												if ( distanceString != "0" )
												{
													text = $"{sign}{distanceString} {Settings.overlay.translationDictionary[ "FeetAbbreviation" ].translation}";
												}
											}
										}
										else
										{
											if ( Settings.overlay.telemetryIsBetweenCars )
											{
												if ( normalizedCarInFront.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ] > 0 )
												{
													if ( !splitLeaderboard )
													{
														var checkpointTime = Math.Abs( (float) ( normalizedCar.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ] - normalizedCarInFront.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ] ) );

														if ( ( normalizedCar.checkpointTime != 0 ) && ( normalizedCar.normalizedCarForTelemetry != null ) && ( normalizedCarInFront.carIdx == normalizedCar.normalizedCarForTelemetry.carIdx ) )
														{
															normalizedCar.checkpointTime = normalizedCar.checkpointTime * 0.95f + checkpointTime * 0.05f;
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
											else if ( normalizedCarClassLeader != null )
											{
												if ( normalizedCarClassLeader.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ] > 0 )
												{
													if ( !splitLeaderboard )
													{
														var checkpointTime = Math.Abs( (float) ( normalizedCar.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ] - normalizedCarClassLeader.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ] ) );

														if ( ( normalizedCar.checkpointTime != 0 ) && ( normalizedCar.normalizedCarForTelemetry != null ) && ( normalizedCarClassLeader.carIdx == normalizedCar.normalizedCarForTelemetry.carIdx ) )
														{
															normalizedCar.checkpointTime = normalizedCar.checkpointTime * 0.95f + checkpointTime * 0.05f;
														}
														else
														{
															normalizedCar.normalizedCarForTelemetry = normalizedCarClassLeader;

															normalizedCar.checkpointTime = checkpointTime;
														}
													}

													text = $"{sign}{normalizedCar.checkpointTime:0.00}";
												}
											}
										}

										if ( text == string.Empty )
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

					if ( IRSDK.normalizedData.highestLapFuelLevelDelta > 0 )
					{
						var fuelLapsRemaining = IRSDK.normalizedData.fuelLevel / IRSDK.normalizedData.highestLapFuelLevelDelta;

						text = $"{fuelLapsRemaining:0.00}";

						if ( ( IRSDK.normalizedData.isInTimedRace && ( fuelLapsRemaining <= 2.0f ) ) || ( !IRSDK.normalizedData.isInTimedRace && ( fuelLapsRemaining < IRSDK.normalizedData.sessionLapsRemaining ) ) )
						{
							color = new Color( 1, 0.25f, 0.25f, 1 );
						}
					}

					return text;
				}

				case SettingsText.Content.Player_RPM:
				{
					var text = "----";

					if ( normalizedCar != null )
					{
						var steppedRPM = (int) Math.Floor( normalizedCar.rpm / 10 ) * 10;

						text = steppedRPM.ToString();

						if ( normalizedCar.gear < IRSDK.normalizedSession.numForwardGears )
						{
							if ( normalizedCar.rpm >= IRSDK.normalizedSession.blinkRpm )
							{
								color = new Color( 1, 0.2f, 0.2f, 1 );
							}
							else if ( normalizedCar.rpm >= IRSDK.normalizedSession.redlineRpm )
							{
								color = new Color( 1, 1, 0.2f, 1 );
							}
							else if ( normalizedCar.rpm >= IRSDK.normalizedSession.shiftRpm )
							{
								color = new Color( 0.2f, 1, 0.2f, 1 );
							}
						}
					}

					return text;
				}

				case SettingsText.Content.Session_CurrentLap:
				{
					if ( IRSDK.normalizedData.isInTimedRace || !IRSDK.normalizedSession.isInRaceSession )
					{
						return Program.GetTimeString( Math.Floor( IRSDK.normalizedData.sessionTimeTotal - IRSDK.normalizedData.sessionTimeRemaining ), false ) + " | " + Program.GetTimeString( IRSDK.normalizedData.sessionTimeTotal, false );
					}
					else
					{
						return IRSDK.normalizedData.lapNumber.ToString() + " | " + IRSDK.normalizedData.sessionLapsTotal.ToString();
					}
				}

				case SettingsText.Content.Session_LapsRemaining:
				{
					if ( IRSDK.normalizedData.isInTimedRace || !IRSDK.normalizedSession.isInRaceSession )
					{
						return Program.GetTimeString( Math.Ceiling( IRSDK.normalizedData.sessionTimeRemaining ), false );
					}
					else if ( IRSDK.normalizedData.sessionLapsRemaining == 1 )
					{
						return Settings.overlay.translationDictionary[ "FinalLap" ].translation;
					}
					else
					{
						var lapsRemaining = Math.Min( IRSDK.normalizedData.sessionLapsTotal, IRSDK.normalizedData.sessionLapsRemaining );

						return lapsRemaining.ToString() + " " + Settings.overlay.translationDictionary[ "ToGo" ].translation;
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
					
					CustomClassSystem.Instance.Results?.UpdateFromPositionModel(IRSDK.session.SessionInfo.Sessions[session].ResultsPositions);

					int pts = results.GetPointsForCar(normalizedCar).TotalPoints;
					if (normalizedCar.carNumberRaw is 30 or 13)
					{
						pts += 1;
					}
					
					return results == null ? "" : $"{pts} PTS";


					break;
				
				case SettingsText.Content.RaceBounusPoints:
					CustomClassSystem.Instance.Results?.UpdateFromPositionModel(IRSDK.session.SessionInfo.Sessions[session].ResultsPositions);

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
					
					CustomClassSystem.Instance.Results?.UpdateFromPositionModel(IRSDK.session.SessionInfo.Sessions[session].ResultsPositions);

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
						}else if (normalizedCar.lastPitLap == normalizedCar.currentLap)
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
					if ( Settings.overlay.translationDictionary.ContainsKey( IRSDK.normalizedSession.sessionName ) )
					{
						return Settings.overlay.translationDictionary[ IRSDK.normalizedSession.sessionName ].translation;
					}
					else
					{
						return IRSDK.normalizedSession.sessionName;
					}
				}

				case SettingsText.Content.Translation_Gear:

					return Settings.overlay.translationDictionary[ "Gear" ].translation;

				case SettingsText.Content.Translation_License:

					return Settings.overlay.translationDictionary[ "License" ].translation;

				case SettingsText.Content.Translation_Rating:

					return Settings.overlay.translationDictionary[ "iRating" ].translation;

				case SettingsText.Content.Translation_RPM:

					return Settings.overlay.translationDictionary[ "RPM" ].translation;

				case SettingsText.Content.Translation_Speed:

					return Settings.overlay.translationDictionary[ "Speed" ].translation;

				case SettingsText.Content.Translation_Units:
				{
					if ( IRSDK.normalizedData.isInTimedRace || !IRSDK.normalizedSession.isInRaceSession )
					{
						return Settings.overlay.translationDictionary[ "Time" ].translation;
					}
					else
					{
						return Settings.overlay.translationDictionary[ "Lap" ].translation;
					}
				}

				case SettingsText.Content.Translation_VoiceOf:

					return Settings.overlay.translationDictionary[ "VoiceOf" ].translation;
			}

			return "";
		}



		private enum ComparisionMode
		{
			Ahead,
			Behind
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
