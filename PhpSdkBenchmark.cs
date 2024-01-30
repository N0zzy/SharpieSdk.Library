using System;

namespace PhpieSdk.Library;

public struct PhpSdkBenchmark
{
    private static DateTimeOffset t0;
    private static DateTimeOffset t1;

    public static void Start()
    {
        t0 = new DateTimeOffset(DateTime.Now);
    }

    public static void Finish()
    {
        t1 = new DateTimeOffset(DateTime.Now);
    }

    public static void Result()
    {
        "".BenchmarkWriteLn("Start:  " + t0.Hour + ":" + t0.Minute + ":" + t0.Second + "." + t0.Millisecond);
        "".BenchmarkWriteLn("Finish: " + t1.Hour + ":" + t1.Minute + ":" + t1.Second + "." + t1.Millisecond);
        var diff = t1 - t0;
        "".BenchmarkWriteLn("Total:  " + Math.Round(diff.TotalMilliseconds / 1000, 2) + " sec. [" +  diff.TotalMilliseconds + " ms.]");

    }
}