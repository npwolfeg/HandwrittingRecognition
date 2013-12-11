using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

namespace HandwrittingRecognition
{
    public class SimpleLearning : Learner
    {      
        // o_O
        Saver.getVectorLength vectorLengthHandler;

        public SimpleLearning(bool load)
        {
            // o_O
            vectorLengthHandler = getVectorLength;
            initialize(127);
            if (load)
                loadDefault(false);
        }

        private void initialize(int defaultWeight)
        {
            vectorLength = 100 * 100;
            for (int n = 0; n < optionsCount; n++)
            {
                weights[n] = new double[vectorLength];
                for (int i = 0; i < vectorLength; i++)
                        weights[n][i] = defaultWeight;
                }
        }

        // o_O
        public int getVectorLength(List<double> parameters)
        {
            return vectorLength;
        }

        private void randomInitialize()
        {
            Random rand = new Random();
            for (int n = 0; n < optionsCount; n++)
            {
                weights[n] = new double[vectorLength];
                for (int i = 0; i < vectorLength; i++)
                    weights[n][i] = rand.Next(255);
            }
        }

        override public void saveWeights(string path)
        {
            LearnerData LD = new LearnerData(weights);
            Saver.saveWeights(path, LD, optionsCount, vectorLength);
        }

        override public void loadWeights(string path)
        {
            LearnerData LD = Saver.loadWeights(path, 0, optionsCount, vectorLengthHandler);
            weights = LD.weights;
        }

        override public Bitmap visualize()
        {
            Bitmap result = new Bitmap(1000, 100);
            loadWeights();
            for (int n = 0; n < optionsCount; n++)
            {
                int counter = 0;
                for (int i = 0; i < 100; i++)
                    for (int j = 0; j < 100; j++)
                    {
                        double pixel = weights[n][counter];
                        if (pixel < 0) pixel = 0;
                        if (pixel > 255) pixel = 255;
                        result.SetPixel(n * 100 + i, j, Color.FromArgb(255, (int)pixel, (int)pixel, (int)pixel));
                        counter++;
                    }
            }
            return result;
        }

        override public double[] getVector(Bitmap bmp)
        {
            double[] result = new double[vectorLength];
            int counter = 0;
            for (int i = 0; i < 100; i++)
                for (int j = 0; j < 100; j++)
                {
                    result[counter] = bmp.GetPixel(i, j).R;
                    counter++;
                }
            return result;
        }

        override public void RunAutoTest(BackgroundWorker bw)
        {
            int i = 0;
            initialize(i);
            AutoTest(bw);
        }
    }
}
