using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using PolyclinicApp;

namespace PolyclinicApp
{
    public partial class AuthForm : Form
    {
        private readonly DatabaseHelper dbHelper;
        private bool isLoginMode = false;
        private TextBox fullNameTextBox;
        private TextBox phoneTextBox;
        private ComboBox genderComboBox;
        private DateTimePicker birthDatePicker;

        public AuthForm(DatabaseHelper dbHelper)
        {
            InitializeComponent();
            this.dbHelper = dbHelper;
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "Авторизация";
            Size = new System.Drawing.Size(500, 400);
            CenterToScreen();

            var titleLabel = new Label
            {
                Text = "АВТОРИЗАЦИЯ",
                Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(150, 30)
            };
            Controls.Add(titleLabel);

            var fullNameLabel = new Label { Text = "ФИО:", Location = new System.Drawing.Point(50, 100), AutoSize = true };
            fullNameTextBox = new TextBox { Location = new System.Drawing.Point(150, 100), Width = 250 };
            Controls.Add(fullNameLabel);
            Controls.Add(fullNameTextBox);

            var phoneLabel = new Label { Text = "Телефон:", Location = new System.Drawing.Point(50, 140), AutoSize = true };
            phoneTextBox = new TextBox { Location = new System.Drawing.Point(150, 140), Width = 250 };
            phoneTextBox.TextChanged += (s, e) => FormatPhoneNumber();
            Controls.Add(phoneLabel);
            Controls.Add(phoneTextBox);

            var genderLabel = new Label { Text = "Пол:", Location = new System.Drawing.Point(50, 180), AutoSize = true };
            genderComboBox = new ComboBox { Location = new System.Drawing.Point(150, 180), Width = 250 };
            genderComboBox.Items.AddRange(new[] { "Мужской", "Женский" });
            Controls.Add(genderLabel);
            Controls.Add(genderComboBox);

            var birthDateLabel = new Label { Text = "Дата рождения:", Location = new System.Drawing.Point(50, 220), AutoSize = true };
            birthDatePicker = new DateTimePicker
            {
                Location = new System.Drawing.Point(150, 220),
                Width = 250,
                MaxDate = DateTime.Today.AddYears(-14),
                MinDate = DateTime.Today.AddYears(-100)
            };
            Controls.Add(birthDateLabel);
            Controls.Add(birthDatePicker);

            var registerButton = new Button { Text = "Регистрация", Location = new System.Drawing.Point(150, 270), Width = 100 };
            var loginButton = new Button { Text = "Вход", Location = new System.Drawing.Point(260, 270), Width = 100 };
            Controls.Add(registerButton);
            Controls.Add(loginButton);

            var switchModeButton = new Button { Text = "У меня есть аккаунт", Location = new System.Drawing.Point(150, 310), Width = 210 };
            Controls.Add(switchModeButton);

            registerButton.Click += (s, ev) =>
            {
                if (!ValidateInputs())
                    return;

                try
                {
                    Console.WriteLine($"Попытка регистрации пользователя:");
                    Console.WriteLine($"ФИО: {fullNameTextBox.Text}");
                    Console.WriteLine($"Телефон: {phoneTextBox.Text}");
                    Console.WriteLine($"Пол: {genderComboBox.SelectedItem}");
                    Console.WriteLine($"Дата рождения: {birthDatePicker.Value.ToShortDateString()}");

                    var userId = dbHelper.RegisterUser(
                        fullNameTextBox.Text,
                        phoneTextBox.Text,
                        genderComboBox.SelectedItem.ToString(),
                        birthDatePicker.Value);

                    MessageBox.Show("Регистрация успешна!");
                    OpenMainForm(userId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка регистрации: {ex.Message}");
                }
            };

            loginButton.Click += (s, ev) =>
            {
                if (isLoginMode)
                {
                    if (!ValidatePhoneNumber(phoneTextBox.Text))
                    {
                        MessageBox.Show("Введите корректный российский номер телефона");
                        return;
                    }

                    var userId = dbHelper.AuthenticateUser(phoneTextBox.Text);
                    if (userId.HasValue)
                    {
                        OpenMainForm(userId.Value);
                    }
                    else
                    {
                        MessageBox.Show("Пользователь не найден");
                    }
                }
                else
                {
                    MessageBox.Show("Нажмите 'У меня есть аккаунт' для входа");
                }
            };

            switchModeButton.Click += (s, ev) =>
            {
                isLoginMode = !isLoginMode;
                fullNameLabel.Visible = !isLoginMode;
                fullNameTextBox.Visible = !isLoginMode;
                genderLabel.Visible = !isLoginMode;
                genderComboBox.Visible = !isLoginMode;
                birthDateLabel.Visible = !isLoginMode;
                birthDatePicker.Visible = !isLoginMode;
                registerButton.Visible = !isLoginMode;

                switchModeButton.Text = isLoginMode ? "У меня нет аккаунта" : "У меня есть аккаунт";
                titleLabel.Text = isLoginMode ? "ВХОД" : "АВТОРИЗАЦИЯ";
            };
        }

        private bool ValidateInputs()
        {
            // Проверка ФИО
            if (string.IsNullOrWhiteSpace(fullNameTextBox.Text) || !IsValidFullName(fullNameTextBox.Text))
            {
                MessageBox.Show("Введите корректное ФИО (только буквы и пробелы, минимум 2 слова)");
                return false;
            }

            // Проверка телефона
            if (!ValidatePhoneNumber(phoneTextBox.Text))
            {
                MessageBox.Show("Введите корректный российский номер телефона (+7XXXXXXXXXX или 8XXXXXXXXXX)");
                return false;
            }

            // Проверка пола
            if (genderComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите пол");
                return false;
            }

            // Проверка даты рождения
            var age = DateTime.Today.Year - birthDatePicker.Value.Year;
            if (birthDatePicker.Value.Date > DateTime.Today.AddYears(-age)) age--;

            if (age < 14)
            {
                MessageBox.Show("Пользователь должен быть старше 14 лет");
                return false;
            }
            else if (age > 100)
            {
                MessageBox.Show("Пользователь должен быть младше 100 лет");
                return false;
            }

            return true;
        }

        private bool IsValidFullName(string fullName)
        {
            // Проверка что ФИО содержит только буквы и пробелы, и состоит минимум из 2 слов
            return Regex.IsMatch(fullName, @"^[а-яА-ЯёЁa-zA-Z\s]{2,}(?:\s+[а-яА-ЯёЁa-zA-Z]{2,})+$");
        }

        private bool ValidatePhoneNumber(string phone)
        {
            // Проверка российского номера телефона (+7XXXXXXXXXX или 8XXXXXXXXXX)
            return Regex.IsMatch(phone, @"^(\+7|8)[0-9]{10}$");
        }

        private void FormatPhoneNumber()
        {
            // Удаляем все нецифровые символы
            string digitsOnly = Regex.Replace(phoneTextBox.Text, @"[^\d]", "");

            // Ограничиваем длину
            if (digitsOnly.Length > 11)
                digitsOnly = digitsOnly.Substring(0, 11);

            // Форматируем номер
            if (digitsOnly.Length >= 1)
            {
                if (digitsOnly[0] == '7' || digitsOnly[0] == '8')
                {
                    phoneTextBox.Text = $"+7{digitsOnly.Substring(1)}";
                }
                else
                {
                    phoneTextBox.Text = $"+7{digitsOnly}";
                }
            }

            // Устанавливаем курсор в конец
            phoneTextBox.SelectionStart = phoneTextBox.Text.Length;
        }

        private void OpenMainForm(int userId)
        {
            var mainForm = new MainForm(dbHelper, userId);
            mainForm.Show();
            Hide();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AuthForm
            // 
            this.ClientSize = new System.Drawing.Size(482, 453);
            this.Name = "AuthForm";
            this.ResumeLayout(false);
        }
    }
}