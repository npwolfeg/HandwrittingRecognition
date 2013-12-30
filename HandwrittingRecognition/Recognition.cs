using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;
using System.IO;

namespace HandwrittingRecognition
{
    class Recognition
    {
        static int optionsCount = 42;

        static public List<string> symbolRecognition(Bitmap bmp)
        {            
            List<Learner> learnerList = new List<Learner>();
            learnerList.Add(new LBPLearning(true));
            learnerList.Add(new CenterLearning(true));
            learnerList.Add(new SimpleLearning(true));
            learnerList.Add(new CountLearning(true));

            bmp = BmpProcesser.normalizeBitmap(bmp, 100, 100);

            double[] symbolVotes = new double[optionsCount];


            foreach (Learner learner in learnerList)
            {
                List<double> dist = learner.guess(bmp);
                for (int i = 0; i < dist.Count(); i++)
                    symbolVotes[i] += dist[i];
            }
            return Vector.toSortedStringList(symbolVotes.ToList());
        }

        static public Bitmap bigPictureRecognition(Bitmap bigBitmap, BackgroundWorker bw)
        {
            Bitmap newBigBitmap = new Bitmap(bigBitmap);

            List<HandwrittenSymbol> symbols = BmpProcesser.getDigitsList(bigBitmap, bw);

            foreach (HandwrittenSymbol symbol in symbols)
                symbol.recognition();

            List<HandwrittenWord> words = SymbolsToWordsLogic.SymbolsToWords(symbols);

            foreach (HandwrittenWord word in words)
                using (Graphics g = Graphics.FromImage(newBigBitmap))
                    g.DrawString(word.symbolBySymbolText() + " " + word.wordDBtext(), new Font("Arial", 20), new SolidBrush(Color.Red), word.left(), word.top());

            saveReconResult(newBigBitmap);
            return newBigBitmap;
        }

        static void saveReconResult(Bitmap bmp)
        {
            int testNumber = Convert.ToInt32(File.ReadAllText(@"Tests\count.txt"));
            testNumber++;
            string filename = testNumber.ToString();
            bmp.Save(@"Tests\" + filename + ".bmp");
            File.WriteAllText(@"Tests\count.txt", filename);
        }
    }
}
