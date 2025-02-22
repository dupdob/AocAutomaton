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
using System.Diagnostics;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text.RegularExpressions;
using NFluent;
using NFluent.Mocks;
using NUnit.Framework;

namespace AoC.AoCTests
{
    public class EngineShould
    {
        [Test]
        public void SetUpEverythingProperly()
        {
            var mockFileSystem = TestHelpers.GetFileSystem();
            var fakeClient = new AoCFakeClient();

            const string testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, null, null);
            using var console = new CaptureConsole();
            engine.RunDay(() => algo);
            // verify the day is properly set up
            Check.That(engine.Day).IsEqualTo(10);
            // it should have received the provided input data
            Check.That(algo.InputData).IsEqualTo(testInputData);
            Check.That(fakeClient.NbRequest).IsEqualTo(1);
        }
        
        [Test]
        public void BeAbleToUseFileAsAnInput()
        {
            var fileSystem = new MockFileSystem();
            var sut = new Automaton(0, new ConsoleUserInterface(), fileSystem);
            const string dataFile = "Input.txt";
            fileSystem.AddFile(dataFile, new MockFileData("Data from file"));
            var solver = new AutoFakeSolverWithParam();
            using var console = new CaptureConsole();
            sut.LoadUserData(dataFile);
            sut.RunDay(() => solver);

            Check.That(solver.Data).IsEqualTo("Data from file");
        }
        
        [Test]
        public void AutoBuildProperInstance()
        {
            var mockFileSystem = TestHelpers.GetFileSystem();
            var fakeClient = new AoCFakeClient();
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.WrongAnswerFile);

