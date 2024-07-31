# region info
// Simple stopwatch class
# endregion

using System;
using System.Diagnostics;

namespace HiResTimersTest.Entity;

public sealed class CStopwatch : IDisposable
{
	private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
	private readonly string _message;
	private readonly int _justifyIdx;
	private readonly bool _splitMessage;

	public CStopwatch(string message = "", bool splitMessage = false, int justifyIdx = 50)
	{
		_message = message.Trim();
		_justifyIdx = justifyIdx;
		_splitMessage = splitMessage;

		if (_splitMessage)
			Console.Write(GetMessage());
	}

	private string GetMessage()
	{
		int messageLength = _message.Length;
		if (messageLength == 0)
			return _message;

		string str = (_message + new string(' ', _justifyIdx))
			.Substring(0, Math.Max(_justifyIdx, messageLength));
		return str;
	}

	private static string GetElapsedMessage(TimeSpan resultTime)
	{
		string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:000}",
			resultTime.Hours,
			resultTime.Minutes,
			resultTime.Seconds,
			resultTime.Milliseconds);

		return elapsedTime;
	}

	public void Dispose()
	{
		_stopwatch.Stop();

		if (!_splitMessage)
			Console.Write(GetMessage());

		string elapsedTime = GetElapsedMessage(_stopwatch.Elapsed);
		Console.WriteLine($"{elapsedTime}");
	}
}