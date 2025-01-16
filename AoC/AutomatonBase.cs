// MIT License
// 
//  AocAutomaton
// 
//  Copyright (c) 2023 Cyrille DUPUYDAUBY
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
using System.Linq;

namespace AoC;

public abstract class AutomatonBase
{
    private readonly Dictionary<string, TestData> _tests = new ();
    private TestData _last;
    private int _currentDay;
    protected DayState DayState;
    
    /// <summary>
    /// Build a new solver for question 2 instead of using the one build for question 1 (if sets to true).
    /// </summary>
    public bool ResetBetweenQuestions { get; set; }

    /// <summary>
    /// xGets/sets the current day
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
            _tests.Clear(); 
            _currentDay = value;
        }
    }

    protected static void Trace(string message) => Console.WriteLine(message);

    protected static void ReportError(string message) => Console.Error.WriteLine(message);

    private static object GetAnswer(ISolver algorithm, int id, string data)
    {
        var clock = new Stopwatch();
        Trace($"Computing answer {id} ({DateTime.Now:HH:mm:ss}).");
        clock.Start();
        var answer = id == 1 ? algorithm.GetAnswer1(data) : algorithm.GetAnswer2(data);
        clock.Stop();
        var message = clock.ElapsedMilliseconds < 2000 ? $"{clock.ElapsedMilliseconds} ms" : $"{clock.Elapsed:c}";
        Trace($"Took {message}.");
        return answer;
    }

    private bool RunTests(int id, SolverFactory factory)
    {
        var success = true;
        if (_tests.Count == 0)
        {
            return true;
        }
        Trace($"* Test question {id} *");
        // is there are no expected value or visual confirmation registered for this question
        if (!_tests.Values.Any(t => t.CanTest(id)))
        {
            // we ensure at least one visual confirmation 
            _tests.Values.First().SetVisualConfirm(id);
        }
        foreach (var testData in _tests.Values)
        {
            var expected = testData.Answers[id - 1];
            if (expected == null && (id==2 || ResetBetweenQuestions || (id == 1 && testData.Answers[1]!= null)) 
                                 && !testData.VisualConfirm[id - 1])
            {
                // no expected value, no visual confirm and no need for the result, we skip
                continue;
            }
            var data = testData.Data;
            // gets a cached algorithm if any
            var testAlgo = factory.GetSolver(data);
            testAlgo.InitRun(true, testData.ExtraParameters);
            var answer = GetAnswer(testAlgo, id, data);
            // no answer provided
            if (answer == null)
            {
                Trace($"Test failed: got no answer instead of {expected} using:");
                Trace(data);
                success = false;
            }
            // no expected answer provided, we request manual confirmation 
            else if (expected == null || testData.VisualConfirm[id-1])
            {
                if (!testData.VisualConfirm[id - 1])
                {
                    continue;
                }
                Trace("Testing with:");
                Trace(data);
                Trace("provided a result but no expected answer provided. Please confirm result manually (y/n). Result below.");
                Trace(answer.ToString());
                if (!AskYesNo())
                {
                    success = false;
                }
            }
            // not the expected answer
            else if (!answer.ToString()!.Equals(expected.ToString()))
            {
                Trace($"Test failed: got {answer} instead of {expected} using:");
                Trace(data);
                success = false;
            }
            else
            {
                Trace($"Test succeeded: got {answer} using:");
                Trace(data);
            }
        }

        return success;
    }

    private static bool AskYesNo()
    {
        var assessment = Console.ReadLine()?.ToLower();
        return !string.IsNullOrEmpty(assessment) && assessment[0] == 'y';
    }

    /// <summary>
    /// Registers test data so that they are used for validating the solver
    /// </summary>
    /// <param name="data">input data as a string.</param>
    /// <param name="expected">expected result (either string or a number).</param>
    /// <param name="question">question id (1 or 2)</param>
    /// <returns>The automation base instance for linked calls.</returns>
    public AutomatonBase RegisterTestDataAndResult(string data, object expected, int question)
    {
        if (!_tests.TryGetValue(data, out var set))
        {   
            set = new TestData(data);
            _tests[data] = set;
        }
        if (question == 1)
        {
            set.Answer1(expected);
        }
        else
        {
            set.Answer2(expected);
        }

        _last = set;
        return this;
    }

    /// <summary>
    /// Register a new test.
    /// </summary>
    /// <param name="data">input data for this test</param>
    /// <param name="init">initial data for this data</param>
    /// <returns>a <see cref="TestData"/> instance that must be enriched with expected results.</returns>
    /// <returns>The automation base instance for linked calls.</returns>
    public TestData RegisterTest(string data)
    {
        _last = new TestData(data);
        return _tests[data] = _last;
    }

    /// <summary>
    /// Registers test data so that they are used for validating the solver
    /// </summary>
    /// <param name="data">input data as a string.</param>
    /// <returns>The automation base instance for linked calls.</returns>
    public AutomatonBase RegisterTestData(string data)
    {
        RegisterTest(data);
        return this;
    }

    /// <summary>
    /// Add example data so that they are used for validating the solver
    /// </summary>
    /// <param name="data">input data as a string.</param>
    /// <returns>The automation base instance for linked calls.</returns>
    public TestData AddExample(string data) => RegisterTest(data);

    /// <summary>
    /// Register that the result should be manually confirmed during execution
    /// </summary>
    /// <param name="question">question's answer to be confirmed manually</param>
    /// <returns>The automation base instance for linked calls.</returns>
    public AutomatonBase AskVisualConfirm(int question)
    {
        _last.SetVisualConfirm(question);
        return this;
    }

    /// <summary>
    /// Registers test result so that they are used for validating the solver
    /// </summary>
    /// <remarks>You need to declare the associated test data first with <see cref="RegisterTestData"/></remarks>
    /// <param name="expected">expected result (either string or a number).</param>
    /// <param name="question">question id (1 or 2)</param>
    /// <returns>The automation base instance for linked calls.</returns>
    public AutomatonBase RegisterTestResult(object expected, int question = 1)
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

    protected bool RunDay(SolverFactory factory)
    {
        try
        {
            ResetAutomaton();

            factory.GetSolver(string.Empty).SetupRun(this);
            if (Day == 0)
            {
                if (DateTime.Now.Month != 12)
                {
                    ReportError($"Error: please specify target day using Day property.");
                    return false;
                }
                Day = DateTime.Now.Day;
                Trace($"Warning: Day not set, assuming day {Day} (use Day property to set day#).");
            }
            
            InitializeDay(Day);
            if (DayState.First.Solved && DayState.Second.Solved)
            {
                Trace($"Day {Day} has already been solved (first part:{DayState.First.Answer}, second part:{DayState.Second.Answer}). Nothing to do.");
                Trace("Do you want to run it anyway?");
                if (!AskYesNo())
                {
                    return true;
                }
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
            Trace("* Computing answer 1 from your input. *");
            if (!CheckResponse(1, GetAnswer(factory.GetSolver(data), 1, data)))
            {
                return false;
            }

            if (Day == 25)
            {
                Trace($"* Only one question on day {Day}. You're done! *");
                return true;
            }
            
            if (!RunTests(2, factory))
            {
                return false;
            }

            Trace("* Computing answer 2 from your input. *");
            return CheckResponse(2, GetAnswer(factory.GetSolver(data), 2, data));
        }
        finally
        {
            CleanUpDay();
        }
    }

    private void ResetAutomaton()
    {
        Day = 0;
        ResetBetweenQuestions = false;
        _tests.Clear();
    }

    private bool CheckResponse(int id, object answer)
    {
        var answerText = answer?.ToString();
        if (string.IsNullOrWhiteSpace(answerText))
        {
            Trace($"No answer provided! Please overload GetAnswer{id}() with your code.");
            return false;
        }

        var state = GetQuestionState(id);

        // new attempt
        state.Attempts.Add(answerText);
        var success = answer switch
        {
            int number => CheckRange(number, state) && SubmitAnswer(id, answerText),
            long lNumber => CheckRange(lNumber, state) && SubmitAnswer(id, answerText),
            _ => SubmitAnswer(id, answerText)
        };
        if (success)
        {
            state.Answer = answerText;
            state.Solved = true;
        }
        Trace($"Question {id} {(success ? "passed" : "failed")}!");
        return success;
    }

    protected DayQuestion GetQuestionState(int id) => id == 1 ? DayState.First : DayState.Second;

    private static bool CheckRange(long number, DayQuestion state)
    {
        if (state.Low.HasValue && number <= state.Low.Value)
        {
            Trace(state.Low == number
                ? $"Answer not submitted. '{number}' was attempted and reported as too low."
                : $"Answer not submitted. Previous attempt '{state.Low.Value}' was reported as too low and {number} is also too low.");
            return false;
        }

        if (!state.High.HasValue || number < state.High.Value)
        {
            return true;
        }
        
        Trace(state.High == number
            ? $"Answer not submitted. '{number}' was attempted and reported as too high."
            : $"Answer not submitted. Previous attempt '{state.High.Value}' was reported as too high and {number} is also too high.");
        return false;
    }

    protected virtual void InitializeDay(int day)
    {
        DayState = new DayState { Day = Day };
    }

    protected virtual void CleanUpDay()
    {}

    protected abstract bool SubmitAnswer(int id, string answer);

    protected abstract string GetPersonalInput();
}