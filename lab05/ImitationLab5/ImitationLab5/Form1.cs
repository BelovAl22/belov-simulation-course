using System;
using System.Drawing;
using System.Windows.Forms;

namespace ImitationLab5
{
    public class Form1 : Form
    {
        private TabControl tabControl;

        private Button btnYesNo;
        private Label lblYesNo;

        private Button btnMagic;
        private Label lblMagic;
        private TextBox txtSeed;
        private Panel panelBall;
        private string currentAnswer = "...";

        private MultiplicativeRandom myRand;

        public Form1()
        {
            myRand = new MultiplicativeRandom(67);
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Моделирование случайных событий";
            this.Size = new Size(500, 300);

            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            Label lblSeed = new Label();
            lblSeed.Text = "Seed:";
            lblSeed.Location = new Point(20, 30);
            lblSeed.Width = 45;

            txtSeed = new TextBox();
            txtSeed.Location = new Point(65, 28);
            txtSeed.Width = 80;
            txtSeed.Text = "67";

            this.Controls.Add(lblSeed);
            this.Controls.Add(txtSeed);

            // ===== TAB 1: Да / Нет =====
            var tab1 = new TabPage("Да / Нет");

            btnYesNo = new Button();
            btnYesNo.Text = "Получить ответ";
            btnYesNo.Location = new Point(150, 50);
            btnYesNo.Click += BtnYesNo_Click;

            lblYesNo = new Label();
            lblYesNo.Text = "...";
            lblYesNo.Font = new Font("Arial", 16);
            lblYesNo.Location = new Point(180, 120);
            lblYesNo.AutoSize = true;

            tab1.Controls.Add(btnYesNo);
            tab1.Controls.Add(lblYesNo);

            // ===== TAB 2: Шар =====
            var tab2 = new TabPage("Шар предсказаний");

            btnMagic = new Button();
            btnMagic.Text = "Предсказать";
            btnMagic.Location = new Point(150, 50);
            btnMagic.Click += BtnMagic_Click;

            lblMagic = new Label();
            lblMagic.Text = "...";
            lblMagic.Font = new Font("Arial", 14);
            lblMagic.Location = new Point(120, 120);
            lblMagic.AutoSize = true;

            tab2.Controls.Add(btnMagic);
            tab2.Controls.Add(lblMagic);

            tabControl.TabPages.Add(tab1);
            tabControl.TabPages.Add(tab2);

            this.Controls.Add(tabControl);
        }

        // ГЕНЕРАТОР
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

        // ДА / НЕТ
        private void BtnYesNo_Click(object sender, EventArgs e)
        {
            double r = myRand.NextDouble();

            if (r < 0.5)
                lblYesNo.Text = "Да";
            else
                lblYesNo.Text = "Нет";
        }

        // ШАР ПРЕДСКАЗАНИЙ
        private void BtnMagic_Click(object sender, EventArgs e)
        {
            double r = myRand.NextDouble();

            string result;

            if (r < 0.20) result = "Да";
            else if (r < 0.40) result = "Нет";
            else if (r < 0.55) result = "Скорее да";
            else if (r < 0.70) result = "Скорее нет";
            else if (r < 0.80) result = "Не знаю";
            else if (r < 0.90) result = "Спроси позже";
            else if (r < 0.95) result = "Определённо да";
            else result = "Определённо нет";

            lblMagic.Text = result;
        }
    }
}