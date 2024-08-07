using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HiResTimersTest.Entity;

namespace HiResTimersTest;

public static class TestTimer1
{
	private const int IterationsCount = 1000;

	[DllImport("kernel32.dll")]
	private static extern void timeBeginPeriod(uint uPeriod);

	[DllImport("kernel32.dll")]
	private static extern void timeEndPeriod(uint uPeriod);

	[DllImport("ntdll.dll", SetLastError = true)]
	static extern int NtQueryTimerResolution(out uint minimumResolution, out uint maximumResolution,
		out uint currentResolution);

	[DllImport("ntdll.dll", SetLastError = true)]
	private static extern void NtSetTimerResolution(uint desiredResolution, bool setResolution,
		out uint currentResolution);

	[DllImport("ntdll.dll")]
	private static extern bool NtDelayExecution(bool alertable, ref long delayInterval);

	public static void RunTests()
	{
		Console.WriteLine($"OS: {Environment.OSVersion}");
		Console.WriteLine($"Iterations count: {IterationsCount}");
		Console.WriteLine();

		Test0();
		Test1();
		Test2();
		// Test3().Wait();
		// Test4();
		Test5();
		Test6();
		Test7();
	}

	private static void Test0()
	{
		using CStopwatch stopWatch = new("Running empty loop estimation:");

		for (int i = 0; i < IterationsCount; ++i)
			Thread.Sleep(0);
	}

	private static void Test1()
	{
		using CStopwatch stopWatch = new("Running timer (timeBeginPeriod: default), Sleep:", true);

		for (int i = 0; i < IterationsCount; ++i)
			Thread.Sleep(1);
	}

	private static void Test2()
	{
		using CStopwatch stopWatch = new("Running timer (timeBeginPeriod(1), Sleep):", true);

		timeBeginPeriod(1);

		for (int i = 0; i < IterationsCount; ++i)
			Thread.Sleep(1);

		timeEndPeriod(1);
	}

	private static async Task Test3()
	{
		using CStopwatch stopWatch = new("Running timer (timeBeginPeriod(1), Delay):", true);

		timeBeginPeriod(1);

		for (int i = 0; i < IterationsCount; ++i)
			await Task.Delay(1).ConfigureAwait(false);

		timeEndPeriod(1);
	}

	private static void Test4()
	{
		Console.WriteLine();
		Console.WriteLine("Running timer (NtSetTimerResolution, NtDelayExecution)");
		using CStopwatch stopWatch = new("Elapsed: ");

		NtQueryTimerResolution(out uint minimumResolution, out uint maximumResolution, out uint previousResolution);

		uint res = maximumResolution;
		// long interval = -maximumResolution;
		long interval = -1L;

		NtSetTimerResolution(res, true, out _);
		NtQueryTimerResolution(out _, out _, out uint newResolution);
		Console.WriteLine(
			$"MinRes = {minimumResolution}, MaxRes = {maximumResolution}, PrevRes = {previousResolution}, NewRes = {newResolution}");

		for (int i = 0; i < IterationsCount; ++i)
			NtDelayExecution(false, ref interval);

		NtSetTimerResolution(previousResolution, true, out _);
	}

	private static void Test5()
	{
		Console.WriteLine();
		Console.WriteLine("Running timer (NtSetTimerResolution, NtDelayExecution)");
		using CStopwatch stopWatch = new("Elapsed (new implementation): ");
		
		using HiResTimer timer = new(0); // max resolution
		if (timer.LastStatus != 0)
		{
			Console.WriteLine($"Error setting timer resolution, nt status code = {timer.LastStatus}");
			return;
		}

		Console.WriteLine(
			$"MinRes = {timer.MinimumResolution}, MaxRes = {timer.MaximumResolution}, " +
			$"PrevRes = {timer.InitialResolution}, NewRes = {timer.CurrentResolution}");

		for (int i = 0; i < IterationsCount; ++i)
			timer.DelayExecution(-1); // min delay
	}

	private static void Test6()
	{
		AutoResetEvent autoResetEvent1 = new AutoResetEvent(false);
		AutoResetEvent autoResetEvent2 = new AutoResetEvent(false);
		new Thread(() => FireEvents6(autoResetEvent1, autoResetEvent2)).Start();

		Thread.Sleep(15);
		using CStopwatch stopWatch = new("Test WaitOne / AutoResetEvent");

		for (int i = 0; i < IterationsCount; ++i)
		{
			autoResetEvent2.Set();
			autoResetEvent1.WaitOne();
		}
	}

	private static void FireEvents6(AutoResetEvent autoResetEvent1, AutoResetEvent autoResetEvent2)
	{
		for (int i = 0; i < IterationsCount; ++i)
		{
			autoResetEvent2.WaitOne();
			autoResetEvent1.Set();
		}
	}
	
	private static void Test7()
	{
		// HiResTimer timer = new(0);

		BlockingCollection<int> blockingCollection = new(3);
		new Thread(() => ProduceValues7(blockingCollection)).Start();

		Thread.Sleep(15);
		using CStopwatch stopWatch = new("Test BlockingCollection");

		for (int i = 0; i < IterationsCount; ++i)
		{
			int val = blockingCollection.Take();
			// Console.WriteLine($"{i} consumed");
			// timer.DelayExecutionMs(10);
		}
	}

	private static void ProduceValues7(BlockingCollection<int> blockingCollection)
	{
		// HiResTimer timer = new(0);
		
		for (int i = 0; i < IterationsCount; ++i)
		{
			blockingCollection.Add(i);
			// Console.WriteLine($"{i} produced");
			// timer.DelayExecutionMs(10);
		}
	}
}