using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AoC;

public sealed class AoCClient : AoCClientBase
{
    private readonly HttpClient _client;
    private readonly HttpClientHandler _handler;
    private readonly string _url;

    public AoCClient(int year) : base(year)
    {
        var sessionId = Environment.GetEnvironmentVariable("AOC_SESSION");
        if (string.IsNullOrEmpty(sessionId))
            throw new InvalidOperationException(
                "AOC_SESSION environment variable must contain an Advent Of Code session id.");

        _url = $"https://adventofcode.com/{year}/day/";
        _handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
        _client = new HttpClient(_handler);
        // add our identifier to the request
        _handler.CookieContainer.Add(new Cookie("session",
            sessionId, "/", ".adventofcode.com"));
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

    public override void Dispose()
    {
        _client.Dispose();
        _handler.Dispose();
    }
}