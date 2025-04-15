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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AoC
{
    public sealed class AoCClient: AoCClientBase
    {
        private readonly HttpClient _client;
        private readonly HttpClientHandler _handler;
        private string Url =>$"https://adventofcode.com/{Year}/day/"; 

        public AoCClient()
        {

            _handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
            _client = new HttpClient(_handler);
            // add our identifier to the request

            _client.DefaultRequestHeaders.UserAgent.Add( new ProductInfoHeaderValue("AocAutomaton", this.GetType().Assembly.GetName().Version?.ToString()??"1.0"));
            _client.DefaultRequestHeaders.UserAgent.Add( new ProductInfoHeaderValue(@"(https://github.com/dupdob/AocAutomaton)"));
        }

        public override void SetSessionCookie(string value)
        {
            var cookie = _handler.CookieContainer.GetAllCookies().FirstOrDefault(c => c.Name == "session");
            if (cookie == null)
            {
                _handler.CookieContainer.Add(new Cookie("session",
                    value, "/", ".adventofcode.com"));
            }
            else
            {
                cookie.Value = value;
            }
        }
        
        public override Task<string> RequestPersonalInput()
        {
            return _client.GetStringAsync($"{Url}{Day}/input");
        }

        public override Task<string> PostAnswer(int question, string value)
        {
            var url = $"{Url}{Day}/answer";
            var data = new Dictionary<string, string>
            {
                ["answer"] = value,
                ["level"] = question.ToString()
            };

            return _client.PostAsync(url, new FormUrlEncodedContent(data)).Result.Content.ReadAsStringAsync();
        }
        
        public override void Dispose()
        {
            _client.Dispose();
            _handler.Dispose();
        }
    }
}