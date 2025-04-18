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
// FITNESS FOR A PARTICULAR PURPOSE AND NON INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.IO;
using System.IO.Abstractions;
using System.Net.Http;

using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AoC
{
    /// <summary>
    /// This class is an AoC automaton handling interactions with the AoC website
    /// </summary>
    public class HttpInterface : IInteract, IDisposable
    {
        // AoC answers RegEx
        private static readonly Regex GoodAnswer = new(".*That's the right answer!.*");
        private static readonly Regex AnswerTooHigh= new(".*your answer is too high.*");
        private static readonly Regex AnswerTooLow= new(".*your answer is too low.*");
        private static readonly Regex AlreadyAnswered = new(".*You don't seem to be solving the right level\\..*");
        private static readonly Regex TooSoon = new(@".*You have (\d*)m? ?(\d*)s? left to wait\..*");
        private readonly AoCClientBase _client;
        private readonly IFileSystem _fileSystem;
        
        // default input cache name
        private string DataCacheFileName => $"InputAoc-{_day,2}-{_client.Year,4}.txt";
        // filter to add to gitignore to ensure cached input is not committed
        // As per AoC website rules, see _Can I copy/redistribute part of Advent of Code?_
        // at https://adventofcode.com/2024/about
        private const string DataCacheFilter = $"InputAoc-*.txt";
        private const string EnvVarName = "AOC_SESSION";
        private const string SessionTokenFileName = "SessionToken.json";

        private int _day;
        private string _dataPath;
        
        // default path
        private string DataCachePathName => string.IsNullOrEmpty(_dataPath)
            ? DataCacheFileName
            : _fileSystem.Path.Combine(_dataPath, DataCacheFileName);

        // internal state
        private Task<string> _myData;
        private Task _pendingWrite;
        private string _rootPath;

        /// <summary>
        /// Build a new automaton.
        /// </summary>
        /// <param name="client">Client instance <see cref="AoCClientBase"/></param>
        /// <param name="fileSystem">File system, used for test.</param>
        public HttpInterface(AoCClientBase client = null, IFileSystem fileSystem = null)
        {
            _client = client ?? new AoCClient();
            _fileSystem = fileSystem ?? new FileSystem();
        }

        /// <summary>
        ///  Cleanup data. Mainly ensure that ongoing writes are persisted, if any, and closes the HTTP session.
        /// </summary>
        public void Dispose()
        {
            _myData?.Dispose();
            _pendingWrite?.Dispose();
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }

        // ensure data are cached properly
        public void CleanUpDay()
        {
            if (_pendingWrite == null)
            {
                return;
            }
            // wait for cache writing completion
            if (!_pendingWrite.IsCompleted && !_pendingWrite.Wait(500))
            {
                Trace("Local caching of input may have failed!");
            }
            _pendingWrite = null;
        }

        public string GetPersonalInput()
        {
            string result;
            try
            {
                result = _myData.Result;
            }
            catch (AggregateException e)
            {
                if (e.InnerExceptions[0] is HttpRequestException requestException)
                {
                    var answer = AnalyseInvalidAnswer(requestException.Message);
                    if (answer.InvalidCookie && answer.CanBeRetried && RequestCookie())
                    {
                        // perform a sync request
                        result = _client.RequestPersonalInput().Result;
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                    throw;
            }
            
            if (_fileSystem.File.Exists(DataCachePathName))
            {
                return result;
            }

            var directoryName = Path.GetDirectoryName(DataCachePathName);
            if (!string.IsNullOrEmpty(directoryName) && !_fileSystem.Directory.Exists(directoryName))
            {
                _fileSystem.Directory.CreateDirectory(directoryName);
            }
            _pendingWrite = _fileSystem.File.WriteAllTextAsync(DataCachePathName, result);
            _fileSystem.File.SetAttributes(DataCachePathName, FileAttributes.ReadOnly);
            return result;
        }

        public void Trace(string message) => Console.WriteLine(message);

        public void ReportError(string message) => Console.Error.WriteLine(message);

        /// <summary>
        ///     Posts an answer to the AoC website.
        /// </summary> 
        /// <param name="part">question id (1 or 2)</param>
        /// <param name="value">proposed answer (as a string)</param>
        /// <returns>true if this is the good answer</returns>
        /// <remarks>
        ///     Posted answers are cached locally to avoid stressing the AoC website.
        ///     The result of previous answers is stored as a html file. The filename depends on the question and the provided
        ///     answer; it contains either
        ///     the answer itself, or its hash if the answer cannot be part of a filename.
        ///     You can examine the response file to get details and/or removes them to resubmit a previously send proposal.
        /// </remarks>
        public AnswerStatus SubmitAnswer(int part, string value)
        {
            Console.WriteLine($"Day {_day}-{part}: {value} [{_client.Year}].");
            var answerId = Helpers.AsAFileName(value);
            var responseFilename = _fileSystem.Path.Combine(_dataPath, $"Answer{part} for {answerId}.html");
            while (true)
            {
                var responseText = PostAndRetrieve(part, value, responseFilename, out var responseTime);
                // extract the response as plain text
                var answer = ExtractAnswerText(responseText);
                OutputAoCMessage(answer.Message);
                if (!answer.IsOk)
                {
                    if (answer.InvalidCookie && answer.CanBeRetried && RequestCookie())
                    {
                        continue;
                    }
                    // technical error, so we abort
                    return AnswerStatus.Wrong;
                }
                CacheResponse(responseFilename, responseText);
                // did we answer it already
                if (AlreadyAnswered.IsMatch(answer.Message))
                {
                    Trace("Question was already answered.");
                    return AnswerStatus.Good;
                }
                // is it too high?
                if (AnswerTooHigh.IsMatch(answer.Message) && long.TryParse(value, out var number))
                {
                    Trace($"{number} is too high.");
                    return AnswerStatus.TooHigh;
                }
                // or too low?
                if (AnswerTooLow.IsMatch(answer.Message) && long.TryParse(value, out number))
                {
                    Trace($"{number} is too low.");
                    return AnswerStatus.TooLow;
                }
                // did we answer too fast?
                var match = TooSoon.Match(answer.Message);
                if (!match.Success)
                {
                    // so it is either a success or an unsupported message
                    return GoodAnswer.IsMatch(answer.Message) ? AnswerStatus.Good : AnswerStatus.Wrong;
                }
                WaitAppropriateTime(match, responseTime);

                // delete any cached response to ensure we post again
                _fileSystem.File.Delete(responseFilename);
            }
        }

        private void WaitAppropriateTime(Match match, DateTime responseTime)
        {
            var secondsToWait = int.Parse(match.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(match.Groups[2].Value))
            {
                // if there is a second value, the first one is in minutes.
                secondsToWait *= 60;
                secondsToWait+= int.Parse(match.Groups[2].Value);
            }
            // we need to wait.
            responseTime += TimeSpan.FromSeconds(secondsToWait);
            Trace($"Waiting until {responseTime} to push answer.");
            // wait until we can try again
            do
            {
                Thread.Sleep(100);
            } while (responseTime >= DateTime.Now);
        }

        private bool RequestCookie()
        {
            Console.WriteLine("Please provide the session cookie (empty line to abort)");
            var newCookie = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(newCookie)) return false;
            // we need to set the cookie
            _client.SetSessionCookie(newCookie);
            // save it to a file but add it to gitignore as a precaution
            var gitIgnore = new SimpleGitIgnoreManager(_fileSystem);
            gitIgnore.AddFilter(SessionTokenFileName, _rootPath);
            _fileSystem.File.WriteAllText(SessionFilename, new SessionJSon(){Token = newCookie}.ToJson());
            
            return true;
        }

        private void CacheResponse(string responseFilename, string responseText)
        {
            if (_pendingWrite is { IsCompleted: false })
            {
                // await the ongoing write
                _pendingWrite.Wait();
            }

            _pendingWrite = _fileSystem.File.WriteAllTextAsync(responseFilename, responseText);
        }

        private void OutputAoCMessage(string resultText)
        {
            Trace($"AoC site response: '{resultText}'");
        }

        private string PostAndRetrieve(int question, string value, string responseFilename, out DateTime responseTime)
        {
            string responseText;
            // if we already have the response file...
            if (_fileSystem.File.Exists(responseFilename))
            {
                Trace($"Response {value} as already been attempted.");
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

        private AoCAnswer AnalyseInvalidAnswer(string response)
        {
            if (response.Contains("500 Internal Server Error"))
            {
                Trace("AoC: Internal Server Error. This is likely an indication of a corrupted AOC session token. See below for setup documentation.");
                Trace(GetSetupDocumentation());
                return new(false, response, true, true);
            }
            if (response.Contains("400 Bad Request") || response.Contains("400 (Bad Request)") || response.Contains("To play, please identify yourself via one of these services:"))
            {
                Trace("AoC: Bad Request. This is likely due to an expired AOC session token. You need to get a fresh session token. See below for setup documentation.");
                Trace(GetSetupDocumentation());
                return new(false, response, true, true);
            }            
            if (response.Contains("404 Not Found"))
            {
                Trace("AoC: Not Found. This is likely due to an expired AOC session token. You need to get a fresh session token. See below for setup documentation.");
                Trace(GetSetupDocumentation());
                return new(false, response, true, true);
            }
            Trace("Failed to parse response.");
            return new AoCAnswer(false, response);           
        }

        private record AoCAnswer(bool IsOk, string Message, bool CanBeRetried = false, bool InvalidCookie = false);
        
        private AoCAnswer ExtractAnswerText(string response)
        {
            var start = response.IndexOf("<article>", StringComparison.InvariantCulture);
            if (start == -1)
            {
                // no text tag, this is a technical error
                return AnalyseInvalidAnswer(response);
            }

            start += 9;
            var end = response.IndexOf("</article>", start, StringComparison.InvariantCulture);
            if (end == -1)
            {
                // no end tag, we got an incorrect response from server
                Trace("Failed to parse response.");
                return new (false, response);
            }   

            response = response.Substring(start, end - start);
            return new (true, Regex.Replace(response, @"<(.|\n)*?>", string.Empty));
        }

        private string GetSetupDocumentation() =>
            @$"Define an environment variable named {EnvVarName} which value is the Advent of Code session id.
The session id is stored in a cookie, named 'session', valid for '.adventofcode.com'.
To get a valid value, you must log through AoC site first.";

        /// <summary>
        /// Retrieves personal data (associated to the AoC session ID).
        /// </summary>
        /// <param name="year">target year (AoC event)</param>
        /// <param name="day">day to fetch</param>
        /// <param name="rootPath"></param>
        /// <param name="dayPath">path to cache storage</param>
        /// <remarks>Input retrieval is done asynchronously, so it can happen in parallel with testing.</remarks>
        public void InitializeDay(int year, int day, string rootPath, string dayPath)
        {
            if (day == _client.Day)
            {
                return;
            }

            _rootPath = rootPath;
            _dataPath = dayPath;
            _day = day;
            _client.Year = year;
            _client.SetCurrentDay(day);
            var fileName = DataCachePathName;
            var gitIgnore = new SimpleGitIgnoreManager(_fileSystem);
            gitIgnore.AddFilter(DataCacheFilter, _dataPath);
            GetSessionId();

            _myData = _fileSystem.File.Exists(fileName)
                ? _fileSystem.File.ReadAllTextAsync(fileName)
                : _client.RequestPersonalInput();
        }

        private void GetSessionId()
        {
            var filename = SessionFilename;
            // get the token from the environment variable
            var sessionId = Environment.GetEnvironmentVariable(EnvVarName);
            // if we have a file, we will use it instead
            if (_fileSystem.File.Exists(filename))
            {
                try
                {
                    var sessionJson = SessionJSon.FromJson(_fileSystem.File.ReadAllText(filename));
                    if (!string.IsNullOrWhiteSpace(sessionJson.Token))
                    {
                        // we got a valid session id
                        sessionId = sessionJson.Token;
                    }
                }
                catch (Exception e)
                {
                    ReportError($"Failed to load the session token: {e}");
                }
            }

            if (!string.IsNullOrEmpty(sessionId))
            {
                _client.SetSessionCookie(sessionId);
                return;
            }

            if (!RequestCookie())
            {
                throw new InvalidOperationException(GetSetupDocumentation());
            }
        }

        private string SessionFilename => _fileSystem.Path.Combine(_rootPath, SessionTokenFileName);
    }
}