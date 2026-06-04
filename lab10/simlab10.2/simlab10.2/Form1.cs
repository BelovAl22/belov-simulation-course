using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Globalization;

namespace simlab10_2
{
    // --- ООП Структура ---
    public class Request
    {
        public double ArrivalTime { get; set; }
        public double ServiceDuration { get; set; }
        public double LeaveTime { get; set; }

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
        public double BusyUntil { get; set; } = 0;
        public bool IsFree(double currentTime) => currentTime >= BusyUntil - 1e-9;

        public void Process(Request req, double currentTime)
        {
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
            this.Text = "СМО M/M/N/M: Бронированная версия";
            this.Size = new Size(1150, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            InitGui();
        }

        private void InitGui()
        {
            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            this.Controls.Add(mainLayout);

            FlowLayoutPanel panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(15), AutoScroll = true };
            mainLayout.Controls.Add(panel, 0, 0);

            panel.Controls.Add(new Label { Text = "Параметры системы", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true });

            AddInput(panel, "λ (Приход заявок в ед. времени):", ref txtLambda, "10.0");
            AddInput(panel, "μ (Скорость обслуживания):", ref txtMu, "1.0");
            AddInput(panel, "N (Кол-во каналов [1 - 100]):", ref txtN, "1");
            AddInput(panel, "M (Мест в очереди [0 - 1000]):", ref txtM, "10");
            AddInput(panel, "Терпение (Время в очереди):", ref txtPatience, "1.5");

            btnStart = new Button { Text = "ЗАПУСТИТЬ РАСЧЕТ", Width = 250, Height = 50, Margin = new Padding(0, 20, 0, 0), BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat };
            btnStart.Click += (s, e) => ValidateAndRun();
            panel.Controls.Add(btnStart);

            lblResults = new Label { Text = "Готов к работе", AutoSize = true, Margin = new Padding(0, 20, 0, 0), Font = new Font("Consolas", 10), ForeColor = Color.DarkSlateGray };
            panel.Controls.Add(lblResults);

            chart1 = new Chart { Dock = DockStyle.Fill, Margin = new Padding(10) };
            chart1.ChartAreas.Add(new ChartArea("MainArea"));
            chart1.Legends.Add(new Legend("Legend"));
            mainLayout.Controls.Add(chart1, 1, 0);
        }

        private void AddInput(Panel p, string label, ref TextBox tb, string def)
        {
            p.Controls.Add(new Label { Text = label, Margin = new Padding(0, 10, 0, 0), AutoSize = true });
            tb = new TextBox { Text = def, Width = 200, Font = new Font("Segoe UI", 10) };
            p.Controls.Add(tb);
        }

