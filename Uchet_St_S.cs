using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace Kersovoi_Strahovka_Malev
{
    public partial class Uchet_St_S : Form
    {
        // Строка подключения к базе данных MySQL
        string connectionStr = @"host=localhost;
                                 uid=root;
                                 pwd=root;
                                 database=kursach;";
        private DataTable dt; // Таблица для хранения данных из базы
        private int userRole; // Роль пользователя (1-админ, 2-страховщик, 3-директор)
        private string photosFolderPath; // Путь к папке с фотографиями
        private string placeholderImagePath; // Путь к изображению-заглушке
        private Image placeholderImage; // Изображение-заглушка
        private DateTime minDateFromDatabase; // Минимальная дата из базы данных
        private DateTime maxDateFromDatabase; // Максимальная дата из базы данных

        // Конструктор формы, принимающий роль пользователя
        public Uchet_St_S(int role)
        {
            InitializeComponent();
            userRole = role;

            // Изменяем пути - используем Application.StartupPath
            photosFolderPath = Path.Combine(Application.StartupPath, "512x512");
            placeholderImagePath = Path.Combine(photosFolderPath, "empty_photocamera.png");

            // Если папки не существует, создаем
            if (!Directory.Exists(photosFolderPath))
            {
                Directory.CreateDirectory(photosFolderPath);
            }

            // Загружаем заглушку
            LoadPlaceholderImage();

            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
            button2.Visible = (userRole == 3);
        }


        // Метод загрузки изображения-заглушки для отсутствующих фотографий
        private void LoadPlaceholderImage()
        {
            try
            {
                // Проверка существования файла заглушки
                if (File.Exists(placeholderImagePath))
                {
                    // Загрузка и изменение размера изображения
                    using (var original = Image.FromFile(placeholderImagePath))
                    {
                        placeholderImage = new Bitmap(128, 128);
                        using (Graphics g = Graphics.FromImage(placeholderImage))
                        {
                            // Настройка качественного масштабирования
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.DrawImage(original, 0, 0, 128, 128);
                        }
                    }
                }
                else
                {
                    // Создание простой заглушки, если файл не найден
                    placeholderImage = new Bitmap(64, 64);
                    using (Graphics g = Graphics.FromImage(placeholderImage))
                    {
                        g.Clear(Color.LightGray);
                        g.DrawString("No Image", new Font("Arial", 8), Brushes.Black, 5, 25);
                    }
                }
            }
            catch
            {
                // Создание заглушки в случае ошибки
                placeholderImage = new Bitmap(64, 64);
                using (Graphics g = Graphics.FromImage(placeholderImage))
                {
                    g.Clear(Color.LightGray);
                    g.DrawString("No Image", new Font("Arial", 8), Brushes.Black, 5, 25);
                }
            }
        }

        // Метод для обновления данных в DataGridView из базы данных
        public void RefreshData()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    // SQL-запрос для получения всех данных о страховках с объединением таблиц
                    MySqlCommand cmd = new MySqlCommand(@"SELECT 
                                                    i.ID_Insurance,
                                                    i.Policy_Number, 
                                                    i.Photo,
                                                    i.Start_Data, 
                                                    i.End_Data, 
                                                    c.FIO_Client, 
                                                    i.Phone_Client,
                                                    u.FIO_Users, 
                                                    i.Insurance_Amount, 
                                                    i.Amount_Payment_Policy, 
                                                    i.ID_Status,
                                                    s.Name_Status,
                                                    i.ID_Users,
                                                    i.ID_Client,
                                                    i.ID_Materials,
                                                    i.Square,
                                                    i.ID_Location, 
                                                    i.ID_Rate,
                                                    i.Year_Build,
                                                    i.Address_Building
                                                FROM Insurance i
                                                INNER JOIN Client c ON i.ID_Client = c.ID_Client
                                                INNER JOIN Users u ON i.ID_Users = u.ID_Users
                                                INNER JOIN Status s ON i.ID_Status = s.ID_Status", con);

                    DataTable newDt = new DataTable();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    da.Fill(newDt);

                    // Сохранение текущих фильтров и сортировки
                    string currentFilter = dt?.DefaultView?.RowFilter ?? "";
                    string currentSort = dt?.DefaultView?.Sort ?? "";

                    dt = newDt;

                    // Восстановление фильтров и сортировки
                    if (!string.IsNullOrEmpty(currentFilter))
                        dt.DefaultView.RowFilter = currentFilter;
                    if (!string.IsNullOrEmpty(currentSort))
                        dt.DefaultView.Sort = currentSort;

                    dataGridView1.DataSource = dt;
                    HideTechnicalColumns();
                    UpdateRecordCount();
                    UpdateTotalAmount();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик загрузки формы
        private void Uchet_St_S_Load(object sender, EventArgs e)
        {
            try
            {
                MySqlConnection con = new MySqlConnection(connectionStr);
                con.Open();

                // Загрузка данных о страховках при запуске формы
                MySqlCommand cmd = new MySqlCommand(@"SELECT 
                                                        i.ID_Insurance,
                                                        i.Policy_Number, 
                                                        i.Photo,
                                                        i.Start_Data, 
                                                        i.End_Data, 
                                                        c.FIO_Client, 
                                                        i.Phone_Client,
                                                        u.FIO_Users, 
                                                        i.Insurance_Amount, 
                                                        i.Amount_Payment_Policy, 
                                                        i.ID_Status,
                                                        s.Name_Status,
                                                        i.ID_Users,
                                                        i.ID_Client,
                                                        i.ID_Materials,
                                                        i.Square,
                                                        i.ID_Location, 
                                                        i.ID_Rate,
                                                        i.Year_Build,
                                                        i.Address_Building
                                                    FROM Insurance i
                                                    INNER JOIN Client c ON i.ID_Client = c.ID_Client
                                                    INNER JOIN Users u ON i.ID_Users = u.ID_Users
                                                    INNER JOIN Status s ON i.ID_Status = s.ID_Status", con);

                dt = new DataTable();
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(dt);

                // Настройка DataGridView
                ConfigureDataGridView();
                dataGridView1.DataSource = dt;
                SetRussianColumnNames(dataGridView1);
                con.Close();

                // Получение минимальной и максимальной дат для фильтрации
                GetMinMaxDatesFromDatabase();
                dateTimePicker1.Value = minDateFromDatabase;
                dateTimePicker2.Value = maxDateFromDatabase;
            }
            catch (Exception ex)
            {
                // Обработка ошибок при загрузке данных
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dt = new DataTable();
                ConfigureDataGridView();
                dataGridView1.DataSource = dt;

                // Установка значений по умолчанию при ошибке
                minDateFromDatabase = new DateTime(1980, 1, 1);
                maxDateFromDatabase = DateTime.Today;
                dateTimePicker1.Value = minDateFromDatabase;
                dateTimePicker2.Value = maxDateFromDatabase;
            }

            // Дополнительная настройка DataGridView
            dataGridView1.AllowUserToAddRows = false;
            HideTechnicalColumns();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView1.ScrollBars = ScrollBars.Both;

            // Установка ширины столбцов
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                if (column.Name != "PhotoColumn")
                {
                    column.Width = 150;
                }
            }

            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.RowTemplate.Height = 70;

            // Загрузка списка страховщиков для фильтрации
            LoadInsuranceAgents();

            // Настройка комбобокса для сортировки
            comboBox1.Items.Clear();
            comboBox1.Items.Add("По возрастанию");
            comboBox1.Items.Add("По убыванию");
            comboBox1.SelectedIndex = 0;

            // Настройка ограничений для выбора дат
            dateTimePicker1.MinDate = minDateFromDatabase;
            dateTimePicker1.MaxDate = maxDateFromDatabase;
            dateTimePicker2.MinDate = minDateFromDatabase;
            dateTimePicker2.MaxDate = maxDateFromDatabase;
            dateTimePicker2.MinDate = dateTimePicker1.Value;

            // Отображение информации о прибыли только для директора
            label14.Visible = (userRole == 3);
            UpdateTotalAmount();

            dataGridView1.DataSource = dt;
            if (dt != null)
            {
                dt.DefaultView.RowFilter = "";
            }
            dataGridView1.ScrollBars = ScrollBars.Both;
            UpdateRecordCount();
        }

        // Метод получения минимальной и максимальной дат из базы данных
        private void GetMinMaxDatesFromDatabase()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT MIN(Start_Data) as MinDate, MAX(Start_Data) as MaxDate FROM Insurance", con);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                            minDateFromDatabase = reader.GetDateTime(0);
                        else
                            minDateFromDatabase = new DateTime(1980, 1, 1);

                        if (!reader.IsDBNull(1))
                            maxDateFromDatabase = reader.GetDateTime(1);
                        else
                            maxDateFromDatabase = DateTime.Today;
                    }
                    else
                    {
                        minDateFromDatabase = new DateTime(1980, 1, 1);
                        maxDateFromDatabase = DateTime.Today;
                    }
                }
            }
            catch
            {
                minDateFromDatabase = new DateTime(1980, 1, 1);
                maxDateFromDatabase = DateTime.Today;
            }
        }

        // Метод конфигурации DataGridView для отображения фотографий
        private void ConfigureDataGridView()
        {
            if (dataGridView1.Columns["PhotoColumn"] == null)
            {
                DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
                imageColumn.Name = "PhotoColumn";
                imageColumn.HeaderText = "Фотография";
                imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
                imageColumn.Width = 80;
                imageColumn.MinimumWidth = 80;
                dataGridView1.Columns.Insert(0, imageColumn);
            }
        }

        // Метод скрытия технических столбцов (ID и т.д.)
        private void HideTechnicalColumns()
        {
            string[] columnsToHide = {
                "ID_Insurance", "ID_Users", "ID_Client", "ID_Materials",
                "ID_Location", "ID_Rate", "ID_Status", "Photo"
            };

            foreach (string columnName in columnsToHide)
            {
                if (dataGridView1.Columns[columnName] != null)
                {
                    dataGridView1.Columns[columnName].Visible = false;
                }
            }
        }

        // Метод установки русских названий столбцов
        private void SetRussianColumnNames(DataGridView grid)
        {
            try
            {
                if (grid.Columns.Count == 0) return;

                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    {"PhotoColumn", "Фото"},
                    {"Policy_Number", "Номер страховки"},
                    {"Start_Data", "Дата оформления"},
                    {"End_Data", "Дата окончания"},
                    {"FIO_Client", "ФИО страхователя"},
                    {"Phone_Client", "Телефон страхователя"},
                    {"FIO_Users", "ФИО страховщика"},
                    {"Insurance_Amount", "Страховая сумма"},
                    {"Amount_Payment_Policy", "Страховой взнос"},
                    {"Square", "Площадь постройки"},
                    {"Year_Build", "Год постройки"},
                    {"Address_Building", "Адрес объекта"},
                    {"ID_Insurance", "ID страховки"},
                    {"ID_Users", "ID пользователя"},
                    {"ID_Client", "ID клиента"},
                    {"ID_Materials", "ID материала"},
                    {"ID_Location", "ID местоположения"},
                    {"Name_Status", "Статус"},
                    {"ID_Rate", "ID тарифа"},
                    {"ID_Status", "ID статуса"},
                    {"Photo", "Фотография (ориг.)"}
                };

                foreach (DataGridViewColumn column in grid.Columns)
                {
                    if (columnNames.ContainsKey(column.Name))
                    {
                        column.HeaderText = columnNames[column.Name];
                    }
                }
            }
            catch { }
        }

        // Метод загрузки и изменения размера изображения
        private Image LoadAndResizeImage(string imageName)
        {
            try
            {
                if (string.IsNullOrEmpty(imageName))
                {
                    return placeholderImage;
                }

                string imagePath = Path.Combine(photosFolderPath, imageName);

                if (!File.Exists(imagePath))
                {
                    return placeholderImage;
                }

                using (var originalImage = Image.FromFile(imagePath))
                {
                    Bitmap resizedImage = new Bitmap(64, 64);
                    using (Graphics g = Graphics.FromImage(resizedImage))
                    {
                        // Настройка качественного масштабирования
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                        g.Clear(Color.White);

                        // Расчет коэффициента масштабирования
                        float scale = Math.Min(64f / originalImage.Width, 64f / originalImage.Height);
                        int newWidth = (int)(originalImage.Width * scale);
                        int newHeight = (int)(originalImage.Height * scale);
                        int x = (64 - newWidth) / 2;
                        int y = (64 - newHeight) / 2;

                        g.DrawImage(originalImage, x, y, newWidth, newHeight);
                    }
                    return resizedImage;
                }
            }
            catch
            {
                return placeholderImage;
            }
        }

        // Обработчик форматирования ячеек DataGridView
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                // Подсветка строк при поиске по номеру полиса
                if (dataGridView1.Columns.Count > 0 &&
                    e.ColumnIndex >= 0 &&
                    e.ColumnIndex < dataGridView1.Columns.Count &&
                    dataGridView1.Columns[e.ColumnIndex].Name == "Policy_Number" &&
                    !string.IsNullOrEmpty(textBox1.Text))
                {
                    string cellValue = e.Value?.ToString() ?? "";
                    if (cellValue.Contains(textBox1.Text))
                    {
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                    }
                    else
                    {
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                    }
                }

                // Загрузка изображений в колонку фотографий
                if (e.ColumnIndex == 0 && dataGridView1.Columns[0].Name == "PhotoColumn" && e.RowIndex >= 0)
                {
                    if (dt != null && e.RowIndex < dt.Rows.Count)
                    {
                        DataRow row = dt.Rows[e.RowIndex];
                        string photoName = row["Photo"]?.ToString();
                        Image photo = LoadAndResizeImage(photoName);
                        e.Value = photo;
                    }
                    else
                    {
                        e.Value = placeholderImage;
                    }
                }
            }
            catch
            {
                e.Value = placeholderImage;
            }
        }

        // Метод применения всех фильтров к данным
        private void ApplyAllFilters()
        {
            if (dt == null) return;

            List<string> filters = new List<string>();

            // Фильтр по номеру полиса
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                string searchText = textBox1.Text.Trim();
                filters.Add($"Policy_Number LIKE '%{EscapeSqlLike(searchText)}%'");
            }

            // Фильтр по диапазону дат
            DateTime dateFrom = dateTimePicker1.Value.Date;
            DateTime dateTo = dateTimePicker2.Value.Date;
            string dateFromStr = dateFrom.ToString("yyyy-MM-dd");
            string dateToStr = dateTo.ToString("yyyy-MM-dd");
            filters.Add($"Start_Data >= '{dateFromStr}' AND Start_Data <= '{dateToStr}'");

            // Фильтр по страховщику
            if (comboBox2.SelectedItem != null && comboBox2.SelectedItem.ToString() != "Все")
            {
                string safeValue = EscapeSqlString(comboBox2.SelectedItem.ToString());
                filters.Add($"FIO_Users = '{safeValue}'");
            }

            // Фильтр по статусам (чекбоксы)
            List<string> statusFilters = new List<string>();

            if (checkBoxWaiting != null && checkBoxWaiting.Checked)
                statusFilters.Add("ID_Status = 1");
            if (checkBoxActive != null && checkBoxActive.Checked)
                statusFilters.Add("ID_Status = 2");
            if (checkBoxCompleted != null && checkBoxCompleted.Checked)
                statusFilters.Add("ID_Status = 3");
            if (checkBoxCanceled != null && checkBoxCanceled.Checked)
                statusFilters.Add("ID_Status = 4");

            // Объединение фильтров статусов через OR
            if (statusFilters.Count > 0)
            {
                filters.Add("(" + string.Join(" OR ", statusFilters) + ")");
            }

            // Создание итогового фильтра
            string finalFilter = filters.Count > 0 ? string.Join(" AND ", filters) : "";

            try
            {
                dt.DefaultView.RowFilter = finalFilter;
            }
            catch
            {
                dt.DefaultView.RowFilter = "";
            }

            // Применение сортировки и обновление интерфейса
            ApplySorting();
            UpdateRecordCount();
            UpdateTotalAmount();
        }

        // Метод экранирования специальных символов для SQL LIKE
        private string EscapeSqlLike(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Replace("[", "[[]")
                       .Replace("%", "[%]")
                       .Replace("_", "[_]")
                       .Replace("'", "''");
        }

        // Метод экранирования строк для SQL
        private string EscapeSqlString(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Replace("'", "''");
        }

        // Метод применения сортировки к данным
        private void ApplySorting()
        {
            if (dt == null || comboBox1.SelectedItem == null) return;

            string selected = comboBox1.SelectedItem.ToString();
            if (selected == "По возрастанию")
            {
                dt.DefaultView.Sort = "Amount_Payment_Policy ASC";
            }
            else if (selected == "По убыванию")
            {
                dt.DefaultView.Sort = "Amount_Payment_Policy DESC";
            }
        }

        // Обработчик изменения текста поиска
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Сброс подсветки всех строк
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                    row.DefaultCellStyle.BackColor = Color.White;
            }

            ApplyAllFilters();
        }

        // Обработчик изменения выбора страховщика
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyAllFilters();
        }

        // Обработчик изменения сортировки
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyAllFilters();
        }

        // Обработчик изменения начальной даты
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            // Корректировка конечной даты, если начальная дата позже
            if (dateTimePicker1.Value > dateTimePicker2.Value)
            {
                dateTimePicker2.Value = dateTimePicker1.Value;
            }

            dateTimePicker2.MinDate = dateTimePicker1.Value;
            ApplyAllFilters();
        }

        // Обработчик изменения конечной даты
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            // Корректировка начальной даты, если конечная дата раньше
            if (dateTimePicker2.Value < dateTimePicker1.Value)
            {
                dateTimePicker1.Value = dateTimePicker2.Value;
            }

            ApplyAllFilters();
        }

        // Обработчики изменения состояния чекбоксов статусов
        private void checkBoxWaiting_CheckedChanged(object sender, EventArgs e)
        {
            ApplyAllFilters();
        }

        private void checkBoxActive_CheckedChanged(object sender, EventArgs e)
        {
            ApplyAllFilters();
        }

        private void checkBoxCompleted_CheckedChanged(object sender, EventArgs e)
        {
            ApplyAllFilters();
        }

        private void checkBoxCanceled_CheckedChanged(object sender, EventArgs e)
        {
            ApplyAllFilters();
        }

        // Обработчик кнопки управления пользователями
        private void button5_Click(object sender, EventArgs e)
        {
            Users form1 = new Users();
            form1.Show();
            this.Hide();
        }

        // Обработчик кнопки закрытия формы
        private void button5_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        // Обработчик кнопки просмотра деталей страховки
        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите строку для просмотра.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Получение данных из выбранной строки
            DataGridViewRow row = dataGridView1.SelectedRows[0];
            string policyNumber = GetSafeString(row, "Policy_Number");
            string startDate = GetSafeDateString(row, "Start_Data");
            string endDate = GetSafeDateString(row, "End_Data");
            string clientFIO = GetSafeString(row, "FIO_Client");
            string clientPhone = GetSafeString(row, "Phone_Client");
            string userFIO = GetSafeString(row, "FIO_Users");
            string insuranceAmount = GetSafeString(row, "Insurance_Amount");
            string paymentAmount = GetSafeString(row, "Amount_Payment_Policy");
            string status = GetSafeString(row, "Name_Status");
            string addressBuilding = GetSafeString(row, "Address_Building");

            // Открытие формы просмотра с передачей данных
            var form = new Prosmotr_Uchet(
                policyNumber,
                startDate,
                endDate,
                clientFIO,
                clientPhone,
                userFIO,
                insuranceAmount,
                paymentAmount,
                status,
                addressBuilding,
                userRole,
                dataGridView1
            );

            form.ShowDialog();
        }

        // Метод безопасного получения строкового значения из ячейки
        private string GetSafeString(DataGridViewRow row, string columnName)
        {
            try
            {
                if (row.Cells[columnName]?.Value != null)
                {
                    return row.Cells[columnName].Value.ToString();
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // Метод безопасного получения даты в виде строки из ячейки
        private string GetSafeDateString(DataGridViewRow row, string columnName)
        {
            try
            {
                if (row.Cells[columnName]?.Value != null && row.Cells[columnName].Value is DateTime)
                {
                    return ((DateTime)row.Cells[columnName].Value).ToShortDateString();
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // Обработчик ввода символов в поле поиска (только цифры)
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        // Метод загрузки списка страховщиков для фильтрации
        private void LoadInsuranceAgents()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    // Запрос для получения уникальных страховщиков (роль 2)
                    string query = @"SELECT DISTINCT u.FIO_Users 
                                   FROM Users u 
                                   INNER JOIN Insurance i ON u.ID_Users = i.ID_Users 
                                   WHERE u.ID_Role = 2";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    comboBox2.Items.Clear();
                    comboBox2.Items.Add("Все");

                    while (reader.Read())
                    {
                        comboBox2.Items.Add(reader.GetString("FIO_Users"));
                    }

                    comboBox2.SelectedIndex = 0;
                }
            }
            catch { }
        }

        // Метод обновления отображения общей прибыли
        private void UpdateTotalAmount()
        {
            if (dt == null) return;

            decimal total = 0;

            // Суммирование страховых взносов
            foreach (DataRowView rowView in dt.DefaultView)
            {
                if (rowView["Amount_Payment_Policy"] != DBNull.Value)
                {
                    total += Convert.ToDecimal(rowView["Amount_Payment_Policy"]);
                }
            }

            label14.Text = $"Сумма прибыли: {total:N2} руб.";
        }

        // Обработчик кнопки сброса всех фильтров
        private void button3_Click(object sender, EventArgs e)
        {
            // Сброс поля поиска
            textBox1.Text = "";

            // Сброс сортировки
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;

            // Сброс фильтра по страховщику
            if (comboBox2.Items.Count > 0)
                comboBox2.SelectedIndex = 0;

            // Сброс чекбоксов статусов
            if (checkBoxWaiting != null) checkBoxWaiting.Checked = false;
            if (checkBoxActive != null) checkBoxActive.Checked = false;
            if (checkBoxCompleted != null) checkBoxCompleted.Checked = false;
            if (checkBoxCanceled != null) checkBoxCanceled.Checked = false;

            // Сброс дат фильтрации
            dateTimePicker1.Value = minDateFromDatabase;
            dateTimePicker2.Value = maxDateFromDatabase;
            dateTimePicker2.MinDate = dateTimePicker1.Value;

            // Сброс фильтров и сортировки в DataTable
            if (dt != null)
            {
                dt.DefaultView.RowFilter = "";
                dt.DefaultView.Sort = "";
            }

            // Сброс подсветки строк
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                    row.DefaultCellStyle.BackColor = Color.White;
            }

            // Обновление интерфейса
            UpdateTotalAmount();
            UpdateRecordCount();
        }

        // Метод обновления счетчика записей
        private void UpdateRecordCount()
        {
            if (dt == null) return;
            label1.Text = $"Количество записей: {dt.DefaultView.Count}";
        }

        // Обработчик кнопки экспорта данных в Excel
        private void button2_Click(object sender, EventArgs e)
        {
            // Проверка наличия данных для экспорта
            if (dataGridView1.Rows.Count == 0 || (dataGridView1.Rows.Count == 1 && dataGridView1.Rows[0].IsNewRow))
            {
                MessageBox.Show("Нет данных для экспорта.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Диалог сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files|*.xlsx";
            saveFileDialog.Title = "Сохранить отчет в Excel";
            saveFileDialog.FileName = $"Отчет по страховкам {DateTime.Now:dd.MM.yyyy}";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    CreateExcelReport(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Метод создания отчета в Excel
        private void CreateExcelReport(string filePath)
        {
            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            {
                excelApp = new Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                workbook = excelApp.Workbooks.Add();
                worksheet = workbook.ActiveSheet;

                int currentRow = 1;
                decimal totalProfit = 0;
                int recordCount = 0;

                // Заголовок отчета
                worksheet.Cells[currentRow, 1] = "ОТЧЕТ ПО СТРАХОВЫМ ПОЛИСАМ";
                worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 8]].Merge();
                worksheet.Cells[currentRow, 1].Font.Bold = true;
                worksheet.Cells[currentRow, 1].Font.Size = 14;
                currentRow += 2;

                // Заголовки столбцов
                string[] headers = { "Номер полиса", "Дата начала", "Дата окончания", "Страхователь",
                                   "Телефон", "Страховщик", "Статус", "Страховой взнос" };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[currentRow, i + 1] = headers[i];
                    worksheet.Cells[currentRow, i + 1].Font.Bold = true;
                }
                currentRow++;

                // Заполнение данными
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    decimal paymentAmount = GetSafeDecimal(row, "Amount_Payment_Policy");
                    totalProfit += paymentAmount;
                    recordCount++;

                    worksheet.Cells[currentRow, 1] = GetSafeString(row, "Policy_Number");
                    worksheet.Cells[currentRow, 2] = GetSafeDateStringExcel(row, "Start_Data");
                    worksheet.Cells[currentRow, 3] = GetSafeDateStringExcel(row, "End_Data");
                    worksheet.Cells[currentRow, 4] = GetSafeString(row, "FIO_Client");
                    worksheet.Cells[currentRow, 5] = GetSafeString(row, "Phone_Client");
                    worksheet.Cells[currentRow, 6] = GetSafeString(row, "FIO_Users");
                    worksheet.Cells[currentRow, 7] = GetSafeString(row, "Name_Status");
                    worksheet.Cells[currentRow, 8] = paymentAmount.ToString("N2") + " руб.";

                    currentRow++;
                }

                // Добавление итогов
                if (recordCount > 0)
                {
                    currentRow++;
                    worksheet.Cells[currentRow, 1] = "ИТОГО:";
                    worksheet.Cells[currentRow, 1].Font.Bold = true;
                    currentRow++;

                    worksheet.Cells[currentRow, 1] = "Количество полисов:";
                    worksheet.Cells[currentRow, 2] = recordCount.ToString();
                    currentRow++;

                    worksheet.Cells[currentRow, 1] = "Общая прибыль:";
                    worksheet.Cells[currentRow, 2] = totalProfit.ToString("N2") + " руб.";
                    worksheet.Cells[currentRow, 2].Font.Bold = true;
                    currentRow++;

                    worksheet.Cells[currentRow, 1] = $"Отчет сформирован: {DateTime.Now:dd.MM.yyyy HH:mm}";
                }

                // Автоматическое выравнивание ширины столбцов
                worksheet.Columns.AutoFit();

                // Сохранение файла
                workbook.SaveAs(filePath);

                MessageBox.Show($"Отчет сохранен: {filePath}\nКоличество записей: {recordCount}\nПрибыль: {totalProfit:N2} руб.",
                              "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Автоматическое открытие файла
                try
                {
                    System.Diagnostics.Process.Start(filePath);
                }
                catch { }
            }
            finally
            {
                // Освобождение COM-объектов
                try
                {
                    if (workbook != null)
                    {
                        workbook.Close(false);
                        Marshal.ReleaseComObject(workbook);
                    }
                }
                catch { }

                try
                {
                    if (excelApp != null)
                    {
                        excelApp.Quit();
                        Marshal.ReleaseComObject(excelApp);
                    }
                }
                catch { }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        // Метод безопасного получения десятичного значения из ячейки
        private decimal GetSafeDecimal(DataGridViewRow row, string columnName)
        {
            try
            {
                if (row.Cells[columnName]?.Value != null && decimal.TryParse(row.Cells[columnName].Value.ToString(), out decimal result))
                {
                    return result;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        // Метод безопасного получения даты в формате Excel
        private string GetSafeDateStringExcel(DataGridViewRow row, string columnName)
        {
            try
            {
                if (row.Cells[columnName]?.Value != null && row.Cells[columnName].Value is DateTime)
                {
                    return ((DateTime)row.Cells[columnName].Value).ToString("dd.MM.yyyy");
                }
                return "";
            }
            catch
            {
                return "";
            }
        }
    }
}