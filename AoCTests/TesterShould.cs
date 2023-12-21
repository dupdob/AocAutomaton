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

using System.IO;
using System.IO.Abstractions.TestingHelpers;
using NFluent;
using NFluent.Mocks;
using NUnit.Framework;

namespace AoC.AoCTests
{
    public class TesterShould
    {

        private static MockFileSystem GetFileSystem()
        {
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.SetCurrentDirectory(Directory.GetCurrentDirectory());
            mockFileSystem.Directory.CreateDirectory(mockFileSystem.Directory.GetCurrentDirectory());
            return mockFileSystem;
        }

        [Test]
        public void StopWhenFirstTestFails()
        {
            var mockFileSystem = GetFileSystem();
            var fakeClient = new AoCFakeClient(2015);

            var testInputData = "Silly input data";
            fakeClient.SetInputData("don't care");
            var engine = new Automaton(2015, fakeClient, mockFileSystem);
            var algo = new FakeSolver(10, 1, 2, x => x.RegisterTestDataAndResult(testInputData, 2, 1));
            using var console = new CaptureConsole();
            engine.RunDay(() => algo);
            // it should request the first answer once, for the test
            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            // and not the second
            Check.That(algo.GetAnswer2Calls).IsEqualTo(0);
            // it should say that no answer was provided
            Check.That(console.Output).Contains("* Test question 1 *");
            Check.That(console.Output).Contains("Test failed: got 1 instead of 2 using:");
            // it should have received the provided input data
            Check.That(algo.InputData).IsEqualTo(testInputData);
        }

        [Test]
        public void KeepOnWhenFirstTestSucceeds()
        {
            var mockFileSystem = GetFileSystem();
            var fakeClient = new AoCFakeClient(2015);

            var testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.GoodAnswerFile);
            fakeClient.SetAnswerResponseFilename(2, TestHelpers.WrongAnswerFile);
            var engine = new Automaton(2015, fakeClient, mockFileSystem);
            var algo = new FakeSolver(10, 1, 2,
                x => x.RegisterTestDataAndResult(testInputData, 1, 1).RegisterTestResult(2, 2));
            using var console = new CaptureConsole();
            engine.RunDay(() => algo);
            // it should request the first answer twice: test and actual data
            Check.That(algo.GetAnswer1Calls).IsEqualTo(2);
            // and once the second
            Check.That(algo.GetAnswer2Calls).IsEqualTo(2);
            // it should say that no answer was provided
            Check.That(console.Output).Contains("* Test question 1 *");
            Check.That(console.Output).Contains("Question 2 failed!");
            // it should have received the provided input data
            Check.That(algo.InputData).IsEqualTo(testInputData);
        }

        [Test]
        public void UseAllTestDataAndStopsIfOneIsWrong()
        {
            var mockFileSystem = GetFileSystem();
            var fakeClient = new AoCFakeClient(2015);

            var testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            var engine = new Automaton(2015, fakeClient, mockFileSystem);
            var algo = new FakeSolver(10, 1, 2,
                x =>
                {
                    x.RegisterTestDataAndResult("random data", 2, 1);
                    x.AskVisualConfirm(1);
                    x.RegisterTestDataAndResult(testInputData, 1, 1);
                });
            using var console = new CaptureConsole();
            engine.RunDay(() => algo);
            // it should request the first answer two times
            Check.That(algo.GetAnswer1Calls).IsEqualTo(2);
            // and not the second
            Check.That(algo.GetAnswer2Calls).IsEqualTo(0);
            // it should say that no answer was provided
            Check.That(console.Output).Contains("* Test question 1 *");
            Check.That(console.Output).Not.Contains("Question 1");
            // it should have received the provided input data
            Check.That(algo.InputData).IsEqualTo(testInputData);
        }

