using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Kersovoi_Strahovka_Malev
{
    public partial class New_Strahovka : Form
    {
        private string connectionStr = @"host=localhost;uid=root;pwd=root;database=kursach;";
        // Переменная для хранения ФИО текущего пользователя
        private string currentUserFIO;
        // Флаг, указывающий был ли выполнен расчет страхового взноса
        private bool isPremiumCalculated = false;
        // Всплывающая подсказка для отображения информации
        private ToolTip toolTip1;
        // Свойство для получения и установки ФИО клиента
        public string FIO
        {
            get => textBox1.Text;
            set => textBox1.Text = value;
        }
        // Свойство для получения и установки номера паспорта
        public string PassportNumber
        {
            get => maskedTextBox1.Text;
            set => maskedTextBox1.Text = value;
        }
        // Конструктор формы, принимает ФИО текущего пользователя
        public New_Strahovka(string userFIO)
        {
            InitializeComponent();
            currentUserFIO = userFIO;

            // Инициализация всплывающей подсказки
            toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;

            // Изначально кнопка просмотра страховки неактивна
            button4.Enabled = false;
            // Добавление обработчиков событий для полей ввода
            AddDataChangeHandlers();
        }
        // Метод для добавления обработчиков событий изменения данных
        private void AddDataChangeHandlers()
        {
            // Обработчики для текстовых полей
            textBox1.TextChanged += OnDataChanged;
            textBox3.TextChanged += OnDataChanged;
            textBox4.TextChanged += OnDataChanged;
            textBox5.TextChanged += OnDataChanged;
            // Обработчики для выпадающих списков
            comboBox1.SelectedIndexChanged += OnDataChanged;
            comboBox2.SelectedIndexChanged += OnDataChanged;
            comboBox3.SelectedIndexChanged += OnDataChanged;
            comboBox4.SelectedIndexChanged += OnDataChanged;
            // Обработчик для выбора даты
            dateTimePicker3.ValueChanged += OnDataChanged;
            // Обработчики для чекбоксов страховых случаев
            checkBox1.CheckedChanged += OnDataChanged;
            checkBox2.CheckedChanged += OnDataChanged;
            checkBox3.CheckedChanged += OnDataChanged;
            checkBox4.CheckedChanged += OnDataChanged;
            checkBox5.CheckedChanged += OnDataChanged;
            checkBox6.CheckedChanged += OnDataChanged;
        }
        // Обработчик события изменения данных в форме
        private void OnDataChanged(object sender, EventArgs e)
        {
            // Если был выполнен расчет, сбрасываем флаг и деактивируем кнопку просмотра
            if (isPremiumCalculated)
            {
                isPremiumCalculated = false;
                button4.Enabled = false;
                // Добавляем отметку о необходимости перерасчета
                if (!label25.Text.Contains("(требуется перерасчет)"))
                {
                    label25.Text = "Сумма страхового взноса: (требуется перерасчет)";
                }
            }
        }
        // Обработчик кнопки закрытия формы
        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        // Обработчик кнопки просмотра страховки
        private void button4_Click(object sender, EventArgs e)
        {
            // Проверка заполнения обязательных полей
            if (!AreRequiredFieldsFilled())
            {
                MessageBox.Show("Заполните все обязательные поля перед просмотром страховки!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Проверка, был ли выполнен расчет страхового взноса
            if (!isPremiumCalculated || string.IsNullOrEmpty(label25.Text) || label25.Text == "Сумма страхового взноса:" || label25.Text.Contains("(требуется перерасчет)"))
            {
                MessageBox.Show("Сначала выполните расчет страхового взноса!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Создание формы для просмотра страховки
            Prosmotr_ST viewForm = new Prosmotr_ST(currentUserFIO);
            // Формирование списка страховых случаев
            List<string> insuranceCasesList = new List<string>();
            if (checkBox1.Checked) insuranceCasesList.Add("Порча имущества");
            if (checkBox2.Checked) insuranceCasesList.Add("Наводнение");
            if (checkBox5.Checked) insuranceCasesList.Add("Кражи");
            if (checkBox4.Checked) insuranceCasesList.Add("Ураганы и бури");
            if (checkBox3.Checked) insuranceCasesList.Add("Град и снегопад");
            if (checkBox6.Checked) insuranceCasesList.Add("Пожары");
            string insuranceCases = string.Join(", ", insuranceCasesList);
            // Извлечение суммы страхового взноса из текста метки
            string paymentAmount = "0";
            if (label25.Text.Contains(":"))
            {
                string[] parts = label25.Text.Split(':');
                if (parts.Length > 1)
                {
                    paymentAmount = parts[1].Replace(" руб.", "").Replace("(требуется перерасчет)", "").Trim();
                }
            }
            // Получение выбранного тарифа
            string rate = comboBox1.SelectedItem?.ToString() ?? "0.00";
            // Передача данных в форму просмотра
            viewForm.SetData(
                policyNumber: label1.Text,
                startDate: dateTimePicker1.Value.ToString("dd.MM.yyyy"),
                endDate: dateTimePicker2.Value.ToString("dd.MM.yyyy"),
                client: textBox1.Text,
                phone: maskedTextBox1.Text,
                address: textBox3.Text,
                amount: textBox4.Text,
                area: textBox5.Text,
                year: dateTimePicker3.Value.Year.ToString(),
                location: comboBox3.SelectedItem?.ToString(),
                material: comboBox2.SelectedItem?.ToString(),
                cases: insuranceCases,
                payment: paymentAmount,
                status: comboBox4.SelectedItem?.ToString(),
                rate: rate,
                photo: pictureBox1.Image
            );
            // Отображение формы просмотра
            viewForm.Show();
        }
        // Обработчик события при отклонении ввода в маскированном текстовом поле
        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            maskedTextBox1.BeginInvoke(new Action(() => { maskedTextBox1.Select(0, 0); }));
        }
        // Обработчик изменения даты начала действия страховки
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            // Установка минимальной даты окончания равной дате начала
            dateTimePicker2.MinDate = dateTimePicker1.Value;
            OnDataChanged(sender, e);
        }
        // Обработчик изменения даты окончания действия страховки
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            OnDataChanged(sender, e);
        }
        // Обработчик изменения текста в поле ФИО
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            int cursorPosition = textBox.SelectionStart;
            string input = textBox.Text;
            // Фильтрация: оставляем только русские буквы и пробелы
            StringBuilder filtered = new StringBuilder();
            foreach (char c in input)
            {
                if ((c >= 'А' && c <= 'я') || c == 'ё' || c == 'Ё' || c == ' ')
                    filtered.Append(c);
            }
            string cleaned = filtered.ToString();
            // Удаление лишних пробелов
            while (cleaned.StartsWith(" ")) cleaned = cleaned.Substring(1);
            while (cleaned.Contains("  ")) cleaned = cleaned.Replace("  ", " ");
            // Приведение к правильному регистру (каждое слово с заглавной буквы)
            StringBuilder result = new StringBuilder();
            bool makeUpper = true;
            foreach (char c in cleaned)
            {
                if (makeUpper && Char.IsLetter(c))
                {
                    result.Append(Char.ToUpper(c));
                    makeUpper = false;
                }
                else result.Append(Char.ToLower(c));

                if (c == ' ') makeUpper = true;
            }
            string finalText = result.ToString();
            // Обновление текста с сохранением позиции курсора
            if (textBox.Text != finalText)
            {
                int oldLength = textBox.Text.Length;
                int newLength = finalText.Length;
                int diff = oldLength - newLength;

                textBox.Text = finalText;
                textBox.SelectionStart = Math.Max(0, cursorPosition - diff);
            }
        }
        // Обработчик загрузки формы
        private void New_Strahovka_Load(object sender, EventArgs e)
        {
            // Установка ограничений для дат
            dateTimePicker2.MinDate = DateTime.Today.AddYears(1);
            dateTimePicker2.MaxDate = DateTime.Today.AddYears(5);
            dateTimePicker3.MinDate = DateTime.Today.AddYears(-50);
            dateTimePicker3.MaxDate = DateTime.Today;
            // Генерация номера страховки
            label1.Text = GenerateInsuranceNumber();
            // Загрузка данных в выпадающие списки
            LoadBasicRates();
            LoadBuildingMaterials();
            LoadLocations();
            LoadStatuses();
        }
        // Метод генерации номера страховки
        private string GenerateInsuranceNumber()
        {
            int lastNumber = GetLastInsuranceNumberFromDB();
            lastNumber++;
            return (2221 + lastNumber).ToString();
        }
        // Метод получения последнего номера страховки из базы данных
        private int GetLastInsuranceNumberFromDB()
        {
            return 0;
        }
        // Обработчик кнопки открытия формы со списком страхователей
        private void button1_Click(object sender, EventArgs e)
        {
            Strahovateli clientsForm = new Strahovateli();
            clientsForm.Show();
        }
        // Обработчик нажатия клавиш в поле адреса
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;
            // Разрешаем ввод цифр
            if (char.IsDigit(ch))
                return;
            // Разрешаем русские буквы
            if ((ch >= 'А' && ch <= 'я') || ch == 'Ё' || ch == 'ё')
                return;
            // Разрешаем точку и запятую
            if (ch == '.' || ch == ',')
                return;
            // Разрешаем управляющие символы (Backspace, Delete и т.д.)
            if (char.IsControl(ch))
                return;
            // Запрещаем все остальные символы
            e.Handled = true;
        }
        // Обработчик изменения текста в поле страховой суммы
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            int selStart = textBox4.SelectionStart;
            int selLength = textBox4.SelectionLength;
            // Фильтрация: оставляем только цифры
            string newText = "";
            foreach (char c in textBox4.Text)
            {
                if (char.IsDigit(c))
                    newText += c;
            }
            textBox4.Text = newText;
            // Восстановление позиции курсора
            textBox4.SelectionStart = selStart > textBox4.Text.Length ? textBox4.Text.Length : selStart;
            textBox4.SelectionLength = selLength;
        }
        // Обработчик изменения текста в поле площади
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            string text = textBox5.Text;
            // Фильтрация: оставляем цифры и одну точку
            string filtered = "";
            bool dotFound = false;
            foreach (char c in text)
            {
                if (char.IsDigit(c))
                {
                    filtered += c;
                }
                else if (c == '.' && !dotFound && filtered.Length > 0)
                {
                    filtered += c;
                    dotFound = true;
                }
            }
            // Обновление текста с сохранением позиции курсора
            if (text != filtered)
            {
                int cursorPos = textBox5.SelectionStart - (text.Length - filtered.Length);
                textBox5.Text = filtered;
                textBox5.SelectionStart = Math.Max(0, cursorPos);
            }
        }
        // Обработчик нажатия клавиш в поле площади
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только цифры, точку и управляющие символы
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
            // Запрещаем точку в начале текста
            if (e.KeyChar == '.' && (textBox5.SelectionStart == 0 || textBox5.Text.Length == 0))
            {
                e.Handled = true;
            }
            // Запрещаем вторую точку
            if (e.KeyChar == '.' && textBox5.Text.Contains("."))
            {
                e.Handled = true;
            }
            // Запрещаем запятую
            if (e.KeyChar == ',')
            {
                e.Handled = true;
            }
        }
        // Обработчик кнопки загрузки изображения
        private void button2_Click(object sender, EventArgs e)
        {
            // Используем папку рядом с исполняемым файлом
            string imagesFolder = Path.Combine(Application.StartupPath, "Image_home");

            // Проверка существования папки с изображениями
            if (!System.IO.Directory.Exists(imagesFolder))
            {
                MessageBox.Show("Папка Image_home не найдена!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Создание и настройка диалогового окна выбора файла
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = imagesFolder;
            openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Все файлы (*.*)|*.*";
            openFileDialog.Title = "Выберите изображение (только JPG и PNG до 2 МБ)";
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            // Обработка выбора файла
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFile = openFileDialog.FileName;
                try
                {
                    // Проверка размера файла
                    FileInfo fileInfo = new FileInfo(selectedFile);
                    long fileSizeInBytes = fileInfo.Length;
                    long maxSizeInBytes = 2 * 1024 * 1024; // 2 МБ

                    if (fileSizeInBytes > maxSizeInBytes)
                    {
                        MessageBox.Show($"Размер файла слишком большой!\n" +
                                      $"Текущий размер: {(fileSizeInBytes / 1024.0 / 1024.0):F2} МБ\n" +
                                      $"Максимальный размер: 2 МБ",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Warning);
                        return;
                    }
                    // Проверка расширения файла
                    string fileExtension = Path.GetExtension(selectedFile).ToLower();
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        MessageBox.Show("Разрешены только файлы форматов JPG и PNG!",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Warning);
                        return;
                    }
                    // Проверка, является ли файл валидным изображением
                    try
                    {
                        using (Image testImage = Image.FromFile(selectedFile))
                        {
                            // Файл является валидным изображением
                        }
                    }
                    catch (Exception imgEx)
                    {
                        MessageBox.Show($"Выбранный файл не является валидным изображением!\n" +
                                      $"Ошибка: {imgEx.Message}",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                        return;
                    }
                    // Загрузка изображения в PictureBox
                    pictureBox1.Image = Image.FromFile(selectedFile);
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    // Отображение информации об изображении
                    ShowImageInfo(selectedFile, fileSizeInBytes);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }
        // Метод для отображения информации об изображении во всплывающей подсказке
        private void ShowImageInfo(string filePath, long fileSizeInBytes)
        {
            try
            {
                using (Image img = Image.FromFile(filePath))
                {
                    string fileName = Path.GetFileName(filePath);
                    string fileExtension = Path.GetExtension(filePath).ToUpper();
                    string fileSize = $"{(fileSizeInBytes / 1024.0):F1} КБ";
                    string imageSize = $"{img.Width} x {img.Height} пикселей";
                    string format = img.PixelFormat.ToString();
                    // Установка текста всплывающей подсказки
                    toolTip1.SetToolTip(pictureBox1,
                        $"Файл: {fileName}\n" +
                        $"Размер: {fileSize}\n" +
                        $"Разрешение: {imageSize}\n" +
                        $"Формат: {fileExtension}\n" +
                        $"Цвет: {format}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении информации об изображении: {ex.Message}");
            }
        }
        // Метод загрузки базовых тарифов из базы данных
        private void LoadBasicRates()
        {
            try
            {
                comboBox1.Items.Clear();
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    string query = "SELECT Coefficient FROM Basic_Rate ORDER BY Coefficient";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string coefficientStr = reader["Coefficient"].ToString();
                            if (decimal.TryParse(coefficientStr.Replace(',', '.'), out decimal coefficient))
                            {
                                comboBox1.Items.Add(coefficient.ToString("0.00"));
                            }
                            else
                            {
                                comboBox1.Items.Add(coefficientStr);
                            }
                        }
                    }
                }
                // Добавление пункта "Все" и настройка выпадающего списка
                comboBox1.Items.Insert(0, "Все");
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                if (comboBox1.Items.Count > 0)
                {
                    comboBox1.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке базовых тарифов: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Метод загрузки материалов постройки из базы данных
        private void LoadBuildingMaterials()
        {
            try
            {
                comboBox2.Items.Clear();
                comboBox2.Tag = new Dictionary<string, string>();
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    string query = "SELECT Name_Materials, Coefficient FROM Building_materials ORDER BY Name_Materials";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        var materialsDict = new Dictionary<string, string>();

                        while (reader.Read())
                        {
                            string name = reader["Name_Materials"].ToString();
                            string coefficient = reader["Coefficient"].ToString();

                            comboBox2.Items.Add(name);
                            materialsDict[name] = coefficient;
                        }

                        comboBox2.Tag = materialsDict;
                    }
                }
                // Добавление пункта "Все" и настройка выпадающего списка
                comboBox2.Items.Insert(0, "Все");
                comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
                if (comboBox2.Items.Count > 0)
                {
                    comboBox2.SelectedIndex = 0;
                    UpdateMaterialCoefficient();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке материалов постройки: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Метод загрузки местоположений из базы данных
        private void LoadLocations()
        {
            try
            {
                comboBox3.Items.Clear();
                comboBox3.Tag = new Dictionary<string, string>();
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    string query = "SELECT Name_Location, Coefficient FROM Location ORDER BY Name_Location";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        var locationsDict = new Dictionary<string, string>();

                        while (reader.Read())
                        {
                            string name = reader["Name_Location"].ToString();
                            string coefficient = reader["Coefficient"].ToString();

                            comboBox3.Items.Add(name);
                            locationsDict[name] = coefficient;
                        }

                        comboBox3.Tag = locationsDict;
                    }
                }
                // Добавление пункта "Все" и настройка выпадающего списка
                comboBox3.Items.Insert(0, "Все");
                comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;

                if (comboBox3.Items.Count > 0)
                {
                    comboBox3.SelectedIndex = 0;
                    UpdateLocationCoefficient();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке местоположений: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Метод загрузки статусов из базы данных (только "В ожидание" и "Действует")
        private void LoadStatuses()
        {
            try
            {
                comboBox4.Items.Clear();
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    string query = "SELECT Name_Status FROM Status WHERE Name_Status IN ('В ожидание', 'Действует') ORDER BY ID_Status";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string statusName = reader["Name_Status"].ToString();
                            comboBox4.Items.Add(statusName);
                        }
                    }
                }
                comboBox4.DropDownStyle = ComboBoxStyle.DropDownList;
                if (comboBox4.Items.Count > 0)
                {
                    // Установка статуса "В ожидание" по умолчанию
                    foreach (var item in comboBox4.Items)
                    {
                        if (item.ToString() == "В ожидание")
                        {
                            comboBox4.SelectedItem = item;
                            return;
                        }
                    }
                    // Если "В ожидание" не найден, выбираем первый элемент
                    comboBox4.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке статусов: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Метод обновления коэффициента материала в метке
        private void UpdateMaterialCoefficient()
        {
            if (comboBox2.SelectedItem != null && comboBox2.Tag is Dictionary<string, string> materialsDict)
            {
                string selectedMaterial = comboBox2.SelectedItem.ToString();

                if (selectedMaterial == "Все")
                {
                    label16.Text = "";
                }
                else if (materialsDict.ContainsKey(selectedMaterial))
                {
                    string coefficient = materialsDict[selectedMaterial];
                    label16.Text = coefficient;
                }
                else
                {
                    label16.Text = "";
                }
            }
            else
            {
                label16.Text = "";
            }
        }
        // Метод обновления коэффициента местоположения в метке
        private void UpdateLocationCoefficient()
        {
            if (comboBox3.SelectedItem != null && comboBox3.Tag is Dictionary<string, string> locationsDict)
            {
                string selectedLocation = comboBox3.SelectedItem.ToString();

                if (selectedLocation == "Все")
                {
                    label17.Text = "";
                }
                else if (locationsDict.ContainsKey(selectedLocation))
                {
                    string coefficient = locationsDict[selectedLocation];
                    label17.Text = coefficient;
                }
                else
                {
                    label17.Text = "";
                }
            }
            else
            {
                label17.Text = "";
            }
        }
        // Обработчик изменения выбранного материала
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMaterialCoefficient();
            OnDataChanged(sender, e);
        }
        // Обработчик изменения выбранного местоположения
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateLocationCoefficient();
            OnDataChanged(sender, e);
        }
        // Обработчик изменения выбранного базового тарифа
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnDataChanged(sender, e);
        }
        // Обработчик изменения года постройки
        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            OnDataChanged(sender, e);
        }
        // Метод обновления всех данных на форме
        public void RefreshAllData()
        {
            LoadBasicRates();
            LoadBuildingMaterials();
            LoadLocations();
            LoadStatuses();
        }
        // Метод проверки заполнения всех обязательных полей
        private bool AreRequiredFieldsFilled()
        {
            // Проверка ФИО
            if (string.IsNullOrWhiteSpace(textBox1.Text)) return false;
            // Проверка номера паспорта
            if (string.IsNullOrWhiteSpace(maskedTextBox1.Text) || maskedTextBox1.Text.Contains(" ")) return false;
            // Проверка адреса
            if (string.IsNullOrWhiteSpace(textBox3.Text)) return false;
            // Проверка базового тарифа
            if (comboBox1.SelectedItem == null || comboBox1.SelectedItem.ToString() == "Все") return false;
            // Проверка страховой суммы
            if (string.IsNullOrWhiteSpace(textBox4.Text)) return false;
            // Проверка площади
            if (string.IsNullOrWhiteSpace(textBox5.Text)) return false;
            // Проверка материала постройки
            if (comboBox2.SelectedItem == null || comboBox2.SelectedItem.ToString() == "Все") return false;
            // Проверка местоположения
            if (comboBox3.SelectedItem == null || comboBox3.SelectedItem.ToString() == "Все") return false;
            // Проверка статуса
            if (comboBox4.SelectedItem == null) return false;
            // Проверка, что выбран хотя бы один страховой случай
            if (!checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked &&
                !checkBox4.Checked && !checkBox5.Checked && !checkBox6.Checked)
                return false;
            return true;
        }
        // Метод расчета страхового взноса
        private decimal CalculateInsurancePremium()
        {
            try
            {
                // Получение страховой суммы и проверка ее валидности
                if (!decimal.TryParse(textBox4.Text, out decimal insuranceAmount) || insuranceAmount <= 0)
                    return 0;
                // Получение площади и проверка ее валидности
                if (!decimal.TryParse(textBox5.Text, out decimal area) || area <= 0)
                    return 0;
                // Расчет стоимости квадратного метра
                decimal costPerSquareMeter = insuranceAmount / area;
                // Получение всех коэффициентов
                decimal ageCoefficient = CalculateAgeCoefficient();
                decimal locationCoefficient = GetLocationCoefficient();
                decimal materialCoefficient = GetMaterialCoefficient();
                decimal casesCoefficient = GetInsuranceCasesCoefficient();
                decimal baseCoefficient = GetBaseCoefficient();
                // Расчет базовой стоимости и итоговой премии
                decimal baseCost = costPerSquareMeter * area;
                decimal premium = baseCost * baseCoefficient * ageCoefficient * locationCoefficient * materialCoefficient * casesCoefficient;
                // Округление результата до 2 знаков после запятой
                return Math.Round(premium, 2);
            }
            catch (Exception)
            {
                return 0;
            }
        }
        // Метод расчета коэффициента возраста постройки
        private decimal CalculateAgeCoefficient()
        {
            if (dateTimePicker3.Value == null) return 1.0m;
            int buildingAge = DateTime.Now.Year - dateTimePicker3.Value.Year;
            // Определение коэффициента в зависимости от возраста постройки
            if (buildingAge <= 10) return 1.0m;
            else if (buildingAge <= 20) return 1.1m;
            else return 1.2m;
        }
        // Метод получения коэффициента местоположения
        private decimal GetLocationCoefficient()
        {
            if (comboBox3.SelectedItem == null || comboBox3.SelectedItem.ToString() == "Все") return 1.0m;
            string location = comboBox3.SelectedItem.ToString();
            // Определение коэффициента в зависимости от местоположения
            switch (location.ToLower())
            {
                case "город": return 1.2m;
                case "село": return 1.0m;
                case "пригород": return 1.1m;
                default: return 1.0m;
            }
        }
        // Метод получения коэффициента материала постройки
        private decimal GetMaterialCoefficient()
        {
            if (comboBox2.SelectedItem == null || comboBox2.SelectedItem.ToString() == "Все") return 1.0m;
            string material = comboBox2.SelectedItem.ToString();
            // Определение коэффициента в зависимости от материала
            switch (material.ToLower())
            {
                case "камень": return 1.5m;
                case "дерево": return 1.2m;
                case "смешанный": return 1.0m;
                default: return 1.0m;
            }
        }
        // Метод расчета коэффициента страховых случаев
        private decimal GetInsuranceCasesCoefficient()
        {
            decimal casesCoefficient = 1.0m;
            // Умножение коэффициента на значение каждого выбранного страхового случая
            if (checkBox1.Checked) casesCoefficient *= 1.19m;
            if (checkBox2.Checked) casesCoefficient *= 1.11m;
            if (checkBox5.Checked) casesCoefficient *= 1.2m;
            if (checkBox4.Checked) casesCoefficient *= 1.12m;
            if (checkBox3.Checked) casesCoefficient *= 1.194m;
            if (checkBox6.Checked) casesCoefficient *= 1.2m;
            return casesCoefficient;
        }
        // Метод получения базового коэффициента
        private decimal GetBaseCoefficient()
        {
            if (comboBox1.SelectedItem == null || comboBox1.SelectedItem.ToString() == "Все")
                return 0.003m;
            string baseRate = comboBox1.SelectedItem.ToString();
            if (decimal.TryParse(baseRate, out decimal coefficient))
                return coefficient;
            return 0.003m;
        }
        // Обработчик кнопки расчета страхового взноса
        private void button3_Click(object sender, EventArgs e)
        {
            // Проверка заполнения обязательных полей перед расчетом
            if (!AreRequiredFieldsFilled())
            {
                MessageBox.Show("Заполните все обязательные поля перед расчетом!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Выполнение расчета
            decimal premium = CalculateInsurancePremium();
            if (premium > 0)
            {
                // Отображение результата расчета
                label25.Text = $"Сумма страхового взноса: {premium:N2} руб.";
                isPremiumCalculated = true;
                button4.Enabled = true;
            }
            else
            {
                MessageBox.Show("Ошибка при расчете страхового взноса!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                isPremiumCalculated = false;
                button4.Enabled = false;
            }
        }
        // Обработчик кнопки очистки формы
        private void button5_Click(object sender, EventArgs e)
        {
            // Очистка всех текстовых полей
            textBox1.Text = "";
            maskedTextBox1.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            // Сброс выпадающих списков к значениям по умолчанию
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            // Установка статуса "В ожидание" или первого доступного
            bool waitingFound = false;
            foreach (var item in comboBox4.Items)
            {
                if (item.ToString() == "В ожидании")
                {
                    comboBox4.SelectedItem = item;
                    waitingFound = true;
                    break;
                }
            }
            if (!waitingFound && comboBox4.Items.Count > 0)
            {
                comboBox4.SelectedIndex = 0;
            }
            // Сброс всех чекбоксов страховых случаев
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox6.Checked = false;
            // Очистка отображаемых коэффициентов и результата расчета
            label16.Text = "";
            label17.Text = "";
            label25.Text = "Сумма страхового взноса:";
            // Удаление загруженного изображения
            pictureBox1.Image = null;
            // Сброс флагов и состояния кнопок
            isPremiumCalculated = false;
            button4.Enabled = false;
            // Генерация нового номера страховки
            label1.Text = GenerateInsuranceNumber();
            // Информирование пользователя об успешной очистке
            MessageBox.Show("Форма очищена!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        // Обработчик изменения выбранного статуса
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnDataChanged(sender, e);
        }
    }
}