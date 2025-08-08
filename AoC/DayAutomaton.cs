// MIT License
// 
//  AocAutomaton
// 
//  Copyright (c) 2023-25 Cyrille DUPUYDAUBY
// ---
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC;

public class DayAutomaton
{
    private readonly List<TestData> _tests = [];
    private TestData _last;
    private DayState _dayState;
    private object[][] _defaultParameters = new object[2][];
    private int _currentDay;

    // user data (forced)
    private string _data;
    private readonly int _year;
    private readonly IAutomaton _automaton;
    private readonly IFileSystem _fileSystem;
    private readonly IInteract _userInterface;
    private readonly string _dataPathNameFormat;
    private readonly string _dataRootPath;

    /// <summary>
    /// Build an automation instance
    /// </summary>
    /// <param name="userInterface">User interface component. Console interface if set to null (default value)</param>
    /// <param name="meta">Meta orchestrator</param>
    /// <param name="fileSystem">Filesystem. For testing purposes.</param>
    public DayAutomaton(IAutomaton meta, IFileSystem fileSystem, IInteract userInterface)
    {
        _year = meta.Year;
        _automaton = meta;
        _fileSystem = fileSystem;
        _userInterface = userInterface;
        _dataPathNameFormat = _automaton.DataPathNameFormat;
        _dataRootPath = _automaton.RootPath;
    }
    
    private string StateFileName => $"AoC-{_year,4}-{Day,2}-state.json";
    private const string IgnoreFilter = "Aoc-*-state.json";
    private const int LastDay = 25;

    private string StatePathName => string.IsNullOrEmpty(DataPath)
        ? StateFileName
        : _fileSystem.Path.Combine(DataPath, StateFileName);
    
    private string DataPath => string.Format(_dataPathNameFormat, Day, _year);
    
    /// <summary>
    /// Build a new solver for question 2 instead of using the one build for question 1 (if sets to true).
    /// </summary>
    public bool ResetBetweenQuestions { get; set; }

    private bool[] ResultIsVisual { get; } = new bool[2];

    /// <summary>
    /// Gets/sets the current day
    /// </summary>
    public int Day
    {
        get => _currentDay;
        set
        {
            if (_currentDay == value)
            {
                return;
            }

            ClearState(value);
        }
    }

    private void ClearState(int value)
    {
        _tests.Clear();
        _last = null;
        _dayState = null;
        _currentDay = value;
        _defaultParameters = new object[2][];
        ResetBetweenQuestions = false;
    }

    /// <summary>
    /// Set default values for extra parameters. These will be used for your actual input and any test for which
    /// those parameters have not been overriden.
    /// </summary>
    /// <param name="part">Puzzle part (1 or 2)</param>
    /// <param name="defaultParameters">default integer parameters</param>
    /// <remarks>Use this method when the advent of code use some custom parameters that are not part of the input,
    /// such as an iteration count or an initial text. </remarks>
    public void SetDefault(int part = 1, params object[] defaultParameters)
    {
        _defaultParameters[part - 1] = defaultParameters;
    }
    
    private object GetAnswer(ISolver algorithm, int id, string data)
    {
        var clock = new Stopwatch();
        _automaton.Trace($"Computing answer {id} ({_automaton.Now():HH:mm:ss}).");
        clock.Start();
        var answer = id == 1 ? algorithm.GetAnswer1(data) : algorithm.GetAnswer2(data);
        clock.Stop();
        var message = clock.ElapsedMilliseconds < 2000 ? $"{clock.ElapsedMilliseconds} ms" : $"{clock.Elapsed:c}";
        _automaton.Trace($"Took {message}.");
        if (!ResultIsVisual[id - 1])
        {
            return answer;
        }
        // this is a visual result, we need to display it and ask
        _userInterface.Trace("Visual result:");
        _userInterface.Trace(answer.ToString());
        _userInterface.Trace("Please input answer (empty to keep the same):");
        var manual = Automaton.AskInput();
        if (!string.IsNullOrEmpty(manual))
        {
            // user provided an answer
            answer = manual;
        }
        return answer;
    }

