using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kersovoi_Strahovka_Malev
{
    public partial class Captha : Form
    {
        private string captchaText;
        private Random random = new Random();

        public string CaptchaResult { get; private set; }
        public bool IsCaptchaCorrect { get; private set; }

        public Captha()
        {
            InitializeComponent();
            GenerateCaptcha();
        }

        private void GenerateCaptcha()
        {
            // Генерация случайного текста для капчи (4-6 символов)
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            captchaText = "";
            for (int i = 0; i < random.Next(4, 7); i++)
            {
                captchaText += chars[random.Next(chars.Length)];
            }

            // Создание изображения капчи
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                // Заливка фона
                g.Clear(Color.White);

                // Добавление шума (точки)
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(bmp.Width);
                    int y = random.Next(bmp.Height);
                    bmp.SetPixel(x, y, Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
                }

                // Добавление линий шума
                for (int i = 0; i < 10; i++)
                {
                    g.DrawLine(
                        new Pen(Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)), 1),
                        new Point(random.Next(bmp.Width), random.Next(bmp.Height)),
                        new Point(random.Next(bmp.Width), random.Next(bmp.Height))
                    );
                }

                // Настройка шрифта
                Font font = new Font("Arial", 20, FontStyle.Bold | FontStyle.Italic);

                // Добавление искажения текста
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddString(
                        captchaText,
                        font.FontFamily,
                        (int)font.Style,
                        font.Size,
                        new Point(10, 10),
                        new StringFormat()
                    );

                    // Искажение пути текста
                    Matrix matrix = new Matrix();
                    matrix.Shear((float)random.NextDouble() * 0.3f - 0.15f, 0);
                    path.Transform(matrix);

                    // Рисование текста
                    g.FillPath(Brushes.DarkBlue, path);
                    g.DrawPath(Pens.Black, path);
                }
            }

            pictureBox1.Image = bmp;
            CaptchaResult = captchaText;
            textBox1.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Кнопка "Обновить капчу"
            GenerateCaptcha();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Кнопка "Проверить"
            if (textBox1.Text.ToUpper() == captchaText.ToUpper())
            {
                IsCaptchaCorrect = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверно введена капча! Попробуйте снова.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox1.Clear();
                textBox1.Focus();
                GenerateCaptcha();
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только буквы и цифры
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void Captha_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Кнопка "Отмена"
            IsCaptchaCorrect = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}