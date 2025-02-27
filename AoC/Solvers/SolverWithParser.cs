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
    public abstract class SolverWithParser : ISolver
    {
        private string _data;

        protected bool IsTest { get; private set; }

        protected int[] ExtraParameters { get; private set; } = [];

        protected string Extra { get; private set; }

        private string Data
        {
            set
            {
                if (value == _data) return;
                if (value[^1] == '\n')
                {
                    value = value.Remove(value.Length - 1);
                }
                _data = value;
                Parse(value);
            }
        }

        public void InitRun(bool isTest, string extra, params int[] extraParameters)
        {
            IsTest = isTest;
            Extra = extra;
            ExtraParameters = extraParameters;
        }

        public abstract void SetupRun(Automaton automaton);

        /// <summary>
        ///     Parse the exercise data.
        /// </summary>
        /// <param name="data">data to be parsed as a single string</param>
        /// <remarks>
        ///     Split the data as an array of lines and call <see cref="SolverWithLineParser.ParseLine" /> for each them.
        ///     Override this method if you prefer to parse data as a whole (you still need to provide an empty ParseLine method
        ///     as it is declared abstract).
        /// </remarks>
        protected abstract void Parse(string data);

        protected int GetParameter(int index, int value) => ExtraParameters.Length > index ? ExtraParameters[index] : value;
        
        public object GetAnswer1(string data)
        {
            Data = data;
            return GetAnswer1();
        }

        public object GetAnswer2(string data)
        {
            Data = data;
            return GetAnswer2();
        }

        public abstract object GetAnswer1();
        
        public abstract object GetAnswer2();
    }
}