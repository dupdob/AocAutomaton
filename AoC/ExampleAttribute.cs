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

namespace AoC;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ExampleAttribute : Attribute
{
    public string Input { get; }
    public object Expected { get; }
    public string TextParameter { get; }
    public int[] Parameters { get; }

    public ExampleAttribute(string input)
    {
        Input = input;
        Expected = null;
    }

    public ExampleAttribute(string input, long expected)
    {
        Input = input;
        Expected = expected;
    }

    public ExampleAttribute(string input, string expected)
    {
        Input = input;
        Expected = expected;
    } 
    
    public ExampleAttribute(string input, string expected, string textParameter = null, params int[] parameters)
    {
        Input = input;
        Expected = expected;
        TextParameter = textParameter;
        Parameters = parameters;
    }
    
}