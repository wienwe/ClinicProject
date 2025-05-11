using System;
using System.Windows.Forms;

namespace PolyclinicApp
{
    public partial class AppointmentSuccessForm : Form
    {
        private readonly DatabaseHelper dbHelper;
        private readonly int userId;

        public AppointmentSuccessForm(DatabaseHelper dbHelper, int userId)
        {
            InitializeComponent();
            this.dbHelper = dbHelper;
            this.userId = userId;
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "Запись подтверждена";
            Size = new System.Drawing.Size(500, 300);
            CenterToScreen();

            var titleLabel = new Label
            {
                Text = "Запись подтверждена!",
                Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(150, 80)
            };
            Controls.Add(titleLabel);

            var backButton = new Button
            {
                Text = "Вернуться на главную",
                Location = new System.Drawing.Point(150, 150),
                Size = new System.Drawing.Size(200, 50)
            };
            Controls.Add(backButton);

            backButton.Click += (s, ev) =>
            {
                var mainForm = new MainForm(dbHelper, userId);
                mainForm.Show();
                Hide();
            };
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AppointmentSuccessForm
            // 
            this.ClientSize = new System.Drawing.Size(482, 453);
            this.Name = "AppointmentSuccessForm";
            this.ResumeLayout(false);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Application.Exit();
        }
    }
}