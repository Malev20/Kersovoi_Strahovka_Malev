using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        // Конструктор формы, принимающий ФИО пользователя
        public Menu_St(string fullName)
        {
            InitializeComponent();
            currentUserFIO = fullName; // Сохраняем ФИО пользователя
            label2.Text = fullName; // Отображаем ФИО на форме
        }
        // Обработчик кнопки выхода
        private void button4_Click(object sender, EventArgs e)
        {
            // Запрашиваем подтверждение выхода
            DialogResult res = MessageBox.Show($"Вы точно хотите выйти?", "Выход", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

            // Если пользователь подтвердил выход
            if (res == DialogResult.Yes)
            {
                this.Close(); // Закрываем форму
            }
        }
        // Обработчик кнопки "Страхователи" (открывает форму со списком страхователей)
        private void button1_Click(object sender, EventArgs e)
        {
            Strahovateli form1 = new Strahovateli(); // Создаем экземпляр формы
            form1.Show(); // Открываем форму
        }
        // Обработчик кнопки "Новая страховка" (открывает форму создания новой страховки)
        private void button2_Click(object sender, EventArgs e)
        {
            // Передаем ФИО пользователя в форму создания страховки
            New_Strahovka form1 = new New_Strahovka(currentUserFIO);
            form1.Show(); // Открываем форму
        }
        // Обработчик кнопки "Учет страховок" (открывает форму учета страховок)
        private void button3_Click(object sender, EventArgs e)
        {
            int role = 2; // Устанавливаем роль пользователя (2 - страховщик)
            Uchet_St_S form1 = new Uchet_St_S(role); // Создаем форму с передачей роли
            form1.Show(); // Открываем форму
        }
        // Обработчик события загрузки формы
        private void Menu_St_Load(object sender, EventArgs e)
        {
            // Метод может быть использован для дополнительной настройки формы при загрузке
        }
    }
}