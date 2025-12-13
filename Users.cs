using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Kersovoi_Strahovka_Malev
{
    public partial class Users : Form
    {
        // Переменная для хранения названия роли текущего пользователя
        private string currentUserRoleName = "";
        // Строка подключения к базе данных MySQL
        string connectionStr = @"host=localhost;
                                 uid=root;
                                 pwd=root;
                                 database=kursach;";
        // Переменная для хранения хеша текущего пароля
        private string currentPasswordHash = "";
        // Идентификатор текущего выбранного пользователя (-1 означает "не выбран")
        private int currentUserId = -1;
        // Путь к текущему файлу фотографии
        private string currentPhotoPath = "";
        // Путь к папке с изображениями
        private string imagesFolderPath = "";
        // Изображение-заглушка для отсутствующих фото
        private Image placeholderImage;
        // Таблица данных для хранения информации о пользователях
        private DataTable dt;

        // Конструктор формы
        public Users()
        {
            InitializeComponent();
        }

        // Обработчик загрузки формы
        private void Users_Load(object sender, EventArgs e)
        {
            try
            {
                // Изменяем путь - используем Application.StartupPath
                imagesFolderPath = Path.Combine(Application.StartupPath, "Image");

                // Если папки не существует, создаем
                if (!Directory.Exists(imagesFolderPath))
                {
                    Directory.CreateDirectory(imagesFolderPath);
                }

                // Загружаем заглушку
                LoadPlaceholderImage();

                // Настройка DataGridView
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;
                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.RowHeadersVisible = false;

                // Подписываемся на событие форматирования ячеек
                dataGridView1.CellFormatting += dataGridView1_CellFormatting;

                LoadUsersToGrid();
                LoadRoles();

                // ОЧИЩАЕМ выделение после загрузки данных
                dataGridView1.ClearSelection();

                // Настройка PictureBox
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox1.BorderStyle = BorderStyle.FixedSingle;
                pictureBox1.Image = placeholderImage;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке: " + ex.Message);
            }
        }


        // Метод загрузки изображения-заглушки
        private void LoadPlaceholderImage()
        {
            try
            {
                string placeholderPath = Path.Combine(imagesFolderPath, "empty_photocamera.png");
                if (File.Exists(placeholderPath))
                {
                    // Загрузка и изменение размера изображения
                    using (var original = Image.FromFile(placeholderPath))
                    {
                        placeholderImage = new Bitmap(128, 128);
                        using (Graphics g = Graphics.FromImage(placeholderImage))
                        {
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.DrawImage(original, 0, 0, 128, 128);
                        }
                    }
                }
                else
                {
                    // Создание простой заглушки программно, если файл не найден
                    placeholderImage = new Bitmap(128, 128);
                    using (Graphics g = Graphics.FromImage(placeholderImage))
                    {
                        g.Clear(Color.LightGray);
                        g.DrawString("No Image", new Font("Arial", 10), Brushes.Black, 30, 55);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заглушки: {ex.Message}");
                // Создание заглушки в случае ошибки
                placeholderImage = new Bitmap(128, 128);
                using (Graphics g = Graphics.FromImage(placeholderImage))
                {
                    g.Clear(Color.LightGray);
                    g.DrawString("No Image", new Font("Arial", 10), Brushes.Black, 30, 55);
                }
            }
        }

        // Метод загрузки и изменения размера изображения
        private Image LoadAndResizeImage(string imageName, int width, int height)
        {
            try
            {
                if (string.IsNullOrEmpty(imageName))
                {
                    return placeholderImage;
                }

                string imagePath = Path.Combine(imagesFolderPath, imageName);

                // Возврат заглушки, если файл не существует
                if (!File.Exists(imagePath))
                {
                    return placeholderImage;
                }

                // Загрузка оригинального изображения
                using (var originalImage = Image.FromFile(imagePath))
                {
                    // Создание нового изображения нужного размера
                    Bitmap resizedImage = new Bitmap(width, height);
                    using (Graphics g = Graphics.FromImage(resizedImage))
                    {
                        // Настройка качественного масштабирования
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                        // Рисование белого фона
                        g.Clear(Color.White);

                        // Расчет коэффициента масштабирования для сохранения пропорций
                        float scale = Math.Min((float)width / originalImage.Width, (float)height / originalImage.Height);
                        int newWidth = (int)(originalImage.Width * scale);
                        int newHeight = (int)(originalImage.Height * scale);
                        int x = (width - newWidth) / 2;
                        int y = (height - newHeight) / 2;

                        // Рисование изображения с сохранением пропорций
                        g.DrawImage(originalImage, x, y, newWidth, newHeight);
                    }
                    return resizedImage;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки изображения {imageName}: {ex.Message}");
                return placeholderImage;
            }
        }

        // Метод отображения фотографии в PictureBox
        private void DisplayPhotoInPictureBox(string imageName)
        {
            try
            {
                if (string.IsNullOrEmpty(imageName))
                {
                    pictureBox1.Image = placeholderImage;
                    return;
                }

                string imagePath = Path.Combine(imagesFolderPath, imageName);

                if (File.Exists(imagePath))
                {
                    // Загрузка оригинального изображения
                    using (var originalImage = Image.FromFile(imagePath))
                    {
                        // Создание изображения под размер PictureBox
                        Bitmap displayImage = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                        using (Graphics g = Graphics.FromImage(displayImage))
                        {
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                            // Рисование белого фона
                            g.Clear(Color.White);

                            // Расчет коэффициента масштабирования для PictureBox
                            float scale = Math.Min((float)pictureBox1.Width / originalImage.Width,
                                                 (float)pictureBox1.Height / originalImage.Height);
                            int newWidth = (int)(originalImage.Width * scale);
                            int newHeight = (int)(originalImage.Height * scale);
                            int x = (pictureBox1.Width - newWidth) / 2;
                            int y = (pictureBox1.Height - newHeight) / 2;

                            // Рисование изображения по центру PictureBox
                            g.DrawImage(originalImage, x, y, newWidth, newHeight);
                        }
                        pictureBox1.Image = displayImage;
                    }
                }
                else
                {
                    pictureBox1.Image = placeholderImage;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отображения фото в PictureBox: {ex.Message}");
                pictureBox1.Image = placeholderImage;
            }
        }

        // Обработчик форматирования ячеек DataGridView
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Обработка колонки с фотографиями
            if (e.ColumnIndex == 0 && dataGridView1.Columns[0].Name == "PhotoColumn" && e.RowIndex >= 0)
            {
                try
                {
                    if (dt != null && e.RowIndex < dt.Rows.Count)
                    {
                        DataRow row = dt.Rows[e.RowIndex];
                        string photoName = row["Photo"]?.ToString();

                        // Загрузка и изменение размера изображения для ячейки
                        Image photo = LoadAndResizeImage(photoName, 64, 64);
                        e.Value = photo;
                    }
                    else
                    {
                        e.Value = placeholderImage;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в CellFormatting: {ex.Message}");
                    e.Value = placeholderImage;
                }
            }
        }

        // Метод хеширования пароля с использованием SHA256
        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 4)
                return "";

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Преобразование байтов в шестнадцатеричную строку
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        // Обработчик клика по ячейке DataGridView
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Проверка, что клик был по строке данных
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                FillTextBoxesFromRow(row);
            }
        }

        // Метод заполнения полей формы данными из строки DataGridView
        private void FillTextBoxesFromRow(DataGridViewRow row)
        {
            try
            {
                // Сохранение ID выбранного пользователя
                if (row.Cells["ID_Users"].Value != null && row.Cells["ID_Users"].Value != DBNull.Value)
                {
                    currentUserId = Convert.ToInt32(row.Cells["ID_Users"].Value);
                }
                else
                {
                    currentUserId = -1;
                }

                // Заполнение поля ФИО
                if (row.Cells["FIO_Users"].Value != null && row.Cells["FIO_Users"].Value != DBNull.Value)
                {
                    textBox1.Text = row.Cells["FIO_Users"].Value.ToString();
                }
                else
                {
                    textBox1.Text = "";
                }

                // Заполнение поля логина
                if (row.Cells["Login"].Value != null && row.Cells["Login"].Value != DBNull.Value)
                {
                    textBox2.Text = row.Cells["Login"].Value.ToString();
                }
                else
                {
                    textBox2.Text = "";
                }

                // Очистка поля пароля из соображений безопасности
                textBox3.Text = "";
                currentPasswordHash = "";

                // Загрузка и отображение фотографии пользователя
                if (row.Cells["Photo"].Value != null && row.Cells["Photo"].Value != DBNull.Value)
                {
                    currentPhotoPath = row.Cells["Photo"].Value.ToString();
                    DisplayPhotoInPictureBox(currentPhotoPath);
                }
                else
                {
                    currentPhotoPath = "";
                    pictureBox1.Image = placeholderImage;
                }

                // Установка роли пользователя в комбобоксе
                if (row.Cells["Name_Role"].Value != null && row.Cells["Name_Role"].Value != DBNull.Value)
                {
                    string roleName = row.Cells["Name_Role"].Value.ToString().Trim();

                    // Поиск точного совпадения роли в списке комбобокса
                    foreach (var item in comboBox1.Items)
                    {
                        if (item.ToString() == roleName)
                        {
                            comboBox1.SelectedItem = item;
                            break;
                        }
                    }
                }
                else
                {
                    comboBox1.SelectedIndex = 0; // Значение "Все"
                }

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
            textBox2.Text = "";
            textBox3.Text = "";
            comboBox1.SelectedIndex = 0;
            currentUserId = -1;
            currentPasswordHash = "";
            currentPhotoPath = "";
            pictureBox1.Image = placeholderImage;

            // Снятие выделения с таблицы
            dataGridView1.ClearSelection();
        }

        // Метод загрузки данных о пользователях из базы данных
        private void LoadUsersToGrid()
        {
            using (MySqlConnection con = new MySqlConnection(connectionStr))
            {
                con.Open();
                // SQL-запрос для получения данных о пользователях с объединением таблиц
                string sql = @"SELECT 
                                   Photo,
                                   Users.ID_Users,
                                   FIO_Users,
                                   Login,
                                   Password,
                                   Users.ID_Role,
                                   Role.Name_Role
                               FROM Users
                               INNER JOIN Role ON Users.ID_Role = Role.ID_Role";
                MySqlDataAdapter da = new MySqlDataAdapter(sql, con);
                dt = new DataTable();
                da.Fill(dt);

                // Конфигурация DataGridView перед установкой источника данных
                ConfigureDataGridView();

                dataGridView1.DataSource = dt;

                // Скрытие технических столбцов
                HideTechnicalColumns();

                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.AllowUserToAddRows = false;

                // Установка русских названий столбцов
                SetRussianColumnNames(dataGridView1);

                // Снятие выделения с таблицы
                dataGridView1.ClearSelection();
                con.Close();
            }
        }

        // Метод конфигурации DataGridView
        private void ConfigureDataGridView()
        {
            // Добавление колонки для изображений, если она отсутствует
            if (dataGridView1.Columns["PhotoColumn"] == null)
            {
                DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
                imageColumn.Name = "PhotoColumn";
                imageColumn.HeaderText = "Фотография";
                imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
                imageColumn.Width = 80;
                imageColumn.MinimumWidth = 80;

                // Вставка колонки с фотографиями на первое место
                dataGridView1.Columns.Insert(0, imageColumn);
            }

            // Увеличение высоты строк для лучшего отображения фотографий
            dataGridView1.RowTemplate.Height = 70;
        }

        // Метод скрытия технических столбцов (ID, пароли и т.д.)
        private void HideTechnicalColumns()
        {
            string[] columnsToHide = {
                "ID_Role", "ID_Users", "Password", "Photo"
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
                // Словарь для сопоставления английских и русских названий столбцов
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    {"PhotoColumn", "Фото"},
                    {"FIO_Users", "ФИО пользователя"},
                    {"Login", "Логин"},
                    {"Name_Role", "Роль"}
                };

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

        // Обработчик кнопки закрытия формы
        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Обработчик изменения выбранной роли в комбобоксе
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedRole = comboBox1.SelectedItem.ToString();
        }

        // Метод загрузки списка ролей из базы данных
        private void LoadRoles()
        {
            try
            {
                comboBox1.Items.Clear();

                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    string query = "SELECT Name_Role FROM Role";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBox1.Items.Add(reader["Name_Role"].ToString());
                        }
                    }
                    con.Close();
                }

                // Добавление пункта "Все" в начало списка
                comboBox1.Items.Insert(0, "Все");

                // Установка стиля выпадающего списка
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

                // Установка значения по умолчанию
                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке ролей: " + ex.Message);
            }
        }

        // Обработчик изменения текста в поле пароля
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            // Хеширование пароля будет выполняться при нажатии соответствующих кнопок
        }

        // Обработчик кнопки генерации пароля
        private void button4_Click(object sender, EventArgs e)
        {
            // Генерация случайного 4-значного пароля
            Random rnd = new Random();
            int password = rnd.Next(1000, 9999);
            string passwordString = password.ToString();

            // Отображение сгенерированного пароля в текстовом поле
            textBox3.Text = passwordString;

            // Хеширование сгенерированного пароля
            currentPasswordHash = HashPassword(passwordString);

            // Отображение пароля пользователю
            MessageBox.Show("Ваш пароль: " + passwordString, "Сгенерированный пароль", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Обработчик кнопки "Редактировать"
        private void button3_Click(object sender, EventArgs e)
        {
            if (currentUserId == -1)
            {
                MessageBox.Show("Выберите запись для редактирования.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string fio = textBox1.Text.Trim();
            string login = textBox2.Text.Trim();
            string selectedRole = comboBox1.SelectedItem.ToString();

            // Проверка заполнения обязательных полей
            if (fio == "" || login == "")
            {
                MessageBox.Show("Заполните ФИО и логин!");
                return;
            }

            if (selectedRole == "Все")
            {
                MessageBox.Show("Выберите роль пользователя!");
                return;
            }

            // Хеширование введенного пароля
            string password = textBox3.Text.Trim();
            if (!string.IsNullOrEmpty(password))
            {
                currentPasswordHash = HashPassword(password);
                if (string.IsNullOrEmpty(currentPasswordHash))
                {
                    MessageBox.Show("Пароль должен содержать не менее 4 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();

                    // Проверка уникальности логина (исключая текущего пользователя)
                    string checkSql = "SELECT COUNT(*) FROM Users WHERE Login = @login AND ID_Users != @id";
                    using (MySqlCommand cmd = new MySqlCommand(checkSql, con))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@id", currentUserId);
                        long count = (long)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Пользователь с таким логином уже существует!");
                            return;
                        }
                    }

                    // Получение ID выбранной роли
                    string getRoleIdSql = "SELECT ID_Role FROM Role WHERE Name_Role = @name";
                    int roleId;
                    using (MySqlCommand cmd2 = new MySqlCommand(getRoleIdSql, con))
                    {
                        cmd2.Parameters.AddWithValue("@name", selectedRole);
                        roleId = Convert.ToInt32(cmd2.ExecuteScalar());
                    }

                    // Формирование SQL-запроса для обновления данных
                    string updateSql = @"UPDATE Users 
                                 SET FIO_Users = @fio, 
                                     Login = @login, 
                                     ID_Role = @rid";

                    // Добавление пароля в запрос, если он был изменен
                    if (!string.IsNullOrEmpty(currentPasswordHash))
                    {
                        updateSql += ", Password = @pwd";
                    }

                    // Добавление фотографии в запрос, если она была выбрана
                    if (!string.IsNullOrEmpty(currentPhotoPath))
                    {
                        updateSql += ", Photo = @photo";
                    }

                    updateSql += " WHERE ID_Users = @id";

                    using (MySqlCommand cmd3 = new MySqlCommand(updateSql, con))
                    {
                        cmd3.Parameters.AddWithValue("@fio", fio);
                        cmd3.Parameters.AddWithValue("@login", login);
                        cmd3.Parameters.AddWithValue("@rid", roleId);
                        cmd3.Parameters.AddWithValue("@id", currentUserId);

                        if (!string.IsNullOrEmpty(currentPasswordHash))
                        {
                            cmd3.Parameters.AddWithValue("@pwd", currentPasswordHash);
                        }

                        if (!string.IsNullOrEmpty(currentPhotoPath))
                        {
                            cmd3.Parameters.AddWithValue("@photo", currentPhotoPath);
                        }

                        cmd3.ExecuteNonQuery();
                    }

                    con.Close();
                }

                // Обновление данных в таблице и очистка полей
                LoadUsersToGrid();
                ClearFields();
                MessageBox.Show("Данные пользователя обновлены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении пользователя: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик кнопки "Удалить"
        private void button2_Click(object sender, EventArgs e)
        {
            if (currentUserId == -1)
            {
                MessageBox.Show("Выберите строку в таблице для удаления!");
                return;
            }

            string roleName;
            using (MySqlConnection con = new MySqlConnection(connectionStr))
            {
                con.Open();
                // Получение роли выбранного пользователя
                string sql = @"
            SELECT r.Name_Role
            FROM Users u
            INNER JOIN Role r ON u.ID_Role = r.ID_Role
            WHERE u.ID_Users = @id";
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", currentUserId);
                    object obj = cmd.ExecuteScalar();
                    if (obj == null)
                    {
                        MessageBox.Show("Не удалось определить роль пользователя!");
                        return;
                    }
                    roleName = obj.ToString();
                }
                con.Close();
            }

            // Проверка: запрет удаления системного администратора
            if (roleName.Equals("Системный администратор", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Нельзя удалить пользователя с ролью 'Системный администратор'!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка: запрет удаления пользователя с той же ролью, что и текущий
            if (roleName.Equals(currentUserRoleName, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Вы не можете удалить пользователя с той же ролью, под которой вы вошли!");
                return;
            }

            // Подтверждение удаления
            DialogResult dr = MessageBox.Show("Удалить выбранного пользователя?", "Подтверждение", MessageBoxButtons.YesNo);
            if (dr != DialogResult.Yes)
                return;

            using (MySqlConnection con = new MySqlConnection(connectionStr))
            {
                con.Open();
                string deleteSql = "DELETE FROM Users WHERE ID_Users = @id";
                using (MySqlCommand cmd = new MySqlCommand(deleteSql, con))
                {
                    cmd.Parameters.AddWithValue("@id", currentUserId);
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }

            // Обновление данных в таблице и очистка полей
            LoadUsersToGrid();
            ClearFields();
            MessageBox.Show("Пользователь удалён.");
        }

        // Обработчик кнопки "Добавить"
        private void button1_Click(object sender, EventArgs e)
        {
            string fio = textBox1.Text.Trim();
            string login = textBox2.Text.Trim();
            string selectedRole = comboBox1.SelectedItem.ToString();

            // Проверка заполнения обязательных полей
            if (fio == "" || login == "")
            {
                MessageBox.Show("Заполните ФИО и логин!");
                return;
            }

            if (selectedRole == "Все")
            {
                MessageBox.Show("Выберите роль пользователя!");
                return;
            }

            // Хеширование введенного пароля
            string password = textBox3.Text.Trim();
            currentPasswordHash = HashPassword(password);
            if (string.IsNullOrEmpty(currentPasswordHash))
            {
                MessageBox.Show("Пароль должен содержать не менее 4 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionStr))
            {
                con.Open();

                // Проверка уникальности логина
                string checkSql = "SELECT COUNT(*) FROM Users WHERE Login = @login";
                using (MySqlCommand cmd = new MySqlCommand(checkSql, con))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    long count = (long)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!");
                        return;
                    }
                }

                // Получение ID выбранной роли
                string getRoleIdSql = "SELECT ID_Role FROM Role WHERE Name_Role = @name";
                int roleId;
                using (MySqlCommand cmd2 = new MySqlCommand(getRoleIdSql, con))
                {
                    cmd2.Parameters.AddWithValue("@name", selectedRole);
                    roleId = Convert.ToInt32(cmd2.ExecuteScalar());
                }

                // SQL-запрос для добавления нового пользователя
                string insertSql = @"
            INSERT INTO Users (FIO_Users, Login, Password, ID_Role, Photo)
            VALUES (@fio, @login, @pwd, @rid, @photo)";
                using (MySqlCommand cmd3 = new MySqlCommand(insertSql, con))
                {
                    cmd3.Parameters.AddWithValue("@fio", fio);
                    cmd3.Parameters.AddWithValue("@login", login);
                    cmd3.Parameters.AddWithValue("@pwd", currentPasswordHash);
                    cmd3.Parameters.AddWithValue("@rid", roleId);
                    cmd3.Parameters.AddWithValue("@photo", currentPhotoPath ?? "");
                    cmd3.ExecuteNonQuery();
                }

                con.Close();
            }

            // Обновление данных в таблице и очистка полей
            LoadUsersToGrid();
            ClearFields();
            MessageBox.Show("Пользователь добавлен!");
        }

        // Обработчик изменения текста в поле логина
        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            int cursorPosition = textBox.SelectionStart;
            string input = textBox.Text;
            StringBuilder result = new StringBuilder();

            // Фильтрация ввода: разрешаем только английские буквы, цифры и подчеркивание
            foreach (char c in input)
            {
                if ((c >= 'A' && c <= 'z') ||  // английские буквы
                    (c >= '0' && c <= '9') ||  // цифры
                    c == '_')                  // подчёркивание
                {
                    result.Append(c);
                }
            }

            // Обновление текста, если он изменился
            if (textBox.Text != result.ToString())
            {
                textBox.Text = result.ToString();
                textBox.SelectionStart = cursorPosition > 0 ? cursorPosition - 1 : 0;
            }
        }

        // Обработчик клика по пустому месту DataGridView
        private void dataGridView1_Click(object sender, EventArgs e)
        {
            // Очистка полей при клике на пустое место таблицы
            if (dataGridView1.HitTest((e as MouseEventArgs).X, (e as MouseEventArgs).Y).RowIndex == -1)
            {
                ClearFields();
            }
        }

        // Обработчик кнопки добавления фотографии
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
                    dlg.Title = "Выберите фотографию пользователя";
                    dlg.InitialDirectory = imagesFolderPath;

                    if (dlg.ShowDialog() != DialogResult.OK)
                        return;

                    string source = dlg.FileName;
                    string fileName = Path.GetFileName(source);
                    string destination = Path.Combine(imagesFolderPath, fileName);

                    // Проверка размера файла (максимум 2 МБ)
                    long sizeMB = new FileInfo(source).Length / (1024 * 1024);
                    if (sizeMB > 2)
                    {
                        MessageBox.Show("Размер файла слишком большой! Максимум 2 МБ.",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Копирование файла, если он не находится в папке images
                    if (!source.StartsWith(imagesFolderPath))
                    {
                        // Добавление суффикса, если файл с таким именем уже существует
                        if (File.Exists(destination))
                        {
                            string name = Path.GetFileNameWithoutExtension(fileName);
                            string ext = Path.GetExtension(fileName);
                            int n = 1;

                            while (File.Exists(destination))
                            {
                                destination = Path.Combine(imagesFolderPath, name + "_" + n + ext);
                                n++;
                            }
                        }

                        File.Copy(source, destination);
                        currentPhotoPath = Path.GetFileName(destination);
                    }
                    else
                    {
                        currentPhotoPath = fileName;
                    }

                    // Отображение выбранной фотографии
                    DisplayPhotoInPictureBox(currentPhotoPath);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик изменения текста в поле ФИО
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            int cursorPosition = textBox.SelectionStart;
            string input = textBox.Text;

            // Фильтрация ввода: разрешаем только русские буквы, пробелы и дефисы
            StringBuilder filtered = new StringBuilder();
            foreach (char c in input)
            {
                if ((c >= 'А' && c <= 'я') || c == 'Ё' || c == 'ё' || c == ' ' || c == '-')
                {
                    filtered.Append(c);
                }
            }

            string cleaned = filtered.ToString();

            // Удаление пробелов в начале строки
            while (cleaned.StartsWith(" "))
                cleaned = cleaned.Substring(1);

            // Замена двойных пробелов на одинарные
            while (cleaned.Contains("  "))
                cleaned = cleaned.Replace("  ", " ");

            // Форматирование текста: каждое слово с заглавной буквы
            StringBuilder result = new StringBuilder();
            bool makeUpper = true;

            foreach (char c in cleaned)
            {
                if (makeUpper && Char.IsLetter(c))
                {
                    result.Append(Char.ToUpper(c));
                    makeUpper = false;
                }
                else
                {
                    result.Append(Char.ToLower(c));
                }

                if (c == ' ')
                {
                    makeUpper = true;
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

                // Корректировка позиции курсора
                textBox.SelectionStart = Math.Max(0, cursorPosition - diff);
            }
        }
    }
}