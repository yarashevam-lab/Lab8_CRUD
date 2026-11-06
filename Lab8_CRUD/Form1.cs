using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace Lab8_CRUD
{
    public partial class Form1 : Form
    {
        private readonly string connectionString = @"Data Source=Madina\SQLEXPRESS;Initial Catalog=StudentDB;Integrated Security=True;";
        private DataTable studentsTable = new DataTable();
        private int selectedId = -1;

        public Form1()
        {
            InitializeComponent();

            this.Load += Form1_Load;
            dataGridViewStudents.SelectionChanged += dataGridViewStudents_SelectionChanged;
            buttonAdd.Click += buttonAdd_Click;
            buttonUpdate.Click += buttonUpdate_Click;
            buttonDelete.Click += buttonDelete_Click;
            buttonClear.Click += buttonClear_Click;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private void LoadStudents()
        {
            string connectionString = @"Data Source=Madina\SQLEXPRESS;Initial Catalog=StudentDB;Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT Id, FullName, Age, GroupName FROM Students";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);

                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    dataGridViewStudents.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private void dataGridViewStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewStudents.CurrentRow == null) return;

            var row = dataGridViewStudents.CurrentRow;

            if (row.Cells["FullName"].Value != null)
                textBoxName.Text = row.Cells["FullName"].Value.ToString();

            if (row.Cells["Age"].Value != null)
                numericAge.Value = Convert.ToDecimal(row.Cells["Age"].Value);

            if (row.Cells["GroupName"].Value != null)
                textBoxGroup.Text = row.Cells["GroupName"].Value.ToString();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string inputName = textBoxName.Text.Trim();

                    string checkSql = "SELECT COUNT(*) FROM Students WHERE LTRIM(RTRIM(FullName)) = @name";
                    SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                    checkCmd.Parameters.AddWithValue("@name", inputName);

                    int count = (int)checkCmd.ExecuteScalar();

                    MessageBox.Show($"Ищем: '{inputName}'.\nНайдено в базе: {count}");

                    if (count > 0)
                    {
                        MessageBox.Show("Студент с таким ФИО уже существует в базе!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return; 
                    }
                    string sql = @"INSERT INTO Students (FullName, Age, GroupName, AverageGrade)
                           VALUES (@name, @age, @group, @grade)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", inputName); 
                        cmd.Parameters.AddWithValue("@age", (int)numericAge.Value);
                        cmd.Parameters.AddWithValue("@group", textBoxGroup.Text.Trim());
                        cmd.Parameters.AddWithValue("@grade", numericGrade.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                LoadStudents();
                ClearFields();
                UpdateStatus("Студент добавлен."); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridViewStudents.CurrentRow == null)
            {
                MessageBox.Show("Выберите студента для редактирования.",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidateInput()) return;

            int id = Convert.ToInt32(dataGridViewStudents.CurrentRow.Cells["Id"].Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"UPDATE Students
                                   SET FullName = @name, Age = @age
                                   WHERE Id = @id";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", textBoxName.Text.Trim());
                        cmd.Parameters.AddWithValue("@age", (int)numericAge.Value);
                        cmd.Parameters.AddWithValue("@group", textBoxGroup.Text.Trim());
                        cmd.Parameters.AddWithValue("@grade", numericGrade.Value);
                        cmd.Parameters.AddWithValue("@id", id);

                        cmd.ExecuteNonQuery();
                    }
                }
                LoadStudents();
                labelStatus.Text = "Данные студента обновлены.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка обновления: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewStudents.CurrentRow == null)
            {
                MessageBox.Show("Выберите студента для удаления.",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dataGridViewStudents.CurrentRow.Cells["Id"].Value);
            string name = dataGridViewStudents.CurrentRow.Cells["FullName"].Value?.ToString();

            var result = MessageBox.Show(
                $"Удалить студента \"{name}\"?", "Подтверждение удаления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "DELETE FROM Students WHERE Id = @id";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadStudents();
                ClearFields();
                labelStatus.Text = "Студент удалён.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            textBoxName.Clear();
            textBoxGroup.Clear();
            numericAge.Value = numericAge.Minimum;
            numericGrade.Value = 0;
            textBoxName.Focus();
            labelStatus.Text = "Поля очищены.";
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                MessageBox.Show("Поле «ФИО» не может быть пустым.",
                    "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxName.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(textBoxGroup.Text))
            {
                MessageBox.Show("Поле «Группа» не может быть пустым.",
                    "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxGroup.Focus();
                return false;
            }
            return true;
        }
        // Экспорт в CSV для лабораторной 9
        private void buttonExport_Click(object sender, EventArgs e)
        {
            
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV-файл (*.csv)|*.csv";
            sfd.FileName = "Students.csv";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DataTable exportTable = new DataTable();
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string sql = "SELECT Id, FullName, Age, GroupName, AverageGrade FROM Students";
                        SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                        adapter.Fill(exportTable);
                    }

                    if (exportTable.Rows.Count == 0)
                    {
                        MessageBox.Show("В базе данных вообще нет записей.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, new UTF8Encoding(true)))
                    {
                        sw.WriteLine("Id;FullName;Age;GroupName;AverageGrade");

                        foreach (DataRow row in exportTable.Rows)
                        {
                            sw.WriteLine($"{row["Id"]};{row["FullName"]};{row["Age"]};{row["GroupName"]};{row["AverageGrade"]}");
                        }
                    }
                    MessageBox.Show("Данные успешно экспортированы!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка экспорта: " + ex.Message);
                }
            }
        }
        
        private void UpdateStatus(string message)
        {
            labelStatus.Text = $"{message} | Всего студентов: {studentsTable.Rows.Count}";
        }
        
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            
            ClearFields(); 
            UpdateStatus("Действие отменено, данные перезагружены.");
        }
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            string searchName = textBoxName.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchName))
            {
                MessageBox.Show("Введите ФИО для поиска.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"SELECT Id, FullName, Age, GroupName 
                           FROM Students 
                           WHERE FullName LIKE @name";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@name", "%" + searchName + "%");

                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    dataGridViewStudents.DataSource = table;
                    labelStatus.Text = "Найдено: " + table.Rows.Count;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка поиска: " + ex.Message);
                }
            }
        }
    }
}
