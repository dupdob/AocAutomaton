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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AoC.AoCTests;

public sealed class AoCFakeClient : AoCClientBase
{
    private readonly Dictionary<int, string> _responseFile = new();
    private string _inputData;

    public AoCFakeClient(int year) : base(year)
    {
    }

    public int NbRequest { get; private set; }

    public void SetInputData(string data)
    {
        _inputData = data;
    }

    public void SetAnswerResponseFilename(int id, string fileName)
    {
        _responseFile[id] = fileName;
    }

    public override Task<string> RequestPersonalInput()
    {
        NbRequest++;
        return Task.FromResult(_inputData);
    }

    public override Task<string> PostAnswer(int id, string value)
    {
        return File.ReadAllTextAsync(_responseFile[id]);
    }

    public override void Dispose()
    {
    }
}