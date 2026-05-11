using System.Threading;

namespace Infrastructure.Snowflake;

public static class IdGenerator
{
    private const long Twepoch = 1577808000000L;
    private static long _lastTimestamp = -1L;
    private static long _sequence = 0L;
    private const long SequenceMask = 4095L;
    private static readonly object Lock = new();

    public static long NextId()
    {
        lock (Lock)
        {
            var timestamp = TimeGen();
            if (timestamp == _lastTimestamp)
            {
                _sequence = (_sequence + 1) & SequenceMask;
                if (_sequence == 0)
                {
                    timestamp = TilNextMillis(_lastTimestamp);
                }
            }
            else
            {
                _sequence = 0L;
            }
            _lastTimestamp = timestamp;
            return ((timestamp - Twepoch) << 12) | _sequence;
        }
    }

    private static long TilNextMillis(long lastTimestamp)
    {
        var timestamp = TimeGen();
        while (timestamp <= lastTimestamp)
        {
            Thread.Sleep(0);
            timestamp = TimeGen();
        }
        return timestamp;
    }

    private static long TimeGen() => System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
