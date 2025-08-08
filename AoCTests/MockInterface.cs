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

namespace AoC.AoCTests;

internal class MockInterface(string data) : IInteract
{
    public AnswerStatus Status1 { get; set; } = AnswerStatus.Good;
    public AnswerStatus Status2 { get; set; } = AnswerStatus.Good;

    public void InitializeDay(int year, int day, string rootpath, string dataPath)
    { }

    public void CleanUpDay()
    { }

    public AnswerStatus SubmitAnswer(int id, string answer)
    {
        return id == 1 ? Status1 : Status2;
    }

    public string GetPersonalInput() => data;
    
    public string GetInteractiveInput()
    {
        throw new NotImplementedException();
    }

    public void Trace(string message)
    {
        Console.WriteLine(message);
    }

    public void ReportError(string message)
    {
        Console.Error.WriteLine(message);
    }
}