        // --- ВАЛИДАТОР "АНТИ-ДУРАК" ---
        private void ValidateAndRun()
        {
            // 1. Пытаемся распарсить числа, заменяя запятые на точки
            bool lOk = double.TryParse(txtLambda.Text.Trim().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double l);
            bool mOk = double.TryParse(txtMu.Text.Trim().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double mu);
            bool nOk = int.TryParse(txtN.Text.Trim(), out int n);
            bool mQOk = int.TryParse(txtM.Text.Trim(), out int m);
            bool pOk = double.TryParse(txtPatience.Text.Trim().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double p);

            // 2. Проверка на формат и пустые поля
            if (!lOk || !mOk || !nOk || !mQOk || !pOk)
            {
                ShowError("Все поля должны быть заполнены числами!");
                return;
            }

            // 3. Проверка логических границ
            if (l <= 0 || mu <= 0 || p <= 0)
            {
                ShowError("Интенсивности и время терпения должны быть больше нуля!");
                return;
            }

            if (n < 1 || n > 100)
            {
                ShowError("Количество каналов N должно быть от 1 до 100.");
                return;
            }

            if (m < 0 || m > 1000)
            {
                ShowError("Размер очереди M должен быть от 0 до 1000.");
                return;
            }

            // 4. Если всё хорошо — запускаем в безопасном режиме
            try
            {
                btnStart.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
                RunSimulation(l, mu, n, m, p);
            }
            catch (Exception ex)
            {
                ShowError("Критический сбой модели: " + ex.Message);
            }
            finally
            {
                btnStart.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        private void ShowError(string msg) => MessageBox.Show(msg, "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);

        // --- ЯДРО МОДЕЛИ ---
        private void RunSimulation(double lambda, double mu, int N, int M, double patienceLimit)
        {
            Random rng = new Random();
            double GenExp(double rate) => -Math.Log(1 - Math.Max(rng.NextDouble(), 1e-10)) / rate;

            List<Server> servers = Enumerable.Range(0, N).Select(i => new Server { Id = i }).ToList();
            Queue<Request> queue = new Queue<Request>();

            double currentTime = 0;
            double maxModelTime = 500.0; // Фиксированный прогон для стабильности

            int totalArrivals = 0, successful = 0, rejectedNoSpace = 0, leftImpatient = 0;
            double accQueue = 0, accBusy = 0;

            double nextArrival = GenExp(lambda);

            while (currentTime < maxModelTime)
            {
                // Определяем время ближайшего ухода нетерпеливого из начала очереди
                double nextImpatientLeave = (queue.Count > 0) ? queue.Peek().LeaveTime : double.MaxValue;

                // Определяем время ближайшего освобождения любого прибора
                double nextFinish = servers.Select(s => s.BusyUntil).Where(t => t > currentTime).DefaultIfEmpty(double.MaxValue).Min();

                // Ищем самое раннее событие
                double nextEvent = Math.Min(nextArrival, Math.Min(nextFinish, nextImpatientLeave));

                // Защита: время не может стоять на месте или идти назад
                if (nextEvent <= currentTime) nextEvent = currentTime + 1e-7;
                if (nextEvent > maxModelTime) nextEvent = maxModelTime;

                // Статистика (интегралы по времени)
                double dt = nextEvent - currentTime;
                accQueue += queue.Count * dt;
                accBusy += servers.Count(s => !s.IsFree(currentTime)) * dt;

                currentTime = nextEvent;

                if (currentTime >= maxModelTime) break;

                // ОБРАБОТКА
                if (currentTime == nextImpatientLeave)
                {
                    queue.Dequeue();
                    leftImpatient++;
                }
                else if (currentTime == nextArrival)
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
                else
                { // Завершение обслуживания
                    var finishedServer = servers.FirstOrDefault(s => Math.Abs(s.BusyUntil - currentTime) < 1e-7);
                    if (finishedServer != null)
                    {
                        if (queue.Count > 0)
                        {
                            finishedServer.Process(queue.Dequeue(), currentTime);
                            successful++;
                        }
                        else
                        {
                            finishedServer.BusyUntil = 0; // Освобождаем
                        }
                    }
                }
            }

            // Вывод и График
            lblResults.Text = $"--- ФИНАЛЬНЫЙ ОТЧЕТ ---\n\n" +
                $"Всего заявок: {totalArrivals}\n" +
                $"Обслужено: {successful}\n" +
                $"Отказ (очередь): {rejectedNoSpace}\n" +
                $"Ушло (терпение): {leftImpatient}\n\n" +
                $"Ср. длина очереди: {(accQueue / currentTime):F3}\n" +
                $"Ср. занято каналов: {(accBusy / currentTime):F3}\n" +
                $"Загрузка: {(accBusy / (currentTime * N) * 100):F1}%";

            UpdateChart(accQueue / currentTime, accBusy / currentTime, N);
        }

        private void UpdateChart(double q, double b, int n)
        {
            chart1.Series.Clear();
            Series s = new Series("Показатели") { ChartType = SeriesChartType.Column };
            s.Points.AddXY("Очередь", q);
            s.Points.AddXY("Занято", b);
            s.Points.AddXY("Всего приборов", n);
            chart1.Series.Add(s);
        }
    }
}