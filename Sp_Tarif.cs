using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace Kersovoi_Strahovka_Malev
{
    public partial class Sp_Tarif : Form
    {
        // Строка подключения к базе данных MySQL
        string connectionStr = @"host=localhost;
                                 uid=root;
                                 pwd=root;
                                 database=kursach;";

        // Переменная для хранения ID текущего выбранного тарифа
        private int currentRateId = -1;

        // Конструктор формы
        public Sp_Tarif()
        {
            InitializeComponent();
        }

        // Обработчик события загрузки формы
        private void Sp_Tarif_Load(object sender, EventArgs e)
        {
            // Настройка внешнего вида и поведения DataGridView
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Автоматическое изменение размера столбцов
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Выделение всей строки
            dataGridView1.MultiSelect = false; // Запрет множественного выбора
            dataGridView1.ReadOnly = true; // Только для чтения
            dataGridView1.AllowUserToAddRows = false; // Запрет добавления строк пользователем
            dataGridView1.RowHeadersVisible = false; // Скрытие заголовков строк

            dataGridView1.ClearSelection(); // Снятие выделения
            LoadCoef(); // Загрузка данных из базы
        }

        // Метод установки русских названий для столбцов DataGridView
        private void SetRussianColumnNames(DataGridView grid)
        {
            try
            {
                // Словарь соответствия английских имен столбцов русским названиям
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    {"ID_Rate", "Номер тарифа"},
                    {"Coefficient", "Коэффициент"}
                };

                // Проход по всем столбцам и замена заголовков
                foreach (var col in grid.Columns)
                {
                    DataGridViewColumn column = (DataGridViewColumn)col;
                    if (columnNames.ContainsKey(column.Name))
                    {
                        column.HeaderText = columnNames[column.Name];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при установке заголовков: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик клика по ячейке DataGridView
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Проверка, что кликнули не по заголовку
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                FillTextBoxesFromRow(row); // Заполнение полей данными из строки
            }
        }

        // Метод заполнения текстовых полей данными из выбранной строки
        private void FillTextBoxesFromRow(DataGridViewRow row)
        {
            try
            {
                // Получение ID тарифа из выбранной строки
                if (row.Cells["ID_Rate"].Value != null && row.Cells["ID_Rate"].Value != DBNull.Value)
                {
                    currentRateId = Convert.ToInt32(row.Cells["ID_Rate"].Value);
                }
                else
                {
                    currentRateId = -1; // Сброс ID при отсутствии данных
                }

                // Заполнение поля коэффициента (замена запятой на точку для корректного отображения)
                if (row.Cells["Coefficient"].Value != null && row.Cells["Coefficient"].Value != DBNull.Value)
                {
                    textBox1.Text = row.Cells["Coefficient"].Value.ToString().Replace(',', '.');
                }
                else
                {
                    textBox1.Text = ""; // Очистка поля при отсутствии данных
                }

                button3.Enabled = true; // Активация кнопки редактирования
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при заполнении полей: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод очистки всех полей формы
        private void ClearFields()
        {
            textBox1.Text = ""; // Очистка поля коэффициента
            currentRateId = -1; // Сброс ID
            dataGridView1.ClearSelection(); // Снятие выделения в таблице
        }

        // Обработчик изменения текста в поле коэффициента
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string text = textBox1.Text;
            string filtered = "";
            bool dotFound = false; // Флаг наличия десятичной точки

            // Фильтрация ввода: разрешены только цифры и одна точка
            foreach (char c in text)
            {
                if (char.IsDigit(c)) // Добавление цифр
                {
                    filtered += c;
                }
                else if (c == '.' && !dotFound && filtered.Length > 0) // Добавление точки (только одной и не в начале)
                {
                    filtered += c;
                    dotFound = true;
                }
            }

            // Если текст изменился после фильтрации
            if (text != filtered)
            {
                // Корректировка позиции курсора
                int cursorPos = textBox1.SelectionStart - (text.Length - filtered.Length);
                textBox1.Text = filtered; // Установка отфильтрованного текста
                textBox1.SelectionStart = Math.Max(0, cursorPos); // Восстановление позиции курсора
            }
        }

        // Обработчик нажатия клавиш в поле коэффициента
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешены только: управляющие символы, цифры, точка
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true; // Блокировка неразрешенных символов
            }

            // Запрет точки в начале строки или если строка пустая
            if (e.KeyChar == '.' && (textBox1.SelectionStart == 0 || textBox1.Text.Length == 0))
            {
                e.Handled = true;
            }

            // Запрет второй точки, если точка уже есть
            if (e.KeyChar == '.' && textBox1.Text.Contains("."))
            {
                e.Handled = true;
            }

            // Запрет запятой (используем точку как десятичный разделитель)
            if (e.KeyChar == ',')
            {
                e.Handled = true;
            }
        }

        // Обработчик кнопки "Закрыть"
        private void button5_Click_1(object sender, EventArgs e)
        {
            this.Close(); // Закрытие формы
        }

        // Обработчик кнопки "Редактировать"
        private void button3_Click(object sender, EventArgs e)
        {
            // Проверка, что тариф выбран
            if (currentRateId == -1)
            {
                MessageBox.Show("Выберите запись для редактирования.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string coefficientText = textBox1.Text.Trim();

            // Валидация ввода: проверка на пустое значение
            if (string.IsNullOrEmpty(coefficientText))
            {
                MessageBox.Show("Введите коэффициент.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            coefficientText = coefficientText.Replace('.', ','); // Замена точки на запятую для SQL

            // Валидация ввода: проверка корректности числа
            if (!decimal.TryParse(coefficientText, out decimal coefficient))
            {
                MessageBox.Show("Введите корректный коэффициент (например: 2.22).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();

                    // Проверка уникальности коэффициента (исключая текущую запись)
                    MySqlCommand checkCmd = new MySqlCommand("SELECT COUNT(*) FROM Basic_Rate WHERE Coefficient = @coef AND ID_Rate != @id", con);
                    checkCmd.Parameters.AddWithValue("@coef", coefficient);
                    checkCmd.Parameters.AddWithValue("@id", currentRateId);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Такой коэффициент уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // SQL-запрос для обновления коэффициента
                    string query = "UPDATE Basic_Rate SET Coefficient = @coef WHERE ID_Rate = @id";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@coef", coefficient);
                    cmd.Parameters.AddWithValue("@id", currentRateId);
                    cmd.ExecuteNonQuery(); // Выполнение запроса
                }

                LoadCoef(); // Обновление таблицы
                ClearFields(); // Очистка полей
                MessageBox.Show("Коэффициент успешно обновлен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении коэффициента: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик кнопки "Удалить"
        private void button2_Click(object sender, EventArgs e)
        {
            // Проверка, что строка выбрана
            if (dataGridView1.CurrentRow == null || currentRateId == -1)
            {
                MessageBox.Show("Выберите строку для удаления.");
                return;
            }

            // Подтверждение удаления
            var result = MessageBox.Show("Удалить выбранный базовый коэффициент?", "Подтверждение", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                DeleteCoefFromDatabase(currentRateId); // Удаление коэффициента
                LoadCoef(); // Обновление таблицы
                ClearFields(); // Очистка полей
                UpdateRowCount(); // Обновление счетчика записей
            }
        }

        // Метод удаления коэффициента из базы данных
        private void DeleteCoefFromDatabase(int id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    // SQL-запрос для удаления коэффициента
                    MySqlCommand cmd = new MySqlCommand("DELETE FROM Basic_Rate WHERE ID_Rate = @id", con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery(); // Выполнение запроса
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении: " + ex.Message);
            }
        }

        // Метод загрузки коэффициентов из базы данных
        private void LoadCoef()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    // SQL-запрос для получения всех коэффициентов из таблицы Basic_Rate
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM Basic_Rate", con);
                    DataTable dt = new DataTable();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    da.Fill(dt);
                    dataGridView1.DataSource = dt; // Привязка DataTable к DataGridView

                    // Настройка свойств DataGridView
                    dataGridView1.ReadOnly = true;
                    dataGridView1.AllowUserToAddRows = false;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    SetRussianColumnNames(dataGridView1); // Установка русских названий столбцов

                    dataGridView1.ClearSelection(); // Снятие выделения

                    UpdateRowCount(); // Обновление счетчика записей
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик кнопки "Добавить"
        private void button1_Click(object sender, EventArgs e)
        {
            string coefficientText = textBox1.Text.Trim();

            // Валидация ввода: проверка на пустое значение
            if (string.IsNullOrEmpty(coefficientText))
            {
                MessageBox.Show("Введите базовый коэффициент.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            coefficientText = coefficientText.Replace('.', ','); // Замена точки на запятую для SQL

            // Валидация ввода: проверка корректности числа
            if (!decimal.TryParse(coefficientText, out decimal coefficient))
            {
                MessageBox.Show("Введите корректный коэффициент (например: 2.22).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionStr))
            {
                try
                {
                    con.Open();

                    // Проверка уникальности коэффициента
                    string checkQuery = "SELECT COUNT(*) FROM Basic_Rate WHERE Coefficient = @coef";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                    checkCmd.Parameters.AddWithValue("@coef", coefficient);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Такой базовый коэффициент уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // SQL-запрос для добавления нового коэффициента
                    string insertQuery = "INSERT INTO Basic_Rate (Coefficient) VALUES (@coef)";
                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, con);
                    insertCmd.Parameters.AddWithValue("@coef", coefficient);
                    insertCmd.ExecuteNonQuery(); // Выполнение запроса

                    MessageBox.Show("Базовый коэффициент успешно добавлен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields(); // Очистка полей
                    LoadCoef(); // Обновление таблицы
                    UpdateRowCount(); // Обновление счетчика записей
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении базового коэффициента: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Обработчик клика по DataGridView (проверка клика вне строк)
        private void dataGridView1_Click(object sender, EventArgs e)
        {
            // Если клик произошел не по строке (по пустой области), очистить поля
            if (dataGridView1.HitTest((e as MouseEventArgs).X, (e as MouseEventArgs).Y).RowIndex == -1)
            {
                ClearFields();
            }
        }

        // Метод обновления счетчика записей в таблице
        private void UpdateRowCount()
        {
            label1.Text = "Количество записей: " + dataGridView1.Rows.Count.ToString();
        }

        // Обработчик клика по метке (заглушка)
        private void label1_Click(object sender, EventArgs e)
        {
            // Можно оставить пустым, так как это просто отображаемая информация
        }
    }
}