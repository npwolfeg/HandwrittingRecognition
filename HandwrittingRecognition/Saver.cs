using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace HandwrittingRecognition
{
    static class Saver
    {
        static public void saveWeights(string path,List<double> parameters, double[][]weights, int vectorLength, int optionsCount)
        {
            File.Delete(path);
            using (StreamWriter sw = new StreamWriter(path))
            {
                for(int x=0;x<parameters.Count;x++)
                    sw.WriteLine(parameters[x].ToString());
                for (int n = 0; n < optionsCount; n++)
                    for (int i = 0; i < vectorLength; i++)
                        sw.WriteLine(weights[n][i].ToString());
            }
        }

        static public void saveWeights(List<double> parameters, double[][]weights, int vectorLength, int optionsCount)
        {
            SaveFileDialog sf = new SaveFileDialog();
            if (sf.ShowDialog() == DialogResult.OK)
            {
                saveWeights(sf.FileName,parameters, weights, vectorLength, optionsCount);
            }
        }

        /*public List<double> loadWeights(string path, int parametersCount)
        {
            List<double> result = new List<double>();
            using (StreamReader sw = new StreamReader(path))
            {
                for (int x = 0; x < parametersCount; x++)
                    result.Add(Convert.ToDouble(sw.ReadLine()));
                    for (int n = 0; n < optionsCount; n++)
                        for (int i = 0; i < vectorLength; i++)
                            weights[n][i] = Convert.ToDouble(sw.ReadLine());
            }
        }

        public void loadWeights()
        {
            OpenFileDialog of = new OpenFileDialog();
            if (of.ShowDialog() == DialogResult.OK)
            {
                loadWeights(of.FileName);
            }
        }*/
    }
}
