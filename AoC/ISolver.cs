namespace AoC;

public interface ISolver
{
    void SetupRun(Automaton automaton);
    object GetAnswer1(string data);
    object GetAnswer2(string data);
}