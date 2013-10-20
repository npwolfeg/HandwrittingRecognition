using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HandwrittingRecognition
{
    interface ILearner
    {
        int getVectorLength(List<double> parameters);
        void loadDefault(bool average);
        List<double> guess(Bitmap bmp);
    }
}
