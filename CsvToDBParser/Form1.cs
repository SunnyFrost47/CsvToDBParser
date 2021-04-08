using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        private Dictionary<string, ICollection> filesLoaded = new Dictionary<string, ICollection>();
        public Form1()
        {
            InitializeComponent();

            openFileDialog1.Filter = "CSV files(*.csv)|*.csv|Text files(*.txt)|*.txt";
            comboBox2.Items.Add("Процессы банка");
        }

        private void cSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel) return;
            string fileName = openFileDialog1.FileName;
            string templ = comboBox2?.SelectedItem?.ToString();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ";",
            };

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
            }
        }

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

        private async void button1_Click(object sender, EventArgs e)
        {
            string templ = comboBox2.SelectedItem.ToString();

            switch (templ)
            {
                case "Процессы банка":
                    List<string> bankDepartments = new List<string>();
                    string patternTheme = @"^\w?[0-9]+$";
                    string patternAction = @"^\w?[0-9]+\.[0-9]+$";
                    string patternProcess = @"^\w?[0-9]+\.[0-9]+\.[0-9]+$";
                    using (BankProcessContext bankProcessDB = new BankProcessContext())
                    {
                        foreach (BankProcCSV bankProc in filesLoaded[templ])
                        {
                            if (bankProc.Department == "")
                            {
                                if (bankProc.Id == "")
                                {
                                    BankProcType bankProcType = new BankProcType { Name = bankProc.Name };
                                    bankProcessDB.BankProcTypes.Add(bankProcType);
                                }
                                else
                                {
                                    if (Regex.IsMatch(bankProc.Id, patternTheme))
                                    {
                                        BankTheme bankTheme = new BankTheme { Name = bankProc.Name, Code = bankProc.Id };
                                        bankProcessDB.BankThemes.Add(bankTheme);
                                    }

                                    if (Regex.IsMatch(bankProc.Id, patternAction))
                                    {
                                        BankAction bankAction = new BankAction { Name = bankProc.Name, Code = bankProc.Id };
                                        bankProcessDB.BankActions.Add(bankAction);
                                    }
                                }
                            }
                            else
                            {
                                if (bankProc.Id != "")
                                {
                                    if (Regex.IsMatch(bankProc.Id, patternProcess))
                                    {
                                        BankProcess bankProcess = new BankProcess { Name = bankProc.Name, Code = bankProc.Id };
                                        bankProcessDB.BankProcesses.Add(bankProcess);
                                        if (!bankDepartments.Contains(bankProc.Department))
                                        {
                                            bankDepartments.Add(bankProc.Department);
                                            BankDepartment bankDepartment = new BankDepartment { Name = bankProc.Department };
                                            bankProcessDB.BankDepartments.Add(bankDepartment);
                                        }

                                    }
                                }
                                else
                                {

                                }
                            }
                            bankProcessDB.SaveChanges();
                        }
                    }
                    
                    label6.Text = "Файл сохранен в БД";
                    break;
                default:
                    label6.Text = "Укажите шаблон";
                    break;
            }
        }

        //private void saveToDB

        private void button2_Click(object sender, EventArgs e)
        {
            string templ = comboBox2.SelectedItem.ToString();

            switch (comboBox2.SelectedItem.ToString())
            {
                case "Процессы банка":
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
                    break;
                default:
                    label6.Text = "Укажите шаблон";
                    break;
            }
        }
    }
}
