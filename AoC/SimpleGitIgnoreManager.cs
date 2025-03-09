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
using System.IO.Abstractions;
using System.Linq;

namespace AoC;

public class SimpleGitIgnoreManager(IFileSystem fileSystem = null)
{
    private readonly IFileSystem _fileSystem = fileSystem ?? new FileSystem();

    public bool AddFilter(string filter, string gitIgnorePath = "")
    {
        var filePath = _fileSystem.Path.Combine(gitIgnorePath, ".gitignore");
        string current;
        if (_fileSystem.File.Exists(filePath))
        {
            try
            {
                current = _fileSystem.File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failed to read gitignore file at {0}, exception {1}", filePath, e);
                return false;
            }
        }
        else
        {
            current = "";
        }
        
        var lines = current.Split('\n').ToList();
        // cleanup any carriage return
        for (var i = 0; i < lines.Count; i++)
        {
            lines[i] = lines[i].Trim('\r');
            if (lines[i].StartsWith(filter))
            {
                // filter already present
                return true;
            }
        }
        // we need to add the filter
        lines.Add(string.Empty);
        lines.Add("# Advent of code automation rules");
        lines.Add(filter);
        try
        {
            _fileSystem.File.WriteAllLines(filePath, lines);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Failed to write gitignore file at {0}, exception {1}", filePath, e);
            return false;
        }
        return true;
    }
}