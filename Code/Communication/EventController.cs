using System;
using System.Windows.Controls.Primitives;
using StreamDeckCommunicator;

namespace iRacingTVController.Communication;

public class EventController
{
	public EventController(ServerMessagePipe server)
	{
		server.OnMessageReceivedEvent += OnGetMessage;
	}

	private void OnGetMessage(object? sender, MessageReceviedEventArgs e)
	{
		Console.WriteLine($"Message Recevied {e.evt.ToString()}");
		
		bool champOn = MainWindow.Instance.customLayerOn[2];
		bool resultsOn = MainWindow.Instance.customLayerOn[4];
		
		switch (e.evt)
		{
			case Events.Cam_SetMode_Cockpit:
				MainWindow.Instance.SetManualCamera(SettingsDirector.CameraType.Inside);
				break;
			case Events.Cam_SetMode_Close:
				MainWindow.Instance.SetManualCamera(SettingsDirector.CameraType.Close);
				break;
			case Events.Cam_SetMode_Medium:
				MainWindow.Instance.SetManualCamera(SettingsDirector.CameraType.Medium);
				break;
			case Events.Cam_SetMode_Far:
				MainWindow.Instance.SetManualCamera(SettingsDirector.CameraType.Far);
				break;
			case Events.Cam_SetMode_VeryFar:
				MainWindow.Instance.SetManualCamera(SettingsDirector.CameraType.VeryFar);
				break;
			case Events.Cam_SetMode_Auto:
				MainWindow.Instance.SetManualCamera(SettingsDirector.CameraType.AutoCam);
				break;
			case Events.Cam_SetMode_StartFinish:
				MainWindow.Instance.SetManualCamera(SettingsDirector.CameraType.StartFinish);
				break;
			case Events.Cam_SetMode_Pits:
				MainWindow.Instance.SetManualCamera(SettingsDirector.CameraType.Pits);
				break;
			case Events.Cam_SetMode_Reverse:
				MainWindow.Instance.SetManualCamera(SettingsDirector.CameraType.Reverse);
				break;
			case Events.GoLiveNow:
			case Events.ResetCam:
				MainWindow.GoLiveNow();
				MainWindow.ResetCam();
				break;
			case Events.HoldCam:
				MainWindow.HoldCam();
				break;
			case Events.DirectorOn:
				MainWindow.Instance.EnableDirectorNow();
				break;
			case Events.MasterOverlay_Toggle:
				ToggleButton(MainWindow.Instance.ControlPanel_Master_Button);
				break;
			case Events.Overlay_ToggleStandingsSide:
				ToggleButton(MainWindow.Instance.ControlPanel_Leaderboard_Button);
				break;
			case Events.Overlay_Intro:
				MainWindow.Instance.customLayerOn[0] = !MainWindow.Instance.customLayerOn[0];
				MainWindow.Instance.ControlPanel_C1_Button.IsChecked = MainWindow.Instance.customLayerOn[0];
				break;
			case Events.Overlay_RaceResults:
				LiveData.Instance.forceShowRaceResult = !LiveData.Instance.forceShowRaceResult;
				break;
			case Events.Overlay_Champ:
				MainWindow.Instance.customLayerOn[2] = !MainWindow.Instance.customLayerOn[2];
				MainWindow.Instance.ControlPanel_C3_Button.IsChecked = MainWindow.Instance.customLayerOn[2];
				break;
			case Events.Overlay_LapComp:
				MainWindow.Instance.customLayerOn[4] = !MainWindow.Instance.customLayerOn[4];
				MainWindow.Instance.ControlPanel_C5_Button.IsChecked = MainWindow.Instance.customLayerOn[4];
				break;
			case Events.Overlays_BattleChyron:
				ToggleButton(MainWindow.Instance.ControlPanel_BattleChyron_Button);
				break;
			case Events.Overlay_Next:
				

				if (champOn)
				{
					MainWindow.Instance.ControlPanel_Champ_NextPage_Button_Click(this, null);
				}
				else if(resultsOn)
				{
					MainWindow.Instance.ControlPanel_Standings_NextPage_Button_Click(this, null);
				}
				
				
				break;
			case Events.Overlay_Prev:
				if (champOn)
				{
					MainWindow.Instance.ControlPanel_Champ_PrevPage_Button_Click(this, null);
				}
				else if(resultsOn)
				{
					MainWindow.Instance.ControlPanel_Standings_PrevPage_Button_Click(this, null);
				}
				
				
				break;
			case Events.ClassOverallToggle:
				MainWindow.Instance.ToggleStandingsMode();
				break;
			
			case Events.GapIntervalToggle:
				MainWindow.Instance.ToggleTimingMode();
				break;
				
			default:
				Console.WriteLine("ERROR! Unknown Message Type");
				break;
		}
	}

	private void ToggleButton(ToggleButton btn)
	{
		btn.IsChecked = !btn.IsChecked;
	}
}