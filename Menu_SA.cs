
using Mysqlx.Crud;
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
    public partial class Menu_SA : Form
    {
        // Конструктор формы, принимающий ФИО пользователя
        public Menu_SA(string fullName)
        {
            InitializeComponent();
            // Отображение ФИО пользователя на форме
            label2.Text = fullName;
        }
        // Обработчик нажатия кнопки выхода
        private void button4_Click(object sender, EventArgs e)
        {
            // Подтверждение выхода с пользователем
            DialogResult res = MessageBox.Show($"Вы точно хотите выйти?", "Выход", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

            // Закрытие формы при подтверждении
            if (res == DialogResult.Yes)
            {
                this.Close();
            }
        }
        // Обработчик нажатия кнопки управления пользователями
        private void button1_Click(object sender, EventArgs e)
        {
            // Открытие формы управления пользователями
            Users form1 = new Users();
            form1.Show();
        }
        // Обработчик нажатия кнопки справочников
        private void button2_Click(object sender, EventArgs e)
        {
            // Открытие формы справочников
            Spravochnik form1 = new Spravochnik();
            form1.Show();
        }
        // Обработчик нажатия кнопки учета страховок
        private void button3_Click(object sender, EventArgs e)
        {
            // Установка роли пользователя (1 - администратор)
            int role = 1;
            // Открытие формы учета страховок с передачей роли
            Uchet_St_S form1 = new Uchet_St_S(role);
            form1.Show();
        }
    }
}