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

using System.Linq;
using System.Reflection;

namespace AoC;

public abstract class SolverWithIntParameters : SolverWithParser
{
    private readonly int[] _defaultParamForPart1;
    private readonly int[] _defaultParamForPart2;

    protected SolverWithIntParameters()
    {
        // getdefault values for parameters
        var info = GetType().GetMethods(BindingFlags.Instance|BindingFlags.NonPublic);
        foreach (var methodInfo in info.Where(m => m.GetParameters().Length>0))
        {
            var isFirst = methodInfo.Name == nameof(GetAnswer1);
            if (!isFirst && methodInfo.Name != nameof(GetAnswer2))
            {
                continue;
            }
            var parameters = methodInfo.GetParameters();
            var defValues = new int[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].HasDefaultValue && parameters[i].ParameterType == typeof(int))
                {
                    defValues[i] = (int) parameters[i].DefaultValue!;
                }
            }

            if (isFirst)
            {
                _defaultParamForPart1 = defValues;
            }
            else
            {
                _defaultParamForPart2 = defValues;
            }
        }
    }

    public override object GetAnswer1() => GetAnswer1(GetParameters(_defaultParamForPart1));

    protected abstract object GetAnswer1(int[] extraParameter);

    public override object GetAnswer2() => GetAnswer2(GetParameters(_defaultParamForPart2));

    protected abstract object GetAnswer2(int[] extraParameter);
}