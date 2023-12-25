using MathNet.Numerics.LinearAlgebra;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using OfficeOpenXml;
using NAudio.Wave;


namespace Wavelette_comparison
{

    public partial class Form1 : Form
    {

        Signal S1 = new Signal();
        Signal S2 = new Signal();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        #region Drag&Drop
        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
            textBox1.Text = file[0];
            ProcessMP3(file[0], S1);
        }
        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;

        }
        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {
            string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
            textBox2.Text = file[0];
            ProcessMP3(file[0], S2);
        }
        private void textBox2_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;

        }
       

        #endregion
        #region Process MP3
        private void ProcessMP3(string file, Signal S)
        {
            List<double> Signal = new List<double>();

            using (Mp3FileReader mp3Reader = new Mp3FileReader(file))
            {
                // Get the number of channels and select the desired channel
                int channels = mp3Reader.WaveFormat.Channels;
                int selectedChannel = 0; // Use the first channel (0-indexed)
                //MessageBox.Show($"Channels: {mp3Reader.WaveFormat.Channels}" + Environment.NewLine + $"Sample Rate: {mp3Reader.WaveFormat.SampleRate}" + Environment.NewLine + $"Bits per Sample: {mp3Reader.WaveFormat.BitsPerSample}" + Environment.NewLine + $"Total Time: {mp3Reader.TotalTime}");
                
                // You can now read and process the audio samples from the selected channel
                int blockSize = channels * mp3Reader.WaveFormat.BitsPerSample / 8;
                byte[] buffer = new byte[blockSize * mp3Reader.WaveFormat.SampleRate];

                int bytesRead;
                while ((bytesRead = mp3Reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Process only the samples from the selected channel
                    for (int i = 0; i < bytesRead; i += channels * (mp3Reader.WaveFormat.BitsPerSample / 8))
                    {
                        // Assuming 16-bit audio, you may need to adjust based on your format
                        short sampleValue = BitConverter.ToInt16(buffer, i);
                        double normalizedSample = sampleValue / (double)short.MaxValue;

                        // Add the normalized sample to the list
                        Signal.Add(normalizedSample);

                    }
                    // MessageBox.Show(System.Convert.ToString(Signal));
                    S.write(Signal);
                }
            }
        }
        #endregion
        #region Wavelette functions
        Vector<double> HaarFun(double a, double b, double length, double step )
        {

            Vector<double> values1=null;
            for (double t = 0; t <length; t += step)
            {
                double T = (t - b) / a;
                if (T > 0 && T < 0.5)
                {
                    values1.Add(1);
                }
                else
                {
                    if (T <= 1 && T >= 0.5)
                    {
                        values1.Add(-1);
                    }
                    else
                    {
                        values1.Add(0);
                    }
                }
            }
            return (values1);

        }
        Vector<double> Mhat(double a, double b, double length)
        {
            double step = 1 / length;
            Vector<double> values = Vector<double>.Build.Dense(System.Convert.ToInt32(length));
            double value = 0;
            double T = 0;
            int i = 0;
            for (double t = 0; t < 1; t+=step)
            {
                if (i >= length) { break; }
                T = (t - b) / a;
                value = (1 - Math.Pow(T, 2)) * Math.Exp(-1 * Math.Pow(T, 2) / 2);
                values[i] = value;
                i++;
            }
            return (values);

        }
        void Transform(Signal S, double a, double b, double length)
        {
            double Bstep, Astep,A,B;
            Bstep = (S.be - S.b) / System.Convert.ToDouble(S.rezb);
            Astep = (S.ae - S.a) / System.Convert.ToDouble(S.reza);
            A = S.a;
            B = S.b;
            double value = 1;
            Matrix<double> matrix = Matrix<double>.Build.Dense(S.reza, S.rezb);
            for (int i=0; i<S.reza; i++)
            {
                A += i * Astep;
                for (int j = 0; j < S.rezb; j++)
                {
                    B += j * Bstep;
                    value=S.S.DotProduct(Mhat(A,B,(S.S.Count)));
                    matrix[i, j] = value;
                    S.progressUpdate(1/(S.reza*S.rezb));
                  
                }
                B = 0;
            }
            S.write(matrix);
            Print(S.readM());
        }
        #endregion
        #region PLOT
        private void PlotVector(Signal s1, Signal s2)
        {
            // Set up the Chart control

            chart1.Series[0].Points.Clear();

            // Add data points to the series
            for (int i = 0; i < S1.S.Count; i++)
            {
                chart1.Series[0].Points.AddY(S1.S[i]);
            }
            // Set up the Chart control


            chart2.Series[0].Points.Clear();

            // Add data points to the series
            for (int i = 0; i < S2.S.Count; i++)
            {
                chart2.Series[0].Points.AddY( S2.S[i]);
            }


        }
        #endregion
        #region Buttons and textboxes

        private void button1_Click(object sender, EventArgs e)
        {
            PlotVector(S1, S2);
            S1.reza = System.Convert.ToInt32(textBox3.Text);
            S2.reza = System.Convert.ToInt32(textBox3.Text);
            S1.rezb = System.Convert.ToInt32(textBox4.Text);
            S2.rezb = System.Convert.ToInt32(textBox4.Text);
            S1.a = System.Convert.ToDouble(textBox10.Text);
            S1.ae = System.Convert.ToDouble(textBox9.Text);
            S2.a = System.Convert.ToDouble(textBox10.Text);
            S2.ae = System.Convert.ToDouble(textBox9.Text);
        } 
        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }
        #endregion
        #region Excel export
        static void Print(Matrix<double> data)
        {
            string filePath = "output.xlsx";
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            WriteArrayToExcel(data, filePath);

            Console.WriteLine("Excel файл создан и заполнен.");
        }
        static void WriteArrayToExcel(Matrix<double> data, string filePath)
        {
            FileInfo newFile = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                string newSheetName = "Sheet" + (package.Workbook.Worksheets.Count + 1);
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(newSheetName);


                int rows = data.RowCount;
                int cols = data.ColumnCount;

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        worksheet.Cells[i + 1, j + 1].Value = data[i, j];
                    }
                }

                package.Save();
            }
        }
        #endregion
        void progress1(Signal s)
        {
            while (progressBar1.Value < 100)
            {
                progressBar1.Value = System.Convert.ToInt32(s.progress);
            }
        }
        void progress2(Signal s)
        {
            while (progressBar2.Value < 100)
            {
            progressBar2.Value = System.Convert.ToInt32(s.progress);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            double l = S1.S.Count();
            /*Thread thread1 = new Thread(()=>Transform(S1,S1.a,S1.b,S1.S.Count()));
            Thread thread2 = new Thread(() => Transform(S2, S2.a, S2.b, S2.S.Count()));
            Thread thread3 = new Thread(()=> progress1(S1));
            Thread thread4 = new Thread(() => progress2(S2));
            thread1.Start();
            thread2.Start();
            thread3.Start();
            thread4.Start();*/
            Transform(S1, S1.a, S1.b, S1.S.Count());

        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            Print(S1.readM());
        }
    }
}
