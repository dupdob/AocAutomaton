// MIT License
// 
//  AocAutomaton
// 
//  Copyright (c) 2022 Cyrille DUPUYDAUBY
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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AoC;

public class Automaton : IDisposable
{
    // default input cache name
    // AoC answers RegEx
    private static readonly Regex GoodAnswer = new(".*That's the right answer!.*");
    private static readonly Regex TooSoon = new(".*You have (\\d*)m? (\\d*)s? left to wait\\..*");
    private readonly AoCClientBase _client;
    private readonly IFileSystem _fileSystem;
    private readonly Dictionary<int, List<(string data, object result)>> _testData = new();
    private int _currentDay;
    private string _dataPathNameFormat = ".";
    private Task<string> _myData;
    private Task _pendingWrite;

    public Automaton(int year, AoCClientBase client = null, IFileSystem fileSystem = null)
    {
        _client = client ?? new AoCClient(year);
        _fileSystem = fileSystem ?? new FileSystem();
    }

    // default path
    private string DataPath => string.Format(_dataPathNameFormat, Day, _client.Year);

    /// <summary>
    ///     Gets/sets the current day
    /// </summary>
    public int Day
    {
        get => _currentDay;
        set
        {
            _currentDay = value;
            InitiatePersonalInputFetching(_currentDay);
        }
    }

    private string DataCacheFileName => $"InputAoc-{Day,2}-{_client.Year,4}.txt";

    private string DataCachePathName => string.IsNullOrEmpty(DataPath)
        ? DataCacheFileName
        : _fileSystem.Path.Combine(DataPath, DataCacheFileName);

