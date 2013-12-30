using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HandwrittingRecognition
{
    class WordDB
    {
        static string WordDBDir = @"C:\Users\Wolf\Dropbox\bakalavr\WordDB\";

        int distanceBetweenWords(string word1, string word2)
        {
            int shortLength;
            if (word1.Length < word2.Length)
                shortLength = word1.Length;
            else
                shortLength = word2.Length;
            int distance = 0;
            for (int i = 0; i < shortLength; i++)
                if (word1[i] != word2[i])
                    distance++;
            return distance;
        }

        string findWord(string word)
        {
            List<string> words = loadTableWords(word.Length.ToString());
            List<double> freqs = loadTableFreqs(word.Length.ToString());

            int id = -1;

            List<double> distances = new List<double>();
            for (int i = 0; i < words.Count(); i++)
            {
                double distance = distanceBetweenWords(word, words[i]);
                distances.Add(distance);
                if (distance == 0)
                {
                    id = i;
                    break;
                }
            }

            if (id == -1)
            {
                id = distances.IndexOf(distances.Min());
            }

            return words[id];
        }

        static public List<string> getPossibleOptions()
        {
            List<string> result = new List<string>();
            using (StreamReader sr = new StreamReader("possible options.txt"))
            {
                for (int i = 0; i < 42; i++)
                    result.Add(sr.ReadLine());
            }
            return result;
        }

        static double distanceBetweenWords(List<HandwrittenSymbol> word1, string word2)
        {
            List<string> possibleSymbols = getPossibleOptions();

            int shortLength;
            if (word1.Count < word2.Length)
                shortLength = word1.Count;
            else
                shortLength = word2.Length;
            double distance = 0;
            for (int i = 0; i < shortLength; i++)
            {
                int id = possibleSymbols.IndexOf(word2[i].ToString().ToLower());
                    distance+=word1[i].symbolVotes[id];
            }
            return distance;
        }

        public static string findWord(List<HandwrittenSymbol> wordSymbols)
        {
            List<string> words = loadTableWords(wordSymbols.Count.ToString());
            List<double> freqs = loadTableFreqs(wordSymbols.Count.ToString());
            List<double> distances = new List<double>();
            for (int i = 0; i < words.Count(); i++)
            {
                double distance = distanceBetweenWords(wordSymbols, words[i]);
                distances.Add(distance);
            }

            return words[distances.IndexOf(distances.Min())];
        }

        static List<string> loadTableWords(string tablename)
        {
            List<string> words = new List<string>();
            using (StreamReader sr = new StreamReader(WordDBDir + tablename + ".words"))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                    words.Add(s);
            }
            return words;
        }

        static List<double> loadTableFreqs(string tablename)
        {
            List<double> freqs = new List<double>();
            using (StreamReader sr = new StreamReader(WordDBDir + tablename + ".freq"))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                    freqs.Add(Convert.ToInt32(s));
            }
            double maxFreq = freqs.Max();
            for (int i = 0; i < freqs.Count; i++)
                freqs[i] = 1 - freqs[i] / maxFreq;
            return freqs;
        }
    }
}
