using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace PolyclinicApp
{
    public partial class DoctorsForm : Form
    {
        private readonly DatabaseHelper dbHelper;
        private readonly int userId;
        private TableLayoutPanel tableLayout;
        private const int StartHour = 8;  // Начало рабочего дня
        private const int EndHour = 22;  // Конец рабочего дня (последний слот в 22:00)
        private const int SlotDuration = 2; // Длительность слота в часах

        public DoctorsForm(DatabaseHelper dbHelper, int userId)
        {
            InitializeComponent();
            this.dbHelper = dbHelper;
            this.userId = userId;
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "Запись к врачу";
            Size = new System.Drawing.Size(900, 600);
            CenterToScreen();

            var titleLabel = new Label
            {
                Text = "Запись к врачу",
                Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(350, 20)
            };
            Controls.Add(titleLabel);

            var backButton = new Button
            {
                Text = "Назад",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(80, 30)
            };
            backButton.Click += (s, ev) =>
            {
                var mainForm = new MainForm(dbHelper, userId);
                mainForm.Show();
                Hide();
            };
            Controls.Add(backButton);

            LoadDoctorsSchedule();
        }

        private void LoadDoctorsSchedule()
        {
            // Рассчитываем количество временных слотов (8:00-22:00 с шагом 2 часа)
            int slotCount = (EndHour - StartHour) / SlotDuration + 1;

            // Получаем список врачей из БД
            var doctors = dbHelper.GetDoctors();
            if (doctors == null || doctors.Count == 0)
            {
                MessageBox.Show("Нет доступных врачей");
                return;
            }

            tableLayout = new TableLayoutPanel
            {
                Location = new System.Drawing.Point(20, 70),
                Size = new System.Drawing.Size(850, 450), // Уменьшили высоту для лучшего отображения
                ColumnCount = slotCount + 1,
                RowCount = doctors.Count + 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                AutoScroll = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            // Настраиваем стили колонок
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            for (int i = 1; i <= slotCount; i++)
            {
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80f / slotCount));
            }

            // Настраиваем стили строк - все строки одинаковой высоты
            for (int i = 0; i <= doctors.Count; i++)
            {
                tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Установили фиксированную высоту для всех строк
            }

            // Заголовки столбцов
            tableLayout.Controls.Add(new Label
            {
                Text = "Врач (Специализация)",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
            }, 0, 0);

            // Временные слоты
            for (int i = 1; i <= slotCount; i++)
            {
                int hour = StartHour + (i - 1) * SlotDuration;
                tableLayout.Controls.Add(new Label
                {
                    Text = $"{hour}:00",
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                    Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
                }, i, 0);
            }

            // Заполняем данные по врачам
            for (int row = 0; row < doctors.Count; row++)
            {
                var doctor = doctors[row];

                // Имя врача и специализация
                var doctorLabel = new Label
                {
                    Text = $"{doctor.Name}\n({doctor.Specialization})",
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                    Font = new System.Drawing.Font("Arial", 10),
                    Margin = new Padding(3) // Добавляем небольшой отступ
                };
                tableLayout.Controls.Add(doctorLabel, 0, row + 1);

                // Доступные слоты
                for (int col = 1; col <= slotCount; col++)
                {
                    int hour = StartHour + (col - 1) * SlotDuration;
                    var time = $"{hour}:00";

                    // Проверяем, занят ли слот в БД
                    bool isBooked = dbHelper.IsTimeSlotBooked(doctor.Id, time);

                    Control cellControl;
                    if (!isBooked)
                    {
                        var button = new Button
                        {
                            Text = "Свободно",
                            Dock = DockStyle.Fill,
                            Tag = new { DoctorId = doctor.Id, Time = time },
                            BackColor = System.Drawing.Color.LightGreen,
                            Font = new System.Drawing.Font("Arial", 9),
                            Margin = new Padding(2) // Добавляем небольшой отступ
                        };
                        button.Click += TimeSlotButton_Click;
                        cellControl = button;
                    }
                    else
                    {
                        cellControl = new Label
                        {
                            Text = "Занято",
                            Dock = DockStyle.Fill,
                            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                            BackColor = System.Drawing.Color.LightPink,
                            Font = new System.Drawing.Font("Arial", 9),
                            Margin = new Padding(2) // Добавляем небольшой отступ
                        };
                    }
                    tableLayout.Controls.Add(cellControl, col, row + 1);
                }
            }

            // Добавляем скроллинг
            var scrollPanel = new Panel
            {
                Location = new System.Drawing.Point(20, 70),
                Size = new System.Drawing.Size(850, 450),
                AutoScroll = true
            };
            scrollPanel.Controls.Add(tableLayout);
            Controls.Add(scrollPanel);
        }

        private void TimeSlotButton_Click(object sender, EventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                dynamic tag = button.Tag;
                int doctorId = tag.DoctorId;
                string time = tag.Time;

                var confirmForm = new ConfirmAppointmentForm(dbHelper, userId, doctorId, time);
                confirmForm.Show();
                Hide();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Name = "DoctorsForm";
            this.ResumeLayout(false);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Application.Exit();
        }
    }
}