﻿using System;
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
        private double[] digitVotes = new double[10];

        public HandwrittenDigit()
        {
        }

        public HandwrittenDigit(Rectangle bounds, HashSet<Point> points)
        {
            this.bounds = bounds;
            this.points = points;
        }

        public void addGuess(List<double> dist)
        {
            for (int i = 0; i < dist.Count(); i++)
                digitVotes[i] += dist[i];
        }

        public int digitWithMaxVotes()
        {
            return digitVotes.ToList().IndexOf(digitVotes.Min());
        }
    }
}