using System;
using System.Windows.Forms;

namespace PolyclinicApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var dbHelper = new DatabaseHelper("localhost", "polyclinic", "postgres", "7994821Kk.");
                dbHelper.InitializeDatabase();
                Application.Run(new AuthForm(dbHelper));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске приложения: {ex.Message}");
            }
        }
    }
}