    private bool RunTests(int id, SolverFactory factory)
    {
        var success = true;
        if (_tests.Count == 0)
        {
            _automaton.Trace($"* /!\\ no test for question {id}! *");
            return true;
        }

        _automaton.Trace($"* Test question {id} *");
        // Is there any actual test?
        if (!_tests.Any(t => t.CanTest(id-1)))
        {
            // we ensure at least one value is tested and request explicit confirmation
            _tests.First().SetVisualConfirm(id-1);
        }
        
        foreach (var testData in _tests)
        {
            var expected = testData.Answers[id - 1];
            // we skip running the test if we are not interested in the result for any reason
            if (expected == null && (id==2 || ResetBetweenQuestions) 
                                 && !testData.VisualConfirm[id - 1])
            {
                continue;
            }

            // gets a cached algorithm if any
            var testAlgo = factory.GetSolver(testData.Data, true, testData.GetParameters(id-1, _defaultParameters));
            success = CheckAnswer(id, GetAnswer(testAlgo, id, testData.Data), expected, testData) && success;
        }

        return success;
    }

    private bool CheckAnswer(int id, object answer, object expected, TestData testData)
    {
        // no answer provided
        if (answer == null)
        {
            _automaton.Trace($"Test failed: got no answer instead of {expected} using:");
            _automaton.Trace(testData.Data);
            return false;
        }
        // no expected answer provided, we request manual confirmation 
        if (expected == null)
        {
            if (!testData.VisualConfirm[id - 1])
            {
                return true;
            }

            _automaton.Trace("Testing with:");
            _automaton.Trace(testData.Data);
            _automaton.Trace("provided a result but no expected answer provided. Please confirm result manually (y/n). Result below.");
            _automaton.Trace(answer.ToString());
            if (!_automaton.AskYesNo())
            {
                return false;
            }
        }
        // not the expected answer
        else if (!answer.ToString()!.Equals(expected.ToString()))
        {
            _automaton.Trace($"Test failed: got {answer} instead of {expected} using:");
            _automaton.Trace(testData.Data);
            return false;
        }
        else
        {
            _automaton.Trace($"Test succeeded: got {answer} using:");
            _automaton.Trace(testData.Data);
        }

        return true;
    }

    /// <summary>
    /// Registers test data so that they are used for validating the solver
    /// </summary>
    /// <param name="data">input data as a string.</param>
    /// <param name="expected">expected result (either string or a number).</param>
    /// <param name="question">question id (1 or 2)</param>
    /// <returns>The automation base instance for linked calls.</returns>
    [Obsolete("Prefer ExampleAttribute or AddExample method instead.")]
    public DayAutomaton RegisterTestDataAndResult(string data, object expected, int question)
    {
        var set = _tests.FirstOrDefault(t => t.Data== data);
        if (set == null)
        {   
            set = new TestData(data, this);
            _tests.Add(set);
        }
        
        if (question == 1)
        {
            set.RegisterAnswer(0, expected);
        }
        else
        {
            set.RegisterAnswer(1, expected);
        }

        _last = set;
        return this;
    }

    /// <summary>
    /// Register a new test.
    /// </summary>
    /// <param name="data">input data for this test</param>
    /// <returns>a <see cref="TestData"/> instance that must be enriched with expected results.</returns>
    /// <returns>The automation base instance for linked calls.</returns>
    [Obsolete("Prefer ExampleAttribute or AddExample method instead.")]
    public TestData RegisterTest(string data)
    {
        _last = AddExample(data);
        return _last;
    }

    /// <summary>
    /// Registers test data so that they are used for validating the solver
    /// </summary>
    /// <param name="data">input data as a string.</param>
    /// <returns>The automation base instance for linked calls.</returns>
    [Obsolete("Prefer AddExample instead.")]
    public DayAutomaton RegisterTestData(string data)
    {
        RegisterTest(data);
        return this;
    }

    /// <summary>
    /// Add example data so that they are used for validating the solver
    /// </summary>
    /// <param name="data">input data as a string.</param>
    /// <param name="force">force adding the example</param>
    /// <returns>The automation base instance for linked calls.</returns>
    /// <remarks>if <paramref name="force"/> is false, it will return test data for provided example if any has been added</remarks>
    public TestData AddExample(string data, bool force = true)
    {
        if (!force)
        {
            var found = _tests.FirstOrDefault(d => d.Data == data);
            if (found != null)
            {
                return found;
            }
        }
        _tests.Add(new TestData(data, this));
        _last = _tests[^1];
        return _last;
    }
        
    public ICollection<TestData> GetExamples() => _tests;
    
