using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;


namespace HandwrittingRecognition
{

    public partial class Form1 : Form
    {
        bool canDraw = false;
        Bitmap drawingBitmap, bigBitmap;
        int drawingWidth = 10;
        int[] count = new int[10];
        Point[] points = new Point[2];
        CountLearning learner = new CountLearning(false);
        DistanceType distanceType = DistanceType.Euclid;

        public void clearImg()
        {
            drawingBitmap = new Bitmap(100, 100);
            pictureBox1.Image = drawingBitmap;
            bigBitmap = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            pictureBox2.Image = bigBitmap;
        }


        /*public int guessWithAutoGrayScale(Bitmap bmp, int step)
        {
            listBox1.Items.Clear();
            double min = 100;
            int ID = 0;
            Bitmap temp = new Bitmap(drawingBitmap);
            List<double> dist;
            progressBar1.Value = 0;
            progressBar1.Maximum = 255;

            for (int i = 0; i < 255; i += step)
            {
                progressBar1.Value = i;
                temp = BmpProcesser.GrayScale(bmp, i);
                temp = BmpProcesser.normalizeBitmap(temp, 100, 100);
                dist = guessWide(temp);
                double currentMin = dist.Min();
                if (currentMin < min)
                {
                    min = currentMin;
                    ID = dist.IndexOf(currentMin);
                    listBox1.Items.Add(ID.ToString() + ' ' + i.ToString() + ' ' + min.ToString());
                    pictureBox1.Image = temp;
                }
            }
            return ID;
        }*/

