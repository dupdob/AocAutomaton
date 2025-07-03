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

namespace AoC;

public class Automaton : IAutomaton
{
    private readonly IFileSystem _fileSystem;
    private readonly Func<DateTime> _now;
    private readonly IInteract _userInterface;
    public string RootPath { get; private set; } = "./";
    public int Year { get; }
    public string DataPathNameFormat { get; private set;  } = "./";

    public Automaton(int year, IInteract userInterface = null, IFileSystem fileSystem = null, Func<DateTime> now = null)
    {
        _now = now ?? (() => DateTime.Now);
        Year = year == 0 ? _now().Year : year;
        _userInterface = userInterface ?? new ConsoleUserInterface();
        _fileSystem = fileSystem ?? new FileSystem();
    }

    public DateTime Now() => _now();

    /// <summary>
    /// Builds an automaton interacting with Advent of code website.
    /// </summary>
    /// <param name="year">Target year</param>
    /// <returns>An AoC automaton instance</returns>
    public static Automaton WebsiteAutomaton(int year =0) => new(year, new HttpInterface());

    /// <summary>
    /// Sets the path used by the engine to cache data (input and response).
    /// You can provide a format pattern string, knowing that {0} will be replaced by
    /// the exercise's day and {1} by the year.
    /// </summary>
    /// <param name="dataPath">path (or format string) used to store data.</param>
    /// <returns>This instance.</returns>
    /// <remarks>Relative paths are relative to the engine current directory.</remarks>
    public void SetDataPath(string dataPath)
    {
        // scan the path to identify the common root
        var rootPath = dataPath;
        var curPath = dataPath;
        while(!string.IsNullOrEmpty(curPath))
        {
            var thisLevel = _fileSystem.Path.GetFileName(curPath);
            curPath = _fileSystem.Path.GetDirectoryName(curPath);
            if (thisLevel.Contains('{'))
            {
                // this is a variable part, cannot be part of the root path
                rootPath = curPath;
            }
        }
        RootPath = rootPath;
        DataPathNameFormat = dataPath;
    }


    public void Trace(string message) => _userInterface.Trace(message);

    public void ReportError(string message) => _userInterface.ReportError(message);

    public static bool AskYesNo()
    {
        var assessment = Console.ReadLine()?.ToLower();
        return !string.IsNullOrEmpty(assessment) && assessment[0] == 'y';
    }

    public static string AskInput() => Console.ReadLine();
    
    /// <summary>
    ///  Runs a given day.
    /// </summary>
    /// <typeparam name="T"><see cref="ISolver" /> type for the day.</typeparam>
    /// <exception cref="InvalidOperationException">when the method fails to create an instance of the algorithm.</exception>
    /// <returns>true if problem was solved (both parts)</returns>
    public bool RunDay<T>() where T : ISolver
    {
        var automaton = new DayAutomaton( this, this._fileSystem, this._userInterface);
        return automaton.RunDay(SolverFactory.ForType<T>());
    }

    /// <summary>
    /// Runs a given day
    /// </summary>
    /// <param name="builder">delegate that must return s a solver</param>
    /// <returns>true if problem was solved (both parts)</returns>
    public bool RunDay(Func<ISolver> builder)
    {
        var automaton = new DayAutomaton( this, this._fileSystem, this._userInterface);
        return automaton.RunDay(new SolverFactory(builder));
    }
}