    /// <summary>
    /// Register that the result should be manually confirmed during execution
    /// </summary>
    /// <param name="question">question's answer to be confirmed manually</param>
    /// <returns>The automation base instance for linked calls.</returns>
    [Obsolete("Prefer ExampleAttribute or AddExample method instead.")]
    public DayAutomaton AskVisualConfirm(int question)
    {
        _last.SetVisualConfirm(question-1);
        return this;
    }

    /// <summary>
    /// Registers test result so that they are used for validating the solver
    /// </summary>
    /// <remarks>You need to declare the associated test data first with <see cref="RegisterTestData"/></remarks>
    /// <param name="expected">expected result (either string or a number).</param>
    /// <param name="question">question id (1 or 2)</param>
    /// <returns>The automation base instance for linked calls.</returns>
    [Obsolete("Prefer ExampleAttribute or AddExample method instead.")]
    public DayAutomaton RegisterTestResult(object expected, int question = 1)
    {
        if (_last == null)
        {
            throw new ApplicationException("You must call RegisterTestData before calling this method.");
        }
        if (question == 1)
        {
            _last.Answer1(expected);
        }
        else
        {
            _last.Answer2(expected);
        }
        return this;
    }

    /// <summary>
    /// Loads user data from a file.
    /// </summary>
    /// <param name="pathName">path name of the user data.</param>
    public void LoadUserData(string pathName) => _data = _fileSystem.File.ReadAllText(pathName);

    public bool RunDay(Func<ISolver> builder) => RunDay(new SolverFactory(builder));

    public bool RunDay(SolverFactory factory)
    {
        try
        {
            ResetAutomaton();

            // we ask the solver to set the exercise context up
            if (!InitializeSolver(factory))
            {
                return false;
            }

            InitializeDay(Day);
            if (!CheckState())
            {
                return true;
            }
            
            factory.CacheActive = !ResetBetweenQuestions;
            
            // tests if data are provided
            var testsSucceeded = RunTests(1, factory); 
            
            var data = GetPersonalInput();
            if (!testsSucceeded)
            {
                return false;
            }
            
            // perform the actual run
            _automaton.Trace("* Computing answer 1 from your input. *");
            var answer = GetAnswer(factory.GetSolver(data, false, _defaultParameters[0]), 1, data);
            _automaton.Trace($"* Attempting {answer ?? "null"} *");

            if (_dayState.First.Solved)
            {
                _automaton.Trace($"* Answer 1 already solved: {_dayState.First.Answer} *");
                if (answer == null || _dayState.First.Answer != answer.ToString())
                {
                    _automaton.Trace($"Solver no longer provides correct answer. Continue?");
                    if (!_automaton.AskYesNo())
                    {
                        // we ask for confirmation
                        return false;
                    }
                }
            }
            else if (!CheckResponse(1, answer))
            {
                return false;
            }

            if (Day == LastDay)
            {
                _automaton.Trace($"* Only one question on day {Day}. You're done! *");
                return true;
            }
            
            if (!RunTests(2, factory))
            {
                return false;
            }

            _automaton.Trace("* Computing answer 2 from your input. *");
            answer = GetAnswer(factory.GetSolver(data, false, _defaultParameters[1]), 2, data);
            return CheckResponse(2, answer);
        }
        finally
        {
            CleanUpDay();
        }
    }

    private bool InitializeSolver(SolverFactory factory)
    {
        var solver = factory.GetSolver(null, true, null);
        // use attributes
        ParseAttributes(solver);
        solver.SetupRun(this);
        if (!CheckSetup())
        {
            return false;
        }

        return RunUniTest(solver);
    }

    private void ParseAttributes(ISolver solver)
    {
        // get class attribute (target day)
        var dayAttribute = (DayAttribute)solver.GetType().GetCustomAttributes(typeof(DayAttribute), false).FirstOrDefault();
        if (dayAttribute != null)
        {
            Day = dayAttribute.Day;
        }
        else
        {
            // try parsing the name
            var scan = Regex.Match(solver.GetType().Name, @"[^\d](\d\d?)$");
            if (scan.Success)
            {
                _automaton.Trace($"Warning: no DayAttribute found on {solver.GetType().Name}, assuming day {scan.Groups[1].Value}.");
                Day = int.Parse(scan.Groups[1].Value);
            }
        }
        
        // get examples data
        // see if there are example attributes
        var methodInfos = solver.GetType().GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        var sharedExamples = ParseMethodAttributes(methodInfos.Where(m => m.Name == "GetAnswer1"), 0);

        ParseMethodAttributes(methodInfos.Where(m => m.Name == "GetAnswer2"), 1);

        // get samples for part 2
        var solverForPart2 = methodInfos.Where( m => m.Name == "GetAnswer2");
        var sharedSamples = solverForPart2.
            SelectMany(m =>m.GetCustomAttributes(typeof(ReuseExampleAttribute), false))
            .Cast<ReuseExampleAttribute>().ToList();
        foreach (var sharedSample in sharedSamples)
        {
            if (!sharedExamples.TryGetValue(sharedSample.Id, out var actual))
            {
                _automaton.ReportError($"SharedExample id {sharedSample.Id} was not defined on GetAnswer1. Skipping it.");
                continue;
            }

            AddExample(actual.Input, false).RegisterAnswer(1, sharedSample.Expected).WithParameters(1, actual.Parameters);
        }
    }

