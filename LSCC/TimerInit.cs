using RAGETimer.Shared;
using GTANetworkAPI;
using System;

class TimerManager : Script
{
	public TimerManager() 
	{
		Timer.Init(NAPI.Util.ConsoleOutput, () => (ulong)Environment.TickCount & int.MaxValue);
	}
	
	[ServerEvent(Event.Update)]
	public static void OnUpdateFunc()
	{
		Timer.OnUpdateFunc();
	}
}