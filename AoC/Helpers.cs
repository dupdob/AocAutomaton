// MIT License
// 
//  AdventOfCode
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
// FITNESS FOR A PARTICULAR PURPOSE AND NON INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Linq;

namespace AoC;

internal static class Helpers
{
    private static bool IsAcceptableInAFileName(string name)
    {
        return name.Length <= 20 && name.All(character =>
            char.IsDigit(character) || char.IsLetter(character) || "-_#*".Contains(character));
    }

    public static string AsAFileName(string value)
    {
        return IsAcceptableInAFileName(value) ? value : $"Hash {ComputeHash(value)}";
    }
    
    /// <summary>
    /// Compute a stable hash for a string using a computation similar to the default one.
    /// </summary>
    /// <param name="value">string we want the hash of</param>
    /// <returns>the hash value, constant across sessions.</returns>
    /// <remarks>
    /// This custom string hash function ensures hash are stables across runs as
    /// the automaton uses them for cache filenames.
    /// </remarks>
    private static int ComputeHash(string value)
    {
        var hash1 = 5381;
        var hash2 = hash1;
        for (var i = 0; i < value.Length; )
        {
            int c = value[i++];
            hash1 = ((hash1 << 5) + hash1) ^ c;
            if (i == value.Length)
            {
                break;
            }
            c = value[i++];
            hash2 = ((hash2 << 5) + hash2) ^ c;
            value += 2;
        }
        return hash1 + (hash2 * 1566083941);
    }
}