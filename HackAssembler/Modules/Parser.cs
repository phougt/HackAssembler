using System.Data;
using HackAssembler.Enums;

namespace HackAssembler.Modules
{
    public class Parser : IDisposable
    {
        public InstructionType Type { get; private set; }
        public string Symbol { get; private set; } = string.Empty;
        public bool LabelPass { get; set; } = false;
        public bool HasMoreCommands { get; private set; } = false;
        public bool IsValidCommand { get; private set; } = false;
        public string Dest { get; private set; } = string.Empty;
        public string Comp { get; private set; } = string.Empty;
        public string Jump { get; private set; } = string.Empty;

        private string _currentCommand = string.Empty;
        private StreamReader _reader;
        private uint _lineNumber = 1;

        public Parser(string filePath)
        {
            _reader = new StreamReader(filePath);
            HasMoreCommands = !_reader.EndOfStream;
        }

        private void RemoveComment()
        {
            int commentStartIndex = _currentCommand.IndexOf(@"//");
            if (commentStartIndex != -1)
            {
                _currentCommand = _currentCommand.Remove(commentStartIndex);
            }
        }

        private void RemoveWhiteSpaces()
        {
            _currentCommand = new string(_currentCommand.Where((c) => !char.IsWhiteSpace(c)).ToArray());
        }

        private bool IsACommand()
        {
            return _currentCommand.Contains('@');
        }

        private bool IsLCommand()
        {
            return _currentCommand.Contains('(')
                 || _currentCommand.Contains(')');
        }

        private bool IsCCommand()
        {
            return !IsLCommand()
                   && !IsACommand();
        }

        private bool ValidateACommand()
        {
            bool startWithAtSign = _currentCommand.StartsWith('@');
            bool hasOneAtSign = _currentCommand.Count((c) => c == '@') == 1;
            bool hasOpenParenthesis = _currentCommand.Any((c) => { return (c == '('); });
            bool hasCloseParenthesis = _currentCommand.Any((c) => { return (c == ')'); });

            if (!startWithAtSign && !LabelPass)
                Console.Error.WriteLine($"Command's expected to start with @. Line: {_lineNumber} \n '{_currentCommand.ElementAt(0)}' is found instead.");

            if (hasOpenParenthesis && !LabelPass)
                Console.Error.WriteLine($"Unexpected '('. Line: {_lineNumber}");

            if (hasCloseParenthesis && !LabelPass)
                Console.Error.WriteLine($"Unexpected ')'. Line: {_lineNumber}");

            if (startWithAtSign && !hasOneAtSign && !LabelPass)
            {
                Console.Error.Write($"\nOnly one @ is expected.\n Line: {_lineNumber} \n Column: ");

                for (int i = 1; i < _currentCommand.Length; i++)
                {
                    if (_currentCommand.ElementAt(i) == '@')
                    {
                        Console.Error.Write($"{i}, ");
                    }
                }
            }

            if (startWithAtSign && hasOneAtSign && !hasCloseParenthesis && !hasOpenParenthesis)
                return true;
            else
                return false;
        }

        private bool ValidateLCommand()
        {
            bool startWithParenthesis = _currentCommand.StartsWith('(');
            bool endWithParenthesis = _currentCommand.EndsWith(')');
            int openParenthesisCount = _currentCommand.Count((c) => c == '(');
            int closeParenthesisCount = _currentCommand.Count((c) => c == ')');
            bool hasOnlyOneOpenParenthesis = openParenthesisCount == 1;
            bool hasOnlyOneCloseParenthesis = closeParenthesisCount == 1;
            bool hasMultiOpenParenthesis = openParenthesisCount > 1;
            bool hasMultiCloseParenthesis = closeParenthesisCount > 1;
            bool hasSymbol = _currentCommand.Any((c) => { return (c != '(') || (c != ')'); });
            bool hasAtSign = _currentCommand.Any((c) => { return (c == '@'); });

            if (!startWithParenthesis && LabelPass)
                Console.Error.WriteLine($"Command's expected to start with '('. \nLine: {_lineNumber} \nColumn: 0");

            if (!endWithParenthesis && LabelPass)
                Console.Error.WriteLine($"Command's expected to end with ')'. Line: {_lineNumber} \n Column: {_currentCommand.Length - 1}");

            if (hasMultiOpenParenthesis && LabelPass)
                Console.Error.WriteLine($"Only one '(' is expected. Line: {_lineNumber}");

            if (hasMultiCloseParenthesis && LabelPass)
                Console.Error.WriteLine($"Only one ')' is expected. Line: {_lineNumber}");

            if (!hasSymbol && LabelPass)
                Console.Error.WriteLine($"Symbol is expected. Line: {_lineNumber}");

            if (hasAtSign && LabelPass)
                Console.Error.WriteLine($"Unexpected '@'. Line: {_lineNumber}");

            return startWithParenthesis
                && endWithParenthesis
                && hasOnlyOneOpenParenthesis
                && hasOnlyOneCloseParenthesis
                && hasSymbol
                && !hasAtSign;
        }

