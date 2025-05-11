using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace PolyclinicApp
{
    public partial class ConfirmAppointmentForm : Form
    {
        private readonly DatabaseHelper dbHelper;
        private readonly int userId;
        private readonly int doctorId;
        private readonly string time;
        private bool isConfirmed = false;

        public ConfirmAppointmentForm(DatabaseHelper dbHelper, int userId, int doctorId, string time)
        {
            InitializeComponent();
            this.dbHelper = dbHelper;
            this.userId = userId;
            this.doctorId = doctorId;
            this.time = time;
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "Подтверждение записи";
            Size = new System.Drawing.Size(500, 300);
            CenterToScreen();

            var titleLabel = new Label
            {
                Text = "Подтверждение записи на прием",
                Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(100, 30)
            };
            Controls.Add(titleLabel);

            var doctorTimeLabel = new Label
            {
                Text = $"Врач: {doctorId}, Время: {time}",
                Font = new System.Drawing.Font("Arial", 12),
                AutoSize = true,
                Location = new System.Drawing.Point(100, 80)
            };
            Controls.Add(doctorTimeLabel);

            var confirmCheckBox = new CheckBox
            {
                Text = "Подтверждаю",
                Location = new System.Drawing.Point(100, 130),
                AutoSize = true
            };
            Controls.Add(confirmCheckBox);

            var bookButton = new Button
            {
                Text = "Записаться",
                Location = new System.Drawing.Point(100, 170),
                Size = new System.Drawing.Size(150, 40),
                Enabled = false
            };
            Controls.Add(bookButton);

            var backButton = new Button
            {
                Text = "Назад",
                Location = new System.Drawing.Point(300, 170),
                Size = new System.Drawing.Size(150, 40)
            };
            Controls.Add(backButton);

            confirmCheckBox.CheckedChanged += (s, ev) =>
            {
                isConfirmed = confirmCheckBox.Checked;
                bookButton.Enabled = isConfirmed;
            };

            bookButton.Click += async (s, ev) =>
            {
                if (isConfirmed)
                {
                    try
                    {
                        int scheduleId = 0;
                        using (var conn = new NpgsqlConnection(dbHelper.ConnectionString))
                        {
                            conn.Open();
                            using (var cmd = new NpgsqlCommand())
                            {
                                cmd.Connection = conn;
                                cmd.CommandText = @"SELECT s.schedule_id 
                                                 FROM schedule s
                                                 WHERE s.doctor_id = @doctorId AND s.time = @time";
                                cmd.Parameters.AddWithValue("@doctorId", doctorId);
                                cmd.Parameters.AddWithValue("@time", TimeSpan.Parse(time));

                                scheduleId = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }

                        if (scheduleId > 0 && dbHelper.CreateAppointment(userId, scheduleId))
                        {
                            var successForm = new AppointmentSuccessForm(dbHelper, userId);
                            successForm.Show();
                            Hide();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось записаться. Возможно время уже занято.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            };

            backButton.Click += (s, ev) =>
            {
                var doctorsForm = new DoctorsForm(dbHelper, userId);
                doctorsForm.Show();
                Hide();
            };
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ConfirmAppointmentForm
            // 
            this.ClientSize = new System.Drawing.Size(482, 453);
            this.Name = "ConfirmAppointmentForm";
            this.ResumeLayout(false);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Application.Exit();
        }
    }
}