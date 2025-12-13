using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace Kersovoi_Strahovka_Malev
{
    public partial class Prosmotr_ST : Form
    {
        private string userFIO;

        public Prosmotr_ST(string fio)
        {
            InitializeComponent();
            userFIO = fio;
            textBox5.Text = userFIO;
        }

        public void SetData(string policyNumber, string startDate, string endDate, string client,
                          string phone, string address, string amount, string area,
                          string year, string location, string material, string cases,
                          string payment, string status, string rate, Image photo)
        {
            textBox1.Text = policyNumber;
            textBox2.Text = client;
            textBox3.Text = phone;
            textBox4.Text = address;
            textBox6.Text = startDate;
            textBox7.Text = endDate;
            textBox8.Text = amount;
            textBox9.Text = area;
            textBox10.Text = year;
            textBox11.Text = location;
            textBox12.Text = material;
            textBox13.Text = cases;
            textBox14.Text = payment;
            textBox15.Text = status;
            textBox16.Text = rate;
            pictureBox1.Image = photo ?? CreateNoImage();
        }

        private Image CreateNoImage()
        {
            Bitmap bmp = new Bitmap(200, 150);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);
                g.DrawString("Нет фото", new Font("Arial", 10), Brushes.Black, 50, 60);
            }
            return bmp;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                SaveToDB();
                MessageBox.Show("Данные сохранены!", "Успех");
                RefreshParentForm();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void RefreshParentForm()
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is Uchet_St_S)
                {
                    ((Uchet_St_S)form).RefreshData();
                    break;
                }
            }
        }

        private void SaveToDB()
        {
            string connStr = @"host=localhost;uid=root;pwd=root;database=kursach;";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                int clientId = GetClientId(conn);
                int userId = GetUserId(conn);
                int materialId = GetMaterialId(conn);
                int locationId = GetLocationId(conn);
                int statusId = GetStatusId(conn);
                int rateId = GetRateId(conn);
                string photoName = SavePhoto();

                string sql = @"INSERT INTO Insurance 
                    (Policy_Number, Start_Data, End_Data, ID_Client, Phone_Client, 
                     ID_Users, Insurance_Amount, Amount_Payment_Policy, ID_Status,
                     ID_Materials, Square, ID_Location, Year_Build, Address_Building, Photo, ID_Rate) 
                    VALUES (@num, @start, @end, @client, @phone, @user, @amount, 
                            @payment, @status, @material, @area, @location, @year, @address, @photo, @rate)";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@num", textBox1.Text);
                    cmd.Parameters.AddWithValue("@start", DateTime.Parse(textBox6.Text));
                    cmd.Parameters.AddWithValue("@end", DateTime.Parse(textBox7.Text));
                    cmd.Parameters.AddWithValue("@client", clientId);
                    cmd.Parameters.AddWithValue("@phone", textBox3.Text);
                    cmd.Parameters.AddWithValue("@user", userId);
                    cmd.Parameters.AddWithValue("@amount", decimal.Parse(textBox8.Text));
                    cmd.Parameters.AddWithValue("@payment", decimal.Parse(textBox14.Text));
                    cmd.Parameters.AddWithValue("@status", statusId);
                    cmd.Parameters.AddWithValue("@material", materialId);
                    cmd.Parameters.AddWithValue("@area", decimal.Parse(textBox9.Text));
                    cmd.Parameters.AddWithValue("@location", locationId);
                    cmd.Parameters.AddWithValue("@year", int.Parse(textBox10.Text));
                    cmd.Parameters.AddWithValue("@address", textBox4.Text);
                    cmd.Parameters.AddWithValue("@photo", photoName);
                    cmd.Parameters.AddWithValue("@rate", rateId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private int GetClientId(MySqlConnection conn)
        {
            string sql = "SELECT ID_Client FROM Client WHERE FIO_Client = @fio";
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@fio", textBox2.Text);
                object result = cmd.ExecuteScalar();
                if (result != null) return Convert.ToInt32(result);
            }

            string insertSql = "INSERT INTO Client (FIO_Client, Phone_Client) VALUES (@fio, @phone); SELECT LAST_INSERT_ID();";
            using (MySqlCommand cmd = new MySqlCommand(insertSql, conn))
            {
                cmd.Parameters.AddWithValue("@fio", textBox2.Text);
                cmd.Parameters.AddWithValue("@phone", textBox3.Text);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int GetUserId(MySqlConnection conn)
        {
            string sql = "SELECT ID_Users FROM Users WHERE FIO_Users = @fio";
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@fio", userFIO);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int GetMaterialId(MySqlConnection conn)
        {
            string sql = "SELECT ID_Materials FROM Building_materials WHERE Name_Materials = @name";
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@name", textBox12.Text);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int GetLocationId(MySqlConnection conn)
        {
            string sql = "SELECT ID_Location FROM Location WHERE Name_Location = @name";
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@name", textBox11.Text);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int GetStatusId(MySqlConnection conn)
        {
            string sql = "SELECT ID_Status FROM Status WHERE Name_Status = @name";
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@name", textBox15.Text);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int GetRateId(MySqlConnection conn)
        {
            string sql = "SELECT ID_Rate FROM Basic_Rate WHERE Coefficient = @coef";
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@coef", textBox16.Text);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private string SavePhoto()
        {
            if (pictureBox1.Image == null || pictureBox1.Image == CreateNoImage())
                return "empty_photocamera.png";

            try
            {
                string folder = Path.Combine(Application.StartupPath, "512x512");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = $"photo_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                string fullPath = Path.Combine(folder, fileName);

                pictureBox1.Image.Save(fullPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                return fileName;
            }
            catch
            {
                return "empty_photocamera.png";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Введите номер полиса!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                GenerateWordDocumentSimple();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании документа: {ex.Message}",
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        // ПРОСТОЙ И СТАБИЛЬНЫЙ МЕТОД БЕЗ ПРОБЛЕМ С COM
        private void GenerateWordDocumentSimple()
        {
            // 1. Используем Open XML SDK как альтернативу
            GenerateUsingTemplateCopy();

            // ИЛИ вариант 2: используем старый добрый подход с Bookmark
            // GenerateUsingBookmarks();
        }

        private void GenerateUsingTemplateCopy()
        {
            try
            {
                string templatePath = Path.Combine(Application.StartupPath, "Страховка.docx");

                if (!File.Exists(templatePath))
                {
                    MessageBox.Show($"Файл шаблона не найден!\n\nПоложите файл 'Страховка.docx' в папку:\n{Application.StartupPath}",
                                  "Шаблон не найден",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                    return;
                }

                // Создаем копию шаблона
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string outputFileName = $"Полис_{textBox1.Text}_{DateTime.Now:yyyyMMddHHmmss}.docx";
                string outputPath = Path.Combine(desktopPath, outputFileName);

                // Копируем шаблон
                File.Copy(templatePath, outputPath, true);

                // Используем простой подход через Word Automation
                GenerateWithSimpleWordAutomation(outputPath);

                // Открываем готовый документ
                System.Diagnostics.Process.Start(outputPath);

                MessageBox.Show($"Документ успешно сформирован и сохранён на Рабочий стол:\n{outputFileName}",
                              "Успех",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateWithSimpleWordAutomation(string filePath)
        {
            Word.Application wordApp = null;
            Word.Document doc = null;

            try
            {
                // Создаем Word
                wordApp = new Word.Application();
                wordApp.Visible = false; // Не показываем Word

                // Открываем документ
                object fileName = filePath;
                object readOnly = false;
                object isVisible = false;
                object missing = Type.Missing;

                doc = wordApp.Documents.Open(ref fileName, ref missing, ref readOnly,
                                           ref missing, ref missing, ref missing,
                                           ref missing, ref missing, ref missing,
                                           ref missing, ref missing, ref isVisible,
                                           ref missing, ref missing, ref missing, ref missing);

                // Простой способ замены через закладки (если они есть в шаблоне)
                ReplaceUsingBookmarks(doc);

                // ИЛИ через поиск и замену текста
                ReplaceUsingFindAndReplace(doc);

                // Сохраняем
                doc.Save();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка Word Automation: {ex.Message}");
            }
            finally
            {
                // Корректно закрываем Word
                if (doc != null)
                {
                    try
                    {
                        object saveChanges = Word.WdSaveOptions.wdSaveChanges;
                        object originalFormat = Word.WdOriginalFormat.wdOriginalDocumentFormat;
                        object routeDocument = false;

                        doc.Close(ref saveChanges, ref originalFormat, ref routeDocument);
                    }
                    catch { }
                }

                if (wordApp != null)
                {
                    try
                    {
                        wordApp.Quit();
                    }
                    catch { }
                }

                // Принудительная сборка мусора для COM-объектов
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        private void ReplaceUsingBookmarks(Word.Document doc)
        {
            try
            {
                // Если в шаблоне используются закладки с такими именами
                ReplaceBookmark(doc, "RATE", textBox16.Text);
                ReplaceBookmark(doc, "CASES", textBox13.Text);
                ReplaceBookmark(doc, "PAYMENT", textBox14.Text);
                ReplaceBookmark(doc, "STATUS", textBox15.Text);
                ReplaceBookmark(doc, "USER_FIO", userFIO);
                ReplaceBookmark(doc, "POLICY_NUMBER", textBox1.Text);
                ReplaceBookmark(doc, "FIO_C", textBox2.Text);
                ReplaceBookmark(doc, "PHONE", textBox3.Text);
                ReplaceBookmark(doc, "ADDRESS", textBox4.Text);
                ReplaceBookmark(doc, "DATE_START", textBox6.Text);
                ReplaceBookmark(doc, "DATE_END", textBox7.Text);
                ReplaceBookmark(doc, "AMOUNT", textBox8.Text);
                ReplaceBookmark(doc, "AREA", textBox9.Text);
                ReplaceBookmark(doc, "YEAR_BUILD", textBox10.Text);
                ReplaceBookmark(doc, "LOCATION", textBox11.Text);
                ReplaceBookmark(doc, "MATERIAL", textBox12.Text);
            }
            catch
            {
                // Если закладок нет, используем другой метод
            }
        }

        private void ReplaceBookmark(Word.Document doc, string bookmarkName, string text)
        {
            try
            {
                if (doc.Bookmarks.Exists(bookmarkName))
                {
                    Word.Bookmark bookmark = doc.Bookmarks[bookmarkName];
                    bookmark.Range.Text = text;
                    doc.Bookmarks.Add(bookmarkName, bookmark.Range);
                }
            }
            catch { }
        }

        private void ReplaceUsingFindAndReplace(Word.Document doc)
        {
            try
            {
                // Более безопасный метод замены
                object missing = Type.Missing;
                object replaceAll = Word.WdReplace.wdReplaceAll;

                Word.Find find = doc.Content.Find;
                find.ClearFormatting();
                find.Replacement.ClearFormatting();

                // Заменяем все плейсхолдеры
                ReplaceTextSafe(doc, "{RATE}", textBox16.Text);
                ReplaceTextSafe(doc, "{CASES}", textBox13.Text);
                ReplaceTextSafe(doc, "{PAYMENT}", textBox14.Text);
                ReplaceTextSafe(doc, "{STATUS}", textBox15.Text);
                ReplaceTextSafe(doc, "{USER_FIO}", userFIO);
                ReplaceTextSafe(doc, "{POLICY_NUMBER}", textBox1.Text);
                ReplaceTextSafe(doc, "{FIO_C}", textBox2.Text);
                ReplaceTextSafe(doc, "{PHONE}", textBox3.Text);
                ReplaceTextSafe(doc, "{ADDRESS}", textBox4.Text);
                ReplaceTextSafe(doc, "{DATE_START}", textBox6.Text);
                ReplaceTextSafe(doc, "{DATE_END}", textBox7.Text);
                ReplaceTextSafe(doc, "{AMOUNT}", textBox8.Text);
                ReplaceTextSafe(doc, "{AREA}", textBox9.Text);
                ReplaceTextSafe(doc, "{YEAR_BUILD}", textBox10.Text);
                ReplaceTextSafe(doc, "{LOCATION}", textBox11.Text);
                ReplaceTextSafe(doc, "{MATERIAL}", textBox12.Text);
            }
            catch { }
        }

        private void ReplaceTextSafe(Word.Document doc, string findText, string replaceText)
        {
            try
            {
                object missing = Type.Missing;
                object replaceAll = Word.WdReplace.wdReplaceAll;

                doc.Content.Find.Execute(
                    FindText: findText,
                    MatchCase: false,
                    MatchWholeWord: false,
                    MatchWildcards: false,
                    MatchSoundsLike: missing,
                    MatchAllWordForms: false,
                    Forward: true,
                    Wrap: Word.WdFindWrap.wdFindContinue,
                    Format: false,
                    ReplaceWith: replaceText,
                    Replace: replaceAll
                );
            }
            catch { }
        }

        // АЛЬТЕРНАТИВНЫЙ ВАРИАНТ: Генерация через XML/OpenXML (без Word)
        private void GenerateUsingXml()
        {
            try
            {
                // Читаем шаблон как текст
                string templatePath = Path.Combine(Application.StartupPath, "Страховка.txt"); // Используем TXT файл

                if (!File.Exists(templatePath))
                {
                    // Создаем простой текстовый шаблон
                    CreateTextTemplate(templatePath);
                }

                string template = File.ReadAllText(templatePath);

                // Заменяем плейсхолдеры
                template = template.Replace("{RATE}", textBox16.Text);
                template = template.Replace("{CASES}", textBox13.Text);
                template = template.Replace("{PAYMENT}", textBox14.Text);
                template = template.Replace("{STATUS}", textBox15.Text);
                template = template.Replace("{USER_FIO}", userFIO);
                template = template.Replace("{POLICY_NUMBER}", textBox1.Text);
                template = template.Replace("{FIO_C}", textBox2.Text);
                template = template.Replace("{PHONE}", textBox3.Text);
                template = template.Replace("{ADDRESS}", textBox4.Text);
                template = template.Replace("{DATE_START}", textBox6.Text);
                template = template.Replace("{DATE_END}", textBox7.Text);
                template = template.Replace("{AMOUNT}", textBox8.Text);
                template = template.Replace("{AREA}", textBox9.Text);
                template = template.Replace("{YEAR_BUILD}", textBox10.Text);
                template = template.Replace("{LOCATION}", textBox11.Text);
                template = template.Replace("{MATERIAL}", textBox12.Text);

                // Сохраняем как текстовый файл
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string outputFileName = $"Полис_{textBox1.Text}_{DateTime.Now:yyyyMMddHHmmss}.txt";
                string outputPath = Path.Combine(desktopPath, outputFileName);

                File.WriteAllText(outputPath, template);

                // Открываем блокнотом
                System.Diagnostics.Process.Start(outputPath);

                MessageBox.Show($"Текстовый документ сформирован:\n{outputFileName}",
                              "Успех",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateTextTemplate(string path)
        {
            string template = @"
СТРАХОВОЙ ПОЛИС № {POLICY_NUMBER}

Данные страхователя:
ФИО: {FIO_C}
Телефон: {PHONE}
Адрес объекта: {ADDRESS}

Данные полиса:
Дата начала: {DATE_START}
Дата окончания: {DATE_END}
Страховая сумма: {AMOUNT} руб.
Площадь: {AREA} кв.м.

Характеристики объекта:
Год постройки: {YEAR_BUILD}
Местоположение: {LOCATION}
Материал стен: {MATERIAL}

Дополнительные данные:
Коэффициент тарифа: {RATE}
Страховые случаи: {CASES}
Сумма выплаты: {PAYMENT} руб.
Статус: {STATUS}

Оформлено сотрудником: {USER_FIO}
Дата формирования: " + DateTime.Now.ToString("dd.MM.yyyy") + @"

";

            File.WriteAllText(path, template);
        }
    }
}