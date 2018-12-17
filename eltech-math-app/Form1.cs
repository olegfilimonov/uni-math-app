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
        private DataTable dt = new DataTable();

        private string xLabel = "x";
        private string yLabel = "y";

        private string format = "N";

        private List<double> xValues;
        private List<double> yValues;
        private List<double> f1Values;
        private List<double> f2Values;
        private List<double> f3Values;

        private bool showLinear = true;
        private bool showTwo = false;
        private bool showThree = false;

        private int selectedFunction = 0;

        private LineSeries f1Series = new LineSeries();
        private LineSeries f2Series = new LineSeries();
        private LineSeries f3Series = new LineSeries();

        private ArrayList t1 = new ArrayList();
        private ArrayList t2 = new ArrayList();
        private ArrayList t3 = new ArrayList();

        // 0 - линейная
        // 1 - 2 степень

        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
        }

        private void calculateEverything()
        {
            calculateLinear();
            calculateParab();
            calculateHyperb();
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

            var chartValues =
                new ChartValues<ObservablePoint>(xValues.Select((x, i) => new ObservablePoint(x, f3Values[i])));

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

            var chartValues =
                new ChartValues<ObservablePoint>(xValues.Select((x, i) => new ObservablePoint(x, f2Values[i])));

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

            var chartValues =
                new ChartValues<ObservablePoint>(xValues.Select((x, i) => new ObservablePoint(x, f1Values[i])));

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

        private void dataGridView1_Click(object sender, EventArgs e)
        {
            if (dt.Columns.Count == 0)
            {
                getTableDataSafe();
            }
        }

        private void getTableDataSafe()
        {
            try
            {
                getTableData();
                calculateEverything();
                updateGraphAppearance();
                updateTable();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Ошибка загрузки файла");
            }
        }

        private void updateTable()
        {
            selectedFunction = comboBox1.SelectedIndex;

            switch (selectedFunction)
            {
                case 0:
                    dataGridView2.Columns[0].HeaderText = "Линейная регрессия";
                    dataGridView2.Columns[1].HeaderText = "Остаточная дисперсия";
                    dataGridView2.Columns[2].HeaderText = "Коэффициент корреляции";
                    dataGridView2.Columns[3].HeaderText = "Коэффициент детерминации";
                    dataGridView2.Columns[4].HeaderText = "Средняя ошибка аппроксимации";
                    dataGridView2.Columns[5].HeaderText = "F-критерий Фишера";
                    dataGridView2.Rows.Clear();
                    dataGridView2.Rows.Add(t1.ToArray());
                    break;
                case 1:
                    dataGridView2.Columns[0].HeaderText = "Парабола второй степени";
                    dataGridView2.Columns[1].HeaderText = "Остаточная дисперсия";
                    dataGridView2.Columns[2].HeaderText = "Индекс корреляции";
                    dataGridView2.Columns[3].HeaderText = "Коэффициент детерминации";
                    dataGridView2.Columns[4].HeaderText = "Средняя ошибка аппроксимации";
                    dataGridView2.Columns[5].HeaderText = "F-критерий Фишера";
                    dataGridView2.Rows.Clear();
                    dataGridView2.Rows.Add(t2.ToArray());
                    break;
                case 2:
                    dataGridView2.Columns[0].HeaderText = "Гиперболическая функция";
                    dataGridView2.Columns[1].HeaderText = "Остаточная дисперсия";
                    dataGridView2.Columns[2].HeaderText = "Индекс корреляции";
                    dataGridView2.Columns[3].HeaderText = "Коэффициент детерминации";
                    dataGridView2.Columns[4].HeaderText = "Средняя ошибка аппроксимации";
                    dataGridView2.Columns[5].HeaderText = "F-критерий Фишера";
                    dataGridView2.Rows.Clear();
                    dataGridView2.Rows.Add(t3.ToArray());
                    break;
                default:
                    break;
            }
        }

        private void getTableData()
        {
            dt = new DataTable();
            var d = new OpenFileDialog();
            d.ShowDialog();

            var file = new System.IO.StreamReader(d.FileName);
            var columns = file.ReadLine().Split(',');
            foreach (var c in columns)
            {
                dt.Columns.Add(c);
            }

            string newline;
            while ((newline = file.ReadLine()) != null)
            {
                var dr = dt.NewRow();
                var values = newline.Split(',');
                for (var i = 0; i < values.Length; i++)
                {
                    dr[i] = values[i];
                }

                dt.Rows.Add(dr);
            }

            file.Close();
            dataGridView1.DataSource = dt;
            foreach (DataGridViewTextBoxColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                column.DefaultCellStyle.Padding = new Padding(10, 0, 10, 0);
            }

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
        }

        private void updateGraphAppearance()
        {
            xLabel = string.IsNullOrWhiteSpace(textBoxX.Text) ? "x" : textBoxX.Text;
            yLabel = string.IsNullOrWhiteSpace(textBoxY.Text) ? "y" : textBoxY.Text;

            ((LineSeries) cartesianChart1.Series[0]).Title = yLabel;
            cartesianChart1.AxisX[0].Title = yLabel;
            cartesianChart1.AxisY[0].Title = xLabel;
            cartesianChart1.AxisY[0].LabelFormatter = d => d.ToString("#.##");

            var show1 = checkBox1.Checked;
            var show2 = checkBox2.Checked;
            var show3 = checkBox3.Checked;

            if (show1 && !cartesianChart1.Series.Contains(f1Series)) cartesianChart1.Series.Add(f1Series);
            if (!show1 && cartesianChart1.Series.Contains(f1Series)) cartesianChart1.Series.Remove(f1Series);

            if (show2 && !cartesianChart1.Series.Contains(f2Series)) cartesianChart1.Series.Add(f2Series);
            if (!show2 && cartesianChart1.Series.Contains(f2Series)) cartesianChart1.Series.Remove(f2Series);

            if (show3 && !cartesianChart1.Series.Contains(f3Series)) cartesianChart1.Series.Add(f3Series);
            if (!show3 && cartesianChart1.Series.Contains(f3Series)) cartesianChart1.Series.Remove(f3Series);
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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateTable();
        }

        private void textBoxX_TextChanged(object sender, EventArgs e)
        {
            if (xValues == null || xValues.Count == 0) return;

            updateGraphAppearance();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (xValues == null || xValues.Count == 0)
            {
                getTableDataSafe();
            }
            else
            {
                calculateEverything();
            }
        }
    }
}