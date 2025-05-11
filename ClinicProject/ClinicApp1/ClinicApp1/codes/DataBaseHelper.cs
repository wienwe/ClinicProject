using Npgsql;
using System;
using System.Collections.Generic;

namespace PolyclinicApp
{
    public class DatabaseHelper
    {
        public string ConnectionString { get; private set; }

        public DatabaseHelper(string host, string database, string username, string password)
        {
            ConnectionString = $"Host={host};Database={database};Username={username};Password={password}";
        }

        public void InitializeDatabase()
        {
            try
            {
                using (var conn = new NpgsqlConnection(ConnectionString))
                {
                    Log("Попытка подключения к базе данных...");
                    conn.Open();
                    Log("Подключение к базе данных успешно установлено");

                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;

                        // 1. Создание таблицы users
                        Log("Создание таблицы users (если не существует)");
                        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS users (
                                    user_id SERIAL PRIMARY KEY,
                                    full_name VARCHAR(100) NOT NULL,
                                    phone VARCHAR(20) NOT NULL UNIQUE,
                                    gender VARCHAR(10) NOT NULL,
                                    birth_date DATE NOT NULL);";
                        cmd.ExecuteNonQuery();

                        // 2. Создание таблицы doctors
                        Log("Создание таблицы doctors (если не существует)");
                        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS doctors (
                                    doctor_id SERIAL PRIMARY KEY,
                                    name VARCHAR(100) NOT NULL UNIQUE,
                                    specialization VARCHAR(100) NOT NULL);";
                        cmd.ExecuteNonQuery();

                        // 3. Создание таблицы schedule
                        Log("Создание таблицы schedule (если не существует)");
                        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS schedule (
                                    schedule_id SERIAL PRIMARY KEY,
                                    doctor_id INTEGER NOT NULL REFERENCES doctors(doctor_id),
                                    time TIME NOT NULL,
                                    is_available BOOLEAN DEFAULT TRUE,
                                    UNIQUE (doctor_id, time));";
                        cmd.ExecuteNonQuery();

                        // 4. Создание таблицы appointments
                        Log("Создание таблицы appointments (если не существует)");
                        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS appointments (
                                    appointment_id SERIAL PRIMARY KEY,
                                    user_id INTEGER NOT NULL REFERENCES users(user_id),
                                    schedule_id INTEGER NOT NULL REFERENCES schedule(schedule_id),
                                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                    UNIQUE (user_id, schedule_id));";
                        cmd.ExecuteNonQuery();

                        // 5. Добавление тестовых врачей (всегда)
                        Log("Добавление тестовых данных врачей");
                        cmd.CommandText = @"INSERT INTO doctors (name, specialization) VALUES 
                                    ('Иванов И.И.', 'Терапевт'),
                                    ('Петров П.П.', 'Хирург'),
                                    ('Сидорова С.С.', 'Офтальмолог')
                                    ON CONFLICT (name) DO NOTHING;";
                        int doctorsAdded = cmd.ExecuteNonQuery();
                        Log($"Добавлено/пропущено врачей: {doctorsAdded}");

                        // 6. Добавление тестовых расписаний (всегда)
                        Log("Добавление тестовых расписаний");
                        var times = new[] { "08:00", "10:00", "12:00", "14:00", "16:00", "18:00", "20:00" };
                        int slotsAdded = 0;

                        for (int i = 1; i <= 3; i++)
                        {
                            foreach (var time in times)
                            {
                                cmd.CommandText = @"INSERT INTO schedule (doctor_id, time) 
                                           VALUES (@doctorId, @time)
                                           ON CONFLICT (doctor_id, time) DO NOTHING";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@doctorId", i);
                                cmd.Parameters.AddWithValue("@time", TimeSpan.Parse(time));

                                slotsAdded += cmd.ExecuteNonQuery();
                            }
                        }
                        Log($"Добавлено/пропущено слотов: {slotsAdded}");

                        // 7. Добавление тестового пользователя (если нет пользователей)
                        Log("Проверка наличия тестового пользователя");
                        cmd.CommandText = "SELECT COUNT(*) FROM users";
                        var userCount = Convert.ToInt64(cmd.ExecuteScalar());

                        if (userCount == 0)
                        {
                            Log("Добавление тестового пользователя");
                            cmd.CommandText = @"INSERT INTO users (full_name, phone, gender, birth_date) 
                                         VALUES ('Тестовый Пользователь', '+79990001122', 'Мужской', '1980-01-01')";
                            cmd.ExecuteNonQuery();
                            Log("Тестовый пользователь добавлен");
                        }
                        else
                        {
                            Log($"В базе уже есть {userCount} пользователей, тестовый не добавлен");
                        }
                    }
                }
                Log("Инициализация базы данных завершена успешно");
            }
            catch (Exception ex)
            {
                Log($"Ошибка при инициализации базы данных: {ex.Message}");
                throw; // Перебрасываем исключение дальше
            }
        }

        private void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }

        public int RegisterUser(string fullName, string phone, string gender, DateTime birthDate)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO users (full_name, phone, gender, birth_date) 
                                     VALUES (@fullName, @phone, @gender, @birthDate) 
                                     RETURNING user_id";
                    cmd.Parameters.AddWithValue("@fullName", fullName);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@gender", gender);
                    cmd.Parameters.AddWithValue("@birthDate", birthDate);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int? AuthenticateUser(string phone)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT user_id FROM users WHERE phone = @phone";
                    cmd.Parameters.AddWithValue("@phone", phone);

                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : (int?)null;
                }
            }
        }

        public List<Doctor> GetDoctors()
        {
            var doctors = new List<Doctor>();
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT doctor_id, name, specialization FROM doctors", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        doctors.Add(new Doctor
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Specialization = reader.GetString(2)
                        });
                    }
                }
            }
            return doctors;
        }

        public List<ScheduleSlot> GetAvailableSlots(int doctorId)
        {
            var slots = new List<ScheduleSlot>();
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT s.schedule_id, s.time 
                                       FROM schedule s
                                       LEFT JOIN appointments a ON s.schedule_id = a.schedule_id
                                       WHERE s.doctor_id = @doctorId AND a.schedule_id IS NULL
                                       ORDER BY s.time";
                    cmd.Parameters.AddWithValue("@doctorId", doctorId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            slots.Add(new ScheduleSlot
                            {
                                Id = reader.GetInt32(0),
                                Time = reader.GetTimeSpan(1).ToString(@"hh\:mm")
                            });
                        }
                    }
                }
            }
            return slots;
        }

        public bool IsTimeSlotBooked(int doctorId, string time)
        {
            try
            {
                using (var conn = new NpgsqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = @"SELECT COUNT(*) 
                                           FROM appointments a
                                           JOIN schedule s ON a.schedule_id = s.schedule_id
                                           WHERE s.doctor_id = @doctorId AND s.time = @time";
                        cmd.Parameters.AddWithValue("@doctorId", doctorId);
                        cmd.Parameters.AddWithValue("@time", TimeSpan.Parse(time));

                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Ошибка при проверке занятости слота: {ex.Message}");
                return true; // В случае ошибки считаем слот занятым
            }
        }

        public bool CreateAppointment(int userId, int scheduleId)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO appointments (user_id, schedule_id) VALUES (@userId, @scheduleId)";
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@scheduleId", scheduleId);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }
    }

    public class Doctor
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
    }

    public class ScheduleSlot
    {
        public int Id { get; set; }
        public string Time { get; set; } = string.Empty;
    }
}