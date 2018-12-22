using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace eltech_math_app
{
    public partial class Form1 : Form
    {
        private DataTable dt = new DataTable()
        {
            Columns =
            {
                new DataColumn("x"),
                new DataColumn("y"),
            }
        };

        private string xLabel = "x";
        private string yLabel = "y";

        private string format = "N";

        private List<double> xValues;
        private List<double> yValues;

        private List<double> f1Values;
        private List<double> f2Values;
        private List<double> f3Values;
        private List<double> f4Values;
        private List<double> f5Values;
        private List<double> f6Values;

        private bool show1 = true;
        private bool show2 = false;
        private bool show3 = false;
        private bool show4 = false;
        private bool show5 = false;
        private bool show6 = false;

        private LineSeries f1Series = new LineSeries();
        private LineSeries f2Series = new LineSeries();
        private LineSeries f3Series = new LineSeries();
        private LineSeries f4Series = new LineSeries();
        private LineSeries f5Series = new LineSeries();
        private LineSeries f6Series = new LineSeries();

        private ArrayList t1 = new ArrayList();
        private ArrayList t2 = new ArrayList();
        private ArrayList t3 = new ArrayList();
        private ArrayList t4 = new ArrayList();
        private ArrayList t5 = new ArrayList();
        private ArrayList t6 = new ArrayList();

        // 0 - линейная
        // 1 - 2 степень

        public Form1()
        {
            InitializeComponent();
            dataGridView1.DataSource = dt;
        }

        private void calculateEverything()
        {
            DataTable dt = (DataTable) dataGridView1.DataSource;
            xValues = dt.AsEnumerable().Select(x => Convert.ToDouble(x["x"])).ToList();
            yValues = dt.AsEnumerable().Select(x => Convert.ToDouble(x["y"])).ToList();

            var chartValues =
                new ChartValues<ObservablePoint>(xValues.Select((x, i) => new ObservablePoint(x, yValues[i])));

            cartesianChart1.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Fill = Brushes.Transparent,
                    StrokeThickness = 4,
                    PointGeometrySize = 15,
                    Values = chartValues,
                }
            };

            cartesianChart1.LegendLocation = LegendLocation.Bottom;

            calculateLinear();
            calculateParab();
            calculateParab2();
            calculateHyperb();
            calculate3();
            calculate4();

            updateGraphAppearance();
            updateTable();
        }

        private void calculateHyperb()
        {
            var count = (double) xValues.Count;

            var ySum = yValues.Sum();

            var overXSum = xValues.Sum(x => 1 / x);

            var b = (yValues.Select((y, i) => y / xValues[i]).Sum() - 1 / count * ySum * overXSum)
                    / (xValues.Sum(x => 1 / Math.Pow(x, 2)) - 1 / count * Math.Pow(overXSum, 2));
            var a = 1 / count * (ySum - b * overXSum);

            f3Values = xValues.Select(x => a + b / x).ToList();

            var yDiff = yValues.Select((y, i) => Math.Pow(y - f3Values[i], 2)).ToList();
            var yDiffSum = yDiff.Sum();

            var dOst = yDiffSum / count;

            var r = Math.Sqrt(1 - yDiffSum / (yValues.Sum(y => Math.Pow(y - yValues.Average(), 2))));
            var R = Math.Pow(r, 2);

            var avrgValues = xValues.Select((x, i) => (Math.Abs((yValues[i] - f3Values[i]) / yValues[i]) * 100))
                .ToList();

            var A = 1 / count * avrgValues.Sum();

            var f1 = R * (count - 2) / (1 - R);

            var chartX = new List<double>();
            var chartY = new List<double>();

            for (var i = xValues.Min(); i <= xValues.Max(); i += (xValues.Max() - xValues.Min()) / 100)
            {
                chartX.Add(i);
                chartY.Add(a + b / i);
            }

            var chartValues =
                new ChartValues<ObservablePoint>(chartX.Select((x, i) => new ObservablePoint(x, chartY[i])));

            var addAfter = false;
            if (cartesianChart1.Series.Contains(f3Series))
            {
                cartesianChart1.Series.Remove(f3Series);
                addAfter = true;
            }

            f3Series = new LineSeries();
            f3Series.Stroke = Brushes.Aquamarine;
            f3Series.Fill = Brushes.Transparent;
            f3Series.PointGeometry = null;
            f3Series.Title = "Гиперболическая функция";
            if (f3Series.Values != null)
                f3Series.Values.Clear();
            f3Series.Values = chartValues;

            if (addAfter)
            {
                cartesianChart1.Series.Add(f3Series);
            }

            t3.Clear();
            t3.Add($"y = {a.ToString(format)} + {b.ToString(format)} / x");
            t3.Add(dOst.ToString(format));
            t3.Add(r.ToString(format));
            t3.Add(R.ToString(format));
            t3.Add(A.ToString(format));
            t3.Add(f1.ToString(format));
        }

        private void calculateParab()
        {
            var count = (double) xValues.Count;
            var p = Polyfit(xValues.ToArray(), yValues.ToArray(), 2);

            var a = p[0];
            var b = p[1];
            var c = p[2];

            f2Values = xValues.Select(x => a + b * x + c * x * x).ToList();

            var yDiff = yValues.Select((y, i) => Math.Pow(y - f2Values[i], 2)).ToList();
            var yDiffSum = yDiff.Sum();

            var dOst = yDiffSum / count;

            var r = Math.Sqrt(1 - yDiffSum / (yValues.Sum(y => Math.Pow(y - yValues.Average(), 2))));
            var R = Math.Pow(r, 2);

            var avrgValues = xValues.Select((x, i) => (Math.Abs((yValues[i] - f2Values[i]) / yValues[i]) * 100))
                .ToList();

            var A = 1 / count * avrgValues.Sum();

            var f1 = R * (count - 3) / (1 - R) / 2;

            var chartX = new List<double>();
            var chartY = new List<double>();

            for (var x = xValues.Min(); x <= xValues.Max(); x += (xValues.Max() - xValues.Min()) / 100)
            {
                chartX.Add(x);
                chartY.Add(a + b * x + c * x * x);
            }

            var chartValues =
                new ChartValues<ObservablePoint>(chartX.Select((x, i) => new ObservablePoint(x, chartY[i])));

            var addAfter = false;
            if (cartesianChart1.Series.Contains(f2Series))
            {
                cartesianChart1.Series.Remove(f2Series);
                addAfter = true;
            }

            f2Series = new LineSeries();
            f2Series.Stroke = Brushes.Orange;
            f2Series.Fill = Brushes.Transparent;
            f2Series.PointGeometry = null;
            f2Series.Title = "Парабола 2 степени";
            if (f2Series.Values != null)
                f2Series.Values.Clear();
            f2Series.Values = chartValues;

            if (addAfter)
            {
                cartesianChart1.Series.Add(f2Series);
            }

            t2.Clear();
            t2.Add($"y = {a.ToString(format)} + {b.ToString(format)}x + {c.ToString(format)}x^2");
            t2.Add(dOst.ToString(format));
            t2.Add(r.ToString(format));
            t2.Add(R.ToString(format));
            t2.Add(A.ToString(format));
            t2.Add(f1.ToString(format));
        }

        private void calculateParab2()
        {
            var count = (double) xValues.Count;
            var p = Polyfit(xValues.ToArray(), yValues.ToArray(), 3);

            var a = p[0];
            var b = p[1];
            var c = p[2];
            var d = p[3];

            f4Values = xValues.Select(x => a + b * x + c * x * x + d * x * x * x).ToList();

            var yDiff = yValues.Select((y, i) => Math.Pow(y - f4Values[i], 2)).ToList();
            var yDiffSum = yDiff.Sum();

            var dOst = yDiffSum / count;

            var r = Math.Sqrt(1 - yDiffSum / (yValues.Sum(y => Math.Pow(y - yValues.Average(), 2))));
            var R = Math.Pow(r, 2);

            var avrgValues = xValues.Select((x, i) => (Math.Abs((yValues[i] - f4Values[i]) / yValues[i]) * 100))
                .ToList();

            var A = 1 / count * avrgValues.Sum();

            var f1 = R * (count - 3) / (1 - R) / 2;

            var chartX = new List<double>();
            var chartY = new List<double>();

            for (var x = xValues.Min(); x <= xValues.Max(); x += (xValues.Max() - xValues.Min()) / 100)
            {
                chartX.Add(x);
                chartY.Add(a + b * x + c * x * x + d * x * x * x);
            }

            var chartValues =
                new ChartValues<ObservablePoint>(chartX.Select((x, i) => new ObservablePoint(x, chartY[i])));

            var addAfter = false;
            if (cartesianChart1.Series.Contains(f4Series))
            {
                cartesianChart1.Series.Remove(f4Series);
                addAfter = true;
            }

            f4Series = new LineSeries();
            f4Series.Stroke = Brushes.OrangeRed;
            f4Series.Fill = Brushes.Transparent;
            f4Series.PointGeometry = null;
            f4Series.Title = "Парабола 3 степени";
            if (f4Series.Values != null)
                f4Series.Values.Clear();
            f4Series.Values = chartValues;

            if (addAfter)
            {
                cartesianChart1.Series.Add(f4Series);
            }

            t4.Clear();
            t4.Add(
                $"y = {a.ToString(format)} + {b.ToString(format)}x + {c.ToString(format)}x^2 + {d.ToString(format)}x^3");
            t4.Add(dOst.ToString(format));
            t4.Add(r.ToString(format));
            t4.Add(R.ToString(format));
            t4.Add(A.ToString(format));
            t4.Add(f1.ToString(format));
        }

        private void calculate3()
        {
            var count = (double) xValues.Count;

            var b =
                (yValues.Select((y, i) => Math.Log(y) * Math.Log(xValues[i])).Sum() - 1 / count *
                 yValues.Select(y => Math.Log(y)).Sum() * xValues.Select(x => Math.Log(x)).Sum())
                /
                (xValues.Select(x => Math.Pow(Math.Log(x), 2)).Sum() -
                 1 / count * Math.Pow(xValues.Select(x => Math.Log(x)).Sum(), 2));

            var a = Math.Exp(1 / count * (yValues.Select(y => Math.Log(y)).Sum() -
                                          b * xValues.Select(x => Math.Log(x)).Sum()));

            f5Values = xValues.Select(x => a * Math.Pow(x, b)).ToList();

            var yDiff = yValues.Select((y, i) => Math.Pow(y - f5Values[i], 2)).ToList();
            var yDiffSum = yDiff.Sum();

            var dOst = yDiffSum / count;

            var r = Math.Sqrt(1 - yDiffSum / (yValues.Sum(y => Math.Pow(y - yValues.Average(), 2))));
            var R = Math.Pow(r, 2);

            var avrgValues = xValues.Select((x, i) => (Math.Abs((yValues[i] - f5Values[i]) / yValues[i]) * 100))
                .ToList();

            var A = 1 / count * avrgValues.Sum();

            var f1 = R * (count - 3) / (1 - R) / 2;

            var chartX = new List<double>();
            var chartY = new List<double>();

            for (var x = xValues.Min(); x <= xValues.Max(); x += (xValues.Max() - xValues.Min()) / 100)
            {
                chartX.Add(x);
                chartY.Add(a * Math.Pow(x, b));
            }

            var chartValues =
                new ChartValues<ObservablePoint>(chartX.Select((x, i) => new ObservablePoint(x, chartY[i])));

            var addAfter = false;
            if (cartesianChart1.Series.Contains(f5Series))
            {
                cartesianChart1.Series.Remove(f5Series);
                addAfter = true;
            }

            f5Series = new LineSeries();
            f5Series.Stroke = Brushes.Chartreuse;
            f5Series.Fill = Brushes.Transparent;
            f5Series.PointGeometry = null;
            f5Series.Title = "Степенная функция";
            if (f5Series.Values != null)
                f5Series.Values.Clear();
            f5Series.Values = chartValues;

            if (addAfter)
            {
                cartesianChart1.Series.Add(f5Series);
            }

            t5.Clear();
            t5.Add(
                $"y = {a.ToString(format)}x ^ {b.ToString(format)}");
            t5.Add(dOst.ToString(format));
            t5.Add(r.ToString(format));
            t5.Add(R.ToString(format));
            t5.Add(A.ToString(format));
            t5.Add(f1.ToString(format));
        }

        private void calculate4()
        {
            var count = (double)xValues.Count;

            var B =
                (yValues.Select((y, i) => Math.Log(y) * (xValues[i])).Sum() - 1 / count *
                 yValues.Select(y => Math.Log(y)).Sum() * xValues.Sum())
                /
                (xValues.Select(x => Math.Pow(x, 2)).Sum() -
                 1 / count * Math.Pow(xValues.Sum(), 2));

            var b = Math.Exp(B);

            var a = Math.Exp(1 / count * (yValues.Select(y => Math.Log(y)).Sum() -
                                          B * xValues.Sum()));

            f6Values = xValues.Select(x => a * Math.Pow(b, x)).ToList();

            var yDiff = yValues.Select((y, i) => Math.Pow(y - f6Values[i], 2)).ToList();
            var yDiffSum = yDiff.Sum();

            var dOst = yDiffSum / count;

            var r = Math.Sqrt(1 - yDiffSum / (yValues.Sum(y => Math.Pow(y - yValues.Average(), 2))));
            var R = Math.Pow(r, 2);

            var avrgValues = xValues.Select((x, i) => (Math.Abs((yValues[i] - f6Values[i]) / yValues[i]) * 100))
                .ToList();

            var A = 1 / count * avrgValues.Sum();

            var f1 = R * (count - 3) / (1 - R) / 2;

            var chartX = new List<double>();
            var chartY = new List<double>();

            for (var x = xValues.Min(); x <= xValues.Max(); x += (xValues.Max() - xValues.Min()) / 100)
            {
                chartX.Add(x);
                chartY.Add(a * Math.Pow(b, x));
            }

            var chartValues =
                new ChartValues<ObservablePoint>(chartX.Select((x, i) => new ObservablePoint(x, chartY[i])));

            var addAfter = false;
            if (cartesianChart1.Series.Contains(f6Series))
            {
                cartesianChart1.Series.Remove(f6Series);
                addAfter = true;
            }

            f6Series = new LineSeries();
            f6Series.Stroke = Brushes.Violet;
            f6Series.Fill = Brushes.Transparent;
            f6Series.PointGeometry = null;
            f6Series.Title = "Показательная функция";
            if (f6Series.Values != null)
                f6Series.Values.Clear();
            f6Series.Values = chartValues;

            if (addAfter)
            {
                cartesianChart1.Series.Add(f6Series);
            }

            t6.Clear();
            t6.Add(
                $"y = {a.ToString(format)} * {b.ToString(format)} ^ x ");
            t6.Add(dOst.ToString(format));
            t6.Add(r.ToString(format));
            t6.Add(R.ToString(format));
            t6.Add(A.ToString(format));
            t6.Add(f1.ToString(format));
        }

        public static double[] Polyfit(double[] x, double[] y, int degree)
        {
            var v = new DenseMatrix(x.Length, degree + 1);
            for (int i = 0; i < v.RowCount; i++)
            for (int j = 0; j <= degree; j++)
                v[i, j] = Math.Pow(x[i], j);
            var yv = new DenseVector(y).ToColumnMatrix();
            QR<double> qr = v.QR();
            var r = qr.R.SubMatrix(0, degree + 1, 0, degree + 1);
            var q = v.Multiply(r.Inverse());
            var p = r.Inverse().Multiply(q.TransposeThisAndMultiply(yv));
            return p.Column(0).ToArray();
        }

        private void calculateLinear()
        {
            // Linear
            var xyValues = xValues.Select((x, i) => x * yValues[i]);
            var xSquaredValues = xValues.Select(x => x * x);

            var xSum = xValues.Sum();
            var ySum = yValues.Sum();
            var xySum = xyValues.Sum();
            var xSquaredSum = xSquaredValues.Sum();

            var count = (double) xValues.Count;

            var b = (xySum - xSum * ySum / count) / (xSquaredSum - xSum * xSum / count);
            var a = (ySum - b * xSum) / count;

            f1Values = xValues.Select(x => a + b * x).ToList();

            var yDiff = yValues.Select((y, i) => Math.Pow(y - f1Values[i], 2)).ToList();
            var yDiffSum = yDiff.Sum();

            var dOst = yDiffSum / count;

            var r = xValues.Select((x, i) => (x - xSum / count) * (yValues[i] - ySum / count)).Sum() /
                    Math.Sqrt(xValues.Sum(x => Math.Pow(x - xSum / count, 2)) *
                              yValues.Sum(y => Math.Pow(y - ySum / count, 2)));
            var R = Math.Pow(r, 2);

            var avrgValues = xValues.Select((x, i) => (Math.Abs((yValues[i] - a - b * x) / yValues[i]) * 100)).ToList();

            var A = 1 / count * avrgValues.Sum();

            var f1 = R * (count - 2) / (1 - R);

            var chartX = new List<double>();
            var chartY = new List<double>();

            for (var x = xValues.Min(); x <= xValues.Max(); x += (xValues.Max() - xValues.Min()) / 100)
            {
                chartX.Add(x);
                chartY.Add(a + b * x);
            }

            var chartValues =
                new ChartValues<ObservablePoint>(chartX.Select((x, i) => new ObservablePoint(x, chartY[i])));

            var addAfter = false;
            if (cartesianChart1.Series.Contains(f1Series))
            {
                cartesianChart1.Series.Remove(f1Series);
                addAfter = true;
            }

            f1Series = new LineSeries();
            f1Series.Stroke = Brushes.Red;
            f1Series.Title = "Линейная функция";
            f1Series.Fill = Brushes.Transparent;
            f1Series.PointGeometry = null;
            f1Series.Values = chartValues;

            if (addAfter)
            {
                cartesianChart1.Series.Add(f1Series);
            }

            t1.Clear();
            t1.Add($"y = {a.ToString(format)} + {b.ToString(format)}x");
            t1.Add(dOst.ToString(format));
            t1.Add(r.ToString(format));
            t1.Add(R.ToString(format));
            t1.Add(A.ToString(format));
            t1.Add(f1.ToString(format));
        }

        private void getTableDataSafe()
        {
            try
            {
                getTableData();
                calculateEverything();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Ошибка загрузки файла");
            }
        }

        private void updateTable()
        {
            dataGridView2.Rows.Clear();
            dataGridView2.Rows.Add(t1.ToArray());
            dataGridView2.Rows.Add(t2.ToArray());
            dataGridView2.Rows.Add(t4.ToArray());
            dataGridView2.Rows.Add(t3.ToArray());
            dataGridView2.Rows.Add(t5.ToArray());
            dataGridView2.Rows.Add(t6.ToArray());
        }

        private void getTableData()
        {
            ((DataTable) dataGridView1.DataSource).Clear();

            var d = new OpenFileDialog();
            d.ShowDialog();

            var file = new System.IO.StreamReader(d.FileName);

            string newline;
            while ((newline = file.ReadLine()) != null)
            {
                try
                {
                    var dr = dt.NewRow();
                    var values = newline.Split(',');
                    for (var i = 0; i < values.Length; i++)
                    {
                        dr[i] = double.Parse(values[i]);
                    }

                    dt.Rows.Add(dr);
                }
                catch
                {
                    // ignored
                }
            }

            file.Close();
        }

        private void updateGraphAppearance()
        {
            ((LineSeries) cartesianChart1.Series[0]).Title = "Функция";
            cartesianChart1.AxisY[0].LabelFormatter = d => d.ToString("#.##");

            var show1 = checkBox1.Checked;
            var show2 = checkBox2.Checked;
            var show3 = checkBox3.Checked;
            var show4 = checkBox6.Checked;
            var show5 = checkBox4.Checked;
            var show6 = checkBox5.Checked;

            if (show1 && !cartesianChart1.Series.Contains(f1Series)) cartesianChart1.Series.Add(f1Series);
            if (!show1 && cartesianChart1.Series.Contains(f1Series)) cartesianChart1.Series.Remove(f1Series);

            if (show2 && !cartesianChart1.Series.Contains(f2Series)) cartesianChart1.Series.Add(f2Series);
            if (!show2 && cartesianChart1.Series.Contains(f2Series)) cartesianChart1.Series.Remove(f2Series);

            if (show4 && !cartesianChart1.Series.Contains(f4Series)) cartesianChart1.Series.Add(f4Series);
            if (!show4 && cartesianChart1.Series.Contains(f4Series)) cartesianChart1.Series.Remove(f4Series);

            if (show3 && !cartesianChart1.Series.Contains(f3Series)) cartesianChart1.Series.Add(f3Series);
            if (!show3 && cartesianChart1.Series.Contains(f3Series)) cartesianChart1.Series.Remove(f3Series);

            if (show5 && !cartesianChart1.Series.Contains(f5Series)) cartesianChart1.Series.Add(f5Series);
            if (!show5 && cartesianChart1.Series.Contains(f5Series)) cartesianChart1.Series.Remove(f5Series);

            if (show6 && !cartesianChart1.Series.Contains(f6Series)) cartesianChart1.Series.Add(f6Series);
            if (!show6 && cartesianChart1.Series.Contains(f6Series)) cartesianChart1.Series.Remove(f6Series);
        }

        private void загрузитьТаблицуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getTableDataSafe();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (xValues == null || xValues.Count == 0) return;

            updateGraphAppearance();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            calculateEverything();
        }
    }
}