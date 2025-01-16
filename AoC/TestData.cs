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

namespace AoC;

public class TestData
{
    public TestData(string data)
    {
        Data = data;
    }
    
    public string Data { get; }
    
    public object[] Answers { get; } = new object[2];

    public int[] ExtraParameters { get; private set; } = [];

    public bool[] VisualConfirm { get; } = new bool[2];
    
    public bool CanTest(int id) => Answers[id - 1] != null || VisualConfirm[id-1];

    public TestData WithParameters(params int[] parameters)
    {
        ExtraParameters = parameters;
        return this;
    }

    public TestData Answer1(object answer)
    {
        Answers[0] = answer;
        return this;
    }
    
    public TestData Answer2(object answer)
    {
        Answers[1] = answer;
        return this;
    }

    public TestData SetVisualConfirm(int question)
    {
        VisualConfirm[question-1] = true;
        return this;
    }
}