namespace AoC.AoCTests
{
    internal class FakeSolver : ISolver
    {
        private readonly int _day;
        private readonly object _answer1;
        private readonly object _answer2;
            
        public int GetAnswer1Calls { get; private set; }
        public int GetAnswer2Calls { get; private set; }
            
        public string InputData { get; private set; }

        public FakeSolver(int day, object answer1, object answer2)
        {
            _day = day;
            _answer1 = answer1;
            _answer2 = answer2;
        }

        public void SetupRun(Automaton automaton)
        {
            automaton.Day = _day;
        }

        public object GetAnswer1(string data)
        {
            InputData = data;
            GetAnswer1Calls++;
            return _answer1;
        }

        public object GetAnswer2(string data)
        {
            GetAnswer2Calls++;
            return _answer2;
        }
    }
}