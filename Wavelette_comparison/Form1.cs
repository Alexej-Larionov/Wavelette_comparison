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
        private Point mouseDownPoint;
        private bool isDragging = false;
        private float zoomFactor = 1.0f;
        private Image originalImage;
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
        Vector<double> Mhat(double a, double b, double length)
        {
            //double step = 1 / length;
            Vector<double> values = Vector<double>.Build.Dense(System.Convert.ToInt32(length));
            double T = 0;
            double t = 0;
            Parallel.For(0, System.Convert.ToInt32(length), i =>
            {
                t = System.Convert.ToDouble(i);// * step;
                T = (t - b) / a;
                values[System.Convert.ToInt32(i)] = (1 - Math.Pow(T, 2)) * Math.Exp(-1 * Math.Pow(T, 2) / 2);
            });
            return (values);

        }
        void Transform(Signal S, double a, double b, double length)
        {
            
            double Bstep, Astep, A, B;
            Bstep = (S.be - S.b) / System.Convert.ToDouble(S.rezb);
            Astep = (S.ae - S.a) / System.Convert.ToDouble(S.reza);
            A = S.a;
            B = S.b;
            double value = 1;
            Matrix<double> matrix = Matrix<double>.Build.Dense(S.reza, S.rezb);
            for (int i = 0; i < S.reza; i++)
            {
               
                A = i * Astep;
                for (int j = 0; j < S.rezb; j++)
                {
                    B = j * Bstep;
                    value = S.S.DotProduct(Mhat(A, B, (S.S.Count)));
                    matrix[i, j] = value;
                
                }
                if (Thread.CurrentThread.Name == "name1")
                {
                    this.Invoke((Action)(() =>
                    {
                        progressBar1.Value = i;
                    }));
                }
                else 
                {
                    this.Invoke((Action)(() =>
                    {
                        progressBar2.Value = i;
                    }));

                }
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
                chart2.Series[0].Points.AddY(S2.S[i]);
            }


        }
        #endregion
        #region Buttons and textboxes

        private void button1_Click(object sender, EventArgs e)
        {
            PlotVector(S1, S2);
            updS(S1);
            updS(S2);
        }
        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }
        private void updS(Signal S)
        {
            S1.reza = System.Convert.ToInt32(textBox3.Text);
            S2.reza = System.Convert.ToInt32(textBox3.Text);
            S1.rezb = System.Convert.ToInt32(textBox4.Text);
            S2.rezb = System.Convert.ToInt32(textBox4.Text);
            S1.a = System.Convert.ToDouble(textBox10.Text);
            S1.ae = System.Convert.ToDouble(textBox9.Text);
            S2.a = System.Convert.ToDouble(textBox10.Text);
            S2.ae = System.Convert.ToDouble(textBox9.Text);
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            ColoredProgressBar bar1 = new ColoredProgressBar();
            bar1.Maximum = 100;
            ColoredProgressBar bar2 = new ColoredProgressBar();
            bar2.Maximum = 100;


            updS(S1);
            updS(S2);
            Thread thread1 = new Thread(() => Transform(S1, S1.a, S1.b, S1.S.Count()));
            Thread thread2 = new Thread(() => Transform(S2, S2.a, S2.b, S2.S.Count()));
            thread1.Name ="name1";
            thread2.Name ="name2";
            thread1.Start();
            thread2.Start();
            


        }
        private void button4_Click(object sender, EventArgs e)
        {
            GenerateAndSaveImage(S1.readM(), "output_imageA.png");
            GenerateAndSaveImage(S2.readM(), "output_imageB.png");
            pictureBox1.Image = Image.FromFile("output_imageA.png"); pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.Image = Image.FromFile("output_imageB.png"); pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;


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
            double min = matrix.Enumerate().Min();
            double max = matrix.Enumerate().Max();

            matrix.MapInplace(x => (x - min) / (max - min));
            return (matrix);
        }
        static void GenerateAndSaveImage(Matrix<double> matrix, string filePath)
        {
            double zoomFactor = 1;
            // Create a new Bitmap to draw the matrix
            Bitmap bitmap = new Bitmap((int)(matrix.ColumnCount * 20 * zoomFactor), (int)(matrix.RowCount * 20 * zoomFactor));

            // Map matrix values to colors and set pixels in the Bitmap
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    double value = matrix[i, j];
                    Color color = MapValueToColor(value);

                    for (int x = 0; x < 20 * zoomFactor; x++) // Adjust pixel size for better visibility
                    {
                        for (int y = 0; y < 20 * zoomFactor; y++) // Adjust pixel size for better visibility
                        {
                            int pixelX = (int)(j * 20 * zoomFactor + x); // Adjust pixel size for better visibility
                            int pixelY = (int)(i * 20 * zoomFactor + y); // Adjust pixel size for better visibility

                            bitmap.SetPixel(pixelX, pixelY, color);
                        }
                    }
                }
            }

            // Save the Bitmap as an image file
            bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

            Console.WriteLine($"Image saved to: {filePath}");
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
        #region pictureboxex
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
        {
            // Apply zoom and position adjustments
            int newWidth = (int)(originalImage.Width * zoomFactor);
            int newHeight = (int)(originalImage.Height * zoomFactor);

            pictureBox1.Size = new Size(newWidth, newHeight);

            int newX = (int)(mouseDownPoint.X - zoomFactor * mouseDownPoint.X);
            int newY = (int)(mouseDownPoint.Y - zoomFactor * mouseDownPoint.Y);

            pictureBox1.Location = new Point(newX, newY);
        }

        private void ResetZoomAndPosition()
        {
            // Reset zoom factor and position
            zoomFactor = 1.0f;
            pictureBox1.Size = new Size(originalImage.Width, originalImage.Height);
            pictureBox1.Location = new Point(0, 0);
        }

        #endregion

    }
}
