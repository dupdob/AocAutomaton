// /* MIT License
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
// */

namespace AoC.AoCTests;

internal class FakeSolver : ISolver
{
    private readonly object _answer1;
    private readonly object _answer2;
    private readonly int _day;

    public FakeSolver(int day, object answer1, object answer2)
    {
        _day = day;
        _answer1 = answer1;
        _answer2 = answer2;
    }

    public int GetAnswer1Calls { get; private set; }
    public int GetAnswer2Calls { get; private set; }

    public string InputData { get; private set; }

    public void SetupRun(Automaton automaton)
    {
        automaton.Day = _day;
    }

    public object GetAnswer1(string data)
    {
        InputData = data;
        GetAnswer1Calls++;
        return _answer1;
    }

    public object GetAnswer2(string data)
    {
        GetAnswer2Calls++;
        return _answer2;
    }
}