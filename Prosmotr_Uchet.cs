using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Kersovoi_Strahovka_Malev
{
    public partial class Prosmotr_Uchet : Form
    {
        // Строка подключения к базе данных
        private string connectionStr = @"host=localhost;uid=root;pwd=root;database=kursach;";
        // Номер страхового полиса
        private string policyNumber;
        // Роль пользователя (1 - администратор, 2 - страховщик)
        private int userRole;
        // Адрес здания
        private string addressBuilding;
        // Ссылка на DataGridView из родительской формы для обновления данных
        private DataGridView dataGridViewRef;
        // Текущий статус страховки
        private string currentStatus;
        // Флаг загрузки данных (чтобы не срабатывали события во время инициализации)
        private bool isLoading = true;
        // Конструктор по умолчанию
        public Prosmotr_Uchet()
        {
            InitializeComponent();
        }

        // Основной конструктор с передачей всех данных страховки
        public Prosmotr_Uchet(string policyNumber, string startDate, string endDate,
                             string fioClient, string phoneClient, string fioUser,
                             string insuranceAmount, string paymentAmount, string status,
                             string addressBuilding,
                             int role = 0, DataGridView dataGridView = null)
        {
            InitializeComponent();

            // Сохранение переданных параметров
            this.currentStatus = status;
            this.policyNumber = policyNumber;
            this.userRole = role;
            this.dataGridViewRef = dataGridView;
            this.addressBuilding = addressBuilding;
            // Заполнение полей формы переданными данными
            textBox1.Text = policyNumber;
            textBox2.Text = startDate;
            textBox3.Text = endDate;
            textBox4.Text = fioClient;
            textBox5.Text = phoneClient;
            textBox6.Text = fioUser;
            textBox7.Text = insuranceAmount;
            textBox8.Text = paymentAmount;
            textBox9.Text = addressBuilding;
            // Загрузка списка статусов в выпадающий список
            LoadStatusComboBox(status);
            // Установка флага загрузки в false - события теперь активны
            isLoading = false;
        }
        // Метод для загрузки доступных статусов в ComboBox
        private void LoadStatusComboBox(string currentStatus)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    // SQL-запрос для получения всех статусов из базы данных
                    string query = "SELECT ID_Status, Name_Status FROM Status";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    // Очистка списка перед загрузкой
                    comboBox1.Items.Clear();

                    // Чтение всех статусов из базы данных
                    while (reader.Read())
                    {
                        int statusId = reader.GetInt32("ID_Status");
                        string statusName = reader.GetString("Name_Status");
                        // Добавление статуса как объекта StatusItem
                        comboBox1.Items.Add(new StatusItem(statusId, statusName));
                    }
                    reader.Close();
                }
                // Установка текущего статуса как выбранного
                foreach (StatusItem item in comboBox1.Items)
                {
                    if (item.statusName == currentStatus)
                    {
                        comboBox1.SelectedItem = item;
                        break;
                    }
                }
                // Включение/отключение ComboBox в зависимости от роли пользователя
                // Только администратор (1) и страховщик (2) могут менять статусы
                comboBox1.Enabled = (userRole == 1 || userRole == 2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки статусов: " + ex.Message);
            }
        }
        // Обработчик изменения выбранного статуса
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Игнорирование события во время загрузки формы
            if (isLoading) return;
            // Проверка, что элемент выбран
            if (comboBox1.SelectedItem == null)
                return;
            // Получение выбранного статуса
            StatusItem selectedStatus = (StatusItem)comboBox1.SelectedItem;
            // Проверка прав для страховщика (роль 2)
            if (userRole == 2)
            {
                // Страховщик может устанавливать только статус "Отменена"
                if (selectedStatus.statusName != "Отменена")
                {
                    MessageBox.Show("Страховщик может устанавливать только статус: 'Отменена'.");
                    // Восстановление предыдущего статуса
                    isLoading = true;
                    LoadStatusComboBox(currentStatus);
                    isLoading = false;
                    return;
                }
                // Проверка ограничения по времени (14 дней)
                if (!Check14DaysLimit())
                {
                    MessageBox.Show("Отмена возможна только в течение 14 дней после оформления страховки.");
                    // Восстановление предыдущего статуса
                    isLoading = true;
                    LoadStatusComboBox(currentStatus);
                    isLoading = false;
                    return;
                }
            }
            // Подтверждение изменения статуса у пользователя
            DialogResult result = MessageBox.Show(
                $"Изменить статус на: {selectedStatus.statusName}?",
                "Подтверждение",
                MessageBoxButtons.YesNo
            );
            // Обновление статуса в базе данных при подтверждении
            if (result == DialogResult.Yes)
            {
                UpdateStatusInDatabase(selectedStatus.statusId, selectedStatus.statusName);
            }
        }
        // Проверка ограничения по времени для отмены (14 дней)
        private bool Check14DaysLimit()
        {
            try
            {
                // Получение даты начала страховки
                DateTime startDate = DateTime.Parse(textBox2.Text);
                DateTime now = DateTime.Now;

                // Проверка, что прошло не более 14 дней
                return (now - startDate).TotalDays <= 14;
            }
            catch
            {
                return false;
            }
        }
        // Обновление статуса в базе данных
        private void UpdateStatusInDatabase(int statusId, string statusName)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    // SQL-запрос для обновления статуса
                    string query = "UPDATE Insurance SET ID_Status = @StatusID WHERE Policy_Number = @PolicyNumber";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@StatusID", statusId);
                    cmd.Parameters.AddWithValue("@PolicyNumber", policyNumber);
                    // Выполнение запроса
                    int rows = cmd.ExecuteNonQuery();
                    // Если запрос выполнен успешно
                    if (rows > 0)
                    {
                        // Обновление данных в родительской форме
                        if (dataGridViewRef != null)
                        {
                            UpdateMainForm(statusName, statusId);
                        }

                        MessageBox.Show("Статус обновлен!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка обновления статуса: " + ex.Message);
            }
        }
        // Обновление данных в родительской форме (DataGridView)
        private void UpdateMainForm(string newStatus, int newStatusId)
        {
            try
            {
                // Проверка, что DataGridView имеет источник данных
                if (dataGridViewRef.DataSource is DataTable dt)
                {
                    // Поиск строки с соответствующим номером полиса
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["Policy_Number"].ToString() == policyNumber)
                        {
                            // Обновление статуса в строке
                            row["Name_Status"] = newStatus;
                            row["ID_Status"] = newStatusId;
                            break;
                        }
                    }
                    // Обновление отображения DataGridView
                    dataGridViewRef.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка обновления таблицы: " + ex.Message);
            }
        }
        // Обработчик нажатия кнопки закрытия формы
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
    // Вспомогательный класс для хранения информации о статусе
    public class StatusItem
    {
        public int statusId;      // ID статуса в базе данных
        public string statusName; // Название статуса

        // Конструктор
        public StatusItem(int id, string name)
        {
            statusId = id;
            statusName = name;
        }
        // Метод для отображения названия статуса в ComboBox
        public override string ToString()
        {
            return statusName;
        }
    }
}