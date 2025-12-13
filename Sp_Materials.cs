using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Kersovoi_Strahovka_Malev
{
    public partial class Sp_Materials : Form
    {
        // Строка подключения к базе данных MySQL
        string connectionStr = @"host=localhost;
                                 uid=root;
                                 pwd=root;
                                 database=kursach;";

        // Переменная для хранения ID текущего выбранного материала
        private int currentMaterialId = -1;

        // Конструктор формы
        public Sp_Materials()
        {
            InitializeComponent();
        }

        // Обработчик события загрузки формы
        private void Sp_Materials_Load(object sender, EventArgs e)
        {
            // Настройка внешнего вида и поведения DataGridView
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Автоматическое изменение размера столбцов
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Выделение всей строки
            dataGridView1.MultiSelect = false; // Запрет множественного выбора
            dataGridView1.ReadOnly = true; // Только для чтения
            dataGridView1.AllowUserToAddRows = false; // Запрет добавления строк пользователем
            dataGridView1.RowHeadersVisible = false; // Скрытие заголовков строк

            dataGridView1.ClearSelection(); // Снятие выделения
            LoadMaterials(); // Загрузка данных из базы
        }

        // Метод загрузки материалов из базы данных
        private void LoadMaterials()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    // SQL-запрос для получения всех материалов из таблицы Building_materials
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM Building_materials", con);
                    DataTable dt = new DataTable();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    da.Fill(dt);
                    dataGridView1.DataSource = dt; // Привязка DataTable к DataGridView

                    // Скрытие столбца с ID материала
                    if (dataGridView1.Columns.Contains("ID_Materials"))
                        dataGridView1.Columns["ID_Materials"].Visible = false;

                    // Настройка свойств DataGridView для безопасности и удобства
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

        // Метод установки русских названий для столбцов DataGridView
        private void SetRussianColumnNames(DataGridView grid)
        {
            try
            {
                // Словарь соответствия английских имен столбцов русским названиям
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    {"Name_Materials", "Название материала"},
                    {"Coefficient", "Коэффициент"}
                };

                // Проход по всем столбцам и замена заголовков
                foreach (DataGridViewColumn col in grid.Columns)
                {
                    if (columnNames.ContainsKey(col.Name))
                        col.HeaderText = columnNames[col.Name];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при установке заголовков: " + ex.Message);
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
                // Получение ID материала из выбранной строки
                currentMaterialId = row.Cells["ID_Materials"].Value != DBNull.Value
                    ? Convert.ToInt32(row.Cells["ID_Materials"].Value)
                    : -1;

                // Заполнение поля названия материала
                textBox1.Text = row.Cells["Name_Materials"].Value != DBNull.Value
                    ? row.Cells["Name_Materials"].Value.ToString()
                    : "";

                // Заполнение поля коэффициента (замена запятой на точку для корректного отображения)
                textBox2.Text = row.Cells["Coefficient"].Value != DBNull.Value
                    ? row.Cells["Coefficient"].Value.ToString().Replace(',', '.')
                    : "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при заполнении полей: " + ex.Message);
            }
        }

        // Метод очистки всех полей формы
        private void ClearFields()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            currentMaterialId = -1; // Сброс ID
            dataGridView1.ClearSelection(); // Снятие выделения в таблице
        }

        // Обработчик изменения текста в поле названия материала
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

        // Обработчик нажатия клавиш в поле названия материала
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

        // Обработчик изменения текста в поле коэффициента
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            string text = textBox2.Text;
            string filtered = "";
            bool dotFound = false;

            // Фильтрация: разрешены только цифры и одна точка
            foreach (char c in text)
            {
                if (char.IsDigit(c))
                    filtered += c; // Добавление цифры
                else if (c == '.' && !dotFound && filtered.Length > 0)
                {
                    filtered += c; // Добавление точки (только одной и не в начале)
                    dotFound = true;
                }
            }

            if (text != filtered)
            {
                // Корректировка позиции курсора
                int cursorPos = textBox2.SelectionStart - (text.Length - filtered.Length);
                textBox2.Text = filtered;
                textBox2.SelectionStart = Math.Max(0, cursorPos);
            }
        }

        // Обработчик нажатия клавиш в поле коэффициента
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешены только: управляющие символы, цифры, точка
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                e.Handled = true;

            // Запрет точки в начале строки или если точка уже есть
            if (e.KeyChar == '.' && (textBox2.SelectionStart == 0 || textBox2.Text.Contains(".")))
                e.Handled = true;

            // Запрет запятой
            if (e.KeyChar == ',')
                e.Handled = true;
        }

        // Обработчик кнопки "Добавить"
        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text.Trim();
            string coefText = textBox2.Text.Trim().Replace('.', ','); // Замена точки на запятую для SQL

            // Валидация ввода: проверка на пустое название
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название материала.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Валидация ввода: проверка корректности коэффициента
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

                    // Проверка уникальности названия материала (без учета регистра)
                    MySqlCommand checkCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM Building_materials WHERE LOWER(Name_Materials) = LOWER(@name)", con);
                    checkCmd.Parameters.AddWithValue("@name", name);
                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Материал с таким названием уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // SQL-запрос для добавления нового материала
                    MySqlCommand cmd = new MySqlCommand(
                        "INSERT INTO Building_materials (Name_Materials, Coefficient) VALUES (@name, @coef)", con);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@coef", coef);
                    cmd.ExecuteNonQuery(); // Выполнение запроса
                }

                LoadMaterials(); // Обновление таблицы
                ClearFields(); // Очистка полей
                MessageBox.Show("Материал успешно добавлен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении материала: " + ex.Message);
            }
        }
        // Обработчик кнопки "Редактировать"
        private void button3_Click(object sender, EventArgs e)
        {
            // Проверка, что материал выбран
            if (currentMaterialId == -1)
            {
                MessageBox.Show("Выберите запись для редактирования.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string name = textBox1.Text.Trim();
            string coefText = textBox2.Text.Trim().Replace('.', ',');

            // Валидация ввода
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название материала.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        "SELECT COUNT(*) FROM Building_materials WHERE LOWER(Name_Materials)=LOWER(@name) AND ID_Materials != @id", con);
                    checkCmd.Parameters.AddWithValue("@name", name);
                    checkCmd.Parameters.AddWithValue("@id", currentMaterialId);
                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Материал с таким названием уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // SQL-запрос для обновления материала
                    MySqlCommand cmd = new MySqlCommand(
                        "UPDATE Building_materials SET Name_Materials=@name, Coefficient=@coef WHERE ID_Materials=@id", con);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@coef", coef);
                    cmd.Parameters.AddWithValue("@id", currentMaterialId);
                    cmd.ExecuteNonQuery();
                }

                LoadMaterials(); // Обновление таблицы
                ClearFields(); // Очистка полей
                MessageBox.Show("Материал успешно обновлен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении материала: " + ex.Message);
            }
        }

        // Обработчик кнопки "Удалить"
        private void button2_Click(object sender, EventArgs e)
        {
            // Проверка, что материал выбран
            if (currentMaterialId == -1)
            {
                MessageBox.Show("Выберите строку для удаления.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Подтверждение удаления
            if (MessageBox.Show("Удалить выбранный материал?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(connectionStr))
                    {
                        con.Open();
                        // SQL-запрос для удаления материала
                        MySqlCommand cmd = new MySqlCommand("DELETE FROM Building_materials WHERE ID_Materials=@id", con);
                        cmd.Parameters.AddWithValue("@id", currentMaterialId);
                        cmd.ExecuteNonQuery();
                    }

                    LoadMaterials(); // Обновление таблицы
                    ClearFields(); // Очистка полей
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

        // Обработчик кнопки "Закрыть"
        private void button5_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}