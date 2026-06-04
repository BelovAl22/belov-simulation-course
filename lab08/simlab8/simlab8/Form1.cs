using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace simlab8
{
    public partial class Form1 : Form
    {
        private NumericUpDown numLambda;
        private NumericUpDown numT;
        private NumericUpDown numN;

        private Button btnStart;

        private DataGridView dgv;

        private Chart chart;

        private Label lblMean;
        private Label lblVariance;

        public Form1()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "Пуассоновский поток. События на сервере";

            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;

            GroupBox gbParams = new GroupBox();
            gbParams.Text = "Параметры";
            gbParams.Location = new Point(10, 10);
            gbParams.Size = new Size(250, 170);

            Controls.Add(gbParams);

            Label lbl1 = new Label();
            lbl1.Text = "Лямбда";
            lbl1.Location = new Point(15, 30);
            lbl1.AutoSize = true;

            gbParams.Controls.Add(lbl1);

            numLambda = new NumericUpDown();
            numLambda.Location = new Point(130, 28);
            numLambda.Minimum = 1;
            numLambda.Maximum = 100;
            numLambda.Value = 5;

            gbParams.Controls.Add(numLambda);

            Label lbl2 = new Label();
            lbl2.Text = "Время T";
            lbl2.Location = new Point(15, 65);
            lbl2.AutoSize = true;

            gbParams.Controls.Add(lbl2);

            numT = new NumericUpDown();
            numT.Location = new Point(130, 63);
            numT.Minimum = 1;
            numT.Maximum = 100;
            numT.Value = 2;

            gbParams.Controls.Add(numT);

            Label lbl3 = new Label();
            lbl3.Text = "Опытов N";
            lbl3.Location = new Point(15, 100);
            lbl3.AutoSize = true;

            gbParams.Controls.Add(lbl3);

            numN = new NumericUpDown();
            numN.Location = new Point(130, 98);
            numN.Minimum = 100;
            numN.Maximum = 100000;
            numN.Increment = 100;
            numN.Value = 1000;

            gbParams.Controls.Add(numN);

            btnStart = new Button();
            btnStart.Text = "Запуск";
            btnStart.Location = new Point(15, 130);
            btnStart.Size = new Size(205, 30);
            btnStart.Click += BtnStart_Click;

            gbParams.Controls.Add(btnStart);

            dgv = new DataGridView();
            dgv.Location = new Point(10, 195);
            dgv.Size = new Size(250, 300);

            dgv.Columns.Add("i", "Число событий (i)");
            dgv.Columns.Add("freq", "Частота (Freq/N)");

            Controls.Add(dgv);

            lblMean = new Label();
            lblMean.Location = new Point(10, 520);
            lblMean.AutoSize = true;
            lblMean.Text = "Среднее (M):";

            Controls.Add(lblMean);

            lblVariance = new Label();
            lblVariance.Location = new Point(10, 550);
            lblVariance.AutoSize = true;
            lblVariance.Text = "Дисперсия (D):";

            Controls.Add(lblVariance);

            chart = new Chart();
            chart.Location = new Point(290, 10);
            chart.Size = new Size(680, 620);

            ChartArea area = new ChartArea();

            area.AxisX.Title = "Число событий (i)";
            area.AxisY.Title = "Вероятность (частота)";

            chart.ChartAreas.Add(area);

            Controls.Add(chart);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            double lambda = (double)numLambda.Value;
            double T = (double)numT.Value;
            int N = (int)numN.Value;

            MCG rnd = new MCG(10);

            List<int> results = new List<int>();

            for (int i = 0; i < N; i++)
            {
                results.Add(
                    SimulatePoissonFlow(lambda, T, rnd)
                );
            }

            FillTable(results, N);
            DrawHistogram(results, N);
            ShowStatistics(results);
        }

        private int SimulatePoissonFlow(double lambda, double T, MCG rnd)
        {
            double currentTime = 0;
            int count = 0;

            while (true)
            {
                double u = rnd.NextDouble();

                double tau = -Math.Log(u) / lambda;

                currentTime += tau;

                if (currentTime > T)
                    break;

                count++;
            }

            return count;
        }

        private void FillTable(List<int> data, int N)
        {
            dgv.Rows.Clear();

            int max = data.Max();

            for (int i = 0; i <= max; i++)
            {
                int freq = data.Count(x => x == i);

                double p = (double)freq / N;

                dgv.Rows.Add(
                    i,
                    p.ToString("F4")
                );
            }
        }

        private void DrawHistogram(List<int> data, int N)
        {
            chart.Series.Clear();

            Series s = new Series();
            s.ChartType = SeriesChartType.Column;

            int max = data.Max();

            for (int i = 0; i <= max; i++)
            {
                int freq = data.Count(x => x == i);

                double p = (double)freq / N;

                s.Points.AddXY(i, p);
            }

            chart.Series.Add(s);

            chart.Legends.Clear();
        }

        private void ShowStatistics(List<int> data)
        {
            double mean = data.Average();

            double variance = data
                .Select(x => Math.Pow(x - mean, 2))
                .Average();

            lblMean.Text =
                $"Среднее (M): {mean:F3}";

            lblVariance.Text =
                $"Дисперсия (D): {variance:F3}";
        }

        public class MCG
        {
            private const long M = 2147483647;
            private const long A = 48271;

            private long seed;

            public MCG(long seed)
            {
                this.seed = seed;
            }

            public double NextDouble()
            {
                seed = (A * seed) % M;

                return (double)seed / M;
            }
        }
    }
}