        private bool ValidateCCommand()
        {
            bool startWithSemicolon = _currentCommand.StartsWith(';');
            int semiColonCount = _currentCommand.Count((c) => c == ';');
            int equalCount = _currentCommand.Count((c) => c == '=');
            bool hasSemiColon = semiColonCount > 0;
            bool hasEqual = equalCount > 0;
            bool hasOnlyOneSemiColon = semiColonCount == 1;
            bool hasOnlyOneEqual = equalCount == 1;
            bool hasMultiSemiColon = semiColonCount > 1;
            bool hasMultiEqual = equalCount > 1;
            int semiColonIndex = _currentCommand.IndexOf(';');
            int equalIndex = _currentCommand.IndexOf('=');
            bool endWithEqual = _currentCommand.EndsWith('=');

            if (hasMultiEqual && !LabelPass)
                Console.Error.WriteLine($"Only one '=' is expected. Line: {_lineNumber}");

            if (hasMultiSemiColon && !LabelPass)
                Console.Error.WriteLine($"Only one ';' is expected. Line: {_lineNumber}");

            if (hasOnlyOneSemiColon && hasOnlyOneEqual && !LabelPass)
            {
                if (semiColonIndex < equalIndex)
                {
                    Console.Error.WriteLine($"'=' is expected to be before ';'. Line: {_lineNumber}");
                }
            }

            if (hasOnlyOneEqual && !hasSemiColon && !LabelPass)
            {
                if (endWithEqual)
                {
                    Console.Error.WriteLine($"Unexpected '='. Line: {_lineNumber}");
                }
            }

            if (startWithSemicolon && !LabelPass)
                Console.Error.WriteLine($"Expression is expected before ';'. Line: {_lineNumber}");

            return (hasOnlyOneSemiColon && hasOnlyOneEqual && !(semiColonCount < equalIndex))
                || (hasOnlyOneEqual && !hasSemiColon && !endWithEqual)
                || (hasOnlyOneSemiColon && !hasEqual)
                && (!startWithSemicolon);
        }

        public void Advance()
        {
            _currentCommand = _reader.ReadLine();
            HasMoreCommands = !_reader.EndOfStream;

            if (_currentCommand == null)
            {
                throw new EndOfStreamException();
            }

            RemoveWhiteSpaces();
            RemoveComment();

            if (_currentCommand == string.Empty)
            {
                Type = InstructionType.NONE;
                _lineNumber++;
                IsValidCommand = true;
                return;
            }

            if (IsACommand())
            {
                if (ValidateACommand() == false)
                {
                    IsValidCommand = false;
                    _lineNumber++;
                    return;
                }

                Type = InstructionType.A_COMMAND;
                Symbol = _currentCommand.Remove(0, 1);
                _lineNumber++;
                IsValidCommand = true;
                return;
            }

            if (IsLCommand())
            {
                if (ValidateLCommand() == false)
                {
                    IsValidCommand = false;
                    _lineNumber++;
                    return;
                }

                Type = InstructionType.L_COMMAND;
                Symbol = new string(_currentCommand.Where((c) => { return (c != '(') && (c != ')'); }).ToArray());
                _lineNumber++;
                IsValidCommand = true;
                return;
            }

            if (IsCCommand())
            {
                if (ValidateCCommand() == false)
                {
                    _lineNumber++;
                    IsValidCommand = false;
                    return;
                }

                Type = InstructionType.C_COMMAND;
                string[] parts = _currentCommand.Split(';');
                string compAndDestPart = string.Empty;
                string jumpPart = string.Empty;

                if (parts.Length >= 1)
                    compAndDestPart = parts[0];

                if (parts.Length == 2)
                    jumpPart = parts[1];

                string[] leftParts = compAndDestPart.Split('=');
                string compPart = string.Empty;
                string destPart = string.Empty;

                if (leftParts.Length == 1)
                {
                    compPart = leftParts[0];
                }

                if (leftParts.Length == 2)
                {
                    destPart = leftParts[0];
                    compPart = leftParts[1];
                }

                Dest = destPart;
                Comp = compPart;
                Jump = jumpPart;

                _lineNumber++;
                IsValidCommand = true;
                return;
            }
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}
