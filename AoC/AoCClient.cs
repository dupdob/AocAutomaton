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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AoC
{
    public sealed class AoCClient : AoCClientBase
    {
        private const string EnvVarName = "AOC_SESSION";
        private readonly HttpClient _client;
        private readonly HttpClientHandler _handler;
        private readonly string _url;

        public AoCClient(int year) : base(year)
        {
            var sessionId = Environment.GetEnvironmentVariable(EnvVarName);
            if (string.IsNullOrEmpty(sessionId))
                throw new InvalidOperationException(GetSetupDocumentation());

            _url = $"https://adventofcode.com/{year}/day/";
            _handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
            _client = new HttpClient(_handler);
            // add our identifier to the request
            _handler.CookieContainer.Add(new Cookie("session",
                sessionId, "/", ".adventofcode.com"));
            _client.DefaultRequestHeaders.UserAgent.Add( new ProductInfoHeaderValue("AocAutomaton", this.GetType().Assembly.GetName().Version?.ToString()??"1.0"));
            _client.DefaultRequestHeaders.UserAgent.Add( new ProductInfoHeaderValue(@"(https://github.com/dupdob/AocAutomaton)"));
        }

        public override Task<string> RequestPersonalInput()
        {
            return _client.GetStringAsync($"{_url}{Day}/input");
        }

        public override Task<string> PostAnswer(int question, string value)
        {
            var url = $"{_url}{Day}/answer";
            var data = new Dictionary<string, string>
            {
                ["answer"] = value,
                ["level"] = question.ToString()
            };

            return _client.PostAsync(url, new FormUrlEncodedContent(data)).Result.Content.ReadAsStringAsync();
        }

        public override string GetSetupDocumentation() =>
            @$"Define an environment variable named {EnvVarName} which value is the Advent of Code session id.
The session id is stored in a cookie, named 'session', valid for '.adventofcode.com'.
To get a valid value, you must log through AoC site first.";

        public override void Dispose()
        {
            _client.Dispose();
            _handler.Dispose();
        }
    }
}