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
using LiveCharts.Wpf;

namespace eltech_math_app
{
    public partial class Form1 : Form
    {
        DataTable dt = new DataTable();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var validForm = validateInput();

            if (!validForm) return;

            var a = double.Parse(textBoxA.Text);
            var n = double.Parse(textBoxN.Text);
        }

        private bool validateInput()
        {
            var errors = new ArrayList();
            double doubleTemp;

            if (string.IsNullOrWhiteSpace(textBoxN.Text)) errors.Add("Поле \"n\" пустое или содержит только пробелы");
            else if (!double.TryParse(textBoxN.Text, out doubleTemp)) errors.Add("Поле \"n\" должно быть числом");

            if (string.IsNullOrWhiteSpace(textBoxA.Text)) errors.Add("Поле \"a\" пустое или содержит только пробелы");
            else if (!double.TryParse(textBoxA.Text, out doubleTemp)) errors.Add("Поле \"a\" должно быть числом");

            var valid = errors.Count == 0;

            if (!valid)
            {
                var message = string.Join("\n", errors.ToArray());
                MessageBox.Show(message, "Ошибка проверки исходных данных");
            }

            return errors.Count == 0;
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
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Ошибка загрузки файла");
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

            List<double> xValues = dt.AsEnumerable().Select(x => Convert.ToDouble(x["x"])).ToList();
            List<double> yValues = dt.AsEnumerable().Select(x => Convert.ToDouble(x["y"])).ToList();

            ChartValues<double> chartValues = new ChartValues<double>(yValues);

            cartesianChart1.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Число травмированных",
                    Values = chartValues,
                }
            };

            cartesianChart1.AxisX[0].Title = "Кол-во пожаров";
            cartesianChart1.AxisX[0].Labels = xValues.ConvertAll(x => x.ToString());
            cartesianChart1.AxisY[0].Title = "Число травмированных";

            cartesianChart1.LegendLocation = LegendLocation.None;
        }

        private void загрузитьТаблицуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getTableDataSafe();
        }
    }
}