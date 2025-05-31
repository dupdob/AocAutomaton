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
using System.Collections.Generic;

namespace AoC;

/// <summary>
/// Provides an AoC automation logic using terminal console for interaction.
/// That is it will ask input data to be keyed in, report result on the console
/// and ask for confirmation.
/// </summary>
public class ConsoleUserInterface : IInteract
{
    public void InitializeDay(int year, int day, string rootPath, string dayPath)
    {
        // nothing to do        
    }

    public void CleanUpDay()
    {
        // nothing to do   
    }

    public AnswerStatus SubmitAnswer(int part, string answer)
    {
        Console.WriteLine("Answer for part {0} is :", part);
        Console.WriteLine(answer??"<null>");
        if (answer == null)
        {
            return AnswerStatus.Wrong;
        }
        Console.WriteLine("Is this answer valid ? (y/n");
        return Automaton.AskYesNo() ? AnswerStatus.Good : AnswerStatus.Wrong;
    }

    public string GetPersonalInput()
    {
        Console.WriteLine("Please provide input for the exercise (insert two empty lines to close input):");
        var lines = new List<string>();
        for(;;)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line) && string.IsNullOrEmpty(lines[^1]))
            {
                // two end of lines, we remove the last line and stop
                lines.RemoveAt(lines.Count-1);
                break;
            }
            lines.Add(line);
        }

        return string.Join(Environment.NewLine, lines);
    }

    public void Trace(string message) => Console.WriteLine(message);

    public void ReportError(string message) => Console.Error.WriteLine(message);
}