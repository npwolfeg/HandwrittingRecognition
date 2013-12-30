using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HandwrittingRecognition
{
    class HandwrittenSymbol
    {
        public Rectangle bounds;
        public HashSet<Point> points;
        public double[] symbolVotes = new double[42];
        public Bitmap bmp;
        public int area = 0;
        public string symbol;

        public HandwrittenSymbol(Rectangle bounds, HashSet<Point> points)
        {
            this.bounds = bounds;
            this.points = points;
            area = bounds.Width * bounds.Height;
        }

        public void addGuess(List<double> dist)
        {
            for (int i = 0; i < dist.Count(); i++)
                symbolVotes[i] += dist[i];
        }

        public string symbolWithMaxVotes()
        {
            List<string> stringDist = Vector.toSortedStringList(symbolVotes.ToList());
            return stringDist[0][0].ToString();
        }

        public void recognition()
        {
            List<Learner> learnerList = new List<Learner>();
            learnerList.Add(new LBPLearning(true));
            learnerList.Add(new CenterLearning(true));
            learnerList.Add(new SimpleLearning(true));
            learnerList.Add(new CountLearning(true));

            bmp = BmpProcesser.normalizeBitmap(bmp, 100, 100);

            foreach (Learner learner in learnerList)
            {
                List<double> dist = learner.guess(bmp);
                addGuess(dist);
            }
            symbolVotes = Vector.normalyzeVector(symbolVotes);
        }
    }
}