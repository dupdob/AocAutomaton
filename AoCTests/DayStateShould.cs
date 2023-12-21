// MIT License
// 
//  AocAutomaton
// 
//  Copyright (c) 2023 Cyrille DUPUYDAUBY
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
using NFluent;
using NUnit.Framework;

namespace AoC.AoCTests;

[TestFixture]
public class DayStateShould
{
    [Test]
    public void BeSerializable()
    {
        var test = new DayState
        {
          Day = 8,
          First =
          {
            Answer = "goodAnswer",
            Attempts = new List<string>() { "Wrong", "False", "goodAnswer" },
            Solved = true
          }
        };

        var text = test.ToJson();
        Check.That(text).IsEqualTo(@"{
  ""SchemaVersion"": ""1"",
  ""Day"": 8,
  ""First"": {
    ""Solved"": true,
    ""Answer"": ""goodAnswer"",
    ""Attempts"": [
      ""Wrong"",
      ""False"",
      ""goodAnswer""
    ],
    ""Low"": null,
    ""High"": null
  },
  ""Second"": {
    ""Solved"": false,
    ""Answer"": null,
    ""Attempts"": [],
    ""Low"": null,
    ""High"": null
  }
}");
    }
    
    [Test]
    public void BeDeSerializable()
    {
        var jSon=@"{
  ""SchemaVersion"": ""1"",
  ""Day"": 12,
  ""First"": {
    ""Solved"": true,
    ""Answer"": ""goodAnswer"",
    ""Attempts"": [
      ""goodAnswer""
    ],
    ""Low"": null,
    ""High"": null
  },
  ""Second"": {
    ""Solved"": false,
    ""Answer"": null,
    ""Attempts"": [""lame""],
    ""Low"": null,
    ""High"": null
  }
}";
        var sut = DayState.FromJson(jSon);

        Check.That(sut.Day).IsEqualTo(12);
        Check.That(sut.First.Solved).IsTrue();
        Check.That(sut.First.Answer).IsEqualTo("goodAnswer");

        Check.That(sut.First.Attempts).HasSize(1);
    }
    
}