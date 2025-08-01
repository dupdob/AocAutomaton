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

namespace AoC;

/// <summary>
/// Advent Of code response status
/// </summary>
public enum AnswerStatus
{
    /// <summary>
    /// Wrong answer
    /// </summary>
    Wrong,
    /// <summary>
    /// Good Answer
    /// </summary>
    Good,
    /// <summary>
    /// Answer is higher than solution
    /// </summary>
    TooHigh,
    /// <summary>
    /// Answer is lower than solution
    /// </summary>
    TooLow
};

/// <summary>
/// Interface for automaton UI implementations
/// </summary>
public interface IInteract
{
    /// <summary>
    /// Called by automation to initialize interface (if needed)
    /// </summary>
    /// <param name="year">exercise year (event)</param>
    /// <param name="day">specific day</param>
    /// <param name="rootPath">rootPath to store data</param>
    /// <param name="dayPath">path to cache</param>
    void InitializeDay(int year, int day, string rootPath, string dayPath);
    
    /// <summary>
    /// Called at the end of the exercise to allow for clean up.
    /// </summary>
    void CleanUpDay();
    /// <summary>
    /// Called by automation to submit an answer
    /// </summary>
    /// <param name="part">part, 1 or 2</param>
    /// <param name="answer">answer</param>
    /// <returns>the answer status</returns>
    AnswerStatus SubmitAnswer(int part, string answer);
    /// <summary>
    /// Called by automation to retrieve exercise's personal input
    /// </summary>
    /// <returns>The personal as a string.</returns>
    string GetPersonalInput();

    /// <summary>
    /// Called by automation when a manual input is required.
    /// </summary>
    /// <returns></returns>
    string GetInteractiveInput();
    
    /// <summary>
    /// Called by automation to trace events
    /// </summary>
    /// <param name="message">message to log/output</param>
    void Trace(string message);
    /// <summary>
    /// Called by automation to report a significant error.
    /// Usually ends the test process.
    /// </summary>
    /// <param name="message"></param>
    void ReportError(string message);
}