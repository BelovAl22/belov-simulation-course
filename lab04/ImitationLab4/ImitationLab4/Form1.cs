using System;
using System.Drawing;
using System.Windows.Forms;

namespace ImitationLab4
{
    public class Form1 : Form
    {

        public Form1()
        {
            SetupUI();
        }

        // ===== GUI =====
        private Button btnRun;
        private TextBox txtOutput;
        private NumericUpDown numSampleSize;
        private TextBox txtSeed;
        private DataGridView dgvResults;
        private Label lblSample;
        private Label lblSeed;

        private void SetupUI()
        {
            this.Text = "Лабораторная: Случайные числа";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // ===== LABEL: Размер выборки =====
            lblSample = new Label();
            lblSample.Text = "Размер выборки:";
            lblSample.Location = new Point(10, 15);
            lblSample.AutoSize = true;

            // ===== NumericUpDown =====
            numSampleSize = new NumericUpDown();
            numSampleSize.Location = new Point(130, 10);
            numSampleSize.Minimum = 1000;
            numSampleSize.Maximum = 1000000;
            numSampleSize.Value = 100000;

            // ===== LABEL: Seed =====
            lblSeed = new Label();
            lblSeed.Text = "Seed:";
            lblSeed.Location = new Point(300, 15);
            lblSeed.AutoSize = true;

            // ===== TextBox Seed =====
            txtSeed = new TextBox();
            txtSeed.Location = new Point(350, 10);
            txtSeed.Width = 100;
            txtSeed.Text = "12345";

            // ===== КНОПКА =====
            btnRun = new Button();
            btnRun.Text = "Запуск";
            btnRun.Location = new Point(480, 8);
            btnRun.Size = new Size(100, 30);
            btnRun.Click += btnRun_Click;

            // ===== DataGridView =====
            dgvResults = new DataGridView();
            dgvResults.Location = new Point(10, 50);
            dgvResults.Size = new Size(760, 200);
            dgvResults.ColumnCount = 4;
            dgvResults.Columns[0].Name = "Генератор";
            dgvResults.Columns[1].Name = "Среднее";
            dgvResults.Columns[2].Name = "Дисперсия";
            dgvResults.Columns[3].Name = "Отклонение";

            dgvResults.AllowUserToAddRows = false;
            dgvResults.RowHeadersVisible = false;

            // ===== TextBox логов =====
            txtOutput = new TextBox();
            txtOutput.Location = new Point(10, 270);
            txtOutput.Size = new Size(760, 280);
            txtOutput.Multiline = true;
            txtOutput.ScrollBars = ScrollBars.Vertical;
            txtOutput.Font = new Font("Consolas", 10);

            // ===== Добавление =====
            this.Controls.Add(lblSample);
            this.Controls.Add(numSampleSize);
            this.Controls.Add(lblSeed);
            this.Controls.Add(txtSeed);
            this.Controls.Add(btnRun);
            this.Controls.Add(dgvResults);
            this.Controls.Add(txtOutput);
        }

        // ===== МУЛЬТИПЛИКАТИВНЫЙ ГЕНЕРАТОР =====
        class MultiplicativeRandom
        {
            private long a = 16807;
            private long m = 2147483647;
            private long current;

            public MultiplicativeRandom(long seed)
            {
                if (seed == 0) seed = 1;
                current = seed;
            }

            public double NextDouble()
            {
                current = (a * current) % m;
                return (double)current / m;
            }
        }

        // ===== РАСЧЁТ =====
        void Calculate(Func<double> generator, int n, out double mean, out double variance)
        {
            double sum = 0;
            double sumSq = 0;

            for (int i = 0; i < n; i++)
            {
                double x = generator();
                sum += x;
                sumSq += x * x;
            }

            mean = sum / n;
            variance = (sumSq / n) - (mean * mean);
        }

        // ===== КНОПКА =====
        private void btnRun_Click(object sender, EventArgs e)
        {
            int N = (int)numSampleSize.Value;
            long seed = long.Parse(txtSeed.Text);

            dgvResults.Rows.Clear();

            // Наш генератор
            var myRand = new MultiplicativeRandom(seed);
            Calculate(() => myRand.NextDouble(), N, out double mean1, out double var1);

            // Встроенный
            Random rand = new Random((int)seed);
            Calculate(() => rand.NextDouble(), N, out double mean2, out double var2);

            // Теория
            double theoryMean = 0.5;
            double theoryVar = 1.0 / 12.0;

            // Добавляем в таблицу
            dgvResults.Rows.Add("MCG", mean1, var1, Math.Abs(mean1 - theoryMean));
            dgvResults.Rows.Add("Random", mean2, var2, Math.Abs(mean2 - theoryMean));

            // Логи
            txtOutput.Text =
                $"Размер выборки: {N}\r\n" +
                $"Seed: {seed}\r\n\r\n" +

                "Теоретические значения:\r\n" +
                $"Среднее = {theoryMean:F6}\r\n" +
                $"Дисперсия = {theoryVar:F6}\r\n\r\n";
        }
    }
}