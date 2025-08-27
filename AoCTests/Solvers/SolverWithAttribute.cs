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

using System.Collections.Generic;

namespace AoC.AoCTests;

[Day(13)]
public class SolverWithAttribute : ISolver
{
    public void SetupRun(DayAutomaton automaton)
    {
    }

    public void InitRun(bool isTest, params object[] extraParameters)
    {
    }
 
    [Example("test", 2)]
    public virtual object GetAnswer1(string data) => null;

    [UnitTest("result", 1)]
    public static string SomeMethod(int param) => "result";

    [UnitTest("other", 2)]
    public string SomeOtherMethod(int param) => "result";

    [Example("test", "response")]
    [Example("OtherTest", 3)]
    public object GetAnswer2(string data) => null;
}

[Day(13)]
internal class SolverWithSkippableAttribute : ISolver
{
    // number of times methods are called
    public static Dictionary<string, int> PartOneOccurrences { get; } = new();
    public static Dictionary<string, int> PartTwoOccurrences { get; } = new();

    public static void ResetStatistics()
    {
        PartOneOccurrences.Clear();
        PartTwoOccurrences.Clear();
    }
    
    public void SetupRun(DayAutomaton dayAutomaton)
    {
    }

    public void InitRun(bool isTest, params object[] extraParameters)
    {
    }

    [Skippable]
    [Example("test", 2)]
    public object GetAnswer1(string data)
    {
        PartOneOccurrences[data] = PartOneOccurrences.GetValueOrDefault(data) + 1;
        return 2;
    }

    [Example("OtherTest", 3)]
    public object GetAnswer2(string data)
    {
        PartTwoOccurrences[data] = PartTwoOccurrences.GetValueOrDefault(data) + 1;
        return 3;
    }
}