// MIT License
// 
//  AocAutomaton
// 
//  Copyright (c) 2025 Cyrille DUPUYDAUBY
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
using System.IO.Abstractions.TestingHelpers;
using NFluent;
using NFluent.Mocks;
using NUnit.Framework;

namespace AoC.AoCTests;

[TestFixture]
public class ConsoleAutomatonShould
{
    [Test]
    public void AskForUserInput()
    {
        var sut = new Automaton(fileSystem: new MockFileSystem());
        var solver = new AutoFakeSolverWithParam();
        using var console = new CaptureConsole();
        console.InputLine("testData");
        console.InputLine("");
        console.InputLine("");
        sut.RunDay(() => solver);

        Check.That(solver.Data).IsEqualTo("testData");
    }

    [Test]
    public void AskForConfirmationPerQuestion()
    {
        var sut = new Automaton(fileSystem: new MockFileSystem());
        var solver = new AutoFakeSolverWithParam();
        using var console = new CaptureConsole();
        console.InputLine("testData");
        console.InputLine("");
        console.InputLine("");
        console.InputLine("y");
        console.InputLine("y");
        var result = sut.RunDay(() => solver);
        Check.That(result).IsTrue();
    }    
    
    [Test]
    public void ReportFailureIfNoValidation()
    {
        var sut = new Automaton(fileSystem: new MockFileSystem());
        var solver = new AutoFakeSolverWithParam();
        using var console = new CaptureConsole();
        console.InputLine("testData");
        console.InputLine("");
        console.InputLine("");
        console.InputLine("y");
        console.InputLine("n");
        var result = sut.RunDay(() => solver);
        Check.That(result).IsFalse();
    }

    [Test]
    public void ReportFailureIfDayNotSet()
    {
        var sut = new Automaton(fileSystem: new MockFileSystem(), nowFunction: () => new DateTime(2025,3,4));
        var solver = new AutoFakeSolverWithParam
        {
            Day = 0
        };
        using var console = new CaptureConsole();
        console.InputLine("testData");
        console.InputLine("");
        console.InputLine("");
        var result = sut.RunDay(() => solver);
        Check.That(result).IsFalse();
        // NFluent captured console does not track error output.
        Check.That(sut.Day).IsEqualTo(0);
    }

    [Test] public void DefaultToCurrentDayInDecember()
    {
        var sut = new Automaton(fileSystem: new MockFileSystem(), nowFunction: () => new DateTime(2024,12,4));
        var solver = new AutoFakeSolverWithParam
        {
            Day = 0
        };
        using var console = new CaptureConsole();
        console.InputLine("testData");
        console.InputLine("");
        console.InputLine("");
        sut.RunDay(() => solver);
        Check.That(sut.Day).IsEqualTo(4);
    }
    
}