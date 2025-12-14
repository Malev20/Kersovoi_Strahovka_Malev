using Mysqlx.Crud;
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
    public partial class Menu_SA : Form
    {
        // Таймер для отслеживания неактивности
        private System.Windows.Forms.Timer inactivityTimer;
        private int secondsLeft = 30;

        // Конструктор формы, принимающий ФИО пользователя
        public Menu_SA(string fullName)
        {
            InitializeComponent();
            // Отображение ФИО пользователя на форме
            label2.Text = fullName;

            // Инициализация таймера неактивности
            InitializeInactivityTimer();
        }

        // Инициализация таймера неактивности
        private void InitializeInactivityTimer()
        {
            // Чтение времени из файла настроек
            secondsLeft = ReadTimeoutFromFile();

            inactivityTimer = new System.Windows.Forms.Timer();
            inactivityTimer.Interval = 1000; // 1 секунда
            inactivityTimer.Tick += InactivityTimer_Tick;
            inactivityTimer.Start();
        }
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

                this.Close();
            }
        }

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
        private void button4_Click(object sender, EventArgs e)
        {
            ResetInactivityTimer(); // Сброс при нажатии кнопки

            DialogResult res = MessageBox.Show($"Вы точно хотите выйти?", "Выход",
                MessageBoxButtons.YesNo, MessageBoxIcon.Error);

            if (res == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ResetInactivityTimer(); // Сброс при нажатии кнопки
            Users form1 = new Users();
            form1.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ResetInactivityTimer(); // Сброс при нажатии кнопки
            Spravochnik form1 = new Spravochnik();
            form1.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ResetInactivityTimer(); // Сброс при нажатии кнопки
            int role = 1;
            Uchet_St_S form1 = new Uchet_St_S(role);
            form1.Show();
        }

        private void Menu_SA_MouseMove(object sender, MouseEventArgs e)
        {
            ResetInactivityTimer(); // Сброс при движении мыши
        }

        private void Menu_SA_KeyPress(object sender, KeyPressEventArgs e)
        {
            ResetInactivityTimer(); // Сброс при нажатии клавиши
        }
    }
}