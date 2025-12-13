
using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Kersovoi_Strahovka_Malev
{
    public partial class Autorization : Form
    {
        // Счетчик неудачных попыток входа
        int count = 3;
        // Строка подключения к базе данных MySQL
        string connectionStr = @"host=localhost;
                                 uid=root;
                                 pwd=root;
                                 database=kursach;";
        public Autorization()
        {
            InitializeComponent();
            // Установка максимальной длины пароля
            textBox2.MaxLength = 10;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // Скрытие вводимых символов пароля
            textBox2.UseSystemPasswordChar = true;
        }
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Валидация вводимых символов для пароля
            // Разрешены только латинские буквы, цифры и специальные символы
            if (!char.IsControl(e.KeyChar) &&
                !char.IsWhiteSpace(e.KeyChar) &&
                (e.KeyChar < 'A' || e.KeyChar > 'z') &&
                (e.KeyChar < '0' || e.KeyChar > '9') &&
                !"!@#$%^&*()-_=+[]{}|;:'\",.<>?/\\`~".Contains(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // Переключение видимости/невидимости пароля
            textBox2.UseSystemPasswordChar = !checkBox1.Checked;
        }
        // Хеширование пароля с использованием SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
        // Обработка нажатия кнопки "Вход"
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string login = textBox1.Text.Trim();
                string passwd = textBox2.Text;
                // Проверка на вход администратора по умолчанию
                if (login == "admin" && passwd == "admin")
                {
                    Menu_SA frm1 = new Menu_SA("Администратор");
                    this.Visible = false;
                    frm1.ShowDialog();
                    ClearFields();
                    this.Visible = true;
                    count = 3;
                    return;
                }
                // Хеширование введенного пароля для сравнения с БД
                string hashedPasswd = HashPassword(passwd);
                using (MySqlConnection con = new MySqlConnection(connectionStr))
                {
                    con.Open();
                    // SQL-запрос для проверки пользователя
                    string query = "SELECT ID_Users, FIO_Users, ID_Role FROM Users WHERE Login = @login AND Password = @hashedPassword";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@hashedPassword", hashedPasswd);
                    MySqlDataAdapter sda = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sda.Fill(dt);
                    // Если пользователь не найден
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show($"Пользователь не найден!", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        count--;
                        CheckLockout();
                        return;
                    }
                    // Получение данных пользователя из БД
                    string roleId = dt.Rows[0]["ID_Role"].ToString();
                    string fullName = dt.Rows[0]["FIO_Users"].ToString();
                    // Открытие соответствующей формы в зависимости от роли пользователя
                    if (roleId == "1")
                    {
                        Menu_SA frm1 = new Menu_SA(fullName);
                        this.Visible = false;
                        frm1.ShowDialog();
                        ClearFields();
                        this.Visible = true;
                    }
                    else if (roleId == "2")
                    {
                        Menu_St frm2 = new Menu_St(fullName);
                        this.Visible = false;
                        frm2.ShowDialog();
                        ClearFields();
                        this.Visible = true;
                    }
                    else if (roleId == "3")
                    {
                        Menu_Dr frm4 = new Menu_Dr(fullName);
                        this.Visible = false;
                        frm4.ShowDialog();
                        ClearFields();
                        this.Visible = true;
                    }
                    else
                    {
                        MessageBox.Show("Неизвестная роль пользователя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    count = 3;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка входа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ClearFields();
            }
        }
        // Проверка блокировки после неудачных попыток входа
        private void CheckLockout()
        {
            if (count <= 0)
            {
                // Блокировка полей ввода и кнопки на 10 секунд
                button1.Enabled = textBox1.Enabled = textBox2.Enabled = false;
                MessageBox.Show("Вы ввели неправильный логин или пароль. Вход заблокирован на 10 секунд.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.DoEvents();
                Thread.Sleep(10000);
                button1.Enabled = textBox1.Enabled = textBox2.Enabled = true;
                count = 3;
            }
            else if (count == 1)
            {
                MessageBox.Show("Вы ошиблись при вводе пароля. Пожалуйста, попробуйте снова.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        // Очистка полей ввода
        private void ClearFields()
        {
            textBox1.Text = "";
            textBox2.Text = "";
        }
        // Обработка нажатия кнопки "Закрыть"
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        // Вспомогательная переменная для отслеживания изменений
        private bool _changing = false;
    }
}