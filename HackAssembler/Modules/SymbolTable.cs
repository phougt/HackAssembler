using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackAssembler.Modules
{
    public class SymbolTable
    {
        private Dictionary<string, int> _symbolTable;
        public SymbolTable(Dictionary<string, int> symbolTable)
        {
            _symbolTable = symbolTable;
        }

        public void AddEntry(string symbol, int address)
        {
            _symbolTable.Add(symbol, address);
        }

        public bool Contains(string symbol)
        {
            return _symbolTable.ContainsKey(symbol);
        }

        public int GetAddress(string symbol) { return _symbolTable[symbol]; }
    }
}
