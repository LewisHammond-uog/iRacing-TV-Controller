namespace iRacingTVController
{
	public enum SectorStatus
	{
		NotCompleted = 0,
		Regular = 1,
		PersonalBest = 2,
		SessionBestInClass = 3,
		SessionBestOverall = 4
	}

	public class SectorLapStatus
	{
		public int Number { get; set; }
		public float Time { get; set; }
		public SectorStatus Status { get; set; }
	}
}
