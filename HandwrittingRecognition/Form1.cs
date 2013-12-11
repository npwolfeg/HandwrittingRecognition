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
        int drawingWidth = 3;
        Point[] points = new Point[2];
        DistanceType distanceType = DistanceType.Euclid;

        NeuralNetwork nn = new NeuralNetwork(100 * 100);

         public void clearImg()
        {
            drawingBitmap = new Bitmap(100, 100);
            BmpProcesser.fillWhite(drawingBitmap);
            pictureBox1.Image = drawingBitmap;
            bigBitmap = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            BmpProcesser.fillWhite(bigBitmap);
            pictureBox2.Image = bigBitmap;
        }


        public int guessWithAutoGrayScale(Bitmap bmp, int step)
        {
            List<Learner> learnerList = new List<Learner>();
            learnerList.Add(new LBPLearning(true));
            learnerList.Add(new CenterLearning(true));
            learnerList.Add(new SimpleLearning(true));
            learnerList.Add(new CountLearning(true));

            listBox1.Items.Clear();
            double min = 100;
            int ID = 0;
            int previousID = 0;
            Bitmap temp = new Bitmap(drawingBitmap);
            temp = BmpProcesser.FillBackGround(new Point(0, 0), temp, 6, Color.White);
            List<double> dist = new List<double>();
            List<double> distTemp;
            for (int j = 0; j < 10; j++)
                dist.Add(0);

            progressBar1.Value = 0;
            progressBar1.Maximum = 255;

            for (int i = 0; i < 255; i += step)
            {
                progressBar1.Value = i;
                temp = BmpProcesser.GrayScale(bmp, i);
                temp = BmpProcesser.normalizeBitmap(temp, 100, 100);
                //dist = guessWide(temp);

                for (int j = 0; j < 10; j++)
                    dist[j] = 0;

                //temp = BmpProcesser.renew(drawingBitmap);

                foreach (Learner learner in learnerList)
                {
                    distTemp = learner.guess(temp);
                    for (int j = 0; j < 10; j++)
                        dist[j] += distTemp[j];
                }

                /*previousID = ID;
                ID = dist.IndexOf(dist.Min());
                double currentArea = area(temp);
                listBox1.Items.Add(ID.ToString() + ' ' + i.ToString() + ' ' + dist.Min().ToString() + ' ' + currentArea.ToString());

                if (currentArea > 0.5)
                    return previousID;*/

                double currentMin = dist.Min();
                if (currentMin < min)
                {
                    min = currentMin;
                    ID = dist.IndexOf(currentMin);
                    //listBox1.Items.Add(ID.ToString() + ' ' + i.ToString() + ' ' + min.ToString());
                    //pictureBox1.Image = temp;
                }
            }
            return ID;
        }

        public Form1()
        {
            InitializeComponent();
            /*drawingBitmap = new Bitmap(100, 100);
            pictureBox1.Image = drawingBitmap;
            bigBitmap = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            pictureBox2.Image = bigBitmap;*/
            clearImg();
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
            drawingBitmap = new Bitmap(@"F:\DigitDB\PictureSaverAll\г0.bmp");
            //drawingBitmap = BmpProcesser.normalizeBitmap(drawingBitmap, 100, 100);
            //drawingBitmap = BmpProcesser.FromAlphaToRGB(drawingBitmap);
            //drawingBitmap = BmpProcesser.GrayScale(drawingBitmap,120);
            //drawingBitmap = BmpProcesser.normalizeBitmap(drawingBitmap, 100, 100);
            pictureBox1.Image = drawingBitmap;
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
            bigBitmap = BmpProcesser.GrayScale(bigBitmap);
            pictureBox2.Image = bigBitmap;            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DateTime date1 = DateTime.Now;
            guessWithAutoGrayScale(drawingBitmap, 10);
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
                pictureBox2.Image = bigBitmap;
            }
            else
            {
                try
                {
                    label1.Text = bigBitmap.GetPixel(e.X, e.Y).ToString();
                    label2.Text = e.Location.ToString();
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            /*bigBitmap = learner.visualize();
            pictureBox2.Image = bigBitmap;*/
        }

        private void button16_Click(object sender, EventArgs e)
        {
            List<Learner> learnerList = new List<Learner>();
            learnerList.Add(new LBPLearning(true));
            learnerList.Add(new CenterLearning(true));
            learnerList.Add(new SimpleLearning(true));
            learnerList.Add(new CountLearning(true));

            drawingBitmap = BmpProcesser.normalizeBitmap(drawingBitmap, 100, 100);
            listBox1.Items.Clear();
            pictureBox1.Image = drawingBitmap;

            /*foreach (Learner learner in learnerList)
            {
                List<string> stringDist = Vector.toSortedStringList(learner.guess(drawingBitmap));
                listBox1.Items.Add("kohonen");  
                for (int i=0;i<5;i++)
                    listBox1.Items.Add(stringDist[i]);
            }*/
            double[] dist = nn.calculateNet(drawingBitmap);
            /*for (int i = 0; i < 42; i++)
                listBox1.Items.Add(dist[i]);*/
            List<string> stringDist = Vector.toSortedStringList(dist.ToList());
            for (int i = 0; i < 42; i++)
                listBox1.Items.Add(stringDist[i]);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            //save bigBitmap to tests directory
            int count;
            using (StreamReader sw = new StreamReader(@"Tests\count.txt"))
            {
                count = Convert.ToInt32(sw.ReadLine());
            }
            bigBitmap.Save(@"Tests\bigBitmap" + count.ToString() + ".bmp");
            count++;
            using (StreamWriter sw = new StreamWriter(@"Tests\count.txt"))
            {
                sw.Write(count);
            }

            List<Learner> learnerList = new List<Learner>();
            learnerList.Add(new LBPLearning(true));
            learnerList.Add(new CenterLearning(true));
            learnerList.Add(new SimpleLearning(true));
            learnerList.Add(new CountLearning(true));

            List<HandwrittenDigit> digits = BmpProcesser.getDigitsList(bigBitmap, new BackgroundWorker());

            foreach(HandwrittenDigit digit in digits)
                foreach (Learner learner in learnerList)
                {
                    digit.addGuess(learner.guess(digit.bmp));

                }

            //gather digits into numbers and display them on top of the first digit in number
            Bitmap newBigBitmap = new Bitmap(bigBitmap);
            List<List<int>> numbers = DigitsToNumbersLogic.digitsToNumbers(digits);
            foreach (List<int> number in numbers)
            {
                int counter = 0;
                int left = digits[number[0]].bounds.Left;
                int top = digits[number[0]].bounds.Top;
                foreach (int digit in number)
                {
                    using (Graphics g = Graphics.FromImage(newBigBitmap))
                        g.DrawString(digits[digit].digitWithMaxVotes().ToString(), new Font("Arial", 20), new SolidBrush(Color.Red), left + counter * 20, top-30);
                        //g.DrawString(digits[digit].digitWithMaxVotes().ToString(), new Font("Arial", 20), new SolidBrush(Color.Red), digits[digit].bounds.Left, digits[digit].bounds.Top-40);
                    counter++;
                }
            }
            pictureBox2.Image = newBigBitmap;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BmpProcesser.DrawGrid(bigBitmap, 100, 100);
            pictureBox2.Image = bigBitmap;
        }

        private void bg_test_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.StackTrace);
                
            }
            else
                MessageBox.Show("Done!");
        }

        private void bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (sender1, e1) =>
            {
                List<Learner> learnerList = new List<Learner>();
                //learnerList.Add(new LBPLearning(false));
                //learnerList.Add(new CenterLearning(false));
                //learnerList.Add(new SimpleLearning(true));
                //learnerList.Add(new CountLearning(false));

                //foreach (Learner learner in learnerList)
                    //learner.RunAutoTest(bw);

                nn.backPropagation(bw, 500, 0.5, 0.1);
                nn.saveGuessNew(nn.guessAll(bw,500),"nn");
            };
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_test_RunWorkerCompleted);
            bw.ProgressChanged += new ProgressChangedEventHandler(bg_ProgressChanged);
            bw.WorkerReportsProgress = true;
            progressBar1.Value = 0;
            progressBar1.Maximum = 100;
            bw.RunWorkerAsync();
        }

        private void distanceListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Distances.DT = (DistanceType)distanceListBox.SelectedIndex;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Text = (Convert.ToInt32(textBox2.Text) + 1).ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            bigBitmap = new Bitmap("photo3.jpg");

            int scale = 8;

            Bitmap tempBigBitmap = new Bitmap("photo3.jpg");
            tempBigBitmap = BmpProcesser.normalizeBitmap(tempBigBitmap, tempBigBitmap.Width/scale, tempBigBitmap.Height/scale);
            tempBigBitmap = BmpProcesser.GrayScale(tempBigBitmap);
            tempBigBitmap = BmpProcesser.FillBackGround(new Point(0, 0), tempBigBitmap, 6, Color.White);
            tempBigBitmap = BmpProcesser.smooth(tempBigBitmap, 3, 3);
            tempBigBitmap = BmpProcesser.GrayScale(tempBigBitmap, 254);

            List<Rectangle> rects = BmpProcesser.getRectList(tempBigBitmap);

            List<int> areas = new List<int>();
            int maxHeight = 0;
            foreach (Rectangle rect in rects)
            {
                areas.Add(rect.Width * rect.Height);
                if (rect.Height>maxHeight)
                    maxHeight = rect.Height;
            }

            List<Rectangle> smallRects = new List<Rectangle>();

            int maxArea = areas.Max();
            for (int i = 0; i < rects.Count; i++)
                if (areas[i] < maxArea / 5)
                {
                    smallRects.Add(rects[i]);
                    rects.RemoveAt(i);
                    areas.RemoveAt(i);
                    i--;
                }

            foreach (Rectangle rect in rects)
            {
                Graphics g = Graphics.FromImage(tempBigBitmap);
                g.DrawRectangle(new Pen(Color.Red), rect);
            }

            for(int j=0;j<smallRects.Count;j++) // foreach smallrect
            {
                double minDist = maxHeight;
                int id = -1;
                for (int i = 0; i < rects.Count; i++) //foreach big rectangle
                    {
                        double distance = Math.Sqrt(Math.Pow(smallRects[j].X - rects[i].X,2) + Math.Pow(smallRects[j].Y - rects[i].Y,2));
                        if (distance < minDist) //if small rect is close to big, remember big
                        {
                            minDist = distance;
                            id = i;
                        }
                    }
                if (id != -1) // if any close big was found, connect them
                {
                    rects[id] = BmpProcesser.connectRects(rects[id], smallRects[j]);
                    smallRects.Remove(smallRects[j]);
                    j--;
                }
                else  // else try to connect close small rects with each other                  
                    {
                        for (int i = j + 1; i < smallRects.Count; i++)
                        {
                            double distance = Math.Sqrt(Math.Pow(smallRects[j].X - smallRects[i].X, 2) 
                                + Math.Pow(smallRects[j].Y - smallRects[i].Y, 2));
                            if (distance < minDist)
                            {
                                minDist = distance;
                                id = smallRects.IndexOf(smallRects[i]);
                            }
                        }
                        if (id != -1)
                        {
                            rects.Add(BmpProcesser.connectRects(smallRects[j], smallRects[id]));
                            smallRects.Remove(smallRects[j]);
                            smallRects.Remove(smallRects[id]);
                            j--;
                        }
                    }
            }

            foreach (Rectangle rect in rects)
            {
                Graphics g = Graphics.FromImage(tempBigBitmap);
                g.DrawRectangle(new Pen(Color.Blue), rect);
            }

            foreach (Rectangle rect in rects)
            {
                Graphics g = Graphics.FromImage(bigBitmap);                
                Rectangle newRect = new Rectangle(rect.X * scale, rect.Y * scale, rect.Width * scale, rect.Height * scale);
                drawingBitmap = BmpProcesser.copyPartOfBitmap(bigBitmap, newRect);                
                int ID = guessWithAutoGrayScale(drawingBitmap, 20);
                g.DrawString(ID.ToString(), new Font("Arial", 20), new SolidBrush(Color.Red), newRect.Left, newRect.Top);
                g.DrawRectangle(new Pen(Color.Red), newRect);
            }

            //tempBigBitmap = BmpProcesser.normalizeBitmap(tempBigBitmap, bigBitmap.Width, bigBitmap.Height);
            //bigBitmap = tempBigBitmap;
            pictureBox2.Image = bigBitmap;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            bigBitmap = BmpProcesser.smooth(bigBitmap,3,3);
            pictureBox2.Image = bigBitmap;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            /*bigBitmap = BmpProcesser.renew(bigBitmap, 1);
            pictureBox2.Image = bigBitmap;*/
            List<HandwrittenDigit> digits = BmpProcesser.getDigitsList(bigBitmap, new BackgroundWorker());
            int maxArea = 0;
            foreach (HandwrittenDigit digit in digits)
            {
                if (digit.area>maxArea)
                    maxArea = digit.area;
            }

            foreach (HandwrittenDigit digit in digits)
            {
                if (digit.area < maxArea/20)
                    foreach (Point p in digit.points)
                    {
                        bigBitmap.SetPixel(p.X, p.Y, Color.FromArgb(255, 255, 255, 255));
                    }
            }
            pictureBox2.Image = bigBitmap;

        }

        private void button9_Click(object sender, EventArgs e)
        {
            bigBitmap = BmpProcesser.FillBackGround(new Point(0, 0), bigBitmap, 6, Color.White);
            pictureBox2.Image = bigBitmap;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            bigBitmap = BmpProcesser.GrayScale(bigBitmap, 254);
            pictureBox2.Image = bigBitmap;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            /*for(int j=0;j<bigBitmap2.Height;j++)
                for(int i=0;i<bigBitmap2.Width;i++)
                    if (bigBitmap2.GetPixel(i, j).R == 0)
                    {
                        bigBitmap = BmpProcesser.FillBackGround(new Point(i, j), bigBitmap, 6, Color.Black);
                        j = bigBitmap2.Height;
                        break;
                    }*/
            bigBitmap = BmpProcesser.FillBackGround(new Point(28, 151), bigBitmap, 6, Color.Black);
            pictureBox2.Image = bigBitmap;
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            Bitmap temp = BmpProcesser.GrayScale(drawingBitmap, trackBar1.Value);
            pictureBox1.Image = temp;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            drawingBitmap = BmpProcesser.normalizeBitmap(pictureBox1.Image as Bitmap, 100, 100);
            pictureBox1.Image = drawingBitmap;
            textBox1.Text = area(drawingBitmap).ToString();
        }

        public double area(Bitmap bmp)
        {
            double result = 0;
            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                    if (bmp.GetPixel(i, j).R < 255)
                        result++;                
            return result/bmp.Width/bmp.Height;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            double[] result = new double[10];
            string path = @"F:\DigitDB\PictureSaverWhiteBackGround\";  
            int[] count = new int[10];
            Bitmap bmp;
            using (StreamReader sr = new StreamReader(path + "count.txt"))
            {
                for (int i = 0; i < 10; i++)
                    count[i] = Convert.ToInt32(sr.ReadLine());
            }
            progressBar1.Value = 0;
            progressBar1.Maximum = 126 * 10;
            for (int k = 0; k < 10; k++)
            {
                for (int n = 0; n < 126; n++)
                {
                    progressBar1.Value++;
                    bmp = new Bitmap(path + k.ToString() + n.ToString() + ".bmp");
                    bmp = BmpProcesser.normalizeBitmap(bmp, 100, 100);
                    result[k] += area(bmp);
                }
                result[k] /= 126;
                listBox1.Items.Add(k.ToString() + " " + result[k].ToString());
            }
            
        }

        private void button19_Click(object sender, EventArgs e)
        {
            //bigBitmap = new Bitmap(@"Tests\bigBitmap269.bmp");
            //pictureBox2.Height = 3200;
            pictureBox2.Image = bigBitmap;

            List<Learner> learnerList = new List<Learner>();
            learnerList.Add(new LBPLearning(true));
            learnerList.Add(new CenterLearning(true));
            learnerList.Add(new SimpleLearning(true));
            learnerList.Add(new CountLearning(true));

            List<HandwrittenDigit> digits = new List<HandwrittenDigit>();
            Bitmap newBigBitmap = new Bitmap(bigBitmap);
            int correctAnswersCount = 0;

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (sender1, e1) =>
            {
                digits = BmpProcesser.getDigitsList(bigBitmap, bw);
                foreach (HandwrittenDigit digit in digits)
                    foreach (Learner learner in learnerList)
                    {
                        digit.addGuess(learner.guess(digit.bmp));

                    }

                //gather digits into numbers and display them on top of the first digit in number                
                List<List<int>> numbers = DigitsToNumbersLogic.digitsToNumbers(digits);

                
                foreach (List<int> number in numbers)
                {
                    int counter = 0;
                    int left = digits[number[0]].bounds.Left;
                    int top = digits[number[0]].bounds.Top;
                    foreach (int digit in number)
                    {
                        using (Graphics g = Graphics.FromImage(newBigBitmap))
                            // g.DrawString(digits[digit].digitWithMaxVotes().ToString(), new Font("Arial", 20), new SolidBrush(Color.Red), left + counter * 20, top - 30);
                            g.DrawString(digits[digit].digitWithMaxVotes().ToString(), new Font("Arial", 20), new SolidBrush(Color.Red), digits[digit].bounds.Left, digits[digit].bounds.Top);
                        string str = digits[digit].digitWithMaxVotes();
                        int symbol = (int)(digits[digit].digitWithMaxVotes()[0]) - 1072;
                        if (symbol == (int)(digits[digit].bounds.Top / 100))
                            correctAnswersCount++;
                        counter++;
                    }
                }
                
            };
            bw.RunWorkerCompleted += (sender1, e1) =>
            {
                textBox1.Text = correctAnswersCount.ToString();
                pictureBox2.Image = newBigBitmap;
            };
            bw.ProgressChanged += new ProgressChangedEventHandler(bg_ProgressChanged);
            bw.WorkerReportsProgress = true;
            //progressBar1.Value = 0;
            //progressBar1.Maximum = 100;
            bw.RunWorkerAsync();

            //while (bw.IsBusy) Thread.Sleep(50) ;
          

            
        }

        private void button20_Click(object sender, EventArgs e)
        {
            for(int i=1;i<1000000;i*=10)
            listBox1.Items.Add(1 / (1 + Math.Exp(i)));
        }
    }
}

