using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsvHelper;
using System.Collections;
using System.IO;
using System.Globalization;
using CsvHelper.Configuration;
using CsvToDBParser.Models;
using System.Text.RegularExpressions;

namespace CsvToDBParser
{
    public partial class Form1 : Form
    {
        // File storage
        private Dictionary<string, ICollection> filesLoaded = new Dictionary<string, ICollection>();
        public Form1()
        {
            InitializeComponent();

            openFileDialog1.Filter = "CSV files(*.csv)|*.csv|Text files(*.txt)|*.txt";
            comboBox2.Items.Add("Процессы банка");
            numericUpDown1.Maximum = numericUpDown2.Value;
            numericUpDown2.Minimum = numericUpDown1.Value;
            button1.Enabled = false;
            button2.Enabled = false;
        }

        private void cSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel) return;
            string fileName = openFileDialog1.FileName;
            string templ = comboBox2?.SelectedItem?.ToString();

            // CsvHelper configs
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ";",
            };

            // The logic depends on the file template
            switch (templ)
            {
                case "Процессы банка":
                    filesLoaded.Add(templ, myCsvReader<BankProcCSV>(fileName, Encoding.GetEncoding(1251), config));
                    label6.Text = "Файл загружен";
                    break;
                default:
                    label6.Text = "Укажите шаблон";
                    break;
            }
            if (templ != null)
            {
                comboBox1.Items.Add(templ);
                comboBox1.SelectedItem = comboBox1.Items[comboBox1.Items.IndexOf(templ)];
                dataGridView1.DataSource = filesLoaded[templ];
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }

        // CsvReader(from CsvHelper) accepts file template
        private ICollection myCsvReader<T1>(string fileName, Encoding encoding, CsvConfiguration config)
        {
            var records = new List<T1>();

            using (StreamReader streamReader = new StreamReader(fileName, encoding))
            {
                using (CsvReader csvReader = new CsvReader(streamReader, config))
                {
                    while (csvReader.Read())
                    {
                        records.Add(csvReader.GetRecord<T1>());
                    }
                }
            }

            return records;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveToDBAsync();
        }

        // The logic and models depends on the file template
        private async void saveToDBAsync()
        {
            string templ = comboBox2?.SelectedItem?.ToString();
            switch (templ)
            {
                case "Процессы банка":
                    await Task.Run(() =>
                    {
                        List<string> bankDepartments = new List<string>();

                        // patterns for process code
                        string patternTheme = @"^\w?[0-9]+$"; // 2 hierarchy level
                        string patternAction = @"^\w?[0-9]+\.[0-9]+$"; // 3 hierarchy level
                        string patternProcess = @"^\w?[0-9]+\.[0-9]+\.[0-9]+$"; // 4 hierarchy level
                        using (BankProcessContext bankProcessDB = new BankProcessContext())
                        {
                            // actual models object
                            // 1 hierarchy level
                            BankProcType bankProcType = null;
                            // 2 hierarchy level
                            BankTheme bankTheme = null;
                            // 3 hierarchy level
                            BankAction bankAction = null;
                            // 4 hierarchy level
                            BankProcess bankProcess = null;
                            // catalog
                            BankDepartment bankDepartment = null;
                            // file processing
                            foreach (BankProcCSV bankProc in filesLoaded[templ])
                            {
                                if (bankProc.Department == "")
                                {
                                    if (bankProc.Id == ""
                                        && bankProcessDB.BankProcTypes.FirstOrDefault(p => p.Name == bankProc.Name)==null)
                                    {
                                        // Row to BankProcTypes (1 lvl)
                                        bankProcType = new BankProcType { Name = bankProc.Name };
                                        bankProcessDB.BankProcTypes.Add(bankProcType);
                                    }
                                    else
                                    {
                                        if (Regex.IsMatch(bankProc.Id, patternTheme)
                                            && bankProcessDB.BankThemes.FirstOrDefault(p => p.Code == bankProc.Id) == null)
                                        {
                                            // Row to BankThemes (2 lvl)
                                            bankTheme = new BankTheme { Name = bankProc.Name, Code = bankProc.Id, BankProcType = bankProcType };
                                            bankProcessDB.BankThemes.Add(bankTheme);
                                        }

                                        if (Regex.IsMatch(bankProc.Id, patternAction)
                                            && bankProcessDB.BankActions.FirstOrDefault(p => p.Code == bankProc.Id) == null)
                                        {
                                            // Row to BankActions (3 lvl)
                                            bankAction = new BankAction { Name = bankProc.Name, Code = bankProc.Id, BankTheme = bankTheme };
                                            bankProcessDB.BankActions.Add(bankAction);
                                        }
                                    }
                                }
                                else
                                {
                                    if (bankProc.Id != "")
                                    {
                                        if (Regex.IsMatch(bankProc.Id, patternProcess)
                                            && bankProcessDB.BankProcesses.FirstOrDefault(p => p.Code == bankProc.Id) == null)
                                        {
                                            // Row to BankProcesses (4 lvl)
                                            if (!bankDepartments.Contains(bankProc.Department))
                                            {
                                                // Row to BankDepartments (catalog)
                                                bankDepartments.Add(bankProc.Department);
                                                bankDepartment = new BankDepartment { Name = bankProc.Department };
                                                bankProcessDB.BankDepartments.Add(bankDepartment);
                                            }
                                            else bankDepartment = bankProcessDB.BankDepartments.FirstOrDefault(d => d.Name == bankProc.Department);

                                            bankProcess = new BankProcess { Name = bankProc.Name, Code = bankProc.Id, BankAction = bankAction, BankDepartment = bankDepartment };
                                            bankProcessDB.BankProcesses.Add(bankProcess);
                                        }
                                    }
                                    else
                                    {
                                        // Logic for empty Rows from file
                                    }
                                }
                                bankProcessDB.SaveChanges();
                            }
                        }
                    });
                    comboBox1.Items.AddRange(new string[]{ "BankProcTypes", "BankThemes", "BankActions", "BankProcesses", "BankDepartments" });
                    comboBox1.SelectedItem = "BankProcesses";
                    label6.Text = "Файл сохранен в БД";
                    break;
                default:
                    label6.Text = "Укажите шаблон";
                    break;
            }
        }

        // Cropping a file data
        private void button2_Click(object sender, EventArgs e)
        {
            string templ = comboBox2?.SelectedItem?.ToString();

            switch (templ)
            {
                case "Процессы банка":
                    try
                    {
                        var records = (List<BankProcCSV>)filesLoaded[templ];
                        BankProcCSV header = new BankProcCSV();
                        for (int i = 1; i < numericUpDown1.Value; i++) records.RemoveAt(0);
                        header.Id = records[0].Id;
                        header.Name = records[0].Name;
                        header.Department = records[0].Department;
                        for (int i = 1; i < (numericUpDown2.Value - numericUpDown1.Value + 1); i++) records.RemoveAt(0);
                        dataGridView1.DataSource = null;
                        dataGridView1.DataSource = filesLoaded[templ];
                        dataGridView1.Columns[0].HeaderText = header.Id;
                        dataGridView1.Columns[1].HeaderText = header.Name;
                        dataGridView1.Columns[2].HeaderText = header.Department;
                        label6.Text = "Обновлено";
                    }
                    catch (Exception ex)
                    {
                        label6.Text = ex.Message;
                    }
                    
                    break;
                default:
                    label6.Text = "Укажите шаблон";
                    break;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string shw = comboBox1.SelectedItem.ToString();
            dataGridView1.DataSource = null;

            // Show DB tables
            using (BankProcessContext bankProcessDB = new BankProcessContext())
            {
                switch (shw)
                {
                    case "Процессы банка":
                        dataGridView1.DataSource = filesLoaded[shw];
                        break;
                    case "BankProcTypes":
                        var bankProcTypes = bankProcessDB.BankProcTypes.ToList();
                        dataGridView1.DataSource = bankProcTypes;
                        break;
                    case "BankThemes":
                        var bankThemes = bankProcessDB.BankThemes.Include(t => t.BankProcType).ToList();
                        dataGridView1.DataSource = bankThemes;
                        break;
                    case "BankActions":
                        var bankActions = bankProcessDB.BankActions.Include(a => a.BankTheme).ToList();
                        dataGridView1.DataSource = bankActions;
                        break;
                    case "BankProcesses":
                        var bankProcesses = bankProcessDB.BankProcesses.Include(p => p.BankAction).Include(p=>p.BankDepartment).ToList();
                        dataGridView1.DataSource = bankProcesses;

                        break;
                    case "BankDepartments":
                        var bankDepartments = bankProcessDB.BankDepartments.ToList();
                        dataGridView1.DataSource = bankDepartments;
                        break;
                    default:
                        break;
                }
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown2.Minimum = numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown1.Maximum = numericUpDown2.Value;
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            TextBox txt = (TextBox)e.Control;
            txt.ReadOnly = true;
        }
    }
}
