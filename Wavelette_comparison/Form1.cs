using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra;
using NAudio.Wave;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wavelette_comparison
{
    public partial class Test : Form
    {
        Signal S1 = new Signal();
        //List<Signal> Sv = new List<Signal>();
        private Point mouseDownPoint;
        private bool isDragging = false;
        private float zoomFactor = 1.0f;
        public Test()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        #region Fourier
        static Complex[] FourierTransform1(Vector<double> inputValues)
        {
            int N = inputValues.Count;
            Complex[] result = new Complex[N];

            for (int k = 0; k < N; k++)
            {
                Complex sum = Complex.Zero;
                for (int n = 0; n < N; n++)
                {
                    double angle = 2 * Math.PI * k * n / N;
                    Complex twiddleFactor = Complex.Exp(new Complex(0, -angle));
                    sum += inputValues[n] * twiddleFactor;
                }
                result[k] = sum;
            }

            return result;
        }
        private Complex[] FourierTransform(Signal f1, Vector<double> signal, double duration)
        {

            // Преобразуем сигнал в комплексный массив
            Complex[] complexSignal = new Complex[signal.Count];
            float[] signalR = new float[signal.Count + 2];
            double[] freq = new double[signal.Count * 2];
            for (int i = 0; i < signal.Count; i++)
            {
                complexSignal[i] = new Complex(signal[i], 0);
                signalR[i] = Convert.ToSingle(f1.S[i]);
            }

            // Выполняем преобразование Фурье
            Fourier.Forward(complexSignal, FourierOptions.Matlab);
            Fourier.ForwardReal(signalR, signalR.Count() - 2, FourierOptions.Matlab);
            // Рассчитываем частоты для оси X графика
            double[] frequencyAxis = new double[signal.Count];
            double sampleRate = signal.Count / duration;
            for (int i = 0; i < signal.Count; i++)
            {
                frequencyAxis[i] = i * sampleRate / signal.Count;
            }

            // Получаем амплитуды для оси Y графика
            double[] amplitudeAxis = new double[signal.Count];
            for (int i = 0; i < signal.Count; i++)
            {
                amplitudeAxis[i] = complexSignal[i].Magnitude;
            }
            freq = Fourier.FrequencyScale(signal.Count() * 2, sampleRate);

            for (int i = 0; i < signal.Count; i++)
            {

                f1.freq.Add(freq[i]);
                f1.ampl.Add(amplitudeAxis[i]);

            }
            for (int i = 0; i < signal.Count; i++)
            {
                complexSignal[i] = new Complex(signalR[i], 0);

            }
            return (complexSignal);
        }
        #endregion
        #region Drag&Drop
        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Get the dropped files
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                int i = 0;
                // Check if the file is a .txt file
                while (i < files.Length)
                {
                    richTextBox1.Text = files[i];
                    i++;
                }
                ProcessMP3(files[0], S1);
            }
        }
        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;

        }
        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {


        }
        private void textBox2_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;

        }
        private void ProcessTextFile(string filePath, Signal S)
        {
            // Your existing code to process the .txt file goes here
            // ...

            // For example, using the code provided in the previous response
            List<double> list = new List<double>();
            string[] lines = File.ReadAllLines(filePath);
            double[] numbers = lines
                .SelectMany(line => line.Split(' ').Select(str => double.Parse(str)))
                .ToArray();
            list = numbers.ToList();
            Vector<double> vector = Vector<double>.Build.Dense(numbers);
            S.x1 = list[list.Count - 2];
            S.x2 = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            list.RemoveAt(list.Count - 2);
            S.write(list);
            S.flag = 1;
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

                        Signal.Add(sampleValue);

                    }
                    //Signal = NormalizeList(Signal);
                    S.write(Signal);
                    S.be = Signal.Count();
                    S.b = 0;
                }
                S.flag = 2;
            }
        }
        #endregion
        #region CSV process
        private void textBox2_DragDrop_1(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Get the dropped files
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                int i = 0;
                // Check if the file is a .txt file
                while (i < files.Length)
                {
                    richTextBox2.Text = files[i];
                    i++;
                }
                ReadCSV(files[0], S1);
            }
        }
        private void ReadCSV(string folderPath, Signal S)
        {
            var count = new DirectoryInfo(folderPath).GetFiles().Length.ToString();
            int Text = System.Convert.ToInt32(count);
            // Предполагаем, что в папке только один CSV файл.
            if (Text != 1)
            {
                Signal[] Sv = new Signal[Text];
                for (int i = 0; i < Text; i++)
                {
                    var filePath = Directory.GetFiles(folderPath, "*.csv")[i];
                    var lines = File.ReadAllLines(filePath);

                    // Создаем массив для хранения значений второго столбца.
                    List<double> secondColumnValues = new List<double>();// Исключаем строку заголовков
                    for (int j = 1; j < lines.Length; j++)
                    {
                        var columns = lines[j].Split(',');
                        if (columns.Length > 1 && float.TryParse(columns[4], NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
                        {
                            secondColumnValues.Add(value);
                        }
                    }
                    Sv[i] = new Signal();
                    Sv[i].write(secondColumnValues);
                    Sv[i].be = secondColumnValues.Count();
                    Sv[i].b = 0;
                    secondColumnValues = null;
                }
                //prepBitmap(Text);
                for (int i = 0; i < Text; i++)
                {
                    Array(Sv[i], i);
                }
                //"C:\Users\posei\source\repos\Wavelette_comparison\Wavelette_comparison\bin\Debug\Array"
            }
            else
            {
                var filePath = Directory.GetFiles(folderPath, "*.csv")[0];
                var lines = File.ReadAllLines(filePath);

                // Создаем массив для хранения значений второго столбца.
                List<double> secondColumnValues = new List<double>();// Исключаем строку заголовков
                for (int i = 1; i < lines.Length; i++)
                {
                    var columns = lines[i].Split(',');
                    if (columns.Length > 1 && float.TryParse(columns[4], NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
                    {
                        secondColumnValues.Add(value);
                    }
                }
                S.write(secondColumnValues);
                S.be = secondColumnValues.Count();
                S.b = 0;
            }


        }
      /*  private void prepBitmap(int i)
        {
            Bitmap[] bit = new Bitmap[i];
            for (int j = 0; j < i; j++)
            {
                bit[j].MakeTransparent;
                bit[j].Save($"C:/Users/posei/source/repos/Wavelette_comparison/Wavelette_comparison/bin/Debug/Array/output_imageA{i}.png");
            }
        }*/
        private void Array(Signal Sv, int i)
        {

            updS(Sv);
            TransformSeq(Sv, Sv.a, Sv.b, Sv.S.Count());
            draw_cor(true, $"C:/Users/posei/source/repos/Wavelette_comparison/Wavelette_comparison/bin/Debug/Array/output_imageA{i}.png");
            Console.WriteLine($"{i}");
            Sv = null;
            pictureBox1.Image = null;
        }
        #endregion
        #region Wavelette functionsParallel

        Vector<double> HaarFun(double a, double b, double length, double step)
        {

            Vector<double> values1 = null;
            for (double t = 0; t < length; t += step)
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
        Vector<double> MhatPar(double a, double b, Signal S)
        {

            Vector<double> values = Vector<double>.Build.Dense(System.Convert.ToInt32(S.S.Count));
            double T = 0;
            double t = 0;
            Parallel.For(0, System.Convert.ToInt32(S.S.Count), i =>
            {
                t = S.b + ((S.be - S.b) / (S.S.Count)) * System.Convert.ToDouble(i);
                T = (t - b) / a;
                values[System.Convert.ToInt32(i)] = Math.Abs(Math.Pow(a, (-0.5f))) * (1 - Math.Pow(T, 2)) * Math.Exp((-1 * Math.Pow(T, 2)) / 2);
            });
            return (values);

        }
        void TransformPar(Signal S, double a, double b, double length)
        {
            Stopwatch stopwatch1 = new Stopwatch();
            Stopwatch stopwatch2 = new Stopwatch();

            double Bstep, Astep, A, B;
            Bstep = (S.be - S.b) / System.Convert.ToDouble(S.rezb);
            Astep = (S.ae - S.a) / System.Convert.ToDouble(S.reza);
            A = S.a;
            B = S.b;
            double value = 1;
            Matrix<double> matrix = Matrix<double>.Build.Dense(S.reza, S.rezb);

            stopwatch1.Start();
            stopwatch2.Start();
            for (int i = 0; i < S.reza; i++)
            {

                A = A + i * Astep;
                for (int j = 0; j < S.rezb; j++)
                {
                    B = B + j * Bstep;
                    value = S.S.DotProduct(MhatPar(A, B, S));
                    matrix[i, j] = value;

                }

                this.Invoke((Action)(() =>
                {
                    progressBar1.Value = i;
                }));

            }


            S.write(matrix);


            stopwatch1.Stop();



        }

        #endregion
        #region WaveFuncSequential

        Vector<double> MhatSeq(double a, double b, double length, Signal S)
        {

            Vector<double> values = Vector<double>.Build.Dense(System.Convert.ToInt32(length));
            double T;
            double t;
            double tstep = (S.be - S.b) / length;
            for (int i = 0; i < System.Convert.ToInt32(length); i++)
            {
                t = S.b + System.Convert.ToDouble(i) * tstep;
                T = (t - b) / a;
                values[System.Convert.ToInt32(i)] = (1 - Math.Pow(T, 2)) * Math.Exp(-1 * Math.Pow(T, 2) / 2);
            }

            return (values);
        }
        void TransformSeq(Signal S, double a, double b, double length)
        {
            Stopwatch stopwatch1 = new Stopwatch();

            double Bstep, Astep, A, B;
            Bstep = (S.be - S.b) / System.Convert.ToDouble(S.rezb);
            Astep = (S.ae - S.a) / System.Convert.ToDouble(S.reza);
            A = S.a;
            B = S.b;
            double value = 1;
            Matrix<double> matrix = Matrix<double>.Build.Dense(S.reza, S.rezb);

            stopwatch1.Start();
            for (int i = 0; i < S.reza; i++)
            {

                A = S.a + i * Astep;
                for (int j = 0; j < S.rezb; j++)
                {
                    B = S.b + j * Bstep;
                    value = S.S.DotProduct(MhatSeq(A, B, (S.S.Count), S));
                    matrix[i, j] = value;

                }
                /*
                this.Invoke((Action)(() =>
                {
                    progressBar1.Value = i;
                }));*/

            }

            S.write(matrix);
            try
            {
                pictureBox1.Image.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            GenerateAndSaveImage(S.readM(), "output_imageA.png");
            //Dispose();
            pictureBox1.Image = Image.FromFile("output_imageA.png"); pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            //ExportMatrixToFile(S.readM(), "output1.txt");
            stopwatch1.Stop();
            matrix = null;
            stopwatch1 = null;
        }
        #endregion
        #region PLOT
        private void PlotVector(Signal s1)
        {
            // Set up the Chart control
            if (S1.flag == 1) { chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline; }
            else { chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine; }

            chart1.Series[0].Points.Clear();

            // Add data points to the series
            for (int i = 0; i < S1.S.Count; i++)
            {
                chart1.Series[0].Points.AddY(S1.S[i]);
            }
            // Set up the Chart contro


        }
        #endregion
        #region Buttons and textboxes

        private void button1_Click(object sender, EventArgs e)
        {
            try { chart1.Series[0].Points.Clear(); chart2.Series[0].Points.Clear(); }
            catch { }
            double time = System.Convert.ToDouble(textBox15.Text);
            if (tabControl1.SelectedTab == tabPage1)
            {
                int N = S1.S.Count;
                Complex[] result = new Complex[N];
                result = FourierTransform(S1, S1.S, time);
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine;
                for (int i = 0; i < S1.S.Count; i++)
                {
                    chart1.Series[0].Points.AddXY(1 / S1.S.Count * i, S1.S[i]);
                    chart2.Series[0].Points.AddXY(S1.freq[i], result[i].Magnitude);

                }
                S1.freq.Clear();

                updS(S1);
            }
            else
            {
                if (tabControl1.SelectedTab == tabPage2)
                {
                    Generate(S1);
                    int N = S1.S.Count;
                    Complex[] result = new Complex[N];
                    result = FourierTransform(S1, S1.S, time);
                    chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                    S1.b = System.Convert.ToDouble(Range1.Text);
                    S1.be = System.Convert.ToDouble(Range2.Text);
                    double step1 = time / S1.S.Count;
                    for (int i = 0; i < S1.S.Count; i++)
                    {
                        chart1.Series[0].Points.AddXY(i * step1, S1.S[i]);
                        chart2.Series[0].Points.AddXY(S1.freq[i], result[i].Magnitude);
                        chart2.ChartAreas[0].AxisX.Title = "Herz";
                        chart2.ChartAreas[0].AxisY.Title = "Amplitude";
                        chart1.ChartAreas[0].AxisX.Title = "Time (seconds)";
                        chart1.ChartAreas[0].AxisY.Title = "Amplitude";

                    }
                    S1.freq.Clear();
                    updS(S1);
                }
                else
                {
                    if (tabControl1.SelectedTab == tabPage3)
                    {
                        int N = S1.S.Count;
                        Complex[] result = new Complex[N];
                        result = FourierTransform(S1, S1.S, time);
                        chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                        double step1 = time / S1.S.Count;
                        for (int i = 0; i < S1.S.Count; i++)
                        {
                            chart1.Series[0].Points.AddXY(i * step1, S1.S[i]);
                            chart2.Series[0].Points.AddXY(S1.freq[i], result[i].Magnitude);
                            chart2.ChartAreas[0].AxisX.Title = "Herz";
                            chart2.ChartAreas[0].AxisY.Title = "Amplitude";
                            chart1.ChartAreas[0].AxisX.Title = "Time (seconds)";
                            chart1.ChartAreas[0].AxisY.Title = "Amplitude";
                        }
                        S1.freq.Clear();

                        updS(S1);
                    }
                    else
                    {
                        Random rnd = new Random();
                        double rand;
                        List<double> rand1 = new List<double>();

                        for (int i = 0; i < System.Convert.ToInt32(RanSampCount.Text); i++)
                        {
                            rand = rnd.NextDouble() + rnd.Next(System.Convert.ToInt32(RanRange1.Text), System.Convert.ToInt32(RanRange2.Text));
                            rand1.Add(rand);
                        }
                        S1.write(rand1);
                        textBox4.Text = System.Convert.ToString(S1.S.Count);
                        int N = S1.S.Count;
                        Complex[] result = new Complex[N];
                        result = FourierTransform(S1, S1.S, 1);
                        chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                        S1.b = System.Convert.ToDouble(Range1.Text);
                        S1.be = System.Convert.ToDouble(Range2.Text);
                        double step = (Math.Abs((System.Convert.ToDouble(Range2.Text) - System.Convert.ToDouble(Range1.Text)))) / System.Convert.ToDouble(resolution.Text);
                        for (int i = 0; i < S1.S.Count; i++)
                        {
                            chart1.Series[0].Points.AddXY(System.Convert.ToDouble(Range1.Text) + step * i, S1.S[i]);
                            chart2.Series[0].Points.AddXY(System.Convert.ToDouble(Range1.Text) + step * i, result[i].Magnitude);

                        }
                        S1.freq.Clear();

                        updS(S1);
                    }
                }

            }

        }
        private void Plot2_Click(object sender, EventArgs e)
        {

        }
        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }
        private void updS(Signal S)
        {
            S.reza = System.Convert.ToInt32(textBox3.Text);
            // S2.reza = System.Convert.ToInt32(textBox3.Text);
            S.rezb = System.Convert.ToInt32(textBox4.Text);
            // S2.rezb = System.Convert.ToInt32(textBox4.Text);
            S.a = System.Convert.ToDouble(textBox10.Text);
            S.ae = System.Convert.ToDouble(textBox9.Text);
            // S2.a = System.Convert.ToDouble(textBox10.Text);
            //S2.ae = System.Convert.ToDouble(textBox9.Text);
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            updS(S1);

            progressBar1.Maximum = S1.reza - 1;

            if (comboBox1.SelectedIndex == 0)
            {
                Task thread1 = Task.Run(() => TransformSeq(S1, S1.a, S1.b, S1.S.Count()));
                thread1.ContinueWith(task => button1.PerformClick(), TaskScheduler.FromCurrentSynchronizationContext());



            }
            else
            {

                Thread thread1 = new Thread(() => TransformPar(S1, S1.a, S1.b, S1.S.Count()));

                thread1.Name = "name1";

                thread1.Start();

            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Image.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            GenerateAndSaveImage(S1.readM(), "output_imageA.png");
            pictureBox1.Image = Image.FromFile("output_imageA.png"); pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            draw_cor(false, "a");
            pictureBox1.Image.Save("output_imageA.png");

        }
        private void button3_Click(object sender, EventArgs e)
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
                        worksheet.Cells[i + 1, j + 1].Value = 100000 * data[i, j];
                    }
                }

                package.Save();
            }
        }
        #endregion
        #region ColorMapping
        static Matrix<double> NormalizeMatrix(Matrix<double> matrix)
        {
            /*
            double max = matrix.Enumerate().Max();
            double min = double.MaxValue;
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    // Check if the current element is not NaN and less than the current minimum
                    if (!double.IsNaN(matrix[i, j]) && matrix[i, j] < min)
                    {
                        min = matrix[i, j]; // Update the minimum value
                    }
                }
            }
            matrix.MapInplace(x => (x - min) / (max - min));*/
            int rows = matrix.RowCount;
            int cols = matrix.ColumnCount;
            double max = matrix.Enumerate().Max();
            double min = double.MaxValue;
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    // Check if the current element is not NaN and less than the current minimum
                    if (!double.IsNaN(matrix[i, j]) && matrix[i, j] < min)
                    {
                        min = matrix[i, j]; // Update the minimum value
                    }
                }
            }
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (double.IsPositiveInfinity(matrix[i, j]))
                    {
                        matrix[i, j] = max;
                    }
                    if (double.IsNegativeInfinity(matrix[i, j]))
                    {
                        matrix[i, j] = min;

                    }
                    if (double.IsNaN(matrix[i, j]))
                    {
                        matrix[i, j] = 0;
                    }
                    matrix[i, j] = Math.Abs(matrix[i, j]);
                    matrix[i, j] += 10e-8;
                    matrix[i, j] = Math.Log10(matrix[i, j]); // Using log base 10
                }
            }
            return (matrix);
            matrix = null;
        }

        static void GenerateAndSaveImage(Matrix<double> matrix, string filePath)
        {

            //matrix = NormalizeMatrix(matrix);
            double max = matrix.Enumerate().Max();
            double min = double.MaxValue;
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    // Check if the current element is not NaN and less than the current minimum
                    if (!double.IsNaN(matrix[i, j]) && matrix[i, j] < min)
                    {
                        min = matrix[i, j]; // Update the minimum value
                    }
                }
            }
            var cmap1 = new SciColorMaps.Portable.ColorMap("jet", min, max);
            // Create a new Bitmap to draw the matrix
            Bitmap bitmap = new Bitmap((int)(matrix.ColumnCount), (int)(matrix.RowCount));
            // Map matrix values to colors and set pixels in the Bitmap
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    double value = matrix[i, j];
                    //Color color = MapValueToColor(value);
                    Color color = cmap1[value].ToMediaColor();
                    bitmap.SetPixel(j, i, color);
                }
            }


            // Save the Bitmap as an image file
            try
            {
                bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            bitmap = null;
            matrix = null;
        }

        static Color MapValueToColor(double value)
        {
            // Example mapping from blue (low values) to red (high values)
            int blueComponent = (int)(255 * (1 - value));
            int redComponent = (int)(255 * value);

            // Ensure color components are within valid range
            blueComponent = Math.Max(0, Math.Min(255, blueComponent));
            redComponent = Math.Max(0, Math.Min(255, redComponent));

            return Color.FromArgb(blueComponent, 0, redComponent);
        }
        #endregion
        #region pictureboxes
        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            // Zoom in and out using the mouse wheel
            const float zoomSpeed = 0.2f;

            if (e.Delta > 0)
                zoomFactor += zoomSpeed;
            else
                zoomFactor -= zoomSpeed;

            ApplyZoomAndPosition();
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // Start dragging when the left mouse button is pressed
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                mouseDownPoint = e.Location;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            // Translate the image when dragging
            if (isDragging)
            {
                int deltaX = e.X - mouseDownPoint.X;
                int deltaY = e.Y - mouseDownPoint.Y;

                pictureBox1.Location = new Point(pictureBox1.Location.X + deltaX, pictureBox1.Location.Y + deltaY);
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            // Stop dragging when the left mouse button is released
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }
        private void pictureBox2_MouseWheel(object sender, MouseEventArgs e)
        {
            // Zoom in and out using the mouse wheel
            const float zoomSpeed = 0.2f;

            if (e.Delta > 0)
                zoomFactor += zoomSpeed;
            else
                zoomFactor -= zoomSpeed;

            ApplyZoomAndPosition();
        }
        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            // Start dragging when the left mouse button is pressed
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                mouseDownPoint = e.Location;
            }
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            // Translate the image when dragging
            if (isDragging)
            {
                int deltaX = e.X - mouseDownPoint.X;
                int deltaY = e.Y - mouseDownPoint.Y;

                pictureBox1.Location = new Point(pictureBox1.Location.X + deltaX, pictureBox1.Location.Y + deltaY);
            }
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            // Stop dragging when the left mouse button is released
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }
        private void ApplyZoomAndPosition()
        {/*
            // Apply zoom and position adjustments
            int newWidth = (int)(originalImage.Width * zoomFactor);
            int newHeight = (int)(originalImage.Height * zoomFactor);

            pictureBox1.Size = new Size(newWidth, newHeight);

            int newX = (int)(mouseDownPoint.X - zoomFactor * mouseDownPoint.X);
            int newY = (int)(mouseDownPoint.Y - zoomFactor * mouseDownPoint.Y);

            pictureBox1.Location = new Point(newX, newY);*/
        }

        private void ResetZoomAndPosition()
        {/*
            // Reset zoom factor and position
            zoomFactor = 1.0f;
            pictureBox1.Size = new Size(originalImage.Width, originalImage.Height);
            pictureBox1.Location = new Point(0, 0);*/
        }

        #endregion
        #region Func_gen
        private void Generate(Signal export)
        {
            Mathos.Parser.MathParser parser = new Mathos.Parser.MathParser();
            string expr = input.Text; // the expression
            double samples = System.Convert.ToDouble(resolution.Text);
            Vector<double> Value = Vector<double>.Build.Dense(System.Convert.ToInt32(samples));
            double x = System.Convert.ToDouble(Range1.Text);
            for (int i = 0; i < System.Convert.ToInt32(samples); i++)
            {
                parser.LocalVariables.Add("x", x);
                Value[i] = parser.Parse(expr);
                x += (System.Convert.ToDouble(Range2.Text) - System.Convert.ToDouble(Range1.Text)) / samples;
                parser.LocalVariables.Clear();
            }
            export.S = Value;

        }
        #endregion
        #region MISC
        public static List<double> NormalizeList(List<double> values)
        {
            // Find the minimum and maximum values in the list
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            foreach (double value in values)
            {
                if (value < minValue)
                    minValue = value;

                if (value > maxValue)
                    maxValue = value;
            }

            // Normalize each value to the range [0, 1]
            List<double> normalizedValues = new List<double>();

            foreach (double value in values)
            {
                double normalizedValue = (value - minValue) / (maxValue - minValue);
                normalizedValues.Add(normalizedValue);
            }

            return normalizedValues;
        }
        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void Range1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Range2_TextChanged(object sender, EventArgs e)
        {

        }
        private void Distribution_Click(object sender, EventArgs e)
        {

        }
        #endregion
        #region export to txt
        public static void ExportMatrixToFile(Matrix<double> matrix, string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    int rows = matrix.RowCount;
                    int columns = matrix.ColumnCount;

                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < columns; j++)
                        {
                            if (!double.IsNaN(matrix[i, j]))
                            {
                                writer.Write(matrix[i, j]);
                                //if (i != rows - 1)
                                //    writer.Write("|");
                            }
                            else
                            {
                                writer.Write("0,0");
                                //if (i != rows - 1)
                                //  writer.Write("|");

                            }
                            if (i * j != (rows - 1) * (columns - 1)) { writer.Write("|"); }

                        }

                        // writer.WriteLine(); // Move to the next line for the next row
                    }
                }

                Console.WriteLine($"Matrix exported to {filePath} successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        #endregion

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void textBox15_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox14_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {

        }

        void draw_cor(bool a, string path)
        {
            if (a)
            {
                int w = pictureBox1.ClientSize.Width / 2;
                int h = pictureBox1.ClientSize.Height / 2;
                Refresh();
                Bitmap bitmap1 = new Bitmap(w * 2, h * 2);
                Bitmap bitmap = new Bitmap(w * 2, h * 2);
                var rect = new Rectangle(0, 0, w * 2, h * 2);
                pictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                pictureBox1.DrawToBitmap(bitmap1, rect);
                Graphics e = Graphics.FromImage(bitmap1);
                e.TranslateTransform(0, h * 2);
                int StepX = System.Convert.ToInt32(w * 2 / 10);
                int StepY = System.Convert.ToInt32(h * 2 / 10);
                DrawXYAxis(new Point(0, 0), new Point(w * 2, 2 * h), e, StepX, StepY);
                bitmap1.Save(path);
                pictureBox1.Image = bitmap1;
                bitmap = null;
                bitmap1 = null;
            }
            else
            {
                int w = pictureBox1.ClientSize.Width / 2;
                int h = pictureBox1.ClientSize.Height / 2;
                Refresh();
                Bitmap bitmap1 = new Bitmap(w * 2, h * 2);
                Bitmap bitmap = new Bitmap(w * 2, h * 2);
                var rect = new Rectangle(0, 0, w * 2, h * 2);
                pictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                pictureBox1.DrawToBitmap(bitmap1, rect);
                Graphics e = Graphics.FromImage(bitmap1);
                e.TranslateTransform(0, h * 2);
                int StepX = System.Convert.ToInt32(w * 2 / 10);
                int StepY = System.Convert.ToInt32(h * 2 / 10);
                DrawXYAxis(new Point(0, 0), new Point(w * 2, 2 * h), e, StepX, StepY);
                bitmap1.Save("output_imageA2.png");
                pictureBox1.Image = bitmap1;

            }

        }
        private void DrawXYAxis(Point start, Point end, Graphics g, int StepX, int StepY)
        {
            int Step = StepX;
            double time = System.Convert.ToDouble(textBox15.Text) / 10;
            double scale = System.Convert.ToDouble(textBox9.Text) / 10;

            int j = 1;
            for (int i = Step; i < end.X; i += Step)
            {
                g.DrawLine(Pens.Black, i, -5, i, 5);
                DrawText(new Point(i, -30), (j * time).ToString(), g, false);
                j++;
            }
            j = 1;
            for (int i = StepY; i < end.Y; i += StepY)
            {
                g.DrawLine(Pens.Black, -5, -i, 5, -i);
                DrawText(new Point(30, -i), ((j * scale).ToString("0.###E+0", CultureInfo.InvariantCulture)), g, false);
                j++;
            }
            DrawText(new Point(50, -end.Y / 2 - 10), "a", g, false);
            DrawText(new Point(end.X / 2, -50), "Time(seconds)", g, false);

            g.DrawLine(Pens.Black, start, end);
            g.DrawString("X", new Font(Font.FontFamily, 10, FontStyle.Bold), Brushes.Black, new Point(end.X - 15, end.Y));

        }

        private void DrawText(Point point, string text, Graphics g, bool isYAxis)
        {
            var f = new Font(Font.FontFamily, 12);
            var size = g.MeasureString(text, f);
            var pt = isYAxis
                ? new PointF(point.X + 1, point.Y - size.Height / 2)
                : new PointF(point.X - size.Width / 2, point.Y + 1);
            var rect = new RectangleF(pt, size);
            g.DrawString(text, f, Brushes.Black, rect);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {


            draw_cor(false, "p");

        }

        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {

        }
    }
}
