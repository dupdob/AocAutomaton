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

/// <summary>
/// Declare an example for the given day with a specific id an optional parameters.
/// </summary>
/// <param name="id">arbitrary id for this example. This id should be reused via a <see cref="ReuseExampleAttribute"/>.</param>
/// <param name="input">example input</param>
/// <param name="expected">expected value (should be int, long or string)</param>
/// <param name="parameters">numerical parameters specific to this example (eg iteration count)</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ExampleAttribute(
    int id,
    string input,
    object expected,
    params object[] parameters)
    : Attribute
{
    public int Id { get; } = id;
    public string Input { get; } = input;
    public object Expected { get; } = expected;
    public object[] Parameters { get; } = parameters;

    /// <summary>
    /// Declare an example for the given day with optional parameters.
    /// </summary>
    /// <param name="input">example input</param>
    /// <param name="expected">expected value (should be int, long or string)</param>
    /// <param name="parameters">numerical parameters specific to this example (eg iteration count)</param>
    public ExampleAttribute(string input, object expected, params object[] parameters): this(-1, input, expected, parameters){}

}