    /// <summary>
    ///     Cleanup data. Mainly ensure that ongoing writes are persisted, if any, and closes the HTTP session.
    /// </summary>
    public void Dispose()
    {
        _myData?.Dispose();
        _pendingWrite?.Dispose();
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Sets the path used by the engine to cache data (input and response).
    ///     You can provide a format pattern string, knowing that {0} will be replaced by
    ///     the exercise's day and {1} by the year.
    /// </summary>
    /// <param name="dataPath">path (or format string) used to store data.</param>
    /// <returns>This instance.</returns>
    /// <remarks>Relative paths are relative to the engine current directory.</remarks>
    public Automaton SetDataPath(string dataPath)
    {
        _dataPathNameFormat = dataPath;
        return this;
    }

    /// <summary>
    ///     Runs a given day.
    /// </summary>
    /// <typeparam name="T"><see cref="ISolver" /> type for the day.</typeparam>
    /// <exception cref="InvalidOperationException">when the method fails to create an instance of the algorithm.</exception>
    public void RunDay<T>() where T : ISolver
    {
        var dayAlgoType = typeof(T);
        var constructorInfo = dayAlgoType.GetConstructor(Type.EmptyTypes);
        if (constructorInfo == null)
            throw new ApplicationException($"Can't find a parameterless constructor for {dayAlgoType}.");

        ISolver Builder()
        {
            return constructorInfo.Invoke(null) as ISolver ??
                   throw new Exception($"Can't build an instance of {dayAlgoType.Name}.");
        }

        RunDay(Builder);
    }

    public void RunDay(Func<ISolver> builder)
    {
        var dayAlgo = builder();
        Day = 0;
        dayAlgo.SetupRun(this);
        if (Day == 0)
        {
            Console.Error.WriteLine("Please specify current day via the Day property");
            return;
        }

        // tests if data are provided
        var algorithms = new Dictionary<string, ISolver>();
        if (_testData.Count > 0 && !RunTest(1, builder, algorithms)) return;
        // perform the actual run
        var data = RetrieveMyData();
        if (!CheckResponse(1, GetAnswer(dayAlgo, 1, data))) return;
        if (_testData.Count > 0 && !RunTest(2, builder, algorithms)) return;
        if (!CheckResponse(2, GetAnswer(dayAlgo, 2, data))) return;
        EnsureDataIsCached();
    }

    private bool CheckResponse(int id, object answer)
    {
        if (answer == null)
        {
            Console.WriteLine($"No answer provided! Please overload GetAnswer{id}() with your code.");
            return false;
        }

        Console.WriteLine($"Day {Day}-{id}: {answer} [{_client.Year}].");
        var success = PostAnswer(id, answer.ToString());
        Console.WriteLine("Question {0} {1}!", id, success ? "passed" : "failed");
        return success;
    }

    // ensure data are cached properly
    private void EnsureDataIsCached()
    {
        if (_pendingWrite == null) return;
        // wait for cache writing completion
        if (!_pendingWrite.IsCompleted)
            if (!_pendingWrite.Wait(500))
                Console.WriteLine("Local caching of input may have failed!");

        _pendingWrite = null;
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

    private bool RunTest(int id, Func<ISolver> builder, Dictionary<string, ISolver> algorithms)
    {
        if (!_testData.ContainsKey(id)) return true;
        var success = true;
        Console.WriteLine($"* Test question {id} *");
        foreach (var (data, expected) in GetTestInfo(id))
        {
            // gets a cached algorithm if any
            var testAlgo = algorithms.GetValueOrDefault(data);
            if (testAlgo == null)
            {
                testAlgo = builder();
                algorithms[data] = testAlgo;
            }

            var answer = GetAnswer(testAlgo, id, data);
            if (!answer.Equals(expected))
            {
                Console.Error.WriteLine($"Test failed: got {answer} instead of {expected} using:");
                Console.Error.WriteLine(data);
                success = false;
            }
            else
            {
                Console.WriteLine($"Test success: got {answer} using:");
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

    private bool IsAcceptableInAFileName(string name)
    {
        return name.Length <= 20 && name.All(character =>
            char.IsDigit(character) || char.IsLetter(character) || "-_#*".Contains(character));
    }

    private string RetrieveMyData()
    {
        var result = _myData.Result;
        if (_fileSystem.File.Exists(DataCachePathName)) return result;

        var directoryName = Path.GetDirectoryName(DataCachePathName);
        if (!string.IsNullOrEmpty(directoryName) && !_fileSystem.Directory.Exists(directoryName))
            _fileSystem.Directory.CreateDirectory(directoryName ?? throw new InvalidOperationException());
        _pendingWrite = _fileSystem.File.WriteAllTextAsync(DataCachePathName, result);
        return result;
    }

    /// <summary>
    ///     Posts an answer to the AoC website.
    /// </summary>
    /// <param name="question">question id (1 or 2)</param>
    /// <param name="value">proposed answer (as a string)</param>
    /// <returns>true if this is the good answer</returns>
    /// <remarks>
    ///     Posted answers are cached locally to avoid stressing the AoC website.
    ///     The result of previous answers is stored as an html file. The filename depends on the question and the provided
    ///     answer; it contains either
    ///     the answer itself, or its hash if the answer cannot be part of a filename.
    ///     You can examine the response file to get details and/or removes them to resubmit a previously send proposal.
    /// </remarks>
    private bool PostAnswer(int question, string value)
    {
        var answerId = value;
        if (!IsAcceptableInAFileName(answerId)) answerId = answerId.GetHashCode().ToString();
        var responseFilename = _fileSystem.Path.Combine(DataPath, $"Answer{question} for {answerId}.html");
        var responseText = PostAndRetrieve(question, value, responseFilename, out var responseTime);
        // extract the response as plain text
        var resultText = ExtractAnswerText(responseText);
        OutputAoCMessage(resultText);
        // did we answer too fast?
        var match = TooSoon.Match(resultText);
        while (match.Success)
        {
            // we need to wait.
            responseTime += TimeSpan.FromSeconds(int.Parse(match.Groups[2].Value)) +
                            TimeSpan.FromMinutes(int.Parse(match.Groups[1].Value));
            Console.WriteLine($"Wait until {responseTime}.");
            // wait until we can try again
            do
            {
                Thread.Sleep(1000);
            } while (responseTime >= DateTime.Now);

            // delete any cached response to ensure we post again
            _fileSystem.File.Delete(responseFilename);
            // send our new answer
            responseText = PostAndRetrieve(question, value, responseFilename, out responseTime);
            OutputAoCMessage(resultText);
            match = TooSoon.Match(resultText);
        }

        // is it the correct answer ?
        var result = GoodAnswer.IsMatch(resultText);

        if (_pendingWrite is { IsCompleted: false })
            // await the ongoing write
            _pendingWrite.Wait();

        _pendingWrite = _fileSystem.File.WriteAllTextAsync(responseFilename, responseText);
        return result;
    }

    private static void OutputAoCMessage(string resultText)
    {
        Console.WriteLine("AoC site response: '{0}'", resultText);
    }

    private string PostAndRetrieve(int question, string value, string responseFilename, out DateTime responseTime)
    {
        string responseText;
        // if we already have the response file...
        if (_fileSystem.File.Exists(responseFilename))
        {
            Console.WriteLine($"Response {value} as already been attempted.");
            responseText = _fileSystem.File.ReadAllText(responseFilename);
            responseTime = _fileSystem.File.GetLastWriteTime(responseFilename);
        }
        else
        {
            var response = _client.PostAnswer(question, value);
            responseTime = DateTime.Now;
            responseText = response.Result;
        }

        return responseText;
    }

    private static string ExtractAnswerText(string response)
    {
        var start = response.IndexOf("<article>", StringComparison.InvariantCulture);
        if (start == -1)
        {
            Console.WriteLine("Failed to parse response.");
            return response;
        }

        start += 9;
        var end = response.IndexOf("</article>", start, StringComparison.InvariantCulture);
        if (end == -1)
        {
            Console.WriteLine("Failed to parse response.");
            return response;
        }

        response = response.Substring(start, end - start);
        return Regex.Replace(response, @"<(.|\n)*?>", string.Empty);
    }

    /// <summary>
    ///     Retrieves personal data (associated to the AoC session ID).
    /// </summary>
    /// <param name="day">day to fetch</param>
    /// <remarks>Input retrieval is done asynchronously, so it can happen in parallel with testing.</remarks>
    private void InitiatePersonalInputFetching(int day)
    {
        _testData.Clear();
        if (day == _client.Day || day == 0) return;
        _client.SetCurrentDay(day);
        var fileName = DataCachePathName;
        _myData = _fileSystem.File.Exists(fileName)
            ? _fileSystem.File.ReadAllTextAsync(fileName)
            : _client.RequestPersonalInput();
    }

    /// <summary>
    ///     Registers test data so that they are used.
    /// </summary>
    /// <param name="data">input data as a string.</param>
    /// <param name="expected">expected result (either string or a number).</param>
    /// <param name="question">question id (1 or 2)</param>
    public Automaton RegisterTestDataAndResult(string data, object expected, int question)
    {
        if (!_testData.ContainsKey(question)) _testData[question] = new List<(string data, object result)>();
        _testData[question].Add((data, expected));
        return this;
    }

    public Automaton RegisterTestData(string data, int question = 1)
    {
        if (!_testData.ContainsKey(question)) _testData[question] = new List<(string data, object result)>();

        _testData[question].Add((data, null));
        return this;
    }

    public Automaton RegisterTestResult(object result, int question = 1)
    {
        if (!_testData.ContainsKey(question))
            throw new ApplicationException("You must call RegisterTestData before colling this method.");

        _testData[question][^1] = (_testData[question][^1].data, result);
        return this;
    }
}