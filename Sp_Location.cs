using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Kersovoi_Strahovka_Malev
{
    public partial class Sp_Location : Form
    {
        // Строка подключения к базе данных MySQL
        string connectionStr = @"host=localhost;
                                 uid=root;
                                 pwd=root;
                                 database=kursach;";

        // Переменная для хранения ID текущей выбранной записи
        private int currentLocationId = -1;

        // Конструктор формы
        public Sp_Location()
        {
            InitializeComponent();
        }

        // Обработчик события загрузки формы
        private void Sp_Location_Load(object sender, EventArgs e)
        {
            // Настройка внешнего вида DataGridView
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;

            // Снятие выделения с таблицы
            dataGridView1.ClearSelection();
            // Загрузка данных из базы данных
            LoadLocations();
        }

        // Метод загрузки данных о местоположениях из базы данных
        private void LoadLocations()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    // SQL-запрос для получения всех записей из таблицы Location
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM Location", con);
                    DataTable dt = new DataTable();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;

                    // Скрытие столбца с ID для пользователя
                    if (dataGridView1.Columns.Contains("ID_Location"))
                        dataGridView1.Columns["ID_Location"].Visible = false;

                    // Установка русских названий для столбцов
                    SetRussianColumnNames();
                    dataGridView1.ClearSelection();
                    // Обновление счетчика записей
                    UpdateRowCount();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для установки русских названий столбцов
        private void SetRussianColumnNames()
        {
            // Словарь соответствия английских названий столбцов русским
            Dictionary<string, string> columnNames = new Dictionary<string, string>
            {
                { "Name_Location", "Местоположение" },
                { "Coefficient", "Коэффициент" }
            };

            // Проход по всем столбцам и замена заголовков
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                if (columnNames.ContainsKey(col.Name))
                    col.HeaderText = columnNames[col.Name];
            }
        }

        // Обработчик клика по ячейке DataGridView
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Проверка, что клик не по заголовку столбца
            if (e.RowIndex < 0) return;
            // Заполнение текстовых полей данными из выбранной строки
            FillTextBoxesFromRow(dataGridView1.Rows[e.RowIndex]);
        }

        // Метод заполнения текстовых полей данными из строки таблицы
        private void FillTextBoxesFromRow(DataGridViewRow row)
        {
            // Получение ID выбранной записи
            currentLocationId = row.Cells["ID_Location"].Value != DBNull.Value
                ? Convert.ToInt32(row.Cells["ID_Location"].Value)
                : -1;

            // Заполнение поля названия местоположения
            textBox1.Text = row.Cells["Name_Location"].Value != DBNull.Value
                ? row.Cells["Name_Location"].Value.ToString()
                : "";

            // Заполнение поля коэффициента (замена запятой на точку)
            textBox2.Text = row.Cells["Coefficient"].Value != DBNull.Value
                ? row.Cells["Coefficient"].Value.ToString().Replace(',', '.')
                : "";
        }

        // Метод очистки всех полей формы
        private void ClearFields()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            currentLocationId = -1;
            dataGridView1.ClearSelection();
        }

        // Обработчик изменения текста в поле названия местоположения
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Фильтрация ввода: разрешены только русские буквы и пробелы
            string filtered = new string(textBox1.Text.Where(c => IsRussianLetter(c) || char.IsWhiteSpace(c)).ToArray());
            if (textBox1.Text != filtered)
            {
                // Сохранение позиции курсора
                int cursor = textBox1.SelectionStart - 1;
                textBox1.Text = filtered;
                textBox1.SelectionStart = Math.Max(0, cursor);
            }
        }

        // Обработчик нажатия клавиш в поле названия местоположения
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешены только управляющие символы, русские буквы и пробел
            if (!char.IsControl(e.KeyChar) && !IsRussianLetter(e.KeyChar) && e.KeyChar != ' ')
                e.Handled = true;
        }

        // Метод проверки, является ли символ русской буквой
        private bool IsRussianLetter(char c)
        {
            return (c >= 'А' && c <= 'я') || c == 'Ё' || c == 'ё';
        }

        // Обработчик изменения текста в поле коэффициента
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            string text = textBox2.Text;
            string filtered = "";
            bool dotFound = false;

            // Фильтрация ввода: разрешены цифры и одна точка
            foreach (char c in text)
            {
                if (char.IsDigit(c))
                    filtered += c;
                else if (c == '.' && !dotFound && filtered.Length > 0)
                {
                    filtered += c;
                    dotFound = true;
                }
            }

            // Если текст изменился, обновляем поле
            if (text != filtered)
            {
                int cursorPos = textBox2.SelectionStart - (text.Length - filtered.Length);
                textBox2.Text = filtered;
                textBox2.SelectionStart = Math.Max(0, cursorPos);
            }
        }

        // Обработчик нажатия клавиш в поле коэффициента
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешены только управляющие символы, цифры и точка
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                e.Handled = true;

            // Запрет на точку в начале или если точка уже есть
            if (e.KeyChar == '.' && (textBox2.SelectionStart == 0 || textBox2.Text.Contains(".")))
                e.Handled = true;

            // Запрет на запятую
            if (e.KeyChar == ',')
                e.Handled = true;
        }

        // Обработчик нажатия кнопки "Добавить"
        private void button1_Click(object sender, EventArgs e)
        {
            AddLocation();
        }

        // Обработчик нажатия кнопки "Изменить"
        private void button3_Click(object sender, EventArgs e)
        {
            UpdateLocation();
        }

        // Обработчик нажатия кнопки "Удалить"
        private void button2_Click(object sender, EventArgs e)
        {
            DeleteLocation();
        }

        // Обработчик нажатия кнопки "Закрыть"
        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Метод добавления новой записи о местоположении
        private void AddLocation()
        {
            string name = textBox1.Text.Trim();
            string coefText = textBox2.Text.Trim().Replace('.', ',');

            // Валидация ввода: проверка на пустое поле
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите местоположение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Валидация ввода: проверка на корректное число
            if (!decimal.TryParse(coefText, out decimal coef))
            {
                MessageBox.Show("Введите корректный коэффициент.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    // Проверка на уникальность названия местоположения
                    MySqlCommand checkCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM Location WHERE LOWER(Name_Location) = LOWER(@name)", con);
                    checkCmd.Parameters.AddWithValue("@name", name);
                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Такое местоположение уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // SQL-запрос для добавления новой записи
                    MySqlCommand cmd = new MySqlCommand(
                        "INSERT INTO Location (Name_Location, Coefficient) VALUES (@name, @coef)", con);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@coef", coef);
                    cmd.ExecuteNonQuery();
                }

                // Обновление таблицы и очистка полей
                LoadLocations();
                ClearFields();
                MessageBox.Show("Запись успешно добавлена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении: " + ex.Message);
            }
        }

        // Метод обновления существующей записи
        private void UpdateLocation()
        {
            // Проверка, что запись выбрана
            if (currentLocationId == -1)
            {
                MessageBox.Show("Выберите запись для редактирования.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string name = textBox1.Text.Trim();
            string coefText = textBox2.Text.Trim().Replace('.', ',');

            // Валидация ввода
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите местоположение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(coefText, out decimal coef))
            {
                MessageBox.Show("Введите корректный коэффициент.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    // Проверка уникальности названия (исключая текущую запись)
                    MySqlCommand checkCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM Location WHERE LOWER(Name_Location) = LOWER(@name) AND ID_Location != @id", con);
                    checkCmd.Parameters.AddWithValue("@name", name);
                    checkCmd.Parameters.AddWithValue("@id", currentLocationId);
                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Такое местоположение уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // SQL-запрос для обновления записи
                    MySqlCommand cmd = new MySqlCommand(
                        "UPDATE Location SET Name_Location=@name, Coefficient=@coef WHERE ID_Location=@id", con);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@coef", coef);
                    cmd.Parameters.AddWithValue("@id", currentLocationId);
                    cmd.ExecuteNonQuery();
                }

                // Обновление таблицы и очистка полей
                LoadLocations();
                ClearFields();
                MessageBox.Show("Запись успешно обновлена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении: " + ex.Message);
            }
        }

        // Метод удаления записи
        private void DeleteLocation()
        {
            // Проверка, что запись выбрана
            if (currentLocationId == -1)
            {
                MessageBox.Show("Выберите строку для удаления.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Подтверждение удаления
            if (MessageBox.Show("Удалить выбранное местоположение?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(connectionStr))
                    {
                        con.Open();
                        // SQL-запрос для удаления записи
                        MySqlCommand cmd = new MySqlCommand("DELETE FROM Location WHERE ID_Location=@id", con);
                        cmd.Parameters.AddWithValue("@id", currentLocationId);
                        cmd.ExecuteNonQuery();
                    }

                    // Обновление таблицы и очистка полей
                    LoadLocations();
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении: " + ex.Message);
                }
            }
        }

        // Метод обновления счетчика записей в таблице
        private void UpdateRowCount()
        {
            label2.Text = "Количество записей: " + dataGridView1.Rows.Count.ToString();
        }
    }
}