        [Test]
        public void HandleSharedDataForTwoQuestions()
        {
            var mockFileSystem = GetFileSystem();
            var fakeClient = new AoCFakeClient(2015);

            const string testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.GoodAnswerFile);
            fakeClient.SetAnswerResponseFilename(2, TestHelpers.WrongAnswerFile);
            var engine = new Automaton(2015, fakeClient, mockFileSystem);
            var algo = new FakeSolver(10, 1, 2, x =>
            {
                x.RegisterTestData(testInputData);
                x.RegisterTestResult(1);
                x.RegisterTestResult(1, 2);
            });
            using var console = new CaptureConsole();
            engine.RunDay(() => algo);
            // it should request the first answer once, for the test
            Check.That(algo.GetAnswer1Calls).IsEqualTo(2);
            // and not the second
            Check.That(algo.GetAnswer2Calls).IsEqualTo(1);
            // it should say that no answer was provided
            Check.That(console.Output).Contains("* Test question 2 *");
            Check.That(console.Output).Contains("Test failed: got 2 instead of 1 using:");
            // it should have received the provided input data
            Check.That(algo.InputData).IsEqualTo(testInputData);
        }

        [Test]
        public void DoNotTestFirstAnswerWhenNotNeeded()
        {
            var mockFileSystem = GetFileSystem();
            var fakeClient = new AoCFakeClient(2015);

            const string testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.GoodAnswerFile);
            fakeClient.SetAnswerResponseFilename(2, TestHelpers.WrongAnswerFile);
            var engine = new Automaton(2015, fakeClient, mockFileSystem);
            var algo = new FakeSolver(10, 1, 2, x =>
            {
                x.RegisterTestDataAndResult("random data", 1, 2);
                x.RegisterTestDataAndResult(testInputData, 1, 1);

            });
            using var console = new CaptureConsole();
            engine.RunDay(() => algo);
            // it should request the first answer three times: two tests + actual data
            Check.That(algo.GetAnswer1Calls).IsEqualTo(3);
            // Second test fails
            Check.That(algo.GetAnswer2Calls).IsEqualTo(2);

            Check.That(console.Output).Contains("* Test question 2 *");
            Check.That(console.Output).Contains("Test failed: got 2 instead of 1 using:");
            // it should have received the provided input data
            Check.That(algo.InputData).IsEqualTo(testInputData);
        }

        [Test]
        public void AskVisualConfirmationWhenRequested()
        {
            var mockFileSystem = GetFileSystem();
            var fakeClient = new AoCFakeClient(2015);

            const string testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.GoodAnswerFile);
            fakeClient.SetAnswerResponseFilename(2, TestHelpers.WrongAnswerFile);
            var engine = new Automaton(2015, fakeClient, mockFileSystem);
            var algo = new FakeSolver(10, 1, 2, x =>
            {
                x.RegisterTestDataAndResult("random data", 2, 2);
                x.RegisterTestDataAndResult(testInputData, 1, 1);
                x.AskVisualConfirm(2);

            });
            using var console = new CaptureConsole();
            engine.RunDay(() => algo);
            // it should request the first answer three times: two tests + actual data
            Check.That(algo.GetAnswer1Calls).IsEqualTo(3);
            // and only two times for second test (as the second test is not confirmed manually)
            Check.That(algo.GetAnswer2Calls).IsEqualTo(2);
            Check.That(console.Output).Contains("* Test question 2 *");
            Check.That(console.Output)
                .Contains("Test failed: got a result but no expected answer provided. Please confirm result manually");
            // it should have received the provided input data
            Check.That(algo.InputData).IsEqualTo(testInputData);
        }

        [Test]
        public void ShouldStoreState()
        {
            var mockFileSystem = GetFileSystem();
            var fakeClient = new AoCFakeClient(2015);

            const string testInputData = "Silly input data"; 
            fakeClient.SetInputData(testInputData);
            fakeClient.SetAnswerResponseFilename(1, TestHelpers.GoodAnswerFile);
            fakeClient.SetAnswerResponseFilename(2, TestHelpers.WrongAnswerFile);
            var engine = new Automaton(2015, fakeClient, mockFileSystem);
            var algo = new FakeSolver(10, 1, 2, x =>
            {
                x.RegisterTestDataAndResult(testInputData, 1, 1);
            });
            using var console = new CaptureConsole();
            engine.RunDay(() => algo);
            Check.That(mockFileSystem.FileExists("AoC-10-2015-state.json")).IsTrue();
        }
    }
}