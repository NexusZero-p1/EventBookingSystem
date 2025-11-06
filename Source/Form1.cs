using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace EventBookingSystem
{
    public partial class Form1 : Form
    {
        private string connString = @"Server=DESKTOP-BDO4K4M\SQLEXPRESS;Database=EventBookingSystem;Trusted_Connection=true;TrustServerCertificate=true";

        public Form1()
        {
            InitializeComponent();
            LoadEvents();
            LoadGenres();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BookTicket();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadEvents();
            MessageBox.Show("Данные обновлены!", "Обновление");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SearchEvents();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ResetFilters();
        }

        private void LoadEvents()
        {
            try
            {
                using (var conn = new SqlConnection(connString))
                {
                    var da = new SqlDataAdapter("SELECT EventID, EventName, EventDate, AvailableTickets, Price FROM Events WHERE AvailableTickets > 0", conn);
                    var dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        private void LoadGenres()
        {
            try
            {
                comboBox1.Items.Clear();
                comboBox1.Items.Add("Все жанры");

                using (var conn = new SqlConnection(connString))
                {
                    var da = new SqlDataAdapter("SELECT DISTINCT EventName FROM Events", conn);
                    var dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        string eventName = row["EventName"].ToString();
                        string genre = eventName.Split(' ')[0];
                        if (!comboBox1.Items.Contains(genre))
                        {
                            comboBox1.Items.Add(genre);
                        }
                    }
                }
                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки жанров: " + ex.Message);
            }
        }

        private void SearchEvents()
        {
            try
            {
                string dateFilter = textBox3.Text.Trim();
                string genreFilter = comboBox1.SelectedItem?.ToString();

                string sql = "SELECT EventID, EventName, EventDate, AvailableTickets, Price FROM Events WHERE AvailableTickets > 0";

                using (var conn = new SqlConnection(connString))
                {
                    var cmd = new SqlCommand();
                    cmd.Connection = conn;

                    // Фильтр по дате
                    if (!string.IsNullOrEmpty(dateFilter))
                    {
                        sql += " AND CAST(EventDate AS DATE) = @searchDate";
                        cmd.Parameters.AddWithValue("@searchDate", DateTime.Parse(dateFilter).Date);
                    }

                    // Фильтр по жанру
                    if (!string.IsNullOrEmpty(genreFilter) && genreFilter != "Все жанры")
                    {
                        sql += " AND EventName LIKE @genre + '%'";
                        cmd.Parameters.AddWithValue("@genre", genreFilter);
                    }

                    cmd.CommandText = sql;
                    var da = new SqlDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }

                MessageBox.Show("Поиск выполнен!", "Поиск");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска: " + ex.Message);
            }
        }

        private void ResetFilters()
        {
            textBox3.Text = "";
            comboBox1.SelectedIndex = 0;
            LoadEvents();
            MessageBox.Show("Фильтры сброшены!", "Сброс");
        }

        private void BookTicket()
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите событие из таблицы!");
                return;
            }

            try
            {
                var row = dataGridView1.SelectedRows[0];
                int eventId = (int)row.Cells["EventID"].Value;
                string eventName = row.Cells["EventName"].Value.ToString();
                int tickets = (int)numericUpDown1.Value;

                using (var conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // Проверка билетов
                    var checkCmd = new SqlCommand("SELECT AvailableTickets FROM Events WHERE EventID = @eid", conn);
                    checkCmd.Parameters.AddWithValue("@eid", eventId);
                    int available = (int)checkCmd.ExecuteScalar();

                    if (tickets > available)
                    {
                        MessageBox.Show($"Доступно только {available} билетов!");
                        return;
                    }

                    string name = textBox1.Text.Trim();
                    string email = textBox2.Text.Trim();

                    // Проверка заполнения полей
                    if (string.IsNullOrWhiteSpace(name) || name == "Имя")
                    {
                        MessageBox.Show("Введите имя!");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                    {
                        MessageBox.Show("Введите корректный email!");
                        return;
                    }

                    // Бронирование
                    var bookCmd = new SqlCommand(@"INSERT INTO Bookings (EventID, CustomerName, CustomerEmail, TicketsCount, TotalAmount) 
                                                  VALUES (@eid, @name, @email, @tickets, 
                                                  (SELECT Price FROM Events WHERE EventID = @eid) * @tickets)", conn);
                    bookCmd.Parameters.AddWithValue("@eid", eventId);
                    bookCmd.Parameters.AddWithValue("@name", name);
                    bookCmd.Parameters.AddWithValue("@email", email);
                    bookCmd.Parameters.AddWithValue("@tickets", tickets);
                    bookCmd.ExecuteNonQuery();

                    // Обновление билетов
                    var updateCmd = new SqlCommand("UPDATE Events SET AvailableTickets = AvailableTickets - @tickets WHERE EventID = @eid", conn);
                    updateCmd.Parameters.AddWithValue("@tickets", tickets);
                    updateCmd.Parameters.AddWithValue("@eid", eventId);
                    updateCmd.ExecuteNonQuery();
                }

                MessageBox.Show($"Успешно! {eventName}\nБилетов: {tickets}");
                LoadEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка бронирования: " + ex.Message);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
