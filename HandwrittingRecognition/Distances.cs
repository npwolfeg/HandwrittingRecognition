using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandwrittingRecognition
{

    enum DistanceType { Euclid, KullbackLeibler };

    static class Distances
    {
        /*static public double EuclidDistance(double[] vector1, double[] vector2)
        {
            double result = 0;
            if (vector1.Length == vector2.Length)
            for (int i = 0; i < vector1.Length; i++)
                result += Math.Pow(vector1[i] - vector2[i], 2);
            return Math.Sqrt(result);
        }

        static public double KullbackLeiblerDistance(double[] vector1, double[] vector2)
        {
            double result = 0;
            if (vector1.Length == vector2.Length)
                for (int i = 0; i < vector1.Length; i++)
                {
                    double temp = Math.Log(vector1[i] / vector2[i])*(vector1[i]-vector2[i]);
                    if (vector2[i] != 0 && vector1[i] != 0)
                    if (!double.IsNaN(temp))
                        result += temp;
                }
            return result;
        }*/

        static public DistanceType DT
        {
            get;
            set;
        }

        static Distances()
        {
            DT = DistanceType.Euclid;
        }

        static public double Distance(double[] vector1, double[] vector2)
        {
            double result = 0;
            if (vector1.Length == vector2.Length)
                switch (DT)
                {
                    case DistanceType.Euclid:
                        for (int i = 0; i < vector1.Length; i++)
                            result += Math.Pow(vector1[i] - vector2[i], 2);
                        result = Math.Sqrt(result);
                        break;
                    case DistanceType.KullbackLeibler:
                        for (int i = 0; i < vector1.Length; i++)
                        {
                            double temp = Math.Log(vector1[i] / vector2[i]) * (vector1[i] - vector2[i]);
                            if (vector2[i] != 0 && vector1[i] != 0)
                                if (!double.IsNaN(temp))
                                    result += temp;
                        }
                        break;
                }                
            return result;
        }
    }
}
