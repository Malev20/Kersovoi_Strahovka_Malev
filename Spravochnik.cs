using System;
using System.Windows.Forms;
namespace Kersovoi_Strahovka_Malev
{
    public partial class Spravochnik : Form
    {
        // Конструктор формы справочника
        public Spravochnik()
        {
            InitializeComponent();
        }

        // Обработчик кнопки закрытия формы
        private void button4_Click(object sender, EventArgs e)
        {
            this.Close(); // Закрытие текущей формы
        }

        // Обработчик кнопки открытия справочника ролей
        private void button1_Click(object sender, EventArgs e)
        {
            Sp_Role form1 = new Sp_Role(); // Создание экземпляра формы справочника ролей
            form1.Show(); // Отображение формы
        }

        // Обработчик кнопки открытия справочника материалов
        private void button2_Click(object sender, EventArgs e)
        {
            Sp_Materials form1 = new Sp_Materials(); // Создание экземпляра формы справочника материалов
            form1.Show(); // Отображение формы
        }

        // Обработчик кнопки открытия справочника тарифов
        private void button3_Click(object sender, EventArgs e)
        {
            Sp_Tarif form1 = new Sp_Tarif(); // Создание экземпляра формы справочника тарифов
            form1.Show(); // Отображение формы
        }

        // Обработчик кнопки открытия справочника местоположений
        private void button5_Click(object sender, EventArgs e)
        {
            Sp_Location form1 = new Sp_Location(); // Создание экземпляра формы справочника местоположений
            form1.Show(); // Отображение формы
        }
    }
}