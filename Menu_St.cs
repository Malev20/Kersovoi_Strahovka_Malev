using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kersovoi_Strahovka_Malev
{
    public partial class Menu_St : Form
    {
        // Поле для хранения ФИО текущего пользователя
        private string currentUserFIO;

        // Таймер для отслеживания неактивности
        private System.Windows.Forms.Timer inactivityTimer;
        private int secondsLeft = 30;

        // Конструктор формы, принимающий ФИО пользователя
        public Menu_St(string fullName)
        {
            InitializeComponent();
            currentUserFIO = fullName; // Сохраняем ФИО пользователя
            label2.Text = fullName; // Отображаем ФИО на форме

            // Инициализация таймера неактивности
            InitializeInactivityTimer();
        }

        private void InitializeInactivityTimer()
        {
           
            secondsLeft = ReadTimeoutFromFile();

            // Создание и настройка таймера
            inactivityTimer = new System.Windows.Forms.Timer();
            inactivityTimer.Interval = 1000; // 1 секунда
            inactivityTimer.Tick += InactivityTimer_Tick;
            inactivityTimer.Start();
        }

        // Обработчик тика таймера
        private void InactivityTimer_Tick(object sender, EventArgs e)
        {
            secondsLeft--;

            if (secondsLeft <= 0)
            {
                inactivityTimer.Stop();
                MessageBox.Show("Сессия завершена из-за неактивности!", "Блокировка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Открываем форму авторизации
                Autorization authForm = new Autorization();
                authForm.Show();

                // Закрываем текущую форму
                this.Close();
            }
        }

        // Чтение времени из файла настроек
        private int ReadTimeoutFromFile()
        {
            try
            {
                string filePath = "timeout.txt";
                if (File.Exists(filePath))
                {
                    string text = File.ReadAllText(filePath);
                    if (int.TryParse(text, out int seconds))
                    {
                        return seconds;
                    }
                }
            }
            catch (Exception)
            {
            }
            return 30; 
        }

        private void ResetInactivityTimer()
        {
            secondsLeft = ReadTimeoutFromFile();
        }


        private void Menu_St_MouseMove(object sender, MouseEventArgs e)
        {
            ResetInactivityTimer();
        }

        private void Menu_St_KeyPress(object sender, KeyPressEventArgs e)
        {
            ResetInactivityTimer();
        }


        private void button4_Click(object sender, EventArgs e)
        {
            ResetInactivityTimer();

            DialogResult res = MessageBox.Show($"Вы точно хотите выйти?", "Выход",
                MessageBoxButtons.YesNo, MessageBoxIcon.Error);

            if (res == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ResetInactivityTimer();
            Strahovateli form1 = new Strahovateli();
            form1.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ResetInactivityTimer();
            New_Strahovka form1 = new New_Strahovka(currentUserFIO);
            form1.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ResetInactivityTimer();
            int role = 2;
            Uchet_St_S form1 = new Uchet_St_S(role);
            form1.Show();
        }

        private void Menu_St_Load(object sender, EventArgs e)
        {
        }
    }
}