using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Globalization;

namespace lab9
{
    public partial class Form1 : Form
    {
        // Элементы интерфейса
        private TextBox txtLambda, txtMu, txtN;
        private Label lblResults;
        private Chart chart1;
        private Button btnStart;

        public Form1()
        {
            // Настройки окна
            this.Text = "СМО M/M/1/0 с поломками (Без конструктора)";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            // Основной контейнер (Сетка 1 строка, 2 колонки)
            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            this.Controls.Add(mainLayout);

            // Левая панель (Параметры)
            FlowLayoutPanel leftPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(10) };
            mainLayout.Controls.Add(leftPanel, 0, 0);

            leftPanel.Controls.Add(new Label { Text = "Параметры", Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true });

            // Поля ввода
            leftPanel.Controls.Add(new Label { Text = "λ (Интенсивность прихода):", Margin = new Padding(0, 10, 0, 0) });
            txtLambda = new TextBox { Text = "2.0", Width = 150 };
            leftPanel.Controls.Add(txtLambda);

            leftPanel.Controls.Add(new Label { Text = "μ (Интенсивность обсл.):", Margin = new Padding(0, 10, 0, 0) });
            txtMu = new TextBox { Text = "3.0", Width = 150 };
            leftPanel.Controls.Add(txtMu);

            leftPanel.Controls.Add(new Label { Text = "Кол-во заявок (N):", Margin = new Padding(0, 10, 0, 0) });
            txtN = new TextBox { Text = "1000", Width = 150 };
            leftPanel.Controls.Add(txtN);

            btnStart = new Button { Text = "ПУСК", Width = 150, Height = 40, Margin = new Padding(0, 20, 0, 0), BackColor = Color.LightSkyBlue };
            btnStart.Click += BtnStart_Click;
            leftPanel.Controls.Add(btnStart);

            lblResults = new Label { Text = "Результаты появятся здесь...", AutoSize = true, Margin = new Padding(0, 20, 0, 0), Font = new Font("Consolas", 9) };
            leftPanel.Controls.Add(lblResults);

            // Правая панель (График)
            chart1 = new Chart { Dock = DockStyle.Fill };
            ChartArea area = new ChartArea("Main");
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 1;
            chart1.ChartAreas.Add(area);

            Legend leg = new Legend("Legend") { Docking = Docking.Top };
            chart1.Legends.Add(leg);

            mainLayout.Controls.Add(chart1, 1, 0);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            // 1. Предварительная очистка текста от лишних пробелов
            string rawLambda = txtLambda.Text.Trim().Replace(',', '.');
            string rawMu = txtMu.Text.Trim().Replace(',', '.');
            string rawN = txtN.Text.Trim();

