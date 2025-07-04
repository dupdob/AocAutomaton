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

public abstract class SolverWithParams<T, TU> : SolverWithParser
{
    private readonly T _defaultParamForPart1;
    private readonly T _defaultParamForPart2;

    protected SolverWithParams()
    {
        // getdefault values for parameters
        var info = GetType().GetMethod(nameof(GetAnswer1), BindingFlags.Instance|BindingFlags.NonPublic, [typeof(T), typeof(TU)]);
        _defaultParamForPart1 = (T) GetDefaultValueForParam(info, 0) ?? default(T);

        info = GetType().GetMethod(nameof(GetAnswer2), BindingFlags.Instance|BindingFlags.NonPublic, [typeof(T), typeof(TU)]);
        _defaultParamForPart2 = (T) GetDefaultValueForParam(info, 0) ?? default(T);
    }
    

    private static object GetDefaultValueForParam(MethodInfo info, int index)
    {
        if (info== null || info.GetParameters().Length == 0)
        {
            return null;
        }
        
        var parameterInfo = info.GetParameters()[index];
        if (!parameterInfo.HasDefaultValue || parameterInfo.ParameterType != typeof(T))
        {
            return null;
        }

        return (T) parameterInfo.DefaultValue!;
    }

    private T GetParameter(T defaultValue)
    {
        if (ExtraParameters.Length > 0 && ExtraParameters[0] is T param)
        {
            return param;
        }
        return defaultValue;
    }
    
    public override object GetAnswer1() => GetAnswer1(GetParameter(_defaultParamForPart1));

    protected abstract object GetAnswer1(T extraParameter);

    public override object GetAnswer2() => GetAnswer2(GetParameter(_defaultParamForPart2));

    protected abstract object GetAnswer2(T extraParameter);
}


