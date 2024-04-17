using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra;

namespace Wavelette_comparison
{
    public static class ColorUtils
    {
        public static Color ToMediaColor(this byte[] rgb)
        {
            return Color.FromArgb(rgb[0], rgb[1], rgb[2]);
        }
    }
    class Signal
    {
        public Vector<double> S;
        public int reza=0;
        public int rezb=0;
        Matrix<double> m;
        public double a=0;
        public double ae=0;
        public double b=0;
        public double be=0;
        public double progress = 0;
        public double x1 = 0;
        public double x2 = 0;
        public int flag = 0;


        public void write(List<double> A)
        {
            this.S = Vector<double>.Build.DenseOfEnumerable(A);
            //this.be = this.S.Count();
        }
        public void write(Matrix<double> A)
        {
            this.m = A;
        }
        public Vector<double> read()
        {
            return (this.S);
        }
        public Matrix<double> readM()
        {
            return (this.m);
        }

    }
}
