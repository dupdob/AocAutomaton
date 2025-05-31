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

namespace AoC;

public class TestData
{
    private readonly DayAutomaton _dayAutomaton;

    public TestData(string data, DayAutomaton dayAutomaton)
    {
        _dayAutomaton = dayAutomaton;
        Data = data;
    }
    
    public string Data { get; }
    
    public object[] Answers { get; } = new object[2];

    public int[] ExtraParameters { get; private set; } = null;
    
    public string Extra { get; private set; }

    public bool[] VisualConfirm { get; } = new bool[2];
    
    public bool CanTest(int id) => Answers[id - 1] != null || VisualConfirm[id-1];

    public bool CanTest() => CanTest(1) || CanTest(2);

    public TestData WithParameters(params int[] parameters) => WithParameters(string.Empty, parameters);

    public TestData WithParameters(string text, params int[] parameters)
    {
        if (ExtraParameters?.Length>0 || !string.IsNullOrEmpty(Extra))
        {
            if (!CanTest())
            {
                throw new InvalidOperationException("Must specify an expected result before declaring a new test case");
            }

            return _dayAutomaton.AddExample(this.Data).WithParameters(text, parameters);
        }
        ExtraParameters = parameters;
        Extra = text;
        return this;
    }

    public TestData Answer1(object answer) => RegisterAnswer(0, answer);

    public TestData Answer2(object answer) => RegisterAnswer(1, answer);

    public TestData RegisterAnswer(int index, object answer)
    {
        if (answer == null)
        {
            VisualConfirm[index] = true;
        }

        Answers[index] = answer;
        return this;
    }


    public TestData SetVisualConfirm(int question)
    {
        VisualConfirm[question-1] = true;
        return this;
    }
}