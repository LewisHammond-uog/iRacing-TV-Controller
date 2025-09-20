using System;

namespace iRacingTVController;

public static class UIntExtensions
{
	public static bool HasAnyFlag<T>(this uint value, T flags) where T : Enum
	{
		var mask = Convert.ToUInt32(flags);
		return (value & mask) != 0;
	}
}