            const string testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            using var console = new CaptureConsole();
            var count = AutoFakeSolver.Count;
            engine.RunDay<AutoFakeSolver>();
            // one instance should have been created
            Check.That(AutoFakeSolver.Count).IsEqualTo(count + 2);
            // verify the day is properly set up
            Check.That(engine.Day).IsEqualTo(10);
        }

        [Test]
        public void FailToBuildInstanceIfNoParameterlessConstructor()
        {
            var mockFileSystem = TestHelpers.GetFileSystem();
            var fakeClient = new AoCFakeClient();
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.WrongAnswerFile);

            const string testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            using var console = new CaptureConsole();
            Check.ThatCode(() => engine.RunDay<FakeSolver>()).ThrowsAny();
        }

        [Test]
        public void HandleWhenNoAnswerProvided()
        {
            var mockFileSystem = TestHelpers.GetFileSystem();
            var fakeClient = new AoCFakeClient();

            var testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, null, null);
            using var console = new CaptureConsole();
            engine.RunDay(() => algo);
            // it should request the first answer
            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            // and not the second
            Check.That(algo.GetAnswer2Calls).IsEqualTo(0);
            // it should say that no answer was provided
            Check.That(console.Output).Contains("No answer provided");
            // it should have received the provided input data
            Check.That(algo.InputData).IsEqualTo(testInputData);
        }

        [Test]
        public void HandleWrongAnswer()
        {
            var fakeClient = new AoCFakeClient();
            var mockFileSystem = TestHelpers.GetFileSystem();
            using var console = new CaptureConsole();

            fakeClient.SetAnswerResponseFilename(1, TestHelpers.WrongAnswerFile);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, 58, null);
            engine.RunDay(() => algo);

            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            Check.That(algo.GetAnswer2Calls).IsEqualTo(0);

            Check.That(console.Output).Contains("AoC site response");
            Check.That(console.Output).Contains("Day 10-1:");
            Check.That(mockFileSystem.AllFiles.Any(p => Regex.IsMatch(p, "Answer1.*\\.html")));
            Check.That(console.Output).Contains("Question 1 failed!");
        }
        
        [Test]
        public void HandleAnswerStillToHigh()
        {
            var fakeClient = new AoCFakeClient();
            var mockFileSystem = TestHelpers.GetFileSystem();
            using var console = new CaptureConsole();
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, 58, null);
            // our attempt will get a 'too high response'
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.AnswerIsTooHigh);
            engine.RunDay(() => algo);
            var calls = fakeClient.NbSubmissions;
            // our next attempt is higher than the previous one
            // so it must not be submitted to the AoC site.
            var secondAlgo = new FakeSolver(10, 60, null);
            engine.RunDay(() => secondAlgo);

            // no supplemental call
            Check.That(fakeClient.NbSubmissions).IsEqualTo(calls);
        }
        
        [Test]
        public void HandleAnswerStillToLow()
        {
            var fakeClient = new AoCFakeClient();
            var mockFileSystem = TestHelpers.GetFileSystem();
            using var console = new CaptureConsole();
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, 58, null);
            // our attempt will get a 'too low response'
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.AnswerIsTooLow);
            engine.RunDay(() => algo);
            var calls = fakeClient.NbSubmissions;
            // our next attempt is higher than the previous one
            // so it must not be submitted to the AoC site.
            var secondAlgo = new FakeSolver(10, 54, null);
            engine.RunDay(() => secondAlgo);

            // no supplemental call
            Check.That(fakeClient.NbSubmissions).IsEqualTo(calls);
        }
        
        [Test]
        public void UpdateHighLimit()
        {
            var fakeClient = new AoCFakeClient();
            var mockFileSystem = TestHelpers.GetFileSystem();
            using var console = new CaptureConsole();
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, 58, null);
            // our attempt will get a 'too high response'
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.AnswerIsTooHigh);
            engine.RunDay(() => algo);
            var calls = fakeClient.NbSubmissions;
            // our next attempt is lower than the previous one, but still too high
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.AnswerIsTooHigh);
            // so it must be submitted to the AoC site.
            var secondAlgo = new FakeSolver(10, 54, null);
            engine.RunDay(() => secondAlgo);
            Check.That(fakeClient.NbSubmissions).IsEqualTo(calls + 1);
            // our next attempt is higher than the previous one
            // so it must not be submitted to the AoC site, proving that the upper limit has been adjusted
            var thirdAlgo = new FakeSolver(10, 56, null);
            engine.RunDay(() => thirdAlgo);
            // no supplemental call
            Check.That(fakeClient.NbSubmissions).IsEqualTo(calls+1);
        }
        
        [Test]
        public void UpdateLowLimit()
        {
            var fakeClient = new AoCFakeClient();
            var mockFileSystem = TestHelpers.GetFileSystem();
            using var console = new CaptureConsole();
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, 54, null);
            // our attempt will get a 'too high response'
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.AnswerIsTooLow);
            engine.RunDay(() => algo);
            var calls = fakeClient.NbSubmissions;
            // our next attempt is lower than the previous one, but still too high
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.AnswerIsTooLow);
            // so it must be submitted to the AoC site.
            var secondAlgo = new FakeSolver(10, 58, null);
            engine.RunDay(() => secondAlgo);
            Check.That(fakeClient.NbSubmissions).IsEqualTo(calls + 1);
            // our next attempt is higher than the previous one
            // so it must not be submitted to the AoC site, proving that the upper limit has been adjusted
            var thirdAlgo = new FakeSolver(10, 56, null);
            engine.RunDay(() => thirdAlgo);
            // no supplemental call
            Check.That(fakeClient.NbSubmissions).IsEqualTo(calls+1);
        }
        
        [Test]
        public void HandleWait()
        {
            var fakeClient = new AoCFakeClient();
            var mockFileSystem = TestHelpers.GetFileSystem();
            using var console = new CaptureConsole();

            fakeClient.SetAnswerResponseFilename(1, TestHelpers.NeedToWaitFile);
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.GoodAnswerFile);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, 58, null);
            var start = new Stopwatch();
            start.Start();
            engine.RunDay(() => algo);
            start.Stop();
            Check.That(start.Elapsed).IsGreaterThan(TimeSpan.FromSeconds(3));
            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            Check.That(algo.GetAnswer2Calls).IsEqualTo(1);

            Check.That(console.Output).Contains("AoC site response");
            Check.That(console.Output).Contains("Day 10-1:");
            Check.That(mockFileSystem.AllFiles.Any(p => Regex.IsMatch(p, "Answer1.*\\.html")));
            Check.That(console.Output).Contains("Question 1 passed!");
        }

        [Test]
        public void HandleError500()
        {
            var fakeClient = new AoCFakeClient();
            var mockFileSystem = TestHelpers.GetFileSystem();
            using var console = new CaptureConsole();

            fakeClient.SetAnswerResponseFilename(1, TestHelpers.InternalErrorFile);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, 58, null);
            engine.RunDay(() => algo);

            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            Check.That(algo.GetAnswer2Calls).IsEqualTo(0);

            Check.That(console.Output).Contains("AoC site response");
            Check.That(console.Output).Contains("Day 10-1:");
            Check.That(mockFileSystem.AllFiles.Any(p => Regex.IsMatch(p, "Answer1.*\\.html")));
            Check.That(console.Output).Contains("Internal Server Error");
        }
        
        [Test]
        public void HandleError400()
        {
            var fakeClient = new AoCFakeClient();
            var mockFileSystem = TestHelpers.GetFileSystem();
            using var console = new CaptureConsole();

            fakeClient.SetAnswerResponseFilename(1, TestHelpers.BadRequestFile);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, 58, null);
            engine.RunDay(() => algo);

            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            Check.That(algo.GetAnswer2Calls).IsEqualTo(0);

            Check.That(console.Output).Contains("AoC site response");
            Check.That(console.Output).Contains("Day 10-1:");
            Check.That(mockFileSystem.AllFiles.Any(p => Regex.IsMatch(p, "Answer1.*\\.html")));
            Check.That(console.Output).Contains("Bad Request");
        }
        
        [Test]
        public void HandleGoodAnswer()
        {
            var fakeClient = new AoCFakeClient();
            using var console = new CaptureConsole();
            var mockFileSystem = TestHelpers.GetFileSystem();

            fakeClient.SetAnswerResponseFilename(1, TestHelpers.GoodAnswerFile);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, 12, null);
            engine.RunDay(() => algo);

            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            Check.That(console.Output).Contains("AoC site response");
            Check.That(console.Output).Contains("Day 10-1:");
            Check.That(algo.GetAnswer2Calls).IsEqualTo(1);
            Check.That(mockFileSystem.AllFiles.Any(p => Regex.IsMatch(p, "Answer1.*\\.html")));
            Check.That(console.Output).Contains("Question 1 passed!");
        }
        
        [Test]
        public void ResetAlgorithmBetweenQuestions()
        {
            var fakeClient = new AoCFakeClient();
            using var console = new CaptureConsole();
            var mockFileSystem = TestHelpers.GetFileSystem();
            var buildCounter = 0;
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.GoodAnswerFile);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, 12, null, automaton => automaton.ResetBetweenQuestions = true);
            engine.ResetBetweenQuestions = true;
            engine.RunDay(() =>
            {
                buildCounter++;
                return algo;
            });
            // three solver will be built
            // one for day setup and one for each question
            Check.That(buildCounter).IsEqualTo(3);
        }

        [Test]
        public void HandlerWrongAnswerToQuestion2()
        {
            var fakeClient = new AoCFakeClient();
            using var console = new CaptureConsole();
            var mockFileSystem = TestHelpers.GetFileSystem();

            fakeClient.SetAnswerResponseFilename(1, TestHelpers.GoodAnswerFile);
            fakeClient.SetAnswerResponseFilename(2, TestHelpers.WrongAnswerFile);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, 12, 13);
            engine.RunDay(() => algo);

            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            Check.That(console.Output).Contains("AoC site response");
            Check.That(console.Output).Contains("Day 10-1:");
            Check.That(console.Output).Contains("Day 10-2:");
            Check.That(algo.GetAnswer2Calls).IsEqualTo(1);
            Check.That(mockFileSystem.AllFiles.Any(p => Regex.IsMatch(p, "Answer1.*\\.html")));
            Check.That(mockFileSystem.AllFiles.Any(p => Regex.IsMatch(p, "Answer2.*\\.html")));
            Check.That(console.Output).Contains("Question 1 passed!");
            Check.That(console.Output).Contains("Question 2 failed!");
        }

        [Test]
        public void HandlerGoodAnswerToQuestion2()
        {
            var fakeClient = new AoCFakeClient();
            using var console = new CaptureConsole();
            var mockFileSystem = TestHelpers.GetFileSystem();

            fakeClient.SetAnswerResponseFilename(1, TestHelpers.GoodAnswerFile);
            fakeClient.SetAnswerResponseFilename(2, TestHelpers.GoodAnswerFile);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, 12, 13);
            engine.RunDay(() => algo);

            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            Check.That(console.Output).Contains("Question 1 passed!");
            Check.That(console.Output).Contains("Question 2 passed!");
        }

        [Test]
        public void CacheInputData()
        {
            var mockFileSystem = TestHelpers.GetFileSystem();
            var fakeClient = new AoCFakeClient();

            const string testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            // run the algo twice
            engine.RunDay(() => new FakeSolver(10, null, null));
            engine.RunDay(() => new FakeSolver(10, null, null));
            // input data should have been called once
            Check.That(fakeClient.NbRequest).IsEqualTo(1);
        }

        [Test]
        public void HandleCorruptedState()
        {
            var mockFileSystem = TestHelpers.GetFileSystem();
            var fakeClient = new AoCFakeClient();

            const string testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var year = 2015;
            var day = 10;
            mockFileSystem.AddFile($"AoC-{year,4}-{day,2}-state.json", new MockFileData("this is definitely not json"));
            var engine = new Automaton(year, inputInterface, mockFileSystem);
            // run the algo twice
            engine.RunDay(() => new FakeSolver(day, null, null));
            // input data should have been called once
            Check.That(fakeClient.NbRequest).IsEqualTo(1);
        }

        private const string MockState =
            """
            {
                "SchemaVersion": "1",
                "Day": 10,
                "First": {
                    "Solved": false,
                    "Answer": null,
                    "Attempts": [],
                    "Low": null,
                    "High": null
                },
                "Second": {
                    "Solved": false,
                    "Answer": null,
                    "Attempts": [],
                    "Low": null,
                    "High": null
                }
            }
            """;
        
        [Test]
        public void HandleIncorrectState()
        {
            var mockFileSystem = TestHelpers.GetFileSystem();
            var fakeClient = new AoCFakeClient();

            const string testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var year = 2015;
            var day = 11;
            mockFileSystem.AddFile($"AoC-{year,4}-{day,2}-state.json", new MockFileData(MockState));
            var engine = new Automaton(year, inputInterface, mockFileSystem);
            using var console = new CaptureConsole();
            // run the algo twice
            engine.RunDay(() => new FakeSolver(day, null, null));
            // input data should have been called once
            Check.That(console.Output)
                .Contains(
                    "Warning: failed to restore state, day does not match expectation: 10 instead of 11.");
        }
        
        [Test]
        public void SkipAlreadyAnsweredQuestions()
        {
            var fakeClient = new AoCFakeClient();
            using var console = new CaptureConsole();
            var mockFileSystem = TestHelpers.GetFileSystem();

            fakeClient.SetAnswerResponseFilename(1, TestHelpers.AlreadyAnsweredFile);
            fakeClient.SetAnswerResponseFilename(2, TestHelpers.WrongAnswerFile);
            var inputInterface = new HttpInterface(fakeClient, mockFileSystem);
            var engine = new Automaton(2015, inputInterface, mockFileSystem);
            var algo = new FakeSolver(10, 12, 13);
            engine.RunDay(() => algo);

            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            Check.That(console.Output).Contains("AoC site response");
            Check.That(console.Output).Contains("Day 10-1:");
            Check.That(console.Output).Contains("Day 10-2:");
            Check.That(algo.GetAnswer2Calls).IsEqualTo(1);
            Check.That(mockFileSystem.AllFiles.Any(p => Regex.IsMatch(p, "Answer1.*\\.html")));
            Check.That(mockFileSystem.AllFiles.Any(p => Regex.IsMatch(p, "Answer2.*\\.html")));
            Check.That(console.Output).Contains("Question 1 passed!");
            Check.That(console.Output).Contains("Question 2 failed!");
        }
    }
}