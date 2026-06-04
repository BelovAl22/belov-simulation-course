using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Globalization;

namespace simlab10_2
{
    public class Request
    {
        public double ArrivalTime { get; set; }
        public double ServiceDuration { get; set; }
        public double LeaveTime { get; set; } // Момент, когда заявка уйдет из очереди

        public Request(double arrival, double service, double patience)
        {
            ArrivalTime = arrival;
            ServiceDuration = service;
            LeaveTime = arrival + patience;
        }
    }

    public class Server
    {
        public int Id { get; set; }
        public Request CurrentRequest { get; set; }
        public double BusyUntil { get; set; } = 0;
        public bool IsFree(double currentTime) => currentTime >= BusyUntil;

        public void Process(Request req, double currentTime)
        {
            CurrentRequest = req;
            BusyUntil = currentTime + req.ServiceDuration;
        }
    }

    public partial class Form1 : Form
    {
        private TextBox txtLambda, txtMu, txtN, txtM, txtPatience;
        private Label lblResults;
        private Chart chart1;
        private Button btnStart;

        public Form1()
        {
            this.Text = "СМО M/M/N/M: Исправленная нетерпеливость";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            InitGui();
        }

        private void InitGui()
        {
            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            this.Controls.Add(mainLayout);

            FlowLayoutPanel panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(15) };
            mainLayout.Controls.Add(panel, 0, 0);

            panel.Controls.Add(new Label { Text = "Параметры системы", Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true });

            AddInput(panel, "λ (Приход):", ref txtLambda, "10.0"); // Высокая интенсивность для тестов
            AddInput(panel, "μ (Обслуживание):", ref txtMu, "1.0");
            AddInput(panel, "N (Приборов):", ref txtN, "1"); // 1 прибор, чтобы быстрее росла очередь
            AddInput(panel, "M (Мест в очереди):", ref txtM, "10");
            AddInput(panel, "Терпение (Wait Time):", ref txtPatience, "1.5"); // Малое терпение

            btnStart = new Button { Text = "ЗАПУСТИТЬ", Width = 200, Height = 45, Margin = new Padding(0, 20, 0, 0), BackColor = Color.LightGreen };
            btnStart.Click += (s, e) => TryRunSimulation();
            panel.Controls.Add(btnStart);

            lblResults = new Label { Text = "Результаты...", AutoSize = true, Margin = new Padding(0, 20, 0, 0), Font = new Font("Consolas", 9) };
            panel.Controls.Add(lblResults);

            chart1 = new Chart { Dock = DockStyle.Fill };
            chart1.ChartAreas.Add(new ChartArea("Main"));
            mainLayout.Controls.Add(chart1, 1, 0);
        }

        private void AddInput(Panel p, string label, ref TextBox tb, string def)
        {
            p.Controls.Add(new Label { Text = label, Margin = new Padding(0, 8, 0, 0), AutoSize = true });
            tb = new TextBox { Text = def, Width = 100 };
            p.Controls.Add(tb);
        }

        private void TryRunSimulation()
        {
            if (!double.TryParse(txtLambda.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double l) || l <= 0) return;
            if (!double.TryParse(txtMu.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double mu) || mu <= 0) return;
            if (!int.TryParse(txtN.Text, out int n) || n < 1) return;
            if (!int.TryParse(txtM.Text, out int m) || m < 0) return;
            if (!double.TryParse(txtPatience.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double p) || p <= 0) return;

            RunSimulation(l, mu, n, m, p);
        }

        private void RunSimulation(double lambda, double mu, int N, int M, double patienceLimit)
        {
            var rng = new Random();
            double GenExp(double rate) => -Math.Log(1 - rng.NextDouble()) / rate;

            List<Server> servers = Enumerable.Range(0, N).Select(i => new Server { Id = i }).ToList();
            Queue<Request> queue = new Queue<Request>();

            double currentTime = 0;
            double maxModelTime = 1000.0;

            int totalArrivals = 0;
            int successful = 0;
            int rejectedNoSpace = 0;
            int leftImpatient = 0;

            double accQueue = 0;
            double accBusy = 0;

            double nextArrival = GenExp(lambda);

            while (currentTime < maxModelTime)
            {
                // 1. Находим время ближайшего "терпения" в очереди
                double nextImpatientLeave = (queue.Count > 0) ? queue.Peek().LeaveTime : double.MaxValue;

                // 2. Находим время ближайшего освобождения прибора
                double nextFinish = servers.Where(s => s.BusyUntil > currentTime)
                                           .Select(s => s.BusyUntil)
                                           .DefaultIfEmpty(double.MaxValue).Min();

                // 3. Выбираем самое ближайшее событие из ТРЕХ
                double nextEvent = Math.Min(nextArrival, Math.Min(nextFinish, nextImpatientLeave));
                if (nextEvent > maxModelTime) nextEvent = maxModelTime;

                // Сбор статистики
                double dt = nextEvent - currentTime;
                accQueue += queue.Count * dt;
                accBusy += servers.Count(s => s.BusyUntil > currentTime) * dt;

                currentTime = nextEvent;
                if (currentTime >= maxModelTime) break;

                // ОБРАБОТКА СОБЫТИЙ
                if (currentTime == nextImpatientLeave) // КТО-ТО УШЕЛ ИЗ ОЧЕРЕДИ
                {
                    queue.Dequeue();
                    leftImpatient++;
                }
                else if (currentTime == nextArrival) // ПРИШЛА ЗАЯВКА
                {
                    totalArrivals++;
                    Request req = new Request(currentTime, GenExp(mu), patienceLimit);

                    var freeServer = servers.FirstOrDefault(s => s.IsFree(currentTime));
                    if (freeServer != null)
                    {
                        freeServer.Process(req, currentTime);
                        successful++;
                    }
                    else if (queue.Count < M)
                    {
                        queue.Enqueue(req);
                    }
                    else
                    {
                        rejectedNoSpace++;
                    }
                    nextArrival = currentTime + GenExp(lambda);
                }
                else // КТО-ТО ЗАКОНЧИЛ ОБСЛУЖИВАНИЕ
                {
                    foreach (var s in servers)
                    {
                        if (s.BusyUntil <= currentTime && s.CurrentRequest != null)
                        {
                            s.CurrentRequest = null;
                            if (queue.Count > 0)
                            {
                                var nextReq = queue.Dequeue();
                                s.Process(nextReq, currentTime);
                                successful++;
                            }
                        }
                    }
                }
            }

            lblResults.Text = $"--- РЕЗУЛЬТАТЫ ---\n\n" +
                $"Всего пришло: {totalArrivals}\n" +
                $"Успешно: {successful}\n" +
                $"Отказ (нет мест): {rejectedNoSpace}\n" +
                $"УШЛО (не дождались): {leftImpatient}\n\n" +
                $"Ср. длина очереди: {(accQueue / currentTime):F2}\n" +
                $"Загрузка: {(accBusy / (currentTime * N) * 100):F1}%";

            UpdateChart(accQueue / currentTime, accBusy / currentTime, N);
        }

        private void UpdateChart(double q, double b, int n)
        {
            chart1.Series.Clear();
            var s = new Series("Статистика") { ChartType = SeriesChartType.Column };
            s.Points.AddXY("Очередь", q);
            s.Points.AddXY("Занято", b);
            chart1.Series.Add(s);
        }
    }
}