            // 2. Проверка λ (Интенсивность прихода)
            if (!double.TryParse(rawLambda, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double lambda) || lambda <= 0)
            {
                MessageBox.Show("Ошибка: λ (лямбда) должна быть положительным числом больше 0.\nПри λ=0 заявки никогда не придут.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Проверка μ (Интенсивность обслуживания)
            if (!double.TryParse(rawMu, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double mu) || mu <= 0)
            {
                MessageBox.Show("Ошибка: μ (мю) должна быть положительным числом больше 0.\nПри μ=0 обслуживание будет длиться вечно.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 4. Проверка N (Количество заявок)
            if (!int.TryParse(rawN, out int totalRequests) || totalRequests <= 0)
            {
                MessageBox.Show("Ошибка: Количество заявок должно быть целым положительным числом.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ограничение на слишком большое число, чтобы UI не завис намертво
            if (totalRequests > 1000000)
            {
                MessageBox.Show("Ошибка: Слишком много заявок (макс. 1 000 000). Это может привести к долгому ожиданию.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 5. Если все проверки пройдены — запускаем расчет
            try
            {
                btnStart.Enabled = false; // Блокируем кнопку на время расчета
                this.Cursor = Cursors.WaitCursor; // Меняем курсор на "ожидание"

                Simulate(lambda, mu, totalRequests);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при моделировании: {ex.Message}", "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnStart.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        private void Simulate(double lambda, double mu, int totalRequests)
        {
            var rng = new MultRandom(DateTime.Now.Ticks & 0x7FFFFFFF);
            double GenExp(double rate) => -Math.Log(1 - Math.Max(rng.Next(), 1e-10)) / rate;

            double currentTime = 0;
            double lastEventTime = 0;

            // Состояния
            bool isBusy = false;
            bool isBroken = false;

            // Таймеры событий
            double nextArrival = GenExp(lambda);
            double serviceFinish = double.MaxValue;
            double nextBreakdownStart = 200.0;
            double nextBreakdownEnd = double.MaxValue;

            // Статистика
            int accepted = 0;
            int rejected = 0;
            int processed = 0;
            int breaksCount = 0;
            double timeBusy = 0; // Время когда прибор реально работал

            while (processed < totalRequests)
            {
                // Ищем время ближайшего события
                double nextEvent = Math.Min(nextArrival, Math.Min(serviceFinish, nextBreakdownStart));
                if (isBroken) nextEvent = Math.Min(nextArrival, nextBreakdownEnd);

                // Считаем время занятости до наступления события
                if (isBusy && !isBroken)
                    timeBusy += (nextEvent - lastEventTime);

                currentTime = nextEvent;
                lastEventTime = currentTime;

                // ОБРАБОТКА СОБЫТИЙ
                if (currentTime == nextBreakdownStart && !isBroken) // Поломка началась
                {
                    isBroken = true;
                    isBusy = false; // Прибор сломался - работа прервана
                    serviceFinish = double.MaxValue;
                    nextBreakdownEnd = currentTime + 20;
                    nextBreakdownStart = double.MaxValue;
                    breaksCount++;
                }
                else if (currentTime == nextBreakdownEnd && isBroken) // Ремонт окончен
                {
                    isBroken = false;
                    nextBreakdownStart = currentTime + 200;
                    nextBreakdownEnd = double.MaxValue;
                }
                else if (currentTime == nextArrival) // Пришла заявка
                {
                    processed++;
                    if (!isBusy && !isBroken)
                    {
                        isBusy = true;
                        accepted++;
                        serviceFinish = currentTime + GenExp(mu);
                    }
                    else
                    {
                        rejected++;
                    }
                    nextArrival = currentTime + GenExp(lambda);
                }
                else if (currentTime == serviceFinish) // Обслуживание завершено
                {
                    isBusy = false;
                    serviceFinish = double.MaxValue;
                }
            }

            // 3. Расчеты
            double rho = lambda / mu;
            double p0Theory = 1.0 / (1.0 + rho);
            double p1Theory = rho / (1.0 + rho);

            double p1Emp = timeBusy / currentTime;
            double p0Emp = 1.0 - p1Emp - (breaksCount * 20.0 / currentTime); // Упрощенно: 1 - занят - в ремонте

            // 4. Обновление GUI
            lblResults.Text = $"--- РЕЗУЛЬТАТЫ ---\n\n" +
                $"Принято: {accepted}\n" +
                $"Отказано: {rejected}\n" +
                $"Поломок: {breaksCount}\n" +
                $"Время модел.: {currentTime:F2}\n" +
                $"P_отказа (модель): {(double)rejected / totalRequests:F4}\n\n" +
                $"P0 (теория): {p0Theory:F4}\n" +
                $"P0 (модель): {p0Emp:F4}\n\n" +
                $"P1 (теория): {p1Theory:F4}\n" +
                $"P1 (модель): {p1Emp:F4}";

            DrawChart(p0Theory, p0Emp, p1Theory, p1Emp);
        }

        private void DrawChart(double p0T, double p0E, double p1T, double p1E)
        {
            chart1.Series.Clear();

            Series sTheory = new Series("Теория") { ChartType = SeriesChartType.Column, Color = Color.FromArgb(100, 65, 140, 240) };
            sTheory.Points.AddXY("P0", p0T);
            sTheory.Points.AddXY("P1", p1T);

            Series sModel = new Series("Модель") { ChartType = SeriesChartType.Column, Color = Color.Orange };
            sModel.Points.AddXY("P0", p0E);
            sModel.Points.AddXY("P1", p1E);

            chart1.Series.Add(sTheory);
            chart1.Series.Add(sModel);
        }
    }

    public class MultRandom
    {
        private long x_mult;
        private const long c = 132149;
        private const long m = (long)int.MaxValue;
        public MultRandom(long seed) { x_mult = seed; }
        public double Next()
        {
            x_mult = (c * x_mult) % m;
            return (double)x_mult / m;
        }
    }
}