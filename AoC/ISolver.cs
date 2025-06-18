// MIT License
// 
//  AocAutomaton
// 
//  Copyright (c) 2022 Cyrille DUPUYDAUBY
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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace AoC
{
    public interface ISolver
    {
        /// <summary>
        /// This method is called once by the automation logic.
        /// It is supposed to provide exercise specific information, such as:
        /// - day
        /// - examples and associated expected results
        /// - any extra parameters (sometimes used for more advanced problems)
        /// </summary>
        /// <param name="dayAutomaton"></param>
        void SetupRun(DayAutomaton dayAutomaton);

        /// <summary>
        /// This method is called by orchestration to pass any extra parameters.
        /// </summary>
        /// <param name="isTest">set to true if this is an example</param>
        /// <param name="extraText">supplemental text</param>
        /// <param name="extraParameters">any extra parameters provided along test data.</param>
        void InitRun(bool isTest, params object[] extraParameters);
        
        /// <summary>
        /// This method is called to get the answer to the first part of the problem
        /// </summary>
        /// <param name="data">input data provided by AoC</param>
        /// <returns>the computed answer, often an int or a long, can also be a string</returns>
        object GetAnswer1(string data);
        
        /// <summary>
        /// This method is called to get the answer to second first part of the problem
        /// </summary>
        /// <param name="data">input data provided by AoC</param>
        /// <returns>the computed answer, often an int or a long, can also be a string</returns>
        object GetAnswer2(string data);
    }
}