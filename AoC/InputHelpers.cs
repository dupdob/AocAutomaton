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

namespace AoC;

public static class InputHelpers
{
    public static string[] SplitLines(this string input)
    {
        var lines = input.Split('\n');
        // we discard the last line if it is empty (trailing newline), but we keep any intermediate newlines
        return lines[^1].Length == 0 ? lines[..^1] : lines;
    }

    public static List<string[]> SplitLineBlocks(this string input)
    {
        var blockStart = 0;
        List<string[]> result = [];
        var splittedLines = SplitLines(input);
        for (var index = 0; index < splittedLines.Length; index++)
        {
            var line = splittedLines[index];
            if (!string.IsNullOrEmpty(line)) continue;
            result.Add(splittedLines[blockStart..index]);
            blockStart = ++index;
        }
        // add the last block
        result.Add(splittedLines[blockStart..]);
        return result;
    }
}