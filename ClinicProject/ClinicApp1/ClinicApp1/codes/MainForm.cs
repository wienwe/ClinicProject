using System;
using System.Windows.Forms;

namespace PolyclinicApp
{
    public partial class MainForm : Form
    {
        private readonly DatabaseHelper dbHelper;
        private readonly int userId;

        public MainForm(DatabaseHelper dbHelper, int userId)
        {
            InitializeComponent();
            this.dbHelper = dbHelper;
            this.userId = userId;
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "Главная";
            Size = new System.Drawing.Size(800, 600);
            CenterToScreen();

            var titleLabel = new Label
            {
                Text = "ГЛАВНАЯ",
                Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(350, 30)
            };
            Controls.Add(titleLabel);

            var bookAppointmentButton = new Button
            {
                Text = "Запись к врачу",
                Location = new System.Drawing.Point(50, 100),
                Size = new System.Drawing.Size(200, 50)
            };
            Controls.Add(bookAppointmentButton);

            var settingsButton = new Button
            {
                Text = "Настройки",
                Location = new System.Drawing.Point(50, 170),
                Size = new System.Drawing.Size(200, 50)
            };
            Controls.Add(settingsButton);

            var descriptionLabel = new Label
            {
                Text = "Онлайн-запись в клинику\n\nУдобный сервис для записи на прием к врачу",
                Location = new System.Drawing.Point(300, 100),
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 12)
            };
            Controls.Add(descriptionLabel);

            bookAppointmentButton.Click += (s, ev) =>
            {
                var doctorsForm = new DoctorsForm(dbHelper, userId);
                doctorsForm.Show();
                Hide();
            };

            settingsButton.Click += (s, ev) =>
            {
                MessageBox.Show("Настройки пока не реализованы");
            };
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(782, 553);
            this.Name = "MainForm";
            this.ResumeLayout(false);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Application.Exit();
        }
    }
}