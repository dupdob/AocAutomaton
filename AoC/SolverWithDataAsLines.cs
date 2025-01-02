// MIT License
// 
//  AocAutomaton
// 
//  Copyright (c) 2024 Cyrille DUPUYDAUBY
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

public abstract class SolverWithDataAsLines : SolverWithParser
{
    protected override void Parse(string data)
    {
        var lines = data.Split('\n');
        // we discard the last line if it is empty (trailing newline), but we keep any intermediate newlines
        if (lines[^1].Length == 0)
        {
            lines = lines[..^1];
        }
        
        ParseLines(lines);
    }

    protected abstract void ParseLines(string[] lines);
}