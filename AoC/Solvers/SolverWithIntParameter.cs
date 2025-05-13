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

using System.Reflection;

namespace AoC;

public abstract class SolverWithIntParameter : SolverWithParser
{
    private readonly int _defaultParamForPart1;
    private readonly int _defaultParamForPart2;

    protected SolverWithIntParameter()
    {
        // getdefault values for parameters
        var info = GetType().GetMethod(nameof(GetAnswer1), BindingFlags.Instance|BindingFlags.NonPublic, [typeof(int)]);
        _defaultParamForPart1 = GetDefaultValueForParam(info);

        info = GetType().GetMethod(nameof(GetAnswer2), BindingFlags.Instance|BindingFlags.NonPublic, [typeof(int)]);
        _defaultParamForPart2 = GetDefaultValueForParam(info);
    }

    private static int GetDefaultValueForParam(MethodInfo info)
    {
        if (info== null || info.GetParameters().Length == 0)
        {
            return 0;
        }
        
        var parameterInfo = info.GetParameters()[0];
        if (!parameterInfo.HasDefaultValue || parameterInfo.ParameterType != typeof(int))
        {
            return 0;
        }

        return (int) parameterInfo.DefaultValue!;
    }

    public override object GetAnswer1() => GetAnswer1(GetParameter(0, _defaultParamForPart1));

    protected abstract object GetAnswer1(int extraParameter);

    public override object GetAnswer2() => GetAnswer2(GetParameter(0, _defaultParamForPart2));

    protected abstract object GetAnswer2(int extraParameter);
}