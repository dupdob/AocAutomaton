namespace AoC;

public abstract class SolverWithLineParser : SolverWithParser
{
    protected abstract void ParseLine(string line, int index, int lineCount);

    /// <summary>
    ///     Parse the exercise data.
    /// </summary>
    /// <param name="data">data to be parsed as a single string</param>
    /// <remarks>
    ///     Split the data as an array of lines and call <see cref="ParseLine" /> for each them.
    ///     Override this method if you prefer to parse data as a whole (you still need to provide an empty ParseLine method
    ///     as it is declared abstract).
    /// </remarks>
    protected override void Parse(string data)
    {
        var lines = data.Split('\n');
        // we discard the last line if it is empty (trailing newline), but we keep any internal newlines
        if (lines[^1].Length == 0) lines = lines[..^1];
        var index = 0;
        foreach (var line in lines) ParseLine(line, index++, lines.Length);
    }
}