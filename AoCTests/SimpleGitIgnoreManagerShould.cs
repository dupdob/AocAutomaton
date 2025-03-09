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

using System;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NFluent;
using NUnit.Framework;

namespace AoC.AoCTests;

[TestFixture]
public class SimpleGitIgnoreManagerShould
{
    [Test]
    public void CreateAGitignoreFileWhenMissing()
    {
        var mockFS = new MockFileSystem();
        var sut = new SimpleGitIgnoreManager(mockFS);
        Check.That(sut.AddFilter("myfilter.*")).IsTrue();
        Check.That(mockFS.AllFiles.Where( f => f.Contains(".gitignore"))).CountIs(1);
    }
    
    [Test]
    public void CompleteExistingGitignoreFile()
    {
        var mockFS = new MockFileSystem();
        var sut = new SimpleGitIgnoreManager(mockFS);
        var initialContent = "# custom"+Environment.NewLine+"*.obj"+Environment.NewLine;
        var gitignore = ".gitignore";
        mockFS.AddFile(gitignore, new MockFileData(initialContent));
        var arbitraryFilter = "myfilter.*";
        Check.That(sut.AddFilter(arbitraryFilter)).IsTrue();
        var updatedContent = mockFS.GetFile(gitignore).TextContents;
        // check that the filter has been added
        Check.That(updatedContent).Contains(arbitraryFilter);
        Check.That(sut.AddFilter(arbitraryFilter)).IsTrue();
        // the contexnt should not have changed
        Check.That(mockFS.GetFile(gitignore).TextContents).IsEqualTo(updatedContent);
    }
}