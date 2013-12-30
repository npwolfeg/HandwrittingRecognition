using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HandwrittingRecognition
{
    class SymbolsToWordsLogic
    {
        private static double distance(Rectangle rect1, Rectangle rect2)
        {
            double x1 = rect1.X + rect1.Width / 2;
            double x2 = rect2.X + rect2.Width / 2;
            double y1 = rect1.Y + rect1.Height / 2;
            double y2 = rect2.Y + rect2.Height / 2;
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        private static bool isNear(Rectangle rect1, Rectangle rect2)
        {
            int averageWidth = (rect1.Width + rect2.Width) / 2;
            return distance(rect1, rect2) < 1.5*averageWidth;
        }

        private static bool isHeightTheSame(Rectangle rect1, Rectangle rect2, int deltaPercent)
        {
            double delta = Math.Max(rect1.Height, rect2.Height) * deltaPercent / 100;
            return Math.Abs(rect1.Height - rect2.Height) < delta;
        }

        private static bool isOnTheSameLine(Rectangle rect1, Rectangle rect2, int deltaPercent)
        {
            double delta = Math.Max(rect1.Height, rect2.Height) * deltaPercent / 100;
            return Math.Abs(rect1.Bottom - rect2.Bottom) < delta;
        }

        private static bool isSameWord(Rectangle rect1, Rectangle rect2)
        {
            return isNear(rect1, rect2)/* & isHeightTheSame(rect1, rect2, 25) & isOnTheSameLine(rect1, rect2, 60)*/;
        }

        public static List<HandwrittenWord> SymbolsToWords(List<HandwrittenSymbol> symbols)
        {
            int[] symbolWords = new int[symbols.Count]; // int = number of symbols word in words list
            for (int i = 0; i < symbols.Count; i++)
                symbolWords[i] = i;

            for (int i = 0; i < symbols.Count - 1; i++)
                for (int j = i + 1; j < symbols.Count; j++)
                    if (isSameWord(symbols[i].bounds, symbols[j].bounds))
                        symbolWords[j] = symbolWords[i];               //now each int in symbolWords contains correct number of symbols word

            HashSet<int> words = new HashSet<int>();
            foreach (int word in symbolWords)
                words.Add(word);                                     //delete duplicates

            //get rid of this block?
            List<List<int>> numbersOfSymbolsInWords = new List<List<int>>();
            foreach (int word in words)
                numbersOfSymbolsInWords.Add(new List<int>());
            for (int i = 0; i < symbolWords.Length; i++)
                numbersOfSymbolsInWords[words.ToList().IndexOf(symbolWords[i])].Add(i);

            // List<List<int>> to List<HandwrittenWord>
            List<HandwrittenWord> result = new List<HandwrittenWord>();
            for (int i = 0; i < numbersOfSymbolsInWords.Count;i++ )
            {
                result.Add(new HandwrittenWord());
                for(int j=0;j<numbersOfSymbolsInWords[i].Count;j++)
                    result[i].symbols.Add(symbols[numbersOfSymbolsInWords[i][j]]);
            }
            return result;
        }
    }
}
