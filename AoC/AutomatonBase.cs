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

namespace AoC;

public abstract class AutomatonBase
{
    private readonly Dictionary<int, List<(string data, object result)>> _testData = new();
    private int _currentDay;

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
            _testData.Clear();
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
        if (!_testData.ContainsKey(id))
        {
            return true;
        }
        var success = true;
        Console.WriteLine($"* Test question {id} *");
        foreach (var (data, expected) in GetTestInfo(id))
        {
            // gets a cached algorithm if any
            var testAlgo = factory.GetSolver(data);
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
                Console.WriteLine($"Test failed: got a result but no expected answer provided. Please confirm result manually (y/n). Result below.");
                Console.WriteLine(answer);
                var assessment = Console.ReadLine()!.ToLower();

                if (assessment.Length == 0 || assessment[0] != 'y')
                {
                    success = false;
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
    ///     Get the test data.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private IEnumerable<(string data, object result)> GetTestInfo(int id)
    {
        for (var i = 0; i < _testData[id].Count; i++)
        {
            var (data, result) = _testData[id][i];
            if (string.IsNullOrEmpty(data))
            {
                if (id == 1)
                    throw new Exception($"Can't test question 1 for expected {result}. No associated test data.");

                if (_testData[1].Count <= i)
                    throw new Exception(
                        $"Can't test question 2 for expected {result}. No associated test data (incl. for question 1).");
                // fetch data used for question 1.
                data = _testData[1][i].data;
            }

            yield return (data, result);
        }
    }

    /// <summary>
    ///     Registers test data so that they are used for validating the solver
    /// </summary>
    /// <param name="data">input data as a string.</param>
    /// <param name="expected">expected result (either string or a number).</param>
    /// <param name="question">question id (1 or 2)</param>
    public AutomatonBase RegisterTestDataAndResult(string data, object expected, int question)
    {
        if (!_testData.ContainsKey(question))
        {
            _testData[question] = new List<(string data, object result)>();
        }
        _testData[question].Add((data, expected));
        return this;
    }

    /// <summary>
    ///     Registers test data so that they are used for validating the solver
    /// </summary>
    /// <param name="data">input data as a string.</param>
    /// <param name="question">question id (1 or 2)</param>
    public AutomatonBase RegisterTestData(string data, int question = 3)
    {
        if (question == 3)
        {
            StoreTestData(1, data);
            StoreTestData(2, data);
        }
        else
        {
            StoreTestData(question, data);
        }
        return this;
    }

    private void StoreTestData(int question, string data)
    {
        if (!_testData.ContainsKey(question))
        {
            _testData[question] = new List<(string data, object result)>();
        }

        _testData[question].Add((data, null));
    }

    /// <summary>
    ///     Registers test result so that they are used for validating the solver
    /// </summary>
    /// <remarks>You need to declare the associated test data first with <see cref="RegisterTestData"/></remarks>
    /// <param name="expected">expected result (either string or a number).</param>
    /// <param name="question">question id (1 or 2)</param>
    public AutomatonBase RegisterTestResult(object expected, int question = 1)
    {
        if (!_testData.ContainsKey(question))
        {
            if (question == 2 && _testData.ContainsKey(1))
            {
                // we assume the data is reused across questions
                RegisterTestData(_testData[1][^1].data, 2);
            }
            else
            {
                throw new ApplicationException("You must call RegisterTestData before calling this method.");
            }
        }

        if (_testData[question][^1].result != null)
        {
            throw new ApplicationException("You must call RegisterTestData before calling this method.");
        }
        _testData[question][^1] = (_testData[question][^1].data, expected);
        return this;
    }

    protected bool RunDay(SolverFactory factory)
    {
        try
        {
            Day = 0;
            ResetBetweenQuestions = false;

            factory.GetSolver(string.Empty).SetupRun(this);
            if (Day == 0)
            {
                Console.Error.WriteLine("Please specify current day via the Day property");
                return false;
            }
            
            InitializeDay(Day);
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
            return CheckResponse(2, GetAnswer(factory.GetSolver(data), 2, data));
        }
        finally
        {
            CleanUpDay();
        }
    }

    private bool CheckResponse(int id, object answer)
    {
        if (answer == null ||string.IsNullOrWhiteSpace(answer.ToString()))
        {
            Console.WriteLine($"No answer provided! Please overload GetAnswer{id}() with your code.");
            return false;
        }

        var success = SubmitAnswer(id, answer.ToString());
        Console.WriteLine("Question {0} {1}!", id, success ? "passed" : "failed");
        return success;
    }

    protected virtual void CleanUpDay()
    {
    }

    protected abstract bool SubmitAnswer(int id, string answer);

    protected abstract string GetPersonalInput();

    protected virtual void InitializeDay(int day)
    {
    }
}