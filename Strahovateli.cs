using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kersovoi_Strahovka_Malev
{
    public partial class Strahovateli : Form
    {
        // Строка подключения к базе данных MySQL
        string connectionStr = @"host=localhost;uid=root;pwd=root;database=kursach;";

        // Идентификатор текущего выбранного клиента (-1 означает "не выбран")
        private int currentClientId = -1;

        // Конструктор формы
        public Strahovateli()
        {
            InitializeComponent();

            // Настройка выпадающего списка для выбора пола
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Items.Clear();
            comboBox1.Items.Add("Все"); // Элемент "Все" для фильтрации или сброса выбора
            comboBox1.Items.Add("Муж"); // Мужской пол
            comboBox1.Items.Add("Жен"); // Женский пол
            comboBox1.SelectedIndex = 0; // Установка значения по умолчанию

            // Настройка ограничений для выбора даты выдачи паспорта
            dateTimePicker1.MinDate = new DateTime(1932, 1, 1); // Минимальная дата
            dateTimePicker1.MaxDate = DateTime.Now; // Максимальная дата (сегодня)
            dateTimePicker1.Value = DateTime.Today; // Значение по умолчанию (сегодня)

            // Настройка обработчиков событий для MaskedTextBox
            // Обеспечивает сброс позиции курсора при клике или фокусе
            foreach (var ctrl in this.Controls)
            {
                if (ctrl is MaskedTextBox m)
                {
                    m.Click += (s, e) => { m.SelectionStart = 0; m.SelectionLength = 0; };
                    m.Enter += (s, e) => { m.SelectionStart = 0; m.SelectionLength = 0; };
                }
            }
        }

        // Обработчик события загрузки формы
        private void Strahovateli_Load(object sender, EventArgs e)
        {
            // Настройка внешнего вида и поведения DataGridView
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Автоматическое изменение ширины столбцов
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Выделение всей строки
            dataGridView1.MultiSelect = false; // Запрет множественного выбора
            dataGridView1.ReadOnly = true; // Запрет редактирования ячеек
            dataGridView1.AllowUserToAddRows = false; // Запрет добавления строк пользователем
            dataGridView1.RowHeadersVisible = false; // Скрытие заголовков строк
            dataGridView1.ClearSelection(); // Снятие выделения с таблицы

            LoadClients(); // Загрузка данных о страхователях
        }

        // Метод загрузки данных о страхователях из базы данных
        private void LoadClients()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open(); // Открытие соединения с базой данных
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM Client", con); // SQL-запрос для получения всех клиентов
                    DataTable dt = new DataTable(); // Создание DataTable для хранения данных
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd); // Адаптер для заполнения DataTable
                    da.Fill(dt); // Заполнение DataTable данными из базы
                    dataGridView1.DataSource = dt; // Привязка DataTable к DataGridView

                    // Дополнительная настройка DataGridView
                    dataGridView1.ReadOnly = true; // Запрет редактирования
                    dataGridView1.AllowUserToAddRows = false; // Запрет добавления строк
                    dataGridView1.AllowUserToOrderColumns = false; // Запрет изменения порядка столбцов
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Автоматическое заполнение столбцов

                    // Обновление метки с количеством записей
                    label77.Text = $"Количество записей: {dataGridView1.Rows.Count}";

                    // Установка русских названий столбцов
                    SetRussianColumnNames(dataGridView1);

                    dataGridView1.ClearSelection(); // Снятие выделения после загрузки данных
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при загрузке данных
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик события клика по ячейке DataGridView
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Проверка, что клик был по строке данных (не по заголовку)
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex]; // Получение строки по индексу
                FillTextBoxesFromRow(row); // Заполнение полей формы данными из выбранной строки
            }
        }

        // Метод заполнения текстовых полей данными из строки DataGridView
        private void FillTextBoxesFromRow(DataGridViewRow row)
        {
            try
            {
                // Сохранение ID выбранного клиента
                if (row.Cells["ID_Client"].Value != null && row.Cells["ID_Client"].Value != DBNull.Value)
                {
                    currentClientId = Convert.ToInt32(row.Cells["ID_Client"].Value); // Преобразование значения в int
                }
                else
                {
                    currentClientId = -1; // Если ID отсутствует
                }

                // Заполнение полей формы данными из строки
                textBox1.Text = row.Cells["FIO_Client"].Value?.ToString() ?? ""; // ФИО (значение по умолчанию - пустая строка)
                maskedTextBox1.Text = row.Cells["Phone_Client"].Value?.ToString() ?? ""; // Телефон
                maskedTextBox2.Text = row.Cells["Series"].Value?.ToString() ?? ""; // Серия паспорта
                maskedTextBox3.Text = row.Cells["Number"].Value?.ToString() ?? ""; // Номер паспорта
                textBox6.Text = row.Cells["Issued_By_Whom"].Value?.ToString() ?? ""; // Кем выдан паспорт
                maskedTextBox4.Text = row.Cells["Code"].Value?.ToString() ?? ""; // Код подразделения

                // Установка значения в ComboBox для пола
                string gender = row.Cells["Gender"].Value?.ToString() ?? ""; // Получение значения пола
                if (gender == "Муж")
                {
                    comboBox1.SelectedIndex = 1; // Мужской пол
                }
                else if (gender == "Жен")
                {
                    comboBox1.SelectedIndex = 2; // Женский пол
                }
                else
                {
                    comboBox1.SelectedIndex = 0; // Значение по умолчанию ("Все")
                }

                // Установка даты выдачи паспорта
                if (row.Cells["Data_extradition"].Value != null && row.Cells["Data_extradition"].Value != DBNull.Value)
                {
                    dateTimePicker1.Value = Convert.ToDateTime(row.Cells["Data_extradition"].Value); // Преобразование строки в DateTime
                }
                else
                {
                    dateTimePicker1.Value = DateTime.Today; // Значение по умолчанию (сегодня)
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при заполнении полей
                MessageBox.Show($"Ошибка при заполнении полей: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод очистки всех полей формы
        private void ClearFields()
        {
            textBox1.Text = ""; // Очистка ФИО
            maskedTextBox1.Text = ""; // Очистка телефона
            maskedTextBox2.Text = ""; // Очистка серии паспорта
            maskedTextBox3.Text = ""; // Очистка номера паспорта
            maskedTextBox4.Text = ""; // Очистка кода подразделения
            textBox6.Text = ""; // Очистка поля "Кем выдан"
            comboBox1.SelectedIndex = 0; // Сброс выбора пола
            dateTimePicker1.Value = DateTime.Today; // Сброс даты на сегодня
            currentClientId = -1; // Сброс ID текущего клиента

            dataGridView1.ClearSelection(); // Снятие выделения с таблицы
        }

        // Метод установки русских названий столбцов в DataGridView
        private void SetRussianColumnNames(DataGridView grid)
        {
            // Словарь для сопоставления английских и русских названий столбцов
            Dictionary<string, string> columnNames = new Dictionary<string, string>
            {
                {"ID_Client", "№"}, // ID клиента
                {"FIO_Client", "ФИО Страхователя"}, // ФИО страхователя
                {"Phone_Client", "Телефон страхователя"}, // Телефон
                {"Series", "Серия"}, // Серия паспорта
                {"Number", "Номер"}, // Номер паспорта
                {"Gender", "Пол"}, // Пол
                {"Issued_By_Whom", "Кем выдан"}, // Кем выдан паспорт
                {"Data_extradition", "Дата выдачи"}, // Дата выдачи
                {"Code", "Код подразделения"} // Код подразделения
            };

            // Проход по всем столбцам DataGridView
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (columnNames.ContainsKey(col.Name))
                {
                    col.HeaderText = columnNames[col.Name]; // Установка русского названия
                }
            }
        }

        // Обработчик кнопки закрытия формы
        private void button5_Click(object sender, EventArgs e)
        {
            this.Close(); // Закрытие формы
        }

        // Обработчик изменения текста в поле ФИО
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender; // Приведение sender к TextBox
            int cursorPosition = textBox.SelectionStart; // Сохранение позиции курсора
            string input = textBox.Text; // Получение текущего текста

            // Фильтрация ввода: оставляем только русские буквы, пробелы и дефисы
            StringBuilder filtered = new StringBuilder();
            foreach (char c in input)
            {
                if ((c >= 'А' && c <= 'я') || c == 'ё' || c == 'Ё' || c == ' ' || c == '-')
                    filtered.Append(c);
            }

            string cleaned = filtered.ToString();

            // Удаление пробелов в начале строки
            while (cleaned.StartsWith(" ")) cleaned = cleaned.Substring(1);

            // Замена двойных пробелов на одинарные
            while (cleaned.Contains("  ")) cleaned = cleaned.Replace("  ", " ");

            // Форматирование текста: каждое слово с заглавной буквы
            StringBuilder result = new StringBuilder();
            bool makeUpper = true; // Флаг для указания, что следующий символ должен быть заглавным

            foreach (char c in cleaned)
            {
                if (makeUpper && Char.IsLetter(c))
                {
                    result.Append(Char.ToUpper(c)); // Преобразование в заглавную букву
                    makeUpper = false; // Следующая буква будет строчной
                }
                else if (c == ' ')
                {
                    result.Append(c);
                    makeUpper = true; // Следующее слово начинается с заглавной буквы
                }
                else if (c == '-')
                {
                    result.Append(c);
                    makeUpper = true; // После дефиса слово начинается с заглавной буквы
                }
                else
                {
                    result.Append(Char.ToLower(c)); // Преобразование в строчную букву
                }
            }
            string finalText = result.ToString();
            // Обновление текста, если он изменился
            if (textBox.Text != finalText)
            {
                int oldLength = textBox.Text.Length; // Длина старого текста
                int newLength = finalText.Length; // Длина нового текста
                int diff = oldLength - newLength; // Разница в длине

                textBox.Text = finalText; // Установка нового текста
                textBox.SelectionStart = Math.Max(0, cursorPosition - diff); // Восстановление позиции курсора
            }
        }

        // Обработчик изменения значения даты
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            ApplyDateFilter(); // Применение фильтра по дате
        }

        // Метод применения фильтра по дате (в данном случае не полностью реализован)
        private void ApplyDateFilter()
        {
            string filter = ""; // Инициализация строки фильтра
            DateTime dateFrom = dateTimePicker1.Value.Date; // Начальная дата
            DateTime dateTo = dateTimePicker1.Value.Date; // Конечная дата

            string dateFromStr = dateFrom.ToString("yyyy-MM-dd"); // Форматирование даты для SQL
            string dateToStr = dateTo.ToString("yyyy-MM-dd");
            // Формирование условия фильтра по дате
            filter = $"Start_Data >= '{dateFromStr}' AND Start_Data <= '{dateToStr}'";
            // Добавление условия по номеру полиса, если введен текст
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                filter += $" AND Policy_Number LIKE '%{textBox1.Text}%'";
            }
        }

        // Обработчики событий для MaskedTextBox при отклонении ввода
        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            maskedTextBox1.BeginInvoke(new Action(() => { maskedTextBox1.Select(0, 0); })); // Сброс позиции курсора
        }
        private void maskedTextBox2_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            maskedTextBox2.BeginInvoke(new Action(() => { maskedTextBox2.Select(0, 0); }));
        }
        private void maskedTextBox3_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            maskedTextBox3.BeginInvoke(new Action(() => { maskedTextBox3.Select(0, 0); }));
        }
        private void maskedTextBox4_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            maskedTextBox4.BeginInvoke(new Action(() => { maskedTextBox4.Select(0, 0); }));
        }

        // Обработчик кнопки "Редактировать"
        private void button3_Click(object sender, EventArgs e)
        {
            // Проверка, выбран ли клиент для редактирования
            if (currentClientId == -1)
            {
                MessageBox.Show("Выберите запись для редактирования.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // Получение данных из полей формы
            string fio = textBox1.Text.Trim();
            string phone = maskedTextBox1.Text.Trim();
            string series = maskedTextBox2.Text.Trim();
            string number = maskedTextBox3.Text.Trim();
            string gender = comboBox1.SelectedItem.ToString();
            string issuedBy = textBox6.Text.Trim();
            string code = maskedTextBox4.Text.Trim();
            DateTime dateExtradition = dateTimePicker1.Value;
            // Проверка выбора пола
            if (gender == "Все")
            {
                MessageBox.Show("Выберите пол страхователя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Проверка заполнения обязательных полей (ДОБАВЛЕНО проверка code)
            if (string.IsNullOrEmpty(fio) || string.IsNullOrEmpty(phone) ||
                string.IsNullOrEmpty(series) || string.IsNullOrEmpty(number) ||
                string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Проверка уникальности данных клиента (исключая текущего)
            if (IsClientExists(phone, series, number, currentClientId))
            {
                MessageBox.Show("Страхователь с таким телефоном и паспортными данными уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open(); // Открытие соединения
                    // SQL-запрос для обновления данных клиента
                    string updateQuery = @"UPDATE Client 
                                   SET FIO_Client = @fio, 
                                       Phone_Client = @phone, 
                                       Series = @series, 
                                       Number = @number, 
                                       Gender = @gender, 
                                       Issued_By_Whom = @issuedBy, 
                                       Data_extradition = @date, 
                                       Code = @code
                                   WHERE ID_Client = @id";
                    using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, con))
                    {
                        // Добавление параметров запроса
                        updateCmd.Parameters.AddWithValue("@fio", fio);
                        updateCmd.Parameters.AddWithValue("@phone", phone);
                        updateCmd.Parameters.AddWithValue("@series", series);
                        updateCmd.Parameters.AddWithValue("@number", number);
                        updateCmd.Parameters.AddWithValue("@gender", gender);
                        updateCmd.Parameters.AddWithValue("@issuedBy", issuedBy);
                        updateCmd.Parameters.AddWithValue("@date", dateExtradition);
                        updateCmd.Parameters.AddWithValue("@code", code);
                        updateCmd.Parameters.AddWithValue("@id", currentClientId);
                        updateCmd.ExecuteNonQuery(); // Выполнение запроса
                    }
                    MessageBox.Show("Данные страхователя обновлены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadClients(); // Перезагрузка данных
                    ClearFields(); // Очистка полей
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик кнопки "Очистить"
        private void button6_Click(object sender, EventArgs e)
        {
            ClearFields(); // Очистка всех полей формы
        }

        // Обработчик кнопки "Добавить"
        private void button1_Click(object sender, EventArgs e)
        {
            // Получение данных из полей формы
            string fio = textBox1.Text.Trim();
            string phone = maskedTextBox1.Text.Trim();
            string series = maskedTextBox2.Text.Trim();
            string number = maskedTextBox3.Text.Trim();
            string gender = comboBox1.SelectedItem.ToString();
            string issuedBy = textBox6.Text.Trim();
            string code = maskedTextBox4.Text.Trim();
            DateTime dateExtradition = dateTimePicker1.Value;
            // Проверка выбора пола
            if (gender == "Все")
            {
                MessageBox.Show("Выберите пол страхователя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Проверка заполнения обязательных полей (ДОБАВЛЕНО проверка code)
            if (string.IsNullOrEmpty(fio) || string.IsNullOrEmpty(phone) ||
                string.IsNullOrEmpty(series) || string.IsNullOrEmpty(number) ||
                string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Проверка уникальности данных клиента
            if (IsClientExists(phone, series, number))
            {
                MessageBox.Show("Страхователь с таким телефоном и паспортными данными уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open(); // Открытие соединения
                    // SQL-запрос для добавления нового клиента
                    string insertQuery = @"INSERT INTO Client 
                                   (FIO_Client, Phone_Client, Series, Number, Gender, Issued_By_Whom, Data_extradition, Code) 
                                   VALUES 
                                   (@fio, @phone, @series, @number, @gender, @issuedBy, @date, @code)";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                    {
                        // Добавление параметров запроса
                        insertCmd.Parameters.AddWithValue("@fio", fio);
                        insertCmd.Parameters.AddWithValue("@phone", phone);
                        insertCmd.Parameters.AddWithValue("@series", series);
                        insertCmd.Parameters.AddWithValue("@number", number);
                        insertCmd.Parameters.AddWithValue("@gender", gender);
                        insertCmd.Parameters.AddWithValue("@issuedBy", issuedBy);
                        insertCmd.Parameters.AddWithValue("@date", dateExtradition);
                        insertCmd.Parameters.AddWithValue("@code", code);
                        insertCmd.ExecuteNonQuery(); // Выполнение запроса
                    }
                    MessageBox.Show("Страхователь добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadClients(); // Перезагрузка данных
                    ClearFields(); // Очистка полей
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик кнопки "Удалить"
        private void button2_Click(object sender, EventArgs e)
        {
            // Проверка, выбрана ли строка для удаления
            if (dataGridView1.CurrentRow == null || currentClientId == -1)
            {
                MessageBox.Show("Выберите строку для удаления!");
                return;
            }
            // Запрос подтверждения удаления
            var result = MessageBox.Show("Удалить выбранного страхователя?", "Подтверждение", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(connectionStr))
                    {
                        con.Open(); // Открытие соединения
                        MySqlCommand delCmd = new MySqlCommand("DELETE FROM Client WHERE ID_Client=@id", con); // SQL-запрос на удаление
                        delCmd.Parameters.AddWithValue("@id", currentClientId); // Добавление параметра
                        delCmd.ExecuteNonQuery(); // Выполнение запроса
                        MessageBox.Show("Страхователь удален!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadClients(); // Перезагрузка данных
                        ClearFields(); // Очистка полей
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Метод проверки существования клиента с заданными данными
        private bool IsClientExists(string phone, string series, string number, int excludeId = -1)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open(); // Открытие соединения
                    // SQL-запрос для подсчета записей с совпадающими данными
                    string query = @"SELECT COUNT(*) 
                             FROM Client 
                             WHERE ((Phone_Client=@phone AND Series=@series AND Number=@number)
                                OR (Series=@series AND Number=@number))
                                AND ID_Client != @excludeId";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        // Добавление параметров запроса
                        cmd.Parameters.AddWithValue("@phone", phone);
                        cmd.Parameters.AddWithValue("@series", series);
                        cmd.Parameters.AddWithValue("@number", number);
                        cmd.Parameters.AddWithValue("@excludeId", excludeId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar()); // Выполнение скалярного запроса
                        return count > 0; // Возврат true, если найдены совпадения
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок и возврат true для блокировки добавления
                MessageBox.Show("Ошибка при проверке уникальности: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
        }

        // Обработчик кнопки "Новая страховка"
        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return; // Проверка выбора строки
            // Получение данных выбранного клиента
            string fio = dataGridView1.CurrentRow.Cells["FIO_Client"].Value.ToString();
            string phone = dataGridView1.CurrentRow.Cells["Phone_Client"].Value.ToString();
            string currentUserFIO = "Страховщик"; // ФИО текущего пользователя (заглушка)
            // Поиск уже открытой формы New_Strahovka
            New_Strahovka newInsuranceForm = Application.OpenForms.OfType<New_Strahovka>().FirstOrDefault();
            if (newInsuranceForm == null)
            {
                // Создание новой формы, если она не открыта
                newInsuranceForm = new New_Strahovka(currentUserFIO);
                newInsuranceForm.Show();
            }
            else
            {
                // Активация существующей формы
                if (newInsuranceForm.WindowState == FormWindowState.Minimized)
                    newInsuranceForm.WindowState = FormWindowState.Normal;
                newInsuranceForm.BringToFront();
            }
            // Передача данных выбранного клиента в форму создания страховки
            newInsuranceForm.FIO = fio;
            newInsuranceForm.PassportNumber = phone;

            this.Close(); // Закрытие текущей формы
        }

        // Обработчик клика по DataGridView
        private void dataGridView1_Click(object sender, EventArgs e)
        {
            // Проверка, кликнули ли на пустое место (не на строку)
            if (dataGridView1.HitTest((e as MouseEventArgs).X, (e as MouseEventArgs).Y).RowIndex == -1)
            {
                ClearFields(); // Очистка полей при клике на пустое место
            }
        }

        // Обработчик изменения текста в поле "Кем выдан"
        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            int cursorPosition = textBox.SelectionStart;
            string input = textBox.Text;
            // Фильтрация ввода: оставляем русские буквы, пробелы, точки и запятые
            StringBuilder filtered = new StringBuilder();
            foreach (char c in input)
            {
                if ((c >= 'А' && c <= 'я') || c == 'ё' || c == 'Ё' || c == ' ' || c == '.' || c == ',')
                    filtered.Append(c);
            }
            string cleaned = filtered.ToString();
            // Удаление пробелов в начале строки
            while (cleaned.StartsWith(" ")) cleaned = cleaned.Substring(1);
            // Замена двойных пробелов на одинарные
            while (cleaned.Contains("  ")) cleaned = cleaned.Replace("  ", " ");
            // Удаление пробелов перед точкой и запятой
            cleaned = cleaned.Replace(" .", ".");
            cleaned = cleaned.Replace(" ,", ",");
            // Добавление пробела после точки или запятой, если за ними следует буква
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < cleaned.Length; i++)
            {
                result.Append(cleaned[i]);

                if ((cleaned[i] == '.' || cleaned[i] == ',') &&
                    i + 1 < cleaned.Length &&
                    Char.IsLetter(cleaned[i + 1]))
                {
                    result.Append(' ');
                }
            }
            string finalText = result.ToString();
            // Обновление текста, если он изменился
            if (textBox.Text != finalText)
            {
                int oldLength = textBox.Text.Length;
                int newLength = finalText.Length;
                int diff = oldLength - newLength;

                textBox.Text = finalText;
                textBox.SelectionStart = Math.Max(0, cursorPosition - diff);
            }
        }
    }
}