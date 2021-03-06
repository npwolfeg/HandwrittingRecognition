﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

namespace HandwrittingRecognition
{
    class CenterLearning : Learner
    {
        int blockRows = 16;
        int blockCols = 16;
        static int picWidth = 100;
        static int picHeight = 100;
        int blockWidth;
        int blockHeight; 
      
        Saver.getVectorLength vectorLengthHandler;

        public CenterLearning(bool load)
        {
            vectorLengthHandler = getVectorLength;
            initialize(16,16);
            if (load)
                loadDefault(false);
        }

        private void initialize(int blockCols, int blockRows)
        {
            this.blockCols = blockCols;
            this.blockRows = blockRows;
            weights = new double[optionsCount][];
            vectorLength = 2 * blockRows * blockCols;
            blockWidth = picWidth / blockCols;
            blockHeight = picHeight / blockRows;
            for (int n = 0; n < optionsCount; n++)
            {
                weights[n] = new double[vectorLength];
                for (int i = 0; i < vectorLength; i++)
                    weights[n][i] = 0;
            }
        }

        public int getVectorLength(List<double> parameters)
        {
            return (int) (2 * parameters[0] * parameters[1]);
        }

        override public void saveWeights(string path)
        {
            LearnerData LD = new LearnerData(blockCols, blockRows, weights);
            Saver.saveWeights(path, LD, optionsCount, vectorLength);
        }

        override public void loadWeights(string path)
        {
            LearnerData LD = Saver.loadWeights(path, 2, optionsCount, vectorLengthHandler);
            initialize((int)LD.parameters[0], (int)LD.parameters[1]);
            weights = LD.weights;
        }

        override public void RunAutoTest(BackgroundWorker bw)
        {
            for (int i = 4; i <= 4; i *= 2)
            {
                AutoTest(bw);
            }
        }

        override public Bitmap visualize()
        {            
            Bitmap result = new Bitmap(1000, 100);
            loadWeights();
            for (int n = 0; n < optionsCount; n++)
                using (Graphics g = Graphics.FromImage(result))
                {
                    int counter = 0;
                    for (int i = 0; i < blockCols; i++)
                        for (int j = 0; j < blockRows; j++)
                        {
                            g.DrawRectangle(new Pen(Color.Blue, 4), n * picWidth + i * blockWidth, j * blockHeight, blockWidth, blockHeight);
                            g.DrawRectangle(new Pen(Color.Orange, 4), new Rectangle(n * picWidth + i * blockWidth + (int)weights[n][counter], j * blockHeight + (int)weights[n][counter+1], 1, 1));
                            g.DrawLine(new Pen(Color.Black, 1), n * picWidth + i * blockWidth + blockWidth / 2, j * blockHeight, n * picWidth + i * blockWidth + blockWidth / 2, (j + 1) * blockHeight);
                            g.DrawLine(new Pen(Color.Black, 1), n * picWidth + i * blockWidth, j * blockHeight + blockHeight/2, n * picWidth + (i+1) * blockWidth, j * blockHeight + blockHeight/2);
                            counter++;
                        }
                }
            return result;
        }

        double[] Center(Bitmap bmp)
        {
            int counter = 0;
            double[] result = new double[2];
            for (int i=0;i<bmp.Width;i++)
                for(int j=0;j<bmp.Height;j++)
                    if (bmp.GetPixel(i, j).R < 255)
                    {
                        counter++;
                        result[0] += i;
                        result[1] += j;
                    }
            if (counter != 0)
            {
                result[0] = result[0] / counter;
                result[1] = result[1] / counter;
            }
            else
            {
                result[0] = blockWidth / 2;
                result[1] = blockHeight / 2;
            }
            return result;
        }

        override public double[] getVector(Bitmap bmp)
        {
            double[] result = new double[vectorLength];
            Rectangle copyRect;
            int counter = 0;

            for (int i = 0; i < blockCols; i++)
                for (int j = 0; j < blockRows; j++)
                {
                    copyRect = new Rectangle(i * blockWidth, j * blockHeight, blockWidth, blockHeight);
                    Bitmap partOfBmp = BmpProcesser.copyPartOfBitmap(bmp, copyRect);
                    double[] currentCenter = Center(partOfBmp);
                    result[counter] = currentCenter[0];
                    result[counter + 1] = currentCenter[1];
                    counter++;
                }
            return result;
        }
    }
}
