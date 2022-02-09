using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AoC.AoCTests
{
    public sealed class AoCFakeClient : AoCClientBase
    {
        private string _inputData;
        private readonly Dictionary<int, string> _responseFile = new();
        
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

        public override Task<string> PostAnswer(int id, string value) => File.ReadAllTextAsync(_responseFile[id]);
        
        public override void Dispose()
        {}
    }
}