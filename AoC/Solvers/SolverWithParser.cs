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

using System;

namespace AoC
{
    public abstract class SolverWithParser : ISolver
    {
        private string _data;

        protected bool IsTest { get; private set; }

        protected object[] ExtraParameters { get; private set; } = [];
        
        private string Data
        {
            set
            {
                if (value[^1] == '\n')
                {
                    value = value.Remove(value.Length - 1);
                }
                if (value == _data) return;
                _data = value;
                Parse(value);
            }
        }

        public void InitRun(bool isTest, params object[] extraParameters)
        {
            IsTest = isTest;
            ExtraParameters = extraParameters ?? ExtraParameters;
        }

        public virtual void SetupRun(DayAutomaton dayAutomaton){}

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

        protected int GetParameter(int index, int defaultValue) => ExtraParameters.Length > index ? ((int) ExtraParameters[index]) : defaultValue;

        protected object[] GetParameters(object[] defaultValues)
        {
            if (ExtraParameters.Length == 0)
            {
                return defaultValues;
            }
            if (ExtraParameters.Length == defaultValues.Length)
            {
                return ExtraParameters;
            }
            // we need to complete with default values
            var result = new object[defaultValues.Length];
            Array.Copy(ExtraParameters, result, ExtraParameters.Length);
            for(var i = ExtraParameters.Length; i < defaultValues.Length; i++)
            {
                result[i] = defaultValues[i];
            }

            return result;
        }
        
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