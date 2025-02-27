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

using System.Collections.Generic;

namespace AoC;

/// <summary>
/// Solver parsing input data as a series of block of lines separated by empty lines.
/// </summary>
public abstract class SolverWithBlockParser : SolverWithDataAsLines
{
    protected abstract void ParseBlock(List<string> block, int blockIndex);
    
    protected override void ParseLines(string[] lines)
    {
        var blockIndex = 0;
        var nextBlock = new List<string>();
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line))
            {
                ParseBlock(nextBlock, blockIndex++);
                nextBlock = [];
            }
            else
            {
                nextBlock.Add(line);
            }
        }
        ParseBlock(nextBlock, blockIndex);
    }
}