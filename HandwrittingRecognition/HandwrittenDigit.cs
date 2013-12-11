using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HandwrittingRecognition
{
    class HandwrittenDigit
    {
        public Rectangle bounds;
        public HashSet<Point> points;
        public double[] digitVotes = new double[42];
        public Bitmap bmp;
        public int area = 0;

        public HandwrittenDigit()
        {
        }

        public HandwrittenDigit(Rectangle bounds, HashSet<Point> points)
        {
            this.bounds = bounds;
            this.points = points;
            area = bounds.Width * bounds.Height;
        }

        public void addGuess(List<double> dist)
        {
            for (int i = 0; i < dist.Count(); i++)
                digitVotes[i] += dist[i];
        }

        public string digitWithMaxVotes()
        {
            List<string> stringDist = Vector.toSortedStringList(digitVotes.ToList());
            return stringDist[0][0].ToString();
        }

    }
}