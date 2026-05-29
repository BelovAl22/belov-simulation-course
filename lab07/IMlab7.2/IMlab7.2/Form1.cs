using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IMlab7._2
{
    public partial class Form1 : Form
    {
        private double[,] Q = new double[3, 3];
        private double[] stateTimes = new double[3];
        private double[] theoreticalProbs = new double[3];
        private Random rnd = new Random();
        private bool running = false;
        private int currentState = 0;
        private double totalTime = 0;
        private int eventCount = 0;

        private string[] stateNames = { "Ясно", "Облачно", "Пасмурно" };
        private Color[] stateColors = { Color.Gold, Color.Gray, Color.DarkGray };

        public Form1()
        {
            InitializeComponent();
            SetupGrids();
            InitMatrix();
        }

        private bool ValidateMatrix()
        {
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    double rowSum = 0;

                    for (int j = 0; j < 3; j++)
                    {
                        double value =
                            Convert.ToDouble(dgvQ.Rows[i].Cells[j].Value);

                        // Диагональ
                        if (i == j)
                        {
                            if (value >= 0)
                            {
                                MessageBox.Show($"Диагональный элемент [{i},{j}] должен быть отрицательным");
                                return false;
                            }
                        }
                        else
                        {
                            //Остальные элементы >=0
                            if (value < 0)
                            {
                                MessageBox.Show($"Элемент [{i},{j}] должен быть положительным");
                                return false;
                            }
                        }

                        rowSum += value;
                    }

                    if (Math.Abs(rowSum) > 0.01)
                    {
                        MessageBox.Show($"Сумма строки {i} должна быть равна 0");
                        return false;

                    }
                }

                return true;
            } catch {
                MessageBox.Show("Введите число через запятую.");
                return false; }

        }
        private void SetupGrids()
        {
            dgvQ.ColumnCount = 3;
            dgvQ.RowCount = 3;
            for (int i = 0; i < 3; i++)
            {
                dgvQ.Columns[i].HeaderText = "→ " + stateNames[i];
                dgvQ.Rows[i].HeaderCell.Value = "Из " + stateNames[i];
                dgvQ.Columns[i].Width = 80;
            }
            dgvQ.RowHeadersWidth = 110;

            dgvStats.ColumnCount = 4;
            dgvStats.Columns[0].HeaderText = "Тип";
            dgvStats.Columns[1].HeaderText = "Эмп.";
            dgvStats.Columns[2].HeaderText = "Теор.";
            dgvStats.Columns[3].HeaderText = "Разн.";
            dgvStats.RowHeadersVisible = false;
            dgvStats.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void InitMatrix()
        {
            dgvQ[0, 0].Value = -0.5; dgvQ[1, 0].Value = 0.3; dgvQ[2, 0].Value = 0.2;
            dgvQ[0, 1].Value = 0.1; dgvQ[1, 1].Value = -0.4; dgvQ[2, 1].Value = 0.3;
            dgvQ[0, 2].Value = 0.2; dgvQ[1, 2].Value = 0.1; dgvQ[2, 2].Value = -0.3;
        }

        private double SafeDouble(object val)
        {
            if (val == null) return 0;
            string s = val.ToString().Replace(',', '.');
            return double.Parse(s, CultureInfo.InvariantCulture);
        }

        private void CalculateTheoretical()
        {
            try
            {
                double[,] a = new double[3, 3];
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 2; j++)
                        a[j, i] = Q[i, j];

                a[2, 0] = 1; a[2, 1] = 1; a[2, 2] = 1;
                double[] b = { 0, 0, 1 };

                double det = Det3x3(a);
                if (Math.Abs(det) < 1e-9) return;

                for (int i = 0; i < 3; i++)
                {
                    double[,] temp = (double[,])a.Clone();
                    for (int j = 0; j < 3; j++) temp[j, i] = b[j];
                    theoreticalProbs[i] = Det3x3(temp) / det;
                }
            }
            catch { /* Игнорируем ошибки расчета Pi */ }
        }

        private double Det3x3(double[,] m) =>
            m[0, 0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1]) - m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) + m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (!ValidateMatrix())
                return;


            if (running) return;
            try
            {
                // Считываем матрицу правильно: Row i, Col j
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        Q[i, j] = SafeDouble(dgvQ[j, i].Value);
            }
            catch { MessageBox.Show("Проверьте формат чисел в матрице!"); return; }


            CalculateTheoretical();
            running = true;
            double limit = (double)numLimit.Value;

            while (running)
            {
                if (rbEvents.Checked && eventCount >= limit) break;
                if (rbDays.Checked && totalTime >= limit) break;

                double lambda = -Q[currentState, currentState];
                if (lambda <= 0) break; // Защита от деления на 0

                double dt = -Math.Log(rnd.NextDouble()) / lambda;

                string timeStart = FormatTime(totalTime);
                double oldTotal = totalTime;
                totalTime += dt;

                stateTimes[currentState] += dt;
                eventCount++;

                LogEvent(timeStart, FormatTime(totalTime), currentState, dt);
                UpdateUI(limit);

                currentState = GetNextState(currentState);
                await Task.Delay(trackDelay.Value);
            }

            running = false;
            if (eventCount > 0) ExportToCSV();
        }

        private string FormatTime(double days)
        {
            int d = (int)days;
            int h = (int)((days - d) * 24);
            int m = (int)((((days - d) * 24) - h) * 60);
            return $"{d}д {h:D2}:{m:D2}";
        }

        private int GetNextState(int s)
        {
            double r = rnd.NextDouble();
            double sum = 0;
            double lambda = -Q[s, s];
            for (int j = 0; j < 3; j++)
            {
                if (s == j) continue;
                sum += Q[s, j] / lambda;
                if (r <= sum) return j;
            }
            return s;
        }

        private void UpdateUI(double limit)
        {
            lblStatus.Text = $"Событие {eventCount}: {stateNames[currentState]}";
            lblTime.Text = $"Время: {totalTime:F2} дней";

            double progress = rbEvents.Checked ? (eventCount / limit * 100) : (totalTime / limit * 100);
            progressBar.Value = (int)Math.Min(100, progress);

            chartPie.Series[0].Points.Clear();
            dgvStats.Rows.Clear();
            for (int i = 0; i < 3; i++)
            {
                double emp = totalTime > 0 ? stateTimes[i] / totalTime : 0;
                var p = chartPie.Series[0].Points.AddXY(stateNames[i], emp);
                chartPie.Series[0].Points[i].Color = stateColors[i];
                chartPie.Series[0].Points[i].Label = (i + 1).ToString();

                dgvStats.Rows.Add(stateNames[i], $"{emp:F3}", $"{theoreticalProbs[i]:F3}", $"{Math.Abs(emp - theoreticalProbs[i]):F3}");
            }
        }

        private void LogEvent(string start, string end, int state, double dur)
        {
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionColor = Color.Orange;
            txtLog.AppendText($"[{start}-{end}] ");

            txtLog.SelectionColor = Color.Black;
            txtLog.AppendText($"{stateNames[state].PadRight(10)} | ");

            txtLog.SelectionColor = Color.Green;
            txtLog.AppendText($"{FormatDuration(dur)}\n");
            txtLog.ScrollToCaret();
        }

        private string FormatDuration(double d) => $"{(int)d}д {(int)((d - (int)d) * 24)}ч";

        private void btnStop_Click(object sender, EventArgs e) => running = false;

        private void btnReset_Click(object sender, EventArgs e)
        {
            running = false;
            totalTime = 0; eventCount = 0; currentState = 0;
            Array.Clear(stateTimes, 0, 3);
            txtLog.Clear();
            progressBar.Value = 0;
            chartPie.Series[0].Points.Clear();
            dgvStats.Rows.Clear();
            lblStatus.Text = "Событие: 0/0";
            lblTime.Text = "Время: 0.00 дней";
        }

        private void ExportToCSV()
        {
            try
            {
                var lines = new List<string> { "State;Empirical;Theoretical;Diff" };
                for (int i = 0; i < 3; i++)
                {
                    double emp = stateTimes[i] / totalTime;
                    lines.Add($"{stateNames[i]};{emp:F4};{theoreticalProbs[i]:F4};{Math.Abs(emp - theoreticalProbs[i]):F4}");
                }
                File.WriteAllLines("weather_results.csv", lines);
            }
            catch { }
        }
    }
}