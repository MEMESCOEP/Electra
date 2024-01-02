using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electra
{
    public class MathUtilities
    {
        public static double GetPercentageOfNumber(double Number, double Percentage)
        {
            //Math.Round((48 * (WindowDPI[1] * 100)) / 100)
            return Math.Round((Number * Percentage) / 100);
        }
    }
}
