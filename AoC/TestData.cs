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

    private readonly object[][] _extraParameters = new object[2][];

    public bool[] VisualConfirm { get; } = new bool[2];
    
    public bool CanTest(int id) => Answers[id] != null || VisualConfirm[id];

    private bool CanTest() => CanTest(0) || CanTest(1);

    public TestData WithParameters(int id, params object[] parameters)
    {
        if (_extraParameters[id]?.Length>0)
        {
            if (!CanTest())
            {
                throw new InvalidOperationException("Must specify an expected result before declaring a new test case");
            }

            return _dayAutomaton.AddExample(this.Data).WithParameters(id, parameters);
        }
        _extraParameters[id] = parameters;
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
        VisualConfirm[question] = true;
        return this;
    }
    
    public object[] GetParameters(int part, object[] defaultParameters)
    {
        if (_extraParameters[part] == null || _extraParameters[part].Length == 0)
        {
            return defaultParameters;
        }

        var parameters = new object[Math.Max(defaultParameters.Length, _extraParameters[part].Length)];
        for(var i = 0; i < parameters.Length; i++)
        {
            parameters[i] = i >= _extraParameters[part].Length || (_extraParameters[part][i] == null && i< defaultParameters.Length)
                ? defaultParameters[i]
                : _extraParameters[part][i];
        }
        return parameters;
    }
}