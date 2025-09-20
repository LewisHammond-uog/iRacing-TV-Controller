
using System;
using System.Text.Json.Serialization;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataLeaderboardSlot
	{
		[JsonInclude] public bool show = false;
		public bool showCurrentTarget = false;
		public bool showPreferredCar = false;

		public Vector2 offset = Vector2.zero;

		[JsonInclude] public string textLayer1 = string.Empty;
		public Color textLayer1Color = Color.white;

		[JsonInclude] public string textLayer2 = string.Empty;
		public Color textLayer2Color = Color.white;

		[JsonInclude] public string textLayer3 = string.Empty;
		public Color textLayer3Color = Color.white;

		[JsonInclude] public string textLayer4 = string.Empty;
		public Color textLayer4Color = Color.white;
		
		[JsonInclude] public string textLayer5 = string.Empty;
		public Color textLayer5Color = Color.white;

		[JsonInclude] public bool penaltyFlag;
		[JsonInclude] public bool slowDownFlag;
		[JsonInclude] public bool meatballFlag;
		[JsonInclude] public bool finished;

		[JsonInclude] public string currentTargetTextLayer1 = string.Empty;
	}
}
