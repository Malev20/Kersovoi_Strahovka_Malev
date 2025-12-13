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
    public partial class Menu_Dr : Form
    {
        // Конструктор формы, принимающий ФИО пользователя (директора)
        public Menu_Dr(string fullName)
        {
            InitializeComponent();
            label2.Text = fullName; // Отображаем ФИО директора на форме в label2
        }
        // Обработчик кнопки выхода из системы
        private void button4_Click(object sender, EventArgs e)
        {
            // Запрашиваем подтверждение выхода у пользователя
            DialogResult res = MessageBox.Show($"Вы точно хотите выйти?", "Выход", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            // Если пользователь подтвердил выход
            if (res == DialogResult.Yes)
            {
                this.Close(); // Закрываем текущую форму
            }
        }
        // Обработчик кнопки для перехода к учету страховок
        private void button3_Click(object sender, EventArgs e)
        {
            int role = 3; // Устанавливаем роль пользователя (3 - директор)
            Uchet_St_S form1 = new Uchet_St_S(role); // Создаем экземпляр формы учета страховок, передавая роль
            form1.Show(); // Открываем форму
        }
    }
}