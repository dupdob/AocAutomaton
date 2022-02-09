using System;
using System.Threading.Tasks;

namespace AoC;

public abstract class AoCClientBase : IDisposable
{
    protected AoCClientBase(int year)
    {
        Year = year;
    }

    public int Day { get; private set; }
    public int Year { get; }

    public abstract void Dispose();

    public void SetCurrentDay(int day)
    {
        Day = day;
    }

    public abstract Task<string> RequestPersonalInput();
    public abstract Task<string> PostAnswer(int question, string value);
}