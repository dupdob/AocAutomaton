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
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NFluent;
using NFluent.Mocks;
using NUnit.Framework;

namespace AoC.AoCTests;

internal class MockInterface : IInteract
{
    private readonly string _data;

    public AnswerStatus Status1 { get; set; } = AnswerStatus.Good;
    public AnswerStatus Status2 { get; set; } = AnswerStatus.Good;

    public MockInterface(string data)
    {
        _data = data;
    }

    public void InitializeDay(int year, int day, string dataPath)
    { }

    public void CleanUpDay()
    { }

    public AnswerStatus SubmitAnswer(int id, string answer)
    {
        return id == 1 ? Status1 : Status2;
    }

    public string GetPersonalInput() => _data;

    public void Trace(string message)
    {
        Console.WriteLine(message);
    }

    public void ReportError(string message)
    {
        Console.Error.WriteLine(message);
    }
}
    
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
        var testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
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
    public void StopWhenAlgoProvidesNoAnswer()
    {
        var mockFileSystem = GetFileSystem();
        var testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
        var engine = new Automaton(2015, fakeClient, mockFileSystem);
        var algo = new FakeSolver(10, null, 2, x => x.RegisterTestDataAndResult(testInputData, 2, 1));
        using var console = new CaptureConsole();
        engine.RunDay(() => algo);
        // it should request the first answer once, for the test
        Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
        // and not the second
        Check.That(algo.GetAnswer2Calls).IsEqualTo(0);
        // it should say that no answer was provided
        Check.That(console.Output).Contains("* Test question 1 *");
        Check.That(console.Output).Contains("Test failed: got no answer instead of 2 using:");
        // it should have received the provided input data
        Check.That(algo.InputData).IsEqualTo(testInputData);
    }

    [Test]
    public void KeepOnWhenFirstTestSucceeds()
    {
        var mockFileSystem = GetFileSystem();
        var testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
        fakeClient.Status2 = AnswerStatus.Wrong;
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
    public void ShouldRaiseErrorWhenTestBadlyDeclared()
    {
        var mockFileSystem = GetFileSystem();
        var testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
        fakeClient.Status2 = AnswerStatus.Wrong;
        var engine = new Automaton(2015, fakeClient, mockFileSystem); 
        var solver = new FakeSolver(10, 1, 2,
            x => x.RegisterTestResult(1,2));
        using var console = new CaptureConsole();
        Check.ThatCode(() => engine.RunDay(() => solver)).Throws<ApplicationException>();
    }

    [Test]
    public void UseAllTestDataAndStopsIfOneIsWrong()
    {
        var mockFileSystem = GetFileSystem();
        var testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
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
        var testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
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
    public void SupportDefaultExtraParametersWithText()
    {
        var mockFileSystem = GetFileSystem();
        var testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
        var engine = new Automaton(2015, fakeClient, mockFileSystem);
        engine.SetDefault("extra", 1, 2);
        using var console = new CaptureConsole();
        var list = new List<AutoFakeSolverWithParam>();
        engine.AddExample("Test").Answer1(1);
        engine.RunDay(() =>
        {
            var result = new AutoFakeSolverWithParam();
            list.Add(result);
            return result;
        });
            
        foreach (var solverWithParam in list.Where(solverWithParam => solverWithParam.Data != null))
        {
            Check.That(solverWithParam.GetExtra()).IsEqualTo("extra");
            Check.That(solverWithParam.GetExtraParameters()).IsEqualTo(new []{1,2});
        }
    }
        
    [Test]
    public void SupportDefaultExtraParameters()
    {
        var mockFileSystem = GetFileSystem();
        var testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
        var engine = new Automaton(2015, fakeClient, mockFileSystem);
        engine.SetDefault(1, 2);
        using var console = new CaptureConsole();
        var list = new List<AutoFakeSolverWithParam>();
        engine.AddExample("Test").Answer1(1);
        engine.RunDay(() =>
        {
            var result = new AutoFakeSolverWithParam();
            list.Add(result);
            return result;
        });
            
        foreach (var solverWithParam in list.Where(solverWithParam => solverWithParam.Data != null))
        {
            Check.That(solverWithParam.GetExtra()).IsNull();
            Check.That(solverWithParam.GetExtraParameters()).IsEqualTo(new []{1,2});
        }
    }
    
    [Test]
    public void SupportVariableExtraParameters()
    {
        var mockFileSystem = GetFileSystem();
        var testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
        var engine = new Automaton(2015, fakeClient, mockFileSystem);
        engine.SetDefault(testInputData, 1, 2);
        using var console = new CaptureConsole();
        var list = new List<AutoFakeSolverWithParam>();
        engine.AddExample("Test").WithParameters("Test").Answer1(1);
        engine.RunDay(() =>
        {
            var result = new AutoFakeSolverWithParam();
            list.Add(result);
            return result;
        });
            
        foreach (var solverWithParam in list.Where(solverWithParam => solverWithParam.Data != null))
        {
            // the text parameter should be equal to data
            Check.That(solverWithParam.GetExtra()).IsEqualTo(solverWithParam.Data);
            // but numerical parameters should be equal to degault
            Check.That(solverWithParam.GetExtraParameters()).IsEqualTo(new []{1,2});
        }
    }
        
    [Test]
    public void DoNotTestFirstAnswerWhenNotNeeded()
    {
        var mockFileSystem = GetFileSystem();
        var testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
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
        Check.That(algo.GetAnswer2Calls).IsEqualTo(1);

        Check.That(console.Output).Contains("* Test question 2 *");
        Check.That(console.Output).Contains("Test failed: got 2 instead of 1 using:");
        // it should have received the provided input data
        Check.That(algo.InputData).IsEqualTo(testInputData);
    }

    [Test]
    public void AskVisualConfirmationWhenRequested()
    {
        var mockFileSystem = GetFileSystem();
        var testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
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
            .Contains("but no expected answer provided. Please confirm result manually (y/n). Result below.");
        // it should have received the provided input data
        Check.That(algo.InputData).IsEqualTo(testInputData);
    }

    [Test]
    public void ContinueAfterVisualConfirmation()
    {
        var mockFileSystem = GetFileSystem();
        var testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
        var engine = new Automaton(2015, fakeClient, mockFileSystem);
        var algo = new FakeSolver(10, 1, 2, x =>
        {
            x.RegisterTestDataAndResult("random data", 2, 2);
            x.AskVisualConfirm(2);
        });

        using var console = new CaptureConsole();
        console.InputLine("y");
        engine.RunDay(() => algo);
        // we have two calls to question as we confirmed the answer
        Check.That(algo.GetAnswer2Calls).IsEqualTo(2);
        Check.That(console.Output).Contains("* Test question 2 *");
        Check.That(console.Output)
            .Contains("but no expected answer provided. Please confirm result manually (y/n). Result below.");
        // it should have received the provided input data
        Check.That(algo.InputData).IsEqualTo(testInputData);
    }

[Test]
public void AskVisualConfirmationWhenNoExpectedValueProvided()
    {
        var mockFileSystem = GetFileSystem();
        var testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
        var engine = new Automaton(2015, fakeClient, mockFileSystem);
        var algo = new FakeSolver(10, 1, 2, x =>
        {
            x.RegisterTestDataAndResult(testInputData, 1, 1);
        });
        using var console = new CaptureConsole();
        engine.RunDay(() => algo);
        // it should request the first answer three times: two tests + actual data
        Check.That(algo.GetAnswer1Calls).IsEqualTo(2);
        // and only two times for second test (as the second test is not confirmed manually)
        Check.That(console.Output)
            .Contains("but no expected answer provided. Please confirm result manually (y/n). Result below.");
        // it should have received the provided input data
        Check.That(algo.InputData).IsEqualTo(testInputData);
    }

    [Test]
    public void ShouldStoreState()
    {
        var mockFileSystem = GetFileSystem();
        const string testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
        var engine = new Automaton(2015, fakeClient, mockFileSystem);
        var algo = new FakeSolver(10, 1, 2, x =>
        {
            x.RegisterTestDataAndResult(testInputData, 1, 1);
            x.RegisterTestDataAndResult(testInputData, 2, 2);
        });
        using var console = new CaptureConsole();
        engine.RunDay(() => algo);
        var fileName = "AoC-2015-10-state.json";
        Check.That(mockFileSystem.FileExists(fileName)).IsTrue();
        var state = DayState.FromJson(mockFileSystem.File.ReadAllText(fileName));

        Check.That(state.Day).IsEqualTo(10);
        Check.That(state.First).HasFieldsWithSameValues(new { Solved = true });
        Check.That(state.Second).HasFieldsWithSameValues(new { Solved = true });
    }
        
    [Test]
    public void ShouldSkipIfAlreadyDone()
    {
        var mockFileSystem = GetFileSystem();
        var test = new DayState
        {
            Day = 10,
            First =
            {
                Answer = "goodAnswer",
                Attempts = ["goodAnswer"],
                Solved = true
            },
            Second = 
            {
                Answer = "goodAnswer",
                Attempts = ["goodAnswer"],
                Solved = true
            }
        };
            
        mockFileSystem.File.WriteAllText("AoC-2015-10-state.json", test.ToJson());
        var testInputData = "Silly input data";
        var fakeClient = new MockInterface(testInputData);
        var engine = new Automaton(2015, fakeClient, mockFileSystem);

        var algo = new FakeSolver(10, 1, 2, x =>
        {
            x.RegisterTestDataAndResult(testInputData, 1, 1);
        });
        using var console = new CaptureConsole();
        engine.RunDay(() => algo);
        // no call should happen
        Check.That(algo.GetAnswer1Calls).IsEqualTo(0);
    }
}