        public Form1()
        {
            InitializeComponent();
            drawingBitmap = new Bitmap(100, 100);
            pictureBox1.Image = drawingBitmap;
            bigBitmap = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            pictureBox2.Image = bigBitmap;
            distanceListBox.Items.Add("Euclid");
            distanceListBox.Items.Add("KullbackLeibler");
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            canDraw = true;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            canDraw = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (canDraw)
            {
                for (int i = 0; i < drawingWidth; i++)
                    for (int j = 0; j < drawingWidth; j++)
                        if (e.X + i > -1 && e.X + i < 100 && e.Y + j > -1 && e.Y + j < 100)
                            drawingBitmap.SetPixel(e.X + i, e.Y + j, Color.Black);
                pictureBox1.Refresh();
            }
            else
            {
                label1.Text = drawingBitmap.GetPixel(e.X, e.Y).ToString();
                label2.Text = e.Location.ToString();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*drawingBitmap = new Bitmap(@"F:\DigitDB\PictureSaver\01.bmp");
            drawingBitmap = BmpProcesser.normalizeBitmapRChannel(drawingBitmap, 100, 100);
            pictureBox1.Image = drawingBitmap;*/
            bigBitmap = new Bitmap(@"Tests\bigBitmap" + textBox2.Text + ".bmp");
            pictureBox2.Image = bigBitmap;            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            clearImg();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button13_Click(object sender, EventArgs e)
        {
            drawingBitmap = BmpProcesser.GrayScale(drawingBitmap);
            drawingBitmap = BmpProcesser.ResizeBitmap(drawingBitmap, 100, 100);
            pictureBox1.Image = drawingBitmap;
            drawingBitmap = BmpProcesser.normalizeBitmap(drawingBitmap, 100, 100);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            /*DateTime date1 = DateTime.Now;
            listBox1.Items.Add(guessWithAutoGrayScale(drawingBitmap, 10));
            DateTime date2 = DateTime.Now;
            TimeSpan ts = date2 - date1;
            textBox1.Text = (ts.Seconds * 1000 + ts.Milliseconds).ToString();*/
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            canDraw = true;
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            canDraw = false;

        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (canDraw)
            {
                for (int i = 0; i < drawingWidth; i++)
                    for (int j = 0; j < drawingWidth; j++)
                        if (e.X + i > -1 && e.X + i < bigBitmap.Width && e.Y + j > -1 && e.Y + j < bigBitmap.Height)
                            bigBitmap.SetPixel(e.X + i, e.Y + j, Color.Black);
                pictureBox2.Image = bigBitmap;
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            bigBitmap = learner.visualize();
            pictureBox2.Image = bigBitmap;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            LBPLearning LBPLearner = new LBPLearning(true);
            //LBPLearner.loadWeights(@"weights\LBPLearning\auto\4x4kohonen nonLinearDelta 0,2.txt");
            CenterLearning CLearner = new CenterLearning(true);
            //CLearner.loadWeights(@"weights\CenterLearning\auto\4x4kohonen nonLinearDelta 0,2.txt");
            SimpleLearning SLearner = new SimpleLearning(true);
            //SLearner.loadWeights(@"weights\SimpleLearning\auto\defaultWeight127kohonen nonLinearDelta 0,2.txt");
            CountLearning CountLearner = new CountLearning(true);
            //CountLearner.loadWeights(@"weights\CountLearning\Auto\16x16kohonen nonLinearDelta 0,2.txt");

            if (BmpProcesser.isAlpha(drawingBitmap))
                drawingBitmap = BmpProcesser.FromAlphaToRGB(drawingBitmap);
            drawingBitmap = BmpProcesser.normalizeBitmapRChannel(drawingBitmap, 100, 100);
            drawingBitmap = BmpProcesser.renewRChannel(drawingBitmap);
            listBox1.Items.Clear();
            pictureBox1.Image = drawingBitmap;
            int ID;
            List<double> dist;

            /*dist = LBPLearner.guess(drawingBitmap); 
            listBox1.Items.Add("LBP 4x4 kohonen");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }

            /*dist = CLearner.guess(drawingBitmap);
            listBox1.Items.Add("Center 4x4 kohonen");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }

            CLearner.loadWeights(@"weights\CenterLearning\auto\16x16kohonen nonLinearDelta 0,2.txt");

            dist = CLearner.guess(drawingBitmap);
            listBox1.Items.Add("Center 16x16 kohonen");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }*/


            /*dist = SLearner.guess(drawingBitmap);
            listBox1.Items.Add("Simple kohonen");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }

            dist = SLearner.guessKullbackLeiblerDistance (drawingBitmap);
            listBox1.Items.Add("Simple kohonen");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }*/

            /*dist = CountLearner.guess(drawingBitmap);
            listBox1.Items.Add("count 16x16 kohonen");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }*/

            /*dist = CountLearner.guess(drawingBitmap);
            listBox1.Items.Add("count 16x16 kohonen");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }*/

            

            CountLearner.loadWeights(@"weights\CountLearning\Auto\4x4kohonen nonLinearDelta 0,2.txt");

            List<string> stringDist = Vector.toSortedStringList(CountLearner.guess(drawingBitmap));
            listBox1.Items.Add("count 4x4 kohonen");
            foreach(string st in stringDist)
                listBox1.Items.Add(st);

            CountLearner.loadWeights(@"weights\CountLearning\AutoKBDistance\4x4kohonen nonLinearDelta 0,2.txt");

            dist = CountLearner.guess(drawingBitmap);
            listBox1.Items.Add("count 4x4 kohonen");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }

            /*dist = CountLearner.guessKullbackLeiblerDistance(drawingBitmap);
            listBox1.Items.Add("count 4x4 kohonen");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }*/

            /*LBPLearner.loadWeights(@"weights\LBPLearning\auto\4x4average .txt");
            CLearner.loadWeights(@"weights\CenterLearning\auto\16x16average .txt");
            SLearner.loadWeights(@"weights\SimpleLearning\auto\defaultWeight127average .txt");
            CountLearner.loadWeights(@"weights\CountLearning\Auto\16x16average .txt");

            dist = LBPLearner.guess(drawingBitmap);
            listBox1.Items.Add("LBP 4x4 average");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }

            dist = CLearner.guess(drawingBitmap);
            listBox1.Items.Add("Center 4x4 average");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }

            dist = SLearner.guess(drawingBitmap);
            listBox1.Items.Add("Simple average");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }

            dist = CountLearner.guess(drawingBitmap);
            listBox1.Items.Add("count 16x16 average");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }*/

        }

        private void button17_Click(object sender, EventArgs e)
        {
            LBPLearning LBPLearner = new LBPLearning(true);
            CenterLearning CLearner = new CenterLearning(true);
            SimpleLearning SLearner = new SimpleLearning(true);
            CountLearning CountLearner = new CountLearning(true);            

            //find digits in the bitmap
            progressBar1.Value = 0;
            progressBar1.Maximum = bigBitmap.Width * bigBitmap.Height;
            List<HandwrittenDigit> digits = new List<HandwrittenDigit>();
            Rectangle rect;
            HashSet<Point> pts;
            //List<int> possibleDigits;
            Bitmap newBigBitmap = new Bitmap(bigBitmap);
            for (int i = 0; i < newBigBitmap.Width; i++)
                for (int j = 0; j < newBigBitmap.Height; j++)
                {
                    progressBar1.Value++;
                    if (newBigBitmap.GetPixel(i, j).A != 0)
                    {
                        pts = BmpProcesser.getConnectedPicture(new Point(i, j), newBigBitmap);

                        Bitmap bmp = new Bitmap(newBigBitmap.Width, newBigBitmap.Height);
                        foreach (Point p in pts)
                        {
                            bmp.SetPixel(p.X, p.Y, Color.Black);
                            newBigBitmap.SetPixel(p.X, p.Y, Color.FromArgb(0, 0, 0, 0));
                        }
                        rect = BmpProcesser.getBounds(bmp);
                        digits.Add(new HandwrittenDigit(rect, pts));

                        bmp = BmpProcesser.copyPartOfBitmap(bmp, rect);

                        //possibleDigits = new List<int>();

                        drawingBitmap = BmpProcesser.FromAlphaToRGB(bmp);
                        drawingBitmap = BmpProcesser.normalizeBitmapRChannel(drawingBitmap, bmp.Width, bmp.Height);
                        drawingBitmap = BmpProcesser.ResizeBitmap(drawingBitmap, 100, 100);

                        List<double> dist = LBPLearner.guess(drawingBitmap);
                        digits.Last().addGuess(dist);

                        dist = SLearner.guess(drawingBitmap);
                        digits.Last().addGuess(dist);

                        dist = CLearner.guess(drawingBitmap);
                        digits.Last().addGuess(dist);

                        dist = CountLearner.guess(drawingBitmap);
                        digits.Last().addGuess(dist);

                        /*LBPLearner.loadWeights(@"weights\LBPLearning\auto\4x4average .txt");
                        CLearner.loadWeights(@"weights\CenterLearning\auto\4x4average .txt");
                        SLearner.loadWeights(@"weights\SimpleLearning\auto\defaultWeight127average .txt");
                        CountLearner.loadWeights(@"weights\CountLearning\Auto\4x4average .txt");*/

                        LBPLearner.loadDefault(true);
                        CLearner.loadDefault(true);
                        SLearner.loadDefault(true);
                        CountLearner.loadDefault(true);

                        dist = LBPLearner.guess(drawingBitmap);
                        digits.Last().addGuess(dist);

                        dist = SLearner.guess(drawingBitmap);
                        digits.Last().addGuess(dist);

                        dist = CLearner.guess(drawingBitmap);
                        digits.Last().addGuess(dist);

                        dist = CountLearner.guess(drawingBitmap);
                        digits.Last().addGuess(dist);
                    }
                }
            //gather digits into numbers and display them on top ot fte first digit in number
            newBigBitmap = new Bitmap(bigBitmap);
            List<List<int>> numbers = DigitsToNumbersLogic.digitsToNumbers(digits);
            foreach (List<int> number in numbers)
            {
                int counter = 0;
                int left = digits[number[0]].bounds.Left;
                int top = digits[number[0]].bounds.Top;
                foreach (int digit in number)
                {
                    using (Graphics g = Graphics.FromImage(newBigBitmap))
                    {
                        g.DrawString(digits[digit].digitWithMaxVotes().ToString(), new Font("Arial", 20), new SolidBrush(Color.Red), left + counter * 20, top - 30);
                        //g.DrawString(digits[digit].possiebleDigits[1].ToString(), new Font("Arial", 20), new SolidBrush(Color.Orange), left + counter * 20, top - 50);
                        //g.DrawString(digits[digit].possiebleDigits[2].ToString(), new Font("Arial", 20), new SolidBrush(Color.Green), left + counter * 20, top - 70);
                        //g.DrawRectangle(new Pen(Color.Orange, 4), currentDigit.bounds);
                    }
                    counter++;
                }
            }
            pictureBox2.Image = newBigBitmap;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bigBitmap = BmpProcesser.DrawGrid(bigBitmap, 100, 100);
            pictureBox2.Image = bigBitmap;
        }

        private void bg_test_work(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            learner.AutoTest(bw);
        }

        private void bg_test_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Done!");
        }

        private void bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(bg_test_work);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_test_RunWorkerCompleted);
            bw.ProgressChanged += new ProgressChangedEventHandler(bg_ProgressChanged);
            bw.WorkerReportsProgress = true;
            progressBar1.Value = 0;
            progressBar1.Maximum = 100;
            bw.RunWorkerAsync(count);
        }

        private void distanceListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Distances.DT = (DistanceType)distanceListBox.SelectedIndex;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Text = (Convert.ToInt32(textBox2.Text) + 1).ToString();
        }
    }

}

