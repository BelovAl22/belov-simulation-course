namespace IMlab7._2
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // Элементы управления
        private System.Windows.Forms.DataGridView dgvQ;
        private System.Windows.Forms.Button btnStart, btnStop, btnReset;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.TrackBar trackDelay;
        private System.Windows.Forms.RadioButton rbEvents, rbDays;
        private System.Windows.Forms.NumericUpDown numLimit;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPie;
        private System.Windows.Forms.Label lblStatus, lblTime, lblDelay, lblMode;
        private System.Windows.Forms.DataGridView dgvStats;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.dgvQ = new System.Windows.Forms.DataGridView();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.trackDelay = new System.Windows.Forms.TrackBar();
            this.rbEvents = new System.Windows.Forms.RadioButton();
            this.rbDays = new System.Windows.Forms.RadioButton();
            this.numLimit = new System.Windows.Forms.NumericUpDown();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.chartPie = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblDelay = new System.Windows.Forms.Label();
            this.lblMode = new System.Windows.Forms.Label();
            this.dgvStats = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvQ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPie)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStats)).BeginInit();
            this.SuspendLayout();

            // Матрица Q
            this.dgvQ.AllowUserToAddRows = false;
            this.dgvQ.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvQ.Location = new System.Drawing.Point(12, 12);
            this.dgvQ.Size = new System.Drawing.Size(480, 115);

            // Режим (текст)
            this.lblMode.Location = new System.Drawing.Point(12, 140);
            this.lblMode.Text = "Режим остановки:";
            this.lblMode.Size = new System.Drawing.Size(150, 20);

            // Радиокнопки
            this.rbEvents.Checked = true;
            this.rbEvents.Location = new System.Drawing.Point(15, 160);
            this.rbEvents.Text = "По событиям (N)";
            this.rbEvents.Size = new System.Drawing.Size(130, 24);

            this.rbDays.Location = new System.Drawing.Point(150, 160);
            this.rbDays.Text = "По дням (T)";
            this.rbDays.Size = new System.Drawing.Size(130, 24);

            // Ввод N или T
            this.numLimit.Location = new System.Drawing.Point(15, 190);
            this.numLimit.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            this.numLimit.Value = 1000;
            this.numLimit.Size = new System.Drawing.Size(120, 20);

            // Слайдер задержки (Выровнен с выбором дней)
            this.trackDelay.Location = new System.Drawing.Point(145, 190);
            this.trackDelay.Maximum = 1000;
            this.trackDelay.Minimum = 10;
            this.trackDelay.Value = 200;
            this.trackDelay.Size = new System.Drawing.Size(250, 45);
            this.trackDelay.Scroll += (s, e) => lblDelay.Text = trackDelay.Value + " мс";

            this.lblDelay.Location = new System.Drawing.Point(400, 195);
            this.lblDelay.Text = "200 мс";
            this.lblDelay.Size = new System.Drawing.Size(60, 20);

            // Кнопки
            this.btnStart.BackColor = System.Drawing.Color.LightGreen;
            this.btnStart.Location = new System.Drawing.Point(12, 250);
            this.btnStart.Size = new System.Drawing.Size(120, 45);
            this.btnStart.Text = "СТАРТ";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);

            this.btnStop.BackColor = System.Drawing.Color.LightCoral;
            this.btnStop.Location = new System.Drawing.Point(140, 250);
            this.btnStop.Size = new System.Drawing.Size(120, 45);
            this.btnStop.Text = "СТОП";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);

            this.btnReset.Location = new System.Drawing.Point(270, 250);
            this.btnReset.Size = new System.Drawing.Size(120, 45);
            this.btnReset.Text = "СБРОС";
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

            // Лог
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLog.Location = new System.Drawing.Point(12, 310);
            this.txtLog.Size = new System.Drawing.Size(480, 240);

            // График
            chartArea1.Name = "ChartArea1";
            this.chartPie.ChartAreas.Add(chartArea1);
            this.chartPie.Location = new System.Drawing.Point(510, 12);
            this.chartPie.Size = new System.Drawing.Size(400, 280);
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series1.Name = "States";
            this.chartPie.Series.Add(series1);

            // Индикаторы
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblStatus.Location = new System.Drawing.Point(510, 305);
            this.lblStatus.Size = new System.Drawing.Size(400, 25);
            this.lblStatus.Text = "Событие: 0/0";

            this.lblTime.Location = new System.Drawing.Point(510, 330);
            this.lblTime.Size = new System.Drawing.Size(400, 20);
            this.lblTime.Text = "Время: 0.00 дней";

            this.progressBar.Location = new System.Drawing.Point(510, 360);
            this.progressBar.Size = new System.Drawing.Size(380, 20);

            // Статистика
            this.dgvStats.AllowUserToAddRows = false;
            this.dgvStats.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvStats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStats.Location = new System.Drawing.Point(510, 395);
            this.dgvStats.Size = new System.Drawing.Size(380, 155);

            // Форма
            this.ClientSize = new System.Drawing.Size(920, 565);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.dgvQ, this.btnStart, this.btnStop, this.btnReset, this.txtLog,
                this.trackDelay, this.rbEvents, this.rbDays, this.numLimit, this.progressBar,
                this.chartPie, this.lblStatus, this.lblTime, this.lblDelay, this.lblMode, this.dgvStats
            });
            this.Text = "Марковская модель погоды (СТМС)";
            ((System.ComponentModel.ISupportInitialize)(this.dgvQ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPie)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStats)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}