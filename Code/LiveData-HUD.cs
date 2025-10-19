using irsdkSharp.Serialization.Enums.Fastest;

namespace iRacingTVController;

public partial class LiveData
{
	public void UpdateHud()
	{
		if (IRSDK.data == null)
		{
			return;
		}

		// player car

		var normalizedCar = IRSDK.normalizedData.normalizedCars[IRSDK.data.PlayerCarIdx];

		liveDataHud.textLayer1 = GetTextContent(out liveDataHud.textLayer1Color, "HudTextLayer1", normalizedCar);
		liveDataHud.textLayer2 = GetTextContent(out liveDataHud.textLayer2Color, "HudTextLayer2", normalizedCar);
		liveDataHud.textLayer3 = GetTextContent(out liveDataHud.textLayer3Color, "HudTextLayer3", normalizedCar);
		liveDataHud.textLayer4 = GetTextContent(out liveDataHud.textLayer4Color, "HudTextLayer4", normalizedCar);
		liveDataHud.textLayer5 = GetTextContent(out liveDataHud.textLayer5Color, "HudTextLayer5", normalizedCar);
		liveDataHud.textLayer6 = GetTextContent(out liveDataHud.textLayer6Color, "HudTextLayer6", normalizedCar);
		liveDataHud.textLayer7 = GetTextContent(out liveDataHud.textLayer7Color, "HudTextLayer7", normalizedCar);
		liveDataHud.textLayer8 = GetTextContent(out liveDataHud.textLayer8Color, "HudTextLayer8", normalizedCar);
		liveDataHud.textLayer9 = GetTextContent(out liveDataHud.textLayer9Color, "HudTextLayer9", normalizedCar);
		liveDataHud.textLayer10 = GetTextContent(out liveDataHud.textLayer10Color, "HudTextLayer10", normalizedCar);
		liveDataHud.textLayer11 = GetTextContent(out liveDataHud.textLayer11Color, "HudTextLayer11", normalizedCar);
		liveDataHud.textLayer12 = GetTextContent(out liveDataHud.textLayer12Color, "HudTextLayer12", normalizedCar);
		liveDataHud.textLayer13 = GetTextContent(out liveDataHud.textLayer13Color, "HudTextLayer13", normalizedCar);
		liveDataHud.textLayer14 = GetTextContent(out liveDataHud.textLayer14Color, "HudTextLayer14", normalizedCar);
		liveDataHud.textLayer15 = GetTextContent(out liveDataHud.textLayer15Color, "HudTextLayer15", normalizedCar);
		liveDataHud.textLayer16 = GetTextContent(out liveDataHud.textLayer16Color, "HudTextLayer16", normalizedCar);

		// speech to text

		var recognizedString = SpeechToText.GetRecognizingString();

		if (recognizedString == string.Empty)
		{
			if (speechToTextTimer > 0)
			{
				speechToTextTimer -= Program.deltaTime;

				if (speechToTextTimer <= 0.0f)
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

		switch ((CarLeftRight) IRSDK.data.CarLeftRight)
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

		if (carsLeft == 1)
		{
			liveDataHud.showLeftSpotterIndicator = true;
		}
		else if (carsLeft == 2)
		{
			liveDataHud.showLeftSpotterIndicator = (Program.elapsedMilliseconds % 250) >= 100;
		}
		else
		{
			liveDataHud.showLeftSpotterIndicator = false;
		}

		if (carsRight == 1)
		{
			liveDataHud.showRightSpotterIndicator = true;
		}
		else if (carsRight == 2)
		{
			liveDataHud.showRightSpotterIndicator = (Program.elapsedMilliseconds % 250) >= 100;
		}
		else
		{
			liveDataHud.showRightSpotterIndicator = false;
		}
	}
}