namespace iRacingTVController
{
	public class SectorTimingInfo
	{
		public int Number { get; set; } = 0;
		public float StartPercentage { get; set; } = 0f;
		public float EnterSessionTime { get; set; } = 0f;
		public float SectorTime { get; set; } = 0f;

		public SectorTimingInfo Copy()
		{
			return new SectorTimingInfo
			{
				Number = this.Number,
				StartPercentage = this.StartPercentage,
				EnterSessionTime = this.EnterSessionTime,
				SectorTime = this.SectorTime
			};
		}
	}
}
