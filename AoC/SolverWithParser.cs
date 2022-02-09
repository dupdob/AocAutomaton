namespace AoC;

public abstract class SolverWithParser : ISolver
{
    private string _data;

    private string Data
    {
        get => _data;
        set
        {
            if (value == _data) return;
            _data = value;
            Parse(value);
        }
    }

    public abstract void SetupRun(Automaton automaton);

    public object GetAnswer1(string data)
    {
        Data = data;
        return GetAnswer1();
    }

    public object GetAnswer2(string data)
    {
        Data = data;
        return GetAnswer2();
    }

    public abstract object GetAnswer1();
    public abstract object GetAnswer2();

    /// <summary>
    ///     Parse the exercise data.
    /// </summary>
    /// <param name="data">data to be parsed as a single string</param>
    /// <remarks>
    ///     Split the data as an array of lines and call <see cref="SolverWithLineParser.ParseLine" /> for each them.
    ///     Override this method if you prefer to parse data as a whole (you still need to provide an empty ParseLine method
    ///     as it is declared abstract).
    /// </remarks>
    protected abstract void Parse(string data);
}