    private Dictionary<int, ExampleAttribute> ParseMethodAttributes(IEnumerable<MethodInfo> methodInfos, int partId)
    {
        // get samples declared on GetAnswser1
        var samples = methodInfos
            .SelectMany(m =>m.GetCustomAttributes(typeof(ExampleAttribute), false))
            .Cast<ExampleAttribute>().ToList();
        var sharedExamples = new Dictionary<int, ExampleAttribute>();
        foreach (var sample in samples)
        {
            if (sample.Id != 0)
            {
                // it has an id so it can be reused for part 2
                sharedExamples[sample.Id] = sample;
            }

            AddExample(sample.Input, false).RegisterAnswer(partId, sample.Expected).WithParameters(partId, sample.Parameters);
        }

        if (methodInfos.Any(m => m.GetCustomAttribute(typeof(VisualResultAttribute)) != null))
        {
            ResultIsVisual[partId] = true;
        }

        return sharedExamples;
    }

    private bool RunUniTest(ISolver solver)
    {
        // search for test if any
        var methodInfos = solver.GetType().GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance);
        foreach (var info in methodInfos)
        {
            var tests= info.GetCustomAttributes(typeof(UnitTestAttribute), false).Cast<UnitTestAttribute>().ToList();
            if (tests.Count == 0)
            {
                continue;
            }
            if (!info.IsStatic)
            {
                _automaton.Trace($"Warning: Trace method '{info.Name}' is not static, tests results may be impacted by lack of isolation.");
            }

            foreach (var test in tests)
            {
                // invoke the method
                var builder = new StringBuilder();
                var result = info.Invoke(solver, test.Parameters);
                if (result?.Equals(test.Expected) == true)
                {
                    // test success
                    // generate proper message
                    builder.Append($"Unit Test succeeded. {GenerateUnitTestResultLog(info.Name, result, test.Parameters)}");
                    _automaton.Trace(builder.ToString());
                    continue;
                }
                // test failed
                // generate proper message
                builder.AppendFormat("Unit Test failed. {0} instead of {1}", 
                    GenerateUnitTestResultLog(info.Name, result, test.Parameters), 
                    ValueToString(test.Expected));
                _automaton.Trace(builder.ToString());
                return false;
            }
        }
        return true;
    }

    private static string GenerateUnitTestResultLog(string method, object result, params object[] parameters)
    {
        var builder = new StringBuilder();
        builder.AppendFormat("{0}({1})", method, string.Join(',', parameters.Select(ValueToString)));
        builder.AppendFormat("= {0}",ValueToString(result));
        return builder.ToString();
    }

    private static string ValueToString(object value) =>
        value switch
        {
            null => null,
            string asString => $"\"{asString}\"",
            _ => value.ToString()
        };

    private bool CheckState()
    {
        if (!_dayState.First.Solved || !_dayState.Second.Solved)
        {
            return true;
        }

        _automaton.Trace($"Day {Day} has already been solved (first part:{_dayState.First.Answer}, second part:{_dayState.Second.Answer}). Nothing to do.");
        _automaton.Trace("Do you want to run it anyway?");
        return _automaton.AskYesNo();
    }

    private bool CheckSetup()
    {
        if (_tests.Count == 0)
        {
            _automaton.Trace("Warning: no test case provided.");
        }
        if (Day != 0)
        {
            return true;
        }

        var now = _automaton.Now();
        if (now.Month != 12)
        {
            _automaton.ReportError($"Error: please specify target day using Day property.");
            return false;
        }   
        Day = now.Day;
        _automaton.Trace($"Warning: Day not set, assuming day {Day} (use Day property to set day#).");

        return true;
    }

    private void ResetAutomaton()
    {
        Day = 0;
        ResetBetweenQuestions = false;
        _tests.Clear();
        var gitIgnore = new SimpleGitIgnoreManager(_fileSystem);
        gitIgnore.AddFilter(IgnoreFilter, _dataRootPath);
    }

    private bool CheckResponse(int id, object answer)
    {
        var answerText = answer?.ToString();
        if (string.IsNullOrWhiteSpace(answerText))
        {
            _automaton.Trace($"No answer provided! Please overload GetAnswer{id}() with your code.");
            return false;
        }

        long numericalValue;
        try
        {
            numericalValue = Convert.ToInt64(answer);
            if (!CheckForNegative(numericalValue))
            {
                return false;
            }
        }
        catch (Exception)
        {
            numericalValue = 0;
        }
        
        var state = id == 1 ? _dayState.First : _dayState.Second;

        // new attempt
        state.Attempts.Add(answerText);
        bool success;
        if (numericalValue != 0)
        {
            success = CheckAndUpdateRangedAnswer(id, numericalValue, state, answerText);
        }
        else
        {
            success = SubmitAnswer(id, answerText) == AnswerStatus.Good;
        }

        if (success)
        {
            // capture success
            state.Answer = answerText;
            state.Solved = true;
        }

        _automaton.Trace($"Question {id} {(success ? "passed" : "failed")}!");
        return success;

        bool CheckForNegative(long numericAnswer)
        {
            switch (numericAnswer)
            {
                case 0:
                    _automaton.Trace("Answer cannot be zero.");
                    return false;
                case >= 0:
                    return true;
                default:
                    _automaton.Trace($"Answer cannot be negative, not submitted: {numericAnswer}");
                    return false;
            }
        }
    }

    private bool CheckAndUpdateRangedAnswer(int id, long lNumber, DayQuestion state, string answerText)
    {
        bool success;
        if (CheckRange(lNumber, state))
        {
            var result = SubmitAnswer(id, answerText);
            switch (result)
            {
                case AnswerStatus.TooHigh:
                    state.High = state.High.HasValue ? Math.Min(state.High.Value, lNumber) : lNumber;
                    break;
                case AnswerStatus.TooLow:
                    state.Low = state.Low.HasValue ? Math.Max(state.Low.Value, lNumber) : lNumber;
                    break;
            }

            success = result == AnswerStatus.Good;
        }
        else
        {
            success = false;
        }

        return success;
    }

    private bool CheckRange(long number, DayQuestion state)
    {
        if (state.Low.HasValue && number <= state.Low.Value)
        {
            _automaton.Trace(state.Low == number
                ? $"Answer not submitted. '{number}' was attempted and reported as too low."
                : $"Answer not submitted. Previous attempt '{state.Low.Value}' was reported as too low and {number} is also too low.");
            return false;
        }

        if (!state.High.HasValue || number < state.High.Value)
        {
            return true;
        }

        _automaton.Trace(state.High == number
            ? $"Answer not submitted. '{number}' was attempted and reported as too high."
            : $"Answer not submitted. Previous attempt '{state.High.Value}' was reported as too high and {number} is also too high.");
        return false;
    }

    private void InitializeDay(int day)
    {
        _dayState = null;
        // deal with state
        var fullPath = _fileSystem.Path.GetFullPath(StatePathName);
        if (_fileSystem.File.Exists(fullPath))
        {
            try
            {
                _dayState = DayState.FromJson(_fileSystem.File.ReadAllText(fullPath));
                if (_dayState.Day != day)
                {
                    // the state looks corrupted
                    _automaton.Trace($"Warning: failed to restore state, day does not match expectation: {_dayState.Day} instead of {day}.");
                    _dayState = null;
                }

            }
            catch (Exception e)
            {
                _automaton.ReportError($"Failed to load the current state: {e}");
            }
        }

        // if it failed for any reason
        _dayState ??= new DayState { Day = day };
        _userInterface.InitializeDay(_year, Day, _dataRootPath, DataPath);
    }

    private void CleanUpDay()
    {
        if (_dayState != null)
        {
            _fileSystem.File.WriteAllText(StatePathName, _dayState.ToJson());
        }

        _userInterface.CleanUpDay(); 
    }

    private AnswerStatus SubmitAnswer(int id, string answer) => _userInterface.SubmitAnswer(id, answer);

    private string GetPersonalInput() => string.IsNullOrEmpty(_data) ? _userInterface.GetPersonalInput() : _data;
}