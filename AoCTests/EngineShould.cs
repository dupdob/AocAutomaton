using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text.RegularExpressions;
using NFluent;
using NFluent.Mocks;
using NUnit.Framework;

namespace AoC.AoCTests
{
    public class EngineShould
    {
        private const string GoodAnswerFile = "GoodAnswer.html";
        private const string WrongAnswerFile = "WrongAnswer.html";

        private static MockFileSystem GetFileSystem()
        {
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.SetCurrentDirectory(Directory.GetCurrentDirectory());
            return mockFileSystem;
        }
        
        [Test]
        public void SetUpEverythingProperly()
        {
            var mockFileSystem = GetFileSystem();
            var fakeClient = new AoCFakeClient(2015);

            var testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            var engine = new Automaton(2015, fakeClient, mockFileSystem);
            var algo = new FakeSolver(10, null, null);
            using var console = new CaptureConsole();
            engine.RunDay( ()=> algo);
            // verify the day is properly set up
            Check.That(engine.Day).IsEqualTo(10);
            // it should have received the provided input data
            Check.That(algo.InputData).IsEqualTo(testInputData);
            Check.That(fakeClient.NbRequest).IsEqualTo(1);
        }
        
        [Test]
        public void HandleWhenNoAnswerProvided()
        {
            var mockFileSystem = GetFileSystem();
            var fakeClient = new AoCFakeClient(2015);

            var testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            var engine = new Automaton(2015, fakeClient, mockFileSystem);
            var algo = new FakeSolver(10, null, null);
            using var console = new CaptureConsole();
            engine.RunDay( ()=> algo);
            // it should request the first answer
            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            // and not the second
            Check.That(algo.GetAnswer2Calls).IsEqualTo(0);
            // it should say that no answer was provided
            Check.That(console.Output).Contains("No answer provided");
            // it should have received the provided input data
            Check.That(algo.InputData).IsEqualTo(testInputData);
        }
        
        [Test]
        public void HandlerWrongAnswer()
        {
            var fakeClient = new AoCFakeClient(2015);
            var mockFileSystem = GetFileSystem();
            using var console = new CaptureConsole();
          
            fakeClient.SetAnswerResponseFilename(1,WrongAnswerFile);
            var engine = new Automaton(2015, fakeClient, mockFileSystem);
            var algo = new FakeSolver(10, 58, null);
            engine.RunDay( ()=> algo);

            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            Check.That(algo.GetAnswer2Calls).IsEqualTo(0);

            Check.That(console.Output).Contains("AoC site response");
            Check.That(console.Output).Contains("Day 10-1:");
            Check.That(mockFileSystem.AllFiles.Any(p => Regex.IsMatch(p, "Answer1.*\\.html")));
            Check.That(console.Output).Contains("Question 1 failed!");
        }
        
        [Test]
        public void HandlerGoodAnswer()
        {
            var fakeClient = new AoCFakeClient(2015);
            using var console = new CaptureConsole();
            var mockFileSystem = GetFileSystem();
            
            fakeClient.SetAnswerResponseFilename(1, GoodAnswerFile);
            var engine = new Automaton(2015, fakeClient, mockFileSystem);
            var algo = new FakeSolver(10, 12, null);
            engine.RunDay( ()=> algo);

            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            Check.That(console.Output).Contains("AoC site response");
            Check.That(console.Output).Contains("Day 10-1:");
            Check.That(algo.GetAnswer2Calls).IsEqualTo(1);
            Check.That(mockFileSystem.AllFiles.Any(p => Regex.IsMatch(p, "Answer1.*\\.html")));
            Check.That(console.Output).Contains("Question 1 passed!");
        }
        
        [Test]
        public void HandlerWrongAnswerToQuestion2()
        {
            var fakeClient = new AoCFakeClient(2015);
            using var console = new CaptureConsole();
            var mockFileSystem = GetFileSystem();
            
            fakeClient.SetAnswerResponseFilename(1,GoodAnswerFile);
            fakeClient.SetAnswerResponseFilename(2,WrongAnswerFile);
            var engine = new Automaton(2015, fakeClient, mockFileSystem);
            var algo = new FakeSolver(10, 12, 13);
            engine.RunDay( ()=> algo);

            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            Check.That(console.Output).Contains("AoC site response");
            Check.That(console.Output).Contains("Day 10-1:");
            Check.That(console.Output).Contains("Day 10-2:");
            Check.That(algo.GetAnswer2Calls).IsEqualTo(1);
            Check.That(mockFileSystem.AllFiles.Any(p => Regex.IsMatch(p, "Answer1.*\\.html")));
            Check.That(mockFileSystem.AllFiles.Any(p => Regex.IsMatch(p, "Answer2.*\\.html")));
            Check.That(console.Output).Contains("Question 1 passed!");
            Check.That(console.Output).Contains("Question 2 failed!");
        }

        [Test]
        public void HandlerGoodAnswerToQuestion2()
        {
            var fakeClient = new AoCFakeClient(2015);
            using var console = new CaptureConsole();
            var mockFileSystem = GetFileSystem();
            
            fakeClient.SetAnswerResponseFilename(1,GoodAnswerFile);
            fakeClient.SetAnswerResponseFilename(2,GoodAnswerFile);
            var engine = new Automaton(2015, fakeClient, mockFileSystem);
            var algo = new FakeSolver(10, 12, 13);
            engine.RunDay( ()=> algo);

            Check.That(algo.GetAnswer1Calls).IsEqualTo(1);
            Check.That(console.Output).Contains("Question 1 passed!");
            Check.That(console.Output).Contains("Question 2 passed!");
        }
        
        [Test]
        public void CacheInputData()
        {
            var mockFileSystem = GetFileSystem();
            var fakeClient = new AoCFakeClient(2015);

            var testInputData = "Silly input data";
            fakeClient.SetInputData(testInputData);
            var engine = new Automaton(2015, fakeClient, mockFileSystem);
            // run the algo twice
            engine.RunDay(()=> new FakeSolver(10, null, null));
            engine.RunDay(()=> new FakeSolver(10, null, null));
            // input data should have been called once
            Check.That(fakeClient.NbRequest).IsEqualTo(1);
        }
    }
}