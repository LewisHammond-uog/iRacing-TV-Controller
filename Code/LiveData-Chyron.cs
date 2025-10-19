namespace iRacingTVController;

public partial class LiveData
{
	public void UpdateChyron()
	{
		var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx(IRSDK.normalizedData.camCarIdx);

		if ((normalizedCar != null) && normalizedCar.includeInLeaderboard && Director.showChyron &&
		    (!liveDataControlPanel.voiceOfOn || (IRSDK.normalizedData.radioTransmitCarIdx == -1)))
		{
			Unity.Color color;

			liveDataChyron.show = true;

			liveDataChyron.textLayer1 = GetTextContent(out color, "ChyronTextLayer1", normalizedCar);
			liveDataChyron.textLayer2 = GetTextContent(out color, "ChyronTextLayer2", normalizedCar);
			liveDataChyron.textLayer3 = GetTextContent(out color, "ChyronTextLayer3", normalizedCar);
			liveDataChyron.textLayer4 = GetTextContent(out color, "ChyronTextLayer4", normalizedCar);
			liveDataChyron.textLayer5 = GetTextContent(out color, "ChyronTextLayer5", normalizedCar);
			liveDataChyron.textLayer6 = GetTextContent(out color, "ChyronTextLayer6", normalizedCar);
			liveDataChyron.textLayer7 = GetTextContent(out color, "ChyronTextLayer7", normalizedCar);
			liveDataChyron.textLayer8 = GetTextContent(out color, "ChyronTextLayer8", normalizedCar);
			liveDataChyron.textLayer9 = GetTextContent(out color, "ChyronTextLayer9", normalizedCar);
			liveDataChyron.textLayer10 = GetTextContent(out color, "ChyronTextLayer10", normalizedCar);
			liveDataChyron.textLayer11 = GetTextContent(out color, "ChyronTextLayer11", normalizedCar);
			liveDataChyron.textLayer12 = GetTextContent(out color, "ChyronTextLayer12", normalizedCar);
			liveDataChyron.textLayer13 = GetTextContent(out color, "ChyronTextLayer13", normalizedCar);
			liveDataChyron.textLayer14 = GetTextContent(out color, "ChyronTextLayer14", normalizedCar);
			liveDataChyron.textLayer15 = GetTextContent(out color, "ChyronTextLayer15", normalizedCar);

			liveDataChyron.carIdx = normalizedCar.carIdx;
		}
		else
		{
			liveDataChyron.show = false;
		}
	}
}