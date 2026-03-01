using Microsoft.Win32;
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

namespace kzprs
{
    public partial class Form1 : Form
    {
        string filePath = AppContext.BaseDirectory + "\\savedData.txt";

        public Form1()
        {
            InitializeComponent();

            dataGridView1.RowsRemoved += (s, e) => UpdateTotals();

            this.FormClosing += Form1_FormClosing;
            this.Load += Form1_Load;
        }
        private void InitializeDataGridView()
        {
            dataGridView1.ColumnCount = 5;
            dataGridView1.Columns[0].Name = "Date";
            dataGridView1.Columns[1].Name = "Expense";
            dataGridView1.Columns[2].Name = "Cost";
            dataGridView1.Columns[3].Name = "Description";
            dataGridView1.Columns[4].Name = "Category";
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            string expense = txtName.Text.Trim();
            string sumText = txtAmount.Text.Trim();
            string description = txtDescription.Text.Trim();
            string category = "";
            bool valid = true;
            bool sumValid = true;
            bool categoryChosen = true;

            if (radioFood.Checked) category = "Food";
            else if (radioTransport.Checked) category = "Transport";
            else if (radioFun.Checked) category = "Entertainment";
            else categoryChosen = false;

            if (string.IsNullOrEmpty(expense) || string.IsNullOrEmpty(sumText) || string.IsNullOrEmpty(description))
            {
                valid = false;
            }

            float sum;
            if (!float.TryParse(sumText.Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out sum))
            {
                sumValid = false;
            }

            if (sum < 0)
            {
                sumValid = false;
            }

            if (!valid && !sumValid && !categoryChosen)
            {
                MessageBox.Show("Check the correctness of the entered data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!valid && (!sumValid || !categoryChosen))
            {
                MessageBox.Show("Check the correctness of the entered data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!valid)
            {
                MessageBox.Show("Fill in all fields", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!sumValid)
            {
                MessageBox.Show("Enter a valid value", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!categoryChosen)
            {
                MessageBox.Show("Select a category", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dataGridView1.Rows.Add(dateTimePicker1.Value.ToShortDateString(), expense, sum.ToString("0.00"), description, category);

            txtName.Clear();
            txtAmount.Clear();
            txtDescription.Clear();
            radioFood.Checked = radioTransport.Checked = radioFun.Checked = false;

            UpdateTotals();
        }
        private void UpdateTotals()
        {
            float total = 0;
            float foodTotal = 0;
            float transportTotal = 0;
            float entertainmentTotal = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                float sum;
                if (float.TryParse(row.Cells["Cost"].Value.ToString(), out sum))
                {
                    string category = row.Cells["Category"].Value.ToString();
                    total += sum;

                    switch (category)
                    {
                        case "Food":
                            foodTotal += sum;
                            break;
                        case "Transport":
                            transportTotal += sum;
                            break;
                        case "Entertainment":
                            entertainmentTotal += sum;
                            break;
                    }
                }
            }

            lblTotal.Text = total > 0 ? total.ToString("0.00") : "*";
            lblFood.Text = foodTotal > 0 ? foodTotal.ToString("0.00") : "*";
            lblTransport.Text = transportTotal > 0 ? transportTotal.ToString("0.00") : "*";
            lblFun.Text = entertainmentTotal > 0 ? entertainmentTotal.ToString("0.00") : "*";
        }
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                if (dataGridView1.SelectedCells.Count > 0)
                {
                    MessageBox.Show("Select a row to delete by clicking the empty box on the left", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Select a row to delete first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }

            var confirm = MessageBox.Show("Delete the selected row?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    if (!row.IsNewRow)
                        dataGridView1.Rows.Remove(row);
                }

                UpdateTotals();
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(filePath))
            {
                InitializeDataGridView();
                return;
            }

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            InitializeDataGridView();

            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split('|');

                if (parts[0] == "TOTAL") lblTotal.Text = parts.Length > 1 ? parts[1] : "*";

                else if (parts[0] == "FOOD") lblFood.Text = parts.Length > 1 ? parts[1] : "*";

                else if (parts[0] == "TRANSPORT") lblTransport.Text = parts.Length > 1 ? parts[1] : "*";

                else if (parts[0] == "FUN") lblFun.Text = parts.Length > 1 ? parts[1] : "*";

                else if (parts.Length == dataGridView1.Columns.Count) dataGridView1.Rows.Add(parts);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<string> lines = new List<string>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;
                string[] cells = new string[row.Cells.Count];
                for (int i = 0; i < row.Cells.Count; i++)
                    cells[i] = row.Cells[i].Value?.ToString() ?? "";
                lines.Add(string.Join("|", cells));
            }

            lines.Add($"TOTAL|{lblTotal.Text}");
            lines.Add($"FOOD|{lblFood.Text}");
            lines.Add($"TRANSPORT|{lblTransport.Text}");
            lines.Add($"FUN|{lblFun.Text}");

            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
