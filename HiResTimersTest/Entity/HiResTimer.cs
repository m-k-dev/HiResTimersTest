using System;
using System.Runtime.InteropServices;

namespace HiResTimersTest.Entity;

public class HiResTimer : IDisposable
{
	#region fields
	private uint _minimumResolution;
	private uint _maximumResolution;
	private uint _currentResolution;
	#endregion

	#region DllImport
	[DllImport("ntdll.dll", SetLastError = true)]
	private static extern uint NtQueryTimerResolution(out uint minimumResolution, out uint maximumResolution,
		out uint currentResolution);

	[DllImport("ntdll.dll", SetLastError = true)]
	private static extern uint NtSetTimerResolution(uint desiredResolution, bool setResolution,
		out uint currentResolution);

	[DllImport("ntdll.dll")]
	private static extern uint NtDelayExecution(bool alertable, in long delayInterval);
	#endregion

	public uint LastStatus { get; private set; }
	public uint InitialResolution { get; }

	public uint MinimumResolution => _minimumResolution;

	public uint MaximumResolution => _maximumResolution;

	public uint CurrentResolution
	{
		get => _currentResolution;
		set => SetTimerResolution(value);
	}

	public HiResTimer()
	{
		QueryTimerResolution();
		if (LastStatus == 0)
			InitialResolution = CurrentResolution;
	}

	public HiResTimer(uint newResolution)
	{
		QueryTimerResolution();
		if (LastStatus == 0)
		{
			InitialResolution = CurrentResolution;
			SetTimerResolution(Math.Max(newResolution, _maximumResolution));
		}
	}

	public uint QueryTimerResolution()
	{
		return QueryTimerResolution(out _minimumResolution, out _maximumResolution, out _currentResolution);
	}

	public uint QueryTimerResolution(out uint minimumResolution, out uint maximumResolution, out uint currentResolution)
	{
		return LastStatus = NtQueryTimerResolution(out minimumResolution, out maximumResolution, out currentResolution);
	}

	public uint SetTimerResolution(uint newResolution)
	{
		LastStatus = NtSetTimerResolution(newResolution, true, out _);
		if (LastStatus == 0)
			_currentResolution = newResolution;
		return LastStatus;
	}

	public uint DelayExecution(long delayInterval)
	{
		if (delayInterval > 0)
			delayInterval = -delayInterval;

		return LastStatus = NtDelayExecution(false, in delayInterval);
	}

	public uint DelayExecutionMs(long delayInterval) => DelayExecution(delayInterval * 10000);
	
	public void Dispose()
	{
		if (InitialResolution != default)
			SetTimerResolution(InitialResolution);
	}
}