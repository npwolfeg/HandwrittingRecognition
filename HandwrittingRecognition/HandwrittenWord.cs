using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandwrittingRecognition
{
    class HandwrittenWord
    {
        public List<HandwrittenSymbol> symbols = new List<HandwrittenSymbol>();

        public string wordDBtext()
        {
            return WordDB.findWord(symbols);
        }

        public string symbolBySymbolText()
        {
            string result = "";
            for (int i = 0; i < symbols.Count; i++)
                result += symbols[i].symbolWithMaxVotes();
            return result;
        }

        public int left()
        {
            return symbols[0].bounds.Left;
        }

        public int top()
        {
            return symbols[0].bounds.Top-40;
        }
    }
}
