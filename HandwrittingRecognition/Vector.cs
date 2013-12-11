using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HandwrittingRecognition
{
    class Vector
    {
        public static double[] normalyzeVektor(double[] vektor, double max)
        {
            double sum = 0;

            /*double min = vektor.Min();
            for (int i = 0; i < vektor.Length; i++)
                vektor[i] -= min;*/
            for (int i = 0; i < vektor.Length; i++)
                sum += vektor[i];
            for (int i = 0; i < vektor.Length; i++)
            {
                vektor[i] = vektor[i] / sum;
                vektor[i] = Math.Round(vektor[i], 3) * max;
            }
            return vektor;
        }

        public static List<double> normalyzeVektor(List<double> vektor, double max)
        {
            return normalyzeVektor(vektor.ToArray(), max).ToList();
        }

        public static List<string> toSortedStringList(List<double> dist)
        {
            string[] possibleOptions = new string[dist.Count];
            using (StreamReader sr = new StreamReader("possible options.txt"))
            {
                for (int i = 0; i < dist.Count; i++)
                    possibleOptions[i] = sr.ReadLine();
            }

            //int counterStart = 1072;
            List<string> result = new List<string>();
            int ID;
            for (int i = 0; i < dist.Count; i++)
            {
                ID = dist.IndexOf(dist.Max());
                result.Add(possibleOptions[ID] + ' ' + dist[ID].ToString());
                dist[ID] = -100000;
            }
            return result;
        }
    }
}
