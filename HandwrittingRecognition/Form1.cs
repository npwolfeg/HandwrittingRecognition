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
        int pointsCount = 8;
        int radius = 2;
        static int blockRows = 4;
        static int blockCols = 4;
        static int picWidth = 100;
        static int picHeight = 100;
        static int blockWidth = picWidth / blockCols;
        static int blockHeight = picHeight / blockRows;
        double[, ,][] wideHistograms = new double[10, blockCols, blockRows][];
        int[] histogramCount = new int[10];
        byte[] uniformPatterns = new byte[57];
        int[] count = new int[10];
        //string path = @"F:\C#\MNIST Reader\MNIST Reader\bin\Debug\";
        string path = @"F:\C#\NumberPicturesSaver\NumberPicturesSaver\bin\Debug\";
        Point[] points = new Point[2];
        CountLearning learner = new CountLearning();

        public void clearWideHistograms()
        {
            for (int i = 0; i < 10; i++)
                for (int x = 0; x < blockCols; x++)
                    for (int y = 0; y < blockRows; y++)
                        wideHistograms[i, x, y] = new double[300];
            histogramCount = new int[10];
        }

        public void saveWideHistograms(string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                for (int i = 0; i < 10; i++)
                    for (int x = 0; x < blockCols; x++)
                        for (int y = 0; y < blockRows; y++)
                        {
                            sw.WriteLine(histogramCount[i].ToString());
                            for (int j = 0; j < 300; j++)
                                sw.WriteLine(wideHistograms[i, x, y][j].ToString());
                        }
            }
        }

        public void loadWideHistograms(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                for (int i = 0; i < 10; i++)
                    for (int x = 0; x < blockCols; x++)
                        for (int y = 0; y < blockRows; y++)
                        {
                            histogramCount[i] = Convert.ToInt32(sr.ReadLine());
                            for (int j = 0; j < 300; j++)
                                wideHistograms[i, x, y][j] = Convert.ToDouble(sr.ReadLine());
                        }
            }
        }

        public void clearImg()
        {
            drawingBitmap = new Bitmap(100, 100);
            pictureBox1.Image = drawingBitmap;
            bigBitmap = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            pictureBox2.Image = bigBitmap;
        }

        //bad
        private void fillUnformPatterns()
        {
            for (int i = 1; i < 8; i++)

                for (int j = 0; j < 8; j++)
                {
                    uniformPatterns[(i - 1) * 8 + j] = (byte)(Math.Pow(2, i) - 1);
                    //UniformPatterns[(i - 1) * 8 + j] = UniformPatterns[(i - 1) * 8 + j] >> j;
                    listBox1.Items.Add(uniformPatterns[(i - 1) * 8 + j].ToString());
                }
        }

        public int analyzePixel(Bitmap bmp, int x, int y)
        {
            int sum = 0;
            if (x > 0 && x < 99 && y > 0 && y < 99)
            {
                int power = 0;
                for (int i = -1; i < 2; i++)
                    for (int j = -1; j < 2; j++)
                        if (i != 0 || j != 0)
                        {
                            if (bmp.GetPixel(x + i, y + j).A >= bmp.GetPixel(x, y).A)
                                sum += (int)Math.Pow(2, power);
                            power++;
                        }
            }
            return sum;
        }

        public double[] getHistogram(Bitmap bmp, int x, int y, int width, int height)
        {
            double[] result = new double[300];
            for (int i = x; i < width + x; i++)
                for (int j = y; j < height + y; j++)
                    result[analyzePixel(bmp, i, j)]++;
            result = Vector.normalyzeVektor(result);
            return result;
        }

        public void learnWide(Bitmap bmp, int n)
        {
            for (int x = 0; x < blockCols; x++)
                for (int y = 0; y < blockRows; y++)
                {
                    double[] hist = getHistogram(bmp, blockWidth * x, blockHeight * y, blockWidth, blockHeight);
                    for (int i = 0; i < 300; i++)
                        wideHistograms[n, x, y][i] = (hist[i] + wideHistograms[n, x, y][i] * histogramCount[n]) / (histogramCount[n] + 1);
                }
            histogramCount[n]++;
        }

        public void learnAll(string path, int learningCount)
        {
            Bitmap bmp;
            using (StreamReader sr = new StreamReader(path + "count.txt"))
            {
                for (int i = 0; i < 10; i++)
                    count[i] = Convert.ToInt32(sr.ReadLine());
            }
            progressBar1.Maximum = learningCount * 10;
            progressBar1.Value = 0;
            for (int k = 0; k < 10; k++)
            {
                for (int n = 0; n < learningCount; n++)
                {
                    progressBar1.Value++;
                    bmp = new Bitmap(path + k.ToString() + n.ToString() + ".bmp");
                    bmp = BmpProcesser.normalizeBitmap(bmp, 100, 100);               
                    learnWide(bmp, k);
                }
            }
        }

        public double Distance(double[] hist1, double[] hist2)
        {
            double sum = 0;
            for (int i = 0; i < 300; i++)
                sum += Math.Pow((hist1[i] - hist2[i]), 2);
            return sum;
        }

        public List<double> guessWide(Bitmap bmp)
        {
            double[,][] hist = new double[blockCols, blockRows][];
            List<double> result = new List<double>();
            for (int x = 0; x < blockCols; x++)
                for (int y = 0; y < blockRows; y++)
                {
                    hist[x, y] = getHistogram(bmp, blockWidth * x, blockHeight * y, blockWidth, blockHeight);
                }
            for (int i = 0; i < 10; i++)
            {
                result.Add(0);
                for (int x = 0; x < blockCols; x++)
                    for (int y = 0; y < blockRows; y++)
                    {
                        result[i] += Distance(hist[x, y], wideHistograms[i, x, y]);
                    }
            }
            result = Vector.normalyzeVektor(result);
            return result;
        }

        public int guessWithAutoGrayScale(Bitmap bmp, int step)
        {
            listBox1.Items.Clear();
            double min = 100;
            int ID = 0;
            Bitmap temp = new Bitmap(drawingBitmap);
            List<double> dist;
            progressBar1.Value = 0;
            progressBar1.Maximum = 255;

            //has local mins, retry after neural network learning will work
            /*int left = 50;
            int right = 200;
            int epsilon = 10;
            int x=0,x1,x2;
            double f1; 
            double f2;
            while (right - left > epsilon)
            {               
                x = (left + right) / 2;
                x1 = x - epsilon;
                x2 = x + epsilon;
                temp = GrayScale(bmp, x1);
                temp = normalizeBitmap(temp, 100, 100);
                dist = guessWide(temp);
                f1 = dist.Min();
                temp = GrayScale(bmp, x2);
                temp = normalizeBitmap(temp, 100, 100);
                dist = guessWide(temp);
                f2 = dist.Min();
                if (f1 < f2)
                    right = x1;
                else
                    left = x2;
            }
            temp = GrayScale(bmp, x);
            temp = normalizeBitmap(temp, 100, 100);
            dist = guessWide(temp);
            pictureBox1.Image = temp;
            return dist.IndexOf(dist.Min());*/

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

        }

        public int[,] guessAll(int guessingCount)
        {
            Bitmap bmp;
            int[,] result = new int[10, 2];
            using (StreamReader sr = new StreamReader(path + "count.txt"))
            {
                for (int i = 0; i < 10; i++)
                    count[i] = Convert.ToInt32(sr.ReadLine());
            }
            progressBar1.Maximum = 1000;
            progressBar1.Value = 0;
            for (int n = 0; n < 0 + guessingCount; n++)
            {
                for (int k = 0; k < 10; k++)
                {
                    progressBar1.Value++;
                    bmp = new Bitmap(path + k.ToString() + n.ToString() + ".bmp");
                    bmp = BmpProcesser.normalizeBitmap(bmp, 100, 100);
                    //List<double> dist = guess();
                    List<double> dist = guessWide(bmp);
                    int ID = dist.IndexOf(dist.Min());
                    if (ID == k)
                        //if (dist[k]<5)
                        result[k, 0]++;
                    else
                        result[ID, 1]++;
                }
            }
            return result;
        }

        public Form1()
        {
            InitializeComponent();
            drawingBitmap = new Bitmap(100, 100);
            pictureBox1.Image = drawingBitmap;
            bigBitmap = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            pictureBox2.Image = bigBitmap;
            for (int i = 0; i < 10; i++)
                for (int x = 0; x < blockCols; x++)
                    for (int y = 0; y < blockRows; y++)
                    {
                        wideHistograms[i, x, y] = new double[300];
                    }
            loadWideHistograms("NumberWideHistograms4x4.txt");
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
            bigBitmap = new Bitmap("bigBitmapForTests.bmp");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            drawingBitmap = BmpProcesser.preprocessBitmap(drawingBitmap);
            listBox1.Items.Clear();
            pictureBox1.Image = drawingBitmap;
            //List<double> dist = guess();
            List<double> dist = guessWide(drawingBitmap);
            int ID;
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }
            //clearImg();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            clearImg();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //saveHistograms("numberHistograms.txt");
            SaveFileDialog sf = new SaveFileDialog();
            if (sf.ShowDialog() == DialogResult.OK)
            {
                saveWideHistograms(sf.FileName);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //loadHistograms("numberHistograms.txt");
            OpenFileDialog of = new OpenFileDialog();
            if (of.ShowDialog() == DialogResult.OK)
            {
                loadWideHistograms(of.FileName);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            clearWideHistograms();
            learnAll(path, 1000);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            int[,] rightNwrong = guessAll(100);
            int sum = 0;
            for (int i = 0; i < 10; i++)
            {
                listBox1.Items.Add(i.ToString() + " " + rightNwrong[i, 0].ToString() + " " + rightNwrong[i, 1].ToString());
                sum += rightNwrong[i, 0];
            }
            listBox1.Items.Add(sum);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {
            clearWideHistograms();
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
            DateTime date1 = DateTime.Now;
            listBox1.Items.Add(guessWithAutoGrayScale(drawingBitmap, 10));
            DateTime date2 = DateTime.Now;
            TimeSpan ts = date2 - date1;
            textBox1.Text = (ts.Seconds * 1000 + ts.Milliseconds).ToString();
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
                pictureBox2.Refresh();
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            bigBitmap = learner.visualize();
            pictureBox2.Image = bigBitmap;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            LBPLearning LBPLearner = new LBPLearning();
            LBPLearner.loadWeights(@"weights\LBPLearning\auto\4x4kohonen nonLinearDelta 0,2.txt");
            CenterLearning CLearner = new CenterLearning();
            CLearner.loadWeights(@"weights\CenterLearning\auto\4x4kohonen nonLinearDelta 0,2.txt");
            SimpleLearning SLearner = new SimpleLearning();
            SLearner.loadWeights(@"weights\SimpleLearning\auto\defaultWeight127kohonen nonLinearDelta 0,2.txt");
            CountLearning CountLearner = new CountLearning();
            CountLearner.loadWeights(@"weights\CountLearning\Auto\16x16kohonen nonLinearDelta 0,2.txt");
        
            drawingBitmap = BmpProcesser.FromAlphaToRGB(drawingBitmap);
            drawingBitmap = BmpProcesser.normalizeBitmapRChannel(drawingBitmap, 100, 100);
            listBox1.Items.Clear();
            pictureBox1.Image = drawingBitmap;
            int ID;
            List<double> dist;

            dist = LBPLearner.guess(drawingBitmap); 
            listBox1.Items.Add("LBP 4x4 kohonen");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }

            dist = CLearner.guess(drawingBitmap);
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
            }


            dist = SLearner.guess(drawingBitmap);
            listBox1.Items.Add("Simple kohonen");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }

            dist = CountLearner.guess(drawingBitmap);
            listBox1.Items.Add("count 16x16 kohonen");
            for (int i = 0; i < 10; i++)
            {
                ID = dist.IndexOf(dist.Min());
                listBox1.Items.Add(ID.ToString() + ' ' + dist[ID].ToString());
                dist[ID] = 100000;
            }

            LBPLearner.loadWeights(@"weights\LBPLearning\auto\4x4average .txt");
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
            }

        }

        private void button17_Click(object sender, EventArgs e)
        {
            LBPLearning LBPLearner = new LBPLearning();
            LBPLearner.loadWeights(@"weights\LBPLearning\auto\4x4kohonen nonLinearDelta 0,2.txt");
            CenterLearning CLearner = new CenterLearning();
            CLearner.loadWeights(@"weights\CenterLearning\auto\4x4kohonen nonLinearDelta 0,2.txt");
            SimpleLearning SLearner = new SimpleLearning();
            SLearner.loadWeights(@"weights\SimpleLearning\auto\defaultWeight127kohonen nonLinearDelta 0,2.txt");
            CountLearning CountLearner = new CountLearning();
            CountLearner.loadWeights(@"weights\CountLearning\Auto\16x16kohonen nonLinearDelta 0,2.txt");
            

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

                        LBPLearner.loadWeights(@"weights\LBPLearning\auto\4x4average .txt");
                        CLearner.loadWeights(@"weights\CenterLearning\auto\16x16average .txt");
                        SLearner.loadWeights(@"weights\SimpleLearning\auto\defaultWeight127average .txt");
                        CountLearner.loadWeights(@"weights\CountLearning\Auto\16x16average .txt");

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
            List<List<int>> numbers = DigitsToNumbersLogic.digitsToNumbers(digits);
            foreach (List<int> number in numbers)
            {
                int counter = 0;
                int left = digits[number[0]].bounds.Left;
                int top = digits[number[0]].bounds.Top;
                foreach (int digit in number)
                {
                    using (Graphics g = Graphics.FromImage(bigBitmap))
                    {
                        g.DrawString(digits[digit].digitWithMaxVotes().ToString(), new Font("Arial", 20), new SolidBrush(Color.Red), left + counter * 20, top - 30);
                        //g.DrawString(digits[digit].possiebleDigits[1].ToString(), new Font("Arial", 20), new SolidBrush(Color.Orange), left + counter * 20, top - 50);
                        //g.DrawString(digits[digit].possiebleDigits[2].ToString(), new Font("Arial", 20), new SolidBrush(Color.Green), left + counter * 20, top - 70);
                        //g.DrawRectangle(new Pen(Color.Orange, 4), currentDigit.bounds);
                    }
                    counter++;
                }
            }
            pictureBox2.Image = bigBitmap;
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

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int count;
                using (StreamReader sw = new StreamReader(@"Tests\count.txt"))
                {
                    count = Convert.ToInt32(sw.ReadLine());
                }

                bigBitmap.Save(@"Tests\bigBitmap"+count.ToString()+".bmp");
                count++;
                using (StreamWriter sw = new StreamWriter(@"Tests\count.txt"))
                {
                    sw.Write(count);
                }
            }
        }
    }

}

