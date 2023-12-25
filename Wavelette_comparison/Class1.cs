using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace Wavelette_comparison
{
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
        public void write(List<double> A)
        {
            this.S = Vector<double>.Build.DenseOfEnumerable(A);
            this.be = 1;
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
        public void progressUpdate(double a)
        {
            this.progress += a;
        }
        public void progressnull()
        {
            this.progress=0;
        }
    }
}
