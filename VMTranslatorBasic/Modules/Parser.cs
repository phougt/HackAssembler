using VMTranslatorBasic.Enums;

namespace VMTranslatorBasic.Modules
{
    public class Parser : IDisposable
    {
        public CommandType Type { get; private set; }
        public string Symbol { get; private set; } = string.Empty;
        public bool HasMoreCommands { get; private set; } = false;
        public bool IsValidCommand { get; private set; } = false;

        private string _currentCommand = string.Empty;
        private StreamReader _reader;
        private uint _lineNumber = 1;

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}
