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

    private static object GetAnswer(ISolver algorithm, int id, string data)
    {
        var clock = new Stopwatch();
        Console.WriteLine($"Computing answer {id} ({DateTime.Now:HH:mm:ss}).");
        clock.Start();
        var answer = id == 1 ? algorithm.GetAnswer1(data) : algorithm.GetAnswer2(data);
        clock.Stop();
        var message = clock.ElapsedMilliseconds < 2000 ? $"{clock.ElapsedMilliseconds} ms" : $"{clock.Elapsed:c}";
        Console.WriteLine($"Took {message}.");
        return answer;
    }

    private bool RunTests(int id, SolverFactory factory)
    {
        var success = true;
        if (_tests.Count == 0)
        {
            return true;
        }
        Console.WriteLine($"* Test question {id} *");
        var solved = DayState?.First.Solved == true;
        // is there are no expected value or visual confirmation registered for this question
        if (!_tests.Values.Any(t => t.CanTest(id)))
        {
            // we ensure at least one visual confirmation 
            _tests.Values.First().SetVisualConfirm(id);
        }
        foreach (var testData in _tests.Values)
        {
            var expected = testData.Answers[id - 1];
            var data = testData.Data;
            // gets a cached algorithm if any
            var testAlgo = factory.GetSolver(data, testData.Init);
            testData.SetupActions[id - 1]?.Invoke();
            var answer = GetAnswer(testAlgo, id, data);
            // no answer provided
            if (answer == null)
            {
                Console.WriteLine($"Test failed: got no answer instead of {expected} using:");
                Console.WriteLine(data);
                success = false;
            }
            // no expected answer provided, we request manual confirmation 
            else if (expected == null)
            {
                if (testData.VisualConfirm[id-1])
                {
                    Console.WriteLine("Test failed: got a result but no expected answer provided. Please confirm result manually (y/n). Result below.");
                    Console.WriteLine(answer);
                    var assessment = Console.ReadLine()?.ToLower();

                    if (string.IsNullOrEmpty(assessment) || assessment[0] != 'y')
                    {
                        success = false;
                    }
                }
            }
            // not the expected answer
            else if (!answer.ToString()!.Equals(expected.ToString()))
            {
                Console.WriteLine($"Test failed: got {answer} instead of {expected} using:");
                Console.WriteLine(data);
                success = false;
            }
            else
            {
                Console.WriteLine($"Test succeeded: got {answer} using:");
                Console.WriteLine(data);
            }
        }

        return success;
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
    public TestData RegisterTest(string data, string init = null)
    {
        _last = new TestData(data, init);
        return _tests[data] = _last;
    }

    /// <summary>
    /// Registers test data so that they are used for validating the solver
    /// </summary>
    /// <param name="data">input data as a string.</param>
    /// <param name="init">initial data for this data</param>
    /// <returns>The automation base instance for linked calls.</returns>
    public AutomatonBase RegisterTestData(string data, string init = null)
    {
        RegisterTest(data, init);
        return this;
    }
    
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
            _tests.Clear();

            factory.GetSolver(string.Empty).SetupRun(this);
            if (Day == 0)
            {
                Console.Error.WriteLine("Please specify current day via the Day property");
                return false;
            }

            DayState = new DayState { Day = Day };
            InitializeDay(Day);
            if (DayState?.First.Solved == true && DayState.Second.Solved)
            {
                Console.WriteLine("Day has already been solved. Nothing to do.");
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
            Console.WriteLine("* Computing answer 1 from your input. *");
            if (!CheckResponse(1, GetAnswer(factory.GetSolver(data), 1, data)))
            {
                return false;
            }
            
            if (!RunTests(2, factory))
            {
                return false;
            }

            Console.WriteLine("* Computing answer 2 from your input. *");
            if (CheckResponse(2, GetAnswer(factory.GetSolver(data), 2, data)))
            {
                return true;
            }

            return false;
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
    }

    private bool CheckResponse(int id, object answer)
    {
        var answerText = answer?.ToString();
        if (string.IsNullOrWhiteSpace(answerText))
        {
            Console.WriteLine($"No answer provided! Please overload GetAnswer{id}() with your code.");
            return false;
        }

        var state = id == 1 ? DayState.First : DayState.Second;
        bool success;
        if (!state.Attempts.Contains(answerText))
        {
            // new attempt
            state.Attempts.Add(answerText);
            success = SubmitAnswer(id, answerText);
            if (success)
            {
                state.Answer = answerText;
                state.Solved = true;
            }
        }
        else
        {
            // already tried, was it the correct answer?
            success = state.Answer == answerText;
        }
        Console.WriteLine("Question {0} {1}!", id, success ? "passed" : "failed");
        return success;
    }

    protected virtual void InitializeDay(int day)
    {}

    protected virtual void CleanUpDay()
    {}

    protected abstract bool SubmitAnswer(int id, string answer);

    protected abstract string GetPersonalInput();


}