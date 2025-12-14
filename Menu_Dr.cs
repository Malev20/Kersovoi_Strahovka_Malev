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
    public partial class Menu_Dr : Form
    {
        // Таймер для отслеживания неактивности
        private System.Windows.Forms.Timer inactivityTimer;
        private int secondsLeft = 30;

        // Конструктор формы, принимающий ФИО пользователя (директора)
        public Menu_Dr(string fullName)
        {
            InitializeComponent();
            label2.Text = fullName; // Отображаем ФИО директора на форме в label2

            // Инициализация таймера неактивности
            InitializeInactivityTimer();
        }

        private void InitializeInactivityTimer()
        {
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
            return 30; // Значение по умолчанию 30 секунд
        }

        // Сброс таймера при активности пользователя
        private void ResetInactivityTimer()
        {
            secondsLeft = ReadTimeoutFromFile();
        }

        // Обработчики активности

        private void Menu_Dr_MouseMove(object sender, MouseEventArgs e)
        {
            ResetInactivityTimer();
        }

        private void Menu_Dr_KeyPress(object sender, KeyPressEventArgs e)
        {
            ResetInactivityTimer();
        }

        // Обработчик кнопки выхода из системы
        private void button4_Click(object sender, EventArgs e)
        {
            ResetInactivityTimer();

            // Запрашиваем подтверждение выхода у пользователя
            DialogResult res = MessageBox.Show($"Вы точно хотите выйти?", "Выход",
                MessageBoxButtons.YesNo, MessageBoxIcon.Error);

            // Если пользователь подтвердил выход
            if (res == DialogResult.Yes)
            {
                this.Close(); // Закрываем текущую форму
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ResetInactivityTimer();
            int role = 3; // Устанавливаем роль пользователя (3 - директор)
            Uchet_St_S form1 = new Uchet_St_S(role); // Создаем экземпляр формы учета страховок, передавая роль
            form1.Show(); // Открываем форму
        }
    }
}