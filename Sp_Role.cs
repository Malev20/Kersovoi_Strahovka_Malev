using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Kersovoi_Strahovka_Malev
{
    public partial class Sp_Role : Form
    {
        // Строка подключения к базе данных MySQL
        string connectionStr = @"host=localhost;
                                 uid=root;
                                 pwd=root;
                                 database=kursach;";

        // Переменная для хранения ID текущей выбранной роли
        private int currentRoleId = -1;

        // Конструктор формы
        public Sp_Role()
        {
            InitializeComponent();
        }

        // Обработчик события загрузки формы
        private void Sp_Role_Load(object sender, EventArgs e)
        {
            // Настройка внешнего вида и поведения DataGridView
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;

            dataGridView1.ClearSelection(); // Снятие выделения
            LoadRoles(); // Загрузка данных о ролях из базы
        }

        // Метод установки русских названий для столбцов DataGridView
        private void SetRussianColumnNames(DataGridView grid)
        {
            try
            {
                // Словарь соответствия английских имен столбцов русским названиям
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    {"ID_Role", "Номер роли"},
                    {"Name_Role", "Роль"}
                };

                // Проход по всем столбцам и замена заголовков
                foreach (var col in grid.Columns)
                {
                    DataGridViewColumn column = (DataGridViewColumn)col;
                    if (columnNames.ContainsKey(column.Name))
                        column.HeaderText = columnNames[column.Name];
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
                FillTextBoxesFromRow(dataGridView1.Rows[e.RowIndex]); // Заполнение полей данными из строки
        }

        // Метод заполнения текстовых полей данными из выбранной строки
        private void FillTextBoxesFromRow(DataGridViewRow row)
        {
            try
            {
                // Получение ID роли из выбранной строки
                currentRoleId = row.Cells["ID_Role"].Value != null && row.Cells["ID_Role"].Value != DBNull.Value
                    ? Convert.ToInt32(row.Cells["ID_Role"].Value)
                    : -1;

                // Заполнение поля названия роли
                textBox1.Text = row.Cells["Name_Role"].Value != null && row.Cells["Name_Role"].Value != DBNull.Value
                    ? row.Cells["Name_Role"].Value.ToString()
                    : "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при заполнении полей: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод очистки всех полей формы
        private void ClearFields()
        {
            textBox1.Text = "";
            currentRoleId = -1; // Сброс ID
            dataGridView1.ClearSelection(); // Снятие выделения в таблице
        }

        // Обработчик изменения текста в поле названия роли
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Фильтрация ввода: разрешены только русские буквы и пробелы
            string filtered = new string(textBox1.Text.Where(c => IsRussianLetter(c) || char.IsWhiteSpace(c)).ToArray());
            if (textBox1.Text != filtered)
            {
                // Сохранение позиции курсора
                int cursor = textBox1.SelectionStart - 1;
                textBox1.Text = filtered; // Установка отфильтрованного текста
                textBox1.SelectionStart = Math.Max(0, cursor); // Восстановление позиции курсора
            }
        }

        // Обработчик нажатия клавиш в поле названия роли
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешены только: управляющие символы, русские буквы, пробел
            if (!char.IsControl(e.KeyChar) && !IsRussianLetter(e.KeyChar) && e.KeyChar != ' ')
                e.Handled = true; // Блокировка неразрешенных символов
        }

        // Метод проверки, является ли символ русской буквой
        private bool IsRussianLetter(char c)
        {
            return (c >= 'А' && c <= 'я') || c == 'Ё' || c == 'ё';
        }

        // Обработчик кнопки "Закрыть"
        private void button5_Click(object sender, EventArgs e)
        {
            this.Close(); // Закрытие формы
        }

        // Обработчик кнопки "Редактировать"
        private void button3_Click(object sender, EventArgs e)
        {
            // Проверка, что роль выбрана
            if (currentRoleId == -1)
            {
                MessageBox.Show("Выберите запись для редактирования.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string name = textBox1.Text.Trim();

            // Валидация ввода: проверка на пустое название
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название роли.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();

                    // Проверка уникальности названия роли (без учета регистра, исключая текущую запись)
                    MySqlCommand checkCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM Role WHERE LOWER(Name_Role) = LOWER(@name) AND ID_Role != @id", con);
                    checkCmd.Parameters.AddWithValue("@name", name);
                    checkCmd.Parameters.AddWithValue("@id", currentRoleId);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Роль с таким названием уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // SQL-запрос для обновления роли
                    MySqlCommand cmd = new MySqlCommand(
                        "UPDATE Role SET Name_Role = @name WHERE ID_Role = @id", con);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@id", currentRoleId);
                    cmd.ExecuteNonQuery(); // Выполнение запроса
                }

                LoadRoles(); // Обновление таблицы
                ClearFields(); // Очистка полей
                MessageBox.Show("Роль успешно обновлена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении роли: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод загрузки ролей из базы данных
        private void LoadRoles()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    // SQL-запрос для получения всех ролей из таблицы Role
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM Role", con);
                    DataTable dt = new DataTable();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    da.Fill(dt);
                    dataGridView1.DataSource = dt; // Привязка DataTable к DataGridView

                    dataGridView1.Columns["ID_Role"].Visible = false; // Скрытие столбца с ID

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

        // Обработчик кнопки "Удалить"
        private void button2_Click(object sender, EventArgs e)
        {
            // Проверка, что строка выбрана
            if (dataGridView1.CurrentRow == null || currentRoleId == -1)
            {
                MessageBox.Show("Выберите строку для удаления.");
                return;
            }

            // Подтверждение удаления
            var result = MessageBox.Show("Удалить выбранную роль?", "Подтверждение", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                DeleteRole(currentRoleId); // Удаление роли
                LoadRoles(); // Обновление таблицы
                ClearFields(); // Очистка полей
            }
        }

        // Метод удаления роли из базы данных
        private void DeleteRole(int id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    // SQL-запрос для удаления роли
                    MySqlCommand cmd = new MySqlCommand("DELETE FROM Role WHERE ID_Role = @id", con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery(); // Выполнение запроса
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении: " + ex.Message);
            }
        }

        // Обработчик клика по DataGridView (проверка клика вне строк)
        private void dataGridView1_Click(object sender, EventArgs e)
        {
            // Если клик произошел не по строке (по пустой области), очистить поля
            if (dataGridView1.HitTest((e as MouseEventArgs).X, (e as MouseEventArgs).Y).RowIndex == -1)
                ClearFields();
        }

        // Метод обновления счетчика записей в таблице
        private void UpdateRowCount()
        {
            label1.Text = "Количество записей: " + dataGridView1.Rows.Count.ToString();
        }
    }
}